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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement
{
    class ElectionManager
    {
        private IShard _shard;
        private NodeContext _context = null;
        private ClusterConfigurationManager _clusterConfigMgr = null;

        public int LocalServerPriority
        {
            get
            {
                ShardConfiguration sConfig = null;
                if (_clusterConfigMgr != null)
                {
                    sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                    if (sConfig == null || sConfig.Servers == null)
                        throw new ArgumentNullException("Shard Configuration is missing.");
                    ServerNode node = null;
                    if (_context != null && _context.LocalAddress != null)
                        node = sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString());
                    if (node != null)
                        return node.Priority;
                    else
                        return 0;
                    //return sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString()).Priority;

                }
                throw new Exception("Cluster configuration is missing.");
            }
        }


        public ServerInfo LocalServerInfo()
        {
            //RTD: this needs to be looked into. ServerInfo should not take int as an op id
            ServerInfo sInfo = new ServerInfo();
           
            sInfo.Address = _context.LocalAddress;
            return sInfo;
        }

        public ElectionManager(IShard shard, NodeContext context, ClusterConfigurationManager clusterConfigMgr)
        {
            this._shard = shard;
            this._context = context;
            this._clusterConfigMgr = clusterConfigMgr;
            //if (_context != null)
            //    _clusterConfig = _context.ConfigurationSession.GetDatabaseClusterConfiguration(_context.ClusterName);
        }

        internal ResponseCollection<object> MulticastRequest(Object data)
        {
            Message message = new Message();
            message.Payload = data;
            message.NeedsResponse = true;

            message.MessageType = MessageType.MembershipOperation;
            ShardMulticastRequest<ResponseCollection<object>, object> request = _shard.CreateMulticastRequest<ResponseCollection<object>, object>(_shard.ActiveChannelsList, message);
            IAsyncResult result = request.BeginExecute();
            return request.EndExecute(result);
        }

        internal Configuration.Services.ElectionResult ConductElection(ElectionId electionId, Address[] activeNodes, Activity activity)
        {
            Address[] votingNodes = null;
            if (activeNodes == null)
                throw new ArgumentNullException("Active nodes are null");
            Election election = null;
            if (votingNodes == null)
                votingNodes = new Address[activeNodes.Length];
            try
            {
                ElectionType electionType = ElectionType.None;
                if (activity == Activity.GeneralElectionsTriggered)
                    electionType = ElectionType.GeneralElections;
                else if (activity == Activity.TakeoverElectionsTriggered)
                    electionType = ElectionType.TakeoverElections;

                election = new Election(electionId, electionType);
                votingNodes = activeNodes;
                election.RequestingServerInfo = LocalServerInfo();

                if (election.StartElection(votingNodes))
                {
                    ResponseCollection<object> response = (ResponseCollection<object>)MulticastRequest(election);

                    if (response != null)
                    {
                        foreach (var server in _shard.ActiveChannelsList)
                        {
                            IClusterResponse<object> serverResponse = response.GetResponse(server);
                            if (serverResponse.IsSuccessfull)
                            {
                                if (serverResponse.Value != null)
                                {
                                    ElectionVote vote = serverResponse.Value as ElectionVote;
                                    if (vote != null)
                                        election.AddVote(vote);
                                }
                            }
                        }
                    }
                    return election.GetElectionResult();
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("ElectionManager.ConductElection()", e.ToString());
                }
            }
            return null;
        }

        internal int GetHighestNodePriority(IList<Address> nodes, IList<string> maxOplogAddress)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = "" });
            ShardConfiguration sConfig = null;
            if (_clusterConfigMgr != null)
                sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
            if (sConfig == null || sConfig.Servers == null)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                    LoggerManager.Instance.ShardLogger.Warn("ElectionManager.GetHeighestNodePriority() ","The shard (or the nodes of the shard) " + _context.LocalShardName + " does not exist in the configuration.");
                return 0;
            }

            ServerNodes staticNodes = sConfig.Servers;
            int maxPriority = Int32.MaxValue;
            if (nodes != null)
            {
                foreach (Address node in nodes)
                {
                    ServerNode serverNode = staticNodes.GetServerNode(node.IpAddress.ToString());
                    if (serverNode != null)
                    {
                        int thisPriority = serverNode.Priority;
                        if ((thisPriority < maxPriority) && maxOplogAddress.Contains(serverNode.Name))
                            maxPriority = thisPriority;
                    }
                }
                return maxPriority;
            }
            else
                throw new Exception("Membership is null.");
        }

        internal ElectionVote CastVote(Election election, Membership existingMembership, HeartbeatInfo info)
        {
            if (election == null || election.ElectionId == null)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                    LoggerManager.Instance.ShardLogger.Warn("ElectionManager.CastVote()", "Invalid election.");
                return null;
            }
            ElectionVote vote = new ElectionVote();
            vote.RequestingNode = election.ElectionId.RequestingNode;
            vote.Sourcenode = new ServerNode();
            vote.Sourcenode.Name = _context.LocalAddress.IpAddress.ToString();
            vote.Sourcenode.Priority = (int)LocalServerPriority;
            if (existingMembership != null && (election.ElectionType == ElectionType.GeneralElections && existingMembership.Primary == null) || election.ElectionType == ElectionType.TakeoverElections)
            {
                if (election.RequestingServerInfo.LastOperationId > LocalServerInfo().LastOperationId)
                    vote.NodeVote = ElectionVote.Vote.yes;
                else if (election.RequestingServerInfo.LastOperationId == LocalServerInfo().LastOperationId)
                {
                    if (election.ElectionId.RequestingNode.Priority <= (int)LocalServerPriority)
                        vote.NodeVote = ElectionVote.Vote.yes;
                    else
                    {
                        ShardConfiguration sConfig = null;
                        if (_clusterConfigMgr != null)
                            sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                        int configuredNodes = 0;
                        if (sConfig != null || sConfig.Servers != null)
                        {
                            configuredNodes = sConfig.Servers.Nodes.Count;
                        }
                        if (info != null && info.CSStatus == ConnectivityStatus.Connected && (configuredNodes != null && existingMembership.Servers.Count >= configuredNodes / 2))
                            vote.NodeVote = ElectionVote.Vote.no;
                        else
                            vote.NodeVote = ElectionVote.Vote.yes;
                    }
                }
                else
                    vote.NodeVote = ElectionVote.Vote.no;
            }
            else
                vote.NodeVote = ElectionVote.Vote.no;

            return vote;

        }

    }
}
