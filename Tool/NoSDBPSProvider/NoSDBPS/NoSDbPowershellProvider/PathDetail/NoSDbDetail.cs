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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System.Management.Automation;

namespace Alachisoft.NosDB.NosDBPS
{
    public class NoSDbDetail:NodeDetail
    {
        internal NoSDbDetail(string[] pathChunks, PSDriveInfo drive)
        {
            if (ConfigurationConnection.ClusterConfiguration != null)
            {
                PathChunks = pathChunks;
                Drive = (NosDBPSDriveInfo)drive;
                ClusterConfiguration configuration = ConfigurationConnection.ClusterConfiguration;
                new ConfigClusterInfo(ConfigurationConnection.ConfigCluster);
                Configuration = configuration;
                IsContainer = true;
                IsValid = true;
                NodeType = PathType.NoSDB;
                ChilderanName = new List<string>();
                ChilderanTable = new PrintableTable();
                ChilderanTable.AddHeader("Database Cluster");
                ChilderanTable.AddRow(ConfigurationConnection.ClusterConfiguration.DisplayName);
                ChilderanName.Add(ConfigurationConnection.ClusterConfiguration.DisplayName);
            }

        }

        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            if (PathChunks.Length == 0)
            {
                nodeDetail = new EndNodeDetail();
                return false;
            }
            if (string.IsNullOrEmpty(ConfigurationConnection.ClusterConfiguration.DisplayName) || (!PathChunks[0].Equals(ConfigurationConnection.ClusterConfiguration.DisplayName)))
            {
                nodeDetail = this;
                return false;
            }
            NodeDetail thisNode = new EndNodeDetail(); 
            bool sucess = false;
            string[] childPathChunks = new string[this.PathChunks.Length - 1];
            Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
            if (PathChunks[0].Equals(ConfigurationConnection.ClusterConfiguration.DisplayName))
            {
                thisNode = new ConfigManagerDetail(PathChunks[0], true, true, (ClusterConfiguration)this.Configuration,
                      childPathChunks);
            }
            else if (PathChunks[0].Equals(MiscUtil.STAND_ALONE))
            {
                thisNode = new LocalDatabaseDetail(PathChunks[0],true,true,(ClusterConfiguration)this.Configuration
                    , childPathChunks);

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
