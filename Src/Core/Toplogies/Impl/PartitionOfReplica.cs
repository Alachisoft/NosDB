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
using System.IO;
using System.Linq;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Storage.Provider;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Statistics;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Stats;
using System.Collections;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Net;
using System.Threading;
using Alachisoft.NosDB.Common.Logger;
using Newtonsoft.Json;
using CollectionConfigurations = Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfigurations;
using LMDBConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.LMDBConfiguration;
using StorageConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.StorageConfiguration;
using StorageProviderConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.StorageProviderConfiguration;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Core.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using MiscUtil = Alachisoft.NosDB.Core.Util.MiscUtil;
using Alachisoft.NosDB.Core.Security.Impl;

using ShardInfo = Alachisoft.NosDB.Common.Configuration.Services.ShardInfo;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using JsonSerializer = Alachisoft.NosDB.Common.JsonSerializer;
using Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Core.Toplogies.Impl
{
    public class PartitionOfReplica : IDatabaseTopology, IClusterListener, IConfigurationListener,IDispatcher
    {
        NodeContext _context = null;
        private ICluster _cluster = null;
        private NodeStateTranferManager nodeStateTxfrMgr = null;
       
        private IDatabasesManager _databasesManager = null;
        private IDictionary<String, IDictionary<String, IDistribution>> _distributionMaps = new Dictionary<string, IDictionary<string, IDistribution>>(StringComparer.InvariantCultureIgnoreCase);
        /// <summary> The runtime status of this node. </summary>
        internal Latch _statusLatch = new Latch();
      
        private ShardRecoveryManager _recoveryManager;

        Thread _getClusterConfig;
        private readonly object _getConfigLock = new object();

        private readonly object _storeConfigLock = new object();
        
        private PrimaryAccess _currentPrimaryRole = PrimaryAccess.UnrestrictedAccess;
       

        private Object _onPrimaryRole = new Object();        

        public NodeContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public ICluster Cluster
        {
            get { return _cluster; }
        }

        public NodeRole NodeRole
        {
            get
            {
                if (Cluster != null)
                    return Cluster.ShardNodeRole;
                return NodeRole.None;
            }
        }

        public PartitionOfReplica(NodeContext context)
        {
            _getClusterConfig = new Thread(new ThreadStart(GetClusterConfAndStoreTask));

            _getClusterConfig.Name = "GetClusterConf";

            this._context = context;
            _databasesManager = context.DatabasesManager;
            
        }

        public IDBResponse InitDatabase(InitDatabaseOperation initOperation)
        {
            var response = initOperation.CreateResponse() as InitDatabaseResponse;
            try
            {
                if (response != null)
                    response.IsInitialized = _databasesManager.InitDatabase(initOperation.Database);
            }
            catch (DatabaseException databaseException)
            {
                if (response != null)
                {
                    response.IsInitialized = false;
                    response.IsSuccessfull = false;
                    response.ErrorCode = databaseException.ErrorCode;
                    response.ErrorParams = databaseException.Parameters;
                }
            }
            return response;
        }



        #region IDatabaseTopology Implementation

        public bool Initialize(ClusterConfiguration configuration)
        {
            if (configuration == null) return false;

            Boolean isClusterInitialized = false;

            _context.StatusLatch.SetStatusBit(NodeStatus.Initializing, NodeStatus.Stopped);

            if (configuration.Deployment != null)
            {
                if (_cluster == null)
                    _cluster = new ClusterManager(_context);

                isClusterInitialized = _cluster.Initialize(configuration);

                if (!_context.ClusterName.Equals(Common.MiscUtil.LOCAL, StringComparison.OrdinalIgnoreCase))
                {
                    nodeStateTxfrMgr = new NodeStateTranferManager(_context, this);
                }
            }

            Thread dbInitializer = new Thread(InitializeDatabases);

            dbInitializer.Name = "DbInitializer";
            dbInitializer.Start(configuration);

            if (isClusterInitialized)
            {
                _cluster.RegisterClusterListener(this);
                _cluster.Start();
                //_cluster.RegisterClusterListener(this);
                _cluster.RegisterConfigChangeListener(this);
            }


            _recoveryManager = new ShardRecoveryManager(_context);
            return true;
        }

        private void InitializeDatabases(object param)
        {
            try
            {
                LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = "" });
                ClusterConfiguration newClusterconf = (ClusterConfiguration)param;
                DatabaseConfiguration sysDbConfig = GetSystemDatabaseConfiguration(newClusterconf);

                sysDbConfig.Storage.Collections.Configuration = MiscUtil.GetSystemCollections(_context.LocalShardName);
                //sysDbConfig.Storage.Collections.Configuration[0] = _replication.GetCollectionConfiguration();

                //lengthyTask
                if (!_databasesManager.CreateSystemDatabase(sysDbConfig, _context))
                    throw new InitializeException("error while initialization of Systemdatabase");

                GetLatestDistributionMap();

                _getClusterConfig.Start();
                //get OldClusterConf maybe null if node is starting for the first time.
                ClusterConfiguration oldClusterConf = GetClusterConf(newClusterconf.Name);
                if (oldClusterConf == null)
                    oldClusterConf = newClusterconf;

                if (oldClusterConf.Databases != null)
                {
                    Boolean initialized = _databasesManager.Initialize(oldClusterConf.Databases, _context,_distributionMaps);

                    
                    if (!initialized)
                        throw new InitializeException("error while initialization of databases");

                   
                }

                TrySynchronizeClusterConf(newClusterconf);


                GetLatestDistributionMap();
                SetDistributionMaps();

                //TODO: this is a temporary fix, after implementing event for database initialize completion , this needs to be moved there, 
                DbSSecurityDatabase securityDatabase = new DbSSecurityDatabase();
                securityDatabase.Initialize(_databasesManager);
                _context.SecurityManager.AddSecurityDatabase(_context.LocalShardName, securityDatabase);
                ConfigSecurityServer configSecurityServer = new ConfigSecurityServer();
                configSecurityServer.Initialize(_context.ConfigurationSession);
                configSecurityServer.ClusterName = _context.ClusterName;
                _context.SecurityManager.SecurityServer = configSecurityServer;
                _context.SecurityManager.InitializeSecurityInformation(_context.LocalShardName);
                //Getting security information from configuration server
                IList<IUser> users = _context.ConfigurationSession.GetUsersInformation();
                IList<IResourceItem> resources = _context.ConfigurationSession.GetResourcesInformation(_context.ClusterName);
                _context.SecurityManager.PopulateSecurityInformation(_context.LocalShardName, resources, users);

                _context.StatusLatch.SetStatusBit(NodeStatus.Running, NodeStatus.Initializing);

              
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                    LoggerManager.Instance.ServerLogger.Info("DB initialization complete", "Setting node status to running. Cluster = " + _context.ClusterName + " Shard = " + _context.LocalShardName);

            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("Error Initializing databases", "Shutting Down Node. Cluster = " + _context.ClusterName + " Shard = " + _context.LocalShardName + " " + e);
                Dispose();
                //throw;
            }
        }

        #region Synchronization Code
        private void TrySynchronizeClusterConf(ClusterConfiguration newClusterConf)
        {
            ClusterConfiguration oldClusterConf = GetClusterConf(newClusterConf.Name);

            if (oldClusterConf == null)
            {
                //insert its new
                JSONDocument serializedNewConf = JsonSerializer.Serialize<ClusterConfiguration>(newClusterConf);
                serializedNewConf.Key = newClusterConf.Name;

                _databasesManager.InsertDocuments(new InsertDocumentsOperation()
                {
                    Database = Common.MiscUtil.SYSTEM_DATABASE,
                    Collection = MiscUtil.SystemCollection.ConfigCollection,
                    Documents = new IJSONDocument[] { serializedNewConf }
                });
            }
            else
            {
                //SYnc
                SynchronizeCluster(oldClusterConf, newClusterConf);
            }
        }

        public ClusterConfiguration InitializeAndGetClusterConf(ClusterConfiguration clusterConf)
        {
            DatabaseConfiguration sysDbConfig = GetSystemDatabaseConfiguration(clusterConf);

            sysDbConfig.Storage.Collections.AddCollection(MiscUtil.GetConfigCollectionConfig(_context.LocalShardName));

            if (!_databasesManager.CreateSystemDatabase(sysDbConfig, _context))
                throw new InitializeException("error while initialization of Systemdatabase");

            return GetClusterConf(_context.ClusterName.ToLower());
        }

        public ClusterConfiguration GetClusterConf(string clusterName)
        {
            IGetResponse response = _databasesManager.GetDocuments(new GetDocumentsOperation()
            {
                Database = Common.MiscUtil.SYSTEM_DATABASE,
                Collection = MiscUtil.SystemCollection.ConfigCollection,
                DocumentIds = new List<IJSONDocument>() { new JSONDocument() { Key = clusterName } }
            });
            if (response.DataChunk.Documents.Count <= 0)
                return null;

            //assumption only 1 document will return issue if document count is greater than one.
            return JsonSerializer.Deserialize<ClusterConfiguration>(response.DataChunk.Documents[0]);
        }

        /// <summary>
        /// Gets Latest ClusterConf from ConfigServer and stores in SysDB
        /// The method will be replaced by indivisual operations afterwards
        /// </summary>
        internal void PulseGetClusterConfAndStore()
        {
            lock (_getConfigLock)
            {
                Monitor.Pulse(_getConfigLock);
            }
        }

        private void GetClusterConfAndStoreTask()
        {
            while (true)
            {
                try
                {
                    lock (_getConfigLock)
                    {
                        Monitor.Wait(_getConfigLock);

                    }
                    //use seperate lock as the call is synchronous and creates a deadlock
                    //plus the thread to enter first should store first for proper synchronyzation
                    lock (_storeConfigLock)
                    {

                        //todo: this is not an optimized method and will be replaced in each operation seperately
                        ClusterConfiguration clusterConf = _context.ConfigurationSession.GetDatabaseClusterConfiguration(Context.ClusterName);
                        InsertOrUpdateClusterConf(clusterConf);
                    }
                }
                catch (ThreadAbortException)
                {
                    //todo: log maybe in debug
                    return;
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                        LoggerManager.Instance.ServerLogger.Error("Error getting and storing config.", ex);
                }
            }
        }

        private void InsertOrUpdateClusterConf(ClusterConfiguration clusterConf)
        {
            IGetResponse response = _databasesManager.GetDocuments(new GetDocumentsOperation()
            {
                Database = Common.MiscUtil.SYSTEM_DATABASE,
                Collection = MiscUtil.SystemCollection.ConfigCollection,
                DocumentIds = new List<IJSONDocument>() { new JSONDocument() { Key = Context.ClusterName } }
            });

            JSONDocument serializedNewConf = JsonSerializer.Serialize<ClusterConfiguration>(clusterConf);
            serializedNewConf.Key = clusterConf.Name;

            if (response.DataChunk.Documents.Count <= 0)    //insert
            {
                _databasesManager.InsertDocuments(new InsertDocumentsOperation()
                {
                    Database = Common.MiscUtil.SYSTEM_DATABASE,
                    Collection = MiscUtil.SystemCollection.ConfigCollection,
                    Documents = new IJSONDocument[] { serializedNewConf }
                });
            }
            else //update
            {
                _databasesManager.ReplaceDocuments(new ReplaceDocumentsOperation()
                {
                    Database = Common.MiscUtil.SYSTEM_DATABASE,
                    Collection = MiscUtil.SystemCollection.ConfigCollection,
                    Documents = new IJSONDocument[] { serializedNewConf }
                });
            }
        }

        private void SynchronizeCluster(ClusterConfiguration oldClusterConf, ClusterConfiguration newClusterConf)
        {
            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Started " + DateTime.Now.ToString());

            //Drop Databases that are not present in new Config
            foreach (KeyValuePair<string, DatabaseConfiguration> database in oldClusterConf.Databases.Configurations)
            {
                //if database does not exist in newConfig
                if (!newClusterConf.Databases.ContainsDatabase(database.Key))
                {
                    if (LoggerManager.Instance.StorageLogger != null &&
                        LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        LoggerManager.Instance.StorageLogger.Info("Database synchronization",
                            "Dropping database = " + database.Key + " reason = Does not Exist in New Config.");
                    DropDatabase(database.Key, true);
                }
                //else if UID doesnt match
                else if (newClusterConf.Databases.GetDatabase(database.Key).UID != database.Value.UID)
                {
                    if (LoggerManager.Instance.StorageLogger != null &&
                        LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        LoggerManager.Instance.StorageLogger.Info("Database synchronization",
                            "Dropping database = " + database.Key + " reason = UID does not match. New Uid = " +
                            newClusterConf.Databases.GetDatabase(database.Key).UID + " Old Uid = " + database.Value.UID);
                    DropDatabase(database.Key, true);
                }
                //else iterate inner object
                else
                {
                    DatabaseConfiguration newDbConf = newClusterConf.Databases.GetDatabase(database.Key);
                
                    //Drop collections that are not present in new Config
                    foreach (KeyValuePair<string, CollectionConfiguration> collection in database.Value.Storage.Collections.Configuration)
                    {
                        //if collection does not exist in newConfig
                        if (!newDbConf.Storage.Collections.ContainsCollection(collection.Key))
                        {
                            if (LoggerManager.Instance.StorageLogger != null &&LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                                LoggerManager.Instance.StorageLogger.Info("Database synchronization",    "Dropping collection = " + collection.Key +" reason = Does not Exist in New Config.");
                            DropCollection(new DropCollectionOperation()
                            {
                                Database = database.Key,
                                Collection = collection.Key
                            });
                        }
                        //else if UID doesnt match
                        else if (newDbConf.Storage.Collections.GetCollection(collection.Key).UID != collection.Value.UID)
                        {
                            if (LoggerManager.Instance.StorageLogger != null &&LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                                LoggerManager.Instance.StorageLogger.Info("Database synchronization",    "Dropping collection = " + collection.Key +" reason = UID does not match. New Uid = " +
                                    newDbConf.Storage.Collections.GetCollection(collection.Key).UID + " Old Uid = " +
                                    collection.Value.UID);
                            DropCollection(new DropCollectionOperation()
                            {
                                Database = database.Key,
                                Collection = collection.Key
                            });
                        }
                        //else iterate inner object
                        else
                        {
                            CollectionConfiguration newColConf = newDbConf.Storage.Collections.GetCollection(collection.Key);
                            if (collection.Value.Indices == null)
                                continue;
                            //Drop indices that are not present in new Config
                            foreach (KeyValuePair<string, IndexConfiguration> index in collection.Value.Indices.IndexConfigurations)
                            {
                                //if index does not exist in newConfig
                                if (!newColConf.Indices.ContainsIndexWithUID(index.Value.UID))
                                {
                                    if (LoggerManager.Instance.StorageLogger != null &&LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                                        LoggerManager.Instance.StorageLogger.Info("Database synchronization",    "Dropping index = " + index.Key +" reason = Uid does not exist in New Config. Uid = " + index.Value.UID);
                                    DropIndex(new DropIndexOperation()
                                    {
                                        Database = database.Key,
                                        Collection = collection.Key,
                                        IndexName = index.Key,
                                    });
                                }
                                else
                                {
                                    IndexConfiguration newIndexConf = newColConf.Indices.GetIndexWithUID(index.Value.UID);
                                    //if Name doesnt Match Call Rename
                                    if (!newIndexConf.IndexName.Equals(index.Value.IndexName))
                                    {
                                        RenameIndex(new RenameIndexOperation()
                                        {
                                            Database = database.Key,
                                            Collection = collection.Key,
                                            OldIndexName = index.Value.IndexName,
                                            NewIndexName = newIndexConf.IndexName,
                                        });
                                    }
                                    //if Order doesnt Match Call reorder

                                    if (!newIndexConf.Attributes.Name.Equals(index.Value.Attributes.Name) || !newIndexConf.Attributes.SortOrder.Equals(index.Value.Attributes.SortOrder))
                                    {
                                        RecreateIndex(new RecreateIndexOperation()
                                        {
                                            Database = database.Key,
                                            Collection = collection.Key,
                                            Configuration = newIndexConf,
                                        });
                                        break;
                                    }

                                }
                            }
                        }
                    }
                }
            }

            //Create Databases that are not present in oldConfig
            foreach (KeyValuePair<string, DatabaseConfiguration> database in newClusterConf.Databases.Configurations)
            {
                //if database does not exist in oldConfig
                if (!oldClusterConf.Databases.ContainsDatabase(database.Key))
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Creating database = " + database.Key + " reason = Does not Exist in Old Config.");
                    CreateDatabase(database.Value, null);
                }
                //else if UID doesnt match
                else if (oldClusterConf.Databases.GetDatabase(database.Key).UID != database.Value.UID)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Creating database = " + database.Key + " reason = UID does not match. New Uid = " + database.Value.UID + " Old Uid = " + oldClusterConf.Databases.GetDatabase(database.Key).UID);
                    CreateDatabase(database.Value, null);
                }
                //else iterate inner object
                else
                {
                    DatabaseConfiguration oldDbConf = oldClusterConf.Databases.GetDatabase(database.Key);
                    //Create collections that are not present in oldConfig
                    foreach (KeyValuePair<string, CollectionConfiguration> collection in database.Value.Storage.Collections.Configuration)
                    {
                        //if collection does not exist in oldConfig
                        if (!oldDbConf.Storage.Collections.ContainsCollection(collection.Key))
                        {
                            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                                LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Creating collection = " + collection.Key + " reason = Does not Exist in Old Config.");
                            CreateCollection(new CreateCollectionOperation()
                            {
                                Database = database.Key,
                                Configuration = collection.Value
                            });
                        }
                        //else if UID doesnt match
                        else if (oldDbConf.Storage.Collections.GetCollection(collection.Key).UID != collection.Value.UID)
                        {
                            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                                LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Creating collection = " + collection.Key + " reason = UID does not match. New Uid = " + collection.Value.UID + " Old Uid = " + oldDbConf.Storage.Collections.GetCollection(collection.Key).UID);
                            CreateCollection(new CreateCollectionOperation()
                            {
                                Database = database.Key,
                                Configuration = collection.Value
                            });
                        }
                        //else iterate inner object
                        else
                        {
                            CollectionConfiguration oldColConf = oldDbConf.Storage.Collections.GetCollection(collection.Key);
                           
                            if (collection.Value.Indices == null)
                                continue;

                            //Create indices that are not present in oldConfig
                            foreach (KeyValuePair<string, IndexConfiguration> index in collection.Value.Indices.IndexConfigurations)
                            {
                                //if index does not exist in oldConfig
                                if (!oldColConf.Indices.ContainsIndexWithUID(index.Value.UID))
                                {
                                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                                        LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Creating index = " + index.Key + " reason = Uid does not exist in Old Config. Uid = " + index.Value.UID);
                                    CreateIndex(new CreateIndexOperation()
                                    {
                                        Database = database.Key,
                                        Collection = collection.Key,
                                        Configuration = index.Value
                                    });
                                }
                                else
                                {
                                    //case handeled above
                                }
                            }
                        }
                    }
                }
            }

            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                LoggerManager.Instance.StorageLogger.Info("Database synchronization", "Completed Successfully " + DateTime.Now.ToString());
        }
        #endregion

        /// <summary>
        /// Get Latest Cluster Information form Config server and populate local Distribution Map From that info
        /// </summary>
        private void GetLatestDistributionMap()
        {
            if (this._context != null && _context.ConfigurationSession != null)
            {
                ClusterInfo clusterInfo = _context.ConfigurationSession.GetDatabaseClusterInfo(_context.ClusterName);
                if (clusterInfo != null)
                {
                    if (clusterInfo.Databases != null && clusterInfo.Databases.Count > 0)
                    {
                        lock (_distributionMaps)
                        {
                            foreach (DatabaseInfo dbInfo in clusterInfo.Databases.Values)
                            {
                                if (!_distributionMaps.ContainsKey(dbInfo.Name))
                                {
                                    _distributionMaps[dbInfo.Name] = new Dictionary<String, IDistribution>();
                                }

                                if (dbInfo.Collections != null)
                                {
                                    foreach (CollectionInfo colInfo in dbInfo.Collections.Values)
                                    {
                                        if (colInfo.DataDistribution != null)
                                            _distributionMaps[dbInfo.Name][colInfo.Name] = colInfo.DataDistribution;
                                        else
                                        {
                                            if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                                                LoggerManager.Instance.ServerLogger.Fatal("GetLatestDistributionMap", "No distribution found for collection " + dbInfo.Name + "." + colInfo.Name);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetDistributionMaps()
        {
            _databasesManager.DistributionMap = _distributionMaps;
        }

        private String GetDestinationShard(String dbName, String colName, DocumentKey key)
        {
            try
            {
                IDistribution dist = null;
                lock (_distributionMaps)
                {
                    if (_distributionMaps.ContainsKey(dbName) && _distributionMaps[dbName].ContainsKey(colName))
                        dist = _distributionMaps[dbName][colName];
                }

                if (dist != null && dist.GetDistributionRouter() != null)
                {
                    return dist.GetDistributionRouter().GetShardForDocument(key);
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        public bool CreateDatabase(DatabaseConfiguration configuration, IDictionary<string, IDistributionStrategy> collectionStrategy)
        {
            Common.MiscUtil.IsArgumentNull(configuration);
            IDictionary<string, IDistribution> colDistributions;
            _distributionMaps.TryGetValue(configuration.Name, out colDistributions);
            // set 
            if (collectionStrategy != null)
            {
                if (collectionStrategy.Count > 0)
                {
                    if (colDistributions == null)
                        colDistributions = new Dictionary<string, IDistribution>();

                    foreach (KeyValuePair<string, IDistributionStrategy> kvp in collectionStrategy)
                    {
                        if (colDistributions.ContainsKey(kvp.Key))
                        {
                            colDistributions[kvp.Key] = kvp.Value.Distribution;
                        }
                        else
                            colDistributions.Add(kvp.Key, kvp.Value.Distribution);
                    }
                }
            }
            bool response = _databasesManager.CreateDatabase(configuration, _context, colDistributions);
            if (response)
                PulseGetClusterConfAndStore();
            //if (_cluster.ShardNodeRole == Common.Configuration.Services.NodeRole.Primary)
            //    _databasesManager.StartBucketInfoTask();
            return response;

        }



        /// <summary>
        /// Create Collection on specified database
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public Common.Server.Engine.IDBResponse CreateCollection(Common.Server.Engine.ICreateCollectionOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            IDBResponse response = _databasesManager.CreateCollection(operation);
            if (response.IsSuccessfull)
                PulseGetClusterConfAndStore();
            return response;
        }

        /// <summary>
        /// Create Index on specified collection
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public Common.Server.Engine.IDBResponse CreateIndex(Common.Server.Engine.ICreateIndexOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            IDBResponse response = _databasesManager.CreateIndex(operation);
            if (response.IsSuccessfull)
                PulseGetClusterConfAndStore();
            return response;
        }

        public Common.Server.Engine.IDBResponse RenameIndex(Common.Server.Engine.IRenameIndexOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            IDBResponse response = _databasesManager.RenameIndex(operation);
            if (response.IsSuccessfull)
                PulseGetClusterConfAndStore();
            return response;
        }

        public Common.Server.Engine.IDBResponse RecreateIndex(Common.Server.Engine.IRecreateIndexOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            IDBResponse response = _databasesManager.RecreateIndex(operation);
            if (response.IsSuccessfull)
                PulseGetClusterConfAndStore();
            return response;
        }

        public Common.Server.Engine.IDocumentsWriteResponse DeleteDocuments(Common.Server.Engine.IDocumentsWriteOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            if (!(this.NodeRole == NodeRole.Primary))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Operation not supported on secondary node");

            if (_currentPrimaryRole.Equals(PrimaryAccess.RestrictedAccess))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Primary access restricted.");
            String dbName = operation.Database;
            String colName = operation.Collection;
            IDictionary<string, int> failedDocuments = new HashVector<string, int>();

            Common.Server.Engine.IDocumentsWriteResponse response = _databasesManager.DeleteDocuments(operation);

            if (failedDocuments.Count > 0)
            {
                response.IsSuccessfull = false;

                foreach (var pair in failedDocuments)
                {
                    FailedDocument failedDocument = new FailedDocument();
                    failedDocument.DocumentKey = pair.Key;
                    failedDocument.ErrorCode = pair.Value;
                    response.AddFailedDocument(failedDocument);
                }

                //response.FailedDocuments.Concat(failedDocuments).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            }

            return response;
        }

        public Common.Server.Engine.IDBResponse DropCollection(Common.Server.Engine.IDropCollectionOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            if (nodeStateTxfrMgr != null)
                nodeStateTxfrMgr.OnDropCollection(operation.Database, operation.Collection);

            IDBResponse response = _databasesManager.DropCollection(operation);
            if (response.IsSuccessfull)
            {
                PulseGetClusterConfAndStore();
            }
            return response;
        }

        public Common.Server.Engine.IDBResponse DropIndex(Common.Server.Engine.IDropIndexOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            IDBResponse response = _databasesManager.DropIndex(operation);
            if (response.IsSuccessfull)
                PulseGetClusterConfAndStore();
            return response;
        }

        public Common.Server.Engine.IUpdateResponse ExecuteNonQuery(Common.Server.Engine.INonQueryOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            if (!(this.NodeRole == NodeRole.Primary))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Operation not supported on secondary node");
            if (_currentPrimaryRole.Equals(PrimaryAccess.RestrictedAccess))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Primary access restricted.");
            return _databasesManager.ExecuteNonQuery(operation);
        }

        public Common.Server.Engine.IQueryResponse ExecuteReader(Common.Server.Engine.IQueryOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            return _databasesManager.ExecuteReader(operation);
        }

        public Common.Server.Engine.IGetResponse GetDocuments(Common.Server.Engine.IGetOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            return _databasesManager.GetDocuments(operation);
        }

        public Common.Server.Engine.IDocumentsWriteResponse InsertDocuments(Common.Server.Engine.IDocumentsWriteOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            if (!(this.NodeRole.Equals(NodeRole.Primary)))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException(ErrorCodes.Cluster.NOT_PRIMARY, new string[] { _context.LocalShardName, _context.LocalAddress.ToString() });

            if (_currentPrimaryRole.Equals(PrimaryAccess.RestrictedAccess))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException(ErrorCodes.Database.PRIMARY_RESTRICTED_ACCESS, new string[] { _context.LocalAddress.ToString() });
            String dbName = operation.Database;
            String colName = operation.Collection;
            IDictionary<string, int> failedDocuments = new HashVector<string, int>();

            Common.Server.Engine.IDocumentsWriteResponse response = _databasesManager.InsertDocuments(operation);
            //if (response.IsSuccessfull)
            //{
            //    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
            //        LoggerManager.Instance.ShardLogger.Error("Document Added Successfully " + DateTime.Now.ToString());
            //}
            if (failedDocuments.Count > 0)
            {
                response.IsSuccessfull = false;

                foreach (var pair in failedDocuments)
                {
                    FailedDocument failedDocument = new FailedDocument();
                    failedDocument.DocumentKey = pair.Key;
                    failedDocument.ErrorCode = pair.Value;
                    response.AddFailedDocument(failedDocument);
                }

                //response.FailedDocuments.Concat(failedDocuments).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            }

            return response;
        }

        public Common.Server.Engine.IUpdateResponse UpdateDocuments(Common.Server.Engine.IUpdateOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            if (!(this.NodeRole == Common.Configuration.Services.NodeRole.Primary))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Operation not supported on secondary node");
            if (_currentPrimaryRole.Equals(PrimaryAccess.RestrictedAccess))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Primary access restricted.");
            return _databasesManager.UpdateDocuments(operation);
        }

        public IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            if (!(this.NodeRole == Common.Configuration.Services.NodeRole.Primary))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Operation not supported on secondary node");
            if (_currentPrimaryRole.Equals(PrimaryAccess.RestrictedAccess))
                throw new Alachisoft.NosDB.Common.Exceptions.DatabaseException("Primary access restricted.");
            return _databasesManager.ReplaceDocuments(operation);
        }

        public Common.Server.Engine.IGetChunkResponse GetDataChunk(Common.Server.Engine.IGetChunkOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);

            return _databasesManager.GetDataChunk(operation);
        }

        public Common.Server.Engine.IDBResponse DiposeReader(Common.Server.Engine.IDiposeReaderOperation operation)
        {
            Common.MiscUtil.IsArgumentNull(operation);
            if (_databasesManager != null)
                return _databasesManager.DiposeReader(operation);
            else
                return null;
        }
        #endregion

        #region IClusterListener Implementation

        public object OnMessageReceived(Message message, Server source)
        {
            return HandleMessage((DatabaseMessage)message, source);
        }

        private object HandleMessage(DatabaseMessage databaseMessage, Server source)
        {
            switch (databaseMessage.OpCode)
            {
                //case OpCode.CreateCollection: break;
                //case OpCode.CreateIndex: break;
                //case OpCode.DropCollection: break;
                //case OpCode.DropIndex: break;
                case OpCode.StateTransferOperation:
                    return HandleStateTransferOperation((IStateTransferOperation)databaseMessage.Payload);
                case OpCode.ShardConnected:
                    OnShardConnected((NodeIdentity)databaseMessage.Payload);
                    break;
                case OpCode.PrimaryChanged:
                    OnPrimaryChanged();
                    return 0;
               }

            return null;
        }

       
        /// <summary>
        /// Shard is added to cluster
        /// </summary>
        /// <param name="shard"></param>
        public void OnShardAdd(Alachisoft.NosDB.Common.Configuration.Services.ShardInfo shard)
        {
            //((ClusterManager)_cluster).OnShardAdded(shard);
            //OnShardAdd For now we will only get latest distribution map
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = shard.Name != null ? shard.Name : "", DatabaseName = "" });
            GetLatestDistributionMap();
            SetDistributionMaps();

            if ((this.NodeRole == NodeRole.Primary))
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Info(" PartitionOfReplica.OnShardAdded()", "Shard added event occured. ");
                }
            }
        }

        /// /// <summary>
        /// Shard is removed from cluster
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="isGraceful"></param>
        public void OnShardRemove(ShardInfo shard, bool isGraceful)
        {
            //OnShardAdd For now we will only get latest distribution map
            GetLatestDistributionMap();
            SetDistributionMaps();

            if (!isGraceful)
            {
                if (!_cluster.RemoveRemoteShard(shard.Name))
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Error("IClusterListener.OnShardRemove(): " ,"couldn't remove shard:"
                            + shard.Name + ", from shard:" + Context.LocalShardName);
                    }
                }
            }
        }


        public void OnCollectionMove(string database, string collection)
        {
            try
            {
                GetLatestDistributionMap();
                SetDistributionMaps();
                if ((this.NodeRole == NodeRole.Primary))
                {
                    IDictionary<String, IDictionary<String, IDistribution>> map = GetCollectionDistributionMap(database,
                        collection);
                    if (map == null)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error(" PartitionOfReplica.OnCollectionMove()", database + " not exists in distribution map. ");
                        return;
                    }

                    if (nodeStateTxfrMgr != null)
                    {
                        nodeStateTxfrMgr.Initialize((IDictionary)map, StateTransferType.INTER_SHARD);
                        nodeStateTxfrMgr.Start();
                    }
                   
                }
            }
            catch (Exception exception)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error(" PartitionOfReplica.OnCollectionMove()", exception.Message+" "+exception.StackTrace);
            }
        }

        public void OnNewRangeAdded()
        {
            GetLatestDistributionMap();
            SetDistributionMaps();
        }

        public void OnRangesUpdated()
        {
            GetLatestDistributionMap();
            SetDistributionMaps();

           
        }

        /// <summary>
        /// handle distribution changed event on cluster
        /// </summary>
        public void OnDistributionChanged()
        {
            //OnShardAdd For now we will only get latest distribution map
            //GetLatestDistributionMap();
            //SetDistributionMaps();

            //if ((this.NodeRole == Configuration.Services.NodeRole.Primary))
            //{
            //    if (this.nodeStateTxfrMgr == null)
            //        nodeStateTxfrMgr = new NodeStateTranferManager(_context, this);
            //    nodeStateTxfrMgr.Start((IDictionary)_distributionMaps, false);
            //}
        }

        #region Migrate Database Methods

        private IDictionary<String, IDictionary<String, IDistribution>> GetDistributionMap(string database)
        {
            if (!_distributionMaps.ContainsKey(database))
                return null;

            IDictionary<string, IDistribution> distributions = _distributionMaps[database];

            IDictionary<String, IDictionary<String, IDistribution>> map =
                new Dictionary<string, IDictionary<string, IDistribution>>();

            map.Add(database, distributions);
            return map;
        }

        private IDictionary<String, IDictionary<String, IDistribution>> GetCollectionDistributionMap(string database, string collection)
        {
            if (!_distributionMaps.ContainsKey(database))
                return null;

            IDictionary<string, IDistribution> distributions = _distributionMaps[database];

            IDistribution collectionDistribution = distributions[collection];

            distributions = new Dictionary<string, IDistribution> {{collection, collectionDistribution}};

            IDictionary<String, IDictionary<String, IDistribution>> map =
                new Dictionary<string, IDictionary<string, IDistribution>>();

            map.Add(database, distributions);
            return map;
        }

        private void OnIntraShardStateTrxferCompleted(string database, string shard)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(" PartitionOfReplica.OnIntraShardStateTrxferCompleted()", "Intra Shard State Trxfer Completed event occured. ");

            GetLatestDistributionMap();
            SetDistributionMaps();

            if ((this.NodeRole == NodeRole.Secondary && _context.LocalShardName.Equals(shard, StringComparison.OrdinalIgnoreCase)))
            {
                IDictionary<String, IDictionary<String, IDistribution>> map = GetDistributionMap(database);

                if (map == null)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error(" PartitionOfReplica.OnIntraShardStateTrxferCompleted()", database + " not exists in distribution map. ");
                    return;
                }
                

            }
        }
        #endregion


        private Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration GetSystemDatabaseConfiguration(ClusterConfiguration clusterconfig)
        {
            Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration systemDatabaseConfiguration = new DatabaseConfiguration();

            systemDatabaseConfiguration.Name = Common.MiscUtil.SYSTEM_DATABASE;

          

            systemDatabaseConfiguration.Storage = new StorageConfiguration();

            systemDatabaseConfiguration.Storage.Collections = new CollectionConfigurations();
            systemDatabaseConfiguration.Storage.CacheConfiguration = new CachingConfiguration();
            systemDatabaseConfiguration.Storage.CacheConfiguration.CachePolicy = "fcfs";
            systemDatabaseConfiguration.Storage.CacheConfiguration.CacheSpace = Common.MiscUtil.DEFAULT_CACHE_SPACE;
            systemDatabaseConfiguration.Storage.StorageProvider = new StorageProviderConfiguration();
            systemDatabaseConfiguration.Storage.StorageProvider.StorageProviderType = ProviderType.LMDB;
            systemDatabaseConfiguration.Storage.StorageProvider.MaxFileSize = 1073741824;
            systemDatabaseConfiguration.Storage.StorageProvider.IsMultiFileStore = true;

            systemDatabaseConfiguration.Storage.StorageProvider.LMDBProvider = new LMDBConfiguration();
            systemDatabaseConfiguration.Storage.StorageProvider.LMDBProvider.EnvironmentOpenFlags = LMDBEnvOpenFlags.NoSubDir;

            systemDatabaseConfiguration.Storage.StorageProvider.LMDBProvider.MaxReaders = 126;
            int maxCol = 0;
            foreach (var database in clusterconfig.Databases.Configurations.Values)
            //for (int i = 0; i < clusterconfig.Databases.Configurations.Count; i++)
            {
                switch (database.Storage.StorageProvider.StorageProviderType)
                {
                    case ProviderType.LMDB:
                        maxCol += 2 * database.Storage.StorageProvider.LMDBProvider.MaxCollections;
                        break;
                    
                }
            }
            // Query Result
            maxCol += 1;
            // Recovery Job Status
            maxCol += 1;
            //todo get max tasks
            // MapReduce stuff
            //maxCol += clusterconfig.MaxTasks;
            // Replication Collections
            maxCol += 1;
            // Security Information Collections
            maxCol += 1;
            // User Information Collections
            maxCol += 1;

            //Umer ConfigurationCollections
            maxCol += 1;
            // DifLog collection
            maxCol += 1;

            //todo: Umer
            // as runtime collections can not be added dynamically
            maxCol += 1000;


            systemDatabaseConfiguration.Storage.StorageProvider.LMDBProvider.MaxCollections = maxCol;

            if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                LoggerManager.Instance.ServerLogger.Info("Max Collections on SysDB set = ", maxCol.ToString());

            return systemDatabaseConfiguration;
        }

        public void OnPrimaryChanged()
        {
            try
            {
                ((ClusterManager)_cluster).ManageRemoteShards();
                if (((ClusterManager)_cluster).ShardNodeRole == NodeRole.Secondary/* || ((ClusterManager)_cluster).ShardNodeRole == NodeRole.Intermediate*/)
                {
                    //if (_databasesManager != null)
                    //    _databasesManager.StopBucketInfoTask();
                    
                   
                }
                else if (this.NodeRole == NodeRole.Primary)
                {
                    //if (_databasesManager != null)
                    //   // _databasesManager.StartBucketInfoTask();

                    lock (_onPrimaryRole)
                    {
                        _currentPrimaryRole = PrimaryAccess.UnrestrictedAccess;
                    }
                }
                else if (this.NodeRole == NodeRole.None)
                {

                    //if (_databasesManager != null)
                    //    _databasesManager.StopBucketInfoTask();
                }
               
            }
            catch (Exception e)
            {

                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("PartitionOfReplica.OnPrimaryChanged",
                       e.ToString());
                }
            }
        }



        #endregion

        #region IConfigurationChange Listener
        public void OnConfigurationChanged(ConfigChangeEventArgs arguments)
        {
            ChangeType type = ChangeType.None;
            if (arguments != null)
            {
                string clusterName = arguments.GetParamValue<string>(EventParamName.ClusterName);
                if (clusterName != null && !clusterName.Equals(_context.ClusterName)) return;
                type = arguments.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType);
            }

            switch (type)
            {
                case ChangeType.DistributionStrategyConfigured:
                    break;
                case ChangeType.DatabaseDropped:
                case ChangeType.ConfigRestored:
                case ChangeType.CollectionDropped:
                    GetLatestDistributionMap();
                    SetDistributionMaps();
                    break;
                case ChangeType.CollectionMoved:
                    if (arguments != null)
                        OnCollectionMove(arguments.GetParamValue<string>(EventParamName.DatabaseName), arguments.GetParamValue<string>(EventParamName.CollectionName));
                    break;
               case ChangeType.ResyncDatabase:
                    try
                    {
                        string dbName = arguments.GetParamValue<string>(EventParamName.DatabaseName);
                        string clusterName = arguments.GetParamValue<string>(EventParamName.ClusterName);
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("POR.OnConfigChanged()", "Resynchronization of config initiated");
                        // fetch new cluster config
                        ClusterConfiguration newClusterConf = _context.ConfigurationSession.GetDatabaseClusterConfiguration(_context.ClusterName);
                        //
                        TrySynchronizeClusterConf(newClusterConf);

                        GetLatestDistributionMap();
                        SetDistributionMaps();
                        if (clusterName.Equals(_context.ClusterName))
                        {
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("POR.OnConfigChanged()", "Resynchronization of " + dbName + " cluster: " + _context.ClusterName + " Success");
                        }
                    }
                    catch (Exception exp)
                    {
                        string dbName = arguments.GetParamValue<string>(EventParamName.DatabaseName);
                        string clusterName = arguments.GetParamValue<string>(EventParamName.ClusterName);

                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Error("POR.OnConfigChanged()", exp.ToString());

                        if (clusterName.Equals(_context.ClusterName))
                        {
                        }
                    }
                    break;
                case ChangeType.IntraShardStateTrxferCompleted:
                    if (arguments != null)
                        OnIntraShardStateTrxferCompleted(arguments.GetParamValue<string>(EventParamName.DatabaseName), arguments.GetParamValue<string>(EventParamName.ShardName));
                    break;
            }
        }
        #endregion

        #region IDisposible Implmentation
        public void Dispose()
        {
            Dispose(false);
        }
        public void Dispose(bool destroy)
        {
            _context.StatusLatch.SetStatusBit(NodeStatus.Stopped, NodeStatus.Initializing | NodeStatus.Running);
            if (_recoveryManager != null)
                _recoveryManager.Dispose();

            if (nodeStateTxfrMgr != null)
                nodeStateTxfrMgr.Stop();

            if (this._cluster != null)
            {
                _cluster.Stop();
                _cluster.Dispose();
            }

            if (_databasesManager != null)
            {
                _databasesManager.Dispose(destroy);
                _databasesManager = null;
            }

            _getClusterConfig.Abort();
        }
        #endregion


        #region Methods for State transfer

        /// <summary>
        /// Fetch state from a cluster member. If the node is the coordinator there is
        /// no need to do the state transfer.
        /// End of state transfer announced locally and also set latch to running for current collection, update local statistics and annonce presence [Broadcast stats]
        /// </summary>
        internal virtual void EndStateTransfer(IStateTransferOperation operation)
        {
            /// Set the status to fully-functional (Running) and tell everyone about it.
            //_statusLatch.SetStatusBit(NodeStatus.Running, NodeStatus.Initializing);

            UpdateCacheStatistics();
            AnnouncePresence(true);
        }


        /// <summary>
        /// Updates the statistics for the cache scheme.
        /// </summary>
        protected virtual void UpdateCacheStatistics()
        {
            // NTD: [Normal] provide implementation for this method

            //try
            //{
            //    _stats.LocalNode.Statistics = _internalCache.Statistics;
            //    _stats.LocalNode.Status.Data = _statusLatch.Status.Data;

            //    _stats.SetServerCounts(Convert.ToInt32(Servers.Count),
            //        Convert.ToInt32(ValidMembers.Count),
            //        Convert.ToInt32(Members.Count - ValidMembers.Count));
            //    CacheStatistics c = CombineClusterStatistics(_stats);
            //    _stats.UpdateCount(c.Count);
            //    _stats.HitCount = c.HitCount;
            //    _stats.MissCount = c.MissCount;
            //    _stats.MaxCount = c.MaxCount;
            //    _stats.MaxSize = c.MaxSize;
            //}
            //catch (Exception)
            //{
            //}
        }

        public bool AnnouncePresence(bool urgent)
        {
            // NTD: [Normal] provide implementation for this method

            //try
            //{
            //    UpdateCacheStatistics();
            //    if (Context.NCacheLog.IsInfoEnabled) Context.NCacheLog.Info("ClusteredCacheBase.AnnouncePresence()", " announcing presence ;urget " + urgent);
            //    if (this.ValidMembers.Count > 1)
            //    {
            //        NodeInfo localStats = _stats.LocalNode;
            //        localStats.StatsReplicationCounter++;
            //        Function func = new Function((int)OpCodes.PeriodicUpdate, _stats.LocalNode.Clone());
            //        if (!urgent)
            //            Cluster.SendNoReplyMessage(func);
            //        else
            //            Cluster.Broadcast(func, GroupRequest.GET_NONE, false, Priority.Normal);
            //    }
            //    return true;
            //}
            //catch (Exception e)
            //{
            //    Context.NCacheLog.Error("ClusteredCacheBase.AnnouncePresence()", e.ToString());
            //}
            return false;
        }
        #endregion

        #region IDispacther Implementation
        /// <summary>
        /// Dispatch State Transfer Operation to other shard(s)/node(s)
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">actual operation</param>
        /// <returns></returns>
        public T DispatchOperation<T>(IStateTransferOperation operation) where T : class
        {
            switch (operation.OpCode)
            {
                case StateTransferOpCode.IsShardConnected:
                    return IsShardConnected((String)operation.Params.GetParamValue(ParamName.ShardName)) as T;

                case StateTransferOpCode.CreateCorresponder:
                    return CreateCorresponder(operation) as T;

                case StateTransferOpCode.DestroyCorresponder:
                    DestroyCorresponder(operation);
                    break;

                case StateTransferOpCode.TransferBucketKeys:
                case StateTransferOpCode.TransferBucketData:
                    return TransferBucket(operation) as T;

                case StateTransferOpCode.AckBucketTxfer:
                    AckBucketTxfer(operation);
                    break;
            }
            return default(T);
        }


        /// <summary>
        /// Acknowledge transfer completed, and broadcast to all shards so that they can remove it from there local buckets.
        /// </summary>
        private void AckBucketTxfer(IStateTransferOperation operation)
        {
            Message msg = CreateStateTransferMessage(operation);
            NodeIdentity owner = (NodeIdentity)operation.Params.GetParamValue(ParamName.OpDestination);
            //if(Cluster!=null)
            //    this.Cluster.SendMessageToAllShards<Object>(msg, true);
            if (Cluster != null)
                this.Cluster.SendMessage<Object>(owner.ShardName, /*new Server(owner.Address, Status.Running), */msg);
        }

        /// <summary>
        /// Send End of State Transfer signal to corrosponding shard, so that it can remove resources for this state transfer like crossponders removal etc.
        /// </summary>
        private void DestroyCorresponder(IStateTransferOperation operation)
        {
            Message msg = CreateStateTransferMessage(operation);
            NodeIdentity owner = (NodeIdentity)operation.Params.GetParamValue(ParamName.OpDestination);

            if (Cluster != null)
            {
                Object obj = this.Cluster.SendMessage<Object>(owner.ShardName,/* new Server(owner.Address, Status.Running),*/ msg);
                if (obj is Exception)
                {
                    throw obj as Exception;
                }
            }
        }


        /// <summary>
        /// Transfer Bucket data from target node 
        /// </summary>  
        private Boolean CreateCorresponder(IStateTransferOperation operation)
        {
            Message msg = CreateStateTransferMessage(operation);
            NodeIdentity owner = (NodeIdentity)operation.Params.GetParamValue(ParamName.OpDestination);

            if (Cluster != null)
            {
                Object result = Cluster.SendMessage<Object>(owner.ShardName,/* new Server(owner.Address, Status.Running),*/ msg);
                if (result is Exception)
                {
                    throw result as Exception;
                }

                return (Boolean)result;
            }

            return false;
        }


        /// <summary>
        /// Transfer Bucket data from target node 
        /// </summary>  
        private StateTxfrInfo TransferBucket(IStateTransferOperation operation)
        {
            StateTxfrInfo info = null;
            Message msg = CreateStateTransferMessage(operation);
            NodeIdentity owner = (NodeIdentity)operation.Params.GetParamValue(ParamName.OpDestination);

            //if(Cluster!=null)
            //    this.Cluster.SendMessageToAllShards<Object>(msg, true);       

            if (Cluster != null)
            {
                Object result = this.Cluster.SendMessage<Object>(owner.ShardName,/* new Server(owner.Address, Status.Running),*/ msg);
                if (result is Exception)
                {
                    throw result as Exception;
                }

                info = result as StateTxfrInfo;
            }

            return info;
        }


        /// <summary>
        /// /// Get Primary node of given shard name
        /// /// </summary>
        private Boolean IsShardConnected(String shardName)
        {
            if (Cluster == null || Cluster.ShardNodeRole == NodeRole.None) return false;

            return Cluster.IsShardConnected(shardName);
        }

        private DatabaseMessage CreateStateTransferMessage(IStateTransferOperation operation)
        {
            DatabaseMessage msg = new DatabaseMessage();
            msg.MessageType = MessageType.DBOperation;
            msg.OpCode = OpCode.StateTransferOperation;
            msg.Payload = operation;

            return msg;
        }
        #endregion

        #region State Transfer operation reciever
        /// <summary>
        /// Handle State Transfer Related Operation
        /// </summary>
        /// <param name="stateTransferOperation">Actual Operation for state transfer</param>
        /// <returns></returns>
        private object HandleStateTransferOperation(IStateTransferOperation operation)
        {
            // Check for OpCode if anything that could be performed on topology level,
            // then no need to delegate this operation to lower level [statetxfer]
            //
            //((ClusterManager)_cluster).ShardNodeRole == NodeRole.Secondary/

            if (this.nodeStateTxfrMgr != null)
            {
                if (operation.OpCode != StateTransferOpCode.DestroyCorresponder && NodeRole != NodeRole.Primary)
                {
                    return new Alachisoft.NosDB.Core.Toplogies.Exceptions.StateTransferException(Common.ErrorHandling.ErrorCodes.StateTransfer.PRIMARY_CHANGED, new string[] { Context.LocalAddress != null ? Context.LocalAddress.ToString() : String.Empty, Context.LocalShardName }); //  (Context.LocalAddress+" no more primary for shard "+Context.LocalShardName);               
                }

                return nodeStateTxfrMgr.OnOperationRecieved(operation);
            }
            return null;
        }

        private void OnShardConnected(NodeIdentity shard)
        {
            if (this.nodeStateTxfrMgr != null)
            {
                nodeStateTxfrMgr.OnShardConnected(shard);
            }
        }

        #endregion


        public bool DropDatabase(string databaseName, bool dropFiles)
        {
            if (nodeStateTxfrMgr != null)
                nodeStateTxfrMgr.OnDropDatabase(databaseName);
          
            bool response = _databasesManager.DropDatabase(databaseName, dropFiles);
            if (response)
            {
                PulseGetClusterConfAndStore();
            }
            return response;
        }

        public IList<Server> GetActiveChannelList()
        {
            return _cluster.GetActiveChannelList();
        }

        #region Recovery Operations
        public RecoveryOperationStatus OnRecoveryOperationReceived(RecoveryOperation opContext)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            if (opContext != null)
            {
                state = _recoveryManager.RecoveryOperationReceived(opContext);
            }
            return state;
        }

        #endregion
      

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        enum PrimaryAccess : int
        {
            RestrictedAccess = 0,
            UnrestrictedAccess = 1,
        }
        public bool SetDatabaseMode(string databaseName, DatabaseMode databaseMode)
        {
            Common.MiscUtil.IsArgumentNull(databaseName);
            return _databasesManager.SetDatabaseMode(databaseName, databaseMode);
        }

        public void IsOpertionAllow(string database)
        {
            Common.MiscUtil.IsArgumentNull(database);
            if (_databasesManager != null)
            {
                _databasesManager.IsOperationAllow(database);
            }
            else
            {
                throw new DatabaseException(ErrorCodes.Cluster.DATABASE_MANAGER_DISPOSED);
            }
        }

        
    }
}
