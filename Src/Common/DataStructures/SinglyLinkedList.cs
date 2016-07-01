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
using Alachisoft.NosDB.Common.Threading;

namespace Alachisoft.NosDB.Common.DataStructures
{
    //A temporary implementation for singlylinked list (a wrapper on .Net generic linkedlist)
    //which provides uninterruptible enumurator.
    public class SinglyLinkedList<T> : IEnumerable<T>, TimeScheduler.Task, IDisposable
    {
        private readonly LinkedList<T> _internalList;
        private HashSet<T> _deleteList;
        private LinkedListNode<T> _currentNode = null;
        private readonly object _locker = new object();
        private int _cleanInterval = 15000;
        private bool isDisposed;

        private bool _isRunning;
        private object _isRunnigLock = new object();
        private bool _runAgain = false;

        public SinglyLinkedList(int cleanInterval = 15000)
        {
            _internalList = new LinkedList<T>();
            _deleteList = new HashSet<T>();
            _cleanInterval = cleanInterval;
            TimeScheduler.Global.AddTask(this);
        }

        public LinkedListNode<T> Add(T key)
        {
            lock (_locker)
                return _internalList.AddLast(key);
        }

        public void Remove(T key)
        {
            lock (_locker)
                _deleteList.Add(key);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_internalList.Count > 0)
            {
                LinkedListNode<T> currentNode = _internalList.First;

                while (currentNode != null)
                {
                    if (!_deleteList.Contains(currentNode.Value))
                        yield return currentNode.Value;
                    currentNode = currentNode.Next;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void DeletionWork()
        {
            HashSet<T> currentSet;

            lock (_locker)
            {
                 currentSet = _deleteList;
                _deleteList = new HashSet<T>();
            }

            foreach (var value in currentSet)
            {
                lock (_locker)
                    _internalList.Remove(value);
            }
        }

        public bool IsCancelled()
        {
            return isDisposed;
        }

        public long GetNextInterval()
        {
            return _cleanInterval;
        }

        public void Run()
        {
            DeletionWork();
        }

        public void Dispose()
        {
            isDisposed = true;
        }

        public bool RunAgain
        {
            get { return _runAgain; }
            set { _runAgain = value; }
        }

        public bool IsRunning
        {
            get
            {
                lock (_isRunnigLock)
                {
                    return _isRunning;
                }
            }
            set
            {
                lock (_isRunnigLock)
                {
                    _isRunning = value;
                }
            }
        }
    }
}
