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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using System;
using System.Collections.Generic;
using Alachisoft.NosDB.Client.Exceptions;
using Alachisoft.NosDB.Distributor;

namespace Alachisoft.NosDB.Client
{
    public class Database : IDisposable
    {
        private IResource _resource;
        private string _databaseName;
        private IDatabaseEngine _executionMapper;

        internal Database(IResource resource)
        {
            _resource = resource;
        }

        #region Properties
        internal IDatabaseEngine ExecutionMapper
        {
            get { return _executionMapper; }
            set { _executionMapper = value; }
        }

        internal String DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }
        #endregion

        #region Get Collection Operations

        public Collection<JSONDocument> GetCollection(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                throw new ArgumentException("Value cannot be null or empty string.", "collectionName");

            collectionName = collectionName.ToLower().Trim();
            if (!((DistributorDatabaseEngine)_executionMapper).CollectionExists(DatabaseName, collectionName))
                throw new OperationFailedException("Collection '" + collectionName + "' does not exist");

            Collection<JSONDocument> collection = new Collection<JSONDocument>();
            collection.Name = collectionName;
            collection.Database = this;
            return collection;
        }

        public Collection<T> GetCollection<T>(string collectionName)
        {
            if (String.IsNullOrEmpty(collectionName))
                throw new ArgumentException("value can not be null or empty string.", "collectionName");

            collectionName = collectionName.ToLower().Trim();
            if (!((DistributorDatabaseEngine)_executionMapper).CollectionExists(DatabaseName, collectionName))
                throw new OperationFailedException("Collection '" + collectionName + "' does not exist");

            Collection<T> collection = new Collection<T>();
            collection.Name = collectionName;
            collection.Database = this;
            return collection;
        }

        #endregion

        #region Query Operations

        public ICollectionReader ExecuteReader(string queryText)
        {
            return ExecuteReader(queryText, new List<IParameter>());
        }

        public ICollectionReader ExecuteReader(string queryText, ICollection<IParameter> parameters)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentException("value can not be null or empty string.", "queryText");

            Query query = new Query();
            query.QueryText = queryText;
            query.Parameters = (List<IParameter>)parameters;

            ReadQueryOperation readQueryOperation = new ReadQueryOperation();
            readQueryOperation.Database = _databaseName;
            //Collection Name Cannot be null(Protobuf)
            readQueryOperation.Collection = "";
            readQueryOperation.Query = query;

      
            ReadQueryResponse readQueryResponse = (ReadQueryResponse)this.ExecutionMapper.ExecuteReader(readQueryOperation);

            if (readQueryResponse.IsSuccessfull)
            {
                //TODO ReadQueryResponse must have Collection Name or server must share collection name becuse it is needed for 
                // GetNextChunk and CloseDataChunk operations
                CollectionReader reader = new CollectionReader((DataChunk)readQueryResponse.DataChunk, this.ExecutionMapper, this.DatabaseName, readQueryOperation.Collection);

                return reader;
            }
            else

                if (readQueryResponse.ErrorParams != null && readQueryResponse.ErrorParams.Length > 0)
                    throw new Exception(string.Format("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(readQueryResponse.ErrorCode, readQueryResponse.ErrorParams)));
            throw new Exception("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(readQueryResponse.ErrorCode));
        }

        public object ExecuteScalar(string queryText)
        {
            return this.ExecuteScalar(queryText, new List<IParameter>());
        }

        public object ExecuteScalar(string queryText, ICollection<IParameter> parameters)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentException("Value can not be null or empty string.", "queryText");

            Query query = new Query();
            query.QueryText = queryText;
            query.Parameters = (List<IParameter>)parameters;

            ReadQueryOperation readQueryOperation = new ReadQueryOperation();
            readQueryOperation.Database = _databaseName;
            //Collection Name Cannot be null(Protobuf)
            readQueryOperation.Collection = "";
            readQueryOperation.Query = query;

            return this.ExecutionMapper.ExecuteScalar(readQueryOperation);
        }

        public long ExecuteNonQuery(string queryText)
        {
            return this.ExecuteNonQuery(queryText, new List<IParameter>());
        }

        public long ExecuteNonQuery(string queryText, ICollection<IParameter> parameters)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentException("Value can not be null or empty string.", "queryText");

            Query query = new Query();
            query.QueryText = queryText;
            query.Parameters = (List<IParameter>)parameters;

            WriteQueryOperation writeQueryOperation = new WriteQueryOperation();
            writeQueryOperation.Database = _databaseName;
            //Collection Name Cannot be null(Protobuf)            
            writeQueryOperation.Collection = "";
            writeQueryOperation.Query = query;

            var writeQueryResponse = this.ExecutionMapper.ExecuteNonQuery(writeQueryOperation);

            if (writeQueryResponse.IsSuccessfull)
            {
                return writeQueryResponse.AffectedDocuments;
            }
            else
                if (writeQueryResponse.ErrorParams != null && writeQueryResponse.ErrorParams.Length > 0)
                    throw new Exception(string.Format("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(writeQueryResponse.ErrorCode, writeQueryResponse.ErrorParams)));
            throw new Exception("Operation failed Error: " + Common.ErrorHandling.ErrorMessages.GetErrorMessage(writeQueryResponse.ErrorCode));
        }

        #endregion

        public void Dispose()
        {
            if(_resource != null ) _resource.FreeResource();
            //if (_executionMapper != null)
            //    _executionMapper.Dispose();
        }
    }
}
