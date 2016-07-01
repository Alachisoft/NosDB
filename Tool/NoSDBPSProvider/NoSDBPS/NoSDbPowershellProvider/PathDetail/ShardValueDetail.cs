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
using Alachisoft.NosDB.Common.Configuration;

using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.NosDBPS
{
    public class ShardValueDetail:NodeDetail
    {

        public ShardValueDetail(string nodeName, ShardConfiguration configuration,  string[] pathChunks)
        {
            NodeName = nodeName;
            NodeType = PathType.Shard;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanName = new List<string>();
            ClusterInfo clusterInfo = ConfigurationConnection.ClusterInfo;
            ShardInfo shardInfo = clusterInfo.GetShard(nodeName);
            List<ServerInfo> running = new List<ServerInfo>();
            if (shardInfo != null)
            {
                if (shardInfo.RunningNodes != null)
                    running = shardInfo.RunningNodes.Values.ToList();
            }
            ChilderanTable.AddHeader("Node IP Address", "Running Status", "Primary Status", "Priority");
            
            foreach (ServerNode node in configuration.Servers.Nodes.Values)
            {
                ServerInfo serverInfo = null;
                string isPrimery = ProviderUtil.NOT_PRIMARY; 
                string isRunning = ProviderUtil.NOT_RUNNING;
                if (running != null)
                    serverInfo = running.Find(x => x.Address.ip.Equals(node.Name));

                if (serverInfo != null)
                {
                    if (running.Contains(serverInfo))
                        isRunning = ProviderUtil.RUNNING;
                    if (shardInfo.Primary != null && serverInfo.Equals(shardInfo.Primary))
                        isPrimery = ProviderUtil.PRIMARY;
                }
                ChilderanTable.AddRow(node.Name, isRunning, isPrimery,node.Priority.ToString());
                ChilderanName.Add(node.Name);
            }

        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            NodeDetail thisNode = new EndNodeDetail();
            List<string> childernNaames = new List<string>();
            ShardConfiguration shardConfig = (ShardConfiguration)Configuration;
            foreach (ServerNode node in shardConfig.Servers.Nodes.Values)
            {
                if (node.Name.Equals(PathChunks[0]))
                {
                    childernNaames.Add(node.Priority.ToString());
                    string[] childPathChunks = new string[this.PathChunks.Length - 1];
                    Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
                    ServerDetail s1 = new ServerDetail()
                    {
                        IPAddress = node.Name,
                        Priority = node.Priority
                    };
                    thisNode = new ServerValueDetail(PathChunks[0], PathType.Server, s1, childPathChunks);
                    
                }
            }
            if (PathChunks.Length == 1)
            {
                nodeDetail = thisNode;
                return true;
            }

            return thisNode.TryGetNodeDetail(out nodeDetail);
        }
    }
}
