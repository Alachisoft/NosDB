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
using System.Net.Sockets;
using System.Text;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Threading;

namespace Alachisoft.NosDB.Common.Communication
{
    public class ServerChannel : IServerChannel, IChannelEventListener
    {
        private Address peerAddress;
        private IChannel tcpChannel = null;
        private SessionTypes _sessionType;
        private IRequestListener requestListener = null;
        private IThreadPool _threadPool;

        public ServerChannel(IConnection connection, string peerIP, int peerPort, string bindingIP, SessionTypes sessionType, ITraceProvider traceProvider, IChannelFormatter channelFormatter)
        {
            tcpChannel = new TcpChannel(connection, peerIP, peerPort, bindingIP, traceProvider);
            tcpChannel.Formatter = channelFormatter;
            peerAddress = new Address(peerIP, peerPort);
            tcpChannel.RegisterEventListener(this);

            _threadPool = new ClrThreadPool();
            _threadPool.Initialize();
        }

        public void RegisterRequestHandler(IRequestListener requestHander)
        {
            throw new NotImplementedException();
        }

        public Alachisoft.NosDB.Common.Net.Address PeerAddress
        {
            get { return peerAddress; }
        }

        public bool Connect(bool shouldStartReceiver)
        {
            if (tcpChannel != null)
                return tcpChannel.Connect(true);

            return false;
        }

        public void Disconnect()
        {
            if (tcpChannel != null)
                tcpChannel.Disconnect();
        }

        public object SendMessage(object message, Boolean NoResponse)
        {
            if (tcpChannel != null)
            {
                return tcpChannel.SendMessage(message);
            }

            throw new ChannelException("Channel is not initialized");
        }

        public void AddRequestListener(IRequestListener listener)
        {
            requestListener = listener;
        }

        public void RemoveRequestListener(IRequestListener listener)
        {
            requestListener = null;
        }

        public void ReceiveMessage(IChannelMessage response)
        {
            if (response is IRequest)
            {
                _threadPool.ExecuteTask(new RequestDeliverTask(response, requestListener));
            }
        }

        public void ChannelDisconnected(string reason)
        {
            if (this.requestListener != null)
                requestListener.ChannelDisconnected(this, reason);
        }

        #region /                                       --- Thread Pool Tasks ---                           /

        class RequestDeliverTask : IThreadPoolTask
        {
            IChannelMessage _message;
            IRequestListener _requestListener;

            public RequestDeliverTask(IChannelMessage message, IRequestListener requestLisetner)
            {
                _message = message;
                _requestListener = requestLisetner;
            }

            public void Execute()
            {
                if (_requestListener != null)
                {
                    _requestListener.OnRequest((IRequest)_message);
                }
            }
        }
        #endregion

        public bool IsAuthenticated { set; get; }

        public IAsyncResult BeginSendMessage(object msg)
        {
            throw new NotImplementedException();
        }

        public object EndSendMessage(IAsyncResult result)
        {
            throw new NotImplementedException();
		}
		
        public bool RetryConnect()
        {
            throw new NotImplementedException("Server Channel");
        }

        public float ClientsBytesSent
        {
            get { return tcpChannel != null ? tcpChannel.ClientsBytesSent : 0; }
        }

        public float ClientBytesReceived
        {
            get { return tcpChannel != null ? tcpChannel.ClientBytesReceived : 0; }
        }
    }

}
