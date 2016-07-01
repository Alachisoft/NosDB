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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElectionId = Alachisoft.NosDB.Common.Configuration.Services.ElectionId;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement
{
    public class Election : ICloneable, ICompactSerializable
    {
        private Address[] _voters = null;
        private IDictionary<ServerNode, ElectionVote> _electionVotes = null;
        private ServerInfo _requestingServerInfo = null;


        public Election(ElectionId id, ElectionType electionType)
        {
            ElectionId = id;
            ElectionType = electionType;
        }

        public ElectionId ElectionId { get; private set; }
        public ElectionType ElectionType { get; private set; }
        public bool IsVotingCompleted
        {
            get
            {
                if (_voters != null && _electionVotes != null && (_electionVotes.Count().Equals(_voters.Count())))
                    return true;
                return false;
            }
        }

        public ServerInfo RequestingServerInfo
        {
            get { return _requestingServerInfo; }
            set { _requestingServerInfo = value; }
        }

        public bool StartElection(Address[] voters)
        {

            if (voters.Length.Equals(0))
                return false;

            if (_voters == null)
                _voters = new Address[voters.Count()];
            Array.Copy(voters, _voters, voters.Count());
            return true;

        }

        public void AddVote(ElectionVote vote)
        {
            if (_electionVotes == null)
                _electionVotes = new Dictionary<ServerNode, ElectionVote>();
            if (vote != null && !_electionVotes.ContainsKey(vote.Sourcenode))
            {
                _electionVotes.Add(vote.Sourcenode, vote);
            }
        }

        public ElectionResult GetElectionResult()
        {

            int votingCount = 0;
            ElectionResult electionResult = new ElectionResult();


            if (IsVotingCompleted)
            {
                foreach (KeyValuePair<ServerNode, ElectionVote> serverVote in _electionVotes)
                {
                    if (serverVote.Value.NodeVote.Equals(ElectionVote.Vote.yes))
                    {
                        votingCount++;
                        electionResult.Voters.Add(serverVote.Key);
                    }

                }
                electionResult.ElectionId = ElectionId;

                if (votingCount.Equals(_voters.Count()))
                {
                    electionResult.ElectedPrimary = ElectionId.RequestingNode;
                    electionResult.PollingResult = ElectionResult.Result.PrimarySelected;
                }
                else
                {
                    electionResult.ElectedPrimary = null;
                    electionResult.PollingResult = ElectionResult.Result.NoChangeInMembership;
                }
            }

            else
            {
                electionResult.ElectionId = ElectionId;
                electionResult.ElectedPrimary = null;
                electionResult.PollingResult = ElectionResult.Result.NoChangeInMembership;
            }
            return electionResult;
        }

        #region ICloneable Member
        public object Clone()
        {
            Election election = new Election(ElectionId, ElectionType);
            election.ElectionId = ElectionId;
            election.RequestingServerInfo = RequestingServerInfo;
            election.ElectionType = ElectionType;

            return election;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            RequestingServerInfo = reader.ReadObject() as ServerInfo;
            ElectionId = reader.ReadObject() as ElectionId;
            ElectionType = (ElectionType)reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(RequestingServerInfo);
            writer.WriteObject(ElectionId);
            writer.Write((int)ElectionType);
        }
        #endregion
    }
}
