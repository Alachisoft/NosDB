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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.SecurityConfig
{
    [ConfigurationRoot("configuration")]
    public class SecurityConfiguration : ICloneable, ICompactSerializable
    {
        string defaultWindowsUser = "";
        [ConfigurationAttribute("defaultWindowsUser")]
        public string DefaultWindowsUser { get { return defaultWindowsUser.ToLower(); } set { defaultWindowsUser = value; } }
        
        public object Clone()
        {
            SecurityConfiguration securityConfig = new SecurityConfiguration();
            securityConfig.DefaultWindowsUser = this.DefaultWindowsUser;
            return securityConfig;
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            DefaultWindowsUser = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(DefaultWindowsUser);
        }
    }
}
