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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Communication
{
    public class LocalChannel : IDualChannel, IRequestListener    
    {
        private IRequestListener _requestListener;

        public LocalChannel(Address localAddress,IRequestListener listener)
        {
            PeerAddress = localAddress;
            RegisterRequestHandler(listener);
        }

        public void RegisterRequestHandler(IRequestListener requestHander)
        {
            _requestListener = requestHander;
        }

        public bool Connected
        {
            get { return true; }
        }

        public byte[] ReadFromSocket()
        {
            throw new NotImplementedException();
        }

        public bool ShouldTryReconnecting
        {
            get
            {
                return false;
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        public ConnectInfo ConnectInfo
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void StartReceiverThread()
        {
           // throw new NotImplementedException();
        }

        public Net.Address PeerAddress
        {
            get;
            private set;
        }

        public bool Connect(bool shouldStartReceiving)
        {
            return true;
            //throw new NotImplementedException();
        }

        public bool RetryConnect()
        {
            return true;
        }

        public bool IsAuthenticated { set; get; }

        public void Disconnect()
        {
            
        }

        public object SendMessage(object message, bool NoResponse)
        {
            IRequest request = message as IRequest;

            if(!(message is IRequest))
            {
                request = new ChannelRequest();
               // request.Channel = this;
                request.Message = message;
                request.NoResponse = NoResponse;
                request.Source = PeerAddress;
            }
                
            return OnRequest(request);
        }

        public IAsyncResult BeginSendMessage(object msg)
        {
            AsyncResult result = new AsyncResult(this);
            result.Message = msg;

            ThreadPool.QueueUserWorkItem(new WaitCallback(OnSendMessage), result);

            return result;
        }

        private void OnSendMessage(object state)
        {
            AsyncResult result = state as AsyncResult;
            try
            {
                result.ResponseMessage =  result.Channel.SendMessage(result.Message, false);
                result.IsCompleted = true;
            }
            catch(Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("LocalChannel.OnSendMessage() " , e.ToString());
            }


        }

        public object EndSendMessage(IAsyncResult result)
        {
            AsyncResult asyncResult = result as AsyncResult;

            asyncResult.AsyncWaitHandle.WaitOne();
            return asyncResult.ResponseMessage;
        }

        public object OnRequest(IRequest request)
        {
            return _requestListener.OnRequest(request);
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                LoggerManager.Instance.ShardLogger.Warn("LocalChannel.OnChDisconnected", "local channel disconnected.");
        }


        class AsyncResult :IAsyncResult
        {
            LocalChannel _parentChannel;
            Object _state;
            ManualResetEvent _waitHandle = new ManualResetEvent(false);
            bool _synchronousCompletion;
            private bool _completed;

            public AsyncResult(LocalChannel parentChannel)
            {
                _parentChannel = parentChannel;

            }

            public object AsyncState
            {
                get { return _state; }
            }

            public LocalChannel Channel
            {
                get { return _parentChannel; }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return _waitHandle; }
            }

            public bool CompletedSynchronously
            {
                get { return _synchronousCompletion; }
            }

            public bool IsCompleted
            {
                get { return _completed; }
                set
                {
                    _completed = value;
                    if (value)
                        _waitHandle.Set();
                }
            }

            public object Message { get; set; }

            public object ResponseMessage { get; set; }
        }

    }
}
