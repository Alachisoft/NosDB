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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Exceptions;

namespace Alachisoft.NosDB.Common.Communication
{  
    public class DualChannel : IDualChannel, IRequestListener
    {
        private Address _peerAddress;
        private IRequestListener _requestListener;
      //  private IDualChannelListener dualChannelListener;
        private TcpChannel tcpChannel = null;
        private RequestManager requestManager = null;
        private SessionTypes _sessionType;
        private bool _shouldTryReconnecting = true;

        public DualChannel(string peerIP, int peerPort, string bindingIP, SessionTypes sessionType, ITraceProvider traceProvider, IChannelFormatter channelFormatter, int requestTimeout = 90)
        {
            tcpChannel = new TcpChannel(peerIP, peerPort, bindingIP,sessionType, traceProvider);
            tcpChannel.Formatter = channelFormatter;
            requestManager = new RequestManager(tcpChannel,false,requestTimeout);
            _peerAddress = new Address(peerIP, peerPort);
            DualChannelListener _lisetner = new DualChannelListener(tcpChannel.UsesAsynchronousIO?null: new ClrThreadPool());
            _lisetner.RegisterRequestListener(this);
            _lisetner.RegisterResponseListener(requestManager);
            tcpChannel.RegisterEventListener(_lisetner);            
        }

        public DualChannel(IConnection connection, string peerIP, int peerPort, string bindingIP, SessionTypes sessionType, ITraceProvider traceProvider, IChannelFormatter channelFormatter, int requestTimeout = 90)
        {
            //==================================

            tcpChannel = new TcpChannel(connection, peerIP, peerPort, bindingIP, traceProvider);
            tcpChannel.Formatter = channelFormatter;
            requestManager = new RequestManager(tcpChannel, false, requestTimeout);
            _peerAddress = new Address(peerIP, peerPort);
            DualChannelListener _lisetner = new DualChannelListener(tcpChannel.UsesAsynchronousIO ? null : new ClrThreadPool());
            _lisetner.RegisterRequestListener(this);
            _lisetner.RegisterResponseListener(requestManager);
            tcpChannel.RegisterEventListener(_lisetner);

            //=================================

        }

        public bool IsAuthenticated { set; get; }


        public Alachisoft.NosDB.Common.Net.Address PeerAddress
        {
            get { return _peerAddress; }
        }               

        public bool Connect(bool shouldStartReceiving)
        {           
            if (tcpChannel != null)
                return tcpChannel.Connect(shouldStartReceiving);

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
                IRequest request = new ChannelRequest();
                request.Message = message;
                request.NoResponse = NoResponse;

                IResponse channelResponse = (IResponse)requestManager.SendRequest(request);
                if(channelResponse!=null && channelResponse.ResponseMessage!=null)
                    return channelResponse.ResponseMessage;
                
                return null;
            }

            throw new ChannelException("Channel is not initialized");
        }

        #region IRequestListener Implementation

        public Object OnRequest(IRequest request)
        {
            if (request != null)
            {
                IResponse response = new ChannelResponse();
                response.RequestId = request.RequestId;
                response.Channel = request.Channel;
                response.Source = request.Source;
                try
                {
                    response.ResponseMessage = _requestListener.OnRequest(request);
                }
                catch (Exception ex)
                {
                    response.Error = new RemoteException(ex);
                }
                try
                {
                    if (requestManager != null && !request.NoResponse)
                        requestManager.SendResponse(response);
                }
                catch (ChannelException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null &&
                        LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("Error: Dualchannel.OnRequest()", e.ToString());
                }
            }
            return null;
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            if (this._requestListener != null)
                _requestListener.ChannelDisconnected(this, reason);
        }

        #endregion

        #region IDualChannel

        public void RegisterRequestHandler(IRequestListener requestHander)
        {
            this._requestListener = requestHander;
        }


        public IAsyncResult BeginSendMessage(object msg)
        {
            if (tcpChannel != null && requestManager != null)
            {
                IRequest request = new ChannelRequest();
                request.Message = msg;

                //return new DualAsyncResult(requestManager.BeginSendRequest(request));
                return requestManager.BeginSendRequest(request);
            }
            
            throw new ChannelException("Channel is not initialized");
        }




        public bool Connected
        {
            get
            {
                if (this.tcpChannel != null)
                {
                    return tcpChannel.Connected;
                }

                return false;             
            }
        }


        public bool RetryConnect()
        {
            if(tcpChannel!=null)
                return tcpChannel.RetryConnection();
            
            return false;
        }

        public object EndSendMessage(IAsyncResult result)
        {
            if (result == null)
                return null;
            //IAsyncResult requestIAysncResult = ((DualAsyncResult)result).AsyncResult;
            //return requestManager.EndSendRequest(requestIAysncResult);            

            IResponse channelResponse = (IResponse) requestManager.EndSendRequest(result);

            if (channelResponse != null && channelResponse.ResponseMessage != null)
                return channelResponse.ResponseMessage;

            return null;
        }

        #endregion

        #region /                                   --- Chaneel Listener ---                            /

        class DualChannelListener : IChannelEventListener
        {
            private IChannelEventListener _responseListener;
            private IRequestListener _requestListener;
            private IThreadPool _threadPool;

            public DualChannelListener(IThreadPool threadPool)
            {
                _threadPool = threadPool;
                if (_threadPool != null) _threadPool.Initialize();
            }

            public void RegisterResponseListener(IChannelEventListener responseListener)
            {
                _responseListener = responseListener;
            }

            public void RegisterRequestListener(IRequestListener requestListener)
            {
                if (requestListener == null)
                    throw new ArgumentNullException("requestListener");

                _requestListener = requestListener;
            }

            public void ReceiveMessage(IChannelMessage message)
            {
                try
                {
                    IRequest request = message as IRequest;
                    if (request != null)
                    {
                        if (_threadPool != null)
                            _threadPool.ExecuteTask(new RequestDeliverTask(message, _requestListener));
                        else
                            _requestListener.OnRequest(request);
                    }
                    else
                    {
                        IResponse response = message as IResponse;

                        if (_responseListener != null)
                        {
                            _responseListener.ReceiveMessage(message);
                        }
                        //if(response != null)
                        //    _threadPool.ExecuteTask(new ResponseDeliverTask(message, _responseListener));
                    }
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                        LoggerManager.Instance.ServerLogger.Error("DualChannelListener.RecieveMessage", ex.Message);
                }
            }

            public void ChannelDisconnected(string reason)
            {
                if (_responseListener != null)
                    _responseListener.ChannelDisconnected(reason);

                if (_threadPool != null)
                    _threadPool.ExecuteTask(new ChannelDisconnectEventTask(null, reason, _requestListener));
                else
                    _requestListener.ChannelDisconnected(null, reason);
                //if (_requestListener != null)
                //    _requestListener.ChannelDisconnected(null, reason);
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
                    try
                    {
                        if (_requestListener != null)
                        {
                            _requestListener.OnRequest((IRequest)_message);
                        }
                    }
                    catch(Exception)
                    {

                    }
                }
            }

            class ChannelDisconnectEventTask : IThreadPoolTask
            {
                String reason;
                IRequestListener _requestListener;
                IRequestResponseChannel channel;

                public ChannelDisconnectEventTask(IRequestResponseChannel channel,string reason, IRequestListener requestLisetner)
                {
                    this.reason = reason;
                    _requestListener = requestLisetner;
                    this.channel = channel;
                }

                public void Execute()
                {
                    try
                    {
                        if (_requestListener != null)
                        {
                            _requestListener.ChannelDisconnected(channel, reason);
                        }
                    }
                    catch(Exception)
                    {

                    }
                }
            }


            //class ResponseDeliverTask : IThreadPoolTask
            //{
            //    IChannelMessage _message;
            //    IChannelEventListener _responseListener;
                

            //    public ResponseDeliverTask(IChannelMessage message, IChannelEventListener responseListener)
            //    {
            //        _message = message;
            //        _responseListener = responseListener;
            //    }

            //    public void Execute()
            //    {
            //        try
            //        {
            //            if (_responseListener != null)
            //            {
            //                _responseListener.ReceiveMessage(_message);
            //            }
            //        }
            //        catch (ChannelException e)
            //        {
            //            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
            //                LoggerManager.Instance.ShardLogger.Error("DualChannel.ResponseDeliverTask.Execute()", e.ToString());
                        
            //            throw;
            //        }
            //    }
            //}

            #endregion
        }
        #endregion





        public bool ShouldTryReconnecting
        {
            get
            {
                return _shouldTryReconnecting;
            }
            set
            {
                _shouldTryReconnecting = value;
            }
        }

        public ConnectInfo ConnectInfo
        {
            get;
            set;
        }

        public void StartReceiverThread()
        {
            tcpChannel.StartReceiverThread();
        }
        public byte[] ReadFromSocket()
        {
            Socket socket = null;
            if(tcpChannel != null)
            {
                IConnection connection = tcpChannel.GetConnection;
                if (connection != null)
                {
                    if (connection.IsConnected)
                    {
                        int DATA_SIZE_BUFFER_LENGTH = 10;
                        Byte[] sizeBuffer = new Byte[DATA_SIZE_BUFFER_LENGTH];

                        connection.Receive(sizeBuffer, sizeBuffer.Length);
                        int rspLength = Convert.ToInt32(UTF8Encoding.UTF8.GetString(sizeBuffer, 0, sizeBuffer.Length));

                        if (rspLength > 0)
                        {
                            byte[] dataBuffer = new byte[rspLength];
                            connection.Receive(dataBuffer, dataBuffer.Length);
                            return dataBuffer;
                            //deserialize the message
                            //IRequest message = ChannelFormatter.Deserialize(dataBuffer) as IRequest;
                            //info = (SessionInfo)message.Message;
                        }
                    }
                }
            }
            return null;
        }


         public void Dispose()
        {
            if(requestManager!=null)
            {
                requestManager.Dispose();
            }            
        }

         ~DualChannel()
        {
            Dispose();
        }

       
    }

    /*DualAsyncResult class could wrap the IAsyncResult returned by RequestManager
    public class DualAsyncResult : IAsyncResult
    {
        IAsyncResult asyncResult = null;

        public IAsyncResult AsyncResult
        {
            get { return asyncResult; }           
        }

        public DualAsyncResult(IAsyncResult result) 
        {
            Common.MiscUtil.IsArgumentNull(result);

            asyncResult = result;
        }

        public object AsyncState
        {
            get { return asyncResult.AsyncState; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return asyncResult.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return asyncResult.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return asyncResult.IsCompleted; }
        }

        public Object Response
        {
            get
            {
                IResponse response = ((Alachisoft.NosDB.Common.Communication.RequestManager.AsyncResult)asyncResult).Response as IResponse;

                if (response != null)
                    return response.ResponseMessage;

                return null;
            }
        }

        public Exception Error
        {
            get 
            {
                return ((Alachisoft.NosDB.Common.Communication.RequestManager.AsyncResult)asyncResult).Error;                
            }
        
        }
    }

    */
}
