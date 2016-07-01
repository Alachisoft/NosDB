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
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public class ListedResultSet<T> : IResultSet<T>, ICompactSerializable
    {
        private ClusteredList<T> _results;

        public ListedResultSet()
        {
            _results = new ClusteredList<T>();
        }

        public ListedResultSet(IEnumerator<T> inData)
            : this()
        {
            Populate(inData);
        }

        public void Add(T result)
        {
            _results.Add(result);
        }

        public void Remove(T result)
        {
            _results.Remove(result);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        public int Count
        {
            get { return _results.Count; }
        }

        public object Clone()
        {
            ListedResultSet<T> target = new ListedResultSet<T>();
            target._results = new ClusteredList<T>(_results);
            return target;
        }

        public void Populate(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                _results.Add(enumerator.Current);
        }

        public void Clear()
        {
            _results.Clear();
        }
        
        public ResultType ResultType
        {
            get { return ResultType.Listed; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _results.GetEnumerator();
        }


        public T this[T reference]
        {
            get
            {
                int index;
                if ((index = _results.IndexOf(reference)) > -1)
                    return _results[index];
                return default(T);
            }
            set
            {
                int index;
                if ((index = _results.IndexOf(reference)) > -1)
                    _results[index] = value;
                else _results.Add(value);
            }
        }


        public bool Contains(T value)
        {
            return _results.Contains(value);
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _results = SerializationUtility.DeserializeClusteredList<T>(reader);
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeClusteredList(_results, writer);
        }

        public void Dispose()
        {
            _results.Clear();
            _results = null;
        }
    }
}
