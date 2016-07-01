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
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    public class ResourceItem : IResourceItem, ICompactSerializable
    {
        public ResourceItem(ResourceId resourceId)
        {
            this.ResourceId = resourceId;
            Roles = new Dictionary<IRole, IRoleInstance>();
            switch (this.ResourceId.ResourceType)
            {
                case ResourceType.System:
                    Roles[Role.sysadmin] = new RoleInstance();
                    Roles[Role.securityadmin] = new RoleInstance();
                    Roles[Role.distributor] = new RoleInstance();
                    break;
                case ResourceType.Cluster:
                case ResourceType.ConfigurationCluster:
                    Roles[Role.clusteradmin] = new RoleInstance();
                    Roles[Role.clustermanager] = new RoleInstance();
                    Roles[Role.dbcreator] = new RoleInstance();
                    break;
                case ResourceType.Database:
                    Roles[Role.db_owner] = new RoleInstance();
                    Roles[Role.db_admin] = new RoleInstance();
                    Roles[Role.db_user] = new RoleInstance();
                    Roles[Role.db_datawriter] = new RoleInstance();
                    Roles[Role.db_datareader] = new RoleInstance();
                    break;
                default:
                    break;
            }
            SubResources = new List<ResourceId>();
        }
        public SecurityInformationTypes SecurityInformationType
        {
            get
            { return SecurityInformationTypes.Resource; }
        }

        public ResourceId ResourceId { set; get; }

        public Dictionary<IRole, IRoleInstance> Roles { set; get; }

        public bool GrantRole(IRole role, string username)
        {
            bool isSuccessful = false;
            if (Roles == null)
                Roles = new Dictionary<IRole, IRoleInstance>();

            IRoleInstance roleInstance;
            if (Roles.ContainsKey(role))
            {
                roleInstance = Roles[role];

                if (!roleInstance.AuthorizedUsers.Contains(username, StringComparer.CurrentCultureIgnoreCase))
                {
                    roleInstance.AuthorizedUsers.Add(username);
                }
                isSuccessful = true;
            }
            return isSuccessful;
        }

        public bool RevokeRole(IRole role, string username)
        {
            bool isSuccessful = false;

            if (Roles.ContainsKey(role))
            {
                IRoleInstance roleInstance = Roles[role];

                if (roleInstance.AuthorizedUsers.Contains(username, StringComparer.CurrentCultureIgnoreCase))
                {
                    if (role.Equals(Role.sysadmin) && roleInstance.AuthorizedUsers.Count == 1)
                    {
                        throw new SecurityException(ErrorHandling.ErrorCodes.Security.LAST_SYSTEM_USER);
                    }
                    int index = roleInstance.AuthorizedUsers.BinarySearch(username, StringComparer.CurrentCultureIgnoreCase);
                    if (index == -1)
                    {
                        roleInstance.AuthorizedUsers.Sort();
                        index = roleInstance.AuthorizedUsers.BinarySearch(username, StringComparer.CurrentCultureIgnoreCase);
                    }

                    if (index >= 0)
                        roleInstance.AuthorizedUsers.RemoveAt(index);

                    isSuccessful = true;
                }
            }
            return isSuccessful;
        }

        public List<ResourceId> SubResources { set; get; }

        public void AddSubResource(ResourceId subResourceId)
        {
            if (!ValidateAddOrRemoveResource(subResourceId))
                return;
            if(!this.SubResources.Contains(subResourceId))
                this.SubResources.Add(subResourceId);
        }

        public void RemoveSubResource(ResourceId subResourceId)
        {
            if (!ValidateAddOrRemoveResource(subResourceId))
                return;
            if (this.SubResources.Contains(subResourceId))
                this.SubResources.Remove(subResourceId);
        }

        private bool ValidateAddOrRemoveResource(ResourceId resourceId)
        {
            switch (this.ResourceId.ResourceType)
            {
                case ResourceType.System:
                    if (resourceId.ResourceType != ResourceType.Cluster)
                        return false;
                    return true;
                case ResourceType.Cluster:
                case ResourceType.ConfigurationCluster:
                    if (resourceId.ResourceType != ResourceType.Database)
                        return false;
                    return true;
                default:
                    return false;
            }
        }

        public void Merge(IResourceItem resourceItem)
        {
            foreach (IRole role in resourceItem.Roles.Keys)
            {
                if (Roles.ContainsKey(role))
                {
                    foreach (string username in resourceItem.Roles[role].AuthorizedUsers)
                    {
                        if(!Roles[role].AuthorizedUsers.Contains(username))
                            Roles[role].AuthorizedUsers.Add(username);
                    }
                }
            }
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            this.ResourceId = reader.ReadObject() as ResourceId;
            this.Roles = Common.Util.SerializationUtility.DeserializeDictionary<IRole, IRoleInstance>(reader);
            //this.Roles = reader.ReadObject() as Dictionary<IRole, IRoleInstance>;
            this.SubResources = Common.Util.SerializationUtility.DeserializeList<ResourceId>(reader);
            //this.SubResources = reader.ReadObject() as List<ResourceId>;
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(this.ResourceId);
            Common.Util.SerializationUtility.SerializeDictionary<IRole, IRoleInstance>(this.Roles, writer);
            //writer.WriteObject(this.Roles);
            Common.Util.SerializationUtility.SerializeList<ResourceId>(this.SubResources, writer);
            //writer.WriteObject(this.SubResources);
        }

        public static IDictionary<string, IList<string>> GetUserInfo(IResourceItem resourceItem)
        {
            IDictionary<string, IList<string>> usersRoleSet = null;
            if (resourceItem != null)
            {
                usersRoleSet = new Dictionary<string, IList<string>>();
                foreach (IRole role in resourceItem.Roles.Keys)
                {
                    foreach (string user in resourceItem.Roles[role].AuthorizedUsers)
                    {
                        if (!usersRoleSet.ContainsKey(user))
                            usersRoleSet[user] = new List<string>();
                        usersRoleSet[user].Add(role.RoleName);
                    }
                }
            }
            return usersRoleSet;
        }

        public object Clone()
        {
            IResourceItem resourceItem = new ResourceItem(this.ResourceId.Clone() as ResourceId);
            resourceItem.Roles = this.Roles != null ? this.Roles.Clone<IRole, IRoleInstance>() : null;
            resourceItem.SubResources = this.SubResources != null ? this.SubResources.Clone<ResourceId>() : null;
            resourceItem.ClusterName = this.ClusterName;
            return resourceItem;
        }


        public string ClusterName { set; get; }
    }
}
