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
using System.Threading.Tasks;

namespace Alachisoft.NosDB.NosDBPS
{
    public class ConfigurationNodesDetail:NodeDetail
    {

        public ConfigurationNodesDetail(string nodeName, bool isContainer, bool isValid,
             ConfigClusterInfo configCluster, string[] pathChunks)
        {
            NodeName = nodeName;
            NodeType = PathType.ConfigurationNodes;
            IsContainer = isContainer;
            IsValid = isValid;
            Configuration = configCluster;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanTable.AddHeader("Node IP Address","Status");
            ChilderanName = new List<string>();
            foreach (ServerDetail server in ((ConfigClusterInfo)this.Configuration).Servers)
            {
                ChilderanName.Add(server.IPAddress);
                string statusString = ConfigurationConnection.Current.ConfigServerIP.Equals(server.IPAddress)
                    ? ProviderUtil.ACTIVE
                    : "";
                ChilderanTable.AddRow(server.IPAddress, statusString);
            }
        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            
            NodeDetail thisNode = new EndNodeDetail();
            bool sucess = false;
            List<string> childernNaames = new List<string>();
            foreach (var node in ((ConfigClusterInfo)Configuration).Servers)
            {
                if (node.IPAddress.Equals(PathChunks[0]))
                {
                    childernNaames.Add(node.Priority.ToString());
                    string[] childPathChunks = new string[this.PathChunks.Length - 1];
                    Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
                    
                    thisNode = new ServerValueDetail(PathChunks[0], PathType.ConfigurationNode,  node, childPathChunks);
                    sucess = true;
                }
                
            }
            if (PathChunks.Length == 1)
            {
                nodeDetail = thisNode;
                return sucess;
            }
            return thisNode.TryGetNodeDetail(out nodeDetail);

        }
    }
}
