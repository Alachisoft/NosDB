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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Core.Util;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class PersistenceManager : IPersistenceManager
    {
        private StorageManagerBase _storageManager;
        private Dictionary<string, MetadataIndex> _metadataIndices;
        private bool enableCompression = true;

        public bool Initialize(StorageManagerBase storangeManager)
        {
            try
            {
            }
            finally
            {
                enableCompression = Convert.ToBoolean(ConfigurationSettings.AppSettings["CompressionEnabled"]);
            }

            _storageManager = storangeManager;
            _metadataIndices = new Dictionary<string, MetadataIndex>();
            return true;
        }

        public void AddMetadataIndex(string collection, MetadataIndex metadataIndex)
        {
            lock (_metadataIndices)
            {
                _metadataIndices.Add(collection,metadataIndex);
            }
        }

        public void RemoveMetadataIndex(string collection)
        {
            lock (_metadataIndices)
            {
                _metadataIndices.Remove(collection);
            }
        }

        public StoreResult StoreDocument(ITransaction transaction, string collection, long rowId, JSONDocument document)
        {
            var pmTransaction = transaction as PersistenceManagerTransaction;
            
            StorageResult<byte[]> storageResult = _storageManager.StoreDocument(pmTransaction, collection, rowId,SerializeDocument(document));
            
            if (storageResult.Status != StoreResult.Success)
                return storageResult.Status;

            lock (_metadataIndices)
            {
                _metadataIndices[collection].Add(rowId, storageResult.FileId);
            }
            StoreResult metadataResult =_metadataIndices[collection].StoreKeyMetadata(pmTransaction.MetadataTransaction.InnerObject as ITransaction, document, rowId);

            if (metadataResult != StoreResult.Success) //undo data operation
            {
                _storageManager.DeleteDocument(pmTransaction, storageResult.FileId, collection, rowId);
                return metadataResult;
            }
            return StoreResult.Success;
        }

        public StoreResult UpdateDocument(ITransaction transaction, string collection, long rowId, JSONDocument update)
        {
            PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;
            StorageResult<byte[]> storageResult = _storageManager.UpdateDocument(pmTransaction,
                    _metadataIndices[collection].GetFileId(rowId), collection, rowId, SerializeDocument(update));
            if (storageResult.Status != StoreResult.Success)
                return storageResult.Status;

            lock (_metadataIndices)
            {
                _metadataIndices[collection].Add(rowId, storageResult.FileId);
            }
            MetaDataIndexOperation operation = new MetaDataIndexOperation() { ActualDocumentSize = update.Size, FileId = storageResult.FileId, OperationType = OperationType.Update, RowId = _metadataIndices[collection].GetRowId(new DocumentKey(update.Key)) };
            (pmTransaction.MetadataTransaction as MetaDataIndexTransaction).AddOperation(new DocumentKey(update.Key), operation);
            StoreResult metadataResult = _metadataIndices[collection].StoreKeyMetadata(pmTransaction.MetadataTransaction.InnerObject as ITransaction, update, rowId);

            if (metadataResult != StoreResult.Success) //undo data operation
            {
                //_storageManager.DeleteDocument(pmTransaction.DataTransaction, storageResult.FileId, _collectionName, rowId);
                return metadataResult;
            }
            return StoreResult.Success;
        }

        public JSONDocument GetDocument(string collection, long rowId)
        {
            //PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;
            var result = _storageManager.GetDocument(_metadataIndices[collection].GetFileId(rowId), collection, rowId);
            if (result.Status == StoreResult.Success)
                return DeserializeDocument(result.Document);
            return null;
        }

        public StoreResult RemoveDocument(ITransaction transaction, string collection, long rowId, JSONDocument document, IOperationContext context)
        {
            PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;
            StorageResult<byte[]> storageResult = _storageManager.DeleteDocument(pmTransaction, _metadataIndices[collection].GetFileId(rowId), collection, rowId);
            if (storageResult.Status != StoreResult.Success && storageResult.Status != StoreResult.SuccessDelete && storageResult.Status != StoreResult.SuccessKeyDoesNotExist)
                return storageResult.Status;
            MetaDataIndexOperation operation = new MetaDataIndexOperation() { ActualDocumentSize = document.Size, FileId = storageResult.FileId, OperationType = OperationType.Remove, RowId = rowId };
            (pmTransaction.MetadataTransaction as MetaDataIndexTransaction).AddOperation(new DocumentKey(document.Key), operation);
            StoreResult metadataResult = _metadataIndices[collection].RemovekeyMetadata(pmTransaction.MetadataTransaction.InnerObject as ITransaction, document, rowId, context);

            if (metadataResult != StoreResult.Success) //undo data operation
            {
                _storageManager.StoreDocument(pmTransaction, collection, rowId, SerializeDocument(document));
                return metadataResult;
            }
            return StoreResult.SuccessDelete;
        }

        public long GenerateRowId(string collectionName)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<long, JSONDocument>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public ITransaction BeginTransaction(ITransaction parentTransaction, bool isReadOnly)
        {
            PersistenceManagerTransaction pmTransaction = new PersistenceManagerTransaction();
            pmTransaction.DataTransaction = _storageManager.BeginTransaction(parentTransaction, isReadOnly);
            if(!isReadOnly)
                pmTransaction.MetadataTransaction = new MetaDataIndexTransaction(_storageManager.BeginMetadataTransaction(isReadOnly));
            return pmTransaction;
        }

        public void Commit(ITransaction transaction)
        {
            var w1 = new Stopwatch();
            var w2 = new Stopwatch();
            var w3 = new Stopwatch();

            var pmTransaction = transaction as PersistenceManagerTransaction;
            //will be null for read transactions to be exact.
            if (pmTransaction != null && pmTransaction.MetadataTransaction != null)
            {
                var metaDataIndexTransaction = pmTransaction.MetadataTransaction as MetaDataIndexTransaction;
                if (metaDataIndexTransaction != null)
                    metaDataIndexTransaction.ClearPerformedOperations();
                lock (_metadataIndices)
                {
                    w1.Start();
                    foreach (KeyValuePair<string, MetadataIndex> kvp in _metadataIndices)
                    {
                        _metadataIndices[kvp.Key].StoreLastRowId(pmTransaction.MetadataTransaction);
                    }
                    w1.Stop();
                }
                
                w2.Start();
                _storageManager.CommitMetadataTransaction(pmTransaction.MetadataTransaction.InnerObject as ITransaction);
                w2.Stop();
            }

            w3.Start();
            if (pmTransaction != null) _storageManager.Commit(pmTransaction.DataTransaction);
            w3.Stop();
            
            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsDebugEnabled)
            {
                LoggerManager.Instance.StorageLogger.Debug("PersistanceManager.Commit","MetadataIndices Commit: " + w1.ElapsedMilliseconds + ", Commit Metadata Transaction: " +w2.ElapsedMilliseconds + ", Storage Manager Commit: " + w3.ElapsedMilliseconds);
            }

            foreach (KeyValuePair<string, MetadataIndex> kvp in _metadataIndices)
            {
                _metadataIndices[kvp.Key].OnCommit();
            }


        }

        private void RollbackMetaDataIndex(ITransaction transaction)
        {
            MetaDataIndexTransaction metadataTransaction = transaction as MetaDataIndexTransaction;
            foreach (var operation in metadataTransaction.OperationsPerformedOnMetaData)
            {
                switch (operation.Value.OperationType)
                {
                    //For now we only need to revert remove and update operations only
                    /*case OperationType.Add:
                        _metadataIndices.RollbackInsertOperation(transaction, operation);
                        break;*/
                    case OperationType.Update:
                        lock (_metadataIndices)
                        {
                            foreach (KeyValuePair<string, MetadataIndex> kvp in _metadataIndices)
                            {
                                _metadataIndices[kvp.Key].RollbackUpdateOperation(transaction, operation);
                            }
                        }
                        break;
                    case OperationType.Remove:
                        lock (_metadataIndices)
                        {
                            foreach (KeyValuePair<string, MetadataIndex> kvp in _metadataIndices)
                            {
                                _metadataIndices[kvp.Key].RollbackRemoveOperation(transaction, operation);
                            }
                        }
                        break;
                    default:
                        //TODO: Cannot rollback. Invalid Operation Type
                        if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                            LoggerManager.Instance.StorageLogger.Debug("PresistenceManager.RollbackMetaDataIndex() ","PersistenceManager.cs. Unable to Rollback invalid operation type.");
                        break;
                }
            }
        }

        public void Rollback(ITransaction transaction)
        {
            PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;
            _storageManager.RollbackMetadataTransaction(pmTransaction.MetadataTransaction);
            RollbackMetaDataIndex(pmTransaction.MetadataTransaction);
            _storageManager.Rollback(pmTransaction.DataTransaction);
        }

        public MetadataIndex MetadataIndex(string collection)
        {
            return _metadataIndices[collection];
        }

        #region Serialization/DeSerialization
        
        private byte[] SerializeDocument(JSONDocument document)
        {
            Stream stream = new ClusteredMemoryStream();
            JSONDocument.Serialize(stream, document);
            stream.Position = 0;
            if (enableCompression)
            {
                stream = CompressionUtil.Compress(stream);
            }

            var array = new byte[stream.Length + 1];
            if (enableCompression)
                array[0] |= (byte) PersistenceBits.Compressed;

            stream.Read(array, 1, (int)stream.Length);
            stream.Dispose();
            return array;

        }

        private JSONDocument DeserializeDocument(byte[] data)
        {
            var stream = new ClusteredMemoryStream(data);
            int header = stream.ReadByte();
            if ((header & (long) PersistenceBits.Compressed) == (decimal) PersistenceBits.Compressed)
            {
                stream = CompressionUtil.Decompress(stream);
            }
            var document = JSONDocument.Deserialize(stream);// CompactBinaryFormatter.Deserialize(stream, string.Empty);
            stream.Dispose();
            return document as JSONDocument;

        }
        
        #endregion

        public void Dispose()
        {

        }
    }
}
