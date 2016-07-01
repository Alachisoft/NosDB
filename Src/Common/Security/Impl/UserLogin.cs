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
using Alachisoft.NosDB.Common.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    public class UserLogin : IUserLogin
    {
        public string Username { set; get; }

        public IDictionary<IRole, IList<ResourceId>> Roles { set; get; }

        public bool IsAuthorized(Permission permission, ResourceId resourceId)
        {
            if (permission.Equals(Permission.Read_Configuration_Cluster_Configuration) || permission.Equals(Permission.Read_Database_Cluster_Configuration))
                return true;
            foreach (IRole role in Roles.Keys)
            {
                if (role.HasPermission(permission))
                {
                    IList<ResourceId> resources = Roles[role];
                    if (resources != null && resources.Contains(resourceId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool GrantRole(IRole role, ResourceId resourceId)
        {
            if (!Roles.ContainsKey(role))
            {
                Roles[role] = new List<ResourceId>();
            }
            Roles[role].Add(resourceId);
            return true;
        }

        public bool RevokeRole(IRole role, ResourceId resourceId)
        {
            if (Roles.ContainsKey(role))
            {
                Roles[role].Remove(resourceId);
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            UserLogin user = obj as UserLogin;
            if (user != null)
                isEqual = user.Username.Equals(this.Username);
            return isEqual;
        }
    }
}
