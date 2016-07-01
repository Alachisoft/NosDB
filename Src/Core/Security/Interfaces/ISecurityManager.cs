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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.DBEngine;

namespace Alachisoft.NosDB.Core.Security.Interfaces
{
    public interface ISecurityManager: IClientDisconnection
    {
        /// <summary>
        /// this method initializes the user against username provided
        /// </summary>
        /// <param name="Username">username of the authenticated user</param>
        /// <param name="Username">session id of the user</param>
        /// <returns></returns>
        void LoadAuthorizationInformation(string localShardName, IUser user, ISessionId sessionId);

        /// <summary>
        /// Validates if a user has permission to do operation against resource
        /// </summary>
        /// <param name="sessionId">user's current session id</param>
        /// <param name="resource">resource on which operation is to be performed</param>
        /// <param name="operationPermission">permission against operation</param>
        /// <returns>true if user is validated and false if user is not validated</returns>
        bool Authorize(string localShardName, ISessionId sessionId, ResourceId resourceId, ResourceId superResourceId,
            Permission operationPermission, bool LogError = true);

        IServerAuthenticationCredential Authenticate(string localShardName, IClientAuthenticationCredential clientCredentials, ISessionId sessionId, bool isLocalServer, string serviceName = null);

        AuthToken Authenticate(string localShardName, IAuthenticationOperation opertion, bool isLocalClient, string distributorServiceName = null);

        void Initialize(string localShardName);

        void AddSecurityDatabase(string clusterName, ISecurityDatabase securityDatabase);

        void RemoveSecurityDatabase(string clusterName);

        ISecurityDatabase GetSecurityDatabase(string clusterName);

        void InitializeSecurityInformation(string shard);

        void AddResource(string localShardName, IResourceItem resourceItem, ISessionId sessionId, ResourceId superResourceId = null, string clusterName = null);

        void RemoveResource(string localShardName, ResourceId resourceId, ISessionId sessionId, ResourceId superResourceId = null, string clusterName = null);

        bool Grant(string localShardName, ResourceId resourceId, IUser userInfo, IRole roleInfo, string clusterName = null);

        bool Grant(string localShardName, ResourceId resourceId, string userName, string roleName, string clusterName = null);

        bool Revoke(string localShardName, ResourceId resourceId, string userName, string roleName, string clusterName = null);

        bool Revoke(string localShardName, ResourceId resourceId, IUser usrrInfo, IRole roleInfo, string clusterName = null);

        IList<IResourceItem> GetSubResources(string localShardName, ResourceId resourceId);

        void PopulateSecurityInformation(string shardName, IList<IResourceItem> resourceItems, IList<IUser> users);
        
        ISecurityServer SecurityServer { set; get; }

        bool CreateUser(string localShardName, IUser userInfo);

        bool DropUser(string localShardName, IUser userInfo);

        void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId, string username);

        IList<IUser> Users(string localShardName);

        void OnChannelDisconnected(ISessionId sessionId);

        IUser GetAuthenticatedUserInfo(ISessionId sessionId);

        IList<IResourceItem> GetDatabaseResources(string localShardName);

        IList<IResourceItem> GetClusterResources(string localShardName);

        IResourceItem GetResource(string localShardName, string cluster, ResourceId resourceId);

        void SetResource(string localShardName, string cluster, ResourceItem resourceItem);

        string GetConnectionInfo(ISessionId sessionId);

        IDictionary<IRole, IList<ResourceId>> GetUserInfo(string localShardName, IUser userInfo);

        IUser GetUser(ISessionId sessionId);
    }
}
