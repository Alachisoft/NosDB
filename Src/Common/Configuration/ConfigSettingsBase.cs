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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Configuration
{
    public abstract class ConfigSettingsBase
    {
        public abstract IPAddress IP { get; protected set; }
        public abstract int Port { get; protected set; }
        public abstract string LogConfiguration { get; protected set; }
        public abstract string BasePath { get; protected set; }
        public abstract string ConfigurationFile { get; protected set; }
        public abstract bool IsSecurityEnabled { get; protected set; }
        public abstract string SecurityConfigFile { get; protected set; }

        public abstract void Load();

    }
}
