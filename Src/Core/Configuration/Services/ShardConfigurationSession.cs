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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Protobuf;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.RPCFramework;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Alachisoft.NosDB.Common.Communication.Formatters;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using Google.ProtocolBuffers;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ShardConfigurationSession : IShardConfigurationSession,  IRequestListener
    {
        public ConfigurationProvider ConfigurationProvider { set; get; }

        private string cluster;
        private string shard;
        private ServerNode server;
        private ConfigurationServer service;
        private DateTime sessionStartTime;
        private string sessionId;

        private IDictionary<Server, IDualChannel> _configChannels = null;
        private ITraceProvider _traceProvider = null;
        private IChannelFormatter _channelFormatter = new ShardChannelFormatter();
        IList<IConfigurationListener> configurationChangeListener = new List<IConfigurationListener>();

        public ShardConfigurationSession(ConfigurationServer service, string cluster,string shard,ServerNode server)
        {
            this.service = service;
            this.cluster = cluster;
            this.shard = shard;
            this.server = server;
            this.sessionStartTime = DateTime.Now;
            this.sessionId = Guid.NewGuid().ToString();

            this.ConfigurationProvider = new ConfigurationProvider();

            this._traceProvider = new TraceProvider();
        }

        public string Cluster
        {
            get { return this.cluster; }
        }

        public string Shard
        {
            get { return this.shard; }
        }

        public bool IsValid
        {
            get { throw new NotImplementedException(); }
        }

        public string SessionId
        {
            get { return sessionId; }
        }

        public DateTime SessionStartTime
        {
            get { return sessionStartTime; }
        }

        public void Close()
        {
            service.Stop();
            configurationChangeListener.Clear();
        }

        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {
            try
            {
                return service.GetDatabaseClusterConfiguration(clusterName);
            }
            catch(System.Exception ex)
            {

            }
            return null;
        }

        public void SetNodeStatus(ServerNode primary, NodeRole status)
        {
            try
            {
                this.service.SetNodeStatus(this.Cluster, this.shard, primary, status);
                NotifyConfigurationChange(this.cluster);
            }
            catch(System.Exception ex)
            { 

            }
        }

        public Membership[] GetMembershipInfo()
        {
            try
            {
                List<ShardConfiguration> shardList = service.GetDatabaseClusterConfiguration(this.cluster).Deployment.Shards.Values.ToList<ShardConfiguration>();
                if (shardList.Count > 0)
                {
                    Membership[] allMemberShip = new Membership[shardList.Count];
                    int index = 0;
                    foreach (ShardConfiguration sc in shardList)
                    {
                        allMemberShip[index] = this.service.GetMembershipInfo(this.cluster, sc.Name);
                    }

                    return allMemberShip;
                }
                else
                {
                    return null;
                }
            }
            catch(System.Exception ex)
            {

            }

            return null;
        }

        public Membership GetMembershipInfo(string shard)
        {
            try
            {
                return this.service.GetMembershipInfo(this.cluster, shard);
            }
            catch(System.Exception ex)
            {

            }

            return null;
        }

        public void UpdateNodeStatus(Status status)
        {
            try
            {
                this.service.UpdateNodeStatus(this.cluster, this.shard, this.server, status);
                NotifyConfigurationChange(this.cluster);
            }
            catch(System.Exception ex)
            {

            }
        }

        public Object BeginElection(ServerNode server, ElectionType electionType)
        {
            try
            {
                return this.service.BeginElection(this.cluster, this.shard, server, electionType);
            }
            catch (System.Exception ex)
            {


            }
            return null;
        }


        public void SubmitElectionResult(ElectionResult result)
        {
            try
            {
                this.service.SubmitElectionResult(this.cluster, this.shard, result);

            }
            catch (System.Exception ex)
            {

            }
        }

        public void EndElection(ElectionId electionId)
        {
            try
            {
                this.service.EndElection(this.cluster, this.shard, electionId);
                NotifyConfigurationChange(this.cluster);
            }
            catch (System.Exception ex)
            {

            }
        }
       


       

       

        public void ConfigureDistributionStategy(string collection, IDistributionStrategy strategy)
        {
            try
            {
                this.service.ConfigureDistributionStategy(this.cluster, this.shard, collection, strategy,true);
                NotifyConfigurationChange(cluster);
            }
            catch(System.Exception ex)
            {

            }
        }

        public IDistributionStrategy GetDistriubtionStrategy(string collection)
        {
            return this.service.GetDistriubtionStrategy(this.cluster, this.shard, collection);
        }

        public IDistribution GetCurrentDistribution(string collection)
        {
            return this.service.GetCurrentDistribution(this.cluster, this.shard, collection);
        }

        public IDistribution BalanceData(string collection)
        {
            return this.service.BalanceData(this.cluster, this.shard, collection);
        }

        protected object[] GetTargetMethodParameters(byte[] graph)
        {
            TargetMethodParameter parameters = CompactBinaryFormatter.FromByteBuffer(graph, "ok") as TargetMethodParameter;
            return parameters.ParameterList.ToArray();
        }
        
        #region IRequestListener Members
        public object OnRequest(IRequest request)
        {
            if (request.Message is ManagementCommand)
            {
                //ConfigurationProvider.Provider = service;
                ManagementCommand command = (ManagementCommand)request.Message;

                ManagementResponse response = new ManagementResponse();
                response.MethodName = command.MethodName;
                response.Version = command.CommandVersion;
                response.RequestId = command.RequestId;
                response.ReturnVal = CompactBinaryFormatter.ToByteBuffer(this.ConfigurationProvider.ManagementRpcService.InvokeMethodOnTarget(command.MethodName,
                        command.Overload,
                        GetTargetMethodParameters(CompactBinaryFormatter.ToByteBuffer(command.Parameters,null))),null);

                return response;
            }
            else
                return new object();
        } 
        #endregion
        
        public void AddConfigurationListener(IConfigurationListener listener)
        {
            if (!configurationChangeListener.Contains(listener))
                configurationChangeListener.Add(listener);
        }

        public void RemoveConfigurationListener(IConfigurationListener listener)
        {
            if (configurationChangeListener.Contains(listener))
                configurationChangeListener.Remove(listener);
        }

        private void NotifyConfigurationChange(string cluster)
        {
            if (configurationChangeListener != null)
            {
                foreach (IConfigurationListener listener in configurationChangeListener)
                {
                    //listener.OnClusterConfigurationChanged(cluster);
                }
            }
        }


        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            throw new NotImplementedException("Shard Configuration");
        }
    }
}
