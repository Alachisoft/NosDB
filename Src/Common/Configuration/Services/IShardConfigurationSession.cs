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
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;

namespace Alachisoft.NosDB.Common.Configuration.Services
{
    public interface IShardConfigurationSession
    {
        string Cluster { get; }
        string Shard { get; }
        bool IsValid { get; }
        string SessionId { get; }
        DateTime SessionStartTime { get; }

        void Close();

        void AddConfigurationListener(IConfigurationListener listener);
        void RemoveConfigurationListener(IConfigurationListener listener);

        #region /                           --- Membership Management---                           /

        ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName);
        void SetNodeStatus(ServerNode primary, NodeRole status);
        Membership[] GetMembershipInfo();
        Membership GetMembershipInfo(string shard);

        void UpdateNodeStatus(Status status);
        Object BeginElection(ServerNode server, ElectionType electionType);
        void SubmitElectionResult(ElectionResult result);
        void EndElection(ElectionId electionId);

       
      

        #endregion

        #region /                           --- Data distribution ---                           /

        void ConfigureDistributionStategy(string collection, IDistributionStrategy strategy);
        IDistributionStrategy GetDistriubtionStrategy(string collection);
        IDistribution GetCurrentDistribution(string collection);
        IDistribution BalanceData(string collection);

        #endregion

        #region /                           --- Meta-data book keeping ---                           /




        #endregion

        #region /                           --- Meta-data book keeping ---                           /




        #endregion
    }
}
