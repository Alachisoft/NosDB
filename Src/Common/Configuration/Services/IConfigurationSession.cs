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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Common.Configuration.Services
{
    public interface IConfigurationSession
    {
        ISessionId SessionId { get; }
        DateTime SessionStartTime { get; }

        void Close();

        #region Security
        IServerAuthenticationCredential Authenticate(IClientAuthenticationCredential clientCredentials);
        IServerAuthenticationCredential OpenConfigurationSession(string cluster, IClientAuthenticationCredential clientCredentials);

        bool Grant(string clusterName, ResourceId resourceId, string userName, string roleName);
        bool Revoke(string clusterName, ResourceId resourceId, string userName, string roleName);

        bool CreateUser(IUser userInfo);
        bool DropUser(IUser userInfo);

        void CreateRole(IRole roleInfo);
        void AlterRole(IRole roleInfo);
        void DropRole(IRole roleInfo);

        IList<IUser> GetUsersInformation();
        IList<IResourceItem> GetResourcesInformation(string cluster);
        IDictionary<IRole, IList<ResourceId>> GetUserInfo(IUser userInfo);

        IUser GetAuthenticatedUserInfoFromConfigServer(Common.Security.Interfaces.ISessionId sessionId);

        IResourceItem GetResourceSecurityInfo(string cluster, ResourceId databaseName);

        bool IsAuthorized(ISessionId sessionId, ResourceId resourceId, ResourceId superResourceId, Permission operationPermission);
        
        void MarkDatabaseSession();
        void MarkDistributorSession();
        void MarkConfigurationSession();
        #endregion

        void AddConfigurationListener(IConfigurationListener listener);
        void RemoveConfigurationListener(IConfigurationListener listener);

        #region Configuration

        byte[] ValidateProfessional(byte[] token);

        ConfigServerConfiguration GetConfigurationClusterConfiguration(string configCluster);
        ShardConfiguration GetShardConfiguration(string cluster, string shard);
        ClusterConfiguration GetDatabaseClusterConfiguration(string cluster);
        void UpdateConfigServerNodePriority(string cluster, string nodeName, int priority);
        Dictionary<string, int> GetShardsPort(string clusterName);
        void RegisterClusterConfiguration(ClusterConfiguration configuration);
        void UnregisterClusterConfiguration(ClusterConfiguration configuration);
        void UpdateClusterConfiguration(ClusterConfiguration configuration);
        //void ReportLastOperationTime(long operationId,string cluster,string shard,ServerInfo serverInfo);
        void CreateCluster(string name, ClusterConfiguration configuration);
        void RemoveCluster(string name);
        bool AddShardToCluster(string cluster, string shard, ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration);
        bool RemoveShardFromCluster(string cluster, string shard, IDistributionConfiguration configuration, Boolean isGraceFull);
        bool AddServerToShard(string cluster, string shard, ServerNode server);
        bool RemoveServerFromShard(string cluster, string shard, ServerNode server);
        bool StartNode(string cluster, string shard, ServerNode server);
        bool StopNode(string cluster, string shard, ServerNode server);
        void StartCluster(string cluster);
        void StopCluster(string cluster);
        bool StartShard(string cluster, string shard);
        bool StopShard(string cluster, string shard);
        NodeRole GetCurrentRole();

        void CreateConfigurationCluster(ConfigServerConfiguration serverConfig, int heartBeat, ReplicationConfiguration replConfig, string displayName);

        void RemoveConfigurationCluster(string configClusterName);
        void StartConfigurationServer(string name);
        void StopConfigurationServer(string name);
        void AddNodeToConfigurationCluster(string name, ServerNode node);
       void RemoveNodeFromConfigurationCluster(string cluster, ServerNode node);
        bool VerifyConfigurationServerAvailability();
        bool VerifyConfigurationClusterAvailability(string configClusterName);
        bool VerifyConfigurationCluster(string configClusterName);
        bool VerifyConfigurationClusterUID(string configClusterName);
        bool VerifyConfigurationClusterPrimery(string configClusterName);
        string[] ListDatabases(string cluster);
        string[] ListCollections(string cluster,string database);
        string[] ListIndices(string cluster, string database, string collection);
        void DeployAssemblies(string cluster, string deploymentId, string deploymentName, string assemblyFileName, byte[] buffer);
        bool IsRemoteClient();
        #endregion

        #region MetaData Management

        ClusterInfo[] GetConfiguredClusters();
        ClusterInfo GetDatabaseClusterInfo(string cluster);
        IDistribution GetCollectionDistribution(string cluster, string database, string collection);
        void CreateDatabase(string cluster, DatabaseConfiguration configuration);
        void DropDatabase(string cluster, string database, bool dropFiles);
        void CreateCollection(string cluster, string database, CollectionConfiguration configuration);
        void MoveCollection(string cluster, string database, string collection, string newShard);
        void DropCollection(string cluster, string database, string collection);
        void CreateIndex(string cluster, string database, string collection, IndexConfiguration configuration);
        void DropIndex(string cluster, string database, string collection, string indexName);
        void UpdateDatabaseConfiguration(string cluster, string database, DatabaseConfiguration databaseConfiguration);
        void UpdateServerPriority(string cluster, string shard, ServerNode server, int priority);

        void UpdateDeploymentConfiguration(string cluster, int heartBeatInterval);

        void UpdateCollectionConfiguration(string cluster, string database, string collection, CollectionConfiguration collectionConfiguration);
        #endregion

        #region /                           --- Membership Management---                           /

        void SetNodeStatus(string cluster, string shard, ServerNode primary, NodeRole status);
        Membership[] GetMembershipInfo(string cluster);

        Membership GetMembershipInfo(string cluster, string shard);
        void UpdateNodeStatus(string cluster, string shard, ServerNode server, Status status);
        Object BeginElection(string cluster, string shard, ServerNode server, ElectionType electionType);
        void SubmitElectionResult(string cluster, string shard, ElectionResult result);
        void EndElection(string cluster, string shard, ElectionId electionId);

        void UpdateMembership(string cluster, string shard, Membership mebership);

        /// <summary>
        /// A joining node report's it's joining to the configuration server and get's the existing membership 
        /// </summary>
        /// <param cluster="cluster"></param>
        /// <param cluster="shard"></param>
        /// <param cluster="server"></param>
        /// <returns></returns>
        Membership ReportNodeJoining(string cluster, string shard, ServerNode joiningServer);

        /// <summary>
        /// A leaving node report's it's leaving in case of gracefull shutdow to the configuration server and get's the existing membership 
        /// </summary>
        /// <param cluster="cluster"></param>
        /// <param cluster="shard"></param>
        /// <param cluster="leavingServer"></param>
        /// <returns></returns>
        Membership ReportNodeLeaving(string cluster, string shard, ServerNode leavingServer);

        /// <summary>
        /// Every server sends a heatbeat message to configuration manager that he is alive
        /// </summary>
        /// <param cluster="cluster"></param>
        /// <param cluster="shard"></param>
        /// <param cluster="leavingServer"></param>
        /// <returns>returns next heart beat interval at which this server should report again </returns>
        int ReportHeartbeat(string cluster, string shard, ServerNode reportingServer, Membership membership, OperationId lastOpId);

        #endregion
        
        #region /                           --- Data distribution ---                           /

        void ConfigureDistributionStategy(string cluster, string database, string collection, IDistributionStrategy strategy);
        IDistributionStrategy GetDistriubtionStrategy(string cluster, string database, string collection);
        IDistribution GetCurrentDistribution(string cluster,string database, string collection);
        IDistribution BalanceData(string cluster, string database, string collection);

        #endregion

        #region Statistics Operations
        void UpdateCollectionStatistics(string cluster, string database, string collection, CollectionStatistics statistics);
        void UpdateBucketStatistics(string cluster, string database, string collection, Common.Stats.ShardInfo shardInfo);
        #endregion

        #region /                           --- Recovery ---                                   /
        RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config);
        RecoveryOperationStatus CancelRecoveryJob(string identifier);
        RecoveryOperationStatus[] CancelAllRecoveryJobs();
        ClusteredRecoveryJobState GetJobState(string identifier);
        //ClusteredRecoveryJobState[] GetAllRunningJobs();
        ClusterJobInfoObject[] GetAllRunningJobs();
        void SubmitShardJobStatus(ShardRecoveryJobState status);

        #endregion


        IDictionary<string, byte[]> GetDeploymentSet(string implIdentifier);

        #region State Transfer Operations

        Object StateTransferOperation(String clusterName, IStateTransferOperation operation);

        #endregion

        List<Address> GetDataBaseServerNode();

        bool SetDatabaseMode(string cluster, string databaseName, DatabaseMode databaseMode);
        
        bool HasSynchronizedWithPrimaryConfigServer();

        #region /                          --- InterConfiguration Nodes operations ----             /

        #endregion

        List<Address> GetConfServers(String cluster);
        bool IsNodeRunning(string node);
    }
}
