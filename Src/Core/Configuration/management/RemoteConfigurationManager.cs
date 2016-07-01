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
using Alachisoft.NosDB.Common.Configuration.RPC;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration.Services.Client;

namespace Alachisoft.NosDB.Core.Configuration
{
    public class RemoteConfigurationManager
    {
        DatabaseRPCService _rpc = null;
        IConfigurationServer _remote = null;
        IConfigurationSession _session = null;
        private bool _initialised;
        private string _configServerIp;
        private int _port;

        internal bool IsDatabaseSession { set; get; }

        public bool IsInitialised
        {
            get
            {
                return _initialised;
            }
        }

        public void Initilize(string configServerIp, IChannelFormatter channelFormatter, IClientAuthenticationCredential clientCredentials)
        {
            if (!_initialised)
            {
                ManagementHost.RegisterCompactTypes();
            }

            _rpc = new DatabaseRPCService(configServerIp);
            ConfigServerIP = configServerIp;
            Port = Common.MiscUtil.DEFAULT_CS_PORT;
            _remote = _rpc.GetConfigurationServer(new TimeSpan(0, 1, 30), Common.Communication.SessionTypes.Client,channelFormatter);
            if (IsDatabaseSession)
                _remote.MarkDatabaseSesion();
            _session = _remote.OpenConfigurationSession(clientCredentials);
            clientCredentials.UserName = ((OutProcConfigurationSession)_session).SessionId.Username;
            ClientCredential = clientCredentials;
            _initialised = true;
        }

        private void Initialize(ConfigSessionParams configurationSessionParams)
        {
            _session = configurationSessionParams.Session;

            _rpc = configurationSessionParams.Rpc;
            ConfigServerIP = configurationSessionParams.ConfigServerIp;
            Port = configurationSessionParams.Port;
            _remote = configurationSessionParams.Remote;
            if (IsDatabaseSession)
                _remote.MarkDatabaseSesion();
            _session = configurationSessionParams.Session;
            _initialised = true;
        }

        public string ConfigServerIP
        {
            get { return _configServerIp; }
            set { _configServerIp = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public IClientAuthenticationCredential ClientCredential { get; private set; }

        public void Initilize(string cluster, string configServerIp, int port, IChannelFormatter channelFormatter, IClientAuthenticationCredential clientCredentials)
        {
            ManagementHost.RegisterCompactTypes();
            ConfigServerIP = configServerIp;
            Port = port;
            _rpc = new DatabaseRPCService(configServerIp, port);
            _remote = _rpc.GetConfigurationServer(new TimeSpan(0, 1, 30), SessionTypes.Client, channelFormatter);
            if (IsDatabaseSession)
                _remote.MarkDatabaseSesion();
            _session = _remote.OpenConfigurationSession(clientCredentials);
            clientCredentials.UserName = ((OutProcConfigurationSession)_session).SessionId.Username;
            ClientCredential = clientCredentials;
            _initialised = true;
        }

        #region deployment

        public bool StartNode(string shardName, ServerNode node)
        {
            IPAddress add;

            const string clusterName = MiscUtil.CLUSTERED;

            if (string.IsNullOrWhiteSpace(clusterName))
                throw new Exception("Cluster name must have some value.");

            if (string.IsNullOrWhiteSpace(shardName))
                throw new Exception("Shard name must have some value.");

            if (!IPAddress.TryParse(node.Name, out add))
                throw new Exception("ServerNode must have valid IPAdress. ");

            if (node.Priority < 0)
                throw new Exception("ServerNode must have  valid priority.");

            return _session.StartNode(clusterName, shardName, node);
        }

        public bool StopNode(string shardName, ServerNode node)
        {
            try
            {
                IPAddress add;

                const string clusterName = MiscUtil.CLUSTERED;

                if (string.IsNullOrWhiteSpace(shardName))
                    throw new Exception("Shard name must have some value.");

                if (!IPAddress.TryParse(node.Name, out add))
                    throw new Exception("ServerNode must have valid IPAdress. ");

                if (node.Priority < 0)
                    throw new Exception("ServerNode must have  valid priority.");

                return _session.StopNode(clusterName, shardName, node);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddNodeToConfigurationCluster(string name, ServerNode node)
        {
            try
            {
                IPAddress add;

                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("Configuration Cluster name must have some value.");

                if (!IPAddress.TryParse(node.Name, out add))
                    throw new Exception("ServerNode must have valid IPAdress. ");

                if (node.Priority < 0)
                    throw new Exception("ServerNode must have  valid priority.");

                _session.AddNodeToConfigurationCluster(name, node);
            }
            catch (Exception e)
            { throw e; }
        }

        public void RemoveNodeFromConfigurationCluster(string name, ServerNode node)
        {
            try
            {
                IPAddress add;

                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("Configuration Cluster name must have some value.");

                if (!IPAddress.TryParse(node.Name, out add))
                    throw new Exception("ServerNode must have valid IPAdress. ");

                if (node.Priority < 0)
                    throw new Exception("ServerNode must have  valid priority.");

                _session.RemoveNodeFromConfigurationCluster(name, node);
            }
            catch (Exception e)
            { throw e; }
        }

        public bool AddServerToShard(string shardName, ServerNode node)
        {
            try
            {
                IPAddress add;
                string clusterName = MiscUtil.CLUSTERED;

                if (string.IsNullOrWhiteSpace(shardName))
                    throw new Exception("Shard name must have some value.");

                if (!IPAddress.TryParse(node.Name, out add))
                    throw new Exception("ServerNode must have valid IPAdress. ");

                if (node.Priority < 0)
                    throw new Exception("ServerNode must have  valid priority.");


                ConfigServerConfiguration configServerConfiguration = _session.GetConfigurationClusterConfiguration(clusterName);
                CheckAndCreateNewConfigServer(clusterName, configServerConfiguration, node);
                try
                {
                    if (_session.AddServerToShard(clusterName, shardName, node))
                    {
                        return true;
                    }
                    ValidateConfigurationCluster(clusterName);
                    return false;
                }
                catch (Exception)
                {
                    if(_initialised)
                        ValidateConfigurationCluster(clusterName);
                    throw;
                }
            }
            catch (Exception e)
            { throw e; }
        }

        public bool RemoveServerFromShard(string shardName, ServerNode node)
        {
            try
            {
                IPAddress add;

                string clusterName = MiscUtil.CLUSTERED;

                if (string.IsNullOrWhiteSpace(shardName))
                    throw new Exception("Shard name must have some value.");

                if (!IPAddress.TryParse(node.Name, out add))
                    throw new Exception("ServerNode must have valid IPAdress. ");

                if (node.Priority < 0)
                    throw new Exception("ServerNode must have  valid priority.");

                CheckAndRemoveConfigClusterNode(clusterName, _session.GetConfigurationClusterConfiguration(clusterName), node);

                try
                {
                    if (_session.RemoveServerFromShard(clusterName, shardName, node))
                    {
                        return true;
                    }
                    ValidateConfigurationCluster(clusterName);
                }
                catch (Exception)
                {
                    if (_initialised)
                    {
                        ValidateConfigurationCluster(clusterName);
                        SelectNewNodeForConfigServer(_session.GetConfigurationClusterConfiguration(clusterName), _session.GetDatabaseClusterConfiguration(clusterName), null);
                    }
                    throw;
                }

                return false;
            }
            catch (Exception e)
            { throw e; }

        }

        //if clientauth parameter is null, it means we need to impersonate the new session using username and password specified
        private ConfigSessionParams GetNewSession(string configServerIp, int port, IClientAuthenticationCredential clientAuth = null)
        {
            ConfigSessionParams configSessionParams = new ConfigSessionParams();
            configSessionParams.Rpc = new DatabaseRPCService(configServerIp, port);
            configSessionParams.Remote = configSessionParams.Rpc.GetConfigurationServer(new TimeSpan(0, 1, 30), Common.Communication.SessionTypes.Client, new ConfigurationChannelFormatter());
            if (IsDatabaseSession)
                _remote.MarkDatabaseSesion();
            configSessionParams.Session = configSessionParams.Remote.OpenConfigurationSession(clientAuth);
            configSessionParams.ConfigServerIp = configServerIp;
            configSessionParams.Port = port;
            return configSessionParams;
        }

        public void CreateCluster(ClusterConfiguration configuration, IClientAuthenticationCredential credentials = null)
        {
            try
            {
                if (configuration == null)
                    throw new Exception("can't find cluster configuration");

                if (string.IsNullOrWhiteSpace(configuration.Name))
                    throw new Exception("cluster name must have some value");

                if (!_initialised)
                    ManagementHost.RegisterCompactTypes();
                if (_initialised)
                    Dispose();
                #region This code needs refactoring. SelectNewNodeForConfigServer can be used after some refactoring

                List<ConfigSessionParams> configSessionsParamters = new List<ConfigSessionParams>();
                ConfigServerConfiguration configServerConfiguration = new ConfigServerConfiguration();
                configServerConfiguration.Name = MiscUtil.CLUSTERED;
                configServerConfiguration.Servers = new ServerNodes();
                configServerConfiguration.UID = Guid.NewGuid().ToString();
                configServerConfiguration.Port = Common.MiscUtil.DEFAULT_CS_PORT;
                int priority = 1;

                foreach (var shardConfiguration in configuration.Deployment.Shards.Values)
                {
                    foreach (var node in shardConfiguration.Servers.Nodes.Values)
                    {
                        if (!configServerConfiguration.Servers.ContainsNode(node.Name))
                        {
                            IClientAuthenticationCredential configSessionCredential = credentials == null ? ((OutProcConfigurationSession)_session).ClientCredentials : credentials;
                            ConfigSessionParams configSessionParams = GetNewSession(node.Name, Common.MiscUtil.DEFAULT_CS_PORT, configSessionCredential);
                            configSessionParams.Session.IsNodeRunning(node.Name);
                            if (configSessionParams.Session.VerifyConfigurationClusterAvailability(configuration.Name))
                            {
                                ServerNode server = node.Clone() as ServerNode;
                                server.Priority = priority;
                                priority++;
                                configSessionsParamters.Add(configSessionParams);
                                configServerConfiguration.Servers.AddNode(server);

                                if (configServerConfiguration.Servers.Nodes.Count == 2)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                throw new Exception(configSessionParams.ConfigServerIp + " cannot be added to the cluster. Make sure that it is not part of any other cluster");
                            }
                        }
                    }
                    if (priority == 3)
                        break;
                }

                foreach (var configurationSessionParams in configSessionsParamters)
                {
                    configurationSessionParams.Session.CreateConfigurationCluster(configServerConfiguration, configuration.Deployment.HeartbeatInterval, configuration.Deployment.Replication, configuration.DisplayName);

                    if (!_initialised)
                    {
                        Initialize(configurationSessionParams);
                    }
                    else
                    {
                        configurationSessionParams.Dispose();
                    }
                }
                #endregion

                foreach (ShardConfiguration shardConfig in configuration.Deployment.Shards.Values)
                {
                    AddShardToCluster(shardConfig, null);
                }
                ////_session.CreateCluster(configuration.Name, configuration);
            }
            catch (Exception e)
            {
                if(_initialised)
                    ValidateConfigurationCluster(MiscUtil.CLUSTERED);
                throw e;
            }
        }

        public bool AddShardToCluster(ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration)
        {
            try
            {
                const string cluster = MiscUtil.CLUSTERED;
                if (string.IsNullOrWhiteSpace(cluster))
                    throw new Exception("cluster name not specified");

                if (string.IsNullOrWhiteSpace(shardConfiguration.Name))
                    throw new Exception("shard name not specified");

                foreach (var node in shardConfiguration.Servers.Nodes.Values)
                {
                    _session.IsNodeRunning(node.Name);  //Ensure that all the nodes are running
                }

                ConfigServerConfiguration configServerConfiguration = _session.GetConfigurationClusterConfiguration(cluster);
                foreach (var node in shardConfiguration.Servers.Nodes.Values)
                {
                    CheckAndCreateNewConfigServer(cluster, configServerConfiguration, node);
                }

                try
                {
                    if (_session.AddShardToCluster(cluster, shardConfiguration.Name, shardConfiguration, distributionConfiguration))
                    {
                        return true;
                    }

                    ValidateConfigurationCluster(cluster);
                    return false;
                }
                catch (Exception)
                {
                    if(_initialised)
                        ValidateConfigurationCluster(cluster);
                    throw;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public ClusterConfiguration GetDatabaseClusterConfig(bool cluster)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);
                return _session.GetDatabaseClusterConfiguration(clusterName);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public bool StartShard(string shard)
        {
            try
            {
                string cluster = MiscUtil.CLUSTERED;

                if (string.IsNullOrWhiteSpace(shard))
                    throw new Exception("shard name not specified");

                return _session.StartShard(cluster, shard);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public bool IsNodeRunning(string nodeAddress)
        {
            try
            {
                
                return _session.IsNodeRunning(nodeAddress);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public bool StopShard(string shard)
        {
            try
            {
                string cluster = MiscUtil.CLUSTERED;

                if (string.IsNullOrWhiteSpace(shard))
                    throw new Exception("shard name not specified");

                return _session.StopShard(cluster, shard);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        //
        public void StartCluster()
        {
            try
            {
                string clusterName = MiscUtil.CLUSTERED;

                _session.StartCluster(clusterName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void StopCluster()
        {
            try
            {
                string clusterName = MiscUtil.CLUSTERED;
                _session.StopCluster(clusterName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool RemoveShardFromCluster(string shard, IDistributionConfiguration configuration, Boolean isGraceFull)
        {
            try
            {
                string cluster = MiscUtil.CLUSTERED;

                if (string.IsNullOrWhiteSpace(shard))
                    throw new Exception("shard name not specified");

                ShardConfiguration shardConfiguration = _session.GetShardConfiguration(cluster, shard);
                if (shardConfiguration == null) throw new Exception("Shard '" + shard + "' does not exist");

                ConfigServerConfiguration configServerConfiguration = _session.GetConfigurationClusterConfiguration(cluster);
                foreach (var node in shardConfiguration.Servers.Nodes.Values)
                {
                    CheckAndRemoveConfigClusterNode(cluster, configServerConfiguration, node);
                }

                try
                {
                    if (_session.RemoveShardFromCluster(cluster, shard, configuration, isGraceFull))
                    {
                        return true;
                    }
                    else
                    {
                        ValidateConfigurationCluster(cluster);
                        return false;
                    }
                }
                catch (Exception)
                {
                    if (_initialised)
                    {
                        ValidateConfigurationCluster(cluster);
                        SelectNewNodeForConfigServer(_session.GetConfigurationClusterConfiguration(cluster), _session.GetDatabaseClusterConfiguration(cluster), null);
                    }
                    throw;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void DeployAssemblies(string cluster,string deploymentId,string deploymentName,string assemblyFileName, Byte[] bytes)
        {
            try
            {                
                //if (string.IsNullOrWhiteSpace(shard))
                //    throw new Exception("shard name not specified");
                _session.DeployAssemblies(cluster, deploymentId, deploymentName,assemblyFileName, bytes);
            }
            catch (Exception e)
            {
                throw e;
            }

        }



        #endregion

        public void Dispose()
        {
            if (_session != null)
            {
                _session.Close();
                
            }
            if (_rpc != null)
                _rpc.Dispose();

            _initialised = false;
        }

        internal Dictionary<string, int> GetShardsPort(string clusterName)
        {
            
            return _session.GetShardsPort(clusterName);
        }

        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {
            return _session.GetDatabaseClusterConfiguration(clusterName);
        }

        public ConfigServerConfiguration GetConfigurationClusterConfiguration()
        {
            //if(!EditionInfo.IsRemoteClient)
            return _session.GetConfigurationClusterConfiguration(MiscUtil.CLUSTERED);
            //else
            //{
            //    throw new Exception("This operation is not supported in the current install edition of NosDB. ");
            //}
        }

        public ClusterInfo GetDatabaseClusterInfo(string cluster)
        {
            return _session.GetDatabaseClusterInfo(cluster);
        }

        public void ConfigureDistributionStategy(bool cluster, string database, string collection, IDistributionStrategy streategy)
        {
            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("database name not specified");

            if (string.IsNullOrWhiteSpace(collection))
                throw new Exception("collection name not specified");

            _session.ConfigureDistributionStategy(clusterName, database, collection, streategy);
        }

        public void CreateDatabase(bool cluster, DatabaseConfiguration dc)
        {
            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (dc == null)
                throw new Exception("DatabaseConfiguration is null");

            _session.CreateDatabase(clusterName, dc);
        }

        

        public void DropDatabase(bool cluster, string database, bool dropFiles)
        {
            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("Database name not specified.");

            _session.DropDatabase(clusterName, database, dropFiles);
        }

        public void CreateCollection(bool cluster, string database, CollectionConfiguration cc)
        {

            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("Database name not specified");

            if (cc == null)
                throw new Exception("CollectionConfiguration is null");
            _session.CreateCollection(clusterName, database, cc);

        }

        public void MoveCollection(bool cluster, string database, string collection, string newShard)
        {

            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("Database name not specified");

            if (string.IsNullOrWhiteSpace(collection))
                throw new Exception("Collection name not specified");

            if (string.IsNullOrWhiteSpace(newShard))
                throw new Exception("Shard name not specified");

            _session.MoveCollection(clusterName, database, collection, newShard);

        }

        public ClusterJobInfoObject[] GetAllRunningJobs()
        {   
            return _session.GetAllRunningJobs();
        }

        public void DropCollection(bool cluster, string database, string collectionName)
        {
            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("database name not specified.");

            if (string.IsNullOrWhiteSpace(collectionName))
                throw new Exception("collection name not specified.");

            _session.DropCollection(clusterName, database, collectionName);
        }

        public void CreateIndex(bool cluster, string database, string collection, IndexConfiguration configuration)
        {
            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("Database name not specified");

            if (string.IsNullOrWhiteSpace(collection))
                throw new Exception("collection name not specified");

            if (configuration == null)
                throw new Exception("index configuration is null");

            _session.CreateIndex(clusterName, database, collection, configuration);

        }

        public void DropIndex(bool cluster, string database, string collection, string indexName)
        {

            string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

            if (string.IsNullOrWhiteSpace(database))
                throw new Exception("Database name not specified");

            if (string.IsNullOrWhiteSpace(collection))
                throw new Exception("collection name not specified");

            if (string.IsNullOrWhiteSpace(indexName))
                throw new Exception("index name not specified");

            _session.DropIndex(clusterName, database, collection, indexName);

        }

        public bool IsRemoteClient()
        {
            try
            {
                return _session.IsRemoteClient();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void StartConfigurationServer(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("Config server must have some name.");

                _session.StartConfigurationServer(name);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void StopConfigurationServer(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("Config server must have some name.");

                _session.StopConfigurationServer(name);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void CreateConfigurationServer(ConfigServerConfiguration configuration, int heartBeat, ReplicationConfiguration repConfig, string displayName)
        {
            try
            {
                throw new NotImplementedException("Create Configuration Cluster. ConfigServer is now automatically managed");
                if (configuration == null)
                    throw new Exception("Config server configuration is null.");
                configuration.Name = MiscUtil.CONFIG_CLUSTER;

                if (
                    !EncryptionUtil.ValidateManagementTokenResponse(
                        _session.ValidateProfessional(EncryptionUtil.GetProfManagerToken())))
                {
                    throw new ManagementException("The operation is not supported due to edition mismatch");
                }
                _session.CreateConfigurationCluster(configuration, heartBeat, repConfig, displayName);


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool VerifyConfigurationClusterAvailability(string configClusterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configClusterName))
                    throw new Exception("Config server must have some name.");


                return _session.VerifyConfigurationClusterAvailability(configClusterName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool VerifyConfigurationCluster()
        {
            try
            {
                string configClusterName = MiscUtil.CLUSTERED;
                if (string.IsNullOrWhiteSpace(configClusterName))
                    throw new Exception("Config server must have some name.");


                return _session.VerifyConfigurationCluster(configClusterName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public bool VerifyConfigurationClusterUID(string Uid)
        {
            try
            {               
                return _session.VerifyConfigurationClusterUID(Uid);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public bool VerifyConfigurationServerAvailability()
        {
            try
            {

                return _session.VerifyConfigurationServerAvailability();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool VerifyConfigurationClusterPrimery(string configClusterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configClusterName))
                    throw new Exception("Config server must have some name.");


                return _session.VerifyConfigurationClusterPrimery(configClusterName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

       

        public void AddConfigurationListener(IConfigurationListener listioner)
        {
            try
            {
                _session.AddConfigurationListener(listioner);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IList<string> ListDatabases(bool cluster)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);
                return _session.ListDatabases(clusterName).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IList<string> ListCollections(bool cluster, string database)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

                if (string.IsNullOrEmpty(database))
                    throw new Exception("Database name not specified");

                return _session.ListCollections(clusterName, database).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IList<string> ListIndices(bool cluster, string database, string collection)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

                if (string.IsNullOrEmpty(database))
                    throw new Exception("Database name not specified");
                if (string.IsNullOrEmpty(collection))
                    throw new Exception("Collection name not specified");

                return _session.ListIndices(clusterName, database, collection).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void UpdateDatabaseConfiguration(bool cluster, string database, DatabaseConfiguration databaseConfiguration)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

                if (string.IsNullOrEmpty(database))
                    throw new Exception("Database name not specified");

                 _session.UpdateDatabaseConfiguration(clusterName, database, databaseConfiguration);
            }
            catch (Exception e)
            {
                throw e;
            }
        } 

        public void UpdateServerPriority(bool cluster, string shard, ServerNode server, int priority)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

                if (string.IsNullOrEmpty(shard))
                    throw new Exception("shard name not specified");

                _session.UpdateServerPriority(clusterName, shard, server, priority);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void UpdateDeploymentConfiguration(bool cluster, int heartBeatInterval)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);
                _session.UpdateDeploymentConfiguration(clusterName, heartBeatInterval);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

       

        

        public void UpdateCollectionConfiguration(bool cluster, string database, string collection, CollectionConfiguration collectionConfiguration)
        {
            try
            {
                string clusterName = (cluster ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);

                if (string.IsNullOrEmpty(database))
                    throw new Exception("Database name not specified");
                if (string.IsNullOrEmpty(collection))
                    throw new Exception("Collection name not specified");


                _session.UpdateCollectionConfiguration(clusterName, database, collection, collectionConfiguration);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void RemoveConfigurationCluster()
        {
            string configurationManagerName=MiscUtil.CLUSTERED;
            try
            {
                CheckIfAllCSServersAreRunning(configurationManagerName);
                ClusterConfiguration dbConfig = _session.GetDatabaseClusterConfiguration(configurationManagerName);
                
                ConfigServerConfiguration configServerConfiguration = _session.GetConfigurationClusterConfiguration(configurationManagerName);
                if (dbConfig != null && configServerConfiguration != null)
                {
                    foreach (var shardConfiguration in dbConfig.Deployment.Shards.Values)
                    {
                        foreach (var node in shardConfiguration.Servers.Nodes.Values)
                        {
                            OutProcConfigurationSession session = _session as OutProcConfigurationSession;
                            if (session != null && !session.Channel.PeerAddress.IpAddress.ToString().Equals(node.Name))
                            {
                                CheckAndRemoveConfigClusterNode(configurationManagerName, configServerConfiguration, node, false);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (Common.Logger.LoggerManager.Instance.ManagerLogger.IsErrorEnabled)
                {
                    Common.Logger.LoggerManager.Instance.ManagerLogger.Error("RemoveConfigurationCluster", "At RemoteConfigurationManager: " + e);
                }
                throw;
            }
            _session.RemoveConfigurationCluster(configurationManagerName);
        }

        private void CheckIfAllCSServersAreRunning(string configurationManagerName)
        {
            try
            {
                ConfigServerConfiguration configServerConfiguration = _session.GetConfigurationClusterConfiguration(configurationManagerName);

                if (configServerConfiguration != null)
                {
                    foreach (var server in configServerConfiguration.Servers.Nodes.Values)
                    {
                        OutProcConfigurationSession session = _session as OutProcConfigurationSession;
                        if (session != null && !session.Channel.PeerAddress.IpAddress.ToString().Equals(server.Name))
                        {
                            var newSession = GetNewSession(server.Name, _port, session.ClientCredentials);

                            if (newSession != null)
                            {
                                newSession.Dispose();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("NosDB configuration service is not running on one or more database servers", e);
            }


        }
        #region Recovery Operations
        public RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config)
        {
            try
            {
                if (config == null)
                    throw new Exception("Provided configuration is empty");

                return _session.SubmitRecoveryJob(config);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public RecoveryOperationStatus CancelRecoveryJob(string identifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                    throw new Exception("Job Identifier cannot be null");

                return _session.CancelRecoveryJob(identifier);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public RecoveryOperationStatus[] CancelAllRecoveryJobs()
        {
            try
            {

                return _session.CancelAllRecoveryJobs();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ClusteredRecoveryJobState GetJobState(string identifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                    throw new Exception("Job Identifier cannot be null");

                return _session.GetJobState(identifier);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

      
        #endregion

        #region Security
        public bool Grant(bool clustered, ResourceId resourceId, string userName, string roleName)
        {
            bool isSuccessful = false;
            try
            {
                string clusterName = (clustered ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);
                if (resourceId.ResourceType == Common.Security.Impl.Enums.ResourceType.Database && !resourceId.Name.Contains(clusterName.ToLower() + "/"))
                    resourceId.Name = clusterName.ToLower() + "/" + resourceId.Name;
                isSuccessful = _session.Grant(clusterName, resourceId, userName, roleName);
            }
            catch(Exception exc)
            {
                throw exc;
            }
            return isSuccessful;
        }

        public bool Revoke(bool clustered, ResourceId resourceId, string userName, string roleName)
        {
            bool isSuccessful = false;
            try
            {
                string clusterName = (clustered ? MiscUtil.CLUSTERED : MiscUtil.LOCAL);
                if (resourceId.ResourceType == Common.Security.Impl.Enums.ResourceType.Database && !resourceId.Name.Contains(clusterName.ToLower() + "/"))
                    resourceId.Name = clusterName.ToLower() + "/" + resourceId.Name;
                isSuccessful = _session.Revoke(clusterName, resourceId, userName, roleName);
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return isSuccessful;
        }

        public bool CreateUser(IUser userInfo)
        {
            bool isSuccessful = false;
            try
            {
                isSuccessful = _session.CreateUser(userInfo);
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return isSuccessful;
        }

        public bool DropUser(IUser userInfo)
        {
            bool isSuccessful = false;
            try
            {
                isSuccessful = _session.DropUser(userInfo);
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return isSuccessful;
        }

        /// <summary>
        /// this method is specific to security
        /// </summary>
        /// <returns></returns>
        public IList<IResourceItem> GetResourcesSecurityInfo(string cluster)
        {
            return _session.GetResourcesInformation(cluster);
        }

        /// <summary>
        /// this method is specific to security
        /// </summary>
        /// <returns></returns>
        public IResourceItem GetResourceSecurityInfo(string cluster, ResourceId resourceId)
        {
            return _session.GetResourceSecurityInfo(cluster, resourceId);
        }

        public IDictionary<string, IList<string>> GetUsersRoleSet(string cluster, ResourceId resourceId)
        {
            IResourceItem resourceItem = GetResourceSecurityInfo(cluster, resourceId);
            IDictionary<string, IList<string>> usersRoleSet = ResourceItem.GetUserInfo(resourceItem);
            return usersRoleSet;
        }

        public IList<IUser> GetLogins()
        {
            return _session.GetUsersInformation();
        }

        public IDictionary<IRole, IList<ResourceId>> GetUserInfo(IUser userInfo)
        {
            return _session.GetUserInfo(userInfo);
        }

       

        #endregion

        public List<Address> GetDataBaseServerNode()
        {
            return _session.GetDataBaseServerNode();
        }
        public bool SetDatabaseMode(string clusterName, string databaseName,DatabaseMode databaseMode)
        {
            return _session.SetDatabaseMode(clusterName, databaseName, databaseMode);
        }
        public IDictionary<string, byte[]> GetDeploymentSet(string deploymentID)
        {
            try
            {
                return _session.GetDeploymentSet(deploymentID);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region Config Cluster Management Code

        /// <summary>
        /// Adds the node to config cluster if the node is not already part of config cluster AND config cluster max node limit is not reached
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="configServerConfiguration"></param>
        /// <param name="node"></param>
        private void CheckAndCreateNewConfigServer(string cluster, ConfigServerConfiguration configServerConfiguration, ServerNode node)
        {
            if (configServerConfiguration.Servers.Nodes.Count < MiscUtil.MAX_CS_LIMIT)
            {
                if (!configServerConfiguration.Servers.ContainsNode(node.Name))
                {
                    ConfigSessionParams configSessionParams = GetNewSession(node.Name, configServerConfiguration.Port, ((OutProcConfigurationSession)_session).ClientCredentials);
                    if (configSessionParams.Session.VerifyConfigurationClusterAvailability(cluster))
                    {
                        ServerNode server = node.Clone() as ServerNode;
                        if (configServerConfiguration.Servers != null && configServerConfiguration.Servers.Nodes.Count == 0)
                        {
                            server.Priority = 1;
                        }
                        else
                        {
                            server.Priority = 2;
                        }
                        //configServerConfiguration.Servers.Nodes.First().Value.Priority = 1;
                        //_session.UpdateConfigServerNodePriority(cluster, node.Name, 1);
                        configServerConfiguration.Servers.AddNode(server);
                        ClusterConfiguration config = _session.GetDatabaseClusterConfiguration(cluster);
                        configSessionParams.Session.CreateConfigurationCluster(configServerConfiguration, config.Deployment.HeartbeatInterval, config.Deployment.Replication, config.DisplayName);
                        _session.AddNodeToConfigurationCluster(cluster, server);
                        ((OutProcConfigurationSession)_session).DetermineSecondaryConfigurationServer();
                    }

                    configSessionParams.Dispose();
                }
            }
        }

        /// <summary>
        /// If config cluster does not have max number of nodes then this methods check if any node in database cluster can be made part of config cluster.
        /// The process continues until config cluster is full or shard nodes finish 
        /// </summary>
        /// <param name="configServerConfiguration"></param>
        /// <param name="clusterConfiguration"></param>
        /// <param name="removedNode"></param>
        private void SelectNewNodeForConfigServer(ConfigServerConfiguration configServerConfiguration, ClusterConfiguration clusterConfiguration, ServerNode removedNode)
        {
            if (configServerConfiguration.Servers.Nodes.Count < MiscUtil.MAX_CS_LIMIT)
            {
                foreach (var shard in clusterConfiguration.Deployment.Shards.Values)
                {
                    foreach (var node in shard.Servers.Nodes.Values)
                    {
                        if (node.Equals(removedNode)) continue;
                        CheckAndCreateNewConfigServer(clusterConfiguration.Name, configServerConfiguration, node);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a node from config cluster if it can be removed without any issue
        /// Also select new nodes for config server (if required)
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="configServerConfiguration"></param>
        /// <param name="nodeRemoved"></param>
        private void CheckAndRemoveConfigClusterNode(string cluster, ConfigServerConfiguration configServerConfiguration, ServerNode nodeRemoved, bool shiftConfigurationServer = true)
        {
            if (configServerConfiguration.Servers.ContainsNode(nodeRemoved.Name))
            {
                ClusterConfiguration clusterConfiguration = _session.GetDatabaseClusterConfiguration(cluster);
                if (shiftConfigurationServer && IsNodePartOfMultipleShards(nodeRemoved, clusterConfiguration)) return;

                if (configServerConfiguration.Servers.Nodes.Count < 2) throw new Exception("Cannot remove node '" + nodeRemoved.Name + " because it is the last node of config cluster");

                if (!CanRemoveNodeFromConfigCluster(configServerConfiguration, nodeRemoved)) throw new Exception("Cannot remove node because config cluster is not synced at the moment. Try again later");

                RemoveNodeFromConfigCluster(cluster, configServerConfiguration, nodeRemoved);
                if (shiftConfigurationServer)
                {
                    ConfigServerConfiguration configConfiguration = _session.GetConfigurationClusterConfiguration(cluster);
                    SelectNewNodeForConfigServer(configConfiguration, clusterConfiguration, nodeRemoved);
                }
            }
        }

        /// <summary>
        /// Removes the node from config cluster and also informs the other nodes in cluster about it
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="configServerConfiguration"></param>
        /// <param name="nodeRemoved"></param>
        private void RemoveNodeFromConfigCluster(string cluster, ConfigServerConfiguration configServerConfiguration, ServerNode nodeRemoved)
        {
            foreach (var node in configServerConfiguration.Servers.Nodes.Values)
            {
                if (node.Name.Equals(_configServerIp))
                {
                    _session.RemoveNodeFromConfigurationCluster(cluster, nodeRemoved);
                    if (node.Name.Equals(nodeRemoved.Name))
                        Dispose();
                }
                else
                {
                    ConfigSessionParams configSessionParams = GetNewSession(node.Name, configServerConfiguration.Port, ((OutProcConfigurationSession)_session).ClientCredentials);
                    configSessionParams.Session.RemoveNodeFromConfigurationCluster(cluster, nodeRemoved);

                    if (!node.Equals(nodeRemoved))
                    {
                        if (node.Priority > 1)
                        {
                            configSessionParams.Session.UpdateConfigServerNodePriority(cluster, configSessionParams.ConfigServerIp, 1);
                        }
                        Initialize(configSessionParams);
                    }
                    else
                    {
                        configSessionParams.Dispose();
                    }
                }
            }
            //configServerConfiguration.Servers.RemoveNode(nodeRemoved.Name);
            ((OutProcConfigurationSession)_session).DetermineSecondaryConfigurationServer();
        }

        /// <summary>
        /// Checks if node is part of multiple shards or not
        /// </summary>
        /// <param name="node"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private bool IsNodePartOfMultipleShards(ServerNode node, ClusterConfiguration config)
        {
            int count = 0;
            foreach (var shard in config.Deployment.Shards.Values)
            {
                foreach (var shardNode in shard.Servers.Nodes.Values)
                {
                    if (shardNode.Equals(node))
                    {
                        count++;
                        if (count > 1)
                            return true;
                    }
                }
            }
            return count > 1;
        }

        private bool CanRemoveNodeFromConfigCluster(ConfigServerConfiguration configServerConfiguration, ServerNode removeNode)
        {
            if (configServerConfiguration.Servers.Nodes.Count < 2) throw new Exception("Cannot remove node because it is the last node of config cluster");

            IConfigurationSession session;
            ConfigSessionParams configSessionParams = null;

            foreach (var node in configServerConfiguration.Servers.Nodes.Values)
            {
                if (removeNode.Name.Equals(node.Name)) continue;

                try
                {
                    if (node.Name.Equals(_configServerIp))
                    {
                        session = _session;
                    }
                    else
                    {
                        configSessionParams = GetNewSession(node.Name, _port, ((OutProcConfigurationSession)_session).ClientCredentials);
                        session = configSessionParams.Session;
                    }

                    if (session.HasSynchronizedWithPrimaryConfigServer())
                    {
                        return true;
                    }
                }
                finally
                {
                    if (configSessionParams != null)
                    {
                        configSessionParams.Dispose();
                        configSessionParams = null;
                    }
                }
            }
            return false;
        }

        private bool IsNodePartOfDatabaseCluster(ServerNode node, ClusterConfiguration databaseClusterConfiguration)
        {
            if(databaseClusterConfiguration == null || databaseClusterConfiguration.Deployment == null)
                return false;

            foreach (var shardConfiguration in databaseClusterConfiguration.Deployment.Shards.Values)
            {
                foreach (var shardNode in shardConfiguration.Servers.Nodes.Values)
                {
                    if (node.Name.Equals(shardNode.Name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ValidateDatabaseCluster(string cluster, ClusterConfiguration clusterConfiguration, ConfigServerConfiguration configServerConfiguration)
        {
            if (clusterConfiguration == null || clusterConfiguration.Deployment == null || clusterConfiguration.Deployment.Shards == null || clusterConfiguration.Deployment.Shards.Count == 0)
            {
                if (configServerConfiguration != null && configServerConfiguration.Servers != null && configServerConfiguration.Servers.Nodes.Count > 0)
                {
                    IClientAuthenticationCredential authenticationCredential = ((OutProcConfigurationSession)_session).ClientCredentials;
                    foreach (var configServerNode in configServerConfiguration.Servers.Nodes.Values)
                    {
                        if (!configServerNode.Name.Equals(((OutProcConfigurationSession)_session).Channel.PeerAddress.IpAddress.ToString()))
                        {
                            CheckAndRemoveConfigClusterNode(configServerConfiguration.Name, configServerConfiguration, configServerNode, false);
                        }
                    }
                    _session.RemoveConfigurationCluster(cluster);
                    Dispose();
                }
                return false;
            }
            return true;
        }

        private void ValidateConfigurationCluster(string cluster)
        {
            ConfigServerConfiguration configServerConfiguration = _session.GetConfigurationClusterConfiguration(cluster);
            ClusterConfiguration clusterConfiguration = _session.GetDatabaseClusterConfiguration(cluster);

            //TODO: Validate ConfigurationClusterConfiguration as well
            if (clusterConfiguration == null)   //Temporary Check
                return;
            if(!ValidateDatabaseCluster(cluster, clusterConfiguration, configServerConfiguration))
                return;

            foreach (var node in configServerConfiguration.Servers.Nodes.Values)
            {
                if (!IsNodePartOfDatabaseCluster(node, clusterConfiguration))
                {
                    CheckAndRemoveConfigClusterNode(cluster, configServerConfiguration, node, false);
                }
            }
        }

        #endregion

    }
}
