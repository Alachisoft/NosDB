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

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class MutableQueue<T>
    {
        private T[] items;
        private int tail;
        
        public MutableQueue()
        {
            items = new T[1];
            tail = 0;
        }

        public MutableQueue(int length)
        {
            items = new T[length];
            tail = 0;
        }

        private void expand()
        {
            if (items.Length == tail)
                Array.Resize(ref items, items.Length*2);
        }

        public void Enqueue(T item)
        {
            expand();
            items[tail++] = item;
        }

        public T Dequeue()
        {
            T returner = items[0];
            Array.Copy(items, 1, items, 0, items.Length - 1);
            return returner;
        }

        public T Peek()
        {
            return items[0];
        }

        public int Count
        {
            get { return tail; }
        }

        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public int IndexOf(T value)
        {
            int index = Array.IndexOf(items, value);
            if (index >= tail)
                return -1;
            return index;
        }

        public bool Remove(T value)
        {
            int index = IndexOf(value);
            if (index != -1)
            {
                Array.Copy(items, index + 1, items, index, items.Length - index - 1);
                tail--;
                return true;
            }
            return false;
        }

        public bool Contains(T value)
        {
            int index = IndexOf(value);
            if (index < 0 || index >= tail)
                return false;
            return true;
        }

    }
}
