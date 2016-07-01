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
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Results.Transforms;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public class GroupResult : QueryResult
    {
        private bool isDisposing = false;

        public GroupResult(IEnumerable<long> resultSet, IQueryStore source, IDataTransform dataTransform, IOperationContext context, long queryId)
            : base(resultSet, source, dataTransform, context, queryId)
        {
        }

        protected override bool FillNewData(int lastChunkId, ref IDataChunk dataChunk)
        {
            foreach (var rowId in _lastChunkData)
            {
                _store.DeleteDocument(rowId, _context);
            }
            return base.FillNewData(lastChunkId, ref dataChunk);
        }

        public override void Dispose()
        {
            if (!isDisposing)
            {
                isDisposing = true;
                foreach (var rowId in _lastChunkData)
                {
                    _store.DeleteDocument(rowId, _context);
                }
                base.Dispose();
            }
        }

        ~GroupResult()
        {
            if(!isDisposing)
                Dispose();
        }

        #region Old Implementation
        //private IDataTransform _transform;
        //private string _id;
        //private int _chunkSize;
        //private IEnumerator<long> _enumerator;
        //IJSONDocument _lastElement;
        //bool _lastElementCheck;
        //private int _lastChunkId;
        //private List<IJSONDocument> _lastChunkData;
        //private bool _isLastChunk;

        //public GroupResult(IEnumerable<long> resultSet, IDataTransform dataTransform)
        //{
        //    _lastElementCheck = false;
        //    _transform = dataTransform;
        //    _enumerator = resultSet.GetEnumerator();
        //    _id = Guid.NewGuid().ToString();
        //    _lastChunkId = 0;
        //    _lastChunkData = new List<IJSONDocument>();
        //    _isLastChunk = false;
        //    _chunkSize = 100;                          /////////////Number of Records in a Chunk Assign Temporarily Configured By Client
        //}

        //public string Id
        //{
        //    get { return _id; }
        //    set { _id = value; }
        //}

        //public int ChunkSize
        //{
        //    get { return _chunkSize; }
        //    set { _chunkSize = value; }
        //}

        //public IDataTransform Transformation
        //{
        //    get { return _transform; }
        //    set { _transform = value; }
        //}

        //public bool FillDataChunk(int lastChunkId, ref IDataChunk dataChunk)
        //{
        //    bool check = _lastChunkId == lastChunkId ? FillNewData(lastChunkId, ref dataChunk) : FillPreviousData(lastChunkId, ref dataChunk);

        //    return check;
        //}

        ///// <summary>
        ///// Get New Data according to chunk size From Store and Fill data Chunk
        ///// Stores New Data Locally also in case chunk not recieve by user next time can send from local
        ///// </summary>
        ///// <param name="lastChunkId"></param>
        ///// <param name="dataChunk"></param>
        //private bool FillNewData(int lastChunkId, ref IDataChunk dataChunk)
        //{
        //    int count = 0;
        //    bool check = false;
        //    _lastChunkData.Clear();

        //    if (_lastElementCheck)
        //    {
        //        if (_lastElement != null)
        //        {
        //            IJSONDocument document = _transform.Transform(_lastElement);
        //            dataChunk.Documents.Add(document);
        //            _lastChunkData.Add(document);
        //            count++;
        //        }
        //        _lastElementCheck = false;
        //    }

        //    while (_enumerator.MoveNext())
        //    {
        //        if (check)
        //        {
        //            check = false;
        //            _lastElement = _enumerator.Current;
        //            _lastElementCheck = true;
        //            break;
        //        }

        //        if (_enumerator.Current != null)
        //        {
        //            IJSONDocument document = _transform.Transform(_enumerator.Current);
        //            dataChunk.Documents.Add(document);
        //            _lastChunkData.Add(document);
        //            count++;
        //        }
        //        if (count == _chunkSize)
        //            check = true;
        //    }

        //    if (count < _chunkSize || check)
        //    {
        //        dataChunk.IsLastChunk = true;
        //        _isLastChunk = true;
        //    }
        //    else
        //    {
        //        dataChunk.IsLastChunk = false;
                
        //    }

        //    dataChunk.ReaderUID = _id;
        //    dataChunk.ChunkId = lastChunkId + 1;
        //    _lastChunkId = lastChunkId + 1;

        //    return true;
        //}
        ///// <summary>
        ///// if last chunk not recieved by client simply send the stored data again
        ///// </summary>
        ///// <param name="lastChunkId"></param>
        ///// <param name="dataChunk"></param>
        //private bool FillPreviousData(int lastChunkId, ref IDataChunk dataChunk)
        //{
        //    foreach (IJSONDocument document in _lastChunkData)
        //    {
        //        dataChunk.Documents.Add(document);
        //    }

        //    dataChunk.IsLastChunk = _isLastChunk;

        //    dataChunk.ChunkId = lastChunkId + 1;
        //    _lastChunkId = lastChunkId + 1;
        //    dataChunk.ReaderUID = _id;

        //    return true;
        //}

        //public void Dispose()
        //{
        //    _enumerator.Dispose();
        //    _lastChunkData.Clear();
        //}
        #endregion
    }
}
