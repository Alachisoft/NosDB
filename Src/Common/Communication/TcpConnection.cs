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
//using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Util;
using System.Net.NetworkInformation;
using Alachisoft.NosDB.Common.Threading;
using System.Threading;

namespace Alachisoft.NosDB.Common.Communication
{
    public class TcpConnection :IConnection
    {

        public enum  Status
        {
            Connected = 1,
            Disconnected = 2,
            Reconnecting = 4
        }


        private Socket _socket;
        private bool _connected;
        private object _sync_lock = new object();
        private IPAddress _bindIP;
        private SessionTypes _sessionType;
        private int _port;
        private Latch _statusLatch = new Latch((byte)Status.Disconnected);
        private SocketAsyncEventArgs _receiveEventArgs;
        private IConnectionDataListener _dataLisetner;
        private float _clientsBytesRecieved = 0;
        private float _clientsBytesSent = 0;

        public Socket GetSocket
        { get { return _socket; } }

        public TcpConnection(SessionTypes sessionType)
        {
            _sessionType = sessionType;
        }

        //Constructor for already connected socket to communicate over it
        public TcpConnection(Socket socket,SessionTypes sessionType/*, IPAddress bindIP*/)
        {
            if (socket == null)
                throw new ArgumentNullException("Socket is null");

            this._socket = socket;
            this._connected = _socket.Connected;
            _statusLatch.SetStatusBit((byte)Status.Connected, (byte)Status.Disconnected);
            this._sessionType = sessionType;
        }

        public void SetDataListener(IConnectionDataListener listener)
        {
            _dataLisetner = listener;
        }

        public bool Connect(string serverIP, int port)
        {
            bool connected = false;

            try
            {
                _statusLatch.SetStatusBit((byte)Status.Reconnecting, (byte)Status.Disconnected);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (_bindIP != null)
                    socket.Bind(new IPEndPoint(_bindIP, 0/*_port*/));

                IPAddress ip = IPAddress.Parse(serverIP);
                IAsyncResult result = socket.BeginConnect(ip, port,null,null);

                // 4-sec should be enough on local LAN
                bool signalled = result.AsyncWaitHandle.WaitOne(10000, true);

                if(signalled)
                {
                    connected = socket.Connected;
                }

                if (!connected)
                {
                    socket.Close();
                    throw new SocketException((int)SocketError.TimedOut); 
                }

                _receiveEventArgs = null;
                _socket = socket;
                _connected = connected;

                _statusLatch.SetStatusBit((byte)Status.Connected, (byte)Status.Disconnected | (byte)Status.Reconnecting);

                byte[] sessionType = BitConverter.GetBytes((int)_sessionType);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sessionType);

                Send(sessionType, 0, 4);
            }
            catch (Exception e)
            {
                _statusLatch.SetStatusBit((byte)Status.Disconnected, (byte)Status.Connected | (byte)Status.Reconnecting);
                throw new ConnectionException("[" + serverIP+ ":"+port + "] " +  e.Message,e);
            }

            return connected;
        }

        public void Disconnect()
        {
            try
            {
                lock (_sync_lock)
                {
                    _statusLatch.SetStatusBit((byte)Status.Disconnected, (byte)Status.Connected);
                    if (_socket != null && _socket.Connected)
                    {
                        _socket.Close();
                        //_socket.Dispose();
                    }
                }
            }
            catch (Exception) { }
        }

        public bool Send(byte[] buffer, int offset, int count)
        {
            bool sent = false;

            //If we want to wait for re-connect
            _statusLatch.WaitForAny((byte)Status.Disconnected |(byte)Status.Connected);

            if (!_statusLatch.IsAnyBitsSet((byte)Status.Connected))
                throw new ConnectionException();

            

            lock (_sync_lock)
            {
                if (_connected)
                {
                    int dataSent = 0;

                    while (count > 0)
                    {
                        try
                        {
                            dataSent = _socket.Send(buffer, offset, count, SocketFlags.None);
                            offset += dataSent;
                            count = count - dataSent;
                        }
                        catch (SocketException se)
                        {
                            _statusLatch.SetStatusBit((byte)Status.Disconnected, (byte)Status.Connected| (byte)Status.Reconnecting);
                            _connected = false;
                            throw new ConnectionException(se.Message,se);
                        }
                    }
                    sent = true;
                }
                else
                    throw new ConnectionException();
            }

            AddToClientsBytesSent(buffer.Length);

            return sent;
        }

        public bool Receive(byte[] buffer, int count)
        {
            bool received = false;
            //lock (_sync_lock)
            {
                if (_connected)
                {
                    int receivedCount = 0;
                    int offset = 0;
                    while (count > 0)
                    {
                        try
                        {
                            receivedCount = _socket.Receive(buffer, offset, count, SocketFlags.None);
                            
                            if (receivedCount == 0) throw new SocketException((int)SocketError.ConnectionReset);
                            
                            offset += receivedCount;
                            count = count - receivedCount;
                        }
                        catch (SocketException se)
                        {
                            _statusLatch.SetStatusBit((byte)Status.Disconnected, (byte)Status.Connected | (byte)Status.Reconnecting);
                            _connected = false;
                            throw new ConnectionException(se.Message,se);
                        }
                    }
                    received = true;
                }
                else
                    throw new ConnectionException();
            }
            AddToClientsBytesRecieved(buffer.Length);
            return received;
        }
        /// <summary>
        /// Post a receive request on connection. Receive is performed in backgroud. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <param name="context"></param>
        public void ReceiveAsync(byte[] buffer, int count,Object context)
        {
            if(_receiveEventArgs == null)
            {
                _receiveEventArgs = new SocketAsyncEventArgs();
                _receiveEventArgs.RemoteEndPoint = _socket.RemoteEndPoint;
                _receiveEventArgs.Completed += OnReceiveCompleted;
            }

            BeginReceive(buffer,0, count, context);

        }

        private void BeginReceive(byte[] buffer, int offset,int count, Object context)
        {
            _receiveEventArgs.UserToken = context;
            _receiveEventArgs.SetBuffer(buffer, offset, count);
            if (!_socket.ReceiveAsync(_receiveEventArgs))
                OnReceiveCompleted(null, _receiveEventArgs);
        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (_socket == null) return;

                if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success)
                {
                    _statusLatch.SetStatusBit((byte)Status.Disconnected, (byte)Status.Connected | (byte)Status.Reconnecting);
                    _connected = false;
 
                    if (_dataLisetner != null)
                    {
                        SocketException socketError = new SocketException((int)SocketError.ConnectionReset);
                        _dataLisetner.OnDisconnected(this,new ConnectionException(socketError.Message,socketError));
                    }
                    return;
                }

                if(e.BytesTransferred < e.Count)
                {
                    BeginReceive(e.Buffer, e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred, e.UserToken);
                    return;
                }

                AddToClientsBytesRecieved(e.Buffer.Length);
                if (_dataLisetner != null)
                    _dataLisetner.OnDataReceived(this, e.Buffer, e.UserToken);

            }
            catch(Exception ex)
            {
                Disconnect();
                if (_dataLisetner != null)
                    _dataLisetner.OnDisconnected(this,ex);
            }
        }

        public bool IsConnected
        {
            get
            {
                return !_statusLatch.IsAnyBitsSet((byte)Status.Disconnected);
            }
        }

        public void Bind(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _bindIP = IPAddress.Parse(address);
        }

        public float ClientsBytesSent
        {
            get { return _clientsBytesSent; }
        }

        public float ClientBytesReceived
        {
            get { return _clientsBytesRecieved; }
        }

        internal void AddToClientsBytesSent(long value)
        {
            Interlocked.Exchange(ref this._clientsBytesSent, value);
        }

        internal void AddToClientsBytesRecieved(long value)
        {
            Interlocked.Exchange(ref this._clientsBytesRecieved, value);
        }

    }
}
