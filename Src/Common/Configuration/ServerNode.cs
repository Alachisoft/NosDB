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
using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration
{
    public class ServerNode : ICloneable, ICompactSerializable, IEquatable<ServerNode>,IObjectId
    {

        [ConfigurationAttribute("ip")]
        [JsonProperty(PropertyName = "ip")]
        public string Name
        {
            get;
            set;
        }

        [ConfigurationAttribute("UID")]
        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationAttribute("priority")]
        [JsonProperty(PropertyName = "Priority")]
        public int Priority
        {
            get;
            set;
        }


        #region ICloneable Member
        public object Clone()
        {
            ServerNode sNode = new ServerNode();
            sNode.Name = Name;
            sNode.Priority = Priority;
            sNode.UID = UID;

            return sNode;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            Priority = reader.ReadInt32();
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.Write(Priority);
            writer.WriteObject(UID);
        }
        #endregion

        public bool Equals(ServerNode node)
        {
            if (node == null)
                return false;
            return this.Name == node.Name;
        }
    }
}