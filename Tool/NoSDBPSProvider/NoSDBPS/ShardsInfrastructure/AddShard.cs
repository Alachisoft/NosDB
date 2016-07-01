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
using System.Management.Automation;
using Alachisoft.NosDB.Common.Configuration;

namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsCommon.Add,"Shard")]
    public class AddShard:PSCmdlet
    {
        private string _shard = string.Empty;
        private string _serverNode;
        private int _shardPort;
        private int _heartBeat= 5;
       
        [Parameter(
            Mandatory=true,
            ValueFromPipelineByPropertyName=true,
            ValueFromPipeline=true,
            Position= 1,
            HelpMessage = "name of shard to be added in database cluster.")]
        [Alias("n")]
        public string Name
        {
            get { return _shard; }
            set { _shard = value; }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "port of shard to be added in database cluster.")]
        [Alias("p")]
        public int Port
        {
            get { return _shardPort; }
            set { _shardPort = value; }
        }

        [Parameter(
            Mandatory=false,
            ValueFromPipelineByPropertyName=true,
            ValueFromPipeline=true,
            Position= 3,
            HelpMessage = "specify IP address and priority of the shard node, uses local IP resolved by DNS as default. e.g. -node 192.168.1.22[1]")]
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
            const string exceptionString = "Invalid context, to Add-Shard you must be in \n NoSDB:\\databasecluster\\Shards \n ";
            
            SessionState s1 = SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path, s1.Drive.Current);
            
            if (!(s1.Drive.Current is NosDBPSDriveInfo))
                throw new Exception(exceptionString);

            if (ConfigurationConnection.ConfigCluster == null)
                throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
            
            NodeDetail thisNode;
            new NoSDbDetail(pathChunks, s1.Drive.Current).TryGetNodeDetail(out thisNode);
                
            if (!thisNode.NodeType.Equals(PathType.Shards))
                throw new Exception(exceptionString);
        }

        protected override void ProcessRecord()
        {
            var sConfiguration = new ShardConfiguration
            {
                Port = Port,
                Name = Name,
                NodeHeartbeatInterval = HeartBeat
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
            try
            {
                if (ConfigurationConnection.Current.AddShardToCluster(sConfiguration, null))
                {
                    WriteObject("Shard Added Sucessfully");
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }


    }
}
