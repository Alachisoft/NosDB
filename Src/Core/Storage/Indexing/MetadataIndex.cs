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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Providers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Common.Logger;
using UpdateOperation = Alachisoft.NosDB.Core.Storage.Operations.UpdateOperation;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class MetadataIndex : IEnumerable<long>, IDisposable
    {
        protected readonly DoubleVector<DocumentKey, long> _keyToRowIndex;
        protected HashVector<int, ClusteredList<DocumentKey>> _bucketKeyIndex;
        protected readonly HashVector<long, long> _rowToFileIndex = new HashVector<long, long>();
        protected readonly ClusteredHashSet<long> _enumerationSet;
        protected BaseCollection _parent;
        protected long _lastRowId;
        protected IPersistenceProvider _metadataPersister;
        protected const char _seperator = '_';
        protected IStatsCollector _statsCollector;
        
        public IPersistenceProvider MetadataPersister
        {
            get { return _metadataPersister; }
        }

        public MetadataIndex(StatsIdentity statsIdentity, IPersistenceProvider metadataPersister)
        {
            _keyToRowIndex = new DoubleVector<DocumentKey, long>();
            _bucketKeyIndex = new HashVector<int, ClusteredList<DocumentKey>>();
            _enumerationSet = new ClusteredHashSet<long>();

            _statsCollector = StatsManager.Instance.GetStatsCollector(statsIdentity);
            _metadataPersister = metadataPersister;
        }

        public int KeyCount
        {
            get { return _keyToRowIndex.Count; }
        }

        public int ValueCount
        {
            get { return _keyToRowIndex.Count; }
        }

        public int RowToFileIndexCount
        {
            get { return _rowToFileIndex.Count; }
        }

        public virtual bool Initialize(BaseCollection parent)
        {
            try
            {
                _parent = parent;
                bool check = _metadataPersister.CreateCollection(GetLasRowIdCollection(), typeof(string), typeof(long));

                if (check)
                {
                    ITransaction transaction = _metadataPersister.BeginTransaction(null, false);
                    _metadataPersister.StoreDocument(transaction, GetLasRowIdCollection(), "LastRowId", _lastRowId);
                    _metadataPersister.Commit(transaction);
                }

                _metadataPersister.CreateCollection(GetKeyMetadataCollection(), typeof(long), typeof(byte[]));

                RegenerateLastRowId();
                RegeneratekeyMetadata();

                //if (_statsCollector != null)
                //{
                //    _statsCollector.IncrementStatsValue(StatisticsType.DocumentCount, _rowToFileIndex.Count);
                //    SetAverageDocumentSize();
                //}
                return true;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Error Initializing MetadataIndex.", e);
                throw;
            }
        }

        protected string GetLasRowIdCollection()
        {
            return _parent.Name + _seperator + "LastRowId";
        }

        protected string GetKeyMetadataCollection()
        {
            return _parent.Name + _seperator + "Metadata";
        }

        public long GenerateRowId()
        {
            return Interlocked.Increment(ref _lastRowId);
        }

        public virtual void Add(DocumentKey key, long rowId, IJSONDocument document, IOperationContext context, long size = 0)
        {
            lock (_keyToRowIndex)
            {
                _keyToRowIndex[key] = rowId;
                _enumerationSet.Add(rowId);
            }
            AddBucketKeyIndex(key);
            AddSize(rowId, document, size, context);
        }

        protected virtual void AddSize(long rowId, IJSONDocument document, long size, IOperationContext context)
        {
        }

        public virtual void Update(DocumentKey key, UpdateOperation operation)
        {
        }

        protected void AddBucketKeyIndex(DocumentKey key)
        {
            HashMapBucket bucket = _parent.GetKeyBucket(key);
            if (bucket != null)
            {
                lock (_bucketKeyIndex)
                {
                    if (!_bucketKeyIndex.ContainsKey(bucket.BucketId))
                        _bucketKeyIndex.Add(bucket.BucketId, new ClusteredList<DocumentKey>());
                    _bucketKeyIndex[bucket.BucketId].Add(key);
                }
            }
        }

        public virtual void Remove(DocumentKey key)
        {
            lock (_keyToRowIndex)
            {
                long rowId;
                if (_keyToRowIndex.TryGetValue(key, out rowId))
                {
                    if (!_keyToRowIndex.Remove(key))
                        Console.WriteLine("Failure for key: " + key);

                    if (rowId != -1)
                        _enumerationSet.Remove(rowId);
                }
            }
            RemoveBucketKeyIndex(key);
           
        }

        protected void RemoveBucketKeyIndex(DocumentKey key)
        {
            HashMapBucket bucket = _parent.GetKeyBucket(key);
            if (bucket != null)
            {
                lock (_bucketKeyIndex)
                {
                    if (_bucketKeyIndex.ContainsKey(bucket.BucketId))
                        _bucketKeyIndex[bucket.BucketId].Remove(key);
                }
            }
        }

        public void Add(long rowId, long fileId)
        {
            lock (_rowToFileIndex)
            {
                _rowToFileIndex[rowId] = fileId;
            }
        }

        public virtual long GetRowId(DocumentKey key)
        {
            lock (_keyToRowIndex)
            {
                long rowId;
                if (_keyToRowIndex.TryGetValue(key, out rowId))
                    return rowId;
            }
            return -1;
        }

        public ClusteredList<DocumentKey> GetKeysForBucket(int bucketId)
        {
            ClusteredList<DocumentKey> list = null;
            lock (_bucketKeyIndex)
                if (!_bucketKeyIndex.TryGetValue(bucketId, out list))
                    list = new ClusteredList<DocumentKey>();
            return list;
        }

        public virtual long GetFileId(long rowId)
        {
            long fileId;
            lock (_rowToFileIndex)
                if (_rowToFileIndex.TryGetValue(rowId, out fileId))
                    return fileId;
            return -1;
        }

       
        public virtual bool ContainsRowId(long rowId)
        {
            return _keyToRowIndex.ContainsValue(rowId);
        }

        public virtual  bool ContainsKey(DocumentKey key)
        {
            return _keyToRowIndex.ContainsKey(key);
        }

        public virtual StoreResult StoreKeyMetadata(ITransaction metadataTransaction, JSONDocument document, long rowId)
        {
            StorageResult<byte[]> result = _metadataPersister.StoreDocument(metadataTransaction,
                GetKeyMetadataCollection(), rowId, GetSerializedMetadata(document.Key, rowId));

            if (result.Status == StoreResult.Success || result.Status == StoreResult.SuccessOverwrite)
            {
                //if (_statsCollector != null && result.Status == StoreResult.Success)
                //{
                //    _statsCollector.IncrementStatsValue(StatisticsType.DocumentCount);
                //    SetAverageDocumentSize();
                //}
                return StoreResult.Success;
            }

            return result.Status;
        }

        public virtual StoreResult RemovekeyMetadata(ITransaction metadataTransaction, JSONDocument document, long rowId, IOperationContext context)
        {
            StorageResult<byte[]> result = _metadataPersister.DeleteDocument<long, byte[]>(metadataTransaction, GetKeyMetadataCollection(), rowId);
            if (result.Status == StoreResult.Success || result.Status == StoreResult.SuccessDelete || result.Status == StoreResult.SuccessKeyDoesNotExist)
            {
                lock (_rowToFileIndex)
                {
                    _rowToFileIndex.Remove(rowId);
                }

                if (result.Status != StoreResult.SuccessKeyDoesNotExist)
                {
                    //if (_statsCollector != null)
                    //{
                    //    _statsCollector.DecrementStatsValue(StatisticsType.DocumentCount);
                    //    SetAverageDocumentSize();
                    //}
                }

                lock (_keyToRowIndex)
                {
                    _enumerationSet.Remove(rowId);
                }
                return StoreResult.Success;
            }
            return result.Status;
        }

        public void StoreLastRowId(ITransaction transaction)
        {
            if(!transaction.IsReadOnly)
            {
                StorageResult<long> result = _metadataPersister.UpdateDocument(transaction.InnerObject as ITransaction, GetLasRowIdCollection(), "LastRowId", _lastRowId);
                if (result.Status != StoreResult.SuccessOverwrite && result.Status != StoreResult.Success)
                {
                    throw new Exception("Error storing lastRowId. Status = " + result.Status);
                }

            }
        }

        public virtual void OnCommit()
        {
            //do nothing as it is for DiskBased metadataindex
        }

        public virtual void OnRollback()
        {
            //do nothing as it is for DiskBased metadataindex
        }


        #region regenerateMetadata
        protected void RegenerateLastRowId()
        {
            StorageResult<long> rowId = _metadataPersister.GetDocument<string, long>(GetLasRowIdCollection(), "LastRowId");
            _lastRowId = rowId.Document;
        }

        protected virtual void RegeneratekeyMetadata()
        {

            IDataReader<long, byte[]> dataReader = _metadataPersister.GetAllDocuments<long, byte[]>(GetKeyMetadataCollection());

            while (dataReader.MoveNext())
            {
                KeyValuePair<long, byte[]> kvp = dataReader.Current();
                //inMemory Store 
                DocumentKey docKey;// = DeserializeKey(kvp.Key);
                long rowId, fileId, size;
                string key;
                DeserializeMetadata(kvp.Value, out key, out fileId, out size);
                docKey = new DocumentKey(key);
                rowId = kvp.Key;
                //JSONDocument jDoc = DeserializeDocument(kvp.Value);
                ReloadSizes(rowId, size);

                if (rowId > _lastRowId)
                    _lastRowId = rowId;

                Add(docKey, rowId, null, new OperationContext(), size);
                _rowToFileIndex[rowId] = fileId;
                //_keyToRowIndex[new DocumentKey(kvp.Key)] = kvp.Value;
                //_enumerationSet.Add(kvp.Value);
                //if (_parent.Distribution != null)
                //    AddBucketKeyIndex(docKey);
            }
            dataReader.Dispose();
            //_metadataPersister.Commit(regenTransaction);
        }

        protected virtual void ReloadSizes(long rowId, long size)
        {
            //Do nothing as it is a capped collectio thing
        }

        #endregion

        #region IEnumerable<long> members

        public virtual IEnumerator<long> GetEnumerator()
        {
            return _enumerationSet.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _enumerationSet.GetEnumerator();
        }

        #endregion

        #region Serialization/DeSerialization

        protected byte[] SerializeKey(DocumentKey key)
        {
            return CompactBinaryFormatter.ToByteBuffer(key, "");
        }

        protected DocumentKey DeserializeKey(byte[] dataBytes)
        {
            return (DocumentKey)CompactBinaryFormatter.FromByteBuffer(dataBytes, "");
        }

        protected byte[] SerializeDocument(JSONDocument document)
        {
            return CompactBinaryFormatter.ToByteBuffer(document, "");
        }

        protected JSONDocument DeserializeDocument(byte[] dataBytes)
        {
            return (JSONDocument)CompactBinaryFormatter.FromByteBuffer(dataBytes, "");
        }

        protected virtual byte[] GetSerializedMetadata(string key,  long rowId)
        {
            var stringBytes = Encoding.UTF8.GetBytes(key);
            var bytes = new byte[stringBytes.Length + 8];
            SerializationUtility.PutInt64(bytes, 0, _rowToFileIndex[rowId]);
            stringBytes.CopyTo(bytes, 8);
            return bytes;
        }

        protected virtual void DeserializeMetadata(byte[] bytes, out string key, out long fileId, out long size)
        {
            fileId = SerializationUtility.GetInt64(bytes, 0);
            key = Encoding.UTF8.GetString(bytes, 8, bytes.Length - 8);
            size = 0;
        }
        #endregion

        public virtual DocumentKey GetDocKey(long rowId)
        {
            DocumentKey key;
            lock (_keyToRowIndex)
            {
                _keyToRowIndex.TryGetKey(rowId, out key);
            }
            return key;
        }

        #region Iterators
        public virtual long this[DocumentKey key]
        {
            get { return _keyToRowIndex[key]; }
            set
            {
                lock (_keyToRowIndex)
                {
                    _keyToRowIndex[key] = value;
                }
            }
        }
        public virtual DocumentKey this[long row]
        {
            get { return _keyToRowIndex[row]; }
            set
            {
                lock (_keyToRowIndex)
                {
                    _keyToRowIndex[row] = value;
                }
            }
        }

        #endregion

        public virtual  bool TryGetKey(long rowId, out DocumentKey key)
        {
            lock (_keyToRowIndex)
            {
                return _keyToRowIndex.TryGetKey(rowId, out key);
            }
        }

        public virtual bool TryGetRowId(DocumentKey key, out long rowId)
        {
            lock (_keyToRowIndex)
            {
                return _keyToRowIndex.TryGetValue(key, out rowId);
            }
        }

        public void Destroy()
        {
            //2 Reasons to comment the lines below
            
            //1) No Need of this because at the end we delete the file. 
            //2) In Case of esent session violation exception occurs

            _metadataPersister.DropCollection(GetLasRowIdCollection());
            _metadataPersister.DropCollection(GetKeyMetadataCollection());
          
            //Do not remove FileMetadata as it is related to main DB files and is common (only to be removed on databaseRemoval)
            //_metadataPersister.DropCollection(GetFileMetadataCollection());
            //throw new System.NotImplementedException();
        }

        #region MetadataIndex Rollback Methods
        internal virtual void RollbackInsertOperation(ITransaction transaction,
            KeyValuePair<DocumentKey, MetaDataIndexOperation> operation)
        {
            lock (_rowToFileIndex)
            {
                _rowToFileIndex.Remove(operation.Value.RowId);
            }
            lock (_keyToRowIndex)
            {
                _keyToRowIndex.Remove(operation.Key);
            }
            //if (_statsCollector != null)
            //    _statsCollector.DecrementStatsValue(StatisticsType.DocumentCount);
            RemoveBucketKeyIndex(operation.Key);
        }

        internal virtual void RollbackUpdateOperation(ITransaction transaction,
            KeyValuePair<DocumentKey, MetaDataIndexOperation> operation)
        {
            lock (_rowToFileIndex)
            {
                _rowToFileIndex[operation.Value.RowId] = operation.Value.FileId;
            }
            lock (_keyToRowIndex)
            {
                _keyToRowIndex[operation.Key] = operation.Value.RowId;
            }
            AddBucketKeyIndex(operation.Key);
        }

        internal virtual void RollbackRemoveOperation(ITransaction transaction,
            KeyValuePair<DocumentKey, MetaDataIndexOperation> operation)
        {
            RollbackUpdateOperation(transaction, operation);
            //if (_statsCollector != null)
            //    _statsCollector.IncrementStatsValue(StatisticsType.DocumentCount);
        }
        #endregion

        public virtual void Dispose()
        {
            _keyToRowIndex.Clear();
            _rowToFileIndex.Clear();
            _bucketKeyIndex.Clear();
        }
    }
}
