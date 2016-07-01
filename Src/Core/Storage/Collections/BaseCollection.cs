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
using System.Configuration;
using System.IO;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Results.Transforms;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Storage.Caching;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Storage.Providers;
using Alachisoft.NosDB.Core.Queries.Filters;
using Alachisoft.NosDB.Core.Queries.Optimizer;
using Alachisoft.NosDB.Core.Queries.Results;
﻿using System;
using System.Threading;
using Alachisoft.NosDB.Core.Toplogies;
using CollectionConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration;
using IndexConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration;
using UpdateOperation = Alachisoft.NosDB.Core.Storage.Operations.UpdateOperation;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using FailedDocument = Alachisoft.NosDB.Common.Server.Engine.FailedDocument;


namespace Alachisoft.NosDB.Core.Storage.Collections
{
    /// <summary>
    /// Responsible for managing indexing , handling collection level operations
    /// key based collection level locking
    /// eventual persistence
    /// document caching
    /// </summary>
    public class BaseCollection : ICollectionStore
    {
        protected DatabaseContext _dbContext;
        protected CollectionConfiguration _configuration;
        protected IQueryOptimizer _queryOptimizer;
        //private ICollectionStore _documentStore;
        protected IDocumentStore _docStore;
        protected MetadataIndex _metadataIndex;
        protected QueryResultManager _queryResultManager;
        protected IStatsCollector _statsCollector;
        protected IStore _databaseStore;
        public IDatabasesManager _dbManager;
        protected NodeContext _context;
        protected Latch _statusLatch;
        protected ThreadManager _threadsEntered;
       
        protected long _lastTemporaryAssignedRowId = -1;

      

        private static long queryId = new Random().Next();

        protected bool _disposed;


        public virtual IDistribution Distribution
        {
            get { return null; }
            set { }
        }


        public virtual IDictionary<int, KeyValuePair<HashMapBucket, BucketStatistics>> BucketStatistics
        {
            get { throw new Exception("BucketStats not available in BaseCollection class."); }
        }

        public string Name
        {
            get { return _configuration.CollectionName; }
        }

        public IDocumentStore DocumentStore
        {
            get { return _docStore; }
        }

        public CollectionConfiguration CollectionConfiguration
        {
            get { return _configuration; }
        }

        public DatabaseContext DbContext
        { get { return _dbContext; } }

        public CollectionIndexManager IndexManager
        { get { return _docStore.IndexManager; } }

        public MetadataIndex MetadataIndex
        {
            get { return _metadataIndex; }
            set { _metadataIndex = value; }
        }

        public virtual string ShardName
        {
            get { throw new Exception("ShardName not available in BaseCollection class."); }
            set { }
        }

        public bool HasDisposed
        {
            get { return _disposed; }
        }

        public Latch Status { get { return _statusLatch; } }

        public BaseCollection(DatabaseContext dbContext, NodeContext nodeContext)
        {
            _dbContext = dbContext;
            _statusLatch = new Latch(CollectionStatus.INITIALIZING);
            _threadsEntered = new ThreadManager();
            bool inMemMetadataIndex = true;
            if (ConfigurationManager.AppSettings["InMemoryMetadataIndex"] != null)
            {
                Boolean.TryParse(ConfigurationManager.AppSettings["InMemoryMetadataIndex"], out inMemMetadataIndex);
            }
            if (inMemMetadataIndex)
                _metadataIndex = new MetadataIndex(dbContext.StatsIdentity, dbContext.StorageManager.MetadataPersister);
            else
                _metadataIndex = new MetadataIndexDisk(dbContext.StatsIdentity, dbContext.StorageManager.MetadataPersister);

            _statsCollector = StatsManager.Instance.GetStatsCollector(_dbContext.StatsIdentity);
            _dbManager = nodeContext.DatabasesManager;
            _docStore = new DocumentCache(this, nodeContext);
            _context = nodeContext;
        }

        public virtual bool Initialize(CollectionConfiguration configuration, QueryResultManager queryResultManager, IStore databaseStore, IDistribution distribution)
        {
            _threadsEntered.IncrementCount();
            try
            {
                _configuration = configuration;
                _databaseStore = databaseStore;
                _queryResultManager = queryResultManager;
                _metadataIndex.Initialize(this);
              
                _dbContext.StorageManager.CreateCollection(_configuration.CollectionName);
                _docStore.Initialize(_dbContext, configuration);

                _queryOptimizer = new CostBasedOptimizer(IndexManager);

                ////Umer Temporarily disabling this
                ////: once thew distribution is set the status will be set to running
                //_statusLatch.SetStatusBit(CollectionStatus.RUNNING, CollectionStatus.INITIALIZING);
                return true;
            }
            catch (Exception exc)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Initialize", "Failed to Initialize collection, " + exc);
                return false;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        public virtual void PopulateData()
        {
            _statusLatch.SetStatusBit(CollectionStatus.RUNNING, CollectionStatus.INITIALIZING);
        }

        public long DocumentCount
        {
            get
            {
                ValidateCollection();
                return _docStore.GetStorageManagerBase.CollectionDocumentCount(Name);
            }
        }

        public int RowToFileIndexCount
        {
            get { return _metadataIndex.RowToFileIndexCount; }
        }

        public long Size
        {
            get
            {
                ValidateCollection();
                return _docStore.GetStorageManagerBase.CollectionSize(Name);
            }
        }

        public long GetRowId(DocumentKey key)
        {
            ValidateCollection();
            return _metadataIndex.GetRowId(key);
        }

        protected virtual void UpdateBucketInfo()
        {

            throw new Exception("BucketStats not available in BaseCollection class.");

        }

        public bool CreateIndex(IndexConfiguration indexConfiguration)
        {
            return false; //return _indexManager.CreateIndex(indexConfiguration, _dbContext.StorageManager);
        }

        public IDBResponse CreateIndex(ICreateIndexOperation operation)
        {
            _threadsEntered.IncrementCount();
            IDBResponse response = null;
            try
            {
                response = operation.CreateResponse();
                var indexConfiguration = operation.Configuration;
                //indexConfiguration.Name = operation.Configuration.Name;
                //indexConfiguration.Attributes = new IndexAttribute[operation.Configuration.Attributes.Length];
                //for (int i = 0; i < operation.Configuration.Attributes.Length; i++)
                //    indexConfiguration.Attributes[i].Name = operation.Configuration.Attributes[i];

                IndexManager.CreateIndex(indexConfiguration);
                response.IsSuccessfull = true;

            }
            catch (DatabaseException ex)
            {
                response.IsSuccessfull = false;
                response.ErrorCode = ex.ErrorCode;
                response.ErrorParams = ex.Parameters;
            }
            catch (Exception ex)
            {
                if (response != null)
                    response.IsSuccessfull = false;

                if (LoggerManager.Instance.IndexLogger != null)
                    LoggerManager.Instance.IndexLogger.Error("CollectionIndexManager", "Failed to create index, " + ex);
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
            return response;
        }

        public IDBResponse DropIndex(IDropIndexOperation operation)
        {
            _threadsEntered.IncrementCount();
            IDBResponse response = null;
            try
            {
                response = operation.CreateResponse();
                IndexManager.DropIndex(operation.IndexName);
                response.IsSuccessfull = true;
            }
            catch (DatabaseException ex)
            {
                response.IsSuccessfull = false;
                response.ErrorCode = ex.ErrorCode;
                response.ErrorParams = ex.Parameters;
            }
            catch (Exception ex)
            {
                if (response != null)
                    response.IsSuccessfull = false;

                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Error("CollectionIndexManager", "Failed to drop index, " + ex);
                }
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
            return response;
        }

        public IDBResponse RenameIndex(IRenameIndexOperation operation)
        {
            _threadsEntered.IncrementCount();
            IDBResponse response = null;
            try
            {
                response = operation.CreateResponse();
                IndexManager.RenameIndex(operation.OldIndexName, operation.NewIndexName);
                response.IsSuccessfull = true;
            }
            catch (DatabaseException ex)
            {
                response.IsSuccessfull = false;
                response.ErrorCode = ex.ErrorCode;
                response.ErrorParams = ex.Parameters;
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.IndexLogger != null)
                    LoggerManager.Instance.IndexLogger.Error("CollectionIndexManager", "Failed to Rename index, " + ex);

                if (response != null)
                    response.IsSuccessfull = false;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
            return response;
        }

        public IDBResponse RecreateIndex(IRecreateIndexOperation operation)
        {
            _threadsEntered.IncrementCount();
            IDBResponse response = null;
            try
            {
                response = operation.CreateResponse();
                IndexManager.RecreateIndex(operation.Configuration);
                response.IsSuccessfull = true;
            }
            catch (DatabaseException ex)
            {
                response.IsSuccessfull = false;
                response.ErrorCode = ex.ErrorCode;
                response.ErrorParams = ex.Parameters;
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.IndexLogger != null)
                    LoggerManager.Instance.IndexLogger.Error("CollectionIndexManager", "Failed to recreate index, " + ex);

                if (response != null)
                    response.IsSuccessfull = false;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
            return response;
        }

        protected virtual bool OnPreInsert(IJSONDocument document, IOperationContext context, DatabaseOperationType opType)
        {
            return true;
        }

        protected virtual void OnInsertOperationFailed(IJSONDocument document)
        {

        }

        private bool CollectionStatusValid()
        {
            if (_statusLatch.Status.Data == CollectionStatus.DROPPING)
                return false;
            return true;
        }

        public IDocumentsWriteResponse InsertDocuments(IDocumentsWriteOperation operation)
        {
            _threadsEntered.IncrementCount();
            IDocumentsWriteResponse response = null;
            try
            {
                response = operation.CreateResponse() as IDocumentsWriteResponse;
                response.IsSuccessfull = true;
                long lastOperationId = -1;

                foreach (var document in operation.Documents)
                {
                    //DO NOT REMOVE THE FOLLOWING COMMENTS -UMER
                    if (!IsKeyValid(document.Key)) //this check can be uncommented on need basis
                        document.GenerateDocumentKey();

                    var key = new DocumentKey(document.Key); // get document key from document
                    if (!OnPreInsert(document, operation.Context,operation.OperationType))
                    {
                        FailedDocument failedDocument = new FailedDocument();
                        failedDocument.DocumentKey = key.Value as string;
                        failedDocument.ErrorCode = ErrorCodes.Collection.BUCKET_UNAVAILABLE;
                        failedDocument.ErrorParameters = new string[0];
                        failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                        response.AddFailedDocument(failedDocument);
                        response.IsSuccessfull = false;
                        continue;
                    }



                    if (_disposed)
                    {
                        response.IsSuccessfull = false;
                        FailedDocument failedDocument = new FailedDocument();
                        failedDocument.DocumentKey = key.Value.ToString();
                        failedDocument.ErrorCode = ErrorCodes.Collection.COLLECTION_DISPODED;
                        failedDocument.ErrorParameters = new[] { operation.Collection };
                        failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode, failedDocument.ErrorParameters);
                        response.AddFailedDocument(failedDocument);
                        continue;
                    }

                    try
                    {

                        InsertResult<JSONDocument> result;
                        InsertOperation insertOperation = null;

                        try
                        {
                            using (_dbContext.LockManager.GetKeyWriterLock(_dbContext.DatabaseName, Name, key))
                            {
                                if (_metadataIndex.GetRowId(key) == -1)
                                {
                                    insertOperation = new InsertOperation
                                    {
                                        OperationId = _dbContext.GenerateOperationId(),
                                        RowId = _metadataIndex.GenerateRowId(),
                                        Collection = Name,
                                        Document = document as JSONDocument,
                                        Context = operation.Context
                                    };

                                    result = _docStore.InsertDocument(insertOperation);
                                }
                                else
                                {
                                    FailedDocument failedDocument = new FailedDocument();
                                    failedDocument.DocumentKey = key.Value.ToString();
                                    failedDocument.ErrorCode = ErrorCodes.Collection.KEY_ALREADY_EXISTS;  //If anyone change it then please inform as these codes are also used in REST API.
                                    failedDocument.ErrorParameters = new string[2];
                                    failedDocument.ErrorParameters[0] = key.ToString();
                                    failedDocument.ErrorParameters[1] = _configuration.CollectionName;
                                    failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode, failedDocument.ErrorParameters);
                                    response.AddFailedDocument(failedDocument);

                                    response.IsSuccessfull = false;
                                    OnInsertOperationFailed(document);
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (LoggerManager.Instance.StorageLogger != null)
                            {
                                LoggerManager.Instance.StorageLogger.Error("InsertDocuments", "Error during document insertion:" + ex);
                            }
                            _docStore.AddFailedOperation(insertOperation);
                            throw;
                        }

                        if (result != null && result.Success)
                        {
                            //Either add in replication queue or update bucket stats depends upon type of collection
                            JSONDocument rollbackDocument = new JSONDocument();
                            rollbackDocument.Key = insertOperation.Document.Key;
                            OnPostInsertDocument(insertOperation, rollbackDocument);

                            lastOperationId = insertOperation.OperationId;
                        }
                        else
                        {
                            _docStore.AddFailedOperation(insertOperation);
                            response.IsSuccessfull = false;

                            FailedDocument failedDocument = new FailedDocument();
                            failedDocument.DocumentKey = key.Value.ToString();
                            failedDocument.ErrorCode = ErrorCodes.Cache.UNKNOWN_ISSUE;
                            failedDocument.ErrorParameters = new string[1];
                            failedDocument.ErrorParameters[0] = "Insert operation was unsuccessfull.";
                            failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(ErrorCodes.Collection.UNKNOWN_ISSUE);
                            response.AddFailedDocument(failedDocument);

                            OnInsertOperationFailed(document);
                            //_dbContext.LockManager.ReleaseKeyWriterLock(_dbContext.DatabaseName, this.Name, key.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        response.IsSuccessfull = false;

                        FailedDocument failedDocument = new FailedDocument();
                        failedDocument.DocumentKey = key.Value.ToString();
                        failedDocument.ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE;
                        failedDocument.ErrorParameters = new string[1];
                        failedDocument.ErrorParameters[0] = e.Message;
                        failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                        response.AddFailedDocument(failedDocument);

                        OnInsertOperationFailed(document);

                        Console.WriteLine(e);
                        throw;
                    }


                    if (!CollectionStatusValid()) // If dropping, discard further documents and return response.
                        return response;

                }

                if (lastOperationId == -1) return response;

                OnPostDataManipulation();
                return response;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("BaseCollection", e);

                if (response != null)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE;
                    response.ErrorParams = new[] { e.Message };
                    foreach (var doc in operation.Documents)
                    {
                        response.AddFailedDocument(new FailedDocument() { DocumentKey = doc.Key, ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE, ErrorParameters = new string[0] });
                    }

                }

                return response;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        public bool InsertDocument(IJSONDocument document, IOperationContext operationContext)
        {
            ValidateCollection();
            long lastOperationId = -1;
            bool preTriggerFired = true;


            if (!IsKeyValid(document.Key))
                document.GenerateDocumentKey();

            if (!OnPreInsert(document, operationContext,DatabaseOperationType.Default))
                return false;
            var key = new DocumentKey(document.Key);

            if (preTriggerFired)
            {
                InsertResult<JSONDocument> result;
                InsertOperation insertOperation = null;


                try
                {
                    using (_dbContext.LockManager.GetKeyWriterLock(_dbContext.DatabaseName, Name, key))
                    {
                        if (_metadataIndex.GetRowId(key) == -1)
                        {
                            insertOperation = new InsertOperation
                            {
                                OperationId = _dbContext.GenerateOperationId(),
                                RowId = _metadataIndex.GenerateRowId(),
                                Collection = Name,
                                Document = document as JSONDocument
                            };

                            result = _docStore.InsertDocument(insertOperation);
                        }
                        else
                        {
                            OnInsertOperationFailed(document);
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    _docStore.AddFailedOperation(insertOperation);
                    throw;
                }

                if (result != null && result.Success)
                {

                    //Either add in replication queue or update bucket stats depends upon type of collection
                    JSONDocument rollbackDocument = new JSONDocument();
                    rollbackDocument.Key = insertOperation.Document.Key;
                    OnPostInsertDocument(insertOperation, rollbackDocument);

                    lastOperationId = insertOperation.OperationId;

                }
                else
                {
                    _docStore.AddFailedOperation(insertOperation);
                    OnInsertOperationFailed(document);
                    return false;
                }
            }
            else
            {
                OnInsertOperationFailed(document);
                return false;
            }


            if (lastOperationId == -1)
            {
                OnInsertOperationFailed(document);
                return false;
            }

            return true;
        }

        protected virtual void OnPostInsertDocument(InsertOperation insertOperation, JSONDocument rollbackDocument)
        {
        }

        protected int ApplyInsert(QueryPlan targets, IOperationContext operationContext)
        {
            return InsertDocument(targets.Criteria.NewDocument, operationContext) ? 1 : 0;
        }

        protected virtual bool OnPreUpdate(DocumentKey key, IOperationContext context)
        {
            return true;
        }

        protected virtual void OnPostDataManipulation()
        { }

        public virtual HashMapBucket GetKeyBucket(DocumentKey key)
        {
            return null;
        }
        public IUpdateResponse UpdateDocuments(IUpdateOperation operation)
        {
            _threadsEntered.IncrementCount();
            try
            {
                return ExecuteNonQuery(new WriteQueryOperation
                {
                    Collection = operation.Collection,
                    Context = operation.Context,
                    Database = operation.Database,
                    OperationType = operation.OperationType,
                    Query = operation.Query,
                    RequestId = operation.RequestId,
                });
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        public virtual bool UpdateDocument(long rowId, IJSONDocument newDocument, IOperationContext operationContext)
        {
            ValidateCollection();
            long lastOperationId = -1;
            bool success;

            bool preTriggerFired = true;

            DocumentKey dKey;

            try
            {
                if (preTriggerFired)
                {
                    UpdateOperation operation = null;
                    UpdateResult<JSONDocument> result;

                    try
                    {

                        if (_metadataIndex.TryGetKey(rowId, out dKey))
                        {
                            using (_dbContext.LockManager.GetKeyWriterLock(_dbContext.DatabaseName, Name, dKey))
                            {
                                //Cross Check needs to be done as contains is not in lock
                                if (_metadataIndex.ContainsKey(dKey))
                                {
                                    if (!OnPreUpdate(dKey, operationContext))
                                        return false;

                                    operation = new UpdateOperation()
                                    {
                                        Collection = Name,
                                        OperationId = _dbContext.GenerateOperationId(),
                                        RowId = rowId,
                                        Update = newDocument as JSONDocument,
                                        Context = operationContext
                                    };

                                    result = _docStore.UpdateDocument(operation);
                                }
                                else return false;
                            }

                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.StorageLogger != null)
                        {
                            LoggerManager.Instance.StorageLogger.Error("DeleteDocument", "Error during document updation through query:" + ex);
                        }
                        _docStore.AddFailedOperation(operation);
                        throw;
                    }
                    success = result != null && result.Success;

                    if (success)
                    {

                        //Either add in replication queue or update bucket stats depends upon type of collection
                        OnPostUpdateDocument(result, operation);

                        lastOperationId = operation.OperationId;


                    }
                    else
                    {
                        _docStore.AddFailedOperation(operation);
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Update Doc error", e);
                return false;
            }

            if (lastOperationId == -1)
                return success;


            return success;
        }

        public virtual IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation op)
        {
            _threadsEntered.IncrementCount();
            var response = op.CreateResponse() as IDocumentsWriteResponse;
            try
            {
                long lastOperationId = -1;

                foreach (var jsonDocument in op.Documents)
                {
                    var document = (JSONDocument)jsonDocument;
                    try
                    {
                        DocumentKey key = new DocumentKey(document.Key);

                        UpdateResult<JSONDocument> result = null;
                        UpdateOperation operation = null;

                        try
                        {
                            using (_dbContext.LockManager.GetKeyWriterLock(_dbContext.DatabaseName, Name, key))
                            {
                                long rowId;
                                if ((rowId = _metadataIndex.GetRowId(key)) != -1)
                                {
                                    operation = new UpdateOperation()
                                    {
                                        Collection = Name,
                                        RowId = rowId,
                                        OperationId = _dbContext.GenerateOperationId(),
                                        Update = document
                                    };

                                    result = _docStore.UpdateDocument(operation);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (LoggerManager.Instance.StorageLogger != null)
                            {
                                LoggerManager.Instance.StorageLogger.Error("DeleteDocument", "Error during document replacement:" + ex);
                            }
                            _docStore.AddFailedOperation(operation);
                            throw;
                        }

                        if (result == null)
                        {
                            response.IsSuccessfull = false;
                            var failedDocument = new FailedDocument
                            {
                                DocumentKey = document.Key,
                                ErrorCode = ErrorCodes.Collection.DOCUMENT_DOES_NOT_EXIST,
                                ErrorParameters = new string[1]
                            };
                            //If anyone change it then please inform as these codes are also used in REST API.
                            failedDocument.ErrorParameters[0] = document.Key;
                            failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                            response.AddFailedDocument(failedDocument);
                            continue;
                        }
                        if (result != null && result.Success)
                        {
                            response.IsSuccessfull = true;
                            //Either add in replication queue or update bucket stats depends upon type of collection
                            OnPostUpdateDocument(result, operation);

                            lastOperationId = operation.OperationId;
                        }
                        else
                        {
                            _docStore.AddFailedOperation(operation);
                        }
                    }
                    catch (Exception ex)
                    {
                        response.IsSuccessfull = false;

                        FailedDocument failedDocument = new FailedDocument();
                        failedDocument.DocumentKey = document.Key;
                        failedDocument.ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE;
                        failedDocument.ErrorParameters = new[] { ex.Message };
                        failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                        response.AddFailedDocument(failedDocument);
                        //return false;
                    }
                    if (lastOperationId == -1) return response;

                }
                return response;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("BaseCollection", e);

                if (response != null)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE;
                    response.ErrorParams = new[] { e.Message };
                    foreach (var doc in op.Documents)
                    {
                        response.AddFailedDocument(new FailedDocument()
                        {
                            DocumentKey = doc.Key,
                            ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE,
                            ErrorParameters = new string[0]
                        });
                    }

                }

                return response;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        protected virtual void OnPostUpdateDocument(UpdateResult<JSONDocument> result, UpdateOperation operation)
        {
            //TODO:  - update operation replication logic.
        }


        protected Result ApplyUpdate(QueryPlan targets, IOperationContext operationContext)
        {
            var result = new Result { affectedDocs = 0 };
            var terminal = targets.Predicate as TerminalPredicate;
            if (terminal != null)
            {
                IResultSet<KeyValuePair<AttributeValue, long>> resultSet = null;
                terminal.Evaluate(ref resultSet, targets.Criteria);
                foreach (var keyValuePair in resultSet)
                {
                    IJSONDocument oldDocument = GetDocument(keyValuePair.Value, operationContext);
                    IJSONDocument newDocument;
                    if (!targets.Criteria.DocumentUpdate.TryUpdate(oldDocument, out newDocument))
                        continue;
                    if (UpdateDocument(keyValuePair.Value, newDocument, operationContext))
                    {
                        result.affectedDocs++;
                    }
                }
            }
            return result;
        }

        protected virtual bool OnPreGetDocuments(DocumentKey documentKey, IOperationContext context)
        {
            return true;
        }

        public IGetResponse GetDocuments(IGetOperation operation)
        {
            try
            {
                _threadsEntered.IncrementCount();
                var response = operation.CreateResponse() as IGetResponse;
                var resultSet = new ListedResultSet<long>();
                IDataTransform transform = new NoTransform();
                DocumentKey key;
                foreach (var document in operation.DocumentIds)
                {
                    try
                    {
                        key = new DocumentKey(document.Key);
                        if (operation.OperationType != DatabaseOperationType.StateTransferInsert && !OnPreGetDocuments(key, operation.Context))
                        {

                            resultSet.Add(_lastTemporaryAssignedRowId);
                            Interlocked.Decrement(ref _lastTemporaryAssignedRowId);
                            continue;
                        }

                        using (_dbContext.LockManager.GetKeyReaderLock(_dbContext.DatabaseName, Name, key))
                        {
                            long rowId;
                            if ((rowId = _metadataIndex.GetRowId(key)) != -1)
                            {
                                resultSet.Add(rowId);
                            }
                        }
                    }
                    catch (DatabaseException ex)
                    {
                        response.IsSuccessfull = false;
                        response.ErrorCode = ex.ErrorCode;
                        response.ErrorParams = ex.Parameters;
                    }
                    catch (Exception ex)
                    {
                        response.IsSuccessfull = false;
                        if (LoggerManager.Instance.ServerLogger != null)
                        {
                            LoggerManager.Instance.ServerLogger.Error("GetDocuments", ex.ToString());
                        }
                    }
                }

                IQueryResult result = new QueryResult(resultSet, this, transform, operation.Context, 0);

                if (response != null)
                {
                    IDataChunk dataChunk = response.DataChunk;
                    response.IsSuccessfull = result.FillDataChunk(0, ref dataChunk);
                    if (!dataChunk.IsLastChunk)
                        _queryResultManager.Add(result.Id, result);
                }
                return response;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        public IJSONDocument GetDocument(long rowId, IOperationContext context)
        {
            ValidateCollection();
            IJSONDocument document = _metadataIndex.ContainsRowId(rowId)
                ? _docStore.GetDocument(new GetOperation { Collection = Name, RowId = rowId }).Document
                : null;
            if (context != null && document != null && document.Contains(CappedConstant.InsertionOrder.ToString()) &&
                context.ContainsKey(ContextItem.RemoveInsertionAttribute) &&
                (bool)context[ContextItem.RemoveInsertionAttribute])
            {
                document = document.Clone() as IJSONDocument;
                document.Remove(CappedConstant.InsertionOrder.ToString());
            }
            return document;

        }

        public IJSONDocument GetDocument(DocumentKey key, IOperationContext context)
        {
            ValidateCollection();
            _threadsEntered.IncrementCount();
            long rowId = _metadataIndex.GetRowId(key);
            if (rowId != -1)
                return GetDocument(rowId, context);
            return null;
        }



        protected virtual bool OnPreDelete(DocumentKey key, IOperationContext context)
        {
            return true;
        }

        public virtual IDocumentsWriteResponse DeleteDocuments(IDocumentsWriteOperation operation)
        {
            _threadsEntered.IncrementCount();
            IDocumentsWriteResponse response = null;
            try
            {
                response = operation.CreateResponse() as IDocumentsWriteResponse;
                response.IsSuccessfull = true;
                long lastOperationId = -1;
                DocumentKey key;

                foreach (var document in operation.Documents)
                {
                    key = new DocumentKey(document.Key); // get document key from document


                    try
                    {
                        if (!OnPreDelete(key, operation.Context))
                        {
                            response.IsSuccessfull = false;

                            FailedDocument failedDocument = new FailedDocument();
                            failedDocument.DocumentKey = key.Value.ToString();
                            failedDocument.ErrorCode = ErrorCodes.Collection.BUCKET_UNAVAILABLE;
                            failedDocument.ErrorParameters = new string[0];
                            failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                            response.AddFailedDocument(failedDocument);

                            continue;
                        }

                        long rowId;

                        DeleteResult<JSONDocument> result = null;
                        RemoveOperation removeOp = null;


                        try
                        {
                            using (_dbContext.LockManager.GetKeyWriterLock(_dbContext.DatabaseName, Name, key))
                            {
                                rowId = _metadataIndex.GetRowId(key);
                                if (rowId != -1)
                                {
                                    removeOp = new RemoveOperation
                                    {
                                        OperationId = _dbContext.GenerateOperationId(),
                                        RowId = rowId,
                                        Key = key,
                                        Collection = Name,
                                        Context = operation.Context
                                    };

                                    result = _docStore.DeleteDocument(removeOp);
                                }
                                else
                                {
                                    //if key not exists then still delete will be succesfull.
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (LoggerManager.Instance.StorageLogger != null)
                            {
                                LoggerManager.Instance.StorageLogger.Error("DeleteDocument", "Error during document deletion:" + ex);
                            }
                            _docStore.AddFailedOperation(removeOp);
                            throw;
                        }

                        if (result != null && result.Success)
                        {

                            //Either add in replication queue or update bucket stats depends upon type of collection
                            OnPostDeleteDocument(result, removeOp, result.Document);

                            lastOperationId = removeOp.OperationId;
                        }
                        else
                        {
                            _docStore.AddFailedOperation(removeOp);
                            response.IsSuccessfull = false;

                            FailedDocument failedDocument = new FailedDocument();
                            failedDocument.DocumentKey = key.Value.ToString();
                            failedDocument.ErrorCode = ErrorCodes.Cache.UNKNOWN_ISSUE;
                            failedDocument.ErrorParameters = new string[1];
                            failedDocument.ErrorParameters[0] = "Delete operation was unsuccessfull.";
                            failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                            response.AddFailedDocument(failedDocument);
                        }

                    }
                    catch (Exception ex)
                    {
                        response.IsSuccessfull = false;

                        FailedDocument failedDocument = new FailedDocument();
                        failedDocument.DocumentKey = key.Value.ToString();
                        failedDocument.ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE;
                        failedDocument.ErrorParameters = new string[1];
                        failedDocument.ErrorParameters[0] = ex.Message;
                        failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode);
                        response.AddFailedDocument(failedDocument);
                    }

                }

                if (lastOperationId == -1) return response;


                OnPostDataManipulation();
                return response;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("BaseCollection", e);

                if (response != null)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE;
                    response.ErrorParams = new[] { e.Message };
                    foreach (var doc in operation.Documents)
                    {
                        response.AddFailedDocument(new FailedDocument()
                        {
                            DocumentKey = doc.Key,
                            ErrorCode = ErrorCodes.Collection.UNKNOWN_ISSUE,
                            ErrorParameters = new string[0]
                        });
                    }

                }
                return response;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        protected virtual void OnPostDeleteDocument(DeleteResult<JSONDocument> result, RemoveOperation removeOp, IJSONDocument rollbackDocument)
        {
        }

        public virtual bool DeleteDocument(long rowId, IOperationContext operationContext)
        {
            ValidateCollection();
            long lastOperationId = -1;
            bool success;
            bool preTriggerFired = true;

            DocumentKey dKey;


            if (preTriggerFired)
            {
                RemoveOperation operation = null;
                DeleteResult<JSONDocument> result;

                try
                {
                    if (_metadataIndex.TryGetKey(rowId, out dKey))
                    {
                        using (_dbContext.LockManager.GetKeyWriterLock(_dbContext.DatabaseName, Name, dKey))
                        {
                            //Cross Check needs to be done as contains is not in lock
                            if (_metadataIndex.ContainsKey(dKey))
                            {
                                if (!OnPreDelete(dKey, operationContext))
                                    return false;

                                operation = new RemoveOperation
                                {
                                    Collection = Name,
                                    OperationId = _dbContext.GenerateOperationId(),
                                    RowId = rowId,
                                    Key = dKey,
                                    Context = operationContext
                                };


                                result = _docStore.DeleteDocument(operation);
                            }
                            else return false;
                        }
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.StorageLogger != null)
                    {
                        LoggerManager.Instance.StorageLogger.Error("DeleteDocument", "Error during document deletion through query:" + ex);
                    }
                    _docStore.AddFailedOperation(operation);
                    throw;
                }
                success = result != null && result.Success;

                if (success)
                {

                    //Either add in replication queue or update bucket stats depends upon type of collection
                    OnPostDeleteDocument(result, operation, result.Document);

                    lastOperationId = operation.OperationId;

                }
                else
                {
                    _docStore.AddFailedOperation(operation);
                }
            }
            else
            {
                return false;
            }

            if (lastOperationId == -1) return success;

            return success;
        }

        public virtual Result ApplyDeleteIterator(IResultSet<KeyValuePair<AttributeValue, long>> resultSet, IOperationContext operationContext)
        {
            var result = new Result { affectedDocs = 0 };
            foreach (var kvp in resultSet)
            {
                if (DeleteDocument(kvp.Value, operationContext))
                    result.affectedDocs++;
            }
            return result;
        }
        protected virtual Result ApplyDelete(QueryPlan targets, IOperationContext operationContext)
        {
            var result = new Result { affectedDocs = 0 };
            var terminal = targets.Predicate as TerminalPredicate;
            if (terminal != null)
            {
                IResultSet<KeyValuePair<AttributeValue, long>> resultSet = null;
                terminal.Evaluate(ref resultSet, targets.Criteria);

                result = ApplyDeleteIterator(resultSet, operationContext);
            }
            return result;
        }

        //protected virtual OperationId OnPreExecuteNonQuery(INonQueryOperation operation, bool isReplicationOperation)
        //{
        //    return null;
        //}

        public IUpdateResponse ExecuteNonQuery(INonQueryOperation operation)
        {
            ValidateCollection();

            try
            {
                _threadsEntered.IncrementCount();
                var stats = new UsageStats();
                stats.BeginSample();
                var response = operation.CreateResponse() as IUpdateResponse;

                try
                {
                    IDmObject parsedQuery = null;
                    int affectedDocs = 0;

                    //Operation logging
                    // var isReplicationOperation = (bool) operation.Context[ContextItem.IsReplicationOperation];
                    //var operationId = OnPreExecuteNonQuery(operation, isReplicationOperation);
                    //operation id to add in its child operations (minor operations)

                    if (operation.Context != null)
                        parsedQuery = operation.Context[ContextItem.ParsedQuery] as IDmObject;

                    QueryPlan queryPlan = _queryOptimizer.GetQueryPlan(parsedQuery, operation.Query, this, _metadataIndex);

                    if (LoggerManager.Instance.QueryLogger != null && LoggerManager.Instance.QueryLogger.IsDebugEnabled)
                    {
                        var writer = new StringWriter();
                        queryPlan.Print(writer);
                        LoggerManager.Instance.QueryLogger.Debug("QueryPlan", writer.ToString());
                    }

                    switch (queryPlan.Criteria.UpdateOption)
                    {
                        case UpdateOption.Insert:
                            affectedDocs = ApplyInsert(queryPlan, operation.Context);
                            break;

                        case UpdateOption.Update:
                            affectedDocs = ApplyUpdate(queryPlan, operation.Context).affectedDocs;
                            break;

                        case UpdateOption.Delete:
                            affectedDocs = ApplyDelete(queryPlan, operation.Context).affectedDocs;
                            break;
                    }
                    response.AffectedDocuments = affectedDocs;
                    response.IsSuccessfull = true;
                    //locking to be done on lower-level where document ids/keys are visible
                    OnPostDataManipulation();
                }
                catch (DatabaseException ex)
                {
                    LoggerManager.Instance.QueryLogger.Error("ExecuteNonQuery", "Query Execution Failure, " + ex);
                    response.IsSuccessfull = false;
                    if (ex.ErrorCode.Equals(ErrorCodes.Collection.DELETE_NOT_ALLOWED_CAPPED))
                    {
                        response.ErrorCode = ex.ErrorCode;
                        response.AffectedDocuments = 0;
                        return response;
                    }
                    response.ErrorCode = ex.ErrorCode;
                    if (ex.Parameters != null)
                        response.ErrorParams = ex.Parameters;

                }
                catch (Exception ex)
                {
                    LoggerManager.Instance.QueryLogger.Error("ExecuteNonQuery", "Query Execution Failure, " + ex);
                    response.IsSuccessfull = false;
                    response.ErrorCode = ErrorCodes.Query.UNKNOWN_ISSUE;
                }
                finally
                {
                    if (_statsCollector != null)
                    {
                        stats.EndSample();
                        _statsCollector.IncrementStatsValue(StatisticsType.AvgQueryExecutionTime, stats.Current);
                    }
                }
                return response;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        protected virtual void OnPreExecuteQuery(IDmObject parsedQuery, IOperationContext context)
        {

        }

        private IQueryStore QueryCollection
        {
            get
            {
                var sysDb = _dbManager.GetDatabase(MiscUtil.SYSTEM_DATABASE) as SystemDatabaseStore;
                return sysDb != null ? sysDb.GetCollection(Util.MiscUtil.SystemCollection.QueryResultCollection) : null;
            }
        }

        public IQueryResponse ExecuteQuery(IQueryOperation operation)
        {
            if (!operation.Context.ContainsKey(ContextItem.InternalOperation)) ValidateCollection();
            try
            {
                _threadsEntered.IncrementCount();
                long currentQueryId = Interlocked.Increment(ref queryId);
                if (queryId == long.MaxValue) queryId = queryId = new Random().Next();
                if (LoggerManager.Instance.QueryLogger != null)
                {
                    LoggerManager.Instance.QueryLogger.Debug("ExecuteQuery",
                        "Collection " + Name + " queried for: " + operation.Query.QueryText + ", QueryID: " + currentQueryId);
                }

                var stats = new UsageStats();
                stats.BeginSample();
                var response = operation.CreateResponse() as IQueryResponse;
                try
                {
                    var queryResult = GetQueryResult(operation, currentQueryId);
                    if (response != null)
                    {
                        var dataChunk = response.DataChunk;

                        response.IsSuccessfull = queryResult.FillDataChunk(0, ref dataChunk);
                    }

                    if (response != null && !response.DataChunk.IsLastChunk)
                        _queryResultManager.Add(queryResult.Id, queryResult);
                }
                catch (DatabaseException ex)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = ex.ErrorCode;
                    if (ex.Parameters != null)
                        response.ErrorParams = ex.Parameters;
                    if (LoggerManager.Instance.QueryLogger != null)
                        LoggerManager.Instance.QueryLogger.Error("ExecuteQuery", "Query Execution Failure, " + ex);
                }
                catch (Exception ex)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = ErrorCodes.Query.UNKNOWN_ISSUE;
                    if (LoggerManager.Instance.QueryLogger != null)
                        LoggerManager.Instance.QueryLogger.Error("ExecuteQuery", "Query Execution Failure, " + ex);
                }
                finally
                {
                    if (_statsCollector != null)
                    {
                        stats.EndSample();
                        _statsCollector.IncrementStatsValue(StatisticsType.AvgQueryExecutionTime, stats.Current);
                        if (LoggerManager.Instance.QueryLogger != null)
                            LoggerManager.Instance.QueryLogger.Debug("ExecuteQuery",
                                "QueryID: " + currentQueryId + ", Query Execution Time: " +
                                ((float)stats.Current / stats.Frequency) + " sec");
                    }
                }
                return response;
            }
            finally
            {
                _threadsEntered.DecrementCount();
            }
        }

        private IQueryResult GetQueryResult(IQueryOperation operation, long currentQueryId)
        {
            IDmObject parsedQuery = null;
            if (operation.Context != null)
                parsedQuery = operation.Context[ContextItem.ParsedQuery] as IDmObject;

            OnPreExecuteQuery(parsedQuery, operation.Context);

            var queryPlan = _queryOptimizer.GetQueryPlan(parsedQuery, operation.Query, this, _metadataIndex);
            if (queryPlan.Criteria.IsGrouped || queryPlan.IsSpecialExecution)
            {
                queryPlan.Criteria.SubstituteStore = QueryCollection;
            }


            queryPlan.Criteria.QueryId = currentQueryId;


            //locking to be done on lower-level where document ids/keys are visible
            var finalizer = new Predicator(queryPlan, operation.Context);

            //Warning, do not uncomment this semicolon
            //;

            if (LoggerManager.Instance.QueryLogger != null && LoggerManager.Instance.QueryLogger.IsDebugEnabled)
            {
                var writer = new StringWriter();
                queryPlan.Print(writer);
                LoggerManager.Instance.QueryLogger.Debug("QueryPlan", writer.ToString());
            }
            return finalizer.Result;
        }

        public IGetChunkResponse GetDataChunk(IGetChunkOperation operation)
        {
            return null;
            //IQueryResult queryResult = _queryResultManager.Get(operation.ReaderUID);

            //if (queryResult is QueryResult)
            //    queryResult = (QueryResult)queryResult;
            //else if (queryResult is GroupResult)
            //    queryResult = (GroupResult)queryResult;

            //IGetChunkResponse response = operation.CreateResponse() as IGetChunkResponse;
            //if (response != null)
            //{
            //    IDataChunk dataChunk = response.DataChunk;
            //    response.IsSuccessfull = queryResult.FillDataChunk(operation.LastChunkId, ref dataChunk);
            //}
            //return response;
        }

        private ElectionId _electionResult;

        public ElectionId ElectionResult
        {
            set
            {
                //if (_electionResult == null || (value != null && value.Id != _electionResult.Id))
                // ElectionBasedSequencialId = 0;
                _electionResult = value;
            }
            get
            {
                return _electionResult ?? _context.ElectionResult.ElectionId;
            }
        }



        public IEnumerator<IJSONDocument> GetEnumerator()
        {
            foreach (var rowId in _metadataIndex)
            {
                if (_metadataIndex.ContainsRowId(rowId))
                {
                    OperationContext context = new OperationContext();
                    context.Add(ContextItem.QueryExecution, true);
                    var document = _docStore.GetDocument(new GetOperation
                    {
                        RowId = rowId,
                        Context = context
                    }).Document;
                    if (document == null) continue;
                    yield return document;
                }

            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IDBResponse DiposeReader(IDiposeReaderOperation operation)
        {
            return null;

        }

        public void Dispose()
        {
            try
            {
                _statusLatch.SetStatusBit(CollectionStatus.DISPOSING, CollectionStatus.RUNNING);
                //Thread.Sleep(2000); // wait for 2 seconds.
                _threadsEntered.WaitForCount(); // don't wait if disposing.

                _disposed = true;
                _docStore.Dispose();

                _metadataIndex.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error while Disposing Collection : " + ex);
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("BaseCollection.Dispose() ", "Error while Disposing Collection : " + ex);
                throw;
            }
        }


        public void Destroy(bool destroyingNode)
        {
            try
            {
                _statusLatch.SetStatusBit(CollectionStatus.DROPPING, CollectionStatus.RUNNING);
                _threadsEntered.WaitForCount();

                //if (_statsCollector != null)
                //{
                //    _statsCollector.DecrementStatsValue(StatisticsType.DocumentCount, DocumentCount);
                //}
                _docStore.Dispose();

                _dbContext.PersistenceManager.RemoveMetadataIndex(Name);
                IndexManager.Destroy();
                _metadataIndex.Destroy();
                _dbContext.LockManager.DropCollectionLocks(_dbContext.DatabaseName, Name);

            }
            catch (Exception e)
            {
                Console.WriteLine(@"Error while Dropping Collection : " + e);
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("BaseCollection.Destroy() ", "Error while Dropping Collection : " + e);

                throw;
            }
        }

        public bool ContainsKey(DocumentKey key)
        {
            ValidateCollection();
            return _metadataIndex.ContainsKey(key);
        }

        public bool IsKeyValid(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            //Type valueType = Key.Value.GetType();
            //TypeCode actualType = Type.GetTypeCode(valueType);
            //switch (actualType)
            //{
            //    case TypeCode.Boolean:
            //        return true;
            //    case TypeCode.Byte:
            //    //case TypeCode.Decimal:
            //    case TypeCode.Double:
            //    case TypeCode.Int16:
            //    case TypeCode.Int32:
            //    case TypeCode.Int64:
            //    case TypeCode.SByte:
            //    case TypeCode.Single:
            //    case TypeCode.UInt16:
            //    case TypeCode.UInt32:
            //    case TypeCode.UInt64:
            //        return true;
            //    case TypeCode.Char:
            //    case TypeCode.String:
            //        return true;
            //    case TypeCode.DateTime:
            //        return true;
            //}
            ////if (valueType.IsArray)
            ////    return true;
            return true;
        }
        public void Print(TextWriter output)
        {
            output.Write("Collection:{");
            output.Write("Name=" + _configuration.CollectionName);
            output.Write("}");
        }

        private void ValidateCollection()
        {
            if (
                Status.IsAnyBitsSet(CollectionStatus.INITIALIZING | CollectionStatus.DISPOSING |
                                    CollectionStatus.DISPOSING))
                throw new DatabaseException(ErrorCodes.Collection.COLLECTION_NOT_AVAILABLE, new[] { Name });
        }

        public struct Result
        {
            internal int affectedDocs;
            internal HashSet<long> rowIdList;
        }
    }
}
