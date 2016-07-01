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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Protobuf;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.RPCFramework;
using Alachisoft.NosDB.Common.RPCFramework.DotNetRPC;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Core.Security.Impl;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common;
using System.Diagnostics;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Google.ProtocolBuffers;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ConfigurationSession : IConfigurationSession, IRequestListener
    {
        #region Security
        private ISecurityManager _securityManager;

        private bool isDatabaseSession = false;
        private bool isDistributorSession = false;
        private bool isConfigurationSession = false;
        #endregion

        ConfigurationProvider ConfigurationProvider { set; get; }
        private static Dictionary<string, object> s_safeList = new System.Collections.Generic.Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        ConfigurationServer configServer;
        DateTime _sessionStartTime;
        ISessionId _sessionid;
        private Common.RPCFramework.RPCService<ConfigurationSession> _rpcService = null;
        private IDualChannel _channel;
        private bool _internalSession;

        public IDualChannel Channel { get { return _channel; } set { _channel = value; if (configServer != null) configServer.AddClientChannel((DualChannel)_channel); } }

        public IDualChannel NodeChannel { get { return _channel; } set { _channel = value; if(configServer != null)configServer.AddNodeChannel((DualChannel)_channel); } }

        static ConfigurationSession()
        {
            //A method included in safe-list can be called on passive configuration server
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetConfiguredClusters, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetAllClusterConfiguration, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetAllRunningJobs, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetCollectionDistribution, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetConfigurationClusterConfiguration, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetConfiguredShards, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetConfigureServerNodes, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetCurrentDistribution, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetCurrentRole, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetDatabaseCluster, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetDatabaseClusterConfiguration, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetDatabaseClusterInfo, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetDistriubtionStrategy, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetPercentageCPUUsage, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetRunningServerNodes, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetRunningServers, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.ReplicateTransaction, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetState, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.StartNode, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.StopNode, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.AddNodeToConfigurationCluster, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.RemoveNodeFromConfigurationCluster, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.Authenticate, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.AuthenticateNoSDbClient, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterUID, null);
            // s_safeList.Add(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterPrimery,null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.CreateConfigurationServer, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.HasSynchronizedWIthPrimaryServer, null);

            s_safeList.Add(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterPrimery, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterAvailability, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.VerifyConfigurationServerAvailability, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.UpdateCSNodePriority, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetConfClusterServers, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.GetUsersInformation, null);

            s_safeList.Add(ConfigurationCommandUtil.MethodName.MarkConfiguritonSession, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.MarkDatabaseSession, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.MarkDistributorSession, null);
            s_safeList.Add(ConfigurationCommandUtil.MethodName.IsNodeRunning, null);
            //s_safeList.Add(ConfigurationCommandUtil.MethodName.getcon);
        }

        public ConfigurationSession(ConfigurationServer server, IClientAuthenticationCredential credentials)
        {
            _securityManager = server.SecurityManager;

            this.configServer = server;
            ConfigurationProvider = new RPC.ConfigurationProvider();
            this.ConfigurationProvider.Provider = this;
            this._sessionStartTime = DateTime.Now;
            _sessionid = new RouterSessionId();
            _sessionid.SessionId = Guid.NewGuid().ToString();
            _rpcService = new Common.RPCFramework.RPCService<ConfigurationSession>(new TargetObject<ConfigurationSession>(this));
        }

        public ISessionId SessionId
        {
            get
            {
                return _sessionid;
            }
        }

        public DateTime SessionStartTime
        {
            get { return _sessionStartTime; }
        }

        public SessionManager Parent { get; set; }

        public void Close()
        {
            configServer.Stop();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfiguredClusters, 1)]
        public ClusterInfo[] GetConfiguredClusters()
        {
            try
            {
                return configServer.GetConfiguredClusters();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
		}

        [TargetMethod(ConfigurationCommandUtil.MethodName.OpenConfigurationSession, 1)]
        public IServerAuthenticationCredential OpenConfigurationSession(string cluster, IClientAuthenticationCredential clientCredentials)
        {
            return Authenticate(clientCredentials as SSPIClientAuthenticationCredential);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.Authenticate, 1)]
        public IServerAuthenticationCredential Authenticate(IClientAuthenticationCredential clientCredentials)
        {
            IServerAuthenticationCredential serverAuthenticationCredentials = _securityManager.Authenticate(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, clientCredentials, this._sessionid, SSPIUtility.IsLocalServer(_channel.PeerAddress.IpAddress), isDatabaseSession ? MiscUtil.NOSDB_DBSVC_NAME : isDistributorSession ? MiscUtil.NOSDB_DISTSVC_NAME : isConfigurationSession ? MiscUtil.NOSDB_CSVC_NAME : null);
            if (serverAuthenticationCredentials.IsAuthenticated)
            {
 
            }
            return serverAuthenticationCredentials;
        }

       // public DatabaseConfigurations

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDatabaseClusterConfiguration, 1)]
        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, clusterName, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                    return configServer.GetDatabaseClusterConfiguration(clusterName);
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetState, 1)]
        public object GetState()
        {
            return configServer.GetState();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.BeginTakeOver, 1)]
        public void BeginTakeOver()
        {
            configServer.BeginTakeOver();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.Demote, 1)]
        public bool Demote()
        {
            return configServer.Demote();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ReplicateTransaction, 1)]
        public bool ReplicateTransaction(object transaction)
        {
            return configServer.ReplicateTransaction(transaction);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetShardsPort,1)]
        public Dictionary<string, int> GetShardsPort(string clusterName)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            SecurityManager.GetSecurityInformation(Permission.Create_Cluster, MiscUtil.LOCAL, out resourceId, out superResourceId, MiscUtil.CLUSTERED, MiscUtil.NOSDB_CLUSTER_SERVER);
            if (_securityManager.Authorize(MiscUtil.CONFIGURATION_SHARD_NAME, _sessionid, resourceId, superResourceId, Permission.Create_Cluster))
            {
                Dictionary<string, int> shardPorts = configServer.GetShardsPort(clusterName, _sessionid);
                configServer.AddSecurityInformation(MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, superResourceId, clusterName, _sessionid);
                return shardPorts;
            }
            throw new SecurityException(Common.ErrorHandling.ErrorCodes.Security.UNAUTHORIZED_USER);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RegisterClusterConfiguration, 1)]
        public void RegisterClusterConfiguration(ClusterConfiguration configuration)
        {
            try
            {
                configServer.RegisterClusterConfiguration(configuration);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetCurrentRole, 1)]
        public NodeRole GetCurrentRole()
        {
            if (configServer == null) return NodeRole.None;
            return configServer.GetCurrentRole();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UnregisterClusterConfiguration, 1)]
        public void UnregisterClusterConfiguration(ClusterConfiguration configuration)
        {
            try
            {

                configServer.UnregisterClusterConfiguration(configuration);    
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateClusterConfiguration, 1)]
        public void UpdateClusterConfiguration(ClusterConfiguration configuration)
        {
            try
            {
                configServer.UpdateClusterConfiguration(configuration);
                NotifyConfigurationChange(configuration.Name, null, ChangeType.ConfigurationUpdated, null);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateCluster, 1)]
        public void CreateCluster(string name, ClusterConfiguration configuration)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Cluster, name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME,this._sessionid, resourceId, superResourceId, Permission.Create_Configuration_Cluster))
                {
                    if (configServer.VerifyConfigurationClusterAvailability(configuration.Name))
                    {
                        if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME,
                            this._sessionid, resourceId, superResourceId, Permission.Create_Cluster))
                        {
                            #region Extract Configuration Cluster Information From Database Cluster Information
                            ConfigServerConfiguration configServerConfiguration = new ConfigServerConfiguration();
                            configServerConfiguration.Name = configuration.Name;
                            configServerConfiguration.Servers = new ServerNodes();
                            configServerConfiguration.UID = Guid.NewGuid().ToString();
                            configServerConfiguration.Port = ConfigurationSettings<CSHostSettings>.Current.Port;
                            bool configComplete = false;
                            foreach (var shardConfiguration in configuration.Deployment.Shards.Values)
                            {
                                foreach (var node in shardConfiguration.Servers.Nodes.Values)
                                {
                                    if (!configServerConfiguration.Servers.ContainsNode(node.Name))
                                    {
                                        configServerConfiguration.Servers.AddNode(node);

                                        if (configServerConfiguration.Servers.Nodes.Count == 2)
                                        {
                                            configComplete = true;
                                            break;
                                        }
                                    }
                                }
                                if(configComplete)
                                    break;
                            }
                            #endregion

                            configServer.CreateConfigurationCluster(configServerConfiguration, configuration.Deployment.HeartbeatInterval, configuration.Deployment.Replication,  configuration.DisplayName);
                            ResourceId clusterResourceId;
                            ResourceId clusterSuperResourceId;
                            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Cluster, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, out clusterResourceId, out clusterSuperResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                            configServer.AddSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, clusterResourceId, clusterSuperResourceId, MiscUtil.CLUSTERED, this._sessionid);
                           
                            configServer.AddSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME,resourceId, superResourceId, name);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveCluster, 1)]
        public void RemoveCluster(string name)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Cluster, name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Delete_Cluster))
                {
                    configServer.RemoveCluster(name);
                    NotifyConfigurationChange(name, null, ChangeType.ConfigurationRemoved, null);
                    configServer.RemoveSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, superResourceId, name, this._sessionid);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.AddShardToCluster, 1)]
        public bool AddShardToCluster(string cluster, string shard, ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Modify_Cluster, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Modify_Cluster))
                {
                configServer.VerifyValidClusterOpperation(cluster);
                configServer.AddShardToCluster(cluster, shard, shardConfiguration, distributionConfiguration);
                NotifyConfigurationChange(cluster, shard, ChangeType.ShardAdded, null);
                return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveShardFromCluster, 1)]
        public bool RemoveShardFromCluster(string cluster, string shard, IDistributionConfiguration configuration, Boolean isGraceful)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Modify_Cluster, cluster, out resourceId, out superResourceId, cluster, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Modify_Cluster))
                {
                    configServer.VerifyValidClusterOpperation(cluster);
                    configServer.RemoveShardFromCluster(cluster, shard, configuration, isGraceful);
                    NotifyConfigurationChange(cluster, shard, 
                        isGraceful ? ChangeType.ShardRemovedGraceful : ChangeType.ShardRemovedForceful, null);
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.AddServerToShard, 1)]
        public bool AddServerToShard(string cluster, string shard, ServerNode server)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Modify_Cluster, cluster, out resourceId, out superResourceId, cluster, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Modify_Cluster))
                {
                    configServer.VerifyValidClusterOpperation(cluster);
                    Membership membership = configServer.AddServerToShard(cluster, shard, server);
                    NotifyConfigurationChange(cluster, shard, ChangeType.NodeAdded, membership);
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveServerFromShard, 1)]
        public bool RemoveServerFromShard(string cluster, string shard, ServerNode server)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Modify_Cluster, cluster, out resourceId, out superResourceId, cluster, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Modify_Cluster))
                {
                configServer.VerifyValidClusterOpperation(cluster);
                Membership membership = this.configServer.RemoveServerFromShard(cluster, shard, server);
                NotifyConfigurationChange(cluster, shard, ChangeType.NodeRemoved, membership);
                return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        protected object[] GetTargetMethodParameters(byte[] graph)
        {
            TargetMethodParameter parameters = CompactBinaryFormatter.FromByteBuffer(graph, "ok") as TargetMethodParameter;
            return parameters.ParameterList.ToArray();
        }

        #region IRequestListener Member
        public object OnRequest(IRequest request)
        {

            if (request.Message is ManagementCommand)
            {
                //CommandBase commandbase = (CommandBase)request.Message;
                //ManagementCommand command = (ManagementCommand)commandbase.command;

                ManagementCommand command = request.Message as ManagementCommand;

                if (command == null)
                    return null;

                ManagementResponse response = new ManagementResponse();
                response.MethodName = command.MethodName;
                response.Version = command.CommandVersion;
                response.RequestId = command.RequestId;

                try
                {
                    if (String.Compare(command.MethodName, ConfigurationCommandUtil.MethodName.MarkSessionInternal, true) == 0)
                    {
                        _internalSession = true;
                    }
                    if (!_internalSession)
                    {

                        //if (!EditionInfo.IsRemoteClient)
                        //{

                        if (configServer.IsPassive && !s_safeList.ContainsKey(command.MethodName))
                        {
                            if (LoggerManager.Instance.CONDBLogger != null &&
                                LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("OnRequest",
                                    command.MethodName +
                                    " ->serving operations while config server is running as secondary");

                            throw new System.Exception("Configuration server is currently running in passive mode");

                            //}

                        }


                    }

                    response.ResponseMessage = ConfigurationProvider.ManagementRpcService.InvokeMethodOnTarget(command.MethodName,
                            command.Overload,
                            command.Parameters.ParameterList.ToArray());
                }
                catch (DatabaseException e)
                {
                    response.Exception = new ManagementException(e.ErrorCode, e.Parameters, e.Message);
                }
                catch (System.Exception e)
                {
                    response.Exception = new ManagementException(e.Message, e);
                }


                return response;

            }
            else if (request.Message is ReplicationArgs)
            {
                ReplicationArgs replicateMsg = request.Message as ReplicationArgs;
                ManagementCommand command = replicateMsg.Command;

                ManagementResponse response = new ManagementResponse();

                if (command != null)
                {
                    response.MethodName = command.MethodName;
                    response.Version = command.CommandVersion;
                    response.RequestId = command.RequestId;
                    byte[] arguments = CompactBinaryFormatter.ToByteBuffer(command.Parameters, null);

                    try
                    {
                        response.ReturnVal = CompactBinaryFormatter.ToByteBuffer(ConfigurationProvider.ManagementRpcService.InvokeMethodOnTarget(command.MethodName,
                                command.Overload,
                                GetTargetMethodParameters(arguments)), null);
                    }
                    catch (System.Exception ex)
                    {
                        response.Exception = ex;
                    }
                }


                return response;
            }

            else
                return null;
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            if (configServer != null && channel.PeerAddress != null)
            {
                configServer.SecurityManager.OnChannelDisconnected(this.SessionId);
                configServer.RemoveClientChannel(channel.PeerAddress);
                configServer.RemoveNodeChannel(channel.PeerAddress);
                if (Parent != null)
                {
                    Parent.OnSessionDisconnected(this);
                }
            }
        }
        #endregion

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDatabaseClusterInfo, 1)]
        public ClusterInfo GetDatabaseClusterInfo(string cluster)
        {
            ClusterInfo clusterInfo = new ClusterInfo();
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                    clusterInfo = configServer.GetDatabaseClusterInfo(cluster);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return clusterInfo;
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.StartCluster,1)]
        public void StartCluster(string cluster)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Start_Cluster, cluster, out resourceId, out superResourceId, null);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Start_Cluster))
                {
                    this.configServer.StartCluster(cluster);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.StopCluster, 1)]
        public void StopCluster(string cluster)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Stop_Cluster, cluster, out resourceId, out superResourceId, null);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Stop_Cluster))
                {
                    this.configServer.StopCluster(cluster);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.StartShard, 1)]
        public bool StartShard(string cluster, string shard)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Start_Cluster, cluster, out resourceId, out superResourceId, null);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Start_Cluster))
                {
                    configServer.VerifyValidClusterOpperation(cluster);
                    return this.configServer.StartShard(cluster,shard);
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.StopShard, 1)]
        public bool StopShard(string cluster, string shard)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Stop_Cluster, cluster, out resourceId, out superResourceId, null);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Stop_Cluster))
                {
                    configServer.VerifyValidClusterOpperation(cluster);
                    return this.configServer.StopShard(cluster, shard);
                    
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetCollectionDistribution, 1)]
        public IDistribution GetCollectionDistribution(string cluster, string database, string collection)
        {
            try
            {
                return this.configServer.GetCurrentDistribution(cluster, database, collection);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.AddConfigurationListener, 1)]
        public void AddConfigurationListener(IConfigurationListener listener)
        {
            try
            {
                configServer.UpdateClientChannel((DualChannel)_channel);
            }
            catch(System.Exception ex)
            {
            }

        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveConfigurationListener, 1)]
        public void RemoveConfigurationListener(IConfigurationListener listener)
        {
            try
            {
                configServer.RemoveClientChannel((DualChannel)_channel);
            }
            catch (System.Exception ex)
            {
            }
        }

        private void NotifyConfigurationChange(string cluster, string shardName, ChangeType eventType, object obj)
        {
            
            ConfigChangeEventArgs args = new ConfigChangeEventArgs(cluster, shardName, eventType);

            switch (eventType)
            {
                case ChangeType.NodeJoined:
                case ChangeType.NodeLeft:
                case ChangeType.PrimaryGone:
                case ChangeType.PrimarySelected:
                    args.SetParamValue(EventParamName.Membership, obj as Membership);
                    //args.Membership = obj as Membership;
                    break;
                case ChangeType.NewRangeAdded:
                case ChangeType.RangeUpdated:
                case ChangeType.CollectionCreated:
                case ChangeType.CollectionDropped:
                case ChangeType.CollectionMoved:
                case ChangeType.DistributionChanged:
                {
                    var array = obj as string[];
                    if (array != null)
                    {
                        args.SetParamValue(EventParamName.DatabaseName, array[0]);
                        args.SetParamValue(EventParamName.CollectionName, array[1]);
                        //args.DatabaseName = array[0];
                        //args.CollectionName = array[1];
                    }
                    break;
                }
                case ChangeType.DatabaseCreated:
                case ChangeType.DatabaseDropped:
                {
                    var databaseName = obj as string;
                    args.SetParamValue(EventParamName.DatabaseName, databaseName);
                    //args.DatabaseName = databaseName;
                    break;
                }
                case ChangeType.ModeChange:
               {
                    var array = obj as object[];
                   if (array != null)
                   {
                       var databaseName = array[0] as string ;
                       var mode = array[1] is DatabaseMode ? (DatabaseMode) array[1] : DatabaseMode.Online;
                       args.SetParamValue(EventParamName.DatabaseName, databaseName);
                       args.SetParamValue(EventParamName.DatabaseMode, mode);
                   }
                   //args.DatabaseName = databaseName;
                    break;
                }
                case ChangeType.ConfigServerAdded:
                case ChangeType.ConfigServerRemoved:
                case ChangeType.ConfigServerDemoted:
               args.SetParamValue(EventParamName.ConfigServer, obj);
               break;

            }

            if (configServer != null)
                configServer.SendNotification(args);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.SetNodeStatus, 1)]
        public void SetNodeStatus(string cluster, string shard, ServerNode primary, NodeRole status)
        {
            try
            {

                Membership membership = this.configServer.SetNodeStatus(cluster, shard, primary, status);
                ChangeType changeType = status == NodeRole.None ? ChangeType.PrimaryGone : ChangeType.MembershipChanged;
                NotifyConfigurationChange(cluster, shard, changeType, membership);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 1)]
        public Membership[] GetMembershipInfo(string cluster)
        {
            try
            {
                DeploymentConfiguration deploymentConf = configServer.GetDatabaseClusterConfiguration(cluster).Deployment;
                //List<ShardConfiguration> shardList = configServer.GetDatabaseClusterConfiguration(cluster).Deployment.Shards.ToList<ShardConfiguration>();
                if (deploymentConf.Shards.Count > 0)
                {
                    Membership[] allMemberShip = new Membership[deploymentConf.Shards.Count];
                    int index = 0;
                    foreach (ShardConfiguration sc in deploymentConf.Shards.Values)
                    {
                        allMemberShip[index] = this.configServer.GetMembershipInfo(cluster, sc.Name);
                        index++;
                    }

                    return allMemberShip;
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }




        [TargetMethod(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 2)]
        public Membership GetMembershipInfo(string cluster, string shard)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, cluster, out resourceId, out superResourceId, cluster, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                    return this.configServer.GetMembershipInfo(cluster, shard);
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateNodeStatus, 1)]
        public void UpdateNodeStatus(string cluster, string shard, ServerNode server, Status status)
        {
            try
            {
                Membership membership = this.configServer.UpdateNodeStatus(cluster, shard, server, status);
                if (status == Status.Running)
                    NotifyConfigurationChange(cluster, shard, ChangeType.NodeJoined, membership);
                if (status == Status.Stopped)
                    NotifyConfigurationChange(cluster, shard, ChangeType.NodeLeft, membership);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        //[TargetMethod(ConfigurationCommandUtil.MethodName.ReportLastOperationTime, 1)]
        //public void ReportLastOperationTime(long operationId, string cluster, string shard, ServerInfo serverInfo)
        //{
        //    try
        //    {
        //        this.configServer.ReportLastOperationTime(operationId, cluster, shard, serverInfo);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [TargetMethod(ConfigurationCommandUtil.MethodName.BeginElection, 1)]
        public Object BeginElection(string cluster, string shard, ServerNode server, ElectionType electionType)
        {
            try
            {
                return this.configServer.BeginElection(cluster, shard, server, electionType);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.SubmitElectionResult, 1)]
        public void SubmitElectionResult(string cluster, string shard, ElectionResult result)
        {
            try
            {
                int selected = this.configServer.SubmitElectionResult(cluster, shard, result);

                if (selected == 1 && result.PollingResult == Alachisoft.NosDB.Core.Configuration.Services.ElectionResult.Result.PrimarySelected)
                    NotifyConfigurationChange(cluster, shard, ChangeType.PrimarySelected, this.configServer.GetMembershipInfo(cluster, shard));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }



        [TargetMethod(ConfigurationCommandUtil.MethodName.EndElection, 1)]
        public void EndElection(string cluster, string shard, ElectionId electionId)
        {
            try
            {
                this.configServer.EndElection(cluster, shard, electionId);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
      

       
       

      

        [TargetMethod(ConfigurationCommandUtil.MethodName.ConfigureDistributionStategy, 1)]
        public void ConfigureDistributionStategy(string cluster, string database, string collection, IDistributionStrategy strategy)
        {
            try
            {
                this.configServer.ConfigureDistributionStategy(cluster, database, collection, strategy,true);
                var data = new string[2];
                data[0] = database;
                data[1] = collection;
                NotifyConfigurationChange(cluster, null, ChangeType.DistributionStrategyConfigured, data);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDistriubtionStrategy, 1)]
        public IDistributionStrategy GetDistriubtionStrategy(string cluster, string database, string collection)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                    return this.configServer.GetDistriubtionStrategy(cluster, database, collection);
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetCurrentDistribution, 1)]
        public IDistribution GetCurrentDistribution(string cluster, string database, string collection)
        {
            try
            {
                return this.configServer.GetCurrentDistribution(cluster, database, collection);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.BalanceData, 1)]
        public IDistribution BalanceData(string cluster, string database, string collection)
        {
            try
            {
                IDistribution distribution = this.configServer.BalanceData(cluster, database, collection);
                var data = new string[2];
                data[0] = database;
                data[1] = collection;
                NotifyConfigurationChange(cluster, null, ChangeType.DistributionChanged, data);
                return distribution;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateDatabase, 1)]
        public void CreateDatabase(string cluster, DatabaseConfiguration configuration)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Database, configuration.Name, out resourceId, out superResourceId, cluster, cluster);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Create_Database))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                this.configServer.CreateDatabase(cluster, configuration, _sessionid);
                NotifyConfigurationChange(cluster, null, ChangeType.DatabaseCreated, configuration.Name);
                    configServer.AddSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, superResourceId, cluster, this._sessionid);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropDatabase, 1)]
        public void DropDatabase(string cluster, string database,bool dropFiles)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Database, database, out resourceId, out superResourceId, cluster, cluster);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Delete_Database))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                this.configServer.RecoveryOperationExecting(cluster, database);
                    this.configServer.DropDatabase(cluster, database, dropFiles);
                NotifyConfigurationChange(cluster, null, ChangeType.DatabaseDropped, database);
                    configServer.RemoveSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, resourceId, superResourceId, cluster, this._sessionid);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateCollection, 1)]
        public void CreateCollection(string cluster, string database, CollectionConfiguration configuration)
        {
            //Temporary check for PowerShell
            if (string.IsNullOrEmpty(database))
                throw new Exception("Database cluster is not specified.");
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Collection, configuration.CollectionName, out resourceId, out superResourceId, cluster, database);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Create_Collection))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                this.configServer.RecoveryOperationExecting(cluster, database);
                this.configServer.CreateCollection(cluster, database, configuration);
                var data = new string[2];
                data[0] = database;
                data[1] = configuration.CollectionName;
                NotifyConfigurationChange(cluster, null, ChangeType.CollectionCreated, data);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetShardConfiguration, 1)]
        public ShardConfiguration GetShardConfiguration(string cluster, string shard)
        {
            try
            {
                return configServer.GetShardConfiguration(cluster, shard);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.MoveCollection, 1)]
        public void MoveCollection(string cluster, string database, string collection, string newShard)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Collection, collection, out resourceId, out superResourceId, cluster, database);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Create_Collection))
                {
                    this.configServer.VerifyValidDatabaseOperation(cluster);
                    this.configServer.MoveCollection(cluster, database, collection, newShard);
                    var data = new string[2];
                    data[0] = database;
                    data[1] = collection;

                    NotifyConfigurationChange(cluster, null, ChangeType.CollectionMoved, data);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropCollection, 1)]
        public void DropCollection(string cluster, string database, string collection)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Collection, collection, out resourceId, out superResourceId, cluster, database);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Delete_Collection))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                this.configServer.RecoveryOperationExecting(cluster, database);
                this.configServer.DropCollection(cluster, database, collection);
                var data = new string[2];
                data[0] = database;
                data[1] = collection;
                NotifyConfigurationChange(cluster, null, ChangeType.CollectionDropped, data);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateMembership, 1)]
        public void UpdateMembership(string cluster, string shard, Membership membership)
        {
            try
            {
                this.configServer.UpdateMembership(cluster, shard, membership);
                NotifyConfigurationChange(cluster, shard, ChangeType.MembershipChanged, membership);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ReportNodeJoining, 1)]
        public Membership ReportNodeJoining(string cluster, string shard, ServerNode joiningServer)
        {
            try
            {
                Membership membership = this.configServer.ReportingNodeJoining(cluster, shard, joiningServer);
                NotifyConfigurationChange(cluster, shard, ChangeType.NodeJoined, membership);
                AppUtil.LogEvent(AppUtil.EventLogSource, string.Format("Node {0} has joined shard \"{1}\"", joiningServer.Name, shard),
                    EventLogEntryType.Information, EventCategories.Information, EventID.NodeJoined);
                return membership;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ReportNodeLeaving, 1)]
        public Membership ReportNodeLeaving(string cluster, string shard, ServerNode leavingServer)
        {
            try
            {
                Membership membership = this.configServer.ReportingNodeLeft(cluster, shard, leavingServer);
                NotifyConfigurationChange(cluster, shard, ChangeType.NodeLeft, membership);
                AppUtil.LogEvent(AppUtil.EventLogSource, string.Format("Node {0} has left shard \"{1}\"}", leavingServer.Name, shard),
                    EventLogEntryType.Warning, EventCategories.Warning, EventID.NodeLeft);
                return membership;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ReportHeartbeat, 1)]
        public int ReportHeartbeat(string cluster, string shard, ServerNode reportingServer, Membership membership, OperationId lastOpId)
        {
            try
            {
                return this.configServer.ReportingHeartBeat(cluster, shard, reportingServer, membership, lastOpId);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateIndex, 1)]
        public void CreateIndex(string cluster, string database, string collection, IndexConfiguration configuration)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Index, configuration.IndexName, out resourceId, out superResourceId, cluster, database);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Create_Index))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                this.configServer.RecoveryOperationExecting(cluster, database);
                configServer.CreateIndex(cluster, database, collection, configuration);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropIndex, 1)]
        public void DropIndex(string cluster, string database, string collection, string indexName)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Index, indexName, out resourceId, out superResourceId, cluster, database);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Delete_Index))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                this.configServer.RecoveryOperationExecting(cluster, database);
                configServer.DropIndex(cluster, database, collection, indexName);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateCollectionStatistics, 1)]
        public void UpdateCollectionStatistics(string cluster, string database, string collection, CollectionStatistics statistics)
        {
            try
            {
                configServer.UpdateCollectionStatistics(cluster, database, collection, statistics);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateBucketStatistics, 1)]
        public void UpdateBucketStatistics(string cluster, string database, string collection, Common.Stats.ShardInfo shardInfo)
        {
            try
            {
                configServer.UpdateBucketStatistics(cluster, database, collection, shardInfo);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterUID, 1)]
        public bool VerifyConfigurationClusterUID(string UID)
        {
            try
            {
                return configServer.VerifyConfigurationClusterUID(UID);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ValidateProfessional,1)]
        public byte[] ValidateProfessional(byte[] token)
        {
            return EncryptionUtil.ValidateManagementToken(token);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateConfigurationServer, 1)]
        public void CreateConfigurationCluster(ConfigServerConfiguration serverConfig, int heartBeat, ReplicationConfiguration replConfig, string displayName)
        {
            try 
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Configuration_Cluster, serverConfig.Name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Create_Configuration_Cluster))
                {
                    if (configServer.VerifyConfigurationClusterAvailability(serverConfig.Name))
					{
                        configServer.CreateConfigurationCluster(serverConfig, heartBeat, replConfig, displayName);


                    //Adding Cluster resource in Security Manager, being done here because of single cluster creation at the time of configuration cluster creation

                    ResourceId clusterResourceId;
                    ResourceId clusterSuperResourceId;
                    Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Cluster, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, out clusterResourceId, out clusterSuperResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                    configServer.AddSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, clusterResourceId, clusterSuperResourceId, MiscUtil.CLUSTERED, this._sessionid);
					}

                }

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.StartConfigurationServer, 1)]
        public void StartConfigurationServer(string name)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Start_Configuration_Cluster, name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Start_Configuration_Cluster))
                {
                    configServer.StartConfigurationServer(name);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.StopConfigurationServer, 1)]
        public void StopConfigurationServer(string name)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Stop_Configuration_Cluster, name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Stop_Configuration_Cluster))
                {
                    configServer.StopConfigurationServer(name);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.AddNodeToConfigurationCluster, 1)]
        public void AddNodeToConfigurationCluster(string cluster, ServerNode node)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Modify_Configuration_Cluster, cluster, out resourceId, out superResourceId, cluster, MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Modify_Configuration_Cluster))
                {
                    configServer.AddNodeToConfigurationCluster(cluster, node);
                    NotifyConfigurationChange(cluster, null, ChangeType.ConfigServerAdded, node);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveNodeFromConfigurationCluster, 1)]
        public void RemoveNodeFromConfigurationCluster(string cluster, ServerNode node)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Modify_Configuration_Cluster, cluster, out resourceId, out superResourceId, cluster, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Modify_Configuration_Cluster))
                {
                    configServer.RemoveNodeFromConfigurationCluster(cluster, node);
                    NotifyConfigurationChange(cluster, null, ChangeType.ConfigServerRemoved, node);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.VerifyConfigurationServerAvailability, 1)]
        public bool VerifyConfigurationServerAvailability()
        {
             return configServer.VerifyConfigurationServerAvailability();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.VerifyConfigurationCluster, 1)]
        public bool VerifyConfigurationCluster(string configClusterName)
        {
            try
            {
                return this.configServer.VerifyConfigurationCluster(configClusterName);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterPrimery, 1)]
        public bool VerifyConfigurationClusterPrimery(string configClusterName)
        {
            try
            {
                return this.configServer.VerifyConfigurationClusterPrimery(configClusterName);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ListDatabases, 1)]
        public string[] ListDatabases(string cluster)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                return this.configServer.ListDatabases(cluster);
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ListCollections, 1)]
        public string[] ListCollections(string cluster, string database)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                return this.configServer.ListCollections(cluster, database);
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.ListIndices, 1)]
        public string[] ListIndices(string cluster, string database, string collection)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Database_Cluster_Configuration, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Database_Cluster_Configuration))
                {
                this.configServer.VerifyValidDatabaseOperation(cluster);
                return this.configServer.ListIndices(cluster, database, collection);
                }
                else return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterAvailability, 1)]
        public bool VerifyConfigurationClusterAvailability(string configClusterName)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Read_Configuration_Cluster_Configuration, configClusterName, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Read_Configuration_Cluster_Configuration))
                {
                    return this.configServer.VerifyConfigurationClusterAvailability(configClusterName);
                }
                else return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

		[TargetMethod(ConfigurationCommandUtil.MethodName.StartNode,1)]
        public bool StartNode(string cluster, string shard, ServerNode server)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Start_Cluster, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, null);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Start_Cluster))
                {
                configServer.VerifyValidClusterOpperation(cluster);
                bool isShardStarted = configServer.StartNode(cluster, shard, server);

                return isShardStarted;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.StopNode, 1)]
        public bool StopNode(string cluster, string shard, ServerNode server)
        {
            try
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Stop_Cluster, cluster, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, null);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Stop_Cluster))
                {
                    configServer.VerifyValidClusterOpperation(cluster);
                    bool isShardStopped = configServer.StopNode(cluster, shard, server);
                    return isShardStopped;
				}
                return false;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfigurationClusterConfiguration,1)]
        public ConfigServerConfiguration GetConfigurationClusterConfiguration(string configCluster)
        {
            try
            {

                //if (!EditionInfo.IsRemoteClient)
                    //return configServer.GetConfigurationClusterConfiguration(configCluster);
                //else
                //    return null;

                return configServer.GetConfigurationClusterConfiguration(configCluster);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        internal object MessageReceived(ReplicationArgs replicateMsg)
        {
            ManagementCommand command = replicateMsg.Command;
            ManagementResponse response = new ManagementResponse();
            
            if (command != null)
            {
                response.MethodName = command.MethodName;
                response.Version = command.CommandVersion;
                response.RequestId = command.RequestId;
                byte[] arguments = CompactBinaryFormatter.ToByteBuffer(command.Parameters, null);

                try
                {
                    response.ReturnVal = CompactBinaryFormatter.ToByteBuffer(ConfigurationProvider.ManagementRpcService.InvokeMethodOnTarget(command.MethodName,
                            command.Overload,
                            GetTargetMethodParameters(arguments)),null);
                }
                catch (System.Exception ex)
                {
                    response.Exception = ex;
                }

            }

            if (replicateMsg.Type == ReplicationType.Configuration)
            {
                configServer.ReplicateClusterConfiguration(replicateMsg.Configuration);
            }

            else if (replicateMsg.Type == ReplicationType.Metadata)
            {
                configServer.ReplicateMetaInfo(replicateMsg.Metadata);
            }

            else if (replicateMsg.Type == ReplicationType.ConfigurationAndMetadata)
            {
                configServer.ReplicateClusterConfiguration(replicateMsg.Configuration);
                configServer.ReplicateMetaInfo(replicateMsg.Metadata);
            }

            return response;
        }



        #region State Transfer Operations

        [TargetMethod(ConfigurationCommandUtil.MethodName.StateTransferOperation, 1)]

        public Object StateTransferOperation(String clusterName, IStateTransferOperation operation)
        {
            object returnValue = configServer.StateTransferOperation(clusterName, operation);
            switch (operation.OpCode)
            {
                case StateTransferOpCode.FinalizeStateTransfer:
                    var data = new string[2];
                    data[0] = operation.TaskIdentity.DBName;
                    data[1] = operation.TaskIdentity.ColName;
                    NotifyConfigurationChange(clusterName, null, ChangeType.DistributionChanged, data);
                    break;
            }
            return returnValue;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.HasSynchronizedWIthPrimaryServer, 1)]
        public bool HasSynchronizedWithPrimaryConfigServer()
        {
            return configServer.HasSynchronizedWithPrimaryConfigServer();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfClusterServers, 1)]
        public List<Address> GetConfServers(string cluster)
        {
            return configServer.GetConfClusterServers(cluster);
        }

        public System.Collections.ArrayList GetCollectionBucketsMap(string p1, string p2, string colName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Recovery Operations
         
        [TargetMethod(ConfigurationCommandUtil.MethodName.SubmitRecoveryJob, 1)]
        public Common.Recovery.RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Backup_Database, config.Cluster, out resourceId, out superResourceId, config.Cluster);
            if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Backup_Database))
            {
                return configServer.SubmitRecoveryJob(config);
            }
            return null;
        }
         
        [TargetMethod(ConfigurationCommandUtil.MethodName.CancelRecoveryJob, 1)]
        public Common.Recovery.RecoveryOperationStatus CancelRecoveryJob(string identifier)
        {
            RecoveryConfiguration config = configServer.GetJobConfiguration(identifier);
            ResourceId resourceId;
            ResourceId superResourceId;
            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Backup_Database, config.Cluster, out resourceId, out superResourceId, config.Cluster);
            if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Backup_Database))
            {
                return configServer.CancelRecoveryJob(identifier);
            }
            return null;
        }
         
        [TargetMethod(ConfigurationCommandUtil.MethodName.CancelAllRecoveryJobs, 1)]
        public Common.Recovery.RecoveryOperationStatus[] CancelAllRecoveryJobs()
        {
             return configServer.CancelAllRecoveryJobs();
        }
         
        [TargetMethod(ConfigurationCommandUtil.MethodName.GetJobState, 1)]
        public Common.Recovery.ClusteredRecoveryJobState GetJobState(string identifier)
        {
            return configServer.GetJobState(identifier);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.SubmitJobState, 1)]
        public void SubmitShardJobStatus(ShardRecoveryJobState status)
        { configServer.SubmitRecoveryState(status); }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetAllRunningJobs, 1)]
        public ClusterJobInfoObject[] GetAllRunningJobs()
        {
            return configServer.GetAllRunningJobs();
        }

        #endregion

        public object SubmitPollingCommand(PollingOperation command)
        {
            throw new NotImplementedException();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveConfigurationCluster,1)]
        public void RemoveConfigurationCluster(string configClusterName)
        {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Configuration_Cluster, configClusterName, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Delete_Configuration_Cluster))
                {
                    configServer.RemoveConfigurationCluster(configClusterName);

                    ResourceId clusterResourceId;
                    ResourceId clusterSuperResourceId;
                    Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Cluster, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, out clusterResourceId, out clusterSuperResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER);
                    configServer.RemoveSecurityInformation(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, clusterResourceId, clusterSuperResourceId, MiscUtil.CLUSTERED, this._sessionid);
                }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.Grant, 1)]
        public bool Grant(string clusterName, ResourceId resourceID, string userName, string roleName)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Grant_Role, resourceID.Name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, clusterName);
            resourceId.ResourceType = resourceID.ResourceType;
            superResourceId = resourceId;
            if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Grant_Role))
            {
                return configServer.GrantRole(clusterName, resourceID, userName, roleName);
            }
            return false;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.Revoke, 1)]
        public bool Revoke(string clusterName, ResourceId resourceID, string userName, string roleName)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Revoke_Role, resourceID.Name, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED, clusterName);
            resourceId.ResourceType = resourceID.ResourceType;
            superResourceId = resourceId;
            if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Revoke_Role))
            {
                return configServer.RevokeRole(clusterName, resourceID, userName, roleName);
            }
            return false;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateUser, 1)]
        public bool CreateUser(IUser userInfo)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_User, MiscUtil.NOSDB_CLUSTER_SERVER, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED);
            if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Create_User))
            {
                return configServer.CreateUser(userInfo);
            }
            return false;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropUser, 1)]
        public bool DropUser(IUser userInfo)
        {
            ResourceId resourceId;
            ResourceId superResourceId;
            Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_User, MiscUtil.NOSDB_CLUSTER_SERVER, out resourceId, out superResourceId, Alachisoft.NosDB.Common.MiscUtil.CLUSTERED);
            if (_securityManager.Authorize(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, this._sessionid, resourceId, superResourceId, Permission.Delete_User))
            {
                return configServer.DropUser(userInfo);
            }
            return false;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateRole, 1)]
        public void CreateRole(IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.AlterRole, 1)]
        public void AlterRole(IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropRole, 1)]
        public void DropRole(IRole roleInfo)
        {
            throw new NotImplementedException();
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUsersInformation, 1)]
        public IList<IUser> GetUsersInformation()
        {
            return _securityManager.Users(MiscUtil.CONFIGURATION_SHARD_NAME);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetResourcesInformation, 1)]
        public IList<IResourceItem> GetResourcesInformation(string cluster)
        {
            ResourceId resourceId = new ResourceId() { Name = cluster, ResourceType = ResourceType.Cluster };
            return _securityManager.GetSubResources(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, resourceId);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateDatabaseConfiguration, 1)]
        public void UpdateDatabaseConfiguration(string cluster, string database, DatabaseConfiguration databaseConfiguration)
        {
            try
            {
                configServer.UpdateDatabaseConfiguration(cluster, database, databaseConfiguration);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateCSNodePriority, 1)]
        public void UpdateConfigServerNodePriority(string cluster, string nodeName, int priority)
        {
            try
            {
                configServer.UpdateConfigServerNodePriority(cluster, nodeName, priority);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateServerPriority, 1)]
        public void UpdateServerPriority(string cluster, string shard, ServerNode server, int priority)
        {
            try
            {
                configServer.UpdateServerPriority(cluster, shard, server, priority);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateDeploymentConfiguration, 1)]
        public void UpdateDeploymentConfiguration(string cluster, int heartBeatInterval)
        {
            try
            {
                configServer.UpdateDeploymentConfiguration(cluster, heartBeatInterval);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateIndexAttribute, 1)]
        public void UpdateIndexAttribute(string cluster, string database, string collection, string index, IndexAttribute attributes)
        {
            try
            {
                configServer.UpdateIndexAttribute(cluster, database, collection, index, attributes);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateShardConfiguration, 1)]
        public void UpdateShardConfiguration(string cluster, string shard, int heartbeat, int port)
        {
            try
            {
                configServer.UpdateShardConfiguration(cluster, shard, heartbeat, port);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
		}
		
        [TargetMethod(ConfigurationCommandUtil.MethodName.GetAuthenticatedUserInfoFromConfigServer, 1)]
        public IUser GetAuthenticatedUserInfoFromConfigServer(ISessionId sessionId)
        {
            return _securityManager.GetAuthenticatedUserInfo(sessionId);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetResourceSecurityInfo, 1)]
        public IResourceItem GetResourceSecurityInfo(string cluster, ResourceId resourceId)
        {
            return _securityManager.GetResource(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, cluster, resourceId);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUserInfo, 1)]
        public IDictionary<IRole, IList<ResourceId>> GetUserInfo(IUser userInfo)
        {
            return _securityManager.GetUserInfo(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME, userInfo);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.UpdateCollectionConfiguration, 1)]
        public void UpdateCollectionConfiguration(string cluster, string database, string collection, CollectionConfiguration collectionConfiguration)
        {
            try
            {
                configServer.UpdateCollectionConfiguration(cluster, database, collection, collectionConfiguration);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }



        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDataBaseServerNode, 1)]
        public List<Address> GetDataBaseServerNode()
        {
            return configServer.GetDataBaseServerNode();
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.SetDatabaseMode, 1)]
        public bool SetDatabaseMode(string cluster, string databaseName, DatabaseMode databaseMode)
        {
            if (configServer.SetDatabaseMode(cluster, databaseName, databaseMode))
            {
                object[] argValue = {databaseName, databaseMode};
                NotifyConfigurationChange(cluster, null, ChangeType.ModeChange, argValue);
                return true;
            }
            return false;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetImplementation, 1)]
        public IDictionary<string, byte[]> GetDeploymentSet(string implIdentifier)
        {
            try
            {
                return configServer.GetImplementation(implIdentifier);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.MarkSessionInternal, 1)]
        public void MarkSessionInternal()
        {
            _internalSession = true;
        }


        [TargetMethod(ConfigurationCommandUtil.MethodName.CopyAssemblies, 1)]
        public void DeployAssemblies(string cluster, string deploymentId, string deploymentName, string assemblyFileName, byte[] buffer)
        {
            configServer.DeployAssemblies(cluster, deploymentId, deploymentName,assemblyFileName, buffer);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.IsRemoteClient, 1)]
        public bool IsRemoteClient()
        {
            return false;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.IsAuthorized, 1)]
        public bool IsAuthorized(ISessionId sessionId, ResourceId resourceId, ResourceId superResourceId, Permission operationPermission)
        {
            return this._securityManager.Authorize(MiscUtil.CONFIGURATION_SHARD_NAME, sessionId, resourceId, superResourceId, operationPermission);
        }


        [TargetMethod(ConfigurationCommandUtil.MethodName.MarkDatabaseSession, 1)]
        public void MarkDatabaseSession()
        {
            isDatabaseSession = true;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.MarkDistributorSession, 1)]
        public void MarkDistributorSession()
        {
            isDistributorSession = true;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.MarkConfiguritonSession, 1)]
        public void MarkConfigurationSession()
        {
            isConfigurationSession = true;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.IsNodeRunning, 1)]
        public bool IsNodeRunning(string node)
        {
            return configServer.IsNodeRunning(node);
        }
    }
}
