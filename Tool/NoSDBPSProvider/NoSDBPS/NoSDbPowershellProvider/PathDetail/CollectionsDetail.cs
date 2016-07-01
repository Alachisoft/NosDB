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
using Alachisoft.NosDB.Common.Configuration.DOM;

namespace Alachisoft.NosDB.NosDBPS
{
    public class CollectionsDetail : NodeDetail,IDatabaseContext
    {
        private readonly string _databaseName;

        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }

        }

        internal CollectionsDetail(string databaseName,string nodeName, CollectionConfigurations configuration, string[] pathChunks)
        {
            _databaseName = databaseName;
            NodeName = nodeName;
            NodeType = PathType.Collections;
            IsContainer = true;
            IsValid = true;
            ChilderanTable = new PrintableTable();
            Configuration = configuration;
            ChilderanTable= new PrintableTable();
            ChilderanTable.AddHeader("Name", "Type", "Detail");
            ChilderanName = new List<string>();
            foreach(CollectionConfiguration cConfig in configuration.Configuration.Values)
            {
                ChilderanTable.AddRow(cConfig.CollectionName, "Non Sharded", cConfig.Shard);
            }
            PathChunks = pathChunks;

        }

        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            NodeDetail thisNode = new EndNodeDetail();
            bool sucess = false;
            
            if (((CollectionConfigurations)Configuration).ContainsCollection(PathChunks[0].ToLower()))
            {
                string[] childPathChunks = new string[PathChunks.Length - 1];
                Array.Copy(PathChunks, 1, childPathChunks, 0, PathChunks.Length - 1);
                CollectionConfiguration cConfig= ((CollectionConfigurations)Configuration).GetCollection(PathChunks[0].ToLower());
                thisNode = new CollectionValueDetail(_databaseName, PathChunks[0], cConfig, childPathChunks);
                sucess = true;
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
