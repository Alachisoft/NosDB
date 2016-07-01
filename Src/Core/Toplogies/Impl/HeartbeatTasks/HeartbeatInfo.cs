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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks
{
   // [Serializable]
    public class HeartbeatInfo: ICompactSerializable, ICloneable
    {
        public DateTime LastHeartbeatTimestamp { get; internal set; }
        public Membership CurrentMembership { get; internal set; }
        public int MissingHeartbeatsCounter { get; internal set; }
        public OperationId LastOplogOperationId { get; internal set; }
        public ConnectivityStatus CSStatus { get; set; }

        

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            LastHeartbeatTimestamp = reader.ReadDateTime();
            CurrentMembership = reader.ReadObject() as Membership;
            MissingHeartbeatsCounter = reader.ReadInt32();
            LastOplogOperationId = reader.ReadObject() as OperationId;
            CSStatus = (ConnectivityStatus)reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(LastHeartbeatTimestamp);
            writer.WriteObject(CurrentMembership);
            writer.Write(MissingHeartbeatsCounter);
            writer.WriteObject(LastOplogOperationId);
            writer.Write((int)CSStatus);
        } 
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            HeartbeatInfo nodeInfo = new HeartbeatInfo();
            nodeInfo.CurrentMembership = CurrentMembership;
            nodeInfo.LastOplogOperationId = LastOplogOperationId;
            nodeInfo.MissingHeartbeatsCounter = MissingHeartbeatsCounter;
            nodeInfo.LastOplogOperationId = LastOplogOperationId;
            nodeInfo.CSStatus = CSStatus;
            return nodeInfo;
        } 
        #endregion
    }
}
