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
using System.Net;
using System.Threading;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using System.IO;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Net;

namespace Alachisoft.NosDB.Common.Communication
{
    public class TcpChannel: IChannel,IConnectionDataListener
    {
        enum ReciveContext
        {
            LengthReceive,
            DataReceive
        }

        const int DATA_SIZE_BUFFER_LENGTH = 10; //(in bytes)
        
        protected IConnection _connection;
        protected string _serverIP;
        protected string _bindIP;
        protected int _port;
        byte[] _sizeBuffer = new byte[DATA_SIZE_BUFFER_LENGTH];
        protected IChannelFormatter _formatter;
        protected IChannelEventListener _eventListener;
        protected Thread _receiverThread;
        protected ITraceProvider _traceProvider;
        private string _name;
        private Address _sourceAddress;
        private SessionTypes _sessionType;
        private Object _connectionMutex = new object();
        private bool _forcedDisconnected;
        private bool _useAsyncReceive = true;
        private Exception _e;

        public IConnection GetConnection
        { get { return _connection; } }

        public bool UsesAsynchronousIO { get { return _useAsyncReceive; } }

        public TcpChannel(string serverIP, int port,string bindingIP,SessionTypes sessionType,ITraceProvider traceProvider)
        {
            if (string.IsNullOrEmpty(serverIP))
                throw new ArgumentNullException("serverIP");
            _serverIP = serverIP;
            _port = port;
            _bindIP = bindingIP;
            _sessionType = sessionType;
            _traceProvider = traceProvider;
            _sourceAddress = new Address(serverIP, port);
        }

        public TcpChannel(IConnection connection, string serverIP, int port, string bindingIP, ITraceProvider traceProvider)
        {
            if (string.IsNullOrEmpty(serverIP))
                throw new ArgumentNullException("serverIP");

            _connection = connection;
            _connection.SetDataListener(this);
            _serverIP = serverIP;
            _port = port;
            _bindIP = bindingIP;            
            _traceProvider = traceProvider;
            _sourceAddress = new Address(serverIP, port);
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        
        public virtual bool Connect(bool shouldStartReceiving)
        {
            lock (_connectionMutex)
            {
                if (_formatter == null)
                {
                    throw new Exception("Channel formatter is not specified");
                }

                if (_eventListener == null)
                {
                    throw new Exception("There is no channel event listener specified");
                }

                try
                {
                    if (_connection == null)
                    {
                        _connection = new TcpConnection(_sessionType);
                       
                        _connection.SetDataListener(this);

                        if (!string.IsNullOrEmpty(_bindIP))
                            _connection.Bind(_bindIP);
                    }

                    if (!_connection.IsConnected)
                        _connection.Connect(_serverIP, _port);
                    _forcedDisconnected = false;

                    if (shouldStartReceiving)
                    {
                        StartReceiverThread();
                    }
                    return true;
                }
                catch (ConnectionException ce)
                {
                    if (_traceProvider != null)
                    {
                        _traceProvider.TraceError(Name + ".Connect", ce.ToString());
                    }
                    throw new ChannelException(ce.Message, ce);
                }
            }
            return false;
        }    

        public virtual void Disconnect()
        {
            lock (_connectionMutex)
            {
                _forcedDisconnected = true;
                if (_connection != null)
                {
                    _connection.Disconnect();
                    if (_receiverThread != null && _receiverThread.IsAlive)
                        _receiverThread.Abort();
                }
            }
        }

        public bool SendMessage(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            //first serialize the message using channel formatter
            byte[] serailizedMessage = _formatter.Serialize(message);

            byte[] msgLength = UTF8Encoding.UTF8.GetBytes(serailizedMessage.Length.ToString());

            //message is written in a specific order as expected by Socket server
            ClusteredMemoryStream stream = new ClusteredMemoryStream();
            //stream.Position = 20;//skip discarding buffer
            stream.Write(msgLength,0,msgLength.Length);
            stream.Position = 10;
            stream.Write(serailizedMessage,0,serailizedMessage.Length);

            byte[] finalBuffer = stream.ToArray();
            stream.Close();

            try
            {
                if (EnsureConnected())
                {
                    try
                    {
                        _connection.Send(finalBuffer, 0, finalBuffer.Length);
                        return true;
                    }
                    catch (ConnectionException)
                    {
                        if (EnsureConnected())
                        {
                            _connection.Send(finalBuffer, 0, finalBuffer.Length);
                            return true;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw new ChannelException(e.Message, e);
            }

            return false;
        }

        private bool EnsureConnected()
        {
            lock (_connectionMutex)
            {
                if (_connection != null && !_connection.IsConnected)
                {
                    Disconnect();
                    Connect(true);
                }
            }

            return _connection.IsConnected;
        }

        public void StartReceiverThread()
        {
            if (!_useAsyncReceive)
            {
                _receiverThread = new Thread(new ThreadStart(Run));
                _receiverThread.IsBackground = true;
                _receiverThread.Start();
            }
            else
                StartReceivingData();
        }
        public bool Connected
        {
            get 
            {
                if (_connection != null)
                    return _connection.IsConnected;

                return false;
            }
        }

        public Boolean RetryConnection()
        {
            return EnsureConnected();
        }

        public virtual void RegisterEventListener(IChannelEventListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException("listener");

            _eventListener = listener;
        }

        public virtual void UnRegisterEventListener(IChannelEventListener listener)
        {
            if(listener != null && listener.Equals(_eventListener))
                _eventListener = null;
        }

        public IChannelFormatter Formatter
        {
            get
            {
                return _formatter;
            }
            set
            {
                _formatter = value;
            }
        }

        protected virtual Address GetSourceAddress()
        {
            return _sourceAddress;
        }

        private void StartReceivingData()
        {
            if (_connection != null)
                _connection.ReceiveAsync(_sizeBuffer, DATA_SIZE_BUFFER_LENGTH, ReciveContext.LengthReceive);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Run()
        {
            while (true)
            {
                try
                {

                    //receive data size for the response
                    if (_connection != null)
                    {
                        _connection.Receive(_sizeBuffer, DATA_SIZE_BUFFER_LENGTH);

                        int rspLength = Convert.ToInt32(UTF8Encoding.UTF8.GetString(_sizeBuffer, 0, _sizeBuffer.Length));

                        if (rspLength > 0)
                        {
                            byte[] dataBuffer = new byte[rspLength];
                            _connection.Receive(dataBuffer, rspLength);

                            //deserialize the message
                            IChannelMessage message = null;
                            if (_formatter != null)
                                message = _formatter.Deserialize(dataBuffer) as IChannelMessage;

                            message.Channel = this;
                            message.Source = GetSourceAddress();
                            if (_eventListener != null)
                                _eventListener.ReceiveMessage(message);
                        }

                    }
                    else
                    {
                        break;
                    }

                }
                catch (ThreadAbortException) { break; }
                catch (ThreadInterruptedException) { break; }
                catch (ConnectionException ce)
                {
                    if (_traceProvider != null && _forcedDisconnected)
                    {
                        _traceProvider.TraceError(Name + ".Run", ce.ToString());
                    }
                    if (_eventListener != null & !_forcedDisconnected) _eventListener.ChannelDisconnected(ce.Message);
                    break;
                }
                catch (Exception e)
                {
                    if (_traceProvider != null)
                    {
                        _traceProvider.TraceError(Name + ".Run", e.ToString());
                    }
                    break;
                    //new ChannelException();
                }
            }
        }


        public string Server
        {
            get {
                
                if (_serverIP != null) return _serverIP;

                return "";
            }
        }



        public void OnDataReceived(IConnection sender, byte[] buffer, object context)
        {
            ReciveContext receiveContext = (ReciveContext)context;

            switch (receiveContext)
            {
                case ReciveContext.LengthReceive:
                    int rspLength = Convert.ToInt32(UTF8Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                    buffer = new byte[rspLength];
                    sender.ReceiveAsync(buffer, buffer.Length, ReciveContext.DataReceive);
                    break;

                case ReciveContext.DataReceive:
                    sender.ReceiveAsync(_sizeBuffer, DATA_SIZE_BUFFER_LENGTH, ReciveContext.LengthReceive);

                    IChannelMessage message = null;
                    if (_formatter != null)
                        message = _formatter.Deserialize(buffer) as IChannelMessage;

                    message.Channel = this;
                    message.Source = GetSourceAddress();
                    

                    try
                    {
                        if (_eventListener != null)
                            _eventListener.ReceiveMessage(message);
                    }
                    catch (Exception e)
                    {
                        
                        if (_traceProvider != null)
                        {
                            _traceProvider.TraceError(Name + ".Run", e.ToString());
                        }
                    }
                    break;
            }
            
        }

        public void OnDisconnected(IConnection sender, Exception error)
        {
            if (_traceProvider != null && _forcedDisconnected)
            {
                _traceProvider.TraceError(Name + ".Run", error.ToString());
            }
            if (_eventListener != null & !_forcedDisconnected) _eventListener.ChannelDisconnected(error.Message);
        }

        public float ClientsBytesSent
        {
            get { return _connection != null ? _connection.ClientsBytesSent : 0; }
        }

        public float ClientBytesReceived
        {
            get { return _connection != null ? _connection.ClientBytesReceived : 0; }
        }

    }
}
