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
using Alachisoft.NosDB.Common.Annotations;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer.Enumerators;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.DataStructures;
using System.Collections.Concurrent;
using Alachisoft.NosDB.Common.Replication;
//using Alachisoft.NosDB.Core.Toplogies.Impl.Replication;



#if DEBUGSTATETRANSFER
using Alachisoft.NCache.Caching.Topologies.History;
#endif

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{

    #region	/                 --- StateTxfrCorresponder ---           /


    class StateTxfrCorresponder : ICorresponder, IDisposable
	{
        /// <summary> 
        /// 200K is the threshold data size. 
        /// Above this threshold value, data will be
        /// transfered in chunks. 
        /// </summary>
        protected long _threshold = 20000 * 1024;//200 * 1000;        

        protected int _currentBucket = -1;        
        private int _lastTxfrId = 0;

        private int _lastKeysChunkId = 0;
        protected int _currentKeyBucket = -1;

        private bool _isBalanceDataLoad;
                       
        private ArrayList _logableBuckets = new ArrayList();
        private StateTransferIdentity corresponderIdentity;

        public StateTransferIdentity CorresponderIdentity
        {
            get { return corresponderIdentity; }           
        }
        private IDispatcher _dispatcher;
        private NodeContext _context = null;        
        private string bucketKeysColName;
        private string prefix = "corresponder";
        private string tokenizer = "_";
        private IEnumerator<DocumentKey> _bucketKeysFilterEnumerator = null;
        //private Alachisoft.NosDB.Common.Threading.IThreadPool threadPool;

        public NodeContext Context { get { return _context; } }
        
        //private IEnumerator<DocumentKey> _bucketkeysReader;
        private ClusteredList<DocumentKey> _currentBucketkeys;
        private int _sentKeysCount;
        private OperationId _startedFrom = null;

        private ClusteredList<DocumentKey> _transferedBucketKeys=new ClusteredList<DocumentKey>();

        private StatsIdentity _statsIdentity;
        private string loggingModule;

        private IDatabasesManager _databasesManager = null;
        private bool collectionCreated;

        private Boolean _isTransferCompleted = false;
        private bool _isLocal;        
        private ConcurrentQueue<IResourceRemovalInfo> transferedBuckets = null;
        private string _resourceID;

        /// <summary>
        /// Gets or sets a value indicating whether this StateTransfer Corresponder is in Data balancing mode or not.
        /// </summary>
        public bool IsBalanceDataLoad
        {
            get { return _isBalanceDataLoad; }
            set { _isBalanceDataLoad = value; }
        }

        internal StateTxfrCorresponder(NodeContext context,IDispatcher dispatcher,StateTransferIdentity identity, bool isLocal)//,String dbName,String colName,NodeIdentity requestingShard, StateTransferType transferType)
		 {
             _context = context;
             _dispatcher = dispatcher;
            _isLocal = isLocal;            
            if(_context!=null)
                _databasesManager = _context.DatabasesManager;
                        
            corresponderIdentity = identity;
            _currentBucketkeys = new ClusteredList<DocumentKey>();
            // Create Collection for keeping keys being transfered
            collectionCreated = CreateBucketKeysCollection();

            transferedBuckets = new ConcurrentQueue<IResourceRemovalInfo>();

            _statsIdentity = new StatsIdentity(context.LocalShardName, corresponderIdentity.DBName);
            loggingModule = corresponderIdentity.DBName + ":" + corresponderIdentity.ColName + ":" + GetType().ToString();

            _resourceID = Guid.NewGuid().ToString();
		 }

        private void EmptyNonShardedCollection() 
        {
            //currently no need to empty non-sharded collection as state transfer for non-sharded collection is supported in shard-removal(gracefull)
            return;
        }

        private void AckStateTransferCompeleted(ArrayList bucketIds)
        {
            if (_isTransferCompleted)
            {
                LoggerManager.Instance.StateXferLogger.Info("AckStateTransferCompeleted", "State Transfer Ended "+bucketIds[0]);                
            }

            switch (CorresponderIdentity.DistributionType)
            {
                case Common.Toplogies.Impl.Distribution.DistributionMethod.HashDistribution:
                case Common.Toplogies.Impl.Distribution.DistributionMethod.RangeDistribution:
                case Common.Toplogies.Impl.Distribution.DistributionMethod.TagDistribution:
                    {
                        try
                        {
                            IEnumerator ie = bucketIds.GetEnumerator();
                            
                            while (ie.MoveNext())
                            {
                                //muds:
                                //remove this bucket from the local buckets.
                                //this bucket has been transfered to some other node.
                                /// Remove the bucket if this node is no longer owner of this bucket.
                                int bucketId = (int)ie.Current;

                                //if (!VerifyFinalOwnership(bucketId, new NodeIdentity(Context.LocalShardName, Context.LocalAddress)))
                                {
                                    lock (transferedBuckets)
                                    {
                                        IEnumerable<DocumentKey> bucketKeys = new ClusteredList<DocumentKey>(_currentBucketkeys);//GetBucketKeys(bucketId, false).Clone() as ClusteredList<DocumentKey>;
                                        transferedBuckets.Enqueue(new BucketRemovalInfo(corresponderIdentity.DBName,corresponderIdentity.ColName, bucketId, bucketKeys.GetEnumerator()));                                        
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerManager.Instance.StateXferLogger.Error("AckStateTransferCompeleted.BucketKeysEnumerator", ex.Message);
                        }                      
                    }
                    break;
                case Common.Toplogies.Impl.Distribution.DistributionMethod.NonShardedDistribution:
                    try
                    {
                        lock (transferedBuckets)
                        {
                            IEnumerable<DocumentKey> bucketKeys = new ClusteredList<DocumentKey>(_currentBucketkeys);
                            //GetBucketKeys(bucketId, false).Clone() as ClusteredList<DocumentKey>;
                            transferedBuckets.Enqueue(new CollectionRemvoalInfo(CorresponderIdentity.DBName,CorresponderIdentity.ColName));
                        }
                    }
                    catch (Exception)
                    {

                    }
                    break;
            }
        }

        private StateTxfrInfo TransferBucket(ArrayList bucketIds, bool sparsedBuckets, int expectedTxfrId)
		 {
			 if (sparsedBuckets)
			 {
                 return new StateTxfrInfo(true);            
			 }
			 else
			 {
				 if (bucketIds != null && bucketIds.Count > 0)
				 {
					 if (!(_currentBucket == (int)bucketIds[0]))
					 {
                         if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                             LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TxfrBucket", "bucketid : " + bucketIds[0] + " exptxfrId : " + expectedTxfrId);

						 _lastTxfrId = expectedTxfrId;
						 //request for a new bucket. get its key list from parent.
						 _currentBucket = (int)bucketIds[0];
                        
                         //these are comment out becoz enable log flag has been passed on Get Keys for bucket call from requesting shard
                         ///bool enableLogs = _transferType == StateTransferType.MOVE_DATA ? true : false;
                         //_keyList = GetKeyList(_currentBucket, enableLogs) as ClusteredArrayList;

                         //bool enableLogs = corresponderIdentity.Type == StateTransferType.INTRA_SHARD ? true : false;


                         //Need to reset data-structures for next bucket transfer
                         _currentBucketkeys.Clear();
                         _sentKeysCount = 0;

                         //STD: Start Bucket Logging if enable
                         if (corresponderIdentity.Type==StateTransferType.INTER_SHARD)
                         {
                             _startedFrom = StartLoggingOnSource(_currentBucket);
                         }

                         _currentBucketkeys.AddRange(GetBucketKeys(_currentBucket));//.Clone() as ClusteredList<DocumentKey>;

						 _logableBuckets.Add(_currentBucket);

						 //muds:
						 //reset the _lastLogTblCount
						 //_sendLogData = false;
					 }
					 else
					 {
                         if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                             LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TxfrBucket", "bucketid : " + bucketIds[0] + " exptxfrId : " + expectedTxfrId);
                         
                         //remove all the last sent keys from keylist that has not been
                         //modified during this time.
                         if (_currentBucketkeys != null && expectedTxfrId > _lastTxfrId)
                         {
                             //lock (_currentBucketkeys)
                             //{
                             //    _currentBucketkeys.RemoveRange(0, _sentKeysCount);
                             //    _sentKeysCount = 0;
                             //}
                             _lastTxfrId = expectedTxfrId;
                         }

					 }
				 }
				 else
				 {
                     return new StateTxfrInfo(new HashVector(), true, 0);//,null);
				 }

				 //muds:
				 //take care that we need to send data in chunks if 
				 //bucket is too large.
				 return GetData(_currentBucket);
			 }
		 }
        
        /// <summary>
        /// Get Keys for requested bucket ids
        /// </summary>
        /// <param name="bucketIds"></param>
        /// <param name="chunkid"></param>
        /// <returns></returns>
         private StateTxfrInfo TransferBucketKeys(ArrayList bucketIds, int chunkid)
         {             
             if (bucketIds != null && bucketIds.Count > 0)
             {

                 if (!(_currentKeyBucket == (int)bucketIds[0]))
                 {
                     if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                         LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetBucketKeys", "bucketid : " + bucketIds[0] + " chunkid : " + chunkid);

                     _lastKeysChunkId = chunkid;                    
                     _currentKeyBucket = (int)bucketIds[0];
                     bool enableLogs = corresponderIdentity.Type == StateTransferType.INTER_SHARD ? true : false;
                     _bucketKeysFilterEnumerator = GetBucketKeysFilterEnumerator(_currentKeyBucket, enableLogs);
                     
                     _logableBuckets.Add(_currentKeyBucket);                     
                 }              
             }
             else
             {
                 return new StateTxfrInfo(new HashVector(), true, 0);//, null);
             }

             return GetKeysChunk(_currentKeyBucket);
         }

         private StateTxfrInfo GetKeysChunk(int bucketId)
         {
             HashVector result = new HashVector();

             long sizeToSend = 0;
             bool lastChunk = true;

             try
             {
                 if (_bucketKeysFilterEnumerator != null && _bucketKeysFilterEnumerator.MoveNext())
                 {
                     List<DocumentKey> bucketKeysList = new List<DocumentKey>();

                     do
                     {
                         DocumentKey key = _bucketKeysFilterEnumerator.Current;

                         if (key != null)
                         {
                             bucketKeysList.Add(key);

                             long size = GetDocumentSize(key as ISize);
                             sizeToSend += size;

                             if (sizeToSend > _threshold)
                             {
                                 lastChunk = false;
                                 break;
                             }
                         }

                     } while (_bucketKeysFilterEnumerator.MoveNext());

                     result.Add(bucketId, bucketKeysList);
                     InsertKeysToCollection(bucketId, new ArrayList(bucketKeysList));

                     return new StateTxfrInfo(result, lastChunk, sizeToSend);//, this.stream);
                 }
                 else
                 {
                     return new StateTxfrInfo(null, true, 0);//, null);
                 }
             }
             finally 
             {
                 if (LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                 {
                     List<DocumentKey> keys=result[bucketId] as List<DocumentKey>;
                     int count = keys==null?0:keys.Count;
                     LoggerManager.Instance.StateXferLogger.Debug("StateXfer", loggingModule+" Corresponder.TransferBucketKeys - BucketId: " + bucketId + " keys : " + count);
                 }
             }
         }

        ///// <summary>
        ///// Implement this method for verifying tempory owener ship change in case of multiple state transfer
        ///// </summary>
        ///// <param name="bkId"></param>
        ///// <param name="p"></param>
        ///// <returns></returns>
        // private Boolean VerifyFinalOwnership(int bkId, NodeIdentity finalShard)
        // {
        //     if (finalShard!=null)
        //     {
        //         IStateTransferOperation operation = null;
        //         try
        //         {
        //             operation = CreateStateTransferOperation(StateTransferOpCode.VerifyFinalOwnerShip);
        //             operation.Params.SetParamValue(ParamName.BucketFinalShard, finalShard);
        //             operation.Params.SetParamValue(ParamName.BucketID, bkId);
        //             return (Boolean)_dispatcher.DispatchOperation<Object>(operation);
        //         }
        //         catch (Exception e)
        //         {
        //             if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
        //                 LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".VerifyFinalOwnership", e.ToString());
        //             try
        //             {
        //                 return (Boolean)Context.ConfigurationSession.StateTransferOperation(Context.ClusterName, operation);
        //             }
        //             catch (Exception ex)
        //             {
        //                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
        //                     LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".VerifyFinalOwnership", ex.ToString());
        //             }
        //         }
        //     }

        //     return false;
        // }

		 private StateTxfrInfo GetData(int bucketId)
		 {
             HashVector result = new HashVector();
			 long sizeToSend = 0;
             bool lastChunk = true;

             if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                 LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetData(2)", "state txfr request for :" + bucketId + " _lastTxfrId :" + _lastTxfrId);

             if (_currentBucketkeys != null && _currentBucketkeys.Count > 0 && _sentKeysCount<_currentBucketkeys.Count)
			 {

                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                     LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetData(2)", "bucket size :");// + _keyList.Count);

                 //do{                     
                 for (/*_sentKeysCount = 0*/; _sentKeysCount < _currentBucketkeys.Count; _sentKeysCount++)
                 {
                     //DocumentKey docKey = _currentBucketkeys.Current as DocumentKey;

                     DocumentKey docKey = _currentBucketkeys[_sentKeysCount] as DocumentKey;
                     if (docKey == null) continue;

                     JSONDocument jdoc = new JSONDocument();                    
                     jdoc.Key = docKey.Value as string;
                     
                     System.Collections.Generic.List<IJSONDocument> documents = new System.Collections.Generic.List<IJSONDocument>();                     
                     documents.Add(jdoc);

                     IGetOperation operation = new LocalGetOperation();
                     operation.Database = corresponderIdentity.DBName;
                     operation.Collection = corresponderIdentity.ColName;
                     operation.DocumentIds = documents;
                     IGetResponse response = null;
                     if (_databasesManager != null)
                     {
                         response = _databasesManager.GetDocuments(operation);
                     }
                     if (response != null)
					 {
                         IJSONDocument doc = (response.DataChunk!=null && response.DataChunk.Documents!=null)?response.DataChunk.Documents[0]:null;
                         
                         if (doc != null)
                         {

                             long size = GetDocumentSize(doc as ISize);

                             result[docKey] = doc;
                             sizeToSend += size;
                             if (sizeToSend > _threshold)
                             {
                                 lastChunk = false;
                                 break;
                             }
                         }
					 }
				 }
                 //} while (_currentBucketkeys.MoveNext());

                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                     LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetData()", "bucketid: "+bucketId+" items sent: " + result.Count);

                 if (_isBalanceDataLoad)
                     if (StatsManager.Instance.GetStatsCollector(_statsIdentity) != null)
                         StatsManager.Instance.GetStatsCollector(_statsIdentity).IncrementStatsValue(StatisticsType.DataBalancePerSec, result.Count);
                     else
                         if (StatsManager.Instance.GetStatsCollector(_statsIdentity) != null)
                             StatsManager.Instance.GetStatsCollector(_statsIdentity).IncrementStatsValue(StatisticsType.StateTransferPerSec, result.Count);

                 return new StateTxfrInfo(result, false, sizeToSend);//, this.stream);
			 }
             else if (corresponderIdentity.Type == StateTransferType.INTER_SHARD)
             {
                 SetBucketStatus(corresponderIdentity.NodeInfo,new ArrayList(){_currentBucket});                

                 StopLoggingOnSource(_currentBucket);
                
                 //We need to transfer the logs.
                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                     LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetData(2)", "sending log data for bucket: " + bucketId);
                
                 return GetLoggedData(_currentBucket);
             }
             else
             {
                 //As transfer mode is not MOVE_DATA, therefore no logs are maintained
                 //and hence are not transferred.
                 return new StateTxfrInfo(null,true,0);//, null);
             }
		 }

         private long GetDocumentSize(ISize doc)
         {                          
             if(doc==null)return 1;

             return doc.Size;
         }

		 private StateTxfrInfo GetLoggedData(int bucketId)
		 {             			
             ICollection logTbl = null;
			 StateTxfrInfo info = null;			 			 
             try
			 {
                 LoggingIdentity identity=new LoggingIdentity(corresponderIdentity.DBName,corresponderIdentity.ColName,bucketId);
                 logTbl =  GetLoggedOperations(_startedFrom,identity);
				 if (logTbl != null)
				 {  
                     info = new StateTxfrInfo(logTbl, true, 0);
                     info.loggedData = true;

                     if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                         LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetLoggedData()", info == null ? "returning null state-txfr-info" : "returning " + info.data.Count.ToString() + " items in state-txfr-info");
					 return info;
				 }
				 else
                     if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                         LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".GetLoggedData", "no logged data found");
			 }
			 catch (Exception e)
			 {
                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                     LoggerManager.Instance.StateXferLogger.Error(loggingModule + ".GetLoggedData", e.ToString());
				 throw;
			 }

			 //muds:
			 //no operation has been logged during state transfer.
			 //so announce completion of state transfer for this bucket.
             return new StateTxfrInfo(new HashVector(), true, 0);
		 }
         /// <summary>
         /// Set Status of transfered bucket locally        
         /// </summary>
         /// <param name="owner"></param>
         /// <param name="buckets"></param>
         /// <param name="sparsed"></param>
         private void SetBucketStatus(NodeIdentity owner, ArrayList buckets)
         {
             if (this._dispatcher != null)
             {
                 IStateTransferOperation operation = CreateStateTransferOperation(StateTransferOpCode.SetBucketStatus);
                 operation.Params.SetParamValue(ParamName.BucketList, buckets);
                 operation.Params.SetParamValue(ParamName.BucketStatus, BucketStatus.Functional);
                 operation.Params.SetParamValue(ParamName.BucketFinalShard,owner);
                 operation.Params.SetParamValue(ParamName.IsSource, true);
                 _dispatcher.DispatchOperation<Object>(operation);
             }             
         }

         #region Database Operations

         private bool CreateBucketKeysCollection()
         {
             //bucketKeysColName = prefix + tokenizer + corresponderIdentity.DBName + tokenizer + corresponderIdentity.ColName;

             //LocalCreateCollectionOperation operation = new LocalCreateCollectionOperation();
             //operation.Database = Common.MiscUtil.SYSTEM_DATABASE;
             //operation.Configuration = Alachisoft.NosDB.Core.Util.MiscUtil.GetBaseCollectionConfiguration(_context.LocalShardName,bucketKeysColName);
             //if (_databasesManager != null)
             //    _databasesManager.CreateCollection(operation);
             //else
             //    return false; 
             return true;
         }

         private void DropCollection(String database,String collection)
         {
             var operation = new LocalDropCollectionOperation();
             operation.Database = database;
             operation.Collection = collection;

             if (_databasesManager != null)
             {
                 IDBResponse dbResponse = _databasesManager.DropCollection(operation);
                 if (dbResponse.IsSuccessfull)
                 {
                     ((PartitionOfReplica)Context.TopologyImpl).PulseGetClusterConfAndStore();
                 }
             }
         }

         private ClusteredList<DocumentKey> GetBucketKeys(int _currentBucket)
         {
             if (_dispatcher != null)
             {
                 IStateTransferOperation operation = CreateStateTransferOperation(StateTransferOpCode.GetBucketKeys);                 
                 operation.Params.SetParamValue(ParamName.BucketID, _currentBucket);

                 return _dispatcher.DispatchOperation<ClusteredList<DocumentKey>>(operation);
             }
             return new ClusteredList<DocumentKey>();
         }

         /// <summary>
         /// This is a temporaray method should be removed
         /// </summary>
         /// <param name="_currentBucket"></param>
         /// <param name="keys"></param>
         private void InsertKeysToCollection(int _currentBucket, ICollection keys)
         {
             if (keys != null && keys.Count > 0)
             {
                 IEnumerator keysEnum = keys.GetEnumerator();
                 LocalInsertOperation operation = new LocalInsertOperation();
                 operation.Database = Common.MiscUtil.SYSTEM_DATABASE;
                 operation.Collection = this.bucketKeysColName;
                 operation.Documents = new ClusteredList<IJSONDocument>();

                 while (keysEnum.MoveNext())
                 {
                     try
                     {
                         StateTransferKey key = new StateTransferKey(_currentBucket, keysEnum.Current as DocumentKey);

                         JSONDocument stateTransferKeyDoc = JsonSerializer.Serialize <StateTransferKey>(key);
                         operation.Documents.Add(stateTransferKeyDoc);
                     }
                     catch(Exception ex)
                     {
                         //log exception
                     }
                 }
                 if (_databasesManager != null)
                    _databasesManager.InsertDocuments(operation);
             }

         }
                
         #endregion

         #region Dispactech Operations

         /// <summary>
        ///  Bucket Keys Filter Enumerator is special Enumerator which iterate collection to filter out keys which belong to specified bucket
        ///  And enable logging for that bucket on oplog if provided
        /// </summary>
        /// <param name="_currentBucket"></param>
        /// <param name="enableLogs"></param>
        /// <returns></returns>
         private IEnumerator<DocumentKey> GetBucketKeysFilterEnumerator(int _currentBucket, bool enableLogs)
         {
             IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.GetBucketKeysFilterEnumerator);
             operation.Params.SetParamValue(ParamName.BucketID, _currentBucket);             
             return _dispatcher.DispatchOperation<IEnumerator<DocumentKey>>(operation);
         }

        /// <summary>
        /// Get Log Table to be transfered from oplog for bucket being provided
        /// </summary>
        /// <param name="bucketIds"></param>
        /// <param name="isLoggingStopped"></param>
        /// <returns></returns>
         //private ICollection GetLogTable(ArrayList bucketIds, ref bool isLoggingStopped)
         //{
         //    IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.GetLogTable);
         //    operation.Params.SetParamValue(ParamName.BucketList, bucketIds);
         //    operation.Params.SetParamValue(ParamName.IsLoggingStopped, isLoggingStopped);

         //    ICollection vector = _dispatcher.DispatchOperation<ICollection>(operation);
         //    isLoggingStopped = (bool)operation.Params.GetParamValue(ParamName.IsLoggingStopped);

         //    return vector;
         //}

        /// <summary>
        /// Remove Log Table from oplog for provided bucket id
        /// </summary>
        /// <param name="bucketID"></param>
         //private void RemoveLoggedOperations(int bucketID)
         //{
         //    IStateTransferOperation operation = this.CreateStateTransferOperation(StateTransferOpCode.RemoveLoggedOperations);
         //    operation.Params.SetParamValue(ParamName.BucketID, bucketID);

         //    _dispatcher.DispatchOperation<HashVector>(operation);
         //}

         private IStateTransferOperation CreateStateTransferOperation(StateTransferOpCode stateTransferOpCode)
         {
             return new StateTransferOperation(corresponderIdentity, stateTransferOpCode, new OperationParam());
         }

         #endregion

         #region IDisposable Members

         /// <summary>
         /// Disposes the state txfr corresponder. On dispose corresponder should
         /// stop logger in the hashed cache if it has turned on any one.
         /// </summary>
         public void Dispose()
         {
             try
             {
                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                     LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Dispose", corresponderIdentity.NodeInfo.ToString() + " corresponder disposed");

                 if (corresponderIdentity.Type == StateTransferType.INTER_SHARD)
                 {
                     if (_logableBuckets != null)
                     {
                         for (int i = 0; i < _logableBuckets.Count; i++)
                         {
                             if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                 LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Dispose", " removing logs for bucketid " + _logableBuckets[i]);
                             RemoveLoggedOperations((int)_logableBuckets[i]);
                         }
                     }
                 }

                 if (collectionCreated)
                 {
                     //DropBucketKeysCollection();
                 }
             }
             catch (Exception ex)
             {
                 if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                 {
                     LoggerManager.Instance.StateXferLogger.Error("StateTxferCorresponder.Dispose", "error on " + this.corresponderIdentity.ToString() + " " + ex.Message);
                 }
             }
         }
       
         #endregion

              
         
        
        #region ICorresponder Implementation

         public bool FreeResources()
         {
             try
             {
                 IResourceRemovalInfo info = null;

                 while (transferedBuckets.Count != 0)
                 {
                     lock (transferedBuckets)
                     {
                         transferedBuckets.TryDequeue(out info);
                     }

                     if (info != null)
                     {
                         switch (info.Type)
                         {                                 
                             case ResourceType.Bucket:
                                 BucketRemoval.Execute((BucketRemovalInfo)info, Context.DatabasesManager, _isLocal);
                                 break;
                             case ResourceType.Collection:
                                 var colRemInfo = info as CollectionRemvoalInfo;
                                 if(colRemInfo!=null)
                                     DropCollection(colRemInfo.Database,colRemInfo.Collection);
                                 break;
                         }
                     }
                 }

                 if (_isTransferCompleted && transferedBuckets.Count == 0)
                 {
                     //try
                     //{
                     //    DropBucketKeysCollection();
                     //}
                     //catch (Exception ex)
                     //{
                     //    LoggerManager.Instance.StateXferLogger.Error("StateTxferCorresponder.FreeResources(1)", ex.Message);
                     //}

                     return true;
                 }
             }
             catch (Exception ex)
             {
                 LoggerManager.Instance.StateXferLogger.Error("StateTxferCorresponder.FreeResources(2)", ex.Message);
             }

             return false;

         }

         public String ResourceID { get { return _resourceID; } }

        
        //public bool FreeResources()
        // {
        //           try
        //    {
        //        BucketRemovalInfo info = null;

        //        while (transferedBuckets.Count != 0)
        //        {
        //            lock (transferedBuckets)
        //            {
        //                transferedBuckets.TryDequeue(out info);
        //            }

        //            if (info != null)
        //            {
        //                BucketRemoval.Execute(info, Context.DatabasesManager, _isLocal, _isReplica);
        //            }
        //        }

        //        if (_isTransferCompleted && transferedBuckets.Count == 0)
        //        {
        //            try
        //            {
        //                DropBucketKeysCollection();
        //            }
        //            catch (Exception ex)
        //            {
        //                LoggerManager.Instance.StateXferLogger.Error("StateTxferCorresponder.FreeResources(1)", ex.Message);
        //            }

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerManager.Instance.StateXferLogger.Error("StateTxferCorresponder.FreeResources(2)", ex.Message);
        //    }

        //    return false;
        
        // }

        public StateTxfrInfo GetBucketData(OperationParam op)
         {
             return TransferBucket((ArrayList)op.GetParamValue(ParamName.BucketList), (bool)op.GetParamValue(ParamName.SparsedBuckets), (int)op.GetParamValue(ParamName.TransferId));
         }
        
        public StateTxfrInfo GetBucketKeys(OperationParam op)
        {
            return TransferBucketKeys((ArrayList)op.GetParamValue(ParamName.BucketList), (int)op.GetParamValue(ParamName.TransferId));
        }
        
        public void TransferCompleted()
        {

            try
            {
                _isTransferCompleted = true;

                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".TransferComplete", corresponderIdentity.NodeInfo.ToString() + " corresponder disposed");

                if (corresponderIdentity.Type == StateTransferType.INTER_SHARD)
                {
                    if (_logableBuckets != null)
                    {
                        for (int i = 0; i < _logableBuckets.Count; i++)
                        {
                            if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                                LoggerManager.Instance.StateXferLogger.Debug(loggingModule + ".Dispose", " removing logs for bucketid " + _logableBuckets[i]);
                            RemoveLoggedOperations((int)_logableBuckets[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.StateXferLogger.Error("StateTxferCorresponder.Dispose", "error on " + this.corresponderIdentity.ToString() + " " + ex.Message);
                }
            }
        }

        public void BucketTransferCompeleted(OperationParam op)
        {
            AckStateTransferCompeleted(op.GetParamValue(ParamName.BucketList) as ArrayList);
        }
        
        #endregion


        #region State Transfer Logging Code
        private OperationId StartLoggingOnSource(int bucketID)
        {
            OperationId opId = new OperationId();

            //if (Context.OperationLog != null)
            //{
            //    ILogOperation lastOp = Context.OperationLog.LastOperation;
            //    if (lastOp != null && lastOp.OperationId != null)
            //    {
            //        opId = lastOp.OperationId;
            //    }

            //    Context.OperationLog.StartLogging(new LoggingIdentity(corresponderIdentity.DBName, corresponderIdentity.ColName, bucketID), LogMode.AfterActualOperation);

            //}
            return opId;
        }

        private void StopLoggingOnSource(int bucketID)
        {
            //if (Context.OperationLog != null)
            //{
            //    Context.OperationLog.StopLogging(new LoggingIdentity(corresponderIdentity.DBName,corresponderIdentity.ColName, bucketID));
            //}
        }

        private ICollection GetLoggedOperations(OperationId operationId,LoggingIdentity identity)
        {
            //if (Context.OperationLog != null)
            //{
            //    return Context.OperationLog.GetLoggedOperations(identity,operationId);
            //}
            return null;
        }

        private object RemoveLoggedOperations(int bucketID)
        {
            //LoggingIdentity identity = new LoggingIdentity(this.CorresponderIdentity.DBName,CorresponderIdentity.ColName, bucketID);
            //if (Context.OperationLog != null)
            //    return Context.OperationLog.RemoveLoggedOperations(identity);
            return null;
        }

        #endregion
    }


   


    #endregion



    #region Bucket Data Removal Task

    #endregion         

}
