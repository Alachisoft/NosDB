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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
   
    public class ElectionResult : ICloneable, ICompactSerializable
    {
        private ElectionId _electionId = null;
        private Result _pollingresult;
        private List<ServerNode> _voters = new List<ServerNode>();
        private ServerNode _primary;

        public ElectionId ElectionId
        { 
            get { return _electionId; }
            set { _electionId = value; } 
        }
        public Result PollingResult 
        {
            get { return _pollingresult; }
            set { _pollingresult = value; }
        }
        public ServerNode ElectedPrimary
        {
            get { return _primary; }
            set { _primary = value; }
        }

        public List<ServerNode> Voters
        {
            get { return _voters; }
            set { _voters = value; }
        }
        public enum Result
        { 
            PrimarySelected,
            NoChangeInMembership,
        }

        #region ICloneable Members
        public object Clone()
        {
            ElectionResult result = new ElectionResult();
            result.ElectionId = ElectionId;
            result.PollingResult = PollingResult;
            result.Voters = Voters;

            return result;
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            ElectionId = reader.ReadObject() as ElectionId;
            PollingResult = (Result)reader.ReadInt32();
            Voters = Common.Util.SerializationUtility.DeserializeList<ServerNode>(reader);// reader.ReadObject() as IList<ServerNode>;
            ElectedPrimary = reader.ReadObject() as ServerNode;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(ElectionId);
            writer.Write((int)PollingResult);
            Common.Util.SerializationUtility.SerializeList<ServerNode>(Voters, writer);
            writer.WriteObject(ElectedPrimary);
        } 
        #endregion
    }

   
}
