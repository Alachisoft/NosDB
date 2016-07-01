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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Attachments;
using Alachisoft.NosDB.Common.Storage.Provider;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Core.DBEngine.Management;
using Exception = System.Exception;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Configuration.Recovery;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Core.Security.Impl;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.Client;
using DeploymentConfiguration = Alachisoft.NosDB.Common.Configuration.DeploymentConfiguration;
using ShardInfo = Alachisoft.NosDB.Common.Configuration.Services.ShardInfo;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ConfigurationServer : IConfigurationServer, IShardListener, IRecoveryCommunicationHandler, IConfigOperationExecutor, ITransactionListener
    {
        private ConfigurationStore _configurationStore;
        private NodeContext _nodeContext;
        private string _filePath = "";
        private string _databaseConfingFileName = "";
        private string _configServerConfigurationFileName = "";
        private ReaderWriterLock _rwLock = new ReaderWriterLock();
        private ReaderWriterLock _rwHeartBeatLock = new ReaderWriterLock();
        private MembershipData _membershipMetadatastore;
        private Hashtable _electionLockingMap = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        private IDictionary<DualChannel, bool> _connectedClientList;
        private List<DualChannel> _connectedNodes;
        private HeartbeatReporting _heartbeatreporting;
        private bool _isPassive;
        private ConfigurationSession _localSesssion;
      public Hashtable _dbMgtSessions = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private Hashtable _nodeMembership = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private bool _isConfigClusterEstablished;
        private ConfigServerConfiguration _configurationServerConfig = new ConfigServerConfiguration();
        private static System.Timers.Timer _evalWarningTask;
        private System.Timers.Timer _reactWarningTask;
        private IRecoveryManager _recoveryManager;
        private GracefulRemovalMonitoring _gracefulRemovalMonitoring;
        private ISecurityManager _securityManager;
        private Dictionary<string, ConfigServerHeartBeatTask> _checkHeartbeatTasks = new Dictionary<string, ConfigServerHeartBeatTask>(StringComparer.InvariantCultureIgnoreCase);
        private ConfigurationCluster _cfgCluster;
        private bool _start = false;
        private HashSet<string> _reservedWords; 


        public ISecurityManager SecurityManager
        {
            get { return _securityManager; }
        }

       
        public bool IsStarted
        {
            get { return _start; }

            set { _start = value; }
        }

        public bool IsConfigClusterEstablished
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_configurationServerConfig.Name))
                {
                    _isConfigClusterEstablished = false;
                }
                else
                {
                    _isConfigClusterEstablished = true;
                }
                return _isConfigClusterEstablished;
            }
        }

        public ConfigurationStore ConfigurationStore
        {
            get { return _configurationStore; }
        }

        public ConfigurationServer()
        {
            try
            {
                string basePath = ConfigurationSettings<CSHostSettings>.Current.BasePath;
                string csConfigFile = ConfigurationSettings<CSHostSettings>.Current.ConfigurationFile;
                //_fileName = configPath + "\\database.conf";
                _filePath = basePath;
                _databaseConfingFileName = _filePath + "\\database.conf";

                _configServerConfigurationFileName = csConfigFile + "configcluster.config";

                if (!File.Exists(_configServerConfigurationFileName))
                {
                    //DONT REMOVE USING PLZ
                    using (FileStream fs = File.Create(_configServerConfigurationFileName))
                    {
                    }
                }
                //_configServerLogger.Initialize(LoggerNames.ConfigurationServer, "Configuration Server");
                DatabaseConfiguration dbConfig = GetDatabaseConfiguration();
                InitilizeNodeContext();
                _configurationStore = new ConfigurationStore(dbConfig, _nodeContext);
                _configurationStore.Initialize();
                _membershipMetadatastore = new MembershipData(ConfigurationStore);

                _connectedClientList = new Dictionary<DualChannel, bool>();
                _connectedNodes = new List<DualChannel>();
                _heartbeatreporting = new HeartbeatReporting();

                _recoveryManager = new RecoveryManager(this, ConfigurationStore);
                _recoveryManager.RegisterRecoveryCommunicationHandler(this);

                _heartbeatreporting.Load();
                _configurationStore.RegiserTransactionListener(this);

                _gracefulRemovalMonitoring = new GracefulRemovalMonitoring(this);

            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ConfigurationServer", ex.ToString());
                throw;
            }

        }

        public void Start()
        {
            try
            {
                InitializeReservedWords();
                _rwLock.AcquireWriterLock(Timeout.Infinite);
               
                #region Security Initialization

                try
                {
                    _securityManager = new SecurityManager();
                    _securityManager.Initialize(MiscUtil.CONFIGURATION_SHARD_NAME);

                    _securityManager.SecurityServer = new DbSecurityServer();
                    ((DbSecurityServer) _securityManager.SecurityServer).Initialize(this);

                    var securityDatabase = new CSSecurityDatabase();
                    securityDatabase.Initialize(_configurationStore);

                    _securityManager.AddSecurityDatabase(MiscUtil.CONFIGURATION_SHARD_NAME,
                        securityDatabase);

                    _securityManager.InitializeSecurityInformation(MiscUtil.CONFIGURATION_SHARD_NAME);
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.SecurityLogger.Info("ConfigurationServer.Start", "Security initialized");
                    }

                }
                catch (Exception exc)
                {
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.SecurityLogger.Error("ConfigurationServer.Start", exc);
                    }
                    throw exc;
                }


                #endregion
                //Licensing.LicenseMonitor.VerifyLicenseMonitoring();
                //Licensing.LicenseManager.LicenseType type = Licensing.LicenseManager.LicenseMode(null);
                //if (type == Licensing.LicenseManager.LicenseType.InEvaluation)
                //{
                //    _evalWarningTask = new System.Timers.Timer();
                //    _evalWarningTask.Interval = 1000 * 60 * 60 * 12;// 12 hour interval.
                //    _evalWarningTask.Elapsed += new ElapsedEventHandler(NotifyEvalLicense);
                //    _evalWarningTask.Enabled = true;
                //    NotifyEvalLicense(null, null);
                //}
                //else if (type == Licensing.LicenseManager.LicenseType.Expired)
                //{
                //    string message = "Your license for NosDB has been expiered. Please contact sales@alachisoft.com for further terms and conditions.";
                //    AppUtil.LogEvent(message, EventLogEntryType.Error);
                //    throw new LicensingException(message);
                //}
                //if (Alachisoft.NosDB.Core.Licensing.LicenseManager.Reactivate)
                //{
                //    _reactWarningTask = new System.Timers.Timer();
                //    _reactWarningTask.Interval = 1000 * 60 * 60 * 24;//1 day
                //    _reactWarningTask.Elapsed += new ElapsedEventHandler(NotifyReactivateLicense);
                //    _reactWarningTask.Enabled = true;
                //    NotifyReactivateLicense(null, null);
                //}

               
                LoadConfigServerConfiguration();
                
                _isConfigClusterEstablished = !string.IsNullOrWhiteSpace(_configurationServerConfig.Name);
                
                LoadConfiguration();

                StartConfigurationCluster();
                IsStarted = false;

                StartShardRemovalTask();

                //Start Local shard instance with provided configruation and register this class as shard listerner
                _localSesssion = OpenConfigurationSession(new SSPIClientAuthenticationCredential()) as ConfigurationSession;

                foreach (ClusterConfiguration cc in _configurationStore.GetAllClusterConfiguration())
                {
                    if (cc != null && cc.Deployment != null)
                    {
                        StartHeartbeatChekcTask(cc);
                    }
                }
                CreateLocalCluster();
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                        LoggerManager.Instance.ServerLogger.Info("ConfigurationServer.Start()", "ConfigurationServer is started successfully.");
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("Start", ex.ToString());
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

        }

       

        private void InitializeReservedWords()
        {
            _reservedWords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            _reservedWords.Add("local");
        }

        private bool IsReservedWord(string word)
        {
            if (_reservedWords.Contains(word))
                return true;
            return false;
        }

        internal void BeginTakeOver()
        {
            _cfgCluster.BeginTakeOver();
        }

        private void CreateLocalCluster()
        {
            try
            {
                RemoteManagementSession rmtSession = GetManagementSession(ConfigurationSettings<CSHostSettings>.Current.IP.ToString());
                rmtSession.CreateLocalCluster();
            }
            catch (Exception exc)
            { }
        }

        internal NodeRole GetCurrentRole()
        {
            if (_cfgCluster == null) return NodeRole.None;
            return _cfgCluster.CurrentRole;
        }

        public bool HasSynchronizedWithPrimaryConfigServer()
        {
            return _cfgCluster.HasSynchrnonized;
        }

        private void StartShardRemovalTask()
        {
            _gracefulRemovalMonitoring = new GracefulRemovalMonitoring(this);
            if (ConfigurationStore != null)
            {
                ClusterInfo[] infos = ConfigurationStore.GetAllClusterInfo();

                if (infos != null)
                {
                    foreach (ClusterInfo info in infos)
                    {
                        if (info != null)
                        {
                            IList<string> shardsTobeRmeoved = info.GetShardsUnderGracefullRemoval();
                            if (shardsTobeRmeoved != null)
                            {
                                foreach (string shard in shardsTobeRmeoved)
                                {
                                    _gracefulRemovalMonitoring.Add(new GracefullShardInfo(info.Name, shard, this));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            try
            {
                _connectedNodes.Clear();
                _connectedClientList.Clear();
                ConfigurationStore.Dispose();

                if (_cfgCluster != null)
                    _cfgCluster.Dispose();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("Stop", ex.ToString());
            }
        }

        private void AssignObjectUID(IObjectId uniqueObject)
        {
            if (uniqueObject != null && string.IsNullOrEmpty(uniqueObject.UID))
            {
                uniqueObject.UID = Guid.NewGuid().ToString();
            }
        }

        private ShardConfiguration GetShardConfigurationForLocal()
        {
            String serverName = NetworkUtil.GetLocalIPAddress().ToString();

            if (ConfigurationSettings<CSHostSettings>.Current != null && ConfigurationSettings<CSHostSettings>.Current.IP != null)
                serverName = ConfigurationSettings<CSHostSettings>.Current.IP.ToString();



            ShardConfiguration shardConfiguration = new ShardConfiguration();
            shardConfiguration.Name = MiscUtil.LOCAL;
            shardConfiguration.Port = ConfigurationSettings<CSHostSettings>.Current.LocalShardPort;
            shardConfiguration.Servers = new ServerNodes();

            ServerNode server = new ServerNode();
            server.Name = serverName;
            server.Priority = 0;
            shardConfiguration.Servers.AddNode(server);

            return shardConfiguration;
        }

        private ClusterConfiguration GetClusterConfiguration(int heartBeat, string displayName, ReplicationConfiguration repConfig)
        {

            var clusterConfiguration = new ClusterConfiguration
            {
                Name = MiscUtil.CLUSTERED,
                Deployment = new DeploymentConfiguration { HeartbeatInterval = heartBeat }
            };
            clusterConfiguration.Databases = new DatabaseConfigurations();
            clusterConfiguration.Deployment.Replication = repConfig;
            clusterConfiguration.DisplayName = displayName;

            //I commented out following lines because I believed that this code was redundant. Is also being called from CreateCluster method, hence overwriting it. Could be wrong, only testing would tell this.
            //ClusterInfo clusterInfo = GetClusterInfoFromClusterConfiguration(clusterConfiguration);

            //if (clusterInfo != null)
            //    _configurationStore.InsertOrUpdateClusterInfo(clusterInfo);
            return clusterConfiguration;
        }

        private ClusterConfiguration GetLocalDatabaseConfiguration()
        {
            ClusterConfiguration clusterConfiguration = new ClusterConfiguration
            {
                DisplayName = MiscUtil.STAND_ALONE,
                Name = MiscUtil.LOCAL,
                Deployment = new DeploymentConfiguration()
            };
            clusterConfiguration.Deployment.AddShard(GetShardConfigurationForLocal());
            clusterConfiguration.Databases = new DatabaseConfigurations();

            ConfigurationStore.Transaction transaction = ConfigurationStore.BeginTransaction(MiscUtil.LOCAL, false);

            ClusterInfo clusterInfo = GetClusterInfoFromClusterConfiguration(clusterConfiguration);

            if (clusterInfo != null)
                transaction.InsertOrUpdateClusterInfo(clusterInfo);

            return clusterConfiguration;
        }

        private int GetNextFreePort()
        {
            int port = 2100;
            foreach (ClusterConfiguration clusterConfiguration in ConfigurationStore.GetAllClusterConfiguration())
            {
                if (clusterConfiguration.Deployment != null && clusterConfiguration.Deployment.Shards != null && clusterConfiguration.Deployment.Shards.Count > 0)
                {
                    foreach (KeyValuePair<string, ShardConfiguration> shardConfig in clusterConfiguration.Deployment.Shards)
                    {
                        if (port == shardConfig.Value.Port)
                            port++;
                    }
                }
            }

            return port;
        }

        public void InitilizeNodeContext()
        {

            _nodeContext = new NodeContext
            {
                ClusterName = "configurationcluster",
                LocalShardName = MiscUtil.CONFIGURATION_SHARD_NAME
            };
            //_nodeContext.ConfigurationSession=new InProcConfigurationSession();
            string basePath = ConfigurationSettings<CSHostSettings>.Current.BasePath;
            _nodeContext.BasePath = basePath + _nodeContext.LocalShardName + "\\";
            IPAddress localIp = GetLocalAddress();
            _nodeContext.LocalAddress = new Address(localIp, NetworkUtil.DEFAULT_CS_HOST_PORT);


        }

        public IPAddress GetLocalAddress()
        {
            #region Getting Local Address Logic; might be replace with getting address from service config

            IPAddress localAddress = null;

            string localIP = ConfigurationSettings.AppSettings["ManagementServerIP"];
            if (!string.IsNullOrEmpty(localIP))
            {
                try
                {
                    localAddress = System.Net.IPAddress.Parse(localIP);
                }
                catch (Exception ex)
                {

                    localAddress = NetworkUtil.GetLocalIPAddress();
                }
            }
            else
            {
                localAddress = NetworkUtil.GetLocalIPAddress();
            }
            return localAddress;
            #endregion

        }

        private DatabaseConfiguration GetDatabaseConfiguration()
        {
            var DatabaseConfiguration = new DatabaseConfiguration();

            DatabaseConfiguration.Name = MiscUtil.CONFIGURATION_DATABASE;

          

            DatabaseConfiguration.Storage = new StorageConfiguration();

            DatabaseConfiguration.Storage.Collections = new CollectionConfigurations();
            DatabaseConfiguration.Storage.CacheConfiguration = new CachingConfiguration();
            DatabaseConfiguration.Storage.CacheConfiguration.CachePolicy = "fcfs";
            DatabaseConfiguration.Storage.CacheConfiguration.CacheSpace = MiscUtil.DEFAULT_CACHE_SPACE;
            DatabaseConfiguration.Storage.StorageProvider = new StorageProviderConfiguration();
            DatabaseConfiguration.Storage.StorageProvider.StorageProviderType = ProviderType.LMDB;
            DatabaseConfiguration.Storage.StorageProvider.MaxFileSize = MiscUtil.MAX_FILE_SIZE;
            DatabaseConfiguration.Storage.StorageProvider.DatabaseId = "ConfigurationDatabase";
            DatabaseConfiguration.Storage.StorageProvider.IsMultiFileStore = false;

            DatabaseConfiguration.Storage.StorageProvider.LMDBProvider = new LMDBConfiguration();
            DatabaseConfiguration.Storage.StorageProvider.LMDBProvider.EnvironmentOpenFlags = LMDBEnvOpenFlags.NoSubDir;
            DatabaseConfiguration.Storage.StorageProvider.LMDBProvider.MaxReaders = 126;

            int maxCol = 0;
            // MembershipInfo
            maxCol += 1;
            // ClusterInfo
            maxCol += 1;
            // ClusterInfo.Collections
            maxCol += 1;
            // ClusterInfo.Collections.Buckets
            maxCol += 1;
            // CousterConfig
            maxCol += 1;
            // SecurityInfo
            maxCol += 1;
            // UserInfo
            maxCol += 1;
            // RoleInfo
            maxCol += 100;

            DatabaseConfiguration.Storage.StorageProvider.LMDBProvider.MaxCollections = maxCol;

            return DatabaseConfiguration;
        }

        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);

                ConfigurationStore.Transaction transaction = ConfigurationStore.BeginTransaction(clusterName, false);

                if (clusterName != null && transaction.ContainsCluster(clusterName.ToLower()))
                    return transaction.GetClusterConfiguration(clusterName);

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetDatabaseClusterConfiguration", clusterName.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return null;
        }

        public Dictionary<string, int> GetShardsPort(string clusterName, ISessionId sessionId)
        {
            var shardPorts = new Dictionary<string, int>();
            switch (clusterName)
            {
                case MiscUtil.LOCAL:
                    if (!_configurationStore.ContainsCluster(MiscUtil.LOCAL))
                    {
                        CreateLocalCluster(sessionId);
                    }
                    var local = _configurationStore.GetClusterConfiguration(MiscUtil.LOCAL);
                    shardPorts.Add(MiscUtil.LOCAL, local.Deployment.GetShard(MiscUtil.LOCAL).Port);
                    break;
                case MiscUtil.CLUSTERED:
                    var cluster = _configurationStore.GetClusterConfiguration(MiscUtil.CLUSTERED);
                    if (cluster != null && cluster.Deployment != null && cluster.Deployment.Shards != null)
                    {
                        foreach (var pair in cluster.Deployment.Shards)
                        {
                            if (!shardPorts.ContainsKey(pair.Key))
                                shardPorts.Add(pair.Key, pair.Value.Port);
                        }
                    }
                    break;
            }
            return shardPorts;

        }

        public ConfigServerConfiguration GetConfigurationClusterConfiguration(string configCluster)
        {
            try
            {
                if (_configurationServerConfig.Name.Equals(configCluster))
                {
                    return _configurationServerConfig.Clone() as ConfigServerConfiguration;
                }
                else if (configCluster.Equals("*"))
                {
                    if (IsConfigClusterEstablished)
                        return _configurationServerConfig.Clone() as ConfigServerConfiguration;
                    else
                        return new ConfigServerConfiguration();

                }
            }

            catch (System.Exception ex)
            {
                // TODO logger
                throw;
            }
            return null;
        }

        public ClusterInfo[] GetConfiguredClusters()
        {
            return ConfigurationStore.GetAllClusterInfo();
        }

        public ClusterInfo GetDatabaseClusterInfo(string clusterName)
        {
            ClusterInfo clusterInfo = null;
            if (string.IsNullOrEmpty(clusterName))
                return null;

            ConfigurationStore.Transaction transaction = ConfigurationStore.BeginTransaction(clusterName, false);

            if (!transaction.ContainsCluster(clusterName.ToLower()))
                throw new Exception("There is no database cluster register on this node.");

            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);

                clusterInfo = transaction.GetClusterInfo(clusterName);

                //if (clusterInfo == null && _clusterConfiguration.ContainsValue(clusterName))
                //clusterInfo = GetClusterInfoFromClusterConfiguration(_clusterConfiguration[clusterName.ToLower()] as ClusterConfiguration);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetDatabaseClusterInfo", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return clusterInfo;
        }

        internal ClusterInfo[] GetAllDatabaseClusterInfo()
        {
            return ConfigurationStore.GetAllClusterInfo();
        }

        internal void RemoveNodeFromMembership(string clusterName, string shardName, ServerNode node)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                bool isPrimary = _membershipMetadatastore.RemoveNodeFromMemberList(clusterName, shardName, node);

                ChangeType changeType = isPrimary ? ChangeType.PrimaryGone : ChangeType.NodeLeft;
                ConfigChangeEventArgs args = new ConfigChangeEventArgs(clusterName, changeType);
                args.SetParamValue(EventParamName.Membership, _membershipMetadatastore.GetMemberShip(clusterName, shardName));
                args.SetParamValue(EventParamName.ShardName, shardName);
                args.SetParamValue(EventParamName.ClusterName, clusterName);

                //args.Membership = _membershipMetadatastore.GetMemberShip(clusterName, shardName);
                //args.ShardName = shardName;
                //args.ClusterName = clusterName;

                transaction = ConfigurationStore.BeginTransaction(clusterName);

                ClusterConfiguration clusterConf = transaction.GetClusterConfiguration(clusterName);
                ClusterInfo clusterInfo = transaction.GetClusterInfo(clusterName);
                //List<ShardInfo> shardInfoList = clusterInfo.ShardInfo.ToList<ShardInfo>();

                ShardInfo shardInfo = clusterInfo.GetShardInfo(shardName);

                //List<ServerInfo> runningNodes = shardInfo.RunningNodes.ToList<ServerInfo>();
                int shardPort = clusterConf.Deployment.GetShard(shardName).Port;
                ServerInfo serverInfo = GetServerInfoFromServerNode(node, shardPort);

                if (isPrimary || (shardInfo.Primary != null && shardInfo.Primary.Equals(serverInfo)))
                    shardInfo.Primary = null;

                shardInfo.RemoveRunningNode(serverInfo.Address);
                transaction.InsertOrUpdateClusterInfo(clusterInfo);

                ConfigurationStore.CommitTransaction(transaction);
                // _distributionMetadataStore.Save();
                //_membershipMetadatastore.Save();
                _nodeMembership.Remove(clusterName + shardName + node.Name);
                RemoveFromHeartBeat(clusterName, shardName, serverInfo);

                Thread sendNotification = new Thread(() => SendNotification(args))
                {
                    Name = "RemoveNodeSendNotificationThread"
                };
                sendNotification.Start();

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RegisterClusterConfiguration", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void RegisterClusterConfiguration(ClusterConfiguration configuration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                string cluster = configuration.Name;

                transaction = ConfigurationStore.BeginTransaction(cluster);
                if (cluster != null && !transaction.ContainsCluster(cluster.ToLower()))
                {
                    transaction.InsertOrUpdateClusterConfiguration(configuration);

                    if (configuration.Deployment != null)
                    {
                        StartHeartbeatChekcTask(configuration);
                    }
                }


                // adding in meta-data
                transaction.InsertOrUpdateClusterInfo(GetClusterInfoFromClusterConfiguration(configuration));

                ArrayList parameter = new ArrayList() { configuration };
                SendMessageToReplica(configuration.Name, parameter, ConfigurationCommandUtil.MethodName.RegisterClusterConfiguration, 1, null, null);
                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RegisterClusterConfiguration", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void UnregisterClusterConfiguration(ClusterConfiguration configuration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                transaction = ConfigurationStore.BeginTransaction(configuration.Name);
                if (configuration.Name != null && transaction.ContainsCluster(configuration.Name.ToLower()))
                {
                    transaction.RemoveClusterConfiguration(configuration.Name.ToLower());
                }

                //removing from meta-data
                //_clusterInfo = _configurationStore.GetClusterInfo(configuration.Name);
                transaction.RemoveClusterInfo(configuration.Name);

                ArrayList parameter = new ArrayList() { configuration };
                SendMessageToReplica(configuration.Name, parameter, ConfigurationCommandUtil.MethodName.UnregisterClusterConfiguration, 1, oldConfiguration, oldClusterInfo);
                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UnregisterClusterConfiguration", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                // SaveConfiguration();
                _rwLock.ReleaseWriterLock();
            }
        }

        public bool MakeActive(ServerNode serverNode)
        {

            if (_configurationServerConfig.Servers.Nodes.Count >= 2)
                throw new Exception("Maximum two nodes are allowed to add into database cluster. ");
            string ip = serverNode.Name;

            lock (this)
            {
                //_configurationServerConfig.Servers.AddNode(node);
                _configurationServerConfig.Servers.RemoveNode(ip);
                _configurationServerConfig.Servers.AddNode(serverNode);
                SaveConfigServerConfiguration();
                //_cfgCluster.UpdateConfiguration(_configurationServerConfig);
            }
            return false;
        }

        public void UpdateClusterConfiguration(ClusterConfiguration configuration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                string cluster = configuration.Name;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    transaction.InsertOrUpdateClusterConfiguration(configuration);
                    transaction.InsertOrUpdateClusterInfo(GetClusterInfoFromClusterConfiguration(configuration));

                    ArrayList parameter = new ArrayList() { configuration };
                    SendMessageToReplica(configuration.Name, parameter,
                        ConfigurationCommandUtil.MethodName.UpdateClusterConfiguration, 1, null,
                        null);

                    ConfigurationStore.CommitTransaction(transaction);
                }
                else
                    throw new System.Exception("Given configuration does not exist");

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateClusterConfiguration", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                //SaveConfiguration();
                _rwLock.ReleaseWriterLock();
            }
        }

        public ClusterConfiguration[] GetAllClusterConfiguration()
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                return ConfigurationStore.GetAllClusterConfiguration();
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetAllClusterConfiguration", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        public IShardConfigurationSession OpenShardConfigurationSession(string cluster, string shard, ServerNode node, UserCredentials credentials, IChannelFormatter channelFormatter)
        {
            return new ShardConfigurationSession(this, cluster, shard, node);
        }

        public IConfigurationSession OpenConfigurationSession(IClientAuthenticationCredential credentials)
        {
            return new ConfigurationSession(this, credentials);
        }

        internal void CreateLocalCluster(ISessionId sessionId)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            Core.Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Cluster, MiscUtil.LOCAL, out resourceId, out superResourceId, MiscUtil.CLUSTERED, MiscUtil.NOSDB_CLUSTER_SERVER);
            if (_securityManager.Authorize(MiscUtil.CONFIGURATION_SHARD_NAME, sessionId, resourceId, superResourceId, Permission.Create_Cluster))
            {
                ConfigurationStore.Transaction transaction = null;
                try
                {
                    transaction = ConfigurationStore.BeginTransaction(MiscUtil.LOCAL);
                    ClusterConfiguration configuration = GetLocalDatabaseConfiguration();
                    ClusterInfo clusterInfo = null;
                    if (!transaction.ContainsCluster(MiscUtil.LOCAL))
                    {
                        clusterInfo = GetClusterInfoAndUpdateCappedCollectionShard(ref configuration);
                        transaction.InsertOrUpdateClusterConfiguration(configuration);

                    }
                    transaction.InsertOrUpdateClusterInfo(clusterInfo);
                    ConfigurationStore.CommitTransaction(transaction);
                    AddSecurityInformation(MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, superResourceId, MiscUtil.LOCAL, sessionId);
                }
                catch (Exception)
                {
                    if (transaction != null)
                        ConfigurationStore.CommitTransaction(transaction);

                    throw;
                }
            }
        }

        public void CreateCluster(string cluster, ClusterConfiguration configuration)
        {
            if (IsReservedWord(cluster))
                throw new Exception(cluster + " is a reserved word");

            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterInfo clusterInfo = null;

                if (cluster != null && !ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    AssignObjectUID(configuration);
                    clusterInfo = GetClusterInfoAndUpdateCappedCollectionShard(ref configuration);
                    transaction.InsertOrUpdateClusterConfiguration(configuration);
                    transaction.InsertOrUpdateClusterInfo(clusterInfo);

                    AddServersToCluster(ref configuration);

                    if (configuration.Deployment != null)
                    {
                        StartHeartbeatChekcTask(configuration);
                    }
                }
                else
                    throw new System.Exception("Configuration with same name already exist.");


                //adding in meta-data

                ArrayList parameter = new ArrayList() { cluster, configuration };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.CreateCluster, 1, null, null);

                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("CreateCluster", ex.ToString());

                throw;

            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                // SaveConfiguration();

            }
        }
        private void StartHeartbeatChekcTask(ClusterConfiguration clusterConfig)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _nodeContext.LocalShardName != null ? _nodeContext.LocalShardName : "", DatabaseName = "" });
            lock (_checkHeartbeatTasks)
            {
                if (!_checkHeartbeatTasks.ContainsKey(clusterConfig.Name.ToLower()))
                {
                    ConfigServerHeartBeatTask task = new ConfigServerHeartBeatTask(clusterConfig.Name, this, clusterConfig.Deployment.HeartbeatInterval,2000);
                    task.Start(_nodeContext.LocalShardName);
                    _checkHeartbeatTasks.Add(clusterConfig.Name.ToLower(), task);

                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                        LoggerManager.Instance.ServerLogger.Info("ConfigurationServer.Start()", "Heart beat task started for '" + clusterConfig.Name + "'");
                }
            }
        }

        private void StopHeartbeatChekcTask(ClusterConfiguration clusterConfig)
        {
            lock (_checkHeartbeatTasks)
            {
                if (_checkHeartbeatTasks.ContainsKey(clusterConfig.Name.ToLower()))
                {
                    ConfigServerHeartBeatTask task = _checkHeartbeatTasks[clusterConfig.Name.ToLower()];
                    task.Stop();
                    _checkHeartbeatTasks.Remove(clusterConfig.Name.ToLower());
                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                        LoggerManager.Instance.ServerLogger.Info("ConfigurationServer.Start()", "Heart beat task stopped for '" + clusterConfig.Name + "'");

                }
            }
        }

        private void AddServersToCluster(ref ClusterConfiguration configuration)
        {
            try
            {
                lock (configuration.Deployment.Shards)
                {
                    Address[] configServers = GetConfigurationServers();

                    foreach (KeyValuePair<string, ShardConfiguration> shard in configuration.Deployment.Shards)
                    {
                        int peerPort = shard.Value.Port;
                        try
                        {
                            List<string> configServerIPs = new List<string>();
                            List<int> configServerPort = new List<int>();
                            foreach (ServerNode node in shard.Value.Servers.Nodes.Values)
                            {
                                RemoteManagementSession dbMgtRemote = GetManagementSession(node.Name);
                                if (!dbMgtRemote.AddServerToShard(_configurationServerConfig.Name, configuration.UID, configServers, configuration.Name, shard.Key, shard.Value.UID, shard.Value.Port, false, null))
                                {
                                    shard.Value.Servers.RemoveNode(node.Name);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null &&
                                LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("DMS AddServerToShard", ex.ToString());
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("DMS AddServerToCluster", ex.ToString());
            }
        }

        public void AddShardToCluster(string cluster, string shard, ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration)
        {
            if (IsReservedWord(shard))
                throw new Exception(shard + " is a reserved word");

            ConfigurationStore.Transaction transaction = null;
            try
            {
                AssignObjectUID(shardConfiguration);
                ClusterConfiguration existingConfig = ConfigurationStore.GetClusterConfiguration(cluster);

                //First check if all nodes are avaialble 
                foreach (KeyValuePair<string, ServerNode> node in shardConfiguration.Servers.Nodes)
                {
                    try
                    {
                        CheckServerAvaialbilityForShard(cluster, existingConfig.UID, shard, node.Key, shardConfiguration.UID);
                    }
                    catch (Exception)
                    {
                        //In check avaialability, all server nodes must be avialble otherwise operation will fail.
                        throw;
                    }
                }
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                transaction = ConfigurationStore.BeginTransaction(cluster);

                ClusterInfo info = transaction.GetClusterInfo(cluster);
                VerifyIfShardRemovalInProgress(info);

                //Let's modify the existing configuration
                AddShardToConfiguration(transaction, cluster, shardConfiguration, distributionConfiguration);
                existingConfig = transaction.GetClusterConfiguration(cluster);
                List<string> exceptionMessage = new List<string>();
                Dictionary<string, ServerNode> connectedNodes = new Dictionary<string, ServerNode>();
                Dictionary<string, ServerNode> failedNodes = new Dictionary<string, ServerNode>();

                Address[] configServers = GetConfigurationServers();

                foreach (KeyValuePair<string, ServerNode> node in shardConfiguration.Servers.Nodes)
                {
                    #region call to DBManagementHost

                    RemoteManagementSession dbMgtRemote = null;
                    try
                    {
                        dbMgtRemote = GetManagementSession(node.Key);
                    }
                    catch (Exception ex)
                    {
                        failedNodes.Add(node.Key, node.Value);
                        exceptionMessage.Add(ex.Message + " Node IP [" + node.Value.Name + "]");
                        continue;
                    }

                    try
                    {
                        if (dbMgtRemote.AddServerToShard(_configurationServerConfig.Name, existingConfig.UID, configServers, cluster, shard, shardConfiguration.UID, shardConfiguration.Port, true, existingConfig))
                        {
                            connectedNodes.Add(node.Key, node.Value);
                        }
                        else if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        {
                            failedNodes.Add(node.Key, node.Value);
                            LoggerManager.Instance.CONDBLogger.Warn("DMS AddServerToShard", "Unable to add " + node.Key + " to " + shard);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        failedNodes.Add(node.Key, node.Value);
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                            LoggerManager.Instance.CONDBLogger.Error("DMS AddServerToShard", ex.ToString());
                        exceptionMessage.Add(ex.Message + " Node IP [" + node.Value.Name + "]");

                    }
                    #endregion
                }
                //if (connectedNodes.Count == 0)
                //    throw new Exception("Cann't create shard  as failed to connect with database server nodes");

                if (failedNodes.Count > 0)
                {
                    if (failedNodes.Count == shardConfiguration.Servers.Nodes.Count)
                    {
                        //complete failure.Just role back. No server could be added to the shard.
                        if (exceptionMessage.Count != 0)
                        {
                            string responseMsg = "";
                            foreach (string msg in exceptionMessage)
                            {
                                responseMsg = responseMsg + msg + "\n";
                            }
                            throw new Exception(responseMsg);
                        }
                        else
                            throw new Exception("Failed to create shard");
                    }
                    else
                    {
                        //at least some of nodes have been added; so remove failed nodes from configuration
                        foreach (string nodeIP in failedNodes.Keys)
                        {
                            RemoveNodeFromShardConfiguration(cluster, shard, transaction, nodeIP, failedNodes[nodeIP]);
                        }

                    }
                }

                if (connectedNodes.Count != 0)
                {
                    ArrayList parameter = new ArrayList() { cluster, shard, shardConfiguration };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.AddShardToCluster, 1,
                        null, null);

                    //foreach (var connectedNode in connectedNodes)
                    //{
                    //    AddNodeToConfigurationServerConfig(cluster, connectedNode.Value);
                    //}
                }

                ConfigurationStore.CommitTransaction(transaction);
                transaction = null;

                if (exceptionMessage.Count != 0)
                {
                    string responseMsg = "";
                    foreach (string msg in exceptionMessage)
                    {
                        responseMsg = responseMsg + msg + "\n";
                    }
                    throw new Exception(responseMsg);
                }
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("AddShardToCluster", ex.ToString());

                throw;
            }
            finally
            {
                if (_rwLock.IsWriterLockHeld)
                    _rwLock.ReleaseWriterLock();
                // SaveConfiguration();

            }
        }

        private void VerifyIfShardRemovalInProgress(ClusterInfo info)
        {
            if (info != null)
            {
                if (info.ShardInfo != null)
                {
                    foreach (ShardInfo shard in info.ShardInfo.Values)
                    {
                        if (shard.GracefullRemovalInProcess)
                            throw new Exception("Add shard can not be performed, because a gracefull shard removal is in progress");
                    }
                }
            }
        }

        private void RemoveNodeFromShardConfiguration(string cluster, string shard, ConfigurationStore.Transaction transaction, string nodeIP, ServerNode serverNode)
        {
            ClusterConfiguration clusterConfiguration = transaction.GetClusterConfiguration(cluster);

            if (clusterConfiguration != null)
            {
                ShardConfiguration shardConfig = clusterConfiguration.Deployment.GetShard(shard);

                if (shardConfig != null)
                {
                    shardConfig.Servers.RemoveNode(nodeIP);
                }

                transaction.InsertOrUpdateClusterConfiguration(clusterConfiguration);

                ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);

                if (clusterInfo != null)
                {
                    ShardInfo shardInfo = clusterInfo.GetShardInfo(shard);

                    if (shardInfo != null)
                    {
                        Address address = new Address(nodeIP, shardConfig.Port);

                        shardInfo.RemoveConfigureNode(address);
                        shardInfo.RemoveRunningNode(address);
                    }

                    transaction.InsertOrUpdateClusterInfo(clusterInfo);
                }
            }


        }

        private void AddShardToConfiguration(ConfigurationStore.Transaction transaction, string cluster, ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration)
        {
            ClusterConfiguration oldConfiguration = null;
            ClusterInfo oldClusterInfo = null;
            string shard = shardConfiguration.Name;

            ClusterConfiguration existingCluster = transaction.GetClusterConfiguration(cluster);
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = shard != null ? shard : "", DatabaseName = "configdb" });
            if (existingCluster != null)
            {
                if (LoggerManager.Instance.CONDBLogger != null &&
                    LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationServer.AddShardtoCluster()",
                        "Shard :" + shard + " adding  into cluster .");
                }

                DeploymentConfiguration deploymentConf = existingCluster.Deployment;
                if (deploymentConf == null)
                    deploymentConf = new DeploymentConfiguration();

                if (deploymentConf.ContainsShard(shardConfiguration.Name))
                    throw new System.Exception("Shard with specified name already exist");

                deploymentConf.AddShard(shardConfiguration);

                //Meta-data info

                ClusterInfo info = transaction.GetClusterInfo(cluster);
                if (info.ShardInfo != null)
                {
                    if (!info.ContainsShard(shard))
                    {
                        ShardInfo sInfo = new ShardInfo();
                        sInfo.UID = shardConfiguration.UID;
                        sInfo.Name = shardConfiguration.Name;
                        sInfo.Port = shardConfiguration.Port;
                        sInfo.IsReadOnly = shardConfiguration.Status == NodeRole.Primary ? false : true;

                        if (shardConfiguration.Servers != null && shardConfiguration.Servers.Nodes != null)
                        {
                            sInfo.ConfigureNodes = new Dictionary<Address, ServerInfo>();
                            foreach (KeyValuePair<string, ServerNode> node in shardConfiguration.Servers.Nodes)
                            {
                                ServerInfo serverInfo = new ServerInfo();
                                AssignObjectUID(serverInfo);
                                node.Value.UID = serverInfo.UID;
                                serverInfo.Address = new Address(node.Key, shardConfiguration.Port);
                                sInfo.AddConfigureNode(serverInfo);
                            }
                        }
                        info.AddShard(sInfo);
                        transaction.InsertOrUpdateClusterInfo(info);
                    }
                }
                DatabaseConfigurations databaseConfs = existingCluster.Databases;

                foreach (KeyValuePair<string, DatabaseConfiguration> dc in databaseConfs.Configurations)
                {

                    if (dc.Value.Storage.Collections != null &&
                        dc.Value.Storage.Collections.Configuration != null)
                    {
                        CollectionConfigurations colConfs = dc.Value.Storage.Collections;

                        //List<CollectionConfiguration> collectionConfiguration = dc.Storage.Collections.Configuration.ToList<CollectionConfiguration>();
                        // dc.Storage.Collections.Configuration = collectionConfiguration.ToArray();
                        foreach (KeyValuePair<string, CollectionConfiguration> cc in colConfs.Configuration)
                        {
                            IDistributionStrategy distributionStrategy = transaction.GetDistributionStrategy(cluster, dc.Key, cc.Key);
                            IDistributionStrategy distribution;
                            if (distributionStrategy != null)
                            {
                                distribution = distributionStrategy.AddShard(shard, distributionConfiguration, true);
                                info.GetDatabase(dc.Key).GetCollection(cc.Key).SetDistributionStrategy(cc.Value.DistributionStrategy, distribution);

                                transaction.InsertOrUpdateDistributionStrategy(info, dc.Key, cc.Key);
                            }

                            //No need to update if strategy is null.
                            //transaction.InsertOrUpdateDistributionStrategy(info, dc.Key, cc.Key);
                        }
                    }
                }

                transaction.InsertOrUpdateClusterConfiguration(existingCluster);
               

                if (LoggerManager.Instance.CONDBLogger != null &&
                    LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationServer.AddShardtoCluster()", "Shard :" + shard + " added successfully into configuration");
                }
            }
            else
                throw new System.Exception("Cluster with given name does not exist.");
        }

        internal bool Demote()
        {
            return _cfgCluster.Demote();
        }

        private bool CheckServerAvaialbilityForShard(string cluster, string clusterUID, string shard, string server, string shardUid)
        {
            RemoteManagementSession dbMgtRemote = null;
            try
            {
                dbMgtRemote = GetManagementSession(server);
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                dbMgtRemote.CanAddToDatabaseCluster(_configurationServerConfig.Name, clusterUID, cluster, shard, shardUid);
            }
            catch (System.Exception ex)
            {

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("CheckServerAvaialbilityForShard", "availability check failed for server '" + server + "' with error : " + ex.ToString());
                throw;

            }

            return false;
        }

        private DistributionType GetDistributionType(string distributionStrategy)
        {
            if (string.IsNullOrEmpty(distributionStrategy))
                throw new Exception("Invalid distribution strategy configuration specified.");

            if (distributionStrategy.Equals(DistributionType.NonSharded.ToString(), StringComparison.OrdinalIgnoreCase))
                return DistributionType.NonSharded;
            else
                throw new Exception("DistributionStrategyName cannot be null or empty string.");
        }

        private string GetFirstShardName(ClusterConfiguration clusterConfiguration, DatabaseConfiguration database)
        {
            if (clusterConfiguration == null || clusterConfiguration.Deployment == null || clusterConfiguration.Deployment.Shards == null)
                return null;

            if (clusterConfiguration.Deployment.Shards.Count == 1)
            {
                return clusterConfiguration.Deployment.Shards.FirstOrDefault().Key;
            }
            else if (clusterConfiguration.Deployment.Shards.Count > 1)
            {

                if (database != null && database.Storage != null && database.Storage.Collections != null && database.Storage.Collections.Configuration != null)
                {
                    Hashtable shardTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

                    if (database.Storage.Collections.Configuration.Count == 0)
                        return clusterConfiguration.Deployment.Shards.FirstOrDefault().Key;

                    foreach (string shard in clusterConfiguration.Deployment.Shards.Keys)
                    {
                        if (!shardTable.Contains(shard))
                        {
                            shardTable.Add(shard, (int)0);
                        }
                    }

                    foreach (CollectionConfiguration existingCollection in database.Storage.Collections.Configuration.Values)
                    {
                        if (!shardTable.Contains(existingCollection.Shard))
                        {
                            shardTable.Add(existingCollection.Shard, (int)0);
                        }

                        shardTable[existingCollection.Shard] = (int)shardTable[existingCollection.Shard] + 1;
                    }

                    string shardWithMinCollection = clusterConfiguration.Deployment.Shards.FirstOrDefault().Key;
                    int minCollectionPerShard = int.MaxValue;
                    foreach (string shard in clusterConfiguration.Deployment.Shards.Keys)
                    {
                        if (shardTable.Contains(shard))
                        {
                            int collectionPershard = (int)shardTable[shard];
                            if (collectionPershard < minCollectionPerShard)
                            {

                                shardWithMinCollection = shard;
                                minCollectionPerShard = collectionPershard;
                            }
                        }
                        else
                        {
                            return clusterConfiguration.Deployment.Shards.FirstOrDefault().Key;
                        }
                    }

                    return shardWithMinCollection;
                }
                else
                    return clusterConfiguration.Deployment.Shards.FirstOrDefault().Key;

            }

            return null;
        }



        class GracefulRemovalMonitoring : IDisposable//,IThreadRunnable
        {
            private Thread monitoringThread = null;
            private Dictionary<ITaskInfo, ITaskInfo> gracefullyRemovedShards = new Dictionary<ITaskInfo, ITaskInfo>();
            private ConfigurationServer parent = null;
            private volatile bool running;
            private Object mutex = new Object();
            private Boolean canCheck = true;

            public GracefulRemovalMonitoring(ConfigurationServer configServer)
            {
                parent = configServer;
            }

            internal void Add(ITaskInfo info)
            {
                lock (gracefullyRemovedShards)
                {
                    if (!gracefullyRemovedShards.ContainsKey(info))
                        gracefullyRemovedShards.Add(info, null);

                    Monitor.PulseAll(gracefullyRemovedShards);
                }

                Start();
            }

            internal void Remove(ITaskInfo info)
            {
                lock (gracefullyRemovedShards)
                {
                    if (gracefullyRemovedShards.ContainsKey(info))
                        gracefullyRemovedShards.Remove(info);

                    Monitor.PulseAll(gracefullyRemovedShards);
                }
            }


            void Start()
            {
                lock (mutex)
                {
                    if (!running)
                    {
                        running = true;

                        monitoringThread = new Thread(new ThreadStart(Run));
                        monitoringThread.Name = "Shard Removal Monitoring";
                        monitoringThread.IsBackground = true;
                        monitoringThread.Start();
                    }
                }
            }

            internal void CheckRemovalStatus()
            {
                lock (mutex)
                {
                    canCheck = true;
                    Monitor.PulseAll(mutex);
                }
            }

            public void Run()
            {
                //LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = .LocalShardName != null ? _nodeContext.LocalShardName : "", DatabaseName = "" });
                try
                {
                    while (running)
                    {
                        lock (gracefullyRemovedShards)
                        {
                            if (gracefullyRemovedShards.Count == 0)
                            {
                                Monitor.Wait(gracefullyRemovedShards);

                                /*bool pulsed = Monitor.Wait(gracefullyRemovedShards,Common.MiscUtil.MONITORING_WAIT_TIME);
                                if (!pulsed) continue;*/
                            }

                            List<ITaskInfo> removableList = new List<ITaskInfo>();
                            foreach (KeyValuePair<ITaskInfo, ITaskInfo> pair in gracefullyRemovedShards)
                            {
                                switch (pair.Key.TaskType)
                                {
                                    case TaskType.GracefullShardRemoval:
                                        if (parent.ShardExists(pair.Key.Cluster, pair.Key.Shard))
                                        {
                                            try
                                            {
                                                if (pair.Key.IsTaskCompleted())
                                                {
                                                    if (LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                                        LoggerManager.Instance.CONDBLogger.Info("GracefulRemovalMonitoring.Run", "removing " + pair.Key.Shard);
                                                    pair.Key.OnTaskCompleted();
                                                    //parent.RemoveShardFromConfiguration(null, pair.Key.Cluster, pair.Key.Shard);
                                                    removableList.Add(pair.Key);
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                if (LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                                    LoggerManager.Instance.CONDBLogger.Error("GracefulRemovalMonitoring.Run", "An error occured while removing shard " + pair.Key.Shard + ". " + e);
                                            }
                                        }
                                        else
                                        {
                                            if (LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                                LoggerManager.Instance.CONDBLogger.Info("GracefulRemovalMonitoring.Run", pair.Key.Shard + " is already removed from the cluster ");

                                            removableList.Add(pair.Key);
                                        }
                                        break;
                                    case TaskType.DatabaseMigration:
                                        try
                                        {
                                            if (pair.Key.IsTaskCompleted())
                                            {
                                                if (LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                                    LoggerManager.Instance.CONDBLogger.Info("DatabaseMigrationMonitoring.Run", "Migration Task Completed");
                                                pair.Key.OnTaskCompleted();
                                                removableList.Add(pair.Key);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            if (LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                                LoggerManager.Instance.CONDBLogger.Error("DatabaseMigrationMonitoring.Run", "An error occured while evaluating MigrationTask." + e);
                                        }
                                        break;

                                    case TaskType.DifferentialRestore:
                                        try
                                        {
                                            if (pair.Key.IsTaskCompleted())
                                            {
                                                DifferentialRestoreJobInfo jobInfo = (DifferentialRestoreJobInfo)pair.Key;

                                                if (LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                                    LoggerManager.Instance.RecoveryLogger.Info("GracefulRemovalMonitoring.Run", "Job " + jobInfo.Job.JobIdentifier + " passed DifCOnfig Restore removing " + pair.Key.Shard);
                                                removableList.Add(pair.Key);
                                            }
                                            else
                                            {
                                                DifferentialRestoreJobInfo jobInfo = (DifferentialRestoreJobInfo)pair.Key;
                                                jobInfo.Parent.CancelRecoveryJob(jobInfo.Job.JobIdentifier);

                                                if (LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                                    LoggerManager.Instance.RecoveryLogger.Info("GracefulRemovalMonitoring.Run", "Job " + jobInfo.Job.JobIdentifier + " failed during DifCOnfig Restore removing " + pair.Key.Shard);
                                                removableList.Add(pair.Key);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            if (LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                                LoggerManager.Instance.RecoveryLogger.Error("GracefulRemovalMonitoring.Run", "An error occured while evaluating DiffRestore " + pair.Key.Shard + ". " + e);
                                        }
                                        break;
                                }

                            }


                            if (removableList.Count > 0)
                            {
                                foreach (ITaskInfo info in removableList)
                                    lock (gracefullyRemovedShards)
                                    {
                                        gracefullyRemovedShards.Remove(info);
                                    }
                            }
                        }
                        lock (mutex)
                        {
                            //This task should pro-actively monitor the state transfer status. We check it every helf minute
                            Monitor.Wait(mutex, new TimeSpan(0, 0, 30));
                            if (!running) break;
                        }
                    }


                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("GracefulRemovalMonitoring.Run", ex);
                }
                finally
                {
                    running = false;
                }
            }

            public void Stop()
            {
                lock (mutex)
                {
                    running = false;

                    Monitor.PulseAll(mutex);

                    lock (gracefullyRemovedShards)
                    {
                        Monitor.PulseAll(gracefullyRemovedShards);
                    }

                    while (monitoringThread.IsAlive) ;
                }
                //lock (mutex)
                //{
                //    if(monitoringThread!=null)
                //        monitoringThread.Abort();
                //}
            }

            public void Dispose()
            {
                Stop();
            }
        }

        //class HeartbeatManager
        //{
        //    private IDictionary<String,IDictionary<String,IDictionary<String,Membership>>> heartbeatTable=new Dictionary<String,IDictionary<String,IDictionary<String,Membership>>>();
        //    private string _cluster;
        //    private string _shard;
        //    private string _node;

        //    public void ReportHeartbeat(string cluster,string shard,string node,Membership membership)
        //    {
        //        lock (heartbeatTable)
        //        {
        //            IDictionary<String, IDictionary<String, Membership>> shardReportTable = heartbeatTable[cluster];

        //            if (shardReportTable == null)
        //            {
        //                shardReportTable = new Dictionary<String, IDictionary<String, Membership>>();
        //                heartbeatTable[cluster] = shardReportTable;
        //            }

        //            if()

        //        }
        //    }
        //}


        class GracefullShardInfo : ITaskInfo
        {
            private ConfigurationServer parent;

            public GracefullShardInfo(String cluster, String shard, ConfigurationServer configurationServer)
            {
                Cluster = cluster;
                Shard = shard;
                parent = configurationServer;
            }

            public String Cluster { get; set; }
            public String Shard { get; set; }

            public override int GetHashCode()
            {
                int code = 0;

                code += Cluster != null ? Cluster.ToLower().GetHashCode() : 0;
                code += Shard != null ? Shard.ToLower().GetHashCode() : 0;

                return code;
            }

            public override bool Equals(object obj)
            {
                GracefullShardInfo other = obj as GracefullShardInfo;
                if (other == null) return false;

                if (string.Compare(other.Cluster, Cluster, true) == 0 && string.Compare(other.Shard, Shard, true) == 0) return true;

                return false;
            }

            public bool IsTaskCompleted()
            {
                Boolean canRemove = true;
                ConfigurationStore.Transaction transaction = parent.ConfigurationStore.BeginTransaction(Cluster, false);
                ClusterConfiguration configuration = transaction.GetClusterConfiguration(Cluster);

                if (configuration != null)
                {
                    DatabaseConfigurations databaseConfs = configuration.Databases;

                    if (databaseConfs != null)
                    {
                        foreach (KeyValuePair<string, DatabaseConfiguration> database in databaseConfs.Configurations)
                        {
                            CollectionConfigurations collectionConfs = database.Value.Storage.Collections;
                            if (collectionConfs != null)
                            {
                                foreach (KeyValuePair<string, CollectionConfiguration> cc in collectionConfs.Configuration)
                                {
                                    //if (cc.Key.Equals(AttachmentAttributes.ATTACHMENT_COLLECTION, StringComparison.OrdinalIgnoreCase)) continue;

                                    IDistributionStrategy distributionStrategy = transaction.GetDistributionStrategy(Cluster, database.Key, cc.Key);

                                    if (distributionStrategy != null && !distributionStrategy.CanRemoveShard(Shard))
                                    {
                                        canRemove = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return canRemove;
            }

            public void OnTaskCompleted()
            {
                parent.RemoveShardFromConfiguration(null, Cluster, Shard);
            }


            public TaskType TaskType
            {
                get { return TaskType.GracefullShardRemoval; }
            }
        }

        public void RemoveShardFromConfiguration(ConfigurationStore.Transaction transaction, String cluster, String shard)
        {
            Boolean lockHeld = false;
            bool transactionLocallyInitialized = false;
            try
            {
                if (transaction == null)
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    transactionLocallyInitialized = true;
                }

                lockHeld = false;
                if (!_rwLock.IsWriterLockHeld)
                {
                    lockHeld = true;
                    _rwLock.AcquireWriterLock(Timeout.Infinite);
                }
                ClusterConfiguration exisingConfiguration = transaction.GetClusterConfiguration(cluster);
                DeploymentConfiguration deploymentConf = exisingConfiguration.Deployment;
                ShardConfiguration sConfig = deploymentConf.GetShard(shard);
                deploymentConf.RemoveShard(sConfig.Name);

                ClusterInfo info = transaction.GetClusterInfo(cluster);
                if (info.ShardInfo != null)
                {
                    if (info.ContainsShard(shard))
                    {
                        info.RemoveShard(shard);
                        transaction.InsertOrUpdateClusterInfo(info);
                    }
                }

                transaction.RemoveMembershipData(cluster, shard);
                transaction.InsertOrUpdateClusterConfiguration(exisingConfiguration);
                if (sConfig != null)
                {
                    foreach (KeyValuePair<string, ServerNode> server in sConfig.Servers.Nodes)
                    {
                        try
                        {
                            RemoteManagementSession dbMgtRemote = GetManagementSession(server.Key);
                            dbMgtRemote.RemoveServerFromShard(cluster, shard);
                        }
                        catch (Exception e)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("RemoveShardFromCluster", "An error occured while removing " + server.Key + " from shard '" + shard + "'.  " + e.ToString());

                        }
                    }

                }

                if (transactionLocallyInitialized)
                    ConfigurationStore.CommitTransaction(transaction);
                //Meta-data info
            }
            catch (Exception e)
            {
                if (transactionLocallyInitialized)
                    ConfigurationStore.RollbackTranscation(transaction);
                throw;
            }
            finally
            {
                if (lockHeld)
                    _rwLock.ReleaseWriterLock();
            }
        }

        public void RemoveShardFromCluster(string cluster, string shard, IDistributionConfiguration configuration, Boolean isGraceFull)
        {
            ConfigurationStore.Transaction transaction = null;
            GracefullShardInfo gracefullRemovalInfo = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;
                ClusterConfiguration existingConfiguration = ConfigurationStore.GetClusterConfiguration(cluster);

                if (existingConfiguration != null)
                {
                    if (existingConfiguration.Deployment != null)
                    {
                        if(existingConfiguration.Deployment.GetShard(shard) == null)
                             throw new Exception("Given shard does not exist in the cluster");
                        if (existingConfiguration.Deployment.Shards.Count == 1)
                            throw new Exception("This is the only shard. Instead of removing shard, remove database cluster.");
                    }

                transaction = ConfigurationStore.BeginTransaction(cluster);
                    existingConfiguration = transaction.GetClusterConfiguration(cluster);
                    ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);

                    if (!isGraceFull)
                    {
                        //see if we a shard is already under gracefull removal
                        if (clusterInfo.IsShardUnderRemoval(shard))
                        {
                            gracefullRemovalInfo = new GracefullShardInfo(cluster, shard, this);
                        }
                        RemoveShardFromConfiguration(transaction, cluster, shard);
                    }
                    else
                    {
                        if (clusterInfo != null)
                        {
                            if (clusterInfo.IsShardUnderRemoval(shard))
                            {
                                //shard removal is already in-progress.
                                throw new Exception("Shard is already under gracefull removal.");
                            }
                            if (clusterInfo.GetShardInfo(shard).Primary == null)
                                throw new Exception("Primary must be running for gracefull removel of shard.");

                            clusterInfo.MarkShardForGracefullRemoval(shard);
                            transaction.InsertOrUpdateClusterInfo(clusterInfo);
                        }
                        try
                        {
                            gracefullRemovalInfo = new GracefullShardInfo(cluster, shard, this);
                            _gracefulRemovalMonitoring.Add(gracefullRemovalInfo);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }

                    //Change the distribution
                    DatabaseConfigurations databaseConfs = existingConfiguration.Databases;

                    foreach (KeyValuePair<string, DatabaseConfiguration> dc in databaseConfs.Configurations)
                    {
                        if (dc.Value.Storage != null && dc.Value.Storage.Collections != null && dc.Value.Storage.Collections.Configuration != null)
                        {
                            //List<CollectionConfiguration> collectionConfiguration = dc.Value.Storage.Collections.Configuration.ToList<CollectionConfiguration>();
                            CollectionConfigurations collectionConfs = dc.Value.Storage.Collections;
                            foreach (CollectionConfiguration cc in collectionConfs.Configuration.Values)
                            {
                                IDistributionStrategy distribution = transaction.GetDistributionStrategy(cluster, dc.Value.Name, cc.CollectionName);
                                if (distribution != null)
                                {
                                    distribution = distribution.RemoveShard(shard, null, isGraceFull);

                                    if (cc.DistributionStrategy.Name.Equals(DistributionType.NonSharded.ToString()))
                                    {
                                        NonShardedDistribution nonShardedDistribution = distribution.Distribution as NonShardedDistribution;
                                        cc.Shard = nonShardedDistribution.GetFinalShard();

                                        //on demand creation of capped collection on new shard
                                        DeploymentConfiguration deploymentConfs = existingConfiguration.Deployment;
                                        ShardConfiguration newShard = deploymentConfs.Shards[cc.Shard];

                                        if (newShard.Servers.Nodes.Count > 0)
                                        {
                                            foreach (ServerNode server in newShard.Servers.Nodes.Values)
                                            {
                                                RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                                dbMgtSession.CreateCollection(cluster, newShard.Name, dc.Key, cc, distribution);
                                            }
                                        }
                                    }


                                    if (clusterInfo.Databases != null)
                                    {
                                        clusterInfo.GetDatabase(dc.Value.Name).GetCollection(cc.CollectionName).DistributionStrategy = distribution;
                                        transaction.InsertOrUpdateDistributionStrategy(clusterInfo, dc.Value.Name, cc.CollectionName);
                                    }
                                }
                            }
                        }
                    }

                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.CONDBLogger.Info("ConfigurationServer.RemovedShardtoCluster()", "Shard :" + shard + " removed  from cluster: " + cluster + " .");
                    }



                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");

               
                ArrayList parameter = new ArrayList() { cluster, shard };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.RemoveShardFromCluster, 1, oldConfiguration, oldClusterInfo);


                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);



                //un-register old shard removal task.
                if (!isGraceFull && gracefullRemovalInfo != null)
                    _gracefulRemovalMonitoring.Remove(gracefullRemovalInfo);
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                {
                    ConfigurationStore.RollbackTranscation(transaction);
                    if (isGraceFull && gracefullRemovalInfo != null)
                        _gracefulRemovalMonitoring.Remove(gracefullRemovalInfo);
                }

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RemoveShardFromCluster", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                // SaveConfiguration();
            }
        }

        public Membership AddServerToShard(string cluster, string shard, ServerNode server)
        {
            Membership membership = null;
            DeploymentConfiguration deploymentConfs = null;
            bool connected = false;
            ConfigurationStore.Transaction transaction = null;
            try
            {
                ClusterConfiguration existingConfiguration = ConfigurationStore.GetClusterConfiguration(cluster);

                if (existingConfiguration != null)
                {

                    CheckServerAvaialbilityForShard(cluster, existingConfiguration.UID, shard, server.Name, null);

                    _rwLock.AcquireWriterLock(Timeout.Infinite);

                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    membership = AddServerToShard(transaction, cluster, shard, server);

                    existingConfiguration = transaction.GetClusterConfiguration(cluster);
                    deploymentConfs = existingConfiguration.Deployment;
                    ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);

                    bool running = false;

                    if (clusterInfo != null)
                    {
                        ShardInfo shardInfo = clusterInfo.GetShard(shard);

                        if (shardInfo != null)
                            running = shardInfo.RunningNodes != null ? shardInfo.RunningNodes.Count > 0 : false;
                    }

                    ShardConfiguration shardConfig = deploymentConfs.GetShard(shard);

                    try
                    {
                        Address[] configServers = GetConfigurationServers();

                        RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);
                        connected = dbMgtRemote.AddServerToShard(_configurationServerConfig.Name, existingConfiguration.UID, configServers, cluster, shard, shardConfig.UID, shardConfig.Port, running, existingConfiguration);
                    }
                    catch (System.Exception ex)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null &&
                            LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                            LoggerManager.Instance.CONDBLogger.Error("DMS AddServerToShard", ex.ToString());
                        throw;

                    }
                    //AddNodeToConfigurationServerConfig(cluster, server);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");

                ArrayList parameter = new ArrayList() { cluster, shard, server };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.AddServerToShard, 1,
                    null, null);

                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("AddServerToShard", ex.ToString());

                throw;
            }
            finally
            {
                if (_rwLock.IsWriterLockHeld)
                    _rwLock.ReleaseWriterLock();

                if (_rwLock.IsReaderLockHeld)
                    _rwLock.ReleaseReaderLock();

                //SaveConfiguration();
            }
            return membership;
        }

        private Membership AddServerToShard(ConfigurationStore.Transaction transaction, string cluster, string shard, ServerNode server)
        {
            Membership membership = null;
            DeploymentConfiguration deploymentConfs = null;
            ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);

            deploymentConfs = existingConfiguration.Deployment;

            if (deploymentConfs.ContainsShard(shard))
            {
                ShardConfiguration shardConfig = deploymentConfs.GetShard(shard);
                //List<ServerNode> nodeList = shardList.Find(p => p.Name.Equals(shard)).Servers.Nodes.ToList<ServerNode>();
                var nodes = shardConfig.Servers;
                if (nodes.ContainsNode(server.Name))
                    throw new System.Exception("Specified node already part of this shard");

                AssignObjectUID(server);
                nodes.AddNode(server);

                transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);

                ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                ServerInfo serverInfo = GetServerInfoFromServerNode(server, shardConfig.Port);
                AssignObjectUID(serverInfo);

                clusterInfo.GetShardInfo(shard).AddConfigureNode(serverInfo);

                transaction.InsertOrUpdateClusterInfo(clusterInfo);

                membership = transaction.GetMembershipData(cluster, shard);
                if (membership == null)
                {
                    membership = new Membership();
                    membership.Cluster = cluster;
                    membership.Shard = shard;
                    membership.Servers = new List<ServerNode>();
                }
                else if (membership.Servers == null)
                {
                    membership.Servers = new List<ServerNode>();
                }


                if (!membership.Servers.Contains(server))
                {
                    membership.Servers.Add(server);
                    transaction.InsertOrUpdateMembershipData(membership);
                }

                return membership;

            }
            else
                throw new Exception(shard + " does not exist in the cluster");
        }
        private Address[] GetConfigurationServers()
        {
            List<Address> configServers = new List<Address>();
            foreach (var servernode in _configurationServerConfig.Servers.Nodes)
            {
                configServers.Add(new Address(servernode.Value.Name, _configurationServerConfig.Port));
            }

            return configServers.ToArray();
        }

        public Membership RemoveServerFromShard(string cluster, string shard, ServerNode server)
        {

            Membership membership = null;
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                ClusterConfiguration existingConfiguraiton = ConfigurationStore.GetClusterConfiguration(cluster);

                if (existingConfiguraiton != null)
                {
                    DeploymentConfiguration deploymentConfs = existingConfiguraiton.Deployment;

                    if (deploymentConfs.GetShard(shard) == null)
                        throw new Exception("Given shard does not exist");

                    if (!deploymentConfs.GetShard(shard).Servers.ContainsNode(server.Name))
                        throw new System.Exception("Specified node is not part of given shard");

                    if (deploymentConfs.GetShard(shard).Servers.Nodes.Count == 1)
                        throw new Exception("This is the only server in the given shard. Instead of removing server, remove shard from the cluster");

                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    existingConfiguraiton = transaction.GetClusterConfiguration(cluster);

                    existingConfiguraiton.Deployment.GetShard(shard).Servers.RemoveNode(server.Name);

                    //meta-data
                    int shardPort = deploymentConfs.GetShard(shard).Port;

                    //call to DBM
                    try
                    {
                        RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);
                        dbMgtRemote.RemoveServerFromShard(cluster, shard);
                    }
                    catch (System.Exception ex)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                            LoggerManager.Instance.CONDBLogger.Error("DMS RemoveServerFromShard", ex.ToString());
                    }
                    //end call

                    ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                    ShardInfo shardInfo = clusterInfo.GetShard(shard);
                    ServerInfo serverInfo = GetServerInfoFromServerNode(server, shardPort);
                    shardInfo.RemoveConfigureNode(serverInfo.Address);
                    shardInfo.RemoveRunningNode(serverInfo.Address);

                    if (shardInfo.Primary != null && shardInfo.Primary.Equals(serverInfo.Address))
                        shardInfo.Primary = null;
                    //else
                    //{
                    //    if (shardInfoList.First<ShardInfo>(p => p.Name.Equals(shard)).Secondaries != null && shardInfoList.First<ShardInfo>(p => p.Name.Equals(shard)).Secondaries.Contains(GetServerInfoFromServerNode(server, shardPort)))
                    //    {
                    //        List<ServerInfo> secondaries = shardInfoList.First<ShardInfo>(p => p.Name.Equals(shard)).Secondaries.ToList<ServerInfo>();
                    //        {
                    //            secondaries.Remove(GetServerInfoFromServerNode(server, shardPort));
                    //            shardInfoList.First<ShardInfo>(p => p.Name.Equals(shard)).Secondaries = secondaries.ToArray();
                    //        }
                    //    }
                    //}

                    transaction.InsertOrUpdateClusterInfo(clusterInfo);

                    membership = transaction.GetMembershipData(cluster, shard);

                    if (membership != null)
                    {
                        if (membership.Servers != null)
                        {
                            membership.Servers.Remove(server);
                            transaction.InsertOrUpdateMembershipData(membership);
                        }
                    }
                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguraiton);

                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");


                ArrayList parameter = new ArrayList() { cluster, shard, server };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.RemoveServerFromShard, 1, null, null);

                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RemoveServerFromShard", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                // SaveConfiguration();
            }

            return membership;
        }

        public Membership SetNodeStatus(string cluster, string shard, ServerNode primary, NodeRole status)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    DeploymentConfiguration deploymentConfs = transaction.GetClusterConfiguration(cluster).Deployment;
                    if (deploymentConfs.ContainsShard(shard) && deploymentConfs.GetShard(shard).Servers.GetNode(primary.Name) != null)
                    {
                        ServerNodes nodes = deploymentConfs.GetShard(shard).Servers;
                        if (nodes.ContainsNode(primary.Name))
                        {
                            deploymentConfs.GetShard(shard).Status = status;
                        }

                        ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                        if (clusterInfo.GetShard(shard).Primary != null && status == NodeRole.None)
                            clusterInfo.GetShard(shard).Primary = null;
                        else
                            clusterInfo.GetShard(shard).Primary = GetServerInfoFromServerNode(primary, deploymentConfs.GetShard(shard).Port);

                        transaction.InsertOrUpdateClusterInfo(clusterInfo);

                        Membership membership = transaction.GetMembershipData(cluster, shard);
                        if (membership.Primary != null && status == NodeRole.None)
                            membership.Primary = null;
                        else
                            membership.Primary = primary;

                        _membershipMetadatastore.AddMembership(cluster, shard, membership);

                        transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                        ArrayList parameter = new ArrayList() { cluster, shard, primary, status };
                        SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.SetNodeStatus, 1, oldConfiguration, oldClusterInfo);

                        ConfigurationStore.CommitTransaction(transaction);

                        return membership;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("SetNodeStatus", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
            return null;
        }

        public Membership GetMembershipInfo(string cluster, string shard)
        {
            Membership memberShip = null;
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                memberShip = _membershipMetadatastore.GetMemberShip(cluster, shard);

                if (memberShip != null)
                    return memberShip;

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    memberShip = new Membership();
                    ClusterInfo clusterInfo = ConfigurationStore.GetClusterInfo(cluster);
                    if (clusterInfo.ContainsShard(shard))
                    {
                        ShardInfo shardInfo = clusterInfo.GetShardInfo(shard);

                        memberShip.Cluster = cluster;
                        memberShip.Shard = shard;
                   
                     

                        if (memberShip.Primary != null && LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                            LoggerManager.Instance.CONDBLogger.Error("GetMembershipInfo", "primary is set " + memberShip.Primary.Name);

                        _membershipMetadatastore.AddMembership(cluster, shard, memberShip);
                    }
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetMembershipInfo", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
            return memberShip;
        }

        public Membership UpdateNodeStatus(string cluster, string shard, ServerNode node, Status status)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                //only updating meta data
                List<ServerInfo> serverNodes = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    DeploymentConfiguration deploymentConfs = transaction.GetClusterConfiguration(cluster).Deployment;

                    //List<ShardConfiguration> shardList = _configurationStore.GetClusterConfiguration(cluster).Deployment.Shards.ToList<ShardConfiguration>();

                    ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                    //ShardInfo shardInfo = _configurationStore.GetClusterInfo(cluster).ShardInfo.ToList<ShardInfo>();
                    foreach (KeyValuePair<string, ShardInfo> info in clusterInfo.ShardInfo)
                    {
                        if (info.Key.ToLower().Equals(shard))
                        {
                            if (info.Value.ConfigureNodes != null)
                            {
                                //serverNodes = info.Value.ConfigureNodes.ToList<ServerInfo>();
                                foreach (KeyValuePair<Address, ServerInfo> sInfo in info.Value.ConfigureNodes)
                                {
                                    if (sInfo.Key.Equals(new Address(node.Name, deploymentConfs.GetShard(shard).Port)))
                                    {
                                        sInfo.Value.Status = status;
                                        if (status == Status.Running)
                                        {
                                            //List<ServerInfo> runningNodes = info.Value.RunningNodes != null ? info.Value.RunningNodes.ToList<ServerInfo>() : new List<ServerInfo>();
                                            if (!info.Value.ContainsRunningNode(sInfo.Key))
                                                info.Value.AddRunningNode(sInfo.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Membership memberShip = _membershipMetadatastore.GetMemberShip(cluster, shard);

                    if (memberShip == null)
                    {
                        memberShip = new Membership();
                        memberShip.Cluster = cluster;
                        memberShip.Shard = shard;
                        memberShip.Servers = new List<ServerNode>();
                        _membershipMetadatastore.AddMembership(cluster, shard, memberShip);
                    }

                    if (status == Status.Running && memberShip.Servers != null && !memberShip.Servers.Contains(node))
                        _membershipMetadatastore.AddNodeToMemberList(cluster, shard, node);

                    if (status == Status.Stopped && memberShip.Servers != null && memberShip.Servers.Contains(node))
                        _membershipMetadatastore.RemoveNodeFromMemberList(cluster, shard, node);

                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));

                    ArrayList parameter = new ArrayList() { cluster, shard, node, status };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.UpdateNodeStatus, 1, oldConfiguration, oldClusterInfo);

                    ConfigurationStore.CommitTransaction(transaction);
                }

                return _membershipMetadatastore.GetMemberShip(cluster, shard);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateNodeStatus", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
            return null;
        }

      
      
        public void ConfigureDistributionStategy(string cluster, string database, string collection, IDistributionStrategy strategy, Boolean needTransfer)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                transaction = ConfigurationStore.BeginTransaction(cluster);

                ConfigureDistributionStategy(transaction, cluster, database, collection, strategy, needTransfer);
                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (Exception e)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

        }

        public Object BeginElection(string cluster, string shard, ServerNode server, ElectionType electionType)
        {
            ConfigurationStore.Transaction transaction = null;
            Object response = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                //lock (this)
                {
                    string key = cluster + shard;


                    Membership membership = _membershipMetadatastore.GetMemberShip(cluster, shard);
                    //RTD: whaaaat?
                    if (electionType != ElectionType.TakeoverElections)
                    {
                        if (membership != null && membership.Primary != null && membership.Servers != null && membership.Servers.Count % 2 == 0)
                        {
                            ClusterInfo clusterInfo = ConfigurationStore.GetClusterInfo(cluster);
                            //List<ShardInfo> shardList = clusterInfo.ShardInfo.ToList<ShardInfo>();
                            if (clusterInfo.ContainsShard(shard))
                            {
                                ShardInfo sInfo = clusterInfo.GetShard(shard);
                                //if (sInfo.Primary != null && sInfo.RunningNodes != null && sInfo.RunningNodes.Count == 2)
                                if (sInfo.Primary != null && sInfo.ConfigureNodes != null && sInfo.ConfigureNodes.Count % 2 == 0)
                                {
                                    DatabaseException ex = new DatabaseException();
                                    ex.ErrorCode = ErrorCodes.Cluster.PRIMARY_ALREADY_EXISTS;
                                    response = ex;

                                    throw new System.Exception("Primary already exists ");
                                }
                            }
                        }
                    }
                    ElectionId eID = new ElectionId();

                    if (_electionLockingMap.Contains(key))
                    {
                        ElectionId electionId = membership.ElectionId;

                        if (electionId != null)
                        {
                            if (electionId.ElectionTime.Add((TimeSpan)electionId.AllowedDuration) < DateTime.Now)
                                _electionLockingMap.Remove(key);
                            else
                            {
                                throw new System.Exception("Previous Election hasn't finished.");
                            }
                        }
                    }

                    if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                    {
                        ElectionId electionId = null;

                        if (membership != null)
                            electionId = membership.ElectionId;

                        if (electionId == null)
                        {
                            eID.Id = 1;
                            eID.UID = eID.Id.ToString();
                            eID.RequestingNode = server;
                            eID.ElectionTime = DateTime.Now;
                            eID.AllowedDuration = new TimeSpan(0, 0, 90);

                            _membershipMetadatastore.UpdateElectionId(cluster, shard, eID);
                            _electionLockingMap[key] = false;
                            //return eID;
                        }

                        else if (electionId != null && electionId.TimeTaken != null && electionId.ElectionTime.Add((TimeSpan)electionId.TimeTaken) < DateTime.Now)
                        {
                            eID.Id = electionId == null ? 1 : electionId.Id + 1;
                            eID.UID = eID.Id.ToString();
                            eID.RequestingNode = server;
                            eID.ElectionTime = DateTime.Now;
                            eID.AllowedDuration = new TimeSpan(0, 0, 90);

                            _membershipMetadatastore.UpdateElectionId(cluster, shard, eID);
                            _electionLockingMap[key] = false;
                            //return eID;
                        }
                        else
                        {
                            throw new System.Exception("Previous Election hasn't finished.");
                        }

                        transaction = ConfigurationStore.BeginTransaction(cluster);

                        transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));

                        ArrayList parameter = new ArrayList() { cluster, shard, server };
                        SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.BeginElection, 1, null, null);

                        ConfigurationStore.CommitTransaction(transaction);
                        return eID;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("BeginElection", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
            return response;
        }

        public int SubmitElectionResult(string cluster, string shard, ElectionResult result)
        {
            ConfigurationStore.Transaction transaction = null;

            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = shard != null ? shard : "", DatabaseName = "" });
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);



                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                    LoggerManager.Instance.CONDBLogger.Info("SubmitElectionResult ", cluster + ":" + shard + " primary : " + result.ElectedPrimary.Name);

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    if (result.PollingResult == ElectionResult.Result.PrimarySelected)
                    {
                        DeploymentConfiguration deploymentConfs = transaction.GetClusterConfiguration(cluster).Deployment;
                        //List<ShardConfiguration> shardList = _configurationStore.GetClusterConfiguration(cluster).Deployment.Shards.ToList<ShardConfiguration>();
                        deploymentConfs.GetShard(shard).Status = NodeRole.Primary;
                        //shardList.First(p => p.Name.ToLower().Equals(shard)).Status = NodeRole.Primary;

                        if (transaction.GetClusterInfo(cluster) != null)
                        {
                            ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                            //List<ShardInfo> shardInfo = _configurationStore.GetClusterInfo(cluster).ShardInfo.ToList<ShardInfo>();

                            if (result.PollingResult == Alachisoft.NosDB.Core.Configuration.Services.ElectionResult.Result.PrimarySelected)
                            {
                                ServerInfo primary = GetServerInfoFromServerNode(result.ElectedPrimary, deploymentConfs.GetShard(shard).Port);

                                if (clusterInfo.GetShard(shard).RunningNodes != null && !clusterInfo.GetShard(shard).ContainsRunningNode(primary.Address))
                                    return 0;

                                clusterInfo.GetShard(shard).Primary = primary;
                            }

                            Membership memberShip = transaction.GetMembershipData(cluster, shard);

                            if (memberShip == null)
                            {
                                memberShip = new Membership();
                                memberShip.Cluster = cluster;
                                memberShip.Shard = shard;
                            }
                            memberShip.Primary = result.ElectedPrimary;
                            memberShip.Servers = result.Voters;
                            memberShip.ElectionId = result.ElectionId;

                            _membershipMetadatastore.AddMembership(cluster, shard, memberShip);
                            transaction.InsertOrUpdateClusterInfo(clusterInfo);
                        }
                    }
                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));

                    ArrayList parameter = new ArrayList() { cluster, shard, result };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.SubmitElectionResult, 1, null, null);

                    ConfigurationStore.CommitTransaction(transaction);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("SubmitElectionResult", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();

                //SaveConfiguration();
            }
            return 1;
        }

        public void EndElection(string cluster, string shard, ElectionId electionId)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    electionId.TimeTaken = DateTime.Now - electionId.ElectionTime;
                    _membershipMetadatastore.UpdateElectionId(cluster, shard, electionId);
                    _electionLockingMap.Remove(cluster + shard);

                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                    ArrayList parameter = new ArrayList() { cluster, shard, electionId };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.EndElection, 1, null, null);

                    ConfigurationStore.CommitTransaction(transaction);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("EndElection", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }
      
      


        private void ConfigureDistributionStategy(ConfigurationStore.Transaction transaction, string cluster, string database, string collection, IDistributionStrategy strategy, Boolean needTransfer, string shardName = null)
        {
            try
            {
                //_rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                if (cluster != null && transaction.ContainsCluster(cluster.ToLower()))
                {
                    ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);
                    CollectionConfigurations collectionConfs = existingConfiguration.Databases.GetDatabase(database).Storage.Collections;
                    if (collectionConfs.ContainsCollection(collection))
                    {
                        DistributionStrategyConfiguration dsc = GetDSConfiguration(strategy);
                        collectionConfs.GetCollection(collection).DistributionStrategy = dsc;

                        //Meta-data info
                        ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                        AddDistributionStrategy(clusterInfo, cluster, database, collection, strategy, dsc, needTransfer, shardName);
                        transaction.InsertOrUpdateClusterInfo(clusterInfo);
                        transaction.InsertOrUpdateDistributionStrategy(clusterInfo, database, collection);
                    }
                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");


                ArrayList parameter = new ArrayList() { cluster, database, collection, strategy };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.ConfigureDistributionStategy, 1, oldConfiguration, oldClusterInfo);

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ConfigureDistributionStategy", ex.ToString());
                throw;
            }
            finally
            {
                //_rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        public void AddDistributionStrategy(ClusterInfo clusterInfo, string cluster, string database, string collection, IDistributionStrategy strategy, DistributionStrategyConfiguration configuration, Boolean needTransfer, string shardName = null)
        {
            if (clusterInfo.Databases != null)
            {
                DatabaseInfo databaseInfo = clusterInfo.GetDatabase(database);

                CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);
                collectionInfo.SetDistributionStrategy(configuration, strategy);
                if (collectionInfo.DistributionStrategy.Name.Equals(DistributionType.NonSharded.ToString()))
                {
                    collectionInfo.DistributionStrategy.AddShard(collectionInfo.CollectionShard, null, needTransfer);
                    //In case of NonShardedDistributionStrategy: The shard told by user must be added first. That is why this check is placed
                }
                if (clusterInfo.ShardInfo != null)
                {

                    if (clusterInfo.ShardInfo.Count > 0 )
                    {
                        if (shardName != null)
                            collectionInfo.DistributionStrategy.AddShard(shardName, null, false);

                        foreach (ShardInfo sInfo in clusterInfo.ShardInfo.Values)
                        {
                            try
                            {
                                if (collectionInfo.DistributionStrategy.Name.Equals(DistributionType.NonSharded.ToString()) && collectionInfo.CollectionShard.Equals(sInfo.Name))
                                {
                                    // Don't add this shard because it is already added above
                                    continue;
                                }

                                if ((shardName != null && sInfo.Name.Equals(shardName, StringComparison.InvariantCultureIgnoreCase)) || sInfo.GracefullRemovalInProcess)
                                    continue;
                                collectionInfo.DistributionStrategy.AddShard(sInfo.Name, null, needTransfer);
                            }
                            catch (Exception ex)
                            {
                                //throw;
                            }
                        }
                        //already assigned
                        //collectionInfo.DataDistribution = collectionInfo.DistributionStrategy.GetCurrentBucketDistribution();
                    }
                    else
                    {
                        throw new Exception("Cannot configure distribution strategy. Please configure atleast one shard before creating collection");
                    }
                }
                else
                {
                    throw new Exception("Cannot configure distribution strategy. Please configure atleast one shard before creating collection");
                }
            }
            else
            {
                throw new Exception("Cannot configure distribution strategy. Please configure atleast one database before creating collection");
            }
        }
        public IDistributionStrategy GetDistriubtionStrategy(string cluster, string database, string collection)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);

                ConfigurationStore.Transaction transaction = ConfigurationStore.BeginTransaction(cluster, false);

                IDistributionStrategy distributionStrategy = null;

                if (cluster != null && transaction.ContainsCluster(cluster))
                {
                    distributionStrategy = transaction.GetDistributionStrategy(cluster, database, collection);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");

                return distributionStrategy;
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetDistriubtionStrategy", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
            return null;
        }

        public ShardConfiguration GetShardConfiguration(string cluster, string shard)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);

                ConfigurationStore.Transaction transaction = ConfigurationStore.BeginTransaction(cluster, false);

                if (cluster != null && transaction.ContainsCluster(cluster.ToLower()))
                {
                    ClusterConfiguration clusterConfiguration = transaction.GetClusterConfiguration(cluster);
                    if (clusterConfiguration.Deployment.Shards.ContainsKey(shard))
                        return clusterConfiguration.Deployment.Shards[shard];
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetShardConfiguration", cluster.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return null;
        }


        public IDistribution GetCurrentDistribution(string cluster, string database, string collection)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);

                IDistribution distribution = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    if (ConfigurationStore.GetClusterInfo(cluster) != null)
                    {
                        DatabaseInfo databaseInfo = ConfigurationStore.GetClusterInfo(cluster).GetDatabase(database);

                        if (databaseInfo != null)
                            distribution = databaseInfo.GetCollection(collection).DataDistribution;
                    }
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");

                return distribution;
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetCurrentDistribution", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
            return null;
        }

        public IDistribution BalanceData(string cluster, string database, string collection)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                IDistribution distribution = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {



                    //List<CollectionConfiguration> collectionList = _configurationStore.GetClusterConfiguration(cluster).Databases.Configurations.ToList<DatabaseConfiguration>().Find(p => p.Name.Equals(collection)).Storage.Collections.Configuration.ToList<CollectionConfiguration>();
                    //DistributionStrategyConfiguration distributionStrategy = collectionList.Find(p => p.CollectionName.Equals(collection)).DistributionStrategy;                                        
                    //return new HashDistribution();

                    if (ConfigurationStore.GetClusterInfo(cluster) != null)
                    {
                        if (ConfigurationStore.GetClusterInfo(cluster) != null)
                        {
                            DatabaseInfo databaseInfo = ConfigurationStore.GetClusterInfo(cluster).GetDatabase(database);

                            if (databaseInfo != null)
                                distribution = databaseInfo.GetCollection(collection).DistributionStrategy.BalanceShards();
                        }
                    }
                    //return _distributionMetadatStore.GetClusterInfo(cluster).Databases.ToList<DatabaseInfo>().First<DatabaseInfo>(p => p.Name.Equals(database)).Collections.ToList<CollectionInfo>().First<CollectionInfo>(p => p.Name.Equals(collection)).DistributionStrategy.BalanceShards();
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");

                ArrayList parameter = new ArrayList() { cluster, database, collection };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.BalanceData, 1, oldConfiguration, oldClusterInfo);

                return distribution;
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("BalanceData", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
            return null;
        }

        private void RemoveClusterLocally(string cluster)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);

                    StopHeartbeatChekcTask(ConfigurationStore.GetClusterConfiguration(cluster));
                    transaction.RemoveClusterConfiguration(cluster);
                    transaction.RemoveClusterInfo(cluster);

                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");

                ConfigurationStore.CommitTransaction(transaction);

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RemoveCluster", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void RemoveCluster(string cluster)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);

                    #region Remove connected shards and nodes

                    DeploymentConfiguration deploymentConf = existingConfiguration.Deployment;
                    foreach (ShardConfiguration shardconfig in deploymentConf.Shards.Values)
                    {
                        string shard = shardconfig.Name;
                        foreach (KeyValuePair<string, ServerNode> server in shardconfig.Servers.Nodes)
                        {

                            try
                            {
                                RemoteManagementSession dbMgtRemote = GetManagementSession(server.Key);
                                dbMgtRemote.RemoveServerFromShard(cluster, shard);
                            }
                            catch (Exception ex)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("cannot remove shard " + shard + " from " + server.Value.Name + " ", ex.ToString());

                            }

                        }

                        transaction.RemoveMembershipData(cluster, shardconfig.Name);
                    }
                    #endregion

                    StopHeartbeatChekcTask(ConfigurationStore.GetClusterConfiguration(cluster));
                    transaction.RemoveClusterConfiguration(cluster);
                    transaction.RemoveClusterInfo(cluster);

                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");


                ArrayList parameter = new ArrayList() { cluster };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.RemoveCluster, 1, null, null);
                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RemoveCluster", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        private ServerInfo GetServerInfoFromServerNode(ServerNode node, int port)
        {
            ServerInfo serverInfo = new ServerInfo();
            serverInfo.Address = new Address(node.Name, port);

            return serverInfo;
        }

        private DistributionStrategyConfiguration GetDSConfiguration(IDistributionStrategy strategy)
        {
            DistributionStrategyConfiguration distributionStrategy = new DistributionStrategyConfiguration();
            distributionStrategy.Name = strategy.Name;
            return distributionStrategy;
        }

        private PartitionKey GetPartitionKey(CollectionConfiguration collectionConfiguration)
        {
            PartitionKey partitionKey = new PartitionKey();
            partitionKey.Attributes = new PartitionKeyAttribute[collectionConfiguration.PartitionKey.PartitionKeyAttributes.Count];
            int i = 0;
            foreach (var attribute in collectionConfiguration.PartitionKey.PartitionKeyAttributes)
            {
                partitionKey.Attributes[i] = new PartitionKeyAttribute();
                partitionKey.Attributes[i].Name = attribute.Value.Name;
                partitionKey.Attributes[i].Type = attribute.Value.Type;
                i++;
            }
            return partitionKey;
        }

        private ClusterInfo GetClusterInfoAndUpdateCappedCollectionShard(ref ClusterConfiguration configuration)
        {
            ClusterInfo clusterInfo = new ClusterInfo();
            try
            {
                clusterInfo.Name = configuration.Name;
                AssignObjectUID(clusterInfo);
                if (configuration.Deployment.Shards != null)
                {
                    foreach (ShardConfiguration shardConfig in configuration.Deployment.Shards.Values)
                    {
                        ShardInfo shardInfo = new ShardInfo();
                        AssignObjectUID(shardInfo);
                        shardInfo.Name = shardConfig.Name;
                        shardInfo.Port = shardConfig.Port;
                        shardInfo.IsReadOnly = shardConfig.Status == NodeRole.Primary ? false : true;

                        if (shardConfig.Servers.Nodes != null)
                        {
                            shardInfo.ConfigureNodes = new Dictionary<Address, ServerInfo>();
                            foreach (ServerNode node in shardConfig.Servers.Nodes.Values)
                            {
                                ServerInfo serverInfo = new ServerInfo();
                                AssignObjectUID(serverInfo);
                                serverInfo.Address = new Address(node.Name, shardConfig.Port);
                                shardInfo.AddConfigureNode(serverInfo);
                            }
                           
                        }
                        clusterInfo.AddShard(shardInfo);
                    }
                }

                if (configuration.Databases.Configurations != null)
                {
                    foreach (DatabaseConfiguration databaseConfiguration in configuration.Databases.Configurations.Values)
                    {
                        DatabaseInfo dbInfo = new DatabaseInfo();
                        AssignObjectUID(dbInfo);
                        dbInfo.Name = databaseConfiguration.Name;
                        if (databaseConfiguration.Storage.Collections.Configuration != null)
                        {
                            foreach (CollectionConfiguration cc in databaseConfiguration.Storage.Collections.Configuration.Values)
                            {
                                CollectionInfo colInfo = new CollectionInfo();
                                AssignObjectUID(colInfo);
                                colInfo.Name = cc.CollectionName;
                                colInfo.ParitionKey = GetPartitionKey(cc);

                                dbInfo.AddCollection(colInfo);
                            }
                        }
                        clusterInfo.AddDatabase(dbInfo);
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RemoveCluster", ex.ToString());
                throw;
            }
            return clusterInfo;
        }

        private ClusterInfo GetClusterInfoFromClusterConfiguration(ClusterConfiguration configuration)
        {
            ClusterInfo clusterInfo = new ClusterInfo();
            try
            {
                clusterInfo.Name = configuration.Name;

                if (configuration.Deployment != null && configuration.Deployment.Shards != null)
                {
                    foreach (ShardConfiguration shardConfig in configuration.Deployment.Shards.Values)
                    {
                        ShardInfo shardInfo = new ShardInfo();
                        shardInfo.Name = shardConfig.Name;
                        shardInfo.Port = shardConfig.Port;
                        shardInfo.IsReadOnly = shardConfig.Status == NodeRole.Primary ? false : true;

                        if (shardConfig.Servers.Nodes != null)
                        {
                            shardInfo.ConfigureNodes = new Dictionary<Address, ServerInfo>();
                            foreach (ServerNode node in shardConfig.Servers.Nodes.Values)
                            {
                                ServerInfo serverInfo = new ServerInfo();
                                serverInfo.Address = new Address(node.Name, shardConfig.Port);
                                shardInfo.AddConfigureNode(serverInfo);
                            }
                           
                        }
                        clusterInfo.AddShard(shardInfo);
                    }
                }

                if (configuration.Databases != null && configuration.Databases.Configurations != null)
                {
                    foreach (DatabaseConfiguration databaseConfiguration in configuration.Databases.Configurations.Values)
                    {
                        DatabaseInfo dbInfo = new DatabaseInfo();
                        dbInfo.Name = databaseConfiguration.Name;
                        if (databaseConfiguration.Storage.Collections.Configuration != null)
                        {
                            foreach (CollectionConfiguration cc in databaseConfiguration.Storage.Collections.Configuration.Values)
                            {
                                CollectionInfo colInfo = new CollectionInfo();
                                colInfo.Name = cc.CollectionName;
                                colInfo.ParitionKey = GetPartitionKey(cc);
                                dbInfo.AddCollection(colInfo);
                            }
                        }
                        clusterInfo.AddDatabase(dbInfo);
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("RemoveCluster", ex.ToString());
                throw;
            }
            return clusterInfo;
        }

        internal ServerNode GetServerNodeFromServerInfo(ServerInfo serverInfo, int priority)
        {
            if (serverInfo == null)
                return null;

            ServerNode node = new ServerNode();
            node.Name = serverInfo.Address.IpAddress.ToString();
            node.Priority = priority;

            return node;
        }

        private ServerInfo[] GetSecondaryNodes(ShardInfo shardInfo)
        {
            List<ServerInfo> secondaryNodes = new List<ServerInfo>();
            if (shardInfo.Primary != null && shardInfo.RunningNodes != null)
            {
                foreach (ServerInfo node in shardInfo.RunningNodes.Values)
                {
                    if (!node.Address.Equals(shardInfo.Primary.Address))
                    {
                        secondaryNodes.Add(node);
                    }
                }
            }
            return secondaryNodes.ToArray();
        }

        //private void LoadConfiguration()
        //{

        //    ConfigurationBuilder builder = new ConfigurationBuilder(_fileName);
        //    builder.RegisterRootConfigurationObject(typeof(Alachisoft.NoSQL.Core.Configuration.ClusterConfiguration));
        //    builder.ReadConfiguration();
        //    ClusterConfiguration[] configuration = new ClusterConfiguration[builder.Configuration.Length];
        //    builder.Configuration.CopyTo(configuration, 0);

        //    foreach (ClusterConfiguration cc in configuration)
        //    {
        //        if (cc != null && cc.Name != null && !_configurationStore.ContainsCluster(cc.Name.ToLower()))
        //        {
        //            _clusterConfiguration.Add(cc.Name.ToLower(), cc);


        //            //if (_configurationStore.GetClusterInfo(cc.Name) == null)
        //            //_distributionMetadataStore.AddClusterInfo(cc.Name, GetClusterInfoFromClusterConfiguration(cc));
        //        }
        //    }

        //ConfigurationBuilder builder = new ConfigurationBuilder(_databaseConfingFileName);
        //builder.RegisterRootConfigurationObject(typeof(Alachisoft.NoSQL.Core.Configuration.ClusterConfiguration));
        //builder.ReadConfiguration();
        //ClusterConfiguration[] configuration = new ClusterConfiguration[builder.Configuration.Length];
        //builder.Configuration.CopyTo(configuration, 0);

        //}

        private void LoadConfiguration()
        {
            ConfigurationStore.GetAllClusterConfiguration();
            ClusterInfo[] infos = ConfigurationStore.GetAllClusterInfo();
            ConfigurationStore.GetAllMembershipData();


            if (infos != null)
            {
                ConfigurationStore.GetAllDistributionStrategies(infos);
            }
        }

        private void LoadConfigServerConfiguration()
        {

            ConfigurationBuilder builder = new ConfigurationBuilder(_configServerConfigurationFileName);
            builder.RegisterRootConfigurationObject(typeof(ConfigServerConfiguration));
            builder.ReadConfiguration();
            ConfigServerConfiguration[] configuration = new ConfigServerConfiguration[builder.Configuration.Length];
            builder.Configuration.CopyTo(configuration, 0);
            if (configuration.Length == 1)
            {
                ConfigServerConfiguration cSC = configuration.First();
                if (cSC != null && cSC.Name != null && !ConfigurationStore.ContainsCluster(cSC.Name.ToLower()))
                {
                    _configurationServerConfig = cSC;

                    //if (_configurationStore.GetClusterInfo(cc.Name) == null)
                    //_distributionMetadataStore.AddClusterInfo(cc.Name, GetClusterInfoFromClusterConfiguration(cc));
                }
            }

            else
            {
              //  CreateDefaultConfigCluster();
            }

        }



        private void WriteXMLToFile(string xml, string fileName)
        {

            if (fileName.Length == 0)
            {
                throw new System.Exception("file not found");
            }

            FileStream fs = null;
            StreamWriter sw = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Create);
                sw = new StreamWriter(fs);

                sw.Write(xml);
                sw.Flush();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
            finally
            {
                if (sw != null)
                {
                    try
                    {
                        sw.Close();
                    }
                    catch (System.Exception)
                    { }
                    sw.Dispose();
                    sw = null;
                }
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                    }
                    catch (System.Exception)
                    { }
                    fs.Dispose();
                    fs = null;
                }
            }
        }



        private void SaveConfiguration()
        {
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                List<ClusterConfiguration> configurations = new List<ClusterConfiguration>();

                foreach (ClusterConfiguration config in ConfigurationStore.GetAllClusterConfiguration())
                {
                    configurations.Add(config);
                }
                SaveConfiguration(configurations.ToArray());

                //_membershipMetadatastore.Save();
                //_distributionMetadataStore.Save();

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        private void SaveConfigServerConfiguration(object[] configuration)
        {
            StringBuilder xml = new StringBuilder();
            if (configuration != null && configuration.Length > 0)
            {
                ConfigurationBuilder builder = new ConfigurationBuilder(configuration);
                builder.RegisterRootConfigurationObject(typeof(ConfigServerConfiguration));
                xml.Append(builder.GetXmlString());
            }
            WriteXMLToFile(xml.ToString(), _configServerConfigurationFileName);
        }

        private void SaveConfiguration(object[] configuration)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<configuration>\r\n");
            if (configuration != null && configuration.Length > 0)
            {
                ConfigurationBuilder builder = new ConfigurationBuilder(configuration);
                builder.RegisterRootConfigurationObject(typeof(ClusterConfiguration));
                xml.Append(builder.GetXmlString());
            }
            xml.Append("\r\n</configuration>");
            WriteXMLToFile(xml.ToString(), _databaseConfingFileName);
        }

        public void CreateDatabase(string cluster, Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration configuration, ISessionId sessionId)
        {
            ConfigurationStore.Transaction transaction = null;

            ClusterConfiguration config = GetClusterConfiguration(cluster);
            if (config == null || config.Deployment == null || config.Deployment.Shards == null || config.Deployment.Shards.Count == 0)
            {
                throw new DataException("Please add atleast 1 shard before creating database");
            }

            try
            {
                AssignObjectUID(configuration);
                ClusterConfiguration existingConfiguration = null;
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                DatabaseConfiguration.ValidateConfiguration(configuration);

                transaction = ConfigurationStore.BeginTransaction(cluster);

                existingConfiguration = transaction.GetClusterConfiguration(cluster);

                if (existingConfiguration == null)
                {
                    if (string.Compare(cluster, MiscUtil.LOCAL, true) == 0)
                    {
                        CreateLocalCluster(sessionId);
                        existingConfiguration = GetLocalDatabaseConfiguration();
                        //transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                    }
                    else
                        throw new Exception("Given cluster does not exist");
                }

                if (existingConfiguration != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;
                    if (!dbConfs.ContainsDatabase(configuration.Name))
                    {

                        //UMER

                        if (configuration != null && configuration.Storage != null && configuration.Storage.StorageProvider != null && configuration.Storage.StorageProvider.LMDBProvider != null)
                            configuration.Storage.StorageProvider.LMDBProvider.MaxCollections++;
                        configuration.Type = DatabaseType.Normal;
                        dbConfs.AddDatabase(configuration);


                        //meta-data
                        ClusterInfo info = transaction.GetClusterInfo(cluster);
                        DatabaseInfo dInfo = GetDatabaseInfoFromConfiguration(configuration);
                        dInfo.Type = DatabaseType.Normal;
                        if (dInfo != null)
                        {
                            info.AddDatabase(dInfo);
                        }
                        transaction.InsertOrUpdateClusterInfo(info);
                    }
                    else
                    {
                        throw new Exception("'" + configuration.Name + "' already exists. Provide a different database name.");
                    }


                    ArrayList parameter = new ArrayList() { cluster, configuration };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.CreateDatabase, 1, null, null);

                    DeploymentConfiguration deploymentConf = existingConfiguration.Deployment;

                    // List<ServerNode> successfullNodes 
                    foreach (ShardConfiguration shard in deploymentConf.Shards.Values)
                    {
                        foreach (ServerNode server in shard.Servers.Nodes.Values)
                        {
                            try
                            {
                                RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                bool s = dbMgtSession.CreateDatabase(cluster, shard.Name, configuration, null);
                            }
                            catch (Exception e)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("CreateDatabase", "An error occured while creating database " + configuration.Name + " on servre '" + server.Name + "'. " + e.ToString());

                            }
                        }
                    }

                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("CreateDatabase", ex.ToString());

                throw;
            }
            finally
            {
                //SaveConfiguration();
                _rwLock.ReleaseWriterLock();
            }
        }

        private CollectionConfiguration GenerateAttachmentCollectionConf(string path)
        {
            CollectionConfiguration attachmentCollectionConf = new CollectionConfiguration();
            attachmentCollectionConf.EvictionConfiguration = new EvictionConfiguration();
            attachmentCollectionConf.EvictionConfiguration.EnabledEviction = true;
            attachmentCollectionConf.EvictionConfiguration.Policy = "lru";
            attachmentCollectionConf.CollectionName = AttachmentAttributes.ATTACHMENT_COLLECTION;
            attachmentCollectionConf.Path = path;
            //cc.CollectionSize = cParam.CappedCollSize;
            //cc.MaxDocuments = cParam.CappedCollMaxDocs;
            attachmentCollectionConf.DistributionStrategy = new DistributionStrategyConfiguration();

            attachmentCollectionConf.PartitionKey = new PartitionKeyConfiguration();
            attachmentCollectionConf.PartitionKey = new PartitionKeyConfiguration();
            attachmentCollectionConf.PartitionKey.PartitionKeyAttributes = new Dictionary<string, PartitionKeyConfigurationAttribute>();
            //new PartitionKeyConfigurationAttribute[attributes.Length];
            PartitionKeyConfigurationAttribute pka = new PartitionKeyConfigurationAttribute();
            pka.Name = "_key";
            pka.Type = "string";
            attachmentCollectionConf.PartitionKey.PartitionKeyAttributes.Add(pka.Name, pka);

            IDistributionStrategy strategy = null;
            attachmentCollectionConf.DistributionStrategy.Name = string.Empty;
            return attachmentCollectionConf;
        }

        public void DropDatabase(string cluster, string database, bool dropFiles, bool recoveryRollback = false)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration existingConfiguration = ConfigurationStore.GetClusterConfiguration(cluster);

                //only updating meta data
                if (existingConfiguration != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;

                    if (!dbConfs.ContainsDatabase(database))
                    {
                        throw new Exception("Given database does not exist");
                    }

                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    existingConfiguration = transaction.GetClusterConfiguration(cluster);
                    dbConfs = existingConfiguration.Databases;

                    if (dbConfs.ContainsDatabase(database))
                    {
                        dbConfs.RemoveDatabase(database);

                        //meta-data
                        ClusterInfo info = transaction.GetClusterInfo(cluster);
                        info.RemoveDatabase(database);

                        transaction.InsertOrUpdateClusterInfo(info);
                    }

                    ArrayList parameter = new ArrayList() { cluster, database };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.DropDatabase, 1, null, null);

                    DeploymentConfiguration deploymentConf = existingConfiguration.Deployment;
                    foreach (ShardConfiguration shard in deploymentConf.Shards.Values)
                    {
                        foreach (ServerNode server in shard.Servers.Nodes.Values)
                        {
                            try
                            {
                                RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                dbMgtSession.DropDatabase(cluster, shard.Name, database, dropFiles);
                            }
                            catch (Exception e)
                            {
                                if (!recoveryRollback)
                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                        LoggerManager.Instance.CONDBLogger.Error("DropDatabase", "An error occured while dropping database '" + database + "' from server " + server.Name + ". " + e.ToString());

                            }
                        }
                    }
                  
                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                    ConfigurationStore.CommitTransaction(transaction);
                }
                else
                    throw new Exception("Given cluster does not exist");
            }
            catch (System.Exception ex)
            {
                if (!recoveryRollback)
                {
                    if (transaction != null)
                        ConfigurationStore.RollbackTranscation(transaction);

                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("DropDatabase", ex.ToString());
                }
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        public void CreateCollection(string cluster, string database, CollectionConfiguration configuration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                transaction = ConfigurationStore.BeginTransaction(cluster);
                CreateCollection(transaction, cluster, database, configuration);

                ConfigurationStore.CommitTransaction(transaction);

            }
            catch (Exception e)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
                throw;
            }
        }

        public void MoveCollection(string cluster, string database, string collection, string newShard)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                ClusterConfiguration existingConfiguration = null;
                //configuration.Storage.StorageProvider.LMDBProvider.MaxCollections++;
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                transaction = ConfigurationStore.BeginTransaction(cluster);

                existingConfiguration = transaction.GetClusterConfiguration(cluster);
                ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);
                if (existingConfiguration == null)
                {
                    if (String.Compare(cluster, MiscUtil.LOCAL, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        existingConfiguration = GetLocalDatabaseConfiguration();
                        transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                    }
                    else
                        throw new Exception("Given cluster does not exist");
                }
                else
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        DatabaseConfiguration configuration = dbConfs.GetDatabase(database);
                        if (configuration.Storage != null && configuration.Storage.Collections != null)
                        {
                            if (configuration.Storage.Collections.ContainsCollection(collection))
                            {
                                CollectionConfiguration collectionConfiguration = configuration.Storage.Collections.GetCollection(collection);

                                if (collectionConfiguration.Shard.Equals(newShard,StringComparison.OrdinalIgnoreCase))
                                    throw new Exception(collection + " already exists on " + newShard);

                                collectionConfiguration.Shard = newShard;
                                IDistributionStrategy distribution = transaction.GetDistributionStrategy(cluster,database, collectionConfiguration.CollectionName);

                                var nonShardedDistribution = distribution as NonShardedDistributionStrategy;
                                if (nonShardedDistribution != null) nonShardedDistribution.UpdateShard(newShard);
                       
                                DeploymentConfiguration deploymentConfs = existingConfiguration.Deployment;

                                if (!deploymentConfs.Shards.ContainsKey(newShard))
                                    throw new Exception(newShard + " does not exists.");

                                ShardConfiguration newShardConfiguration = deploymentConfs.Shards[newShard];

                                if (newShardConfiguration.Servers.Nodes.Count > 0)
                                {
                                    foreach (ServerNode server in newShardConfiguration.Servers.Nodes.Values)
                                    {
                                        RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                        dbMgtSession.CreateCollection(cluster, newShardConfiguration.Name, database,
                                            collectionConfiguration, distribution);
                                    }
                                }

                                if (clusterInfo.Databases != null)
                                {
                                    clusterInfo.GetDatabase(database)
                                        .GetCollection(collectionConfiguration.CollectionName)
                                        .DistributionStrategy = distribution;
                                    clusterInfo.GetDatabase(database)
                                        .GetCollection(collectionConfiguration.CollectionName)
                                        .CollectionShard = newShard;
                                    transaction.InsertOrUpdateDistributionStrategy(clusterInfo, database,
                                        collectionConfiguration.CollectionName);
                                    transaction.InsertOrUpdateClusterInfo(clusterInfo);
                                }
                            }
                            else
                            {
                                throw new Exception(collection + " not exists");
                            }
                        }
                        else
                        {
                            throw new Exception("no collection exists in " + database);
                        }
                    }
                    else
                    {
                        throw new Exception("'" + database + "' not exists.");
                    }


                    ArrayList parameter = new ArrayList() { cluster, database, collection, newShard };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.MoveCollection, 1, null,
                        null);

                    //CreateCollection(transaction, cluster, configuration.Name, GenerateAttachmentCollectionConf());
                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("MoveCollection", ex.ToString());

                throw;
            }
            finally
            {
                //SaveConfiguration();
                _rwLock.ReleaseWriterLock();
            }

        }

        private void CreateCollection(ConfigurationStore.Transaction transaction, string cluster, string database, CollectionConfiguration configuration)
        {
            try
            {
                AssignObjectUID(configuration);
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                CollectionConfiguration.ValidateConfiguration(configuration);

                ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);

                //only updating meta data
                if (existingConfiguration != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;
                    IDistributionStrategy strategy = null;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        DatabaseConfiguration selectedDatabase = dbConfs.GetDatabase(database);
                        if (selectedDatabase.Storage.Collections == null)
                            selectedDatabase.Storage.Collections = new CollectionConfigurations();
                        //if (selectedDatabase.Storage.Collections.Configuration == null)
                        //    selectedDatabase.Storage.Collections.Configuration = new CollectionConfiguration[1];


                        if (!selectedDatabase.Storage.Collections.ContainsCollection(configuration.CollectionName.ToLower()))
                        {
                            StorageProviderConfiguration spc = selectedDatabase.Storage.StorageProvider;
                            if (spc.StorageProviderType == ProviderType.LMDB)
                            {
                                if (selectedDatabase.Storage.Collections.Configuration.Count >= spc.LMDBProvider.MaxCollections)
                                    throw new Exception("Max Collection limit reached.");
                            }

                            DistributionType distributonType = GetDistributionType(configuration.DistributionStrategy.Name);

                            CollectionInfo cInfo = GetCollectionInfoAndUpdateCappedCollectionShard(ref configuration, existingConfiguration,selectedDatabase);
                            AssignObjectUID(cInfo);
                            selectedDatabase.Storage.Collections.AddCollection(configuration);

                            //meta-data
                            ClusterInfo info = transaction.GetClusterInfo(cluster);

                            if (cInfo != null)
                            {
                                info.GetDatabase(database).AddCollection(cInfo);
                                transaction.InsertOrUpdateClusterInfo(info);
                            }

                            switch (distributonType)
                            {
                                case DistributionType.NonSharded:
                                    strategy = new NonShardedDistributionStrategy();
                                    ConfigureDistributionStategy(transaction, cluster, database, configuration.CollectionName, strategy, false);
                                    break;
                            }

                            if (strategy != null)
                            {
                                info.GetDatabase(database).GetCollection(configuration.CollectionName).SetDistributionStrategy(configuration.DistributionStrategy, strategy);
                            }

                            transaction.InsertOrUpdateClusterInfo(info);
                            transaction.InsertOrUpdateDistributionStrategy(info, database, configuration.CollectionName);
                        }
                        else
                        {
                            throw new Exception("Collection with name: " + configuration.CollectionName + " already exists");
                        }
                    }
                    else
                    {
                        throw new Exception("Database with name: " + database + " does not exist");
                    }

                    ArrayList parameter = new ArrayList() { cluster, database, configuration };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.CreateCollection, 1, null, null);

                    DeploymentConfiguration deploymentConfs = existingConfiguration.Deployment;
                    foreach (ShardConfiguration shard in deploymentConfs.Shards.Values)
                    {
                        if (shard.Servers.Nodes.Count > 0)
                        {
                            foreach (ServerNode server in shard.Servers.Nodes.Values)
                            {
                                try
                                {
                                    RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                    dbMgtSession.CreateCollection(cluster, shard.Name, database, configuration, strategy);
                                }
                                catch (Exception e)
                                {
                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                        LoggerManager.Instance.CONDBLogger.Error("CreateCollection", "An error occured while creating collection '" + configuration.CollectionName + "' on " + server.Name + ". Databaase is " + database + ". " + e.ToString());
                                }
                            }
                        }
                    }
                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                }
                else
                    throw new Exception("Given cluster does not exist");
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("CreateCollection", ex.ToString());
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                // SaveConfiguration();
            }
        }

        public void DropCollection(string cluster, string database, string collection)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                transaction = ConfigurationStore.BeginTransaction(cluster);
                ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);

                //only updating meta data
                if (existingConfiguration != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;
                    //List<DatabaseConfiguration> databaseList = .Configurations.ToList<DatabaseConfiguration>();
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;

                        if (colConfs.ContainsCollection(collection))
                        {
                            colConfs.RemoveCollection(collection);

                            //meta-data
                            ClusterInfo info = transaction.GetClusterInfo(cluster);

                            info.GetDatabase(database).RemoveCollection(collection);

                            transaction.InsertOrUpdateClusterInfo(info);
                            transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                        }
                        else
                        {
                            throw new Exception(string.Format("Specified collection '{0}' does not exists in database '{1}'", collection, database));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Specified database '{0}' does not exist", database));
                    }

                    ArrayList parameter = new ArrayList() { cluster, database, collection };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.DropCollection, 1, null, null);

                    DeploymentConfiguration deploymentConfs = existingConfiguration.Deployment;
                    foreach (ShardConfiguration shard in deploymentConfs.Shards.Values)
                    {
                        foreach (ServerNode server in shard.Servers.Nodes.Values)
                        {
                            try
                            {
                                RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                dbMgtSession.DropCollection(cluster, shard.Name, database, collection);
                            }
                            catch (Exception e)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("DropDatabase", "An error occured while dropping collection '" + collection + "' from server " + server.Name + ". " + e.ToString());

                            }
                        }
                    }
                }
                else
                    throw new Exception("Given cluster does not exist");

                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("DropCollection", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        public void UpdateMembership(string cluster, string shard, Membership membership)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    transaction.InsertOrUpdateMembershipData(membership);
                }
                else
                    throw new System.Exception("Cluster with given name does not exist.");
                ArrayList parameter = new ArrayList() { cluster, shard, membership };
                SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.UpdateMembership, 1, null, null);
                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateMembership", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                //SaveConfiguration();
                _rwLock.ReleaseWriterLock();
            }
        }

        #region ConfigurationCluster Management

        private void SaveConfigServerConfiguration()
        {
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);


                SaveConfigServerConfiguration(new ConfigServerConfiguration[] { _configurationServerConfig });
                //_membershipMetadatastore.Save();
                //_distributionMetadataStore.Save();

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        internal void CreateConfigurationCluster(ConfigServerConfiguration serverConfig, int heartBeat, ReplicationConfiguration repConfig, string displayName)
        {
            if (IsReservedWord(serverConfig.Name))
                throw new Exception(serverConfig.Name + " is a reserved word");

            if (VerifyConfigurationClusterAvailability(serverConfig.Name))
            {
                _configurationServerConfig = serverConfig;

                StartConfigurationCluster();
                AssignObjectUID(_configurationServerConfig);
                if (!(ConfigurationStore.ContainsCluster(MiscUtil.CLUSTERED)|| _isPassive))
                {
                    ClusterConfiguration cluster = GetClusterConfiguration(heartBeat, displayName, repConfig);
                    CreateCluster(MiscUtil.CLUSTERED, cluster);
                }
            }
            SaveConfigServerConfiguration();
        }

        internal void RemoveConfigurationCluster(string configClusterName)
        {
            _isConfigClusterEstablished = false;
            RemoveCluster(configClusterName);
            _cfgCluster.Dispose();
            _configurationServerConfig = new ConfigServerConfiguration();
            SaveConfigServerConfiguration();
            StopConfigurationServer();
        }

        public bool IsPassive
        {
            get { return _cfgCluster != null && !_cfgCluster.IsActive; }
            set { _isPassive = value; }
        }

        internal void AddNodeToConfigurationCluster(string name, ServerNode node)
        {
            if (_configurationServerConfig.Servers.ContainsNode(node.Name))
                throw new Exception("Node: [" + node.Name + " ] already exists in configuration cluster.");
            if (_configurationServerConfig.Servers.Nodes.Count >= MiscUtil.MAX_CS_LIMIT)
                throw new Exception("Maximum two nodes are allowed to add into configuration cluster. ");

            if (!IsPassive)
            {
                foreach (var shardConfiguration in ConfigurationStore.GetClusterConfiguration(name).Deployment.Shards.Values)
                {
                    foreach (var shardNode in shardConfiguration.Servers.Nodes.Values)
                    {
                        try
                        {
                            RemoteManagementSession session = GetManagementSession(shardNode.Name);
                            session.NodeAddedToConfigurationCluster(name, node);
                        }
                        catch (Exception e)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("AddNodeToConfigurationCluster", "An error occured while adding node'" + node.Name + "' to configuration cluster '" + name + "' of DB Node '" + shardNode.Name + "'. " + e.ToString());
                        }
                    }
                }
            }
            lock (this)
            {
                _configurationServerConfig.Servers.AddNode(node);
                SaveConfigServerConfiguration();
                _cfgCluster.UpdateConfiguration(_configurationServerConfig);
            }

        }

        internal void UpdateConfigServerNodePriority(string cluster, string nodeName, int priority)
        {
            if (string.IsNullOrEmpty(cluster))
                throw new ArgumentNullException(cluster);
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentNullException(nodeName);
            lock (this)
            {
                ServerNode node = _configurationServerConfig.Servers.GetNode(nodeName);
                if(node == null) throw new Exception("'"+nodeName+" does not exist in config server configuration");
                node.Priority = priority;
                SaveConfigServerConfiguration();
                _cfgCluster.UpdateConfiguration(_configurationServerConfig);

            }
        }






        internal void RemoveNodeFromConfigurationCluster(string cluster, ServerNode node)
        {
            if (_configurationServerConfig.Name.Equals(cluster))
            {
                if (!IsPassive)
                {
                    foreach (var shardConfiguration in ConfigurationStore.GetClusterConfiguration(cluster).Deployment.Shards.Values)
                    {
                        foreach (var shardNode in shardConfiguration.Servers.Nodes.Values)
                        {
                            try
                            {
                                RemoteManagementSession session = GetManagementSession(shardNode.Name);
                                session.NodeRemovedFromConfigurationCluster(cluster, node);
                            }
                            catch (Exception e)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("RemoveNodeFromConfigurationCluster", "An error occured while removing node'" + node.Name + "' from configuration cluster '" + cluster + "' of DB Node '" + shardNode.Name + "'. " + e.ToString());
                            }
                        }
                    }
                }

                string localIp = ConfigurationSettings<CSHostSettings>.Current.IP.ToString();
                if (node.Name.Equals(localIp))
                {
                    StopConfigurationServer();
                    _configurationServerConfig = new ConfigServerConfiguration();
                    _cfgCluster.Dispose();
                    //_cfgCluster = new ConfigurationCluster(_configurationServerConfig);
                    _isConfigClusterEstablished = false;
                    RemoveClusterLocally(cluster);
                    SaveConfigServerConfiguration();
                }
                else if (_configurationServerConfig.Servers.ContainsNode(node.Name))
                {
                    lock (this)
                    {
                        _configurationServerConfig.Servers.RemoveNode(node.Name);
                        SaveConfigServerConfiguration();
                        _cfgCluster.UpdateConfiguration(_configurationServerConfig);
                    }
                }
            }
        }

        private void StartConfigurationServer()
        {
            try
            {
                LoadConfiguration();

                StartConfigurationCluster();
            }
            catch (System.Exception ex)
            {
                // TODO add logger
            }
        }

        private void StartConfigurationCluster()
        {
            if (_configurationServerConfig != null && _configurationServerConfig.Servers != null)
            {
                List<ServerNode> nodesList = _configurationServerConfig.Servers.Nodes.Values.ToList();
                string localIp = ConfigurationSettings<CSHostSettings>.Current.IP.ToString();
                ServerNode thisNode = null;
                bool IsThisNodePart = false;
                foreach (ServerNode sN in nodesList)
                {
                    if (sN.Name.Equals(localIp))
                    {
                        thisNode = sN.Clone() as ServerNode;
                        IsThisNodePart = true;
                    }
                }

                _cfgCluster = new ConfigurationCluster(localIp, this);
                _cfgCluster.Initialize(_configurationServerConfig);
                IsStarted = true;
            }
        }



        internal void StopConfigurationServer(string name)
        {
            if (_configurationServerConfig.Name.Equals(name))
            {
                StopConfigurationServer();
            }
        }

        private void StopConfigurationServer()
        {

        }

        internal bool VerifyConfigurationServerAvailability()
        {
            var cc = _configurationStore.GetClusterConfiguration(MiscUtil.CLUSTERED);
            if (cc != null)
            {
                if (cc.Deployment != null && cc.Deployment.Shards != null && cc.Deployment.Shards.Count > 0)
                    return false;
                if (cc.Databases != null && cc.Databases.Configurations != null &&
                    cc.Databases.Configurations.Count > 0)
                    return false;
            }
            return true;
        }

        internal bool VerifyConfigurationClusterAvailability(string configClusterName)
        {
            
                if (_configurationServerConfig != null && _configurationServerConfig.Servers != null &&
                    _configurationServerConfig.Servers.Nodes.Count != 0)
                    return false;
                return true;
        }

        internal void StartConfigurationServer(string name)
        {
            if (_configurationServerConfig.Name.Equals(name.ToLower()))
            {
                StartConfigurationServer();
            }
        }


        internal bool VerifyConfigurationCluster(string configClusterName)
        {
            if (_configurationServerConfig.Name.Equals(configClusterName))
                return true;
            else
                return false;
        }

        internal bool VerifyConfigurationClusterUID(string UID)
        {
            if (_configurationServerConfig.UID.Equals(UID))
                return true;
            else
                return false;

        }

        internal bool VerifyConfigurationClusterPrimery(string configClusterName)
        {
            if (_configurationServerConfig.Name.Equals(configClusterName))
               return true;

            return false;
        }

        internal void VerifyValidDatabaseOperation(string cluster)
        {
            if (cluster.Equals(MiscUtil.LOCAL))
            {
                return;
            }
            else if (!ConfigurationStore.ContainsCluster(cluster))
            {
                throw new Exception("Cluster: " + cluster + " not found.");
            }
          

        }

        internal void VerifyValidClusterOpperation(string cluster)
        {
            if (cluster.Equals(MiscUtil.LOCAL))
            {
                throw new Exception("Can't perform cluster operation on Local databases");
            }
            else if (!ConfigurationStore.ContainsCluster(cluster))
            {
                throw new Exception("Cluster: " + cluster + " not found.");
            }
           

        }

        #endregion

        private DatabaseInfo GetDatabaseInfoFromConfiguration(DatabaseConfiguration configuration)
        {
            if (configuration != null)
            {
                DatabaseInfo database = new DatabaseInfo();
                database.UID = configuration.UID;
                database.Name = configuration.Name;
                if (configuration.Storage != null && configuration.Storage.Collections != null && configuration.Storage.Collections.Configuration != null)
                {
                    foreach (CollectionConfiguration cc in configuration.Storage.Collections.Configuration.Values)
                    {
                        CollectionInfo colInfo = new CollectionInfo();
                        AssignObjectUID(colInfo);
                        cc.UID = colInfo.UID;
                        colInfo.Name = cc.CollectionName;
                        colInfo.ParitionKey = GetPartitionKey(cc);
                        database.AddCollection(colInfo);
                    }
                }
                return database;
            }
            return null;
        }

        private CollectionInfo GetCollectionInfoAndUpdateCappedCollectionShard(
            ref CollectionConfiguration configuration, ClusterConfiguration clusterConfig,DatabaseConfiguration database)
        {
            if (configuration != null)
            {
                CollectionInfo collection = new CollectionInfo();
                collection.Name = configuration.CollectionName;
                collection.ParitionKey = GetPartitionKey(configuration);

                if (string.IsNullOrEmpty(configuration.Shard) ||
                    "All".Equals(configuration.Shard, StringComparison.OrdinalIgnoreCase))
                    configuration.Shard = GetFirstShardName(clusterConfig,database);

                if (configuration.Shard == null || clusterConfig.Deployment.ContainsShard(configuration.Shard))
                    collection.CollectionShard = configuration.Shard;
                else
                    throw new Exception("Specified shard '" + configuration.Shard + "' does not exist.");


                collection.CollectionShard = configuration.Shard;


                return collection;
            }
            return null;
        }

        private String GetFirstShard(ClusterInfo clusterInfo)
        {
            if (clusterInfo != null)
                return clusterInfo.ShardInfo.Keys.OrderBy(shard => shard).First();
            return null;
        }
        private CollectionInfo GetCollectionInfoFromConfiguration(CollectionConfiguration configuration)
        {
            if (configuration != null)
            {
                CollectionInfo collection = new CollectionInfo();
                collection.Name = configuration.CollectionName;
                collection.ParitionKey = GetPartitionKey(configuration);
                return collection;
            }
            return null;
        }

        internal void AddClientChannel(DualChannel channel)
        {
            lock (_connectedClientList)
            {
                if (channel != null && !_connectedClientList.ContainsKey(channel))
                    _connectedClientList.Add(channel, false);
            }
        }

        internal void UpdateClientChannel(DualChannel channel)
        {
            lock (_connectedClientList)
            {
                if (channel != null && _connectedClientList.ContainsKey(channel))
                    _connectedClientList[channel] = true;
            }
        }

        internal void RemoveClientChannel(DualChannel channel)
        {
            lock (_connectedClientList)
            {
                if (channel != null && _connectedClientList.ContainsKey(channel))
                    _connectedClientList.Remove(channel);
            }
        }

        internal void AddNodeChannel(DualChannel channel)
        {
            lock (_connectedNodes)
            {
                if (channel != null && !_connectedNodes.Contains(channel))
                    _connectedNodes.Add(channel);
            }
        }

        internal void RemoveClientChannel(Address address)
        {
            try
            {
                lock (_connectedClientList)
                {
                    List<DualChannel> channelList = _connectedClientList.Keys.ToList<DualChannel>();
                    DualChannel channel = channelList.Find(p => p.PeerAddress.Equals(address));
                    if (channel != null && !channel.Connected)
                    {
                        _connectedClientList.Remove(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ConfigurationServer.RemoveClientChannel", ex.Message);
            }
        }

        internal void RemoveNodeChannel(Address address)
        {
            try
            {
                lock (_connectedNodes)
                {
                    DualChannel channel = _connectedNodes.Find(p => p.PeerAddress.Equals(address));
                    if (channel != null && !channel.Connected)
                    {
                        _connectedNodes.Remove(channel);//.RemoveAll(p => p.PeerAddress.Equals(address));
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ConfigurationServer.RemoveNodeChannel", ex.Message);
            }
        }

        internal void SendNotification(ConfigChangeEventArgs args)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _nodeContext.LocalShardName != null ? _nodeContext.LocalShardName : "", DatabaseName = "" });
            ChangeType type = ChangeType.None;
            if (args != null)
                type = args.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType);
            if (type != null)
            {
                try
                {
                    List<DualChannel> cloneList = new List<DualChannel>();


                    if (type == ChangeType.ShardAdded || type == ChangeType.ShardRemovedGraceful || type == ChangeType.ShardRemovedForceful || type == ChangeType.DistributionChanged || type == ChangeType.DistributionStrategyConfigured || type == ChangeType.CollectionCreated || type == ChangeType.CollectionMoved || type == ChangeType.CollectionDropped || type == ChangeType.ConfigRestored || type == ChangeType.ResyncDatabase || type == ChangeType.IntraShardStateTrxferCompleted || type == ChangeType.NodeLeft || type == ChangeType.PrimaryGone || type == ChangeType.RangeUpdated || type == ChangeType.NewRangeAdded || type == ChangeType.ConfigServerAdded || type == ChangeType.ConfigServerRemoved || type == ChangeType.ConfigServerDemoted)
                    {
                        lock (_connectedNodes)
                        {
                            _connectedNodes.ForEach((item) =>
                            {
                                cloneList.Add(item);
                            });
                        }

                        foreach (DualChannel channel in cloneList) // Send Notification to shard nodes
                        {
                            if (channel.Connected)
                            {
                                try
                                {
                                    channel.SendMessage(args, true);
                                }
                                catch (System.Exception ex)
                                {
                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                        LoggerManager.Instance.CONDBLogger.Error("SendNotification", ex.ToString());
                                }
                            }
                        }

                    }

                    cloneList.Clear();

                    lock (_connectedClientList)
                    {
                        cloneList = _connectedClientList.Keys.ToList<DualChannel>();
                    }

                    foreach (DualChannel channel in cloneList) //Send Notification to query router && clients
                    {
                        if (channel.Connected && _connectedClientList.ContainsKey(channel) && _connectedClientList[channel])
                        {
                            try
                            {
                                channel.SendMessage(args, true);
                            }
                            catch (System.Exception ex)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("SendNotification", ex.ToString());

                            }
                        }
                    }

                    // notify recovery manager of memebership change, create a small running task to end the job
                    if (type == ChangeType.ShardAdded || type == ChangeType.ShardRemovedGraceful || type == ChangeType.ShardRemovedForceful || type == ChangeType.PrimaryGone)
                    {

                        System.Threading.Tasks.Task.Factory.StartNew(() => _recoveryManager.OnMembershipChanged(args));
                    }

                }
                catch (System.Exception ex)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("SendNotification", ex.ToString());

                }

            }
        }

        public Membership ReportingNodeLeft(string cluster, string shard, ServerNode node)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;
                //only updating meta data
                List<ServerInfo> serverNodes = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    DeploymentConfiguration deploymentConf = transaction.GetClusterConfiguration(cluster).Deployment;
                    int port = deploymentConf.GetShard(shard).Port;

                    ShardInfo info = transaction.GetClusterInfo(cluster).GetShard(shard);
                    if (info != null)
                    {
                        if (info.ConfigureNodes != null)
                        {
                            //serverNodes = info.ConfigureNodes.ToList<ServerInfo>();
                            foreach (ServerInfo sInfo in info.ConfigureNodes.Values)
                            {
                                Address leavingNodeAddress = new Address(node.Name, port);
                                if (sInfo.Address.Equals(leavingNodeAddress))
                                {
                                    ServerInfo serverInf = info.GetRunningNode(sInfo.Address);
                                    if (serverInf != null)
                                    {
                                        serverInf.Status = Status.Stopped;
                                        info.RunningNodes.Remove(sInfo.Address);
                                        if (info.Primary != null && info.Primary.Address.Equals(leavingNodeAddress))
                                        {
                                            info.Primary = null;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Membership membership = _membershipMetadatastore.GetMemberShip(cluster, shard);

                    if (membership != null)
                    {
                        _membershipMetadatastore.RemoveNodeFromMemberList(cluster, shard, node);
                    }

                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                    ArrayList parameter = new ArrayList() { cluster, shard, node };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.ReportNodeLeaving, 1, oldConfiguration, oldClusterInfo);

                    ConfigurationStore.CommitTransaction(transaction);
                }
                return _membershipMetadatastore.GetMemberShip(cluster, shard);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ReportingNodeLeft", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
            return null;
        }

        public Membership ReportingNodeJoining(string cluster, string shard, ServerNode node)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                //only updating meta data
                List<ServerInfo> serverNodes = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    DeploymentConfiguration deploymentConf = transaction.GetClusterConfiguration(cluster).Deployment;
                    int port = deploymentConf.GetShard(shard).Port;

                    ShardInfo info = transaction.GetClusterInfo(cluster).GetShard(shard);
                    if (info != null)
                    {
                        if (info.ConfigureNodes != null)
                        {
                            //serverNodes = info.ConfigureNodes.ToList<ServerInfo>();
                            foreach (ServerInfo sInfo in info.ConfigureNodes.Values)
                            {
                                if (sInfo.Address.Equals(new Address(node.Name, port)))
                                {
                                    ServerInfo serverInf = info.GetConfiguredNode(sInfo.Address);
                                    if (serverInf != null)
                                    {
                                        serverInf.Status = Status.Running;
                                        if (!info.ContainsRunningNode(sInfo.Address))
                                            info.AddRunningNode(sInfo);
                                    }
                                }
                            }
                        }
                    }

                    //  _configurationStore.inser
                   
                    Membership memberShip = _membershipMetadatastore.GetMemberShip(cluster, shard);

                    if (memberShip == null)
                    {
                        memberShip = new Membership();
                        memberShip.Cluster = cluster;
                        memberShip.Shard = shard;
                        memberShip.Servers = new List<ServerNode>();
                        _membershipMetadatastore.AddMembership(cluster, shard, memberShip);
                    }

                    if (memberShip.Servers == null)
                        memberShip.Servers = new List<ServerNode>();

                    if (memberShip.Servers != null && !memberShip.Servers.Contains(node))
                        _membershipMetadatastore.AddNodeToMemberList(cluster, shard, node);

                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                    ArrayList parameter = new ArrayList() { cluster, shard, node };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.ReportNodeJoining, 1, oldConfiguration, oldClusterInfo);

                    ConfigurationStore.CommitTransaction(transaction);
                }
                return _membershipMetadatastore.GetMemberShip(cluster, shard);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ReportingNodeJoining", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

            return null;
        }

        public int ReportingHeartBeat(string cluster, string shard, ServerNode reportingNode, Membership membership, OperationId lastOpId)
        {
            int interval = 0;

            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = shard ?? "", DatabaseName = "" });
            try
            {
                try
                {
                    _rwHeartBeatLock.AcquireWriterLock(Timeout.Infinite);  //ask umer

                    _heartbeatreporting.AddToReport(cluster, shard, reportingNode);
                    _nodeMembership[cluster + shard + reportingNode.Name] = membership;
                }
                finally
                {
                    _rwHeartBeatLock.ReleaseWriterLock();
                }

                Membership existingMembership = _membershipMetadatastore.GetMemberShip(cluster, shard, true);

                if (membership != null && existingMembership != null && existingMembership.Servers != null)
                {
                    bool isPrimary = membership.Primary != null &&
                            membership.Primary.Equals(reportingNode);
                    if (!existingMembership.Servers.Contains(reportingNode))
                    {
                        _rwLock.AcquireWriterLock(Timeout.Infinite);
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                            LoggerManager.Instance.CONDBLogger.Info("ReportingHeartBeat",
                                "adding " + reportingNode.Name + " to membership for shard" + shard);

                        AddNodeToRunningList(cluster, shard, reportingNode, isPrimary);

                    }
                    if (isPrimary && lastOpId != null && cluster != null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                            LoggerManager.Instance.CONDBLogger.Debug("ReportingHeartBeat",
                                reportingNode.Name + " is the primary for shard " + shard + " with last operation as "
                                + lastOpId.ElectionId + ":" + lastOpId.ElectionBasedSequenceId);
                        ClusterInfo cInfo = _configurationStore.GetClusterInfo(cluster);
                        if (cInfo != null && shard != null)
                        {
                            ShardInfo sInfo = cInfo.GetShardInfo(shard);
                            if (sInfo != null)
                            {
                                sInfo.LastOperationId = lastOpId;
                                _configurationStore.InsertOrUpdateClusterInfo(cInfo);
                            }
                        }
                    }
                    if (_nodeMembership != null && _nodeMembership.Count > 0 && membership.Primary == null)
                    {
                        bool primaryExists = false;

                        List<ServerNode> servers = existingMembership.Servers;
                        if (servers != null)
                        {
                            foreach (ServerNode node in servers)
                            {
                                if (_nodeMembership.ContainsKey(cluster + shard + node.Name) &&
                                    ((Membership)_nodeMembership[cluster + shard + node.Name]).Primary != null)
                                {
                                    primaryExists = true;
                                }
                            }
                        }

                        //foreach (DictionaryEntry node in _nodeMembership)
                        //{

                        //    if (node.Value != null && ((Membership)node.Value).Primary != null)
                        //        primaryExists = true;
                        //}

                        if (!primaryExists)
                        {
                            //memberShip.ElectionId = null;
                            if (!_rwLock.IsWriterLockHeld)
                                _rwLock.AcquireWriterLock(Timeout.Infinite);

                            existingMembership.Primary = null;
                            _membershipMetadatastore.AddMembership(cluster, shard, existingMembership);
                        }
                    }
                }

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    interval = ConfigurationStore.GetClusterConfiguration(cluster).Deployment.HeartbeatInterval;
                }

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ReportingHeartBeat", ex.ToString());

                throw;
            }
            finally
            {
                if (_rwLock.IsWriterLockHeld)
                    _rwLock.ReleaseWriterLock();
            }

            return interval;
        }

        public DateTime? GetLastHeartBeat(string cluster, string shard, ServerNode node)
        {
            DateTime? heartBeatTime = _heartbeatreporting.GetHeartBeat(cluster, shard, node);
            //if (!heartBeatTime.HasValue)
            //_heartbeatreporting.AddToReport(cluster, shard, node);

            return heartBeatTime;
        }

        public void RemoveFromHeartBeat(string cluster, string shard, ServerInfo node)
        {
            _heartbeatreporting.RemoveFromReport(cluster, shard, GetServerNodeFromServerInfo(node, 0));
        }

        public void CreateIndex(string cluster, string database, string collection, IndexConfiguration configuration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                transaction = ConfigurationStore.BeginTransaction(cluster);
                CreateIndex(transaction, cluster, database, collection, configuration);

                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (Exception e)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
                throw;
            }
        }

        private void CreateIndex(ConfigurationStore.Transaction transaction, string cluster, string database, string collection, IndexConfiguration configuration)
        {
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                IndexConfiguration.ValidateConfiguration(configuration);
                AssignObjectUID(configuration);
                ClusterConfiguration existingConfiguraton = transaction.GetClusterConfiguration(cluster);
                if (existingConfiguraton != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguraton.Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;
                        if (colConfs.ContainsCollection(collection))
                        {
                            CollectionConfiguration selectedCollection = colConfs.GetCollection(collection);

                            if (selectedCollection.Indices == null)
                            {
                                selectedCollection.Indices = new Indices();
                            }

                            if (selectedCollection.Indices.IndexConfigurations == null)
                                selectedCollection.Indices.IndexConfigurations = new Dictionary<string, IndexConfiguration>();

                            Indices indices = selectedCollection.Indices;

                            if (!indices.ContainsIndex(configuration.IndexName))
                            {
                                indices.AddIndex(configuration);

                                DeploymentConfiguration deploymentConfs = existingConfiguraton.Deployment;
                                foreach (ShardConfiguration shard in deploymentConfs.Shards.Values)
                                {
                                    foreach (ServerNode server in shard.Servers.Nodes.Values)
                                    {
                                        try
                                        {
                                            RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                            dbMgtSession.CreateIndex(cluster, shard.Name, database, collection, configuration);
                                        }
                                        catch (Exception e)
                                        {
                                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                                LoggerManager.Instance.CONDBLogger.Error("CreateIndex", "An error occured while creating Index '" + configuration.IndexName + "' on " + server.Name + ". Databaase is " + database + ". Collection is " + collection + ". " + e.ToString());
                                        }
                                    }
                                }

                            }
                            else
                            {
                                throw new Exception("Index with name: " + configuration.IndexName + " already exists");
                            }
                        }
                        else
                        {
                            throw new Exception("Collection with name: " + collection + " does not exist");
                        }
                    }
                    else
                    {
                        throw new Exception("Database with name: " + database + " does not exist");
                    }

                    ArrayList parameter = new ArrayList() { cluster, database, collection };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.CreateIndex, 1, null, null);
                    transaction.InsertOrUpdateClusterConfiguration(existingConfiguraton);
                }
                else
                    throw new Exception("Given cluster does not exist");
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("CreateIndex", ex.ToString());
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        public List<string> GetUserDefinedFunctions(string cluster, string database)
        {
            var functionsList = new List<string>();
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                transaction = ConfigurationStore.BeginTransaction(cluster);

                ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);
                if (existingConfiguration != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        DatabaseConfiguration dbConf = dbConfs.GetDatabase(database);
                        ConfigurationStore.CommitTransaction(transaction);
                    }
                    else
                    {
                        throw new Exception(string.Format("Specified database '{0}' does not exist", database));
                    }

                }
                else
                    throw new Exception("Given cluster does not exist");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetUserDefinedFunction", ex.ToString());
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }

            return functionsList;
        }
        
        public static T[] RemoveAt<T>(T[] source, int index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public void DropIndex(string cluster, string database, string collection, string indexName)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                transaction = ConfigurationStore.BeginTransaction(cluster);
                ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);
                if (existingConfiguration != null)
                {
                    DatabaseConfigurations dbConfs = existingConfiguration.Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;
                        if (colConfs.ContainsCollection(collection))
                        {
                            CollectionConfiguration selectedCol = colConfs.GetCollection(collection);
                            if (selectedCol.Indices == null || selectedCol.Indices.IndexConfigurations == null)
                            {
                                selectedCol.Indices = new Indices();
                            }
                            Indices indices = selectedCol.Indices;

                            if (indices.ContainsIndex(indexName))
                            {
                                indices.RemoveIndex(indexName);

                                transaction.InsertOrUpdateClusterConfiguration(existingConfiguration);
                            }
                            else
                            {
                                throw new Exception(string.Format("Specified index '{0}' does not exists in collection '{1}' database '{2}'", indexName, collection, database));
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Specified collection '{0}' does not exists in database '{1}'", collection, database));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Specified database '{0}' does not exist", database));
                    }

                    ArrayList parameter = new ArrayList() { cluster, database, collection, indexName };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.DropIndex, 1, null, null);

                    DeploymentConfiguration deploymentConf = existingConfiguration.Deployment;
                    foreach (ShardConfiguration shard in deploymentConf.Shards.Values)
                    {
                        foreach (ServerNode server in shard.Servers.Nodes.Values)
                        {
                            try
                            {
                                RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                                dbMgtSession.DropIndex(cluster, shard.Name, database, collection, indexName);
                            }
                            catch (Exception e)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("DropIndex", "An error occured while dropping Index '" + collection + "' from server " + server.Name + ". " + e.ToString());
                            }
                        }
                    }
                }
                else
                    throw new Exception("Given cluster does not exist");

                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("DropIndex", ex.ToString());

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        //public void ReportLastOperationTime(long operationId, string cluster, string shard, ServerInfo serverInfo)
        //{
        //    try
        //    {
        //        _rwLock.AcquireWriterLock(Timeout.Infinite);
        //        //only updating meta data
        //        List<ServerInfo> serverNodes = null;
        //        ClusterConfiguration oldConfiguration = null;
        //        ClusterInfo oldClusterInfo = null;
        //        if (cluster != null && _configurationStore.ContainsCluster(cluster.ToLower()))
        //        {
        //            
        //            
        //            List<ShardInfo> shardInfo = _configurationStore.GetClusterInfo(cluster).ShardInfo.ToList<ShardInfo>();
        //            foreach (ShardInfo info in shardInfo)
        //            {
        //                if (info.Name.ToLower().Equals(shard))
        //                {
        //                    if (info.RunningNodes != null)
        //                    {
        //                        serverNodes = info.RunningNodes.ToList<ServerInfo>();
        //                        foreach (ServerInfo sInfo in serverNodes)
        //                        {
        //                            if (sInfo.Address.Equals(serverInfo.Address))
        //                            {
        //                                sInfo.LastOpLogTime = operationId;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            _configurationStore.InsertOrUpdateClusterConfiguration(_configurationStore.GetClusterConfiguration(cluster));
        //            ArrayList parameter = new ArrayList() { operationId, cluster, shard, serverInfo };
        //            SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.ReportLastOperationTime, 1, oldConfiguration, oldClusterInfo);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
        //            LoggerManager.Instance.CONDBLogger.Error("ReportLastOperationTime", ex.ToString());
        //        throw;
        //    }
        //    finally
        //    {
        //        _rwLock.ReleaseWriterLock();
        //        // SaveConfiguration();
        //    }
        //}

        public void UpdateCollectionStatistics(string cluster, string database, string collection, CollectionStatistics statistics)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;
                //only updating meta data
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    //meta-data
                    ClusterInfo info = transaction.GetClusterInfo(cluster);
                    DatabaseInfo databaseInfo = info.GetDatabase(database);
                    CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);

                    if (collectionInfo != null)
                    {
                        collectionInfo.Statistics = statistics;
                        transaction.InsertOrUpdateClusterInfo(info);
                    }

                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                    ArrayList parameter = new ArrayList() { cluster, database, collection, statistics };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.UpdateCollectionStatistics, 1, oldConfiguration, oldClusterInfo);

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateCollectionStatistics", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                // SaveConfiguration();
            }
        }

        public void UpdateBucketStatistics(string cluster, string database, string collection, Common.Stats.ShardInfo shardInfo)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                //only updating meta data
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    //meta-data
                    ClusterInfo info = transaction.GetClusterInfo(cluster);
                    if (info == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("UpdateBucketStatistics", "Cluster info not found. cluster = " + cluster);
                        return;
                    }
                    DatabaseInfo databaseInfo = info.GetDatabase(database);
                    if (databaseInfo == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("UpdateBucketStatistics", "Database not found. databaseID = " + database);
                        return;
                    }
                    CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);
                    if (collectionInfo != null)
                    {
                        collectionInfo.DistributionStrategy.UpdateBucketStats(shardInfo);
                    }
                    else
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                            LoggerManager.Instance.CONDBLogger.Debug("UpdateBucketStatistics", "Collection not found. databaseID = " + database + " collectionId = " + collection);
                    }
                    transaction.InsertOrUpdateBucketStats(cluster, database, collection, shardInfo.Statistics.LocalBuckets);
                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateBucketStatistics", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //  SaveConfiguration();
            }
        }

        internal void ReplicateClusterConfiguration(ClusterConfiguration configuration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                if (configuration != null && configuration.Name != null)
                {
                    transaction = ConfigurationStore.BeginTransaction(configuration.Name);
                    transaction.InsertOrUpdateClusterConfiguration(configuration);
                    ConfigurationStore.CommitTransaction(transaction);
                }

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ReplicateClusterConfiguration", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        internal void ReplicateMetaInfo(ClusterInfo clusterInfo)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                if (clusterInfo != null && clusterInfo.Name != null)
                {
                    transaction = ConfigurationStore.BeginTransaction(clusterInfo.Name);
                    transaction.InsertOrUpdateClusterInfo(clusterInfo);
                    ConfigurationStore.CommitTransaction(transaction);
                }

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ReplicateMetaInfo", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //SaveConfiguration();
            }
        }

        private void SendMessageToReplica(string clusterName, ArrayList parameters, string methodName, int overload, ClusterConfiguration configuration, ClusterInfo clusterInfo)//ClusterConfiguration configuration, ClusterInfo info, ReplicationType type)
        {

        }

        private void RollBackOperation(string clusterName, ClusterConfiguration configuration, ClusterInfo clusterInfo)
        {
            try
            {
                //if (_configurationStore.ContainsCluster(clusterName))
                //    _clusterConfiguration[clusterName] = configuration;

                //_distributionMetadataStore.AddClusterInfo(clusterName, clusterInfo);
            }
            catch (System.Exception ex)
            {

            }
        }

     

        private static T Clone<T>(T source)
        {

            byte[] data = Alachisoft.NosDB.Serialization.Formatters.CompactBinaryFormatter.ToByteBuffer(source, String.Empty);
            return (T)Alachisoft.NosDB.Serialization.Formatters.CompactBinaryFormatter.FromByteBuffer(data, string.Empty);

        }

        #region Recovery Operations
        public Common.Recovery.RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config)
        {
            //Submits info held by configserver required for recovery operations
            Dictionary<string, object> additionalParams = new Dictionary<string, object>();
            CsBackupableEntities entity = new CsBackupableEntities();
            try
            {
                switch (config.JobType)
                {
                    case RecoveryJobType.ConfigBackup:
                    case RecoveryJobType.FullBackup:

                        #region Backup
                        //populate information needed for configuration

                        Dictionary<string, Dictionary<string, IDistributionStrategy>> distributionMap = new Dictionary<string, Dictionary<string, IDistributionStrategy>>();

                        if (config.DatabaseMap.Keys.Count > 0)
                        {
                            foreach (string db in config.DatabaseMap.Keys)
                            {
                                entity.ShardList = GetDatabaseClusterConfiguration(config.Cluster).Deployment.Shards.Keys.ToList();
                                DatabaseConfiguration dbConfig = GetDatabaseClusterConfiguration(config.Cluster).Databases.GetDatabase(db);
                                if (dbConfig != null)
                                    entity.Database.Add(dbConfig.Name, dbConfig);
                                else
                                {
                                    RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
                                    status.Message = "No configuration exists against the given database";
                                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                        LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.SubmitRecoveryJob()", status.Message);

                                    return status;
                                }

                                // backup security info 
                                ResourceId resId = new ResourceId();
                                resId.Name = dbConfig.Name;
                                resId.ResourceType = Common.Security.Impl.Enums.ResourceType.Database;
                                entity.SecurityResource = SecurityManager.GetResource(MiscUtil.CONFIGURATION_SHARD_NAME, config.Cluster, resId) as ResourceItem;

                                List<string> collections = null;
                                if (dbConfig.Storage.Collections != null)
                                {
                                    collections = dbConfig.Storage.Collections.Configuration.Keys.ToList();
                                }
                                else
                                {
                                    collections = new List<string>();
                                }
                                Dictionary<string, IDistributionStrategy> collectionMap = new Dictionary<string, IDistributionStrategy>();
                                foreach (string collection in collections)
                                {
                                    IDistributionStrategy strategy = GetDistriubtionStrategy(config.Cluster, db, collection);
                                    collectionMap.Add(collection, strategy);
                                }
                                distributionMap.Add(db, collectionMap);
                            }
                        }
                        else
                        {
                            foreach (DatabaseConfiguration dbConfig in GetDatabaseClusterConfiguration(config.Cluster).Databases.Configurations.Values)
                            {
                                if (dbConfig != null)
                                    entity.Database.Add(dbConfig.Name, dbConfig);
                                else
                                {
                                    RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
                                    status.Message = "No configuration exists against the given database";
                                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                        LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.SubmitRecoveryJob()", status.Message);

                                    return status;
                                }
                                List<string> collections = null;
                                if (dbConfig.Storage.Collections != null)
                                {
                                    collections = dbConfig.Storage.Collections.Configuration.Keys.ToList();
                                }
                                else
                                {
                                    collections = new List<string>();
                                }
                                Dictionary<string, IDistributionStrategy> collectionMap = new Dictionary<string, IDistributionStrategy>();
                                foreach (string collection in collections)
                                {
                                    IDistributionStrategy strategy = GetDistriubtionStrategy(config.Cluster, dbConfig.Name, collection);
                                    collectionMap.Add(collection, strategy);
                                }
                                distributionMap.Add(dbConfig.Name, collectionMap);
                            }
                        }

                        entity.DistributionStrategyMap = distributionMap;
                        break;
                        #endregion
                    case RecoveryJobType.DataRestore:
                    case RecoveryJobType.DataBackup:

                        foreach (DatabaseConfiguration dbConfig in GetDatabaseClusterConfiguration(config.Cluster).Databases.Configurations.Values)
                        {

                            if (dbConfig != null)
                                entity.Database.Add(dbConfig.Name, dbConfig);
                            else
                            {
                                RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
                                status.Message = "No configuration exists against the given database";
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.SubmitRecoveryJob()", status.Message);

                                return status;
                            }
                        }
                        break;
                    case RecoveryJobType.ConfigRestore:
                    case RecoveryJobType.Restore:

                        entity.ShardList = GetDatabaseClusterConfiguration(config.Cluster).Deployment.Shards.Keys.ToList();
                        foreach (DatabaseConfiguration dbConfig in GetDatabaseClusterConfiguration(config.Cluster).Databases.Configurations.Values)
                        {
                            if (dbConfig != null)
                                entity.Database.Add(dbConfig.Name, dbConfig);
                            else
                            {
                                RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
                                status.Message = "No configuration exists against the given database";
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.SubmitRecoveryJob()", status.Message);

                                return status;
                            }
                        }
                        break;
                    case RecoveryJobType.Export:
                        break;
                    case RecoveryJobType.Import:
                        break;
                }

                return _recoveryManager.SubmitRecoveryJob(config, entity);
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.SubmitRecoveryJob()", exp.ToString());
                RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
                state.Message = exp.Message;
                return state;
            }
        }
                
        public Common.Recovery.RecoveryOperationStatus CancelRecoveryJob(string identifier)
        {
            return _recoveryManager.CancelRecoveryJob(identifier);
        }

        public Common.Recovery.RecoveryOperationStatus[] CancelAllRecoveryJobs()
        {
            return _recoveryManager.CancelAllRecoveryJobs();
        }

        public Common.Recovery.ClusteredRecoveryJobState GetJobState(string identifier)
        {
            return _recoveryManager.GetJobState(identifier);
        }

        public ClusterJobInfoObject[] GetAllRunningJobs()
        {
            return _recoveryManager.GetAllRunningJobs();
        }

       

        public void RecoveryOperationExecting(string cluster,string database)
        {
            bool allowed = _recoveryManager.IsOperationAllowed(cluster, database);
            if (!allowed)
                throw new InvalidOperationException("A recovery operation is under execution against " + database);
        }

        public RecoveryConfiguration GetJobConfiguration(string identifier)
        {
            return _recoveryManager.GetJobConfiguration(identifier);
        }

        #region IRecovery Communication Handler
        RecoveryOperationStatus IRecoveryCommunicationHandler.SubmitRecoveryJob(string node, string cluster, string shard, RecoveryOperation opContext)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";

            try
            {
                RemoteManagementSession dbMgtRemote = GetManagementSession(node);
                state = dbMgtRemote.SubmitDataRecoveryJob(cluster, shard, opContext);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigServer.SubmitRecoveryJob()", ex.ToString());
                state.Message = ex.ToString();
            }

            return state;
        }

        // broadcast call to all nodes in shard
        RecoveryOperationStatus IRecoveryCommunicationHandler.SubmitRecoveryJob(string cluster, string shard, RecoveryOperation opContext)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";

            try
            {
                ShardInfo shardInfo = GetDatabaseClusterInfo(cluster).GetShardInfo(shard);

                foreach (Address node in shardInfo.RunningNodes.Keys)
                {
                    RemoteManagementSession dbMgtRemote = GetManagementSession(node.ip);
                    state = dbMgtRemote.SubmitDataRecoveryJob(cluster, shard, opContext);
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigServer.SubmitRecoveryJob()", ex.ToString());
            }

            return state;
        }
        #endregion

        public void SubmitRecoveryState(object state)
        {
            _recoveryManager.SubmitRecoveryState(state);
        }


        #region Restore helper methods

        internal ClusterConfiguration GetClusterConfiguration(string cluster)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                foreach (ClusterConfiguration config in ConfigurationStore.GetAllClusterConfiguration())
                {
                    if (config!=null && config.Name.Equals(cluster))
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_rwLock.IsReaderLockHeld)
                    _rwLock.ReleaseReaderLock();
            }
            return null;
        }

        public void Restore(CsBackupableEntities entity, Dictionary<string, string> databases, string cluster)
        {
            bool allowOperation = false;
            //M_TODO: node mapping or different nodes but same configurations
            //M_TODO: redistribution for diufferent cinbfig
            try
            {
                //
                //
                //NOTE: local cluster notification and naming will lead to an issue fix that
                //
                //
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.Restore()", "Entering");

                // ensure if all nodes are connected
                ClusterInfo deploymentConf = GetDatabaseClusterInfo(cluster);
                foreach (ShardInfo shard in deploymentConf.ShardInfo.Values.ToList())
                {
                    foreach (Address ip in shard.RunningNodes.Keys)
                    {
                        try
                        {
                            RemoteManagementSession dbMgtSession = GetManagementSession(ip.ip);
                            allowOperation = true;

                        }
                        catch (Exception exp)
                        {
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                LoggerManager.Instance.RecoveryLogger.Error("ConfigServer.Restore()", "Unable to create a connection with " + ip.ip);

                            // HACK: send op failed status in case of exception
                            RecoveryOperationStatus failStatus = new RecoveryOperationStatus(RecoveryStatus.Failure);
                            failStatus.Message = "Unable to create a connection with " + shard.Primary.Address.ip;
                            KeyValuePair<string, string> dbMap = databases.First();
                            failStatus.JobIdentifier = dbMap.Key + "_" + dbMap.Value;
                            allowOperation = false;
                            System.Threading.Tasks.Task.Factory.StartNew(() => _recoveryManager.SubmitConfigChanged(failStatus));
                        }
                    }
                }
                if (GetDatabaseClusterConfiguration(cluster).Deployment.Shards.Count == entity.ShardList.Count)
                {
                foreach (string shard in entity.ShardList)
                {
                    if (!GetDatabaseClusterConfiguration(cluster).Deployment.ContainsShard(shard))
                    {
                        allowOperation = false;
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                            LoggerManager.Instance.RecoveryLogger.Error("ConfigServer.Restore()", "Given shard name is not valid " + shard);
                        // HACK: send op failed status in case of exception
                        RecoveryOperationStatus failStatus = new RecoveryOperationStatus(RecoveryStatus.Failure);
                        failStatus.Message = "Provided shard is invalid " + shard;
                        KeyValuePair<string, string> dbMap = databases.First();
                        failStatus.JobIdentifier = dbMap.Key + "_" + dbMap.Value;
                        allowOperation = false;
                        System.Threading.Tasks.Task.Factory.StartNew(() => _recoveryManager.SubmitConfigChanged(failStatus));
                        break;
                    }
                }
                }
                 else
                 {
                     RecoveryOperationStatus failStatus = new RecoveryOperationStatus(RecoveryStatus.Failure);
                     failStatus.Message = "Number of shards while backup is different from current configuration";
                     KeyValuePair<string, string> dbMap = databases.First();
                     failStatus.JobIdentifier = dbMap.Key + "_" + dbMap.Value;
                     allowOperation = false;
                     System.Threading.Tasks.Task.Factory.StartNew(() => _recoveryManager.SubmitConfigChanged(failStatus));

                 }

                if (allowOperation)
                {
                    if (databases.Count > 0)
                    {
                        foreach (string database in databases.Keys)
                        {

                            // restore cluster configurations
                            RestoreClusterConfiguration(cluster, entity.Database, database.ToLower(), databases[database].ToLower());

                            // restore distribution strategy
                            RestoreDistributionStrategy(cluster, entity, database.ToLower(), databases[database].ToLower());

                            // recreate database on shards
                            RestoreDatabaseonShards(cluster, entity, entity.Database, database.ToLower(), databases[database].ToLower());


                            // notify local nodes on new config changed
                            ConfigChangeEventArgs eventArgs = new ConfigChangeEventArgs();
                            eventArgs.SetParamValue(EventParamName.ConfigurationChangeType, ChangeType.ConfigRestored);
                            eventArgs.SetParamValue(EventParamName.ClusterName, cluster);
                            SendNotification(eventArgs);

                            // set security info
                            if (!string.IsNullOrEmpty(databases[database].ToLower()))
                            {
                                entity.SecurityResource.ResourceId.Name = databases[database].ToLower();
                            }
                            SecurityManager.SetResource(MiscUtil.CONFIGURATION_SHARD_NAME, cluster, entity.SecurityResource);
                        }
                    }
                    else
                    {
                        foreach (string database in entity.Database.Keys)
                        {
                            // restore cluster configurations
                            RestoreClusterConfiguration(cluster, entity.Database, database.ToLower(), string.Empty);

                            // restore distribution strategy
                            RestoreDistributionStrategy(cluster, entity, database.ToLower(), string.Empty);

                            // recreate database on shards
                            RestoreDatabaseonShards(cluster, entity, entity.Database, database.ToLower(), string.Empty);

                            // set security info
                            if (!string.IsNullOrEmpty(databases[database].ToLower()))
                            {
                                entity.SecurityResource.ResourceId.Name = databases[database].ToLower();
                            }
                            SecurityManager.SetResource(MiscUtil.CONFIGURATION_SHARD_NAME, cluster, entity.SecurityResource);


                            // notify local nodes on new config changed
                            ConfigChangeEventArgs eventArgs = new ConfigChangeEventArgs();
                            eventArgs.SetParamValue(EventParamName.ConfigurationChangeType, ChangeType.ConfigRestored);
                            eventArgs.SetParamValue(EventParamName.ClusterName, cluster);
                            SendNotification(eventArgs);
                        }
                    }
                    //M_Note: think of a better code for this
                    // notify recovery manager on updated config
                    CsBackupableEntities newEntity = new CsBackupableEntities();
                    //newEntity.DbConfig = GetDatabaseClusterConfiguration(cluster);
                    foreach (DatabaseConfiguration dbConfig in GetDatabaseClusterConfiguration(cluster).Databases.Configurations.Values)
                    {
                        KeyValuePair<string, string> dbMap = databases.First();
                        bool valid = false;

                        if (!string.IsNullOrEmpty(dbMap.Value))
                        {
                            if (dbMap.Value.ToLower().Equals(dbConfig.Name))
                            {
                                valid = true;
                            }
                        }
                        else
                        {
                            if (dbMap.Key.ToLower().Equals(dbConfig.Name))
                            {
                                valid = true;
                            }
                        }

                        if (valid)
                            newEntity.Database.Add(dbConfig.Name, dbConfig);
                    }

                    // update recovered config to recovery
                    System.Threading.Tasks.Task.Factory.StartNew(() => _recoveryManager.SubmitConfigChanged(newEntity));
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigServer.Restore()", ex.ToString());

                KeyValuePair<string, string> dbMap = databases.First();
                if (!string.IsNullOrEmpty(dbMap.Value))
                {
                    RollbackRestoredDB(cluster, dbMap.Value);
                }
                else
                {
                    RollbackRestoredDB(cluster, dbMap.Key);
                }

                // HACK: send op failed status in case of exception
                RecoveryOperationStatus failStatus = new RecoveryOperationStatus(RecoveryStatus.Failure);
                failStatus.Message = ex.ToString();

                failStatus.JobIdentifier = dbMap.Key + "_" + dbMap.Value;

                System.Threading.Tasks.Task.Factory.StartNew(() => _recoveryManager.SubmitConfigChanged(failStatus));
            }
            if (allowOperation)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.Restore()", "Complete");
            }
            else
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.Restore()", "Failed");
            }
        }

        private void RestoreDatabaseonShards(string cluster, CsBackupableEntities entity, Dictionary<string, DatabaseConfiguration> restorableConfiguration, string source, string destination)
        {
            bool result;
            DatabaseConfiguration databaseConfiguration = restorableConfiguration[source];

            string dbName = source;
            if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
            {
                dbName = destination.ToLower();
            }

            if (databaseConfiguration.Name.ToLower().Equals(dbName.ToLower()))
            {
                DeploymentConfiguration deploymentConf = GetDatabaseClusterConfiguration(cluster).Deployment;
                foreach (ShardConfiguration shard in deploymentConf.Shards.Values)
                {
                    foreach (ServerNode server in shard.Servers.Nodes.Values)
                    {
                        databaseConfiguration.Name = dbName;
                        IDictionary<string, IDistributionStrategy> collectionStrategy = entity.DistributionStrategyMap[source];

                        RemoteManagementSession dbMgtSession = GetManagementSession(server.Name);
                        result = dbMgtSession.CreateDatabase(cluster, shard.Name, databaseConfiguration, collectionStrategy);

                        if (result == false)
                        {
                            throw new Exception("Failed to create database " + databaseConfiguration.Name + " on shard " + shard.Name);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Restores distribution strategy for all participating shards
        /// </summary>
        /// <param name="entity"></param>
        private void RestoreDistributionStrategy(string cluster, CsBackupableEntities entity, string source, string destination)
        {
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDistribution()", "Entering");

            foreach (DatabaseConfiguration dbConfig in entity.Database.Values)
            {
                string dbName = dbConfig.Name;
                if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                {
                    dbName = destination.ToLower();
                }

                if (dbConfig.Name.ToLower().Equals(dbName))
                {
                    List<string> collections = dbConfig.Storage.Collections.Configuration.Keys.ToList();

                    foreach (string collection in collections)
                    {
                        IDistributionStrategy strategy = entity.DistributionStrategyMap.Where(x => x.Key.ToLower().Equals(source)).FirstOrDefault().
                                                                                                Value.Where(y => y.Key.ToLower().Equals(collection.ToLower())).First().Value;
                        // entity.DistributionStrategyMap.

                        ClusterInfo cluserInfo = ConfigurationStore.GetClusterInfo(cluster);

                        if (cluserInfo != null)
                        {
                            ConfigurationStore.SetDistributionStrategy(cluster, dbName, collection, strategy);
                        }

                    }
                }
            }
            //
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDistribution()", "Complete");
        }

        private void RestoreClusterConfiguration(string clusterName, Dictionary<string, DatabaseConfiguration> restorableConfiguration, string source, string destination)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreConfig()", "Entering");

                ClusterConfiguration originalConfiguration = ConfigurationStore.GetClusterConfiguration(clusterName);
                if (originalConfiguration == null)
                    throw new Exception("The given cluster does not exist.");

                ClusterInfo oldClusterInfo = null;

                ClusterInfo clusterInfo = null;

                clusterInfo = GetDatabaseClusterInfo(clusterName);
                //// rename database
                //if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                //    clusterInfo.GetDatabase(source).Name = destination;
                // get database info 
                DatabaseInfo[] dbInfo = null;

                // hack because the value is already set to the name of new database in case of differential with a new name

                dbInfo = GetSourceDatabaseInfo(restorableConfiguration, source);

                if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                    dbInfo[0].Name = destination;

                clusterInfo.UpdateDatabase(dbInfo[0]);

                // add database configuration to original config
                DatabaseConfiguration restorableDB = restorableConfiguration[source];

                if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                {
                    restorableDB.Name = destination;

                }

                originalConfiguration.Databases.UpdateDatabase(restorableDB.Name, restorableDB);

                // AddServersToCluster(ref originalConfiguration); // bug 9264 fix. no need to add server to cluster
                transaction = ConfigurationStore.BeginTransaction(clusterName);
                transaction.InsertOrUpdateClusterConfiguration(originalConfiguration);

                if (originalConfiguration.Deployment != null)
                {
                    StartHeartbeatChekcTask(originalConfiguration);
                }

                //adding in meta-data
                transaction.InsertOrUpdateClusterInfo(clusterInfo);

                ArrayList parameter = new ArrayList() { clusterName, restorableConfiguration };
                SendMessageToReplica(clusterName, parameter, ConfigurationCommandUtil.MethodName.CreateCluster, 1, originalConfiguration, oldClusterInfo);
                ConfigurationStore.CommitTransaction(transaction);
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RestoreCluster", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;

            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                // SaveConfiguration();
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreConfig()", "Complete");
            }
        }

        /// <summary>
        /// returns DatabaseInfo from a given configuration
        /// </summary>
        /// <param name="restorableConfiguration"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private DatabaseInfo[] GetSourceDatabaseInfo(Dictionary<string, DatabaseConfiguration> restorableConfiguration, string source)
        {
            DatabaseInfo[] Databases = null;
            try
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDatabaseInfo()", "Entering");

                Databases = new DatabaseInfo[restorableConfiguration.Count];// restorableconfiguration.sioze
                int index = 0;

                DatabaseConfiguration databaseConfiguration = restorableConfiguration[source];


                if (databaseConfiguration.Name.ToLower().Equals(source.ToLower()))
                {
                    Databases[index] = new DatabaseInfo();
                    Databases[index].Name = databaseConfiguration.Name;
                    if (databaseConfiguration.Storage.Collections.Configuration != null)
                    {
                        int collectionCount = 0;
                        foreach (CollectionConfiguration cc in databaseConfiguration.Storage.Collections.Configuration.Values)
                        {
                            CollectionInfo colInfo = new CollectionInfo();
                            colInfo.Name = cc.CollectionName;
                            colInfo.ParitionKey = GetPartitionKey(cc);

                            Databases[index].AddCollection(colInfo);
                            collectionCount++;
                        }
                    }
                    index++;
                }

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDatabaseInfo()", "Complete");

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigServer.RestoreDatabaseInfo()", ex.ToString());

                throw;

            }
            return Databases;
        }

        private void RollbackRestoredDB(string cluster, string db)
        {
            // delete database from config server
            // delete database from shards
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RollbackRestoredDB()", "Dropping " + db + " due to failure in restoration");

            if (GetDatabaseClusterConfiguration(cluster).Databases.ContainsDatabase(db))
                DropDatabase(cluster, db, true, true);

        }
        #endregion

        #region RestoreDifferential
      
        private void UpdateNotifyDB(CsBackupableEntities entity, string cluster, KeyValuePair<string, string> dbToRestore, string sourceDBName)
        {
            // restore cluster configurations
            RestoreClusterConfiguration(cluster, entity.Database, dbToRestore.Key, dbToRestore.Value);

            // restore distribution strategy
            RestoreDistributionStrategy(cluster, entity, dbToRestore.Key, dbToRestore.Value);

            // set security info
            if (!string.IsNullOrEmpty(dbToRestore.Value.ToLower()))
            {
                entity.SecurityResource.ResourceId.Name = dbToRestore.Value.ToLower();
            }
            SecurityManager.SetResource(MiscUtil.CONFIGURATION_SHARD_NAME, cluster, entity.SecurityResource);


            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDifferential()", "Starting update dbConfig " + sourceDBName);

            //Update Database configuration
            DatabaseConfiguration dbConfig = entity.Database[sourceDBName];

            if (!string.IsNullOrEmpty(dbToRestore.Value))
            {
                dbConfig.Name = dbToRestore.Value;
            }

            UpdateDatabaseConfiguration(cluster, dbConfig.Name, dbConfig);

            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDifferential()", "Complete update dbConfig " + sourceDBName);
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ConfigServer.RestoreDifferential()", "Notifying shards ");
            // notify shard to resync database
            // notify local nodes on new config changed
            ConfigChangeEventArgs eventArgs = new ConfigChangeEventArgs();
            eventArgs.SetParamValue(EventParamName.ConfigurationChangeType, ChangeType.ResyncDatabase);
            eventArgs.SetParamValue(EventParamName.ClusterName, cluster);
            eventArgs.SetParamValue(EventParamName.DatabaseName, dbConfig.Name);
            SendNotification(eventArgs);
        }
        #endregion


        #endregion

        #region IShardListener Members

        public object OnMessageReceived(Message message, Server source)
        {
            if (message.Payload is ReplicationArgs)
            {
                return _localSesssion.MessageReceived(message.Payload as ReplicationArgs);
            }
            return null;
        }

        public void OnMemberJoined(Server server)
        {

        }

        public void OnMemberLeft(Server server)
        {
           
          
        }
        #endregion

        internal object StateTransferOperation(String clusterName, IStateTransferOperation operation)
        {
            switch (operation.OpCode)
            {
                case StateTransferOpCode.LockBucketsOnCS:
                    {
                        ArrayList bucketIds = operation.Params.GetParamValue(ParamName.BucketList) as ArrayList;
                        NodeIdentity finalShard = operation.Params.GetParamValue(ParamName.BucketFinalShard) as NodeIdentity;
                        String dbName = operation.TaskIdentity.DBName;
                        String colName = operation.TaskIdentity.ColName;

                        return LockCollectionBuckets(clusterName, dbName, colName, bucketIds, finalShard);
                    }

                case StateTransferOpCode.ReleaseBucketsOnCS:
                    {
                        ArrayList bucketIds = operation.Params.GetParamValue(ParamName.BucketList) as ArrayList;
                        NodeIdentity finalShard = operation.Params.GetParamValue(ParamName.BucketFinalShard) as NodeIdentity;
                        String dbName = operation.TaskIdentity.DBName;
                        String colName = operation.TaskIdentity.ColName;

                        ReleaseCollectionBuckets(clusterName, dbName, colName, bucketIds, finalShard);
                    }
                    break;
                case StateTransferOpCode.AnnounceBucketTxfer:
                    return AnnounceBucketTxfer();

                case StateTransferOpCode.IsSparsedBucket:
                    {
                        String dbName = operation.TaskIdentity.DBName;
                        String colName = operation.TaskIdentity.ColName;
                        int bucketid = (int)operation.Params.GetParamValue(ParamName.BucketID);
                        long threshold = (long)operation.Params.GetParamValue(ParamName.Threshold);
                        return IsSparsedBucket(clusterName, dbName, colName, bucketid, threshold);
                    }
                case StateTransferOpCode.VerifyFinalOwnerShip:
                    {
                        String dbName = operation.TaskIdentity.DBName;
                        String colName = operation.TaskIdentity.ColName;
                        int bucketid = (int)operation.Params.GetParamValue(ParamName.BucketID);
                        NodeIdentity shard = (NodeIdentity)operation.Params.GetParamValue(ParamName.BucketFinalShard);
                        return VerifyFinalOwnerShip(clusterName, dbName, colName, bucketid, shard);
                    }
                case StateTransferOpCode.FinalizeStateTransfer:
                    {
                        FinalizeStateTransfer(clusterName, operation.TaskIdentity.NodeInfo.ShardName, operation.TaskIdentity.DBName, operation.TaskIdentity.ColName);
                    }
                    break;
            }

            return null;
        }


        private void FinalizeStateTransfer(String cluster, String finalizingShard, String database, String collection)
        {
            //StateTransferCompleted(cluster, finalizingShard, database, collection);

            if (_gracefulRemovalMonitoring != null)
            {
                _gracefulRemovalMonitoring.CheckRemovalStatus();
            }
        }
 
        /* Pro Does not need this
        /// <summary>
        /// Inform Collection Distribution about state transfer completion to update its status        
        /// </summary>
       
        private void StateTransferCompleted(String cluster, String completedBy, String database, String collection)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);

                //only updating meta data
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = _configurationStore.BeginTransaction(cluster, true, false);
                    //meta-data
                    ClusterInfo info = transaction.GetClusterInfo(cluster);
                    if (info == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("StateTransferCompleted", "Cluster info not found. cluster = " + cluster);

                        return;
                    }
                    DatabaseInfo databaseInfo = info.GetDatabase(database);
                    if (databaseInfo == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("StateTransferCompleted", "Database not found. databaseID = " + database);

                        return;
                    }

                    CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);
                    if (collectionInfo != null)
                    {
                        collectionInfo.DistributionStrategy.StateTransferCompleted(completedBy);
                    }
                    else
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                            LoggerManager.Instance.CONDBLogger.Debug("StateTransferCompleted", "Collection not found. databaseID = " + database + " collectionId = " + collection);
                    }
                    transaction.InsertOrUpdateDistributionStrategy(transaction.GetClusterInfo(cluster), database, collection);
                    _configurationStore.CommitTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("StateTransferCompleted", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }
        */

        private Boolean VerifyFinalOwnerShip(string ClusterName, string DBName, string ColName, int bucketid, NodeIdentity finalShard)
        {
            //IDistributionStrategy strategy = ConfigurationStore.GetDistributionStrategy(ClusterName, DBName, ColName);

            //if (strategy != null)
            //{
            //    return strategy.VerifyFinalOwnerShip(bucketid, finalShard.ShardName);
            //}

            return false;
        }

        private Boolean IsSparsedBucket(string clusterName, string dbName, string colName, int bucketid, long threshold)
        {
            //IDistributionStrategy strategy = ConfigurationStore.GetDistributionStrategy(clusterName, dbName, colName);

            //if (strategy != null)
            //{
            //    strategy.IsSparsedBucket(bucketid, threshold);
            //}

            return false;
        }

        private object AnnounceBucketTxfer()
        {
            // Currently no need for this information during state transfer

            return null;
        }

        private Hashtable LockCollectionBuckets(String cluster, String database, String collection, ArrayList bucketIds, NodeIdentity finalShard)
        {

            ConfigurationStore.Transaction transaction = null;
            Hashtable result = new Hashtable();
            DateTime start = DateTime.Now;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                //  _rwLock.AcquireReaderLock(Timeout.Infinite);

                //only updating meta data
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {

                    transaction = _configurationStore.BeginTransaction(cluster, true, false);

                    //meta-data
                    ClusterInfo info = _configurationStore.GetClusterInfo(cluster);//transaction.GetClusterInfo(cluster);
                    if (info == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("LockCollectionBuckets", "Cluster info not found. cluster = " + cluster);
                        return new Hashtable();
                    }
                    DatabaseInfo databaseInfo = info.GetDatabase(database);
                    if (databaseInfo == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("LockCollectionBuckets", "Database not found. databaseID = " + database);
                        return new Hashtable();
                    }

                    CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);
                    if (collectionInfo != null)
                    {
                        result = collectionInfo.DistributionStrategy.LockBuckets(bucketIds, finalShard.ShardName);
                    }
                    else
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                            LoggerManager.Instance.CONDBLogger.Debug("LockCollectionBuckets", "Collection not found. databaseID = " + database + " collectionId = " + collection);
                    }

                    //transaction.UpdateBucketStatus(cluster, database, collection, (ArrayList)result[BucketLockResult.LockAcquired], BucketStatus.UnderStateTxfr);

                    transaction.InsertOrUpdateDistributionStrategy(info, database, collection);


                    //transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));

                    //if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                    //    LoggerManager.Instance.CONDBLogger.Debug("LockCollectionBuckets.Detail", transaction.GetTransactionInfo());

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("LockCollectionBuckets", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //_rwLock.ReleaseReaderLock();
                TimeSpan t = DateTime.Now - start;

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                    LoggerManager.Instance.CONDBLogger.Debug("LockCollectionBuckets.Total -> Time", t.TotalMilliseconds.ToString());
            }

            return result;
        }

        /// <summary>
        /// Releases a bucket by setting its status again to functional. Only 
        /// node who has set its status to state trxfr can change its status.
        /// </summary>
        /// <param name="buckets"></param>
        /// <param name="node"></param>
        private void ReleaseCollectionBuckets(String cluster, String database, String collection, ArrayList bucketIds, NodeIdentity finalShard)
        {
            DateTime start = DateTime.Now;
            TimeSpan releaseTime = TimeSpan.MinValue;
            ConfigurationStore.Transaction transaction = null;

            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                // _rwLock.AcquireReaderLock(Timeout.Infinite);

                //only updating meta data
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = _configurationStore.BeginTransaction(cluster, true, false);
                    //meta-data
                    ClusterInfo info = _configurationStore.GetClusterInfo(cluster); //transaction.GetClusterInfo(cluster);
                    if (info == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("ReleaseCollectionBuckets", "Cluster info not found. cluster = " + cluster);

                        return;
                    }
                    DatabaseInfo databaseInfo = info.GetDatabase(database);
                    if (databaseInfo == null)
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsWarnEnabled)
                            LoggerManager.Instance.CONDBLogger.Warn("ReleaseCollectionBuckets", "Database not found. databaseID = " + database);

                        return;
                    }

                    CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);
                    if (collectionInfo != null)
                    {
                        DateTime startTime = DateTime.Now;

                        collectionInfo.DistributionStrategy.ReleaseBuckets(bucketIds, finalShard.ShardName);

                        releaseTime = DateTime.Now - startTime;

                    }
                    else
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                            LoggerManager.Instance.CONDBLogger.Debug("ReleaseCollectionBuckets", "Collection not found. databaseID = " + database + " collectionId = " + collection);
                    }
                    //transaction.UpdateBucketStatus(cluster, database, collection, bucketIds, BucketStatus.Functional, finalShard.ShardName);

                    transaction.InsertOrUpdateDistributionStrategy(info, database, collection);

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ReleaseCollectionBuckets", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
                //_rwLock.ReleaseReaderLock();

                TimeSpan t = DateTime.Now - start;
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                {
                    LoggerManager.Instance.CONDBLogger.Debug("ReleaseCollectionBuckets.Total -> Time", t.TotalMilliseconds.ToString());
                    LoggerManager.Instance.CONDBLogger.Debug("ReleaseCollectionBuckets.Rlease -> Time", releaseTime.TotalMilliseconds.ToString());

                }
            }
        }


        private void AssignShardToCollection(CollectionInfo collection, CollectionConfiguration cc, ClusterInfo clusterInfo, ShardConfiguration[] shards)
        {
            try
            {
                collection.CollectionShard = cc.Shard.Equals("All") ? GetFirstShard(clusterInfo) /*clusterInfo.GetShardWithLowestAmountOfData()*/ : cc.Shard;
            }
            catch (Exception)
            {
                collection.CollectionShard = shards.OrderBy(shard => shard.Name).First().Name;
            }
            cc.Shard = collection.CollectionShard;
        }

        internal bool StartNode(string cluster, string shard, ServerNode server)
        {
            if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
            {
                DeploymentConfiguration deploymentConf = ConfigurationStore.GetClusterConfiguration(cluster).Deployment;
                if (deploymentConf.ContainsShard(shard))
                {
                    //List<ServerNode> nodeList = shardList.Find(p => p.Name.Equals(shard)).Servers.Nodes.ToList<ServerNode>();
                    if (deploymentConf.GetShard(shard).Servers.ContainsNode(server.Name))
                    {
                        int shardPort = deploymentConf.GetShard(shard).Port;
                        try
                        {
                            RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);

                            bool running = dbMgtRemote.StartNode(cluster, shard, shardPort);
                            //if (running)
                            //{
                            //    ReportingNodeJoining(cluster, shard, server);
                            //    AddNodeToRunningList(cluster, shard, server, false);
                            //}
                            return running;
                        }
                        catch (System.Exception ex)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("DMS StartNode", ex.ToString());
                            throw ex;
                        }
                    }
                    else
                    {
                        Exception ex = new Exception("Specified server is not part of shard.");
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                            LoggerManager.Instance.CONDBLogger.Error("DMS StartNode", ex.ToString());
                        throw ex;

                    }

                }
                else
                {
                    Exception ex = new Exception("Specified shard doesn't exist.");
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("DMS StartNode", ex.ToString());
                    throw ex;

                }
            }
            return false;
        }


        internal bool StartShard(string cluster, string shard)
        {
            List<bool> startedNodes = new List<bool>();
            if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
            {
                DeploymentConfiguration deploymentConf = (ConfigurationStore.GetClusterConfiguration(cluster)).Deployment;
                if (deploymentConf.ContainsShard(shard))
                {
                    int shardPort = deploymentConf.GetShard(shard).Port;
                    foreach (ServerNode server in deploymentConf.GetShard(shard).Servers.Nodes.Values)
                    {
                        try
                        {
                            RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);
                            bool running = dbMgtRemote.StartNode(cluster, shard, shardPort);
                            //if (running)
                            //{
                            //    ReportingNodeJoining(cluster, shard, server);
                            //    AddNodeToRunningList(cluster, shard, server, false);
                            //}
                            startedNodes.Add(running);
                        }
                        catch (System.Exception ex)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("DMS StartShard", ex.ToString());
                            throw ex;
                        }
                    }
                }
                else
                    throw new System.Exception("Specified shard does not exist.");
            }
            if (startedNodes.Contains(true))
                return true;
            else
                return false;
        }

        internal void StartCluster(string cluster)
        {
            if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
            {
                DeploymentConfiguration deploymentConf = ConfigurationStore.GetClusterConfiguration(cluster).Deployment;
                foreach (ShardConfiguration shardconfig in deploymentConf.Shards.Values)
                {
                    string shard = shardconfig.Name;
                    int shardPort = deploymentConf.GetShard(shard).Port;
                    foreach (ServerNode server in deploymentConf.GetShard(shard).Servers.Nodes.Values)
                    {
                        try
                        {
                            RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);
                            dbMgtRemote.StartNode(cluster, shard, shardPort);
                            //if (dbMgtRemote.StartNode(cluster, shard, shardPort))
                            //{
                            //    ReportingNodeJoining(cluster, shard, server);
                            //    AddNodeToRunningList(cluster, shard, server, false);
                            //}
                        }
                        catch (System.Exception ex)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("DMS StartNode", ex.ToString());
                        }
                    }
                }
            }
        }

        internal bool ShardExists(string cluster, string shard)
        {
            ConfigurationStore.Transaction transaction = ConfigurationStore.BeginTransaction(cluster, false);

            ClusterConfiguration config = transaction.GetClusterConfiguration(cluster);
            if (config == null)
                return false;

            if (config.Deployment != null && config.Deployment.GetShard(shard) != null)
            {
                return true;
            }

            return false;

        }

        internal bool StopShard(string cluster, string shard)
        {
            try
            {
                //_rwLock.AcquireReaderLock(Timeout.Infinite);
                List<bool> stoppedNodes = new List<bool>();
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    DeploymentConfiguration deploymentConf = ConfigurationStore.GetClusterConfiguration(cluster).Deployment;
                    if (deploymentConf.ContainsShard(shard))
                    {
                        foreach (ServerNode server in deploymentConf.GetShard(shard).Servers.Nodes.Values.ToList())
                        {
                            try
                            {
                                RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);
                                bool stopped = dbMgtRemote.StopNode(cluster, shard);
                                if (stopped)
                                    RemoveNodeFromMembership(cluster, shard, server);
                                stoppedNodes.Add(stopped);
                            }
                            catch (System.Exception ex)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("DMS StopNode", ex.ToString());
                            }
                            finally
                            {
                                DisposeManagementSession(server.Name);
                            }
                        }
                    }
                }
                if (stoppedNodes.Contains(false))
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                throw e;
            }
            //finally
            //{
            //    _rwLock.ReleaseReaderLock();
            //}
        }

        internal bool StopNode(string cluster, string shard, ServerNode server)
        {
            if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
            {
                DeploymentConfiguration depoloymentConf = ConfigurationStore.GetClusterConfiguration(cluster).Deployment;
                if (depoloymentConf.ContainsShard(shard))
                {
                    //List<ServerNode> nodeList = shardList.Find(p => p.Name.Equals(shard)).Servers.Nodes.ToList<ServerNode>();
                    if (depoloymentConf.GetShard(shard).Servers.ContainsNode(server.Name))
                    {

                        int shardPort = depoloymentConf.GetShard(shard).Port;
                        try
                        {
                            RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);

                            bool stopped = dbMgtRemote.StopNode(cluster, shard);
                            if (stopped)
                            {
                                RemoveNodeFromMembership(cluster, shard, server);
                            }
                            return stopped;
                        }
                        catch (System.Exception ex)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("DMS StopNode", ex.ToString());
                            throw;
                        }
                        finally
                        {
                            DisposeManagementSession(server.Name);
                        }
                    }
                }
            }
            return false;
        }

        internal void StopCluster(string cluster)
        {
            if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
            {
                DeploymentConfiguration deploymentConf = ConfigurationStore.GetClusterConfiguration(cluster).Deployment;
                //List<ShardConfiguration> shardList = .Shards.ToList<ShardConfiguration>();
                foreach (ShardConfiguration shardconfig in deploymentConf.Shards.Values)
                {
                    string shard = shardconfig.Name;
                    foreach (ServerNode server in deploymentConf.GetShard(shard).Servers.Nodes.Values)
                    {
                        try
                        {
                            RemoteManagementSession dbMgtRemote = GetManagementSession(server.Name);
                            bool stopped = dbMgtRemote.StopNode(cluster, shard);
                            if (stopped)
                                RemoveNodeFromMembership(cluster, shard, server);
                        }
                        catch (System.Exception ex)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("DMS Stop", ex.ToString());
                        }
                        finally
                        {
                            DisposeManagementSession(server.Name);
                        }
                    }
                }
                StopHeartbeatChekcTask(ConfigurationStore.GetClusterConfiguration(cluster));
            }
        }

        internal void DisposeManagementSession(string peerIP)
        {
            if (_dbMgtSessions.ContainsKey(peerIP))
            {
                try
                {
                    ((RemoteManagementSession)_dbMgtSessions[peerIP]).Dispose();
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("DMS DisposeManagementSession", ex.ToString());
                }
                _dbMgtSessions.Remove(peerIP);
            }

        }

        internal RemoteManagementSession GetManagementSession(string peerIP)
        {
            try
            {
                if (!_dbMgtSessions.ContainsKey(peerIP))
                {

                    int dbMgtPort = ConfigurationSettings<CSHostSettings>.Current.ManagementServerPort >= 1024 ? ConfigurationSettings<CSHostSettings>.Current.ManagementServerPort : NetworkUtil.DEFAULT_DB_HOST_PORT;

                    string csIP = ConfigurationSettings<CSHostSettings>.Current.IP.ToString();
                    RemoteManagementSession dbMgtRemote = new RemoteManagementSession();

                    dbMgtRemote.Connect(peerIP, dbMgtPort /*, csIP*/);

                    dbMgtRemote.MarkConfigurationSession();

                    IServerAuthenticationCredential serverAuthenticationCredenital = dbMgtRemote.Authenticate(new SSPIClientAuthenticationCredential());
                    dbMgtRemote.Channel.IsAuthenticated = serverAuthenticationCredenital.IsAuthenticated;
                    if (dbMgtRemote.Channel.IsAuthenticated)
                    {
                        dbMgtRemote.SessionId = serverAuthenticationCredenital.SessionId;
                    }
                    _dbMgtSessions[peerIP] = dbMgtRemote;

                }
                return (RemoteManagementSession)_dbMgtSessions[peerIP];
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetManagementSession", ex.ToString());
                throw ex;

            }
        }

      

        internal Membership GetNodeMemberShip(string clusterName, string shardName, string node)
        {
            if (_nodeMembership.Contains(clusterName + shardName + node))
                return _nodeMembership[clusterName + shardName + node] as Membership;

            return null;
        }

        private void AddNodeToRunningList(string cluster, string shard, ServerNode node, bool isPrimary)
        {
            ConfigurationStore.Transaction transaction = null;

            try
            {
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                //only updating meta data
                List<ServerInfo> serverNodes = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    DeploymentConfiguration deploymentConf = transaction.GetClusterConfiguration(cluster).Deployment;
                    int port = deploymentConf.GetShard(shard).Port;

                    ClusterInfo clusterInfo = transaction.GetClusterInfo(cluster);

                    foreach (ShardInfo info in clusterInfo.ShardInfo.Values)
                    {
                        if (info.Name.ToLower().Equals(shard))
                        {
                            if (info.ConfigureNodes != null)
                            {
                                //serverNodes = info.ConfigureNodes.ToList<ServerInfo>();
                                foreach (ServerInfo sInfo in info.ConfigureNodes.Values)
                                {
                                    if (sInfo.Address.Equals(new Address(node.Name, port)))
                                    {
                                        List<ServerInfo> runningNodes = info.RunningNodes != null ? info.RunningNodes.Values.ToList<ServerInfo>() : new List<ServerInfo>();
                                        sInfo.Status = Status.Running;
                                        if (!runningNodes.Contains(sInfo))
                                        {
                                            runningNodes.Add(sInfo);
                                            if (isPrimary) info.Primary = sInfo;
                                        }
                                        //todo: refactor this sh*t
                                        info.RunningNodes = runningNodes.ToDictionary(x => x.Address, x => x);
                                    }
                                }
                            }
                        }

                    }
                    Membership memberShip = _membershipMetadatastore.GetMemberShip(cluster, shard);

                    if (memberShip == null)
                    {
                        memberShip = new Membership();
                        memberShip.Cluster = cluster;
                        memberShip.Shard = shard;
                        memberShip.Servers = new List<ServerNode>();
                        _membershipMetadatastore.AddMembership(cluster, shard, memberShip);
                    }

                    if (memberShip.Servers == null)
                        memberShip.Servers = new List<ServerNode>();

                    if (memberShip.Servers != null && !memberShip.Servers.Contains(node))
                        _membershipMetadatastore.AddNodeToMemberList(cluster, shard, node);

                    if (isPrimary)
                        _membershipMetadatastore.SetPrimary(cluster, shard, node);


                    transaction.InsertOrUpdateClusterInfo(clusterInfo);
                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                    ArrayList parameter = new ArrayList() { cluster, shard, node };
                    SendMessageToReplica(cluster, parameter, ConfigurationCommandUtil.MethodName.ReportNodeJoining, 1, oldConfiguration, oldClusterInfo);

                    ConfigurationStore.CommitTransaction(transaction);

                    ConfigChangeEventArgs args = new ConfigChangeEventArgs(cluster, ChangeType.NodeJoined);
                    args.SetParamValue(EventParamName.ShardName, shard);
                    args.SetParamValue(EventParamName.Membership, _membershipMetadatastore.GetMemberShip(cluster, shard));
                    //args.ShardName = shard;
                    //args.Membership = _membershipMetadatastore.GetMemberShip(cluster, shard);

                    Thread sendNotification = new Thread(() => SendNotification(args));
                    sendNotification.Name = "AddNodeSendNotificationThread";
                    sendNotification.Start();

                }

            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("AddNodeToRunningList", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                // SaveConfiguration();
            }

        }

        internal string[] ListDatabases(string cluster)
        {
            try
            {
                List<string> databases = new List<string>();
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    DatabaseConfigurations dbConfs = ConfigurationStore.GetClusterConfiguration(cluster).Databases;
                    foreach (DatabaseConfiguration config in dbConfs.Configurations.Values)
                    {
                        databases.Add(config.Name);
                    }
                }
                return databases.ToArray();
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ListDatabases", ex.ToString());
                throw;
            }
        }

        internal string[] ListCollections(string cluster, string database)
        {
            try
            {
                List<string> collections = new List<string>();
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    DatabaseConfigurations dbConfs = ConfigurationStore.GetClusterConfiguration(cluster).Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;
                        foreach (CollectionConfiguration config in colConfs.Configuration.Values)
                        {
                            collections.Add(config.CollectionName);
                        }
                    }
                }
                return collections.ToArray();
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ListCollections", ex.ToString());
                throw;
            }
        }

        internal string[] ListIndices(string cluster, string database, string collection)
        {
            try
            {
                List<string> indicesList = new List<string>();
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    DatabaseConfigurations dbConfs = ConfigurationStore.GetClusterConfiguration(cluster).Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;
                        if (colConfs.ContainsCollection(collection))
                        {
                            Indices indices = colConfs.GetCollection(collection).Indices;
                            foreach (IndexConfiguration config in indices.IndexConfigurations.Values)
                            {
                                indicesList.Add(config.IndexName);
                                //string index = config.IndexName +"\t\t\t";
                                ////indicesList.Add(config.IndexName + "\t\t\t" + config.Attributes.ToString());
                                //foreach (IndexAttribute attribute in config.Attributes)
                                //{
                                //    index = attribute.ToString();
                                //}
                                //indicesList.Add(index);
                            }
                        }
                    }
                }
                return indicesList.ToArray();
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ListIndices", ex.ToString());
                throw;
            }
        }

        #region Security Related Methods
        public IUser GetUserInformation(string username)
        {
            throw new NotImplementedException();
        }

        public bool GrantRoleOnDatabaseServer(string clusterName, ResourceId resourceId, IUser userInfo, IRole roleInfo)
        {
            bool isSuccessful = false;
            if (ConfigurationStore.ContainsCluster(clusterName))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(clusterName);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                    {
                        RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                        isSuccessful = rmtMgtSession.GrantRole(clusterName, shard.Name, resourceId, userInfo, roleInfo);
                    }
                }
            }
            return isSuccessful;
        }

        public bool RevokeRoleOnDatabaseServer(string clusterName, ResourceId resourceId, IUser userInfo, IRole roleInfo)
        {
            bool isSuccessful = false;
            if (ConfigurationStore.ContainsCluster(clusterName))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(clusterName);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                    {
                        RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                        isSuccessful = rmtMgtSession.RevokeRole(clusterName, shard.Name, resourceId, userInfo, roleInfo);
                    }
                }
            }
            return isSuccessful;
        }

        public bool GrantRole(string clusterName, ResourceId resourceId, string userName, string roleName)
        {
            return SecurityManager.Grant(MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, userName, roleName, clusterName);
        }

        public bool RevokeRole(string clusterName, ResourceId resourceId, string userName, string roleName)
        {
            return SecurityManager.Revoke(MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, userName, roleName, clusterName);
        }

        public void PopulateSecurityInformationOnDBServer(string clusterName, IList<IResourceItem> resources, string shardName = null, ServerNode server = null)
        {
            if (ConfigurationStore.ContainsCluster(clusterName))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(clusterName);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    if (string.IsNullOrEmpty(shardName) || (shardName != null && shard.Name.Equals(shardName)))
                    {
                        foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                        {
                            if (server == null || (server != null && server.ToString().Equals(serverNode.ToString())))
                            {
                                RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                                rmtMgtSession.PopulateSecurityInformationOnDBServer(clusterName, shard.Name, resources);
                            }
                        }
                    }
                }
            }
        }

        public bool CreateUser(IUser userInfo)
        {
            return SecurityManager.CreateUser(MiscUtil.CONFIGURATION_SHARD_NAME, userInfo);
        }

        public bool DropUser(IUser userInfo)
        {
            return SecurityManager.DropUser(MiscUtil.CONFIGURATION_SHARD_NAME, userInfo);
        }

        public bool CreateUserOnDBServer(IUser userInfo)
        {
            bool isSuccessful = false;
            if (ConfigurationStore.ContainsCluster(MiscUtil.CLUSTERED))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(MiscUtil.CLUSTERED);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                    {
                        RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                        isSuccessful = rmtMgtSession.CreateUser(MiscUtil.CLUSTERED, shard.Name, userInfo) || isSuccessful;
                    }
                }
            }
            if (ConfigurationStore.ContainsCluster(MiscUtil.LOCAL))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(MiscUtil.LOCAL);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                    {
                        RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                        isSuccessful = rmtMgtSession.CreateUser(MiscUtil.LOCAL, shard.Name, userInfo) || isSuccessful;
                    }
                }
            }
            return isSuccessful;
        }

        public bool DropUserOnDBServer(IUser userInfo)
        {
            bool isSuccessful = false;
            if (ConfigurationStore.ContainsCluster(MiscUtil.CLUSTERED))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(MiscUtil.CLUSTERED);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                    {
                        RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                        isSuccessful = rmtMgtSession.DropUser(MiscUtil.CLUSTERED, shard.Name, userInfo) || isSuccessful;
                    }
                }
            }
            if (ConfigurationStore.ContainsCluster(MiscUtil.LOCAL))
            {
                ClusterConfiguration clusterConfiguration = ConfigurationStore.GetClusterConfiguration(MiscUtil.LOCAL);

                foreach (ShardConfiguration shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (ServerNode serverNode in shard.Servers.Nodes.Values)
                    {
                        RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Name);
                        isSuccessful = rmtMgtSession.DropUser(MiscUtil.LOCAL, shard.Name, userInfo) || isSuccessful;
                    }
                }
            }
            return isSuccessful;
        }

        public void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId, string username)
        {
            foreach (ClusterInfo cluster in ConfigurationStore.GetAllClusterInfo())
            {
                //TODO: : talk to sir taimoor
                //on removing configmanager it is emptying cluster configuration but not db cluster,
                if (cluster != null)
                {
                    foreach (ShardInfo shard in cluster.ShardInfo.Values)
                    {
                        foreach (ServerInfo serverNode in shard.RunningNodes.Values)
                        {
                            RemoteManagementSession rmtMgtSession = GetManagementSession(serverNode.Address.ip);
                            rmtMgtSession.PublishAuthenticatedUserInfoToDBServer(cluster.Name, shard.Name, sessionId, username);
                        }
                    }
                }
            }
        }

        #endregion


        public void UpdateDatabaseConfiguration(string cluster, string database, DatabaseConfiguration databaseConfiguration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;


                if (cluster != null && cluster.ToLower().Equals(MiscUtil.LOCAL))
                {
                    if (!ConfigurationStore.ContainsCluster(MiscUtil.LOCAL))
                    {
                        transaction = ConfigurationStore.BeginTransaction(cluster);
                        transaction.InsertOrUpdateClusterConfiguration(GetLocalDatabaseConfiguration());
                    }
                }

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    if (transaction == null)
                        transaction = ConfigurationStore.BeginTransaction(cluster);

                    DatabaseConfigurations dbConfs = transaction.GetClusterConfiguration(cluster).Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        DatabaseConfiguration dConfig = dbConfs.GetDatabase(database);
                        if (dConfig != null)
                        {
                            if (databaseConfiguration.Storage != null)
                            {
                                if (databaseConfiguration.Storage.StorageProvider != null)
                                {
                                    dConfig.Storage.StorageProvider.MaxFileSize = databaseConfiguration.Storage.StorageProvider.MaxFileSize;
                                    dConfig.Storage.StorageProvider.IsMultiFileStore = databaseConfiguration.Storage.StorageProvider.IsMultiFileStore;
                                }
                            }
                            

                            if (dConfig.Storage != null && dConfig.Storage.StorageProvider != null)
                            {
                                if (dConfig.Storage.StorageProvider.StorageProviderType == ProviderType.LMDB)
                                {
                                    dConfig.Storage.StorageProvider.LMDBProvider.MaxReaders = databaseConfiguration.Storage.StorageProvider.LMDBProvider.MaxReaders;
                                    dConfig.Storage.StorageProvider.LMDBProvider.MaxCollections = databaseConfiguration.Storage.StorageProvider.LMDBProvider.MaxCollections;
                                }
                            }

                            if (dConfig.Storage != null && dConfig.Storage.CacheConfiguration != null)
                            {
                                dConfig.Storage.CacheConfiguration.CacheSpace = databaseConfiguration.Storage.CacheConfiguration.CacheSpace;
                            }
                            // umer todo might need this line not sure :s
                            //_configurationStore.GetClusterConfiguration(cluster).Databases.Configurations = databaseList.ToArray();
                        }
                    }
                    else
                        throw new Exception(string.Format("Specified database '{0}' does not exist.", database));

                    transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                }
                else
                    throw new Exception(string.Format("Specified cluster {0} does not exist.", cluster));

                if (transaction != null)
                    ConfigurationStore.CommitTransaction(transaction);
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateDatabase", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void UpdateServerPriority(string cluster, string shard, ServerNode server, int priority)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);

                    ClusterConfiguration existingConfiguration = transaction.GetClusterConfiguration(cluster);

                    DeploymentConfiguration deploymentConf = existingConfiguration.Deployment;
                    if (deploymentConf.ContainsShard(shard))
                    {
                        //List<ServerNode> nodeList = shardList.Find(p => p.Name.ToLower().Equals(shard.ToLower())).Servers.Nodes.ToList<ServerNode>();
                        if (deploymentConf.GetShard(shard).Servers.ContainsNode(server.Name))
                        {
                            deploymentConf.GetShard(shard).Servers.GetNode(server.Name).Priority = priority;

                            transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));

                        }
                    }

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateServerPriority", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

        }

        public void UpdateDeploymentConfiguration(string cluster, int heartBeatInterval)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    DeploymentConfiguration deploymentConfig = transaction.GetClusterConfiguration(cluster).Deployment;
                    if (deploymentConfig != null)
                    {
                        deploymentConfig.HeartbeatInterval = heartBeatInterval;
                        transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                    }
                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateHeartBeat", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

        }

        public void UpdateIndexAttribute(string cluster, string database, string collection, string index, IndexAttribute attribute)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;
                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    DatabaseConfigurations dbConfs = transaction.GetClusterConfiguration(cluster).Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;
                        if (colConfs.ContainsCollection(collection))
                        {
                            CollectionConfiguration selectedCollection = colConfs.GetCollection(collection);

                            Indices indices = new Indices();
                            if (selectedCollection.Indices == null)
                            {
                                selectedCollection.Indices = new Indices();
                            }

                            if (selectedCollection.Indices.IndexConfigurations == null)
                                selectedCollection.Indices.IndexConfigurations = new Dictionary<string, IndexConfiguration>();

                            if (selectedCollection.Indices.IndexConfigurations != null)
                                indices = selectedCollection.Indices;

                            if (indices.ContainsIndex(index))
                            {
                                indices.GetIndex(index).Attributes = attribute;

                                transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                            }
                        }
                    }

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateIndexAttribute", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void UpdateShardConfiguration(string cluster, string shard, int heartbeat, int port)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                ClusterConfiguration oldConfiguration = null;
                ClusterInfo oldClusterInfo = null;

                if (cluster != null && ConfigurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    DeploymentConfiguration deploymentConfig = transaction.GetClusterConfiguration(cluster).Deployment;
                    if (deploymentConfig != null)
                    {
                        if (deploymentConfig.Shards != null)
                        {
                            if (deploymentConfig.ContainsShard(shard))
                            {
                                deploymentConfig.GetShard(shard).NodeHeartbeatInterval = heartbeat;
                                deploymentConfig.GetShard(shard).Port = port;

                              


                                transaction.InsertOrUpdateClusterConfiguration(transaction.GetClusterConfiguration(cluster));
                            }
                        }
                    }

                    ClusterInfo oldInfo = transaction.GetClusterInfo(cluster);
                    // Update it in ClusterInfo also
                    Dictionary<string, ShardInfo> shardInfos = oldInfo.ShardInfo;
                    if (shardInfos != null)
                    {
                        if (shardInfos.ContainsKey(shard))
                        {
                            shardInfos[shard].Port = port;
                            Dictionary<Address, ServerInfo> oldOnes = shardInfos[shard].ConfigureNodes.Clone();
                            shardInfos[shard].ConfigureNodes.Clear();
                            foreach (KeyValuePair<Address, ServerInfo> kvp in oldOnes)
                            {
                                ServerInfo sinfo = kvp.Value;
                                sinfo.Address = new Address(kvp.Key.IpAddress, port);
                                shardInfos[shard].AddConfigureNode(sinfo);
                            }

                            transaction.InsertOrUpdateClusterInfo(transaction.GetClusterInfo(cluster));
                        }
                    }

                    ConfigurationStore.CommitTransaction(transaction);
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateShardInfo", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void UpdateCollectionConfiguration(string cluster, string database, string collection, CollectionConfiguration collectionConfiguration)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {

                _rwLock.AcquireWriterLock(Timeout.Infinite);
                if (cluster != null && _configurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = ConfigurationStore.BeginTransaction(cluster);
                    ClusterConfiguration clusterConfiguration = transaction.GetClusterConfiguration(cluster);
                    DatabaseConfigurations dbConfs = clusterConfiguration.Databases;
                    if (dbConfs.ContainsDatabase(database))
                    {
                        CollectionConfigurations colConfs = dbConfs.GetDatabase(database).Storage.Collections;
                        if (colConfs.ContainsCollection(collection))
                        {
                            CollectionConfiguration selectedConfiguration = colConfs.GetCollection(collection);
                            if (collectionConfiguration.EvictionConfiguration != null)
                                selectedConfiguration.EvictionConfiguration.EnabledEviction = collectionConfiguration.EvictionConfiguration.EnabledEviction;
                           
                            transaction.InsertOrUpdateClusterConfiguration(clusterConfiguration);
                        }
                        else
                        {
                            throw new Exception("Collection with name: " + collectionConfiguration.CollectionName + " already exists");
                        }
                    }
                    else
                    {
                        throw new Exception("Database with name: " + database + " does not exist");
                    }
                    _configurationStore.CommitTransaction(transaction);
                }
                else
                    throw new Exception("Given cluster does not exist");
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("UpdateCollection", ex.ToString());

                if (transaction != null)
                    ConfigurationStore.RollbackTranscation(transaction);

                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public IServerAuthenticationCredential AuthenticateClient(IClientAuthenticationCredential credentials)
        {
            throw new NotImplementedException();
        }


        #region Security

        internal void AddSecurityInformation(string shardName, ResourceId resourceId, ResourceId superResourceId, string clusterName, ISessionId sessionId = null)
        {
            IResourceItem resourceItem = new ResourceItem(resourceId);
            SecurityManager.AddResource(shardName, resourceItem, sessionId, superResourceId, clusterName);
        }

        internal void RemoveSecurityInformation(string shardName, ResourceId resourceId, ResourceId superResourceId, string clusterName, ISessionId sessionId = null)
        {
            IResourceItem resourceItem = new ResourceItem(resourceId);
            SecurityManager.RemoveResource(shardName, resourceId, sessionId, superResourceId, clusterName);
        }
        #endregion

        

        //private void CreateDefaultConfigCluster()
        //{

        //    //if (!EditionInfo.IsRemoteClient)
        //    //{

        //    if (!EditionInfo.IsRemoteClient)
        //    {

        //        var repConfig = new ReplicationConfiguration
        //        {
        //            ReplicationBulkSize = 5,
        //            ReplicationTimeInterval = 15,
        //            ReplicationType = "PullModelReplication"
        //        };

        //        var configServer = new ConfigServerConfiguration
        //        {
        //            Port = ConfigurationSettings<CSHostSettings>.Current.Port,
        //            Servers = new ServerNodes(),
        //            Name = MiscUtil.CONFIG_CLUSTER
        //        };
        //        var serverNode = new ServerNode
        //        {
        //            Name = ConfigurationSettings<CSHostSettings>.Current.IP.ToString(),
        //            Priority = 1
        //        };
        //        configServer.Servers.Nodes.Add(serverNode.Name, serverNode);
        //        CreateConfigurationCluster(configServer, 10, repConfig);



        //        //Adding Cluster resource in Security Manager, being done here because of single cluster creation at the time of configuration cluster creation
        //        ResourceId clusterResourceId;
        //        ResourceId clusterSuperResourceId;
        //        Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Cluster,
        //            Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, out clusterResourceId, out clusterSuperResourceId,
        //            Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
        //        AddSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, clusterResourceId,
        //            clusterSuperResourceId, MiscUtil.CLUSTERED, null);

        //        //}

        //    }

        //}

        public List<Address> GetDataBaseServerNode()
        {
            List<Address> serverNodes;
            Address address = MiscUtil.GetDbManagementAddress();
            if (String.IsNullOrEmpty(address.ip)) return new List<Address>();

            RemoteManagementSession dbMgtRemote = GetManagementSession(address.ip);
            try
            {
                serverNodes = dbMgtRemote.GetDatabaseServerNodes();
                //ClusterInfo clusterInfo = GetDatabaseClusterInfo(MiscUtil.CLUSTERED);

                //if(serverNodes == null)
                //    serverNodes =new List<Address>();

                //if(clusterInfo !=null && clusterInfo.ShardInfo != null)
                //{
                //    foreach(KeyValuePair<string, ShardInfo> pair in clusterInfo.ShardInfo)
                //    {
                //        foreach(Address node in pair.Value.ConfigureNodes.Keys)
                //        {
                //            if (!serverNodes.Contains(node))
                //                serverNodes.Add(node);
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("GetDataBaseServerNode", ex.ToString());
                throw;
            }
            return serverNodes;
        }

        public bool SetDatabaseMode(string cluster, string databaseName, DatabaseMode databaseMode)
        {
            ConfigurationStore.Transaction transaction = null;
            bool isStatusSet = false;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                if (_configurationStore.ContainsCluster(cluster.ToLower()))
                {
                    transaction = _configurationStore.BeginTransaction(cluster);

                    DatabaseConfigurations databaseConfs =
                        transaction.GetClusterConfiguration(cluster).Databases;
                    if (databaseConfs.ContainsDatabase(databaseName))
                    {
                        databaseConfs.GetDatabase(databaseName).Mode = databaseMode;
                    }

                    //meta-data
                    ClusterInfo info = transaction.GetClusterInfo(cluster);
                    if (info != null)
                    {
                        DatabaseInfo dinfo = info.GetDatabase(databaseName);
                        dinfo.Mode = databaseMode;
                    }
                    var exceptionMessage = new List<string>();
                    DeploymentConfiguration deploymentConfs =
                        transaction.GetClusterConfiguration(cluster).Deployment;
                    foreach (ShardConfiguration shard in deploymentConfs.Shards.Values)
                    {
                        foreach (var node in shard.Servers.Nodes)
                        {
                            #region call to DBManagementHost

                            RemoteManagementSession dbMgtRemote = null;
                            try
                            {
                                dbMgtRemote = GetManagementSession(node.Key);
                                dbMgtRemote.SetDatabaseMode(cluster, shard.Name, databaseName, databaseMode);
                            }
                            catch (Exception ex)
                            {
                                exceptionMessage.Add(ex.Message + " Node IP [" + node.Value.Name + "]");
                            }
                            finally
                            {
                                if (dbMgtRemote != null)
                                    dbMgtRemote.Dispose();
                            }

                            #endregion
                        }
                    }

                    string responseMsg = "";
                    foreach (string msg in exceptionMessage)
                    {
                        responseMsg = responseMsg + msg + "\n";
                    }
                    if (!String.IsNullOrEmpty(responseMsg))
                        throw new Exception(responseMsg);


                    var parameter = new ArrayList() { databaseMode, cluster, databaseName };
                    SendMessageToReplica(MiscUtil.CLUSTERED, parameter, ConfigurationCommandUtil.MethodName.SetDatabaseMode, 1,
                        null, null);
                    _configurationStore.CommitTransaction(transaction);
                    isStatusSet = true;

                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("SetNodeStatus", ex.ToString());

                if (transaction != null)
                    _configurationStore.RollbackTranscation(transaction);
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
            return isStatusSet;

        }

        public IDictionary<string, byte[]> GetImplementation(string implIdentifier)
        {
            IDictionary<string, byte[]> assemblyArr = new Dictionary<string, byte[]>();
            string path = (Path.Combine(Path.Combine(Path.Combine(AppUtil.InstallDir, "database"), "deployment"), implIdentifier));
            if (!Directory.Exists(path)) throw new Exception("Deployment id not exist: '" + implIdentifier + "'");
            var di = new DirectoryInfo(path);
            foreach (FileInfo f in di.GetFiles("*"))
            {
                if (!Path.HasExtension(f.FullName)) continue;
                var fs = new FileStream(f.FullName, FileMode.Open, FileAccess.Read);
                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Flush();
                fs.Close();
                assemblyArr[f.Name] = bytes;
            }
            return assemblyArr;
        }


        

     

        internal object GetState()
        {
            return _configurationStore.GetState();
        }

      

        internal void ApplyState(object state)
        {
            _configurationStore.ApplyState(state as Alachisoft.NosDB.Core.Configuration.Services.ConfigurationStore.DatabaseCluster);
        }

        public void DeployAssemblies(string cluster, string deploymentId, string deploymentName, string assemblyFileName, Byte[] bytes)
        {
            ConfigurationStore.Transaction transaction = null;
            try
            {


                //string shard = shardConfiguration.Name;
                bool submit = false;
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                transaction = ConfigurationStore.BeginTransaction(cluster);
                ClusterConfiguration existingCluster = transaction.GetClusterConfiguration(cluster);
                if (existingCluster != null)
                {
                    DeploymentConfiguration deploymentConf = existingCluster.Deployment;
                    if (deploymentConf == null)
                        deploymentConf = new DeploymentConfiguration();
                    if (!string.IsNullOrEmpty(deploymentId))
                    {
                        if (!deploymentConf.ContainsDeploymentId(deploymentId))
                        {
                            deploymentConf.AddDeploymentId(deploymentId);
                            deploymentConf.AddDeployment(deploymentId, deploymentName);
                            submit = true;
                        }

                    }


                    if (AppUtil.InstallDir != null)
                    {
                        //System.IO.Directory.CreateDirectory(AppUtil.DeployedAssemblyDir);

                        //string[] folderNames = name.Split(new char[] { '$' });
                        //string folderLevel1 = System.IO.Path.Combine(deployedAssembliesFolder, folderNames[0]);
                        //System.IO.Directory.CreateDirectory(folderLevel1.Trim());

                        string folderLevel2 =
                            (System.IO.Path.Combine(
                                System.IO.Path.Combine(System.IO.Path.Combine(AppUtil.InstallDir, "database"),
                                    "deployment"),
                                deploymentId));
                        System.IO.Directory.CreateDirectory(folderLevel2.Trim());


                        FileStream fs = new FileStream(folderLevel2 + "\\" + assemblyFileName, FileMode.Create,
                            FileAccess.Write);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Flush();
                        fs.Close();
                    }
                    if (submit)
                    {
                        transaction.InsertOrUpdateClusterConfiguration(existingCluster);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.CONDBLogger.Error("ConfigurationServer.DeployAssemblies() ", ex);
                }
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (_rwLock.IsWriterLockHeld)
                    _rwLock.ReleaseWriterLock();
            }

        }


        public void MarkDatabaseSesion()
        {
        }


        public void MarkDistributorSession()
        {
        }

        public void MarkConfiguraitonSession()
        {
		}
		
        private void AddNodeToConfigurationServerConfig(string cluster, ServerNode server)
        {
            ConfigServerConfiguration config = GetConfigurationClusterConfiguration(cluster);

            if (config.Servers.Nodes.Count < 2 && !config.Servers.ContainsNode(server.Name))
            {
                config.Servers.AddNode(server);
                SaveConfigServerConfiguration();
            }
        }

        public bool OnPreCommitTransaction(ConfigurationStore.Transaction transaction)
        {
            TimeStat stat = new TimeStat();
            try
            {
                stat.Begin();
                return ReplicateTransaction(transaction);

            }
            finally
            {
                stat.End();
                stat.ReportTime("OnPreCommitTransaction.Replicate");
            }

        }

        public void OnPostCommitTransaction(ConfigurationStore.Transaction transaction)
        {


        }

        private bool ReplicateTransaction(ConfigurationStore.Transaction transaction)
        {
            if (transaction != null)
            {
                if (_cfgCluster != null)
                {
                    return _cfgCluster.ReplicateTransaction(transaction);
                }
            }

            return true;
        }

        internal bool ReplicateTransaction(object transaction)
        {
            try
            {
                _configurationStore.EnlistAndCommitTrnsaction(transaction as ConfigurationStore.Transaction);
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("ConfigurationServer.ReplicateTransaction", e.ToString());

                throw;
            }
            return true;
        }

        public List<Address> GetConfClusterServers(String cluster)
        {
            List<Address> confClusterServers = null;
            ConfigurationStore.Transaction transaction = null;
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);

                transaction = ConfigurationStore.BeginTransaction(MiscUtil.LOCAL, false);
                ClusterConfiguration clusterConf = transaction.GetClusterConfiguration(cluster);

                if(clusterConf==null)
                    transaction.GetClusterConfiguration(MiscUtil.LOCAL);

                if (clusterConf != null)
                {
                    DeploymentConfiguration deploymentConfs = clusterConf.Deployment;
                    foreach (ShardConfiguration shard in deploymentConfs.Shards.Values)
                    {
                        if (shard.Servers.Nodes.Count > 0)
                        {
                            foreach (ServerNode server in shard.Servers.Nodes.Values)
                            {
                                try
                                {
                                    IManagementSession dbMgtSession = GetManagementSession(server.Name);
                                    confClusterServers = dbMgtSession.GetConfClusterServers(cluster);
                                }
                                catch (Exception e)
                                {
                                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                        LoggerManager.Instance.CONDBLogger.Error("GetConfClusterServers", "An error occured while getting configuration servers for cluster " + cluster + " on " + server.Name + ".[ error" + e.ToString() + " ]");
                                }

                                if (confClusterServers != null && confClusterServers.Count > 0) break;
                            }
                        }
                        if (confClusterServers != null && confClusterServers.Count > 0) break;
                    }
                }
                else
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("GetConfClusterServers", "'" + MiscUtil.LOCAL + "' cluster configuration is unavailable");
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }

            return confClusterServers;
        }

        public bool IsNodeRunning(string node)
        {
            int dbMgtPort = ConfigurationSettings<CSHostSettings>.Current.ManagementServerPort >= 1024 ? ConfigurationSettings<CSHostSettings>.Current.ManagementServerPort : NetworkUtil.DEFAULT_DB_HOST_PORT;

            string csIP = ConfigurationSettings<CSHostSettings>.Current.IP.ToString();
            RemoteManagementSession dbMgtRemote = new RemoteManagementSession();

            dbMgtRemote.Connect(node, dbMgtPort/*, csIP*/);

            dbMgtRemote.MarkConfigurationSession();

            IServerAuthenticationCredential serverAuthenticationCredenital = dbMgtRemote.Authenticate(new SSPIClientAuthenticationCredential());
            dbMgtRemote.Channel.IsAuthenticated = serverAuthenticationCredenital.IsAuthenticated;
            if (dbMgtRemote.Channel.IsAuthenticated)
            {
                dbMgtRemote.SessionId = serverAuthenticationCredenital.SessionId;
            }
            dbMgtRemote.Dispose();
            return true;
        }
    }

    class TimeStat
    {
        DateTime start;

        TimeSpan diff;

        public void Begin()
        {
            start = DateTime.Now;
        }
        public void End()
        {
            diff = DateTime.Now - start;
        }
        public void ReportTime(String method)
        {
            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsDebugEnabled)
                LoggerManager.Instance.CONDBLogger.Debug("ConfigurationStore", "time spent in method (" + method + ") . time taken(ms) " + diff.TotalMilliseconds);
        }
    }
}
