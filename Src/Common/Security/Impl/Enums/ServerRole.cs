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
    public enum ServerRole
    {
        [Description("A user with the sysadmin role can perform any operation on the NosDB server, including cluster and database managerial operations.")]
        sysadmin,
        [Description("A user with the securityadmin role canperform any data definition (DDL)operation on the users, like CREATE, DROP orALTER users.")]
        securityadmin,
        [Description("The distributor is a special role, used only for the distributor service. A distributor is only authorized to distribute client operations.")]
        distributor

    }
}
