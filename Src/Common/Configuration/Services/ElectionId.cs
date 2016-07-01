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

namespace Alachisoft.NosDB.Common.Configuration.Services
{

    public class ElectionId : ICloneable,  ICompactSerializable
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public ServerNode RequestingNode { get; set; }
        public DateTime ElectionTime { get; set; }
        public TimeSpan AllowedDuration { get; set; }
        public TimeSpan TimeTaken { get; set; }

        #region ICloneable Member
        public object Clone()
        {
            ElectionId electionId = new ElectionId();
            electionId.Id = Id;
            electionId.UID = UID;
            electionId.RequestingNode = RequestingNode;
            electionId.ElectionTime = ElectionTime;
            electionId.AllowedDuration = AllowedDuration;
            electionId.TimeTaken = TimeTaken;

            return electionId;
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Id = reader.ReadInt64();
            UID = reader.ReadObject() as string;
            RequestingNode = reader.ReadObject() as ServerNode;
            ElectionTime = reader.ReadDateTime();
            AllowedDuration = (TimeSpan)reader.ReadObject();
            TimeTaken = (TimeSpan)reader.ReadObject();

        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(Id);
            writer.WriteObject(UID);
            writer.WriteObject(RequestingNode);
            writer.Write(ElectionTime);
            writer.WriteObject(AllowedDuration);
            writer.WriteObject(TimeTaken);
        } 
        #endregion
    }
}
