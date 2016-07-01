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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement
{
    public class MembershipChangeArgs : ICloneable, ICompactSerializable
    {
        public Address ServerName { get; set; }
        public ElectionId ElectionId { get; set; }
        public MembershipChangeType ChangeType { get; set; }
        public enum MembershipChangeType
        {
            PrimarySelected = 0,
            PrimaryDemoted = 1,
            PrimaryLost = 2,
            PrimarySet = 3,
            CSLost = 4,
            None = 5,
            NodeJoined =6,
            NodeLeft=7, 
            RestrictPrimary = 8,
            TimeoutOnRestrictedPrimary=9,
            ForcefullyDemotePrimary = 10,
        }

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            ServerName = reader.ReadObject() as Address;
            ElectionId = reader.ReadObject() as ElectionId;
            ChangeType = (MembershipChangeType)reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(ServerName);
            writer.WriteObject(ElectionId);
            writer.Write((int)ChangeType);
        } 
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            MembershipChangeArgs args = new MembershipChangeArgs();
            args.ServerName = ServerName;
            args.ElectionId = ElectionId;
            args.ChangeType = ChangeType;
            return args;
        } 
        #endregion
    }
    
}
