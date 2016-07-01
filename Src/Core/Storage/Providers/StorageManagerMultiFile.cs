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
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Core.Storage.Providers.LMDB;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class StorageManagerMultiFile : StorageManagerBase
    {
        #region IPersistenceProvider Members

        public override long CollectionSize(string collection)
        {
            long result = 0;
            foreach (KeyValuePair<string, FileMetadata<long, byte[]>> fileMetadata in _fileMetadataDictionary)
            {
                result += fileMetadata.Value.Provider.CollectionSize(collection);
            }
            return result;
        }

        public override long CollectionDocumentCount(string collection)
        {
            long result = 0;
            foreach (KeyValuePair<string, FileMetadata<long, byte[]>> fileMetadata in _fileMetadataDictionary)
            {
                result += fileMetadata.Value.Provider.CollectionDocumentCount(collection);
            }
            return result;
        }

        public override StorageResult<byte[]> StoreDocument(ITransaction transaction, string collection, long key, byte[] value)
        {
            lock (_oneOppLock)
            {
                PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;

                if (!_collectionMetadataDictionary.ContainsKey(collection))
                    throw new ArgumentException("Specified collection '" + collection + "' does not exist.");

                if (pmTransaction != null)
                {
                    KeyMetadata keyMetadata = new KeyMetadata(key, GenerateFileId(pmTransaction.MetadataTransaction));
                    string dbId = GetFullDbId(keyMetadata.FileId);
                    FileMetadata<long, byte[]> fileMetadata = _fileMetadataDictionary[dbId];

                    if (fileMetadata == null)
                        throw new Exception("unable to find database file.");

                    if (!pmTransaction.DataTransaction.IsTransactionBegin(dbId))
                        pmTransaction.DataTransaction.Begin(dbId, fileMetadata.Provider.BeginTransaction(null, false));

                    StorageResult<byte[]> result = fileMetadata.Provider.StoreDocument(pmTransaction.DataTransaction.GetTransaction(dbId), collection, keyMetadata.RowId,
                        value);

                    result.FileId = keyMetadata.FileId;

                    if (result.Status == StoreResult.FailureDatabaseFull)
                    {
                        StorageResult<byte[]> insertResult = StoreDocument(transaction, collection, key, value);
                        result.Status = insertResult.Status;
                        result.FileId = insertResult.FileId;
                    }

                    DisplaySize();
                    return result;
                }
            }
            return null;
        }

        public override StorageResult<byte[]> UpdateDocument(ITransaction transaction, long fileId, string collection, long key, byte[] update)
        {
            lock (_oneOppLock)
            {
                PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;

                if (!_collectionMetadataDictionary.ContainsKey(collection))
                    throw new ArgumentException("Specified collection '" + collection + "' does not exist.");

                string dbId = GetFullDbId(fileId);
                FileMetadata<long, byte[]> fileMetadata = _fileMetadataDictionary[dbId];

                if (fileMetadata == null)
                    throw new Exception("Unable to find database file.");

                if (pmTransaction != null)
                {
                    FileTransaction fileTransaction = pmTransaction.DataTransaction;
                    if (!fileTransaction.IsTransactionBegin(dbId))
                        fileTransaction.Begin(dbId, fileMetadata.Provider.BeginTransaction(null, false));

                    StorageResult<byte[]> result = fileMetadata.Provider.UpdateDocument(fileTransaction.GetTransaction(dbId), collection, key,
                        update);

                    if (result.Status == StoreResult.FailureDatabaseFull)
                    {
                        StorageResult<byte[]> deleteResult = DeleteDocument(transaction, fileId, collection, key);
                        if (deleteResult.Status == StoreResult.Failure)
                            throw new Exception("Error deleting document from store. key = " + key);

                        //todo: this decrement is not a proper fix as increments are on one level and decrements are on another level.
                        //if (_statsCollector != null)
                        //{
                        //    _statsCollector.DecrementStatsValue(StatisticsType.DocumentCount);
                        //}
                    
                        StorageResult<byte[]> insertResult = StoreDocument(transaction, collection, key, update);
                        result.Status = insertResult.Status;
                        result.FileId = insertResult.FileId;

                        DisplaySize();
                        return result;
                    }

                    result.FileId = fileId;
                    DisplaySize();
                    return result;
                }
            }
            return null;
        }

        #endregion

        #region StorageManagerBase Members
        public override long GenerateFileId(ITransaction transaction)    //check file size and then generate fileId
        {
            ITransaction innerTransaction = transaction.InnerObject as LMDBPersistenceProvider.LMDBTransaction;

            for (int i = 1; i <= _dbIndex; i++)  //always start with 1
            {
                string dbId = GetFullDbId(i);
                FileMetadata<long, byte[]> fileMetadata = _fileMetadataDictionary[dbId];
                if (!fileMetadata.Provider.IsDatabaseFull())
                    return i;
            }
            CreateNextFile(innerTransaction);

            CreateCollectionsInNewFile(GetFullDbId(_dbIndex));
            return _dbIndex;
        }

        protected void CreateCollectionsInNewFile(string dbId)
        {
            foreach (var kvp in _collectionMetadataDictionary)
            {
                _fileMetadataDictionary[dbId].Provider.CreateCollection(kvp.Key, typeof(long), typeof(byte[]));
            }
        }
        #endregion

   }
}
