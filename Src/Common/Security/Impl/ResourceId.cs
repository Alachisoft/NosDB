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
    public class ResourceId : IEquatable<ResourceId>, ICompactSerializable, ICloneable
    {
        public ResourceId()
        {
        }
        public SecurityInformationTypes SecurityInformationType
        {
            get
            { return SecurityInformationTypes.ResourceId; }
        }

        public string Name { get; set; }
        public ResourceType ResourceType { get; set; }

        public override bool Equals(object obj)
        {
            ResourceId resourceId = obj as ResourceId;
            if (resourceId != null)
            {
                return this.Name.Equals(resourceId.Name) && (int)this.ResourceType == (int)resourceId.ResourceType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashcode = Name.ToLower().GetHashCode() ^ ResourceType.GetHashCode();
            return hashcode;
        }

        public bool Equals(ResourceId other)
        {
            return this.Name.Equals(other.Name, StringComparison.CurrentCultureIgnoreCase) && (int)this.ResourceType == (int)other.ResourceType;
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            this.Name = reader.ReadObject() as string;
            this.ResourceType = (ResourceType)reader.ReadObject();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(this.Name);
            writer.WriteObject(this.ResourceType);
        }

        public object Clone()
        {
            ResourceId resourceId = new ResourceId();
            resourceId.Name = this.Name;
            resourceId.ResourceType = this.ResourceType;
            return resourceId;
        }
    }
}
