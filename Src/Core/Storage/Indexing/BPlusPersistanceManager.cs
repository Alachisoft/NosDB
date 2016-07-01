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
using System.Diagnostics;
using System.Threading;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{

    public class BPlusPersistanceManager : IDisposable
    {
        private enum TransactionSignal
        {
            Idle,
            Commit,
            Rollback
        };

        private ManualResetEventSlim outPoint, inPoint;
        private IList<IBPlusPersister> persisters;
        private IList<IBPlusPersister> disposedList; 
        private ConcurrentQueue<long> opsToPersist;
        private Thread commitThread;
        private readonly object _sync = new object();
        private bool stop = false;
        private TransactionSignal signal;
        private Stopwatch outSyncTime, inSyncTime, opsTime, commitRollbackTime;

        public BPlusPersistanceManager(string databaseName)
        {
            //outpoint for outer sync, inpoint for inner sync
            outPoint = new ManualResetEventSlim();
            inPoint = new ManualResetEventSlim();
            opsToPersist = new ConcurrentQueue<long>();
            persisters = new ClusteredList<IBPlusPersister>();
            disposedList = new ClusteredList<IBPlusPersister>();
            commitThread = new Thread(this.persistTask);
            commitThread.Name = "BPlusPersister:" + databaseName;
            outSyncTime = new Stopwatch();
            inSyncTime = new Stopwatch();
            opsTime = new Stopwatch();
            commitRollbackTime = new Stopwatch();
        }

        public void Start()
        {
            if (LoggerManager.Instance.IndexLogger != null)
                LoggerManager.Instance.IndexLogger.Debug("BPlusPersister", "Commit thread start requested");
            commitThread.Start();
        }

        public void SignalCommit()
        {
            lock (_sync)
            {
                signal = TransactionSignal.Commit;
                if (!inPoint.IsSet)
                    inPoint.Set();
            }
        }

        public void SignalRollback()
        {
            lock (_sync)
            {
                signal = TransactionSignal.Rollback;
                if (!inPoint.IsSet)
                    inPoint.Set();
            }
        }

        

        public void RequestStop()
        {
           
            stop = true;
            if(!inPoint.IsSet)
                inPoint.Set();
            commitThread.Join();
        }

        public void RegisterPersister(IBPlusPersister persister)
        {
            persisters.Add(persister);
        }

        public void UnregisterPersister(IBPlusPersister persister)
        {
            persisters.Remove(persister);
        }

        public void SignalPersist(long toOperationId)
        {
            opsToPersist.Enqueue(toOperationId);
            if (!inPoint.IsSet)
                inPoint.Set();

        }

        public void ResetOutSync()
        {
            outPoint.Reset();
        }

        public void WaitForOutSync()
        {
            outSyncTime.Reset();
            outSyncTime.Start();
            outPoint.Wait();
            outSyncTime.Stop();
            if (LoggerManager.Instance.IndexLogger != null)
                LoggerManager.Instance.IndexLogger.Debug("BPlusPersister",
                    "Wait On Out Sync" + " OutSync Time Taken: " + outSyncTime.ElapsedMilliseconds + " (ms), InSync:" +
                    inPoint.IsSet + ", OutSync:" + outPoint.IsSet);
        }

        private void persistTask()
        {
            while (true)
            {

                long operationId;
                opsTime.Reset();
                opsTime.Start();
                while (!opsToPersist.IsEmpty)
                {
                    if (opsToPersist.TryDequeue(out operationId))
                        foreach (var bPlusPersister in persisters)
                        {
                            if (bPlusPersister.IsDisposed)
                            {
                                disposedList.Add(bPlusPersister);
                                continue;
                            }
                            bPlusPersister.PersistOperation(operationId);
                        }
                }
                opsTime.Stop();
                if (LoggerManager.Instance.IndexLogger != null && persisters.Count>0)
                    LoggerManager.Instance.IndexLogger.Debug("BPlusPersister",
                        "Index Ops Persisted, Time Taken: " + opsTime.ElapsedMilliseconds+ " (ms)");
                switch (signal)
                {
                    case TransactionSignal.Commit:
                        commitRollbackTime.Reset();
                        commitRollbackTime.Start();
                        foreach (var bPlusPersister in persisters)
                        {
                            if (bPlusPersister.IsDisposed)
                            {
                                disposedList.Add(bPlusPersister);
                                continue;
                            }
                            try
                            {
                                if (bPlusPersister != null)
                                    bPlusPersister.Commit();
                            }
                            catch (Exception ex)
                            {
                                disposedList.Add(bPlusPersister);
                                bPlusPersister.Parent.RecreateIndex(bPlusPersister.Configuration);
                                if (LoggerManager.Instance.IndexLogger != null)
                                {
                                    LoggerManager.Instance.IndexLogger.Error("BPlusPersister", "Index corruption detected on commit, " + ex.Message + ", Regenerating index...");
                                }
                            }
                        }
                        commitRollbackTime.Stop();

                        signal = TransactionSignal.Idle;
                        outPoint.Set();
                        if (LoggerManager.Instance.IndexLogger != null && persisters.Count > 0)
                            LoggerManager.Instance.IndexLogger.Debug("BPlusPersister",
                                "Commit Executed, " + "Time Taken: " + commitRollbackTime.ElapsedMilliseconds + "(ms)" +
                                ", InSync:" + inPoint.IsSet + ", OutSync:" + outPoint.IsSet);
                        break;
                    case TransactionSignal.Rollback:
                        commitRollbackTime.Reset();
                        commitRollbackTime.Start();
                        foreach (var bPlusPersister in persisters)
                        {
                            if (bPlusPersister.IsDisposed)
                            {
                                disposedList.Add(bPlusPersister);
                                continue;
                            }
                            try
                            {
                                bPlusPersister.Rollback();
                            }
                            catch (Exception ex)
                            {
                                disposedList.Add(bPlusPersister);
                                bPlusPersister.Parent.RecreateIndex(bPlusPersister.Configuration);
                                if (LoggerManager.Instance.IndexLogger != null)
                                {
                                    LoggerManager.Instance.IndexLogger.Error("BPlusPersister","Index corruption detected on rollback, "+ ex.Message+", Regenerating index...");
                                }
                            }
                        }
                        commitRollbackTime.Stop();

                        signal = TransactionSignal.Idle;
                        outPoint.Set();
                        if (LoggerManager.Instance.IndexLogger != null && persisters.Count > 0)
                            LoggerManager.Instance.IndexLogger.Debug("BPlusPersister",
                                "Rollback Executed" + "Time Taken: " + commitRollbackTime.ElapsedMilliseconds + "(ms)" +
                                ", InSync:" + inPoint.IsSet + ", OutSync:" + outPoint.IsSet);
                        break;

                }
                foreach (var bPlusPersister in disposedList)
                {
                    persisters.Remove(bPlusPersister);
                }

                if (opsToPersist.IsEmpty)
                    inPoint.Reset();

                inSyncTime.Reset();
                inSyncTime.Start();
                inPoint.Wait();
                inSyncTime.Stop();
                
                if (stop)
                    return;
            }

        }


        public void Dispose()
        {
            stop = true;
        }

        ~BPlusPersistanceManager()
        {
            Dispose();
        }
    }
}