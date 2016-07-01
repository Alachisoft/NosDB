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
using System.ComponentModel;

namespace Alachisoft.NosDB.Common.Security.Impl.Enums
{
    public enum ClusterRole
    {
        [Description("A user with the clusteradmin role can GRANT and REVOKE roles over the cluster and can perform managerial operations on the cluster like adding and removingthe shards and nodes of the cluster.")]
        Clusteradmin,
        [Description("A user with the clustermanager role can perform start/stop operations on the shards and nodes of the cluster.")]
        Clustermanager,
        [Description("A user with the dbcreator role can perform any data definition (DDL) operation on the databases, like CREATE, DROP or ALTER databases over the cluster.")]
        Dbcreator
    }
}
