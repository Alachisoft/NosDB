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
    public class CollectionValueDetail : NodeDetail, IDatabaseContext
    {
        private string _databaseName = null;

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public string Database
        {
            get { return _databaseName; }
        }
        internal CollectionValueDetail(string databaseName, string nodeName, CollectionConfiguration configuration, string[] pathChunks)
        {
            _databaseName = databaseName;
            NodeName = nodeName;
            NodeType = PathType.Collection;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanName = new List<string>();
            ChilderanTable.AddRow(ProviderUtil.INDICES);
            ChilderanName.Add(ProviderUtil.INDICES);

        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            
            NodeDetail thisNodeDetail;
            bool sucess;
            string[] childPathChunks = new string[this.PathChunks.Length - 1];
            Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);

            switch (PathChunks[0].ToLower())
            {
                case ProviderUtil.INDICES:
                    thisNodeDetail = new IndicesValueDetail(PathChunks[0], ((CollectionConfiguration)Configuration).Indices, childPathChunks,DatabaseName);
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
