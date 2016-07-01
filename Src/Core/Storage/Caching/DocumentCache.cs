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
using System.DirectoryServices.ActiveDirectory;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Memory;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Storage.Caching.Evictions;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Storage.Providers;
using System;
using System.Collections.Generic;
using Alachisoft.NosDB.Core.Toplogies;

namespace Alachisoft.NosDB.Core.Storage.Caching
{
    public class DocumentCache : IDocumentStore, ICacheSpaceConsumer
    {
        private Dictionary<long, CacheItem> _cache = new Dictionary<long, CacheItem>();
        private object _cacheSyncLock = new object();

        private IPersistenceManager _persistenceManager = null;
        private object _persistenceLock = new object();
        private StorageManagerBase _storageManager = null;
        private BaseCollection _parent = null;
        private object _persistLock = new object();
        private Dictionary<long, PersistenceOperation> _dirtyDocuments;
        private IDictionary<long, string> _unpersistedOperations;
        private IStatsCollector _statsCollector;
        private CollectionIndexManager _indexManager;

        private bool _isEvictionEnabled = true;
        private IEvictionPolicy _evictionPolicy;
        private long _cacheSize;
        private CacheSpace _cacheSpace;
        private NodeContext _nodeContext;
        private bool _isDisposed = false;

        public StorageManagerBase GetStorageManagerBase
        {
            get { return _storageManager; }
        }

        public IPersistenceManager persistenceManager { get { return _persistenceManager; } }

        public Dictionary<long, PersistenceOperation> DirtyDocuments { get { return _dirtyDocuments; } }

        public DocumentCache(BaseCollection parent, NodeContext nodeContext)
        {
            _parent = parent;
            _cacheSpace = parent.DbContext.CacheSpace;

            _cacheSpace.AddConsumer(this);
            _storageManager = parent.DbContext.StorageManager;
            _dirtyDocuments = new Dictionary<long, PersistenceOperation>();
            _unpersistedOperations = _parent.DbContext.UnpersistedOperations;
            _nodeContext = nodeContext;
            _statsCollector = StatsManager.Instance.GetStatsCollector(_parent.DbContext.StatsIdentity);
        }

        public bool Initialize(DatabaseContext _dbContext, CollectionConfiguration config)
        {
            //eviction is always enabled for now and can not be turned off.
            _isEvictionEnabled = true;
            if (_parent.CollectionConfiguration.EvictionConfiguration != null)  //TODO this check is added if eviction configuration is not available at collection level in db.config
            {
                //_isEvictionEnabled = _parent.CollectionConfiguration.EvictionConfiguration.EnabledEviction;
                if (_parent.CollectionConfiguration.EvictionConfiguration.Policy.ToLower() == "lru")
                    _evictionPolicy = new LRUEvictionPolicy(_parent.DbContext.StatsIdentity);
            }
            else
                _evictionPolicy = new LRUEvictionPolicy(_parent.DbContext.StatsIdentity);

            _persistenceManager = _parent.DbContext.PersistenceManager;
            _persistenceManager.AddMetadataIndex(_parent.Name, _parent.MetadataIndex);

            _indexManager = new CollectionIndexManager(_dbContext, _dbContext.IndexPersistanceManager, this, config.CollectionName);
            if (!_indexManager.Initialize(config.Indices))
                return false;
            //if (_statsCollector != null)
            //    _statsCollector.SetStatsValue(StatisticsType.PendingPersistentDocuments, _dirtyDocuments.Count);

            if (_persistenceManager == null)
                return false;
            return true;
        }

        public long GetRowId(string key)
        {
            if (_parent != null && _parent.MetadataIndex != null)
                return _parent.MetadataIndex.GetRowId(new DocumentKey(key));

            return long.MinValue;
        }

        InsertResult<JSONDocument> IDocumentStore.InsertDocument(InsertOperation operation)
        {
            UsageStats stats = new UsageStats();
            stats.BeginSample();

            InsertResult<JSONDocument> result = new InsertResult<JSONDocument>();
            CacheItem citem = new CacheItem();
            citem.Document = operation.Document;
            citem.Flag.SetBit(BitsetConstants.DocumentDirty);
            citem.Flag.SetBit(BitsetConstants.MetaDataDirty);
            citem.Flag.UnsetBit(BitsetConstants.MarkedForDeletion);

            CacheInsert(operation.RowId, citem,false);
           _indexManager.AddToIndex(operation.RowId, operation.Document, operation.OperationId);

            _parent.MetadataIndex.Add(new DocumentKey(operation.Document.Key), operation.RowId, operation.Document, operation.Context);

            PersistenceOperation persistenceOperation = new PersistenceInsertOperation(operation.OperationId, _parent.Name, operation.RowId, citem, operation.Context);
            AddToPersistenceDictionary(persistenceOperation);
            result.RowId = operation.RowId;

            if (_statsCollector != null)
            {
                stats.EndSample();
                _statsCollector.IncrementStatsValue(StatisticsType.AvgInsertTime, stats.Current);
                _statsCollector.IncrementStatsValue(StatisticsType.InsertsPerSec);
            }
            return result;
        }

        DeleteResult<JSONDocument> IDocumentStore.DeleteDocument(RemoveOperation operation)
        {
            UsageStats stats = new UsageStats();
            stats.BeginSample();

            DeleteResult<JSONDocument> result = new DeleteResult<JSONDocument>();
            CacheItem cItem;
            if (!_cache.ContainsKey(operation.RowId))
            {
                //TODO: Read-through
                if (!_persistenceManager.MetadataIndex(_parent.Name).ContainsRowId(operation.RowId))
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                        LoggerManager.Instance.StorageLogger.Warn("Delete : document not found.", "Document not found while deleting in metadataIndex. rowId = " + operation.RowId + " collection = " + Name);
                    AddFailedOperation(operation);
                    return result;
                }
                cItem = LoadDocument(operation.RowId, operation.Context);
            }
            else
                cItem = CacheGet(operation.RowId);
            
            //Remove an item from eviction index, so that it's not marked for eviction.
            if(cItem != null)
                _evictionPolicy.Remove(operation.RowId, cItem.EvictionHint);

            result.Document = cItem.Document;
            result.RowId = operation.RowId;
            //_cache.Remove(operation.RowId);
            //CacheRemove(operation.RowId);

           _indexManager.UpdateIndex(operation.RowId, result.Document, new JSONDocument(), operation.OperationId);

            cItem.Flag.SetBit(BitsetConstants.DocumentDirty);
            cItem.Flag.SetBit(BitsetConstants.MarkedForDeletion);

            PersistenceOperation persistenceOperation = new PersistenceDeleteOperation(operation.OperationId, _parent.Name, operation.RowId, cItem, operation.Context);
            AddToPersistenceDictionary(persistenceOperation);
            _persistenceManager.MetadataIndex(_parent.Name).Remove(new DocumentKey(cItem.Document.Key));

            if (_statsCollector != null)
            {
                stats.EndSample();
                _statsCollector.IncrementStatsValue(StatisticsType.AvgDeleteTime, stats.Current);
                _statsCollector.IncrementStatsValue(StatisticsType.DeletesPerSec);
            }

            return result;
        }

        GetResult<JSONDocument> IDocumentStore.GetDocument(GetOperation operation)
        {
            UsageStats stats = new UsageStats();
            stats.BeginSample();

            GetResult<JSONDocument> result = new GetResult<JSONDocument>();
            CacheItem cItem;
            if (!_cache.ContainsKey(operation.RowId))
            {
                //TODO: Read-through
                cItem = LoadDocument(operation.RowId, operation.Context);
                if (_statsCollector != null)
                    _statsCollector.IncrementStatsValue(StatisticsType.CacheMissesPerSec);
            }
            else
            {
                cItem = CacheGetWithoutNotify(operation.RowId);
                if (_statsCollector != null)
                    _statsCollector.IncrementStatsValue(StatisticsType.CacheHitsPerSec);
            }
            if (cItem == null)
                throw new Exception("Item not found in cache.");
            if (!cItem.Flag.IsBitSet(BitsetConstants.MarkedForDeletion))
            {
                result.Document = cItem.Document;
                result.RowId = operation.RowId;
            }

            if (_statsCollector != null)
            {
                stats.EndSample();
                _statsCollector.IncrementStatsValue(StatisticsType.AvgFetchTime, stats.Current);
                _statsCollector.IncrementStatsValue(StatisticsType.FetchesPerSec);
            }
            return result;
        }

        UpdateResult<JSONDocument> IDocumentStore.UpdateDocument(UpdateOperation operation)
        {
            UsageStats stats = new UsageStats();
            stats.BeginSample();

            UpdateResult<JSONDocument> result = new UpdateResult<JSONDocument>();
            CacheItem cItem;
            if (!_cache.ContainsKey(operation.RowId))
            {
                //TODO: Read-through
                if (!_persistenceManager.MetadataIndex(_parent.Name).ContainsRowId(operation.RowId))
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                        LoggerManager.Instance.StorageLogger.Warn("Update : document not found.", "Document not found while updating in metadataIndex. rowId = " + operation.RowId +" collection = " + Name);
                    AddFailedOperation(operation);
                    return result;
                }

                cItem = LoadDocument(operation.RowId, operation.Context);
            }
            else
                cItem = CacheGet(operation.RowId);//_cache[operation.RowId];
            
            if (cItem == null)
                throw new Exception("Item not found in cache.");
            //removing cache item from eviction policy so that it will not be evicted.
            _evictionPolicy.Remove(operation.RowId, cItem.EvictionHint);
            result.OldDocument = cItem.Document.Clone() as JSONDocument;
            cItem.Document = operation.Update;
            result.RowId = operation.RowId;
            result.NewDocument = cItem.Document.Clone() as JSONDocument;

            cItem.Flag.SetBit(BitsetConstants.DocumentDirty);


            _indexManager.UpdateIndex(operation.RowId, cItem.Document, operation.Update, operation.OperationId);

            PersistenceOperation persistenceOperation = new PersistenceUpdateOperation(operation.OperationId,
                _parent.Name, operation.RowId, cItem, operation.Context);
            AddToPersistenceDictionary(persistenceOperation);
            _parent.MetadataIndex.Update(new DocumentKey(operation.Update.Key), operation);
            if (_statsCollector != null)
            {
                stats.EndSample();
                _statsCollector.IncrementStatsValue(StatisticsType.AvgUpdateTime, stats.Current);
                _statsCollector.IncrementStatsValue(StatisticsType.UpdatesPerSec);
            }

            return result;
        }

        IEnumerator<KeyValuePair<long, JSONDocument>> IEnumerable<KeyValuePair<long, JSONDocument>>.GetEnumerator()
        {
            if (_parent != null && _parent.MetadataIndex != null)
            {
                foreach (var rowId in _parent.MetadataIndex)
                {
                    var result = ((IDocumentStore)this).GetDocument(new GetOperation() { RowId = rowId });
                    yield return new KeyValuePair<long, JSONDocument>(rowId, result.Document);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void AddFailedOperation(Operation operation)
        {
            try
            {
                PersistenceFailOperation failedOperation = new PersistenceFailOperation(operation.OperationId,
                    operation.Collection, operation.RowId, null, operation.Context);
                AddToPersistenceDictionary(failedOperation);
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Exception Adding failed operation: Collection = " + Name, " Exception is = " + e);
                throw;
            }
        }

        private void AddToPersistenceDictionary(PersistenceOperation operation)
        {
            lock (_dirtyDocuments)
                _dirtyDocuments.Add(operation.OperationID, operation);

            lock (_unpersistedOperations)
                _unpersistedOperations.Add(operation.OperationID, Name);

            _parent.DbContext.OppIdToCommit = operation.OperationID;

            if (_statsCollector != null)
                _statsCollector.IncrementStatsValue(StatisticsType.PendingPersistentDocuments);
        }

        public void RemoveFromPersistenceDictionary(long operationId)
        {
            lock (_dirtyDocuments)
                _dirtyDocuments.Remove(operationId);

            lock (_unpersistedOperations)
                _unpersistedOperations.Remove(operationId);

            if (_statsCollector != null)
                _statsCollector.DecrementStatsValue(StatisticsType.PendingPersistentDocuments);
        }

        private CacheItem LoadDocument(long rowId, IOperationContext operationContext)
        {
            ITransaction transaction = null;
            try
            {
                //transaction = _persistenceManager.BeginTransaction(null, true);
                var document = _persistenceManager.GetDocument(_parent.Name, rowId);
                CacheItem cItem = new CacheItem() {Document = document};
                //If query exection then cache insert is optional
                if(operationContext.ContainsKey(ContextItem.QueryExecution) && _cacheSpace.IsFull)
                    return cItem;
                CacheInsert(rowId, cItem);
                return cItem;
            }
            catch (Exception e)
            {
                Console.WriteLine("Read Through Exception " + e);
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Exception Reading Thru : Collection = " + Name, " Row ID: " + rowId + " and Exception is = " + e);
                
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    _persistenceManager.Commit(transaction);
                }
            }
        }

        public void UpdateDirtyFlags(IList<long> toUpdate)
        {
            foreach (long rowId in toUpdate)
            {
                CacheItem citem = null;
        
                if (_cache.TryGetValue(rowId, out citem))
                {
                    citem.Flag.UnsetBit(BitsetConstants.DocumentDirty);
                    citem.Flag.UnsetBit(BitsetConstants.MetaDataDirty);
                   
                    //Once item is persisted, it should be again added to the eviction policy
                    if (!citem.Flag.IsAnyBitSet(BitsetConstants.MarkedForDeletion))
                        _evictionPolicy.Notify(rowId, null, citem.EvictionHint);
                }
            }
        }

        public void CacheInsert(long rowId, CacheItem citem, bool notifyEvictionPolicy = true)
        {
            EvictionHint oldHint = null;
            if (_cache.ContainsKey(rowId))      //Incase of update
            {
                //get current citem calculate size and then subtact it 
                CacheItem tempItem = CacheGetWithoutNotify(rowId);
                _cacheSize -= tempItem.Size;
                oldHint = tempItem.EvictionHint;
                _cacheSpace.Release(this, tempItem.Size);
                if (_statsCollector != null)
                {
                    _statsCollector.DecrementStatsValue(StatisticsType.CacheCount);
                }
            }
            //if (!_cacheSpace.Policy.CanConsumeSpace(_cacheSpace, this, citem.Size))
            //{
            //    //TODO: if policy fails to evict cache what to do? (Need based Eviction)
            //}
            _cacheSpace.Consume(this, citem.Size);
            lock (_cacheSyncLock)
            {
                _cache[rowId] = citem;
            }
            if(notifyEvictionPolicy) _evictionPolicy.Notify(rowId, oldHint, citem.EvictionHint);
            _cacheSize += citem.Size;
            if (_statsCollector != null)
            {
                _statsCollector.IncrementStatsValue(StatisticsType.CacheCount);
            }
        }

        public CacheItem CacheGet(long rowId)
        {
            if(!_cache.ContainsKey(rowId))
                return null;
            CacheItem citem = null;
            _cache.TryGetValue(rowId, out citem);

            if(citem != null && !citem.Flag.IsAnyBitSet(BitsetConstants.DocumentDirty | BitsetConstants.MarkedForDeletion | BitsetConstants.MetaDataDirty))
                _evictionPolicy.Notify(rowId, citem.EvictionHint, null);
            return citem;
        }

        public CacheItem CacheGetWithoutNotify(long rowId)
        {

            CacheItem citem = null;
            _cache.TryGetValue(rowId,out citem);
            //_evictionPolicy.Notify(rowId, citem.EvictionHint, null);
            return citem;
        }

        public void CacheRemove(long rowId)
        {
            if (_cache.ContainsKey(rowId))
            {
                CacheItem tempItem = CacheGetWithoutNotify(rowId);
                _evictionPolicy.Remove(rowId, tempItem.EvictionHint);
                lock (_cacheSyncLock)
                {
                    _cache.Remove(rowId);
                }
                _cacheSize -= tempItem.Size;
                _cacheSpace.Release(this, tempItem.Size);

                if (_statsCollector != null)
                {
                    _statsCollector.DecrementStatsValue(StatisticsType.CacheCount);
                }
            }
        }

        public int CacheCount()
        {
            return _cache.Count;
        }

        #region ICacheSpaceConsumer Members
        public string Name
        {
            get { return _parent.Name; }
        }

        public bool IsEvictionEnabled
        {
            get { return _isEvictionEnabled; }
        }

        public long CacheSize
        {
            get { return _cacheSize; }
        }

        public CollectionIndexManager IndexManager
        {
            get { return _indexManager; }
        }


        public void EvictData(long size)
        {
            //TODO: evict items so that size amount of space is freed
            if (!_evictionPolicy.Execute(this, size))
            {
                //TODO: if eviction does not free space. 
                int a = 1;
            }
            //throw new NotImplementedException();
        }
        #endregion

        public void Dispose()
        {
            if(_isDisposed)
                return;
            _isDisposed = true;

            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                LoggerManager.Instance.StorageLogger.Warn("Collection is being disposed. ", "DatabaseId = " + _parent.DbContext.DatabaseName + " collection = " + Name + " .Persistence operations discarded = " + _dirtyDocuments.Count);

            if (_statsCollector != null)
            {
                _statsCollector.DecrementStatsValue(StatisticsType.PendingPersistentDocuments, _dirtyDocuments.Count);
            }

            if (_statsCollector != null)
            {
                _statsCollector.DecrementStatsValue(StatisticsType.CacheCount, _cache.Count);
                _statsCollector.DecrementStatsValue(StatisticsType.CacheSize, _cacheSize);
            }

            //if (_dirtyDocuments != null && _dirtyDocuments.Count>0)
            //{
            //    foreach (var pair in _dirtyDocuments)
            //    {
            //        lock (_unpersistedOperations)
            //        {
            //            _unpersistedOperations.Remove(pair.Key);
            //        }
            //    }
            //    _dirtyDocuments.Clear();
            //}

           if(_cacheSpace!=null)
               _cacheSpace.RemoveConsumer(this);

           if (_cache != null)
           {
               _cache.Clear();
               _cache = null;
           }

           if (_persistenceManager != null)
           {
               _persistenceManager.Dispose();
           }

           if (_indexManager != null)
           {
               _indexManager.Dispose();
           }
        }
    }
}
