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
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;

namespace Alachisoft.NosDB.Core.Configuration.Services.Client
{
    public class InProcShardConfigurationSession : IShardConfigurationSession
    {

        IShardConfigurationSession _shardSession;       
        
        public InProcShardConfigurationSession(IShardConfigurationSession session)
        {
            _shardSession = session;
        }

        public string Cluster
        {
            get { return this._shardSession.Cluster; }
        }

        public string Shard
        {
            get { return this._shardSession.Shard; }
        }

        public bool IsValid
        {
            get { return this._shardSession.IsValid; }
        }

        public string SessionId
        {
            get { return this._shardSession.SessionId; }
        }

        public DateTime SessionStartTime
        {
            get { return this._shardSession.SessionStartTime; }
        }

        public void Close()
        {
            this._shardSession.Close();
        }

        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {

            return _shardSession.GetDatabaseClusterConfiguration(clusterName);
        }

        public void SetNodeStatus(ServerNode primary, NodeRole status)
        {
            _shardSession.SetNodeStatus(primary, status);
        }

        public Membership[] GetMembershipInfo()
        {

            return _shardSession.GetMembershipInfo();
        }

        public Membership GetMembershipInfo(string shard)
        {
            return _shardSession.GetMembershipInfo(shard);
        }


        public void UpdateNodeStatus(Status status)
        {
            _shardSession.UpdateNodeStatus(status);
        }

        public Object BeginElection(ServerNode server, ElectionType electionType)
        {
            return _shardSession.BeginElection(server, electionType);
        }

        public void SubmitElectionResult(ElectionResult result)
        {
            _shardSession.SubmitElectionResult(result);
        }


        public void EndElection(ElectionId electionId)
        {
            _shardSession.EndElection(electionId);
        }
       

       

        public void ConfigureDistributionStategy(string collection, IDistributionStrategy strategy)
        {           
            _shardSession.ConfigureDistributionStategy(collection, strategy);
        }

        public IDistributionStrategy GetDistriubtionStrategy(string collection)
        {           
            return _shardSession.GetDistriubtionStrategy(collection);
        }

        public IDistribution GetCurrentDistribution(string collection)
        {
            return _shardSession.GetCurrentDistribution(collection);
        }

        public IDistribution BalanceData(string collection)
        {
            return _shardSession.BalanceData(collection);
        }        
                

        public void AddConfigurationListener(IConfigurationListener listener)
        {
            _shardSession.AddConfigurationListener(listener);
        }

        public void RemoveConfigurationListener(IConfigurationListener listener)
        {
            _shardSession.RemoveConfigurationListener(listener);
        }
    }
}
