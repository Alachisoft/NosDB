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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Core.Configuration
{
    public class DbmConfigServer : ICloneable, ICompactSerializable, IEquatable<DbmConfigServer>
    {
        private string _name;
        
        [ConfigurationAttribute("name")]
        public string Name
        {
            get { return  _name.ToLower(); }
            set { _name = value; }
        }

        [ConfigurationAttribute("port")]
        public int Port
        {
            get;
            set;
        }


        #region ICloneable Member
        public object Clone()
        {
            DbmConfigServer csNode = new DbmConfigServer();
            csNode.Name = Name;
            csNode.Port = Port;

            return csNode;
        }
        #endregion

        #region ICompactSerializable Members
        
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            Port = reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.Write(Port);
        }
        
        #endregion

        public bool Equals(DbmConfigServer csNode)
        {
            return (this.Name == csNode.Name && this.Port == csNode.Port);
        }
    }
}