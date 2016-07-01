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
namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy
{
    public class NonShardedDistributionRouter : IDistributionRouter
    {
        private NonShardedDistribution _parent;

        public NonShardedDistributionRouter(NonShardedDistribution parent)
        {
            _parent = parent;
        }

        #region IDistributionRouter Methods

        public Common.DataStructures.HashMapBucket GetBucketForDocument(Common.Server.Engine.DocumentKey documentKey)
        {
            return _parent.Bucket;
        }

        public int GetBucketID(Common.Server.Engine.DocumentKey documentKey)
        {
            return _parent.Bucket.BucketId;
        }

        public string GetShardForDocument(Common.Server.Engine.DocumentKey documentKey)
        {
            return _parent.Bucket.CurrentShard;
        }

        #endregion

        #region ICompactSerialization Methods

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _parent = reader.ReadObject() as NonShardedDistribution;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_parent);
        }

        #endregion
    }
}
