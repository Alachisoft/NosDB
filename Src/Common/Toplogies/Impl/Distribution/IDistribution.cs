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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution
{
    /// <summary>
    /// This class represents the data distribution according to the distribution strategy.
    /// </summary>
  // [JsonConverter(typeof(DistributionJsonConverter))]
    public interface IDistribution : ICompactSerializable,ICloneable
    {
        /// <summary>
        /// Every distribution has a sequence no which is increasing no. This sequence id
        /// not only uniquely identify a distribution but also helps distinguish between old and new
        /// distribution. 
        /// </summary>
        [JsonProperty(PropertyName = "DistributionSequence")]
        int DistributionSequence { get; }

        /// <summary>
        /// Gets buckets assigned to a shard
        /// </summary>
        /// <param name="shard">name of the shard</param>
        /// <returns>Buckets</returns>
        HashMapBucket[] GetBucketsForShard(string shard);

        /// <summary>
        /// Gets names of shards covered by this distribution
        /// </summary>
        /// <returns></returns>
        List<String> GetShards();

        /// <summary>
        /// Gets the distriubtion router based on this distribution
        /// </summary>
        /// <returns></returns>
        IDistributionRouter GetDistributionRouter();

        /// <summary>
        /// SetBucketStatus on database servers during state transfer
        /// </summary>
        /// <param name="bucketID"></param>
        /// <param name="status"></param>
        void SetBucketStatus(HashMapBucket bucket, byte status);

        /// <summary>
        /// DistributionType could be Hash,Range,Tag or NonSharded
        /// </summary>
        /// <param name="bucketID"></param>
        /// <param name="status"></param>
        DistributionMethod Type { get; }

    }
}
