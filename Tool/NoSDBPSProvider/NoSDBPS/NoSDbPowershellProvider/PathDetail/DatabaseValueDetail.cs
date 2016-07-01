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
using Alachisoft.NosDB.Common.Configuration.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.NosDBPS
{
    public class DatabaseValueDetail:NodeDetail,IDatabaseContext
    {
        public string DatabaseName
        {
            get { return NodeName; }
        }

        public DatabaseValueDetail(string nodeName, DatabaseConfiguration configuration, string[] pathChunks)
        {
            NodeName = nodeName;
            NodeType = PathType.Database;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanName = new List<string>();
            ChilderanTable.AddRow(ProviderUtil.COLLECTIONS);
            ChilderanName.Add(ProviderUtil.COLLECTIONS);
            ChilderanTable.AddRow(ProviderUtil.FUNCTIONS);
            ChilderanName.Add(ProviderUtil.FUNCTIONS);


        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            
            NodeDetail thisNodeDetail;
            bool sucess;
            string[] childPathChunks = new string[this.PathChunks.Length - 1];
            Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
            
            switch(PathChunks[0].ToLower())
            {
                case ProviderUtil.COLLECTIONS:
                    thisNodeDetail = new CollectionsDetail(NodeName,PathChunks[0], ((DatabaseConfiguration)Configuration).Storage.Collections,
                         childPathChunks); 
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
