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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.Configuration.Services;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks
{
    /// <summary>
    /// The task which:
    /// 1. receives the heartbeats and adds them to the report table.
    /// 2. continuously checks the received heartbeats.
    /// 3. starts the Send heartbeat task as well.
    /// </summary>
    public class LocalShardCheckHeartbeatTask
    {
        private LocalShardHeartbeatReporting _localShardHeartbeatReporter = null;
        private int _poolingThreshold = 4 * 1000;
        private Thread _checkHbThread = null;
        private System.Threading.ManualResetEvent _startSignal;
        private volatile bool _running;

        private const int _maxInterval = 2;
        private const int _maxNumberOfMissingHeartbeats = 3;
        private MembershipManager _membershipManager = null;
        private LocalShardSendHeartbeatTask _sendHearbeatTask = null;

        private ElectionMechanismExecutionTask _electionExecTask = null;

        private NodeContext _context = null;

        private IShard _shard = null;
        private Object mutex = new Object();
        private ClusterConfigurationManager _clusterConfigMgr = null;

        private Object _syncMutex = new Object();

        public LocalShardHeartbeatReporting HeartbeatReportingTable
        { get { return _localShardHeartbeatReporter; } }

        public LocalShardCheckHeartbeatTask(IShard shard, NodeContext context, MembershipManager membershipManager, ClusterConfigurationManager clusterConfigMgr)
        {
            this._shard = shard;
            this._context = context;
            this._clusterConfigMgr = clusterConfigMgr;
            _localShardHeartbeatReporter = new LocalShardHeartbeatReporting(_context.LocalAddress);

            this._membershipManager = membershipManager;
            this._membershipManager.HeartbeatReport = _localShardHeartbeatReporter;
            ShardConfiguration sConfig = null;
            if (_clusterConfigMgr != null)
                sConfig = _clusterConfigMgr.GetShardConfiguration(context.LocalShardName);

            if (sConfig != null && sConfig.NodeHeartbeatInterval > 0)
                this._poolingThreshold = (sConfig.NodeHeartbeatInterval * 2) * 1000;
            _running = false;
            _startSignal = new ManualResetEvent(false);


            //with the initialization of the heartbeat receiver, we start the send heart beat task.
            _sendHearbeatTask = new LocalShardSendHeartbeatTask(context, shard, _membershipManager, clusterConfigMgr);
        }


        public void OnActivityCompleted()
        {
            lock (_syncMutex)
            {
                Monitor.PulseAll(_syncMutex);
            }
        }

        // Temporary fix for node leaving activity thing.
        public void OnActivityTriggered(Activity activity, Address node)
        {
            if (node != null)
            {
                switch (activity)
                {
                    case Activity.NodeLeaving:
                        if (_localShardHeartbeatReporter != null)
                            _localShardHeartbeatReporter.RemoveFromReport(node);
                        break;
                }
            }
        }

        #region Receive Heartbeats
        // Receives the heartbeats from different nodes which are part of a shard and 
        // adds them to the report table.
        public void ReceiveHeartbeat(Address source, HeartbeatInfo heartbeatInfo)
        {
            try
            {
                if (_localShardHeartbeatReporter != null)
                {
                    // We need to verify if the node sending the heartbeat is part of the existing configuration.
                    // Updating the config is a costly process so we check the node in the existing config. 
                    // If the node was freshly added in an existing (active) shard, we will not add the heartbeat to the report
                    // until it exists in the local node config instance.
                    if (_clusterConfigMgr != null)
                    {
                        ShardConfiguration sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                        ServerNode sNode = null;
                        if (sConfig != null && sConfig.Servers != null)
                            sNode = sConfig.Servers.GetServerNode(source.IpAddress.ToString());
                        if (sNode == null)
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.ReceiveHeartbeat() ", "The node " + source + " is not part of the configuration.");
                            return;
                        }
                    }

                    bool isAnOldNode = _localShardHeartbeatReporter.AddToReport(source, heartbeatInfo);
                   
                    if (!isAnOldNode)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.ReceiveHeartbeat() ", "Node " + source.IpAddress + " added to the table for the first time. ");

                        lock (_membershipManager)
                        {
                            _membershipManager.HeartbeatReport = _localShardHeartbeatReporter;
                        }

                        _electionExecTask = new ElectionMechanismExecutionTask(_membershipManager, Activity.NodeJoining, new Server(source, Status.Running));
                        _electionExecTask.Start();
                        OnActivityCompleted();

                    }

                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("LocalShardCheckHeartbeatTask.ReceiveHeartbeat() ", e.ToString());
            }
        }
        #endregion

        #region Check Heartbeat task
        // the method which runs in a thread.
        public void Run()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = "" });
            _context.StatusLatch.WaitForAny(NodeStatus.Running);
            _startSignal.WaitOne();
            while (_running)
            {
                IDictionary<Address, HeartbeatInfo> reportTable = null;
                try
                {
                    ShardConfiguration sConfig = null;
                    if (_clusterConfigMgr != null)
                        sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                    if (sConfig == null || sConfig.Servers == null)
                    {
                        if (LoggerManager.Instance.ShardLogger != null &&
                            LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                            LoggerManager.Instance.ShardLogger.Warn("LocalShardCheckHeartbeatTask.Run() ", "The shard (or the nodes of the shard) " +
                                                                    _context.LocalShardName +
                                                                    " does not exist in the configuration.");
                        return;
                    }
                    reportTable = _localShardHeartbeatReporter.GetReportTable;
                    if (reportTable != null && reportTable.Count > 0)
                    {
                        CheckHeartbeats(reportTable);

                        IList<Address> tentativeLostNodes = CheckForLostNodes(reportTable);
                        if (tentativeLostNodes != null && tentativeLostNodes.Count > 0)
                        {
                            // 1. These are the nodes lost.
                            // 2. The lost node needs to be removed from the heartbeats table and the missing heartbeats
                            // 3. Elections will be triggered when a node is lost.

                            foreach (var node in tentativeLostNodes)
                            {
                                if (_localShardHeartbeatReporter.GetReportTable.ContainsKey(node) &&
                                    !ChannelExists(sConfig.Servers.GetServerNode(node.IpAddress.ToString())))
                                {
                                    if (LoggerManager.Instance.ShardLogger != null &&
                                        LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                        LoggerManager.Instance.ShardLogger.Debug("LocalShard.CheckHeartbeatTask",
                                            "did not receive heart beat from " + node);


                                    //_localShardHeartbeatReporter.RemoveFromReport(node);
                                    OnActivityTriggered(Activity.NodeLeaving, node);

                                    lock (_membershipManager)
                                    {
                                        _membershipManager.HeartbeatReport =
                                            (LocalShardHeartbeatReporting)_localShardHeartbeatReporter;
                                    }
                                    _electionExecTask = new ElectionMechanismExecutionTask(_membershipManager,
                                        Activity.NodeLeaving, new Server(node, Status.Stopped));
                                    _electionExecTask.Start();
                                    OnActivityCompleted();
                                    

                                }
                                else
                                {
                                    if (LoggerManager.Instance.ShardLogger != null &&
                                        LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                        LoggerManager.Instance.ShardLogger.Debug("LocalShard.CheckHeartbeatTask",
                                            "did not receive heart beat from " + node + ", however channel is connected");

                                }
                            }

                        }
                        //if no primary is set, we proceed to the following steps:
                        if (!_shard.NodeRole.Equals(NodeRole.Intermediate) && _membershipManager.LatestMembership == null ||
                            (/*_membershipManager.LatestMembership != null &&*/
                             (_membershipManager.LatestMembership.Primary == null ||
                              (/*_membershipManager.LatestMembership.Primary != null &&*/
                               !ChannelExists(_membershipManager.LatestMembership.Primary)))))
                        {

                            //First, check if the conduction of elections is feasible. 
                            //If yes, the actual election mechanism is triggered.
                            if (AreElectionsFeasible(_localShardHeartbeatReporter, sConfig))
                            {
                                lock (_membershipManager)
                                {
                                    _membershipManager.HeartbeatReport =
                                        (LocalShardHeartbeatReporting)_localShardHeartbeatReporter;
                                }

                                _electionExecTask = new ElectionMechanismExecutionTask(_membershipManager, Activity.GeneralElectionsTriggered, null);
                                _electionExecTask.Start();
                            }
                            //RTD: badddd logic.
                            //else if (_localShardHeartbeatReporter.PrimaryExists() &&
                            //         _localShardHeartbeatReporter.GetCurrentPrimary() != null &&
                            //         ChannelExists(_localShardHeartbeatReporter.GetCurrentPrimary()))
                            //{
                            //    ServerNode currentPrimaNode = _localShardHeartbeatReporter.GetCurrentPrimary();
                            //    if (currentPrimaNode != null)
                            //    {
                            //        if (LoggerManager.Instance.ShardLogger != null &&
                            //            LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            //            LoggerManager.Instance.ShardLogger.Debug("LocalShard.CheckHeartbeatTask",
                            //                "no primary set so far, setting " + currentPrimaNode.Name +
                            //                " as primary as other nodes are reporting");

                            //        MembershipChangeArgs args = new MembershipChangeArgs();
                            //        args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimarySet;
                            //        args.ElectionId = _localShardHeartbeatReporter.GetCurrentElectionId();
                            //        args.ServerName = new Address(currentPrimaNode.Name, sConfig.Port);
                            //        ((LocalShard)_shard).OnMembershipChanged(args);
                            //    }
                            //}
                        }
                        LogMembership();
                    }
                }
                catch (ThreadAbortException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled && _checkHbThread != null)
                    {
                        LoggerManager.Instance.ShardLogger.Error(_checkHbThread.Name, "Task aborted.");
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("LocalShardCheckHeartbeatTask.Run() ", e.ToString());
                }

                lock (_syncMutex)
                {
                    Monitor.Wait(_syncMutex, _poolingThreshold);
                }
                _startSignal.WaitOne();

            }
        }

        private bool ChannelExists(ServerNode serverNode)
        {
            if (serverNode != null && _shard.ActiveChannelsList != null)
            {
                foreach (var node in _shard.ActiveChannelsList)
                {
                    if (node.Address.IpAddress.ToString() == serverNode.Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private IList<Address> CheckForLostNodes(IDictionary<Address, HeartbeatInfo> reportTable)
        {
            IList<Address> lostNodes = new List<Address>();
            IList<Address> serverList = reportTable.Keys.ToList();
            if (serverList != null)
            {
                for (int index = 0; index < serverList.Count; index++)
                {
                    HeartbeatInfo serverInfo = reportTable[serverList[index]];
                    lock (serverInfo)
                    {
                        if (serverInfo != null && serverInfo.MissingHeartbeatsCounter >= _maxNumberOfMissingHeartbeats)
                        {
                            lostNodes.Add(serverList[index]);
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.CheckForLostNodes() ", serverInfo.MissingHeartbeatsCounter.ToString() + " heartbeats from node " + serverList[index].IpAddress.ToString() + " missed.");
                        }
                    }
                }
            }
            return lostNodes;
        }

        private void LogMembership()
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
            {
                if (_membershipManager.LatestMembership != null)
                {
                    if (_membershipManager.LatestMembership.Primary != null)
                        LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.LogMembership() ", "Current Primary: " + _membershipManager.LatestMembership.Primary.Name.ToString() + " with a " + ((Priority)_membershipManager.LatestMembership.Primary.Priority).ToString() + " node priority.");
                    LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.LogMembership() ", "HeartbeatServerList: ");
                    if (_membershipManager.LatestMembership.Servers != null)
                    {
                        foreach (var value in _membershipManager.LatestMembership.Servers)
                        {
                            LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.LogMembership() ", value.Name.ToString() + " with a " + ((Priority)value.Priority).ToString() + " node priority.");
                        }
                    }
                    LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.LogMembership() ", "ActiveChannelsList: ");
                    if (_shard.ActiveChannelsList != null)
                    {
                        foreach (var server in _shard.ActiveChannelsList)
                        {
                            LoggerManager.Instance.ShardLogger.Debug(server.Address.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This methods checks the heartbeats with the current table.
        /// </summary>
        /// <returns></returns>
        private void CheckHeartbeats(IDictionary<Address, HeartbeatInfo> reportTable)
        {
            if (reportTable != null)
            {
                IList<Address> serverList = reportTable.Keys.ToList();
                if (serverList != null)
                {
                    for (int index = 0; index < serverList.Count; index++)
                    {
                        if (reportTable.ContainsKey(serverList[index]))
                        {
                            HeartbeatInfo serverInfo = reportTable[serverList[index]];
                            lock (serverInfo)
                            {
                                if (serverInfo != null)
                                {
                                    DateTime? lastHeartbeat = serverInfo.LastHeartbeatTimestamp;
                                    if (lastHeartbeat.HasValue)
                                    {
                                        if (lastHeartbeat.Value.AddSeconds(_maxInterval) < DateTime.Now)
                                        {
                                            _localShardHeartbeatReporter.UpdateMissingHeartbeatsCount(serverList[index]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        /// <summary>
        /// function needs to be shifted from here.
        /// this function provides a mini election feasibility check to avoid continuous triggering of the 
        /// election mechanism.
        /// Steps:
        /// 1. Does the node possess a majority of the connections.
        /// 2. If yes, proceed to the next step where the primary is hunted for in the existing heartbeat report table.
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private bool AreElectionsFeasible(LocalShardHeartbeatReporting report, ShardConfiguration sConfig)
        {
            IList<Address> activeServers = report.GetReportTable.Keys.ToList();
            if (activeServers.Count < (int)Math.Ceiling((double)sConfig.Servers.Nodes.Count / 2))
                return false;
            else
            {
                if (report.PrimaryExists())
                {
                    // We verify if a primary exists on any of the nodes at any given point in time and if a channel exists 
                    // because there is a possibility that the primary is not stepping down and is not a valid primary either
                    // (catering for split - brain scenarios)
                    return !ChannelExists(report.GetCurrentPrimary());
                }

            }
            return true;
        }

        public void Stop()
        {
            //Stop the sender first.
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.ShardLogger.Info("CheckHeartbeatTask.Stop", "check heartbeat task stopped for " + _shard.Name + " shard");
            }
            lock (mutex)
            {
                _running = false;
                _startSignal.Set();

                if (_sendHearbeatTask != null)
                    _sendHearbeatTask.Stop();

                if (_checkHbThread != null)
                    _checkHbThread.Abort();

                if (_electionExecTask != null)
                    _electionExecTask.Dispose();
            }
        }

        public void Start()
        {
            lock (mutex)
            {

                if (_sendHearbeatTask != null)
                {
                    _sendHearbeatTask.Start();
                }

                if (_checkHbThread == null)
                    _checkHbThread = new Thread(new ThreadStart(Run));
                _checkHbThread.Name = "CheckHeartbeat." + _shard.Name;

                _checkHbThread.IsBackground = true;
                _checkHbThread.Start();

                _running = true;
                _startSignal.Set();


            }
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.ShardLogger.Info("LocalShardCheckHeartbeatTask.Start()", "Local Shard Heartbeat Task started successfully.");
            }
        }

        #endregion

        #region Election Mechanism Execution Task
        public class ElectionMechanismExecutionTask
        {
            MembershipManager _manager = null;
            Activity _activity;
            Server _server = null;
            Thread _taskthread = null;
            Object mutex = new Object();
            public ElectionMechanismExecutionTask(MembershipManager manager, Activity activity, Server server)
            {
                this._manager = manager;
                this._activity = activity;
                this._server = server;
            }

            public void Run()
            {
                if (_manager != null)
                {
                    switch (_activity)
                    {
                        case Activity.NodeJoining:
                            _manager.OnMemberJoined(_server);
                            break;
                        case Activity.NodeLeaving:
                            _manager.OnMemberLeft(_server);
                            break;
                        case Activity.GeneralElectionsTriggered:
                            _manager.OnGeneralElectionsTriggered();
                            break;
                    }
                }
            }

            public void Start()
            {
                lock (mutex)
                {
                    _taskthread = new Thread(new ThreadStart(Run));
                    string shardName = null;
                    if (_manager != null && _manager.Context != null)
                        shardName = _manager.Context.LocalShardName;
                    _taskthread.Name = _activity + "." + shardName;
                    _taskthread.IsBackground = true;
                    _taskthread.Start();
                }
            }

            public void Dispose()
            {
                lock (mutex)
                {
                    if (_taskthread != null)
                        _taskthread.Abort();
                }
            }
        }
        #endregion
    }
}
