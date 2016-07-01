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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ElectionId = Alachisoft.NosDB.Common.Configuration.Services.ElectionId;

using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.ErrorHandling;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement
{
    class ElectionBasedMembershipStrategy : IMembershipStrategy
    {
        private ElectionManager _manager = null;
        private IShard _shard;
        private NodeContext _context = null;
        private const int _waitTimeout = 120 * 1000;
     
        private ClusterConfigurationManager _clusterConfigMgr = null;

        private Object _mutexOnWait = new Object();


        public ElectionBasedMembershipStrategy(IShard shard, NodeContext context,  LocalShardHeartbeatReporting heartbeatTable, ClusterConfigurationManager clusterConfigMgr)
        {
            this._shard = shard;
            this._context = context;
            this._clusterConfigMgr = clusterConfigMgr;
            Initialize();
        }

        public void Initialize()
        {
            if (_context != null)
            {
                _manager = new ElectionManager(_shard, _context, _clusterConfigMgr);
            }
        }

        public OperationId LastOperationId
        { get; set; }
        public void TriggerElectionMechanism(Activity activity, Server server, LocalShardHeartbeatReporting heartbeatReport, Membership existingMembership)
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = "" });
            if (existingMembership == null)
                existingMembership = new Membership();
            ShardConfiguration sConfig = null;
            //Get the shard configuration 
            if (_clusterConfigMgr != null)
                sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
            IList<Address> activeNodes = null;
            MembershipChangeArgs args = new MembershipChangeArgs();
            ServerNodes staticServerNodes = null;
            
            if(sConfig == null || sConfig.Servers == null)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                    LoggerManager.Instance.ShardLogger.Warn("ElectionBasedMembershipStrategy.TiggerElectionMechanism() ","The shard " + _context.LocalShardName + " does not exist in the configuration.");
                return;
            }
            staticServerNodes = sConfig.Servers;
            ElectionResult result = null;
         
            if (heartbeatReport != null)
                activeNodes = heartbeatReport.GetReportTable.Keys.ToList();

            Address activityNode = null;
            if (server == null)
                activityNode = _context.LocalAddress;
            else
                activityNode = server.Address;

            switch (activity)
            {
                case Activity.NodeJoining:
                    if (server == null)
                        return;
                    //On node join, we need to get membership from the config server for the first time.
                    Membership csMembership = _context.ConfigurationSession.GetMembershipInfo(_context.ClusterName, _context.LocalShardName);
                    ServerNode joiningNode = sConfig.Servers.GetServerNode(server.Address.IpAddress.ToString());
                    // If the added node is configured while the cluster is up and running, do the following.
                    if (joiningNode == null)
                    {
                        if(_clusterConfigMgr != null)
                        {
                            _clusterConfigMgr.UpdateClusterConfiguration();
                            sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                        }
                        if (sConfig == null)
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                LoggerManager.Instance.ShardLogger.Warn("ElectionBasedMembershipStrategy.TriggerElectionMechanism() ","The shard " + _context.LocalShardName + " does not exist in the configuration.");
                            return;
                        }
                        joiningNode = sConfig.Servers.GetServerNode(server.Address.IpAddress.ToString());
                      
                    }
                    if (joiningNode == null)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                            LoggerManager.Instance.ShardLogger.Warn("ElectionBasedMembershipStrategy.TriggerElectionMechanism() ", "The node " + server.Address + " is not part of the configuration.");
                        return;
                    }

                    if (existingMembership == null || existingMembership.Servers == null || !existingMembership.Servers.Contains(joiningNode))
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("ElectBasedMemSt.TriggerElectMech", "Node joining activity triggered for " + activityNode);

                    }
                    bool thisNodeIsPrimary = false;
                    OperationId lastOpId = null;

                    if (heartbeatReport!= null && heartbeatReport.GetReportTable.ContainsKey(server.Address))
                    {
                        args.ServerName = _context.LocalAddress;
                        args.ElectionId = null;
                        args.ChangeType = MembershipChangeArgs.MembershipChangeType.NodeJoined;

                        if (server.Address.Equals(_context.LocalAddress))
                        {
                            _context.ConfigurationSession.ReportNodeJoining(_context.ClusterName, _context.LocalShardName, sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString()));

                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", server.Address + " reported its joining to the config server. ");

                            //if the primary is not null and the channel is not disconnected, it can be set here.
                            if ((existingMembership == null || existingMembership.Primary == null) && csMembership.Primary != null && _shard.ActiveChannelsList.Contains(new Server(new Address(csMembership.Primary.Name, sConfig.Port), Status.Initializing)) && ObeysMajorityRule(_shard.ActiveChannelsList.Count, sConfig.Servers.Nodes.Count))
                            {
                                args.ServerName = new Address(csMembership.Primary.Name, sConfig.Port);
                                args.ElectionId = csMembership.ElectionId;
                                args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimarySet;
                                //if the node which was lost comes back up before the CS or the nodes can declare it dead,
                                //it should resume its status as a primary. There should be no need for an election in this case.
                                if (args.ServerName.Equals(_context.LocalAddress))
                                    thisNodeIsPrimary = true;
                            }
                        }

                        if (thisNodeIsPrimary)
                        {
                            if (csMembership.ElectionId != null && LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", "election_id: " + csMembership.ElectionId.Id + " election time :" + csMembership.ElectionId.ElectionTime);


                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", "I am already declared primary");
                            lastOpId = LastOperationId;
                            ChangeMembershipShardwide(args);
                        }
                        else
                            ((LocalShard)_shard).OnMembershipChanged(args);

                        if (server.Address.Equals(_context.LocalAddress))
                        {
                            ServerNode sNode = sConfig.Servers.GetServerNode(_context.LocalAddress.IpAddress.ToString());
                            if (sNode == null)
                            {
                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                    LoggerManager.Instance.ShardLogger.Warn("ElectionBasedMembershipStrategy.TriggerElectionMechanism() ", "The node " + sNode.Name + " does not exist in the configuration.");
                                return;
                            }

                            _context.ConfigurationSession.ReportHeartbeat(_context.ClusterName, _context.LocalShardName, sNode, existingMembership, lastOpId);
                        } 
                    }
                    else
                    {
                        if(existingMembership.Primary != null && existingMembership.Primary.Name.Equals(server.Address.IpAddress.ToString()))
                        {
                            if(sConfig.Servers== null || sConfig.Servers.Nodes == null || !ObeysMajorityRule(activeNodes.Count,sConfig.Servers.Nodes.Count))
                            {
                                _context.ConfigurationSession.SetNodeStatus(_context.ClusterName, _context.LocalShardName, existingMembership.Primary, NodeRole.None);
                                args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimaryDemoted;
                                args.ServerName = _context.LocalAddress;
                                ChangeMembershipShardwide(args);
                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                    LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", " Node addition activity occured. Primary node " + _context.LocalAddress.IpAddress.ToString() + " demoted.");
                                return;
                            }
                        }
                    }

                    break;
                case Activity.NodeLeaving:
                    if (server == null)
                        return;
                    bool hasMajority = ObeysMajorityRule(activeNodes.Count, staticServerNodes.Nodes.Count);
                    args.ServerName = server.Address;
                    args.ChangeType = MembershipChangeArgs.MembershipChangeType.NodeLeft;

                    _clusterConfigMgr.UpdateClusterConfiguration();
                   
                    if (existingMembership.Primary != null)
                    {
                        // if the existing primary is actually the node lost, we need to update the configuration.

                        if (existingMembership.Primary.Name == server.Address.IpAddress.ToString())
                        {
                            //if Primary leaves, it should be updated locally.
                            args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimaryLost;
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.TriggerElectMech", "Node leaving activity triggered for " + server.Address + " . Primary lost.");

                        }
                        else if (existingMembership.Primary.Name == _context.LocalAddress.IpAddress.ToString()) // if the existing primary is the local node, we need to check for possible demotion of the current primary.
                        {
                            if (!hasMajority)
                            {
                                _context.ConfigurationSession.SetNodeStatus(_context.ClusterName, _context.LocalShardName, existingMembership.Primary, NodeRole.None);
                                args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimaryDemoted;
                                args.ServerName = _context.LocalAddress;
                                ChangeMembershipShardwide(args);

                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                    LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.TriggerElectMech", " Node leaving activity occurred. Primary node " + _context.LocalAddress.IpAddress.ToString() + " demoted.");

                                return;
                            }
                        }
                    }
                    
                   ((LocalShard)_shard).OnMembershipChanged(args);

                    break;
                case Activity.GeneralElectionsTriggered:
                case Activity.TakeoverElectionsTriggered:
                    // this is where the actual election mechanism takes place.

                    //Step 1: if no node in the heartbeat table has a primary and there is no primary in the local node's membership, we proceed forward.
                    //Else if there is a primary but this looks like the takeover election mechanism, we proceed along as well.
                    if ((activity.Equals(Activity.GeneralElectionsTriggered) && !heartbeatReport.PrimaryExists() && existingMembership.Primary == null) || (activity.Equals(Activity.TakeoverElectionsTriggered) && heartbeatReport.PrimaryExists()))
                    {
                        //Step 2: we verify that this node has a majority of the shard nodes connected to it.
                        if (activeNodes != null && ObeysMajorityRule(activeNodes.Count, staticServerNodes.Nodes.Count))
                        {
                            //Step 3: Perform the initial sanity check. (Speculative phase)
                            if (ShouldIInitiateElection(heartbeatReport, activity))
                            {
                                if (existingMembership != null && existingMembership.Primary != null && activity == Activity.GeneralElectionsTriggered)
                                {
                                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                        LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", "A primary has already been selected for " 
                                            + _context.LocalShardName + " hence exiting the election mechanism.");
                                    return;
                                }
                                    //Step 4: The elections take place in real. (Authoritative Phase)
                                result = HoldElection(heartbeatReport, activity);
                                if (result != null)
                                {
                                    if (result.PollingResult == ElectionResult.Result.PrimarySelected)
                                    {
                                        //if the shard is undergoing the takeover election mechanism, the old primary needs to 
                                        //be demoted first.
                                        bool oldPrimaryDemoted = false;
                                        if(activity == Activity.TakeoverElectionsTriggered)
                                        {
                                            MembershipChangeArgs args2 = new MembershipChangeArgs();
                                            args2.ChangeType = MembershipChangeArgs.MembershipChangeType.ForcefullyDemotePrimary;
                                            args2.ServerName = _context.LocalAddress;
                                            args2.ElectionId = existingMembership.ElectionId;

                                            Message msg = new Message();
                                            msg.Payload = args2;
                                            msg.MessageType = MessageType.MembershipOperation;
                                            msg.NeedsResponse = true;
                                            ShardRequestBase<bool> request = _shard.CreateUnicastRequest<bool>(new Server(new Address(existingMembership.Primary.Name, sConfig.Port), Status.Running), msg);
                                            IAsyncResult result2 = request.BeginExecute();
                                            oldPrimaryDemoted = request.EndExecute(result2);
                                            
                                        }
                                        //Submit the result to the CS.
                                        if (activity == Activity.GeneralElectionsTriggered || (activity == Activity.TakeoverElectionsTriggered && oldPrimaryDemoted))
                                            _context.ConfigurationSession.SubmitElectionResult(_context.ClusterName.ToLower(), _context.LocalShardName.ToLower(), result);
                                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                            LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.TriggerElectMech", "Election result submitted for shard " + _context.LocalShardName.ToString());

                                        _context.ElectionResult = result;
                                        args.ServerName = _context.LocalAddress;
                                        args.ElectionId = result.ElectionId;
                                        args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimarySelected;
                                        //Once, the result is submitted, inform the shard nodes.
                                        ChangeMembershipShardwide(args);
                                        _context.ConfigurationSession.ReportHeartbeat(_context.ClusterName, _context.LocalShardName, result.ElectedPrimary, existingMembership, LastOperationId);
                                    }
                                    //Finally, end this round of elections.
                                    _context.ConfigurationSession.EndElection(_context.ClusterName, _context.LocalShardName, result.ElectionId);
                                }
                            }
                        }
                    }

                    break;

                case Activity.CSDisconnected:
                    //this is called whenever a node loses connection with the config server.

                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("ElectionBasedMembershipStrategy.TriggerElectionMechanism() ","Config Server disconnected. ");
                    //if the number of configured nodes are even and the primary loses connection with the CS, it needs to demote itself.
                    if (existingMembership != null && existingMembership.Primary != null && existingMembership.Primary.Name == _context.LocalAddress.IpAddress.ToString() && staticServerNodes.Nodes.Count % 2 == 0)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.TriggerElectMech"," Connection of the node " + _context.LocalAddress.ToString() + " with the config server is lost.");

                        args.ServerName = _context.LocalAddress;
                        args.ElectionId = existingMembership.ElectionId;
                        args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimaryDemoted;
                        ChangeMembershipShardwide(args);
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                            LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", " Primary node " + _context.LocalAddress.IpAddress.ToString() + " demoted because the primary lost connection with the CS.");
                    }
                    break;
                case Activity.ForcefulPrimaryDemotion:
                    if(existingMembership != null && existingMembership.Primary != null && existingMembership.Primary.Name == _context.LocalAddress.IpAddress.ToString())
                    {
                        _context.ConfigurationSession.SetNodeStatus(_context.ClusterName, _context.LocalShardName, existingMembership.Primary, NodeRole.None);

                        args.ChangeType = MembershipChangeArgs.MembershipChangeType.PrimaryDemoted;
                        args.ServerName = _context.LocalAddress;
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                            LoggerManager.Instance.ShardLogger.Info("electBasedMemSt.TriggerElectMech", "Primary node " + _context.LocalAddress.IpAddress.ToString() + " demoted in order to complete the take over election mechanism. ");

                        ((LocalShard)_shard).OnMembershipChanged(args);
                    }
                    break;
            }
        }

        private void ChangeMembershipShardwide(MembershipChangeArgs args)
        {
            Message message = new Message();
            message.NeedsResponse = false;
            message.MessageType = MessageType.MembershipOperation;

            //Since, in order to handle deadlock scenarios, the message cannot be sent to the local node, 
            // this method is being called in its stead.
            ((LocalShard)_shard).OnMembershipChanged(args);

            message.Payload = args;
            IList<Server> activeList = new List<Server>();
            foreach (var node in _shard.ActiveChannelsList)
            {
                if (!node.Address.Equals(_context.LocalAddress))
                    activeList.Add(node);
            }
            ShardMulticastRequest<ResponseCollection<object>, object> request = _shard.CreateMulticastRequest<ResponseCollection<object>, object>(activeList, message);
            IAsyncResult asyncResult = request.BeginExecute();
            request.EndExecute(asyncResult);
        }

        /// <summary>
        /// Speculative Phase:
        /// 1. We search for the node with the latest op-log entry. This detail bears the highest value.
        /// 2. Next, if multiple nodes have the same op-log entry, we move onto the next step.
        /// 3. All those nodes which have the same op-log entry AND are connected to the CS are considered.
        /// 4. Highest priority from amongst these active nodes are taken into consideration.
        /// 5. If this node fulfills all of the above, it successfully passes the speculative phase.
        /// </summary>
        /// <param name="heartbeatReport"></param>
        /// <returns></returns>

        private bool ShouldIInitiateElection(LocalShardHeartbeatReporting heartbeatReport, Activity activity)
        {
            IList<Address> activeNodes = null;
            if (heartbeatReport != null)
            {
                activeNodes = heartbeatReport.GetReportTable.Keys.ToList();
                OperationId maxOplog = null;
                IList<string> matchingOplogServerIPs = new List<string>();
                
                HeartbeatInfo localHeartbeat = heartbeatReport.GetHeartbeatInfo(_context.LocalAddress);
                if (localHeartbeat == null)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                        LoggerManager.Instance.ShardLogger.Warn(
                            "ElectionBasedMembershipStrategy.ShouldIInititateElections()",
                            "local node heartbeat is null");
                    return false;
                }

                OperationId lastRepId = null;
              

                if (activity.Equals(Activity.TakeoverElectionsTriggered))
                {
                 
                    HeartbeatInfo info = null;
                    if (_shard != null && _shard.Primary != null)
                        info = heartbeatReport.GetHeartbeatInfo(_shard.Primary.Address);
                   
                }

                else
                {
                    ShardConfiguration sConfig = _clusterConfigMgr.GetShardConfiguration(_context.LocalShardName);
                    int configuredNodesCount = 0;
                    OperationId OpIdAtCS = null;
                    if(sConfig != null && sConfig.Servers != null && sConfig.Servers.Nodes != null)
                        configuredNodesCount = sConfig.Servers.Nodes.Count;
                    if (configuredNodesCount > 0 && activeNodes != null && activeNodes.Count < configuredNodesCount)
                    {
                        ShardInfo sInfo = null;
                        ClusterInfo cInfo = _context.ConfigurationSession.GetDatabaseClusterInfo(_context.ClusterName);
                        if (cInfo != null)
                            sInfo = cInfo.GetShardInfo(_context.LocalShardName);
                        if (sInfo != null)
                            OpIdAtCS = sInfo.LastOperationId;
                        if(OpIdAtCS > lastRepId)
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                                LoggerManager.Instance.ShardLogger.Info(
                                    "electBasedMemSt.ShouldIInitElections()",
                                    "CS has an operation newer than my operation. Hence, waiting.");

                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                            {
                                if(OpIdAtCS != null)
                                {
                                    LoggerManager.Instance.ShardLogger.Info(
                                   "electBasedMemSt.ShouldIInitElections()",
                                   "Operation ID on the CS:- " + OpIdAtCS.ElectionId + ":"
                                   + OpIdAtCS.ElectionBasedSequenceId);
                                }
                                else
                                {
                                    LoggerManager.Instance.ShardLogger.Info(
                                   "electBasedMemSt.ShouldIInitElections()", "The operation ID at the CS is set to null.");
                                }
                                if(lastRepId != null)
                                {
                                    LoggerManager.Instance.ShardLogger.Info(
                                    "electBasedMemSt.ShouldIInitElections()", "Local node Operation ID:- " + lastRepId.ElectionId + 
                                    ":" + lastRepId.ElectionBasedSequenceId);
                                }
                                else
                                {
                                    LoggerManager.Instance.ShardLogger.Info(
                                   "electBasedMemSt.ShouldIInitElections()", "The local node operation ID is set to null.");
                                }
                            }
                            //We maintain the last replicated operation log entry with the CS.
                            //If a node in a shard with older data (usually the previous secondary) is up before the node with the
                            //latest data(usually the previous primary), it waits for a configurable amount of time (2 minutes for
                            //now) before proceeding with the election procedure if it is still unable to detect a primary node.
                            //This way we give the node with the latest data a chance to become primary and therefore avoid data loss.
                            lock(_mutexOnWait)
                            {
                                Monitor.Wait(_mutexOnWait, _waitTimeout);
                            }
                            
                        }
                    }

                    for (int i = 0; i < activeNodes.Count; i++)
                    {
                        HeartbeatInfo info = heartbeatReport.GetHeartbeatInfo(activeNodes[i]);
                        OperationId currIndexOplog = info.LastOplogOperationId;
                        if (currIndexOplog > maxOplog)
                        {
                            maxOplog = currIndexOplog;
                        }

                        if (((localHeartbeat.LastOplogOperationId == null && info.LastOplogOperationId == null)||localHeartbeat.LastOplogOperationId != null && localHeartbeat.LastOplogOperationId.Equals(info.LastOplogOperationId) )&&
                            info.CSStatus == ConnectivityStatus.Connected)
                            matchingOplogServerIPs.Add(activeNodes[i].IpAddress.ToString());
                    } 
                }
                
                if (localHeartbeat.LastOplogOperationId != null && maxOplog != null && maxOplog > localHeartbeat.LastOplogOperationId && (lastRepId == null || maxOplog > lastRepId))
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "Local operation log is behind max op log wrt " + _context.LocalShardName + " shard.");
                    if (maxOplog != null && LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "maxOplog: " + maxOplog.ElectionId + ":" + maxOplog.ElectionBasedSequenceId);
                    if (localHeartbeat.LastOplogOperationId != null && LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "local opLog (from the heartbeat): " + localHeartbeat.LastOplogOperationId.ElectionId + ":" + localHeartbeat.LastOplogOperationId.ElectionBasedSequenceId);
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled && lastRepId != null)
                        LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "LastOpFromOpLog (from the replication module): " + lastRepId.ElectionId + ":" + lastRepId.ElectionBasedSequenceId);

                    return false;
                }
                else if (maxOplog == localHeartbeat.LastOplogOperationId || (lastRepId != null && lastRepId.Equals(maxOplog)))
                {
                    //if: there are multiple nodes that have the same oplog entry, 
                    //decision will be made on the basis of the priorities.
                    //else: the node with the highest oplog entry will be considered eligible.
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "Local operation log is equal to the max op log wrt " + _context.LocalShardName + " shard.");
                    if (maxOplog != null && (localHeartbeat != null && localHeartbeat.LastOplogOperationId != null))
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "maxOplog: " + maxOplog.ElectionId + ":" + maxOplog.ElectionBasedSequenceId);
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "local opLog (from the heartbeat): " + localHeartbeat.LastOplogOperationId.ElectionId + ":" + localHeartbeat.LastOplogOperationId.ElectionBasedSequenceId);
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled && lastRepId != null)
                            LoggerManager.Instance.ShardLogger.Debug("electBasedMemSt.ShouldIInitiateElect()", "LastOpFromOpLog (from the replication module): " + lastRepId.ElectionId + ":" + lastRepId.ElectionBasedSequenceId);
                    }
                    if (matchingOplogServerIPs.Count > 0)
                    {
                        int highestRunningNodePriority = _manager.GetHighestNodePriority(activeNodes,
                            matchingOplogServerIPs);

                        if (highestRunningNodePriority.Equals(_manager.LocalServerPriority))
                        {
                            if (LoggerManager.Instance.ShardLogger != null &&
                                LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug(
                                    "ElectionBasedMembershipStrategy.ShouldIInitiateElection()",
                                    "Node : " + _context.LocalAddress.IpAddress.ToString() + " in shard: " +
                                    _context.LocalShardName + " is eligible having priority: " +
                                    highestRunningNodePriority + " .");
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Authoritative phase:
        /// 1. the requesting node acquires an election lock up at the config server.
        /// 2. It then proceeds with the election conduction where it asks for votes.
        /// 3. Each node checks their world view once more before voting with a yes or no.
        /// 4. Its mandatory for a consensus amongst all the nodes for an election term to be successful. 
        /// 5. One negative vote nullifies the current election term. If such is the case, the entire procedure 
        /// needs to be repeated.
        /// </summary>
        /// <param name="reportTable"></param>
        /// <returns></returns>
        private ElectionResult HoldElection(LocalShardHeartbeatReporting reportTable, Activity activity)
        {
            ElectionId electionId = null;

            ServerNode requestingNode = new ServerNode();
            requestingNode.Name = _context.LocalAddress.IpAddress.ToString();
            requestingNode.Priority = (int)_manager.LocalServerPriority;
            ElectionType type = ElectionType.None;
            if (activity == Activity.GeneralElectionsTriggered)
                type = ElectionType.GeneralElections;
            else if (activity == Activity.TakeoverElectionsTriggered)
                type = ElectionType.TakeoverElections;

            Object response = _context.ConfigurationSession.BeginElection(_context.ClusterName.ToLower(), _context.LocalShardName.ToLower(), requestingNode, type);
            if (response != null)
            {
                electionId = response as ElectionId;
                if (electionId != null)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        LoggerManager.Instance.ShardLogger.Info("ElectionBasedMembershipStrategy", type + " started for node " + _context.LocalAddress);
                    Address[] activeNodes = reportTable.GetReportTable.Keys.ToArray();

                    return _manager.ConductElection(electionId, activeNodes, activity);
                }
                else
                {
                    //if we get a database exception from the CS, we wait for a configurable amount of time
                    //before trying to elect a new primary again.
                    if (response is DatabaseException && ((DatabaseException)response).ErrorCode == ErrorCodes.Cluster.PRIMARY_ALREADY_EXISTS)
                    {
                        lock (_mutexOnWait)
                        {
                            Monitor.Wait(_mutexOnWait, _waitTimeout);
                        }
                    }
                }
            }
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                LoggerManager.Instance.ShardLogger.Debug("ElectionBasedMembershipStrategy.HoldElection() ", "Node " + _context.LocalAddress.ToString() + " failed to acquire an election lock on the CS.");
             return null;
        }

        private bool ObeysMajorityRule(int existingCount, int maxCount)
        {
            if (existingCount >= (int)Math.Ceiling((double)maxCount / 2))
                return true;
            return false;
        }

        public object OnMessageReceived(object message, Server source, LocalShardHeartbeatReporting heartbeatReport, Membership existingMembership)
        {
            Election election = message as Election;
            if (election != null)
            {
                HeartbeatInfo localHeartbeat = heartbeatReport.GetHeartbeatInfo(_context.LocalAddress);
                if (localHeartbeat != null) return _manager.CastVote(election, existingMembership, localHeartbeat);
            }
            return null;
        }


        public void OnPrimaryChanged()
        {
            lock(_mutexOnWait)
            {
                Monitor.PulseAll(_mutexOnWait);
            }
        }
    }
}
