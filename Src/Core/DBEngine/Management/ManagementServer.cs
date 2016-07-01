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
using System.Collections;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.Threading;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Core.Security.Impl;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Impl;
using System.IO;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common;
namespace Alachisoft.NosDB.Core.DBEngine.Management
{
    public class ManagementServer
    {
        #region Security
        internal void AddSecurityInformation(string cluster, string shardName, ResourceId resourceId, ResourceId superResourceId, ISessionId sessionId = null)
        {
            IResourceItem resourceItem = new ResourceItem(resourceId);
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = shardName };
            if (_shards.ContainsKey(shardIdentity))
                _shards[shardIdentity].SecurityManager.AddResource(shardName, resourceItem, sessionId, superResourceId);
        }

        internal void RemoveSecurityInformation(string cluster, string shardName, ResourceId resourceId, ResourceId superResourceId, ISessionId sessionId = null)
        {
            IResourceItem resourceItem = new ResourceItem(resourceId);
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = shardName };
            if (_shards.ContainsKey(shardIdentity))
                _shards[shardIdentity].SecurityManager.RemoveResource(shardName, resourceId, sessionId, superResourceId);
        }

        private ISecurityManager SecurityManager { set; get; }
        #endregion



        internal static DbmNodeconfiguration s_DbmNodeConfiguration = null;
        private string _fileName = "";
        private ReaderWriterLock _rwLock = new ReaderWriterLock();
        private List<DualChannel> _connectedConfigServers;
        private Dictionary<ShardIdentity, ShardHost> _shards = new Dictionary<ShardIdentity, ShardHost>();
        private int _dbmPort;

        private ManagementSession _managementSession;

        public IShardServer ManagementShardServer { get; set; }

        public Dictionary<ShardIdentity, ShardHost> Shards
        {
            get
            {
                return _shards;
            }
        }

        public ManagementServer()
        {

            _fileName = ConfigurationSettings.AppSettings["ServerConfigfile"];

            ManagementShardServer = new ManagementShardServer();
            _connectedConfigServers = new List<DualChannel>();

        }

        public void Start()
        {
            try
            {
                //security manager is initialized when spn is registered
                SecurityManager = new SecurityManager();
                SecurityManager.Initialize("ManagementServer");
                SecurityManager.InitializeSecurityInformation("ManagementServer");
                //EditionInfo.ReadEditionID();
                LoadConfiguration();

                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Info("ManagementServer.Initialize()", "Load Configuration successfully.");
                }
                if (s_DbmNodeConfiguration != null)
                {
                    if (s_DbmNodeConfiguration.DbmClusters != null)
                    {
                        SynchronizeWithConfigManager();
                    }
                }

                ManagementShardServer.Start();
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Info("ManagementServer.Initialize()", "Management Shard Server is Started");
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Error("ManagementServer.Start()", "Error:", ex);
                }
                throw;
            }

            //_dbNode.Start();
        }

        public void Stop()
        {
            foreach (var shards in _shards.Values)
            {
                shards.Stop(false);
            }
            ManagementShardServer.Stop();
        }


        public ManagementSession OpenManagementSession(UserCredentials credentials)
        {
            return new ManagementSession(this, credentials);
        }

        public void Initialize(IPAddress ip, int port, ISessionListener listener)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = "DBService", DatabaseName = "" });
            ManagementShardServer.Initialize(ip, port);
            if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.ServerLogger.Info("Management Server.Initialize()", "Management Shard Server initialize with ManagementPort: " + port + " and IP Address: " + ip);
            }

            ManagementShardServer.RegisterSessionListener(SessionTypes.Management, listener);

        }

        private void LoadConfiguration()
        {
            try
            {
                ConfigurationBuilder builder = new ConfigurationBuilder(_fileName);
                builder.RegisterRootConfigurationObject(typeof(Alachisoft.NosDB.Core.Configuration.DbmNodeconfiguration));
                builder.ReadConfiguration();
                if (builder.Configuration.Length > 1)
                    throw new System.Exception("Invalid DBM node configuration");
                if (builder.Configuration.Length == 0)
                {
                    string localIp = ConfigurationSettings<DBHostSettings>.Current.IP.ToString();
                    int dbmPort = ConfigurationSettings<DBHostSettings>.Current.Port;
                    DbmNodeconfiguration dbmNodeConfig = new DbmNodeconfiguration() { IP = localIp, Port = dbmPort };
                    _dbmPort = dbmNodeConfig.Port;
                    s_DbmNodeConfiguration = dbmNodeConfig;

                    SaveConfiguration();
                }
                else
                {
                    DbmNodeconfiguration[] nodeconfig = new DbmNodeconfiguration[builder.Configuration.Length];
                    builder.Configuration.CopyTo(nodeconfig, 0);
                    if (nodeconfig.Length == 1)
                    {
                        DbmNodeconfiguration nc = nodeconfig.FirstOrDefault();
                        if (nc != null && nc.IP != null)
                        {
                            _dbmPort = nc.Port;
                            s_DbmNodeConfiguration = nc;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {

                    LoggerManager.Instance.ServerLogger.Error("ManagementServer.LoadConfiguration()", "Error", ex);

                }
                throw;
            }
        }

        #region Database Management

        internal bool CreateIndex(string cluster, string shard, string database, string collection, IndexConfiguration configuration)
        {
            try
            {
                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }
                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();

                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];
                        {
                            if (_node.IsRunning)
                            {
                                ICreateIndexOperation cIO = new CreateIndexOperation();

                                cIO.Database = database;
                                cIO.Collection = collection;
                                cIO.Configuration = configuration;
                                return _node.NodeContext.TopologyImpl.CreateIndex(cIO).IsSuccessfull;
                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        internal bool DropIndex(string cluster, string shard, string database, string collection, string indexName)
        {
            try
            {
                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }
                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();

                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];
                        {
                            if (_node.IsRunning)
                            {
                                IDropIndexOperation dIO = new DropIndexOperation();

                                dIO.Database = database;
                                dIO.Collection = collection;
                                dIO.IndexName = indexName;
                                return _node.NodeContext.TopologyImpl.DropIndex(dIO).IsSuccessfull;
                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal bool DropCollection(string cluster, string shard, string database, string collection)
        {
            try
            {
                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }
                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();

                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];
                        {
                            if (_node.IsRunning)
                            {
                                IDropCollectionOperation dCO = new DropCollectionOperation();

                                dCO.Database = database;
                                dCO.Collection = collection;
                                return _node.NodeContext.TopologyImpl.DropCollection(dCO).IsSuccessfull;
                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal bool CreateCollection(string cluster, string shard, string database, Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration configuration)
        {
            try
            {
                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }
                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();
                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];
                        {
                            if (_node.IsRunning)
                            {
                                ICreateCollectionOperation cCO = new CreateCollectionOperation();
                                cCO.Configuration = configuration;
                                cCO.Database = database;
                                return _node.NodeContext.TopologyImpl.CreateCollection(cCO).IsSuccessfull;
                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal bool CreateCollection(string cluster, string shard, string database, Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration configuration, IDistributionStrategy distribution)
        {
            try
            {
                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }
                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();
                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];
                        {
                            if (_node.IsRunning)
                            {
                                ICreateCollectionOperation cCO = new CreateCollectionOperation();
                                cCO.Configuration = configuration;
                                cCO.Database = database;
                                cCO.Distribution = distribution;
                                return _node.NodeContext.TopologyImpl.CreateCollection(cCO).IsSuccessfull;

                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal bool DropDatabase(string cluster, string shard, string database, bool dropFiles)
        {
            try
            {
                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }
                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();
                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];
                        {
                            if (_node.IsRunning)
                            {
                                return _node.NodeContext.TopologyImpl.DropDatabase(database, dropFiles);
                            }
                        }
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal bool CreateDatabase(string cluster, string shard, DatabaseConfiguration configuration, IDictionary<string, IDistributionStrategy> collectionStrategy)
        {
            try
            {

                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = null;
                if (clusters.Exists(x => x.Name.Equals(cluster)))
                {
                    clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
                }

                if (clusterConfig != null)
                {
                    List<DbmShard> shardList = clusterConfig.Shards.ShardNodes.ToList();
                    ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost _node = _shards[key];

                        if (_node.IsRunning)
                        {
                            return _node.NodeContext.TopologyImpl.CreateDatabase(configuration, collectionStrategy);
                        }

                    }

                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        #endregion

        #region cluster Management

        internal void RemoveServerFromShard(string cluster, string shard)
        {
            if (s_DbmNodeConfiguration.DbmClusters == null)
            {
                throw new System.Exception("can't find the nodes");
            }
            ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
            //check if node is running
            if (_shards.ContainsKey(key) && _shards[key].IsRunning)
            {
                ShardHost _deadshard = _shards[key];

                _rwLock.AcquireWriterLock(Timeout.Infinite);
                _deadshard.RemoveShard();
                _shards.Remove(key);

                _rwLock.ReleaseLock();
            }
            else
            {
                if (s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations.ToList().Exists(x => x.Name.Equals(cluster)))
                {
                    
                    DbmClusterConfiguration dbmClusterConf = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations.ToList().First(x => x.Name.Equals(cluster));
                    if (dbmClusterConf.Shards.ShardNodes.ToList().Exists(x => x.Name.Equals(shard)))
                    {
                        DbmShard shardConf = dbmClusterConf.Shards.ShardNodes.ToList().First(x => x.Name.Equals(shard));
                        string basePath = ConfigurationSettings<DBHostSettings>.Current.BasePath + shard + '.';
                        DeleteDirectories(cluster, shard, basePath);
                        //if (Directory.Exists(basePath))
                        //    Directory.Delete(basePath, true);
                    }
                }
            }
            //UpdateNode config.xml
            List<DbmClusterConfiguration> clusters = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations.ToList();
            DbmClusterConfiguration clusterConfig = null;
            if (clusters.Exists(x => x.Name.Equals(cluster)))
            {
                clusterConfig = clusters.Find(x => x.Name.Equals(cluster));
            }

            if (clusterConfig != null)
            {

                List<DbmShard> dbmShardList = clusterConfig.Shards.ShardNodes.ToList();

                dbmShardList.RemoveAll(x => x.Name.Equals(shard));
                clusterConfig.Shards.ShardNodes = dbmShardList.ToArray();

                if (clusterConfig.Shards.ShardNodes.Length == 0)
                    clusters.Remove(clusterConfig);
            }

            s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations = clusters.ToArray();

            SaveConfiguration();
        }

        private void DeleteDirectories(string cluster, string shard, string basePath)
        {
            NodeContext nodeContext = new NodeContext();
            nodeContext.ClusterName = cluster;
            nodeContext.LocalShardName = shard;
            string datapath = ConfigurationSettings<DBHostSettings>.Current.BasePath;
            nodeContext.DataPath = datapath;
            nodeContext.BasePath = datapath + nodeContext.LocalShardName + Common.MiscUtil.DATA_FOLDERS_SEPERATION;
            nodeContext.DatabasesManager = new DatabasesManager();
            nodeContext.TopologyImpl = new PartitionOfReplica(nodeContext);

            ClusterConfiguration dbClusterConf = ((PartitionOfReplica)nodeContext.TopologyImpl).InitializeAndGetClusterConf(new ClusterConfiguration());
            //It Should Find one if it doesnt will not delete anything.
            if (dbClusterConf != null) 
            {
                foreach (var database in dbClusterConf.Databases.Configurations)
                {
                    string completePath = basePath + database.Key;
                    if (Directory.Exists(completePath))
                        Directory.Delete(completePath, true);
                }
            }
            nodeContext.TopologyImpl.Dispose(true);
        }

        internal bool AddServerToShard(string configCluster, string clusterUID, Address[] configServers, string databaseCluster, string shard, string shardUid, int shardPort, bool start, ClusterConfiguration clusterConfig)
        {
            CanAddToDatabaseCluster(configCluster, clusterUID, databaseCluster, shard, shardUid);
         
            var dbmCsList = new List<DbmConfigServer>();
            foreach (Address configServer in configServers)
            {
                dbmCsList.Add(new DbmConfigServer()
                {
                    Name = configServer.IpAddress.ToString(),
                    Port = configServer.Port
                });
            }


            DbmNodeconfiguration nodeConfig = s_DbmNodeConfiguration;
            if (nodeConfig.DbmClusters == null)
                nodeConfig.DbmClusters = new DbmClustersConfiguration();
            if (!nodeConfig.DbmClusters.ClustersConfigurations.ToList().Exists(x => x.Name.ToLower().Equals(databaseCluster.ToLower())))
            {
                List<DbmClusterConfiguration> dbmClusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration dbmCluster = new DbmClusterConfiguration()
                {
                    Name = databaseCluster,

                    ConfigServers = new DbmConfigServers() { Name = configCluster },

                    Shards = new DbmShards(),
                    UID = clusterUID

                };

                dbmCluster.ConfigServers.Nodes = dbmCsList.ToArray();
                List<DbmShard> dbmShardList = new List<DbmShard>();
                dbmShardList.Add(new DbmShard() { Name = shard, UID = shardUid });
                dbmCluster.Shards.ShardNodes = dbmShardList.ToArray();
                dbmClusters.Add(dbmCluster);
                nodeConfig.DbmClusters.ClustersConfigurations = dbmClusters.ToArray();

            }
            else if (
                nodeConfig.DbmClusters.ClustersConfigurations.ToList()
                    .Exists(x => x.Name.ToLower().Equals(databaseCluster.ToLower())))
            {
                List<DbmClusterConfiguration> dbmClusters =
                    nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration dbmCluster = dbmClusters.First(x => x.Name.ToLower().Equals(databaseCluster.ToLower()));

                List<DbmConfigServer> dbmConfigServerList = dbmCluster.ConfigServers.Nodes.ToList();
                int comamnConfigServers = dbmCsList.RemoveAll(x => dbmConfigServerList.Contains(x));

                if (comamnConfigServers == 0)
                {
                    throw new System.Exception("can't recognize the cluster");
                }


                if (dbmCsList.Count > 0)
                {
                    dbmConfigServerList.AddRange(dbmCsList);
                }
                if (dbmConfigServerList.Count > 2)
                {
                    throw new System.Exception("Configuration server can have two nodes at Max");
                }
                // TODO 
                dbmCluster.ConfigServers.Nodes = dbmConfigServerList.ToArray();

                DbmShard dbmShard = new DbmShard() { Name = shard, UID = shardUid };
                List<DbmShard> shards = dbmCluster.Shards.ShardNodes.ToList();
                if (!shards.Contains(dbmShard))
                {
                    shards.Add(dbmShard);
                }
                dbmCluster.Shards.ShardNodes = shards.ToArray();
                var index = dbmClusters.FindIndex(x => x.Name.Equals(databaseCluster));
                dbmClusters[index] = dbmCluster;
                nodeConfig.DbmClusters.ClustersConfigurations = dbmClusters.ToArray();
            }

            s_DbmNodeConfiguration = nodeConfig;

            SaveConfiguration();
            if (start)
            {
                return StartNode(databaseCluster, shard, shardPort, clusterConfig);
            }


            return true;
            //}
            //else
            //{
            //    throw new ArgumentException("This operation is not supported in the current installed edition of NosDB ");
            //}
        }

        internal bool StartLocalDbNode()
        {

            RemoteConfigurationManager rcm = new RemoteConfigurationManager();
            rcm.IsDatabaseSession = true;
            try
            {
                if (ConfigurationSettings<DBHostSettings>.Current.ConfigurationServicePort == 0
                    || ConfigurationSettings<DBHostSettings>.Current.ConfigurationServiceIP == null)
                    throw new Exception("Unable to access configuration server IP and Port.");

                string localIp = ConfigurationSettings<DBHostSettings>.Current.ConfigurationServiceIP.ToString();


                //try
                //{
                rcm.Initilize(localIp, new ConfigurationChannelFormatter(), new SSPIClientAuthenticationCredential());
                //}
                //catch (Exception ex)
                //{
                //    return false;
                //}



                int port = rcm.GetShardsPort(Common.MiscUtil.LOCAL)[Common.MiscUtil.LOCAL];
                Address[] configServer = { new Address(localIp, Common.MiscUtil.DEFAULT_CS_PORT) };
                AddServerToShard(Common.MiscUtil.LOCAL, null, configServer, Common.MiscUtil.LOCAL, Common.MiscUtil.LOCAL, null, port, false, null);
                StartNode(Common.MiscUtil.LOCAL, Common.MiscUtil.LOCAL, port, null);
                return true;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ManagementServer.StartLocalDbNode", "Error on starting localDbNode ", e);

                return false;
            }
            finally
            {
                rcm.Dispose();
            }

        }

        private void SaveConfiguration()
        {
            try
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                List<DbmNodeconfiguration> configs = new List<DbmNodeconfiguration>();

                configs.Add(s_DbmNodeConfiguration);


                SaveConfiguration(configs.ToArray());

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId, string username)
        {

            foreach (var shardHost in _shards.Values)
            {
                shardHost.NodeContext.SecurityManager.PublishAuthenticatedUserInfoToDBServer(sessionId, username);
            }
        }


        private void SaveConfiguration(object[] nodeConfiguration)
        {
            StringBuilder xml = new StringBuilder();
            if (nodeConfiguration != null && nodeConfiguration.Length > 0)
            {
                ConfigurationBuilder cBuilder = new ConfigurationBuilder(nodeConfiguration);
                cBuilder.RegisterRootConfigurationObject(typeof(Alachisoft.NosDB.Core.Configuration.DbmNodeconfiguration));
                xml.Append(cBuilder.GetXmlString());
            }
            WriteXMLToFile(xml.ToString());
        }

        private void WriteXMLToFile(string xml)
        {
            System.IO.StreamWriter sw = null;
            System.IO.FileStream fs = null;
            try
            {
                fs = new System.IO.FileStream(_fileName, System.IO.FileMode.Create);
                sw = new System.IO.StreamWriter(fs);
                sw.Write(xml);
                sw.Flush();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (sw != null)
                {
                    try
                    {
                        sw.Close();
                    }
                    catch (Exception)
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
                    catch (Exception)
                    { }
                    fs.Dispose();
                    fs = null;
                }
            }
        }

        internal bool StopNodeForClients()
        {
            return false;
        }

        internal bool GrantRole(string cluster, string shardName, ResourceId resourceId, IUser userInfo, IRole roleInfo)
        {
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = shardName };
            if (_shards.ContainsKey(shardIdentity) && _shards[shardIdentity].IsRunning)
                return _shards[shardIdentity].SecurityManager.Grant(shardName, resourceId, userInfo, roleInfo);
            return false;
        }

        internal bool RevokeRole(string cluster, string shardName, ResourceId resourceId, IUser userInfo, IRole roleInfo)
        {
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = shardName };
            if (_shards.ContainsKey(shardIdentity) && _shards[shardIdentity].IsRunning)
                return _shards[shardIdentity].SecurityManager.Revoke(shardName, resourceId, userInfo, roleInfo);
            return false;
        }

        #endregion

        internal void RemoveConfigServerChannel(Common.Net.Address address)
        {
            if (_connectedConfigServers.Exists(p => p.PeerAddress.Equals(address)) && _connectedConfigServers.Exists(p => p.Connected == false))
                _connectedConfigServers.RemoveAll(p => p.PeerAddress.Equals(address));
        }

        internal void OnChannelDisconnected(ISessionId sessionId)
        {
            foreach (ShardHost shardHost in _shards.Values)
            {
                shardHost.SecurityManager.OnChannelDisconnected(sessionId);
            }
        }

        internal bool StartNode(string cluster, string shard, int serverPort, ClusterConfiguration clusterConf)
        {
            try
            {

                DbmNodeconfiguration nodeConfig = (DbmNodeconfiguration)s_DbmNodeConfiguration;
                string serverIp = nodeConfig.IP;
                string clusterName = cluster;
                ShardIdentity key = new ShardIdentity() { Cluster = clusterName, Shard = shard };
                List<DbmClusterConfiguration> clusters = nodeConfig.DbmClusters.ClustersConfigurations.ToList();
                DbmClusterConfiguration clusterConfig = clusters.Find(x => x.Name.Equals(cluster));

                if (clusterConfig != null)
                {
                    string csIP = clusterConfig.ConfigServers.Nodes.First().Name;
                    int csPort = clusterConfig.ConfigServers.Nodes.First().Port;
                    ShardHost node = null;

                    if (!_shards.ContainsKey(key))
                    {
                        node = ReInitializeShard(shard, serverPort, serverIp, clusterName, key, clusterConfig, node);
                    }
                    else
                    {
                        node = _shards[key];
                        // Update and reinitialize shardhost if port is changed.
                        if (node.NodeContext.ShardServer.Port != serverPort)
                            node = ReInitializeShard(shard, serverPort, serverIp, clusterName, key, clusterConfig, node);
                    }
                    if (!node.IsRunning)
                    {
                        node.Start(clusterConf);
                    }
                    //node.NodeContext.LocalShardName = shard;
                    return true;
                }
                throw new Exception("Node is  not part of specified cluster");

            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ManagementServer.StartNode", "Error on starting shard " + shard, e);

                throw;
            }

        }

        private ShardHost ReInitializeShard(string shard, int serverPort, string serverIp, string clusterName, ShardIdentity key, DbmClusterConfiguration clusterConfig, ShardHost node)
        {
            node = new ShardHost();
            node.NodeContext.SecurityManager = new SecurityManager();
            node.NodeContext.SecurityManager.Initialize(shard);

            node.Initialize(clusterConfig, serverIp, serverPort, clusterName, shard);
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            _shards[key] = node;
            _rwLock.ReleaseLock();
            return node;
        }

        internal bool StopNode(string cluster, string shard)
        {
            try
            {
                ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                if (_shards.ContainsKey(key))
                {

                    ShardHost node = _shards[key];
                    if (node.IsRunning)
                    {
                        bool isStopped = node.Stop(false);
                        if (isStopped)
                            node.NodeContext.SecurityManager.RemoveSecurityDatabase(shard);
                        return isStopped;
                    }
                    else
                    {
                        throw new Exception("Node is not running");
                    }
                }
                else
                {
                    throw new Exception("Node is not running");
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ManagementServer.StopNode", "Error on stopping  shard " + shard, e);

                throw;
            }
        }

        public RecoveryOperationStatus OnRecoveryOperationReceived(string cluster, string shard, RecoveryOperation opContext)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            if (opContext != null)
            {
                ShardIdentity key = new ShardIdentity() { Cluster = cluster, Shard = shard };
                if (_shards.ContainsKey(key))
                {
                    ShardHost node = _shards[key];
                    if (node.IsRunning)
                        state = node.NodeContext.TopologyImpl.OnRecoveryOperationReceived(opContext);
                }
            }
            return state;
        }

        internal bool DropUser(string cluster, string localShardName, IUser userInfo)
        {
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = localShardName };
            if (_shards.ContainsKey(shardIdentity))
                return _shards[shardIdentity].SecurityManager.DropUser(localShardName, userInfo);
            return false;
        }

        internal bool CreateUser(string cluster, string localShardName, IUser userInfo)
        {
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = localShardName };
            if (_shards.ContainsKey(shardIdentity))
                return _shards[shardIdentity].SecurityManager.CreateUser(localShardName, userInfo);
            return false;
        }

        internal void PublishAuthenticatedUserInfoToDBServer(string cluster, string shard, ISessionId sessionId, string username)
        {
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = shard };
            if (_shards.ContainsKey(shardIdentity))
                _shards[shardIdentity].SecurityManager.PublishAuthenticatedUserInfoToDBServer(sessionId, username);
        }

        internal void PopulateSecurityInformationOnDBServer(string cluster, string shardName, IList<IResourceItem> resources)
        {
            ShardIdentity shardIdentity = new ShardIdentity() { Cluster = cluster, Shard = shardName };
            if (_shards.ContainsKey(shardIdentity))
                _shards[shardIdentity].SecurityManager.PopulateSecurityInformation(shardName, resources, null);
        }

        internal Common.Security.Server.IServerAuthenticationCredential Authenticate(string localShardName, Common.Security.Client.IClientAuthenticationCredential clientCredentials, ISessionId sessionId, bool isLocalClient, bool isConfigSession)
        {
            return SecurityManager.Authenticate(localShardName, clientCredentials, sessionId, isLocalClient, isConfigSession ? MiscUtil.NOSDB_CSVC_NAME : null);
        }

        internal void SynchronizeWithConfigManager()
        {
            RemoteConfigurationManager rcm;
            //Get all clusters from node Configuration
            foreach (var cluster in s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations)
            {
                rcm = null;
                //get all ConfigurationServer nodes in the cluster.
                foreach (var server in cluster.ConfigServers.Nodes)
                {
                   
                    try
                    {
                        //try connecting with Configuration Server nodes (preferably select primary)
                        RemoteConfigurationManager remoteConfig = new RemoteConfigurationManager();
                        remoteConfig.IsDatabaseSession = true;
                        remoteConfig.Initilize(MiscUtil.CLUSTERED, server.Name, server.Port, new ConfigurationChannelFormatter(), new SSPIClientAuthenticationCredential());
                        if (rcm == null)
                        {
                            rcm = remoteConfig;
                        }
                        else if (remoteConfig.VerifyConfigurationClusterPrimery(cluster.Name))
                        {
                            rcm = remoteConfig;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                            LoggerManager.Instance.ServerLogger.Error("ManagementServer.SynchroniseWithConfigManager()", "unable to connect with " + server.Name + ":" + server.Port, ex);
                    }

                }
                try 
                { 
                    //rcm is null if unable to connect with any configurationServer node
                    if (rcm != null)
                    {
                        //GetClusterConfig from configurationServer
                        ClusterConfiguration cConfig = rcm.GetDatabaseClusterConfig(cluster.Name.Equals(Common.MiscUtil.CLUSTERED));
                        //If config is null it means that cluster doesnot exist anymore
                        if (cConfig == null)
                        {
                            //Remove shard from nodeConfig.xml + Delete shard folder
                            foreach (var shard in cluster.Shards.ShardNodes)
                            {
                                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                                    LoggerManager.Instance.ServerLogger.Info("ManagementServer.SynchroniseWithConfigManager()",
                                        "Removing server from shard. cluster = " + cluster.Name + " shard = " + shard.Name);
                                RemoveServerFromShard(cluster.Name, shard.Name);
                            }
                        }
                        //Else if Config is not null it means that cluster with this name is present on ConfigServer
                        else
                        {
                            //Get all Shard name from NodeConfig.xml
                            foreach (var shard in cluster.Shards.ShardNodes)
                            {
                                //If configurationServer contains shard with same name
                                if (cConfig.Deployment.ContainsShard(shard.Name))
                                {
                                    //If configurationServer doesnot contain this NodeIp in shardNodes
                                    if (!cConfig.Deployment.GetShard(shard.Name).Servers.ContainsNode(s_DbmNodeConfiguration.IP))
                                    {
                                        //Remove shard from nodeConfig.xml + Delete shard folder
                                        if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                                            LoggerManager.Instance.ServerLogger.Info("ManagementServer.SynchroniseWithConfigManager()",
                                                "Removing server from shard. cluster = " + cluster.Name + " shard = " + shard.Name);
                                        RemoveServerFromShard(cluster.Name, shard.Name);
                                    }
                                }
                                //Else if configurationServer does not contain shard with same name
                                else
                                {
                                    //Remove shard from nodeConfig.xml + Delete shard folder
                                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                                        LoggerManager.Instance.ServerLogger.Info("ManagementServer.SynchroniseWithConfigManager()",
                                            "Removing server from shard. cluster = " + cluster.Name + " shard = " + shard.Name);
                                    RemoveServerFromShard(cluster.Name, shard.Name);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                            LoggerManager.Instance.ServerLogger.Error("ManagementServer.SynchronizeWithConfigManager()", "Synchronization failed, as unable to connect with any configManager ");
                    }
                }
                catch(Exception e)
                {
                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                        LoggerManager.Instance.ServerLogger.Error("ManagementServer.SynchronizeWithConfigManager()", "Synchronization failed, Error Getting configuration. "+ e);
                }
            }
        }

        internal string GetDatabaseCluster()
        {
            if (s_DbmNodeConfiguration != null && s_DbmNodeConfiguration.DbmClusters != null)
            {
                DbmClusterConfiguration[] clusterConfigurations = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;
                if (clusterConfigurations != null && clusterConfigurations.Count() > 0)
                {
                    return clusterConfigurations[0].Name;
                }
            }

            return null;
        }

        internal List<Address> GetConfClusterServers(string cluster)
        {
            if (s_DbmNodeConfiguration != null && s_DbmNodeConfiguration.DbmClusters != null)
            {
                DbmClusterConfiguration confCluster = null;
                DbmClusterConfiguration[] clusterConfigurations = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;
                if (clusterConfigurations != null && clusterConfigurations.Count() > 0)
                {

                    foreach (DbmClusterConfiguration conf in clusterConfigurations)
                    {
                        if (String.Equals(conf.Name, cluster, StringComparison.OrdinalIgnoreCase))
                        {
                            confCluster = conf;
                            break;
                        }
                    }

                    if (confCluster != null && confCluster.ConfigServers != null && confCluster.ConfigServers.Nodes.Length > 0)
                    {
                        List<Address> serverList = new List<Address>();
                        foreach (DbmConfigServer ss in confCluster.ConfigServers.Nodes)
                        {
                            serverList.Add(new Address(ss.Name, ss.Port));
                        }
                        return serverList;//confCluster.ConfigServers.Nodes.ToList();
                    }
                }
            }
            return null;
        }

        internal void NodeAddedToConfigurationCluster(string clusterName, ServerNode node)
        {
            bool canAdd = true;
            int port = MiscUtil.DEFAULT_CS_PORT;
            DbmClusterConfiguration myCluster = null;

            //Get all clusters from node Configuration
            foreach (var cluster in s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations)
            {
                if (!cluster.Name.Equals(clusterName)) continue;
                myCluster = cluster;

                //get all ConfigurationServer nodes in the cluster.
                foreach (var server in cluster.ConfigServers.Nodes)
                {
                    port = server.Port;
                    if (server.Name.Equals(node.Name))
                    {
                        canAdd = false;
                        break;
                    }
                }
                if (!canAdd)
                {
                    break;
                }
            }

            if (canAdd)
            {
                if (myCluster == null)
                    throw new Exception("Cluster configuration is null. Cluster '" + clusterName + "' was not found");
                try
                {
                    //try connecting with Configuration Server nodes (preferably select primary)
                    List<DbmConfigServer> list = new List<DbmConfigServer>(myCluster.ConfigServers.Nodes.Length + 1);
                    list.AddRange(myCluster.ConfigServers.Nodes);
                    DbmConfigServer newConfigServer = new DbmConfigServer();
                    newConfigServer.Name = node.Name;
                    newConfigServer.Port = port;
                    list.Add(newConfigServer);
                    myCluster.ConfigServers.Nodes = list.ToArray();
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                        LoggerManager.Instance.ServerLogger.Error("ManagementServer.NodeAddedToConfigurationCluster", "unable to connect with " + node.Name + ":" + port, ex);
                }
                SaveConfiguration();
            }
            else
            {
                throw new Exception(node.Name + " already exist in the configuration");
            }
        }

        internal void NodeRemovedFromConfigurationCluster(string clusterName, ServerNode node)
        {
            DbmConfigServer removedConfigServer = null;
            DbmClusterConfiguration clusterConfiguration = null;

            //Get all clusters from node Configuration
            foreach (var cluster in s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations)
            {
                if (!cluster.Name.Equals(clusterName)) continue;

                //get all ConfigurationServer nodes in the cluster.
                foreach (var server in cluster.ConfigServers.Nodes)
                {
                    try
                    {
                        if (server.Name.Equals(node.Name))
                        {
                            clusterConfiguration = cluster;
                            removedConfigServer = server;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                            LoggerManager.Instance.ServerLogger.Error("ManagementServer.NodeRemovedFromConfigurationCluster", "unable to remove " + server.Name + ":" + server.Port, ex);
                    }
                }
                if (removedConfigServer != null)
                    break;
            }

            if (removedConfigServer != null)
            {
                List<DbmConfigServer> configServers = clusterConfiguration.ConfigServers.Nodes.ToList();
                configServers.Remove(removedConfigServer);
                clusterConfiguration.ConfigServers.Nodes = configServers.ToArray();
                SaveConfiguration();
            }
            else
            {
                throw new Exception(node.Name + " is not part of the configuration cluster");
            }
        }

        internal string[] GetShards()
        {

            if (s_DbmNodeConfiguration != null && s_DbmNodeConfiguration.DbmClusters != null)
            {
                DbmClusterConfiguration[] clusterConfigurations = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;
                if (clusterConfigurations != null && clusterConfigurations.Count() > 0)
                {


                    DbmShards dbmShards = clusterConfigurations[0].Shards;

                    if (dbmShards != null && dbmShards.ShardNodes != null && dbmShards.ShardNodes.Count() > 0)
                    {
                        List<string> shardList = new List<string>();

                        foreach (DbmShard shard in dbmShards.ShardNodes)
                        {
                            shardList.Add(shard.Name);
                        }

                        return shardList.ToArray();
                    }
                }
            }
            return null;
        }

        internal bool CanAddToDatabaseCluster(string configurationCluster, string clusterUID, string databaseCluster, string shard, string shardUid)
        {
            if (string.Compare(databaseCluster, Alachisoft.NosDB.Common.MiscUtil.LOCAL, true) == 0)
                return true;

            if (s_DbmNodeConfiguration != null && s_DbmNodeConfiguration.DbmClusters != null)
            {
                DbmClusterConfiguration[] clusterConfigurations = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;
                if (clusterConfigurations != null && clusterConfigurations.Count() > 0)
                {
                    DbmClusterConfiguration existingConfiguration = clusterConfigurations.FirstOrDefault(p => p.Name.ToLower() == Alachisoft.NosDB.Common.MiscUtil.CLUSTERED.ToLower());

                    //verify if this node is already part of any database cluster
                    if (existingConfiguration == null)
                        return true;
                    else
                    {
                        //Verify if it's part of same database cluster by checking configuration cluster name and UID

                        if (string.Compare(clusterUID, existingConfiguration.UID, true) == 0)
                        {
                            //Verify if this server is already part of the given shard

                            if (!existingConfiguration.Shards.ShardNodes.ToList().Exists(p => p.Name.ToLower() == shard.ToLower()))
                            {
                                //this database cluster is part the database cluster but of different shard
                                return true;
                            }
                            else
                                throw new Exception("Database server is already member of '" + shard + "'");
                        }
                        else
                            throw new Exception("Database server is already part of anonther cluster");
                    }
                }
            }


            return true;
        }

        public List<Address> GetDatabaseServerNodes()
        {
            var serverNodes = new List<Address>();
            if (s_DbmNodeConfiguration != null && s_DbmNodeConfiguration.DbmClusters != null)
            {
                DbmClusterConfiguration[] clusterConfigurations = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;

                if (clusterConfigurations != null && clusterConfigurations.Count() > 0)
                {
                    for (int i = 0; i < clusterConfigurations.Count(); i++)
                    {
                        if (clusterConfigurations[i] != null && clusterConfigurations[i].Name.Equals("cluster"))
                        {
                            if (clusterConfigurations[i].ConfigServers != null)
                            {
                                DbmConfigServer[] dbmConfigServers = clusterConfigurations[i].ConfigServers.Nodes;
                                if (dbmConfigServers != null)
                                {
                                    for (int j = 0; j < dbmConfigServers.Count(); j++)
                                    {
                                        var serverNode = new Address
                                        {
                                            ip = dbmConfigServers[j].Name,
                                            Port = dbmConfigServers[j].Port
                                        };
                                        serverNodes.Add(serverNode);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return serverNodes;
        }

        internal bool StartDbNodes()
        {
            RemoteConfigurationManager rcm = new RemoteConfigurationManager();
            try
            {
                if (s_DbmNodeConfiguration != null && s_DbmNodeConfiguration.DbmClusters != null)
                {
                    DbmClusterConfiguration[] clusterConfigurations = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;
                    if (clusterConfigurations != null && clusterConfigurations.Count() > 0)
                    {
                        for (int i = 0; i < clusterConfigurations.Count(); i++)
                        {
                            if (clusterConfigurations[i] != null && clusterConfigurations[i].Name.Equals("cluster"))
                            {
                                DbmShards dbmShards = clusterConfigurations[i].Shards;

                                if (dbmShards != null && dbmShards.ShardNodes != null && dbmShards.ShardNodes.Count() > 0)
                                {
                                    for (int j = 0; j < clusterConfigurations[i].ConfigServers.Nodes.Count(); j++)
                                    {
                                        string localIp = clusterConfigurations[i].ConfigServers.Nodes[j].Name;
                                        int port = clusterConfigurations[i].ConfigServers.Nodes[j].Port;
                                        try
                                        {
                                            rcm.IsDatabaseSession = true;
                                            rcm.Initilize(MiscUtil.CLUSTERED, localIp, port, new ConfigurationChannelFormatter(),
                                                new SSPIClientAuthenticationCredential());
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                    Dictionary<string, int> shardPorts = rcm.GetShardsPort(Common.MiscUtil.CLUSTERED);
                                    foreach (DbmShard shard in dbmShards.ShardNodes)
                                    {
                                        StartNode(Common.MiscUtil.CLUSTERED, shard.Name, shardPorts[shard.Name], null);
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ManagementServer.StartLocalDbNode", "Error on starting shardDbNode ", e);

                return false;
            }
            finally
            {
                rcm.Dispose();
            }
        }

        internal bool SetDatabaseMode(string cluster, string shardName, string databaseName, DatabaseMode databaseMode)
        {
            DbmClusterConfiguration[] clusters = s_DbmNodeConfiguration.DbmClusters.ClustersConfigurations;
            for (int i = 0; i < clusters.Count(); i++)
            {
                DbmClusterConfiguration clusterConfig = clusters[i];
                if (clusterConfig != null && clusterConfig.Name.Equals(cluster.ToLower()))
                {
                    var key = new ShardIdentity() { Cluster = cluster, Shard = shardName };
                    if (_shards.ContainsKey(key))
                    {
                        ShardHost node = _shards[key];
                        return node.NodeContext.TopologyImpl.SetDatabaseMode(databaseName, databaseMode);
                    }
                }
            }
            return false;
        }
    }
}
