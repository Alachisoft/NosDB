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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration
{
    public class ConnectionRestorationManager : IConnectionRestoration
    {
        private IList<BrokenConnectionEntry> _brokenConnections = null;
        private volatile bool _running;
        private const int _poolingThreshold = 5 * 1000;
        private System.Threading.ManualResetEvent _startSignal;
        private const double _basicRetryInterval = 5;
        private const double _maxRetryIntervalValue = 90;
        private NodeContext _context = null;
        private Object _mutex = new Object();
        private Object _onThread = new Object();
        private Thread _connRestThread = null;

        public ConnectionRestorationManager()
        {
            _startSignal = new System.Threading.ManualResetEvent(false);
        }

        #region IConnectionRestoration Implementation
        public void Initialize(NodeContext context)
        {
            this._context = context;
            _connRestThread = new Thread(new ThreadStart(Run));
            if (this._context != null)
                _connRestThread.Name = _context.LocalShardName + ".ConnRestMgr";
            _connRestThread.IsBackground = true; ;

        }

        public void RegisterListener(BrokenConnectionInfo entryKey, IConnectionRestorationListener listener, string shardName)
        {

            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = shardName != null ? shardName : "", DatabaseName = "" });
            BrokenConnectionEntry entry = new BrokenConnectionEntry();

            entry.CRetryInfo = new BrokenConnectionEntry.RetryInfo();
            entry.CRetryInfo.LastRetryTimestamp = DateTime.Now;
            entry.CRetryInfo.RetryInterval = _basicRetryInterval;

            entry.ConnectionRestorationListener = listener;
            if (_context != null)
                entry.Channel = new DualChannel(entryKey.BrokenAddress.IpAddress.ToString(), entryKey.BrokenAddress.Port, _context.LocalAddress.IpAddress.ToString(), entryKey.SessionType, new TraceProvider(), new ShardChannelFormatter());

            entry.ConnectionInfo = entryKey;
            if (_brokenConnections == null)
            {
                _brokenConnections = new List<BrokenConnectionEntry>();
                this.Start();
            }
            lock (_mutex)
            {
                if (!_brokenConnections.Contains(entry))
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Info("ConnRestorationMgr.RegisterListener()", entry.ToString());
                    }
                    _brokenConnections.Add(entry);
                }
                if (_brokenConnections.Count == 1 && IsPaused)
                    this.Resume();
            }

        }

        public void UnregisterListener(BrokenConnectionInfo entryKey)
        {
            if (_brokenConnections != null)
            {
                IList<BrokenConnectionEntry> brokenConnList = _brokenConnections.ToList();
                foreach (var entry in brokenConnList)
                {
                    if (entry.ConnectionInfo.BrokenAddress.Equals(entryKey.BrokenAddress))
                        UnregisterListener(entry);
                }
            }
        }

        private void UnregisterListener(BrokenConnectionEntry entry)
        {
            if (entry != null)
            {
                if (_brokenConnections != null)
                {
                    lock (_mutex)
                    {
                        _brokenConnections.Remove(entry);

                        if (_brokenConnections.Count == 0)
                            this.Pause();
                    }
                }
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled && entry.ConnectionInfo != null && entry.ConnectionInfo.BrokenAddress != null)
                {
                    LoggerManager.Instance.ShardLogger.Info("ConnRestorationMgr.UnregisterListener()", entry.ToString());
                }
            }
        }

        public void Dispose()
        {
            if (this != null)
                this.Stop();
            _context = null;
            _brokenConnections = null;
        }
        #endregion


        #region Threadpool Task
        public void Run()
        {
            try
            {
                LoggerManager.Instance.SetThreadContext(
                    new LoggerContext()
                    {
                        ShardName = _context.LocalShardName ?? "",
                        DatabaseName = ""
                    });

                _startSignal.WaitOne();
                while (_running)
                {
                    try
                    {
                        if (_brokenConnections != null && _brokenConnections.Count > 0)
                        {
                            IList<BrokenConnectionEntry> brokenChannelsList = _brokenConnections.ToList();
                            for (int i = 0; i < brokenChannelsList.Count; i++)
                            {
                                BrokenConnectionEntry info = brokenChannelsList[i];

                                if (info != null && _brokenConnections.Contains(info))
                                {

                                    if (info.CRetryInfo != null && info.CRetryInfo.LastRetryTimestamp != null 
                                        && info.CRetryInfo.LastRetryTimestamp.AddSeconds(info.CRetryInfo.RetryInterval) <= DateTime.Now)
                                    {
                                        bool isConnected = false;

                                        try
                                        {
                                            if (info.Channel == null)
                                                return;
                                            isConnected = info.Channel.Connect(false);
                                        }
                                        catch (ChannelException ex)
                                        {
                                            if (info.CRetryInfo != null && info.CRetryInfo.Retries == 1 && LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                                LoggerManager.Instance.ShardLogger.Warn("ConRestoreMgr.Run()", info + " Error " + ex.ToString());
                                        }
                                        if (isConnected && info.ConnectionRestorationListener != null)
                                        {
                                            try
                                            {
                                                info.ConnectionRestorationListener.OnConnectionRestoration(info.Channel);
                                                UnregisterListener(brokenChannelsList[i]);
                                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                                {
                                                    LoggerManager.Instance.ShardLogger.Info("ConnRestorationMgr.Run()", "restored connection " + info);
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                                                {
                                                    LoggerManager.Instance.ShardLogger.Error("ConnRestoreMgr.Run()", info + " Error " + e.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //BrokenConnectionEntry cInfo = _brokenConnections[i];
                                            info.CRetryInfo.LastRetryTimestamp = DateTime.Now;
                                            info.CRetryInfo.Retries = info.CRetryInfo.Retries + 1;
                                            double interval = info.CRetryInfo.RetryInterval + _basicRetryInterval;

                                            info.CRetryInfo.RetryInterval = (interval <= _maxRetryIntervalValue) ? interval : _basicRetryInterval;

                                            lock (_mutex)
                                            {
                                                if (_brokenConnections != null && _brokenConnections.Count > 0 &&
                                                    _brokenConnections.Contains(brokenChannelsList[i]))
                                                {
                                                    _brokenConnections[i] = info;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        Thread.Sleep(_poolingThreshold);

                        _startSignal.WaitOne();
                    }
                    catch (ThreadAbortException e)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled && _connRestThread != null)
                        {
                            LoggerManager.Instance.ShardLogger.Error(_connRestThread.Name, "Task aborted.");
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.ShardLogger.Error("ConnRestoreMgr.Run()", e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("ConnRestoreMgr.Run()", e.ToString());
                }
            }
        }

        public void Stop()
        {
            lock (_onThread)
            {
                _running = false;
                _startSignal.Set();

                if (_connRestThread != null && _connRestThread.IsAlive)
                    _connRestThread.Abort();
            }
        }

        public void Start()
        {
            lock (_onThread)
            {
                if (!_running)
                    _running = true;

                if (_connRestThread.IsAlive)
                    _connRestThread.Join();

                _connRestThread.Start();
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
        #endregion


    }
}
