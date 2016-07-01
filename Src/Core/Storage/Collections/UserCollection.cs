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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
//using Alachisoft.NosDB.Common.Queries.UserDefinedFunction;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
//using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.RangeBasedDistribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Queries.Optimizer;
using Alachisoft.NosDB.Core.Queries.Results;
using Alachisoft.NosDB.Core.Toplogies;
//using Alachisoft.NosDB.Core.Toplogies.Impl.Replication;
using System;
using CollectionConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration;
using IndexConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration;
using UpdateOperation = Alachisoft.NosDB.Core.Storage.Operations.UpdateOperation;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Core.Storage.Collections
{
    /// <summary>
    /// Responsible for managing indexing , handling collection level operations
    /// key based collection level locking
    /// eventual persistence
    /// document caching
    /// </summary>
    public class UserCollection : BaseCollection,IStateTxfrOperationListener
    {
        protected IDistribution _distribution;
        protected bool _isSystemCollection = false;

        protected IDictionary<int, KeyValuePair<HashMapBucket, BucketStatistics>> _bucketStats =
            new HashVector<int, KeyValuePair<HashMapBucket, BucketStatistics>>();
        protected string _shardName;

        //protected OperationLog _operationLog = null;

        public override IDistribution Distribution
        {
            get { return _distribution; }
            set
            {
                if (value == null)
                {
                    LoggerManager.Instance.StorageLogger.Info("Distribution", "Null Distribution");
                    return;
                }
                _distribution = value;
                if (_distribution != null)
                    UpdateBucketInfo();
            }
        }

        public override IDictionary<int, KeyValuePair<HashMapBucket, BucketStatistics>> BucketStatistics
        { get { return _bucketStats; } }

        public override string ShardName
        {
            get { return _shardName; }
            set { _shardName = value; }
        }


        public UserCollection(DatabaseContext dbContext, NodeContext nodeContext)
            : base(dbContext, nodeContext)
        {
            //_operationLog = nodeContext.OperationLog;
            _isSystemCollection = String.Compare(_context.ClusterName, MiscUtil.LOCAL, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool IsSystemCollection
        {
            get { return _isSystemCollection; }
            set { _isSystemCollection = value; }
        }


        public override bool Initialize(CollectionConfiguration configuration, QueryResultManager queryResultManager, IStore databaseStore, IDistribution distribution)
        {
            _configuration = configuration;
            //_clusteredOperationListener = clusteredOperationListener;
            Distribution = distribution;
            _databaseStore = databaseStore;
            _queryResultManager = queryResultManager;
            _metadataIndex.Initialize(this);

            _dbContext.StorageManager.CreateCollection(_configuration.CollectionName);
            _docStore.Initialize(_dbContext, configuration);
            //_userDefinedFunctionStore = userDefinedFunctionStore;
            //if (configuration.TriggersConfiguration != null)
            //    _triggerStore.Initialize(configuration.TriggersConfiguration);

            _queryOptimizer = new CostBasedOptimizer(IndexManager);

            _statusLatch.SetStatusBit(CollectionStatus.RUNNING, CollectionStatus.INITIALIZING);
            return true;
        }

        //private void UpdateRangeBucketInfo()
        //{
        //    //old Map
        //    //IDictionary<int, KeyValuePair<RangeBucket, BucketStatistics>> oldMap = new HashVector<int, KeyValuePair<RangeBucket, BucketStatistics>>();
        //    //foreach (KeyValuePair<HashMapBucket, BucketStatistics> bucket in _bucketStats.Values)
        //    //{
        //    //    oldMap[bucket.Key.BucketId] = new KeyValuePair<RangeBucket, BucketStatistics>((RangeBucket)bucket.Key, bucket.Value);
        //    //}

        //    ////new Map
        //    //HashMapBucket[] tempNewMap = _distribution.GetBucketsForShard(_shardName);
        //    //IDictionary<int, RangeBucket> newMap = new HashVector<int, RangeBucket>();
        //    //foreach (HashMapBucket bucket in tempNewMap)                          //N Iterations already
        //    //{
        //    //    newMap.Add(bucket.BucketId, (RangeBucket)bucket);
        //    //}

        //    ////find missing buckets (Was present in Old MAP and is not in new)and keep in list
        //    //IList<RangeBucket> missingBuckets = new ClusteredList<RangeBucket>();
        //    //foreach (KeyValuePair<int, KeyValuePair<RangeBucket, BucketStatistics>> oldBucket in oldMap)
        //    //{
        //    //    if (!newMap.ContainsKey(oldBucket.Key))
        //    //        missingBuckets.Add(oldBucket.Value.Key);
        //    //}

        //    //////find new buckets and keep in list
        //    ////List<RangeBucket> newBuckets = new List<RangeBucket>();
        //    ////foreach (KeyValuePair<int, RangeBucket> newBucket in newMap)
        //    ////{
        //    ////    if (oldMap.ContainsKey(newBucket.Key))
        //    ////        newBuckets.Add(newBucket.Value);
        //    ////}

        //    ////Let the fun begin :p
        //    ////Iterate new Map and match bucket ID
        //    //foreach (KeyValuePair<int, RangeBucket> newBucket in newMap)
        //    //{
        //    //    DocumentKey lastRangeIncluded = null;
        //    //    //check if oldMap contains bucketId 
        //    //    if (oldMap.ContainsKey(newBucket.Key))
        //    //    {
        //    //        RangeBucket oldBucket = oldMap[newBucket.Key].Key;
        //    //        lastRangeIncluded = oldBucket.RangeStart;

        //    //        //match start with start
        //    //        if (newBucket.Value.RangeStart.Equals(oldBucket.RangeStart))
        //    //        {
        //    //            //if yes match end with end
        //    //            if (newBucket.Value.RangeEnd.Equals(oldBucket.RangeEnd))
        //    //            {
        //    //                if (lastRangeIncluded.CompareTo(oldBucket.RangeEnd) < 0)
        //    //                {
        //    //                    lastRangeIncluded = oldBucket.RangeEnd;
        //    //                }
        //    //                //stats will remain same. If this case show wierd behavior contact  => logical error in this fucntion
        //    //            }
        //    //            else if (newBucket.Value.Contains(oldBucket.RangeEnd))
        //    //            {
        //    //                if (lastRangeIncluded.CompareTo(oldBucket.RangeEnd) < 0)
        //    //                {
        //    //                    lastRangeIncluded = oldBucket.RangeEnd;
        //    //                }
        //    //                //add stats
        //    //                _bucketStats.Remove(newBucket.Value.BucketId);
        //    //                KeyValuePair<HashMapBucket, BucketStatistics> tempPair = new KeyValuePair<HashMapBucket, BucketStatistics>(newBucket.Value, new BucketStatistics());
        //    //                _bucketStats[newBucket.Key] = tempPair;
        //    //                _bucketStats[newBucket.Key].Value.Increment(_bucketStats[oldBucket.BucketId].Value.DataSize);
        //    //            }
        //    //            else
        //    //            {
        //    //                //full store scan
        //    //                _bucketStats[newBucket.Key] = new KeyValuePair<HashMapBucket, BucketStatistics>(newBucket.Value, new BucketStatistics());
        //    //                _bucketStats[newBucket.Key].Value.DataSize = ReCalculateBucketSizeFromStore(newBucket.Value);
        //    //                continue;
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            //if (newBucket.Value.RangeStart.CompareTo(oldBucket.RangeStart) > 0)
        //    //            //{

        //    //            //}
        //    //            //full store scan
        //    //            _bucketStats[newBucket.Key].Value.DataSize = ReCalculateBucketSizeFromStore(newBucket.Value);
        //    //            _bucketStats.Remove(oldBucket.BucketId);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        _bucketStats[newBucket.Key] = new KeyValuePair<HashMapBucket, BucketStatistics>(newBucket.Value, new BucketStatistics());

        //    //        foreach (KeyValuePair<int, KeyValuePair<RangeBucket, BucketStatistics>> oldbucket in oldMap)
        //    //        {
        //    //            if (oldbucket.Value.Key.Contains(newBucket.Value.RangeStart))
        //    //            {
        //    //                if (oldbucket.Value.Key.Contains(newBucket.Value.RangeEnd) || oldbucket.Value.Key.Equals(newBucket.Value.RangeEnd))
        //    //                {
        //    //                    //FuLL STORE
        //    //                    KeyValuePair<HashMapBucket, BucketStatistics> temPair =
        //    //                        new KeyValuePair<HashMapBucket, BucketStatistics>(newBucket.Value, new BucketStatistics());
        //    //                    _bucketStats.Add(newBucket.Key, temPair);
        //    //                    _bucketStats[newBucket.Key].Value.DataSize = ReCalculateBucketSizeFromStore(newBucket.Value);
        //    //                }
        //    //                else
        //    //                {
        //    //                    if (oldbucket.Value.Key.RangeStart.Equals(newBucket.Value.RangeStart))
        //    //                    {
        //    //                        if (!(oldbucket.Value.Key.Contains(newBucket.Value.RangeEnd) || oldbucket.Value.Key.RangeEnd.Equals(newBucket.Value.RangeEnd)))
        //    //                        {
        //    //                            //this is a new bucket
        //    //                            _bucketStats.Add(newBucket.Key, new KeyValuePair<HashMapBucket, BucketStatistics>(newBucket.Value, new BucketStatistics()));
        //    //                        }
        //    //                    }
        //    //                    //This case will never happen if happens contact 
        //    //                    //The problem is with range distribution, bucket Id assignment
        //    //                    //TODO: log exception
        //    //                }
        //    //            }
        //    //            else
        //    //            {
        //    //                //////////this is a new range. stats = 0
        //    //                _bucketStats.Add(newBucket.Key, new KeyValuePair<HashMapBucket, BucketStatistics>(newBucket.Value, new BucketStatistics()));
        //    //                ////////if (!newBucket.Value.RangeStart.Equals(lastRangeIncluded))
        //    //                ////////{
        //    //                //full store scan
        //    //                _bucketStats[newBucket.Key].Value.DataSize = ReCalculateBucketSizeFromStore(newBucket.Value);
        //    //                ////////////}
        //    //            }
        //    //        }
        //    //    }

        //    //    IList<RangeBucket> bucketsToBeRemoved = new ClusteredList<RangeBucket>();
        //    //    foreach (RangeBucket missingBucket in missingBuckets)
        //    //    {
        //    //        if (newBucket.Value.Contains(missingBucket.RangeStart))
        //    //        {
        //    //            if (newBucket.Value.Contains(missingBucket.RangeEnd))
        //    //            {
        //    //                //add stats //remove this missing bucket
        //    //                _bucketStats[newBucket.Key].Value.Increment(_bucketStats[missingBucket.BucketId].Value.DataSize);
        //    //                bucketsToBeRemoved.Add(missingBucket);
        //    //                if (lastRangeIncluded.CompareTo(missingBucket.RangeEnd) < 0)
        //    //                {
        //    //                    lastRangeIncluded = missingBucket.RangeEnd;
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //    foreach (RangeBucket bucket in bucketsToBeRemoved)
        //    //    {
        //    //        missingBuckets.Remove(bucket);
        //    //    }

        //    //    //if (!newBucket.Value.RangeEnd.Equals(lastRangeIncluded))
        //    //    //{
        //    //    //    //full store scan
        //    //    //    _bucketStats[newBucket.Key].Value.DataSize = ReCalculateBucketSizeFromStore(newBucket.Value);
        //    //    //}
        //    //}
        //}

        //private long ReCalculateBucketSizeFromStore(RangeBucket bucket)
        //{
        //    long size = 0;
        //    return size;
        //}

        private void UpdateHashBucketInfo()
        {
            if (_distribution == null) return;

            HashMapBucket[] newBuckets = _distribution.GetBucketsForShard(_shardName);
            //Find common bucket Ids, and add new Buckets 
            IList<int> bucketsNotToBeRemoved = new ClusteredList<int>();
            foreach (HashMapBucket bucket in newBuckets)
            {

                //TODO: either use linq query to find the bucket with specific id
                //or calculate hashcode only on the basis of bucket id.
                //otherwise the following contains check will not work correctly
                if (_bucketStats.ContainsKey(bucket.BucketId))
                {
                    bucketsNotToBeRemoved.Add(bucket.BucketId);
                    //needed to be done when hashcode is only based on bucket id.
                    BucketStatistics tempStats = _bucketStats[bucket.BucketId].Value;
                    _bucketStats.Remove(bucket.BucketId);
                    _bucketStats.Add(bucket.BucketId, new KeyValuePair<HashMapBucket, BucketStatistics>(bucket, tempStats));
                }
                else
                {
                    _bucketStats.Add(bucket.BucketId, new KeyValuePair<HashMapBucket, BucketStatistics>(bucket, new BucketStatistics()));
                    bucketsNotToBeRemoved.Add(bucket.BucketId);
                }
            }
            //Find buckets which are no longer part of the collection
            IList<int> bucketsNotOwnedNow = new ClusteredList<int>();
            foreach (KeyValuePair<int, KeyValuePair<HashMapBucket, BucketStatistics>> bucketStat in _bucketStats)
            {
                if (!bucketsNotToBeRemoved.Contains(bucketStat.Key))
                {
                    bucketStat.Value.Key.Status = BucketStatus.NeedTransfer;
                    bucketsNotOwnedNow.Add(bucketStat.Key);
                }
            }
            //Update Bucket Status to Need State Transfer
            foreach (int bucketId in bucketsNotOwnedNow)
            {
                _bucketStats[bucketId].Key.Status = BucketStatus.NeedTransfer;
            }
        }

        protected override void UpdateBucketInfo()
        {
            //DONT REMOVE ANY COMMENTED CODE IN THIS FUCNCTION
            //TODO: Possible enumeration modified exceptions need to handle later after statetransfer is working.
            //VERY VERY SAD LOGIC

            //Update dictionary with bucketID's 
            lock (_bucketStats)
            {

                if (_distribution.Type.Equals(DistributionMethod.NonShardedDistribution))
                {
                    UpdateHashBucketInfo();
                }

                //if (_distribution.Type.Equals(DistributionMethod.NonShardedDistribution))
                //{
                //    UpdateRangeBucketInfo();
                //}
                //else
                //{
                //    UpdateHashBucketInfo();
                //}
            }
        }

        public override HashMapBucket GetKeyBucket(DocumentKey key)
        {
            if (_distribution == null)
                return null;
            return _distribution.GetDistributionRouter().GetBucketForDocument(key);
        }

        private bool IsLocalOperation(DocumentKey key, IOperationContext operationContext)
        {
           // if (!operationContext.ContainsKey(ContextItem.IsReplicationOperation))
            //{
                if (_distribution == null)
                    throw new DatabaseException(ErrorCodes.Collection.DISTRIBUTION_NOT_SET, "Distribution is null");
                HashMapBucket bucket = GetKeyBucket(key);//_distribution.GetDistributionRouter().GetBucketForDocument(key);
                //TODO: Haseedb -> Following code is a crosscheck and should be handled at Query Router too.
                return bucket.CurrentShard == _shardName;
            //}
           // return true;
            //DO NOT REMOVE THE FOLLOWING COMMENTED CODE.

            ////Normal case if Bucket belongs to this Shard (even after shard add/remove)
            //if (bucket.FinalShard == ShardName && bucket.CurrentShard == ShardName)
            //{
            //    switch (bucket.Status)
            //    {
            //        case BucketStatus.Functional:       //Normal Case
            //            return true;
            //        case BucketStatus.NeedTransfer:     //Should not happen probably
            //            return true;
            //        case BucketStatus.UnderStateTxfr:   //Should not happen probably
            //            //TODO: // add operation in oplog.
            //            //dont knw what to return :p ??
            //            break;
            //    }
            //}
            ////Case when a new bucket is being assigned to this shard. 
            //else if (bucket.FinalShard != ShardName && bucket.CurrentShard == ShardName)
            //{
            //    switch (bucket.Status)
            //    {
            //        case BucketStatus.Functional:       //Should not happen probably
            //            return true;
            //        case BucketStatus.NeedTransfer:     //Should not happen but if it does fail the operation.(heppens if QR fails to route the call properly)
            //            return false;
            //        case BucketStatus.UnderStateTxfr:   //Should not happen but if it does fail the operation.(heppens if QR fails to route the call properly)
            //            return false;
            //    }
            //}
            ////Case when a bucket is no longer part of the shard (maybe transfered to another or in progress)
            //else if (bucket.FinalShard == ShardName && bucket.CurrentShard != ShardName)
            //{
            //    switch (bucket.Status)
            //    {
            //        case BucketStatus.Functional:       //Should not happen but if it is then perform the operation here.
            //            return true;
            //        case BucketStatus.NeedTransfer:     //Normal case 
            //            return true;
            //        case BucketStatus.UnderStateTxfr:   //Normal case
            //            //TODO: add operation in oplog.
            //            //dont know what to return :p ??
            //            break;
            //    }
            //}
        }

        protected override bool OnPreInsert(IJSONDocument document, IOperationContext context, DatabaseOperationType opType)
        {
            if (opType == DatabaseOperationType.StateTransferInsert)
                return true;

            if (_isSystemCollection)
                return base.OnPreInsert(document,context, opType);
            else
                return IsLocalOperation(new DocumentKey(document.Key), context);
        }

        protected override bool OnPreGetDocuments(DocumentKey documentKey, IOperationContext context)
        {
            return _isSystemCollection ? base.OnPreGetDocuments(documentKey, context) : IsLocalOperation(documentKey, context);
        }

        /* Code Removal[Prof]
        protected override void OnPostInsertDocument(InsertOperation insertOperation, JSONDocument rollbackDocument)
        {
            try
            {
                if (!_isSystemCollection)
                {
                    // OpLog insertion: do not remove
                    if (!insertOperation.Context.ContainsKey(ContextItem.DoNotLog))
                    {
                        OperationId operationId;
                        if (insertOperation.Context.ContainsKey(ContextItem.ReplicationOperationId))
                        {
                            operationId = insertOperation.Context[ContextItem.ReplicationOperationId] as OperationId;
                            operationId.Id = insertOperation.OperationId;
                        }
                        else
                        {
                            operationId = GetNextOperationId(insertOperation.OperationId, DateTime.Now);
                        }

                        LogDocumentOperation(operationId, insertOperation, rollbackDocument , majOperationId/);
                    }

                    HashMapBucket bucket =
                        _distribution.GetDistributionRouter().GetBucketForDocument(new DocumentKey(insertOperation.Document.Key));
                    if (!_distribution.Type.Equals(DistributionMethod.RangeDistribution))
                    {
                        lock (_bucketStats)
                        {
                            _bucketStats[bucket.BucketId].Value.Increment(insertOperation.Document.Size);
                            _bucketStats[bucket.BucketId].Value.IsDirty = true;
                        }
                    }
                }
                else
                {
                    base.OnPostInsertDocument(insertOperation, rollbackDocument);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
        }
        */
        protected override bool OnPreUpdate(DocumentKey key, IOperationContext context)
        {
            if (_isSystemCollection)
            {
                return base.OnPreUpdate(key, context);
            }
            else
                return IsLocalOperation(key, context);
        }

        /* Code Removal[Prof]
        protected override void OnPostUpdateDocument(UpdateResult<JSONDocument> result, UpdateOperation operation)
        {
            try
            {
                if (!_isSystemCollection)
                {
                    if (!operation.Context.ContainsKey(ContextItem.DoNotLog))
                    {
                        OperationId operationId;
                        if (operation.Context.ContainsKey(ContextItem.ReplicationOperationId))
                        {
                            operationId = operation.Context[ContextItem.ReplicationOperationId] as OperationId;
                        }
                        else
                        {
                            operationId = GetNextOperationId(operation.OperationId, DateTime.Now);
                        }

                        LogDocumentOperation(operationId, operation, result.OldDocument /*, majOperationId/);
                    }

                    if (!_distribution.Type.Equals(DistributionMethod.RangeDistribution))
                    {
                        HashMapBucket bucket = _distribution.GetDistributionRouter().GetBucketForDocument(new DocumentKey(result.OldDocument.Key));
                        lock (_bucketStats)
                        {
                            _bucketStats[bucket.BucketId].Value.Increment(result.NewDocument.Size - result.OldDocument.Size);
                            _bucketStats[bucket.BucketId].Value.IsDirty = true;
                        }
                    }
                }
                else
                {
                    base.OnPostUpdateDocument(result, operation);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
        }
        */

        protected override bool OnPreDelete(DocumentKey key, IOperationContext context)
        {
            if (_isSystemCollection)
                return base.OnPreDelete(key, context);
            else
                return IsLocalOperation(key, context);
        }

        /* Code Removal[Prof]
        protected override void OnPostDeleteDocument(DeleteResult<JSONDocument> result, Operations.RemoveOperation removeOp, IJSONDocument rollbackDocument)
        {
            try
            {
                //TodO NY: Uncomment later

                if (!_isSystemCollection)
                {
                    // OpLog insertion: do not remove
                    if (!removeOp.Context.ContainsKey(ContextItem.DoNotLog))
                    {
                        OperationId operationId;
                        if (removeOp.Context.ContainsKey(ContextItem.ReplicationOperationId))
                        {
                            operationId = removeOp.Context[ContextItem.ReplicationOperationId] as OperationId;
                        }
                        else
                        {
                            operationId = GetNextOperationId(removeOp.OperationId, DateTime.Now);
                        }
                        LogDocumentOperation(operationId, removeOp, result.Document /*, majOperationId/);
                    }

                    if (!_distribution.Type.Equals(DistributionMethod.RangeDistribution))
                    {
                        HashMapBucket bucket = _distribution.GetDistributionRouter().GetBucketForDocument(new DocumentKey(result.Document.Key));
                        lock (_bucketStats)
                        {
                            //This check need to be removed 
                            if (_bucketStats.ContainsKey(bucket.BucketId))
                            {
                                _bucketStats[bucket.BucketId].Value.Decrement(result.Document.Size);
                                _bucketStats[bucket.BucketId].Value.IsDirty = true;
                            }
                        }
                    }
                }
                else
                {
                    base.OnPostDeleteDocument(result, removeOp, rollbackDocument);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
        }
        */

        //protected override OperationId OnPreExecuteNonQuery(INonQueryOperation operation, bool isReplicationOperation)
        //{
        //    if (_isSystemCollection)
        //    {
        //        return base.OnPreExecuteNonQuery(operation, isReplicationOperation);
        //    }
        //    else
        //    {
        //        if (!isReplicationOperation)
        //            return LogQueryOperation(operation);
        //        else return null;
        //        //return null;
        //    }
        //}

        # region -------------- Replication Method -----------------
        /* Code Removal[Prof]
        protected void LogDocumentOperation(OperationId operationId, Operation operation, JSONDocument rollbackDocument/*, OperationId queryId/)
        {
            DocumentLogOperation documentLogOperation = new DocumentLogOperation();
            DocumentKey docKey = null;

            switch (operation.OperationType)
            {
                case OperationType.Add:
                    documentLogOperation.LogOperationType = LogOperationType.Add;
                    Operations.InsertOperation insertOperation = (InsertOperation)operation;
                    documentLogOperation.Document = insertOperation.Document;

                    docKey = new DocumentKey(insertOperation.Document.Key);
                    break;

                case OperationType.Remove:
                    documentLogOperation.LogOperationType = LogOperationType.Remove;
                    JSONDocument document = new JSONDocument();
                    Operations.RemoveOperation removeOperation = (Operations.RemoveOperation)operation;
                    document.Key = removeOperation.Key.Value as string;
                    documentLogOperation.Document = document;
                    documentLogOperation.RollbackDocument = rollbackDocument;
                    docKey = removeOperation.Key;
                    break;

                case OperationType.Update:
                    documentLogOperation.LogOperationType = LogOperationType.Update;
                    Operations.UpdateOperation updateOperation = (Operations.UpdateOperation)operation;
                    documentLogOperation.Document = updateOperation.Update;
                    documentLogOperation.RollbackDocument = rollbackDocument;
                    docKey = new DocumentKey(updateOperation.Update.Key);

                    break;

                case OperationType.Replace:
                    documentLogOperation.LogOperationType = LogOperationType.Replace;
                    Operations.ReplaceOperation replaceOperation = (ReplaceOperation)operation;
                    documentLogOperation.Document = replaceOperation.Replacement;
                    documentLogOperation.RollbackDocument = rollbackDocument;
                    docKey = new DocumentKey(replaceOperation.Replacement.Key);
                    break;
                //TODO: Log exception in log file in case of default
            }
            //documentLogOperation.QueryId = queryId;
            documentLogOperation.Collection = this.Name;
            documentLogOperation.Database = _dbContext.DatabaseName;

            documentLogOperation.OperationId = operationId;

            HashMapBucket bucket = GetKeyBucket(docKey);
            if (bucket == null)
                throw new DatabaseException(ErrorCodes.Collection.DISTRIBUTION_NOT_SET, "Distribution is null");

            LoggingIdentity logInfo = new LoggingIdentity(this.DbContext.DatabaseName, this.Name, bucket.BucketId);

            if (_operationLog != null)
                _operationLog.Log(documentLogOperation, logInfo, LogMode.AfterActualOperation);
        }

        //protected OperationId LogQueryOperation(INonQueryOperation operation)
        //{
        //    OperationId opId = GetNextOperationId(-1, DateTime.Now);

        //    QueryLogOperation replicationOperation = new QueryLogOperation();
        //    replicationOperation.Collection = this.Name;
        //    replicationOperation.Database = _dbContext.DatabaseName;

        //    replicationOperation.LogOperationType = LogOperationType.MajorOperation;
        //    replicationOperation.Operation = operation;

        //    replicationOperation.OperationId = opId;

        //    if (_operationLog != null)
        //        _operationLog.Log(replicationOperation);
        //    return opId;
        //}

        protected LoggingIdentity GetLoggingIdentity(ILogOperation operation)
        {
            HashMapBucket bucket = null;
            DocumentKey key = null;
            switch (operation.LogOperationType)
            {
                case LogOperationType.Add:
                case LogOperationType.Update:
                case LogOperationType.Replace:
                    IJSONDocument document = ((DocumentLogOperation)operation).Document;
                    key = new DocumentKey(document.Key);
                    break;
                case LogOperationType.Remove:
                    key = ((DocumentLogOperation)operation).DocumentKey;
                    break;
            }
            if (key != null)
                bucket = GetKeyBucket(key);

            if (bucket != null)
                return new LoggingIdentity(DbContext.DatabaseName, Name, bucket.BucketId);

            return null;
        }

        public void Replicate(ILogOperation operation)
        {
            if (_operationLog != null && _operationLog.Log(operation, GetLoggingIdentity(operation), LogMode.BeforeActualOperation))
                return;
            // TODO: We have added some other api operations as well e.g. ReplaceDocuments, InsertAttachments etc
            switch (operation.LogOperationType)
            {
                case LogOperationType.Add:
                    ReplicateInsert(operation);
                    break;
                case LogOperationType.Remove:
                    ReplicateRemove(operation);
                    break;
                case LogOperationType.Update:
                    ReplicateUpdate(operation);
                    break;
                case LogOperationType.Replace:
                    ReplicateReplace(operation);
                    break;
                //case Common.Server.Engine.Impl.LogOperationType.MajorOperation:
                //    ReplicateMajorOperation(operation);
                //    break;

                //Attachments Replication was turned off. maybe required in future
                //case LogOperationType.InsertAttachment:
                //    ((AttachmentCollection)this).ReplicateInsertAttachment(operation);
                //    break;
                //case LogOperationType.SendNextChunk:
                //    ((AttachmentCollection)this).ReplicateSendNextChunk(operation);
                //    break;
                //case LogOperationType.DeleteAttachment:
                //    ((AttachmentCollection)this).ReplicateDeleteAttachment(operation);
                //    break;
                //case LogOperationType.ReplaceAttachment:
                //    ((AttachmentCollection)this).ReplicateReplaceAttachment(operation);
                //    break;
            }

            OnPostDataManipulation();
        }

        protected void ReplicateInsert(ILogOperation operation)
        {
            IJSONDocument document = /*JSONDocument.Parse(/((DocumentLogOperation)operation).Document/*)/;
            DocumentKey key = new DocumentKey(document.Key);
            try
            {
                IDocumentsWriteOperation insertOperation = new InsertDocumentsOperation();
                insertOperation.Documents = new ClusteredList<IJSONDocument>() { document };
                insertOperation.Database = operation.Database;
                insertOperation.Collection = operation.Collection;
                insertOperation.Context = new OperationContext();

                if (operation.Context == null || !operation.Context.ContainsKey(ContextItem.RestoreOperation))
                {
                    insertOperation.Context[ContextItem.IsReplicationOperation] = true;
                    insertOperation.Context[ContextItem.ReplicationOperationId] = operation.OperationId;
                    if (operation.Context != null)
                    {
                        foreach (var kvp in operation.Context)
                        {
                            insertOperation.Context[kvp.Key] = kvp.Value;
                        }
                    }
                }

                IDocumentsWriteResponse response = InsertDocuments(insertOperation);
                //TODO: What if the operation fails?

                if (response.IsSuccessfull ||
                    (LoggerManager.Instance.REPLogger == null || !LoggerManager.Instance.REPLogger.IsErrorEnabled))
                    return;

                foreach (var failedDocuments in response.FailedDocumentsList)
                {
                    if (failedDocuments.ErrorCode != ErrorCodes.Collection.KEY_ALREADY_EXISTS)
                    {
                        LoggerManager.Instance.REPLogger.Error("Replication",
                            "Insert Operation Failed. Document Key:" + failedDocuments.DocumentKey + " Error Message:" +
                            ErrorMessages.GetErrorMessage(failedDocuments.ErrorCode, failedDocuments.ErrorParameters));
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.REPLogger != null)
                    LoggerManager.Instance.REPLogger.Error("UserCollection", ex.ToString());
            }
            //: If this fails and all other operations get performed then there might occur an issue of data inconsistency or even that may lead to further failures.
            // Lets assume I insert document#2 at primary, remove and then insert it back. Assume that remove operation fails at replica. 2 inserts together? Thats not feasible

            //if (_operationLog != null && response.IsSuccessfull)
            //    _operationLog.OnOperationPerformed(operation);

        }

        protected void ReplicateUpdate(ILogOperation operation)
        {
            IJSONDocument document = ((DocumentLogOperation)operation).Document;
            DocumentKey key = new DocumentKey(document.Key);
            try
            {
                long rowId;
                if (_metadataIndex.ContainsKey(key))
                {
                    rowId = _metadataIndex[key];
                }
                else
                {
                    if (LoggerManager.Instance.REPLogger != null && LoggerManager.Instance.REPLogger.IsErrorEnabled)
                        LoggerManager.Instance.REPLogger.Error("Replication",
                            "Update Operation Failed. Document Key:" + key);
                    return;
                }

                IOperationContext context = null;
                if (operation.Context == null || !operation.Context.ContainsKey(ContextItem.RestoreOperation))
                {
                    context = new OperationContext();
                    context[ContextItem.IsReplicationOperation] = true;
                    context[ContextItem.ReplicationOperationId] = operation.OperationId;
                    if (operation.Context != null)
                    {
                        foreach (var kvp in operation.Context)
                        {
                            context[kvp.Key] = kvp.Value;
                        }
                    }
                }

                bool success = UpdateDocument(rowId, document, WriteConcern.InMemory, context);

                if (!success)
                {
                    if (LoggerManager.Instance.REPLogger != null && LoggerManager.Instance.REPLogger.IsErrorEnabled)
                        LoggerManager.Instance.REPLogger.Error("Replication",
                            "Update Operation Failed. Document Key:" + key);
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.REPLogger != null)
                    LoggerManager.Instance.REPLogger.Error("UserCollection", ex.ToString());
            }
        }

        protected void ReplicateReplace(ILogOperation operation)
        {
            IJSONDocument document = ((DocumentLogOperation)operation).Document;
            DocumentKey key = new DocumentKey(document.Key);
            try
            {
                IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                replaceOperation.Documents = new ClusteredList<IJSONDocument>() { document };
                replaceOperation.Database = operation.Database;
                replaceOperation.Collection = operation.Collection;
                if (operation.Context == null || !operation.Context.ContainsKey(ContextItem.RestoreOperation))
                {
                    replaceOperation.Context = new OperationContext();
                    replaceOperation.Context[ContextItem.IsReplicationOperation] = true;
                    replaceOperation.Context[ContextItem.ReplicationOperationId] = operation.OperationId;
                    if (operation.Context != null)
                    {
                        foreach (var kvp in operation.Context)
                        {
                            replaceOperation.Context[kvp.Key] = kvp.Value;
                        }
                    }
                }

                ReplaceDocuments(replaceOperation); //TODO: What if the operation fails?
                //if (response.IsSuccessfull ||
                //    (LoggerManager.Instance.REPLogger == null || !LoggerManager.Instance.REPLogger.IsErrorEnabled))
                //    return;

                ////foreach (var failedDocuments in response.FailedDocumentsList)   //TODO: Handle this later
                ////{
                ////    if (failedDocuments.ErrorCode != ErrorCodes.Collection.KEY_ALREADY_EXISTS)
                ////    {
                ////        LoggerManager.Instance.REPLogger.Error("Replication", "Insert Operation Failed. Document Key:" + failedDocuments.DocumentKey + " Error Message:" +
                ////                ErrorMessages.GetErrorMessage(failedDocuments.ErrorCode, failedDocuments.ErrorParameters));
                ////    }
                ////}
            }
            //: If this fails and all other operations get performed then there might occur an issue of data inconsistency or even that may lead to further failures.
            // Lets assume I insert document#2 at primary, remove and then insert it back. Assume that remove operation fails at replica. 2 inserts together? Thats not feasible

            //if (_operationLog != null && response.IsSuccessfull)
            //    _operationLog.OnOperationPerformed(operation);
            catch (Exception ex)
            {
                if (LoggerManager.Instance.REPLogger != null)
                    LoggerManager.Instance.REPLogger.Error("UserCollection", ex.ToString());
            }
        }

        protected void ReplicateRemove(ILogOperation operation)
        {
            //IJSONDocument document = JSONDocument.Parse(((Common.Server.Engine.Impl.MinorReplicationOperation)operation).Document);
            DocumentKey key = ((Common.Server.Engine.Impl.DocumentLogOperation)operation).DocumentKey;
            try
            {
                IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                JSONDocument jdoc = new JSONDocument();
                jdoc.Key = key.Value as string;
                deleteOperation.Documents = new ClusteredList<IJSONDocument>() { jdoc };
                deleteOperation.Database = operation.Database;
                deleteOperation.Collection = operation.Collection;

                if (operation.Context == null || !operation.Context.ContainsKey(ContextItem.RestoreOperation))
                {
                    deleteOperation.Context = new OperationContext();
                    deleteOperation.Context[ContextItem.IsReplicationOperation] = true;
                    deleteOperation.Context[ContextItem.ReplicationOperationId] = operation.OperationId;
                    if (operation.Context != null)
                    {
                        foreach (var kvp in operation.Context)
                        {
                            deleteOperation.Context[kvp.Key] = kvp.Value;
                        }
                    }
                }
                /*Operations.RemoveOperation removeOperation = new Operations.RemoveOperation()
                {
                    OperationId = _dbContext.GenerateOperationId(), //to be reviewed
                    RowId = _metadataIndex.GetRowId(key),
                    Collection = operation.Collection,
                    Key = key
                };

                var result = _docStore.DeleteDocument(removeOperation);
                if (result.Success)
                {
                    _dbContext.Journal.Write(removeOperation);
                }/
                //TODO: What if the delete operation fails?
                IDocumentsWriteResponse response = DeleteDocuments(deleteOperation);
                if (response.IsSuccessfull ||
                    (LoggerManager.Instance.REPLogger == null || !LoggerManager.Instance.REPLogger.IsErrorEnabled))
                    return;

                foreach (var failedDocuments in response.FailedDocumentsList)
                {
                    LoggerManager.Instance.REPLogger.Error("Replication", "Delete Operation Failed. Document Key:" + failedDocuments.DocumentKey +
                        " Error Message:" + ErrorMessages.GetErrorMessage(failedDocuments.ErrorCode, failedDocuments.ErrorParameters));
                }
                //if (_operationLog != null && respoknse.IsSuccessfull)
                //    _operationLog.OnOperationPerformed(operation);

            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.REPLogger != null)
                    LoggerManager.Instance.REPLogger.Error("UserCollection", ex.ToString());
            }
        }
        //protected void ReplicateMajorOperation(ILogOperation operation)
        //{
        //    if (((QueryLogOperation)operation).Operation is INonQueryOperation)
        //    {
        //        this.ExecuteNonQuery((INonQueryOperation)((QueryLogOperation)operation).Operation);
        //    }
        //    if (_operationLog != null)
        //        _operationLog.OnOperationPerformed(operation);
        //}

        protected OperationId GetNextOperationId(long operationId, DateTime timeStamp)
        {
            OperationId nextOperationId = new OperationId();
            nextOperationId.Id = operationId;
            nextOperationId.ElectionId = base.ElectionResult.Id;
            nextOperationId.ElectionBasedSequenceId = _context.ElectionBasedSequenceId;
            nextOperationId.TimeStamp = timeStamp;
            return nextOperationId;
        }

        */
        #endregion

        #region IStateTxfrOperationListener Implementation
        public object OnOperationRecieved(IStateTransferOperation operation)
        {
            switch (operation.OpCode)
            {
                case StateTransferOpCode.GetBucketStats:
                    return this._bucketStats;

                case StateTransferOpCode.GetBucketKeysFilterEnumerator:
                    return GetBucketKeysFilterEnumerator((int)operation.Params.GetParamValue(ParamName.BucketID));

                case StateTransferOpCode.GetBucketKeys:
                    return GetBucketKeys((int)operation.Params.GetParamValue(ParamName.BucketID));

                //case StateTransferOpCode.GetLogTable:
                //    {
                //        operation.Params.SetParamValue(ParamName.IsLoggingStopped, true);
                //        return GetLoggedOperations((ArrayList)operation.Params.GetParamValue(ParamName.BucketList), (Boolean)operation.Params.GetParamValue(ParamName.IsLoggingStopped));
                //    }

                //case StateTransferOpCode.RemoveLoggedOperations:
                //    return RemoveLoggedOperations((int)operation.Params.GetParamValue(ParamName.BucketID));

                //case StateTransferOpCode.StartBeforeOperationLogging:
                //    return StartBeforeOperationLogging((ArrayList)operation.Params.GetParamValue(ParamName.BucketList));

                //case StateTransferOpCode.StopBeforeOperationLogging:
                //    return StopBeforeOperationLogging((ArrayList)operation.Params.GetParamValue(ParamName.BucketList));

                case StateTransferOpCode.ApplyLogOperations:
                    ApplyLogOperations(operation.Params.GetParamValue(ParamName.LogOperations) as ClusteredArrayList);
                    break;

                case StateTransferOpCode.EmptyBucket:
                    return RemoveBucketData((int)operation.Params.GetParamValue(ParamName.BucketID));

                case StateTransferOpCode.SetBucketStatus:
                    SetBucketStatus((ArrayList)operation.Params.GetParamValue(ParamName.BucketList), (byte)operation.Params.GetParamValue(ParamName.BucketStatus), (NodeIdentity)operation.Params.GetParamValue(ParamName.BucketFinalShard), (bool)operation.Params.GetParamValue(ParamName.IsSource));
                    break;
            }
            return null;
        }

        #region Log Apply Helping Methods

        private void ApplyLogOperations(ClusteredArrayList logOperations)
        {

            /* Code Removal[Prof]
            if (logOperations != null && logOperations.Count > 0)
            {
                for (int index = 0; index < logOperations.Count; index++)
                {
                    DocumentLogOperation logOperation = logOperations[index] as DocumentLogOperation;
                    try
                    {
                        if (logOperation != null)
                            ApplyLogOperation(logOperation);
                    }
                    catch (Exception ex)
                    {
                        if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                            LoggerManager.Instance.StateXferLogger.Error("ApplyLogOperations", ex.Message);
                    }
                }
            }
            */
        }

        /* Code Removal[Prof]
        private void ApplyUpdateLog(ILogOperation operation)
        {
            IJSONDocument document = ((DocumentLogOperation)operation).Document;
            DocumentKey key = new DocumentKey(document.Key);
            UpdateOperation updateOperation = null;
            try
            {
                updateOperation = new UpdateOperation()
                {
                    OperationId = _dbContext.GenerateOperationId(), //to be reviewed
                    RowId = _metadataIndex.GetRowId(key),
                    Collection = operation.Collection,
                    Update = (JSONDocument)document
                };

                UpdateResult<JSONDocument> result;


                using (_dbContext.LockManager.GetKeyWriterLock(operation.Database, operation.Collection, key))
                {
                    result = _docStore.UpdateDocument(updateOperation);
                }

                if (result.Success)
                {
                    _dbContext.Journal.Write(updateOperation, WriteConcern.InMemory);
                }
                else
                {
                    _docStore.AddFailedOperation(updateOperation);
                }
                //if (_operationLog != null)
                //    _operationLog.OnOperationPerformed(operation);
            }
            catch (Exception ex)
            {
                _docStore.AddFailedOperation(updateOperation);
                if (LoggerManager.Instance.REPLogger != null)
                    LoggerManager.Instance.REPLogger.Error("UserCollection", ex.ToString());
            }
        }

        private void ApplyLogOperation(Alachisoft.NosDB.Common.Server.Engine.Impl.DocumentLogOperation logOperation)
        {
            switch (logOperation.LogOperationType)
            {
                case LogOperationType.Add:
                    {
                        IJSONDocument jdoc = logOperation.Document;
                        jdoc.Key = logOperation.DocumentKey.Value as string;

                        IList<IJSONDocument> documents = new ClusteredList<IJSONDocument>();
                        documents.Add(jdoc);


                        LocalInsertOperation operation = new LocalInsertOperation();
                        operation.Database = DbContext.DatabaseName;
                        operation.Collection = Name;
                        operation.Documents = documents;
                        operation.OperationType = Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType.StateTransferInsert;

                        IDocumentsWriteResponse resposne = InsertDocuments(operation);

                        break;
                    }
                case LogOperationType.Remove:
                    {
                        IJSONDocument jdoc = logOperation.Document;
                        jdoc.Key = logOperation.DocumentKey.Value as string;

                        IList<IJSONDocument> documents = new ClusteredList<IJSONDocument>();
                        documents.Add(jdoc);


                        LocalDeleteOperation operation = new LocalDeleteOperation();
                        operation.Database = DbContext.DatabaseName;
                        operation.Collection = Name;
                        operation.Documents = documents;
                        operation.OperationType = DatabaseOperationType.StateTransferDelete;

                        IDocumentsWriteResponse resposne = DeleteDocuments(operation);

                        break;
                    }

                case LogOperationType.Update:
                    {
                        //System.Collections.Generic.List<IJSONDocument> documents = new System.Collections.Generic.List<IJSONDocument>();
                        //documents.Add(JSONDocument.Parse(logOperation.Document));
                        //IDocumentsWriteOperation operation = new LocalInsertOperation();
                        //operation.Database = DbContext.DatabaseName;
                        //operation.Collection =Name;
                        //operation.Documents = documents;
                        //operation.OperationType = Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType.StateTransferInsert;

                        ApplyUpdateLog(logOperation);
                        break;
                    }
            }
        }
        */
        #endregion

        private ClusteredList<DocumentKey> GetBucketKeys(int bucketID)
        {
            ClusteredList<DocumentKey> bucketKeys = null;

            if (_metadataIndex != null)
            {
                bucketKeys = _metadataIndex.GetKeysForBucket(bucketID);
            }

            return bucketKeys != null ? bucketKeys : new ClusteredList<DocumentKey>();
        }

        private void SetBucketStatus(ArrayList arrayList, byte status, NodeIdentity nodeIdentity, bool isSource)
        {
            if (arrayList != null && _distribution != null)
            {
                foreach (int bucketid in arrayList)
                {
                    var bucket = new HashMapBucket(nodeIdentity.ShardName, bucketid);
                    this._distribution.SetBucketStatus(bucket, status);
                    if (isSource)
                    {
                        lock (_bucketStats)
                        {
                            if (_bucketStats.ContainsKey(bucketid))
                                _bucketStats.Remove(bucketid);
                        }
                    }
                    else
                    {
                        lock (_bucketStats)
                        {
                            if (!_bucketStats.ContainsKey(bucketid))
                                _bucketStats.Add(bucket.BucketId, new KeyValuePair<HashMapBucket, BucketStatistics>(bucket, new BucketStatistics()));
                        }
                    }
                }
            }
            else
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                {
                    if (arrayList == null)
                        LoggerManager.Instance.ShardLogger.Info("UserCollection.SetBucketStatus", "bucketlist is null");

                    if (_distribution == null)
                        LoggerManager.Instance.ShardLogger.Info("UserCollection.SetBucketStatus", "distribution is null for [" + DbContext.DatabaseName + "," + Name + "]");

                }
            }
        }

        internal void SetBucketStatsToDirty()
        {
            if (BucketStatistics == null || BucketStatistics.Count <= 0) return;
            lock (BucketStatistics)
            {
                foreach (var kvp in BucketStatistics.Values)
                {
                    BucketStatistics[kvp.Key.BucketId].Value.IsDirty = true;
                }
            }
        }

        private object RemoveBucketData(int bucketID)
        {
            //NTD: [High] Change status and current shard of this bucket
            return null;
        }

        private IEnumerator<DocumentKey> GetBucketKeysFilterEnumerator(int bucketID)
        {
            IEnumerator<DocumentKey> bucketKeysFilterEnumerator = null;
            if (Distribution != null && Distribution.GetDistributionRouter() != null)
            {
                bucketKeysFilterEnumerator = new BucketKeysFilterEnumerator(bucketID, _metadataIndex, Distribution);
            }

            return bucketKeysFilterEnumerator;
        }
        #endregion
    }
}
