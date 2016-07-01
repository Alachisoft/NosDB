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

﻿using System;
using System.Collections;
using System.Collections.Generic;
﻿using Alachisoft.NosDB.Common.Annotations;
﻿using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Stats;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution
{
    /// <summary>
    /// A distribution strategy defines how data is distributed among shards. 
    /// A bucket is the minimum unit of data distribution. A bucket can be assigned to only a single
    /// shard at any point in time. There is no limit on no of buckets assigned to a shard, however,
    /// no of buckets should not be too small that data become imbalaced among shards. 
    /// </summary>

    //[JsonConverter(typeof(DistributionStrategyJsonConverter))]
    public interface IDistributionStrategy : ICompactSerializable,ICloneable
    {

        [JsonProperty(PropertyName = "Name")]
        string Name { get; set; }

        /// <summary>
        /// Gets distribution upon adding a shard. 
        /// </summary>
        /// <param name="shard">shard to add</param>
        /// <param name="configuration"></param>
        /// <returns>Updated disribution after addition of shard</returns>
        IDistributionStrategy AddShard(string shard, IDistributionConfiguration configuration,Boolean needTransfer);

        /// <summary>
        /// Gets distribution upon removal of a shard. 
        /// </summary>
        /// <param name="shard">shard to remove</param>
        /// <param name="configuration"></param>
        /// <param name="gracefull">Flag that indicates whether shard removal is gracefull or not. In gracefull shard
        /// removal data is copied from removing shard to other shards. When data is copied, then this shard
        /// is removed from final distribution</param>
        /// <returns>Updated distribution after node removal</returns>
        IDistributionStrategy RemoveShard(string shard, IDistributionConfiguration configuration,bool gracefull);

        /// <summary>
        /// Balance the data among shard if data is imbalanced. It depends on the actual distribution if balancing is 
        /// possible or not.
        /// </summary>
        /// <returns>Updated distribution after balancing</returns>
        IDistribution BalanceShards();

        /// <summary>
        /// Gets the current distribution.
        /// </summary>
        /// <returns>Current distribution</returns>
        IDistribution GetCurrentBucketDistribution();

        void UpdateBucketStats(ShardInfo shardInfo);

        [JsonProperty(PropertyName = "DistributionConfiguration")]
        IDistributionConfiguration DistributionConfiguration { get; }

        Dictionary<string, long> GetAmountOfCollectionDataOnEachShard();

        Hashtable LockBuckets(ArrayList buckets, string requestingShard);

        void ReleaseBuckets(ArrayList buckets, string requestingShard);

        bool CanRemoveShard(string p);

        IDistribution Distribution { get; }
    }
}
