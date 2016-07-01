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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.SSPI;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;

namespace Alachisoft.NosDB.Core.DBEngine.Management
{
    public class RemoteManagementSession : IManagementSession, IDisposable
    {
        public RemoteManagementSession()
        { 
        }

        public void Initialize()
        {
            InitializeSecurityContext();
        }

        private DualChannel _channel;

        public DualChannel Channel
        {
            get { return _channel; }
        }

        public ISessionId SessionId { set; get; }

        public void Connect(string peerIP, int peerPort /*, string bindingIP*/)
        {
            try
            {
                _channel = new DualChannel(peerIP, peerPort, null, SessionTypes.Management, new TraceProvider(), new ConfigurationChannelFormatter());
                _channel.Connect(true);

                Initialize();
            }
            catch (ChannelException ex)
            {
                //RTD: Replace shardLogger with the respective module logger name
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: RemoteMgmtSession.Connect()", ex.ToString());
                throw ex;
            }
        }

        protected object ExecuteCommandOnMgtServer(object message, bool Response)
        {

            ManagementResponse managementResponse = null;
            if (_channel != null)
            {
                try
                {
                    managementResponse = _channel.SendMessage(message, !Response) as ManagementResponse;

                }
                catch (System.Exception e)
                {
                    throw new System.Exception(e.Message, e);
                }
                if (managementResponse != null && managementResponse.Exception != null)
                {
                    throw new System.Exception(managementResponse.Exception.Message);
                }
            }
            if (managementResponse != null)
                return managementResponse.ResponseMessage;

            return null;
        }




        public bool AddServerToShard(string configCluster, string clusterUID, Address[] configServers, string databaseCluster, string shard, string shardUid, int shardPort, bool start, Common.Configuration.ClusterConfiguration clusterConfig)
        {
            try
            {
                ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.AddServerToShard);
                command.Parameters.AddParameter(configCluster);
                command.Parameters.AddParameter(clusterUID);
                command.Parameters.AddParameter(configServers);
                command.Parameters.AddParameter(databaseCluster);
                command.Parameters.AddParameter(shard);
                command.Parameters.AddParameter(shardUid);
                command.Parameters.AddParameter(shardPort);
                command.Parameters.AddParameter(start);
                command.Parameters.AddParameter(clusterConfig);
                return (bool)ExecuteCommandOnMgtServer(command, true);
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public bool StartNode(string cluster, string shard, int serverPort)
        {
            try
            {
                ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StartNode);
                command.Parameters.AddParameter(cluster);
                command.Parameters.AddParameter(shard);
                command.Parameters.AddParameter(serverPort);
                return (bool)ExecuteCommandOnMgtServer(command, true);

            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public bool StopNode(string cluster, string shard)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StopNode);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            return (bool)ExecuteCommandOnMgtServer(command, true);

        }

        public bool StopNodeForClients(string cluster, string shard)
        {
            throw new NotImplementedException();
        }

        public bool RemoveServerFromShard(string cluster, string shard)
        {
            try
            {
                ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RemoveServerFromShard);
                command.Parameters.AddParameter(cluster);
                command.Parameters.AddParameter(shard);
                return (bool)ExecuteCommandOnMgtServer(command, true);
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public bool CreateDatabase(string cluster, string shard, DatabaseConfiguration configuration)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateDatabase);
            command.Parameters.AddParameter(cluster = cluster != null ? cluster.ToLower() : "");
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(configuration);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool CreateDatabase(string cluster, string shard, DatabaseConfiguration configuration,IDictionary<string,IDistributionStrategy> collectionStrategy)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateDatabase);
            command.Parameters.AddParameter(cluster = cluster != null ? cluster.ToLower() : "");
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(configuration);
            command.Parameters.AddParameter(collectionStrategy);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool DropDatabase(string cluster, string shard, string database, bool dropFiles)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropDatabase);
            command.Parameters.AddParameter(cluster = cluster != null ? cluster.ToLower() : "");
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(dropFiles);
            return (bool) ExecuteCommandOnMgtServer(command, true);
        }

        public List<Address> GetConfClusterServers(string cluster)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetConfClusterServers);
            command.Parameters.AddParameter(cluster);
            return ExecuteCommandOnMgtServer(command, true) as List<Address>;
        }

        public void NodeAddedToConfigurationCluster(string cluster, ServerNode node)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.NodeAddedToConfigurationCluster);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(node);
            ExecuteCommandOnMgtServer(command, true);
        }

        public bool CreateCollection(string cluster, string shard, string database, CollectionConfiguration configuration)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateCollection);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(configuration);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool CreateCollection(string cluster, string shard, string database, CollectionConfiguration configuration, IDistributionStrategy distribution)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateCollection);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(configuration);
            command.Parameters.AddParameter(distribution);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool DropCollection(string cluster, string shard, string database, string collection)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropCollection);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool DropIndex(string cluster, string shard, string database, string collection, string indexName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropIndex);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(indexName);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool CreateIndex(string cluster, string shard, string database, string collection, IndexConfiguration configuration)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateIndex);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard.ToLower());
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(configuration);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        private ManagementCommand GetManagementCommand(string method)
        {
            return GetManagementCommand(method, 1);
        }

        private ManagementCommand GetManagementCommand(string method, int overload)
        {
            ManagementCommand command = new ManagementCommand();
            command.MethodName = method;
            command.Overload = overload;
            return command;
        }
        public Common.Recovery.RecoveryOperationStatus SubmitDataRecoveryJob(string cluster, string shard, RecoveryOperation opContext)
        {
            try
            {
                ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SubmitRecoveryJob);
                command.Parameters.AddParameter(cluster);
                command.Parameters.AddParameter(shard);
                command.Parameters.AddParameter(opContext);
                return (Common.Recovery.RecoveryOperationStatus)ExecuteCommandOnMgtServer(command, true);

            }
            catch (System.Exception e)
            {
                throw e;
            }

        }


        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Disconnect();
            }
        }

        ~RemoteManagementSession()
        {
            Dispose();
        }



        public bool GrantRole(string cluster, string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser userInfo, Common.Security.Interfaces.IRole roleInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.Grant);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shardName);
            command.Parameters.AddParameter(resourceId);
            command.Parameters.AddParameter(userInfo);
            command.Parameters.AddParameter(roleInfo);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        public bool RevokeRole(string cluster, string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser userInfo, Common.Security.Interfaces.IRole roleInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.Revoke);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shardName);
            command.Parameters.AddParameter(resourceId);
            command.Parameters.AddParameter(userInfo);
            command.Parameters.AddParameter(roleInfo);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }

        #region Security

        private ClientContext _clientSecurityContext;
        private ClientCredential _clientSecurityCredential;

        public IServerAuthenticationCredential OpenConfigurationSession(IClientAuthenticationCredential clientCredentials)
        {
            return AuthenticateWindowsClient(clientCredentials as SSPIClientAuthenticationCredential);
        }

        public IServerAuthenticationCredential Authenticate(IClientAuthenticationCredential clientCredentials)
        {
            if (clientCredentials is SSPIClientAuthenticationCredential)
                return AuthenticateWindowsClient(clientCredentials as SSPIClientAuthenticationCredential);
            else return null;
        }

        private IServerAuthenticationCredential AuthenticateWindowsClient(SSPIClientAuthenticationCredential clientCredentials)
        {
            AuthToken clientAuthToken;
            if (clientCredentials == null || clientCredentials.Token == null)
            {
                clientCredentials = new SSPIClientAuthenticationCredential();
                clientAuthToken = new AuthToken();
            }
            else
                clientAuthToken = clientCredentials.Token;
            SSPIServerAuthenticationCredential serverAuthenticationCredential = new SSPIServerAuthenticationCredential();
            serverAuthenticationCredential.Token = new AuthToken();
            serverAuthenticationCredential.Token.Status = SecurityStatus.None;
            do
            {
                Byte[] clientToken = null;
                if (_clientSecurityContext == null)
                {
                    InitializeSecurityContext();
                }
                clientAuthToken.Status = this._clientSecurityContext.Init(serverAuthenticationCredential.Token.Token, out clientToken);
                clientAuthToken.Token = clientToken;
                clientCredentials.Token = clientAuthToken;

                if (clientAuthToken.Status == SecurityStatus.ContinueNeeded || (clientAuthToken.Status == SecurityStatus.OK && clientAuthToken.Token != null))
                {
                    ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.OpenConfigurationSession, 1);
                    command.Parameters.AddParameter(clientCredentials);
                    try
                    {
                        serverAuthenticationCredential = ExecuteCommandOnMgtServer(command, true) as SSPIServerAuthenticationCredential;
                    }
                    catch (SecurityException exc)
                    {
                        if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                            LoggerManager.Instance.StorageLogger.Error("Authenticating Database Server", exc.Message);
                    }
                }
                if (serverAuthenticationCredential.Token.Status == SecurityStatus.SecurityDisabled || (clientCredentials.Token.Status == SecurityStatus.OK && serverAuthenticationCredential == null))
                    break;
            } while (clientAuthToken.Status == SecurityStatus.ContinueNeeded);
            _clientSecurityContext = null;
            return serverAuthenticationCredential;
        }

        private void InitializeSecurityContext()
        {
            //+security context initialization
            string SPN;
            if (SSPIUtility.IsLocalServer(_channel.PeerAddress.IpAddress))
                SPN = null;
            else
            {
                try
                {
                    SPN = SSPIUtility.GetServicePrincipalName(MiscUtil.NOSDB_SPN, _channel.PeerAddress.IpAddress);
                    //SPN += (":" + _channel.PeerAddress.Port);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    SPN = null;
                }
            }

            this._clientSecurityCredential = SSPIUtility.GetClientCredentials(SPN);

            this._clientSecurityContext = SSPIUtility.GetClientContext(_clientSecurityCredential, SPN);
            //-security context initialization
        }

        public void PopulateSecurityInformationOnDBServer(string cluster, string shardName, IList<IResourceItem> resources)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.PopulateSecurityInformationOnDBServer);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shardName);
            command.Parameters.AddParameter(resources);
            ExecuteCommandOnMgtServer(command, false);
        }

        public void PublishAuthenticatedUserInfoToDBServer(string cluster, string shard, ISessionId sessionId, string username)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.PublishAuthenticatedUserInfoToDBServer);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(sessionId);
            command.Parameters.AddParameter(username);
            ExecuteCommandOnMgtServer(command, false);
        }

        public bool CreateUser(string cluster, string localShardName, IUser userInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateUser);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(localShardName);
            command.Parameters.AddParameter(userInfo);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }
        public bool DropUser(string cluster, string localShardName, IUser userInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropUser);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(localShardName);
            command.Parameters.AddParameter(userInfo);
            return (bool)ExecuteCommandOnMgtServer(command, true);
        }
        #endregion

        public string GetDatabaseCluster()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDatabaseCluster);
            return (string)ExecuteCommandOnMgtServer(command, true);

        }
        public string[] GetShards()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetShards);
            return (string[])ExecuteCommandOnMgtServer(command, true);
        }


        public bool CanAddToDatabaseCluster(string configurationCluster, string configurationUid, string databaseCluster, string shard, string shardUid)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CanAddToDatabaseCluster);
            command.Parameters.AddParameter(configurationCluster);
            command.Parameters.AddParameter(configurationUid);
            command.Parameters.AddParameter(databaseCluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(shardUid);
            return (bool)ExecuteCommandOnMgtServer(command, true);

        }

        public List<Address> GetDatabaseServerNodes()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDataBaseServerNode);
            return ExecuteCommandOnMgtServer(command, true) as List<Address>;
        }

        public bool SetDatabaseMode(string cluster, string shardName, string databaseName, DatabaseMode databaseMode)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SetDatabaseMode);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shardName);
            command.Parameters.AddParameter(databaseName);
            command.Parameters.AddParameter(databaseMode);
            return(bool) ExecuteCommandOnMgtServer(command, true); 
        }


        public void MarkConfigurationSession()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.MarkConfiguritonSession);
            ExecuteCommandOnMgtServer(command, false);
        }

        public void CreateLocalCluster()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateLocalCluster);
            ExecuteCommandOnMgtServer(command, false);
        }

        public void NodeRemovedFromConfigurationCluster(string cluster, ServerNode node)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.NodeRemovedFromConfigurationCluster);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(node);
            ExecuteCommandOnMgtServer(command, true);
        }
    }
}
