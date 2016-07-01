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
using Alachisoft.NosDB.Common.JSON.CustomConverter;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    [JsonConverter(typeof(SecurityInformationConverter))]
    public class Permission : ICompactSerializable, ICloneable
    {
        public Permission()
        {
        }
        public SecurityInformationTypes SecurityInformationType
        {
            get
            { return SecurityInformationTypes.Permission; }
        }
        public OperationType OperationType { set; get; }
        public ResourceType ResourceType { set; get; }

        public override int GetHashCode()
        {
            return OperationType.GetHashCode() ^ ResourceType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Permission permission = obj as Permission;
            if (permission != null)
            {
                return permission.OperationType == this.OperationType && permission.ResourceType == this.ResourceType;
            }
            return false;
        }

        #region Permissions
        public static readonly Permission Create_Configuration_Cluster = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.ConfigurationCluster };
        public static readonly Permission Delete_Configuration_Cluster = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.ConfigurationCluster };
        public static readonly Permission Modify_Configuration_Cluster = new Permission() { OperationType = OperationType.Modify, ResourceType = ResourceType.ConfigurationCluster };
        public static readonly Permission Start_Configuration_Cluster = new Permission() { OperationType = OperationType.Start, ResourceType = ResourceType.ConfigurationCluster }; 
        public static readonly Permission Stop_Configuration_Cluster = new Permission() { OperationType = OperationType.Stop, ResourceType = ResourceType.ConfigurationCluster };
        public static readonly Permission Read_Configuration_Cluster_Configuration = new Permission() { OperationType = OperationType.Read, ResourceType = ResourceType.ConfigurationCluster };

        public static readonly Permission Create_Cluster = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.Cluster };
        public static readonly Permission Delete_Cluster = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.Cluster };
        public static readonly Permission Modify_Cluster = new Permission() { OperationType = OperationType.Modify, ResourceType = ResourceType.Cluster }; //this permission includes add/remove shard in cluster, add/remove node in shard
        public static readonly Permission Start_Cluster = new Permission() { OperationType = OperationType.Start, ResourceType = ResourceType.Cluster }; //this permission includes start shard/node
        public static readonly Permission Stop_Cluster = new Permission() { OperationType = OperationType.Stop, ResourceType = ResourceType.Cluster }; //this permission includes stop shard/node
        public static readonly Permission Read_Database_Cluster_Configuration = new Permission() { OperationType = OperationType.Read, ResourceType = ResourceType.Cluster };
        
        public static readonly Permission Create_Database = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.Database };
        public static readonly Permission Delete_Database = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.Database };
        public static readonly Permission Read = new Permission() { OperationType = OperationType.Read, ResourceType = ResourceType.Database };
        public static readonly Permission Write = new Permission() { OperationType = OperationType.Write, ResourceType = ResourceType.Database };
        public static readonly Permission Init = new Permission() { OperationType = OperationType.Init, ResourceType = ResourceType.Database };
        public static readonly Permission Create_Store_Procedure = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.StoreProcedure };
        public static readonly Permission Delete_Store_Procedure = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.StoreProcedure };
        public static readonly Permission Create_User_Defined_Function = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.UserDefinedFunction };
        public static readonly Permission Delete_User_Defined_Function = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.UserDefinedFunction };
        public static readonly Permission Create_Collection = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.Collection };
        public static readonly Permission Delete_Collection = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.Collection };
        public static readonly Permission Create_Trigger = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.Trigger };
        public static readonly Permission Modify_Trigger = new Permission() { OperationType = OperationType.Modify, ResourceType = ResourceType.Trigger };
        public static readonly Permission Delete_Trigger = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.Trigger };
        public static readonly Permission Create_Index = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.Index };
        public static readonly Permission Delete_Index = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.Index };

        public static readonly Permission Create_User = new Permission() { OperationType = OperationType.Create, ResourceType = ResourceType.User };
        public static readonly Permission Delete_User = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.User };
        public static readonly Permission Grant_Role = new Permission() { OperationType = OperationType.Grant, ResourceType = ResourceType.Role };
        public static readonly Permission Revoke_Role = new Permission() { OperationType = OperationType.Delete, ResourceType = ResourceType.Role };

        public static readonly Permission Backup_Database = new Permission { OperationType = OperationType.Backup, ResourceType = ResourceType.Database };

        public static readonly Permission Distribute_Operation = new Permission { OperationType = OperationType.Distribute, ResourceType = ResourceType.Operation };
        #endregion

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            this.OperationType = (OperationType)reader.ReadObject();
            this.ResourceType = (ResourceType)reader.ReadObject();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(this.OperationType);
            writer.WriteObject(this.ResourceType);
        }

        public object Clone()
        {
            Permission permission = new Permission();
            permission.OperationType = this.OperationType;
            permission.ResourceType = this.ResourceType;
            return permission;
        }
    }
}
