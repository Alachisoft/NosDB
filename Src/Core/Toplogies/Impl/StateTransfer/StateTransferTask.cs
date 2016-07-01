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
using System.Text;
using System.Collections;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Common.Util;
using System.Threading;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Core.Statistics;
using Alachisoft.NosDB.Core.Toplogies.Exceptions;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Configuration.Services;
using System.Collections.Generic;

using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Configuration.Services;

#if DEBUGSTATETRANSFER
using Alachisoft.NCache.Caching.Topologies.History;
#endif

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    #region	/                 --- StateTransferTask ---           /

    /// <summary>
    /// State Tranfer job.
    /// </summary>
    internal class StateTransferTask:IStateTransferTask
    {

       protected StateTransferIdentity taskIdentity;

        /// <summary>
        /// Temporary make it dynamic 
        /// </summary>
        protected IDispatcher operationDispatcher = null;

        /// <summary> A promise object to wait on. </summary>
        protected Promise _promise = null;

        /// <summary> 10K is the threshold data size. Above this threshold value, data will be state
        /// transfered in chunks. </summary>
        //muds: temporarily we are disabling the bulk transfer of sparsed buckets.
        //in future we may need it back.
        protected long _threshold = 10 * 1000; 

        /// <summary>
        /// All the buckets that has less than threshold data size are sparsed.
        /// This is the list of sparsed bucket ids.
        /// </summary>
        protected ArrayList _sparsedBuckets = new ArrayList();

        /// <summary>
        /// All the buckets that has more than threshold data size are filled.
        /// This is the list of the filled buckted ids.
        /// </summary>
        protected ArrayList _filledBuckets = new ArrayList();        

        protected System.Threading.Thread _worker;

        protected bool _isRunning;
        protected object _stateTxfrMutex = new object();
        protected NodeContext _context;
		protected int _bktTxfrRetryCount = 3;
		protected ArrayList _correspondingShards = new ArrayList();
        /// <summary>Flag which determines that if sparsed buckets are to be transferred in bulk or not.</summary>
        protected bool _allowBulkInSparsedBuckets = false;        
        protected StateTransferType _trasferType = StateTransferType.INTER_SHARD;        

        /// <summary>
        /// Gets or sets a value indicating whether this task is for Balancing Data Load or State Transfer.
        /// </summary>
        protected bool _isBalanceDataLoad = false;

        /// <summary>
        /// Keep List of Failed keys on main node during state txfr
        /// </summary>
        /// 
        ArrayList failedKeysList = null;
        
        /// <summary>
        /// State Transfer Size is used to control the rate of data transfer during State Tranfer i.e per second tranfer rate in MB
        /// </summary>
        private static long MB = 1024 * 1024;
        protected long stateTxfrDataSizePerSecond = 50 * MB;        
        protected object _updateIdMutex = new object();
        private ThrottlingManager _throttlingManager;
        private bool _enableGc;
        private long _gcThreshhold = 1024 * MB * 2;//default is 2 Gb
        private long _dataTransferred;

        private StatsIdentity _statsIdentity;
        private string _dbName;
        protected int updateCount;
        private NodeIdentity requestingShard;

        private string prefix = "requesting";
        private string tokenizer = "_";
        private string bucketKeysColName;        

        protected string loggingModule;        

        protected IDatabasesManager _databasesManager = null;
        private ICollection _map;

        /// <summary>
        /// Constructor
        /// </summary>
        protected StateTransferTask()
        {
            _promise = new Promise();
        }
      
        protected virtual NodeContext Context
        {
            get { return _context; }
        }

        public StateTransferTask(NodeContext context, String dbName, String colName, IDispatcher operationDispatcher, DistributionMethod distributionType)
            : this(context, dbName, colName, operationDispatcher, StateTransferType.INTER_SHARD, distributionType) 
        {

        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public StateTransferTask(NodeContext context, String dbName, String colName, IDispatcher operationDispatcher, StateTransferType type, DistributionMethod distributionType)
		{
            this._context = context;
            _dbName = dbName;
            requestingShard = new NodeIdentity(_context.LocalShardName/*, _context.LocalAddress*/);

            this.taskIdentity = new StateTransferIdentity(dbName, colName, requestingShard, type, distributionType);          
            this.operationDispatcher = operationDispatcher;
            _statsIdentity = new StatsIdentity(context.LocalShardName, dbName);
			_promise = new Promise();

            //if (ConfigurationSettings<DBHostSettings>.Current.StateTransferDataSizePerSecond > 0)
            //    stateTxfrDataSizePerSecond = (long)(ConfigurationSettings<DBHostSettings>.Current.StateTransferDataSizePerSecond * MB);


            //if (ConfigurationSettings<DBHostSettings>.Current.EnableGCDuringStateTransfer)
            //    _enableGc = ConfigurationSettings<DBHostSettings>.Current.EnableGCDuringStateTransfer;
            
            _gcThreshhold = ConfigurationSettings<DBHostSettings>.Current.GCThreshold * MB;

            loggingModule = taskIdentity.DBName + ":" + taskIdentity.ColName + ":" + GetType().Name;

            _databasesManager = context.DatabasesManager;
		}

        #region IStateTransferTask




        public void Initialize(ICollection map, StateTransferType transferType, bool forLocal = false)
        {
            _map = map;
        }
        
        public void  Start()
        {
            DoStateTransfer(_map as ArrayList);
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public StateTxfrStatus Status
        {
            get;
            set;
        }

        private void StartDataTransfer()
        {
            string instanceName = this.ToString();
            _throttlingManager = new ThrottlingManager(stateTxfrDataSizePerSecond);
            _throttlingManager.Start();
            _worker = new System.Threading.Thread(new System.Threading.ThreadStart(Process));
            _worker.IsBackground = true;
            _worker.Start();
        }

        public void Stop()
        {
            lock (_stateTxfrMutex)
            {
                try
                {
                    if (_worker != null)
                    {
                        _worker.Abort();
                        _worker = null;
                    }
                }
                finally
                {
                    _sparsedBuckets.Clear();
                    _filledBuckets.Clear();
                }
            }
        }

        private void DoStateTransfer(ArrayList buckets)
        {
            int updateId = 0;

            lock (_updateIdMutex)
            {
                updateId = ++updateCount;
            }

            //No need for seperate thread to start task
            UpdateAsync(new object[] { buckets, updateId });
            //System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(UpdateAsync), new object[] { buckets, updateId });
        }

        private void UpdateAsync(object state)
        {
            try
            {
                object[] obj = state as object[];
                ArrayList buckets = obj[0] as ArrayList;
                int updateId = (int)obj[1];


                //+Numan@ 26102015
                //_parent.DetermineClusterStatus();
                //-Numan@ 26102015


                if (!UpdateStateTransfer(buckets, updateId))
                {
                    return;
                }

                //+Numan@ 26102015
                //if (_parent.HasDisposed)
                //    return;
                //+Numan@ 26102015


                if (!_isRunning)
                {
                    StartDataTransfer();
                }

                //+Numan @26102015

                #region Code For DataSource State transfer may be needed in case of queries for NOSDB
                //DataSourceReplicationManager dsRepMgr = null;

                //try
                //{
                //    ///Only in case of POR                    
                //    if (transferQueue && _parent is PartitionOfReplicasCacheBase && ((PartitionOfReplicasCacheBase)_parent).CurrentSubCluster != null)
                //    {
                //        dsRepMgr = new DataSourceReplicationManager(_parent, _parent.Context.DsMgr, _parent.Context.NCacheLog);
                //        dsRepMgr.ReplicateWriteBehindQueue();
                //    }
                //}
                //catch (Exception ex)
                //{
                //    _parent.Context.NCacheLog.Error(Name + ".UpdateAsync", "could not transfer queue " + ex.ToString());
                //}
                //finally
                //{
                //    if (dsRepMgr != null) dsRepMgr.Dispose();
                //}
                #endregion

                //+Numan@ 26102015

            }
            catch (Exception e)
            {

                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".UpdateAsync", e.ToString());
            }

        }




        /// <summary>
        /// Gets or sets a value indicating whether this StateTransfer task is initiated for Data balancing purposes or not.
        /// </summary>
        public bool IsBalanceDataLoad
        {
            get { return _isBalanceDataLoad; }
            set { _isBalanceDataLoad = value; }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }
        /// <summary>
        /// Do the state transfer now.
        /// </summary>
        protected virtual void Process()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = _dbName != null ? _dbName : "" });
            _isRunning = true;

            object result = null;
            bool colCreated = false;
            try
            {
                //Just to overcome Cluster Connection lag should be removed once cluster become stable
                Thread.Sleep(5000);

                colCreated = CreateBucketKeysCollection();
                if (!colCreated) return;
                //+Numan@ 26102015               

                #region Code for CQ state transfer

                //try
                //{
                //    if (_parent.IsCQStateTransfer)
                //    {
                //        _parent.Context.NCacheLog.CriticalInfo(Name + ".Process", "CQState transfer has started.");
                //        ContinuousQueryStateTransferManager cqStateTxfrMgr = new ContinuousQueryStateTransferManager(_parent, _parent.QueryAnalyzer);
                //        cqStateTxfrMgr.TransferState(_parent.Cluster.Coordinator);
                //        _parent.IsCQStateTransfer = false;
                //        _parent.Context.NCacheLog.CriticalInfo(Name + ".Process", "CQState transfer has ended.");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    _parent.Context.NCacheLog.Error(Name + ".Process", " Transfering Continuous Query State: " + ex.ToString());
                //}
                #endregion

                //-Numan@ 26102015
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".Process", "State Transfer has started.");

                BucketTxfrInfo info;
                while (true)
                {

                    lock (_stateTxfrMutex)
                    {
                        info = GetBucketsForTxfr();

                        //muds: 
                        //if no more data to transfer then stop.
                        if (info.end)
                        {
                            _isRunning = false;
                            break;
                        }
                    }

                    ArrayList bucketIds = info.bucketIds;
                    NodeIdentity ownerShard = info.ownerShard;
                    bool isSparsed = info.isSparsed;

                    if (bucketIds != null && bucketIds.Count > 0)
                    {

                        if (!_correspondingShards.Contains(ownerShard))
                        {
                            _correspondingShards.Add(ownerShard);

                            if (!CreateStateTxferCorresponder(ownerShard))
                            {
                                throw new StateTransferException("Corresponder creation failed on " + ownerShard);
                            }
                        }

                        TransferData(bucketIds, ownerShard, isSparsed);
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".Process", e.ToString());

                result = e;
            }
            finally
            {
                try
                {
                    if (result == null) result = 0;

                    if (colCreated)
                        DropBucketKeysCollection();

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".Process", " Ending state transfer with result : " + result.ToString());




                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                        LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Process", " Total Corresponding Nodes: " + _correspondingShards.Count);

                    foreach (NodeIdentity corNode in _correspondingShards)
                    {

                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Process", " Corresponding Node: " + corNode.ToString());
                        try
                        {
                            SignalEndOfStateTransfer(corNode);
                        }
                        catch (Exception ex)
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".Process(1)", ex.ToString());
                        }
                    }

                    _correspondingShards.Clear();

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                        LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Process(2)", " Finalizing state transfer");
                    try
                    {
                        FinalizeStateTransfer();
                    }
                    catch (Exception ex)
                    {
                          if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".FinalizeStateTransfer", ex.ToString());

                        FinalizeStateTransfer();
                    }

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                        LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Process(3)", "State transfer has ended");

                    //}


                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".Process(4)", ex.ToString());
                }
                finally 
                {
                    EndStateTransfer(result);
                }
            }
        }


        #endregion       


        private bool CreateStateTxferCorresponder(NodeIdentity ownerShard)
        {

            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.CreateCorresponder);
                        
            operation.Params.SetParamValue(ParamName.StateTransferType, _trasferType);
            operation.Params.SetParamValue(ParamName.OpDestination, ownerShard);
            
            if (operationDispatcher != null)
            {
                return (Boolean)operationDispatcher.DispatchOperation<Object>(operation);
            }

            return false;
           
        }

        private void DropBucketKeysCollection()
        {            
            //LocalDropCollectionOperation operation = new LocalDropCollectionOperation();
            //operation.Database = Common.MiscUtil.SYSTEM_DATABASE;
            //operation.Collection = bucketKeysColName;
            //if(_databasesManager!=null)
            //    _databasesManager.DropCollection(operation);
        }

        private bool CreateBucketKeysCollection()
        {            
            //bucketKeysColName = prefix + tokenizer + taskIdentity.DBName + tokenizer + taskIdentity.ColName;


            //LocalCreateCollectionOperation operation=new LocalCreateCollectionOperation();
            //operation.Database = operation.Database = Common.MiscUtil.SYSTEM_DATABASE;         
            //operation.Configuration = Alachisoft.NosDB.Core.Util.MiscUtil.GetBaseCollectionConfiguration(_context.LocalShardName,bucketKeysColName);
            //if (_databasesManager != null)
            //    _databasesManager.CreateCollection(operation);
            
            return true;
        }

        private void SignalEndOfStateTransfer(NodeIdentity corNode)
        {
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.DestroyCorresponder);
            operation.Params.SetParamValue(ParamName.OpDestination, corNode);

            if (operationDispatcher != null)
                operationDispatcher.DispatchOperation<Object>(operation);
        }

        //No need of result object as there is no use of it

        private bool IsAvoidable(Exception ex) 
        {
            if (ex == null) return true;

            StateTransferException statEx = ex as StateTransferException;
            if (statEx != null)
            { 
                switch(statEx.ErrorCode)
                {
                    case Common.ErrorHandling.ErrorCodes.StateTransfer.SHARD_UNAVAILABLE:  
                    case Common.ErrorHandling.ErrorCodes.StateTransfer.PRIMARY_CHANGED:
                        return false;
                }
            }

            return true;
        }

        private void EndStateTransfer(object result)
        {
            StateTransferOpCode opCode = StateTransferOpCode.StateTxferCompeleted;

            if (result is Exception && !IsAvoidable(result as Exception))
            {
                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                     LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".EndStateTransfer", " State transfer ended with Exception " + result.ToString());
                /// What to do? if we failed the state transfer?. Proabably we'll keep
                /// servicing in degraded mode? For the time being we don't!
                 opCode = StateTransferOpCode.StateTxferFailed;
            }

            IStateTransferOperation operation = this.CreateStateTransferOperation(opCode);

            if (taskIdentity != null && LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                LoggerManager.Instance.StateXferLogger.Info("ColStateTxfrTask.EndStateTransfer()", "State transfer of the collection " + taskIdentity.ColName + " completed with status "+ opCode.ToString());
            if (operationDispatcher != null)
                operationDispatcher.DispatchOperation<Object>(operation);   
        }

        protected virtual void FinalizeStateTransfer() 
        {
            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".FinalizeStateTransfer", " State transfer Finalized");

            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.FinalizeStateTransfer);

            if (operationDispatcher != null)
                operationDispatcher.DispatchOperation<Object>(operation);   
        }

		private void TransferData(int bucketId, NodeIdentity ownerShard,bool sparsedBucket)
		{
			ArrayList tmp = new ArrayList(1);
			tmp.Add(bucketId);
            TransferData(tmp, ownerShard, sparsedBucket);
		}

        protected virtual void TransferData(ArrayList bucketIds, NodeIdentity ownerShard, bool sparsedBuckets)
		{
			ArrayList ownershipChanged = null;
			ArrayList lockAcquired = null;
                        
			//muds:
			//ask coordinator node to lock this/these bucket(s) during the state transfer.
            Hashtable lockResults = AcquireLockOnBuckets(bucketIds, requestingShard);

			if (lockResults != null)
			{
				ownershipChanged = (ArrayList)lockResults[BucketLockResult.OwnerChanged];
				if (ownershipChanged != null && ownershipChanged.Count > 0)
				{
					//muds:
					//remove from local buckets. remove from sparsedBuckets. remove from filledBuckets.
					//these are no more my property.
					IEnumerator ie = ownershipChanged.GetEnumerator();
					while (ie.MoveNext())
					{
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferData", " " + ie.Current.ToString() + " ownership changed");
#if DEBUGSTATETRANSFER
                        _parent.Cluster._history.AddActivity(new Activity("StateTransferTask.TransferData Ownership changed of bucket " + ie.Current.ToString() + ". Should be removed from local buckets."));
#endif
                        Dictionary<int, Common.Stats.BucketStatistics> localBuckets = GetBucketStats();

						if (localBuckets.ContainsKey((int)ie.Current))
						{
                            lock (((ICollection)localBuckets).SyncRoot)
							{
                                localBuckets.Remove((int)ie.Current);								
							}
						}
					}
				}

				lockAcquired = (ArrayList)lockResults[BucketLockResult.LockAcquired];
				if (lockAcquired != null && lockAcquired.Count > 0)
				{
#if DEBUGSTATETRANSFER
                    _parent.Cluster._history.AddActivity(new Activity("StateTransferTask.TransferData Announcing state transfer for bucket " + lockAcquired[0].ToString() + "."));
#endif
                    failedKeysList = new ArrayList();
                    AnnounceBucketTxfer(lockAcquired);
                    //bool successfull= TransferBucketsKeys(lockAcquired,ownerShard);
                    
                    //if (!successfull)
                    //{
                    //    if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    //    {
                    //        LoggerManager.Instance.StateXferLogger.Error("error while transfering keys for bucket " + lockAcquired[0]);
                    //    }
                    //}

                    bool bktsTxfrd = TransferBucketsData(lockAcquired, ref ownerShard, sparsedBuckets);

                    ReleaseBucketsOnCS(lockAcquired);                   
				}
			}
			else
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".TransferData", " Lock acquisition failure");
		}

        private bool TransferBucketsKeys(ArrayList buckets, NodeIdentity ownerShard)
		{
			bool transferEnd;
			bool successfullyTxfrd = false;
			int expectedTxfrId = 1;
            bool resync = false;
            try
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", " Starting transfer. Owner : " + ownerShard.ToString() + " , Bucket : " + ((int)buckets[0]).ToString());                                                      
                long currentIternationData = 0;
                int bucketKeysCount = 0;
                while (true)
                {

                    if (_enableGc && _dataTransferred >= _gcThreshhold)
                    {
                        _dataTransferred = 0;
                        DateTime start = DateTime.Now;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        DateTime end = DateTime.Now;
                        TimeSpan diff = end - start;
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBucket", "explicit GC called. time taken(ms) :" + diff.TotalMilliseconds + " gcThreshold :" + _gcThreshhold);
                    }
                    else
                        _dataTransferred += currentIternationData;

                    resync = false;
                    transferEnd = true;
                    StateTxfrInfo info = null;
                    try
                    {
                        currentIternationData = 0;
                        info = SafeTransferBucketKeys(buckets,ownerShard, expectedTxfrId);

                        if (info != null)
                        {
                            currentIternationData = info.DataSize;
                        }
                    }
                    catch (SuspectedException)
                    {
                        resync = true;
                    }
                    catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException ex)
                    {

                        if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        {
                            LoggerManager.Instance.StateXferLogger.Error("StateTransferTask.TransferBucketKeys() ","error while transfering keys for bucket " + buckets[0]+" " + ex.Message);
                        }
                        //resync = true;
                    }
                    finally 
                    {
                        
                    }

                    if (resync)
                    {
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", ownerShard + " is suspected");
                        NodeIdentity changedOwner = GetChangedOwner((int)buckets[0], ownerShard);

                        if (changedOwner != null)
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", changedOwner + " is new owner");

                            if (changedOwner.Equals(ownerShard))
                            {
                                continue;
                            }
                            else
                            {
                                ownerShard = changedOwner;
                                expectedTxfrId = 1;
                                continue;
                            }

                        }
                        else
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", " Could not get new owner");
                            info = new StateTxfrInfo(true);
                        }
                    }

                    if (info != null)
                    {
                        successfullyTxfrd = true;
                        transferEnd = info.transferCompleted;                        
                        HashVector tbl = info.data as HashVector;
                        DocumentKey docKey = null;

                        //next transfer 
                        expectedTxfrId++;

                        if (tbl != null && tbl.Count > 0)
                        {
                            IDictionaryEnumerator ide = tbl.GetEnumerator();
                            while (ide.MoveNext())
                            {
                                
                                try
                                {                            
                                    if (ide.Value != null)
                                    {
                                        List<DocumentKey> keys = ide.Value as List<DocumentKey>;
                                        Int32? bucketId=ide.Key as Int32?;

                                        if (keys != null && keys.Count>0)
                                        {
                                            IDocumentsWriteOperation operation = new LocalInsertOperation();
                                            operation.Database = Common.MiscUtil.SYSTEM_DATABASE;
                                            operation.Collection = bucketKeysColName;
                                            operation.Documents =new System.Collections.Generic.List<IJSONDocument>();
                                            operation.OperationType = Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType.StateTransferInsert;

                                            IEnumerator keysEnum = keys.GetEnumerator();
                                            while (keysEnum.MoveNext())
                                            {
                                                docKey = keysEnum.Current as DocumentKey;

                                                if (docKey != null)
                                                {
                                                    operation.Documents.Add(GetStateTransferKeyJSONDocument(bucketId.Value, docKey));                                                    
                                                }
                                            }

                                            IDocumentsWriteResponse resposne = null;
                                            if (_databasesManager != null)
                                            {
                                                resposne = _databasesManager.InsertDocuments(operation);
                                            }

                                            if (resposne != null && !resposne.IsSuccessfull)
                                            {
                                                failedKeysList.Add(ide.Key);
                                            }

                                        }
                                        
                                        bucketKeysCount += keys == null ? 0 : keys.Count;                                        
                                    }                                 
                                    
                                }
                                catch (StateTransferException se)
                                {
                                     if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                         LoggerManager.Instance.StateXferLogger.Error(loggingModule +  ".TransferBuckets", " Can not add/remove key = " + ide.Key + " : value is " + ((ide.Value == null) ? "null" : " not null") + " : " + se.Message);
                                }
                                catch (Exception e)
                                {
                                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".TransferBuckets", " Can not add/remove key = " + ide.Key + " : value is " + ((ide.Value == null) ? "null" : " not null") + " : " + e.Message);
                                }
                            }

                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", " BalanceDataLoad = " + _isBalanceDataLoad.ToString());                        
                        }
                    }
                    else
                        successfullyTxfrd = false;

                    if (transferEnd)
                    {
                        if (LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                        {
                            LoggerManager.Instance.StateXferLogger.Debug("StateXfer", "TransferBucketsKeys() - BucketId: " + buckets[0] + " keys : " + bucketKeysCount);
                        }

                        break;
                    }

                    if (info != null)
                        _throttlingManager.Throttle(info.DataSize);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                EndBucketsStateTxfr(buckets);
                throw;
            }
			return successfullyTxfrd;
		}

        private IJSONDocument GetStateTransferKeyJSONDocument(int? bucketId,DocumentKey key) 
        {                       
            StateTransferKey stateTransferKey = new StateTransferKey(bucketId.Value,key);
            return JsonSerializer.Serialize <StateTransferKey>(stateTransferKey);
        }

        private StateTxfrInfo SafeTransferBucketKeys(ArrayList buckets, NodeIdentity ownerShard, int expectedTxfrId)
        {
			StateTxfrInfo info = null;
			int retryCount = _bktTxfrRetryCount;

			while (retryCount > 0)
			{
				try
				{
                    //NTD:[High] need to broadcast these keys to shard for primary down scenarios 
                    info = TransferBucketKeys(buckets, ownerShard, expectedTxfrId);
					return info;
				}
				catch (Exceptions.SuspectedException)
				{
					//Member with which we were doing state txfer has left.
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransferBucketKeys", " " + ownerShard + " is suspected during state transfer");
					foreach (int bucket in buckets)
					{
						try
                        {
                            EmptyBucket(bucket);                            
						}
						catch (Exception e)
						{
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransferBucketKeys", e.ToString());
						}
					}
                    throw;	
				}				
                catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException tout_e)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransferBucketKeys", " State transfer request timed out from " + ownerShard);
                    retryCount--;
                    if (retryCount <= 0)
                        throw;
                }
				catch (Exception e)
				{
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransferBucketKeys", " An error occurred during state transfer " + e.ToString());
					break;
				}
			}
			return info;
		}
        
        protected virtual void EndBucketsStateTxfr(ArrayList buckets) { }

        protected virtual void AnnounceBucketTxfer(ArrayList buckets)
        {
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.AnnounceBucketTxfer);            
            operation.Params.SetParamValue(ParamName.BucketList, buckets);

            if (operationDispatcher != null)
                operationDispatcher.DispatchOperation<Object>(operation);
        }

        protected virtual void ReleaseBucketsOnCS(ArrayList lockedBuckets)
        {
            if (operationDispatcher != null)
            {
                IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.ReleaseBucketsOnCS);
                operation.Params.SetParamValue(ParamName.BucketList, lockedBuckets);
                operation.Params.SetParamValue(ParamName.BucketFinalShard, requestingShard);               

                DateTime start = DateTime.Now;
                
                operationDispatcher.DispatchOperation<Object>(operation);
                
                TimeSpan t = DateTime.Now - start;

                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug("StateTransferTask.ReleaseBuckets", "time taken[Milliseconds] : " + t.TotalMilliseconds);
            }
        }
		/// <summary>
		/// Transfers the buckets from a its owner. We may receive data in chunks.
		/// It is a pull model, a node wanting state transfer a bucket makes request
		/// to its owner.
		/// </summary>
		/// <param name="buckets"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		private bool TransferBucketsData(ArrayList buckets,ref NodeIdentity owner,bool sparsedBuckets)
		{
			bool transferEnd;
			bool successfullyTxfrd = false;
			int expectedTxfrId = 1;
            bool resync = false;
            try
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", " Starting transfer. Owner : " + owner.ToString() + " , Bucket : " + ((int)buckets[0]).ToString());
                //StartBucketLoggingLocally(buckets);
                long dataRecieved = 0;
                long currentIternationData = 0;

                while (true)
                {

                    if (_enableGc && _dataTransferred >= _gcThreshhold)
                    {
                        _dataTransferred = 0;
                        DateTime start = DateTime.Now;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        DateTime end = DateTime.Now;
                        TimeSpan diff = end - start;
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBucket", "explicit GC called. time taken(ms) :" + diff.TotalMilliseconds + " gcThreshold :" + _gcThreshhold);
                    }
                    else
                        _dataTransferred += currentIternationData;

                    Boolean sleep = false;
                    resync = false;
                    transferEnd = true;
                    StateTxfrInfo info = null;
                    try
                    {
#if DEBUGSTATETRANSFER
                        _parent.Cluster._history.AddActivity(new StateTxferActivity((int)buckets[0], owner, expectedTxfrId));
#endif

                        currentIternationData = 0;
                        info = SafeTransferBucketData(buckets, owner, sparsedBuckets, expectedTxfrId);

                        if (info != null)
                        {
                            currentIternationData = info.DataSize;
                            dataRecieved += info.DataSize;
                        }
                    }
                    catch (SuspectedException)
                    {
                        resync = true;
                    }
                    catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException)
                    {
                        //resync = true;
                    }
                    finally 
                    {
                        
                    }

                    if (resync)
                    {
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", owner + " is suspected");
                        NodeIdentity changedOwner = GetChangedOwner((int)buckets[0], owner);

                        if (changedOwner != null)
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", changedOwner + " is new owner");

#if DEBUGSTATETRANSFER
                            _parent.Cluster._history.AddActivity(new Activity("Owner changed. Bucket : " + (int)buckets[0] + ", Owner : " + changedOwner.ToString()));
#endif
                            if (changedOwner.Equals(owner))
                            {
                                continue;
                            }
                            else
                            {
                                owner = changedOwner;
                                expectedTxfrId = 1;
                                continue;
                            }

                        }
                        else
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", " Could not get new owner");
                            info = new StateTxfrInfo(true);
                        }
                    }

                    if (info != null)
                    {
                        successfullyTxfrd = true;
                        transferEnd = info.transferCompleted;
                        IJSONDocument document = null;

                        //next transfer 
                        expectedTxfrId++;
                        //muds:
                        //add data to local
                        if (!info.loggedData)
                        {
                            HashVector tbl = info.data as HashVector;

                            if (tbl != null && tbl.Count > 0)
                            {
                                try
                                {
                                    System.Collections.Generic.List<IJSONDocument> documents = new System.Collections.Generic.List<IJSONDocument>();

                                    IDocumentsWriteOperation operation = new LocalInsertOperation();
                                    operation.Database = this.taskIdentity.DBName;
                                    operation.Collection = this.taskIdentity.ColName;
                                    operation.Documents = documents;
                                    operation.OperationType = Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType.StateTransferInsert;

                                    // During Intra-Shard State Transfer operation will
                                    if (this._trasferType==StateTransferType.INTRA_SHARD)
                                    {
                                        operation.Context.Add(Common.Enum.ContextItem.DoNotLog, true);
                                    }

                                    IDictionaryEnumerator ide = tbl.GetEnumerator();
                                    while (ide.MoveNext())
                                    {
                                        if (ide.Value != null)
                                        {
                                            document = ide.Value as IJSONDocument;

                                            if (document != null)
                                            {
                                                documents.Add(document);
                                            }
                                        }
                                    }

                                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                        LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBucketsData", " Starting transfer. Owner : " + owner.ToString() + " , Bucket : " + ((int)buckets[0]).ToString()+", Total Documents: "+documents.Count);
                                    IDocumentsWriteResponse resposne = null;
                                    if (_databasesManager != null)
                                    {
                                        resposne = _databasesManager.InsertDocuments(operation);
                                    }

                                    if (resposne != null && !resposne.IsSuccessfull && resposne.FailedDocumentsList != null)
                                    {
                                        failedKeysList.AddRange(resposne.FailedDocumentsList);
                                    }

                                }
                                catch (StateTransferException se)
                                {
                                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".TransferBuckets", " Can not add/remove key = " /*+ ide.Key + " : value is " + ((ide.Value == null) ? "null" : " not null") */+ " : " + se);
                                }
                                catch (Exception e)
                                {
                                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".TransferBuckets", " Can not add/remove key = " /*+ ide.Key + " : value is " + ((ide.Value == null) ? "null" : " not null") */+ " : " + e);
                                }


                                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                    LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", " BalanceDataLoad = " + _isBalanceDataLoad.ToString());

                                if (_isBalanceDataLoad)
                                    if (StatsManager.Instance.GetStatsCollector(_statsIdentity) != null)
                                        StatsManager.Instance.GetStatsCollector(_statsIdentity).IncrementStatsValue(StatisticsType.DataBalancePerSec, tbl.Count);
                                    else
                                        if (StatsManager.Instance.GetStatsCollector(_statsIdentity) != null)
                                            StatsManager.Instance.GetStatsCollector(_statsIdentity).IncrementStatsValue(StatisticsType.StateTransferPerSec, tbl.Count);

                            }
                        }
                        else 
                        {
                            ClusteredArrayList tbl = info.data as ClusteredArrayList;
                            
                            if(tbl!=null && tbl.Count>0)
                                ApplyLogOperation(tbl);
                        }
                    }
                    else
                        successfullyTxfrd = false;

                    if (transferEnd)
                    {
                        BucketsTransfered(owner, buckets);
                        EndBucketsStateTxfr(buckets);
                        //muds:
                        //send ack for the state transfer over.
                        //Ask every node to release lock on this/these bucket(s)

                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferBuckets", "Acknowledging transfer. Owner : " + owner.ToString() + " , Bucket : " + ((int)buckets[0]).ToString());

#if DEBUGSTATETRANSFER
                        _parent.Cluster._history.AddActivity(new Activity("Acknowledging transfer. Owner : " + owner.ToString() + " , Bucket : " + ((int)buckets[0]).ToString()));
#endif
                        AckBucketTxfer(owner, buckets);
                        break;
                    }

                    if (info != null)
                        _throttlingManager.Throttle(info.DataSize);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                EndBucketsStateTxfr(buckets);
                throw;
            }
			return successfullyTxfrd;

		}

        private void ApplyLogOperation(ClusteredArrayList logOperations)
        {
            if (this.operationDispatcher != null)
            { 
                IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.ApplyLogOperations);
                operation.Params.SetParamValue(ParamName.LogOperations,logOperations);
                operationDispatcher.DispatchOperation<Object>(operation);
            }
        }
        /*
        private void ApplyLogOperation(Alachisoft.NosDB.Common.Server.Engine.Impl.DocumentLogOperation logOperation)
        {
            switch (logOperation.LogOperationType)
            {
                case Common.Server.Engine.Impl.LogOperationType.Add:
                    {
                        
                        break;
                    }
                case Common.Server.Engine.Impl.LogOperationType.Remove:
                    {
                        IJSONDocument jdoc = logOperation.Document;
                        jdoc.Key = logOperation.DocumentKey.Value as string;

                        System.Collections.Generic.List<IJSONDocument> documents = new System.Collections.Generic.List<IJSONDocument>();
                        documents.Add(jdoc);


                        LocalDeleteOperation operation = new LocalDeleteOperation();
                        operation.Database = this.taskIdentity.DBName;
                        operation.Collection = this.taskIdentity.ColName;
                        operation.Documents = documents;
                        operation.OperationType = Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType.StateTransferInsert;
                        if (_databasesManager != null)
                        {
                            IDocumentsWriteResponse resposne = _databasesManager.DeleteDocuments(operation);
                        }
                                                
                        break;
                    }

                case Common.Server.Engine.Impl.LogOperationType.Update:
                    {
                        System.Collections.Generic.List<IJSONDocument> documents = new System.Collections.Generic.List<IJSONDocument>();
                        documents.Add(logOperation.Document);
                        IDocumentsWriteOperation operation = new LocalInsertOperation();
                        operation.Database = this.taskIdentity.DBName;
                        operation.Collection = this.taskIdentity.ColName;
                        operation.Documents = documents;
                        operation.OperationType = Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType.StateTransferInsert;
                        if (_databasesManager != null)
                        {

                            IDocumentsWriteResponse resposne = _databasesManager.InsertDocuments(operation);
                        }

                        break;
                    }
            }
        }
        */
        public virtual void AckBucketTxfer(NodeIdentity owner, ArrayList buckets)
        {
            if (owner != null)
            {
                IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.AckBucketTxfer);
                operation.Params.SetParamValue(ParamName.OpDestination, owner);
                operation.Params.SetParamValue(ParamName.BucketList, buckets);

                DateTime start = DateTime.Now;

                if (operationDispatcher != null)
                    operationDispatcher.DispatchOperation<Object>(operation);

                TimeSpan t = DateTime.Now - start;

                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug("StateTransferTask.AckBucketTxfer", "time taken[Milliseconds] : " + t.TotalMilliseconds);          
            }
        }

		/// <summary>
		/// Safely transfer a buckets from its owner. In case timeout occurs we
		/// retry once again.
		/// </summary>
		/// <param name="buckets"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		private StateTxfrInfo SafeTransferBucketData(ArrayList buckets, NodeIdentity owner,bool sparsedBuckets,int expectedTxfrId)
		{
			StateTxfrInfo info = null;
			int retryCount = _bktTxfrRetryCount;

			while (retryCount > 0)
			{
                try
                {
                    info = TransferBucketData(buckets, owner, _trasferType, sparsedBuckets, expectedTxfrId, _isBalanceDataLoad);
                    return info;
                }
                catch (StateTransferException ex)
                {
                    //Member with which we were doing state txfer has left.
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransterBucket",Common.ErrorHandling.ErrorMessages.GetErrorMessage(ex.ErrorCode,ex.Parameters));
                    foreach (int bucket in buckets)
                    {
                        try
                        {
                            EmptyBucket(bucket);
                        }
                        catch (Exception e)
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                                LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransterBucket", e.ToString());
                        }
                    }
                    throw;
                }
                catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException tout_e)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransterBucket", " State transfer request timed out from " + owner);
                    retryCount--;
                    if (retryCount <= 0)
                        throw;
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".SafeTransterBucket", " An error occurred during state transfer " + e.ToString());
                    break;
                }
			}
			return info;
		}

        private StateTxfrInfo TransferBucketData(ArrayList buckets, NodeIdentity bucketCurrentShard, StateTransferType _trasferType, bool sparsedBuckets, int expectedTxfrId, bool _isBalanceDataLoad)
        {           
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.TransferBucketData);

            operation.Params.SetParamValue(ParamName.BucketList, buckets);
            operation.Params.SetParamValue(ParamName.OpDestination, bucketCurrentShard);
            operation.Params.SetParamValue(ParamName.StateTransferType, _trasferType);
            operation.Params.SetParamValue(ParamName.SparsedBuckets, sparsedBuckets);
            operation.Params.SetParamValue(ParamName.TransferId,expectedTxfrId);
            operation.Params.SetParamValue(ParamName.IsBalanceDataLoad, _isBalanceDataLoad);

            DateTime start = DateTime.Now;

            StateTxfrInfo info = null;
            if (operationDispatcher != null)
            {
                info= operationDispatcher.DispatchOperation<StateTxfrInfo>(operation);
            }

            TimeSpan t = DateTime.Now - start;

            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                LoggerManager.Instance.StateXferLogger.Debug("StateTransferTask.TransferBucketData", "time taken[Milliseconds] : " + t.TotalMilliseconds);

            return info;
        }

        private StateTxfrInfo TransferBucketKeys(ArrayList buckets, NodeIdentity bucketCurrentShard,int expectedTxfrId)
        {           
            IStateTransferOperation operation = CreateStateTransferOperation(StateTransferOpCode.TransferBucketKeys);
            operation.Params.SetParamValue(ParamName.BucketList, buckets);
            operation.Params.SetParamValue(ParamName.OpDestination, bucketCurrentShard);                        
            operation.Params.SetParamValue(ParamName.TransferId, expectedTxfrId);

            if (operationDispatcher != null)
            {
                return operationDispatcher.DispatchOperation<StateTxfrInfo>(operation);                
            }

            return null;
        }

        private IEnumerator<DocumentKey> GetBucketKeysFilterEnumerator(int _currentBucket)
        {
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.GetBucketKeysFilterEnumerator);
            operation.Params.SetParamValue(ParamName.BucketID, _currentBucket);
            //operation.Params.SetParamValue(ParamName.EnableLog, false);
            return operationDispatcher.DispatchOperation<IEnumerator<DocumentKey>>(operation);

        }

        private void EmptyBucket(int bucket)
        {
            try
            {
                var bucketKeys = GetBucketKeys(bucket);
                if (bucketKeys != null && bucketKeys.Count>0)
                    BucketRemoval.Execute(new BucketRemovalInfo(this.taskIdentity.DBName, this.taskIdentity.ColName, bucket, bucketKeys.GetEnumerator()), _databasesManager, false);
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                    LoggerManager.Instance.StateXferLogger.Error(loggingModule, ex.Message);
            }
        }

        private ClusteredList<DocumentKey> GetBucketKeys(int _currentBucket)
        {
            if (this.operationDispatcher != null)
            {
                IStateTransferOperation operation = CreateStateTransferOperation(StateTransferOpCode.GetBucketKeys);
                operation.Params.SetParamValue(ParamName.BucketID, _currentBucket);

                return operationDispatcher.DispatchOperation<ClusteredList<DocumentKey>>(operation);
            }
            return new ClusteredList<DocumentKey>();
        }

		/// <summary>
		/// Acquire locks on the buckets.
		/// </summary>
		/// <param name="buckets"></param>
		/// <returns></returns>
        protected virtual Hashtable AcquireLockOnBuckets(ArrayList buckets, NodeIdentity finalShard)
		{
			int maxTries = 3;
			while (maxTries > 0)
			{
				try
				{
                    Hashtable lockResults = LockBucketsOnCS(buckets, finalShard);
					return lockResults;
				}
				catch (Exception e)
				{
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".AcquireLockOnBuckets", "could not acquire lock on buckets. error: " + e.ToString());
					maxTries--;
				}
			}
			return null;
		}

        private Hashtable LockBucketsOnCS(ArrayList buckets, NodeIdentity finalShard)
        {
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.LockBucketsOnCS);            
            operation.Params.SetParamValue(ParamName.BucketList, buckets);
            operation.Params.SetParamValue(ParamName.BucketFinalShard, finalShard);

            DateTime start = DateTime.Now;
            Hashtable lockedBuckets = null;
            if (operationDispatcher != null)
                lockedBuckets = operationDispatcher.DispatchOperation<Hashtable>(operation);

            TimeSpan t = DateTime.Now - start;

            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                LoggerManager.Instance.StateXferLogger.Debug("StateTransferTask.LockBucketOnCS", "time taken[Milliseconds] : " + t.TotalMilliseconds);


            return lockedBuckets;
        }

        public virtual BucketTxfrInfo GetBucketsForTxfr()
        {
            ArrayList bucketIds = null;
            NodeIdentity owner = null;
            int bucketId;
            ArrayList filledBucketIds = null;
            bool isSuccessfull = true;
            if (_sparsedBuckets != null && _sparsedBuckets.Count > 0)
            {
                lock (_sparsedBuckets.SyncRoot)
                {
                    int iterations = _sparsedBuckets.Count;

                    for (int index = 0; index < iterations; index++)
                    {
                        BucketsPack bPack = _sparsedBuckets[index] as BucketsPack;

                        if (bPack.Movable)
                        {
                            owner = bPack.Owner;
                            bucketIds = bPack.BucketIds;
                            if (_allowBulkInSparsedBuckets)
                            {
                                //_sparsedBuckets.Remove(bPack);
                                return new BucketTxfrInfo(bucketIds, true, owner);
                            }
                            else
                            {
                                ArrayList list = new ArrayList();
                                list.Add(bucketIds[0]);
                                //Although it is from the sparsed bucket but we intentionally set flag as non-sparsed.
                                return new BucketTxfrInfo(list, false, owner);
                            }
                        }
                        else if (bPack.BucketIds != null && bPack.BucketIds.Count > 0)
                        {
                            isSuccessfull = false;
                        }
                    }
                }
            }

            if (_filledBuckets != null && _filledBuckets.Count > 0)
            {
                lock (_filledBuckets.SyncRoot)
                {
                    int iterations = _filledBuckets.Count;

                    for (int index = 0; index < iterations; index++)
                    {
                        BucketsPack bPack = _filledBuckets[index] as BucketsPack;

                        if (bPack.Movable)
                        {
                            owner = bPack.Owner;
                            filledBucketIds = bPack.BucketIds;
                            if (filledBucketIds != null && filledBucketIds.Count > 0)
                            {
                                bucketId = (int)filledBucketIds[0];
                                //filledBucketIds.Remove(bucketId);

                                //if (filledBucketIds.Count == 0)
                                //    _filledBuckets.Remove(bPack);

                                bucketIds = new ArrayList(1);
                                bucketIds.Add(bucketId);
                                return new BucketTxfrInfo(bucketIds, false, owner);
                            }
                        }
                        else if (bPack.BucketIds != null && bPack.BucketIds.Count > 0)
                        {
                            isSuccessfull = false;
                        }
                    }
                }
            }

            if (!isSuccessfull)
            {
                StateTransferException ex = new StateTransferException("source shard is not available");
                ex.ErrorCode = Common.ErrorHandling.ErrorCodes.StateTransfer.SHARD_UNAVAILABLE;

                throw ex;
            }

            return new BucketTxfrInfo(true);
        }

        /// <summary>
        /// Removes the buckets from the list of transferable buckets after we have
        /// transferred them.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="buckets"></param>
        /// <param name="sparsed"></param>
        protected void BucketsTransfered(NodeIdentity owner,ArrayList buckets)
        {
            BucketsPack bPack = null;
            lock (_stateTxfrMutex)
            {
                if (_sparsedBuckets != null)
                {
                    BucketsPack dummy = new BucketsPack(null, owner);
                    int index = _sparsedBuckets.IndexOf(dummy);
                    if (index != -1)
                    {
                        bPack = _sparsedBuckets[index] as BucketsPack;
                        foreach (int bucket in buckets)
                        {
                            bPack.BucketIds.Remove(bucket);
                        }
                        if (bPack.BucketIds.Count == 0)
                            _sparsedBuckets.RemoveAt(index);
                    }
                }
                if (_filledBuckets != null)
                {
                    BucketsPack dummy = new BucketsPack(null, owner);
                    int index = _filledBuckets.IndexOf(dummy);
                    if (index != -1)
                    {
                        bPack = _filledBuckets[index] as BucketsPack;
                        foreach (int bucket in buckets)
                        {
                            bPack.BucketIds.Remove(bucket);
                        }
                        if (bPack.BucketIds.Count == 0)
                            _filledBuckets.RemoveAt(index);
                    }
                }

                SetBucketStatus(taskIdentity.NodeInfo, buckets);             
            }
        }



        /// <summary>
        /// Set Status of transfered bucket locally        
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="buckets"></param>
        /// <param name="sparsed"></param>
        protected void SetBucketStatus(NodeIdentity owner, ArrayList buckets)
        {
            if (this.operationDispatcher != null)
            {
                IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.SetBucketStatus);
                operation.Params.SetParamValue(ParamName.BucketList, buckets);
                operation.Params.SetParamValue(ParamName.BucketStatus, BucketStatus.Functional);
                operation.Params.SetParamValue(ParamName.BucketFinalShard, taskIdentity.NodeInfo);
                operation.Params.SetParamValue(ParamName.IsSource, false);
                operationDispatcher.DispatchOperation<Object>(operation);
            }            
        }

        /// <summary>
		/// Updates the state transfer task in synchronus way. It adds/remove buckets
		/// to be transferred by the state transfer task.
		/// </summary>
		/// <param name="myBuckets"></param>
        public virtual bool UpdateStateTransfer(ArrayList myBuckets, int updateId)
		{
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = Context.LocalShardName != null ? Context.LocalShardName : "", DatabaseName =_dbName !=null?_dbName:"" });
            try
            {

                if (_databasesManager != null && _databasesManager.HasDisposed(taskIdentity.DBName, taskIdentity.ColName)/* _parent.HasDisposed*/)
                    return false;
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                    LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".UpdateStateTxfr", "UpdateStateTxfer failed due to " + ex.Message);
                return false;
            }

            StringBuilder sb = new StringBuilder();
            lock (_updateIdMutex)
            {
                if (updateId != updateCount)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".UpdateStateTxfr", " Do not need to update the task as update id does not match; provided id :" + updateId + " currentId :" + updateCount);
                    return false;
                }
            }

            lock (_stateTxfrMutex)
            {
                try
                {
                    if (myBuckets != null)
                    {
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                            LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".UpdateStateTxfr", " my buckets " + myBuckets.Count);
                        //we work on the copy of the map.
                        ArrayList buckets = myBuckets.Clone() as ArrayList;
                        ArrayList leavingShards = new ArrayList();

                        if (_sparsedBuckets != null && _sparsedBuckets.Count > 0)
                        {
                            //ArrayList tmp = _sparsedBuckets.Clone() as ArrayList;
                            IEnumerator e = _sparsedBuckets.GetEnumerator();

                            lock (_sparsedBuckets.SyncRoot)
                            {
                                while (e.MoveNext())
                                {
                                    BucketsPack bPack = (BucketsPack)e.Current;
                                    ArrayList bucketIds = bPack.BucketIds.Clone() as ArrayList;
                                    foreach (int bucketId in bucketIds)
                                    {
                                        HashMapBucket current = new HashMapBucket(null, bucketId);

                                        if (!buckets.Contains(current))
                                        {
                                            ((BucketsPack)e.Current).BucketIds.Remove(bucketId);
                                        }
                                        else
                                        {
                                            HashMapBucket bucket = buckets[buckets.IndexOf(current)] as HashMapBucket;
                                            if (!bPack.Owner.Equals(new NodeIdentity(bucket.CurrentShard /*,GetShardPrimary(bucket.CurrentShard)*/)))
                                            {
                                                //either i have become owner of the bucket or 
                                                //some one else for e.g a replica node 
                                                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                                    LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".UpdateStateTxfer", bucket.BucketId + "bucket owner changed old :" + bPack.Owner + " new :" + bucket.CurrentShard);
                                                bPack.BucketIds.Remove(bucketId);
                                            }
                                        }
                                    }
                                    if (bPack.BucketIds.Count == 0)
                                    {
                                        //This owner has left.
                                        leavingShards.Add(bPack.Owner);
                                    }

                                }
                                foreach (NodeIdentity leavingShard in leavingShards)
                                {
                                    BucketsPack bPack = new BucketsPack(null, leavingShard);
                                    _sparsedBuckets.Remove(bPack);
                                }
                                leavingShards.Clear();
                            }
                        }

                        if (_filledBuckets != null && _filledBuckets.Count > 0)
                        {
                            //ArrayList tmp = _filledBuckets.Clone() as ArrayList;
                            IEnumerator e = _filledBuckets.GetEnumerator();
                            lock (_filledBuckets.SyncRoot)
                            {
                                while (e.MoveNext())
                                {
                                    BucketsPack bPack = (BucketsPack)e.Current;
                                    ArrayList bucketIds = bPack.BucketIds.Clone() as ArrayList;
                                    foreach (int bucketId in bucketIds)
                                    {
                                        HashMapBucket current = new HashMapBucket(null, bucketId);
                                        if (!buckets.Contains(current))
                                        {
                                            ((BucketsPack)e.Current).BucketIds.Remove(bucketId);
                                        }
                                        else
                                        {
                                            HashMapBucket bucket = buckets[buckets.IndexOf(current)] as HashMapBucket;
                                            if (!bPack.Owner.Equals(new NodeIdentity(bucket.CurrentShard/*,GetShardPrimary(bucket.CurrentShard)*/)))
                                            {
                                                //either i have become owner of the bucket or 
                                                //some one else for e.g a replica node 
                                                bPack.BucketIds.Remove(bucketId);
                                            }
                                        }
                                    }

                                    if (bPack.BucketIds.Count == 0)
                                    {
                                        //This owner has left.
                                        leavingShards.Add(bPack.Owner);
                                    }

                                }
                                foreach (NodeIdentity leavingShard in leavingShards)
                                {
                                    BucketsPack bPack = new BucketsPack(null, leavingShard);
                                    _filledBuckets.Remove(bPack);
                                }
                                leavingShards.Clear();
                            }
                        }

                        //Now we add those buckets which we have to be state transferred
                        //and are not currently in our list
                        IEnumerator ie = buckets.GetEnumerator();
                        while (ie.MoveNext())
                        {
                            HashMapBucket bucket = ie.Current as HashMapBucket;
                            if (Context.LocalShardName.Equals(bucket.FinalShard,StringComparison.OrdinalIgnoreCase) && !Context.LocalShardName.Equals(bucket.CurrentShard,StringComparison.OrdinalIgnoreCase))
                            {
                                BucketsPack bPack = new BucketsPack(null, new NodeIdentity(bucket.CurrentShard/*, GetShardPrimary(bucket.CurrentShard)*/));

                                if (IsSparsedBucket(bucket.BucketId, bPack.Owner))
                                {
                                    int index = _sparsedBuckets.IndexOf(bPack);
                                    if (index != -1)
                                    {
                                        bPack = _sparsedBuckets[index] as BucketsPack;
                                    }
                                    else
                                        _sparsedBuckets.Add(bPack);

                                    if (!bPack.BucketIds.Contains(bucket.BucketId))
                                    {
                                        bPack.BucketIds.Add(bucket.BucketId);
                                       
                                    }

                                }
                                else
                                {
                                    int index = _filledBuckets.IndexOf(bPack);
                                    if (index != -1)
                                    {
                                        bPack = _filledBuckets[index] as BucketsPack;
                                    }
                                    else
                                        _filledBuckets.Add(bPack);


                                    if (!bPack.BucketIds.Contains(bucket.BucketId))
                                    {
                                        bPack.BucketIds.Add(bucket.BucketId);


                                    }
                                }
                            }
                        }


#if DEBUGSTATETRANSFER
                        ArrayList filledBuckets = new ArrayList();
                        foreach(BucketsPack pack in _filledBuckets) 
                        {
                            filledBuckets.Add(pack.Clone());
                        }
                        ArrayList sparsedBuckets = new ArrayList();
                        foreach(BucketsPack pack in _sparsedBuckets) 
                        {
                            sparsedBuckets.Add(pack.Clone());
                        }
                        _parent.Cluster._history.AddActivity(new StateTxferUpdateActivity(filledBuckets, sparsedBuckets));
#endif
                    }
                }
                catch (NullReferenceException ex)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".UpdateStateTxfr", ex.ToString());
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                        LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".UpdateStateTxfr", e.ToString());
                }
                finally
                {

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                        LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".UpdateStateTxfr", " Do not need to update the task as update id does not match; provided id :" + updateId + " currentId :" + updateCount);

                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".UpdateStateTxfr", " Pulsing waiting thread");

                    VerifyShardConnectivity();
                    System.Threading.Monitor.PulseAll(_stateTxfrMutex);
                }
            }

            return true;
		}

        private void VerifyShardConnectivity()
        {
            //lock (_stateTxfrMutex)
            //{ 
                if(_filledBuckets != null && _filledBuckets.Count > 0)
                {
                    foreach (BucketsPack pack in _filledBuckets)
                    {
                        if (!IsShardConnected(pack.Owner))
                        {
                            pack.Movable = false;
                        }
                    }
                }

                if (_sparsedBuckets != null && _sparsedBuckets.Count > 0)
                {
                    foreach (BucketsPack pack in _sparsedBuckets)
                    {
                        if (!IsShardConnected(pack.Owner))
                        {
                            pack.Movable = false;
                        }
                    }
                }

            //}
        }

        public virtual void OnShardConnected(NodeIdentity shard) 
        {
            if (LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                LoggerManager.Instance.StateXferLogger.Info(this.loggingModule+".OnShardConnected", "Shard connected" + shard.ToString());

            bool needTransfer = false;

			lock (_stateTxfrMutex)
			{
				if (_sparsedBuckets != null && _sparsedBuckets.Count > 0)
				{
					lock (_sparsedBuckets.SyncRoot)
					{
                        int index = _sparsedBuckets.IndexOf(new BucketsPack(null, shard));
                        if(index!=-1)
                        {
                            BucketsPack pack=_sparsedBuckets[index] as BucketsPack;

                            if (!pack.Movable)
                            {
                                pack.Owner = shard;
                                pack.Movable = needTransfer = true;
                            }
                        }
					}
				}
				
                if (_filledBuckets != null && _filledBuckets.Count > 0)
				{
                    lock (_filledBuckets.SyncRoot)
                    {
                        int index = _filledBuckets.IndexOf(new BucketsPack(null, shard));
                        if (index != -1)
                        {
                            BucketsPack pack = _filledBuckets[index] as BucketsPack;

                            if (!pack.Movable)
                            {
                                pack.Owner = shard;
                                pack.Movable = needTransfer = true;
                            }                            
                        }
                    }
				}

                if (!_isRunning && needTransfer)
                {
                    if (LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info(loggingModule+".OnShardConnected", "State Transfer Started");
                    //Start();
                    this.Status = StateTxfrStatus.Waiting;
                }
			}
        }


        public virtual Boolean IsShardConnected(NodeIdentity shard) 
        {
            try
            {
                IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.IsShardConnected);
                operation.Params.SetParamValue(ParamName.ShardName, shard.ShardName);

                if (operationDispatcher != null)
                    return (Boolean)operationDispatcher.DispatchOperation<Object>(operation);                             
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.StateXferLogger.Error("StateTransferTask.IsShardConnected","["+shard.ToString()+"]"+ ex.Message);
                }                
            }

            return false;
        }

        protected virtual Address GetShardPrimary(string p)
        {
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.GetShardPrimary);
            operation.Params.SetParamValue(ParamName.ShardName, p);

            if (operationDispatcher != null)
                return this.operationDispatcher.DispatchOperation<Address>(operation);

            return null;                
        }

        protected NodeIdentity GetChangedOwner(int bucket, NodeIdentity currentOwner)
        {
            NodeIdentity newOwner = null;
            lock (_stateTxfrMutex)
            {
                while(true)
                {
                    if (_sparsedBuckets != null)
                    {
                        foreach (BucketsPack bPack in _sparsedBuckets)
                        {
                            if (bPack.BucketIds.Contains(bucket))
                                newOwner = bPack.Owner;
                        }
                    }
                    if (_filledBuckets != null)
                    {
                        foreach (BucketsPack bPack in _filledBuckets)
                        {
                            if (bPack.BucketIds.Contains(bucket))
                                newOwner = bPack.Owner;
                        }
                    }

                    if (newOwner == null) return null;

                    if (newOwner.Equals(currentOwner))
                    {
                        System.Threading.Monitor.Wait(_stateTxfrMutex);
                    }
                    else
                        return newOwner;
                }
            }
        }
		/// <summary>
		/// Determines whether a given bucket is sparsed one or not. A bucket is
		/// considered sparsed if its size is less than the threshhold value.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="owner"></param>
		/// <returns>True, if bucket is sparsed.</returns>
        public Boolean IsSparsedBucket(int bucketId, NodeIdentity ownerShard)
		{
            if (!_allowBulkInSparsedBuckets) return false;

            IStateTransferOperation operation = CreateStateTransferOperation(StateTransferOpCode.IsSparsedBucket);
            operation.Params.SetParamValue(ParamName.BucketID, bucketId);
            operation.Params.SetParamValue(ParamName.Threshold, _threshold);
            operation.Params.SetParamValue(ParamName.OpDestination, ownerShard);

            if (operationDispatcher != null)
                return (Boolean)operationDispatcher.DispatchOperation<Object>(operation) ;

            return true;
        }
        
        private Dictionary<int, BucketStatistics> GetBucketStats()
        {
            IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.GetBucketStats);
            return operationDispatcher.DispatchOperation<Dictionary<int, BucketStatistics>>(operation);
        }

        protected IStateTransferOperation CreateStateTransferOperation(StateTransferOpCode opCode )
        {
            return new StateTransferOperation(taskIdentity, opCode, new OperationParam());
        }

    }
    
    #endregion
}
