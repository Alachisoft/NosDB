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
using System.Management.Automation;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
namespace Alachisoft.NosDB.NosDBPS
{
    public class ConfigManagerDetail:NodeDetail
    {
        internal ConfigManagerDetail(string nodeName,  bool isContainer, bool isValid,
            ClusterConfiguration configuration,  string[] pathChunks)
        {
            NodeName = nodeName;
            NodeType = PathType.ConfigurationManager;
            IsContainer = isContainer;
            IsValid = isValid;
            Configuration = configuration;
            ChilderanTable = new PrintableTable(" ");
            ChilderanName = new List<string>();
            ChilderanTable.AddRow(ProviderUtil.DATABASES);
            ChilderanName.Add(ProviderUtil.DATABASES);
            if (!ConfigurationConnection.ClusterConfiguration.DisplayName.Equals(MiscUtil.STAND_ALONE))
            {
                ChilderanTable.AddRow(ProviderUtil.SHARDS);
                ChilderanName.Add(ProviderUtil.SHARDS);
                ChilderanTable.AddRow(ProviderUtil.CONFIGCLUSTER);
                ChilderanName.Add(ProviderUtil.CONFIGCLUSTER);
            }

            ChilderanTable.GetTableRows();
            PathChunks = pathChunks;

        }

        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            NodeDetail thisNodeDetail;
            bool sucess;
            List<object> childernNaames = new List<object>();
            string[] childPathChunks = new string[this.PathChunks.Length - 1];
            Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
            
            switch(PathChunks[0].ToLower())
            {

                case ProviderUtil.SHARDS:
                    DeploymentConfiguration deployment = ((ClusterConfiguration)Configuration).Deployment;
                    
                    thisNodeDetail = new ShardsDetail(PathChunks[0], deployment, childPathChunks);
                    
                    sucess = true;
                    break;
                case ProviderUtil.DATABASES:
                    DatabaseConfigurations databasees = ((ClusterConfiguration)Configuration).Databases;
                    
                    thisNodeDetail = new DatabasesDetail(PathChunks[0], databasees, childPathChunks);
                        
                    sucess = true;
                    break;
                case ProviderUtil.CONFIGCLUSTER:
                    
                    thisNodeDetail = new ConfigurationNodesDetail(PathChunks[0], true, true,
                        new ConfigClusterInfo(ConfigurationConnection.ConfigCluster), childPathChunks);
                    
                    sucess = true;
                    break;
                default:
                    thisNodeDetail = null;
                    sucess = false;
                    break;

            }

            if (PathChunks.Length == 1)
            {
                nodeDetail = thisNodeDetail;
                return sucess;
            }
            return thisNodeDetail.TryGetNodeDetail(out nodeDetail);
            
        }
    }
}
