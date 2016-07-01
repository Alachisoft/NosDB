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
using System.Collections.ObjectModel;

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class LinkMap<K,V>:IDictionary<K,V>
    {
        private class Bucket
        {
            public K Key;
            public V Value;

            public Bucket(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }

        private IDictionary<K, int> _keyToIndexMap;
        private IDictionary<int, Bucket> _indexToValueMap;
        private int count = 0;

        public LinkMap()
        {
            _keyToIndexMap = new Dictionary<K, int>();
            _indexToValueMap = new Dictionary<int, Bucket>();
        }

        public void Add(K key, V value)
        {
            if(_keyToIndexMap.ContainsKey(key))
                throw new ArgumentException("The specified key already exists in the collection. ");
            _keyToIndexMap.Add(key,count);
            _indexToValueMap.Add(count++, new Bucket(key, value));
        }


        public bool ContainsKey(K key)
        {
            return _keyToIndexMap.ContainsKey(key);
        }

        public ICollection<K> Keys
        {
            get { return _keyToIndexMap.Keys; }
        }

        public bool Remove(K key)
        {
            int fieldId = -1;
            if (_keyToIndexMap.TryGetValue(key, out fieldId))
            {
                _keyToIndexMap.Remove(key);
                _indexToValueMap.Remove(fieldId);
                for (int k = fieldId; k < count; k++)
                {
                    _indexToValueMap[k] = _indexToValueMap[k + 1];
                    _keyToIndexMap[_indexToValueMap[k + 1].Key] = k;
                }
                count--;
                return true;
            }
            return false;
        }

        public bool TryGetValue(K key, out V value)
        {
            try
            {
                int id = -1;
                if (_keyToIndexMap.TryGetValue(key, out id))
                {
                    value = _indexToValueMap[id].Value;
                    return true;
                }
            }
            catch
            {
            }

            value = default(V);
            return false;

        }

        public ICollection<V> Values
        {
            get
            {
                ICollection<V> values = new Collection<V>();
                foreach (var bucket in _indexToValueMap.Values)
                {
                    values.Add(bucket.Value);
                }
                return values;
            }
        }

        public V this[K key]
        {
            get { return _indexToValueMap[_keyToIndexMap[key]].Value; }
            set { _indexToValueMap[_keyToIndexMap[key]] = new Bucket(key, value); }
        }

        public V this[int index]
        {
            get
            {
                Bucket value;
                if (_indexToValueMap.TryGetValue(index, out value))
                    return value.Value;
                return default(V);
            }
        }

        public K GetKey(int index)
        {
            Bucket value;
            if (_indexToValueMap.TryGetValue(index, out value))
                return value.Key;
            return default(K);
        }

        public int GetIndex(K key)
        {
            int index = -1;
            _keyToIndexMap.TryGetValue(key, out index);
            return index;

        }

        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _keyToIndexMap.Clear();
            _indexToValueMap.Clear();
            count = 0;
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            int index = -1;
            if (_keyToIndexMap.TryGetValue(item.Key, out index))
            {
                Bucket value = _indexToValueMap[index];
                return value.Value.Equals(item.Value);
            }
            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if(array.Length<count)
                throw new ArgumentException("The provided array is not long enough to contain this collection. ");
            if(arrayIndex>=array.Length)
                throw new IndexOutOfRangeException("The array index is outside the length of the array. ");
            for (int i = 0; i < array.Length - arrayIndex; i++)
            {
                if (i >= count)
                    return;
                Bucket bucket = _indexToValueMap[i];
                array[i] = new KeyValuePair<K, V>(bucket.Key, bucket.Value);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new LinkMapEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new LinkMapEnumerator(this);
        }

        public class LinkMapEnumerator : IEnumerator<KeyValuePair<K, V>>
        {
            private LinkMap<K, V> _map;
            private bool isClosed = false;
            private KeyValuePair<K, V> current;
            private int start = -1;

            public LinkMapEnumerator(LinkMap<K, V> map)
            {
                _map = map;
            }

            public KeyValuePair<K, V> Current
            {
                get { return  current; }
            }

            public void Dispose()
            {
                isClosed = true;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }

            public bool MoveNext()
            {
                if (!isClosed)
                {
                    start++;
                    if (start < _map.count)
                    {
                        Bucket currentBucket = _map._indexToValueMap[start];
                        current = new KeyValuePair<K, V>(currentBucket.Key, currentBucket.Value);
                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                start = -1;
            }
        }
    }
}
