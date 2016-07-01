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

using System.Collections.Concurrent;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Core.Monitoring.Configuration;
using System;
using System.Collections.Generic;
using System.Net;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class SessionManager : ISessionListener, ISessionDisconnectListener
    {
        
        ITraceProvider _traceProvider = null;
        IChannelFormatter _channelFormatter = new ConfigurationChannelFormatter();
        readonly IDictionary<ConfigurationSession, IDualChannel> _sessionChannnelMap = new ConcurrentDictionary<ConfigurationSession, IDualChannel>();

        object _mutex = new object();

        public SessionManager()
        {
            _traceProvider = new TraceProvider();
        }

        public ConfigurationServer ConfigServer
        {
            get;
            set;
        }
        
        #region ISessionListener Members
        public bool OnSessionEstablished(Session session)
        {
            if (session == null || session.Connection == null) return false;
            IDualChannel _channel;

            IPAddress _localAddress = DnsCache.Resolve(Environment.MachineName);
            _channel = new DualChannel(session.Connection, session.IP.ToString(), session.RemotePort, _localAddress.ToString(), session.SessionType, _traceProvider, _channelFormatter);

            try
            {
                switch (session.SessionType)
                {
                    case SessionTypes.Management:
                    case SessionTypes.Client:
                        {
                            ConfigurationSession cfgsession = (ConfigurationSession)this.ConfigServer.OpenConfigurationSession(new SSPIClientAuthenticationCredential());//new ConfigurationSession(this.ConfigServer, new UserCredentials());
                            _channel.RegisterRequestHandler(cfgsession);
                            if (session.SessionType == SessionTypes.Client)
                                cfgsession.Channel = _channel;
                            else
                                cfgsession.NodeChannel = _channel;

                            cfgsession.Parent = this;
                            if (_channel.Connect(true))
                            {
                                _sessionChannnelMap.Add(cfgsession, _channel);
                            }
                            return true;
                            
                        }
                    case SessionTypes.Shard:
                        {
                            

                            return false;
                        }
                    case SessionTypes.Monitoring:
                    {

                        ConfigrationMonitorSession cfgMonitorsession = new ConfigrationMonitorSession(
                            this.ConfigServer, new UserCredentials());
                        _channel.RegisterRequestHandler(cfgMonitorsession);
                        cfgMonitorsession.Channel = _channel;

                        if (_channel.Connect(true))
                        {

                        }
                        return true;

                    }
                }
            }
            catch (ChannelException ex)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("Error: SessionMgr.OnSessionEstd()", ex.ToString());
            }
            return false;
        } 
        #endregion

        #region ISessionDisconnectListener
        public void OnSessionDisconnected(Common.Configuration.Services.IConfigurationSession session)
        {
            var configurationSession = session as ConfigurationSession;
            if (configurationSession != null)
            {
                if (_sessionChannnelMap != null)
                {
                    if (_sessionChannnelMap.ContainsKey(configurationSession))
                    {
                        _sessionChannnelMap.Remove(configurationSession);
                    }
                }
            }
        }
        #endregion
    }
}
