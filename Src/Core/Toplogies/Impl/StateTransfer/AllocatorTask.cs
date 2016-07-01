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
using System.Threading;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Stats;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    /// <summary>
    /// Allocator Task Class
    /// This class is responsible to allocating new tasks if resources avaialbe
    /// </summary>
    public class AllocatorTask
    {
        private NodeContext context = null;
        private const int poolingthreshold = 10 * 1000; //10 Seconds 
        private Thread _allocatorThread = null;
        private volatile bool running;
        private IAllocator allocator = null;
        private System.Threading.ManualResetEvent startSignal;

        private Object mutex = new Object();

        public AllocatorTask(IAllocator allocator, NodeContext nc)
        {
            this.context = nc;
            this.allocator = allocator;
        }

        public void Start()
        {
            lock (mutex)
            {
                if (!running)
                {
                    try
                    {
                        running = true;

                        startSignal = new System.Threading.ManualResetEvent(false);

                        _allocatorThread = new Thread(new ThreadStart(Process));
                        _allocatorThread.Name = "TaskAllocatorThread";
                        _allocatorThread.IsBackground = true;
                        _allocatorThread.Start();
                        startSignal.Set();
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("NodeStateTransferManager.TaskAllocatorManager.Start", ex);
                    }
                }
                else
                {
                    this.Resume();
                }
            }
        }

        public void Process()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = context.LocalShardName != null ? context.LocalShardName : "", DatabaseName = "" });
            try
            {
               
                context.StatusLatch.WaitForAny(NodeStatus.Running);
                startSignal.WaitOne();

                while (running)
                {                  
                    try
                    {
                        if (allocator!=null)
                            allocator.AllocateResource();
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("NodeStateTransferManager.TaskAllocatorManager.Process(1)", ex);
                    }

                    if (!allocator.IsTaskAvailable)
                        startSignal.Reset();

                    Thread.Sleep(poolingthreshold);
                    startSignal.WaitOne();
                }
            }
            catch (ThreadAbortException e)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("NodeStateTransferManager.TaskAllocatorManager.Process(2)", e);
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("NodeStateTransferManager.TaskAllocatorManager.Process(3)", ex);

            }
            finally
            {
                lock (mutex)
                {
                    running = false;
                }
            }
        }

        public void Pause()
        {
            startSignal.Reset();
        }

        public void Resume()
        {
            startSignal.Set();
        }

        public void Stop()
        {
            lock (mutex)
            {
                running = false;
                try
                {
                    if (_allocatorThread != null)
                        _allocatorThread.Abort();
                }
                catch (ThreadStateException)
                { }
                catch (System.Security.SecurityException)
                { }
            }            
        }

        public void Dispose()
        {
            Stop();
        }

    }
}