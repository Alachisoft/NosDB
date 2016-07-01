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
using System.Collections.Generic;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.Services
{

    public class ClusterInfo : ICloneable, ICompactSerializable,IObjectId
    {
        private Dictionary<string, ShardInfo> _shards = new Dictionary<string, ShardInfo>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, DatabaseInfo> _databases = new Dictionary<string, DatabaseInfo>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets/Sets the cluster name
        /// </summary>
        /// 
        string _name = "";

        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name!=null? _name.ToLower():null; } set { _name = value; } }

        [JsonProperty(PropertyName = "_key")]
        public string JsonDocumetId
        {
            get { return Name; }
            set { Name = value; }
        }

        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets detail shard information  
        /// </summary>
        [JsonProperty(PropertyName = "ShardInfo")]
        public Dictionary<string, ShardInfo> ShardInfo 
        {
            get { return _shards; }
            set { _shards = value; }
         }

        /// <summary>
        /// Gets the meta-data info about configured databases
        /// </summary>
        [JsonProperty(PropertyName = "Databases")]
        public Dictionary<string, DatabaseInfo> Databases
        {
            get { return _databases; }
            set { _databases = value; }
        }

        public void AddShard(ShardInfo shard)
        {
            lock (ShardInfo)
            {
                ShardInfo.Add(shard.Name, shard);
            }
        }

        public void AddShard(string name, ShardInfo server)
        {
            lock (ShardInfo)
            {
                ShardInfo.Add(name, server);
            }
        }

        public void RemoveShard(string name)
        {
            lock (ShardInfo)
            {
                ShardInfo.Remove(name);
            }
        }

        public bool ContainsShard(string name)
        {
            return ShardInfo.ContainsKey(name);
        }

        public ShardInfo GetShard(string name)
        {
            lock (ShardInfo)
            {
                if (ShardInfo.ContainsKey(name))
                    return ShardInfo[name];
                return null;
            }
        }

        public void AddDatabase(DatabaseInfo database)
        {
            lock (Databases)
            {
                Databases.Add(database.Name, database);
            }
        }
        
        public void AddDatabase(string name, DatabaseInfo database)
        {
            lock (Databases)
            {
                Databases.Add(name, database);
            }
        }

        /// <summary>
        /// Replaces current DatabaseInfo with the one provided if database exists else adds a new database
        /// </summary>
        /// <param name="database"></param>
        public void UpdateDatabase(DatabaseInfo database)
        {
            lock (Databases)
            {
                if (ContainsDatabase(database.Name))
                {
                    Databases[database.Name]= database;
                }
                else
                {
                    AddDatabase(database);
                }
            }
        }

        public void RemoveDatabase(string name)
        {
            lock (Databases)
            {
                Databases.Remove(name);
            }
        }

        public bool ContainsDatabase(string name)
        {
            return Databases.ContainsKey(name);
        }

        public DatabaseInfo GetDatabase(string name)
        {
            lock (Databases)
            {
                if (Databases.ContainsKey(name))
                    return Databases[name];
                return null;
            }
        }

        public bool IsShardUnderRemoval(string shard)
        {
            ShardInfo shardInfo = GetShard(shard);

            if (shardInfo != null)
                return shardInfo.GracefullRemovalInProcess;

            return false;
        }

        public void MarkShardForGracefullRemoval(string shard)
        {
            ShardInfo shardInfo = GetShard(shard);

            if (shardInfo != null)
                shardInfo.GracefullRemovalInProcess = true;
        }

        internal IDictionary<string, long> GetAmountOfDataOnEachShard()
        {
            IDictionary<string, long> amountOfDataOnEachShard = new Dictionary<string, long>();
            foreach (DatabaseInfo databaseInfo in Databases.Values)
            {
                foreach (CollectionInfo collection in databaseInfo.Collections.Values)
                {
                    IDictionary<string, long> sizeOfCollectionOnShards = collection.DistributionStrategy.GetAmountOfCollectionDataOnEachShard();

                    foreach (var sizeOfCollectionOnShard in sizeOfCollectionOnShards)
                    {
                        if (amountOfDataOnEachShard.ContainsKey(sizeOfCollectionOnShard.Key))
                            amountOfDataOnEachShard[sizeOfCollectionOnShard.Key] += sizeOfCollectionOnShard.Value;
                        else
                            amountOfDataOnEachShard.Add(sizeOfCollectionOnShard.Key, sizeOfCollectionOnShard.Value);
                    }
                }
            }
            return amountOfDataOnEachShard;
        }

        public string GetShardWithLowestAmountOfData()
        {
            KeyValuePair<string, long> minimumShardSize = new KeyValuePair<string, long>("", long.MaxValue);
            foreach (var shardSize in GetAmountOfDataOnEachShard())
            {
                if (shardSize.Value < minimumShardSize.Value)
                    minimumShardSize = new KeyValuePair<string, long>(shardSize.Key, shardSize.Value);
            }
            if (string.IsNullOrEmpty(minimumShardSize.Key)) throw new Exception("Cannot tell the shard with minimum amount of data");
            return minimumShardSize.Key;
        }

        public PartitionKey GetPartitonKey(string databaseName, string collectionName)
        {
            // Can I receive null or empty string as collectionName or databaseName?
            foreach (DatabaseInfo databaseInfo in Databases.Values)
            {
                if (databaseInfo.Name.Equals(databaseName))
                    return databaseInfo.GetPartitionKey(collectionName);
            }
            return null;    // To be decided later if to throw exception here or return null
        }

        public ShardInfo GetShardInfo(string shardName)
        {
            foreach (ShardInfo shard in ShardInfo.Values)
            {
                if (shard.Name.Equals(shardName))
                {
                    if (shard.IsReadOnly)
                    {
                        throw new Exception("Primary Shard is unavailable or under selection at the moment. ReadOnly Mode is activated currently");
                    }
                    return shard;
                }
            }
            return null;
        }

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            _shards = SerializationUtility.DeserializeDictionary<string, ShardInfo>(reader);
            _databases = SerializationUtility.DeserializeDictionary<string, DatabaseInfo>(reader);
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            SerializationUtility.SerializeDictionary<string, ShardInfo>(ShardInfo, writer);
            SerializationUtility.SerializeDictionary<string, DatabaseInfo>(Databases, writer);
            writer.WriteObject(UID);
        }

        #endregion

        #region ICloneable Member
        public object Clone()
        {
            ClusterInfo clusterInfo = new ClusterInfo();
            clusterInfo.Name = Name;
            clusterInfo._shards = ShardInfo != null ? ShardInfo.Clone<string,ShardInfo>(): null;
            clusterInfo._databases = Databases != null ? Databases.Clone<string,DatabaseInfo>(): null;
            clusterInfo.UID = UID;

            return clusterInfo;
        }
        #endregion

        public IList<string> GetShardsUnderGracefullRemoval()
        {
            List<string> removeableShards = new List<string>();
            if(ShardInfo !=null)
            {
                foreach(string shard in ShardInfo.Keys)
                {
                    if (IsShardUnderRemoval(shard))
                        removeableShards.Add(shard);

                }
            }
            return removeableShards;
        }
    }
}
