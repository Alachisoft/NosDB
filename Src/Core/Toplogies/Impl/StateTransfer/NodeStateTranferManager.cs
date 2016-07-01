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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Exceptions;
//using Alachisoft.NosDB.Core.Toplogies.Impl.Replication;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{

    public class NodeStateTranferManager : IStateTransferTask, IDispatcher, IStateTxfrOperationListener,IAllocator
    {
        private NodeContext context = null;
        private IDispatcher dispatcher = null;
        
        //private Alachisoft.NosDB.Core.Toplogies.Impl.Replication.PullModelReplication _replication = null;
        private IDictionary<String, DatabaseStateTransferManager> dbStateTxferMgrMap = null;        
        private object _inStTxfrMutex = new object();
        
        //Both Managers should be combined to have a single class 
        // status: in-future
        private ResourceManager _resourceManager = null;
        private AllocatorTask _taskAllocator;
        private LinkedList<String> waitingDBTasks = null;
        private LinkedList<String> runningDBTasks = null;
        private Int32 MAX_RUNNING_DB_TASK = 1;
        private Object _schMutex = new Object();

    
        StateTransferType TransferType { get; set; }

        #region IStateTransferTask Implementation

        public bool IsRunning
        {
            get
            {
                //lock (_stateTxfrMutex)
                //{
                if (dbStateTxferMgrMap == null || dbStateTxferMgrMap.Count == 0)
                    return false;

                return IsAnyTaskRunning();
                //}
            }
        }

        private bool IsAnyTaskRunning()
        {
            foreach (IStateTransferTask task in dbStateTxferMgrMap.Values)
            {
                if (task.IsRunning)
                    return true;
            }

            return false;
        }

        public NodeStateTranferManager(NodeContext c, IDispatcher d)
        {

            context = c;
            dispatcher = d;
            dbStateTxferMgrMap = new ConcurrentDictionary<String, DatabaseStateTransferManager>();
            waitingDBTasks = new LinkedList<string>();
            runningDBTasks = new LinkedList<string>();
            //_replication = replication;
            _resourceManager = new ResourceManager(c);
            _taskAllocator = new AllocatorTask(this, c);
        }

        public void Start()
        {

            if (context.ClusterName.Equals(Common.MiscUtil.LOCAL, StringComparison.OrdinalIgnoreCase)) return;

            if (_taskAllocator != null)
                _taskAllocator.Start();

            if (_resourceManager != null)
                _resourceManager.Start();
        }

        public void Initialize(ICollection map, StateTransferType transferType, bool forLocal = false)
        {           
            if (context.ClusterName.Equals(Common.MiscUtil.LOCAL, StringComparison.OrdinalIgnoreCase)) return;
            
            if (IsRunning)
            {

                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("NodeStateTranferManager.Initialize()", "State Transfer Already Running");

                if (transferType == StateTransferType.INTRA_SHARD && TransferType == StateTransferType.INTER_SHARD)
                {

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info("NodeStateTranferManager.Initialize()", "Stopping already running  State Transfer");

                    //Stopping already running state transfer as there is mis-match between already running 
                    Stop();
                }
            }

            IDictionary<String, IDictionary<String, IDistribution>> distributionMaps = (IDictionary<String, IDictionary<String, IDistribution>>)map;
            
            TransferType = transferType;

            if (distributionMaps != null)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("NodeStateTranferManager.Initialize()", "State Transfer Initialize on " + this.context.LocalAddress.ToString());


                foreach (KeyValuePair<String, IDictionary<String, IDistribution>> dbInfo in distributionMaps)
                {
                    try
                    {
                        DatabaseStateTransferManager dbMgr = null;

                        lock (dbStateTxferMgrMap)
                        {
                            if (!dbStateTxferMgrMap.ContainsKey(dbInfo.Key))
                            {
                                dbStateTxferMgrMap[dbInfo.Key] = new DatabaseStateTransferManager(context, dbInfo.Key, this, _resourceManager);
                            }
                        }

                        dbMgr = dbStateTxferMgrMap[dbInfo.Key];

                        if (dbMgr != null)
                        {
                            dbMgr.Initialize((IDictionary)dbInfo.Value, transferType, forLocal);
                          
                            lock (_schMutex)
                            {
                                waitingDBTasks.AddLast(dbInfo.Key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.StateXferLogger.Error("NodeStateTranferManager.Initialize()", ex.Message);
                        }
                    }
                }

                if (dbStateTxferMgrMap.Count > 0)
                {
                    context.StatusLatch.SetStatusBit(Alachisoft.NosDB.Common.Stats.NodeStatus.InStateTxfer, Alachisoft.NosDB.Common.Stats.NodeStatus.None);
                    IsTaskAvailable = true;
                }

                //if (transferType==StateTransferType.INTRA_SHARD && _replication.NextReplicationChunk == null)
                //{
                //    if (!forLocal)
                //        _replication.NextReplicationChunk = _replication.GetLastOpLog();

                //    lock (_inStTxfrMutex)
                //    {
                //        _replication.NodeIsInStateTxfr = true;
                //    }
                //    _replication.Start();
                //    // Replication will be started from here and last operation will be settled from here
                //}
            }       
        }

        /// <summary>
        /// Pause NST on this node
        /// </summary>
        public void Pause()
        {
            lock (dbStateTxferMgrMap)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("NodeStateTranferManager.Pause()", "State Transfer Paused on " + this.context.LocalAddress.ToString());
               
                if (_resourceManager != null)
                    _resourceManager.Pause();

                foreach (KeyValuePair<String, DatabaseStateTransferManager> dbTaskInfo in dbStateTxferMgrMap)
                {
                    IStateTransferTask task = dbTaskInfo.Value as IStateTransferTask;

                    if (task != null)
                    {
                        task.Pause();
                    }
                }
            }
        }

        public void Stop()
        {
            lock (dbStateTxferMgrMap)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info("NodeStateTranferManager.Stop()", "State Transfer Stoping on " + this.context.LocalAddress.ToString());

                if (_resourceManager != null)
                    _resourceManager.Stop();
                
                if (_taskAllocator != null)
                    _taskAllocator.Stop();

                lock (_schMutex)
                {
                    foreach (var dbName in runningDBTasks)
                    {
                        if (dbStateTxferMgrMap.ContainsKey(dbName))
                        {
                            IStateTransferTask task = dbStateTxferMgrMap[dbName];

                            if (task != null)
                            {
                                task.Stop();
                            }
                        }
                    }
                }

                runningDBTasks.Clear();
                waitingDBTasks.Clear();

                dbStateTxferMgrMap.Clear();
            }

            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                LoggerManager.Instance.StateXferLogger.Info("NodeStateTranferManager.Stop()", "State Transfer Stopped on " + this.context.LocalAddress.ToString());
           
        }

        public void OnShardConnected(Common.Toplogies.Impl.StateTransfer.NodeIdentity shard)
        {
            if (this.dbStateTxferMgrMap != null && dbStateTxferMgrMap.Count > 0)
            {
                foreach (KeyValuePair<String, DatabaseStateTransferManager> task in dbStateTxferMgrMap)
                {
                    try
                    {
                        task.Value.OnShardConnected(shard);
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.StateXferLogger.Error("NodeStateTransferManager.OnShardConnected()", ex.Message);
                        }
                    }
                }
            }
            else
            {
                if (LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.StateXferLogger.Info("NodeStateTransferManager.OnShardConnected()", "No task to update ");
                }
            }
        }

        #endregion

        #region IDisposible Implementation

        public void Dispose()
        {
            lock (dbStateTxferMgrMap)
            {
                //lock (_stateTxfrMutex)
                //{
                if (_resourceManager != null)
                    _resourceManager.Dispose();

                foreach (KeyValuePair<String, DatabaseStateTransferManager> dbTaskInfo in dbStateTxferMgrMap)
                {
                    IStateTransferTask task = dbTaskInfo.Value as IStateTransferTask;

                    if (task != null)
                    {
                        task.Dispose();
                    }
                }
                dbStateTxferMgrMap.Clear();
                dbStateTxferMgrMap = null;
                //}
            }
        }

        #endregion

        #region IDispacther Implementation

        /// <summary>
        /// Dispacth operations to the Configuration Server if configuration operations,and pass to upper layer otherwise
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>

        public T DispatchOperation<T>(IStateTransferOperation operation) where T : class
        {
            switch (operation.OpCode)
            {
                // Send lock buckets request to configuration server
                case StateTransferOpCode.LockBucketsOnCS:
                // Send release lock buckets request to configuration server
                case StateTransferOpCode.ReleaseBucketsOnCS:
                // Announce State Transfer on configuration server
                case StateTransferOpCode.AnnounceBucketTxfer:
                // check if specified bucket is spared
                case StateTransferOpCode.IsSparsedBucket:
                // Verify Bucket Ownership
                case StateTransferOpCode.VerifyFinalOwnerShip:
                // Finalize State Transfer
                case StateTransferOpCode.FinalizeStateTransfer:
                    return context.ConfigurationSession.StateTransferOperation(context.ClusterName, operation) as T;

                case StateTransferOpCode.StateTxferCompeleted:
                    OnStateTxfrCompleted(operation);
                    break;

                case StateTransferOpCode.StateTxferFailed:
                    OnStateTxfrFailed(operation);
                    break;
                case StateTransferOpCode.GetShardPrimary:
                    return GetShardPrimary(operation) as T;
            }

            // For Shard/Cluster Level Operations
            return dispatcher.DispatchOperation<T>(operation);
        }


        /// <summary>
        /// /// Get Primary node of given shard name
        /// /// </summary>
        private Address GetShardPrimary(IStateTransferOperation operation)
        {
            //if (Cluster == null) return null;

            //if (Context.LocalShardName.Equals(shardName, StringComparison.OrdinalIgnoreCase) && Cluster.ThisShard != null && Cluster.ThisShard.Primary != null)
            //{
            //    return Cluster.ThisShard.Primary.Address;
            //}

            //if (Cluster.Shards != null && Cluster.Shards.ContainsKey(shardName))
            //{

            //    if(Cluster.Shards[shardName]!=null)
            //        return this.Cluster.Shards[shardName].Primary.Address;

            //    try
            //    {
            //        Membership membership = Context.ConfigurationSession.GetMembershipInfo(Context.ClusterName, shardName);
            //        if (membership != null && membership.Primary != null)
            //        {
            //            return new Address(membership.Primary.Name, Cluster.Shards[shardName].Port);                        
            //        }
            //    }
            //    catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException ex)
            //    {
            //        return null;
            //    }
            //}

            return null;
        }


        #endregion

        #region IStateTransferOperationListener Implementation

        public object OnOperationRecieved(IStateTransferOperation operation)
        {
            lock (dbStateTxferMgrMap)
            {
                if (!dbStateTxferMgrMap.ContainsKey(operation.TaskIdentity.DBName))
                {
                    dbStateTxferMgrMap[operation.TaskIdentity.DBName] = new DatabaseStateTransferManager(context, operation.TaskIdentity.DBName, this, _resourceManager);
                }
            }

            IStateTxfrOperationListener task = dbStateTxferMgrMap[operation.TaskIdentity.DBName];
            if (task != null)
            {
                return task.OnOperationRecieved(operation);
            }

            return null;
        }

        #endregion

       
        #region IStateTxfrStatusListener

        public void OnStateTxfrCompleted(IStateTransferOperation operation)
        {            
            if (operation != null && operation.TaskIdentity != null && operation.TaskIdentity.DBName != null)               
            {
                if (this.dbStateTxferMgrMap != null && dbStateTxferMgrMap.Count > 0)
                {
                    if (dbStateTxferMgrMap.ContainsKey(operation.TaskIdentity.DBName))
                    {
                        dbStateTxferMgrMap[operation.TaskIdentity.DBName].Status = StateTxfrStatus.CompletedSuccessfully;
                        if (dbStateTxferMgrMap[operation.TaskIdentity.DBName].IsLocal && dbStateTxferMgrMap[operation.TaskIdentity.DBName].TransferType == StateTransferType.INTRA_SHARD)
                        {
                            try
                            {
                                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                                    LoggerManager.Instance.StateXferLogger.Info("NodeStateTxfrMgr.OStateTxfrCompletion()", "Inter Shard State transfer of the migrated database " + operation.TaskIdentity.DBName + " completed successfully.");

                                context.ConfigurationSession.SetDatabaseMode(context.ClusterName, operation.TaskIdentity.DBName,
                                    DatabaseMode.Online);
                            }
                            catch (Exception exception)
                            {
                                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                    LoggerManager.Instance.StateXferLogger.Error("NodeStateTxfrMgr.OStateTxfrCompletion()", "Error changing databasemode to Promoted of database " + operation.TaskIdentity.DBName + "." + exception.Message);
                            }
                        }
                    }
                    VerifyNST();

                    //bool isComplete = true;
                    //foreach (var pair in dbStateTxferMgrMap)
                    //{
                    //    StateTxfrStatus status = pair.Value.Status;
                    //    if (status == StateTxfrStatus.Running || status == StateTxfrStatus.Failed || status == StateTxfrStatus.Waiting)
                    //    {
                    //        isComplete = false;
                    //        break;
                    //    }
                    //}

                    //if (isComplete)
                    //{
                    //    lock (_inStTxfrMutex)
                    //    {
                    //        _replication.NodeIsInStateTxfr = false;
                    //    }
                    //    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    //        LoggerManager.Instance.StateXferLogger.Info("NodeStateTxfrMgr.OStateTxfrCompletion()", "State transfer of the node " + this.context.LocalAddress + " completed successfully.");
                    //}
                }
            }
        }

        public void OnStateTxfrFailed(IStateTransferOperation operation)
        {            
            if (operation != null && operation.TaskIdentity != null && operation.TaskIdentity.DBName != null)
            {
                if (this.dbStateTxferMgrMap != null && dbStateTxferMgrMap.Count > 0)
                {
                    if (dbStateTxferMgrMap.ContainsKey(operation.TaskIdentity.DBName))
                    {
                        dbStateTxferMgrMap[operation.TaskIdentity.DBName].Status = StateTxfrStatus.Failed;                       
                    }
                }
            }
        }


        #endregion

        #region IAllocator Implementation

        public bool IsTaskAvailable
        {
            get;
            set;
        }

        private bool IsCompleted()         
        {
            lock (_inStTxfrMutex)
            {
                return !IsTaskAvailable;
            }
        }

        public void AllocateResource()
        {

            ////Step1: Check if any task remaining for allocation
            //if (IsCompleted())
            //{
            //    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
            //        LoggerManager.Instance.StateXferLogger.Info("NodeStateTxfrMgr.AllocateResource()", "State transfer of the node " + this.context.LocalAddress + " completed successfully.");
            //    return;
            //}

            //Step2: Allocate already task resoruces
            AllocateRunningDBs();

            //Step3: Free Resources for next allocation
            FreeResources();            

            ////Step1: Check if any task remaining for allocation
            //lock(waitingDBTasks)
            //{
            //    if(waitingDBTasks.Count==0)
            //    {
            //        IsTaskAvailable=false;
            //        return;
            //    }
            //}
            
            //Step4: Check if any slot avaialbe for waiting tasks
            if (runningDBTasks.Count >= MAX_RUNNING_DB_TASK)return;
            
            //Step5: Allocate resources to waiting tasks
            while(runningDBTasks.Count<MAX_RUNNING_DB_TASK && waitingDBTasks.Count > 0)
            {
                var firstNode=default(LinkedListNode<String>);
                
                lock(_schMutex)
                {
                    firstNode = waitingDBTasks.First;
                    if (firstNode != null)
                        waitingDBTasks.RemoveFirst();
                }

                if (firstNode != null && firstNode.Value!=null && dbStateTxferMgrMap.ContainsKey(firstNode.Value))
                {
                    var dbName = firstNode.Value;
                    IStateTransferTask task = null;
                   
                    lock (dbStateTxferMgrMap)
                    {
                        task = dbStateTxferMgrMap[dbName];
                    }

                    task.Start();
                    task.Status = StateTxfrStatus.Running;

                    lock(_schMutex)
                    {
                        runningDBTasks.AddLast(firstNode);
                    }
                }
            }
            
        }

        /// <summary>
        /// Verify NodeStateTransferManager if no task is running/waiting/failed then state transfer is compelted on this node.
        /// </summary>
        /// <returns></returns>
        private void VerifyNST()
        {
            bool isComplete = true;
            if (this.dbStateTxferMgrMap != null && dbStateTxferMgrMap.Count > 0)
            {

                foreach (var pair in dbStateTxferMgrMap)
                {
                    StateTxfrStatus status = pair.Value.Status;
                    if (status == StateTxfrStatus.Running || status == StateTxfrStatus.Failed || status == StateTxfrStatus.Waiting)
                    {
                        isComplete = false;
                        break;
                    }
                }

                if (isComplete)
                {
                    lock (_inStTxfrMutex)
                    {
                        //_replication.NodeIsInStateTxfr = false;
                        
                        IsTaskAvailable = false;
                    }

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info("NodeStateTxfrMgr.IsCompleted()", "State transfer of the node " + this.context.LocalAddress + " completed successfully.");
                }
            }
        }

        private void FreeResources()
        {
            try
            {
                if (dbStateTxferMgrMap.Count == 0 || runningDBTasks.Count == 0) return;

                List<String> runningTaskList = runningDBTasks.ToList<String>();
                List<KeyValuePair<String, DatabaseStateTransferManager>> completedTasks = new List<KeyValuePair<string, DatabaseStateTransferManager>>();

                foreach (var dbName in runningTaskList)
                {

                    if (dbStateTxferMgrMap.ContainsKey(dbName))
                    {
                        var dbTask = dbStateTxferMgrMap[dbName];
                        switch (dbTask.Status)
                        {
                            case StateTxfrStatus.CompletedSuccessfully:
                                {
                                    dbTask.Reset();
                                    runningDBTasks.Remove(dbName);
                                }
                                break;

                            //in case of stopped task should be enqueued again
                            case StateTxfrStatus.Failed:
                                dbTask.ReFT();
                                dbTask.Status = StateTxfrStatus.Waiting;

                                runningDBTasks.Remove(dbName);
                                waitingDBTasks.AddLast(dbName);
                                break;
                        }
                    }
                    else
                    {
                        runningDBTasks.Remove(dbName);
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("NodeStateTransferManaer.FreeResources", ex);
            }           
        }

        private void AllocateRunningDBs() 
        {
            if (runningDBTasks != null && runningDBTasks.Count > 0)
            {
                List<String> runningTaskList = runningDBTasks.ToList<String>();
                foreach(String db in runningTaskList)
                {
                    if (dbStateTxferMgrMap.ContainsKey(db))
                    {
                        DatabaseStateTransferManager dbMgr = dbStateTxferMgrMap[db];

                        if (dbMgr != null)
                            dbMgr.AllocateResource();
                    }
                    else 
                    {
                        runningDBTasks.Remove(db);
                    }
                }
            }
        }
        
        #endregion

        public StateTxfrStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void OnDropDatabase(String database)
        {
            IStateTransferTask task = null;
            lock (dbStateTxferMgrMap)
            {
                if (dbStateTxferMgrMap.ContainsKey(database))
                {
                    task = dbStateTxferMgrMap[database];
                    dbStateTxferMgrMap.Remove(database);
                }
            }

            if (task != null)
            {
                lock (_schMutex)
                {
                    //we don't know wether dropped database task is waiting or running so just remove it from both :)
                    
                    waitingDBTasks.Remove(database);
                    runningDBTasks.Remove(database);

                }
                task.Stop();
            }
        }

        public void OnDropCollection(String database, String collection)
        {
            DatabaseStateTransferManager task = null;
            lock (dbStateTxferMgrMap)
            {
                if (dbStateTxferMgrMap.ContainsKey(database))
                    task = dbStateTxferMgrMap[database];
            }

            if (task != null)
            {
                task.OnDropCollection(collection);
            }

        }
    }

    #region Resource Manager Task

    #endregion

    #region Resource Allocator Task

    #endregion

    #region IAllocator 

    #endregion

}
