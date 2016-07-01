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
using System.Configuration;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Core.Monitoring;

namespace Alachisoft.NosDB.Core.DBEngine
{
    public class ClientSessionManager : ISessionListener, IClientDisconnection,IDisposable
    {
        private IList<ISessionId> loggedInSessionsOnCS = new List<ISessionId>();
        private IDictionary<string, ClientSession> _clientSessions;
        private System.Net.IPAddress _localAddress;
        private ITraceProvider _traceProvider;
        private IChannelFormatter _channelFormatter;
        private IDatabaseEngineFactory _databaseEngineFactory;
        private IThreadPool _threadPool;
		 private IClientDisconnection _clientDisconnection;
        
        public ClientSessionManager(IDatabaseEngineFactory databaseEngineFactory)
        {
            _databaseEngineFactory = databaseEngineFactory;
            _clientSessions = new HashVector<string, ClientSession>();
            SetLocalAddress();
            _traceProvider = new TraceProvider();
            _channelFormatter = new DbEngineFormatter();
			 _threadPool= new ClrThreadPool();
            _threadPool.Initialize();
        }

        public IDatabaseEngineFactory DatabaseEngineFactory
        {
            set { _databaseEngineFactory = value; }
            get { return _databaseEngineFactory; }
        }

        public string ShardName { set; get; }
                     
        public bool OnSessionEstablished(Session session)
        {
            if (session == null || session.SessionType != SessionTypes.Client || session.Connection == null) return false;

            var key = MiscUtil.GetAddressInfo(session.IP, session.RemotePort);
            if (_clientSessions.ContainsKey(key))
            {
                //session.Socket.Disconnect(false);
                _clientSessions[key].ServerChannel.Disconnect();

                lock (_clientSessions)
                    _clientSessions[key].ServerChannel = new ServerChannel(session.Connection, session.IP.ToString(), session.RemotePort, this._localAddress.ToString(), session.SessionType, _traceProvider, _channelFormatter);

                try
                {
                    return _clientSessions[key].ServerChannel.Connect(true);
                }
                catch (ChannelException ex)
                {
                    //RTD: Replace shardLogger with the respective module logger name
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("Error: ClientSessionMgr.OnSessionEstd()", ex.ToString());
                    return false;
                }
            }
            var clientSession = new ClientSession(_databaseEngineFactory, this, ShardName)
            {
                ServerChannel =
                    new ServerChannel(session.Connection, session.IP.ToString(), session.RemotePort,
                        _localAddress.ToString(), session.SessionType, _traceProvider, _channelFormatter)
            };
            if (!clientSession.ServerChannel.Connect(true)) return false;
            lock (_clientSessions)
            {

                _clientSessions[key] = clientSession;
            }
            return true;
        }

        public void DisconnectClient(string id)
        {
            if (id == null) return;
            lock (_clientSessions)
            {
                if (_clientSessions.ContainsKey(id))
                {
                    _clientSessions.Remove(id);
                }
                if (_clientDisconnection != null)
                {
                    _clientDisconnection.DisconnectClient(id);
                }
            }
        }

        public void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId)
        {
            if (!loggedInSessionsOnCS.Contains(sessionId))
            {
                loggedInSessionsOnCS.Add(sessionId);
            }
        }

        #region Getting Local Address Logic; might be replace with getting address from service config
        private void SetLocalAddress()
        {
            string localIp = NetworkUtil.GetLocalIPAddress().ToString();
            try
            {
                _localAddress = System.Net.IPAddress.Parse(localIp);
            }
            catch
            {
                _localAddress = System.Net.IPAddress.Parse("127.0.0.1");
            }
        }

        #endregion

        internal void RegisterClientDisconnectListerner(IClientDisconnection listerver)
        {
            _clientDisconnection = listerver;
        }

        public void OnRaisedNotification(object message, string serverInfo)
        {
            try
            {
                lock (_clientSessions)
                {
                    if (_clientSessions.ContainsKey(serverInfo))
                    {
                        var clientmanager = _clientSessions[serverInfo];
                        clientmanager.SendNotification(message);
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (LoggerManager.Instance.QueryLogger != null && LoggerManager.Instance.QueryLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.QueryLogger.Error("OnRaisedNotification.ClientSeesionManager", ex);
                }
            }
        }

        public void Dispose()
        {
            lock (_clientSessions)
            {
                foreach (KeyValuePair<string, ClientSession> clientSession in _clientSessions)
                {
                    DisconnectClient(clientSession.Key);
                }
                _clientSessions.Clear();
            }
        }

        public List<ClientProcessStats> GetClientProcessStats(string database)
        {
            List<ClientProcessStats> clientProcessStats = new List<ClientProcessStats>();
            lock (_clientSessions)
            {
                foreach (var pair in _clientSessions)
                {
                    if(database.Equals(pair.Value.Database, StringComparison.OrdinalIgnoreCase))
                    {
                        if (pair.Value == null)
                            continue;

                        ClientProcessStats clientProcStat = new ClientProcessStats();
                        clientProcStat.Client = pair.Value.ServerChannel.PeerAddress;
                        clientProcStat.ProcessID = pair.Value.ClientProcessID;
                        clientProcStat.BytesSent = pair.Value.ClientsBytesSent;
                        clientProcStat.BytesReceived = pair.Value.ClientBytesReceived;
                        clientProcessStats.Add(clientProcStat);
                    }
                }
            }
            return clientProcessStats;
        }
    }

}
