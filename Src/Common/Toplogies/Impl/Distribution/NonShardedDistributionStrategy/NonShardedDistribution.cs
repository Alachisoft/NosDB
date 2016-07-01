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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy
{
    public class NonShardedDistribution : IDistribution
    {
        private HashMapBucket _bucket;
        private int _distributionSequence;
        private string _name = DistributionName.NonShardedDistribution.ToString();

        public HashMapBucket Bucket 
        {
            get { return _bucket; }
            set { _bucket = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public NonShardedDistribution()
        {
            //_distributionSequence = 0;
        }

      
        public NonShardedDistribution(string shard)
        {
            _bucket = new HashMapBucket(shard,0, BucketStatus.Functional);
            _distributionSequence = 0;
        }

        public bool CanRemoveShard(string shard)
        {
            if(_bucket.CurrentShard.Equals(shard))
                return false;
            return true;
        }

        public bool InStateTransfer()
        {
            return _bucket.Status == BucketStatus.UnderStateTxfr || _bucket.Status == BucketStatus.NeedTransfer;
        }

        public string GetFinalShard()
        {
            return _bucket.FinalShard;
        }

        public void UpdateShard(string newShard)
        {
            _bucket.FinalShard = newShard;
            _bucket.Status = BucketStatus.NeedTransfer;
        }

        #region IDistribution Methods
        public int DistributionSequence
        {
            get { return _distributionSequence; }
            set { _distributionSequence = value; }
        }

        public Common.DataStructures.HashMapBucket[] GetBucketsForShard(string shard)
        {
            if(_bucket.CurrentShard.Equals(shard) || _bucket.FinalShard.Equals(shard)) 
                return new []{_bucket};
            throw new Exception("No buckets exist for Shard '"+shard+"'");
        }

        public List<String> GetShards()
        {
            return new List<string>(new[] {_bucket.CurrentShard});
        }

        public IDistributionRouter GetDistributionRouter()
        {
            return new NonShardedDistributionRouter(this);
        }
        #endregion

        #region StateTransfer Helper Methods

        internal Hashtable LockBuckets(ArrayList buckets, string requestingShard)
        {
            ArrayList lockAcquired = new ArrayList();
            ArrayList ownerChanged = new ArrayList();

            Hashtable result = new Hashtable();

            try
            {
                IEnumerator ie = buckets.GetEnumerator();
                while (ie.MoveNext())
                {
                    if (requestingShard.Equals(_bucket.FinalShard))
                    {
                        //TODO: if (NCacheLog.IsInfoEnabled) NCacheLog.Info("DistributionMgr.lockbuckets", "acquired locked on bucket [" + bucket.BucketId + "] by " + requestingShard);

                        _bucket.Status = BucketStatus.UnderStateTxfr;
                        if (!lockAcquired.Contains(ie.Current))
                            lockAcquired.Add(ie.Current);

                    }
                    else if (!ownerChanged.Contains(ie.Current))
                    {
                        //TODO: if (NCacheLog.IsInfoEnabled) NCacheLog.Info("DistributionMgr.lockbuckets", "bucket [" + bucket.BucketId + "] owner ship is changed; new owner is " + bucket.FinalShard);
                        ownerChanged.Add(ie.Current);
                    }
                }

                result[BucketLockResult.OwnerChanged] = ownerChanged;
                result[BucketLockResult.LockAcquired] = lockAcquired;

                return result;
            }
            catch (Exception e)
            {
                //TODO: NCacheLog.Error("DistributionMgr.lockbuckets", e.ToString());
                return result;
            }
        }

        public void ReleaseBuckets(ArrayList buckets, string requestingShard)
        {
            try
            {
                if (buckets != null)
                {
                    IEnumerator ie = buckets.GetEnumerator();
                    while (ie.MoveNext())
                    {
                        if (requestingShard.Equals(_bucket.FinalShard))
                        {
                            _bucket.Status = BucketStatus.Functional;
                            //Change permnant address only when node who locked the bucket 
                            //has sent request to release after he has transfered the bucket completely.
                            _bucket.CurrentShard = _bucket.FinalShard;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: NCacheLog.Error("DistributionMgr.ReleaseBuckets", e.ToString());
            }
        }
        
        #endregion

        #region ICompactSerializable Methods
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            _distributionSequence = reader.ReadInt32();
            _bucket = reader.ReadObject() as HashMapBucket;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);          
            writer.Write(_distributionSequence);
            writer.WriteObject(_bucket);
        }
        #endregion

        public void SetBucketStatus(HashMapBucket bucket, byte status)
        {
            if (_bucket.BucketId==bucket.BucketId)
            {
                _bucket.Status = status;
                _bucket.CurrentShard = bucket.FinalShard;                
            }
        }


        public Common.Toplogies.Impl.Distribution.DistributionMethod Type
        {
            get { return Common.Toplogies.Impl.Distribution.DistributionMethod.NonShardedDistribution; }
        }

        public object Clone()
        {
            NonShardedDistribution clone = new NonShardedDistribution();

            clone._bucket = _bucket != null ? _bucket.Clone() as HashMapBucket : null;
            clone._distributionSequence = _distributionSequence;
            clone._name = _name;

            return clone;
        }
    }
}
