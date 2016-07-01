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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Logger;
using System.Threading;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ConfigServerHeartBeatTask 
    {
        private readonly ConfigurationServer _configServer;
        private readonly int _interval=2000;
        private readonly int _maxInterval = 50;
        private readonly string _cluster;
        private Thread _mainThread;

        public ConfigServerHeartBeatTask(string cluster, ConfigurationServer configServer,  int heartBeatInterval,int interval)
        {
            _configServer = configServer;
            _interval = interval;
            _cluster = cluster;
            _maxInterval = 3 * (heartBeatInterval == 0 ? 10 : heartBeatInterval);
        }
        
        public void Start(string shardName)
        {
            lock (this)
            {
                if (_mainThread == null || !_mainThread.IsAlive)
                {
                    _mainThread = new Thread(() => Run(shardName))
                    {
                        Name = "CheckHeartbeat." + _cluster,
                        IsBackground = true
                    };
                    _mainThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (_mainThread != null && _mainThread.IsAlive)
                {
                    _mainThread.Abort();
                }

                _mainThread = null;
            }
        }

        private bool IsNodeExist(string clusterName, string shardName, IEnumerable<ServerNode> nodes, ServerNode existingNode)
        {
            try
            {
                bool exist = false;
                if (nodes != null)
                {
                    foreach (ServerNode node in nodes)
                    {
                        Membership nodeMembership = _configServer.GetNodeMemberShip(clusterName, shardName, node.Name);

                        if (nodeMembership != null && nodeMembership.Servers != null && nodeMembership.Servers.Contains(existingNode))
                        {
                            DateTime? lastHeartbeat = _configServer.GetLastHeartBeat(clusterName, shardName, node);

                            if (lastHeartbeat.HasValue && lastHeartbeat.Value.AddSeconds(_maxInterval) > DateTime.Now)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                }

                return exist;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ConfigServerHeartBeatTask.IsNodeExist() ",e.ToString());
                   throw;
            }
        }

        public void Run(string shardName)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext(){ShardName = shardName ?? "",DatabaseName = ""});
            while (true)
            {
                try
                {
                    var info = _configServer.GetDatabaseClusterInfo(_cluster);

                    if (info != null && info.ShardInfo != null && info.ShardInfo.Values!=null)
                    {
                        foreach (ShardInfo sInfo in info.ShardInfo.Values)
                        {
                            if (sInfo != null && sInfo.RunningNodes != null)
                            {
                                try
                                {
                                    IList<ServerInfo> runningNodesInfo = sInfo.RunningNodes.Values.ToList();
                                    foreach (ServerInfo serverInfo in runningNodesInfo)
                                    {
                                        DateTime? lastHeartbeat = _configServer.GetLastHeartBeat(info.Name, sInfo.Name, _configServer.GetServerNodeFromServerInfo(serverInfo, 0));
                                        ServerNode node = _configServer.GetServerNodeFromServerInfo(serverInfo, 0);
                                        if (lastHeartbeat.HasValue)
                                        {
                                            if (lastHeartbeat.Value.AddSeconds(_maxInterval) < DateTime.Now)
                                            {
                                                Membership nodeMembership =_configServer.GetNodeMemberShip(info.Name, sInfo.Name,serverInfo.Address.IpAddress.ToString());

                                                if (nodeMembership == null ||!IsNodeExist(info.Name, sInfo.Name, nodeMembership.Servers, node))
                                                {
                                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                                        LoggerManager.Instance.CONDBLogger.Info("CheckHeartbeatTask", "did not receive heartbeat from " + node.Name + "(shard:" + sInfo.Name + ") since " + lastHeartbeat.Value +", therefore removing from membership");
                                                    
                                                    _configServer.RemoveNodeFromMembership(info.Name, sInfo.Name, node);
                                                }
                                                else
                                                {
                                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                                        LoggerManager.Instance.CONDBLogger.Info( "CheckHeartbeatTask", "did not receive heartbeat from " + node.Name + "(shard:" + sInfo.Name + ") since " + lastHeartbeat.Value +" , however other members reported that he is alive");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                                LoggerManager.Instance.CONDBLogger.Info("CheckHeartbeatTask", "did not receive heartbeat from " + node.Name + "(shard:" + sInfo.Name + ") , therefore removing from membership");
                                            
                                            _configServer.RemoveNodeFromMembership(info.Name, sInfo.Name, node);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                        LoggerManager.Instance.CONDBLogger.Error( "ConfigServerHeartBeatTask.Run() ", ex.ToString());
                                }
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        LoggerManager.Instance.CONDBLogger.Info("CheckHeartbeatTask", "exiting thread");
                    break;

                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("ConfigServerHeartBeatTask.Run() ", e.ToString());
                }

                Thread.Sleep(_interval);
            }
        }

    }
}
