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
using Alachisoft.NosDB.Common.JSON.Indexing;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public class ResultWrapper<T> : IComparable<ResultWrapper<T>>, IComparable
    {
        private T _reference;

        public ResultWrapper(T reference)
        {
            _reference = reference;
        }

        public AttributeValue HashField { get; set; }
        public AttributeValue SortField { get; set; }

        public T Value
        {
            get { return _reference; }
            set { _reference = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj is ResultWrapper<T>)
            {
                ResultWrapper<T> target = (ResultWrapper<T>) obj;
                if (SortField != null)
                {
                    return SortField.Equals(target.SortField);
                }
                return _reference.Equals(target._reference);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (HashField != null) return HashField.GetHashCode();
            return _reference.GetHashCode();
        }


        public int CompareTo(object obj)
        {
            var wrapper = obj as ResultWrapper<T>;
            if (wrapper != null)
                return CompareTo(wrapper);
            throw new ArgumentException();
        }

        public int CompareTo(ResultWrapper<T> other)
        {
            if (SortField != null)
            {
                if (other.SortField != null)
                    return SortField.CompareTo(other.SortField);
                return -1;
            }
            if (other.SortField != null)
                return 1;
            if (_reference.Equals(other._reference))
                return 0;
            return 1;
        }

        public virtual ResultWrapperList<T> ToList()
        {
            return new ResultWrapperList<T>(this);
        } 
    }


}
