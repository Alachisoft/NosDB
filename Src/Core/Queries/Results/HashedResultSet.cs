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
using Alachisoft.NosDB.Common.Queries.Results;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public class HashedResultSet<T> : IResultSet<T>
    {
        private C5.HashSet<T> _results;

        public HashedResultSet()
        {
            _results = new C5.HashSet<T>(new HashEqualityComparer<T>());
        }

        public HashedResultSet(IEnumerator<T> inData)
            : this()
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
            HashedResultSet<T> target = new HashedResultSet<T>();
            target._results = new C5.HashSet<T>();
            foreach (var value in _results)
                target._results.Add(value);
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
            get { return ResultType.Hashed; }
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
                return default(T);
            }
            set
            {
                if (!_results.Contains(reference))
                    _results.Add(value);
                else _results.Update(value);
            }
        }


        public bool Contains(T value)
        {
            return _results.Contains(value);
        }

        public void Dispose()
        {
            _results.Clear();
            _results = null;
        }
    }
}
