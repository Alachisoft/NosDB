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
using Alachisoft.NosDB.Common.Enum;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<ContextItem, object> _contextDictionary = new Dictionary<ContextItem, object>();

        public void Add(ContextItem key, object value)
        {
            _contextDictionary.Add(key, value);
        }

        public void Clear()
        {
           _contextDictionary.Clear();
        }

        public bool Contains(ContextItem key)
        {
            return _contextDictionary.ContainsKey(key);
        }
        
        public bool IsReadOnly { get { return false; } }

        public bool Remove(ContextItem key)
        {
            return _contextDictionary.Remove(key);
        }

        public object this[ContextItem key]
        {
            get { return _contextDictionary[key]; }
            set { _contextDictionary[key] = value; }
        }
        
        public int Count { get { return _contextDictionary.Count; } }
        
        public IEnumerator<KeyValuePair<ContextItem, object>> GetEnumerator()
        {
            return _contextDictionary.GetEnumerator();
        }

        public ICollection<ContextItem> Keys
        {
            get { return _contextDictionary.Keys; }
        }
        
        public ICollection<object> Values
        {
            get { return _contextDictionary.Values; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public bool ContainsKey(ContextItem key)
        {
            return _contextDictionary.ContainsKey(key);
        }

        public bool TryGetValue(ContextItem key, out object value)
        {
            return _contextDictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<ContextItem, object> item)
        {
           _contextDictionary.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<ContextItem, object> item)
        {
            return _contextDictionary.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<ContextItem, object>[] array, int arrayIndex){ }

        public bool Remove(KeyValuePair<ContextItem, object> item)
        {
            return _contextDictionary.Remove(item.Key);
        }
    }
}
