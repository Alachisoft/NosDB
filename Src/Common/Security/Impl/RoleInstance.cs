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
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    public class RoleInstance : IRoleInstance, ICompactSerializable
    {
        public RoleInstance()
        {
            AuthorizedUsers = new List<string>();
        }
        public SecurityInformationTypes SecurityInformationType
        {
            get
            { return SecurityInformationTypes.RoleInstance; }
        }

        public List<string> AuthorizedUsers { set; get; }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            AuthorizedUsers = reader.ReadObject() as List<string>;
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(AuthorizedUsers);
        }

        public object Clone()
        {
            RoleInstance roleInstance = new RoleInstance();
            roleInstance.AuthorizedUsers = this.AuthorizedUsers != null ? this.AuthorizedUsers.Clone<string>() : null;
            return roleInstance;
        }
    }
}
