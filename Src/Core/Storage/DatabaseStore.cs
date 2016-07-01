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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Memory;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Caching.QueryCache;
using Alachisoft.NosDB.Common.Storage.Provider;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;

using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Storage.Providers;
using Alachisoft.NosDB.Core.Storage.Providers.LMDB;
using System;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Queries.Results;
using CollectionConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration;
using IndexConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration;
using Alachisoft.NosDB.Core.Storage.Caching;
using Alachisoft.NosDB.Core.Statistics;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Core.Storage
{
    //Refactoring needed.

    public class DatabaseStore : IDatabaseStore, IStateTxfrOperationListener
    {
        protected DatabaseContext _dbContext;

        protected Dictionary<string, ICollectionStore> _collections =
            new Dictionary<string, ICollectionStore>(StringComparer.OrdinalIgnoreCase);

        protected List<string> _droppedCollections = new List<string>();
        protected QueryCache<IDmObject> _reducedQueryCache = new QueryCache<IDmObject>();
        protected NodeContext _nodeContext;
        protected IDictionary<String, IDistribution> _colDistributionMap;
        protected QueryResultManager _queryResultManager = new QueryResultManager();

        protected IStatsCollector _statsCollector;
        protected Thread _persistenceThread;
        protected IDictionary<long, string> _unpersistedOperations;
        protected Queue<DdlOperation> _unpersistedDdlOperations;
        protected object _persistenceLock = new object();
        protected object _ddlOperationsLock = new object();
        protected UpdateBucketInfoTask _updateBucketInfoTask;

     

     

        public static string METADATA_DB_ID = "metadata";
        public static string METADATA_FILE_ID_COLLECTION = "FileMetadata";
        protected const int OPP_TRIGGER_THRESHOLD = 200;
        protected const int TIME_TRIGGER_THRESHOLD = 300; //mili-Seconds

        protected const int COMMIT_THRESHOLD = 5000;
            //To Be decided temporarily given. Required for esent to improve performance

        protected PersistenceManager _persistenceManager;
        private long _lastCommitId = 1; //this and above to be persisted
        private Latch _statusLatch;

        public IDictionary<String, IDistribution> CollectionDistributionMap
        {
            get { return _colDistributionMap; }
            set
            {
                _colDistributionMap = value;
                SetDistributionMaps();
            }
        }

        public DatabaseContext DatabaseContext { get { return _dbContext; } }

        public Dictionary<string, ICollectionStore> Collections { get { return _collections; } }

        public string Name { get { return _dbContext.DatabaseName; } }
        //: this property should be part of the interface, but it'll be exposed to router aswell, hence a hack for now.
        public long Size
        {
            get
            {
                long size = 0;
                foreach(ICollectionStore collection in _collections.Values)
                {
                    size += collection.Size;
                }
                return size;
            }
        }

        public virtual bool Initialize(DatabaseConfiguration configuration, NodeContext context, IDictionary<string, IDistribution> colDistributions)
        {
            _statusLatch = new Latch(DatabaseStatus.INITIALIZING);
            if (colDistributions != null)
				_colDistributionMap = colDistributions;
            else
            {
                _colDistributionMap = new Dictionary<string, IDistribution>();
                //Why  why ?? :P
                //if(colDistributions != null)
                //{
                //    if(colDistributions.Count>0)
                //    {
                //        _colDistributionMap = colDistributions;
                //    }
                //} 
            }
            _nodeContext = context;
            _dbContext = new DatabaseContext();
            _dbContext.LockManager = new LockManager<string, string, DocumentKey>(LockRecursionPolicy.SupportsRecursion);
            _dbContext.DatabaseConfigurations = configuration;
            _dbContext.DeploymentPath = context.DeploymentPath;
         
            _unpersistedOperations = new HashVector<long, string>();
            _dbContext.UnpersistedOperations = _unpersistedOperations;
            _persistenceManager = new PersistenceManager();
            _dbContext.PersistenceManager = _persistenceManager;
            _dbContext.DatabaseMode = configuration.Mode;
            _dbContext.IndexPersistanceManager = new BPlusPersistanceManager(_dbContext.DatabaseName);
            _unpersistedDdlOperations = new Queue<DdlOperation>();
          

            _dbContext.StatsIdentity = new StatsIdentity(context.LocalShardName, _dbContext.DatabaseName);
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = context.LocalShardName != null ? context.LocalShardName : "", DatabaseName =_dbContext.DatabaseName!=null?_dbContext.DatabaseName:"" });

            try
            {
                InitializePerfmonCounters();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.StorageLogger.Error(
                        _dbContext.DatabaseName + " Perfmon Counters Initialization", ex.Message);
                }
            }

            configuration.Storage.StorageProvider.DatabaseId = _dbContext.DatabaseName;
                configuration.Storage.StorageProvider.DatabasePath = _nodeContext.BasePath;

            switch (configuration.Storage.StorageProvider.StorageProviderType)
            {
                case ProviderType.LMDB:
                    if (configuration.Storage.StorageProvider.IsMultiFileStore)
                    {
                        _dbContext.StorageManager = new StorageManagerMultiFile();
                        if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        {
                            LoggerManager.Instance.StorageLogger.Info("DataBaseStore.Initialized()", "Multifile LMDB Storage initialized for database:" + _dbContext.DatabaseName);
                        }
                    }
                    else
                    {
                        _dbContext.StorageManager = new StorageManagerSingleFile<LMDBPersistenceProvider>();
                        if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        {
                            LoggerManager.Instance.StorageLogger.Info("DataBaseStore.Initialized()", "Singlefile LMDB Storage initialized for database:" + _dbContext.DatabaseName);
                        }
                    }
                    break;                
                default:
                    throw new ConfigurationException("Invalid Provider type specified.");
            }

            //_dbContext.MetadataIndex = new MetadataIndex(configuration.Storage);
            //_dbContext.MetadataIndex.Initialize("mycollection");

            _dbContext.StorageManager.MetadataPersister = GetMetadataPersister(configuration.Storage);
            _dbContext.StorageManager.Initialize(configuration.Storage, _dbContext.StatsIdentity);
            _persistenceManager.Initialize(_dbContext.StorageManager);

            if (configuration.Storage.CacheConfiguration != null)
            {
                long cacheSpace;
                ICacheSpacePolicy cacheSpacePolicy = null;
                if (configuration.Storage.CacheConfiguration.CachePolicy.ToLower() == "fcfs")
                    cacheSpacePolicy = new FirstComeFirstServe();
                cacheSpace = configuration.Storage.CacheConfiguration.CacheSpace;

                _dbContext.CacheSpace = new CacheSpace(cacheSpace, cacheSpacePolicy, _dbContext.StatsIdentity);
            }
            else
                _dbContext.CacheSpace = new CacheSpace(MiscUtil.DEFAULT_CACHE_SPACE, new FirstComeFirstServe(), _dbContext.StatsIdentity);

            RegisterTasks();

            if (configuration.Storage != null && configuration.Storage.Collections != null && configuration.Storage.Collections.Configuration != null)
            {
                foreach (CollectionConfiguration collectionConfig in configuration.Storage.Collections.Configuration.Values)
                {
                    CreateCollectionInternal(collectionConfig, _nodeContext, false);
                }
            }
            SetCollectionsRunning();

            _persistenceThread = new Thread(Persist);
            _persistenceThread.Name = "PersistenceThread." + Name;
            //this is not a background theread do not change it.
            _persistenceThread.IsBackground = false;
            _persistenceThread.Start();
           

            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.StorageLogger.Info("DataBaseStore.Initialized()", "Blob Manager successfully created at " + DateTime.Now);
            }
            SetDocumentCountStats();
            _statusLatch.SetStatusBit(DatabaseStatus.RUNNING, DatabaseStatus.INITIALIZING);
            return true;

        }

        protected IPersistenceProvider GetMetadataPersister(StorageConfiguration storageConf)
        {
            StorageConfiguration clone = (StorageConfiguration)storageConf.Clone();

            if (storageConf.StorageProvider.StorageProviderType == ProviderType.LMDB)
            {
                clone.StorageProvider.LMDBProvider.MaxCollections =
                    (storageConf.StorageProvider.LMDBProvider.MaxCollections * 2) + 1;
            }

            clone.StorageProvider.DatabaseId = METADATA_DB_ID;
            clone.StorageProvider.DatabasePath = clone.StorageProvider.DatabasePath + storageConf.StorageProvider.DatabaseId;
            clone.StorageProvider.DatabasePath += "\\";
            //assumption by usama: metadata file needs to be infinite size theroatically
            clone.StorageProvider.MaxFileSize = 21474836480;//MiscUtil.MAX_FILE_SIZE;
            clone.StorageProvider.IsMultiFileStore = false;

            IPersistenceProvider metadataPersister = ProviderFactory.CreateProvider(storageConf.StorageProvider.StorageProviderType);
            metadataPersister.Initialize(clone);

            //Shared Data among collections. used by StorageManagerBase
            metadataPersister.CreateCollection(METADATA_FILE_ID_COLLECTION, typeof(string), typeof(byte[]));
            return metadataPersister;
        }

        protected virtual void RegisterTasks()
        {
            //if(_nodeContext != null && _nodeContext.ElectionResult != null && _nodeContext.ElectionResult.ElectedPrimary != null && _nodeContext.ElectionResult.ElectedPrimary.Name.Equals(_nodeContext.LocalAddress.IpAddress.ToString()))
            //{
                //Need to be uncommented once used on configuration server
                //_updateBucketInfoTask = new UpdateBucketInfoTask(this, _nodeContext, _dbContext.DatabaseName);
                //TimeScheduler.Global.AddTask(_updateBucketInfoTask);
            //}
			
			//TimeScheduler.Task esentDefragmentationTask = new EsentDefragmentationTask(_dbContext.StorageManager);
            //_dbTimeScheduler.AddTask(esentDefragmentationTask);
        }


       

        private void SetDistributionMaps()
        {
            foreach (KeyValuePair<string, ICollectionStore> collection in _collections)
            {
                if(_colDistributionMap.ContainsKey(collection.Key))
                    collection.Value.Distribution = _colDistributionMap[collection.Key];
                else
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Warn("DatabaseStore.SetDistribution", "Distribution not found for collection "+ collection.Key);
                    }
                }
            }
        }

        private void ExecuteDdlOperation()
        {
            while (_unpersistedDdlOperations.Count > 0)
            {
                DdlOperation ddlOperation;
                IDBResponse dbResponse = null;
                lock (_unpersistedDdlOperations)
                    ddlOperation = _unpersistedDdlOperations.Dequeue();
                try
                {
                    dbResponse = ddlOperation.CreateDbResponse();
                    switch (ddlOperation.DdlOperationType)
                    {
                        case DdlOperationType.CreateCollection:
                            var createCollectionOperation = (ICreateCollectionOperation) ddlOperation.DbOperation;
                            if (createCollectionOperation.Distribution != null)
                            {
                                _colDistributionMap[createCollectionOperation.Configuration.CollectionName]=createCollectionOperation.Distribution.Distribution;
                            }
                            dbResponse.IsSuccessfull = CreateCollectionInternal(createCollectionOperation.Configuration, _nodeContext,true);
                            if (dbResponse.IsSuccessfull)
                            {
                                _collections[createCollectionOperation.Configuration.CollectionName].PopulateData();
                            }
                            break;
                        case DdlOperationType.DropCollection:
                            dbResponse.IsSuccessfull = DropCollectionInternal(ddlOperation.DbOperation.Collection);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    if (dbResponse != null) dbResponse.IsSuccessfull = false; 
                    //todo: Add error code
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                        LoggerManager.Instance.StorageLogger.Error("DatabaseStore.ExecuteDDL()", exception.Message);
                }
                finally
                {
                    lock (_ddlOperationsLock)
                    {
                        Monitor.PulseAll(_ddlOperationsLock);
                    }
                    SetDocumentCountStats();
                }
            }
        }

        private void Persist()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _nodeContext.LocalShardName != null ? _nodeContext.LocalShardName : "", DatabaseName = _dbContext.DatabaseName != null ? _dbContext.DatabaseName : "" });
            if (LoggerManager.Instance.StorageLogger != null)
            {
                LoggerManager.Instance.StorageLogger.Debug("PersistanceThread:"+Name, "Persistance started at "+ DateTime.Now.ToString());
            }
            _dbContext.IndexPersistanceManager.Start();
            while (!_statusLatch.IsAnyBitsSet(DatabaseStatus.DISPOSING | DatabaseStatus.DROPPING))            
            {
                PersistInternal();
            }
            _dbContext.IndexPersistanceManager.RequestStop();
            if (LoggerManager.Instance.StorageLogger != null)
            {
                LoggerManager.Instance.StorageLogger.Debug("PersistanceThread:" + Name, "Persistance stopped at " + DateTime.Now.ToString());
            }
            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                LoggerManager.Instance.StorageLogger.Info("Persistence thread is stopping.", "Persistence stopped on database = " + Name);
        }

        private void PersistInternal()
        {
                int persistedOppCount = 0;
                int count = 0;
                ITransaction transaction = null;
                var persistedBitSets = new Dictionary<string, IList<long>>();
                var deletedRows = new Dictionary<string, IList<long>>();
                try
                {
                    if (_unpersistedDdlOperations.Count > 0)
                        ExecuteDdlOperation();

                    if (_unpersistedOperations.Count != 0)
                    {
                        long operationId = _lastCommitId;
                        IDictionary<long,string> persistedOpps = new HashVector<long, string>();
                        
                        if (_statusLatch.IsAnyBitsSet(DatabaseStatus.DISPOSING | DatabaseStatus.DROPPING))
                            return;

                        transaction = _persistenceManager.BeginTransaction(null, false);

                        for (; operationId <= _dbContext.OppIdToCommit; operationId++)
                        {
                            if (persistedOppCount > COMMIT_THRESHOLD)
                            {

                                Commit(ref transaction, ref persistedOppCount, ref operationId, ref persistedBitSets, ref persistedOpps, ref deletedRows);

                                if (_unpersistedDdlOperations.Count > 0)
                                    ExecuteDdlOperation();

                                if(_statusLatch.IsAnyBitsSet(DatabaseStatus.DISPOSING | DatabaseStatus.DROPPING))
                                    return;

                                transaction = _persistenceManager.BeginTransaction(null, false);
                            }

                            if (!_unpersistedOperations.ContainsKey(operationId))
                            {
                                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                                    LoggerManager.Instance.StorageLogger.Warn("Critical Exception while persisting data : Operation Not found in _persistenceOppLog. ", "DatabaseId = " + Name + " operation id = " + operationId + ". Waiting for next interval.");

                                break;
                            }
                            if (!_collections.ContainsKey(_unpersistedOperations[operationId]))
                            {
                                if (!_droppedCollections.Contains(_unpersistedOperations[operationId]))
                                {
                                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                                        LoggerManager.Instance.StorageLogger.Error("Critical Exception while persisting data : Operation's Collection not found. ", "DatabaseId = " + Name + " operation id = " + operationId + " collection = " + _unpersistedOperations[operationId]);
                                }
                                lock (_unpersistedOperations)
                                {
                                    _unpersistedOperations.Remove(operationId);
                                }
                                continue;
                            }

                            DocumentCache cache = _collections[_unpersistedOperations[operationId]].DocumentStore as DocumentCache;
                            
                            if (!cache.DirtyDocuments.ContainsKey(operationId))
                            {
                                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                                    LoggerManager.Instance.StorageLogger.Error("Critical Exception while persisting data : Operation not found in DirtyDocuments. ", "DatabaseId = " + Name + " operation id = " + operationId + " collection = " + _unpersistedOperations[operationId]);

                                continue;
                            }

                            StoreResult result = cache.DirtyDocuments[operationId].Execute(transaction,
                                _persistenceManager);
                            _dbContext.IndexPersistanceManager.SignalPersist(operationId);

                            switch (result)
                            {
                                case StoreResult.FailureExpansionRequired:
                                    Commit(ref transaction, ref persistedOppCount, ref operationId, ref persistedBitSets, ref persistedOpps, ref deletedRows);
                                    if (_unpersistedDdlOperations.Count > 0)
                                        ExecuteDdlOperation();
                                    //Must as this operation is not performed so decrement to retry
                                    operationId--;

                                    if (_statusLatch.IsAnyBitsSet(DatabaseStatus.DISPOSING | DatabaseStatus.DROPPING))
                                        return;

                                    transaction = _persistenceManager.BeginTransaction(null, false);
                                    break;

                                case StoreResult.FailureReOpenTransaction:
                                    throw new Exception("MDB_BAD_TXN: Transaction was to be rollbacked. Trying rollback. OppId = " + operationId + " Collection = " + cache.Name + " OppType = " + cache.DirtyDocuments[operationId].GetType());

                                case StoreResult.FailureDatabaseFull:
                                    throw new Exception("Failure Database Full. OppId = " + operationId + " Collection = " + cache.Name + " OppType = " + cache.DirtyDocuments[operationId].GetType());


                                case StoreResult.Failure:
                                    throw new Exception("General Database Failure. OppId = " + operationId + " Collection = " + cache.Name + " OppType = " + cache.DirtyDocuments[operationId].GetType());

                                case StoreResult.SuccessDelete:
                                    if (cache.DirtyDocuments[operationId].RowId != -1)
                                    {
                                        if (!deletedRows.ContainsKey(cache.Name))
                                            deletedRows.Add(cache.Name, new ClusteredList<long>());

                                        deletedRows[cache.Name].Add(cache.DirtyDocuments[operationId].RowId);
                                    }
                                    persistedOppCount++;
                                    persistedOpps.Add(operationId, _unpersistedOperations[operationId]);

                                    if (_statsCollector != null)
                                    {
                                        _statsCollector.IncrementStatsValue(StatisticsType.DocumentsPersistedPerSec);
                                    }

                                    break;

                                default:
                                    if (!persistedBitSets.ContainsKey(cache.Name))
                                        persistedBitSets.Add(cache.Name, new ClusteredList<long>());

                                    persistedBitSets[cache.Name].Add(cache.DirtyDocuments[operationId].RowId);
                                    
                                    persistedOppCount++;
                                    persistedOpps.Add(operationId, _unpersistedOperations[operationId]);

                                    if (_statsCollector != null)
                                    {
                                        _statsCollector.IncrementStatsValue(StatisticsType.DocumentsPersistedPerSec);
                                    }
                                    break;
                            }
                        }
                        Commit(ref transaction, ref persistedOppCount, ref operationId, ref persistedBitSets, ref persistedOpps, ref deletedRows);
                        
                        if (_statusLatch.IsAnyBitsSet(DatabaseStatus.DISPOSING | DatabaseStatus.DROPPING))
                            return;

                        if (_unpersistedDdlOperations.Count > 0)
                            ExecuteDdlOperation();
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.StorageLogger.Error("Critical Exception while persisting data : ", "Database = " + Name + " " + e);
                        }

                        if (transaction != null)
                            _persistenceManager.Rollback(transaction);

                        _dbContext.IndexPersistanceManager.SignalRollback();
                    }
                    catch (Exception e2)
                    {
                        if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                            LoggerManager.Instance.StorageLogger.Error("Critical Exception while Rollback in persisting data : ", "Database = " + Name + " " + e2);
                    }
                }
                lock (_persistenceLock)
                {
                Monitor.Wait(_persistenceLock, new TimeSpan(0, 0, 0, 0, TIME_TRIGGER_THRESHOLD));
            }
        }

        /// <summary>
        /// This Method MUST contain everything as a ref 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="persistedOppCount"></param>
        /// <param name="operationId"></param>
        /// <param name="persistedBitSets"></param>
        /// <param name="persistedOpps"></param>
        private void Commit(ref ITransaction transaction, ref int persistedOppCount, ref long operationId, ref Dictionary<string, IList<long>> persistedBitSets, ref IDictionary<long, string> persistedOpps, ref Dictionary<string, IList<long>> deletedRows)
        {
            _dbContext.IndexPersistanceManager.ResetOutSync();
            Stopwatch debugWatch = new Stopwatch();
            debugWatch.Start();

            _persistenceManager.Commit(transaction);

            debugWatch.Stop();
            _dbContext.IndexPersistanceManager.SignalCommit();

            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsDebugEnabled)
                LoggerManager.Instance.StorageLogger.Debug("Commit " + Name, " persistanceManagerCommit: " + debugWatch.ElapsedMilliseconds + " (ms) " + ", OpCount: " + persistedOppCount);

            transaction = null;

            debugWatch.Reset();
            SetDocumentCountStats();
            persistedOppCount = 0;
            //Remove deleted Items from cache
            foreach (KeyValuePair<string, IList<long>> pair in deletedRows)
            {
                if (_collections.ContainsKey(pair.Key) && _collections[pair.Key].Status.AreAllBitsSet(CollectionStatus.RUNNING))
                {
                    DocumentCache cacheToBeCleared = _collections[pair.Key].DocumentStore as DocumentCache;
                    foreach (long rowId in pair.Value)
                    {
                        cacheToBeCleared.CacheRemove(rowId);
                    }
                }
            }
            deletedRows.Clear();


            //UpdateBitSets of persisted documents
            foreach (KeyValuePair<string, IList<long>> pair in persistedBitSets)
            {
                if (_collections.ContainsKey(pair.Key) && _collections[pair.Key].Status.AreAllBitsSet(CollectionStatus.RUNNING))
                {
                    DocumentCache cacheToBeCleared = _collections[pair.Key].DocumentStore as DocumentCache;
                    cacheToBeCleared.UpdateDirtyFlags(pair.Value);
                }
            }

            persistedBitSets.Clear();
           

            foreach (KeyValuePair<long, string> kvp in persistedOpps)
            {
                string collection = kvp.Value;
                if (_collections.ContainsKey(collection) && _collections[collection].Status.AreAllBitsSet(CollectionStatus.RUNNING))
                {
                    IDocumentStore docStore1 = _collections[collection].DocumentStore;
                    DocumentCache cache1 = docStore1 as DocumentCache;
                    cache1.RemoveFromPersistenceDictionary(kvp.Key);
                }
            }

            persistedOpps.Clear();
            _lastCommitId = operationId;
            _dbContext.IndexPersistanceManager.WaitForOutSync();
        }

        private void InitializePerfmonCounters()
        {
            string instanceName = _dbContext.StatsIdentity.ShardName + ":" + _dbContext.StatsIdentity.DatabaseName;
            IStatsCollector statsCollector = new DatabaseStatsCollector();
            statsCollector.Initialize(instanceName);
            statsCollector.SetStatsValue(StatisticsType.CacheCount, 0);
            statsCollector.SetStatsValue(StatisticsType.DocumentCount, 0);
            statsCollector.SetStatsValue(StatisticsType.PendingPersistentDocuments, 0);
            StatsManager.Instance.AddStatsCollector(_dbContext.StatsIdentity, statsCollector);
            _statsCollector = statsCollector;
        }

        private void SetDocumentCountStats()
        {
            try
            {
                long count = 0;
                lock (_collections)
                {
                    foreach (KeyValuePair<string, ICollectionStore> kvp in _collections)
                    {
                        count += kvp.Value.RowToFileIndexCount;
                    }
                }

                if (_statsCollector != null)
                    _statsCollector.SetStatsValue(StatisticsType.DocumentCount, count);

                SetAverageDocumentSize();
            }
            catch (Exception) { /*Just to avoid exception if occur while publishing counter*/ }
        }

        private void SetAverageDocumentSize()
        {
            if (_statsCollector != null)
            {
                long count = _statsCollector.GetStatsValue(StatisticsType.DocumentCount);
                long dbSize = _statsCollector.GetStatsValue(StatisticsType.DatabaseSize);
                long avgSize = (count > 0) ? dbSize / count : 0;
                _statsCollector.SetStatsValue(StatisticsType.AvgDocumentSize, avgSize);
            }
        }

        private void DisposePerfmonCounters()
        {
            IStatsCollector statsCollector = StatsManager.Instance.GetStatsCollector(_dbContext.StatsIdentity);
            if (statsCollector != null)
                statsCollector.Dispose();

            StatsManager.Instance.RemoveStatsCollector(_dbContext.StatsIdentity);
        }

        public Common.Server.Engine.IDBResponse CreateCollection(Common.Server.Engine.ICreateCollectionOperation operation)
        {
            //IDBResponse response = operation.CreateResponse();
            DdlOperation createCollectionOperation = new DdlOperation(DdlOperationType.CreateCollection, operation);
            lock (_unpersistedDdlOperations)
                _unpersistedDdlOperations.Enqueue(createCollectionOperation);

            lock (_persistenceLock)
                Monitor.Pulse(_persistenceLock);
            
            lock (_ddlOperationsLock)
            {
                while (createCollectionOperation.DbResponse == null)
                {
                    Monitor.Wait(_ddlOperationsLock);
                }
            }

            //response.IsSuccessfull = CreateCollectionInternal(operation.Configuration as CollectionConfiguration, _nodeContext);
            return createCollectionOperation.DbResponse;
        }

        public Common.Server.Engine.IDBResponse CreateIndex(Common.Server.Engine.ICreateIndexOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            IDBResponse response = operation.CreateResponse();
            //response.IsSuccessfull = this.CreateIndexInternal(operation.Collection, operation.Configuration as IndexConfiguration);
            response = _collections[operation.Collection].CreateIndex(operation);
            return response;
        }

        public IDBResponse RenameIndex(IRenameIndexOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            return _collections[operation.Collection].RenameIndex(operation);
        }

        public IDBResponse RecreateIndex(IRecreateIndexOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            return _collections[operation.Collection].RecreateIndex(operation);
        }

        public Common.Server.Engine.IDocumentsWriteResponse DeleteDocuments(Common.Server.Engine.IDocumentsWriteOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);

            return _collections[operation.Collection].DeleteDocuments(operation);
        }

        public Common.Server.Engine.IDBResponse DropCollection(Common.Server.Engine.IDropCollectionOperation operation)
        {
           // IDBResponse response = operation.CreateResponse();
            
            DdlOperation dropCollectionOperation = new DdlOperation(DdlOperationType.DropCollection,operation);
            
            lock (_unpersistedDdlOperations)
                _unpersistedDdlOperations.Enqueue(dropCollectionOperation);

            lock (_persistenceLock)
                Monitor.Pulse(_persistenceLock);

            lock (_ddlOperationsLock)
            {
                while (dropCollectionOperation.DbResponse == null)
                {
                    Monitor.Wait(_ddlOperationsLock);
                }
            }
            //response.IsSuccessfull = CreateCollectionInternal(operation.Configuration as CollectionConfiguration, _nodeContext);
            return dropCollectionOperation.DbResponse;

            //response.IsSuccessfull = this.DropCollectionInternal(operation.Collection);
            //return response;
        }

        public Common.Server.Engine.IDBResponse DropIndex(Common.Server.Engine.IDropIndexOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            IDBResponse response = operation.CreateResponse();
            response = _collections[operation.Collection].DropIndex(operation);
            return response;
        }

        public Common.Server.Engine.IUpdateResponse ExecuteNonQuery(Common.Server.Engine.INonQueryOperation operation)
        {
            IUpdateResponse response = null;
            try
            {
                IDmObject parsedObject = _reducedQueryCache.GetParsedQuery(operation.Query.QueryText);
                if (parsedObject != null)
                {
                    ValidateCollection(parsedObject.Collection.ToLower(), operation.Database);
                    if (operation.Context == null)
                    {
                        operation.Context = new OperationContext();
                    }
                    operation.Context[ContextItem.ParsedQuery] = parsedObject;
                    return _collections[parsedObject.Collection.ToLower()].ExecuteNonQuery(operation);
                }
            }
            catch (DatabaseException ex)
            {
                response = operation.CreateResponse() as IUpdateResponse;
                response.IsSuccessfull = false;
                response.ErrorCode = ex.ErrorCode;
                response.ErrorParams = ex.Parameters;
                if (LoggerManager.Instance.QueryLogger != null)
                    LoggerManager.Instance.QueryLogger.Error("ExecuteNonQuery", "Query Execution Failure, " + ex);
            }
            catch (Exception ex)
            {
                response = operation.CreateResponse() as IUpdateResponse;
                response.IsSuccessfull = false;
                response.ErrorCode = ErrorCodes.Query.UNKNOWN_ISSUE;
                response.ErrorParams = new[] { ex.Message };
                if (LoggerManager.Instance.QueryLogger != null)
                    LoggerManager.Instance.QueryLogger.Error("ExecuteNonQuery", "Query Execution Failure, " + ex);
            }
            return response;
        }

        public Common.Server.Engine.IQueryResponse ExecuteReader(Common.Server.Engine.IQueryOperation operation)
        {
            IQueryResponse response = null;
            try
            {
                IDmObject parsedQuery = _reducedQueryCache.GetParsedQuery(operation.Query.QueryText);
                if (parsedQuery != null)
                {
                    if(!operation.Context.ContainsKey(ContextItem.InternalOperation)) ValidateCollection(parsedQuery.Collection.ToLower(), operation.Database);
                    if (operation.Context == null)
                    {
                        operation.Context = new OperationContext();
                    }
                    operation.Context[ContextItem.ParsedQuery] = parsedQuery;
                    IQueryResponse queryResponse= _collections[parsedQuery.Collection.ToLower()].ExecuteQuery(operation);
                  
                    return queryResponse;
                }
            }
            catch (DatabaseException ex)
            {
                response = operation.CreateResponse() as IQueryResponse;
                response.IsSuccessfull = false;
                response.ErrorCode = ex.ErrorCode;
                response.ErrorParams = ex.Parameters;
                if (LoggerManager.Instance.QueryLogger != null)
                    LoggerManager.Instance.QueryLogger.Error("ExecuteQuery", "Query Execution Failure, " + ex);
            }
            catch (Exception ex)
            {
                response = operation.CreateResponse() as IQueryResponse;
                response.IsSuccessfull = false;
                response.ErrorCode = ErrorCodes.Query.UNKNOWN_ISSUE;
                response.ErrorParams = new[] { ex.Message };
                if (LoggerManager.Instance.QueryLogger != null)
                    LoggerManager.Instance.QueryLogger.Error("ExecuteQuery", "Query Execution Failure, " + ex);
            }
            return response;
        }

        public Common.Server.Engine.IGetResponse GetDocuments(Common.Server.Engine.IGetOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            return _collections[operation.Collection].GetDocuments(operation);
        }

        public Common.Server.Engine.IDocumentsWriteResponse InsertDocuments(Common.Server.Engine.IDocumentsWriteOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            return _collections[operation.Collection].InsertDocuments(operation);
        }

        public Common.Server.Engine.IUpdateResponse UpdateDocuments(Common.Server.Engine.IUpdateOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            return _collections[operation.Collection].UpdateDocuments(operation);
        }

        public Common.Server.Engine.IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation)
        {
            this.ValidateCollection(operation.Collection, operation.Database);
            return _collections[operation.Collection].ReplaceDocuments(operation);
        }

        public IGetChunkResponse GetDataChunk(IGetChunkOperation operation)
        {
            IQueryResult queryResult = _queryResultManager.Get(operation.ReaderUID);
            IGetChunkResponse response = operation.CreateResponse() as IGetChunkResponse;
            if (response == null) return null;
            if (queryResult.Store.HasDisposed)
            {
                response.IsSuccessfull = false;
                response.ErrorCode = ErrorCodes.Collection.COLLECTION_DISPODED;
                response.ErrorParams = new string[] { operation.Collection };
            }
            else
            {
                IDataChunk dataChunk = response.DataChunk;
                response.IsSuccessfull = queryResult.FillDataChunk(operation.LastChunkId, ref dataChunk);
            }
            return response;
        }

        public IDBResponse DiposeReader(IDiposeReaderOperation operation)
        {
            IDBResponse response = operation.CreateResponse();

          

         

            var result = _queryResultManager.Remove(operation.ReaderUID);
            if (result != null)
            {
                result.Dispose();
                response.IsSuccessfull = true;
            }
            else response.IsSuccessfull = false;
            return response;
        }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool destory)
        {
            if (destory)
            {
                Destroy();
            }
            else
            {
                _statusLatch.SetStatusBit(DatabaseStatus.DISPOSING, DatabaseStatus.RUNNING);
                _persistenceThread.Join();

                if (_updateBucketInfoTask != null) _updateBucketInfoTask.Dispose();
                foreach (KeyValuePair<string, ICollectionStore> collection in _collections)
                {
                    collection.Value.Dispose();
                }
                _collections.Clear();

                DisposePerfmonCounters();
                if (_queryResultManager != null) _queryResultManager.Dispose();
                if (_dbContext.StorageManager != null) _dbContext.StorageManager.Dispose();
                if (_updateBucketInfoTask != null) _updateBucketInfoTask.Dispose();
            }
        }

        public void Destroy()
        {
            _statusLatch.SetStatusBit(DatabaseStatus.DROPPING, DatabaseStatus.RUNNING);
            _persistenceThread.Join();
            if (_updateBucketInfoTask != null) _updateBucketInfoTask.Dispose();
            foreach (KeyValuePair<string, ICollectionStore> collection in _collections)
            {
                collection.Value.Destroy(true);
            }
            _collections.Clear();
            string indexesFolder = _dbContext.DatabaseConfigurations.Storage.StorageProvider.DatabasePath + _dbContext.DatabaseConfigurations.Storage.StorageProvider.DatabaseId + "\\Indexes";
            if (Directory.Exists(indexesFolder))
                Directory.Delete(indexesFolder,true);
            
            DisposePerfmonCounters();
            if (_queryResultManager != null) _queryResultManager.Dispose();
            if (_dbContext.StorageManager != null) _dbContext.StorageManager.Destroy();
            

            Directory.Delete(_dbContext.DatabaseConfigurations.Storage.StorageProvider.DatabasePath + _dbContext.DatabaseConfigurations.Storage.StorageProvider.DatabaseId, true);
        }

        protected virtual bool CreateCollectionInternal(CollectionConfiguration configuration, NodeContext nodeContext,bool isNew)
        {
            if (!configuration.Shard.Equals(_nodeContext.LocalShardName, StringComparison.OrdinalIgnoreCase))
                return false;

            if (_collections.ContainsKey(configuration.CollectionName))
                return false;

            ICollectionStore collection = new UserCollection(_dbContext, nodeContext); //new BaseCollection(_dbContext, nodeContext);

            IDistribution dirstribution;
            _colDistributionMap.TryGetValue(configuration.CollectionName , out dirstribution);
            collection.ShardName = _nodeContext.LocalShardName;
            bool success = collection.Initialize(configuration, _queryResultManager, this, dirstribution);
            if (success)
            {
                // collection.RegisterRecoveryCommunicationHandler(this);
                lock (Collections)
                {
                    if (_droppedCollections.Contains(collection.Name))
                        _droppedCollections.Remove(collection.Name);
                    _collections[collection.Name] = collection;
                }
                //if (configuration.CollectionType.Equals(CollectionType.CappedCollection.ToString()) && !isNew)
                //    ((CappedCollection)collection).PopulateData();

               // collection.Status.SetStatusBit(CollectionStatus.RUNNING, CollectionStatus.INITIALIZING);
            }
            return success;
        }

        protected void SetCollectionsRunning()
        {
            lock (Collections)
            {
                foreach (var collection in _collections.Values)
                {
                    collection.PopulateData();
                }
            }
        }

        private bool DropCollectionInternal(string name)
        {
            if (!_collections.ContainsKey(name))
                return false;
            lock (Collections)
            {
                _droppedCollections.Add(name);
                _collections[name].Destroy(false);
                if (_dbContext.StorageManager.DropCollection(name))
                {
                    return _collections.Remove(name) && _colDistributionMap.Remove(name);
                }
                return false;
            }
        }

        public ICollectionStore GetCollection(string name)
        {
            ICollectionStore store;
            _collections.TryGetValue(name, out store);
            return store;
        }

        private bool CreateIndexInternal(string collectionName, IndexConfiguration indexConfiguration)
        {
            this.ValidateCollection(collectionName);
            //bool success=_collections[collectionName](indexConfiguration).IsSuccessfull;
            return false;// _dbContext.Collections[collectionName].CreateIndex(indexConfiguration);
        }

        private bool DropIndexInternal(string collectionName, string indexName)
        {
            this.ValidateCollection(collectionName);
            return false;//_dbContext.Collections[collectionName].DropIndex(indexName);
        }

        private void ValidateCollection(string collectionName, string database = "")
        {
            if (!_collections.ContainsKey(collectionName))
            {
                    throw new DatabaseException(ErrorCodes.Collection.COLLECTION_DOESNOT_EXIST , new[] {collectionName, database});
            }
            if (!_collections[collectionName].Status.AreAllBitsSet(CollectionStatus.RUNNING))
            {
                if (_collections[collectionName].Status.AreAllBitsSet(CollectionStatus.INITIALIZING))
                {
                    // original
                    //throw new DatabaseException(ErrorCodes.Collection.COLLECTION_OPERATION_NOTALLOWED, new[] { collectionName, "INITIALIZING" });
                   
                    _collections[collectionName].Status.WaitForAny(CollectionStatus.RUNNING, 60000);
                    // check status afterwards aswell
                    if (_collections[collectionName].Status.AreAllBitsSet(CollectionStatus.INITIALIZING))
                    {
                        throw new DatabaseException(ErrorCodes.Collection.COLLECTION_OPERATION_NOTALLOWED, new[] { collectionName, "INITIALIZING" });
                    }
                }
                    
                if (_collections[collectionName].Status.AreAllBitsSet(CollectionStatus.DROPPING))
                    throw new DatabaseException(ErrorCodes.Collection.COLLECTION_OPERATION_NOTALLOWED, new[] { collectionName, "DROPPING" });
            }

        }

        /*  #region Query parsing region

          private Reduction GetPreparedReduction(string query)
          {
              Reduction reduction = null;
              lock (_queryReductions.SyncRoot)
              {
                  if (!_queryReductions.ContainsKey(query))
                  {
                      var parser = new QueryParsingHelper();
                      if (parser.Parse(query) == ParseMessage.Accept)
                      {
                          reduction = parser.CurrentReduction;
                          AddPreparedReduction(query, reduction);
                      }
                      else
                      {
                          throw new QueryParsingException(QueryParseCode.INVALID_SYNTAX);
                      }
                  }
                  else
                  {
                      reduction = (Reduction) _queryReductions[query];
                  }
              }
              return reduction;
          }

          private void AddPreparedReduction(string query, Reduction currentReduction)
          {
              _queryReductions.Add(new QueryIdentifier(query), currentReduction);

              int _preparedQueryEvictionPercentage = 1000;  // could be considered from configuration.

              if (_queryReductions.Count > _preparedQueryEvictionPercentage)
              {
                  var list = new ArrayList(_queryReductions.Keys); list.Sort();
                  int evictCount = (_queryReductions.Count * _preparedQueryEvictionPercentage) / 100;
                  for (int i = 0; i < evictCount; i++)
                      _queryReductions.Remove(list[i]);
              }
          }
        
          private void RemoveReduction(string query)
          {
              lock (_queryReductions.SyncRoot)
              {
                  _queryReductions.Remove(query);
              }
          }
          #endregion*/

        public ElectionId ElectionResult
        {
            set { return; }
        }

        #region IStateTxfrOperationListener Implementation
        public object OnOperationRecieved(IStateTransferOperation operation)
        {
            //switch (operation.OpCode)
            //{
            //    case StateTransferOpCode.StartQueryLogging:
            //        OnStateTrxferStarted();
            //        break;
            //    case StateTransferOpCode.EndQueryLogging:
            //        _inStateTxfer = false;
            //        ClearTransferedKeys();
            //        break;
            //    default:
                    String colName = operation.TaskIdentity.ColName;
                    if (!String.IsNullOrEmpty(colName) && this._collections.ContainsKey(colName))
                    {
                        IStateTxfrOperationListener colStore = _collections[colName] as IStateTxfrOperationListener;
                        if (colStore != null)
                            return colStore.OnOperationRecieved(operation);
                    }
                    return null;
            //}
            //return null;
        }
       
        #endregion
      
        internal void StartBucketInfoTask()
        {
            //if (_nodeContext != null && _nodeContext.ElectionResult != null && _nodeContext.ElectionResult.ElectedPrimary != null && _nodeContext.ElectionResult.ElectedPrimary.Name.Equals(_nodeContext.LocalAddress.IpAddress.ToString()))
            //{
                if (_updateBucketInfoTask == null)
                    _updateBucketInfoTask = new UpdateBucketInfoTask(this, _nodeContext, _dbContext.DatabaseName);
                _updateBucketInfoTask.Start(); 
            //}
        }

        internal void StopBucketInfoTask()
        {
            //if (_nodeContext != null && (_nodeContext.ElectionResult == null || _nodeContext.ElectionResult.ElectedPrimary == null || !_nodeContext.ElectionResult.ElectedPrimary.Name.Equals(_nodeContext.LocalAddress.IpAddress.ToString())))
            //{

                if (_updateBucketInfoTask != null)
                    _updateBucketInfoTask.Stop(); 
            //}
        }
    }
}
