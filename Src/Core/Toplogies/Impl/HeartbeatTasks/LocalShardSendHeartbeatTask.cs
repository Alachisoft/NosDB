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
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Core.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;

using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks
{
    /// <summary>
    /// This class sends heart beat to the nodes after a set interval.
    /// </summary>
    public class LocalShardSendHeartbeatTask
    {
        private int _poolingThreshold = 2 * 1000; //time set for the next heartbeat.
        private NodeContext _nodeContext = null;

        private System.Threading.ManualResetEvent _startSignal;
        private volatile bool _running;
        private IShard _shard = null;
        private MembershipManager _membershipManager = null;

        private Thread _sendHbThread = null;
        private ClusterConfigurationManager _clusterConfigMgr=null;
        private Object mutex = new Object();


        public LocalShardSendHeartbeatTask(NodeContext nodeContext, IShard shard, MembershipManager membershipManager, ClusterConfigurationManager clusterConfigMgr)
        {
            this._nodeContext = nodeContext;
            this._clusterConfigMgr = clusterConfigMgr;
            ShardConfiguration config = _clusterConfigMgr.GetShardConfiguration(_nodeContext.LocalShardName);//_nodeContext.ConfigurationSession.GetDatabaseClusterConfiguration(_nodeContext.ClusterName).Deployment.GetShardConfiguration(_nodeContext.LocalShardName);

            if (config == null)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("LocalShardSendHeartTask", "shard configuration is null");
                }
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException(" shard configuration is null");
            }

            if (config.NodeHeartbeatInterval > 0)
                this._poolingThreshold = (config.NodeHeartbeatInterval) * 1000;
            this._shard = shard;
            this._membershipManager = membershipManager;

            _running = false;
            _startSignal = new System.Threading.ManualResetEvent(false);
        }

        // The method which runs in a thread.
        public void Run()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _nodeContext.LocalShardName != null ? _nodeContext.LocalShardName : "", DatabaseName = "" });
            _nodeContext.StatusLatch.WaitForAny(NodeStatus.Running);
            _startSignal.WaitOne();
            while (_running)
            {
                try
                {
                    // 1. update the local node information
                    HeartbeatInfo info = UpdateLocalNodeData();
                    if (info != null)
                    {
                        // 2. Send the heartbeat info to the cluster.
                        SendHeartbeat(info);
                        if (LoggerManager.Instance.ShardLogger != null &&
                            LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartbeatTask.Run() ","Heartbeat broadcasted from node " + _nodeContext.LocalAddress.IpAddress.ToString() + " at " + DateTime.Now.ToString());
                    }
                }
                catch(ThreadAbortException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled && _sendHbThread != null)
                    {
                        LoggerManager.Instance.ShardLogger.Error(_sendHbThread.Name, "Task aborted.");
                    }
                    break;
                }
                catch(ChannelException ex)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("LocalShardSendHeartbeatTask.Run() ",  ex.ToString());
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("LocalShardSendHeartbeatTask.Run()  " + ex.ToString());
                }
                Thread.Sleep(_poolingThreshold);
                _startSignal.WaitOne();

            }
        }

        internal void SendHeartbeat(HeartbeatInfo info)
        {
            List<Server> list = new List<Server>();
            if (_shard != null && _shard.ActiveChannelsList != null)
            {
                Message msg = new Message();
                msg.MessageType = MessageType.Heartbeat;
                msg.Payload = info;
                msg.NeedsResponse = false;

                ShardMulticastRequest<ResponseCollection<object>, object> request = _shard.CreateMulticastRequest<ResponseCollection<object>, object>(_shard.ActiveChannelsList, msg);
                IAsyncResult result = request.BeginExecute();

                ResponseCollection<Object> response = request.EndExecute(result);
            }
        }

        internal HeartbeatInfo UpdateLocalNodeData()
        {
            HeartbeatInfo info = new HeartbeatInfo();
            if (_nodeContext != null)
            {
                info.CurrentMembership = _membershipManager.LatestMembership;

               
               
                    info.LastOplogOperationId = null;

                _membershipManager.UpdateLastOperationId(info.LastOplogOperationId);
                info.CSStatus = _membershipManager.CSStatus;
            }
            return info;
        }

        public void Stop()
        {
            lock (mutex)
            {
                _running = false;
                _startSignal.Set();

                if (_sendHbThread != null)
                    _sendHbThread.Abort();
            }
        }

        public void Start()
        {
            lock (mutex)
            {
                if (_sendHbThread == null)
                {
                    _sendHbThread = new Thread(new ThreadStart(Run));
                }
                _sendHbThread.IsBackground = true;
                _sendHbThread.Name = _nodeContext.LocalShardName + ".LSSendHBTask";
                _sendHbThread.Start();

                _running = true;
                _startSignal.Set();
            }
        }

    }
}
