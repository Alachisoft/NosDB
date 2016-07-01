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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.Services
{

    public class CollectionInfo : ICloneable,  ICompactSerializable,IObjectId
    {
        
        private IDistributionStrategy _distributionStrategy = null;

        /// <summary>
        /// Gets/Sets the name of the collection
        /// </summary>
        /// 
        string _name = "";
        private CappedInfo _cappedInfo;

        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name.ToLower(); } set { _name = value; } }

        /// <summary>
        /// Capped Info is defined for capped collections
        /// </summary>
        [JsonProperty(PropertyName = "CappedInfo")]
        public CappedInfo CappedInfo
        {
            set
            {
                //if (value == null)
                //throw new Exception("Maximum size must be defined for a capped collection");
                _cappedInfo = value;
            }
            get
            {
                // if (!IsCapped)
                //throw new Exception("Collection '" + _name + "' is not a capped collection");
                return _cappedInfo;
            }
        }

        /// <summary>
        /// The shards/shard on which the collection would be created
        /// </summary>
         [JsonProperty(PropertyName = "CollectionShard")]
        public string CollectionShard { get; set; }
        

        /// <summary>
        /// Gets/Sets the partition key for this collection
        /// </summary>
        [JsonProperty(PropertyName = "PartitionKey")]
        public PartitionKey ParitionKey { get; set; }

        /// <summary>
        /// Gets the statistics about this collection
        /// </summary>
        [JsonProperty(PropertyName = "Statistics")]
        public CollectionStatistics Statistics { get; set; }

        //[JsonConverter(typeof(DistributionJsonConverter))]
        //[JsonProperty(PropertyName = "DataDistribution")]
        [JsonIgnore] // it was duplicate
        public IDistribution DataDistribution
        {
            get { return _distributionStrategy != null ? _distributionStrategy.GetCurrentBucketDistribution() : null; }

        }

        public void SetDistributionStrategy(DistributionStrategyConfiguration configuration, IDistributionStrategy strategy)
        {
            _distributionStrategy = strategy;
            // DataDistribution = _distributionStrategy.GetCurrentBucketDistribution();            
        }
        //[JsonConverter(typeof(DistributionStrategyJsonConverter))]
        //[JsonProperty(PropertyName = "DistributionStrategy")]
        [JsonIgnore]
        public IDistributionStrategy DistributionStrategy
        {
            get
            {
                if (_distributionStrategy != null)
                    return _distributionStrategy;
                
                return null;
            }
            set { _distributionStrategy = value; }

        }
        
        public void RemoveDistributionStrategy()
        {
            _distributionStrategy = null;
            //DataDistribution = null;
        }
        

        #region ICloneable Member
        public object Clone()
        {
            CollectionInfo collectionInfo = new CollectionInfo();
            collectionInfo.Name = Name;
            collectionInfo.ParitionKey = ParitionKey;
            collectionInfo.Statistics = Statistics;
            //todo: what type of clone is thiss below
            //collectionInfo.DataDistribution = DataDistribution;
            collectionInfo._distributionStrategy = DistributionStrategy != null? DataDistribution.Clone() as IDistributionStrategy: null ;
            collectionInfo.CollectionShard = CollectionShard;
            collectionInfo.UID = UID;
            return collectionInfo;
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            CappedInfo = reader.ReadObject() as CappedInfo;
            ParitionKey = reader.ReadObject() as PartitionKey;
            Statistics = reader.ReadObject() as CollectionStatistics;
            _distributionStrategy = reader.ReadObject() as IDistributionStrategy;
            //DataDistribution = reader.ReadObject() as IDistribution;
            CollectionShard = reader.ReadObject() as string;
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.WriteObject(_cappedInfo);
            writer.WriteObject(ParitionKey);
            writer.WriteObject(Statistics);
            writer.WriteObject(_distributionStrategy);
            //writer.WriteObject(DataDistribution);
            writer.WriteObject(CollectionShard);
            writer.WriteObject(UID);
        } 
        #endregion
    }
}
