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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;

namespace Alachisoft.NosDB.Common.Configuration.RPC
{
    public class RPCService:DBService
    {
        int retries = 3;
        string _addressURI;
        int _port;
      
        static RPCService()
        {
            //registerCompactType
        }

        public RPCService(string serviceName, string addressURI, int port): base(serviceName, addressURI, port)
        {
            _addressURI = addressURI;
            _port = port;
        }

        static bool _enableTracing;
        public static bool EnableTracing
        {
            get { return _enableTracing; }
            set { _enableTracing = value; }
        }

        private void TryIntializeService(OutProcConfigurationClient client, TimeSpan timeout, SessionTypes sessionType, IChannelFormatter channelFormatter)
        {
            try
            {
                TraceProvider traceProvider = EnableTracing ? new TraceProvider() : null;
                if (_port > 0 && _port < 65535)
                    client.Connect(_addressURI, _port, sessionType, channelFormatter);
                else
                    client.Connect(_addressURI, sessionType);

            }
            catch (Exception e)
            {
                if (retries-- > 0)
                {
                    Start(timeout);
                    System.Threading.Thread.Sleep(3000);
                    TryIntializeService(client,timeout, sessionType,channelFormatter);
                }
                else
                    throw e;
            }
            finally
            {
                retries = 3;
            }
        }

        public override IConfigurationServer GetConfigurationServer(TimeSpan timeout, SessionTypes sessionType, IChannelFormatter channelFormatter)
        {
            OutProcConfigurationClient  client= new OutProcConfigurationClient();
            TryIntializeService(client, timeout, sessionType, channelFormatter);
            return client;
        }


    }
}
