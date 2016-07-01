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
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations
{    
    //Information like Cluste Name, Shard Name, Database Name, Collection Name & Node Address would be needed
    public class StateTransferIdentity : ICompactSerializable
    {
        public String DBName { get; private set; }
        public String ColName { get; private set; }
        public StateTransferType Type { get; private set; }
        public NodeIdentity NodeInfo { get; private set; }
        public DistributionMethod DistributionType { get; private set; }

        public StateTransferIdentity(String DBName, String ColName, NodeIdentity nodeInfo, StateTransferType type, Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.DistributionMethod distributionType)
        {
            this.DBName = DBName;
            this.ColName = ColName;
            this.NodeInfo = nodeInfo;
            this.Type = type;
            this.DistributionType = distributionType;
        }
        public override int GetHashCode() 
        {
            return (DBName != null ? DBName.GetHashCode() : 0) +( ColName != null ? ColName.GetHashCode() : 0 )+ (this.NodeInfo!= null ? NodeInfo.GetHashCode() : 0);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is StateTransferIdentity))
                return false;

            StateTransferIdentity identity = ((StateTransferIdentity)obj);
            return ((identity.DBName.Equals(DBName))&&(identity.ColName.Equals(ColName)) && identity.NodeInfo.Equals(this.NodeInfo)) ? true : false;
        }

        public override string ToString()
        {               
            StringBuilder str=new StringBuilder();
            str.Append("[");
            str.Append(String.IsNullOrEmpty(DBName)?"Null":DBName);
            str.Append(":");
            str.Append(String.IsNullOrEmpty(ColName)?"Null":ColName);
            str.Append(":");
            str.Append(Type==StateTransferType.INTER_SHARD?"Move":"Copy");
            str.Append(":");
            str.Append(NodeInfo==null ? "Null":NodeInfo.ToString());
            str.Append("]");

            return str.ToString();
        }

        #region ICompactSerializable Implementation
        
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            this.Type = (StateTransferType)reader.ReadByte();
            this.DBName = reader.ReadString();
            this.ColName = reader.ReadString();
            this.NodeInfo = (NodeIdentity)reader.ReadObject();
            this.DistributionType = (DistributionMethod)reader.ReadByte();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(DBName);
            writer.Write(ColName);
            writer.WriteObject(NodeInfo);
            writer.Write((byte)DistributionType);
        }

        #endregion
        
    }
}
