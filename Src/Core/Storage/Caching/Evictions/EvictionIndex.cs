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
using System.Text;
using System.Collections;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Storage.Caching.Evictions
{
    internal class EvictionIndex : ISizableIndex
    {
        private HashVector _index = new HashVector();
        private long _head = -1; //min
        private long _tail = -1; //max
        private object _syncLock = new object();

        private long _evictionIndexEntriesSize = 0;

        private int _keysCount;

        public int KeysCount
        {
            get { return _keysCount; }
            set { _keysCount = value; }
        }

        internal object SyncRoot
        {
            get { return _syncLock; }
        }

        internal bool Contains(long key, object value)
        {
            if (_index.Contains(key))
            {
                EvictionIndexEntry indexEntry = _index[key] as EvictionIndexEntry;
                return indexEntry.Contains(value);
            }
            return false;
        }

        internal void Add(long key, object value)
        {
            if (_index.Count == 0) _head = key;

            int add = 0, remove = 0;
            bool incrementKeyCount = true;

            if (_index.Contains(key))
            {
                EvictionIndexEntry indexEntry = (EvictionIndexEntry)_index[key];
                if (indexEntry != null)
                {
                    remove = indexEntry.InMemorySize;

                    if (indexEntry.Contains(value)) incrementKeyCount = false;


                    indexEntry.Insert(value);

                    add = indexEntry.InMemorySize;
                }
            }
            else
            {
                EvictionIndexEntry indexEntry = new EvictionIndexEntry();
                indexEntry.Insert(value);

                add = indexEntry.InMemorySize;

                _index[key] = indexEntry;

                EvictionIndexEntry prevEntry = _index[_tail] as EvictionIndexEntry;

                if (prevEntry != null)
                {
                    prevEntry.Next = key;
                }
                indexEntry.Previous = _tail;

            }
            if (key > _tail) _tail = key;

            _evictionIndexEntriesSize -= remove;
            _evictionIndexEntriesSize += add;

            if (incrementKeyCount) _keysCount++;
        }

        /// <summary>
        /// insert at the begining...
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void Insert(long key, object value)
        {
            Insert(key, value, -1, _head);
        }

        /// <summary>
        /// Add method only adds the new node at the tail...
        /// Insert method can add the new nodes in between also....
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void Insert(long key, object value, long previous, long next)
        {
            EvictionIndexEntry nextEntry = _index[next] as EvictionIndexEntry;
            EvictionIndexEntry prevEntry = _index[previous] as EvictionIndexEntry;

            if (_index.Count == 0 || key < _head) _head = key;


            int addSize = 0, removeSize = 0;
            bool incrementKeyCount = true;


            if (_index.Contains(key))
            {
                EvictionIndexEntry indexEntry = (EvictionIndexEntry)_index[key];
                if (indexEntry != null)
                {
                    removeSize = indexEntry.InMemorySize;

                    if (indexEntry.Contains(value)) incrementKeyCount = false;

                    indexEntry.Insert(value);

                    addSize = indexEntry.InMemorySize;
                }
            }
            else
            {
                EvictionIndexEntry indexEntry = new EvictionIndexEntry();
                indexEntry.Insert(value);

                addSize = indexEntry.InMemorySize;

                _index[key] = indexEntry;

                //very first node
                if (prevEntry == null && nextEntry == null)
                {
                    indexEntry.Next = -1; ;
                    indexEntry.Previous = -1;
                }
                //insert at begining
                else if (prevEntry == null && nextEntry != null)
                {
                    indexEntry.Next = next;
                    indexEntry.Previous = -1;
                    nextEntry.Previous = key;
                }
                //insert at end
                else if (prevEntry != null && nextEntry == null)
                {
                    indexEntry.Previous = previous;
                    indexEntry.Next = -1;
                    prevEntry.Next = key;
                }
                //insert in between the two nodes
                else
                {
                    indexEntry.Previous = previous;
                    indexEntry.Next = next;
                    prevEntry.Next = key;
                    nextEntry.Previous = key;
                }
            }
            if (key > _tail) _tail = key;

            _evictionIndexEntriesSize -= removeSize;
            _evictionIndexEntriesSize += addSize;

            if (incrementKeyCount)
                _keysCount++;
        }

        internal void Remove(long key, object value)
        {
            EvictionIndexEntry previousEntry = null;
            EvictionIndexEntry nextEntry = null;

            int addSize = 0, removeSize = 0;


            if (_index.Contains(key))
            {
                EvictionIndexEntry indexEntry = (EvictionIndexEntry)_index[key];
                bool decrementKeyCount = true;
                removeSize = indexEntry.InMemorySize;

                if (!indexEntry.Contains(value)) decrementKeyCount = false;

                if (indexEntry.Remove(value))
                {
                    if (indexEntry.Previous != -1) previousEntry = (EvictionIndexEntry)_index[indexEntry.Previous];
                    if (indexEntry.Next != -1) nextEntry = (EvictionIndexEntry)_index[indexEntry.Next];

                    if (previousEntry != null && nextEntry != null)
                    {
                        previousEntry.Next = indexEntry.Next;
                        nextEntry.Previous = indexEntry.Previous;
                    }
                    else if (previousEntry != null)
                    {
                        previousEntry.Next = indexEntry.Next;
                        _tail = indexEntry.Previous;
                    }
                    else if (nextEntry != null)
                    {
                        nextEntry.Previous = indexEntry.Previous;
                        _head = indexEntry.Next;
                    }
                    else
                    {
                        _tail = _head = -1;
                    }
                    _index.Remove(key);
                }
                else
                {
                    addSize = indexEntry.InMemorySize;
                }

                if (decrementKeyCount)
                    _keysCount--;
            }

            _evictionIndexEntriesSize -= removeSize;
            _evictionIndexEntriesSize += addSize;
        }

        internal void Remove(long key, object value, ref long previous, ref long next)
        {
            EvictionIndexEntry previousEntry = null;
            EvictionIndexEntry nextEntry = null;

            previous = key;

            int addSize = 0, removeSize = 0;


            if (_index.Contains(key))
            {
                EvictionIndexEntry indexEntry = (EvictionIndexEntry)_index[key];

                removeSize = indexEntry.InMemorySize;

                if (indexEntry.Previous != -1) previousEntry = (EvictionIndexEntry)_index[indexEntry.Previous];
                if (indexEntry.Next != -1) nextEntry = (EvictionIndexEntry)_index[indexEntry.Next];

                next = indexEntry.Next;

                bool decrementKeyCount = true;

                if (!indexEntry.Contains(value)) decrementKeyCount = false;

                if (indexEntry.Remove(value))
                {
                    previous = indexEntry.Previous;

                    if (previousEntry != null && nextEntry != null)
                    {
                        previousEntry.Next = indexEntry.Next;
                        nextEntry.Previous = indexEntry.Previous;
                    }
                    else if (previousEntry != null)
                    {
                        previousEntry.Next = indexEntry.Next;
                        _tail = indexEntry.Previous;
                    }
                    else if (nextEntry != null)
                    {
                        nextEntry.Previous = indexEntry.Previous;
                        _head = indexEntry.Next;
                    }
                    else
                    {
                        _tail = _head = -1;
                    }
                    _index.Remove(key);
                }
                else
                {
                    addSize = indexEntry.InMemorySize;
                }

                if (decrementKeyCount) _keysCount--;
            }

            _evictionIndexEntriesSize -= removeSize;
            _evictionIndexEntriesSize += addSize;
        }

        internal void Clear()
        {
            _head = _tail = -1;

            _evictionIndexEntriesSize = 0;
            _keysCount = 0;

            _index = new HashVector();
        }

        internal IList GetSelectedKeys(DocumentCache cache, long evictSize)
        {
            EvictionIndexEntry entry = null;
            ClusteredArrayList selectedKeys = new ClusteredArrayList();
            long totalSize = 0;
            bool selectionCompleted = false;
            long index = _head;
            if (_head != -1)
            {
                do
                {
                    entry = _index[index] as EvictionIndexEntry;

                    if (entry != null)
                    {
                        IList keys = entry.GetAllKeys();
                        foreach (long key in keys)
                        {
                            CacheItem citem = cache.CacheGetWithoutNotify(key);

                            if (citem == null || citem.Flag.IsBitSet(BitsetConstants.DocumentDirty) || citem.Flag.IsBitSet(BitsetConstants.MetaDataDirty))
                            {
                                //if any of the constants is set dont evict this item.
                                continue;
                            }

                            long itemSize = citem.Size;
                            if (totalSize + itemSize >= evictSize && totalSize > 0)
                            {
                                if (evictSize - totalSize > (itemSize + totalSize) - evictSize)
                                    selectedKeys.Add(key);

                                selectionCompleted = true;
                                break;
                            }
                            else
                            {
                                selectedKeys.Add(key);
                                totalSize += itemSize;
                                //prvsSize = itemSize;
                            }
                        }
                    }
                    index = entry.Next;
                }
                while (!selectionCompleted && index != -1);
            }
            return selectedKeys;
        }

        #region ISizable Impelementation
        public long IndexInMemorySize
        {
            get
            {
                return (_evictionIndexEntriesSize + EvictionIndexSize);
            }
        }

        private long EvictionIndexSize
        {
            get
            {
                long temp = 0;

                temp += _index.BucketCount * MemoryUtil.NetHashtableOverHead;

                return temp;
            }
        }
        #endregion
    }
}
