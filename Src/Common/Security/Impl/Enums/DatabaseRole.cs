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
    public enum DatabaseRole
    {

        [Description("A user with the db_owner role can perform all managerial operations on the database as performed by the db_admin, along with having the authorization to DROP the database.")]
        db_owner,
        [Description("A user with the db_admin role can CREATE, DROP or ALTER a collection/index/stored procedure/CLR function/CLR trigger in NosDB. In addition, a db_admin can also GRANT and REVOKE roles to/from a user on the database.")]
        db_admin,
        [Description("A user with the db_user role can perform read and write operations on the database. db_user is a combination of db_datareader and db_datawriter.")]
        db_user,
        [Description("A user with the db_datawriter role can perform only write (INSERT/UPDATE/DELETE) operations on the database.")]
        db_datawriter,
        [Description("A user with the db_datareader role can perform only read operations on the database.")]
        db_datareader


    }
}
