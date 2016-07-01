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
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Common.Communication
{
    public class ClientChannel : IRequestResponseChannel
    {
        private Address _peerAddress;        
        private IChannel tcpChannel = null;
        private RequestManager requestManager = null;
        private SessionTypes _sessionType;

        public ClientChannel(string peerIP, int peerPort, string bindingIP,SessionTypes sessionType, ITraceProvider traceProvider,IChannelFormatter channelFormatter,int requestTimeout)
        {
            _peerAddress = new Address(peerIP, peerPort);
            tcpChannel = new TcpChannel(_peerAddress.IpAddress.ToString(), _peerAddress.Port, bindingIP,sessionType, traceProvider);
            tcpChannel.Formatter = channelFormatter;
            requestManager = new RequestManager(tcpChannel, true, requestTimeout);
        }

        public void RegisterRequestHandler(IRequestListener requestHander)
        {
            throw new NotImplementedException();
        }

        public Alachisoft.NosDB.Common.Net.Address PeerAddress
        {
            get { return _peerAddress; }
        }               

        public bool Connect(bool shouldStartReceiver)
        {
            try
            {
                if (tcpChannel != null)
                    return tcpChannel.Connect(true);
            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: ClientChannel.Connect()", e.ToString());
            }

            return false;
        }

        public void Disconnect()
        {
            if (tcpChannel != null)
                tcpChannel.Disconnect();
        }

        public object SendMessage(object message, Boolean NoResponse)
        {
            if (tcpChannel != null && requestManager != null)
            {
                IRequest request = (IRequest)message;                
                request.NoResponse = NoResponse;

                IResponse channelResponse = (IResponse)requestManager.SendRequest(request);
                if(channelResponse!=null && channelResponse.ResponseMessage!=null)
                    return channelResponse.ResponseMessage;

                return null ;
            }

            throw new ChannelException("Channel is not initialized");
        }

        public IAsyncResult BeginSendMessage(object msg)
        {
            if (tcpChannel != null && requestManager != null)
            {
                IRequest request = (IRequest)msg;
                return requestManager.BeginSendRequest(request);
            }

            throw new ChannelException("Channel is not initialized");
        }

        public object EndSendMessage(IAsyncResult result)
        {
            if (result == null)
                return null;          

            IResponse channelResponse = (IResponse)requestManager.EndSendRequest(result);

            if (channelResponse != null && channelResponse.ResponseMessage != null)
                return channelResponse.ResponseMessage;

            return null;
		}
        public bool RetryConnect()
        {
            throw new NotImplementedException("Client Channel");
        }

        public bool IsAuthenticated { set; get; }

        public void Dispose()
        {
            if(requestManager!=null)
            {
                requestManager.Dispose();
            }            
        }

        ~ClientChannel()
        {
            Dispose();
        }
    }
}
