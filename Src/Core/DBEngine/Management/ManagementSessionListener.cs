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
using System.Text;
using System.Net;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Core.DBEngine.Management
{
    public class ManagementSessionListener : ISessionListener
    {
        private ITraceProvider _traceProvider = null;
        private IChannelFormatter _channelFormatter = new ConfigurationChannelFormatter();
        private IDictionary<ManagementSession, IDualChannel> _sessionChannnelMap = new Dictionary<ManagementSession, IDualChannel>();
        private ManagementServer _managementServer;

        public ManagementSessionListener()
        {
            _traceProvider = new TraceProvider();
        }

        public ManagementServer ManagementServer
        {
            get
            {
                return _managementServer;
            }
            set
            {
                _managementServer = value;
            }
        }

        #region ISessionListener Methods

        public bool OnSessionEstablished(Session session)
        {
            if (session == null || session.SessionType != SessionTypes.Management || session.Connection == null || _managementServer == null)
                return false;
            try
            {
                IDualChannel channel;
                IPAddress localAddress = DnsCache.Resolve(Environment.MachineName);
                channel = new DualChannel(session.Connection, session.IP.ToString(), session.RemotePort, localAddress.ToString(), session.SessionType, _traceProvider, _channelFormatter);

                switch (session.SessionType)
                {
                    case SessionTypes.Management:
                        ManagementSession managementSession = (ManagementSession)_managementServer.OpenManagementSession(new UserCredentials());
                        channel.RegisterRequestHandler(managementSession);
                        managementSession.Channel = channel;
                        if (channel.Connect(true))
                        {
                            _sessionChannnelMap.Add(managementSession, channel);
                        }
                        return true;
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Error("MgtSessionListener.OnSessionEstablished", "Error", ex);
                }
            }
            return false;
        }

        #endregion
    }
}
