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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Core.Storage;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Impl;
using System;
using Alachisoft.NosDB.Common.Logger;
using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Serialization;



namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ConfigurationStore : IConfigurationStore
    {
        private IDatabaseStore _dbStore;
        private DatabaseConfiguration _dbConfiguration;
        private NodeContext _nodeContext;
        private string _dbConfCollection;
        private string _membersCollection;
        private string _clusterInfo;
        private string _ciBucketStatistics;
        private string _ciDistributionInfo;
        private string _securityInformationCollection;
        private string _userInformationCollection;
        private string _roleInformationCollection;
        private string _dbName;
        private object _lock = new object();
        private Dictionary<string, DatabaseCluster> _databaseClusters = new Dictionary<string, DatabaseCluster>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, Transaction> _runningTransactions = new Dictionary<string, Transaction>(StringComparer.CurrentCultureIgnoreCase);
        private bool _clusterInfoLoaded;
        private bool _membershipLoaded;
        private bool _clusterConfigLoaded;
        private bool _securityResourceLoaded;
        private bool _loginsLoaded;
        private string _recoveryInformationCollection;
        private List<string> _clusters = new List<string>();
        private ITransactionListener _transactionListener;



        public ConfigurationStore(DatabaseConfiguration configuration, NodeContext context)
        {
            _dbStore = new SystemDatabaseStore();
            _dbConfiguration = configuration;
            _nodeContext = context;
            _dbConfCollection = "dbconfigcollection";
            _membersCollection = "membershipcollection";
            _clusterInfo = "clusterinfocollection";
            _ciBucketStatistics = "cibucketstatisticsollection";// "cicollectioninfocollection"
            _ciDistributionInfo = "cidistributioncollection";
            _securityInformationCollection = "securityinformationcollection";
            _userInformationCollection = "userinformationcollection";
            _roleInformationCollection = "roleinformationcollection";
            _dbName = "configurationdatabase";
            _recoveryInformationCollection = "recoveryinformationcollection";// who so ever changes this name, kindly update in recovery manager aswell.
        }
        public void Initialize()
        {
            _dbConfiguration.Storage.Collections.Configuration = new Dictionary<string, CollectionConfiguration>();
            //db collection configuration
            CollectionConfiguration dbCollectionConfiguration = new CollectionConfiguration();
            dbCollectionConfiguration.CollectionName = _dbConfCollection;
            dbCollectionConfiguration.Indices = new Indices();
            dbCollectionConfiguration.EvictionConfiguration = new EvictionConfiguration();
            dbCollectionConfiguration.EvictionConfiguration.EnabledEviction = true;
            dbCollectionConfiguration.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(dbCollectionConfiguration);

            //membershipdata collection configuration
            CollectionConfiguration membrCollectionConfiguration = new CollectionConfiguration();
            membrCollectionConfiguration.CollectionName = _membersCollection;
            membrCollectionConfiguration.Indices = new Indices();
            membrCollectionConfiguration.EvictionConfiguration = new EvictionConfiguration();
            membrCollectionConfiguration.EvictionConfiguration.EnabledEviction = true;
            membrCollectionConfiguration.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(membrCollectionConfiguration);

            #region ClusterInfo Colls
            //ClusterInfo collection configuration
            CollectionConfiguration ciClusterInfo = new CollectionConfiguration();
            ciClusterInfo.CollectionName = _clusterInfo;
            ciClusterInfo.Indices = new Indices();
            ciClusterInfo.EvictionConfiguration = new EvictionConfiguration();
            ciClusterInfo.EvictionConfiguration.EnabledEviction = true;
            ciClusterInfo.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(ciClusterInfo);

            //CollectionInfo collection configuration
            CollectionConfiguration ciBucketStatus = new CollectionConfiguration();
            ciBucketStatus.CollectionName = _ciBucketStatistics;
            ciBucketStatus.Indices = new Indices();
            ciBucketStatus.EvictionConfiguration = new EvictionConfiguration();
            ciBucketStatus.EvictionConfiguration.EnabledEviction = true;
            ciBucketStatus.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(ciBucketStatus);

            //BucketInfoCollection collection configuration
            CollectionConfiguration ciDistribution = new CollectionConfiguration();
            ciDistribution.CollectionName = _ciDistributionInfo;
            ciDistribution.Indices = new Indices();
            ciDistribution.EvictionConfiguration = new EvictionConfiguration();
            ciDistribution.EvictionConfiguration.EnabledEviction = true;
            ciDistribution.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(ciDistribution);
            #endregion

            //security information collection configuration
            CollectionConfiguration securityInformationCollection = new CollectionConfiguration();
            securityInformationCollection.CollectionName = _securityInformationCollection;
            securityInformationCollection.Indices = new Indices();
            securityInformationCollection.EvictionConfiguration = new EvictionConfiguration();
            securityInformationCollection.EvictionConfiguration.EnabledEviction = true;
            securityInformationCollection.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(securityInformationCollection);

            //user information collection configuration
            CollectionConfiguration userInformationCollection = new CollectionConfiguration();
            userInformationCollection.CollectionName = _userInformationCollection;
            userInformationCollection.Indices = new Indices();
            userInformationCollection.EvictionConfiguration = new EvictionConfiguration();
            userInformationCollection.EvictionConfiguration.EnabledEviction = true;
            userInformationCollection.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(userInformationCollection);

            //role information collection configuration
            CollectionConfiguration roleInformationCollection = new CollectionConfiguration();
            roleInformationCollection.CollectionName = _roleInformationCollection;
            roleInformationCollection.Indices = new Indices();
            roleInformationCollection.EvictionConfiguration = new EvictionConfiguration();
            roleInformationCollection.EvictionConfiguration.EnabledEviction = true;
            roleInformationCollection.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(roleInformationCollection);

            //recovery information collection configuration
            CollectionConfiguration recoveryInformationCollection = new CollectionConfiguration();
            recoveryInformationCollection.CollectionName = _recoveryInformationCollection;
            recoveryInformationCollection.Indices = new Indices();
            recoveryInformationCollection.EvictionConfiguration = new EvictionConfiguration();
            recoveryInformationCollection.EvictionConfiguration.EnabledEviction = true;
            recoveryInformationCollection.EvictionConfiguration.Policy = "lru";
            _dbConfiguration.Storage.Collections.AddCollection(recoveryInformationCollection);

            ((SystemDatabaseStore) _dbStore).Initialize(_dbConfiguration, _nodeContext, null);
        }

        public Transaction BeginTransaction(string cluster, bool startIfNotRunning = true, bool cloneValues = true)
        {
            Transaction transaction = null;
            lock (_runningTransactions)
            {
                if (_runningTransactions.ContainsKey(cluster))
                {
                    transaction = _runningTransactions[cluster];

                    if (!transaction.Started)
                    {
                        if (startIfNotRunning)
                        {
                            if (transaction.Started && !transaction.IsOwnedByCurrentThread)
                                throw new Exception("A transaction is already in process. Another transaction can not be initiated for cluster '" + cluster + "'");

                            transaction.Begin();
                        }
                    }
                }

                if (transaction == null)
                {
                    transaction = new Transaction(cluster, this);
                    _runningTransactions.Add(cluster, transaction);

                    if (startIfNotRunning)
                        transaction.Begin();
                }
                transaction.CloneValues = cloneValues;
            }

            return transaction;
        }

        public void CommitTransaction(Transaction transaction)
        {
            lock (_runningTransactions)
            {
                if (_transactionListener != null)
                {
                    if (!_transactionListener.OnPreCommitTransaction(transaction))
                    {
                        return;
                    }
                }
                transaction.Commit();
            }
        }



        public void RollbackTranscation(Transaction transaction)
        {
            lock (_runningTransactions)
            {
                transaction.Rollback();
            }
        }


        private DatabaseCluster GetDatabaseCluster(string cluster, bool createIfNotExis)
        {
            DatabaseCluster databaseCluster = null;

            if (!_databaseClusters.ContainsKey(cluster))
            {
                if (createIfNotExis)
                {
                    databaseCluster = new DatabaseCluster();
                    _databaseClusters.Add(cluster, databaseCluster);
                }
                return databaseCluster;
            }

            return _databaseClusters[cluster];
        }

        public ClusterConfiguration[] GetAllClusterConfiguration()
        {
            DateTime start = DateTime.Now;
            ClusterConfiguration[] clusterConfigurations = null;
            lock (_lock)
            {


                int i = 0;
                if (_clusterConfigLoaded)
                {
                    clusterConfigurations = new ClusterConfiguration[_databaseClusters.Count];
                    if (_databaseClusters != null && _databaseClusters.Count > 0)
                    {
                        foreach (DatabaseCluster databaseCluster in _databaseClusters.Values)
                        {
                            if (databaseCluster != null)
                            {
                                clusterConfigurations[i] = databaseCluster.Configuration;
                                i++;
                            }

                        }
                    }

                }
                else
                {
                    //JsonSerializer<ClusterConfiguration> serializer = new JsonSerializer<ClusterConfiguration>();

                    ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_dbConfCollection];

                    clusterConfigurations = new ClusterConfiguration[collection.Count()];
                    i = 0;
                    foreach (IJSONDocument doc in collection)
                    {
                        ClusterConfiguration configuration = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<ClusterConfiguration>((JSONDocument)doc);
                        clusterConfigurations[i] = configuration;
                        AddClusterConfiguration(configuration);
                        i++;
                    }
                    _clusterConfigLoaded = true;
                }
            }
            ReportTimeSpent("GetAllClusterConfiguration", start);

            return clusterConfigurations;
        }

        private void AddClusterConfiguration(ClusterConfiguration configuration)
        {
            if (configuration != null)
            {
                DatabaseCluster cluster = GetDatabaseCluster(configuration.Name, true);
                cluster.Configuration = configuration;
            }
        }

        private void AddSecurityResource(string clusterName, IResourceItem resourceItem)
        {
            if (resourceItem != null)
            {
                DatabaseCluster cluster = GetDatabaseCluster(clusterName, true);
                if(cluster != null)
                    cluster.AddOrUpdateSecurityResource(resourceItem.ResourceId.Name, resourceItem);
            }
        }

        private void AddLogin(string clusterName, IUser user)
        {
            if (user != null)
            {
                DatabaseCluster cluster = GetDatabaseCluster(clusterName, true);

                if (cluster != null)
                {
                    cluster.AddOrUpdateLogin(user.Username, user);
                }
            }
        }

        public ClusterConfiguration GetClusterConfiguration(string cluster)
        {
            DateTime start = DateTime.Now;
            ClusterConfiguration clusterConfiguration = null;

            DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, false);

            if (databaseCluster != null && databaseCluster.Configuration != null)
                return databaseCluster.Configuration;
            
            IJSONDocument jsonDocument = FetchFromDatabase(cluster, _dbName, _dbConfCollection);

            if (jsonDocument != null)
            {
                clusterConfiguration = Common.JsonSerializer.Deserialize<ClusterConfiguration>(jsonDocument);
            }

            if (clusterConfiguration != null)
            {
                AddClusterConfiguration(clusterConfiguration);
            }

            ReportTimeSpent("GetClusterConfiguration", start);

            return clusterConfiguration;
        }

        public void InsertOrUpdateClusterConfiguration(ClusterConfiguration configuration)
        {
            DateTime start = DateTime.Now;

            lock (_lock)
            {
                if (_dbStore == null) return;

                if (configuration != null)
                {

                    IJSONDocument jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<ClusterConfiguration>(configuration);
                    InsertOrUpdateDocumentInDatabase(jdoc, configuration.Name, _dbName, _dbConfCollection);

                    DatabaseCluster databaseCluster = GetDatabaseCluster(configuration.Name, true);
                    if (databaseCluster != null)
                        databaseCluster.Configuration = configuration;
                }
            }
            ReportTimeSpent("InsertOrUpdateClusterConfiguration", start);

        }

        public bool ContainsCluster(string cluster)
        {
            if (_databaseClusters.ContainsKey(cluster))
            {
                return _databaseClusters[cluster].Configuration != null;
            }
            return false;
        }

        public void RemoveClusterConfiguration(string cluster)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                //JsonSerializer<ClusterConfiguration> serializer = new JsonSerializer<ClusterConfiguration>();
                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_dbConfCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (cluster != null)
                {
                    doc.Key = cluster.ToLower();
                    found = FindDocument(cluster, _dbConfCollection);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _dbConfCollection;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }

                    DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, false);
                    if (databaseCluster != null & databaseCluster.Configuration != null)
                        databaseCluster.Configuration = null;
                }
            }
            ReportTimeSpent("RemoveClusterConfiguration", start);
        }

        public ClusterInfo[] GetAllClusterInfo()
        {
            DateTime start = DateTime.Now;
            ClusterInfo[] clusterInfo = null;
            lock (_lock)
            {
                //JsonSerializer<ClusterInfo> serializer = new JsonSerializer<ClusterInfo>();
                int i = 0;

                if (_clusterInfoLoaded)
                {
                    clusterInfo = new ClusterInfo[_databaseClusters.Count];
                    if (_databaseClusters != null && _databaseClusters.Count > 0)
                    {
                        foreach (DatabaseCluster databaseCluster in _databaseClusters.Values)
                        {
                            if (databaseCluster != null)
                            {
                                clusterInfo[i] = databaseCluster.ClusterInfo;
                                i++;
                            }
                        }
                    }

                }
                else
                {
                    ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_clusterInfo];
                    clusterInfo = new ClusterInfo[collection.Count()];
                    foreach (IJSONDocument doc in collection)
                    {
                        ClusterInfo info = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<ClusterInfo>((JSONDocument)doc, new JsonConverter[] { new DistributionJsonConverter(), new DistributionStrategyJsonConverter() });
                        clusterInfo[i] = info;
                        AddClusterInfo(info);
                        //todo: get all bucket info here
                        i++;
                    }
                    _clusterInfoLoaded = true;
                }
            }
            ReportTimeSpent("GetAllClusterInfo", start);
            return clusterInfo;
        }

        private void AddClusterInfo(ClusterInfo info)
        {
            if (info != null)
            {
                DatabaseCluster cluster = GetDatabaseCluster(info.Name, true);
                cluster.ClusterInfo = info;
            }
        }

        public void GetAllDistributionStrategies(ClusterInfo[] clusterinfo)
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<ClusterInfo> serializer = new JsonSerializer<ClusterInfo>();
            // ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_clusterInfo];
            int i = 0;
            if (clusterinfo != null)
            {
                foreach (ClusterInfo cluster in clusterinfo)
                {
                    //todo: get all bucket info here
                    if (cluster != null && cluster.Databases != null)
                    {
                        foreach (var database in cluster.Databases.Values)
                        {
                            if (database != null && database.Collections != null)
                            {
                                foreach (CollectionInfo col in database.Collections.Values)
                                {
                                    col.DistributionStrategy = GetDistributionStrategy(cluster.Name, database.Name, col.Name);
                                }
                            }
                        }
                    }
                }
            }
            ReportTimeSpent("GetAllDistributionInfo", start);
        }

        private void AddDistributionStrategy(string cluster, string database, string collection, IDistributionStrategy strategy)
        {
            DatabaseCluster dbcluster = GetDatabaseCluster(cluster, true);
            dbcluster.AddOrUpdateDistributionStrategy(database, collection, strategy);
        }

        public IDistributionStrategy GetDistributionStrategy(string cluster, string database, string collection)
        {
            DateTime start = DateTime.Now;

            DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, false);
            IDistributionStrategy strategy = null;

            if (databaseCluster != null)
            {
                strategy = databaseCluster.GetDistributionStrategy(database, collection);
            }

            if (strategy != null)
                return strategy;
            else
            {
                IJSONDocument jsonDocument = FetchFromDatabase(cluster, database, _ciDistributionInfo, GetCollectionInfoName(cluster, database, collection));

                if (jsonDocument != null)
                {
                    strategy = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IDistributionStrategy>(jsonDocument, new JsonConverter[] { new DistributionJsonConverter(), new DistributionStrategyJsonConverter() });
                }

                if (strategy != null)
                {

                    AddDistributionStrategy(cluster, database, collection, strategy);
                }
            }

            ReportTimeSpent("GetDistributionStrategy", start);
            return strategy;
        }

        private IJSONDocument FetchFromDatabase(string cluster, string database, string collection, string documentKey = null)
        {
            IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
            JSONDocument doc = new JSONDocument();
            doc.Key = documentKey == null ? cluster.ToLower() : documentKey;
            jsonDocuments.Add(doc);
            IGetOperation getOperation = new GetDocumentsOperation();
            getOperation.Database = database;
            getOperation.Collection = collection;
            getOperation.DocumentIds = jsonDocuments;
            IGetResponse response = _dbStore.GetDocuments(getOperation);
            IDataChunk dataChunk = response.DataChunk;

            if (dataChunk != null && dataChunk.Documents != null && dataChunk.Documents.Count() > 0)
                return dataChunk.Documents.First();

            return null;
        }

        private ICollectionReader FetchReaderFromDatabase(string cluster, string database, string collection, string[] documentKeys)
        {
            IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
            foreach (var key in documentKeys)
            {
                var doc = new JSONDocument {Key = key};
                jsonDocuments.Add(doc);
            }
            
            IGetOperation getOperation = new GetDocumentsOperation();
            getOperation.Database = database;
            getOperation.Collection = collection;
            getOperation.DocumentIds = jsonDocuments;
            IGetResponse response = _dbStore.GetDocuments(getOperation);
            IDataChunk dataChunk = response.DataChunk;



            if (dataChunk != null && dataChunk.Documents != null && dataChunk.Documents.Count > 0)
            {
                ICollectionReader reader = new CollectionReader(dataChunk, _dbStore, _dbName, _ciDistributionInfo);
                return reader;
            }
            return null;
        }

        //public void GetAllBucketInfo(string database, string collection)
        //{
        //    DateTime start = DateTime.Now;
        //    //JsonSerializer<ClusterInfo> serializer = new JsonSerializer<ClusterInfo>();
        //    ClusterInfo[] clusterInfo;
        //    ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_ciBucketInfo];
        //    clusterInfo = new ClusterInfo[collection.Count()];
        //    int i = 0;
        //    foreach (IJSONDocument doc in collection)
        //    {
        //        clusterInfo[i] = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<ClusterInfo>((JSONDocument)doc, new JsonConverter[] { new DistributionJsonConverter(), new DistributionStrategyJsonConverter() });

        //        i++;
        //    }
        //    ReportTimeSpent("GetAllClusterInfo", start);
        //    return clusterInfo;
        //}

        public ClusterInfo GetClusterInfo(string cluster)
        {
            DateTime start = DateTime.Now;
            ClusterInfo clusterInfo = null;
            DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, false);

            if (databaseCluster != null & databaseCluster.ClusterInfo != null)
                return databaseCluster.ClusterInfo;
            else
            {
                IJSONDocument jsonDocument = FetchFromDatabase(cluster, _dbName, _clusterInfo);

                if (jsonDocument != null)
                {
                    clusterInfo = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<ClusterInfo>(jsonDocument);
                }

                if (clusterInfo != null)
                {
                    AddClusterInfo(clusterInfo);
                }
            }

            ReportTimeSpent("GetClusterInfo", start);
            return clusterInfo;
        }

        public void RegiserTransactionListener(ITransactionListener listener)
        {
            _transactionListener = listener;
        }

        public void InsertOrUpdateClusterInfo(ClusterInfo clusterInfo)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                if (clusterInfo != null)
                {
                    IJSONDocument jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<ClusterInfo>(clusterInfo, new JsonConverter[] { new DistributionJsonConverter(), new DistributionStrategyJsonConverter() });
                    InsertOrUpdateDocumentInDatabase(jdoc, clusterInfo.Name, _dbName, _clusterInfo);

                    DatabaseCluster databaseCluster = GetDatabaseCluster(clusterInfo.Name, true);
                    if (databaseCluster != null)
                        databaseCluster.ClusterInfo = clusterInfo;

                }
            }
            ReportTimeSpent("InsertOrUpdateClusterInfo", start);
        }

        public void InsertOrUpdateDistributionStrategy(ClusterInfo clusterInfo, string database, string collection)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                if (clusterInfo != null)
                {
                    IDistributionStrategy strategy = clusterInfo.GetDatabase(database).GetCollection(collection).DistributionStrategy;
                    IJSONDocument jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<IDistributionStrategy>(strategy, new JsonConverter[] { new DistributionJsonConverter(), new DistributionStrategyJsonConverter() });

                    InsertOrUpdateDocumentInDatabase(jdoc, clusterInfo.Name, _dbName, _ciDistributionInfo, GetCollectionInfoName(clusterInfo.Name, database, collection));

                    DatabaseCluster databaseCluster = GetDatabaseCluster(clusterInfo.Name, true);
                    if (databaseCluster != null)
                        databaseCluster.AddOrUpdateDistributionStrategy(database, collection, strategy);
                }
                ReportTimeSpent("InsertOrUpdateDistribution", start);
            }
        }

        public void InsertOrUpdateBucketStats(string cluster, string database, string collection, IDictionary<HashMapBucket,BucketStatistics> updatedBucketStatisticses)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                if (updatedBucketStatisticses != null)
                {
                    IEnumerator<KeyValuePair<HashMapBucket, BucketStatistics>> ide = updatedBucketStatisticses.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        BucketStatistics bucketStatistics = ide.Current.Value;
                        int bucketId = ide.Current.Key.BucketId;
                        IJSONDocument jsonDocument = Common.JsonSerializer.Serialize(bucketStatistics);
                        jsonDocument.Key = GetBucketStatsKey(cluster, database, collection, bucketId);
                        InsertOrUpdateDocumentInDatabase(jsonDocument, cluster, _dbName, _ciBucketStatistics,
                            jsonDocument.Key);
                    }

                    //DatabaseCluster databaseCluster = GetDatabaseCluster(clusterInfo.Name, true);
                    //if (databaseCluster != null)
                    //    databaseCluster.AddOrUpdateDistributionStrategy(database, collection, strategy);
                }
                ReportTimeSpent("InsertOrUpdateBucketStats", start);
            }
        }


        public void UpdateBucketStatus(string cluster, string database, string collection, ArrayList bucketList, byte status, string shard = null)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                if (bucketList != null && bucketList.Count>0)
                {
                    //string part1="UPDATE " + this._ciDistributionInfo + " SET (DistributionMgr" +
                    //             ".InstalledHashMap.status=" + status;
                    //string part2 = ",DistributionMgr.InstalledHashMap.Current='" + shard + "'";
                    
                    //string query = null;
                    //query = part1;
                    //if (!String.IsNullOrEmpty(shard)) 
                    //{
                    //    query += part2;
                    //}
                    //query +=") WHERE _key = '"+GetCollectionInfoName(cluster,database,collection)+"' AND DistributionMgr.InstalledHashMap.Bid IN (" + bucketList[0] + ")";


                    string query = GetQuery(cluster, shard, database, collection, status);
                    ExecuteNonQuery(database, collection, query);
                    //IEnumerator ide = bucketList.GetEnumerator();
                    //while (ide.MoveNext())
                    //{                     
                    //    int bucketId = (int)ide.Current;
                    //    IJSONDocument jsonDocument = Common.JsonSerializer.Serialize(bucketList);
                    //    jsonDocument.Key = GetBucketStatsKey(cluster, database, collection, bucketId);
                    //    InsertOrUpdateDocumentInDatabase(jsonDocument, cluster, _dbName, _ciDistributionInfo,
                    //        jsonDocument.Key);
                    //}
                }
                ReportTimeSpent("UpdateBucketStatus", start);
            }
        }

        private String GetQuery(String cluster,String shard,String database,String collection,byte status)
        {
            string part1 = "UPDATE " + this._ciDistributionInfo + " SET (NonShardedDistribution.Bucket.status=" + status;
            string part2 = ",NonShardedDistribution.Bucket.Current='" + shard + "'";

            string query = null;
            query = part1;
            if (!String.IsNullOrEmpty(shard))
            {
                query += part2;
            }
            query += ") WHERE _key = '" + GetCollectionInfoName(cluster, database, collection) + "'";

            return query;
        }

        internal void SetDistributionStrategy(string cluster, string database, string collection, IDistributionStrategy distribution)
        {
            ClusterInfo info = GetClusterInfo(cluster);

            lock (this)
            {
                if (info != null)
                {
                    DatabaseInfo dbInfo = info.GetDatabase(database);

                    if (dbInfo != null)
                    {
                        CollectionInfo collInfo = dbInfo.GetCollection(collection);

                        if (collInfo != null)
                        {
                            collInfo.SetDistributionStrategy(null, distribution);
                            InsertOrUpdateClusterInfo(info);
                            InsertOrUpdateDistributionStrategy(info, database, collection);
                        }
                    }
                }
            }
        }

        private void ExecuteNonQuery(string database, string collection,string queryText)
        {
            Query query = new Query();
            query.QueryText = queryText;
            //query.Parameters = (List<IParameter>)parameters;

            WriteQueryOperation writeQueryOperation = new WriteQueryOperation();
            writeQueryOperation.Database = database;
            writeQueryOperation.Collection = collection;
            writeQueryOperation.Query = query;
            _dbStore.ExecuteNonQuery(writeQueryOperation);
        }

        private void InsertOrUpdateDocumentInDatabase(IJSONDocument document, string cluster, string database, string collection, string documentKey = null)
        {
            IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
            bool found = false;
            if (documentKey != null)
                document.Key = documentKey;

            found = FindDocument(document.Key, collection);
            if (found)
            {
                IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                replaceOperation.Collection = collection;
                replaceOperation.Database = database;
                jsonDocuments.Add(document);
                replaceOperation.Documents = jsonDocuments;
                _dbStore.ReplaceDocuments(replaceOperation);
            }
            else
            {

                IDocumentsWriteOperation insertOp = new InsertDocumentsOperation();
                insertOp.Collection = collection;
                insertOp.Database = database;
                jsonDocuments.Add(document);
                insertOp.Documents = jsonDocuments;
                _dbStore.InsertDocuments(insertOp);
            }
        }

        private string GetCollectionInfoName(string cluster, string database, string collection)
        {
            return (cluster.ToLower() + "." + database.ToLower() + "." + collection.ToLower());
        }

        private string GetBucketStatsKey(string cluster, string database, string collection, int bucketId)
        {
            return (cluster.ToLower() + "." + database.ToLower() + "." + collection.ToLower() + "." + bucketId);
        }

        public void RemoveClusterInfo(string cluster)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;
                //JsonSerializer<ClusterInfo> serializer = new JsonSerializer<ClusterInfo>();
                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_clusterInfo];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (cluster != null)
                {
                    doc.Key = cluster.ToLower();
                    found = FindDocument(cluster, _clusterInfo);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _clusterInfo;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }

                    DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, true);
                    if (databaseCluster != null & databaseCluster.ClusterInfo != null)
                        databaseCluster.ClusterInfo = null;
                }
            }
            ReportTimeSpent("RemoveClusterInfo", start);
        }



        public Membership[] GetAllMembershipData()
        {
            DateTime start = DateTime.Now;
            Membership[] membershipInfo = null;
            lock (_lock)
            {
                if (_membershipLoaded)
                {
                    //JsonSerializer<Membership> serializer = new JsonSerializer<Membership>();
                    if (_databaseClusters != null && _databaseClusters.Count > 0)
                    {
                        List<Membership> memberships = new List<Membership>();

                        foreach (DatabaseCluster databaseCluster in _databaseClusters.Values)
                        {
                            if (databaseCluster != null && databaseCluster.Configuration != null)
                            {
                                foreach (string shard in databaseCluster.Configuration.Deployment.Shards.Keys)
                                {
                                    Membership membership = databaseCluster.GetMembership(shard);
                                    if (membership != null)
                                        memberships.Add(databaseCluster.GetMembership(shard));
                                }
                            }
                        }

                        return memberships.ToArray();
                    }
                }
                else
                {
                    ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_membersCollection];
                    membershipInfo = new Membership[collection.Count()];
                    int i = 0;

                    foreach (IJSONDocument doc in collection)
                    {
                        Membership membership = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<Membership>((JSONDocument)doc);
                        membershipInfo[i] = membership;
                        AddMembership(membership);
                        i++;
                    }
                    _membershipLoaded = true;
                }
            }
            ReportTimeSpent("GetAllMembershipData", start);
            return membershipInfo;
        }

        private void AddMembership(Membership membership)
        {
            DatabaseCluster cluster = GetDatabaseCluster(membership.Cluster, true);
            cluster.AddOrUpdateMembersip(membership.Shard, membership);
        }

        public Membership GetMembershipData(string cluster, string shard)
        {
            DateTime start = DateTime.Now;
            Membership membershipInfo = null;
            string documentKey = cluster.ToLower() + ":" + shard.ToLower();
            DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, false);

            if (databaseCluster != null)
            {
                membershipInfo = databaseCluster.GetMembership(shard);
            }

            if (membershipInfo == null)
            {

                IJSONDocument jsonDocument = FetchFromDatabase(cluster, _dbName, _membersCollection, documentKey);

                if (jsonDocument != null)
                {
                    membershipInfo = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<Membership>((JSONDocument)jsonDocument);
                }

                if (membershipInfo != null)
                {
                    AddMembership(membershipInfo);
                }
            }

            ReportTimeSpent("GetMembershipData", start);
            return membershipInfo;
        }

        public void InsertOrUpdateMembershipData(Membership membership)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;


                if (membership != null)
                {
                    IJSONDocument jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<Membership>(membership);
                    InsertOrUpdateDocumentInDatabase(jdoc, membership.Cluster, _dbName, _membersCollection);

                    DatabaseCluster databaseCluster = GetDatabaseCluster(membership.Cluster, true);
                    if (databaseCluster != null)
                        databaseCluster.AddOrUpdateMembersip(membership.Shard, membership);

                }
            }
            ReportTimeSpent("InsertOrUpdateMembershipData", start);

        }

        public void RemoveMembershipData(string cluster, string shard)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;
                //JsonSerializer<Membership> serializer = new JsonSerializer<Membership>();
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (cluster != null)
                {
                    doc.Key = cluster.ToLower() + ":" + shard.ToLower();
                    found = FindDocument(doc.Key, _membersCollection);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _membersCollection;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }

                    DatabaseCluster databaseCluster = GetDatabaseCluster(cluster, true);
                    if (databaseCluster != null)
                        databaseCluster.RemoveMembership(shard);

                }
            }
            ReportTimeSpent("RemoveMembershipData", start);
        }

        public bool FindDocument(string name, string colname)
        {
            DateTime start = DateTime.Now;
            bool found = false;
            IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
            JSONDocument doc = new JSONDocument();

            if (name != null)
            {
                doc.Key = name.ToLower();
                jsonDocuments.Add(doc);
                IGetOperation getOperation = new GetDocumentsOperation();
                getOperation.Database = _dbName;
                getOperation.Collection = colname;
                getOperation.DocumentIds = jsonDocuments;
                IGetResponse response = _dbStore.GetDocuments(getOperation);
                IDataChunk dataChunk = response.DataChunk;
                if (dataChunk.Documents.Count != 0)
                {
                    found = true;
                }
                else
                {
                    found = false;
                }
            }
            ReportTimeSpent("FindDocument", start);
            return found;
        }

        public bool FindDocument(string name, string colname, out JSONDocument doc)
        {
            DateTime start = DateTime.Now;
            bool found = false;
            IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
            doc = new JSONDocument();

            if (name != null)
            {
                doc.Key = name.ToLower();
                jsonDocuments.Add(doc);
                IGetOperation getOperation = new GetDocumentsOperation();
                getOperation.Database = _dbName;
                getOperation.Collection = colname;
                getOperation.DocumentIds = jsonDocuments;
                IGetResponse response = _dbStore.GetDocuments(getOperation);
                IDataChunk dataChunk = response.DataChunk;
                if (dataChunk.Documents.Count != 0)
                {
                    doc = dataChunk.Documents[0] as JSONDocument;
                    found = true;
                }
                else
                {
                    doc = null;
                    found = false;
                }
            }
            ReportTimeSpent("FindDocument2", start);
            return found;
        }

        public void Dispose()
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                ((SystemDatabaseStore)_dbStore).Dispose();
                _dbStore = null;
            }
        }

        #region Security configuration related
        #region Resource information related
        public IResourceItem[] GetAllResourcesSecurityInformation()
        {
            DateTime start = DateTime.Now;
            IResourceItem[] resources = null;
            int i = 0;
            //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
            if (_securityResourceLoaded)
            {
                int resourceCount = 0;
                foreach(string clusterName in _clusters)
                {
                    DatabaseCluster cluster = GetDatabaseCluster(clusterName, false);
                    resourceCount += cluster.SecurityResources.Count;
                }
                resources = new ResourceItem[resourceCount];
                foreach(string clusterName in _clusters)
                {
                    DatabaseCluster cluster = GetDatabaseCluster(clusterName, false);
                    foreach (IResourceItem resourceItem in cluster.SecurityResources.Values)
                    {
                        resources[i] = resourceItem;
                    }
                }
            }
            else
            {
                bool allClustersLoaded = true;
                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_securityInformationCollection];

                resources = new ResourceItem[collection.Count()];
                foreach (IJSONDocument doc in collection)
                {
                    IResourceItem resourceItem = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IResourceItem>((JSONDocument)doc);
                    resources[i] = resourceItem;
                    AddSecurityResource(resourceItem.ClusterName, resourceItem);
                    if (GetDatabaseCluster(resourceItem.ClusterName, false) == null)
                        allClustersLoaded = false;
                    else
                        _clusters.Add(resourceItem.ClusterName);
                    i++;
                }
                _securityResourceLoaded = allClustersLoaded;
            }
            ReportTimeSpent("GetAllResourcesSecurityInformation", start);
            return resources;
        }

        public IResourceItem GetResourceSecurityInformation(string resource)
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
            IResourceItem resourceItem = null;
            JSONDocument doc = new JSONDocument();
            bool found = FindDocument(resource, _securityInformationCollection, out doc);
            if (found)
            {
                resourceItem = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IResourceItem>(doc);
            }
            ReportTimeSpent("GetResourceSecurityInformation", start);
            return resourceItem;
        }

        public void InsertOrUpdateResourceSecurityInformation(IResourceItem resourceItem)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;
                AddSecurityResource(MiscUtil.CLUSTERED, resourceItem);
                IDocumentsWriteOperation insertOperation = new InsertDocumentsOperation();
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                if (resourceItem != null)
                {
                    IJSONDocument jdoc = Common.JsonSerializer.Serialize(resourceItem);
                    jdoc.Key = resourceItem.ResourceId.Name;
                    JSONDocument jsonDoc = null;
                    bool found = FindDocument(resourceItem.ResourceId.Name, _securityInformationCollection, out jsonDoc);
                    if (found)
                    {
                        JSONDocument[] oldResources = jsonDoc.GetArray<JSONDocument>("SubResources");
                        if (oldResources != null)
                        {
                            JSONDocument[] newResource = jdoc.GetArray<JSONDocument>("SubResources");
                            if (newResource != null)
                            {
                                JSONDocument[] mergedResources = new JSONDocument[newResource.Length + oldResources.Length];
                                Array.Copy(oldResources, mergedResources, oldResources.Length);
                                Array.Copy(newResource, 0, mergedResources, oldResources.Length, newResource.Length);

                                jdoc["SubResources"] = mergedResources;
                            }
                        }

                        IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                        replaceOperation.Collection = _securityInformationCollection;
                        replaceOperation.Database = _dbName;
                        jsonDocuments.Add(jdoc);
                        replaceOperation.Documents = jsonDocuments;
                        _dbStore.ReplaceDocuments(replaceOperation);
                        //TODO for updating document only deleting previous document require some time to wait In Future this operation done with replace operation.
                    }
                    else
                    {
                        jsonDocuments.Clear();
                        jdoc = Common.JsonSerializer.Serialize(resourceItem);
                        jdoc.Key = resourceItem.ResourceId.Name;
                        jsonDocuments.Add(jdoc);
                        insertOperation.Documents = jsonDocuments;
                        insertOperation.Collection = _securityInformationCollection;
                        insertOperation.Database = _dbName;
                        _dbStore.InsertDocuments(insertOperation);
                    }
                }
            }
            ReportTimeSpent("InsertOrUpdateResourceSecurityInformation", start);
        }
        public void RemoveResourceSecurityInformation(string resource)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_securityInformationCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (resource != null)
                {
                    doc.Key = resource;
                    found = FindDocument(resource, _securityInformationCollection);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _securityInformationCollection;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }
                }
            }
            ReportTimeSpent("RemoveResourceSecurityInformation", start);
        }
        #endregion

        #region User information related
        public IUser[] GetAllUserInformation()
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IUser> serializer = new JsonSerializer<IUser>();
            IUser[] users = null;
            int i = 0;
            if (_loginsLoaded)
            {
                DatabaseCluster cluster = GetDatabaseCluster(MiscUtil.CLUSTERED, false);
                if(cluster != null)
                {
                    users = new IUser[cluster.Logins.Count];
                    foreach (IUser user in cluster.Logins.Values)
                    {
                        users[i] = user;
                        i++;
                    }
                }
            }
            else
            {
                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_userInformationCollection];

                users = new IUser[collection.Count()];
                foreach (IJSONDocument doc in collection)
                {
                    IUser user = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IUser>((JSONDocument)doc);
                    users[i] = user;
                    AddLogin(MiscUtil.CLUSTERED, user);
                    i++;
                }
                if (GetDatabaseCluster(MiscUtil.CLUSTERED, false) != null)
                    _loginsLoaded = true;
            }
            ReportTimeSpent("GetAllUserInformation", start);
            return users;
        }

        public IUser GetUserInformation(string user)
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IUser> serializer = new JsonSerializer<IUser>();
            IUser userInfo = null;
            JSONDocument doc = new JSONDocument();
            bool found = FindDocument(user, _userInformationCollection, out doc);
            if (found)
            {
                userInfo = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IUser>(doc);
            }
            ReportTimeSpent("GetUserInformatio", start);
            return userInfo;
        }

        public void InsertOrUpdateUserInformation(IUser userInfo)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                AddLogin(MiscUtil.CLUSTERED, userInfo);

                IDocumentsWriteOperation insertOperation = new InsertDocumentsOperation();
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                if (userInfo != null)
                {
                    bool found = FindDocument(userInfo.Username, _userInformationCollection);
                    IJSONDocument jdoc;
                    if (found)
                    {
                        jdoc = Common.JsonSerializer.Serialize<IUser>(userInfo);
                        jdoc.Key = userInfo.Username;
                        IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                        replaceOperation.Collection = _userInformationCollection;
                        replaceOperation.Database = _dbName;
                        jsonDocuments.Add(jdoc);
                        replaceOperation.Documents = jsonDocuments;
                        _dbStore.ReplaceDocuments(replaceOperation);
                        //TODO for updating document only deleting previous document require some time to wait In Future this operation done with replace operation.
                    }
                    else
                    {
                        jsonDocuments.Clear();
                        jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<IUser>(userInfo);
                        jdoc.Key = userInfo.Username;
                        jsonDocuments.Add(jdoc);
                        insertOperation.Documents = jsonDocuments;
                        insertOperation.Collection = _userInformationCollection;
                        insertOperation.Database = _dbName;
                        _dbStore.InsertDocuments(insertOperation);
                    }
                }
            }
            ReportTimeSpent("InsertOrUpdateUserInformation", start);
        }

        public void RemoveUserInformation(string username)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_userInformationCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (username != null)
                {
                    doc.Key = username;
                    found = FindDocument(username, _userInformationCollection);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _userInformationCollection;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }
                }
            }
            ReportTimeSpent("RemoveUserInformation", start);
        }
        #endregion

        #region Role Information related
        public IRole[] GetAllRolesInformation()
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IRole> serializer = new JsonSerializer<IRole>();
            IRole[] roles;
            ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_userInformationCollection];

            roles = new IRole[collection.Count()];
            int i = 0;
            foreach (IJSONDocument doc in collection)
            {
                roles[i] = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IRole>((JSONDocument)doc);
                i++;
            }
            ReportTimeSpent("GetAllRolesInformation", start);
            return roles;
        }

        public IRole GetRoleInformatio(string name)
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IRole> serializer = new JsonSerializer<IRole>();
            IRole roleInfo = null;
            JSONDocument doc = new JSONDocument();
            bool found = FindDocument(name, _roleInformationCollection, out doc);
            if (found)
            {
                roleInfo = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<IRole>(doc);
            }
            ReportTimeSpent("GetRoleInformatio", start);
            return roleInfo;
        }

        public void InsertOrUpdateRoleInformation(IRole role)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                IDocumentsWriteOperation insertOperation = new InsertDocumentsOperation();
                //JsonSerializer<IRole> serializer = new JsonSerializer<IRole>();
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                IJSONDocument jdoc = new JSONDocument();
                bool found = false;
                if (role != null)
                {
                    found = FindDocument(role.RoleName, _roleInformationCollection);
                    if (found)
                    {
                        jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<IRole>(role);
                        jdoc.Key = role.RoleName;
                        IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                        replaceOperation.Collection = _roleInformationCollection;
                        replaceOperation.Database = _dbName;
                        jsonDocuments.Add(jdoc);
                        replaceOperation.Documents = jsonDocuments;
                        _dbStore.ReplaceDocuments(replaceOperation);
                        //TODO for updating document only deleting previous document require some time to wait In Future this operation done with replace operation.
                    }
                    else
                    {
                        jsonDocuments.Clear();
                        jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<IRole>(role);
                        jdoc.Key = role.RoleName;
                        jsonDocuments.Add(jdoc);
                        insertOperation.Documents = jsonDocuments;
                        insertOperation.Collection = _roleInformationCollection;
                        insertOperation.Database = _dbName;
                        _dbStore.InsertDocuments(insertOperation);
                    }

                }
            }
            ReportTimeSpent("InsertOrUpdateRoleInformation", start);
        }

        public void RemoveRoleInformation(string rolename)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_roleInformationCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (rolename != null)
                {
                    doc.Key = rolename;
                    found = FindDocument(rolename, _roleInformationCollection);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _roleInformationCollection;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }
                }
            }
            ReportTimeSpent("RemoveRoleInformation", start);
        }
        #endregion
        #endregion

        #region Recovery Operations
        public void InsertOrUpdateRecoveryJobData(ClusterJobInfoObject job)
        {
            try
            {
                DateTime start = DateTime.Now;
                lock (_lock)
                {
                    if (_dbStore == null) return;

                    if (job != null)
                    {
                        IJSONDocument jdoc = Alachisoft.NosDB.Common.JsonSerializer.Serialize<ClusterJobInfoObject>(job);
                        InsertOrUpdateDocumentInDatabase(jdoc, job.ActiveConfig.Cluster, _dbName, _recoveryInformationCollection);
                        
                        ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_recoveryInformationCollection];
                       
                    }
                }
                ReportTimeSpent("InsertOrUpdateRecoveryJobData", start);
            }
            catch(Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigStore.InsertRecovery() ", exp.ToString());
            }
        }

        public void RemoveRecoveryjobData(string id)
        {
            DateTime start = DateTime.Now;
            lock (_lock)
            {
                if (_dbStore == null) return;

                ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_recoveryInformationCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (id != null)
                {
                    doc.Key = id;
                    found = FindDocument(id, _recoveryInformationCollection);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = _userInformationCollection;
                        deleteOperation.Database = _dbName;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _dbStore.DeleteDocuments(deleteOperation);
                    }
                }
            }
            ReportTimeSpent("RemoveRecoveryInformation", start);
        }

        public ClusterJobInfoObject GetRecoveryJobData(string id)
        {
            DateTime start = DateTime.Now;
            ClusterJobInfoObject jobInfo = null;
            JSONDocument doc = new JSONDocument();
            bool found = FindDocument(id, _recoveryInformationCollection, out doc);
            if (found)
            {
                jobInfo = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<ClusterJobInfoObject>(doc);
            }
            ReportTimeSpent("GetRecoveryJobData", start);
            return jobInfo;
        }

        public ClusterJobInfoObject[] GetAllRecoveryJobData()
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
            ClusterJobInfoObject[] recoveryJob;
            ICollectionStore collection = ((SystemDatabaseStore)_dbStore).Collections[_recoveryInformationCollection];

            recoveryJob = new ClusterJobInfoObject[collection.Count()];
            int i = 0;
            foreach (IJSONDocument doc in collection)
            {
                recoveryJob[i] = Alachisoft.NosDB.Common.JsonSerializer.Deserialize<ClusterJobInfoObject>((JSONDocument)doc);
                i++;
            }
            ReportTimeSpent("GetAllRecoveryJobData", start);
            return recoveryJob;
        }

        public ClusterJobInfoObject[] GetRecoveryJobData(Query query)
        {
            DateTime start = DateTime.Now;
            //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
            List<ClusterJobInfoObject> recoveryJob=new List<ClusterJobInfoObject>();
                     
            try
            {
                ReadQueryOperation readQueryOperation = new ReadQueryOperation();
                readQueryOperation.Database = _dbName;
                readQueryOperation.Collection = _recoveryInformationCollection;
                readQueryOperation.Query = query;

                ReadQueryResponse readQueryResponse = (ReadQueryResponse)_dbStore.ExecuteReader(readQueryOperation);
                if (readQueryResponse.IsSuccessfull)
                {
                    var reader = new CollectionReader((DataChunk)readQueryResponse.DataChunk, _dbStore, _dbName, _recoveryInformationCollection);
                    if (reader != null)
                    {
                        while (reader != null && reader.ReadNext() && reader.GetDocument() != null)
                        {
                            ClusterJobInfoObject jobInfo = Common.JsonSerializer.Deserialize<ClusterJobInfoObject>(reader.GetDocument() as JSONDocument);
                            recoveryJob.Add(jobInfo);
                        }
                        reader.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigStore.GetDifRegistered() ", ex.ToString());
            }
            return recoveryJob.ToArray();
        }
        #endregion


        private void ReportTimeSpent(String method, DateTime startTime)
        {
            TimeSpan t = DateTime.Now - startTime;

            if (t.TotalSeconds >= 3)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationStore ", method + " WARNING : more time is spent in method (" + method + ") . time taken(s) " + t.TotalSeconds);

            }

        }

        


        #region                     --- Inner classes ----                          /

        internal class DatabaseCluster:ICompactSerializable
        {
            private Dictionary<string, IDistributionStrategy> _distributions = new Dictionary<string, IDistributionStrategy>(StringComparer.InvariantCultureIgnoreCase);
            private Dictionary<string, Membership> _membership = new Dictionary<string, Membership>(StringComparer.InvariantCultureIgnoreCase);
            private Dictionary<string, IResourceItem> _securityResources = new Dictionary<string, IResourceItem>(StringComparer.InvariantCultureIgnoreCase);
            private Dictionary<string, IUser> _logins = new Dictionary<string, IUser>(StringComparer.CurrentCultureIgnoreCase);

            public ClusterConfiguration Configuration { get; set; }
            public ClusterInfo ClusterInfo { get; set; }
            public Dictionary<string, IResourceItem> SecurityResources { get { return _securityResources; } }
            public Dictionary<string, IUser> Logins { get { return _logins; } }

            public IList<Membership> Memberships
            {
                get
                {
                    List<Membership> memberships = new List<Membership>();

                    if(_membership.Count>0)
                    {
                        memberships.AddRange(_membership.Values);
                    }

                    return memberships;
                }
            }

            public void AddOrUpdateDistributionStrategy(string database, string collection, IDistributionStrategy strategy)
            {
                if (string.IsNullOrEmpty(database))
                    throw new ArgumentNullException("database");

                if (string.IsNullOrEmpty(collection))
                    throw new ArgumentNullException("collection");

                if (strategy == null)
                    throw new ArgumentNullException("strategy");

                string key = GetKey(database, collection);
                if (!_distributions.ContainsKey(key))
                {
                    _distributions.Add(key, strategy);
                }
                else
                    _distributions[key] = strategy;
            }

            public IDistributionStrategy GetDistributionStrategy(string database, string collection)
            {
                string key = GetKey(database, collection);
                if (_distributions.ContainsKey(key))
                {
                    return _distributions[key];
                }
                return null;
            }

            public void AddOrUpdateMembersip(string shard, Membership membership)
            {
                if (string.IsNullOrEmpty(shard))
                    throw new ArgumentNullException("shard");


                if (membership == null)
                    throw new ArgumentNullException("membership");

                if (!_membership.ContainsKey(shard))
                {
                    _membership.Add(shard, membership);
                }
                else
                    _membership[shard] = membership;
            }

            public Membership GetMembership(string shard)
            {
                if (_membership.ContainsKey(shard))
                {
                    return _membership[shard];
                }
                return null;
            }

            

            private string GetKey(string database, string collection)
            {
                return database + ":" + collection;
            }

            internal void RemoveMembership(string shard)
            {
                if (_membership.ContainsKey(shard))
                    _membership.Remove(shard);
            }

         
            public void AddOrUpdateSecurityResource(string resourceId, IResourceItem resourceItem)
            {
                if (resourceId == null)
                    throw new ArgumentNullException("resourceId");


                if (resourceItem == null)
                    throw new ArgumentNullException("resourceItem");

                if (!_securityResources.ContainsKey(resourceId))
                {
                    _securityResources.Add(resourceId, resourceItem);
                }
                else
                    _securityResources[resourceId] = resourceItem;
            }

            public IResourceItem GetSecurityResource(string resourceId)
            {
                if (_securityResources.ContainsKey(resourceId))
                {
                    return _securityResources[resourceId];
                }
                return null;
            }

            internal void RemoveSecurityInformation(string resourceId)
            {
                if (_securityResources.ContainsKey(resourceId))
                    _securityResources.Remove(resourceId);
            }

            public void AddOrUpdateLogin(string username, IUser user)
            {
                if (username == null)
                    throw new ArgumentNullException("username");


                if (user == null)
                    throw new ArgumentNullException("user");

                if (!_logins.ContainsKey(username))
                {
                    _logins.Add(username, user);
                }
                else
                    _logins[username] = user;
            }

            public IUser GetLogin(string username)
            {
                if (_logins.ContainsKey(username))
                {
                    return _logins[username];
                }
                return null;
            }

            internal void RemoveLogin(string username)
            {
                if (_logins.ContainsKey(username))
                    _logins.Remove(username);
            }

            public void Reset()
            {
                Configuration = null;
                ClusterInfo = null;
                _membership.Clear();
                _distributions.Clear();
                _securityResources.Clear();
            }

            public void Deserialize(Common.Serialization.IO.CompactReader reader)
            {
                this.Configuration = reader.ReadObject() as ClusterConfiguration;
                this.ClusterInfo = reader.ReadObject() as ClusterInfo;
                this._distributions = Alachisoft.NosDB.Common.Util.SerializationUtility.DeserializeDictionary<string, IDistributionStrategy>(reader);
                this._membership = Alachisoft.NosDB.Common.Util.SerializationUtility.DeserializeDictionary<string, Membership>(reader);
                this._securityResources = Alachisoft.NosDB.Common.Util.SerializationUtility.DeserializeDictionary<string, IResourceItem>(reader);
                this._logins = Alachisoft.NosDB.Common.Util.SerializationUtility.DeserializeDictionary<string, IUser>(reader);
            }

            public void Serialize(Common.Serialization.IO.CompactWriter writer)
            {
                writer.WriteObject(this.Configuration);
                writer.WriteObject(this.ClusterInfo);
                Alachisoft.NosDB.Common.Util.SerializationUtility.SerializeDictionary<string, IDistributionStrategy>(this._distributions, writer);
                Alachisoft.NosDB.Common.Util.SerializationUtility.SerializeDictionary<string, Membership>(this._membership, writer);
                Alachisoft.NosDB.Common.Util.SerializationUtility.SerializeDictionary<string, IResourceItem>(this._securityResources, writer);
                Alachisoft.NosDB.Common.Util.SerializationUtility.SerializeDictionary<string, IUser>(this._logins, writer);
            }
        }

        public class Transaction : IConfigurationStore,ICompactSerializable
        {
            private string _id;
            private ConfigurationStore _store;
            private DatabaseCluster _cluster = new DatabaseCluster();
            private string _clusterName;
            private List<Operation> _operations = new List<Operation>();
            private bool _committed;
            private bool _rollbacked;
            private bool _started;
            private Thread _thread;

            /// <summary>
            /// This constructor is provided only for serialization. Always call the parameterized constructor
            /// </summary>
            public Transaction() { }
            /// <summary>
            /// Initiate a transaction object for a given transaction.
            /// </summary>
            /// <param name="clusterName"></param>
            /// <param name="store"></param>
            public Transaction(string clusterName, ConfigurationStore store)
            {
                _clusterName = clusterName.ToLower();
                _store = store;
                _id = Guid.NewGuid().ToString();
            }

            public bool CloneValues { get; set; }

            public void SetStore(ConfigurationStore store)
            {
                _store = store;
            }
            public string Cluster { get { return _clusterName; } }

            public bool IsCommittedOrRollbacked { get { lock (this) { return _committed || _rollbacked; } } }

            public void Initialize()
            {

            }

            public bool Started { get { lock (this) { return _started; } } }

            public bool IsOwnedByCurrentThread
            {
                get { lock (this) { return _thread != null ? _thread.Equals(Thread.CurrentThread) : false; } }
            }

            public ClusterInfo[] GetAllClusterInfo()
            {
                return _store.GetAllClusterInfo();
            }

            public ClusterInfo GetClusterInfo(string cluster)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        return _store.GetClusterInfo(cluster);
                    }
                    ClusterInfo clusterInfo = _cluster.ClusterInfo; ;

                    if (clusterInfo == null)
                    {
                        clusterInfo = _store.GetClusterInfo(cluster);

                        if (clusterInfo != null)
                        {
                            if (CloneValues)
                                clusterInfo = clusterInfo.Clone() as ClusterInfo;
                            _cluster.ClusterInfo = clusterInfo;
                        }
                    }
                    return clusterInfo;
                }
            }

            public void InsertOrUpdateClusterInfo(ClusterInfo clusterInfo)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, clusterInfo.Name, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateClusterInfo(clusterInfo);
                        return;
                    }
                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.ClusterInfoModified });
                        _cluster.ClusterInfo = clusterInfo;
                    }
                }
            }

            public void RemoveClusterInfo(string cluster)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.RemoveClusterInfo(cluster);
                        return;
                    }
                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.ClusterInfoDeleted, Arguments = cluster });
                        _cluster.ClusterInfo = null;
                    }
                }
            }

            public void GetAllDistributionStrategies(ClusterInfo[] clusterinfo)
            {
                _store.GetAllDistributionStrategies(clusterinfo);
            }

            public IDistributionStrategy GetDistributionStrategy(string cluster, string database, string collection)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        return _store.GetDistributionStrategy(cluster, database, collection);
                    }
                    IDistributionStrategy distribution = _cluster.GetDistributionStrategy(database, collection);

                    if (distribution == null)
                    {
                        distribution = _store.GetDistributionStrategy(cluster, database, collection);

                        if (distribution != null)
                        {
                            if (CloneValues)
                                distribution = distribution.Clone() as IDistributionStrategy;
                            _cluster.AddOrUpdateDistributionStrategy(database, collection, distribution);
                        }
                    }
                    return distribution;
                }
            }

            public void InsertOrUpdateDistributionStrategy(ClusterInfo clusterInfo, string database, string collection)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, clusterInfo.Name, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateDistributionStrategy(clusterInfo, database, collection);
                        return;
                    }
                    if (_cluster != null)
                    {
                        IDistributionStrategy distribution = clusterInfo.GetDatabase(database).GetCollection(collection).DistributionStrategy;

                        _operations.Add(new Operation() { OperationType = OperationType.ClusterDistributionStrategyModified, Database = database, Collection = collection, Arguments = clusterInfo });
                        _cluster.AddOrUpdateDistributionStrategy(database, collection, distribution);
                    }
                }
            }

            public void InsertOrUpdateBucketStats(string cluster, string database, string collection, IDictionary<HashMapBucket, BucketStatistics> updatedBucketStatisticses)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateBucketStats(cluster, database, collection, updatedBucketStatisticses);
                        return;
                    }
                    if (_cluster != null)
                    {
                        
                       //  IDistributionStrategy distribution = clusterInfo.GetDatabase(database).GetCollection(collection).DistributionStrategy;

                        _operations.Add(new Operation { OperationType = OperationType.BucketStatsModified, Cluster = cluster, Database = database, Collection = collection, Arguments = updatedBucketStatisticses });
                        //_cluster.AddOrUpdateDistributionStrategy(database, collection, distribution);
                    }
                }
            }




            //public void UpdateBucketStatus(string cluster, string database, string collection, ArrayList bucketList, byte status, string shard = null)
            //{
            //    lock (this)
            //    {
            //        if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
            //        {
            //            _store.UpdateBucketStatus(cluster, database, collection, bucketList, status, shard);
            //            return;
            //        }
            //        if (_cluster != null)
            //        {
            //            _operations.Add(new Operation { OperationType = OperationType.BucketStatusModified, Cluster = cluster, Database = database, Collection = collection, Arguments = new object[] { bucketList, status, shard } });
            //        }
            //    }
            //}           

            public Membership[] GetAllMembershipData()
            {
                return _store.GetAllMembershipData();
            }

            public Membership GetMembershipData(string cluster, string shard)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        return _store.GetMembershipData(cluster, shard);
                    }
                    Membership membership = _cluster.GetMembership(shard);

                    if (membership == null)
                    {
                        membership = _store.GetMembershipData(cluster, shard);

                        if (membership != null)
                        {
                            if (CloneValues)
                                membership = membership.Clone() as Membership;
                            _cluster.AddOrUpdateMembersip(shard, membership);
                        }
                    }
                    return membership;
                }
            }

            public void InsertOrUpdateMembershipData(Membership membership)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, membership.Cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateMembershipData(membership);
                        return;
                    }
                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.ClusterMembershipModified, Arguments = membership });
                        _cluster.AddOrUpdateMembersip(membership.Shard, membership);
                    }
                }

            }

            public void RemoveMembershipData(string cluster, string shard)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.RemoveMembershipData(cluster, shard);
                        return;
                    }
                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.ClusterMembershipDeleted, Shard = shard, Arguments = cluster });
                        _cluster.RemoveMembership(shard);
                    }
                }
            }

            public ClusterConfiguration[] GetAllClusterConfiguration()
            {
                return _store.GetAllClusterConfiguration();
            }

            public ClusterConfiguration GetClusterConfiguration(string cluster)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        return _store.GetClusterConfiguration(cluster);
                    }
                    ClusterConfiguration clusterConfiguraton = _cluster.Configuration; ;

                    if (clusterConfiguraton == null)
                    {
                        clusterConfiguraton = _store.GetClusterConfiguration(cluster);

                        if (clusterConfiguraton != null)
                        {
                            if (CloneValues)
                                clusterConfiguraton = clusterConfiguraton.Clone() as ClusterConfiguration;
                            _cluster.Configuration = clusterConfiguraton;
                        }
                    }
                    return clusterConfiguraton;
                }
            }

            public void InsertOrUpdateClusterConfiguration(ClusterConfiguration configuration)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, configuration.Name, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateClusterConfiguration(configuration);
                        return;
                    }

                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.ClusterConfigurationModified });
                        _cluster.Configuration = configuration;
                    }
                }
            }

            public void RemoveClusterConfiguration(string cluster)
            {
                lock (this)
                {
                    if (String.Compare(_clusterName, cluster, true) != 0 || IsCommittedOrRollbacked || !Started)
                    {
                        _store.RemoveClusterConfiguration(cluster);
                        return;
                    }

                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.ClusterConfigurationDeleted, Arguments = cluster });
                        _cluster.Configuration = null;
                    }
                }
            }

            public IResourceItem[] GetAllResourcesSecurityInformation()
            {
                return _store.GetAllResourcesSecurityInformation();
            }

            public IResourceItem GetResourceSecurityInformation(string resource)
            {
                lock (this)
                {
                    if (IsCommittedOrRollbacked || !Started)
                    {
                        return _store.GetResourceSecurityInformation(resource);
                    }
                    IResourceItem resourceItem = _cluster.GetSecurityResource(resource);

                    if (resourceItem == null)
                    {
                        resourceItem = _store.GetResourceSecurityInformation(resource);

                        if (resourceItem != null)
                        {
                            if (CloneValues)
                                resourceItem = resourceItem.Clone() as IResourceItem;
                            _cluster.AddOrUpdateSecurityResource(resource, resourceItem);
                        }
                    }
                    return resourceItem;
                }
            }

            public void InsertOrUpdateResourceSecurityInformation(IResourceItem resourceItem)
            {
                lock (this)
                {
                    if (IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateResourceSecurityInformation(resourceItem);
                        return;
                    }
                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.SecurityInformationModified, Arguments = resourceItem });
                        _cluster.AddOrUpdateSecurityResource(resourceItem.ResourceId.Name, resourceItem);
                    }
                }
            }

            public void RemoveResourceSecurityInformation(string resource)
            {
                lock (this)
                {
                    if (IsCommittedOrRollbacked || !Started)
                    {
                        _store.RemoveResourceSecurityInformation(resource);
                        return;
                    }

                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.SecurityInformationDeleted, Arguments = resource });
                        _cluster.RemoveSecurityInformation(resource);
                    }
                }
            }

            public IUser[] GetAllUserInformation()
            {
                return _store.GetAllUserInformation();
            }

            public IUser GetUserInformation(string user)
            {
                lock (this)
                {
                    if (IsCommittedOrRollbacked || !Started)
                    {
                        return _store.GetUserInformation(user);
                    }
                    IUser login = _cluster.GetLogin(user);

                    if (login == null)
                    {
                        login = _store.GetUserInformation(user);

                        if (login != null)
                        {
                            if (CloneValues)
                                login = login.Clone() as IUser;
                            _cluster.AddOrUpdateLogin(user, login);
                        }
                    }
                    return login;
                }
            }

            public void InsertOrUpdateUserInformation(IUser userInfo)
            {
                lock (this)
                {
                    if (IsCommittedOrRollbacked || !Started)
                    {
                        _store.InsertOrUpdateUserInformation(userInfo);
                        return;
                    }
                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.LoginModified, Arguments = userInfo });
                        _cluster.AddOrUpdateLogin(userInfo.Username, userInfo);
                    }
                }
            }

            public void RemoveUserInformation(string username)
            {
                lock (this)
                {
                    if (IsCommittedOrRollbacked || !Started)
                    {
                        _store.RemoveUserInformation(username);
                        return;
                    }

                    if (_cluster != null)
                    {
                        _operations.Add(new Operation() { OperationType = OperationType.LoginDeleted, Arguments = username });
                        _cluster.RemoveSecurityInformation(username);
                    }
                }
            }

            public IRole[] GetAllRolesInformation()
            {
                return _store.GetAllRolesInformation();
            }

            public IRole GetRoleInformatio(string name)
            {
                return _store.GetRoleInformatio(name);
            }

            public void InsertOrUpdateRoleInformation(IRole userInfo)
            {
                _store.InsertOrUpdateRoleInformation(userInfo);
            }

            public void RemoveRoleInformation(string name)
            {
                _store.RemoveRoleInformation(name);
            }

            public void Dispose()
            {

            }

            public void Commit()
            {
                if(_cluster == null)
                    LoggerManager.Instance.CONDBLogger.Info("Transaction is null", "cluster is NULL");


                if (_operations == null)
                    LoggerManager.Instance.CONDBLogger.Info("operations  is null", "operations is NULL");

                lock (this)
                {
                    try
                    {
                        foreach (Operation operation in _operations)
                        {
                            int retries = 3;

                            while (retries > 0)
                            {
                                try
                                {
                                    CommitToStore(operation);
                                    break;
                                }
                                catch (Exception e)
                                {
                                    if (_cluster == null)
                                        LoggerManager.Instance.CONDBLogger.Error("Transaction.Commit() ", e.ToString());


                                     retries--;
                                    if (retries == 0)
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        _committed = true;
                        Reset();
                    }
                }

            }

            private void CommitToStore(Operation operation)
            {
                switch (operation.OperationType)
                {
                    case OperationType.ClusterConfigurationModified:
                        if (_cluster.Configuration != null)
                        {
                            _store.InsertOrUpdateClusterConfiguration(_cluster.Configuration);
                            _cluster.Configuration = null;
                        }
                        break;

                    case OperationType.ClusterInfoModified:
                        if (_cluster.ClusterInfo != null)
                        {
                            _store.InsertOrUpdateClusterInfo(_cluster.ClusterInfo);
                            _cluster.ClusterInfo = null;
                        }
                        break;

                    case OperationType.ClusterDistributionStrategyModified:
                        if (operation.Arguments != null)
                        {
                            _store.InsertOrUpdateDistributionStrategy((ClusterInfo)operation.Arguments, operation.Database, operation.Collection);
                        }
                        break;

                    case OperationType.ClusterMembershipModified:
                        if (operation.Arguments != null)
                        {
                            _store.InsertOrUpdateMembershipData(operation.Arguments as Membership);
                        }
                        break;

                    case OperationType.ClusterConfigurationDeleted:
                        if (operation.Arguments != null)
                        {
                            _store.RemoveClusterConfiguration(operation.Arguments as string);
                        }
                        break;
                    case OperationType.ClusterInfoDeleted:
                        if (operation.Arguments != null)
                        {
                            _store.RemoveClusterInfo(operation.Arguments as string);
                        }
                        break;

                    case OperationType.ClusterMembershipDeleted:
                        if (operation.Arguments != null)
                        {
                            _store.RemoveMembershipData(operation.Arguments as string, operation.Shard);
                        }
                        break;

                    case OperationType.ClusterDistributionStrategyDeleted:
                        //if (operation.Aguments != null)
                        //{
                        //    _store.RemoveMember(operation.Aguments as string);
                        //}
                        break;
                    case OperationType.SecurityInformationDeleted:
                        if (operation.Arguments != null)
                        {
                            _store.RemoveResourceSecurityInformation(operation.Arguments as string);
                        }
                        break;
                    case OperationType.SecurityInformationModified:
                        if (operation.Arguments != null)
                        {
                            _store.InsertOrUpdateResourceSecurityInformation(operation.Arguments as IResourceItem);
                        }
                        break;
                    case OperationType.LoginDeleted:
                        if (operation.Arguments != null)
                        {
                            _store.RemoveUserInformation(operation.Arguments as string);
                        }
                        break;
                    case OperationType.LoginModified:
                        if (operation.Arguments != null)
                        {
                            _store.InsertOrUpdateUserInformation(operation.Arguments as IUser);
                        }
                        break;
                    case OperationType.BucketStatsModified:
                        if (operation.Arguments != null)
                        {
                            var updatedBucketStatisticses =
                                operation.Arguments as IDictionary<HashMapBucket, BucketStatistics>;
                            if (updatedBucketStatisticses != null)
                                _store.InsertOrUpdateBucketStats(operation.Cluster, operation.Database,
                                    operation.Collection, updatedBucketStatisticses);
                        }
                        break;
                    //case OperationType.BucketStatusModified:
                    //    if (operation.Arguments != null)
                    //    {
                    //        object[] args = operation.Arguments as object[];
                    //        ArrayList bucketList = args[0] as ArrayList;
                    //        byte status = (byte)args[1];
                    //        string shard = args[2] as string;

                    //        _store.UpdateBucketStatus(operation.Cluster, operation.Database, operation.Collection, bucketList, status, shard);
                    //    }
                    //    break;
                }
            }

            public void Rollback()
            {
                lock (this)
                {
                    _rollbacked = true;
                    Reset();
                }

            }

            private void Reset()
            {
                if (_cluster == null)
                    LoggerManager.Instance.CONDBLogger.Info("Transaction.Reset", "cluster is NULL 2");


                if (_operations == null)
                    LoggerManager.Instance.CONDBLogger.Info("Transaction.Reset", "operation is NULL 2");

                lock (this)
                {
                    _cluster.Reset();
                    _operations.Clear();
                    _started = false;
                }
            }
            public override bool Equals(object obj)
            {
                if (obj != null && obj is Transaction)
                    return string.Compare(_id, ((Transaction)obj)._id, true) == 0;

                return false;
            }

            public override int GetHashCode()
            {
                return _id.GetHashCode();
            }
            public enum OperationType
            {
                ClusterConfigurationModified,
                ClusterConfigurationDeleted,
                ClusterInfoModified,
                ClusterInfoDeleted,
                ClusterMembershipModified,
                ClusterMembershipDeleted,
                ClusterDistributionStrategyModified,
                ClusterDistributionStrategyDeleted,
                SecurityInformationModified,
                SecurityInformationDeleted,
                LoginModified,
                LoginDeleted,
                BucketStatsModified,
                BucketStatusModified
            }

            public class Operation :ICompactSerializable
            {
                public OperationType OperationType { get; set; }
                public string Database { get; set; }
                public string Collection { get; set; }
                public Object Arguments { get; set; }
                public string Shard { get; set; }
                public string Cluster { get; set; }


                public void Deserialize(Common.Serialization.IO.CompactReader reader)
                {
                    OperationType = (OperationType)reader.ReadInt32();
                    Database = reader.ReadObject() as string;
                    Collection = reader.ReadObject() as string;
                    Shard = reader.ReadObject() as string;
                    Cluster = reader.ReadObject() as string;
                    Arguments = reader.ReadObject();
                }

                public void Serialize(Common.Serialization.IO.CompactWriter writer)
                {
                    writer.Write((int)OperationType);
                    writer.WriteObject(Database);
                    writer.WriteObject(Collection);
                    writer.WriteObject(Shard);
                    writer.WriteObject(Cluster);
                    writer.WriteObject(Arguments);
                }
            }


            internal void Begin()
            {
                lock (this)
                {
                    if (!Started)
                    {
                        Reset();
                        _committed = false;
                        _rollbacked = false;
                        _started = true;
                        _thread = Thread.CurrentThread;
                    }
                }
            }

            internal bool ContainsCluster(string cluster)
            {
                lock (this)
                {
                    if (_cluster.Configuration != null && string.Compare(cluster, _cluster.Configuration.Name, true) == 0)
                        return true;
                    else
                    {
                        return _store.ContainsCluster(cluster);
                    }

                }
            }


            #region recovery operations
            public void InsertOrUpdateRecoveryJobData(ClusterJobInfoObject job)
            {
                _store.InsertOrUpdateRecoveryJobData(job);
            }

            public void RemoveRecoveryjobData(string id)
            {
                _store.RemoveRecoveryjobData(id);
            }

            public ClusterJobInfoObject GetRecoveryJobData(string id)
            {
                return _store.GetRecoveryJobData(id);
            }
            public ClusterJobInfoObject[] GetAllRecoveryJobData()
            {
                return _store.GetAllRecoveryJobData();
            }
             public ClusterJobInfoObject[] GetRecoveryJobData(Query query)
            {
                return _store.GetRecoveryJobData(query);
            }
            #endregion


             public void Deserialize(Common.Serialization.IO.CompactReader reader)
             {
                 this._id = reader.ReadObject() as string;
                 this._clusterName = reader.ReadObject() as string;
                 this._committed = reader.ReadBoolean();
                 this._rollbacked = reader.ReadBoolean();
                 this._started = reader.ReadBoolean();
                 this._cluster = reader.ReadObject() as DatabaseCluster;
                 this._operations = Alachisoft.NosDB.Common.Util.SerializationUtility.DeserializeList<Operation>(reader);
   
             }

             public void Serialize(Common.Serialization.IO.CompactWriter writer)
             {
                 writer.WriteObject(this._id);
                 writer.WriteObject(this._clusterName);
                 writer.Write(this._committed);
                 writer.Write(this._rollbacked);
                 writer.Write(this._started);
                 writer.WriteObject(this._cluster);
                 Alachisoft.NosDB.Common.Util.SerializationUtility.SerializeList<Operation>(this._operations,writer);   
             }

        }

        #endregion

        internal void EnlistAndCommitTrnsaction(Transaction transaction)
        {
            if(transaction != null)
            {
                transaction.SetStore(this);
                CommitTransaction(transaction);
            }
        }

        internal DatabaseCluster GetState()
        {
            if (_databaseClusters.ContainsKey(MiscUtil.CLUSTERED))
            {
                DatabaseCluster cluster = _databaseClusters[MiscUtil.CLUSTERED];
                return cluster;
            }
                        return null;
        }

       
        internal void ApplyState(DatabaseCluster databaseCluster)
        {
            if(databaseCluster !=null)
            {
                //if(_databaseClusters.ContainsKey(MiscUtil.CLUSTERED))
                //{
                //    _databaseClusters[MiscUtil.CLUSTERED] = databaseCluster;
                //}
                //else
                //{
                //    _databaseClusters.Add(MiscUtil.CLUSTERED, databaseCluster);
                //}

                if (databaseCluster.Configuration != null)
                    InsertOrUpdateClusterConfiguration(databaseCluster.Configuration);

                if (databaseCluster.ClusterInfo != null)
                    InsertOrUpdateClusterInfo(databaseCluster.ClusterInfo);

                
                foreach(Membership membership in databaseCluster.Memberships)
                {
                    if(membership != null)
                    {
                        InsertOrUpdateMembershipData(membership);
                    }
                }

                if (databaseCluster.ClusterInfo != null)
                {
                    if (databaseCluster.ClusterInfo != null && databaseCluster.ClusterInfo.Databases != null)
                    {
                        foreach (var database in databaseCluster.ClusterInfo.Databases.Values)
                        {
                            if (database != null && database.Collections != null)
                            {
                                foreach (CollectionInfo col in database.Collections.Values)
                                {
                                    InsertOrUpdateDistributionStrategy(databaseCluster.ClusterInfo, database.Name, col.Name);
                                }
                            }
                        }
                    }
                }

                if (databaseCluster.Logins != null)
                {
                    foreach (string key in databaseCluster.Logins.Keys)
                    {
                        InsertOrUpdateUserInformation(databaseCluster.Logins[key]);
                    }
                }

                if (databaseCluster.SecurityResources != null)
                {
                    foreach (string key in databaseCluster.SecurityResources.Keys)
                    {
                        InsertOrUpdateResourceSecurityInformation(databaseCluster.SecurityResources[key]);
                    }
                }


            }
        }
       
    }
}
