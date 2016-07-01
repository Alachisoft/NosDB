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
using Alachisoft.NosDB.Common.Enum;
using C5;

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public class SortedResultSet<T> : IResultSet<T>
    {
        private TreeSet<T> _results;

        public SortedResultSet()
        {
            _results = new TreeSet<T>();
        }

        public SortedResultSet(IComparer<T> comparer)
        {
            _results = new TreeSet<T>(comparer);
        } 

        public SortedResultSet(IEnumerator<T> inData)
            : this()
        {
            Populate(inData);
        }

        public SortedResultSet(IEnumerator<T> inData, IComparer<T> comparer)
            : this(comparer)
        {
            Populate(inData);
        }

        public void Add(T result)
        {
            if (!_results.Contains(result))
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
            SortedResultSet<T> target = new SortedResultSet<T>();
            target._results = new TreeSet<T>(_results.Comparer);
            foreach (var value in target)
            {
                target._results.Add(value);
            }
            return target;
        }


        public void Populate(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                if (!_results.Contains(enumerator.Current))
                    _results.Add(enumerator.Current);
            }
        }

        public void Clear()
        {
            _results.Clear();
        }

        public ResultType ResultType
        {
            get { return ResultType.Sorted; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _results.GetEnumerator();
        }


        public T this[T reference]
        {
            get
            {
                if (_results.Find(ref reference))
                    return reference;
                return default (T);
            }
            set
            {
                if (!_results.Contains(reference))
                    _results.Add(value);
                else
                    _results.Update(value);
            }
        }
        
        public bool Contains(T value)
        {
            return _results.Contains(value);
        }

        public void Dispose()
        {
            _results.Clear();
            _results.Dispose();
            _results = null;
        }
    }
}
