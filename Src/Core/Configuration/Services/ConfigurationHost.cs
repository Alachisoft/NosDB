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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Communication.Server;
using System.Configuration;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.RPCFramework.DotNetRPC;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Common.Util;
using System.Net;
using Alachisoft.NosDB.Serialization;
using Alachisoft.NosDB.Common.Security;
using System.IO;
using System.Threading;
using Alachisoft.NosDB.Common.Configuration;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ConfigurationHost : IServerEventListener
    {

        private ConfigurationServer _configurationServer = null;
        private SessionManager _sessionManager = null;
        private IServer _server;
        IDictionary<SessionTypes, ISessionListener> _listeners = new Dictionary<SessionTypes, ISessionListener>();
        const int DATA_BUFFER_LENGTH = 4; //(in bytes)

        public ConfigurationServer ConfigServer
        {
            get
            {
                return _configurationServer;
            }
            set
            {
                _configurationServer = value;
            }
        }

        public ConfigurationHost()
        {
            _sessionManager = new SessionManager();
            ManagementHost.RegisterCompactTypes();
            _configurationServer = new ConfigurationServer();
            Initialize();
            _sessionManager.ConfigServer = _configurationServer;
        }


        public void Initialize()
        {
            _server = new TcpServer();
            _server.Initialize(ConfigurationSettings<CSHostSettings>.Current.IP, ConfigurationSettings<CSHostSettings>.Current.Port);
            _server.AddEventListener(this);
        }

        public void Start()
        {
            // + security: SPN registration
            if (ConfigurationSettings<CSHostSettings>.Current.IsSecurityEnabled)
            {
                SSPIUtility.RegisterSpn(true);
                if (!SSPIUtility.IsSPNRegistered)
                {
                    AppUtil.LogEvent("Configuration Service: SPN is not registered. Only local connections will be served.", EventLogEntryType.Information);
                }
            }
            // - security
            //System.IO.StreamWriter writer = new StreamWriter(@"D:\CSConfigurationLog.txt");
            //try
            //{
            //    writer.AutoFlush = true;
            //    writer.WriteLine("Service begin");
            _configurationServer.Start();
            _server.Start();
            //    }
            //    catch(Exception ex)
            //    {
            //        writer.WriteLine("Exception ::" + ex.ToString());
            //        AppUtil.LogEvent("Configuration Host Start:::"+ex.ToString(), EventLogEntryType.Information);
            //    }
            //}
        }


        public void Stop()
        {
            _server.Stop();
            _configurationServer.Stop();
            // + security: SPN registration
            if (ConfigurationSettings<CSHostSettings>.Current.IsSecurityEnabled)
            {
                SSPIUtility.RegisterSpn(false);
                if (SSPIUtility.IsSPNRegistered)
                {
                    AppUtil.LogEvent("Configuration Service: SPN is not unregistered.", EventLogEntryType.Information);
                }
            }
            // - security
        }

        #region IServerEventListener Member

        public void OnConnectionEstablished(System.Net.Sockets.Socket connectedSocket)
        {
            try
            {
                if (connectedSocket != null && connectedSocket.Connected)
                {
                    byte[] dataBuffer = new byte[DATA_BUFFER_LENGTH];
                    NetworkUtil.ReadFromTcpSocket(connectedSocket, dataBuffer);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(dataBuffer);

                    SessionTypes sessionType = (SessionTypes)BitConverter.ToInt32(dataBuffer, 0);

                    ISessionListener sessionListener = null;
                    switch (sessionType)
                    {
                        case SessionTypes.Management:
                            if (_sessionManager != null)
                                sessionListener = _sessionManager;
                            break;

                        case SessionTypes.Shard:
                            if (_sessionManager != null)
                                sessionListener = _sessionManager;
                            break;

                        case SessionTypes.Client:
                            if (_sessionManager != null)
                                sessionListener = _sessionManager;
                            break;
                        case SessionTypes.Monitoring:
                            if (_sessionManager != null)
                                sessionListener = _sessionManager;
                            break;

                    }
                    if (sessionListener != null)
                    {
                        IPEndPoint localEndPoint = (IPEndPoint)connectedSocket.LocalEndPoint;
                        IPEndPoint remoteEndPoint = (IPEndPoint)connectedSocket.RemoteEndPoint;

                        IConnection connection;
                      
                            connection = new TcpConnection(connectedSocket, sessionType);

                        sessionListener.OnSessionEstablished(new Session(sessionType, connection, localEndPoint.Port, remoteEndPoint.Port, remoteEndPoint.Address));
                    }
                    else //As no session listener found
                        connectedSocket.Close();
                }
            }
            catch (Exception e)
            {
                if (connectedSocket != null)
                    connectedSocket.Close();
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ConfigurationHost.OnConnectionEstablished()", e);

            }
        }

        public void OnServerStopped(bool gracefull)
        {
            _server.Stop();
            _configurationServer.Stop();

        }

        #endregion
    }
}
