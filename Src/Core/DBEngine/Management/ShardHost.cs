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
using Alachisoft.NosDB.Common.Configuration.RPC;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Core.DBEngine;
using Alachisoft.NosDB.Core.Statistics;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using System;
using System.Configuration;
using System.Net;
using Alachisoft.NosDB.Serialization;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Core.Monitoring;
using Alachisoft.NosDB.Core.Security.Interfaces;
using System.IO;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Core.Security.Impl;
using Alachisoft.NosDB.Common;
using System.Diagnostics;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Core.Configuration;


namespace Alachisoft.NosDB.Core
{
    public class ShardHost:IDisposable
    {

        public ISecurityManager SecurityManager
        {
            set
            {
                _nodeContext.SecurityManager = value;
            }
            get
            {
                return _nodeContext.SecurityManager;
            }
        }
        
        private NodeContext _nodeContext;
        private ClientSessionManager _clientSessionManager;
        private MonitorSessionListener _monitorSessionListener;
        private IDatabaseEngineFactory _databaseEngineFactory;
        private bool _initialized = false;
        private bool _running = false;
       
        public ShardHost()
        {
            _nodeContext = new NodeContext();
            //_nodeContext.LocalShardName = "Shard1";
            _databaseEngineFactory = new DatabaseEngineFactory(_nodeContext);
            _nodeContext.ShardServer = new ShardServer();
            _clientSessionManager = new ClientSessionManager(_databaseEngineFactory);
            _nodeContext.ShardStatsCollector = new ShardStatsCollector();
            _monitorSessionListener = new MonitorSessionListener(_clientSessionManager, _nodeContext);
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public NodeContext NodeContext
        {
            get { return _nodeContext; }
        }


        public void Initialize(DbmClusterConfiguration configurationClusterConfig, string ipAddress, int port,  string clusterName, string shardName)
        {
            try
            {
                bool isConfigSessionInit = false;
                Exception csInitExc = null; 
                _nodeContext.ShardServer.Initialize(IPAddress.Parse(ipAddress), port);
                _nodeContext.ShardServer.RegisterSessionListener(SessionTypes.Client, _clientSessionManager);
                _nodeContext.ClusterName = clusterName;

                foreach (var configServer in configurationClusterConfig.ConfigServers.Nodes)
                {
                    try
                    {
                        if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                        {
                            LoggerManager.Instance.ServerLogger.Info("ShardHost.Initialize()", "going to connect with configurration server " + configServer.Name + ":" + configServer.Port);
                        }

                        DatabaseRPCService rpc = new DatabaseRPCService(configServer.Name, configServer.Port);
                        IConfigurationServer remote = rpc.GetConfigurationServer(new TimeSpan(0, 0, 90), SessionTypes.Management, new ConfigurationChannelFormatter());
                        remote.MarkDatabaseSesion();
                        _nodeContext.ConfigurationSession = remote.OpenConfigurationSession(new SSPIClientAuthenticationCredential());
                        if (string.Compare(clusterName, MiscUtil.LOCAL, true) == 0)
                        {
                            ((Alachisoft.NosDB.Common.Configuration.Services.Client.OutProcConfigurationSession)_nodeContext.ConfigurationSession).MarkSessionInternal();
                            ((Alachisoft.NosDB.Common.Configuration.Services.Client.OutProcConfigurationSession)_nodeContext.ConfigurationSession).SwithToActiveNode = false;
                        }
                        

                        
                    }
                    catch(Exception e)
                    {
                        if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.ServerLogger.Error("ShardHost.Initialize()", "Failed to connect with " + configServer.Name + ":" + configServer.Port,e );
                        }
                        csInitExc = e;
                    }
                    
                    if(_nodeContext.ConfigurationSession != null)
                    {
                        isConfigSessionInit = true;
                        break;
                    }
                }
                if (isConfigSessionInit)
                {
                    _nodeContext.LocalShardName = shardName;
                    _clientSessionManager.ShardName = shardName;
                    LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _nodeContext.LocalShardName != null ? _nodeContext.LocalShardName : "", DatabaseName = "" });


                    SecurityManager = new SecurityManager();
                    SecurityManager.Initialize(this.NodeContext.LocalShardName);
                    _initialized = true;
                }
                else
                {
                    if (csInitExc != null)
                        throw csInitExc;
                }
            }
            catch (Exception ex)
            {
                _initialized = false;
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Error("ShardHost.Initialize()", "Error:", ex);
                }
                throw;
            }
        }

        public void Start(ClusterConfiguration clusterConf)
        {
            if (!_initialized)
                throw new Exception("Shard host is not initialized. ");
            try
            {
                //IDatabaseEngine engine =_databaseEngineFactory.GetDatabaseEngine();
                DataBaseEngine engine = _databaseEngineFactory.GetDatabaseEngine() as DataBaseEngine;
                if (engine != null)
                {
                    engine.Start(clusterConf);
                }
                else 
                {
                    throw new Exception("Database Engine not initialized. ");
                }
                                
                _nodeContext.ShardServer.Start();
                
                _clientSessionManager.RegisterClientDisconnectListerner(_nodeContext.SecurityManager);
                _nodeContext.ShardServer.RegisterSessionListener(SessionTypes.Client, _clientSessionManager);
                _nodeContext.ShardServer.RegisterSessionListener(SessionTypes.Monitoring, _monitorSessionListener);

                try
                {
                    if (_nodeContext.ShardStatsCollector == null)
                    {
                        _nodeContext.ShardStatsCollector=new ShardStatsCollector();
                    }
                    _nodeContext.ShardStatsCollector.Initialize(_nodeContext.LocalShardName);
                    _nodeContext.ShardStatsCollector.SetStatsValue(StatisticsType.PendingReplicatedOperation, 0);
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.ServerLogger.Error(
                            _nodeContext.LocalShardName + " Perfmon Counters Initialization ", ex.Message);
                    }
                }

                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Info("ShardHost.Start()", "Shard Host started successfully.");
                }

                string logMessage = string.Format("Node {0}:{1} for shard \"{2}\" has successfully started.", 
                    _nodeContext.ShardServer.BindingIp, _nodeContext.ShardServer.Port, _nodeContext.LocalShardName);
                AppUtil.LogEvent(logMessage, System.Diagnostics.EventLogEntryType.Information);

                _running = true;
            }
            catch (Exception ex)
            {
                _running = false;
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Error("ShardHost.Start()", "Error:", ex);
                }
                string logMessage = string.Format("Node {0}:{1} for shard \"{2}\" cannot be started. \n {3}",
                    _nodeContext.ShardServer.BindingIp, _nodeContext.ShardServer.Port, _nodeContext.LocalShardName, ex.ToString());
                AppUtil.LogEvent(AppUtil.EventLogSource, logMessage, EventLogEntryType.Error, EventCategories.Error, EventID.ShardStartError);
                throw;
            }
        }

        public bool Stop(bool destroy)
        {
            try
            {
                _databaseEngineFactory.GetDatabaseEngine().Stop(destroy);
                _nodeContext.ShardServer.Stop();
                if (_nodeContext.ShardStatsCollector != null)
                    _nodeContext.ShardStatsCollector.Dispose();

                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                {
                    if(destroy)
                        LoggerManager.Instance.ServerLogger.Info("ShardHost.Stop()", "Shard Host stopped & removed from the shard successfully.");
                    else
                        LoggerManager.Instance.ServerLogger.Info("ShardHost.Stop()", "Shard Host stopped successfully.");
                }
                string logMessage = string.Format("Node {0}:{1} for shard \"{2}\" has stopped successflly",
                    _nodeContext.ShardServer.BindingIp, _nodeContext.ShardServer.Port, _nodeContext.LocalShardName);
                AppUtil.LogEvent(logMessage, System.Diagnostics.EventLogEntryType.Information);
                _running = false;
                return true;
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Error("ShardHost.Stop()", "Error:", ex);
                }

                string logMessage = string.Format("Node {0}:{1} for shard \"{2}\" cannot be stoped. \n {3}",
                    _nodeContext.ShardServer.BindingIp, _nodeContext.ShardServer.Port, _nodeContext.LocalShardName, ex.ToString());
                AppUtil.LogEvent(AppUtil.EventLogSource, logMessage, EventLogEntryType.Error, EventCategories.Error, EventID.ShardStopError);
                throw;
            }

            return false;
            
        }

        public bool RemoveShard()
        {
            try
            {
                Stop(true);

                //Line below is commented becuase there is no shard folder exists now.
                // Directory.Delete(_nodeContext.BasePath, true); 
                return true;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Error RemoveShard()", e.ToString());
                throw;
            }
        }

        public void Dispose()
        {
            _clientSessionManager.Dispose();
            _nodeContext.ShardServer.Dispose();
            _nodeContext.DatabasesManager.Dispose();
        }

        public void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId)
        {
            _clientSessionManager.PublishAuthenticatedUserInfoToDBServer(sessionId);
        }
    }
}
