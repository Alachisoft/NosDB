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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;

namespace Alachisoft.NosDB.Common.Configuration.Services.Client
{
    public class OutProcShardConfigurationSession : IShardConfigurationSession
    {

        IShardConfigurationSession _shardSession;
        DualChannel _channel;
        
        public OutProcShardConfigurationSession(string cluster, string shard, ServerNode server, UserCredentials credentials, DualChannel channel)
        {
            _channel = channel;
            //_shardSession = new InProcConfigurationClient().OpenShardConfigurationSession(cluster, shard, server, credentials);        
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
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDatabaseClusterConfiguration, 1);
            command.Parameters.AddParameter(clusterName);

            return ExecuteCommandOnConfigurationServer(command, true) as ClusterConfiguration;
        }

        public void SetNodeStatus(ServerNode primary, NodeRole status)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SetNodeStatus, 1);
            command.Parameters.AddParameter(primary);
            command.Parameters.AddParameter(status);

            ExecuteCommandOnConfigurationServer(command, false);
        }


       
        public void UpdateNodeStatus(Status status)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateNodeStatus, 1);
            command.Parameters.AddParameter(status);

            ExecuteCommandOnConfigurationServer(command, false);
        }

        public Membership[] GetMembershipInfo()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 1);

            return ExecuteCommandOnConfigurationServer(command, true) as Membership[];
        }

        public Membership GetMembershipInfo(string shard)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 2);
            command.Parameters.AddParameter(shard);

            return ExecuteCommandOnConfigurationServer(command, true) as Membership;
        }

        public Object BeginElection(ServerNode server, ElectionType electionType)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.BeginElection, 1);
            command.Parameters.AddParameter(electionType);
            return ExecuteCommandOnConfigurationServer(command, true);
        }

        public void SubmitElectionResult(ElectionResult result)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SubmitElectionResult, 1);
            command.Parameters.AddParameter(result);

            ExecuteCommandOnConfigurationServer(command, false);
        }


        public void EndElection(ElectionId electionId)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.EndElection, 1);
            command.Parameters.AddParameter(electionId);


            ExecuteCommandOnConfigurationServer(command, false);
        }

       

       

      

        public void ConfigureDistributionStategy(string collection, IDistributionStrategy strategy)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ConfigureDistributionStategy, 1);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(strategy);

            ExecuteCommandOnConfigurationServer(command, false);
        }

        public IDistributionStrategy GetDistriubtionStrategy(string collection)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDistriubtionStrategy, 1);
            command.Parameters.AddParameter(collection);

            return ExecuteCommandOnConfigurationServer(command, true) as IDistributionStrategy;
        }

        public IDistribution GetCurrentDistribution(string collection)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetCurrentDistribution, 1);
            command.Parameters.AddParameter(collection);

            return ExecuteCommandOnConfigurationServer(command, true) as IDistribution;
        }

        public IDistribution BalanceData(string collection)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.BalanceData, 1);
            command.Parameters.AddParameter(collection);

            return ExecuteCommandOnConfigurationServer(command, true) as IDistribution;
        }

        protected object  ExecuteCommandOnConfigurationServer(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.ManagementCommand command, bool response)
        {
            ManagementResponse managementResponse = null;
            if (_channel != null)
            {
                try
                {
                    managementResponse = _channel.SendMessage(command, response) as ManagementResponse;
                }
                catch (System.Exception e)
                {
                    throw e;
                }

                if (managementResponse != null && managementResponse.Exception != null)
                {
                    throw new System.Exception(managementResponse.Exception.Message);
                }
            }

            if (managementResponse != null)
                return managementResponse.ReturnVal;

            return null;
        }

        private ManagementCommand GetManagementCommand(string method, int overload)
        {
            ManagementCommand command = new ManagementCommand();
            command.MethodName = method;
            command.Overload = overload;
            return command;
        }


        public void AddConfigurationListener(IConfigurationListener listener)
        {
        }

        public void RemoveConfigurationListener(IConfigurationListener listener)
        {
        }
    }
}
