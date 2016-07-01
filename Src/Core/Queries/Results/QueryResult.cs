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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Results.Transforms;
using Alachisoft.NosDB.Common.Server.Engine;
using System;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Toplogies.Impl;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public class QueryResult : IQueryResult
    {
        protected IQueryStore _store;
        protected IDataTransform _transform;
        protected string _id;
        protected int _chunkSize;
        private IEnumerator<long> _enumerator;
        protected bool _lastElementCheck;
        protected long _lastElement;
        protected int _lastChunkId;
        protected List<long> _lastChunkData;
        protected bool _isLastChunk;
        protected IOperationContext _context;
        protected long _queryId;

        public QueryResult(IEnumerable<long> resultSet, IQueryStore source, IDataTransform dataTransform, IOperationContext context, long queryId)
        {
            _store = source;
            _lastElementCheck = false;
            _id = Guid.NewGuid().ToString();
            _transform = dataTransform;
            _enumerator = resultSet.GetEnumerator();
            _lastChunkId = 0;
            _lastChunkData = new List<long>();        ///////////////store last chunk data sent to user
            _isLastChunk = false;
            _chunkSize = 100;                          /////////////Number of Records Assign Temporarily Configured By Client
            _context = context;
            _queryId = queryId;
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int ChunkSize
        {
            get { return _chunkSize; }
            set { _chunkSize = value; }
        }

        public IDataTransform Transformation
        {
            get { return _transform; }
            set { _transform = value; }
        }

        public IEnumerator<long> Enumerator
        {
            get { return _enumerator; }
        }

        public bool FillDataChunk(int lastChunkId, ref IDataChunk dataChunk)
        {
            UsageStats stats = new UsageStats();
            stats.BeginSample();
            bool check = _lastChunkId == lastChunkId ? FillNewData(lastChunkId, ref dataChunk) : FillPreviousData(lastChunkId, ref dataChunk);
            stats.EndSample();
            //if (LoggerManager.Instance.QueryLogger != null)
            //    LoggerManager.Instance.QueryLogger.Debug("GetNextChunk", "QueryID: "+_queryId+
            //        ", Refilling last chunk: " + check + ", Time taken: " + stats.Current);

            return check;
        }

        /// <summary>
        /// Get New Data according to chunk size From Store and Fill data Chunk
        /// Stores New Data Locally also in case chunk not recieve by user next time can send from local
        /// </summary>
        /// <param name="lastChunkId"></param>
        /// <param name="dataChunk"></param>
        protected virtual bool FillNewData(int lastChunkId, ref IDataChunk dataChunk)
        {
            int count = 0;
            bool check = false;
            _lastChunkData.Clear();

            if (_lastElementCheck)
            {

                IJSONDocument jsonDocument = _store.GetDocument(_lastElement, _context) ;
                if (jsonDocument != null)
                {
                    jsonDocument = _transform.Transform(jsonDocument);
                    if (jsonDocument != null)
                    {
                        dataChunk.Documents.Add(jsonDocument);
                        _lastChunkData.Add(_lastElement);
                        count++;
                    }
                }

                _lastElementCheck = false;
            }

            while (_enumerator.MoveNext())
            {
                if (check)
                {
                    check = false;
                    _lastElement = _enumerator.Current;           ////To Save Round Trip in case Data Chunk fills and Data also finish so that can make the existing chunk as last chunk
                    _lastElementCheck = true;
                    break;
                }

                IJSONDocument jsonDocument = _store.GetDocument(_enumerator.Current, _context) ;
                if (jsonDocument != null)
                {
                    jsonDocument = _transform.Transform(jsonDocument);
                    if (jsonDocument != null)
                    {
                        dataChunk.Documents.Add(jsonDocument);
                        _lastChunkData.Add(_enumerator.Current);
                        count++;
                    }
                }
                
                if (count == _chunkSize)
                    check = true;
            }

            if (count < _chunkSize || check)
            {
                dataChunk.IsLastChunk = true;
                _isLastChunk = true;
            }
            else
            {
                dataChunk.IsLastChunk = false;
            }

            dataChunk.ReaderUID = _id;
            dataChunk.ChunkId = lastChunkId + 1;
            _lastChunkId = lastChunkId + 1;

            return true;
        }

        /// <summary>
        /// if last chunk not recieved by client simply send the stored data again
        /// </summary>
        /// <param name="lastChunkId"></param>
        /// <param name="dataChunk"></param>
        protected bool FillPreviousData(int lastChunkId, ref IDataChunk dataChunk)
        {
            foreach (long rowId in _lastChunkData)
            {
                IJSONDocument jsonDocument = _store.GetDocument(rowId, _context);
                if (jsonDocument != null)
                {
                    jsonDocument = _transform.Transform(jsonDocument);
                    if (jsonDocument != null)
                        dataChunk.Documents.Add(jsonDocument);
                }
            }

            dataChunk.IsLastChunk = _isLastChunk;

            dataChunk.ChunkId = lastChunkId + 1;
            _lastChunkId = lastChunkId + 1;
            dataChunk.ReaderUID = _id;

            return true;
        }

       
        public virtual void Dispose()
        {
            _enumerator.Dispose();
            _lastChunkData.Clear();
        }

        public IQueryStore Store
        {
            get { return _store; }
        }
    }
}
