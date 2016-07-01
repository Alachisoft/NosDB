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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using CollectionConfiguration = Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;

namespace Alachisoft.NosDB.Core.DBEngine.Management
{
    interface IManagementSession
    {
        ISessionId SessionId { set; get; }

        bool StartNode(string cluster,string shard, int serverPort);
        bool StopNode(string cluster,string shard);
        bool StopNodeForClients(string cluster, string shard);
        bool AddServerToShard(string configCluster, string clusterUID, Address[] configServers, string databaseCluster, string shard, string shardUid, int shardPort, bool start,ClusterConfiguration clusterConfig);
        bool RemoveServerFromShard(string cluster ,string shard);
        
        bool CreateDatabase(string cluster, string shard, DatabaseConfiguration configuration,IDictionary<string,IDistributionStrategy> collectionDistribution);
        bool DropDatabase(string cluster, string shard, string database, bool dropFiles);
       
        bool CreateCollection(string cluster, string shard, string database, CollectionConfiguration configuration, IDistributionStrategy distribution);
        bool DropCollection(string cluster, string shard, string database, string collection);
        bool CreateIndex(string cluster, string shard, string database, string collection, IndexConfiguration configuration);
        bool DropIndex(string cluster, string shard, string database, string collection, string indexName);
        
        IServerAuthenticationCredential Authenticate(IClientAuthenticationCredential clientCredentials);
        IServerAuthenticationCredential OpenConfigurationSession(IClientAuthenticationCredential clientCredentials);
        bool GrantRole(string cluster, string shardName, ResourceId resourceId, IUser userInfo, IRole roleInfo);
        bool RevokeRole(string cluster, string shardName, ResourceId resourceId, IUser userInfo, IRole roleInfo);

        void NodeAddedToConfigurationCluster(string cluster, ServerNode node);
        void NodeRemovedFromConfigurationCluster(string cluster, ServerNode node);


        bool CreateUser(string cluster, string localShardName, IUser userInfo);
        bool DropUser(string cluster, string localShardName, IUser userInfo);

        void MarkConfigurationSession();

        void PopulateSecurityInformationOnDBServer(string cluster, string shardName, IList<IResourceItem> resources);
        void PublishAuthenticatedUserInfoToDBServer(string cluster, string shard, ISessionId sessionId, string username);
        string GetDatabaseCluster();
        string[] GetShards();
        bool CanAddToDatabaseCluster(string configurationCluster,string configurationUid, string databaseCluster,string shard,string shardUid);
        List<Address> GetDatabaseServerNodes();

        void CreateLocalCluster();
        #region Recovery Operations
        
        RecoveryOperationStatus SubmitDataRecoveryJob(string cluster, string shard, RecoveryOperation opContext);
        #endregion

        #region Configuration Server Redirection
        List<Address> GetConfClusterServers(String cluster);
        #endregion
    }
}
