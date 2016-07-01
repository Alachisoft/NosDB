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
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using System;

namespace Alachisoft.NosDB.Common
{
    public class CollectionReader : ICollectionReader
    {
        //M_Note: moved to common due to utility for both client and server.
        //in place of Database instance used previously in client, IStore and DatabaseName properties introduced
        
        private IDataChunk _dataChunk;
        private int _documentIndex = -1;
        private string _collectionName;
        private bool _isFirstChunk = true;
        private IStore _store;
        private string _databaseName;

        #region properties
        public IStore Store
        {
            get { return _store; }
            set { _store = value; }
        }


        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }
       

        public string CollectionName
        {
            set { _collectionName = value; }
        }

        public IJSONDocument GetDocument()
        {   
            if (_dataChunk.Documents.Count < 1)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return _dataChunk.Documents[_documentIndex < 0 ? 0 : _documentIndex]; 
        }

        public T GetObject<T>()
        {
            if (_dataChunk.Documents.Count < 1)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            if (typeof(T) == typeof(JSONDocument) || typeof(T) == typeof(IJSONDocument))
                return (T)GetDocument();
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return _dataChunk.Documents[_documentIndex < 0 ? 0 : _documentIndex].Parse<T>();
        }

        public object this[string attribute]
        {
            get 
            {
                if (_dataChunk.Documents.Count < 1)
                    throw new InvalidOperationException("Invalid attempt to read when no data is present.");
                if (_documentIndex < 0)
                    throw new InvalidOperationException("Invalid attempt to read when no data is present.");
                return (_dataChunk.Documents[_documentIndex])[attribute]; 
            }
        }
        #endregion

        public CollectionReader(IDataChunk dataChunk, IStore store,string databaseName, string collectionName)
        {
            _dataChunk = dataChunk;

            if (store != null)
                _store = store;
            else
                throw new ArgumentNullException("store");
            
            _databaseName = databaseName;
            _collectionName = collectionName;
        }

        #region public methods
        public bool ReadNext()
        {
            if (_dataChunk.IsLastChunk) return ++_documentIndex < _dataChunk.Documents.Count;
            if (++_documentIndex < _dataChunk.Documents.Count)
                return true;
            GetNextChunk();
            _documentIndex = 0;
            return true;
        }

        //public bool IsClosed()
        //{
        //    return _disposed || _documentIndex >= _dataChunk.Documents.Count;
        //}

        public void GetNextChunk()
        {
            _isFirstChunk = false;
            GetChunkOperation getChunkOperation = new GetChunkOperation();
            getChunkOperation.Database = _databaseName;//_database.DatabaseName
            getChunkOperation.Collection = _collectionName;
            getChunkOperation.ReaderUID = _dataChunk.ReaderUID;
            getChunkOperation.LastChunkId = _dataChunk.ChunkId;
            getChunkOperation.QueryString = _dataChunk.QueryString;
            getChunkOperation.DoCaching = _dataChunk.DoCaching;

            // GetChunkResponse getChunkResponse = (GetChunkResponse)_database.ExecutionMapper.GetDataChunk(getChunkOperation);
            GetChunkResponse getChunkResponse = (GetChunkResponse)_store.GetDataChunk(getChunkOperation);
            if (!getChunkResponse.IsSuccessfull)
                throw new Exception("Operation failed Error Code " + getChunkResponse.ErrorCode);
            _dataChunk = (DataChunk)getChunkResponse.DataChunk;
        }

        public IJSONDocument GetDocument(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return _dataChunk.Documents[_documentIndex].GetDocument(attribute);
        }

        public short GetInt16(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetAsInt16(attribute);
        }

        public int GetInt32(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetAsInt32(attribute);
        }
        
        public long GetInt64(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetAsInt64(attribute);
        }

        public double GetDouble(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetAsDouble(attribute);
        }
        
        public decimal GetDecimal(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetAsDecimal(attribute);
        }

        public string GetString(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetString(attribute);
        }

        public bool GetBoolean(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetBoolean(attribute);
        }

        public T[] GetArray<T>(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).GetArray<T>(attribute);
        }

        public ExtendedJSONDataTypes GetAttributeDataType(string attribute)
        {
            if (_documentIndex < 0)
                return (_dataChunk.Documents[0]).GetAttributeDataType(attribute);
            return (_dataChunk.Documents[_documentIndex]).GetAttributeDataType(attribute);
        }

        public T Get<T>(string attribute)
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return (_dataChunk.Documents[_documentIndex]).Get<T>(attribute);
        }

        public bool ContainsAttribute(string attribute)
        {
            if (_documentIndex < 0)
                return (_dataChunk.Documents[0]).Contains(attribute);
            return (_dataChunk.Documents[_documentIndex]).Contains(attribute);
        }

        #endregion


        public bool HasRows
        {
            get { return _dataChunk.Documents.Count > (_documentIndex + 1); }
        }

        public void Dispose()
        {
            if (_isFirstChunk && _dataChunk.IsLastChunk)
                return;

            var disposeReaderOperation = new DisposeReaderOperation();
            disposeReaderOperation.Database = _databaseName;//_database.DatabaseName
            disposeReaderOperation.Collection = _collectionName;
            disposeReaderOperation.ReaderUID = _dataChunk.ReaderUID;

            //DatabaseResponse response = (DatabaseResponse)_database.ExecutionMapper.DiposeReader(disposeReaderOperation);
            var response = (DatabaseResponse)_store.DiposeReader(disposeReaderOperation);
            if (!response.IsSuccessfull)
                throw new Exception("Operation Failed Error Code " + response.ErrorCode);
        }


        public System.Collections.Generic.ICollection<string> GetAttributes()
        {
            if (_documentIndex < 0)
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            return _dataChunk.Documents[_documentIndex].GetAttributes();
        }
    }
}
