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
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.RPCFramework;
using Alachisoft.NosDB.Common.RPCFramework.DotNetRPC;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Monitoring.Configuration
{
    public class ConfigrationMonitorSession : IConfigurationMonitor, IRequestListener
    {

        #region /*          Fields          */
        ConfigurationServer configServer;
        DateTime _sessionStartTime;
        string _sessionid;
        private Common.RPCFramework.RPCService<ConfigrationMonitorSession> _rpcService = null;
        private IDualChannel _channel;
        private Dictionary<string, Membership> _membershipPerShard = new Dictionary<string, Membership>();
        private Dictionary<string, List<ServerInfo>> _configuredNodesPerShard;
        private Dictionary<string, List<ServerInfo>> _runningNodesPerShard;
        private ShardInfo[] _configuredShards;
        #endregion

        public IDualChannel Channel 
        { 
            get { return _channel; } 
            set 
            { 
                _channel = value; 
                //if (configServer != null) configServer.AddClientChannel((DualChannel)_channel); 
            } 
        }

        public ConfigrationMonitorSession(ConfigurationServer server, UserCredentials credentials)
        {
            this.configServer = server;
            //ConfigurationProvider.Provider = this;
            this._sessionStartTime = DateTime.Now;
            _sessionid = Guid.NewGuid().ToString();
            _rpcService = new Common.RPCFramework.RPCService<ConfigrationMonitorSession>(new TargetObject<ConfigrationMonitorSession>(this));
        }

        #region /*          IRequestListener Methods         */

        public object OnRequest(IRequest request)
        {
            if (request.Message is ManagementCommand)
            {
                ManagementCommand command = request.Message as ManagementCommand;
                if (command == null)
                    return null;
                ManagementResponse response = new ManagementResponse();
                response.MethodName = command.MethodName;
                response.Version = command.CommandVersion;
                response.RequestId = command.RequestId;
                byte[] arguments= CompactBinaryFormatter.ToByteBuffer(command.Parameters, null);
                try
                {
                    response.ResponseMessage = _rpcService.InvokeMethodOnTarget(command.MethodName,
                        command.Overload,
                        GetTargetMethodParameters(arguments)
                        );
                    _channel.GetType();
                }
                catch (System.Exception ex)
                {
                    response.Exception = ex;
                    
                }
                return response;
            }
            else
                return null;
        }

        protected object[] GetTargetMethodParameters(byte[] graph)
        {
            TargetMethodParameter parameters = CompactBinaryFormatter.FromByteBuffer(graph, "ok") as TargetMethodParameter;
            return parameters.ParameterList.ToArray();
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            //if (configServer != null && channel.PeerAddress != null)
            //    configServer.RemoveClientChannel(channel.PeerAddress);
        }

        #endregion


        #region /*          IConfigurationMonitor Methods         */

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfiguredClusters, 1)]
        public ClusterInfo[] GetConfiguredClusters()
        {
            return configServer.GetConfiguredClusters();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDatabaseClusterInfo, 1)]
        public ClusterInfo GetConfiguredClusters(string cluster)
        {
            return configServer.GetDatabaseClusterInfo(cluster);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 1)]
        public Membership GetMembershipInfo(string cluster, string shard)
        {
            Membership membership =  configServer.GetMembershipInfo(cluster, shard).Clone() as Membership;
            if (!_membershipPerShard.ContainsKey(shard))
                _membershipPerShard.Add(shard, membership);
            else
                _membershipPerShard[shard] = membership;
            return membership;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUpdatedMembershipInfo, 1)]
        public Membership GetUpdatedMembershipInfo(string cluster, string shard)
        {
            if (!_membershipPerShard.ContainsKey(shard))
                return GetMembershipInfo(cluster,shard);

            Membership newMembership =  configServer.GetMembershipInfo(cluster, shard).Clone() as Membership;
            if (newMembership == null && _membershipPerShard[shard] != null)
            {
                Membership unKnownMembership = new Membership();
                unKnownMembership.Cluster = cluster;
                unKnownMembership.Shard = shard;
                unKnownMembership.Primary = null;
                _membershipPerShard[shard] = null;
                return unKnownMembership;
            }
            else if (newMembership != null && _membershipPerShard[shard] == null)
            {
                _membershipPerShard[shard] = newMembership;
                return newMembership;
            }
            else if (newMembership != null && _membershipPerShard[shard] != null)
            {
                if (newMembership.Primary == null)
                {
                    if (_membershipPerShard[shard].Primary != null)
                    {
                        _membershipPerShard[shard] = newMembership;
                        return newMembership;
                    }
                }
                else if (!newMembership.Primary.Equals(_membershipPerShard[shard].Primary))
                {
                    _membershipPerShard[shard] = newMembership;
                    return newMembership;
                }
            }
            return null;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfiguredShards, 1)]
        public ShardInfo[] GetConfiguredShards(string cluster)
        {
            return _configuredShards =  configServer.GetDatabaseClusterInfo(cluster).ShardInfo.Values.ToArray();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUpdatedConfiguredShards, 1)]
        public ShardInfo[] GetUpdatedConfiguredShards(string cluster)
        {
            ShardInfo[] newConfiguredShards = configServer.GetDatabaseClusterInfo(cluster).ShardInfo.Values.ToArray();
            if (_configuredShards == null)
                return _configuredShards = newConfiguredShards;
            if(_configuredShards.Length == newConfiguredShards.Length)
            {
                foreach (ShardInfo shard in newConfiguredShards)
                {
                    if (!_configuredShards.Any(x => x.Name == shard.Name))
                        return _configuredShards = newConfiguredShards;
                }
                return null;
            }
            return _configuredShards = newConfiguredShards;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfigureServerNodes, 1)]
        public Dictionary<string, List<ServerInfo>> GetConfigureServerNodes(string cluster)
        {
            Dictionary<string, List<ServerInfo>> configuredServersPerShard = new Dictionary<string, List<ServerInfo>>();
            foreach (ShardInfo shard in configServer.GetDatabaseClusterInfo(cluster).ShardInfo.Values)
            {
                List<ServerInfo> serverNodes = new List<ServerInfo>();
                if (shard.RunningNodes != null)
                    foreach (ServerInfo server in shard.ConfigureNodes.Values)
                    {
                        serverNodes.Add(server);
                    }
                configuredServersPerShard.Add(shard.Name, serverNodes);
            }
            return _configuredNodesPerShard = configuredServersPerShard;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUpdatedConfigureServerNodes, 1)]
        public Dictionary<string, List<ServerInfo>> GetUpdatedConfigureServerNodes(string cluster)
        {
            if (_configuredNodesPerShard == null)
                return GetConfigureServerNodes(cluster);

            Dictionary<string, List<ServerInfo>> newConfiguredServersPerShard = new Dictionary<string, List<ServerInfo>>();
            foreach (ShardInfo shard in configServer.GetDatabaseClusterInfo(cluster).ShardInfo.Values)
            {
                List<ServerInfo> serverNodes = new List<ServerInfo>();
                if (shard.RunningNodes != null)
                    foreach (ServerInfo server in shard.ConfigureNodes.Values)
                    {
                        serverNodes.Add(server);
                    }
                newConfiguredServersPerShard.Add(shard.Name, serverNodes);
            }

            if (_configuredNodesPerShard.Keys.Count != newConfiguredServersPerShard.Keys.Count)
                return _configuredNodesPerShard = newConfiguredServersPerShard;
            else
            {
                foreach (string shardKey in newConfiguredServersPerShard.Keys)
                {
                    if (!newConfiguredServersPerShard.ContainsKey(shardKey))
                        return _configuredNodesPerShard = newConfiguredServersPerShard;

                    if (newConfiguredServersPerShard[shardKey].Count != _configuredNodesPerShard[shardKey].Count)
                        return _configuredNodesPerShard = newConfiguredServersPerShard;

                    foreach (ServerInfo server in newConfiguredServersPerShard[shardKey])
                    {
                        if (!_configuredNodesPerShard[shardKey].Contains(server))
                            return _configuredNodesPerShard = newConfiguredServersPerShard;
                    }
                }
            }
            return null;
        }


        [TargetMethod(ConfigurationCommandUtil.MethodName.GetRunningServerNodes, 1)]
        public Dictionary<string, List<ServerInfo>> GetRunningServerNodes(string cluster)
        {
            Dictionary<string, List<ServerInfo>> runningServersPerShard = new Dictionary<string, List<ServerInfo>>();
            foreach (ShardInfo shard in configServer.GetDatabaseClusterInfo(cluster).ShardInfo.Values)
            {
                List<ServerInfo> serverNodes = new List<ServerInfo>();
                if(shard.RunningNodes != null)
                    foreach (ServerInfo server in shard.RunningNodes.Values)
                    {
                        serverNodes.Add(server);
                    }
                runningServersPerShard.Add(shard.Name, serverNodes);
            }
            return _runningNodesPerShard = runningServersPerShard;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUpdatedRunningServerNodes, 1)]
        public Dictionary<string, List<ServerInfo>> GetUpdatedRunningServerNodes(string cluster)
        {
            if (_runningNodesPerShard == null)
                return GetRunningServerNodes(cluster);
             Dictionary<string, List<ServerInfo>> newRunningServersPerShard = new Dictionary<string, List<ServerInfo>>();
            foreach (ShardInfo shard in configServer.GetDatabaseClusterInfo(cluster).ShardInfo.Values)
            {
                List<ServerInfo> serverNodes = new List<ServerInfo>();
                if (shard.RunningNodes != null)
                    foreach (ServerInfo server in shard.RunningNodes.Values)
                    {
                        serverNodes.Add(server);
                    }
                newRunningServersPerShard.Add(shard.Name, serverNodes);
            }

            if (_runningNodesPerShard.Keys.Count != newRunningServersPerShard.Keys.Count)
                return _runningNodesPerShard = newRunningServersPerShard;
            else
            {
                foreach (string shardKey in newRunningServersPerShard.Keys)
                {
                    if(!newRunningServersPerShard.ContainsKey(shardKey))
                        return _runningNodesPerShard = newRunningServersPerShard;
                    
                    if(newRunningServersPerShard[shardKey].Count != _runningNodesPerShard[shardKey].Count)
                        return _runningNodesPerShard = newRunningServersPerShard;

                    foreach (ServerInfo server in newRunningServersPerShard[shardKey])
                    {
                        if (!_runningNodesPerShard[shardKey].Contains(server))
                            return _runningNodesPerShard = newRunningServersPerShard;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
