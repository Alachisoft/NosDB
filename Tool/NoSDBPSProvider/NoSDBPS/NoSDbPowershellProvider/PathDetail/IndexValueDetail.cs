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
    public class IndexValueDetail : NodeDetail, IDatabaseContext
    {
        string _databaseName;

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        internal IndexValueDetail(string nodeName,
            IndexConfiguration configuration, string[] pathChunks, string databaseName)
        {
            _databaseName = databaseName;
            NodeName = nodeName;
            NodeType = PathType.Index;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanName = new List<string>();
            ChilderanTable.AddHeader("Attribute Name", "Order");
            if (configuration.Attributes != null)
            {
                ChilderanTable.AddRow(configuration.Attributes.Name, configuration.Attributes.Order);
                ChilderanName.Add(configuration.Attributes.Name);
            }
        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            if (ChilderanName.Contains(PathChunks[0]))
            {
                nodeDetail = new EndNodeDetail(PathChunks[0], PathType.IndexAttribute, true);
                return true;
            }
            else
            {
                nodeDetail = new EndNodeDetail();
                return false;
            }
        }


    }
}
