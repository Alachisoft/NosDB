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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Storage.Caching.Evictions
{
    internal class EvictionIndexEntry : ISizable
    {
        private HashVector _hintIndex = new HashVector();
        private long _previous = -1;
        private long _next = -1;

        internal long Previous
        {
            get { return _previous; }
            set { _previous = value; }
        }

        internal long Next
        {
            get { return _next; }
            set { _next = value; }
        }

        internal void Insert(object key)
        {
            lock (this)
            {
                _hintIndex[key] = null;
            }
        }

        internal bool Remove(object key)
        {
            lock (this)
            {
                if (_hintIndex != null)
                {
                    _hintIndex.Remove(key);
                }
                return _hintIndex.Count == 0;
            }
        }

        internal IList GetAllKeys()
        {
            lock (this)
            {
                return new ClusteredArrayList(_hintIndex.Keys);
            }
        }

        internal bool Contains(object key)
        {
            return _hintIndex.Contains(key);
        }

        #region ISizable Impelementation
        public int Size { get { return EvictionIndexEntrySize; } }

        public int InMemorySize { get { return this.Size; } }

        private int EvictionIndexEntrySize
        {
            get
            {
                int temp = 0;
                temp += MemoryUtil.NetReferenceSize;   //for _hintIndex
                temp += MemoryUtil.NetLongSize;       //for _previous
                temp += MemoryUtil.NetLongSize;      //for _next

                temp += _hintIndex.BucketCount * MemoryUtil.NetHashtableOverHead; //per item overhead of hashtable+bucket         

                return temp;
            }
        }
        #endregion

    }
}