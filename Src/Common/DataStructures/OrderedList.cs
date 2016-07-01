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
using System.Linq;

namespace Alachisoft.NosDB.Common.DataStructures
{
    //A wrapper implementation of Microsoft's SortedList which allows duplicate keys.
    public class OrderedList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly SortedList<TKey, List<TValue>> _list;
        
        public OrderedList()
        {
            _list = new SortedList<TKey, List<TValue>>();
        }

        public OrderedList(int capacity)
        {
            _list = new SortedList<TKey, List<TValue>>(capacity);
        }
        
        public void Add(TKey key, TValue value)
        {
            if (_list.ContainsKey(key))
            {
                _list[key].Add(value);
            }
            else
            {
                _list[key] = new List<TValue>() {value};
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _list.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return _list.Keys; }
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Remove(TKey key)
        {
            return _list.Remove(key);
        }

        //Will return the first item against the key
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_list.ContainsKey(key))
            {
                value = default(TValue);
                return false;
            }
            value=_list[key][0];
            return true;
        }

        //Iteration is needed from highest to lowest.
        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>();
                for (int index = _list.Values.Count-1; index > -1; index--)
                {
                    var valueList = _list.Values[index];
                    values.AddRange(valueList);
                }
                return values;
            }
        }

        public List<TValue> this[TKey key]
        {
            get
            {
                if (!_list.ContainsKey(key))
                {
                    return new List<TValue>() {default(TValue)};
                }
                return _list[key];
            }
            set
            {
                if (_list.ContainsKey(key))
                {
                    _list[key].AddRange(value);
                }
                else
                {
                    foreach (var val in value)
                    {
                        Add(key, val);
                    }
                }
            }
        }

        public List<TValue> FirstValues
        {
            get { return _list.First().Value; }
        }

        public List<TValue> LastValues
        {
            get { return _list.Last().Value; }
        }
        
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (_list.ContainsKey(item.Key))
            {
                _list[item.Key].Add(item.Value);
            }
            else
            {
                _list[item.Key] = new List<TValue>() { item.Value };
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _list.ContainsKey(item.Key);
        }

        //No need.
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        //Values count.
        public int Count
        {
            get
            {
                int count = 0;
                foreach (var value in _list.Values)
                    count += value.Count;
                return count;
            }
        }

        public bool IsReadOnly{ get { return false; } }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = _list[item.Key].Remove(item.Value);
            if (_list[item.Key].Count.Equals(0))
            {
                result = _list.Remove(item.Key);
            }
            return result;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<TKey, List<TValue>>> enumerator = _list.GetEnumerator();

            while (enumerator.MoveNext())
            {
                foreach (var value in enumerator.Current.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(enumerator.Current.Key, value);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
