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
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement
{
   // [Serializable]
    public class ElectionVote : ICloneable, ICompactSerializable
    {
        private ServerNode _requestingNode;
        private ServerNode _sourceNode;
        private Vote _vote;
        public enum Vote
        {
            yes = 1,
            no = 2,
        }
        public ServerNode RequestingNode
        {
            get { return _requestingNode; }
            set { _requestingNode = value; }
        }
        public ServerNode Sourcenode
        {
            get { return _sourceNode; }
            set { _sourceNode = value; }
        }
        public Vote NodeVote
        {
            get { return _vote; }
            set { _vote = value; }
        }


        #region ICloneable Members
        public object Clone()
        {
            ElectionVote vote = new ElectionVote();
            vote.NodeVote = NodeVote;
            vote.RequestingNode = RequestingNode;
            vote.Sourcenode = Sourcenode;

            return vote;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            RequestingNode = reader.ReadObject() as ServerNode;
            Sourcenode = reader.ReadObject() as ServerNode;
            NodeVote = (Vote)reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(RequestingNode);
            writer.WriteObject(Sourcenode);
            writer.Write((int)NodeVote);
        }
        #endregion
    }
   
}
