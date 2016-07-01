// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration;
using Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement
{
    public class MembershipManager : IShardListener
    {
        private IMembershipStrategy _strategy;
        private Membership _latestMembership = new Membership();
        private HeartbeatManager _heartbeatManager;
        private IShard _shard;
        private LocalShardHeartbeatReporting _heartbeatReporting = null;
        private Object _mutex = new Object();
        private NodeContext _context = null;
        private ClusterConfigurationManager _clusterConfigMgr = null;
        private bool _canIInitiateTakeoverElections = false;
        private bool _takeOverStarted;
        private TakeOverElectionTask _takeOverElectionTask;
        private int waitThreshold = 10 * 1000;
        private TakeoverRetryTask _retryTask = null;
        private OperationId _lastOpId = null;
        private Object _mutexOnUpdateConfig = new Object();

        public LocalShardHeartbeatReporting HeartbeatReport
        {
            get { return _heartbeatReporting; }
            set { _heartbeatReporting = value; }
        }
        public Membership LatestMembership
        {
            get { return _latestMembership; }
        }
        public NodeContext Context
        {
            get { return _context; }
        }
        public ConnectivityStatus CSStatus
        {
            get
            {
                return _heartbeatManager.CSStatus;
            }
        }


        public MembershipManager(IShard shard, NodeContext context, ClusterConfigurationManager clusterConfigMgr)
        {
            this._shard = shard;
            this._context = context;
            this._clusterConfigMgr = clusterConfigMgr;
            this._strategy = new ElectionBasedMembershipStrategy(shard, context, _heartbeatReporting, _clusterConfigMgr);
            shard.RegisterShardListener(Common.MiscUtil.MEMBERSHIP_MANAGER, this);
            _heartbeatManager = new HeartbeatManager();

            _latestMembership.Cluster = _context.ClusterName;
            _latestMembership.Shard = _context.LocalShardName;
        }

        /// <summary>
        /// All the election related heartbeat tasks begin here.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="shard"></param>
        public void BeginHeartbeatTasks(NodeContext context, IShard shard, IConnectionRestoration connectionRestoration)
        {
            if (_heartbeatManager != null)
                _heartbeatManager.BeginTasks(context, shard, this, connectionRestoration, _clusterConfigMgr);


        }

        public void OnActivityComplete()
        {
            if (_heartbeatManager != null)
                _heartbeatManager.OnActivityComplete();
        }

        public void OnActivityTriggered(Activity activity, Address node)
        {
            if (_heartbeatManager != null)
                _heartbeatManager.OnActivityTriggered(activity, node);
        }

        public object OnMessageReceived(Message message, Server source)
        {
            try
            {
                if (message != null)
                {
                    switch (message.MessageType)
                    {
                        case MessageType.MembershipOperation:
                            if (_strategy != null)
                                return _strategy.OnMessageReceived(message.Payload, source, _heartbeatReporting, _latestMembership);
                            break;
                        case MessageType.Heartbeat:
                            if (_heartbeatManager != null)
                                _heartbeatManager.ReceiveHeartbeat(source.Address, (HeartbeatInfo)message.Payload);
                            break;
                    }
                }
                return null;

            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnMessageReceived(): ", e.ToString());
                return null;
            }
        }


        public void UpdateLastOperationId(OperationId id)
        {
            _lastOpId = id;
            if (_strategy != null)
                _strategy.LastOperationId = id;
        }

        public OperationId GetLastOperationId
        { get { return _lastOpId; } }

        public void OnMemberJoined(Server server)
        {
            try
            {
                if (_strategy != null)
                {
                    lock (_mutex)
                    {
                        _strategy.TriggerElectionMechanism(Activity.NodeJoining, server, _heartbeatReporting, _latestMembership);
                        SanityCheckForTakeoverElect();

                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnMemberJoined(): ", e.ToString());
            }
        }

        public void OnMemberLeft(Server server)
        {
            try
            {
                if (_strategy != null)
                {
                    lock (_mutex)
                    {
                        _strategy.TriggerElectionMechanism(Activity.NodeLeaving, server, _heartbeatReporting, _latestMembership);
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnMemberLeft(): ", e.ToString());
            }
        }

        public void OnGeneralElectionsTriggered()
        {
            try
            {
                if (_strategy != null)
                {
                    lock (_mutex)
                    {
                        if (_shard != null && _shard.NodeRole != NodeRole.Intermediate && _latestMembership == null || _latestMembership.Primary == null)
                        {
                            _strategy.TriggerElectionMechanism(Activity.GeneralElectionsTriggered, null, _heartbeatReporting, _latestMembership);
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("MembershipManager.OnGeneralElectionsTriggered() ", "Elections triggered at " + DateTime.Now.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnGeneralElectionsTriggered(): ", e.ToString());
            }
        }
        /// <summary>
        /// Triggered when a high priority node has a secondary node level.
        /// </summary>
        public void OnTakeoverElectionsTriggered()
        {
            try
            {
                if (_strategy != null)
                {
                    if (_shard != null && _shard.NodeRole != NodeRole.Intermediate)
                    {
                        lock (_mutex)
                        {
                            _strategy.TriggerElectionMechanism(Activity.TakeoverElectionsTriggered, null, _heartbeatReporting, _latestMembership);
                            
                        }
                    }
                }
            }
            
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnTakeoverElectionsTriggered(): ", e.ToString());
            }
        }

        internal bool OnForcefulPrimaryDemotion(MembershipChangeArgs args)
        {
            try
            {
                if (args != null && args.ChangeType == MembershipChangeArgs.MembershipChangeType.ForcefullyDemotePrimary && args.ServerName != null)
                {
                    if (_strategy != null)
                    {
                        lock (_mutex)
                        {
                            _strategy.TriggerElectionMechanism(Activity.ForcefulPrimaryDemotion, new Server(args.ServerName, Status.Running), _heartbeatReporting, _latestMembership);
                            if (_latestMembership != null && (_latestMembership.Primary == null || _latestMembership.Primary.Name != _context.LocalAddress.IpAddress.ToString()))
                            {
                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                    LoggerManager.Instance.ShardLogger.Debug("MembershipManager.OnForcefulPrimaryDemotion()", "Call for forceful demotion of the primary node received. Primary demoted successfully.");
                                return true;
                            }
                        }
                    }
                }
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                    LoggerManager.Instance.ShardLogger.Info("MembershipManager.OnForcefulPrimaryDemotion()", "Forceful primary demotion unsuccessful.");
                return false;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnForcefulPrimaryDemotion(): ", e.StackTrace);
                return false;
            }
        }

        public void Dispose()
        {
            if (_heartbeatManager != null)
                _heartbeatManager.Dispose();
            _heartbeatManager = null;
            if (_takeOverElectionTask != null)
                _takeOverElectionTask.Stop();
            _takeOverElectionTask = null;
            if (_retryTask != null)
                _retryTask.Stop();
            _retryTask = null;
        }

        /// <summary>
        /// This method is called when the config server is disconnected from the local node.
        /// </summary>
        internal void OnCSDisconnected()
        {
            try
            {
                if (_strategy != null)
                {
                    lock (_mutex)
                    {
                        _strategy.TriggerElectionMechanism(Activity.CSDisconnected, null, _heartbeatReporting, _latestMembership);
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("MembershipManager.OnCSDisconntected() ", "The node lost connection with the CS.");

                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        {
                            LoggerManager.Instance.ShardLogger.Debug("MembershipManager.OnCSDisconntected() ", "Current node connection with the CS: " + CSStatus.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("MembershipMgr.OnCSDisconnected(): ", e.ToString());
            }

        }


        internal void SanityCheckForTakeoverElect()
        {
            if (_latestMembership != null && _latestMembership.Primary != null)
            {
                if (_clusterConfigMgr != null)
                {
                    if (AmIEligibleForTakeoverElections())
                    {
                       
                    }
                }
            }

        }

        internal void UpdateLocalMembership(MembershipChangeArgs args)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = "" });
            ShardConfiguration sConfig = null;
            if (_clusterConfigMgr != null)
                sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
            if (sConfig == null || sConfig.Servers == null)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                    LoggerManager.Instance.ShardLogger.Warn("MembershipManager.UpdateLocalMembership() ", "The shard (or the nodes of the shard) " + _context.LocalShardName + " does not exist in the configuration.");
                return;
            }
            ServerNode affectedServer = sConfig.Servers.GetServerNode(args.ServerName.IpAddress.ToString());
            switch (args.ChangeType)
            {
                case MembershipChangeArgs.MembershipChangeType.NodeJoined:
                    lock (_mutexOnUpdateConfig)
                    {
                        _latestMembership.AddServer(affectedServer);
                    }
                    break;

                case MembershipChangeArgs.MembershipChangeType.NodeLeft:
                    lock (_mutexOnUpdateConfig)
                    {
                        _latestMembership.RemoveServer(affectedServer);
                    }
                    break;

                case MembershipChangeArgs.MembershipChangeType.PrimarySelected:
                case MembershipChangeArgs.MembershipChangeType.PrimarySet:
                    lock (_mutexOnUpdateConfig)
                    {
                        _latestMembership.AddServer(affectedServer);

                        if (args.ServerName != null)
                        {
                            _latestMembership.Primary = sConfig.Servers.GetServerNode(args.ServerName.IpAddress.ToString());

                               
                        }
                        if (args.ElectionId != null)
                            _latestMembership.ElectionId = args.ElectionId;

                        SanityCheckForTakeoverElect();
                    }
                    _strategy.OnPrimaryChanged();

                    break;
                case MembershipChangeArgs.MembershipChangeType.PrimaryLost:
                case MembershipChangeArgs.MembershipChangeType.PrimaryDemoted:
                    lock (_mutexOnUpdateConfig)
                    {
                        if (args.ChangeType.Equals(MembershipChangeArgs.MembershipChangeType.PrimaryLost))
                            _latestMembership.RemoveServer(affectedServer);
                        _latestMembership.Primary = null;
                        _latestMembership.ElectionId = null;
                    }
                    break;
            }

            if (args.ServerName != null && args.ChangeType != MembershipChangeArgs.MembershipChangeType.None)
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                    LoggerManager.Instance.ShardLogger.Debug("MembershipManager.UpdateLocalMembership() ", "Membership updated: " + args.ChangeType.ToString() + " of node " + args.ServerName.IpAddress.ToString());
        }

        internal void StopHeartbeatTasks()
        {
            if (_heartbeatManager != null)
                _heartbeatManager.Stop();
        }

        #region IReplicationStatusListener

        public void OnReplicationMarginWindowEntered()
        {
            if (_takeOverElectionTask == null)
            {
                _takeOverElectionTask = new TakeOverElectionTask(this);

                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                    LoggerManager.Instance.ShardLogger.Debug("MembershipManager.OnRepMarginWinEntered()", "Task started.");
            }
            if (!_takeOverElectionTask.IsStarted())
                _takeOverElectionTask.Start();
            _takeOverElectionTask.SetStateUnsynchronized();
        }

        public void OnReplicationCompleted()
        {
            if (_takeOverElectionTask == null)
            {
                _takeOverElectionTask = new TakeOverElectionTask(this);
            }
            if (!_takeOverElectionTask.IsStarted())
                _takeOverElectionTask.Start();
            _takeOverElectionTask.SetStateSynchronized();

            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                LoggerManager.Instance.ShardLogger.Debug("MembershipManager.OnRepCompleted()", "Task started.");
        }

        private bool InitiateTakeOverElection()
        {
            if (!_canIInitiateTakeoverElections && _latestMembership.Primary != null && AmIEligibleForTakeoverElections())
            {
                //request primary to go into read-only mode, so that I can quickly synchroize
                if (RequestPrimaryToStopOperations())
                {
                    _canIInitiateTakeoverElections = true;
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        LoggerManager.Instance.ShardLogger.Info("MembershipManager." + _shard.Name, "primary " + _latestMembership.Primary.Name + " moved to 'read-only' state ");

                }
                else
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        LoggerManager.Instance.ShardLogger.Info("MembershipManager." + _shard.Name, "primary " + _latestMembership.Primary.Name + " did not go into 'read-only' state ");
                    _canIInitiateTakeoverElections = false;
                }

            }
            return _canIInitiateTakeoverElections;
        }

        private bool AmIEligibleForTakeoverElections()
        {
            if (_context == null)
                return false;

            if (_shard.NodeRole == NodeRole.Secondary && _latestMembership.Primary != null)
            {
                ClusterConfiguration cConfig = _clusterConfigMgr.LatestConfiguration;


                if (cConfig != null && cConfig.Deployment != null)
                {
                    ShardConfiguration sConfig = cConfig.Deployment.GetShardConfiguration(_context.LocalShardName);
                    int port = 0;

                    if (sConfig != null)
                        port = sConfig.Port;
                    Address currentPrimary = new Address(_latestMembership.Primary.Name, port);

                    if (sConfig == null || sConfig.Servers == null)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                            LoggerManager.Instance.ShardLogger.Warn("MembershipMgr.AmIEligibleForTakeoverElections()", "The shard configuration does not exist.");
                        return false;
                    }

                    ServerNode localNode = sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString());
                    if (localNode != null && currentPrimary != null)
                    {
                        if (localNode.Name.Equals(currentPrimary.IpAddress.ToString()))
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                LoggerManager.Instance.ShardLogger.Info("MembershipManager.AmIEligibleforTakeoverElect()", "Local node is already primary with the highest priority amongst the current running nodes.");
                            return false;
                        }

                        ServerNode primaryNode = sConfig.Servers.GetServerNode(currentPrimary.IpAddress.ToString());
                        if (primaryNode != null)
                        {
                            if (localNode.Priority.Equals(primaryNode.Priority))
                                return false;
                            //Opposite: the lower the number, the higher the priority.
                            if (localNode.Priority < primaryNode.Priority)
                            {
                                int maxPriority = localNode.Priority;
                                IList<Server> activeShardNodes = _shard.ActiveChannelsList;

                                if (activeShardNodes != null && activeShardNodes.Count > 0)
                                {
                                    foreach (var server in activeShardNodes)
                                    {
                                        ServerNode node = sConfig.Servers.GetServerNode(server.Address.IpAddress.ToString());
                                        if (node != null)
                                        {
                                            int thisPriority = node.Priority;
                                            if (thisPriority < maxPriority)
                                                maxPriority = thisPriority;
                                        }
                                    }
                                }
                                if (maxPriority == localNode.Priority)
                                {
                                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                        LoggerManager.Instance.ShardLogger.Debug("MembershipManager.AmIEligibleforTakeoverElect()", " Eligibility test passed for the takeover electionMechanism.");
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// We need to ask the current primary to quit taking write ops
        /// so that the left over ops can first be replicated 
        /// before the actual election mechanism begins.
        /// </summary>
        /// <returns></returns>
        private bool RequestPrimaryToStopOperations()
        {
            ShardConfiguration sConfg = null;
            bool endResult = false;
            if (_clusterConfigMgr != null)
                sConfg = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);

            if (sConfg != null)
            {
                MembershipChangeArgs args = new MembershipChangeArgs();
                args.ChangeType = MembershipChangeArgs.MembershipChangeType.RestrictPrimary;
                args.ServerName = _context.LocalAddress;
                args.ElectionId = _latestMembership.ElectionId;

                DatabaseMessage msg = new DatabaseMessage();
                msg.Payload = args;
                msg.NeedsResponse = true;
                msg.OpCode = OpCode.RestrictPrimary;
                msg.MessageType = MessageType.DBOperation;
                ShardRequestBase<bool> request = _shard.CreateUnicastRequest<bool>(new Server(new Address(_latestMembership.Primary.Name, sConfg.Port), Status.Running), msg);
                IAsyncResult result = request.BeginExecute();
                endResult = request.EndExecute(result);
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                    LoggerManager.Instance.ShardLogger.Info("MembershipMgr.RequestPrimaryToStopOperations()", "Requested primary to stop taking write operations. Primary response: " + endResult.ToString());
            }

            return endResult;
        }

        internal bool AbortTakeoverMechanismTask(MembershipChangeArgs args)
        {
            if (args != null && args.ChangeType == MembershipChangeArgs.MembershipChangeType.TimeoutOnRestrictedPrimary)
            {
                
               
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        LoggerManager.Instance.ShardLogger.Info("MembershipMgr.AbortTakeoverMechanismTask()", "Takeover unsuccessful. Beginning takeover retry task.");
                    if (_retryTask == null)
                        _retryTask = new TakeoverRetryTask(this);
                    _retryTask.Start();
                

                if (_takeOverElectionTask != null && _takeOverElectionTask.IsStarted())
                    _takeOverElectionTask.Stop();
                return true;
            }
            return false;
        }

        #endregion


        #region /                   --- Inner Classes ----                          /
        /// <summary>
        /// This task is executed in the case of the take over election mechanism only.
        /// </summary>
        class TakeOverElectionTask
        {
            private MembershipManager _membershipManager;
            private Thread _thread;
            private bool _stateSynchronized;

            public TakeOverElectionTask(MembershipManager manager)
            {
                this._membershipManager = manager;
            }

            public void Start()
            {
                _thread = new Thread(new ThreadStart(Run));
                _thread.Name = "TakeOverElectionTask." + _membershipManager._shard.Name;
                _thread.IsBackground = true;
                _thread.Start();
            }

            public void Stop()
            {
                if (_thread != null && _thread.IsAlive)
                {
                    _thread.Abort();
                }

            }

            public void SetStateSynchronized()
            {
                lock (this)
                {
                    _stateSynchronized = true;
                    Monitor.PulseAll(this);
                }
            }
            public void SetStateUnsynchronized()
            {
                lock (this)
                {
                    _stateSynchronized = false;
                    Monitor.PulseAll(this);
                }
            }
            private void Run()
            {
                try
                {
                    if (!_membershipManager.InitiateTakeOverElection())
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("MembershipMgr.TakeOverElectionTask.Run()", "The initiation phase of the takeover elections unsuccessful.");
                        return;
                    }

                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("MembershipMgr.TakeOverElectionTask.Run()", "The initiation phase of the takeover elections successful.");

                    lock (this)
                    {
                        if (!_stateSynchronized)
                            Monitor.Wait(this);
                    }

                    _membershipManager.OnTakeoverElectionsTriggered();

                }
                catch (ThreadAbortException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error(_thread.Name, "Thread aborted.");
                    _thread = null;
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("TakeOverElectionTask.Run(): ", e.ToString());
                }
            }


            internal bool IsStarted()
            {
                if (this._thread != null)
                    return this._thread.IsAlive;
                return false;
            }
        }

        class TakeoverRetryTask
        {
            MembershipManager manager;
            private Thread thread;
            private const int poolingThreshold = 180 * 1000;
            private ManualResetEvent startSignal;
            private volatile bool running;
            private Object mutex = new Object();

            public TakeoverRetryTask(MembershipManager manager)
            {
                this.manager = manager;
                startSignal = new ManualResetEvent(false);
            }

            internal void Start()
            {
                lock (mutex)
                {
                    if (!running)
                        running = true;
                    startSignal.Set();
                    if (thread == null)
                    {
                        thread = new Thread(new ThreadStart(Run));
                        thread.Name = "OnHighPriorityNodeAsSecondary." + manager._context.LocalShardName;
                        thread.IsBackground = true;
                        thread.Start();

                    }
                }
            }
            internal void Stop()
            {
                lock (mutex)
                {
                    running = false;
                    startSignal.Reset();
                    if (thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }
            }

            internal bool IsStarted()
            {
                if (this.thread != null)
                    return this.thread.IsAlive;
                return false;
            }

            private void Run()
            {
                startSignal.WaitOne();
                Thread.Sleep(poolingThreshold);
                while (running)
                {
                    try
                    {
                        if (manager != null)
                        {
                            if (!manager._takeOverElectionTask.IsStarted())
                            {
                                if (manager._takeOverElectionTask != null)
                                    manager._takeOverElectionTask = null;
                                if (manager._canIInitiateTakeoverElections)
                                    manager._canIInitiateTakeoverElections = false;

                            }
                            if (manager._latestMembership != null && manager._latestMembership.Primary != null && manager._latestMembership.Primary.Name == manager._context.LocalAddress.IpAddress.ToString())
                                Stop();

                        }
                    }
                    catch (ThreadAbortException)
                    {
                        this.thread = null;
                        break;
                    }
                    Thread.Sleep(poolingThreshold);
                    startSignal.WaitOne();
                }
            }

        }
        #endregion

    }

}
