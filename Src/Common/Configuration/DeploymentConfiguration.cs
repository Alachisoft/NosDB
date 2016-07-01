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

using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration
{

    public class DeploymentConfiguration : ICloneable, ICompactSerializable
    {
        Dictionary<string, ShardConfiguration> _shardConfs = new Dictionary<string, ShardConfiguration>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, string> _deploymentName = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private List<String> _deploymentId=new List<string>();
        private int _heartBeatInterval = 10;

        [ConfigurationAttribute("heart-beat-interval", "sec")]
        [JsonProperty(PropertyName = "HeartbeatInterval")]
        public int HeartbeatInterval
        {
            get { return _heartBeatInterval; }
            set { _heartBeatInterval = value == 0 ? 10 : value; }
        }

        [ConfigurationSection("shard")]
        [JsonProperty(PropertyName = "Shards")]
        public Dictionary<string, ShardConfiguration> Shards
        {
            get { return _shardConfs; }
            set { _shardConfs = value; }
        }

        [ConfigurationSection("replication")]
        [JsonProperty(PropertyName = "Replication")]
        public ReplicationConfiguration Replication
        {
            set;
            get;
        }


        [JsonProperty(PropertyName = "DeploymentIds")]
        public List<string> DeploymentList
        {
            get { return _deploymentId; }
            set { _deploymentId = value; }
        }
        
        [JsonProperty(PropertyName = "DeploymentName")]
        public Dictionary<string, string> DeploymentName
        {
            get { return _deploymentName; }
            set { _deploymentName = value; }
        }

        public void AddShard(ShardConfiguration shardConf)
        {
            lock (_shardConfs)
            {
                _shardConfs.Add(shardConf.Name, shardConf);
            }
        }

        public void AddShard(string name, ShardConfiguration shardConf)
        {
            lock (_shardConfs)
            {
                _shardConfs.Add(name, shardConf);
            }
        }

        public void RemoveShard(string name)
        {
            lock (_shardConfs)
            {
                _shardConfs.Remove(name);
            }
        }
        
        public void AddDeploymentId(string name)
        {
            lock (_deploymentId)
            {
                if(!_deploymentId.Contains(name))
                _deploymentId.Add(name);
            }
        }

        public bool ContainsDeploymentId(string depId)
        {
            lock (_deploymentId)
            {
                return _deploymentId.Contains(depId);
            }
        }
        
        public bool ContainsShard(string name)
        {
            return _shardConfs.ContainsKey(name);
        }

        public ShardConfiguration GetShard(string name)
        {
            lock (_shardConfs)
            {
                if (_shardConfs.ContainsKey(name))
                    return _shardConfs[name];
                return null;
            }
        }

        public void AddDeployment(string deploymentId, string deploymentName)
        {
            lock (_deploymentName)
            {
                if (!_deploymentName.ContainsKey(deploymentId))
                    _deploymentName.Add(deploymentId, deploymentName);
            }
        }


        #region ICloneable Member
        public object Clone()
        {
            var tConfiguration = new DeploymentConfiguration
            {
                Shards = Shards != null ? Shards.Clone() : null,
                DeploymentList = DeploymentList != null ? DeploymentList.Clone() : null,
                DeploymentName = DeploymentName != null ? DeploymentName.Clone() : null
            };
            return tConfiguration;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            Shards = SerializationUtility.DeserializeDictionary<string, ShardConfiguration>(reader);
            DeploymentName = SerializationUtility.DeserializeDictionary<string, string>(reader);
            DeploymentList = SerializationUtility.DeserializeList<string>(reader);
            HeartbeatInterval = reader.ReadInt32();
         
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeDictionary<string, ShardConfiguration>(_shardConfs, writer);
            SerializationUtility.SerializeDictionary<string, string>(DeploymentName, writer);
            SerializationUtility.SerializeList(DeploymentList, writer);
            writer.Write(HeartbeatInterval);
           
        }
        #endregion

        public ShardConfiguration GetShardConfiguration(string shardName)
        {
            //get the static nodes that a server has.
            if (!_shardConfs.ContainsKey(shardName))
                throw new Exception("The shard name doesn't exist.");
            return _shardConfs[shardName];
        }
    }
}
