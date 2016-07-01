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
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    public class Role : IRole, ICompactSerializable
    {
        public Role()
        {
            Permissions = new List<Permission>();
            ParentRole = new List<IRole>();
        }
        public SecurityInformationTypes SecurityInformationType
        {
            get
            { return SecurityInformationTypes.Role; }
        }

        public string RoleName { set; get; }
        public RoleType RoleType { set; get; }
        public List<Permission> Permissions { set; get; }

        public List<IRole> ParentRole { get; set; }
        public RoleLevel RoleLevel { get; set; }
        public bool HasPermission(Permission requestedPermission)
        {
            if (Permissions != null && requestedPermission != null && Permissions.Contains(requestedPermission))
                return true;
            else if (ParentRole != null)
            {
                foreach (IRole role in ParentRole)
                {
                    if (role != null && role.HasPermission(requestedPermission))
                        return true;
                }
            }
            return false;
        }

        public static IRole GetRole(Permission permission)
        {
            switch (permission.ResourceType)
            {
                case ResourceType.ConfigurationCluster:
                    switch (permission.OperationType)
                    {
                        case OperationType.Read:
                            return Role.configreader;
                        case OperationType.Create:
                        case OperationType.Delete:
                        case OperationType.Modify:
                        case OperationType.Start:
                        case OperationType.Stop:
                        default:
                            return Role.sysadmin;
                    }
                case ResourceType.Cluster:
                    switch (permission.OperationType)
                    {
                        case OperationType.Read:
                            return Role.configreader;
                        case OperationType.Create:
                        case OperationType.Delete:
                        case OperationType.Modify:
                            return Role.sysadmin;
                        case OperationType.Start:
                        case OperationType.Stop:
                        default:
                            return Role.clustermanager;
                    }
                case ResourceType.StoreProcedure:
                case ResourceType.UserDefinedFunction:
                case ResourceType.Collection:
                case ResourceType.Trigger:
                case ResourceType.Index:
                    {
                        switch (permission.OperationType)
                        {
                            case OperationType.Create:
                            case OperationType.Delete:
                            default:
                                return Role.db_admin;
                        }
                    }
                case ResourceType.User:
                    return Role.dbcreator;
                case ResourceType.Database:
                    switch (permission.OperationType)
                    {
                        case OperationType.Read:
                        case OperationType.Write:
                            return Role.db_user;
                        case OperationType.Create:
                        case OperationType.Delete:
                        default:
                            return Role.dbcreator;
                    }
                default:
                    return Role.configreader;
            }
        }

        public static IRole GetRoleByName(string roleName)
        {
            DCLRole dclRole;
            try
            {
                dclRole = (DCLRole)System.Enum.Parse(typeof(DCLRole), roleName, true);
                switch (dclRole)
                {
                    case DCLRole.sysadmin:
                        return Role.sysadmin;
                    case DCLRole.securityadmin:
                        return Role.securityadmin;
                    case DCLRole.clusteradmin:
                        return Role.clusteradmin;
                    case DCLRole.clustermanager:
                        return Role.clustermanager;
                    case DCLRole.dbcreator:
                        return Role.dbcreator;
                    case DCLRole.distributor:
                        return Role.distributor;
                    case DCLRole.db_owner:
                        return Role.db_owner;
                    case DCLRole.db_admin:
                        return Role.db_admin;
                    case DCLRole.db_user:
                        return Role.db_user;
                    case DCLRole.db_datawriter:
                        return Role.db_datawriter;
                    case DCLRole.db_datareader:
                        return Role.db_datareader;
                    default:
                        return null;
                }
            }
            catch (Exception exc)
            {
                throw new SecurityException(ErrorCodes.Security.INVALID_ROLE);
            }
        }

        public override int GetHashCode()
        {
            return RoleName.GetHashCode() ^ RoleType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Role role = obj as Role;
            if (role != null)
            {
                return this.RoleName.Equals(role.RoleName) && this.RoleType == role.RoleType;
            }
            return false;
        }

        #region Built-In Roles

        #region db_datareader
        public static readonly Role db_datareader = new Role()
        {
            RoleName = "db_datareader",
            RoleType = RoleType.BuiltIn,
            RoleLevel = Enums.RoleLevel.Database,
            Permissions = new List<Permission>() { 
                Permission.Read,
                Permission.Init
            }
        };
        #endregion

        #region db_datawriter
        public static readonly Role db_datawriter = new Role()
        {
            RoleName = "db_datawriter",
            RoleType = RoleType.BuiltIn,
            RoleLevel = Enums.RoleLevel.Database,
            Permissions = new List<Permission>() { 
                Permission.Write,
                Permission.Init
            }
        };
        #endregion

        #region db_user
        public static readonly Role db_user = new Role()
        {
            RoleName = "db_user",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { db_datareader, db_datawriter },
            RoleLevel = Enums.RoleLevel.Database,
            Permissions = new List<Permission>() { }
        };
        #endregion

        #region distributor
        public static readonly Role distributor = new Role()
        {
            RoleName = "distributor",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { },
            RoleLevel = Enums.RoleLevel.Server,
            Permissions = new List<Permission>() {
                Permission.Init,
                Permission.Distribute_Operation
            }
        };
        #endregion
        
        #region db_admin
        public static readonly Role db_admin = new Role()
        {
            RoleName = "db_admin",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { db_user },
            RoleLevel = Enums.RoleLevel.Database,
            Permissions = new List<Permission>() { 
                Permission.Create_Collection, 
                Permission.Delete_Collection, 
                Permission.Create_Index, 
                Permission.Delete_Index,
                Permission.Create_Store_Procedure,
                Permission.Delete_Store_Procedure,
                Permission.Create_User_Defined_Function,
                Permission.Delete_User_Defined_Function,
                Permission.Create_Trigger,
                Permission.Delete_Trigger,
                Permission.Modify_Trigger,
                 
                Permission.Grant_Role,
                Permission.Revoke_Role
            }
        };
        #endregion

        #region db_owner
        public static readonly Role db_owner = new Role()
        {
            RoleName = "db_owner",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { db_admin },
            RoleLevel = Enums.RoleLevel.Database,
            Permissions = new List<Permission>() { 
                Permission.Delete_Database
            }
        };
        #endregion

        #region dbcreator
        public static readonly Role dbcreator = new Role()
        {
            RoleName = "dbcreator",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { db_owner },
            RoleLevel = Enums.RoleLevel.Cluster,
            Permissions = new List<Permission>() { 
                Permission.Create_Database, 
                Permission.Delete_Database,
                Permission.Backup_Database
            }
        };
        #endregion

        #region Cluster_Manager
        public static readonly Role clustermanager = new Role()
        {
            RoleName = "clustermanager",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { dbcreator },
            RoleLevel = Enums.RoleLevel.Cluster,
            Permissions = new List<Permission>() { 
                Permission.Start_Cluster,
                Permission.Stop_Cluster
            }
        };
        #endregion
        
        #region clusteradmin
        public static readonly Role clusteradmin = new Role()
        {
            RoleName = "clusteradmin",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { clustermanager },
            RoleLevel = Enums.RoleLevel.Server,
            Permissions = new List<Permission>() { 
                Permission.Grant_Role,
                Permission.Revoke_Role,

                Permission.Modify_Cluster
            }
        };
        #endregion

        #region securityadmin
        public static readonly Role securityadmin = new Role()
        {
            RoleName = "securityadmin",
            RoleType = RoleType.BuiltIn,
            RoleLevel = Enums.RoleLevel.Server, 
            Permissions = new List<Permission>() { 
                Permission.Create_User,
                Permission.Delete_User
            }
        };
        #endregion

        #region sysadmin
        public static readonly Role sysadmin = new Role()
        {
            RoleName = "sysadmin",
            RoleType = RoleType.BuiltIn,
            ParentRole = new List<IRole>() { clusteradmin, securityadmin },
            RoleLevel = Enums.RoleLevel.Server,
            Permissions = new List<Permission>() { 
                Permission.Create_Cluster,
                Permission.Delete_Cluster,

                Permission.Create_Configuration_Cluster,
                Permission.Delete_Configuration_Cluster,
                Permission.Modify_Configuration_Cluster
            }
        };
        #endregion

        #region configreader
        public static readonly Role configreader = new Role()
        {
            RoleName = "configreader",
            RoleType = RoleType.BuiltIn,
            RoleLevel = Enums.RoleLevel.Server,
            ParentRole = new List<IRole>(),
            Permissions = new List<Permission>() { 
                Permission.Read_Configuration_Cluster_Configuration,
                Permission.Read_Database_Cluster_Configuration
            }
        };
        #endregion

        #endregion

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            this.RoleName = reader.ReadObject() as string;
            this.RoleType = (RoleType)reader.ReadObject();
            this.RoleLevel = (RoleLevel)reader.ReadObject();
            this.Permissions = reader.ReadObject() as List<Permission>;
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(this.RoleName);
            writer.WriteObject(this.RoleType);
            writer.WriteObject(this.RoleLevel);
            writer.WriteObject(this.Permissions);
        }


        public object Clone()
        {
            Role role = new Role();
            role.ParentRole = this.ParentRole != null ? this.ParentRole.Clone<IRole>() : null;
            role.Permissions = this.Permissions != null ? this.Permissions.Clone<Permission>() : null;
            role.RoleLevel = this.RoleLevel;
            role.RoleName = this.RoleName;
            role.RoleType = this.RoleType;
            return role;
        }
    }
}
