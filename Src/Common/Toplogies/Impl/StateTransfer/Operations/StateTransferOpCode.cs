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
namespace Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations
{
    public enum StateTransferOpCode : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Get Primary node of given shard name
        /// </summary>
        GetShardPrimary,
        /// <summary>
        /// Lock Buckets cluster wide on configuration server and change bucket status to state transfer
        /// </summary>
        LockBucketsOnCS,

        /// <summary>
        /// Release locked buckets on configuration service and changes status accordingly
        /// </summary>
        ReleaseBucketsOnCS,

        /// <summary>
        /// Announce State Transfer for given buckets on configuration server and change buckets status to state transfer
        /// </summary>
        AnnounceBucketTxfer,

        /// <summary>
        /// Transfer Bucket data from target node 
        /// </summary>  
        TransferBucketData,

        /// <summary>
        /// Transfer Bucket keys from target node 
        /// </summary>  
        TransferBucketKeys,

        /// <summary>
        /// Acknowledge transfer completed, and broadcast to all shards so that they can remove it from there local buckets.
        /// </summary>
        AckBucketTxfer,

        /// <summary>
        /// Remote Operation to Create Corresponder on Providing Node
        /// </summary>
        CreateCorresponder,

        /// <summary>
        /// Remote Operation to Destroy Corresponder on Providing Node

        /// </summary>
        DestroyCorresponder,

        /// <summary>
        /// End of state transfer announced locally and also set latch to running for current collection, update local statistics and annonce presence [Broadcast stats]
        /// </summary>
        StateTxferCompeleted,

        /// <summary>
        /// End of state transfer announced locally and also set latch to running for current collection, update local statistics and annonce presence [Broadcast stats]
        /// </summary>
        StateTxferFailed,

        /// <summary>
        /// Currently No use of this OpCode, may be in future used for some operation
        /// </summary>
        EmptyBucket,

        /// <summary>
        /// Locally Get Keys List for specified bucket id
        /// </summary>
        GetBucketKeysFilterEnumerator,

        ///// <summary>
        ///// Locally Rmove Log Table for spicified bucket list
        ///// </summary>
        //RemoveLoggedOperations,

        ///// <summary>
        ///// Locally Get Log Table for specified bucket list
        ///// </summary>
        //GetLogTable,

        /// <summary>
        /// Locally Get Bucket Stats for specifed collection
        /// </summary>
        GetBucketStats,

        /// <summary>
        /// Check If the Bucket is sparsed from configuration server
        /// </summary>
        IsSparsedBucket,
        /// <summary>
        /// Verify Final OwnerShip of bucket
        /// </summary>
        VerifyFinalOwnerShip,

        ///// <summary>
        ///// Start Bucket Logging Locally
        ///// </summary>
        //StartBeforeOperationLogging,

        ///// <summary>
        ///// Stop Bucket Logging Locally
        ///// </summary>        
        //StopBeforeOperationLogging,

        /// <summary>
        /// Apply Logged Operation Locally
        /// </summary>
        ApplyLogOperations,

        /// <summary>
        /// Set Bucket Status Locally
        /// </summary>
        SetBucketStatus,

        /// <summary>
        /// Final Steps to be taken on ending state transfer
        /// </summary>
        FinalizeStateTransfer,

        /// <summary>
        /// Check if provided shard is connected with this node or not
        /// </summary>
        IsShardConnected,

        /// <summary>
        /// Get Bucket Keys
        /// </summary>
        GetBucketKeys,
        StateTrxferStarted,
        StartQueryLogging,
        EndQueryLogging,
        
    }
}
