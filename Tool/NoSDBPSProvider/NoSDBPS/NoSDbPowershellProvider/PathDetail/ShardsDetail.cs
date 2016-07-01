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
    public class ShardsDetail:NodeDetail
    {

        public ShardsDetail(string nodeName, DeploymentConfiguration configuration,  string[] pathChunks)
        {
            NodeName = nodeName;
            NodeType = PathType.Shards;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            ChilderanTable = new PrintableTable();
            PathChunks = pathChunks;
            ChilderanName = new List<string>();
            ChilderanTable.AddHeader( "Shard Name", "Node(s)", "Status","Shard Port");
            ClusterInfo clusterInfo = ConfigurationConnection.ClusterInfo; 
            foreach (ShardConfiguration shard in configuration.Shards.Values)
            {
                List<string>  status = new List<string>();
                ShardInfo shardInfo = clusterInfo.GetShard(shard.Name);
                if (shardInfo !=null)
                {
                    foreach (String node in shard.Servers.Nodes.Keys)
                    {
                        string statusString = "";
                        if(shardInfo.RunningNodes.Values.ToList().Exists(k=>k.Address.IpAddress.ToString().Equals(node)))
                        {

                            statusString = ProviderUtil.STARTED;

                            if (shardInfo.Primary!= null)
                                if (shardInfo.Primary.Address.IpAddress.ToString().Equals(node))
                                    statusString += ProviderUtil.PRIMARY_STATUS;
                                
                            
                        }
                        else
                        {
                            statusString = ProviderUtil.STOPPED;
                        }
                        status.Add(statusString);
                    }
                }

                ChilderanTable.AddMultiLineRow(new string[] { shard.Name }, shard.Servers.Nodes.Keys.ToArray(), status.ToArray(), new string[] { shard.Port.ToString() });
                ChilderanName.Add(shard.Name);
            }
        }

        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            NodeDetail thisNode = new EndNodeDetail();
            List<string> childernNaames = new List<string>();
            DeploymentConfiguration config = (DeploymentConfiguration)this.Configuration;
            foreach (ShardConfiguration singleShard in config.Shards.Values)
            {
                if (singleShard.Name.ToLower().Equals(PathChunks[0].ToLower()))
                {
                    
                    string[] childPathChunks = new string[this.PathChunks.Length - 1];
                    Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
                    thisNode = new ShardValueDetail(PathChunks[0],  singleShard, childPathChunks);
                    

                }
            }
            if(PathChunks.Length==1)
            {
                nodeDetail = thisNode;
                return true;
            }

            return thisNode.TryGetNodeDetail(out nodeDetail);
        }
    }
}
