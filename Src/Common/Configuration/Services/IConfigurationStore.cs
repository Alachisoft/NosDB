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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using System.Collections;

namespace Alachisoft.NosDB.Common.Configuration.Services
{
    public interface IConfigurationStore : IDisposable
    {
        void Initialize();

        ClusterInfo[] GetAllClusterInfo();
        ClusterInfo GetClusterInfo(string cluster);
        void InsertOrUpdateClusterInfo(ClusterInfo clusterInfo);
        void RemoveClusterInfo(string cluster);

        void GetAllDistributionStrategies(ClusterInfo[] clusterinfo);
        IDistributionStrategy GetDistributionStrategy(string cluster, string database, string collection);
        void InsertOrUpdateDistributionStrategy(ClusterInfo clusterInfo, string database, string collection);

        Membership[] GetAllMembershipData();
        Membership GetMembershipData(string cluster,string shard);
        void InsertOrUpdateMembershipData(Membership membership);
        void RemoveMembershipData(string cluster, string shard);


        ClusterConfiguration[] GetAllClusterConfiguration();
        ClusterConfiguration GetClusterConfiguration(string cluster);
        void InsertOrUpdateClusterConfiguration(ClusterConfiguration configuration);
        void RemoveClusterConfiguration(string cluster);

        #region Recovery operations
        void InsertOrUpdateRecoveryJobData(ClusterJobInfoObject job);
        void RemoveRecoveryjobData(string id);
        ClusterJobInfoObject GetRecoveryJobData(string id);
        ClusterJobInfoObject[] GetAllRecoveryJobData();
        ClusterJobInfoObject[] GetRecoveryJobData(Query query);
        #endregion

        #region Security configuration related
        IResourceItem[] GetAllResourcesSecurityInformation();
        IResourceItem GetResourceSecurityInformation(string resource);
        void InsertOrUpdateResourceSecurityInformation(IResourceItem resourceItem);
        void RemoveResourceSecurityInformation(string resource);

        IUser[] GetAllUserInformation();
        IUser GetUserInformation(string user);
        void InsertOrUpdateUserInformation(IUser userInfo);
        void RemoveUserInformation(string username);

        IRole[] GetAllRolesInformation();
        IRole GetRoleInformatio(string name);
        void InsertOrUpdateRoleInformation(IRole userInfo);
        void RemoveRoleInformation(string name);
        #endregion

        //void UpdateBucketStatus(string cluster, string database, string collection, ArrayList bucketList, byte status, String shard = null);
    }
}
