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
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.Services
{
   
    public class Membership : ICloneable,  ICompactSerializable 
    {
        private List<ServerNode> _servers = new List<ServerNode>();

        public ElectionId ElectionId { get; set; }


        public ServerNode Primary { get; set; }
        public string Shard { get; set; }
        public string Cluster { get; set; }
        [JsonProperty(PropertyName = "_key")]
        public string JsonDocumetId
        {
            get 
            {
                string documentKey = null;

                if (Cluster != null)
                { 
                    documentKey = Cluster.ToLower();

                    if(Shard != null)
                        documentKey += ":" +Shard.ToLower();
                }

                return documentKey; 
            }
            set { }
        }
        public List<ServerNode> Servers { get { return _servers; } set { _servers = value; } }

        public void AddServer(ServerNode server)
        {
            lock(this)
            {
                if (!_servers.Contains(server))
                    _servers.Add(server);
            }
        }

        public void RemoveServer(ServerNode server)
        {
            lock (this)
            {
                if (_servers.Contains(server))
                    _servers.Remove(server);
            }
        }


        #region ICloneable Member
        public object Clone()
        {
            Membership memberShip = new Membership();
            memberShip.ElectionId = ElectionId;

            memberShip.Primary = Primary;
            memberShip.Shard = Shard;
            memberShip.Cluster = Cluster;
            memberShip.Servers = Servers == null ? null : Servers;

            return memberShip;
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            ElectionId = reader.ReadObject() as ElectionId;

            Primary = reader.ReadObject() as ServerNode;
            Shard = reader.ReadObject() as string;
            Cluster = reader.ReadObject() as string;
            Servers = Common.Util.SerializationUtility.DeserializeList<ServerNode>(reader);
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(ElectionId);

            writer.WriteObject(Primary);
            writer.WriteObject(Shard);
            writer.WriteObject(Cluster);
            Common.Util.SerializationUtility.SerializeList<ServerNode>(Servers, writer);

        } 
        #endregion
    }
}
