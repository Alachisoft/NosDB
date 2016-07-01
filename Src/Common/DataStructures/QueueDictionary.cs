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
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class QueueDictionary<TKey, TValue>
    {
        private readonly LinkedList<Tuple<TKey, TValue>> _queue =
          new LinkedList<Tuple<TKey, TValue>>();

        private readonly Dictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>
          _dictionary = new Dictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>();

        private readonly object _syncRoot = new object();

        public TValue Dequeue()
        {
            lock (_syncRoot)
            {
                Tuple<TKey, TValue> item = _queue.First();
                _queue.RemoveFirst();
                _dictionary.Remove(item.Item1);
                return item.Item2;
            }
        }

        public TValue Dequeue(TKey key)
        {
            lock (_syncRoot)
            {
                LinkedListNode<Tuple<TKey, TValue>> node = _dictionary[key];
                _dictionary.Remove(key);
                _queue.Remove(node);
                return node.Value.Item2;
            }
        }

        public void Enqueue(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                LinkedListNode<Tuple<TKey, TValue>> node =
                  _queue.AddLast(new Tuple<TKey, TValue>(key, value));
                _dictionary.Add(key, node);
            }
        }
    }
}
