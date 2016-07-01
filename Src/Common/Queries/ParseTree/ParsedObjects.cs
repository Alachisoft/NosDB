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
using Alachisoft.NosDB.Common.Queries;

namespace Alachisoft.NosDB.Common.Queries.ParseTree
{
    public class ParsedObjects : IDqlObject, IList<IDqlObject>
    {
        private readonly List<IDqlObject> _queryList = new List<IDqlObject>();
        
        public int IndexOf(IDqlObject item)
        {
            return _queryList.IndexOf(item);
        }

        public void Insert(int index, IDqlObject item)
        {
            _queryList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _queryList.RemoveAt(index);
        }

        public IDqlObject this[int index]
        {
            get { return _queryList[index]; }
            set { _queryList[index] = value; }
        }

        public void Add(IDqlObject item)
        {
            _queryList.Add(item);
        }

        //Parser resolves argument in right to left last, secondLast, 3rdLast,... first
        public void AddFirst(IDqlObject item)
        {
            _queryList.Insert(0, item);
        }

        public void Clear()
        {
            _queryList.Clear();
        }

        public bool Contains(IDqlObject item)
        {
            return _queryList.Contains(item);
        }

        public void CopyTo(IDqlObject[] array, int arrayIndex)
        {
            _queryList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _queryList.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(IDqlObject item)
        {
            return _queryList.Remove(item);
        }

        public IEnumerator<IDqlObject> GetEnumerator()
        {
            return _queryList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
