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

namespace Alachisoft.NosDB.Common.Configuration.RPC
{
    public abstract class ServiceBase : IDisposable
    {
        protected string _serverName = Environment.MachineName;
        protected bool _useTcp = true;
        protected long _port;

        public ServiceBase() { }

        public ServiceBase(string serverURI, long port, bool useTcp)
        {
            _serverName = serverURI;
            _port = port;
            _useTcp = useTcp;
        }

        public string ServiceURI
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        public bool UseTcp
        {
            get { return _useTcp; }
            set { _useTcp = value; }
        }

        public long Port
        {
            get { return _port; }
            set { _port = value; }
        }

        protected virtual void Start(TimeSpan timeout, string service)
        {
            try
            {
                //check service status & start service.
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        protected virtual void Stop(TimeSpan timeout, string service)
        {
            try 
            { 
                // check service status & stop service.
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        protected virtual void Restart(TimeSpan timeout, string service)
        {
            try
            {
                // check service status & restart service.
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        protected virtual bool isServiceRunning(TimeSpan timeout, string service)
        {
            bool isRunning = false;
            
            try
            {
                // get service status & return that.
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return isRunning;
        }

        #region IDisposable Members
        public void Dispose()
        {
           
        } 
        #endregion
    }
}
