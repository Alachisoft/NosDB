using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Exceptions;
using System;
using System.Diagnostics;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Serialization.IO;
using LightningDB;
using LightningDB.Converters;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Statistics;
using Alachisoft.NosDB.Common.Logger;
using System.Runtime.ExceptionServices;
using System.Security;

namespace Alachisoft.NosDB.Core.Storage.Providers.LMDB
{
    public class LMDBPersistenceProvider : IPersistenceProvider
    {
        private LightningEnvironment _environment;
        private StorageConfiguration _configuration;
        private Dictionary<string, LMDBCollection> _collectionTable = new Dictionary<string, LMDBCollection>();
        private const string METADATA_COLLECTION = "internalMetadataCollection";

        private readonly object _providerFileSizeLock = new object();
        private long _providerFileSize;
        private bool _isWriterTaken;
        private readonly object _writerLock = new object();
        private List<ITransaction> _writerTransactionsList = new List<ITransaction>(); //should not contain more than 1 element at a time 
        private readonly Object _readerTransactionLock = new Object();
        private ReadTransaction _readerTransaction = null;

        #region Converters
        //can be made public if LMDB provider is to be used by Generic types (non-native) types
        //currently we are only giving integerbased keys and byte[] so the methods are not in use.
        private void RegisterCoverterToBytes<T>(IConvertToBytes<T> converter)
        {
            _environment.ConverterStore.AddConvertToBytes(converter);
        }

        private void RegisterConverterFromBytes<T>(IConvertFromBytes<T> converter)
        {
            _environment.ConverterStore.AddConvertFromBytes(converter);
        }
        #endregion

        #region Statistics
        public long CollectionSize(string collection)
        {
            if (string.IsNullOrEmpty(collection))
            {
                throw new ArgumentException("Collection name can not be null or empty.");
            }
            if (!CollectionExists(collection))
            {
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);
            }
            return _collectionTable[collection].Stats.DataSize;
        }

        public long CollectionDocumentCount(string collection)
        {
            if (string.IsNullOrEmpty(collection))
            {
                throw new ArgumentException("Collection name can not be null or empty.");
            }
            if (!CollectionExists(collection))
            {
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);
            }
            return _collectionTable[collection].Stats.DocumentCount;
        }

        public void ChangeDataSize(long change)
        {
            lock (_providerFileSizeLock)
            {
                _providerFileSize += change;
            }
        }

        private long CaclulateSize(byte[] keyBytes, byte[] dataBytes)
        {
            long totalSize = 0;
            if (keyBytes != null)
                totalSize = keyBytes.Length;
            if (dataBytes != null)
                totalSize = dataBytes.Length;
            return totalSize + 276;                          //+item overhead;
            //return totalSize + (4096 - (totalSize) % 4096);   //Old formula too much overhead
        }

        public long CurrentDataSize
        {
            get
            {
                lock (_providerFileSizeLock)
                {
                    return _providerFileSize;
                }
            }
        }

        public bool IsDatabaseFull()
        {
            if (CurrentDataSize > LMDBConfiguration.MAX_DATA_THREASHOLD * _configuration.StorageProvider.MaxFileSize)
                return true;
            return false;
        }
        #endregion

        #region IPersistenceProvider Members

        public bool Initialize(StorageConfiguration configuration)
        {
            if (configuration.StorageProvider.LMDBProvider == null)
                throw new ConfigurationException("LMDBPersistenceProvider configuration not found.");

            _configuration = configuration;
            _configuration.StorageProvider.LMDBProvider.MaxCollections += 1; //for internalmetadacollection

            if (string.IsNullOrEmpty(configuration.StorageProvider.DatabaseId))
                throw new ArgumentException("Database name can not be empty or null.");

            if (string.IsNullOrEmpty(configuration.StorageProvider.DatabasePath))
                throw new ArgumentException("Database path can not be empty or null.");

            if (!Directory.Exists(configuration.StorageProvider.DatabasePath))
                Directory.CreateDirectory(configuration.StorageProvider.DatabasePath);

            if (_environment == null || !_environment.IsOpened)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.StorageLogger.Info("LMDB.Initialize", "Initializing LMDB Lightning Environment. " + GetFileInfo());
                }
                EnvironmentConfiguration envConf = new EnvironmentConfiguration();
                envConf.AutoReduceMapSizeIn32BitProcess = true;
                envConf.MapSize = _configuration.StorageProvider.MaxFileSize;
                envConf.MaxDatabases = _configuration.StorageProvider.LMDBProvider.MaxCollections;
                envConf.MaxReaders = _configuration.StorageProvider.LMDBProvider.MaxReaders;

                _environment = new LightningEnvironment(configuration.StorageProvider.DatabasePath + configuration.StorageProvider.DatabaseId + LMDBConfiguration.EXTENSION, envConf);
                //RegisterConverterFromBytes(new StringArrayConverter());
                //RegisterCoverterToBytes(new StringArrayConverter());

                EnvironmentOpenFlags convertedFlags;
                if (!Enum.TryParse(_configuration.StorageProvider.LMDBProvider.EnvironmentOpenFlags.ToString(), out convertedFlags))
                {
                    throw new Exception("LMDB.Initialize. Environment Flags conversion failure." + GetFileInfo());
                }
                _environment.Open(convertedFlags);
            }
            GetPreviouslyStoredFileSize();
            //Major Bug Fix - transaction already commited exception. 
            StoreFileSize();
            InitializePerviouslyStoredCollections();
            StoreCollectionInfo(null);
            return true;

        }

        //Function NOT TO BE USED by someone else.
        private void StoreFileSize()
        {
            ITransaction transaction = BeginTransactionInternal(null, TransactionBeginFlags.None);
            StoreDocument(transaction, METADATA_COLLECTION, "fileSize", _providerFileSize);
            CommitInternal(transaction);
        }

        private void GetPreviouslyStoredFileSize()
        {
            //needed to ensure that internalMetadataCollection exists so that argument exception is not thrown by LMDB.
            CreateCollection(METADATA_COLLECTION, typeof(string), typeof(byte[]));

            StorageResult<long> fileSize = GetDocument<string, long>(METADATA_COLLECTION, "fileSize");
            ChangeDataSize(fileSize.Document == 0 ? _environment.UsedSize : fileSize.Document);
        }

        private void InitializePerviouslyStoredCollections()
        {
            //needed to ensure that internalMetadataCollection exists so that argument exception is not thrown by LMDB.
            _collectionTable[METADATA_COLLECTION] = new LMDBCollection(CreateCollectionInternal(METADATA_COLLECTION, typeof(string)));
            //ITransaction transaction = BeginTransaction(null, true);
            StorageResult<byte[]> result = GetDocument<string, byte[]>(METADATA_COLLECTION, "collections");
            if (result.Document != null)
            {
                Dictionary<string, LMDBCollection> collectionList;
                using (ClusteredMemoryStream ms = new ClusteredMemoryStream(result.Document))
                {
                    using (CompactReader reader = new CompactBinaryReader(ms))
                    {
                        collectionList = SerializationUtility.DeserializeDictionary<string, LMDBCollection>(reader);
                    }
                }
                foreach (var col in collectionList)
                {
                    CreateCollection(col.Key, null, null);
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Info("LMDB.Initialize", "Previously stored collection :" + col.Key + " initialized successfully. ");
                    }
                    _collectionTable[col.Key].Stats.DataSize = col.Value.Stats.DataSize;
                    _collectionTable[col.Key].Stats.DocumentCount = col.Value.Stats.DocumentCount;
                }
            }
        }

        private bool CollectionExists(string collection)
        {
            return _collectionTable.ContainsKey(collection);
        }

        /// <summary>
        /// Creates new collection in lmdb file.
        /// </summary>
        /// <param name="collection">Name of the collection.</param>
        /// <param name="keyType">Parameter Ignored in case of LMDB</param>
        /// <param name="valueType">Parameter Ignored in case of LMDB</param>
        /// <returns></returns>
        public bool CreateCollection(string collection, Type keyType, Type valueType)
        {
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException("Collection name cant not be null or empty.");

            if (CollectionExists(collection))
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.StorageLogger.Warn("LMDBProvider.CreateCollection()", GetFileInfo() + " Collection = " + collection + " already exist. ");
                }
                return false;
            }
            _collectionTable.Add(collection, new LMDBCollection(CreateCollectionInternal(collection, keyType)));
            return true;
        }

        private LightningDatabase CreateCollectionInternal(string name, Type keyType)
        {
            ITransaction transaction = BeginTransactionInternal(null, TransactionBeginFlags.None);
            ValidateTransaction(transaction);

            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;
            LightningDB.DatabaseConfiguration dbOptions = new LightningDB.DatabaseConfiguration();
            dbOptions.Flags = DatabaseOpenFlags.Create;
            //todo: maybe add other integer types too
            if (keyType == typeof(long) || keyType == typeof(ulong) || keyType == typeof(int) || keyType == typeof(uint) || keyType == typeof(short) || keyType == typeof(ushort))
                dbOptions.Flags |= DatabaseOpenFlags.IntegerKey;
            LightningDatabase collectionInstance = lmdbTransaction.OpenDatabase(name, dbOptions);

            if (name != METADATA_COLLECTION)
            {
                StoreCollectionInfo(name, transaction);
            }
            ((LMDBTransaction)transaction).ChangeSize(8192);
            CommitInternal(transaction);

            return collectionInstance;
        }

        /// <summary>
        /// Stores the collection info to Disk, If collection is Null then stores the current Collection created
        /// </summary>
        /// <param name="collection">Store This collection along with current collections</param>
        private void StoreCollectionInfo(string collection)
        {
            ITransaction transaction = BeginTransactionInternal(null, TransactionBeginFlags.None);
            StoreCollectionInfo(collection, transaction);
            CommitInternal(transaction);
        }

        /// <summary>
        /// Stores the collection info to Disk, If collection is Null then stores the current Collection created
        /// </summary>
        /// <param name="collection">Store This collection along with current collections</param>
        /// <param name="transaction">provide a writer transaction to store data</param>
        private void StoreCollectionInfo(string collection, ITransaction transaction)
        {
            Dictionary<string, LMDBCollection> tobeStored = new Dictionary<string, LMDBCollection>();
            foreach (KeyValuePair<string, LMDBCollection> lmdbCollection in _collectionTable)
            {
                tobeStored.Add(lmdbCollection.Key, (LMDBCollection)lmdbCollection.Value.Clone());
            }
            if (!string.IsNullOrEmpty(collection) && !tobeStored.ContainsKey(collection))
                tobeStored.Add(collection, new LMDBCollection(null));
            using (ClusteredMemoryStream stream = new ClusteredMemoryStream())
            {
                using (CompactWriter writer = new CompactBinaryWriter(stream))
                {
                    SerializationUtility.SerializeDictionary<string, LMDBCollection>(tobeStored, writer);
                    UpdateDocumentInternal(transaction, METADATA_COLLECTION, "collections", stream.ToArray());
                }
            }
        }

        public void DropCollection(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Collection name cant not be null or empty.");

            if (!CollectionExists(name))
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + name);

            ITransaction transaction = BeginTransactionInternal(null, TransactionBeginFlags.None);
            ValidateTransaction(transaction);
            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;

            LightningDatabase dbInstance = _collectionTable[name].Collection;
            long dataSize = _collectionTable[name].Stats.DataSize;
            lmdbTransaction.DropDatabase(dbInstance);
            _collectionTable.Remove(name);

            StoreCollectionInfo(null, transaction);
            //StoreDocument(transaction, _metadataCollection, "collections", _collectionTable.Keys.ToArray());

            ((LMDBTransaction)transaction).ChangeSize(-8192);
            ((LMDBTransaction)transaction).ChangeSize(-dataSize);

            CommitInternal(transaction);

        }

        public StorageResult<TValue> StoreDocument<TKey, TValue>(ITransaction transaction, string collection, TKey key, TValue value)
        {
            StorageResult<TValue> result = new StorageResult<TValue>();
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException("Collection name can not be null or empty.");

            if (!CollectionExists(collection))
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);

            if (value == null)
                throw new ArgumentException("Value can not be null.");

            if (IsDatabaseFull())
            {
                result.Status = StoreResult.FailureDatabaseFull;
                return result;
            }
            return StoreDocumentInternal(transaction, collection, key, value);
        }

        private StorageResult<TValue> StoreDocumentInternal<TKey, TValue>(ITransaction transaction, string collection, TKey key, TValue value)
        {
            StorageResult<TValue> result = new StorageResult<TValue>();
            ValidateTransaction(transaction);
            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;

            byte[] keyBytes = _environment.ConverterStore.GetToBytes<TKey>().Convert(_collectionTable[collection].Collection, key);
            byte[] valueBytes = _environment.ConverterStore.GetToBytes<TValue>().Convert(_collectionTable[collection].Collection, value);

            try
            {
                lmdbTransaction.Put(_collectionTable[collection].Collection, keyBytes, valueBytes);
                long size = CaclulateSize(keyBytes, valueBytes);
                ((LMDBTransaction)transaction).ChangeSize(size);
                _collectionTable[collection].IncrementTemporaryStats(size);

                result.Document = value;
                result.Status = StoreResult.Success;
                return result;
            }
            catch (LightningException le)
            {
                result.Status = HandleException(le);
                return result;
            }
        }

        public StorageResult<TValue> UpdateDocument<TKey, TValue>(ITransaction transaction, string collection, TKey key, TValue update)
        {
            StorageResult<TValue> result = new StorageResult<TValue>();
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException("Collection name can not be null or empty.");

            if (!CollectionExists(collection))
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);

            if (update == null)
                throw new ArgumentException("Value can not be null.");

            if (IsDatabaseFull())
            {
                result.Status = StoreResult.FailureDatabaseFull;
                return result;
            }
            return UpdateDocumentInternal(transaction, collection, key, update);
        }

        private StorageResult<TValue> UpdateDocumentInternal<TKey, TValue>(ITransaction transaction, string collection, TKey key, TValue update)
        {
            StorageResult<TValue> result = new StorageResult<TValue>();
            ValidateTransaction(transaction);
            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;

            byte[] keyBytes = _environment.ConverterStore.GetToBytes<TKey>().Convert(_collectionTable[collection].Collection, key);
            byte[] oldValBytes = lmdbTransaction.Get(_collectionTable[collection].Collection, keyBytes);

            long netSize = 0;
            netSize = -CaclulateSize(keyBytes, oldValBytes);

            byte[] valueBytes = _environment.ConverterStore.GetToBytes<TValue>().Convert(_collectionTable[collection].Collection, update);
            try
            {
                lmdbTransaction.Put(_collectionTable[collection].Collection, keyBytes, valueBytes);
                //Incase of update previous size is not being subtracted at the moment
                long size = CaclulateSize(keyBytes, valueBytes);
                netSize += size;
                ((LMDBTransaction)transaction).ChangeSize(netSize);
                _collectionTable[collection].IncrementTemporaryStats(netSize);
                result.Document = update;
                result.Status = StoreResult.SuccessOverwrite;
                return result;
            }
            catch (LightningException le)
            {
                result.Status = HandleException(le);
                return result;
            }
        }

        public StorageResult<TValue> GetDocument<TKey, TValue>(string collection, TKey key)
        {
            StorageResult<TValue> result = new StorageResult<TValue>();
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException("Collection name can not be null or empty.");

            if (!CollectionExists(collection))
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);
            ReadTransaction transaction = null;
            try
            {
                if (_readerTransaction != null)
                {
                    if (_readerTransaction.ShouldRenew)
                        _readerTransaction.WaitUntillFree();
                }

                lock (_readerTransactionLock)
                {
                    if (_readerTransaction == null || !_readerTransaction.Running)
                        _readerTransaction = new ReadTransaction(BeginTransaction(null, true));

                    transaction = _readerTransaction;
                    transaction.Enter();
                }

                ValidateTransaction(transaction.Transaction);
                LightningTransaction lmdbTransaction = (LightningTransaction)transaction.Transaction.InnerObject;

                byte[] keyBytes = _environment.ConverterStore.GetToBytes<TKey>().Convert(_collectionTable[collection].Collection, key);
                byte[] valueBytes = lmdbTransaction.Get(_collectionTable[collection].Collection, keyBytes);
                result.Document = _environment.ConverterStore.GetFromBytes<TValue>().Convert(_collectionTable[collection].Collection, valueBytes);
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Exit();
                }
            }

            result.Status = StoreResult.Success;
            return result;
        }

        public StorageResult<TValue> DeleteDocument<TKey, TValue>(ITransaction transaction, string collection, TKey key)
        {
            StorageResult<TValue> result = new StorageResult<TValue>();
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException("Collection name can not be null or empty.");

            if (!CollectionExists(collection))
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);

            ValidateTransaction(transaction);
            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;

            byte[] keyBytes = _environment.ConverterStore.GetToBytes<TKey>().Convert(_collectionTable[collection].Collection, key);
            byte[] valueBytes = lmdbTransaction.Get(_collectionTable[collection].Collection, keyBytes);

            long size = CaclulateSize(keyBytes, valueBytes);
            result.Document = _environment.ConverterStore.GetFromBytes<TValue>().Convert(_collectionTable[collection].Collection, valueBytes);
            try
            {
                lmdbTransaction.Delete(_collectionTable[collection].Collection, keyBytes);
                ((LMDBTransaction)transaction).ChangeSize(-size);
                _collectionTable[collection].DecrementTemporaryStats(size);

                result.Status = StoreResult.Success;
                return result;
            }
            catch (LightningException le)
            {
                //todo temp fix consider success if doc not found
                if (le.StatusCode == LMDBErrorCodes.MDB_NOTFOUND)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Warn("LMDB.DeleteDocument", "Error Deleting Document." + GetFileInfo() + le);
                    }
                    result.Status = StoreResult.Success;
                    return result;
                }
                result.Status = HandleException(le);
                return result;
            }
        }

        public IDataReader<TKey, TValue> GetAllDocuments<TKey, TValue>(string collection)
        {
            if (string.IsNullOrEmpty(collection))
                throw new ArgumentException("Collection name can not be null or empty.");

            if (!CollectionExists(collection))
                throw new ArgumentException("Specified collection not found in " + GetFileInfo() + " Collection = " + collection);

            ReadTransaction transaction = null;
            try
            {
                if (_readerTransaction != null)
                {
                    if (_readerTransaction.ShouldRenew)
                        _readerTransaction.WaitUntillFree();
                }

                lock (_readerTransactionLock)
                {
                    if (_readerTransaction == null || !_readerTransaction.Running)
                        _readerTransaction = new ReadTransaction(BeginTransaction(null, true));

                    transaction = _readerTransaction;
                    transaction.Enter();
                }
                ValidateTransaction(transaction.Transaction);
                LightningTransaction lmdbTransaction = (LightningTransaction)transaction.Transaction.InnerObject;

                LightningCursor cursor = lmdbTransaction.CreateCursor(_collectionTable[collection].Collection);
                LMDBDataReader<TKey, TValue> reader = new LMDBDataReader<TKey, TValue>(cursor.GetEnumerator(), _environment, _collectionTable[collection].Collection);
                return reader;
            }
            finally
            {
                if (transaction != null) transaction.Exit();
            }
        }
        #endregion

        #region ITransactable Members
        public ITransaction BeginTransaction(ITransaction parentTransaction, bool isReadOnly)
        {
            ITransaction resultTransaction = null;
            TransactionBeginFlags flags; //= isReadOnly ? TransactionBeginFlags.ReadOnly : TransactionBeginFlags.None;

            try
            {
                if (isReadOnly)
                {
                    flags = TransactionBeginFlags.ReadOnly;
                }
                else
                {
                    flags = TransactionBeginFlags.None;
                    if (_isWriterTaken)
                    {
                        lock (_writerLock)
                        {
                            while (_isWriterTaken)
                            {
                                Monitor.Wait(_writerLock);
                            }
                        }
                    }
                    _isWriterTaken = true;
                }

                resultTransaction = BeginTransactionInternal(parentTransaction, flags);

                if (!isReadOnly)
                {
                    lock (_writerTransactionsList)
                    {
                        _writerTransactionsList.Add(resultTransaction);
                    }
                }
                return resultTransaction;
            }
            catch (Exception)
            {
                _isWriterTaken = false;
                throw;
            }
        }

        private ITransaction BeginTransactionInternal(ITransaction parentTransaction, TransactionBeginFlags flags)
        {
            if (parentTransaction == null || parentTransaction.InnerObject == null)
            {
                return new LMDBTransaction(_environment.BeginTransaction(flags), flags == TransactionBeginFlags.ReadOnly ? true : false);
            }
            else
            {
                ValidateTransaction(parentTransaction);
                LightningTransaction lmdbTransaction = (LightningTransaction)parentTransaction.InnerObject;
                return new LMDBTransaction(lmdbTransaction.BeginTransaction(flags), flags == TransactionBeginFlags.ReadOnly ? true : false);
            }
        }

        public void Commit(ITransaction transaction)
        {
            try
            {
                if (!transaction.IsReadOnly)
                {
                    foreach (LMDBCollection collection in _collectionTable.Values)
                    {
                        collection.UpdateTemporaryStats();
                    }
                    StorageResult<long> result = UpdateDocumentInternal(transaction, METADATA_COLLECTION, "fileSize", _providerFileSize + ((LMDBTransaction)transaction).Size);
                    if (result.Status != StoreResult.Success && result.Status != StoreResult.SuccessDelete && result.Status != StoreResult.SuccessKeyDoesNotExist && result.Status != StoreResult.SuccessOverwrite)
                        throw new Exception("Error commiting transaction." + GetFileInfo());
                    StoreCollectionInfo(null, transaction);         //if something happens above check might be needed here too.
                }
                CommitInternal(transaction);
                RemoveTransactionFromLists(transaction);
            }
            catch (TransactionComittedException)
            {
                RemoveTransactionFromLists(transaction);
                //if transaction was already comitted and again comit call was sent then no need to throw exception.
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                    LoggerManager.Instance.StorageLogger.Warn("LMDB.Commit", " Transaction committed exception. " + GetFileInfo());
            }
        }

        private void CommitInternal(ITransaction transaction)
        {
            Stopwatch sw = new Stopwatch();
            ValidateTransaction(transaction);
            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;
            sw.Start();
            lmdbTransaction.Commit();
            sw.Stop();

            ChangeDataSize(((LMDBTransaction)transaction).Size);

            if (!transaction.IsReadOnly)
            {
                if (_readerTransaction != null)
                {
                    lock (_readerTransactionLock)
                    {
                        if (_readerTransaction != null)
                        {
                            _readerTransaction.MarkForCommit();
                            //_readerTransaction = null;
                        }
                    }
                }
            }

            if (LoggerManager.Instance.StorageLogger != null) { LoggerManager.Instance.StorageLogger.Debug("LMDBPersistanceProvider", "LMDB Commit Time: " + sw.ElapsedMilliseconds + " (ms)"); }
        }

        public void Rollback(ITransaction transaction)
        {
            try
            {
                foreach (LMDBCollection collection in _collectionTable.Values)
                {
                    collection.ResetTemporaryStats();
                }
                ValidateTransaction(transaction);
                LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;
                lmdbTransaction.Abort();
            }
            catch (TransactionComittedException)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                    LoggerManager.Instance.StorageLogger.Warn("LMDB.Rollback", " Transaction is comitted already. " + GetFileInfo());
            }
            catch (TransactionAobrtException)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                    LoggerManager.Instance.StorageLogger.Warn("LMDB.Rollback", " Transaction is aborted already. " + GetFileInfo());
            }
            finally
            {
                if (_readerTransaction != null)
                {
                    lock (_readerTransactionLock)
                    {
                        if (_readerTransaction != null)
                        {
                            _readerTransaction.MarkForRollback();
                            //_readerTransaction = null;
                        }
                    }
                }
                RemoveTransactionFromLists(transaction);
            }
        }

        private void RemoveTransactionFromLists(ITransaction transaction)
        {
            if (!transaction.IsReadOnly)
            {
                lock (_writerTransactionsList)
                {
                    _writerTransactionsList.Remove(transaction);
                }
                lock (_writerLock)
                {
                    _isWriterTaken = false;
                    Monitor.PulseAll(_writerLock);
                }
            }
        }
        #endregion

        private void ValidateTransaction(ITransaction transaction)
        {
            if (transaction == null)
                throw new TransactionException("Invalid Transaction.Transaction can not be null.");

            if (transaction.InnerObject == null)
                throw new TransactionException("Invalid Transaction. Inner Transaction can not be null.");

            if (transaction.InnerObject.GetType() != typeof(LightningTransaction))
                throw new TransactionException("Invalid Transaction. Inner Transaction type not valid. Expected type = " + typeof(LightningTransaction) + " Type present = " + transaction.InnerObject.GetType());

            LightningTransaction lmdbTransaction = (LightningTransaction)transaction.InnerObject;
            if (lmdbTransaction.State == LightningTransactionState.Aborted)
                throw new TransactionAobrtException("Invalid Transaction. Inner Transaction was aborted. File = " + GetFileInfo());

            if (lmdbTransaction.State == LightningTransactionState.Commited)
                throw new TransactionComittedException("Invalid Transaction. Inner Transaction was commited. File = " + GetFileInfo());
        }

        private StoreResult HandleException(LightningException ex)
        {
            switch (ex.StatusCode)
            {
                case LMDBErrorCodes.MDB_BAD_DBI:
                    throw ex;
                case LMDBErrorCodes.MDB_BAD_RSLOT:
                    throw ex;
                case LMDBErrorCodes.MDB_BAD_TXN:
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Warn("LMDB.BAD_TXN", "MDB transaction must abort." + GetFileInfo());
                    }
                    return StoreResult.FailureReOpenTransaction;
                case LMDBErrorCodes.MDB_BAD_VALSIZE:
                    throw ex;
                case LMDBErrorCodes.MDB_CORRUPTED:
                    throw ex;
                case LMDBErrorCodes.MDB_CURSOR_FULL:
                    throw ex;
                case LMDBErrorCodes.MDB_DBS_FULL:
                    throw ex;
                case LMDBErrorCodes.MDB_INCOMPATIBLE:
                    throw ex;
                case LMDBErrorCodes.MDB_INVALID:
                    throw ex;
                case LMDBErrorCodes.MDB_KEYEXIST:
                    throw ex;
                case LMDBErrorCodes.MDB_MAP_FULL:
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Info("LMDB.MAP_FULL", "MDB map full occured on" + GetFileInfo());
                    }
                    long size = _configuration.StorageProvider.MaxFileSize - CurrentDataSize;
                    ChangeDataSize(size);
                    throw ex;
                case LMDBErrorCodes.MDB_MAP_RESIZED:
                    throw ex;
                case LMDBErrorCodes.MDB_NOTFOUND:
                    throw ex;
                case LMDBErrorCodes.MDB_PAGE_FULL:
                    throw ex;
                case LMDBErrorCodes.MDB_PAGE_NOTFOUND:
                    throw ex;
                case LMDBErrorCodes.MDB_PANIC:
                    throw ex;
                case LMDBErrorCodes.MDB_READERS_FULL:
                    throw ex;
                case LMDBErrorCodes.MDB_SUCCESS:
                    throw ex;
                case LMDBErrorCodes.MDB_TLS_FULL:
                    throw ex;
                case LMDBErrorCodes.MDB_TXN_FULL:
                    throw ex;
                case LMDBErrorCodes.MDB_VERSION_MISMATCH:
                    throw ex;
                default:
                    throw ex;
            }
        }

        private string GetFileInfo()
        {
            if (_configuration != null)
                if (_configuration.StorageProvider != null)
                    return "Database = " + _configuration.StorageProvider.DatabasePath + _configuration.StorageProvider.DatabaseId;
            return "";
        }

        public void Dispose()
        {
            if (_collectionTable != null)
            {
                foreach (KeyValuePair<string, LMDBCollection> entry in _collectionTable)
                {
                    if (entry.Value != null && entry.Value.Collection.IsOpened)
                        entry.Value.Collection.Dispose();
                }
                _collectionTable = null;
            }

            if (_environment != null && _environment.IsOpened)
                _environment.Dispose();
            _environment = null;
        }

        public void Destroy()
        {
            foreach (KeyValuePair<string, LMDBCollection> entry in _collectionTable)
            {
                if (entry.Value != null && entry.Value.Collection.IsOpened)
                    entry.Value.Collection.Dispose();
            }
            _collectionTable = null;

            if (_environment != null && _environment.IsOpened)
                _environment.Dispose();

            File.Delete(_configuration.StorageProvider.DatabasePath + _configuration.StorageProvider.DatabaseId + LMDBConfiguration.EXTENSION);
            File.Delete(_configuration.StorageProvider.DatabasePath + _configuration.StorageProvider.DatabaseId + LMDBConfiguration.EXTENSION + "-lock");
            _environment = null;
        }

        public int StartDefragmentation()
        {
            throw new NotImplementedException();
        }

        public bool DefragmentationNeeded()
        {
            throw new NotImplementedException();
        }

        public class LMDBTransaction : ITransaction
        {
            private LightningTransaction _transaction;
            private bool _isReadOnly;
            private long _size = 0;

            public long Size
            {
                get
                {
                    lock (this)
                    {
                        return _size;
                    }
                }
            }

            public LMDBTransaction(LightningTransaction transaction, bool isReadOnly)
            {
                if (transaction == null)
                {
                    throw new ArgumentException("Parameter transaction can not be null.");
                }
                _transaction = transaction;
                _isReadOnly = isReadOnly;
            }

            public void ChangeSize(long change)
            {
                lock (this)
                {
                    _size += change;
                }
            }

            #region ITransaction methods
            public ITransaction ParentTransaction
            {
                get { throw new NotImplementedException(); }
            }

            public ITransactable Initiator
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public long Id
            {
                get { throw new NotImplementedException(); }
            }

            public object InnerObject
            {
                get { return _transaction; }
            }

            public bool IsReadOnly
            {
                get { return _isReadOnly; }
            }

            public void Dispose()
            {
                _transaction.Dispose();
            }
            #endregion
        }

    }

    class ReadTransaction
    {
        private bool _running;
        private int _refCount;
        private bool _shouldCommit;
        private bool _shouldRollback;

        public ReadTransaction(ITransaction transaction)
        {
            this.Transaction = transaction;
            _running = true;
        }

        public ITransaction Transaction { get; set; }

        public bool Running { get { return _running; } }

        public bool ShouldRenew { get { return _shouldCommit || _shouldRollback; } }

        public bool Enter()
        {
            lock (this)
            {
                if (ShouldRenew && _refCount == 0)
                    Renew();

                if (_running)
                {
                    _refCount++;

                }
                return _running;
            }
        }

        public void Exit()
        {
            lock (this)
            {
                _refCount--;
                if (ShouldRenew && _refCount == 0)
                    Renew();

                Commit();
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void Commit()
        {
            if (_refCount == 0)
            {
                try
                {
                    //if (_shouldCommit)
                    //    ((LightningTransaction)Transaction.InnerObject).Abort();
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Error("LMDB.CommitReadTransaction", "Error Commiting Read Transaction. " + e);
                    }
                }
                Monitor.PulseAll(this);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void Rollback()
        {
            if (_refCount == 0)
            {
                try
                {
                    //if (_shouldRollback)
                    //    ((LightningTransaction)Transaction.InnerObject).Abort();
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Error("LMDB.RollbackReadTransaction", "Error Rollback Read Transaction. " + e);
                    }
                }
                Monitor.PulseAll(this);
            }
        }

        public void WaitUntillFree()
        {
            lock (this)
            {
                //_running = false;
                if (_refCount > 0)
                    Monitor.Wait(this);
            }
        }

        internal void MarkForCommit()
        {
            _shouldCommit = true;
            lock (this)
            {
                if (ShouldRenew && _refCount == 0)
                {
                    Renew();
                }
                Commit();
            }
        }

        internal void MarkForRollback()
        {
            _shouldRollback = true;
            lock (this)
            {
                if (ShouldRenew && _refCount == 0)
                {
                    Renew();
                }
                Rollback();
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        internal void Renew()
        {
            if (ShouldRenew)
            {
                try
                {
                    ((LightningTransaction)this.Transaction.InnerObject).Reset();
                    ((LightningTransaction)this.Transaction.InnerObject).Renew();
                    _shouldRollback = _shouldCommit = false;
                }
                catch (Exception e)
                {
                    _running = false;
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Error("LMDB.Renew", "Error Renew Read Transaction. " + e);
                    }
                }
            }
        }
    }
}
