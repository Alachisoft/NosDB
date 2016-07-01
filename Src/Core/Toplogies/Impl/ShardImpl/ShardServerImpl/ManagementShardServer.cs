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
using Alachisoft.NosDB.Common.Communication.Server;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Security.Interfaces;

namespace Alachisoft.NosDB.Core.Toplogies.Impl
{
    class ManagementShardServer:IShardServer,IServerEventListener
    {
        const int DATA_BUFFER_LENGTH = 4;
        IServer _server;
        IDictionary<SessionTypes, ISessionListener> _sessiionListioners = new Dictionary<SessionTypes, ISessionListener>();

        public ISecurityManager SecurityManager { set; get; }


        public ManagementShardServer()
        {
            _server = new TcpServer();
            _server.AddEventListener(this);
        }
       
        public int Port
        {
            get { return _server.Port; }
        }

        public System.Net.IPAddress BindingIp
        {
            get { return _server.BindedIp; }
        }

        public bool Initialize(System.Net.IPAddress ip, int port)
        {
            return _server.Initialize(ip, port);

        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void RegisterSessionListener(SessionTypes sessionType, ISessionListener listener)
        {
            if(sessionType.Equals(SessionTypes.Management))
            {
                lock(this)
                {
                    _sessiionListioners[sessionType] = listener;
                }
            }
        }

        public void UnregisterSessionListener(SessionTypes sessionType, ISessionListener listener)
        {
            lock(this)
            {
                if(_sessiionListioners[sessionType].Equals(listener))
                {
                    _sessiionListioners.Remove(sessionType);
                }
            }
        }

        public void Dispose()
        {
            _server.Stop();
        }

        #region IServerEventListener Methods
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

                    ISessionListener sessionListioner = null;

                    if (sessionType == SessionTypes.Management)
                    {
                        if (_sessiionListioners.ContainsKey(sessionType))
                            sessionListioner = _sessiionListioners[SessionTypes.Management];
                    }

                    if (sessionListioner != null)
                    {
                        IPEndPoint localEndPoint = (IPEndPoint)connectedSocket.LocalEndPoint;
                        IPEndPoint remoteEndPoint = (IPEndPoint)connectedSocket.RemoteEndPoint;

                        IConnection connection;
                        connection = new TcpConnection(connectedSocket, sessionType);

                        sessionListioner.OnSessionEstablished(new Session(sessionType, connection, localEndPoint.Port, remoteEndPoint.Port, remoteEndPoint.Address));
                    }

                }
                else
                {
                    connectedSocket.Close();
                }
            }
            catch(Exception ex)
            {
                if (connectedSocket != null)
                    connectedSocket.Close();
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ManagementShardServer.OnConnectionEstablished()", ex);

            }
            
        }

        public void OnServerStopped(bool gracefull)
        {
            //throw new NotImplementedException();
            _sessiionListioners.Clear();
            _server.Stop();
        }
        #endregion
    }
}
