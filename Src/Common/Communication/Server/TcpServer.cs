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
using System.Net;
using System.Net.Sockets;
using System.Text;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Common.Communication.Server
{
    public class TcpServer:IServer
    {
        Socket _listeningSocket;
        int _port;
        IPAddress _bindingIp;
        bool _initialized;
        IServerEventListener _listener;
        bool _started;

        public bool Initialize(System.Net.IPAddress bindingIp, int port)
        {
            _bindingIp = bindingIp;
            _port = port;

            return _initialized= true;
        }

        public IPAddress BindedIp
        {
            get { return _bindingIp; }
        }

        public int Port
        {
            get { return _port; }
        }

        public void Start()
        {
            if(!_initialized)
            {
                throw new Exception("Server not initialized");
            }

            
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeningSocket.Bind(new IPEndPoint(_bindingIp, _port));
            _listeningSocket.Listen(2000);
            _started = true;
            _listeningSocket.BeginAccept(new AsyncCallback(OnConnectionAccepted),null);
           
        }

        private void OnConnectionAccepted(IAsyncResult ar)
        {
            try
            {
                Socket acceptedSocket = _listeningSocket.EndAccept(ar);
               
                _listeningSocket.BeginAccept(new AsyncCallback(OnConnectionAccepted), null);

                if (_listener != null)
                {
                    _listener.OnConnectionEstablished(acceptedSocket);
                }
                else //as there is no listener 
                    acceptedSocket.Close();
            }
            catch(SocketException e)
            {
                if (_listener != null)
                    _listener.OnServerStopped(!_started);
            }
            catch(Exception e)
            {
                if (_listener != null)
                    _listener.OnServerStopped(false);
            }
        }

        public void Stop()
        {
            _listeningSocket.Close();
            _started = false;
            
        }

        public void AddEventListener(IServerEventListener listener)
        {
            _listener = listener;
        }

       
    }
}
