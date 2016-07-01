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

namespace Alachisoft.NosDB.Common.Threading
{
    public class AsyncTimeScheduler : Task
    {
        private List<QueuedTask> _tasks;
        private long _cleanupInterval = 15000;
        //private static AsyncTimeScheduler _global;

        //static AsyncTimeScheduler()
        //{
        //    _global = new AsyncTimeScheduler();
        //}

        public AsyncTimeScheduler()
        {
            _tasks = new List<QueuedTask>();
            _tasks.Add(new QueuedTask(this));
        }


        //public static AsyncTimeScheduler Global
        //{
        //    get { return _global; }
        //}

        public void AddTask(Task task)
        {
            _tasks.Add(new QueuedTask(task));
        }

        public bool IsCancelled()
        {
            return false;
        }

        public long GetNextInterval()
        {
            return _cleanupInterval;
        }

        public void Run()
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].IsCancelled)
                    _tasks.RemoveAt(i);
            }
        }
    }
}
