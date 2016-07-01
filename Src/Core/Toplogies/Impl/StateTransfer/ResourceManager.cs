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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Stats;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    /// <summary>
    /// Resource Manager Class
    /// This class is responsible to free resources
    /// </summary>
    public class ResourceManager
    {
        private NodeContext context = null;
        private const int poolingthreshold = 1 * 1000;
        private Thread _resourceManagerThread = null;
        private volatile bool running;
        private IDictionary<string, IResource> resources = null;
        private System.Threading.ManualResetEvent startSignal;

        private Object mutex = new Object();


        //private ConcurrentQueue<IResource> 

        public ResourceManager(NodeContext nc)
        {
            this.context = nc;
            resources = new ConcurrentDictionary<String, IResource>();
        }

        public void RegisterResource(IResource resource)
        {
            lock (mutex)
            {
                if (!resources.ContainsKey(resource.ResourceID))
                    resources.Add(resource.ResourceID, resource);
                Start();
            }
        }


        public void UnregisterResource(String resourceID)
        {
            if (resources.ContainsKey(resourceID))
                resources.Remove(resourceID);
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

                        _resourceManagerThread = new Thread(new ThreadStart(Process));
                        _resourceManagerThread.Name = "ResourceManagerThread";
                        _resourceManagerThread.IsBackground = true;
                        _resourceManagerThread.Start();
                        startSignal.Set();
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("NodeStateTransferManager.ResourceManager.Start", ex);
                    }
                }
                else
                {
                    Monitor.PulseAll(mutex);
                }
            }
        }

        public void Process()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = context.LocalShardName != null ? context.LocalShardName : "", DatabaseName = "" });
            try
            {
                //NTD:[High] Change Running to StateTransfer
                context.StatusLatch.WaitForAny(NodeStatus.Running);
                startSignal.WaitOne();

                while (running)
                {
                    IResource resource = null;

                    lock (mutex)
                    {
                        if (resources.Count == 0) Monitor.Wait(mutex);
                    }

                    foreach (var pair in resources)
                    {
                        resource = pair.Value;


                        if (resource != null)
                        {
                            try
                            {
                                if (resource.FreeResources())
                                {
                                    resources.Remove(pair.Key);
                                }
                            }
                            catch (ThreadAbortException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                                    LoggerManager.Instance.ShardLogger.Error("ConfigurationChangeTask.Process(1)", ex);
                            }
                        }
                    }

                    Thread.Sleep(poolingthreshold);
                    startSignal.WaitOne();
                }
            }
            catch (ThreadAbortException e)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("ResourceManager.Process(2)", e);
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("ResourceManager.Process(3)", ex);

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
                    if (_resourceManagerThread != null)
                        _resourceManagerThread.Abort();
                }
                catch (ThreadStateException)
                { }
                catch (System.Security.SecurityException)
                { }
            }
            resources = null;
        }

        public void Dispose()
        {
            Stop();
        }

    }
}