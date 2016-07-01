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
using System.Diagnostics;
using System.Threading;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.Threading
{
    /// <summary>
    /// 
    /// </summary>
    public class Latch
    {
        /// <summary> A watchdog that prevents pre-init use of cache. </summary>
        //private Promise _initWatch = new Promise();
        /// <summary> The runtime status of this node. </summary>
        private BitSet _status = new BitSet();

        //Lock object
        private object mutex = new object();


        public Latch() { }
        public Latch(byte initialStatus) { _status.Data = initialStatus; }

        public BitSet Status { get { return _status; } }


        /// <summary>
        /// Check is aall of the bits given in the bitset is set.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool AreAllBitsSet(byte status)
        {
            return _status.IsBitSet(status);
        }

        /// <summary>
        /// Check is any of the bits given in the bitset is set.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool IsAnyBitsSet(byte status)
        {
            return _status.IsAnyBitSet(status);
        }

        /// <summary> The runtime status of this node. </summary>
        public void Clear()
        {
            lock (mutex)
            {
                _status.Data = 0;
                //_initWatch.SetResult(_status);
                Monitor.PulseAll(mutex);
            }
        }

        /// <summary> The runtime status of this node. </summary>
        public void SetStatusBit(byte bitsToSet, byte bitsToUnset)
        {
            lock (mutex)
            {
                _status.Set(bitsToSet, bitsToUnset);
                //_initWatch.SetResult(_status);
                Monitor.PulseAll(mutex);
            }
        }

        /// <summary>
        /// Blocks the thread until any of the two statii is reached.
        /// </summary>
        /// <param name="status"></param>
        public void WaitForAny(byte status)
        {
            lock (mutex)
            {
                while (!IsAnyBitsSet(status))
                {
                    try
                    {
                        Monitor.Wait(mutex);
                    }
                    catch (ThreadAbortException) { return; }
                    catch (ThreadInterruptedException) { return; }
                }
             
            }
        }

        /// <summary>
        /// Blocks the thread until any of the two statii is reached ot timeout expires.
        /// </summary>
        /// <param name="status"></param>
        public void WaitForAny(byte status, long timeout)
        {
            if (timeout > 0)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                lock (mutex)
                {
                    while (!IsAnyBitsSet(status) && timeout > 0)
                    {
                        try
                        {
                            Monitor.Wait(mutex, (int)timeout, true);
                            timeout -= (long)watch.Elapsed.TotalMilliseconds;
                        }
                        catch (ThreadAbortException) { return; }
                        catch (ThreadInterruptedException) { return; }
                    }
                }
            }
        }

        /// <summary>
        /// Blocks the thread until any of the two statii is reached.
        /// </summary>
        /// <param name="status"></param>
        public void WaitForAll(byte status)
        {
            lock (mutex)
            {
                while (!AreAllBitsSet(status))
                {
                    try
                    {
                        Monitor.Wait(status);
                    }
                    catch (ThreadAbortException) { return; }
                    catch (ThreadInterruptedException) { return; }
                }
            }
        }


        // OLD CODE
        //while (!AreAllBitsSet(status))
        //{
        //    object result = _initWatch.WaitResult(Timeout.Infinite); // _initWatch is Promise
        //    /// Result of a reset on the watch dog, done from dispose
        //    if (result == null)
        //    {
        //        return;
        //    }
        //}
    }
}