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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Logger;
using System.Threading;
using Alachisoft.NosDB.Common.Exceptions;

namespace Alachisoft.NosDB.Core.Storage
{
    public class UpdateBucketInfoTask : IDisposable, IConfigurationSessionListener
    {
        private const int PoolingThreshold = 120 * 1000;                 //Temporary 5 Sec

        private readonly DatabaseStore _dbStore;
        private readonly NodeContext _nodeContext;
        private readonly string _dbName;
        private volatile bool _running;
        private readonly ManualResetEvent _startSignal;
        private readonly Object _onThread = new Object();
        private Thread _bucketInfoThread;

        public UpdateBucketInfoTask(DatabaseStore dbstore, NodeContext nodeContext, string dbName)
        {
            _dbStore = dbstore;
            _nodeContext = nodeContext;
            _dbName = dbName;
            _startSignal = new ManualResetEvent(false);
        }

        #region Thread Task

        public void Run()
        {
            try
            {
                LoggerManager.Instance.SetThreadContext(new LoggerContext {ShardName = _nodeContext.LocalShardName ?? "", DatabaseName = _dbName });
                _startSignal.WaitOne();

                if (_nodeContext.ConfigurationSession != null)
                {
                    ((OutProcConfigurationSession)_nodeContext.ConfigurationSession).RegisterListener(this);
                }

                while (_running)
                {
                    if (_dbStore != null && _dbStore.Collections != null)
                    {
                        List<string> collectionKeys = _dbStore.Collections.Keys.ToList();
                        if (collectionKeys.Count > 0)
                        {
                            foreach (string col in collectionKeys)
                            {
                                if (_dbStore.Collections.ContainsKey(col))
                                {
                                    var shardInfo = new ShardInfo
                                    {
                                        ShardName = _nodeContext.LocalShardName,
                                        Statistics = new ShardStatistics()
                                    };
                                    var colValue = _dbStore.Collections[col] as BaseCollection;
                                    if (colValue != null)
                                    {
                                        shardInfo.Statistics.LocalBuckets = new HashVector<HashMapBucket, BucketStatistics>();
                                        if (colValue.BucketStatistics != null && colValue.BucketStatistics.Count > 0)
                                        {
                                            lock (colValue.BucketStatistics)
                                            {
                                                foreach (var kvp in colValue.BucketStatistics.Values)
                                                {
                                                    if (kvp.Value.IsDirty)
                                                        shardInfo.Statistics.LocalBuckets.Add(kvp.Key, kvp.Value);
                                                    colValue.BucketStatistics[kvp.Key.BucketId].Value.IsDirty = false;
                                                }
                                            }

                                            try
                                            {
                                                if (_nodeContext.ConfigurationSession != null)
                                                    _nodeContext.ConfigurationSession.UpdateBucketStatistics(_nodeContext.ClusterName, _dbName, col, shardInfo);
                                            }
                                            catch (ManagementException e)
                                            {
                                                if (e.Message.Contains("No configuration server is available to process the request"))
                                                {
                                                    Pause();
                                                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                                    {
                                                        LoggerManager.Instance.ShardLogger.Warn("UpdateBucketInfo.Run()", "Node disconnected with the Configuration server.");
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(PoolingThreshold);

                    _startSignal.WaitOne();
                }

            }
            catch (ThreadAbortException)
            {
                _bucketInfoThread = null;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("UpdateBucketInfo.Run()", e.ToString());
                }
              
            }
        }

        public void Stop()
        {

            lock (_onThread)
            {

                if (_nodeContext.ConfigurationSession != null)
                {
                    ((OutProcConfigurationSession)_nodeContext.ConfigurationSession).UnregisterListener(this);
                }

                _running = false;
                _startSignal.Set();

                if (_bucketInfoThread != null && _bucketInfoThread.IsAlive)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Info("BucketInfoTask.Stop", "Node " + _nodeContext.LocalAddress + " stopped sending bucket stats to the CS.");
                    }
                    _bucketInfoThread.Abort();
                }
            }
        }

        public void Start()
        {
            lock (_onThread)
            {
                if (_bucketInfoThread == null)
                {
                    _bucketInfoThread = new Thread(Run)
                    {
                        Name = _nodeContext.LocalShardName + ".bucketInfo",
                        IsBackground = true
                    };
                }
                if (!_running)
                    _running = true;

                if (!_bucketInfoThread.IsAlive)
                {
                    _bucketInfoThread.Start();
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.StorageLogger.Info("BucketInfoTask.Start", "Node " + _nodeContext.LocalAddress + " begins sending bucket stats to the CS for " + _dbName);
                    }
                }
                _startSignal.Set();
            }

        }
        public void Pause()
        {
            _startSignal.Reset();
        }

        public void Resume()
        {
            _startSignal.Set();
        }
        public bool IsPaused
        {
            get { return !_startSignal.WaitOne(0); }
        }

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region IConfigurationSessionListener
        public void OnSessionDisconnected(Common.Configuration.Services.IConfigurationSession session)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(_nodeContext.LocalShardName + ".UpdateBucketInfoTask", "connection disconnected with configuration server");
        }

        public void OnSessionConnected(Common.Configuration.Services.IConfigurationSession session)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info(_nodeContext.LocalShardName + ".UpdateBucketInfoTask", "connection restored with configuration server");
            if (IsPaused)
                Resume();
        }
        #endregion
    }
}