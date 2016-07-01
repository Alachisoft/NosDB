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
using System.Collections.Generic;
using System.Linq;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

namespace Alachisoft.NosDB.Distributor.DistributedDataSets
{
    /// <summary>
    /// Each Shard returns some or all of data chunks available against a query
    /// QueryRouter only sends required number of documents to the client
    /// This class stores the excessive data chunks returned by the shard
    /// </summary>
    public class DistributedDataSet : ISet
    {
        private string _shardName;
        private bool _isExhausted;
        IDataChunk _dataChunk;
        bool _isFixedSize;  // tells if this current datachunk is fully comsumed or not
        IDBOperation _operation;
        IDataLoader _dataLoader;
        private Server _readerAddress;


        public DistributedDataSet(IDataChunk dataChunk, string shardName, IDBOperation operation, IDataLoader dataLoader, Server readerLocationAddress)
        {
            _shardName = shardName;
            _dataChunk = dataChunk;
            _isFixedSize = dataChunk.IsLastChunk;
            _isExhausted = false;
            _operation = operation;
            _dataLoader = dataLoader;
            _readerAddress = readerLocationAddress;
        }

        public bool IsLastChunk
        {
            get { return _dataChunk.IsLastChunk; }
        }

        public Server ReaderAddress
        {
            get { return _readerAddress; }
            set { _readerAddress = value; }
        }

        public bool IsExhausted
        {
            get { return _isExhausted; }
        }

        public string ReaderUID
        {
            get { return _dataChunk.ReaderUID; }
        }

        public IDataChunk DataChunk
        {
            get { return _dataChunk; }
        }

        public IDBOperation Operation
        {
            get { return _operation; }
        }

        private void InsertDataChunk(IDataChunk dataChunk)
        {
            foreach (IJSONDocument jDoc in dataChunk.Documents)
                _dataChunk.Documents.Add(jDoc);

            _dataChunk.ChunkId = dataChunk.ChunkId;
            _dataChunk.IsLastChunk = dataChunk.IsLastChunk;
        }

        internal IList<IJSONDocument> GetDocuments(int numOfRequiredDocuments)
        {
            IList<IJSONDocument> jDocs = new List<IJSONDocument>();
            int counter = 0;
            foreach (IJSONDocument jDoc in _dataChunk.Documents)
            {
                if (counter == numOfRequiredDocuments)
                    break;

                jDocs.Add(jDoc);
                counter++;
            }

            //////////////////////////// Remove Items ////////////////////////////////////////
            var set = new HashSet<IJSONDocument>(jDocs);

            var list = _dataChunk.Documents as List<IJSONDocument>;
            if (list == null)
            {
                int i = 0;
                while (i < _dataChunk.Documents.Count)
                {
                    if (set.Contains(_dataChunk.Documents[i])) _dataChunk.Documents.RemoveAt(i);
                    else i++;
                }
            }
            else
            {
                list.RemoveAll(set.Contains);
            }
            /////////////////////////// #End Remove Items ////////////////////////////////////
            if (_dataChunk.Documents.Count == 0 && _dataChunk.IsLastChunk) { _isExhausted = true;  }
            return jDocs;
        }

        public string ShardName
        {
            get { return _shardName; }
        }

        public void Load()
        {
            IDataChunk dataChunk = _dataLoader.LoadData(this);
            InsertDataChunk(dataChunk);
        }

        public ISetElement GetTopElement()
        {
            if (_dataChunk.Documents.Count <= 0) return null;
            IJSONDocument doc = _dataChunk.Documents.First();
            ISetElement setElement = new SetElement(this, _dataChunk.IsLastChunk, doc);
            return setElement;
        }

        public ISetElement GetNextElement(ISetElement currentElelemt)
        {
            IJSONDocument doc = _dataChunk.Documents.Skip(1).First();
            ISetElement setElement = new SetElement(this, _dataChunk.IsLastChunk, doc);
            return setElement;
        }

        public bool IsFixedSize
        {
            get { return _isFixedSize; }
        }

        public ISetElement DeleteTopElement()
        {
            IJSONDocument doc = _dataChunk.Documents.First();
            _dataChunk.Documents.Remove(doc);
            _isFixedSize = _dataChunk.IsLastChunk;
            bool isLastElement = false;
            if (_dataChunk.Documents.Count == 0 )
            {
                isLastElement = true;
                if (_dataChunk.IsLastChunk)
                {
                    _isFixedSize = true;
                }
            }
            ISetElement setElement = new SetElement(this, isLastElement, doc);
            return setElement;
        }


        public void DisposeReader()
        {
            _dataLoader.DisposeSetReader(this);
        }
    }
}
