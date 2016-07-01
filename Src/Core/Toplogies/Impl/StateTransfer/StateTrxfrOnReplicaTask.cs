using System;
using System.Collections;
using System.Text;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Core.Toplogies.Impl.Replication;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    class StateTrxfrOnReplicaTask : StateTransferTask
    {
        private OperationId _startedFrom;

        public StateTrxfrOnReplicaTask(NodeContext context, String dbName, String colName, IDispatcher operationDispatcher, DistributionMethod distributionType)
            : base(context,dbName,colName,operationDispatcher,StateTransferType.INTRA_SHARD,distributionType)
        {            
            _allowBulkInSparsedBuckets = false;
            _trasferType = StateTransferType.INTRA_SHARD;
        }
       
        /// <summary>
        /// Remove Log Table from oplog for provided bucket id
        /// </summary>
        /// <param name="bucketID"></param>
        //private void RemoveLoggedOperations(int bucketID)
        //{
        //    IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.RemoveLoggedOperations);
        //    operation.Params.SetParamValue(ParamName.BucketID, bucketID);

        //    operationDispatcher.DispatchOperation<Object>(operation);
        //}

        protected override void EndBucketsStateTxfr(ArrayList buckets)
        {
            if (buckets != null)
            {
                LoggingIdentity identity=new LoggingIdentity(taskIdentity.DBName, taskIdentity.ColName, (int)buckets[0]);
                StopLoggingOnReplica(identity);
                ICollection loggedOperations = GetLoggedOperations(_startedFrom, identity);

                
                //STD: Apply logged operations on collection
                ApplyLogOperation(loggedOperations as ClusteredArrayList);

                RemoveLoggedOperations((int)buckets[0]);
            }
        }
        
        /// <summary>
        /// Updates the state transfer task in synchronus way. It adds/remove buckets
        /// to be transferred by the state transfer task.
        /// </summary>
        /// <param name="myBuckets"></param>
        public override bool UpdateStateTransfer(ArrayList myBuckets, int updateId)
        {
            if (_databasesManager != null && _databasesManager.HasDisposed(taskIdentity.DBName, taskIdentity.ColName)/* _parent.HasDisposed*/)
                return false;

            StringBuilder sb = new StringBuilder();
            lock (_updateIdMutex)
            {
                if (updateId != updateCount)
                {
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info(loggingModule + "UpdateStateTxfr", " Do not need to update the task as update id does not match; provided id :" + updateId + " currentId :" + updateCount);
                    return false;
                }
            }

            lock (_stateTxfrMutex)
            {
                try
                {
                    if (myBuckets != null)
                    {
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                            LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".UpdateStateTxfr", " my buckets " + myBuckets.Count);
                        //we work on the copy of the map.
                        ArrayList buckets = myBuckets.Clone() as ArrayList;
                        ArrayList leavingShards = new ArrayList();

                        //if (_sparsedBuckets != null && _sparsedBuckets.Count > 0)
                        //{
                        //    //ArrayList tmp = _sparsedBuckets.Clone() as ArrayList;
                        //    IEnumerator e = _sparsedBuckets.GetEnumerator();

                        //    lock (_sparsedBuckets.SyncRoot)
                        //    {
                        //        while (e.MoveNext())
                        //        {
                        //            BucketsPack bPack = (BucketsPack)e.Current;
                        //            ArrayList bucketIds = bPack.BucketIds.Clone() as ArrayList;
                        //            foreach (int bucketId in bucketIds)
                        //            {
                        //                HashMapBucket current = new HashMapBucket(null, bucketId);

                        //                if (!buckets.Contains(current))
                        //                {
                        //                    ((BucketsPack)e.Current).BucketIds.Remove(bucketId);
                        //                }
                        //                else
                        //                {
                        //                    HashMapBucket bucket = buckets[buckets.IndexOf(current)] as HashMapBucket;
                        //                    if (!bPack.Owner.Equals(new NodeIdentity(bucket.CurrentShard, GetShardPrimary(bucket.CurrentShard))))
                        //                    {
                        //                        //either i have become owner of the bucket or 
                        //                        //some one else for e.g a replica node 
                        //                        if (logger != null && logger.IsInfoEnabled)
                        //                            logger.Info(loggingModule + ".UpdateStateTxfer", bucket.BucketId + "bucket owner changed old :" + bPack.Owner + " new :" + bucket.CurrentShard);
                        //                        bPack.BucketIds.Remove(bucketId);
                        //                    }
                        //                }
                        //            }
                        //            if (bPack.BucketIds.Count == 0)
                        //            {
                        //                //This owner has left.
                        //                leavingShards.Add(bPack.Owner);
                        //            }

                        //        }
                        //        foreach (NodeIdentity leavingShard in leavingShards)
                        //        {
                        //            BucketsPack bPack = new BucketsPack(null, leavingShard);
                        //            _sparsedBuckets.Remove(bPack);
                        //        }
                        //        leavingShards.Clear();
                        //    }
                        //}

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
                                            if (!bPack.Owner.Equals(new NodeIdentity(bucket.CurrentShard/*, GetShardPrimary(bucket.CurrentShard)*/)))
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
                        ArrayList loggableBuckets = new ArrayList();

                        while (ie.MoveNext())
                        {
                            HashMapBucket bucket = ie.Current as HashMapBucket;
                            if (Context.LocalShardName.Equals(bucket.FinalShard, StringComparison.OrdinalIgnoreCase) && Context.LocalShardName.Equals(bucket.CurrentShard, StringComparison.OrdinalIgnoreCase))
                            {
                                BucketsPack bPack = new BucketsPack(null, new NodeIdentity(bucket.CurrentShard/*, GetShardPrimary(bucket.CurrentShard)*/));

                                //if (IsSparsedBucket(bucket.BucketId, bPack.Owner))
                                //{
                                //    int index = _sparsedBuckets.IndexOf(bPack);
                                //    if (index != -1)
                                //    {
                                //        bPack = _sparsedBuckets[index] as BucketsPack;
                                //    }
                                //    else
                                //        _sparsedBuckets.Add(bPack);

                                //    if (!bPack.BucketIds.Contains(bucket.BucketId))
                                //    {
                                //        bPack.BucketIds.Add(bucket.BucketId);
                                //    }

                                //}
                                //else
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
                                        loggableBuckets.Add(bucket.BucketId);

                                    }
                                }
                            }
                        }

                        _startedFrom = StartLoggingOnReplica(loggableBuckets);

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
                    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsInfoEnabled)
                        LoggerManager.Instance.StateXferLogger.Info(loggingModule + ".UpdateStateTxfr", " Pulsing waiting thread");
                    System.Threading.Monitor.PulseAll(_stateTxfrMutex);
                }
            }

            return true;
        }

        protected override Hashtable AcquireLockOnBuckets(ArrayList buckets, NodeIdentity finalShard)
        {
            //In case of a replica node replicate a bucket from the source,it is not required
            //to get a proper lock. we simply simulate
            Hashtable result = new Hashtable();
            result[BucketLockResult.OwnerChanged] = null;
            result[BucketLockResult.LockAcquired] = buckets;
            return result;
        }
        public override void AckBucketTxfer(NodeIdentity owner, ArrayList buckets)
        {
            //no need to send an acknowlegement to the owner.
        }

        protected override void FinalizeStateTransfer()
        {            
            //NTD: [High] Remove Extra buckets locally

            //PartitionOfReplicasServerCache cache = _parent as PartitionOfReplicasServerCache;
            //cache.RemoveExtraBuckets();
        }


        #region State Transfer Logging Code

        private OperationId StartLoggingOnReplica(ArrayList bucketIds)
        {
            //opId is latest operation id at time when bucket logging been started so that we can use it during query
            OperationId opId = new OperationId();
            if (Context.OperationLog != null)
            {
                ILogOperation lastOperation = Context.OperationLog.LastOperation;

                if (lastOperation != null && lastOperation.OperationId != null)
                    opId = Context.OperationLog.LastOperation.OperationId;

            }

            foreach (int bucketID in bucketIds)
            {
                if (Context.OperationLog != null)
                    Context.OperationLog.StartLogging(new LoggingIdentity(taskIdentity.DBName,taskIdentity.ColName, bucketID), LogMode.BeforeActualOperation);
            }

            return opId;
        }
        
        private void StopLoggingOnReplica(LoggingIdentity identity)
        {
            if (Context.OperationLog != null)
            {
                Context.OperationLog.StopLogging(identity);
            }
        }

        private ICollection GetLoggedOperations(OperationId operationId, LoggingIdentity identity)
        {
            if (Context.OperationLog != null)
            {
                return Context.OperationLog.GetLoggedOperations(identity, operationId);
            }
            return null;
        }

        private void ApplyLogOperation(ClusteredArrayList logOperations)
        {
            if (this.operationDispatcher != null && logOperations!=null && logOperations.Count>0)
            {
                IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.ApplyLogOperations);
                operation.Params.SetParamValue(ParamName.LogOperations, logOperations);
                operationDispatcher.DispatchOperation<Object>(operation);
            }
        }

        private object RemoveLoggedOperations(int bucketID)
        {
            LoggingIdentity identity = new LoggingIdentity(taskIdentity.DBName, taskIdentity.ColName, bucketID);
            if (Context.OperationLog != null)
                return Context.OperationLog.RemoveLoggedOperations(identity);
            return null;
        }

        #endregion
    }
}