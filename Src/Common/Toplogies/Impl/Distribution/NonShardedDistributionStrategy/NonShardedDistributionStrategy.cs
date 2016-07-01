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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Stats;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy
{

    // [JsonConverter(typeof(NonShardedDistributionStrategy))]
    public class NonShardedDistributionStrategy :IDistributionStrategy
    {
        private List<string> _shards;
        private ReaderWriterLock _syncLock;
        private NonShardedDistribution _nonShardedDistribution;
        private string _name = DistributionType.NonSharded.ToString();
        BucketStatistics _bucketsStats;

        public NonShardedDistributionStrategy()
        {
            _syncLock = new ReaderWriterLock();
            _shards = new List<string>();
        }

        public ReaderWriterLock Sync
        {
            get { return _syncLock; }
            set { _syncLock = value; }
        }

        public NonShardedDistribution NonShardedDistribution
        {
            get { return _nonShardedDistribution; }
            set { _nonShardedDistribution = value; }
        }

        public BucketStatistics BucketStatistics
        {
            get { return _bucketsStats; }
            set { _bucketsStats = value; }
        }

        public List<string> Shards
        {
            get { return _shards; }
            set { _shards = value; }
        }
        #region IDistributionStrategy Methods

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public IDistribution Distribution
        {
            get { return _nonShardedDistribution; }
        }

        public IDistributionStrategy AddShard(string shard, IDistributionConfiguration configuration,Boolean needTransfer)
        {
            if (string.IsNullOrEmpty(shard)) throw new Exception("Shard Name cannot be null or empty. Failed to configure distribution");
            _syncLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                shard = shard.ToLower();
                if (_shards.Count < 1)
                {
                    _shards.Add(shard);
                    _nonShardedDistribution = new NonShardedDistribution(shard);
                    return this;
                }

                if (_shards.Contains(shard))
                    throw new Exception("Shard '"+shard+"' already Exists");

                _shards.Add(shard);
                return this;
            }
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
        }

        public void UpdateShard(string newShard)
        {
            if (string.IsNullOrEmpty(newShard)) throw new Exception("Shard Name cannot be null or empty. Failed to configure distribution");
            _syncLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                newShard = newShard.ToLower();
                if (!_shards.Contains(newShard))
                    throw new Exception("Shard '" + newShard + "' not Exists");
                _nonShardedDistribution.UpdateShard(newShard);
            }
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
        }

        public IDistributionStrategy RemoveShard(string shard, IDistributionConfiguration configuration, bool gracefull)
        {
            if (string.IsNullOrEmpty(shard)) throw new ArgumentNullException(shard);
            _syncLock.AcquireWriterLock(Timeout.Infinite);
            shard = shard.ToLower();
            try
            {
                if (_shards.Contains(shard))
                {
                    if (_shards.Count < 2)
                    {
                        if (shard.Equals(_shards.First()))
                        {
                            throw new Exception("Shard '" + shard + "' is the only shard in the cluster. Cannot remove it");
                        }
                        else
                        {
                            return this;
                        }
                    }
                }
                else
                {
                    //In case shard has already been removed. For e.g. a gracefull -removal is followed by a forcefull removal.
                    if(_nonShardedDistribution.Bucket.CurrentShard.Equals(shard))
                    {
                        _nonShardedDistribution.Bucket.CurrentShard = _nonShardedDistribution.Bucket.FinalShard;
                        _nonShardedDistribution.Bucket.Status = BucketStatus.Functional;
                    }
                    return this;
                }

                _shards.Remove(shard);
                if (_nonShardedDistribution.Bucket.FinalShard.Equals(shard))
                {
                    _nonShardedDistribution.Bucket.FinalShard = _shards.First();
                    if (gracefull)
                    {
                        _nonShardedDistribution.Bucket.Status = BucketStatus.NeedTransfer;
                    }
                    else
                    {
                        _nonShardedDistribution.Bucket.CurrentShard = _nonShardedDistribution.Bucket.FinalShard;
                        _nonShardedDistribution.Bucket.Status = BucketStatus.Functional;
                    }
                }
                return this;
            }
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
        }

        public IDistribution BalanceShards()
        {
            throw new NotImplementedException();
        }

        public IDistribution GetCurrentBucketDistribution()
        {
            _syncLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _nonShardedDistribution;
            }
            finally
            {
                _syncLock.ReleaseReaderLock();
            }
        }

        public void UpdateBucketStats(Common.Stats.ShardInfo shardInfo)
        {
            _syncLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (shardInfo == null) return;

                if (_bucketsStats == null)
                    _bucketsStats = new BucketStatistics();

                if (shardInfo.Statistics != null && shardInfo.Statistics.LocalBuckets != null)
                {
                    var bucketStats = shardInfo.Statistics.LocalBuckets;
                    if (bucketStats != null)
                    {
                        IEnumerator<KeyValuePair<HashMapBucket, BucketStatistics>> ide = bucketStats.GetEnumerator();
                        while (ide.MoveNext())
                        {
                            if (_nonShardedDistribution.Bucket.CurrentShard.Equals(shardInfo.ShardName))
                            {
                                BucketStatistics stats = ide.Current.Value;

                                _bucketsStats = stats;
                            }
                        }
                    }
                }
            }
           /* catch (Exception e)
            {
                if (NCacheLog != null && NCacheLog.IsErrorEnabled) NCacheLog.Error("DistributionMgr.UpdateBucketStats()", e.ToString());
            }*/
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
        }

        public IDistributionConfiguration DistributionConfiguration
        {
            get { return null; }
        }

        public Dictionary<string, long> GetAmountOfCollectionDataOnEachShard()
        {
            _syncLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                List<String> shards = _nonShardedDistribution.GetShards();
                if (!shards.Any()) throw new Exception("Cannot tell the lowest shard size because no Shard exists");

                Dictionary<string, long> shardsSizes = new Dictionary<string, long>();

                foreach (string shard in shards)
                {
                    shardsSizes.Add(shard, _bucketsStats.DataSize);
                }
                return shardsSizes;
            }
            finally
            {
                _syncLock.ReleaseReaderLock();
            }
        }

       

       

       

        public bool IsSparsedBucket(int bucketid, long threshold)
        {
            throw new NotImplementedException("IsSparsedBucket not implemented in NonShardedDistributionStrategy");
        }
        #endregion

        #region ICompactSerializable Methods

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _shards = Common.Util.SerializationUtility.DeserializeList<string>(reader);
            _name = reader.ReadObject() as string;
            _nonShardedDistribution =reader.ReadObject() as NonShardedDistribution;
            _bucketsStats = reader.ReadObject() as BucketStatistics;
            _syncLock = new ReaderWriterLock();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            Common.Util.SerializationUtility.SerializeList(_shards,writer);
            writer.WriteObject(_name);
            writer.WriteObject(_nonShardedDistribution);
            writer.WriteObject(_bucketsStats);
        }

        #endregion

        


        //public override bool CanConvert(Type objectType)
        //{
        //    return typeof(NonShardedDistributionStrategy).IsAssignableFrom(objectType);
        //}

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    JObject jObject = JObject.Load(reader);
        //    this._name = (string)jObject["Name"];
        //    this._nonShardedDistribution = ((JObject)jObject["NonShardedDistribution"]).ToObject<NonShardedDistribution>();
        //    this._bucketsStats = ((JObject)jObject["BucketStats"]).ToObject<BucketStatistics>();
        //    //IEnumerator enums=  ((JObject) jObject["Shards"]).GetEnumerator();
        //    //foreach (IEnumerator<string,JToken> in enums)
        //    //{
                
        //    //}
        //    this._syncLock=new ReaderWriterLock();

        //    return this;
        //}

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    writer.WriteStartObject();
        //    writer.WritePropertyName("Name");
        //    serializer.Serialize(writer, _name);
        //    writer.WritePropertyName("NonShardedDistribution");
        //    serializer.Serialize(writer, _nonShardedDistribution);
        //    writer.WritePropertyName("BucketStats");
        //    serializer.Serialize(writer, _bucketsStats);
        //    writer.WritePropertyName("Shards");
        //    foreach (string str in _shards)
        //    {
        //        writer.WriteStartObject();
        //        serializer.Serialize(writer,str);
                
        //    }
        //    writer.WriteEndObject();
           
        //}
        //public override bool CanRead
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        //public override bool CanWrite
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        public Hashtable LockBuckets(ArrayList buckets, string requestingShard)
        {
            _syncLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (buckets != null)
                {
                    return _nonShardedDistribution.LockBuckets(buckets, requestingShard);
                }
            }
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
            return null;    // This will never be returned
        }

        public void ReleaseBuckets(ArrayList buckets, string requestingShard)
        {
            _syncLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (buckets != null)
                {
                    _nonShardedDistribution.ReleaseBuckets(buckets, requestingShard);
                }
            }
            finally
            {
                _syncLock.ReleaseWriterLock();
            }
        }
       
        public bool CanRemoveShard(string p)
        {
            //NTD:[High] Ask  if its possible to remove shard gracefully in nosharded distribtion
            _syncLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _nonShardedDistribution.CanRemoveShard(p);
            }
            finally
            {
                _syncLock.ReleaseReaderLock();
            }
        }

        public bool InStateTransfer()
        {
            _syncLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _nonShardedDistribution.InStateTransfer();
            }
            finally
            {
                _syncLock.ReleaseReaderLock();
            }
        }

        public Object Clone()
        {
            NonShardedDistributionStrategy clone = new NonShardedDistributionStrategy();
            clone._bucketsStats = _bucketsStats != null ? _bucketsStats.Clone() as BucketStatistics : null;
            clone._name = this._name;
            clone._nonShardedDistribution = _nonShardedDistribution != null ? _nonShardedDistribution.Clone() as NonShardedDistribution : null;
            clone._shards = _shards != null ? _shards.Clone<string>() : null;

            return clone;
        }
    }
}
