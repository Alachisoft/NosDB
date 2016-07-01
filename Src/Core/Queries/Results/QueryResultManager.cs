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
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using System;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public class QueryResultManager : IDisposable
    {
        private HashVector _queryResults = HashVector.Synchronized(new HashVector());
        private bool _disposed;

        public void Add(string id, IQueryResult queryResult)
        {
            if (_disposed)
                throw new Exception("Database Disposed");


            if (_queryResults.Contains(id))
                throw new Exception("Reader already Present");

            _queryResults[id] = queryResult;

        }

        public IQueryResult Get(string readerId)
        {
            if (_disposed)
                throw new Exception("Database Disposed");

            if (_queryResults.Contains(readerId))
                return _queryResults[readerId] as IQueryResult;

            throw new Exception("Reader ID not Present");
        }

        public IQueryResult Remove(string readerId)
        {
            if (!_queryResults.Contains(readerId)) return null;
            var result = _queryResults[readerId];
            _queryResults.Remove(readerId);
            return result as IQueryResult;
        }

        public ICollection GetReadersList()
        {
            return _queryResults.Keys;
        }

        public void Dispose()
        {
            _disposed = true;
            var keys = new object[_queryResults.Count];
            _queryResults.Keys.CopyTo(keys,0);
            foreach (var key in keys)
            {
                var result = Remove((string) key);
                if (result != null)
                    result.Dispose();
            }
        }
    }
}
