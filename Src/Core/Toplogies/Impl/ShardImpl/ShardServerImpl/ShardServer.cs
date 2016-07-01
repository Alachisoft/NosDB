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
using Alachisoft.NosDB.Common.Communication.Server;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Alachisoft.NosDB.Serialization.Formatters;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Core.Toplogies.Impl
{
    public class ShardServer:IShardServer,IServerEventListener
    {
        const int DATA_BUFFER_LENGTH = 4; //(in bytes)
        IServer _server;
        IDictionary<SessionTypes, ISessionListener> _listeners = new Dictionary<SessionTypes, ISessionListener>();

        public ShardServer()
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
            if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.ServerLogger.Info("ShardServer.start()", "Shard server started on :" + BindingIp + ":" + Port + ".");
            }
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void RegisterSessionListener(SessionTypes sessionType, ISessionListener listener)
        {
            lock(this)
            {
                _listeners[sessionType] = listener;
            }
        }

        public void UnregisterSessionListener(SessionTypes sessionType, ISessionListener listener)
        {
            lock (this)
            {
                if (_listeners.ContainsKey(sessionType))
                    _listeners.Remove(sessionType);
            }
        }

        public void Dispose()
        {
            _server.Stop();
        }

        public void OnConnectionEstablished(System.Net.Sockets.Socket connectedSocket)
        {
            try
            {
                if (connectedSocket != null && connectedSocket.Connected)
                {
                    //Read session type; A session type is a number encoded as UTF8 String of 10 bytes
                    byte[] dataBuffer = new byte[DATA_BUFFER_LENGTH];

                    NetworkUtil.ReadFromTcpSocket(connectedSocket, dataBuffer);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(dataBuffer);

                    SessionTypes sessionType = (SessionTypes)BitConverter.ToInt32(dataBuffer, 0);
                   
                    ISessionListener sessionListener = null;
                    switch (sessionType)
                    {
                        case SessionTypes.Shard:
                            if(_listeners.ContainsKey(SessionTypes.Shard))
                                sessionListener = _listeners[SessionTypes.Shard];
                            break;

                        case SessionTypes.Client:
                            if (_listeners.ContainsKey(SessionTypes.Client))
                                sessionListener = _listeners[SessionTypes.Client];
                            break;

                        case SessionTypes.Management:
                            if (_listeners.ContainsKey(SessionTypes.Management))
                                sessionListener = _listeners[SessionTypes.Management];
                            break;
                        case SessionTypes.Monitoring:
                            if (_listeners.ContainsKey(SessionTypes.Monitoring))
                                sessionListener = _listeners[SessionTypes.Monitoring];
                            break;

                    }

                    if (sessionListener != null)
                    {
                        IPEndPoint localEndPoint = (IPEndPoint)connectedSocket.LocalEndPoint;
                        IPEndPoint remoteEndPoint = (IPEndPoint)connectedSocket.RemoteEndPoint;

                        IConnection connection = new TcpConnection(connectedSocket, sessionType);

                        sessionListener.OnSessionEstablished(new Session(sessionType, connection, localEndPoint.Port, remoteEndPoint.Port, remoteEndPoint.Address));
                    }
                    else //As no session listener found
                        connectedSocket.Close();
                }
            }
            catch(Exception e)
            {
                if (connectedSocket != null)
                    connectedSocket.Close();
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("ShardServer.OnConnectionEstablished()", e);

            }
              
        }

        public void OnServerStopped(bool gracefull)
        {
            
            //throw new NotImplementedException();
        }
    }
}
