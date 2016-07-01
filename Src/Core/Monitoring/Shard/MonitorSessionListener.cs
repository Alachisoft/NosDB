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
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.DBEngine;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Alachisoft.NosDB.Core.Monitoring
{
    internal class MonitorSessionListener : ISessionListener
    {
        IDictionary<MonitorServer, IDualChannel> _sessionChannnelMap = new Dictionary<MonitorServer, IDualChannel>();
        IChannelFormatter _channelFormatter = new MonitorChannelFormatter();
        private NodeContext _nodeContext;
        private ClientSessionManager _clientSessionManager;

        public NodeContext NodeContext { get { return _nodeContext; } set { _nodeContext = value; } }

        public MonitorSessionListener(ClientSessionManager clientSession, NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
            _clientSessionManager = clientSession;
        }

        public bool OnSessionEstablished(Session session)
        {
            if (session == null || session.SessionType != SessionTypes.Monitoring || session.Connection == null)
                return false;
            IDualChannel _channel;
            IPAddress _localAddress = DnsCache.Resolve(Environment.MachineName);
            _channel = new DualChannel(session.Connection, session.IP.ToString(), session.RemotePort, _localAddress.ToString(), session.SessionType, null, _channelFormatter);

            switch (session.SessionType)
            {
                case SessionTypes.Monitoring:
                {
                    MonitorServer monitorServerSession = new MonitorServer(_clientSessionManager, _nodeContext, new UserCredentials());
                    _channel.RegisterRequestHandler(monitorServerSession);
                    monitorServerSession.Channel = _channel;
                    try
                    {
                        if (_channel.Connect(true))
                        {
                            _sessionChannnelMap.Add(monitorServerSession, _channel);
                        }
                    }
                    catch (ChannelException ex)
                    {
                        //RTD: Replace shardLogger with the respective module logger name
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("Error: MonitorSessionListener.OnSessionEstd()", ex.ToString());
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
