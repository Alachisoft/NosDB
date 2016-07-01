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
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.RPCFramework;
using Alachisoft.NosDB.Common.RPCFramework.DotNetRPC;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using CollectionConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;

namespace Alachisoft.NosDB.Core.DBEngine.Management
{
    public class ManagementSession : IRequestListener,IManagementSession
    {
        ManagementServer dbMgtServer;
        DateTime _sessionStartTime;
        ISessionId _sessionId;
        private Common.RPCFramework.RPCService<ManagementSession> _rpcService=null;
        private IDualChannel _channel;

        public IDualChannel Channel { get { return _channel; } set { _channel = value; } }

        private bool IsConfigSession = false;
        
        public ManagementSession(ManagementServer server, UserCredentials credentials)
        {
            this.dbMgtServer = server;
            this._sessionStartTime = DateTime.Now;
            _sessionId = new RouterSessionId();
            _sessionId.SessionId = Guid.NewGuid().ToString();
            _rpcService = new RPCService<ManagementSession>(new TargetObject<ManagementSession>(this));
            ManagementProvider.Provider = this;
        }

        public ISessionId SessionId { set { _sessionId = value; } get { return _sessionId; } }
        
        #region                      IRequestListener Methods
        
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
               
               // LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = "shard1", DatabaseName = "database" });
                try
                {
                    byte[] arguements = CompactBinaryFormatter.ToByteBuffer(command.Parameters, null);
                    response.ResponseMessage= ManagementProvider.ManagementRpcService.InvokeMethodOnTarget(command.MethodName,
                        command.Overload,
                        GetTargetMethodParameters(arguements)
                        );
                    _channel.GetType();
                }
                catch(System.Exception ex)
                {
                    response.Exception = ex;
                }
                return response;
            }
            else
                return null;
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            if(dbMgtServer!=null && channel.PeerAddress!=null)
            {
                dbMgtServer.OnChannelDisconnected(this._sessionId);
                dbMgtServer.RemoveConfigServerChannel(channel.PeerAddress);
            }
        }
        
        #endregion
        
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.StartNode, 1)]
        public bool StartNode(string cluster,string shard, int serverPort)
        {
            try
            {
                return dbMgtServer.StartNode(cluster,shard, serverPort,null);
                
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.StopNode,1)]
        public bool StopNode(string cluster,string shard)
        {
            try
            {
                 return  dbMgtServer.StopNode(cluster,shard);
                
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.StopNodeForClients, 1)]
        public bool StopNodeForClients(string cluster, string shard)
        {
            try
            {
                return dbMgtServer.StopNodeForClients();

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        
        [TargetMethod(ConfigurationCommandUtil.MethodName.AddServerToShard, 1)]
        public bool AddServerToShard(string configCluster, string clusterUID, Address[] configServers, string databaseCluster, string shard, string shardUid, int shardPort, bool start, Common.Configuration.ClusterConfiguration clusterConfig)
        {
            bool nodeAdded = false;
            try
            {
                nodeAdded = dbMgtServer.AddServerToShard(configCluster, clusterUID, configServers, databaseCluster, shard, shardUid, shardPort, start, clusterConfig);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return nodeAdded;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.RemoveServerFromShard, 1)]
        public bool RemoveServerFromShard(string cluster,string shard)
        {
            try
            {
                 this.dbMgtServer.RemoveServerFromShard(cluster,shard);
                 return true;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

               

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateDatabase, 1)]
        public bool CreateDatabase(string cluster, string shard, DatabaseConfiguration configuration,IDictionary<string,IDistributionStrategy> collectionStrategy)
        {
            try
            {
                bool isCreated = this.dbMgtServer.CreateDatabase(cluster, shard, configuration,collectionStrategy);
                if (isCreated)
                {
                    ResourceId resourceId;
                    ResourceId superResourceId;

                    Security.Impl.SecurityManager.GetSecurityInformation(Permission.Create_Database, configuration.Name, out resourceId, out superResourceId, cluster);

                    dbMgtServer.AddSecurityInformation(cluster, shard, resourceId, superResourceId, this._sessionId);
                }
                return isCreated;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropDatabase, 1)]
        public bool DropDatabase(string cluster, string shard, string database, bool dropFiles)
        {
            try
            {
                bool isDropped = this.dbMgtServer.DropDatabase(cluster, shard, database, dropFiles);

				if(isDropped)
				{
                ResourceId resourceId;
                ResourceId superResourceId;

                Security.Impl.SecurityManager.GetSecurityInformation(Permission.Delete_Database, database, out resourceId, out superResourceId, cluster);

                dbMgtServer.RemoveSecurityInformation(cluster, shard, resourceId, superResourceId);
				}
				return isDropped;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

       
         [TargetMethod(ConfigurationCommandUtil.MethodName.CreateCollection, 1)]
        public bool CreateCollection(string cluster, string shard, string database, CollectionConfiguration configuration, IDistributionStrategy distribution) 
        {
            try 
            {
                return this.dbMgtServer.CreateCollection(cluster, shard, database, configuration, distribution);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropCollection, 1)]
        public bool DropCollection(string cluster, string shard, string database, string collection)
        {
            try
            {
                return this.dbMgtServer.DropCollection(cluster, shard, database, collection);
                
                
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateIndex, 1)]
        public bool CreateIndex(string cluster, string shard, string database, string collection, IndexConfiguration configuration)
        {
            try
            {
                return dbMgtServer.CreateIndex(cluster, shard, database, collection, configuration);
                
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        
        [TargetMethod(ConfigurationCommandUtil.MethodName.DropIndex, 1)]
        public bool DropIndex(string cluster, string shard, string database, string collection, string indexName)
        {
            try
            {
                return dbMgtServer.DropIndex(cluster, shard, database, collection, indexName);
                
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

        

        #region Recovery Operations
        [TargetMethod(ConfigurationCommandUtil.MethodName.SubmitRecoveryJob, 1)]
        public RecoveryOperationStatus SubmitDataRecoveryJob(string cluster, string shard, RecoveryOperation opContext)
        {
            try
            {
                return dbMgtServer.OnRecoveryOperationReceived(cluster,shard, opContext);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        #endregion



        [TargetMethod(ConfigurationCommandUtil.MethodName.Grant, 1)]
        public bool GrantRole(string cluster, string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser userInfo, Common.Security.Interfaces.IRole roleInfo)
        {
            try
            {
                return dbMgtServer.GrantRole(cluster, shardName, resourceId, userInfo, roleInfo);
            }
            catch (System.Exception exc)
            {
                throw exc;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.Revoke, 1)]
        public bool RevokeRole(string cluster, string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser userInfo, Common.Security.Interfaces.IRole roleInfo)
        {
            try
            {
                return dbMgtServer.RevokeRole(cluster, shardName, resourceId, userInfo, roleInfo);
            }
            catch (System.Exception exc)
            {
                throw exc;
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.PopulateSecurityInformationOnDBServer, 1)]
        public void PopulateSecurityInformationOnDBServer(string cluster, string shardName, IList<IResourceItem> resources)
        {
            dbMgtServer.PopulateSecurityInformationOnDBServer(cluster, shardName, resources);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.PublishAuthenticatedUserInfoToDBServer, 1)]
        public void PublishAuthenticatedUserInfoToDBServer(string cluster, string shard, ISessionId sessionId, string username)
        {
            dbMgtServer.PublishAuthenticatedUserInfoToDBServer(cluster, shard, sessionId, username);
        }


        [TargetMethod(ConfigurationCommandUtil.MethodName.Authenticate, 1)]
        public Common.Security.Server.IServerAuthenticationCredential Authenticate(Common.Security.Client.IClientAuthenticationCredential clientCredentials)
        {
            return dbMgtServer.Authenticate("ManagementServer", clientCredentials, this._sessionId, SSPIUtility.IsLocalServer(_channel.PeerAddress.IpAddress), IsConfigSession);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.OpenConfigurationSession, 1)]
        public Common.Security.Server.IServerAuthenticationCredential OpenConfigurationSession(Common.Security.Client.IClientAuthenticationCredential clientCredentials)
        {
            return Authenticate(clientCredentials);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateUser, 1)]
        public bool CreateUser(string cluster, string localShardName, IUser userInfo)
        {
            return dbMgtServer.CreateUser(cluster, localShardName, userInfo);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.DropUser, 1)]
        public bool DropUser(string cluster, string localShardName, IUser userInfo)
        {
            return dbMgtServer.DropUser(cluster, localShardName, userInfo);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDatabaseCluster, 1)]
        public string GetDatabaseCluster()
        {
            return dbMgtServer.GetDatabaseCluster();
        }
        [TargetMethod(ConfigurationCommandUtil.MethodName.GetShards, 1)]
        public string[] GetShards()
        {
            return dbMgtServer.GetShards();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CanAddToDatabaseCluster, 1)]
        public bool CanAddToDatabaseCluster(string configurationCluster, string configurationUid, string databaseCluster, string shard, string shardUid)
        {
            return dbMgtServer.CanAddToDatabaseCluster(configurationCluster, configurationUid, databaseCluster, shard, shardUid);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetDataBaseServerNode, 1)]
        public List<Address> GetDatabaseServerNodes()
        {
            return dbMgtServer.GetDatabaseServerNodes();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.SetDatabaseMode, 1)]
        public bool SetDatabaseMode( string cluster, string shardName, string databaseName,DatabaseMode databaseMode)
        {
            return dbMgtServer.SetDatabaseMode(cluster, shardName, databaseName, databaseMode);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.MarkConfiguritonSession, 1)]
        public void MarkConfigurationSession()
        {
            IsConfigSession = true;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.CreateLocalCluster, 1)]
        public void CreateLocalCluster()
        {
            dbMgtServer.StartLocalDbNode();
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetConfClusterServers, 1)]

        public List<Address> GetConfClusterServers(string cluster)
        {
            if (dbMgtServer != null)
                return dbMgtServer.GetConfClusterServers(cluster);

            return null;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.NodeAddedToConfigurationCluster, 1)]
        public void NodeAddedToConfigurationCluster(string cluster, ServerNode node)
        {
            if (dbMgtServer != null)
                dbMgtServer.NodeAddedToConfigurationCluster(cluster, node);
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.NodeRemovedFromConfigurationCluster, 1)]
        public void NodeRemovedFromConfigurationCluster(string cluster, ServerNode node)
        {
            if (dbMgtServer != null)
                dbMgtServer.NodeRemovedFromConfigurationCluster(cluster, node);
        }
    }
}
