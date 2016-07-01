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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.Services
{

    public class ServerInfo : ICloneable, ICompactSerializable, IEquatable<ServerInfo>,IObjectId
    {
        /// <summary>
        /// Gets/sets the address of the serevr
        /// </summary>
        [JsonProperty(PropertyName = "Address")]
        public Address Address { get; set; }

        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the timestamp for lastest replicated operation
        /// </summary>
        [JsonProperty(PropertyName = "LastOperationId")]
        public OperationId LastOperationId { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public Status Status
        {
            get;
            set;
        }
         
        #region ICloneable Member

        public object Clone()
        {
            ServerInfo serverInfo = new ServerInfo();
            serverInfo.Address = Address;
            serverInfo.LastOperationId = LastOperationId;
            serverInfo.Status = Status;
            serverInfo.UID = UID;
            return serverInfo;
        } 
        #endregion

        #region ICompactSerializable Member
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Address = reader.ReadObject() as Address;
            LastOperationId = reader.ReadObject() as OperationId;
            Status = (Status)reader.ReadInt32();
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Address);
            writer.WriteObject(LastOperationId);
            writer.Write((int)Status);
            writer.WriteObject(UID);
        } 
        #endregion

        public bool Equals(ServerInfo other)
        {
            if (other == null)
                return false;
            return this.Address.Equals(other.Address);
        }

        public Toplogies.Impl.ShardImpl.Server ToServer()
        {
            return new Toplogies.Impl.ShardImpl.Server(Address, Status);
        }
    }
}
