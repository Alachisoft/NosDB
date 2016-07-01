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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Replication;

namespace Alachisoft.NosDB.Core.Configuration.Services.Client
{
    public class InProcConfigurationSession :  IConfigurationSession
    {
        IConfigurationSession _session;
        public InProcConfigurationSession(IConfigurationSession session)
        {
            _session = session;
        }

        public void Connect(string serviceURI)
        {
           
        }

        public void Disconnect()
        {
           
        }

        public void Start()
        {
           
        }

        public void Stop()
        {
           
        }

        public ClusterInfo[] GetConfiguredClusters()
        {
            return _session.GetConfiguredClusters();
		}
		
        public IServerAuthenticationCredential OpenConfigurationSession(string cluster, IClientAuthenticationCredential clientCredentials)
        {
            SSPIServerAuthenticationCredential serverAuthenticationCredential = new SSPIServerAuthenticationCredential();
            serverAuthenticationCredential.Token = new AuthToken();
            serverAuthenticationCredential.Token.Status = Common.Security.SSPI.SecurityStatus.OK;
            return serverAuthenticationCredential;
        }

        public IServerAuthenticationCredential Authenticate(IClientAuthenticationCredential clientCredentials)
        {
            SSPIServerAuthenticationCredential serverAuthenticationCredential = new SSPIServerAuthenticationCredential();
            serverAuthenticationCredential.Token = new AuthToken();
            serverAuthenticationCredential.Token.Status = Common.Security.SSPI.SecurityStatus.OK;
            return serverAuthenticationCredential;
        }

        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {
            return _session.GetDatabaseClusterConfiguration(clusterName);
        }

        public Dictionary<string, int> GetShardsPort(string clusterName)
        {
            return _session.GetShardsPort(clusterName);
        }

        public ConfigServerConfiguration GetConfigurationClusterConfiguration(string configCluster)
        {
            return _session.GetConfigurationClusterConfiguration(configCluster);
        }

        public void RegisterClusterConfiguration(ClusterConfiguration configuration)
        {
           _session.RegisterClusterConfiguration(configuration);
        }

        public void UnregisterClusterConfiguration(ClusterConfiguration configuration)
        {
            _session.UnregisterClusterConfiguration(configuration);
        }

        public void UpdateClusterConfiguration(ClusterConfiguration configuration)
        {
            _session.UpdateClusterConfiguration(configuration);
        }

        //public void ReportLastOperationTime(long operationId, string cluster, string shard, ServerInfo serverInfo)
        //{
        //    _session.ReportLastOperationTime(operationId, cluster, shard, serverInfo);
        //}

        public ISessionId SessionId
        {
            get { return _session.SessionId; }
        }

        public DateTime SessionStartTime
        {
            get { return this._session.SessionStartTime; }
        }

        public void Close()
        {
            if (_session != null)
                _session.Close();
        }

        public void CreateCluster(string name, ClusterConfiguration configuration)
        {
            _session.CreateCluster(name, configuration);
        }

        public void RemoveCluster(string name)
        {
            _session.RemoveCluster(name);
        }

        public bool AddShardToCluster(string cluster, string shard, ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration)
        {
            return _session.AddShardToCluster(cluster, shard, shardConfiguration, distributionConfiguration);
        }

        public bool RemoveShardFromCluster(string cluster, string shard, IDistributionConfiguration configuration,Boolean isGraceFull)
        {
            return _session.RemoveShardFromCluster(cluster, shard, configuration, isGraceFull);
        }

        public bool AddServerToShard(string cluster, string shard, ServerNode server)
        {
            return _session.AddServerToShard(cluster, shard, server);
        }

        public bool RemoveServerFromShard(string cluster, string shard, ServerNode server)
        {
            return _session.RemoveServerFromShard(cluster, shard, server);
        }

        public ClusterInfo GetDatabaseClusterInfo(string cluster)
        {
            return _session.GetDatabaseClusterInfo(cluster);
        }

        public IDistribution GetCollectionDistribution(string cluster, string database, string collection)
        {
            return _session.GetCollectionDistribution(cluster, database, collection);
        }

        public void AddConfigurationListener(IConfigurationListener listener)
        {
            _session.AddConfigurationListener(listener);
        }

        public void RemoveConfigurationListener(IConfigurationListener listener)
        {
            _session.RemoveConfigurationListener(listener);
        }

        public void UpdateConfigServerNodePriority(string cluster, string nodeName, int priority)
        {
            _session.UpdateConfigServerNodePriority(cluster, nodeName, priority);
        }

        public void SetNodeStatus(string cluster, string shard, ServerNode primary, NodeRole status)
        {
            _session.SetNodeStatus(cluster, shard, primary, status);
        }

        public Membership[] GetMembershipInfo(string cluster)
        {
            return _session.GetMembershipInfo(cluster);
        }


        public Membership GetMembershipInfo(string cluster, string shard)
        {
            return _session.GetMembershipInfo(cluster, shard);
        }

        public void UpdateNodeStatus(string cluster, string shard, ServerNode server, Status status)
        {
            _session.UpdateNodeStatus(cluster, shard, server, status);
        }

        public Object BeginElection(string cluster, string shard, ServerNode server, ElectionType electionType)
        {
            return _session.BeginElection(cluster, shard, server, electionType);
        }

        public void SubmitElectionResult(string cluster, string shard, ElectionResult result)
        {
            _session.SubmitElectionResult(cluster, shard, result);
        }

        public void EndElection(string cluster, string shard, ElectionId electionId)
        {
            _session.EndElection(cluster, shard, electionId);
        }


        public void ConfigureDistributionStategy(string cluster, string database, string collection, IDistributionStrategy strategy)
        {
            _session.ConfigureDistributionStategy(cluster, database, collection, strategy);
        }

        public IDistributionStrategy GetDistriubtionStrategy(string cluster, string database, string collection)
        {
            return _session.GetDistriubtionStrategy(cluster, database, collection);
        }

        public IDistribution GetCurrentDistribution(string cluster, string database, string collection)
        {
            return _session.GetCurrentDistribution(cluster, database, collection);
        }

        public IDistribution BalanceData(string cluster, string database, string collection)
        {
            return _session.BalanceData(cluster, database, collection);
        }


        public void CreateDatabase(string cluster, DatabaseConfiguration configuration)
        {
            _session.CreateDatabase(cluster, configuration);
        }

        public void DropDatabase(string cluster, string database,bool dropFiles)
        {
            _session.DropDatabase(cluster, database, dropFiles);
        }

        public void CreateCollection(string cluster, string database, CollectionConfiguration configuration)
        {
            _session.CreateCollection(cluster, database, configuration);
        }

        public void MoveCollection(string cluster, string database, string collection, string newShard)
        {
            _session.MoveCollection(cluster, database, collection, newShard);
        }

        public void DropCollection(string cluster, string database, string collection)
        {
            _session.DropCollection(cluster, database, collection);
        }

        public void UpdateMembership(string cluster, string shard, Membership mebership)
        {
            _session.UpdateMembership(cluster, shard, mebership);
        }


        public Membership ReportNodeJoining(string cluster, string shard, ServerNode joiningServer)
        {
            return _session.ReportNodeJoining(cluster, shard, joiningServer);
        }

        public Membership ReportNodeLeaving(string cluster, string shard, ServerNode leavingServer)
        {
            return _session.ReportNodeJoining(cluster, shard, leavingServer);
        }

        public int ReportHeartbeat(string cluster, string shard, ServerNode reportingServer, Membership membership, OperationId lastOpId)
        {
            return _session.ReportHeartbeat(cluster, shard, reportingServer, membership, lastOpId);
        }


        public void CreateIndex(string cluster, string database, string collection, IndexConfiguration configuration)
        {
            _session.CreateIndex(cluster, database, collection, configuration);
        }

        public void DropIndex(string cluster, string database, string collection, string indexName)
        {
            _session.DropIndex(cluster, database, collection, indexName);
        }

        public void UpdateCollectionStatistics(string cluster, string database, string collection, CollectionStatistics statistics)
        {
            _session.UpdateCollectionStatistics(cluster, database, collection, statistics);
        }

        public void UpdateBucketStatistics(string cluster, string database, string collection, Common.Stats.ShardInfo shardInfo)
        {
            _session.UpdateBucketStatistics(cluster, database, collection, shardInfo);
        }

        public bool StartNode(string cluster, string shard, ServerNode server)
        {
            return _session.StartNode(cluster, shard, server);
        }

        public bool StopNode(string cluster, string shard, ServerNode server)
        {
            return _session.StopNode(cluster, shard, server);
        }

        public void StartCluster(string cluster)
        {
            _session.StartCluster(cluster);
        }

        public void StopCluster(string cluster)
        {
            _session.StopCluster(cluster);
        }

        public bool StartShard(string cluster, string shard)
        {
            return _session.StartShard(cluster, shard);
        }

        public bool StopShard(string cluster, string shard)
        {
            return _session.StartShard(cluster, shard);
        }



        public void CreateConfigurationCluster(ConfigServerConfiguration serverConfig, int heartBeat, ReplicationConfiguration replConfig, string displayName)
        {
            _session.CreateConfigurationCluster(serverConfig, heartBeat, replConfig,  displayName);
        }


        public void StartConfigurationServer(string name)
        {
            _session.StartConfigurationServer(name);
        }

        public void StopConfigurationServer(string name)
        {
            _session.StopConfigurationServer(name);
        }


        public void AddNodeToConfigurationCluster(string name, ServerNode node)
        {
            _session.AddNodeToConfigurationCluster(name, node);
        }

        public void RemoveNodeFromConfigurationCluster(string cluster, ServerNode node)
        {
            _session.RemoveNodeFromConfigurationCluster(cluster, node);
        }

        public bool VerifyConfigurationServerAvailability()
        {
            return _session.VerifyConfigurationServerAvailability();
        }


        public bool VerifyConfigurationClusterAvailability(string configClusterName)
        {
            return _session.VerifyConfigurationClusterAvailability(configClusterName);
        }


        public bool VerifyConfigurationCluster(string configClusterName)
        {
            return _session.VerifyConfigurationCluster(configClusterName);
        }

        public bool VerifyConfigurationClusterPrimery(string configClusterName)
        {
            return _session.VerifyConfigurationClusterPrimery(configClusterName);
        }

        public string[] ListDatabases(string cluster)
        {
            return _session.ListDatabases(cluster);
        }

        public string[] ListCollections(string cluster, string database)
        {
            return _session.ListCollections(cluster, database);
        }

        public string[] ListIndices(string cluster, string database, string collection)
        {
            return _session.ListIndices(cluster, database, collection);
        }


        #region Recovery Operations
        public Common.Recovery.RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config)
        {
            throw new NotImplementedException();
        }

        public Common.Recovery.RecoveryOperationStatus CancelRecoveryJob(string identifier)
        {
            throw new NotImplementedException();
        }

        public Common.Recovery.RecoveryOperationStatus[] CancelAllRecoveryJobs()
        {
            throw new NotImplementedException();
        }

        public Common.Recovery.ClusteredRecoveryJobState GetJobState(string identifier)
        {
            throw new NotImplementedException();
        }

        public void SubmitShardJobStatus(ShardRecoveryJobState status)
        { throw new NotImplementedException(); }

        public ClusterJobInfoObject[] GetAllRunningJobs()
        {
            throw new NotImplementedException();
        }


        public IDictionary<string, byte[]> GetDeploymentSet(string implIdentifier)
        {
            throw new NotImplementedException();
        }

        #endregion



        public ShardConfiguration GetShardConfiguration(string cluster, string shard)
        {
            return _session.GetShardConfiguration(cluster, shard);
        }




        public bool Grant(string clusterName, Common.Security.Impl.ResourceId resourceId, string userName, string roleName)
        {
            bool isSuccessful = false;
            isSuccessful = _session.Grant(clusterName, resourceId, userName, roleName);
            return isSuccessful;
        }

        public bool Revoke(string clusterName, Common.Security.Impl.ResourceId resourceId, string userName, string roleName)
        {
            bool isSuccessful = false;
            isSuccessful = _session.Revoke(clusterName, resourceId, userName, roleName);
            return isSuccessful;
        }

        public bool CreateUser(Common.Security.Interfaces.IUser userInfo)
        {
            return _session.CreateUser(userInfo);
        }

        public bool DropUser(Common.Security.Interfaces.IUser userInfo)
        {
            return _session.DropUser(userInfo);
        }

        public void CreateRole(Common.Security.Interfaces.IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public void AlterRole(Common.Security.Interfaces.IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public void DropRole(Common.Security.Interfaces.IRole roleInfo)
        {
            throw new NotImplementedException();
        }
        public IList<IUser> GetUsersInformation()
        {
            return _session.GetUsersInformation();
        }

    
        public IList<IResourceItem> GetResourcesInformation(string cluster)
        {
            return _session.GetResourcesInformation(cluster);
        }

        public void UpdateDatabaseConfiguration(string cluster, string database, DatabaseConfiguration databaseConfiguration)
        {
            _session.UpdateDatabaseConfiguration(cluster, database, databaseConfiguration);
        }

        public void UpdateServerPriority(string cluster, string shard, ServerNode server, int priority)
        {
            _session.UpdateServerPriority(cluster, shard, server, priority);
        }

        public void UpdateDeploymentConfiguration(string cluster, int heartBeatInterval)
        {
            _session.UpdateDeploymentConfiguration(cluster, heartBeatInterval);
        }

      

        public void UpdateShardConfiguration(string cluster, string shard, int heartbeat, int port)
        {
            throw new NotImplementedException();
		}
		
        public IUser GetAuthenticatedUserInfoFromConfigServer(ISessionId sessionId)
        {
            return _session.GetAuthenticatedUserInfoFromConfigServer(sessionId);
        }



        public IResourceItem GetResourceSecurityInfo(string cluster, ResourceId resourceId)
        {
            return _session.GetResourceSecurityInfo(cluster, resourceId);
        }

        public IDictionary<IRole, IList<ResourceId>> GetUserInfo(IUser userInfo)
        {
            return _session.GetUserInfo(userInfo);
        }


        public void UpdateCollectionConfiguration(string cluster, string database, string collection, CollectionConfiguration collectionConfiguration)
        {
            _session.UpdateCollectionConfiguration(cluster, database, collection, collectionConfiguration);
        }


        public void RemoveConfigurationCluster(string configClusterName)
        {
            _session.RemoveConfigurationCluster(configClusterName);
        }




        public List<Address> GetDataBaseServerNode()
        {
            return _session.GetDataBaseServerNode();
        }

        public bool SetDatabaseMode(string cluster, string databaseName, DatabaseMode databaseMode)
        {
            return _session.SetDatabaseMode(cluster, databaseName, databaseMode);
        }


        public void DeployAssemblies(string cluster, string deploymentId, string deploymentName, string assemblyFileName, byte[] buffer)
        {
            _session.DeployAssemblies(cluster, deploymentId, deploymentName,assemblyFileName, buffer);
        }

        public bool VerifyConfigurationClusterUID(string UID)
        {
            return _session.VerifyConfigurationClusterUID(UID);
        }

       

        public bool IsRemoteClient()
        {
           return _session.IsRemoteClient();      
        }


        public bool IsAuthorized(ISessionId sessionId, ResourceId resourceId, ResourceId superResourceId, Permission operationPermission)
        {
            return _session.IsAuthorized(sessionId, resourceId, superResourceId, operationPermission);
        }


        public void MarkDatabaseSession()
        {
            _session.MarkDatabaseSession();
        }


        public void MarkDistributorSession()
        {
            _session.MarkDistributorSession();
        }


        public void MarkConfigurationSession()
        {
            _session.MarkConfigurationSession();
        }

        public byte[] ValidateProfessional(byte[] token)
        {
            return _session.ValidateProfessional(token);
        }

        public bool HasSynchronizedWithPrimaryConfigServer()
		{
            throw new NotImplementedException();
        }


        public object StateTransferOperation(string clusterName, Common.Toplogies.Impl.StateTransfer.Operations.IStateTransferOperation operation)
        {
            throw new NotImplementedException();
        }


        public NodeRole GetCurrentRole()
        {
            return _session.GetCurrentRole();
        }


        public List<Address> GetConfServers(string cluster)
        {
            throw new NotImplementedException();
        }


        public bool IsNodeRunning(string node)
        {
            throw new NotImplementedException();
        }
    }
}
