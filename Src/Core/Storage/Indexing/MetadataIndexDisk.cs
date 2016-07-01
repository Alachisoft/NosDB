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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Providers;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class MetadataIndexDisk : MetadataIndex, IEnumerable<long>
    {
        private ClusteredHashSet<long> _transactionRowIds;
        private ClusteredHashSet<DocumentKey> _keysDeleted;

        public MetadataIndexDisk(StatsIdentity statsIdentity, IPersistenceProvider metadataPersister)
            : base(statsIdentity, metadataPersister)
        {
            _transactionRowIds = new ClusteredHashSet<long>();
            _keysDeleted = new ClusteredHashSet<DocumentKey>();
        }

        public override bool Initialize(BaseCollection parent)
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

                _metadataPersister.CreateCollection(GetKeyMetadataCollection(), typeof(string), typeof(byte[]));
                _metadataPersister.CreateCollection(GetRowMetadataCollection(), typeof(long), typeof(byte[]));

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

        protected string GetRowMetadataCollection()
        {
            return _parent.Name + _seperator + "RMetadata";
        }

        public override void Add(DocumentKey key, long rowId, IJSONDocument document, IOperationContext context, long size = 0)
        {
            lock (_keyToRowIndex)
            {
                _keyToRowIndex[key] = rowId;
                //_enumerationSet.Add(rowId);
                lock (_keysDeleted)
                {
                    if (_keysDeleted.Contains(key))
                        _keysDeleted.Remove(key);
                }
            }
            //AddBucketKeyIndex(key);
            AddSize(rowId, document, size, context);
        }

        public override void Remove(DocumentKey key)
        {
            long rowId;

            lock (_keyToRowIndex)
            {
                if (_keyToRowIndex.TryGetValue(key, out rowId))
                {
                    if (!_keyToRowIndex.Remove(key))
                        Console.WriteLine("Failure for key: " + key);

                    if (rowId != -1)
                        _enumerationSet.Remove(rowId);
                }
            }
           
        }

        private void GetK2RFromStore(DocumentKey key, out long rowId, out long fileId, out long size)
        {
            StorageResult<byte[]> result = _metadataPersister.GetDocument<string, byte[]>(GetKeyMetadataCollection(), key.ToString());
            if (result.Document != null)
            {
                DeserializeMetadata(result.Document, out rowId, out fileId, out size);
                return;
            }
            rowId = -1;
            fileId = -1;
            size = -1;
        }

        private void GetR2KFromStore(long rowId, out DocumentKey key)
        {
            StorageResult<string> result = _metadataPersister.GetDocument<long, string>(GetRowMetadataCollection(), rowId);
            if (result.Document != null)
            {
                key = new DocumentKey(result.Document);
                return;
            }
            key = null;
        }

        public override long GetRowId(DocumentKey key)
        {
            long rowId;
            long fileId;
            long size;
            lock (_keyToRowIndex)
            {
                if (_keyToRowIndex.TryGetValue(key, out rowId))
                    return rowId;

                lock (_keysDeleted)
                {
                    if (_keysDeleted.Contains(key))
                        return -1;
                }
            }

            GetK2RFromStore(key, out rowId, out fileId, out size);
            return rowId;
        }

        public override long GetFileId(long rowId)
        {
            long fileId;
            long size = -1;
            lock (_rowToFileIndex)
            {
                if (_rowToFileIndex.TryGetValue(rowId, out fileId))
                    return fileId;
            }
            DocumentKey dkey;
            GetR2KFromStore(rowId, out dkey);
            if (dkey != null)
            {
                GetK2RFromStore(dkey, out rowId, out fileId, out size);
                return fileId;
            }
            else
            {
                return -1;
            }
        }

        public override DocumentKey GetDocKey(long rowId)
        {
            DocumentKey key;
            lock (_keyToRowIndex)
            {
                if (_keyToRowIndex.TryGetKey(rowId, out key))
                    return key;
            }
            GetR2KFromStore(rowId, out key);
            return key;
        }

        public override bool TryGetKey(long rowId, out DocumentKey key)
        {
            key = GetDocKey(rowId);
            return key != null;
        }

        public override bool TryGetRowId(DocumentKey key, out long rowId)
        {
            rowId = GetRowId(key);
            return rowId != -1;
        }

        public override bool ContainsRowId(long rowId)
        {
            lock (_keyToRowIndex)
            {
                if (_keyToRowIndex.ContainsValue(rowId))
                    return true;
            }

            DocumentKey dkey;
            GetR2KFromStore(rowId, out dkey);
            return dkey != null;
        }

        public override bool ContainsKey(DocumentKey key)
        {
            lock (_keyToRowIndex)
            {
                if (_keyToRowIndex.ContainsKey(key))
                    return true;

                lock (_keysDeleted)
                {
                    if (_keysDeleted.Contains(key))
                        return false;
                }
            }

            long rowId, fileId, size;
            GetK2RFromStore(key, out rowId, out fileId, out size);
            return rowId != -1;
        }

        public override StoreResult StoreKeyMetadata(ITransaction metadataTransaction, JSONDocument document, long rowId)
        {
            StorageResult<byte[]> result = _metadataPersister.StoreDocument(metadataTransaction,
                GetKeyMetadataCollection(), document.Key, GetSerializedMetadata(document.Key, rowId));

            if (result.Status == StoreResult.Success || result.Status == StoreResult.SuccessOverwrite)
            {
                //if (_statsCollector != null && result.Status == StoreResult.Success)
                //{
                //    _statsCollector.IncrementStatsValue(StatisticsType.DocumentCount);
                //}
                //SetAverageDocumentSize();
                //return StoreResult.Success;
            }
            else
                return result.Status;

            StorageResult<string> resultR2K = _metadataPersister.StoreDocument(metadataTransaction,
                GetRowMetadataCollection(), rowId, document.Key.ToString());

            if (resultR2K.Status == StoreResult.Success || resultR2K.Status == StoreResult.SuccessOverwrite)
            {
                lock (_transactionRowIds)
                {
                    _transactionRowIds.Add(rowId);
                }
                return StoreResult.Success;
            }
            return resultR2K.Status;
        }

        public override StoreResult RemovekeyMetadata(ITransaction metadataTransaction, JSONDocument document, long rowId, IOperationContext context)
        {
            StorageResult<byte[]> result = _metadataPersister.DeleteDocument<string, byte[]>(metadataTransaction, GetKeyMetadataCollection(), document.Key);
            if (result.Status == StoreResult.Success || result.Status == StoreResult.SuccessDelete ||
                result.Status == StoreResult.SuccessKeyDoesNotExist)
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
                //return StoreResult.Success;
            }
            else
            {
                return result.Status;
            }

            StorageResult<byte[]> resultR2K = _metadataPersister.DeleteDocument<long, byte[]>(metadataTransaction, GetRowMetadataCollection(), rowId);
            if (result.Status == StoreResult.Success || result.Status == StoreResult.SuccessDelete ||
                result.Status == StoreResult.SuccessKeyDoesNotExist)
            {
                return StoreResult.Success;
            }
            return resultR2K.Status;
        }

        public override void OnCommit()
        {
            lock (_transactionRowIds)
            {
                foreach (long rowPersisted in _transactionRowIds)
                {
                    lock (_rowToFileIndex)
                    {
                        DocumentKey key = GetDocKey(rowPersisted);
                        if (key != null)
                        {
                            long rowId;
                            if (_keyToRowIndex.TryGetValue(key, out rowId))
                            {
                                if (rowId == rowPersisted)
                                    Remove(key);
                            }
                        }
                        _rowToFileIndex.Remove(rowPersisted);
                        _transactionRowIds.Remove(rowPersisted);
                    }
                }
                _transactionRowIds.Clear();
            }
        }

        public override void OnRollback()
        {
            lock (_transactionRowIds)
            {
                _transactionRowIds.Clear();
            }
        }

        protected override void RegeneratekeyMetadata()
        {
        }

        public override IEnumerator<long> GetEnumerator()
        {
            IDataReader<long, string> reader = _metadataPersister.GetAllDocuments<long, string>(GetRowMetadataCollection());
            while (reader.MoveNext())
            {
                yield return reader.Current().Key;
            }
            reader.Dispose();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override byte[] GetSerializedMetadata(string key, long rowId)
        {
            var bytes = new byte[16];
            SerializationUtility.PutInt64(bytes, 0, rowId);
            SerializationUtility.PutInt64(bytes, 8, _rowToFileIndex[rowId]);
            return bytes;
        }

        protected void DeserializeMetadata(byte[] bytes, out long rowId, out long fileId, out long size)
        {
            rowId = SerializationUtility.GetInt64(bytes, 0);
            fileId = SerializationUtility.GetInt64(bytes, 8);
            size = 0;
        }

        public override long this[DocumentKey key]
        {
            get
            {
                return GetRowId(key);
            }
            set
            {
                lock (_keyToRowIndex)
                {
                    _keyToRowIndex[key] = value;
                }
            }
        }

        public override DocumentKey this[long row]
        {
            get { return GetDocKey(row); }
            set
            {
                lock (_keyToRowIndex)
                {
                    _keyToRowIndex[row] = value;
                }
            }
        }

    }
}
