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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Configuration.Services;
using ShardInfo = Alachisoft.NosDB.Common.Configuration.Services.ShardInfo;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Net;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks
{
    /// <summary>
    /// Configuraiton Change class.
    /// This class is responsible for pooling any change on config server
    /// </summary>
    public class ConfigurationChangeTask : IConfigurationSessionListener
    {
        private IConfigurationSession configSession = null;
        private IConfigurationListener listener = null;
        private ClusterInfo clusterInfo = null;
        private ShardConfiguration shardConfig = null;
        private ShardInfo affectedShard = null;
        private NodeContext context = null;
        private IThreadPool _threadPool;
        private const int poolingthreshold = 30 * 1000;//time for every config change

        private Thread _changeTaskThread = null;
        private Object mutex = new Object();

        private System.Threading.ManualResetEvent startSignal;
        private volatile bool running;

        public ConfigurationChangeTask(IConfigurationListener changeListener, NodeContext context)
        {
            this.configSession = context.ConfigurationSession;
            this.listener = changeListener;
            this.context = context;

            _threadPool = new ClrThreadPool();

            _threadPool.Initialize();
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

                        _changeTaskThread = new Thread(new ThreadStart(Process));
                        _changeTaskThread.Name = "ChangeTaskThread";
                        _changeTaskThread.IsBackground = true;
                        _changeTaskThread.Start();
                        startSignal.Set();
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("ConfigurationChangeTask.Start", ex);
                    }
                }
            }
        }

        public void Process()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = context.LocalShardName != null ? context.LocalShardName : "", DatabaseName = "" });
            try
            {
                this.context.StatusLatch.WaitForAny(NodeStatus.Running);
                startSignal.WaitOne();

                if (configSession != null)
                {
                    ((OutProcConfigurationSession)configSession).RegisterListener(this);
                    if (clusterInfo == null)
                        clusterInfo = configSession.GetDatabaseClusterInfo(context.ClusterName);
                    if (shardConfig == null)
                    {
                        ClusterConfiguration cConfig = configSession.GetDatabaseClusterConfiguration(context.ClusterName);
                        if (cConfig != null && cConfig.Deployment != null)
                            shardConfig = cConfig.Deployment.GetShard(context.LocalShardName);
                    }
                }

                while (running)
                {
                    try
                    {
                        //To-do: The task should try re-connecting with the CS
                        ClusterInfo latestInfo = configSession.GetDatabaseClusterInfo(context.ClusterName);

                        ShardConfiguration latestConfig = null;
                        ClusterConfiguration cConfig = configSession.GetDatabaseClusterConfiguration(context.ClusterName);
                        if (cConfig != null && cConfig.Deployment != null)
                            latestConfig = cConfig.Deployment.GetShard(context.LocalShardName);

                        ChangeType type = CheckForClusterInfoConfigChange(clusterInfo, latestInfo);

                        IList<ConfigChangeParams> changeParams = CheckForLocalShardConfigChange(shardConfig, latestConfig);

                        if (type != ChangeType.None)
                        {
                            clusterInfo = latestInfo;
                            this._threadPool.ExecuteTask(new ConfigChangeDeliverTask(listener,
                                new ConfigChangeEventArgs(context.ClusterName, type), affectedShard.Name));
                        }

                        if (changeParams != null && changeParams.Count > 0)
                        {
                            shardConfig = latestConfig;
                            foreach (var item in changeParams)
                            {
                                ConfigChangeEventArgs args = new ConfigChangeEventArgs();
                                args.SetParamValue(EventParamName.ShardName, context.LocalShardName);
                                args.SetParamValue(EventParamName.ClusterName, context.ClusterName);
                                args.SetParamValue(EventParamName.ConfigurationChangeType, item.ChangeType);
                                args.SetParamValue(EventParamName.NodeAddress, item.AfftctedNode);

                                this._threadPool.ExecuteTask(new ConfigChangeDeliverTask(listener,
                            args, context.LocalShardName));

                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                    LoggerManager.Instance.ShardLogger.Info("ConfigChangeTask.Run()", "Config change: " + item.ChangeType + " for " + item.AfftctedNode);

                            }
                        }
                    }
                    catch (ThreadAbortException e)
                    {
                        if (LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                            LoggerManager.Instance.ShardLogger.Warn("ConfigurationChangeTask.Process", "thread aborted.");
                        _changeTaskThread = null;
                        break;
                    }
                    catch (ManagementException e)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.ShardLogger.Error("ConfigurationChangeTask.Process",e.ToString());
                        }

                        Pause();
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        {
                            LoggerManager.Instance.ShardLogger.Info("ConfigurationChangeTask.Process", "Configuration change task paused.");
                        }

                    }
                    Thread.Sleep(poolingthreshold);
                    startSignal.WaitOne();
                }
            }
            
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("ConfigurationChangeTask.Process", ex);
                // RTD: safety check. Needs to be removed later (exception should be caught in the ManagementException section.
                if (ex.Message.Contains("No configuration server is available to process the request"))
                {
                    if (!this.IsPaused)
                    {
                        this.Pause();
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        {
                            LoggerManager.Instance.ShardLogger.Info("ConfigurationChangeTask.Process",
                                "Configuration change task paused.");
                        }
                    }
                }


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
        
        public bool IsPaused
        {
            get { return !startSignal.WaitOne(0); }
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

                if (_changeTaskThread != null)
                    _changeTaskThread.Abort();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private ChangeType CheckForClusterInfoConfigChange(ClusterInfo oldInfo, ClusterInfo latestInfo)
        {
            //Check for Deployment[cluster] change

            //For Phase I we will only check for if there is any membership change in remote shards info            

            if (latestInfo.ShardInfo != null)
            {
                foreach (ShardInfo latestShard in latestInfo.ShardInfo.Values)
                {
                    if (latestShard.Name.Equals(context.LocalShardName, StringComparison.OrdinalIgnoreCase)) continue;

                    ShardInfo oldShard = null;

                    foreach (ShardInfo shard in clusterInfo.ShardInfo.Values)
                    {
                        if (shard.Name.Equals(latestShard.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            oldShard = shard;
                            break;
                        }
                    }

                    if (oldShard != null)
                    {
                        if (oldShard.Primary == null && latestShard.Primary == null) continue;

                        if ((oldShard.Primary == null && latestShard.Primary != null) || (oldShard.Primary != null && latestShard.Primary == null) || (!oldShard.Primary.Equals(latestShard.Primary)))
                        {
                            //if (affectedShards == null)
                            //    affectedShards = new List<ShardInfo>();
                            //if (!affectedShards.Contains(latestShard))
                            //    affectedShards.Add(latestShard);
                            affectedShard = latestShard;
                            return ChangeType.MembershipChanged;
                        }
                    }
                }
            }

            //Check for DDL[database] change            

            return ChangeType.None;
        }

        private IList<ConfigChangeParams> CheckForLocalShardConfigChange(ShardConfiguration oldConfig, ShardConfiguration latestConfig)
        {
            IList<ConfigChangeParams> configChanges = new List<ConfigChangeParams>();
            if (oldConfig != null && latestConfig != null)
            {
                if (oldConfig.Servers != null && latestConfig.Servers != null)
                {
                    IDictionary<string, ServerNode> oldNodes = oldConfig.Servers.Nodes;
                    if (oldNodes != null)
                    {
                        foreach (KeyValuePair<string, ServerNode> oldNode in oldNodes)
                        {
                            ServerNode latestNode = latestConfig.Servers.GetServerNode(oldNode.Value.Name);
                            if (latestNode != null)
                            {
                                if (oldNode.Value.Priority != latestNode.Priority)
                                {
                                    configChanges.Add(new ConfigChangeParams(new Address(latestNode.Name, latestConfig.Port), ChangeType.PriorityChanged));
                                }
                            }
                            else
                            {
                                configChanges.Add(new ConfigChangeParams(new Address(oldNode.Value.Name, oldConfig.Port), ChangeType.NodeRemoved));
                            }
                        }
                    }
                    IDictionary<string, ServerNode> latestNodes = latestConfig.Servers.Nodes;
                    if (latestNodes != null)
                    {
                        foreach (KeyValuePair<string, ServerNode> newNode in latestNodes)
                        {
                            ServerNode oldNode = oldConfig.Servers.GetServerNode(newNode.Value.Name);
                            if (oldNode == null)
                                configChanges.Add(new ConfigChangeParams(new Address(newNode.Value.Name, latestConfig.Port), ChangeType.NodeAdded));
                        }
                    }
                }
            }
            return configChanges;
        }


        public void OnSessionDisconnected(Common.Configuration.Services.IConfigurationSession session)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(context.LocalShardName + ".ConfigChangeTask", "connection disconnected with configuration server");
        }

        public void OnSessionConnected(Common.Configuration.Services.IConfigurationSession session)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(context.LocalShardName + ".ConfigChangeTask", "connection restored with configuration server");

            if (IsPaused)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                    LoggerManager.Instance.ShardLogger.Info(context.LocalShardName + ".ConfigChangeTask", "Resuming config change task thread.");
                this.Resume();
            }
        }

    }

    #region Latch Pattren Code

    //public class CountDownLatch
    //{
    //    private volatile int m_remain;
    //    private EventWaitHandle m_event;

    //    public CountDownLatch(int count)
    //    {
    //        if (count < 0)
    //            throw new ArgumentOutOfRangeException();
    //        m_remain = count;
    //        m_event = new ManualResetEvent(false);
    //        if (m_remain == 0)
    //        {
    //            m_event.Set();
    //        }
    //    }

    //    public void Signal()
    //    {
    //        // The last thread to signal also sets the event.
    //        if (Interlocked.Decrement(ref m_remain) == 0)
    //            m_event.Set();
    //    }

    //    public void Wait()
    //    {
    //        m_event.WaitOne();
    //    }
    //}   
    #endregion
}