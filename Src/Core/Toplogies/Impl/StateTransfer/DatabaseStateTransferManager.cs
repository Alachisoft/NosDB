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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Storage.Attachments;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Exceptions;


namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{

    public class DatabaseStateTransferManager : IStateTransferTask, IDispatcher, IStateTxfrOperationListener, IAllocator
    {
        private IDispatcher dispatcher = null;
        private IDictionary<String, IStateTransferTask> collectionTasksMap = null;
        private IDictionary<StateTransferIdentity, ICorresponder> collectionCorresponderMap = null;
        private NodeContext context;
        private String dbName;        
        private bool _isLocal;
        private ResourceManager resourceManager;        
        public StateTransferType TransferType { get; set; }

        private LinkedList<String> waitingColTasks = null;
        private LinkedList<String> runningColTasks = null;
        private Int32 MAX_RUNNING_COL_TASK = 1;
        private Object _schMutex = new Object();

        #region IStateTransferTask Implementation
        public bool IsRunning
        {
            get
            {
                if (collectionTasksMap == null || collectionTasksMap.Count == 0)
                    return false;

                return IsAnyTaskRunning();               
            }
        }

        private bool IsAnyTaskRunning()
        {
            foreach (IStateTransferTask task in collectionTasksMap.Values)
            {
                if (task.IsRunning)
                    return true;
                else
                    continue;
            }

            return false;
        }

        public DatabaseStateTransferManager(NodeContext nc, String dbn, IDispatcher d, ResourceManager rm)
        {
            dispatcher = d;
            dbName = dbn;
            context = nc;
            resourceManager = rm;
            collectionTasksMap = new ConcurrentDictionary<String, IStateTransferTask>();
            collectionCorresponderMap = new ConcurrentDictionary<StateTransferIdentity, ICorresponder>();

            runningColTasks = new LinkedList<string>();
            waitingColTasks = new LinkedList<string>();

        }

        public void Start()
        {
            AllocateResource();
        }

        /// <summary>
        /// Re-Enqueue Failed Task
        /// </summary>
        public void ReFT() 
        {
            if(collectionTasksMap!=null && collectionTasksMap.Count>0)
            {
                runningColTasks.Clear();

                foreach(var pair in collectionTasksMap)
                {
                    if(pair.Value.Status==StateTxfrStatus.Failed)
                   {
                       lock (_schMutex)
                        {
                            //runningColTasks.Remove(pair.Key);
                            waitingColTasks.AddLast(pair.Key);
                            pair.Value.Status = StateTxfrStatus.Waiting;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Re-Enqueue Failed Task
        /// </summary>
        public void Reset()
        {
            if (collectionTasksMap != null)
                collectionTasksMap.Clear();

            if(runningColTasks!=null)
                runningColTasks.Clear();

            if (waitingColTasks != null)
                waitingColTasks.Clear();
        }

        public void Initialize(ICollection map, StateTransferType transferType, bool forLocal = false)
        {
            _isLocal = forLocal;
            TransferType = transferType;

            IDictionary<String, IDistribution> collectionMap = map as IDictionary<String, IDistribution>;
            if (collectionMap != null && collectionMap.Count > 0)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug("DatabaseStateTransferManager.Start()", "State Transfer Started for " + this.dbName);

                //lock (_stateTxfrMutex)
                //{
                foreach (KeyValuePair<String, IDistribution> colInfo in collectionMap)
                {
                    try
                    {
                        //NTD: [Normal] Kindly perform state transfer for attachment collection

                        //if (colInfo.Key.Equals(AttachmentAttributes.ATTACHMENT_COLLECTION, StringComparison.OrdinalIgnoreCase)) continue;

                        if (TransferType == StateTransferType.INTER_SHARD && colInfo.Value.Type == Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.DistributionMethod.NonShardedDistribution)
                        {
                            NonShardedDistribution nonShardedDist = colInfo.Value as NonShardedDistribution;

                            if (!nonShardedDist.Bucket.FinalShard.Equals(context.LocalShardName, StringComparison.OrdinalIgnoreCase)) continue;
                        }


                        IStateTransferTask task = null;
                        ICollection bucketList = GetBucketsForStateTransfer((IDistribution)colInfo.Value);

                        lock (collectionTasksMap)
                        {
                            if (!collectionTasksMap.ContainsKey((String)colInfo.Key))
                            {
                                if (TransferType == StateTransferType.INTER_SHARD)
                                    collectionTasksMap[(String)colInfo.Key] = new StateTransferTask(context, dbName, (String)colInfo.Key, this, colInfo.Value.Type);
                                //else
                                //    collectionTasksMap[(String)colInfo.Key] = new StateTrxfrOnReplicaTask(context, dbName, (String)colInfo.Key, this, colInfo.Value.Type);
                            }

                            task = collectionTasksMap[(String)colInfo.Key];
                        }

                        if (task != null)
                        {
                            task.Initialize(bucketList, transferType, IsLocal);

                            //KeyValuePair<String, IStateTransferTask> pair = new KeyValuePair<string, IStateTransferTask>(colInfo.Key, task);
                            lock (_schMutex)
                            {
                                waitingColTasks.AddLast(colInfo.Key);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.StateXferLogger.Error("DatabaseStateTransferManager.Start()", ex.Message);
                        }
                    }
                }
            }
        }

        private IStateTransferOperation CreateStateTransferOperation(StateTransferOpCode stateTransferOpCode)
        {
            StateTransferIdentity transferIdentity = new StateTransferIdentity(this.dbName, null, null, this.TransferType, DistributionMethod.None);
            IStateTransferOperation stateTransferOperation = new StateTransferOperation(transferIdentity,
                stateTransferOpCode, new OperationParam());
            return stateTransferOperation;
        }

        private ICollection GetBucketsForStateTransfer(IDistribution distribution)
        {
            ArrayList bucketList = new ArrayList();
            if (distribution != null)
            {
                try
                {
                    bucketList.AddRange(distribution.GetBucketsForShard(context.LocalShardName));
                }
                catch (Exception) { }
            }
            return bucketList;
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            lock (_schMutex)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("DatabaseStateTransferManager.Stop()", "State Transfer Stopping for " + this.dbName);


                foreach (var ColName in runningColTasks)
                {
                    IStateTransferTask task = collectionTasksMap[ColName];

                    if (task != null)
                    {
                        task.Stop();
                    }
                }

                collectionTasksMap.Clear();

                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("DatabaseStateTransferManager.Stop()", "State Transfer Stopped for " + this.dbName);
            }

            lock (collectionCorresponderMap)
            {
                foreach (KeyValuePair<StateTransferIdentity, ICorresponder> pair in collectionCorresponderMap)
                {
                    ICorresponder crossponder = collectionCorresponderMap[pair.Key];

                    if (crossponder != null)
                    {
                        if (resourceManager != null)
                            resourceManager.UnregisterResource(crossponder.ResourceID);

                        crossponder.Dispose();
                    }
                }

                collectionCorresponderMap.Clear();
            }
        }

        public void OnShardConnected(Common.Toplogies.Impl.StateTransfer.NodeIdentity shard)
        {           
            if (collectionTasksMap != null && collectionTasksMap.Count > 0)
            {
                foreach (KeyValuePair<String, IStateTransferTask> task in collectionTasksMap)
                {
                    try
                    {
                        StateTxfrStatus previousStatus = task.Value.Status;

                        task.Value.OnShardConnected(shard);

                        if (previousStatus == StateTxfrStatus.Failed && task.Value.Status == StateTxfrStatus.Waiting)
                            waitingColTasks.AddLast(task.Key);
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.StateXferLogger.Error("DatabaseStateTransferManager.OnShardConnected()", ex.Message);
                        }
                    }
                }
            }
            else
            {
                if (LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.StateXferLogger.Info("DatabaseStateTransferManager.OnShardConnected()", "No task to update ");
                }
            }
        }

        #endregion

        #region IDisposible Implementation

        public void Dispose()
        {
            Stop();

            if (collectionTasksMap != null)
            {
                lock (collectionTasksMap)
                {
                    collectionTasksMap.Clear();
                    collectionTasksMap = null;
                }
            }

            if (collectionCorresponderMap != null)
            {
                lock (collectionCorresponderMap)
                {
                    collectionCorresponderMap.Clear();
                    collectionCorresponderMap = null;
                }
            }

        }

        #endregion

        #region IDispatcher Implemenation
        /// <summary>
        /// Dispacth operations to the underlying Database Manager if local operations,and pass to upper layer otherwise
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>
        public T DispatchOperation<T>(IStateTransferOperation operation) where T : class
        {
            switch (operation.OpCode)
            {
                // Param should contain at least db plus collection name and bucket id
                case StateTransferOpCode.GetBucketKeysFilterEnumerator:
                //case StateTransferOpCode.GetLogTable:
                //case StateTransferOpCode.RemoveLoggedOperations:
                case StateTransferOpCode.EmptyBucket:
                //case StateTransferOpCode.StartBeforeOperationLogging:
                //case StateTransferOpCode.StopBeforeOperationLogging:
                case StateTransferOpCode.ApplyLogOperations:
                case StateTransferOpCode.SetBucketStatus:
                case StateTransferOpCode.GetBucketStats:
                case StateTransferOpCode.GetBucketKeys:
                    {
                        if (context != null && context.DatabasesManager != null)
                            return context.DatabasesManager.OnOperationRecieved(operation) as T;
                        return default(T);
                    }

                //If State Transfer for this Database has been compeleted then call node level manager for compeletion
                case StateTransferOpCode.StateTxferCompeleted:
                //If State Transfer for this Database has been stopped then call node level manager for compeletion
                case StateTransferOpCode.StateTxferFailed:
                    {
                        OnCollectionTaskEnd(operation);
                        return default(T);
                    }

                ////If State Transfer for this Database has been stopped then call node level manager for compeletion
                //case StateTransferOpCode.StateTxferFailed:
                //    {
                //        if (OnStateTxfrFailed(operation)) break;
                //        return default(T);
                //    }
            }
            return (T)dispatcher.DispatchOperation<T>(operation);
        }

        #endregion

        #region StateTrxferOperationListener Implementation

        /// <summary>
        /// Will recieve handle cluster operations from remote nodes/shards 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public object OnOperationRecieved(IStateTransferOperation operation)
        {
            //Look for crossponder and handover the operation         
            switch (operation.OpCode)
            {
                case StateTransferOpCode.CreateCorresponder:
                    return HandleCreateCorresponder(operation);
                case StateTransferOpCode.TransferBucketKeys:
                    return HandleTransferBucketKeys(operation);
                case StateTransferOpCode.TransferBucketData:
                    return HandleTransferBucketData(operation);
                case StateTransferOpCode.AckBucketTxfer:
                    return HandleAckBucketTxfer(operation);
                case StateTransferOpCode.DestroyCorresponder:
                    HandleDestroyCorresponder(operation);
                    break;

            }
            return null;
        }

        /// <summary>
        /// Handle Transfer Bucket, Actually transfer bucket data to requesting node/shard
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private object HandleCreateCorresponder(IStateTransferOperation operation)
        {
            lock (collectionCorresponderMap)
            {
                if (collectionCorresponderMap.ContainsKey(operation.TaskIdentity))
                {
                    if (LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info("DatabaseStateTransferManager.HandleCreateCorresponder", "StateTxferCorrosponder for :" + operation.TaskIdentity.ToString() + "already exist");
                    //Temporary Fix because this condition should not be true
                    collectionCorresponderMap.Remove(operation.TaskIdentity);
                    //return true;
                }

                if (LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug("DatabaseStateTransferManager.HandleCreateCorresponder", "Creating New StateTxferCorrosponder for :" + operation.TaskIdentity.ToString());

                ICorresponder corresponder = new StateTxfrCorresponder(context, this, operation.TaskIdentity, IsLocal);
                collectionCorresponderMap[operation.TaskIdentity] = corresponder;

                resourceManager.RegisterResource(corresponder);
                OnCorresponderCreated();
            }

            return true;
        }

        /// <summary>
        /// AckState Transfer Compelete for buckets, remove buckets being transfered from local bucket list on this node
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private object HandleAckBucketTxfer(IStateTransferOperation operation)
        {
            try
            {
                lock (collectionCorresponderMap)
                {
                    if (!collectionCorresponderMap.ContainsKey(operation.TaskIdentity))
                    {
                        throw new Alachisoft.NosDB.Core.Toplogies.Exceptions.StateTransferException("no corrosponder found for " + operation.TaskIdentity.ToString());
                    }
                }

                ICorresponder corrosponder = collectionCorresponderMap[operation.TaskIdentity];

                corrosponder.BucketTransferCompeleted(operation.Params);

            }
            catch (Exception anyException)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error("DatabaseStateTransferManager.handleAckStateTxfr()", anyException.ToString());

                return anyException;
            }

            return null;

        }

        /// <summary>
        /// Handle Signal End Of State Transfer, remove corrosponder associated with state transfer
        /// </summary>
        /// <param name="operation"></param>
        private void HandleDestroyCorresponder(IStateTransferOperation operation)
        {
            if (this.collectionCorresponderMap != null)
            {
                lock (collectionCorresponderMap)
                {
                    if (collectionCorresponderMap.ContainsKey(operation.TaskIdentity))
                    {
                        ICorresponder cor = collectionCorresponderMap[operation.TaskIdentity] as StateTxfrCorresponder;
                        if (cor != null)
                        {
                            //mark transfer compelete and remove from corresponderMap
                            cor.TransferCompleted();
                            collectionCorresponderMap.Remove(operation.TaskIdentity);
                        }
                    }
                    //else
                    //{
                    //   if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    //   {
                    //     LoggerManager.Instance.StateXferLogger.Error("DatabaseStateTransferManager.HandleSignalEndOfStateTransfer", "Coresponder not found for " + operation.TaskIdentity.ToString());
                    //}
                    //}
                    OnCorresponderDestroyed();
                }
            }
        }

        /// <summary>
        /// Handle Transfer Bucket, Actually transfer bucket data to requesting node/shard
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private object HandleTransferBucketKeys(IStateTransferOperation operation)
        {
            ICorresponder corrosponder = null;

            lock (collectionCorresponderMap)
            {
                //if (!collectionCorresponderMap.ContainsKey(operation.TaskIdentity))
                //{
                //    if (LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                //        LoggerManager.Instance.StateXferLogger.Debug("DatabaseStateTransferManager.HandleTransferBucketKeys","Creating StateTxferCorrosponder for :"+operation.TaskIdentity.ToString());

                //    collectionCorresponderMap[operation.TaskIdentity] = new StateTxfrCorresponder(context, this, operation.TaskIdentity);//.DBName, operation.TaskIdentity.ColName, operation.TaskIdentity.NodeInfo, operation.TaskIdentity.Type);
                //}

                corrosponder = collectionCorresponderMap[operation.TaskIdentity];
            }

            if (corrosponder == null)
            {
                Exception ex = new StateTransferException("No corrosponder found for :" + operation.TaskIdentity.ToString());
                if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error("DatabaseStateTransferManager.HandleTransferBucketKeys", ex);
                return ex;
            }

            return corrosponder.GetBucketKeys(operation.Params);
        }

        /// <summary>
        /// Handle Transfer Bucket, Actually transfer bucket data to requesting node/shard
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private object HandleTransferBucketData(IStateTransferOperation operation)
        {
            ICorresponder corrosponder = null;

            lock (collectionCorresponderMap)
            {
                if (collectionCorresponderMap.ContainsKey(operation.TaskIdentity))
                    corrosponder = collectionCorresponderMap[operation.TaskIdentity];
            }

            if (corrosponder == null)
            {
                Exception ex = new StateTransferException("No corrosponder found for :" + operation.TaskIdentity.ToString());
                if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error("DatabaseStateTransferManager.HandleTransferBucketData", ex);
                return ex;
            }
            return corrosponder.GetBucketData(operation.Params);
        }
        #endregion

        #region IStateTxfrEventListener
        public void OnCollectionTaskEnd(IStateTransferOperation operation)
        {           
            if (operation != null && operation.TaskIdentity != null)
            {               
                if (collectionTasksMap != null && collectionTasksMap.Count > 0)
                {
                    if (collectionTasksMap.ContainsKey(operation.TaskIdentity.ColName))
                    {
                        collectionTasksMap[operation.TaskIdentity.ColName].Status = operation.OpCode == StateTransferOpCode.StateTxferCompeleted ? StateTxfrStatus.CompletedSuccessfully : StateTxfrStatus.Failed;
                        
                        VerifyDST();
                    }
                    else
                    {
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                            LoggerManager.Instance.StateXferLogger.Info("DBStateTxfrMgr.OStateTxfrCompletion()", "Task not found for collection " + operation.TaskIdentity.ColName);
                        return;
                    }


                    //bool isComplete = true;
                    //StateTxfrStatus dbTaskStatus = StateTxfrStatus.CompletedSuccessfully;
                    //foreach (KeyValuePair<String, IStateTransferTask> pair in collectionTasksMap)
                    //{
                    //    if (pair.Value.Status.Equals(StateTxfrStatus.Failed)) dbTaskStatus = StateTxfrStatus.Failed;

                    //    if (pair.Value.Status.Equals(StateTxfrStatus.Waiting) || pair.Value.Status.Equals(StateTxfrStatus.Running))
                    //    {
                    //        isComplete = false;
                    //        break;
                    //    }
                    //}

                    //if (isComplete)
                    //{
                    //    this.Status = dbTaskStatus;
                    //    //this.Status = operation.OpCode == StateTransferOpCode.StateTxferCompeleted ? StateTxfrStatus.CompletedSuccessfully : StateTxfrStatus.Failed;
                    //    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    //        LoggerManager.Instance.StateXferLogger.Info("DBStateTxfrMgr.OStateTxfrCompletion()", "State transfer of the database " + this.dbName + " completed with status "+this.Status.ToString());

                    //    if (this.dispatcher != null)
                    //    {
                    //        // in case database tasks  been entertained, notify NodeStateTransferManager about status
                    //        StateTransferOpCode opCode = this.Status == StateTxfrStatus.CompletedSuccessfully ? StateTransferOpCode.StateTxferCompeleted : StateTransferOpCode.StateTxferFailed;
                    //        IStateTransferOperation dbOperation = CreateStateTransferOperation(opCode);
                    //        this.dispatcher.DispatchOperation<Object>(dbOperation);
                    //    }
                    //}
                }
            }
        }

        public bool OnStateTxfrFailed(IStateTransferOperation operation)
        {           
            if (operation != null && operation.TaskIdentity != null)
            {

                if (collectionTasksMap != null && collectionTasksMap.Count > 0)
                {
                    if (collectionTasksMap.ContainsKey(operation.TaskIdentity.ColName))
                        collectionTasksMap[operation.TaskIdentity.ColName].Status = StateTxfrStatus.Failed;

                    bool isComplete = true;
                    foreach (KeyValuePair<String, IStateTransferTask> pair in collectionTasksMap)
                    {
                        if (pair.Value.Status.Equals(StateTxfrStatus.Waiting) || pair.Value.Status.Equals(StateTxfrStatus.Running))
                        {
                            isComplete = false;
                            break;
                        }
                    }

                    if (isComplete)
                    {
                        this.Status = StateTxfrStatus.Failed;
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                            LoggerManager.Instance.StateXferLogger.Info("DBStateTxfrMgr.OnStateTxfrFailed()", "State transfer of the database " + this.dbName + " stopped.");
                    }

                    return isComplete;
                }
            }
            return false;
        }

        private bool IsCompleted()
        {
            bool isComplete = true;
            StateTxfrStatus dbTaskStatus = StateTxfrStatus.CompletedSuccessfully;
            foreach (KeyValuePair<String, IStateTransferTask> pair in collectionTasksMap)
            {
                if (pair.Value.Status.Equals(StateTxfrStatus.Failed)) dbTaskStatus = StateTxfrStatus.Failed;

                if (pair.Value.Status.Equals(StateTxfrStatus.Waiting) || pair.Value.Status.Equals(StateTxfrStatus.Running))
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
            {
                this.Status = dbTaskStatus;
            }
            return isComplete;
        }
        /// <summary>
        /// Verify DatabaseStateTransferManager if no task is running/waiting/failed then state transfer is compelted for this database.
        /// </summary>
        /// <returns></returns>
        /// 
        private void VerifyDST() 
        {
            //bool isComplete = true;
            //StateTxfrStatus dbTaskStatus = StateTxfrStatus.CompletedSuccessfully;
            //foreach (KeyValuePair<String, IStateTransferTask> pair in collectionTasksMap)
            //{
            //    if (pair.Value.Status.Equals(StateTxfrStatus.Failed)) dbTaskStatus = StateTxfrStatus.Failed;

            //    if (pair.Value.Status.Equals(StateTxfrStatus.Waiting) || pair.Value.Status.Equals(StateTxfrStatus.Running))
            //    {
            //        isComplete = false;
            //        break;
            //    }
            //}

            //if (isComplete)
            //{
            //    this.Status = dbTaskStatus;
            if(IsCompleted())
            {
                //this.Status = operation.OpCode == StateTransferOpCode.StateTxferCompeleted ? StateTxfrStatus.CompletedSuccessfully : StateTxfrStatus.Failed;
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("DBStateTxfrMgr.VerifyDST()", "State transfer of the database " + this.dbName + " completed with status " + this.Status.ToString());

                if (this.dispatcher != null)
                {
                    // in case database tasks  been entertained, notify NodeStateTransferManager about status
                    StateTransferOpCode opCode = this.Status == StateTxfrStatus.CompletedSuccessfully ? StateTransferOpCode.StateTxferCompeleted : StateTransferOpCode.StateTxferFailed;
                    IStateTransferOperation dbOperation = CreateStateTransferOperation(opCode);
                    this.dispatcher.DispatchOperation<Object>(dbOperation);
                }
            }
        }

        #endregion

        private void OnCorresponderCreated()
        {
            if (collectionCorresponderMap.Count == 1)
            {
                if (context != null && context.DatabasesManager != null)
                    context.DatabasesManager.OnOperationRecieved(CreateStateTransferOperation(StateTransferOpCode.StartQueryLogging));
            }
        }

        private void OnCorresponderDestroyed()
        {
            if (collectionCorresponderMap.Count == 0)
            {
                if (context != null && context.DatabasesManager != null)
                    context.DatabasesManager.OnOperationRecieved(CreateStateTransferOperation(StateTransferOpCode.EndQueryLogging));
            }
        }

        public StateTxfrStatus Status
        {
            get;
            set;
        }

        public bool IsLocal
        {
            get { return _isLocal; }
        }

        #region IAllocator Implementation

        public bool IsTaskAvailable
        {
            get;
            set;
        }

        public void AllocateResource()
        {
            //Step1: Free Resources for next allocation
            FreeResources();

            //Step2: Check if any task remaining for allocation
            lock (_schMutex)
            {
                if (waitingColTasks.Count == 0 && runningColTasks.Count==0)
                {
                    VerifyDST();
                    //IsCompleted();
                    //IsTaskAvailable = false;
                    return;
                }
            }

            //Step3: Check if any slot avaialbe for waiting tasks
            if (runningColTasks.Count >= MAX_RUNNING_COL_TASK) return;

            //Step4: Allocate resources to waiting tasks
            while (runningColTasks.Count < MAX_RUNNING_COL_TASK && waitingColTasks.Count != 0)
            {
                var first = default(LinkedListNode<String>);

                lock (_schMutex)
                {
                    first = waitingColTasks.First;
                    if (first != null)
                        waitingColTasks.RemoveFirst();
                }

                if (first != null && first.Value!=null && collectionTasksMap.ContainsKey(first.Value))
                {
                    var colName = first.Value;
                    IStateTransferTask task = collectionTasksMap[colName];
                    task.Start();                    
                    task.Status = StateTxfrStatus.Running;

                    lock (_schMutex)
                    {
                        runningColTasks.AddLast(first);
                    }
                }
            }

        }

        public void FreeResources()
        {
            try
            {
                if (collectionTasksMap != null)
                {
                    if (runningColTasks.Count < 1) return;

                    List<String> runningTaskList = runningColTasks.ToList<String>();

                    foreach (var colName in runningTaskList)
                    {
                        if (collectionTasksMap.ContainsKey(colName))
                        {
                            switch (collectionTasksMap[colName].Status)
                            {
                                case StateTxfrStatus.CompletedSuccessfully:
                                    {
                                        collectionTasksMap.Remove(colName);
                                        runningColTasks.Remove(colName);
                                    }
                                    break;

                                //in case of stopped task should be enqueued 
                                case StateTxfrStatus.Failed:
                                    //NTD:[High] enqueue in waiting for next turn
                                    runningColTasks.Remove(colName);
                                    break;
                            }
                        }
                        else 
                        {
                            runningColTasks.Remove(colName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("DatabaseStateTransferManager.FreeResources", ex);
            }
        }

        #endregion

        #region CML Handling

        internal void OnDropCollection(String collection)
        {
            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                LoggerManager.Instance.StateXferLogger.Info("DatabaseStateTransferManager.OnDropCollection()", "collection " + collection + " being dropped");

            IStateTransferTask task = null;
            lock (_schMutex)
            {
                lock (collectionTasksMap)
                {
                    if (collectionTasksMap.ContainsKey(collection))
                    {
                        task = collectionTasksMap[collection];
                        collectionTasksMap.Remove(collection);
                    }
                }

                if (task != null)
                {
                    waitingColTasks.Remove(collection);
                    runningColTasks.Remove(collection);
                    task.Stop();
                }

            }

            lock (collectionCorresponderMap)
            {
                foreach (KeyValuePair<StateTransferIdentity, ICorresponder> pair in collectionCorresponderMap)
                {
                    if (pair.Key.ColName.Equals(collection, StringComparison.OrdinalIgnoreCase))
                    {
                        ICorresponder crossponder = collectionCorresponderMap[pair.Key];

                        if (crossponder != null)
                        {
                            crossponder.Dispose();

                            if (resourceManager != null)
                                resourceManager.UnregisterResource(crossponder.ResourceID);
                        }
                    }
                }
            }
            
            VerifyDST();
        }

        #endregion
    }
}
