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
using Alachisoft.NosDB.Common.Configuration.RPC;
using Alachisoft.NosDB.Common.Configuration.Services;

namespace Alachisoft.NosDB.Core.Configuration
{
    internal class ConfigSessionParams
    {
        internal IConfigurationServer Remote { get; set; }
        internal IConfigurationSession Session { get; set; }
        internal string ConfigServerIp { get; set; }
        internal int Port { get; set; }
        internal DatabaseRPCService Rpc { get; set; }
        public void Dispose()
        {
            if (Session != null)
            {
                Session.Close();

            }
            if (Rpc != null)
                Rpc.Dispose();

        }

    }
}