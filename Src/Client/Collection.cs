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
using System.Data;
using System.Linq;

using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Client.Exceptions;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.EXIM;
using Alachisoft.NosDB.Common.Logger;


namespace Alachisoft.NosDB.Client
{
    public class Collection<T>
    {
        private Database _database;
        private string _collectionName;
        
        internal Database Database
        {
            set { _database = value; }
        }

        public string Name
        {
            internal set { _collectionName = value; }
            get { return _collectionName; }
        }

        public string FullName
        {
               get { return _database.DatabaseName + "." + _collectionName; }
        }

        internal Collection() { }

        #region CRUD Operations

        public void InsertDocument(T document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            //JsonSerializer<T> serializer = new JsonSerializer<T>();
            List<JSONDocument> Jsondocuments = new List<JSONDocument>();
            Jsondocuments.Add(JsonSerializer.Serialize<T>(document));

            List<FailedDocument> failedDocuments = InsertDocuments(Jsondocuments);

            //if (failedDocuments != null && failedDocuments.Count > 0)
            //    return failedDocuments.First().Value;
            //return 0;
            if (failedDocuments != null && failedDocuments.Count > 0)
                throw new OperationFailedException("Operation failed with error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(failedDocuments.First().ErrorCode, failedDocuments.First().ErrorParameters));
        }

        public List<FailedDocument> InsertDocuments(ICollection<T> documents)
        {
            if (documents == null)
                throw new ArgumentNullException("documents");
            if (documents.Count < 1)
                throw new ArgumentException("No document is present to insert");

            //JsonSerializer<T> serializer = new JsonSerializer<T>();
            List<JSONDocument> Jsondocuments = JsonSerializer.Serialize<T>(documents.ToList());

            return InsertDocuments(Jsondocuments);
        }

        internal List<FailedDocument> InsertDocuments(ICollection<JSONDocument> documents, bool noResponse = false)
        {
            InsertDocumentsOperation insertOperation = new InsertDocumentsOperation();
            insertOperation.Documents = documents.Cast<IJSONDocument>().ToList();
            insertOperation.Database = _database.DatabaseName;
            insertOperation.Collection = _collectionName;
            insertOperation.NoResponse = noResponse;

            InsertDocumentsResponse insertResponse = (InsertDocumentsResponse)_database.ExecutionMapper.InsertDocuments(insertOperation);

            if (!insertResponse.IsSuccessfull)
            {
                if (insertResponse.FailedDocumentsList == null || insertResponse.FailedDocumentsList.Count == 0)
                {
                    throw new DataException(ErrorMessages.GetErrorMessage(insertResponse.ErrorCode, insertResponse.ErrorParams));
                }
                return insertResponse.FailedDocumentsList;
            }
            return new List<FailedDocument>();
        }

        public T GetDocument(string documentKey)
        {
            if (documentKey == null)
                throw new ArgumentNullException("documentKey");

            List<string> documentKeys = new List<string>();
            documentKeys.Add(documentKey);

            ICollectionReader reader = GetDocuments(documentKeys);
            if (reader != null && reader.ReadNext() && reader.GetDocument() != null)
                return reader.GetObject<T>();
            else
                return default(T);
            //throw new OperationFailedException("Operation Failed with Error code: " + failedDocuments.First().ErrorCode);
        }

        public ICollectionReader GetDocuments(ICollection<string> documentKeys)
        {
            if (documentKeys == null)
                throw new ArgumentNullException("documentKeys");
            if (documentKeys.Count < 1)
                throw new ArgumentException("No DocumentKey specified");

            List<JSONDocument> documents = new List<JSONDocument>();
            foreach (string documentKey in documentKeys)
            {
                if (documentKey != null)
                {
                    JSONDocument jdoc = new JSONDocument();
                    jdoc.Key = documentKey;
                    documents.Add(jdoc);
                }
            }

            GetDocumentsOperation getDocumentsOperation = new GetDocumentsOperation();

            getDocumentsOperation.DocumentIds = documents.Cast<IJSONDocument>().ToList();
            getDocumentsOperation.Database = _database.DatabaseName;
            getDocumentsOperation.Collection = _collectionName;

            GetDocumentsResponse getDocumentsResponse = (GetDocumentsResponse)_database.ExecutionMapper.GetDocuments(getDocumentsOperation);

            if (getDocumentsResponse.IsSuccessfull)
            {
                CollectionReader reader = new CollectionReader((DataChunk)getDocumentsResponse.DataChunk, _database.ExecutionMapper, _database.DatabaseName, _collectionName);
                return reader;
            }
            else
                throw new Exception("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(getDocumentsResponse.ErrorCode, getDocumentsResponse.ErrorParams));
        }

        public void ReplaceDocument(T document)
        {
            if (document == null) throw new ArgumentNullException("document");

            List<T> Jsondocuments = new List<T>();
            Jsondocuments.Add(document);

            List<FailedDocument> failedDocuments = ReplaceDocuments(Jsondocuments);

            if (failedDocuments != null && failedDocuments.Count > 0)
                throw new OperationFailedException("Operation failed with error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(failedDocuments.First().ErrorCode, failedDocuments.First().ErrorParameters));
        }

        public List<FailedDocument> ReplaceDocuments(ICollection<T> documents)
        {
            if (documents == null)
                throw new ArgumentException("documents");
            if (documents.Count < 1)
                throw new ArgumentException("No Document specified.");

            //JsonSerializer<T> serializer = new JsonSerializer<T>();
            List<JSONDocument> Jsondocuments = JsonSerializer.Serialize<T>(documents.ToList());

            return ReplaceDocuments(Jsondocuments);

        }


        internal List<FailedDocument> ReplaceDocuments(ICollection<JSONDocument> documents, bool noResponse = false)
        {         

            ReplaceDocumentsOperation replaceOperation = new ReplaceDocumentsOperation();
            replaceOperation.Documents = documents.Cast<IJSONDocument>().ToList();
            replaceOperation.Database = _database.DatabaseName;
            replaceOperation.Collection = _collectionName;
            replaceOperation.NoResponse = noResponse;

            ReplaceDocumentsResponse replaceResponse = (ReplaceDocumentsResponse)_database.ExecutionMapper.ReplaceDocuments(replaceOperation);

            if (!replaceResponse.IsSuccessfull)
            {
                if (replaceResponse.FailedDocumentsList == null || replaceResponse.FailedDocumentsList.Count == 0)
                {
                    throw new DataException(ErrorMessages.GetErrorMessage(replaceResponse.ErrorCode, replaceResponse.ErrorParams));
                }
                return replaceResponse.FailedDocumentsList;
            }
            return new List<FailedDocument>();

        }

        public void DeleteDocument(string documentKey)
        {
            if (documentKey == null)
                throw new ArgumentNullException("documentKey");

            List<string> documentKeys = new List<string>();
            documentKeys.Add(documentKey);

            List<FailedDocument> failedDocuments = DeleteDocuments(documentKeys);
            if (failedDocuments != null && failedDocuments.Count > 0)
                throw new OperationFailedException("Operation failed with error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(failedDocuments.First().ErrorCode, failedDocuments.First().ErrorParameters));
        }

        public List<FailedDocument> DeleteDocuments(ICollection<string> documentKeys)
        {
            if (documentKeys == null)
                throw new ArgumentNullException("documentKeys");
            if (documentKeys.Count < 1)
                throw new ArgumentException("No DocumentKey specified");

            List<JSONDocument> documents = new List<JSONDocument>();
            foreach (string documentKey in documentKeys)
            {
                if (documentKey != null)
                {
                    JSONDocument jdoc = new JSONDocument();
                    jdoc.Key = documentKey;
                    documents.Add(jdoc);
                }
                //else
                //    throw new ArgumentException("Document key cannot be an empty string or null");
            }

            DeleteDocumentsOperation deleteDocumentsOperation = new DeleteDocumentsOperation();
            deleteDocumentsOperation.Documents = documents.Cast<IJSONDocument>().ToList();
            deleteDocumentsOperation.Database = _database.DatabaseName;
            deleteDocumentsOperation.Collection = _collectionName;

            DeleteDocumentsResponse deleteDocumentsResponse = (DeleteDocumentsResponse)_database.ExecutionMapper.DeleteDocuments(deleteDocumentsOperation);

            if (!deleteDocumentsResponse.IsSuccessfull)
            {
                if (deleteDocumentsResponse.FailedDocumentsList == null || deleteDocumentsResponse.FailedDocumentsList.Count == 0)
                {
                    throw new DataException(ErrorMessages.GetErrorMessage(deleteDocumentsResponse.ErrorCode, deleteDocumentsResponse.ErrorParams));
                }
                return deleteDocumentsResponse.FailedDocumentsList;
            }
            return new List<FailedDocument>();
        }

        internal List<FailedDocument> DeleteDocuments(ICollection<string> documentKeys, bool noResponse)
        {
           
            List<JSONDocument> documents = new List<JSONDocument>();
            foreach (string documentKey in documentKeys)
            {
                if (documentKey != null)
                {
                    JSONDocument jdoc = new JSONDocument();
                    jdoc.Key = documentKey;
                    documents.Add(jdoc);
                }
                //else
                //    throw new ArgumentException("Document key cannot be an empty string or null");
            }

            DeleteDocumentsOperation deleteDocumentsOperation = new DeleteDocumentsOperation();
            deleteDocumentsOperation.Documents = documents.Cast<IJSONDocument>().ToList();
            deleteDocumentsOperation.Database = _database.DatabaseName;
            deleteDocumentsOperation.Collection = _collectionName;
            deleteDocumentsOperation.NoResponse = noResponse;

            DeleteDocumentsResponse deleteDocumentsResponse = (DeleteDocumentsResponse)_database.ExecutionMapper.DeleteDocuments(deleteDocumentsOperation);

            if (!deleteDocumentsResponse.IsSuccessfull)
            {
                if (deleteDocumentsResponse.FailedDocumentsList == null || deleteDocumentsResponse.FailedDocumentsList.Count == 0)
                {
                    throw new DataException(ErrorMessages.GetErrorMessage(deleteDocumentsResponse.ErrorCode, deleteDocumentsResponse.ErrorParams));
                }
                return deleteDocumentsResponse.FailedDocumentsList;
            }
            return new List<FailedDocument>();
        }


        public object ExecuteScalar(string queryText)
        {
            return this.ExecuteScalar(queryText, new List<IParameter>());
        }

        public object ExecuteScalar(string queryText, ICollection<IParameter> parameters)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentNullException("queryText can not be null or empty string");

            if (typeof(T) == typeof(IJSONDocument) || typeof(T) == typeof(JSONDocument))
            {
                Type type;
                if (!JsonDocumentUtil.IsSupportedParameterType(parameters, out type))
                {
                    throw new ArgumentException(string.Format("Type {0} is not supported on Collection<JSONDocument>", type), "parameters");
                }
            }

            Query query = new Query();
            query.QueryText = queryText;
            query.Parameters = parameters.Cast<IParameter>().ToList();

            ReadQueryOperation readQueryOperation = new ReadQueryOperation();
            readQueryOperation.Database = _database.DatabaseName;
            readQueryOperation.Collection = _collectionName;
            readQueryOperation.Query = query;

            return _database.ExecutionMapper.ExecuteScalar(readQueryOperation);
        }

        public ICollectionReader ExecuteReader(string queryText)
        {
            return this.ExecuteReader(queryText, new List<IParameter>());
        }

        public ICollectionReader ExecuteReader(string queryText, ICollection<IParameter> parameters)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentNullException("queryText can not be null or empty string");
            if (typeof(T) == typeof(IJSONDocument) || typeof(T) == typeof(JSONDocument))
            {
                Type type;
                if (!JsonDocumentUtil.IsSupportedParameterType(parameters, out type))
                {
                    throw new ArgumentException(string.Format("Type {0} is not supported on Collection<JSONDocument>", type), "parameters");
                }
            }

            Query query = new Query();
            query.QueryText = queryText;
            query.Parameters = parameters.Cast<IParameter>().ToList();

            ReadQueryOperation readQueryOperation = new ReadQueryOperation();
            readQueryOperation.Database = _database.DatabaseName;
            readQueryOperation.Collection = _collectionName;
            readQueryOperation.Query = query;


            ReadQueryResponse readQueryResponse = (ReadQueryResponse)_database.ExecutionMapper.ExecuteReader(readQueryOperation);
            if (!readQueryResponse.IsSuccessfull)
            {
                if (readQueryResponse.ErrorParams != null && readQueryResponse.ErrorParams.Length > 0)
                    throw new Exception(string.Format("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(readQueryResponse.ErrorCode, readQueryResponse.ErrorParams)));
                throw new Exception("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(readQueryResponse.ErrorCode));
            }

            CollectionReader reader = new CollectionReader((DataChunk)readQueryResponse.DataChunk, _database.ExecutionMapper, _database.DatabaseName, _collectionName);
            return reader;
        }

        public long ExecuteNonQuery(string queryText)
        {
            return this.ExecuteNonQuery(queryText, new List<IParameter>());
        }

        public long ExecuteNonQuery(string queryText, ICollection<IParameter> parameters)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentNullException("queryText can not be null or empty string");

            if (typeof(T) == typeof(IJSONDocument) || typeof(T) == typeof(JSONDocument))
            {
                Type type;
                if (!JsonDocumentUtil.IsSupportedParameterType(parameters, out type))
                {
                    throw new ArgumentException(string.Format("Type {0} is not supported on Collection<JSONDocument>", type), "parameters");
                }
            }

            Query query = new Query();
            query.QueryText = queryText;
            query.Parameters = (List<IParameter>)parameters;

            WriteQueryOperation writeQueryOperation = new WriteQueryOperation();
            writeQueryOperation.Database = _database.DatabaseName;
            writeQueryOperation.Collection = _collectionName;
            writeQueryOperation.Query = query;
            var writeQueryResponse = _database.ExecutionMapper.ExecuteNonQuery(writeQueryOperation);

            if (writeQueryResponse.IsSuccessfull)
            {
                return writeQueryResponse.AffectedDocuments;
            }

            if (writeQueryResponse.ErrorParams != null && writeQueryResponse.ErrorParams.Length > 0)
                throw new Exception(string.Format("Operation failed Error :" + Common.ErrorHandling.ErrorMessages.GetErrorMessage(writeQueryResponse.ErrorCode, writeQueryResponse.ErrorParams)));

            throw new Exception("Operation failed Error: " +
                Common.ErrorHandling.ErrorMessages.GetErrorMessage(writeQueryResponse.ErrorCode));
        }

       

        #endregion

        #region Import/Export
        /// <summary>
        /// Exports data against a specified collection, incase no query is specified entire collection data will be dumped to file
        /// </summary>
        /// <param name="database"></param>
        /// <param name="collection"></param>
        /// <param name="query"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RecoveryOperationStatus Export(string database, string collection, string query,ICollection<IParameter> parameters, string path,string filename, EXIMDataType dataType)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            try
            {
                //create query if not provided
                if (!string.IsNullOrEmpty(collection))
                {
                    if (string.IsNullOrEmpty(query))
                    {
                        if (!string.IsNullOrEmpty(collection))
                        {
                            query = "Select * from $" + collection + "$";
                            parameters = new List<IParameter>();
                        }                        
                    }
                    // query data from the collection
                    ICollectionReader reader = ExecuteReader(query, parameters);
                    
                    List<IJSONDocument> exportList = new List<IJSONDocument>();
                    while (reader.ReadNext())
                    {
                        exportList.Add(reader.GetObject<IJSONDocument>());
                    }
                    //write json documents to the file
                    state = Export(collection, exportList, path,filename,database, dataType);                    
                }
                else
                {                     
                    throw new ArgumentException("Invalid Collection provided");
                }
            }
            catch(Exception exp)
            {
                if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.EXIMLogger.Error("Export()",exp.ToString());
                state.Message = exp.ToString();
            }

            return state;
        }

        /// <summary>
        /// Exports IJSONDocuments to export file. 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="docList"></param>
        /// <param name="path"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public RecoveryOperationStatus Export(string collection, List<IJSONDocument> docList, string path,string fileName,string database, EXIMDataType dataType)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            EXIMBase eximBase = null;

            switch (dataType)
            {
                case EXIMDataType.CSV:
                    eximBase = new CSVEXIMUtil();

                    break;
                case EXIMDataType.JSON:
                    eximBase = new JSONEXIMUtil();
                    break;
            }

            if (eximBase != null)
            {
                state = eximBase.Write(dataType, path, collection,fileName,database, docList);
            }
            return state;
        }

        public RecoveryOperationStatus Import(string database, string collection, string path, bool updateMode, EXIMDataType dataType)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Success);

            EXIMBase eximBase = null;

            switch (dataType)
            {
                case EXIMDataType.CSV:
                    eximBase = new CSVEXIMUtil();

                    break;
                case EXIMDataType.JSON:
                    eximBase = new JSONEXIMUtil();
                    break;
            }

            if (eximBase != null)
            {
                try
                {
                    foreach (List<JSONDocument> docList in eximBase.Read(dataType, path))
                    {
                        if (docList != null)
                        {
                            if (docList.Count > 0)
                            {
                                // create insert operation if mode is update perform replace operation on 
                                List<FailedDocument> failedDoc = InsertDocuments(docList);


                                if (failedDoc != null)
                                {
                                    if (failedDoc.Count > 0)
                                    {
                                        if (updateMode)
                                        {
                                            List<JSONDocument> retryList = new List<JSONDocument>();
                                            foreach (FailedDocument failed in failedDoc)
                                            {
                                                foreach (JSONDocument orgDoc in docList)
                                                {
                                                    if (orgDoc.Key.Equals(failed.DocumentKey))
                                                    {
                                                        retryList.Add(orgDoc);
                                                    }
                                                }
                                            }

                                            failedDoc = ReplaceDocuments(retryList);
                                            if (failedDoc != null)
                                            {
                                                if (failedDoc.Count > 0)
                                                {
                                                    if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                                        LoggerManager.Instance.EXIMLogger.Error("Import()", failedDoc.Count + "failed to import in collection" + collection);
                                                    
                                                    state.Status = RecoveryStatus.Failure;
                                                    state.Message = failedDoc.Count + "failed to import in collection" + collection;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                                LoggerManager.Instance.EXIMLogger.Error("Import()", failedDoc.Count + "failed to import in collection" + collection);
                                            state.Status = RecoveryStatus.Failure;
                                            state.Message = failedDoc.Count + "failed to import in collection" + collection;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.EXIMLogger.Error("Import()", exp.ToString());
                    state.Status = RecoveryStatus.Failure;
                    state.Message = exp.Message.ToString() +" For details kindly review the log file";
                }
            }

            return state;
        }
        #endregion
        
        //Collection level Management Operations
        private void CreateIndex(string attribute, SortOrder sortOrder) { }

        private void CreateIndex(Hashtable attributes) { }

        private void CreateIndexAsync(string attribute, SortOrder sorOrder, IndexCreationCallBack callBack) { }

        private void CreateIndexAsync(Hashtable attributes, IndexCreationCallBack callBack) { }

        private bool IndexExist(string attribute) { return false; }

        private bool IndexExist(HashSet<string> attributes) { return false; }

        private HashSet<string> GetAllIndexes() { return null; }

        private void DropIndex(string attribute) { }

        private void DropIndex(HashSet<string> attributes) { }

        private void DropAllIndexes() { }

        private void DropIndexAsync(string attribute, IndexDroppedCallBack callBack) { }

        private void DropIndexAsync(HashSet<string> attributes, IndexDroppedCallBack callBack) { }

        private void DropAllIndexesAsync(IndexDroppedCallBack callBack) { }


      
    }
}
