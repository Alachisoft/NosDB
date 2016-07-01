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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Serialization;
﻿using Alachisoft.NosDB.Common.Util;
﻿using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Replication;

namespace Alachisoft.NosDB.Common.Configuration.Services
{

    public class ShardInfo: ICloneable, ICompactSerializable,IEquatable<ShardInfo>,IObjectId
    {
        Dictionary<Address, ServerInfo> _runningNodes = new Dictionary<Address, ServerInfo>();

        /// <summary>
        /// Gets the shard name
        /// </summary>
        /// 
        string _name = "";
        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name.ToLower(); } set { _name = value; } }

        [JsonProperty(PropertyName = "Port")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "IsReadOnly")]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets the primary server information
        /// </summary>
        [JsonProperty(PropertyName = "Primary")]
        public ServerInfo Primary { get; set; }

        [JsonProperty(PropertyName = "gracefull-removal-in-process")]
        public bool GracefullRemovalInProcess
        {
            get;
            set;
        }

        /// <summary>
        /// Last operation performed on the primary of the shard
        /// </summary>
        [JsonProperty(PropertyName = "LastOperationId")]
        public OperationId LastOperationId { get; set; }

        /// <summary>
        /// Gets the secondary servers
        /// </summary>
        //[JsonProperty(PropertyName = "Secondaries")]
        public ServerInfo[] Secondaries { 
            get
            {
                List<ServerInfo> secondaries = new List<ServerInfo>();
                if(RunningNodes != null )
                {
                    foreach(ServerInfo sInfo in RunningNodes.Values)
                    {
                        if (!sInfo.Equals(Primary) && !secondaries.Contains(sInfo))
                            secondaries.Add(sInfo);
                    }
                }
                return secondaries.ToArray();
            } 
            //set; 
        }

        [JsonProperty(PropertyName = "RunningNodes")]
        public Dictionary<Address, ServerInfo> RunningNodes
        {
            get { return _runningNodes; }
            set { _runningNodes = value; }
        }
        [JsonProperty(PropertyName = "ConfigureNodes")]
        public Dictionary<Address, ServerInfo> ConfigureNodes { get; set; }

        public void AddRunningNode(ServerInfo server)
        {
            lock (RunningNodes)
            {
                RunningNodes.Add(server.Address, server);
            }
        }

        public void AddRunningNode(Address address, ServerInfo server)
        {
            lock (RunningNodes)
            {
                if(!RunningNodes.ContainsKey(address))
                    RunningNodes.Add(address, server);
            }
        }



        public void RemoveRunningNode(Address address)
        {
            lock (RunningNodes)
            {
                if(RunningNodes.ContainsKey(address))
                    RunningNodes.Remove(address);
            }
        }

        public bool ContainsRunningNode(Address address)
        {
            return RunningNodes.ContainsKey(address);
        }

        public ServerInfo GetRunningNode(Address address)
        {
            lock (RunningNodes)
            {
                if (RunningNodes.ContainsKey(address))
                    return RunningNodes[address];
                return null;
            }
        }

        public void AddConfigureNode(ServerInfo server)
        {
            lock (ConfigureNodes)
            {
                ConfigureNodes.Add(server.Address, server);
            }
        }

        public void AddConfigureNode(Address address, ServerInfo server)
        {
            lock (ConfigureNodes)
            {
                ConfigureNodes.Add(address, server);
            }
        }

        public void RemoveConfigureNode(Address address)
        {
            lock (ConfigureNodes)
            {
                if (ConfigureNodes.ContainsKey(address))
                    ConfigureNodes.Remove(address);
            }
        }

        public bool ContainsConfiguredNode(Address address)
        {
            return ConfigureNodes.ContainsKey(address);
        }

        public ServerInfo GetConfiguredNode(Address address)
        {
            lock (ConfigureNodes)
            {
                if (ConfigureNodes.ContainsKey(address))
                    return ConfigureNodes[address];
                return null;
            }
        }


        #region ICloneable Member
        public object Clone()
        {
            ShardInfo shardInfo = new ShardInfo();
            shardInfo.Name = Name;
            shardInfo.Port = Port;
            shardInfo.IsReadOnly = IsReadOnly;
            shardInfo.Primary = Primary;
            shardInfo.UID = UID;
            GracefullRemovalInProcess = GracefullRemovalInProcess;
            shardInfo.LastOperationId = LastOperationId;
            //shardInfo.Secondaries = Secondaries;

            return shardInfo;
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            Port = reader.ReadInt32();
            IsReadOnly = reader.ReadBoolean();
            Primary = reader.ReadObject() as ServerInfo;
            ConfigureNodes = SerializationUtility.DeserializeDictionary<Address, ServerInfo>(reader);
            RunningNodes = SerializationUtility.DeserializeDictionary<Address, ServerInfo>(reader);
            UID = reader.ReadObject() as string;
            GracefullRemovalInProcess = reader.ReadBoolean();
            LastOperationId = reader.ReadObject() as OperationId;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.Write(Port);
            writer.Write(IsReadOnly);
            writer.WriteObject(Primary);
            SerializationUtility.SerializeDictionary<Address, ServerInfo>(ConfigureNodes, writer);
            SerializationUtility.SerializeDictionary<Address, ServerInfo>(RunningNodes, writer);
            writer.WriteObject(UID);
            writer.Write(GracefullRemovalInProcess);
            writer.WriteObject(LastOperationId);
        } 
        #endregion

        public ServerInfo GetServerInfo(Address address)
        {
            if (ConfigureNodes != null && ConfigureNodes.Count > 0)
            {
                if (ConfigureNodes.ContainsKey(address))
                    return ConfigureNodes[address];
            }
            return null;
        }

        public bool Equals(ShardInfo other)
        {
            return Name.Equals(other.Name) ? true : false;
        }
    }
}
