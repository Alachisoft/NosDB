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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Core.Configuration;


namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsCommon.New, "DatabaseCluster")]
    public class CreateDatabaseCluster : PSCmdlet
    {
        private string _shard = string.Empty;
        private string _serverNode;
        private int _shardPort;
        private int _heartBeat = 5;
        private string _clusterName;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "name of database cluster.")]
        [Alias("n")]
        public string Name
        {
            get { return _clusterName; }
            set { _clusterName = value; }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "name of shard to be added in database cluster.")]
        public string Shard
        {
            get { return _shard; }
            set { _shard = value; }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 3,
            HelpMessage = "port of shard to be added in database cluster.")]
        [Alias("p")]
        public int Port
        {
            get { return _shardPort; }
            set { _shardPort = value; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 4,
            HelpMessage =
                "specify IP address and priority of the node, uses local IP resolved by DNS as default. e.g. -node 192.168.1.22[1]"
            )]
        [Alias("s")]
        public string Server
        {
            get { return _serverNode; }
            set { _serverNode = value; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,

            HelpMessage = "specify HeartBeat for shards.")]
        [Alias("H")]
        public int HeartBeat
        {
            get { return _heartBeat; }
            set { _heartBeat = value; }
        }

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(Server))
            {
                Server = ProviderUtil.GetLocalAddress();
            }
        }

        protected override void ProcessRecord()
        {
            var sConfiguration = new ShardConfiguration
            {
                Port = Port,
                Name = Shard,
                NodeHeartbeatInterval = HeartBeat,
                
            };

            var serverNodes = new Dictionary<string, ServerNode>();

            var sNode = new ServerNode {Name = Server.Split('[', ']')[0]};
            
            int priority = 1;

            if (Server.Contains('['))
            {
                string priorityString = Server.Split('[', ']')[1];
                int.TryParse(priorityString, out priority);
                priority = priority > 3 ? 3 : priority;
            }

            sNode.Priority = priority < 1 ? 1 : priority;
            serverNodes.Add(sNode.Name, sNode);

            sConfiguration.Servers = new ServerNodes();
            if (serverNodes.Count > 0)
                sConfiguration.Servers.Nodes = serverNodes;

            ClusterConfiguration config = new ClusterConfiguration
            {
                Name =Common.MiscUtil.CLUSTERED,
                DisplayName = Name,
                Deployment = new DeploymentConfiguration()
            };
            config.Deployment.AddShard(sConfiguration);


            var current = new RemoteConfigurationManager();
            current.CreateCluster(config,new SSPIClientAuthenticationCredential());
            if (current.VerifyConfigurationCluster())
            {
                ConfigurationConnection.ConfigCluster = current.GetConfigurationClusterConfiguration();
                ConfigurationConnection.ClusterConfiguration = current.GetDatabaseClusterConfig(true);
                ConfigurationConnection.Current = current;
                ConfigurationConnection.UpdateDatabaseClusterInfo();

            }
            SessionState.Path.SetLocation(ProviderUtil.DRIVE_ROOT + ":\\" + ConfigurationConnection.ClusterConfiguration.DisplayName);
        }


    }
}
