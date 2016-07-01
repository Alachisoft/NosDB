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

﻿using Alachisoft.NosDB.Common.Server.Engine.Impl;

﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;

namespace Alachisoft.NosDB.Common.JSON.CustomConverter
{
    class SecurityInformationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IResourceItem).IsAssignableFrom(objectType) || typeof(IRole).IsAssignableFrom(objectType) || typeof(IRoleInstance).IsAssignableFrom(objectType) || typeof(IRole).IsAssignableFrom(objectType) || typeof(PermissionEnum).IsAssignableFrom(objectType) || typeof(ResourceId).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var securityInformationType = (SecurityInformationTypes)((int)jObject["SecurityInformationType"]);
            switch (securityInformationType)
            {
                case SecurityInformationTypes.Resource:

                    var resourceId = new ResourceId();
                    JToken idToken = jObject["ResourceId"];
                    resourceId.ResourceType = (ResourceType)(int)idToken["ResourceType"];
                    resourceId.Name = (string)idToken["Name"];
                    var resourceItem = new ResourceItem(resourceId);

                    IDictionary<IRole, IRoleInstance> roles = new Dictionary<IRole, IRoleInstance>();
                    var rolesArray = (JArray)jObject["Roles"];
                    foreach (var jToken in rolesArray)
                    {
                        var childArray = (JArray) jToken;
                        var childRole = new Role();
                        JToken keyToken = childArray[0];
                        childRole.RoleName = (string)keyToken["RoleName"];
                        childRole.RoleType = (RoleType)(int)keyToken["RoleType"];
                        JArray permissions = (JArray)keyToken["Permissions"];
                        foreach (var jToken1 in permissions)
                        {
                            var permissionJo = (JObject) jToken1;
                            var permissionInst = new Permission();
                            permissionInst.OperationType = (OperationType)(int)permissionJo["OperationType"];
                            permissionInst.ResourceType = (ResourceType)(int)permissionJo["ResourceType"];
                            childRole.Permissions.Add(permissionInst);
                        }

                        RoleInstance roleInstanceObj = new RoleInstance();
                        JToken valueToken = childArray[1];
                        JArray authorizedUsers = (JArray)valueToken["AuthorizedUsers"];
                        foreach (JValue authorizedUserJO in authorizedUsers)
                        {
                            roleInstanceObj.AuthorizedUsers.Add((string)authorizedUserJO);
                        }

                        if (!roles.ContainsKey(childRole))
                        {
                            resourceItem.Roles[childRole] = roleInstanceObj;
                        }
                    }

                    List<ResourceId> subResources = new List<ResourceId>();
                    JArray subResourcesArray = (JArray)jObject["SubResources"];
                    foreach (JToken subResourceJT in subResourcesArray)
                    {
                        ResourceId subResourceId = new ResourceId();
                        subResourceId.Name = (string)subResourceJT["Name"];
                        subResourceId.ResourceType = (ResourceType)(int)subResourceJT["ResourceType"];
                        subResources.Add(subResourceId);
                    }
                    resourceItem.SubResources = subResources;
                    //resourceItem.Roles = roles;

                    resourceItem.ClusterName = jObject["ClusterName"].ToString();

                    //serializer.Populate(jObject.CreateReader(), resourceItem);
                    return resourceItem;
                case SecurityInformationTypes.Role:
                    Role role = new Role();
                    serializer.Populate(jObject.CreateReader(), role);
                    return role;
                case SecurityInformationTypes.User:
                    string username = jObject["UserName"].ToString();
                    User user = new User(username);
                    return user;
                case SecurityInformationTypes.RoleInstance:
                    RoleInstance roleInstance = new RoleInstance();
                    serializer.Populate(jObject.CreateReader(), roleInstance);
                    return roleInstance;
                case SecurityInformationTypes.Permission:
                    Permission permission = new Permission();
                    serializer.Populate(jObject.CreateReader(), permission);
                    return permission;
                case SecurityInformationTypes.ResourceId:
                    ResourceId resourceIdentity = new ResourceId();
                    serializer.Populate(jObject.CreateReader(), resourceIdentity);
                    return resourceIdentity;
                default:
                    return serializer.Deserialize(reader);
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value is IResourceItem)
            {
                IResourceItem resourceItem = value as IResourceItem;
                if (resourceItem != null)
                {
                    writer.WritePropertyName("ResourceId");
                    serializer.Serialize(writer, resourceItem.ResourceId);
                    writer.WritePropertyName("SecurityInformationType");
                    serializer.Serialize(writer, resourceItem.SecurityInformationType);
                    writer.WritePropertyName("ClusterName");
                    serializer.Serialize(writer, resourceItem.ClusterName);
                    writer.WritePropertyName("Roles");
                    writer.WriteStartArray();
                    foreach (var kvp in resourceItem.Roles)
                    {
                        writer.WriteStartArray();
                        serializer.Serialize(writer, kvp.Key);
                        serializer.Serialize(writer, kvp.Value);
                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                    writer.WritePropertyName("SubResources");
                    writer.WriteStartArray();
                    foreach (ResourceId resourceId in resourceItem.SubResources)
                    {
                        serializer.Serialize(writer, resourceId);
                    }
                    writer.WriteEndArray();
                }
            }
            else if (value is IRole)
            {
                IRole role = value as IRole;
                if(role != null)
                {
                    writer.WritePropertyName("RoleName");
                    serializer.Serialize(writer, role.RoleName);
                    writer.WritePropertyName("RoleType");
                    serializer.Serialize(writer, role.RoleType);
                    writer.WritePropertyName("SecurityInformationType");
                    serializer.Serialize(writer, role.SecurityInformationType);
                    writer.WritePropertyName("Permissions");
                    writer.WriteStartArray();
                    foreach (Permission permission in role.Permissions)
                    {
                        serializer.Serialize(writer, permission);
                    }
                    writer.WriteEndArray();
                }
            }
            else if (value is User)
            {
                User user = value as User;
                if (user != null)
                {
                    writer.WritePropertyName("UserName");
                    serializer.Serialize(writer, user.Username);
                    writer.WritePropertyName("SecurityInformationType");
                    serializer.Serialize(writer, user.SecurityInformationType);
                }
            }
            else if(value is IRoleInstance)
            {
                IRoleInstance roleInstance = value as IRoleInstance;
                if (roleInstance != null)
                {
                    writer.WritePropertyName("SecurityInformationType");
                    serializer.Serialize(writer, roleInstance.SecurityInformationType);
                    writer.WritePropertyName("AuthorizedUsers");
                    writer.WriteStartArray();
                    foreach (string user in roleInstance.AuthorizedUsers)
                    {
                        serializer.Serialize(writer, user);
                    }
                    writer.WriteEndArray();
                }
            }
            else if (value is Permission)
            {
                Permission permission = value as Permission;
                if(permission != null)
                {
                    writer.WritePropertyName("OperationType");
                    serializer.Serialize(writer, permission.OperationType);
                    writer.WritePropertyName("ResourceType");
                    serializer.Serialize(writer, permission.ResourceType);
                    writer.WritePropertyName("SecurityInformationType");
                    serializer.Serialize(writer, permission.SecurityInformationType);
                }
            }
            else if (value is ResourceId)
            {
                ResourceId resourceId = value as ResourceId;
                if (resourceId != null)
                {
                    writer.WritePropertyName("Name");
                    serializer.Serialize(writer, resourceId.Name);
                    writer.WritePropertyName("ResourceType");
                    serializer.Serialize(writer, resourceId.ResourceType);
                    writer.WritePropertyName("SecurityInformationType");
                    serializer.Serialize(writer, resourceId.SecurityInformationType);
                }
            }
            writer.WriteEndObject();
        }
    }
}
