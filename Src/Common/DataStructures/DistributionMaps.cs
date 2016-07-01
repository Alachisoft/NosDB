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
using System.Text;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;

namespace Alachisoft.NosDB.Common.DataStructures
{
    [Serializable]
    public class DistributionMaps : ICloneable, ICompactSerializable
    {
        private List<HashMapBucket> _hashmap;
        private Dictionary<string,List<HashMapBucket>> _bucketsOwnershipMap;
        private BalancingResult _result = BalancingResult.Default;

        /// <summary>
        /// Normally buckets are owned by main partition nodes (in POR) and other main partition nodes as well as replica
        /// nodes transfer those buckets from owner partition nodes. But their are exceptional cases when we inform other 
        /// nodes to transfer those buckets from replica nodes. One very obvious use case is when all the nodes left the cluster
        /// except one and this last node need to transfer data from the backup available on the same node. 
        /// This table is used to maintain the bucket ids for which we are not following the rule but exception. Against 
        /// bucket ids we store backup node address from where other nodes will transfer the bucket.
        /// </summary>
        private Hashtable _specialBucketOwners = new Hashtable();

        public DistributionMaps(List<HashMapBucket> hashmap, Dictionary<string,List<HashMapBucket>> bucketsOwnershipMap)
        {
            _hashmap = hashmap;
            _bucketsOwnershipMap = bucketsOwnershipMap;
        }

        public DistributionMaps(BalancingResult balResult)
        {
            _hashmap = null;
            _bucketsOwnershipMap = null;
            _result = balResult;
        }

        public List<HashMapBucket> Hashmap
        {
            get { return _hashmap; }
            set { _hashmap = value; }
        }

        public Dictionary<string,List<HashMapBucket>> BucketsOwnershipMap
        {
            get { return _bucketsOwnershipMap; }
            set { _bucketsOwnershipMap = value; }
        }

        public Hashtable SpecialBucketOwners
        {
            get { return _specialBucketOwners; }
            set { _specialBucketOwners = value; }
        }

        public BalancingResult BalancingResult
        {
            get { return _result; }
            set { _result = value; }
        }

        public override string ToString()
        {
            IDictionaryEnumerator idict = _bucketsOwnershipMap.GetEnumerator();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _bucketsOwnershipMap.Count; i++)
            {
                idict.MoveNext();
                sb.Append("Key: " + idict.Key.ToString() + "Bucket Count: " + (idict.Value as ArrayList).Count + "\n");
                ArrayList values = idict.Value as ArrayList;
                //foreach (HashMapBucket hs in values)
                //    sb.Append(hs.ToString() + "\n");
            }
            return sb.ToString();
        }

        #region ICloneable Members

        public object Clone()
        {
            DistributionMaps maps = new DistributionMaps(_result);
            if (_hashmap != null) maps.Hashmap = new List<HashMapBucket>(_hashmap);
            if (_bucketsOwnershipMap != null) maps.BucketsOwnershipMap = new Dictionary<string, List<HashMapBucket>>(_bucketsOwnershipMap);
            if (_specialBucketOwners != null) maps.SpecialBucketOwners = _specialBucketOwners.Clone() as Hashtable;

            return maps;
        }

        #endregion

        #region ICompactSerializable Members

        public void Deserialize(CompactReader reader)
        {
            _result = (BalancingResult)reader.ReadObject();
            _hashmap = (List<HashMapBucket>)reader.ReadObject();
            _bucketsOwnershipMap = (Dictionary<string,List<HashMapBucket>>)reader.ReadObject();
            _specialBucketOwners = (Hashtable)reader.ReadObject();
        }

        public void Serialize(CompactWriter writer)
        {
            writer.WriteObject(_result);
            writer.WriteObject(_hashmap);
            writer.WriteObject(_bucketsOwnershipMap);
            writer.WriteObject(_specialBucketOwners);
        }

        #endregion
    }
}