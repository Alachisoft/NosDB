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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class StorageManagerSingleFile<TPersistenceProvider> : StorageManagerBase, IFilePersistenceProvider
    {
        #region IPersistenceProvider Members

        public override bool Initialize(StorageConfiguration configuration,StatsIdentity statsIdentity)
        {
            if (!configuration.StorageProvider.IsMultiFileStore)
            {
                configuration.StorageProvider.MaxFileSize = 3221225472;//3GB//MiscUtil.MAX_FILE_SIZE;
            }
            return base.Initialize(configuration,statsIdentity);
        }

        public override long CollectionSize(string collection)
        {
            return _fileMetadataDictionary[GetFullDbId(1)].Provider.CollectionSize(collection);
        }

        public override long CollectionDocumentCount(string collection)
        {
            return _fileMetadataDictionary[GetFullDbId(1)].Provider.CollectionDocumentCount(collection);
        }

        public override StorageResult<byte[]> StoreDocument(ITransaction transaction, string collection, long key, byte[] value)
        {
            lock (_oneOppLock)
            {
                PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;
                if (!_collectionMetadataDictionary.ContainsKey(collection))
                    throw new ArgumentException("Specified collection '" + collection + "' does not exist.");

                KeyMetadata keyMetadata = new KeyMetadata(key, GenerateFileId(pmTransaction.MetadataTransaction));
                string dbId = GetFullDbId(keyMetadata.FileId);
                FileMetadata<long, byte[]> fileMetadata = _fileMetadataDictionary[dbId];

                if (fileMetadata == null)
                    throw new Exception("unable to find database file.");

                if (!pmTransaction.DataTransaction.IsTransactionBegin(dbId))
                    pmTransaction.DataTransaction.Begin(dbId, fileMetadata.Provider.BeginTransaction(null, false));

                StorageResult<byte[]> result = fileMetadata.Provider.StoreDocument(pmTransaction.DataTransaction.GetTransaction(dbId), collection,
                    keyMetadata.RowId,
                    value);

                result.FileId = keyMetadata.FileId;
                DisplaySize();
                return result;
            }
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
                    throw new Exception("unable to find database file.");

                FileTransaction fileTransaction = pmTransaction.DataTransaction as FileTransaction;
                if (!fileTransaction.IsTransactionBegin(dbId))
                    fileTransaction.Begin(dbId, fileMetadata.Provider.BeginTransaction(null, false));

                StorageResult<byte[]> result = fileMetadata.Provider.UpdateDocument(fileTransaction.GetTransaction(dbId), collection, key,
                    update);

                result.FileId = fileId;
                DisplaySize();
                return result;
            }
        }

        #endregion

        #region StorageManagerBase Members
        public override long GenerateFileId(ITransaction transaction)           //check file size and then generate fileId
        {
            return 1;
        }
        #endregion

        public override void Dispose()
        {
            //additional dispose if needed, then call base dispose.
            base.Dispose();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

    }
}
