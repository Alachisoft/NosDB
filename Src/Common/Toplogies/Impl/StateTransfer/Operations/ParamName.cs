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
    public enum ParamName : byte
    {
        /// <summary>
        /// Bucket Owner Shard with type [BucketSourceInfo]
        /// </summary>
        OpDestination,

        /// <summary>
        /// Bucket Owner Shard with type [BucketSourceInfo]
        /// </summary>
        BucketFinalShard,

        /// <summary>
        /// Buckets List with type [ArrayList]
        /// </summary>
        BucketList,

        /// <summary>
        /// Bucket ID with type [Int32]
        /// </summary>
        BucketID,

        /// <summary>
        /// Name of shard with type[String]
        /// </summary>
        ShardName,

        /// <summary>
        /// StateTransferType Move/Copy Data with Type[StateTransferType] enum
        /// </summary>
        StateTransferType,

        /// <summary>
        /// IsSparsedBuckets is a boolean argument for checking if the transfered buckets are sparsed or not 
        /// </summary>
        SparsedBuckets,
        /// <summary>
        /// Expected State Transfer ID with type [Int32]
        /// </summary>
        TransferId,

        /// <summary>
        /// IsBalanceDataLoad to check if its normal state transfer for data balance or not type [Boolean]
        /// </summary>
        IsBalanceDataLoad,

        /// <summary>
        /// Should be set in collection whether logging is stopped for bucket being requested.
        /// </summary>
        IsLoggingStopped,

        /// <summary>
        /// A bucket to be sparsed the theshold value is checked
        /// </summary>
        Threshold,

        /// <summary>
        /// Log Operations 
        /// </summary>
        LogOperations,
        
        /// <summary>
        /// Status Of Bucket
        /// </summary>
        BucketStatus,
        /// <summary>
        /// If is source then Bucket Stats will be removed else Bucket Stats will be Added.
        /// </summary>
        IsSource
    }
}