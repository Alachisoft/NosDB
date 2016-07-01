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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Security.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Configuration.Services.Client
{
    public class InProcConfigurationClient : IConfigurationClient
    {
        static InProcConfigurationClient s_instance = new InProcConfigurationClient();
        public static IConfigurationClient Instance { get { return s_instance; } }

        ConfigurationServer _server;
        public void Connect(string serviceURI, SessionTypes sessionType)
        {
            _server = new ConfigurationServer();
            _server.Start();
        }     

        public void Disconnect()
        {
            _server.Stop();
        }

        public IShardConfigurationSession OpenShardConfigurationSession(string cluster, string shard, ServerNode node, UserCredentials credentials, IChannelFormatter channelFormatter)
        {
            return  new InProcShardConfigurationSession(_server.OpenShardConfigurationSession(cluster, shard, node, credentials,channelFormatter));
        }

        public IConfigurationSession OpenConfigurationSession(IClientAuthenticationCredential credentials)
        {
            return new InProcConfigurationSession(_server.OpenConfigurationSession(credentials));
        }






        public Common.Security.Server.IServerAuthenticationCredential AuthenticateClient(IClientAuthenticationCredential credentials)
        {
            throw new NotImplementedException();
        }

        public bool AutoReconnect
        {
            get
            {
                return false;
            }
            set
            {
                
            }
        }

        public void SetChannelDisconnectedListener(IRequestListener listener)
        {
            
        }


        public void MarkDatabaseSesion()
        {
        }


        public void MarkDistributorSession()
        {
        }


        public void MarkConfiguraitonSession()
        {
        }
    }
}
