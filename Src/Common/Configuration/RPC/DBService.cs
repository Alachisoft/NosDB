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
using System.Net.Sockets;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration.Services;

namespace Alachisoft.NosDB.Common.Configuration.RPC
{
    public class DBService : ServiceBase
    {
        private string _serviceName;
        protected IConfigurationServer _service;

        public DBService(string serviceName, string serviceURI, long port)
        {
            this._serviceName = serviceName;
        }

        public IConfigurationServer ConfigurationService
        {
            get
            {
                return this._service;
            }
        }


        public virtual IConfigurationServer GetConfigurationServer(TimeSpan timeout, SessionTypes sessionType, IChannelFormatter channelFormatter)
        {
            IConfigurationServer cs = null;

            try
            {
                cs = ConnectConfigurationService();
            }
            catch(SocketException exception)
            {
                if(exception.SocketErrorCode == SocketError.TimedOut)
                {
                    throw exception;
                }
                try
                {
                    Start(timeout);
                    cs = ConnectConfigurationService();
                }
                catch(Exception ex)
                {
                    throw  exception;
                }
            }

            return cs;

        }

        public virtual IConfigurationServer ConnectConfigurationService()
        {
            return null;
        }

        public void Start(TimeSpan timeout)
        {
            //call service start method
        }

        public void Stop(TimeSpan timeout)
        {
            //call service stop method
        }

        public void Restart(TimeSpan timeout)
        {
            //call service restart method
        }

        public bool isRunning(TimeSpan timeout)
        {
            //call service isRunning method
            return false;
        }


    }
}
