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
using System.Text;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer
{
    public class NodeIdentity : ICompactSerializable
    {
        private String shardName;
        //private Address address;

        public NodeIdentity(String shardName/*, Address addr*/)
        {
            this.shardName = shardName;
           // this.address = addr;
        }

        public String ShardName { get { return shardName; } }
       // public Address Address{get{return address;}}
      

        public override bool Equals(object obj)
        {
            if (obj == null ||  ! (obj is NodeIdentity))
                return false;

            NodeIdentity info=((NodeIdentity)obj);


            return shardName.Equals(info.shardName);

            //return  (address.Equals(info.address) && shardName.Equals(info.shardName))? true : false;

        }

        public override int GetHashCode()
        {
            return /*address.GetHashCode() +*/ shardName.GetHashCode();
        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append("[");
            str.Append(String.IsNullOrEmpty(shardName) ? "Null" : shardName);
            str.Append(":");
            //str.Append(address == null ? "Null" : address.ToString());
            str.Append("]");

            return str.ToString();
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            this.shardName = reader.ReadString();
            //this.address = (Address) reader.ReadObject();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(this.shardName);
           // writer.WriteObject(address);
        }
    }
}
