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
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Security.Interfaces
{
    [JsonConverter(typeof(SecurityInformationConverter))]
    public interface IResourceItem : ICloneable
    {
        ResourceId ResourceId { set; get; }

        string ClusterName { set; get; }

        Dictionary<IRole, IRoleInstance> Roles { set; get; }
        List<ResourceId> SubResources { set; get; }

        SecurityInformationTypes SecurityInformationType { get; }

        bool GrantRole(IRole role, string username);
        bool RevokeRole(IRole role, string username);
        void AddSubResource(ResourceId subResourceId);
        void RemoveSubResource(ResourceId subResourceId);

        void Merge(IResourceItem resourceItem);
    }
}
