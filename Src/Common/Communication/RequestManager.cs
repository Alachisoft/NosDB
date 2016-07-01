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
using System.Collections;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using System.Threading;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;

namespace Alachisoft.NosDB.Common.Communication
{
    public class RequestManager : IChannelEventListener, IDisposable
    {
        IChannel _channel;
        Hashtable _requests = new Hashtable();
        object _lock = new object();
        long _lastRequestId;
        bool _resendRequestOnChannelDisconnect = false;
        private int _requestTimeout; //= 90 * 1000; //default is ninety second

        public RequestManager(IChannel channel, bool isRegisterResponseListener,int requestTimeout = 90)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            _channel = channel;

            if (requestTimeout == 0) _requestTimeout = Timeout.Infinite;
            else _requestTimeout = requestTimeout*1000;
            
            if (isRegisterResponseListener)
                _channel.RegisterEventListener(this);
        }

        public int RequestTimedout
        {
            get { return _requestTimeout; }
            set { _requestTimeout = value; }
        }

        public IAsyncResult BeginSendRequest(IRequest request)
        {
            AsyncResult result = new AsyncResult();
            result.Timeout = this.RequestTimedout;

            SendRequest(request, false, result);
            return result;
        }

        public IResponse EndSendRequest(IAsyncResult result)
        {
            AsyncResult asyncResult = result as AsyncResult;

            bool signaled = asyncResult.AsyncWaitHandle.WaitOne(asyncResult.Timeout);

            if (!signaled)
            {
                throw new Common.Exceptions.TimeoutException("Request has timed out. Did not receive response from " + _channel.Server);
            }

            if (asyncResult.Error != null)
                throw asyncResult.Error;

            IResponse rsp = asyncResult.Response as IResponse;

            if (rsp != null && rsp.Error != null)
                throw rsp.Error;

            return (IResponse)asyncResult.Response;
        }

        public object SendRequest(IRequest request)
        {
            return SendRequest(request, true, null);
        }

        private object SendRequest(IRequest request, bool waitForResponse, IResponseListener listener)
        {
            IResponse response = null;

            if (request.NoResponse)
            {

                _channel.SendMessage(request);

                return response;
            }

            request.RequestId = GenerateRequestId();
            bool lockReacquired = false;
            RequestResponsePair reqRespPair = new RequestResponsePair();
            reqRespPair.Listener = listener;

            lock (_lock)
            {
                reqRespPair.Request = request;

                if (!_requests.Contains(request.RequestId))
                {
                    _requests.Add(request.RequestId, reqRespPair);
                }
            }
            bool unregiserRequst = waitForResponse;

            lock (reqRespPair)
            {
                try
                {
                    _channel.SendMessage(request);
                    reqRespPair.RequestSentOverChannel = true;


                    if (waitForResponse)
                    {
                        lockReacquired = System.Threading.Monitor.Wait(reqRespPair, _requestTimeout);
                    }
                    else
                        return null;
                }
                catch (ChannelException e)
                {
                    if (unregiserRequst)
                    {
                        lock (_lock)
                        {
                            _requests.Remove(request.RequestId);
                        }
                    }

                    if (listener != null)
                    {
                        listener.OnError(e);
                    }
                    throw;
                }
                finally
                {
                    if (unregiserRequst)
                    {
                        lock (_lock)
                        {
                            _requests.Remove(request.RequestId);
                        }
                    }
                }
            }

            if (!lockReacquired)
                throw new Alachisoft.NosDB.Common.Exceptions.TimeoutException("Request has timed out. Did not receive response from " + _channel.Server);

            if (reqRespPair.ChannelException != null)
                throw reqRespPair.ChannelException;

            response = reqRespPair.Response as IResponse;

            if (response != null && response.Error != null)
                throw response.Error;

            return response;
        }

        public void SendResponse(IResponse response)
        {
            try
            {
                if(_channel != null)
                    _channel.SendMessage(response);
            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: RequestMgr.SendResponse()", e.ToString());

            }
        }

        private long GenerateRequestId()
        {
            lock (this)
            {
                long requestId = ++_lastRequestId;
                if (requestId < 0)
                {
                    _lastRequestId = 0;
                    requestId = 0;
                }
                return requestId;
            }
        }

        #region  /                      --- IChannelEventListener's Implementation---               /

        public void ReceiveMessage(IChannelMessage response)
        {
            IResponse _response = (IResponse)response;
            RequestResponsePair reqResponsePair = _requests[_response.RequestId] as RequestResponsePair;
            bool unregister = false;
            if (reqResponsePair != null)
            {
                lock (reqResponsePair)
                {

                    if (reqResponsePair != null)
                    {
                        reqResponsePair.Response = _response;
                        System.Threading.Monitor.Pulse(reqResponsePair);

                        if (reqResponsePair.Listener != null)
                        {
                            reqResponsePair.Listener.OnResponseReceived(response);
                            unregister = true;

                        }
                    }

                }

                if (unregister)
                {
                    lock (_lock)
                    {
                        _requests.Remove(_response.RequestId);
                    }
                }
            }
        }

        public void ChannelDisconnected(string reason)
        {
            lock (_lock)
            {
                Hashtable requestClone = _requests.Clone() as Hashtable;
                IDictionaryEnumerator ide = requestClone.GetEnumerator();

                while (ide.MoveNext())
                {
                    RequestResponsePair reqRspPair = ide.Value as RequestResponsePair;

                    if (!reqRspPair.RequestSentOverChannel) continue;

                    lock (reqRspPair)
                    {
                        if (_resendRequestOnChannelDisconnect)
                        {
                            //resend the request when channel is disconnected
                            try
                            {
                                if (_channel != null) _channel.SendMessage(reqRspPair.Request);
                            }
                            catch (ChannelException ce)
                            {
                                reqRspPair.ChannelException = ce;
                                System.Threading.Monitor.PulseAll(reqRspPair);

                                if (reqRspPair.Listener != null)
                                {
                                    reqRspPair.Listener.OnError(reqRspPair.ChannelException);
                                }
                            }
                        }
                        else
                        {
                            reqRspPair.ChannelException = new ChannelException(reason);
                            System.Threading.Monitor.PulseAll(reqRspPair);

                            if (reqRspPair.Listener != null)
                            {
                                reqRspPair.Listener.OnError(reqRspPair.ChannelException);
                            }
                        }
                    }
                }
            }
        }


        #endregion

        private void Dispose(bool gracefull)
        {
            try
            {
                lock (_lock)
                {
                    if (_requests != null)
                        _requests.Clear();

                    if (_channel != null)
                    {
                        _channel.Disconnect();
                        _channel = null;
                    }
                }
            }
            catch (Exception e)
            {
            }
            if (gracefull)
                GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~RequestManager()
        {
            Dispose(false);
        }

        #region /                                   --- Inner classes and intrfaces ----                            /

        public interface IResponseListener
        {
            void OnResponseReceived(Object response);
            void OnError(Exception exception);
        }

        public class AsyncResult : IAsyncResult, IResponseListener
        {
            private bool _completedSynchronously;
            private bool _isCompleted;
            private ManualResetEvent _waitHandle = new ManualResetEvent(false);
            private object _state;
            private object _response;
            private Exception _error;
            private int _timeout;

            public object AsyncState
            {
                get { return _state; }
            }

            public int Timeout
            {
                get { return _timeout; }
                set { _timeout = value; }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return _waitHandle; }
            }

            public Address ChannelAddress { get; set; }

            public bool CompletedSynchronously
            {
                get { return _completedSynchronously; }
            }

            public bool IsCompleted
            {
                get { return _isCompleted; }
            }

            public Object Response { get { return _response; } }

            public Exception Error { get { return _error; } }

            public void SetCompleted(bool completedSynchronously)
            {
                lock (this)
                {
                    _completedSynchronously = completedSynchronously;
                    _isCompleted = true;
                    _waitHandle.Set();
                }
            }

            public void OnResponseReceived(object response)
            {
                lock (this)
                {
                    _response = response;
                    SetCompleted(false);
                }
            }


            public void OnError(Exception exception)
            {
                lock (this)
                {
                    _error = exception;
                    SetCompleted(false);
                }
            }
        }

        #endregion
    }
}
