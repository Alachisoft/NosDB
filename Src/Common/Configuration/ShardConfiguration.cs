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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Core.Configuration.Services;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration
{

    public class ShardConfiguration : ICloneable, ICompactSerializable, IEquatable<ShardConfiguration>,IObjectId
    {
        string _name = "";
        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name.ToLower(); } set { _name = value; } }

        [ConfigurationAttribute("UID")]
        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationAttribute("port")]
        [JsonProperty(PropertyName = "Port")]
        public int Port { get; set; }

        [ConfigurationSection("servers")]
        [JsonProperty(PropertyName = "Servers")]
        public ServerNodes Servers { get; set; }

        [JsonProperty(PropertyName = "status")]
        public NodeRole Status { get; set; }

        [ConfigurationAttribute("node-heart-beat-interval", " sec")]
        [JsonProperty(PropertyName = "NodeHeartBeatInterval")]
        public int NodeHeartbeatInterval
        {
            get;
            set;
        }


        #region ICloneable Member
        public object Clone()
        {
            ShardConfiguration sConfiguration = new ShardConfiguration();
            sConfiguration.Name = Name;
            sConfiguration.Port = Port;
            sConfiguration.Servers = Servers != null ? (ServerNodes)Servers.Clone() : null;
            sConfiguration.Status = Status;
            sConfiguration.NodeHeartbeatInterval = NodeHeartbeatInterval;
            sConfiguration.UID = UID;
            
            

            return sConfiguration;
        } 
        #endregion
        
        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            Port = reader.ReadInt32();
            Servers = reader.ReadObject() as ServerNodes;
            Status = (NodeRole)reader.ReadInt32();
            NodeHeartbeatInterval = reader.ReadInt32();
            UID = reader.ReadObject() as string;
            
         
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.Write(Port);
            writer.WriteObject(Servers);
            writer.Write((int)Status);
            writer.Write(NodeHeartbeatInterval);
            writer.WriteObject(UID);

        } 
        #endregion

        public bool Equals(ShardConfiguration other)
        {
            return Name.Equals(other.Name) ? true : false;                
        }
    }
}
