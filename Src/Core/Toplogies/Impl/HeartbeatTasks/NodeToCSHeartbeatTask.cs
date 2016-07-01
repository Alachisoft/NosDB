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
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.DBEngine.Management;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks
{
    public class NodeToCSHeartbeatTask : IConnectionRestorationListener,IConfigurationSessionListener
    {
        private NodeContext _context = null;
        private int _heartbeatInterval = 1000 * 20;
        private ServerNode _node;
        private MembershipManager _membershipManager = null;

        private Thread _nodeToCSThread = null;

        private IConnectionRestoration _connectionRestoration = null;
        private volatile ConnectivityStatus _csStatus = ConnectivityStatus.Connected;

        private System.Threading.ManualResetEvent _startSignal;
        private volatile bool _running;
        private Object mutex = new Object();

        private ClusterConfigurationManager _clusterConfigMgr = null;

        public NodeToCSHeartbeatTask(NodeContext context, MembershipManager manager, IConnectionRestoration connectionRestoration, ClusterConfigurationManager clusterConfigMgr)
        {
            this._context = context;
            this._clusterConfigMgr = clusterConfigMgr;
            ShardConfiguration sConfig = null;
            if (_clusterConfigMgr != null )
            {
                sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                if (_clusterConfigMgr.LatestConfiguration != null && _clusterConfigMgr.LatestConfiguration.Deployment != null)
                    _heartbeatInterval = 1000 * _clusterConfigMgr.LatestConfiguration.Deployment.HeartbeatInterval;
            }
            if (sConfig != null && sConfig.Servers != null)
            {
                _node = sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString());

            }

            this._membershipManager = manager;

            this._connectionRestoration = connectionRestoration;

            _running = false;
            _startSignal = new System.Threading.ManualResetEvent(false);
        }

        public ConnectivityStatus CSStatus { get { return _csStatus; } }

        public void Stop()
        {
            lock (mutex)
            {
                if (_context.ConfigurationSession != null)
                {
                    ((OutProcConfigurationSession)_context.ConfigurationSession).UnregisterListener(this);
                }
                _running = false;
                _startSignal.Set();

                if (_nodeToCSThread != null)
                    _nodeToCSThread.Abort();
            }
        }

        public void Start()
        {
            lock (mutex)
            {
                if (_nodeToCSThread == null)
                {
                    _nodeToCSThread = new Thread(new ThreadStart(Run));
                    _nodeToCSThread.IsBackground = true;
                    _nodeToCSThread.Name = _context.LocalShardName + ".NodeToCSHBTask";
                }
               
                _nodeToCSThread.Start();

                _running = true;
                _startSignal.Set();
            }

            if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.ServerLogger.Info("NodeToCSHeartbeatTask.Start()", "NodeToCS Heartbeat Task started successfully.");
            }
        }

        public void Pause()
        {
            _startSignal.Reset();
        }

        public void Resume()
        {
            _startSignal.Set();
        }

        public bool IsPaused
        {
            get { return !_startSignal.WaitOne(0); }
        }


        public void Run()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = "" });
            _context.StatusLatch.WaitForAny(NodeStatus.Running);
            _startSignal.WaitOne();

            if (_context.ConfigurationSession != null)
            {
                ((OutProcConfigurationSession)_context.ConfigurationSession).RegisterListener(this);
            }

            while (_running)
            {
                try
                {
                    if (_node == null)
                        _clusterConfigMgr.UpdateClusterConfiguration();

                    if(_clusterConfigMgr != null)
                    {
                        ShardConfiguration sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                        if(sConfig != null && sConfig.Servers != null)
                        {
                            _node = sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString());
                        }
                    }
                    if (_node == null && LoggerManager.Instance.ShardLogger != null &&
                        LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Warn("NodeToCSHeartbeatTask.Run() ","Node " + _context.LocalAddress.ToString() +
                                                                " is not part of the configuration.");
                        return;
                    }
                    OperationId lastOpId = null;
                    if (_membershipManager != null && _membershipManager.LatestMembership != null && _membershipManager.LatestMembership.Primary != null && _membershipManager.LatestMembership.Primary.Name.Equals(_context.LocalAddress.IpAddress.ToString()))
                        lastOpId = _membershipManager.GetLastOperationId;
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    _heartbeatInterval =
                        _context.ConfigurationSession.ReportHeartbeat(_context.ClusterName, _context.LocalShardName,
                            _node, _membershipManager.LatestMembership, lastOpId) * 1000;
                    watch.Stop();
                    _csStatus = ConnectivityStatus.Connected;
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("NodeToCSHeartbeatTask.Run() ", "Heartbeat sent to the CS at " +
                                                                 DateTime.Now.ToString() + " time taken to report heartbeat :" + watch.Elapsed.TotalSeconds);
                    if (_heartbeatInterval > 0 && (watch.Elapsed.TotalSeconds > _heartbeatInterval / 2))
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("NodeToCSHeartbeatTask.Run() ", "Heartbeat sent to the CS at " +
                                                                 DateTime.Now.ToString() + " time taken to report heartbeat :" + watch.Elapsed.TotalSeconds + " which is greater than half of the hb interval.");
                    }
                }
                catch (ThreadAbortException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled && _nodeToCSThread != null)
                    {
                        LoggerManager.Instance.ShardLogger.Error(_nodeToCSThread.Name, "Task aborted.");
                    }
                    break;
                }
                //the following should only be done when a connection exception occurs.
                catch (ChannelException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null)
                    {
                        if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("NodeToCSHeartbeatTask.Run()  ",  e.ToString());
                        if (LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("NodeToCSHeartbeatTask.Run() ",
                                "On CS disconnected process of the membership manager begins execution at " +
                                DateTime.Now.ToString());
                    }

                    _csStatus = ConnectivityStatus.NotConnected;
                    _membershipManager.OnCSDisconnected();
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("NodeToCSHeartbeatTask.Run() ", "The NodeToCS task paused at " +
                                                                 DateTime.Now.ToString());

                    //foreach (var cluster in ManagementServer.s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations)
                    //{
                    //    string csIp = cluster.ConfigServers.Nodes[0].Name;
                    //    int csPort = cluster.ConfigServers.Nodes[0].Port;

                    //    BrokenConnectionInfo info = new BrokenConnectionInfo();
                    //    info.BrokenAddress = new Address(csIp, csPort);
                    //    info.SessionType = Common.Communication.SessionTypes.Management;

                    //    _connectionRestoration.RegisterListener(info, this,_context.LocalShardName);
                    //}
      

                    this.Pause();

                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Error("NodeToCSHbTask.Run() General Exception: ",
                            e.ToString());
                    }

                    if (e.Message.Contains("No configuration server is available to process the request"))
                    {
                        _csStatus = ConnectivityStatus.NotConnected;
                        _membershipManager.OnCSDisconnected();
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("NodeToCSHeartbeatTask.Run() ", "The NodeToCS task paused at " +
                                                                     DateTime.Now.ToString());
                        this.Pause();
                    }
                }
                Stopwatch sleepWatch = new Stopwatch();
                sleepWatch.Start();
                Thread.Sleep(_heartbeatInterval);
                _startSignal.WaitOne();
                sleepWatch.Stop();

                if(sleepWatch.Elapsed.TotalMilliseconds > (_heartbeatInterval + 2000))
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("NodeToCSHeartbeatTask.Run() ", "CS task waited for  " + sleepWatch.Elapsed.TotalSeconds);

                }

            }

        }

        #region IConnectionRestorationListener Implementation
        public void OnConnectionRestoration(Common.Communication.IDualChannel channel)
        {
            //To-do: Add logic here.
            if (IsPaused)
                this.Resume();
        }

        public string Name
        {
            get { return "CSHeartbeatTask: " + _context.LocalShardName; }
        }
        #endregion






        public void OnSessionDisconnected(Common.Configuration.Services.IConfigurationSession session)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(_context.LocalShardName + ".CSHearbeatTask", "connection disconnected with configuration server");
        }

        public void OnSessionConnected(Common.Configuration.Services.IConfigurationSession session)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(_context.LocalShardName +  ".CSHearbeatTask", "connection restored with configuration server");

            if (IsPaused)
                this.Resume();
        }
    }
}
