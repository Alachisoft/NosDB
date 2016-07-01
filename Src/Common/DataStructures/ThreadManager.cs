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
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class ThreadManager
    {
        private long _threadCount = 0;
        private readonly object _waitObj= new object();

        /// <summary>
        /// Returns the current value of threadCount
        /// </summary>
        public long Current { get { return Interlocked.Read(ref _threadCount); } }

        /// <summary>
        /// Initializes a new instance of ThreadManager
        /// </summary>
        /// <param name="threadCount"> Initial value of threadCount</param>
        public ThreadManager(int threadCount =0)
        {
            _threadCount = threadCount;
        }

        /// <summary>
        /// Increments threadCount by 1
        /// </summary>
        /// <returns> Returns threadCount after incrementing </returns>
        public long IncrementCount()
        {
            lock (_waitObj)
            {
                Interlocked.Increment(ref _threadCount);
                Monitor.Pulse(_waitObj);
            }
            return Interlocked.Read(ref _threadCount);
        }

        /// <summary>
        /// decrements threadCount by 1
        /// </summary>
        /// <returns> Returns threadCount after decrementing </returns>
        public long DecrementCount()
        {
            lock (_waitObj)
            {
                Interlocked.Decrement(ref _threadCount);
                Monitor.Pulse(_waitObj);
            }
            return Interlocked.Read(ref _threadCount);
        }

        /// <summary>
        /// Blocking call. waits for thread count to be at specific value.
        /// </summary>
        public void WaitForCount(int threadCount=0)
        {
            while (_threadCount > threadCount)
            {
                lock (_waitObj)
                {
                    Monitor.Wait(_waitObj);
                }
            }
        }
    }
}
