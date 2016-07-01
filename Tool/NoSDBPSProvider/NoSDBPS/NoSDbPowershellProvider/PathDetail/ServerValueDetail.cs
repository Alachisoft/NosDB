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
    public class ServerValueDetail:NodeDetail
    {

        public ServerValueDetail(string nodeName,PathType nodeType, ServerDetail configuration,  string[] pathChunks)
        {
            NodeName = nodeName;
            NodeType = nodeType;
            IsContainer = true;
            IsValid = true;
            Configuration = configuration;
            PathChunks = pathChunks;
            ChilderanTable = new PrintableTable();
            ChilderanTable.AddHeader("Priority");
            ChilderanTable.AddRow(((ServerDetail)configuration).Priority.ToString());
        }
        public override bool TryGetNodeDetail(out NodeDetail nodeDetail)
        {
            NodeDetail thisNode = new EndNodeDetail();
            bool sucess = false;
            if (((ServerDetail)this.Configuration).IPAddress.Equals(PathChunks[0]))
            {
                string[] childPathChunks = new string[this.PathChunks.Length - 1];
                Array.Copy(this.PathChunks, 1, childPathChunks, 0, this.PathChunks.Length - 1);
            
                thisNode = new EndNodeDetail(PathChunks[0], PathType.Priority, true);
                sucess = true;
            }

            if (PathChunks.Length == 1)
            {
                nodeDetail = thisNode;
                return sucess;
            }
            Array.Copy(this.PathChunks, 1, thisNode.PathChunks, 0, this.PathChunks.Length - 1);
            return thisNode.TryGetNodeDetail(out nodeDetail);
        }
    }
}
