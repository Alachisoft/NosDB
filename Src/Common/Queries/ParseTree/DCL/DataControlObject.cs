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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Impl.Enums;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DCL
{
    public class DataControlObject : IDcObject
    {
        private readonly string _userName;
        private readonly string _roleName;
        private readonly ControlType _controlType;
        private readonly ResourceId _resourceIdentifier;

        public DataControlObject(ControlType controlType, string cluster,
            string database, string userName, string roleName)
        {
            _controlType = controlType;
            _userName = userName;
            _roleName = roleName;
            _resourceIdentifier = new ResourceId();
            if (database == null)
            {
                if ("system".Equals(cluster, System.StringComparison.OrdinalIgnoreCase))
                {
                    _resourceIdentifier.ResourceType = ResourceType.System;
                    _resourceIdentifier.Name = MiscUtil.NOSDB_CLUSTER_SERVER;
                }
                else
                {
                    _resourceIdentifier.ResourceType = ResourceType.Cluster;
                    _resourceIdentifier.Name = cluster;
                }
            }
            else
            {
                _resourceIdentifier.ResourceType = ResourceType.Database;
                _resourceIdentifier.Name = cluster.ToLower() + "/" + database.ToLower();
            }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public string RoleName
        {
            get { return _roleName; }
        }

        public ControlType Type
        {
            get { return _controlType; }
        }

        public ResourceId ResourceIdentifier
        {
            get { return _resourceIdentifier; }
        }
    }
}
