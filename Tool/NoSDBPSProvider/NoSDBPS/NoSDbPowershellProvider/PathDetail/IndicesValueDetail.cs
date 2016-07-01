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
using Alachisoft.NosDB.Common.Configuration.DOM;

namespace Alachisoft.NosDB.NosDBPS
{
    public class IndicesValueDetail : NodeDetail, IDatabaseContext
    {
        private string _databaseName;

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        internal IndicesValueDetail(string nodeName,
            Indices configuration, string[] pathChunks, string databaseName)
        {

            NodeName = nodeName;
            NodeType = PathType.Indexes;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanTable.AddHeader("Indices");
            ChilderanName = new List<string>();
            if (configuration.IndexConfigurations != null)
            {
                foreach (IndexConfiguration index in configuration.IndexConfigurations.Values)
                {
                    ChilderanName.Add(index.IndexName);
                    ChilderanTable.AddRow(index.IndexName);
                }
            }
        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            NodeDetail thisNode = null;
            bool sucess = false;
            if (((Indices)Configuration).ContainsIndex(PathChunks[0]))
            {

                IndexConfiguration dbconfig = (((Indices)Configuration).GetIndex(PathChunks[0]));
                string[] childPathChunks = new string[this.PathChunks.Length - 1];
                Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
                thisNode = new IndexValueDetail(PathChunks[0], dbconfig, childPathChunks, _databaseName);
                sucess = true;
            }
            if (PathChunks.Length == 1)
            {
                nodeDetail = new EndNodeDetail();
                return sucess;
            }
            return thisNode.TryGetNodeDetail(out nodeDetail);
        }


    }
}
