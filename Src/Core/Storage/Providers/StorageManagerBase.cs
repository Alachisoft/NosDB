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
using System.Configuration;
using System.IO;
using System.Linq;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public abstract class StorageManagerBase : IFilePersistenceProvider
    {
        //provider for metadata persistence 
        private IPersistenceProvider _metadataPersister;
        protected object _oneOppLock = new Object();
        protected IStatsCollector _statsCollector;
        private int _fileToBeDefragmented = 0;

        //fileId -> file path, provider
        protected IDictionary<string, FileMetadata<long, byte[]>> _fileMetadataDictionary =
            new HashVector<string, FileMetadata<long, Byte[]>>();

        //collectionId -> collection statistics, IndexingInfo  
        protected IDictionary<string, CollectionMetadata> _collectionMetadataDictionary =
            new HashVector<string, CollectionMetadata>();

        protected const char Seperator = '.';
        protected int _dbIndex = 0;
        protected string _dbId = "data";
        protected StorageConfiguration _userConfig;

        #region IPersistenceProvider Members
        public string DbId
        {
            get { return _dbId; }
        }

        public IPersistenceProvider MetadataPersister
        {
            get { return _metadataPersister; }
            set { _metadataPersister = value; }
        }

        public abstract long CollectionSize(string collection);

        public abstract long CollectionDocumentCount(string collection);

        public virtual bool Initialize(StorageConfiguration configuration,StatsIdentity statsIdentity)
        {
            _userConfig = (StorageConfiguration)configuration.Clone();
            if (configuration == null || configuration.StorageProvider == null)
                throw new ConfigurationException("PersistenceProvider configuration not found.");

            _statsCollector = StatsManager.Instance.GetStatsCollector(statsIdentity);

            RegenerateMetaData();

            ITransaction transaction = _metadataPersister.BeginTransaction(null, false);
            CreateNextFile(transaction);
            _metadataPersister.Commit(transaction);

            DisplaySize();

            return true;
        }

        protected void DisplaySize()
        {
            long size = 0;
            foreach (KeyValuePair<string, FileMetadata<long, byte[]>> pair in _fileMetadataDictionary)
                size += pair.Value.Provider.CurrentDataSize;
            if (_statsCollector != null)
                _statsCollector.SetStatsValue(StatisticsType.DatabaseSize, size);
        }

        public bool CreateCollectionInternal(string collection)
        {
            bool singleSuccess = false;
            foreach (var kvp in _fileMetadataDictionary)
            {
                try
                {
                    kvp.Value.Provider.CreateCollection(collection, typeof(long), typeof(byte[]));
                    singleSuccess = true;
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                        LoggerManager.Instance.StorageLogger.Error("Critical Exception while creating collection ", ex); 
                }
            }
            return singleSuccess;
        }

        public bool CreateCollection(string collection)
        {
            if (!CreateCollectionInternal(collection))
                throw new Exception("Unable to create collection: " + collection);

            if (!_collectionMetadataDictionary.ContainsKey(collection))
            { _collectionMetadataDictionary.Add(collection, new CollectionMetadata()); }

            DisplaySize();

            return true;
        }

        public bool DropCollection(string collection)
        {
            bool singleSuccess = false;
            foreach (var kvp in _fileMetadataDictionary)
            {
                try
                {
                    kvp.Value.Provider.DropCollection(collection);
                    singleSuccess = true;
                }
                catch (Exception)
                {
                    //dont do anything, only generate exception if all files fail.
                }
            }

            if (!singleSuccess)
            {
                return false;
            }

            if (_collectionMetadataDictionary.ContainsKey(collection))
            { _collectionMetadataDictionary.Remove(collection); }

            DisplaySize();

            return true;
        }

        public int StartDefragmentation()
        {
            int defragmentationTime = 0;
            if (_fileToBeDefragmented == 0)
            {
                if (_metadataPersister.DefragmentationNeeded())
                    defragmentationTime = _metadataPersister.StartDefragmentation();
            }
            else
            {
                string dbId = GetFullDbId(_fileToBeDefragmented);
                FileMetadata<long, byte[]> fileMetadata = _fileMetadataDictionary[dbId];

                if (fileMetadata == null)
                    throw new Exception("unable to find database file.");

                if (fileMetadata.Provider.DefragmentationNeeded())
                    defragmentationTime = fileMetadata.Provider.StartDefragmentation();
            }
            _fileToBeDefragmented++;
            if (_fileToBeDefragmented > _fileMetadataDictionary.Count)
                _fileToBeDefragmented = 0;

            return defragmentationTime;
        }

        public abstract StorageResult<byte[]> StoreDocument(ITransaction transaction, string collection, long key,
            byte[] value);

        public abstract StorageResult<byte[]> UpdateDocument(ITransaction transaction, long fileId, string collection,
            long key, byte[] update);

        public StorageResult<byte[]> GetDocument(long fileId, string collection, long key)
        {
            if (!_collectionMetadataDictionary.ContainsKey(collection))
                throw new ArgumentException("Specified collection '" + collection + "' does not exist.");

            if (fileId <= 0)
            {
                var result = new StorageResult<byte[]>();
                result.Status = StoreResult.FailureInvalidFileName;
                return result;
            }
            string dbId = GetFullDbId(fileId);
            FileMetadata<long, byte[]> fileMetadata;
            _fileMetadataDictionary.TryGetValue(dbId, out fileMetadata);
            if (fileMetadata == null)
                throw new Exception("Unable to find database file. Searching for file: " + dbId);

            return fileMetadata.Provider.GetDocument<long, byte[]>(collection, key);
        }

        public StorageResult<byte[]> DeleteDocument(ITransaction transaction, long fileId, string collection, long key)
        {
            lock (_oneOppLock)
            {
                PersistenceManagerTransaction pmTransaction = transaction as PersistenceManagerTransaction;

                if (!_collectionMetadataDictionary.ContainsKey(collection))
                    throw new ArgumentException("Specified collection '" + collection + "' does not exist.");

                string dbId = GetFullDbId(fileId);
                if (!_fileMetadataDictionary.ContainsKey(dbId))
                {
                    StorageResult<byte[]> newresult = new StorageResult<byte[]>();
                    newresult.Status = StoreResult.SuccessKeyDoesNotExist;
                    return newresult;
                }

                FileMetadata<long, byte[]> fileMetadata = _fileMetadataDictionary[dbId];
                if (fileMetadata == null)
                    throw new Exception("unable to find database file.");
                FileTransaction fileTransaction = pmTransaction.DataTransaction as FileTransaction;
                if (!fileTransaction.IsTransactionBegin(dbId))
                    fileTransaction.Begin(dbId, fileMetadata.Provider.BeginTransaction(null, false));

                StorageResult<byte[]> result = fileMetadata.Provider.DeleteDocument<long, byte[]>(fileTransaction.GetTransaction(dbId),
                    collection, key);
                result.FileId = fileId;
                DisplaySize();

                return result;
            }
        }

        public IDataReader<string, byte[]> GetAllDocuments(ITransaction transaction, string collection)
        //yet to be implemented properly
        {
            //if (!_dbMetadata[DbId].Contains(collection))
            //    throw new ArgumentException("Specified collection does not exist.");
            //KeyMetadataDictionary keyMetadata = new KeyMetadataDictionary(GenerateRowId(key), GenerateFileId());
            //string dbId = _dbId + Seperator + keyMetadata.FileId;
            //FileMetadataDictionary fileMetadata = _fileMetadata[dbId];
            //if (fileMetadata == null)
            //    throw new Exception("unable to find database file.");
            //IDataReader<long, Byte[]> result = fileMetadata.Provider.GetAllDocuments(transaction, collection);
            //_keyMetadata.Remove(GetFullKey(collection,key));
            //_keyMetadata.Add(GetFullKey(collection,key), keyMetadata);
            //_keyMetadata.
            throw new NotImplementedException();
        }
        #endregion

        #region ITransactable Members
        public FileTransaction BeginTransaction(ITransaction parentTransaction, bool isReadOnly)
        {
            return new FileTransaction(_fileMetadataDictionary.Keys, isReadOnly);
        }

        public void Commit(ITransaction transaction)
        {
            FileTransaction fileTransaction = transaction as FileTransaction;
            string[] keysArr = _fileMetadataDictionary.Keys.ToArray();
            foreach (var key in keysArr)
            {
                if (fileTransaction.IsTransactionBegin(key))
                {
                    _fileMetadataDictionary[key].Provider.Commit(fileTransaction.GetTransaction(key));
                }
            }
            DisplaySize();
        }

        public void Rollback(ITransaction transaction)
        {
            FileTransaction fileTransaction = transaction as FileTransaction;
            string[] keysArr = _fileMetadataDictionary.Keys.ToArray();
            foreach (var key in keysArr)
            {
                if (fileTransaction.IsTransactionBegin(key))
                {
                    _fileMetadataDictionary[key].Provider.Rollback(fileTransaction.GetTransaction(key));
                }
            }
        }
        #endregion

        #region StorageManagerBase Members
        public long GenerateRowId(string collection)
        {
            if (_collectionMetadataDictionary.ContainsKey(collection))
            {
                long rowId = _collectionMetadataDictionary[collection].GetNextRowId();
                return rowId;
            }
            throw new Exception("Collection Metadata does not contain the specified collection.");
        }

        public abstract long GenerateFileId(ITransaction transaction); //check file size and then generate fileId

        public void RegenerateMetaData()
        {
            RegenerateFileMetadata();
        }
        #endregion

        protected string GetFullDbId(long fileId)
        {
            return _dbId + Seperator + fileId;
        }
        protected string GetFullMetadataDbId()
        {
            return "metadata";
        }
        protected string GetFullKey(string collection, long key)
        {
            return collection + Seperator + key;
        }
        protected string GetFullKey(string collection, string key)
        {
            return collection + Seperator + key;
        }

        protected void CreateNextFile(ITransaction metadataTransaction)
        {
            StorageConfiguration cloneConfig = (StorageConfiguration)_userConfig.Clone();
            cloneConfig.StorageProvider.DatabasePath = cloneConfig.StorageProvider.DatabasePath  + _userConfig.StorageProvider.DatabaseId + "\\";
            bool createFile = false;
            if (!_fileMetadataDictionary.ContainsKey(GetFullDbId(_dbIndex)))
            {
                createFile = true;
            }
            else
            {
                //if provider does exist i.e. already loaded probably due to regenration of metadata
                // then check for if it is full or expansion possible
                if (_fileMetadataDictionary[GetFullDbId(_dbIndex)].Provider.IsDatabaseFull())
                    createFile = true;
            }
            if (createFile)
            {
                _dbIndex++;
                cloneConfig.StorageProvider.DatabaseId = GetFullDbId(_dbIndex);

                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                    LoggerManager.Instance.StorageLogger.Info("StorageProvider.CreateNextFile", "Creating File " + cloneConfig.StorageProvider.DatabasePath + cloneConfig.StorageProvider.DatabaseId);

                FileMetadata<long, byte[]> fMetadata = new FileMetadata<long, byte[]>(ProviderFactory.CreateProvider(cloneConfig.StorageProvider.StorageProviderType), cloneConfig);

                PersistFileMetadata(cloneConfig.StorageProvider.DatabaseId, fMetadata, metadataTransaction);
            }

        }

        #region Persist File Metadata
        protected void PersistFileMetadata(string databaseId, FileMetadata<long, byte[]> fileMetadata, ITransaction transaction)
        {
            if (fileMetadata == null) //if null then remove
            {
                //inMemory StoreKeyMetadata (needs to be done first)
                _fileMetadataDictionary.Remove(databaseId);
                //dbPersist
                _metadataPersister.DeleteDocument<string, byte[]>(transaction, "FileMetadata", databaseId);
            }
            else //else update previous
            {
                //inMemory StoreKeyMetadata
                _fileMetadataDictionary[databaseId] = fileMetadata;
                //dbPersist
                _metadataPersister.StoreDocument(transaction, "FileMetadata", databaseId, CompactBinaryFormatter.ToByteBuffer(fileMetadata, ""));
            }
        }

        public ITransaction BeginMetadataTransaction(bool isReadOnly)
        {
            return _metadataPersister.BeginTransaction(null, isReadOnly);
        }

        public void CommitMetadataTransaction(ITransaction metadataTransaction)
        {
            _metadataPersister.Commit(metadataTransaction);
        }

        public void RollbackMetadataTransaction(ITransaction metadataTransaction)
        {
            _metadataPersister.Rollback(metadataTransaction.InnerObject as ITransaction);
        }
        #endregion

        #region ReGenerate File Metadata
        protected void RegenerateFileMetadata()
        {
            //_metadataPersister.CreateCollection("FileMetadata");
            //ITransaction regenTransaction = _metadataPersister.BeginTransaction(null, false);
            //ITransaction iterationTransaction = _MetadataPersistner.Provider.BeginTransaction(null, true);
            IDataReader<string, byte[]> dataReader = _metadataPersister.GetAllDocuments<string, byte[]>("FileMetadata");

            while (dataReader.MoveNext())
            {
                KeyValuePair<string, byte[]> kvp = dataReader.Current();
                //inMemory StoreKeyMetadata
                FileMetadata<long, byte[]> fileMetadata =
                    (FileMetadata<long, byte[]>)CompactBinaryFormatter.FromByteBuffer(kvp.Value, "");
                fileMetadata.Provider = ProviderFactory.CreateProvider(fileMetadata.ProviderType);
                StorageConfiguration clone = (StorageConfiguration)_userConfig.Clone();
                clone.StorageProvider.DatabaseId = fileMetadata.DatabaseId;
                clone.StorageProvider.DatabasePath =
                    _userConfig.StorageProvider.DatabasePath + _userConfig.StorageProvider.DatabaseId;
                clone.StorageProvider.DatabasePath += "\\"; 
                //clone.StorageProvider.DatabasePath = fileMetadata.FilePath;
                fileMetadata.Provider.Initialize(clone);
                _dbIndex++;
                _fileMetadataDictionary[kvp.Key] = fileMetadata;
            }
            dataReader.Dispose();
            //_metadataPersister.Commit(regenTransaction);
        }
        #endregion

        public virtual void Dispose()
        {
            _metadataPersister.Dispose();
            lock (_fileMetadataDictionary)
            {
                foreach (KeyValuePair<string,FileMetadata<long, byte[]>> fMetadata in _fileMetadataDictionary)
                {
                    fMetadata.Value.Provider.Dispose();
                }
            }
            _collectionMetadataDictionary = null;
            _fileMetadataDictionary = null;
            _dbId = null;
        }

        public virtual void Destroy()
        {
            lock (_fileMetadataDictionary)
            {
                foreach (KeyValuePair<string, FileMetadata<long, byte[]>> fMetadata in _fileMetadataDictionary)
                {
                    fMetadata.Value.Provider.Destroy();
                }
            }
            _collectionMetadataDictionary = null;
            _fileMetadataDictionary = null;
            _dbId = null;
            MetadataPersister.Destroy();
          //  Directory.Delete(_userConfig.StorageProvider.DatabasePath + _userConfig.StorageProvider.DatabaseId, false);
            //File.Delete(_userConfig.StorageProvider.DatabasePath + _userConfig.StorageProvider.DatabaseId);
        }
    }
}
