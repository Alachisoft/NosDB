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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Core.Toplogies.Exceptions;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Threading;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ShardImpl
{
    public class RemoteShard : IShard, IRequestListener, ISessionListener, IConnectionRestorationListener
    {
        private String _name;
        private int _shardPort;
        private Server _primary;
        private IList<Server> _servers;
        //
        private IRequestResponseChannel _remoteShardChannel = null;
        private ITraceProvider _traceProvider = null;
        private IChannelFormatter _channelFormatter = null;
        private String _bindingIP;
        private IShardListener _shardListener;
        private NodeContext context = null;
        private IChannelFactory factory;

        private ResolveChannelDispute _resolveDispute = null;
        private IThreadPool _threadPool;

        //private IList<Server> _activeChannelsList = null;

        private Object _onChannel = new Object();
        private Object _onPrimary = new Object();
        private IConnectionRestoration _connectionRestoration = null;

        public bool IsStarted = false;

        public IChannelFormatter ChannelFormatter
        {
            get { return _channelFormatter; }
        }

        public RemoteShard(IChannelFactory factory, IChannelFormatter channelFormatter, NodeContext context, IConnectionRestoration connectionRestoration)
        {
            this.factory = factory;
            this._channelFormatter = channelFormatter;
            this.context = context;
            this._connectionRestoration = connectionRestoration;
            this._threadPool=new ClrThreadPool();
            this._threadPool.Initialize();
        }

        public NodeContext Context
        {
            get { return context; }
            set { context = value; }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Server Primary
        {
            get { return _primary; }
        }

        public IList<Server> Servers
        {
            get { return _servers; }
        }

        public bool Initialize(ShardConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("remote shard configuration is null");

            this._name = configuration.Name;
            this._shardPort = configuration.Port;
            this._servers = GetShardServerList(configuration);

            this._resolveDispute = new ResolveChannelDispute(context, this);

            return true;
        }

        private IList<Server> GetShardServerList(ShardConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("remote shard configuration is null");

            IList<Server> serverList = new List<Server>();

            int shardPort = configuration.Port;

            if (configuration.Servers != null && configuration.Servers.Nodes != null)
            {
                foreach (Alachisoft.NosDB.Common.Configuration.ServerNode node in configuration.Servers.Nodes.Values)
                {
                    Server server = new Server(new Address(node.Name, shardPort), Status.Stopped);
                    serverList.Add(server);
                }
            }

            return serverList;

        }

        public bool Start()
        {
            if (context != null && context.ConfigurationSession != null)
            {
                Membership membershipInfo = context.ConfigurationSession.GetMembershipInfo(context.ClusterName, _name.ToLower());
                if (membershipInfo != null && membershipInfo.Primary != null)
                {
                    ConnectPrimary(new Address(membershipInfo.Primary.Name, _shardPort));
                  

                    
                }
            }
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                LoggerManager.Instance.ShardLogger.Info("remoteShard.Start()", "Remote shard " + _name + " started successfully");
            return IsStarted = true;

        }

        public bool OnSessionEstablished(Session session)
        {
            // check if the shardConnected event is required to be raised
            bool shardConnected = false;
            Server server = new Server(new Common.Net.Address(session.IP.ToString(), this._shardPort), Status.Initializing);
            IRequestResponseChannel channel = factory.GetChannel(session.Connection, session.IP.ToString(), this._shardPort, context.LocalAddress.ToString(), SessionTypes.Shard, _traceProvider, _channelFormatter);
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _name != null ? _name : "", DatabaseName = "" });
            try
            {
                if (_remoteShardChannel != null && !((IDualChannel)_remoteShardChannel).Connected && _remoteShardChannel.PeerAddress.Equals(server))
                {
                    session.Connection.Disconnect();
                    //throw new ChannelException("already connected with shard"+_name);
                }

                if (channel.Connect(false))
                {
                    ConnectInfo.ConnectStatus status = ConnectInfo.ConnectStatus.CONNECT_FIRST_TIME;
                    if (_remoteShardChannel != null && _remoteShardChannel.PeerAddress.Equals(server.Address))
                        status = ConnectInfo.ConnectStatus.RECONNECTING;

                    lock (_onChannel)
                    {
                        _remoteShardChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel as IDualChannel, status), _remoteShardChannel);
                        ((IDualChannel)_remoteShardChannel).StartReceiverThread();
                        ((IDualChannel)_remoteShardChannel).RegisterRequestHandler(this);

                        shardConnected = true;
                    }
                    if (_primary != null && !_primary.Address.Equals(server.Address))
                    {
                        BrokenConnectionInfo info = new BrokenConnectionInfo();
                        info.BrokenAddress = _primary.Address;
                        info.SessionType = SessionTypes.Shard;
                        _connectionRestoration.UnregisterListener(info);
                    }
                    lock (_onPrimary)
                    {
                        _primary = server;
                    }

                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        LoggerManager.Instance.ShardLogger.Info("RemoteShard.OnSessionEstd()", "Session of the shard " + _name + " estd successfully");
                    return IsStarted = true;


                }
                else
                    return false;
            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: RemoteShard.OnSessionEstd()", e.ToString());
                return false;
            }
            finally
            {
                if (shardConnected)
                    ShardConnected();
            }
        }

        private void ConnectPrimary(Address primary) 
        {
            // check if the shardConnected event is required to be raised
            bool shardConnected = false;

            // If primary is null, it means the respective shard has no primary anymore so
            if (primary == null)
            {
                if (_remoteShardChannel != null)
                {
                    ((DualChannel)_remoteShardChannel).ShouldTryReconnecting = false;
                    _remoteShardChannel.Disconnect();
                    _remoteShardChannel = null;
                }
            }
            else
            {
                if (_remoteShardChannel != null)
                {
                    if (_remoteShardChannel.PeerAddress.Equals(primary) && ((IDualChannel)this._remoteShardChannel).Connected)
                        return;
                }


                bool isConnected = false;
                IRequestResponseChannel channel = factory.GetChannel(primary.IpAddress.ToString(), _shardPort, context.LocalAddress.IpAddress.ToString(), SessionTypes.Shard, _traceProvider, _channelFormatter);
                try
                {
                    isConnected = channel.Connect(false);
                }
                catch (ChannelException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("Error: RemoteShard.OnPrimaryChanged()", e.ToString());
                }
                //catch (Exception ex)
                //{
                //    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                //        LoggerManager.Instance.ShardLogger.Error("Error: RemoteShard.OnPrimaryChanged()", e.ToString());
                //}

                if (isConnected)
                {
                    SessionInfo info = new SessionInfo();
                    info.Cluster = this.Context.ClusterName;
                    info.Shard = this.Context.LocalShardName;

                    channel.SendMessage(info, true);

                    lock (_onChannel)
                    {
                        _remoteShardChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel as IDualChannel, ConnectInfo.ConnectStatus.CONNECT_FIRST_TIME), _remoteShardChannel);
                        ((IDualChannel)_remoteShardChannel).StartReceiverThread();
                        ((IDualChannel)_remoteShardChannel).RegisterRequestHandler(this);
                        shardConnected = true;
                    }
                    lock (_onPrimary)
                    {
                        _primary = new Server(new Address(primary.IpAddress.ToString(), primary.Port), Status.Running);
                    }
                }
            }

            if (shardConnected)
                ShardConnected();

        }

        public void OnPrimaryChanged(ServerInfo newPrimary, int port)
        {
            //RTD: review
            if ((newPrimary == null && _primary != null) ||
                (newPrimary != null && _primary != null && !this._primary.Address.Equals(newPrimary.Address)))
            {
                BrokenConnectionInfo info = new BrokenConnectionInfo();
                info.BrokenAddress = _primary.Address;
                info.SessionType = SessionTypes.Shard;
                _connectionRestoration.UnregisterListener(info);
            }
            
            Address primaryAddress=null;
            if (newPrimary != null && newPrimary.Address != null)
            { 
                primaryAddress=new Address(newPrimary.Address.IpAddress,port);
            }

            ConnectPrimary(primaryAddress);
                
            //// If primary is null, it means the respective shard has no primary anymore so
            //if (newPrimary == null)
            //{
            //    if (_remoteShardChannel != null)
            //    {
            //        ((DualChannel)_remoteShardChannel).ShouldTryReconnecting = false;
            //        _remoteShardChannel.Disconnect();
            //        _remoteShardChannel = null;
            //    }
            //}
            //else
            //{
            //    if (_remoteShardChannel != null)
            //    {
            //        if (_remoteShardChannel.PeerAddress.Equals(newPrimary) && ((IDualChannel)this._remoteShardChannel).Connected)
            //            return;
            //    }


            //    bool isConnected = false;
            //    IRequestResponseChannel channel = factory.GetChannel(newPrimary.Address.IpAddress.ToString(), _shardPort, context.LocalAddress.IpAddress.ToString(), SessionTypes.Shard, _traceProvider, _channelFormatter);
            //    try
            //    {
            //        isConnected = channel.Connect(false);
            //    }
            //    catch (ChannelException e)
            //    {
            //        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
            //            LoggerManager.Instance.ShardLogger.Error("Error: RemoteShard.OnPrimaryChanged()", e.ToString());
            //    }
            //    //catch (Exception ex)
            //    //{
            //    //    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
            //    //        LoggerManager.Instance.ShardLogger.Error("Error: RemoteShard.OnPrimaryChanged()", e.ToString());
            //    //}

            //    if (isConnected)
            //    {
            //        SessionInfo info = new SessionInfo();
            //        info.Cluster = this.Context.ClusterName;
            //        info.Shard = this.Context.LocalShardName;

            //        channel.SendMessage(info, true);

            //        lock (_onChannel)
            //        {
            //            _remoteShardChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel as IDualChannel, ConnectInfo.ConnectStatus.CONNECT_FIRST_TIME), _remoteShardChannel);
            //            ((IDualChannel)_remoteShardChannel).StartReceiverThread();
            //            ((IDualChannel)_remoteShardChannel).RegisterRequestHandler(this);
            //        }
            //        lock (_onPrimary)
            //        {
            //            _primary = new Server(new Address(newPrimary.Address.IpAddress.ToString(), port), Status.Running);
            //        }
            //    }
            //}

        }

        public object SendUnicastMessage(Server destination, Object message)
        {
            if (this._remoteShardChannel != null)
            {
                return this._remoteShardChannel.SendMessage(message, false);
            }

            throw new RemoteShardException("Remote shard is not connected");
        }

        public object SendBroadcastMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public object SendMulticastMessage(List<Server> destinations, Message message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// RegisterQuery Remote Shard Listener, for now its cluster manager and it will be the only listener for
        /// indiviual remote shard so no need for having list of listener
        /// </summary>
        /// <param name="name"> name of listener</param>
        /// <param name="shardListener"> listener instance</param>
        public void RegisterShardListener(string name, IShardListener shardListener)
        {
            _shardListener = shardListener;
        }


        /// <summary>
        /// 
        /// UnRegister Remote Shard Listener, for now its cluster manager and it will be the only listener for
        /// indiviual remote shard so no need for having list of listener
        /// </summary>
        /// <param name="name"> name of listener</param>
        /// <param name="shardListener"> listener instance</param>

        public void UnregisterShardListener(String name, IShardListener shardListener)
        {
            _shardListener = null;
        }

        public void Dispose()
        {
            _shardListener = null;
            _remoteShardChannel = null;
            //_activeChannelsList = null;
            _servers = null;
        }

        #region IRequestListener Implemenattion

        public object OnRequest(IRequest request)
        {
            Message msg = (Message)request.Message;
            //String DBName = msg.Destination;
            //MessageResponse response = new MessageResponse();
            Server source = null;

            if (request != null)
            {

                if (request.Source != null)
                {
                    //Status might be changed in future
                    source = new Server(request.Source, Status.Running);
                }
                if (_shardListener != null)
                {
                    try
                    {
                        /*response.Response = */
                        return _shardListener.OnMessageReceived(msg, source);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }
                else
                    /*response.Response =*/
                    throw new DatabaseException("Destination is NULL");
            }

            return null;
            //return response;
        }

        #endregion

        public IList<Server> ActiveChannelsList
        {
            get
            {
                IList<Server> activeChannelsList = new List<Server>();
                if (_remoteShardChannel != null)
                {
                    activeChannelsList.Add(new Server(_remoteShardChannel.PeerAddress, Status.Running));
                }
                return activeChannelsList;                
            }
        }

        public IAsyncResult BeginSendMessage(Server destination, object msg)
        {
            if (this._remoteShardChannel != null && _remoteShardChannel is IDualChannel)
            {
                return ((IDualChannel)_remoteShardChannel).BeginSendMessage(msg);
            }

            throw new Exception("No server exist in servers list");
        }

        public object EndSendMessage(Server destination, IAsyncResult result)
        {
            Common.MiscUtil.IsArgumentNull(result);

            if (this._remoteShardChannel != null && _remoteShardChannel is IDualChannel)
            {
                return ((IDualChannel)_remoteShardChannel).EndSendMessage(result);
            }

            throw new Exception("Specified server does not exist");
        }

        public ShardRequestBase<T> CreateUnicastRequest<T>(Server destination, Message message)
        {
            return new ShardUnicastRequest<T>(this, destination, message);
        }


        public ShardMulticastRequest<R, T> CreateMulticastRequest<R, T>(IList<Server> destinations, Message message) where R : IResponseCollection<T>, new()
        {
            return new ShardMulticastRequest<R, T>(this, destinations, message);
        }


        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            try
            {
                if (channel != null && ((IDualChannel)channel).ShouldTryReconnecting && _primary != null && _primary.Address.Equals(channel.PeerAddress))
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)

                        LoggerManager.Instance.ShardLogger.Info("RemoteShard.ChannelDisconnected.ShoudTryReconnecting", ((IDualChannel)channel).ShouldTryReconnecting.ToString());

                    lock (_onChannel)
                    {
                        _remoteShardChannel = null;
                    }
                    BrokenConnectionInfo info = new BrokenConnectionInfo();
                    info.BrokenAddress = channel.PeerAddress;
                    info.SessionType = SessionTypes.Shard;

                    _connectionRestoration.RegisterListener(info, this,context.LocalShardName);
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("RemoteShard.ChannelDisconnected()", e.ToString());
            }
        }


        public bool Stop()
        {
            if (LoggerManager.Instance.ServerLogger.IsDebugEnabled)
                LoggerManager.Instance.ServerLogger.Debug("RemoteShard.Stop", "Stopping Remote Shard '" + Name + "'");

            if (this._remoteShardChannel != null)
            {
                ((IDualChannel)_remoteShardChannel).ShouldTryReconnecting = false;
                _remoteShardChannel.Disconnect();
            }
            if (_connectionRestoration != null)
                _connectionRestoration.Stop();
            _connectionRestoration = null;
            IsStarted = false;
            return true;
        }

        /// <summary>
        /// Removes the broken connection of the shard from the restoration manager.
        /// </summary>
        public void RemoveBrokenConnection()
        {
            if (_primary != null)
            {
                BrokenConnectionInfo info = new BrokenConnectionInfo
                {
                    BrokenAddress = _primary.Address,
                    SessionType = SessionTypes.Shard
                };

                _connectionRestoration.UnregisterListener(info);
            }
        }


        public NodeRole NodeRole
        {
            //to-do: this needs to be set as per the need
            get { return NodeRole.None; }
        }

        #region IConnectionRestorationListener Implementation
        public void OnConnectionRestoration(IDualChannel channel)
        {
            // check if the shardConnected event is required to be raised
            bool shardConnected = false;

            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
            {
                LoggerManager.Instance.ShardLogger.Debug("RemoteShard.OnConnectionRestoration()", "Before Connection Restoration for " + channel != null ? channel.PeerAddress.ToString() : "channel is null");
                LoggerManager.Instance.ShardLogger.Debug("RemoteShard.OnConnectionRestoration()", "Before Connection Restoration Should Retry " + channel != null ? channel.ShouldTryReconnecting.ToString() : "channel is null");
                LoggerManager.Instance.ShardLogger.Debug("RemoteShard.OnConnectionRestoration()", "Before Connection Restoration primary is  " + _remoteShardChannel != null ? _remoteShardChannel.PeerAddress.ToString() : "priamry is null");
            }

            SessionInfo info = new SessionInfo();
            info.Cluster = this.Context.ClusterName;
            info.Shard = this.Context.LocalShardName;
            ((IRequestResponseChannel)channel).SendMessage(info, true);

            lock (_onChannel)
            {
                _remoteShardChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel, ConnectInfo.ConnectStatus.RECONNECTING), _remoteShardChannel);
                ((IDualChannel)_remoteShardChannel).StartReceiverThread();
                ((IDualChannel)_remoteShardChannel).RegisterRequestHandler(this);
                shardConnected = true;
            }
            lock (_onPrimary)
            {
                _primary = new Server(channel.PeerAddress, Status.Running);
            }

            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
            {
                LoggerManager.Instance.ShardLogger.Debug("RemoteShard.OnConnectionRestoration()", "After Connection Restoration for " + channel != null ? channel.PeerAddress.ToString() : "channel is null");
                LoggerManager.Instance.ShardLogger.Debug("RemoteShard.OnConnectionRestoration()", "After Connection Restoration Should Retry " + channel != null ? channel.ShouldTryReconnecting.ToString() : "channel is null");
            }

            if (shardConnected)
                ShardConnected();

        }

        #endregion

        private void ShardConnected()
        {
            DatabaseMessage msg = new DatabaseMessage();
            msg.MessageType = MessageType.DBOperation;
           
           

            if (_threadPool != null)
            {
                _threadPool.ExecuteTask(new ShardEventDeliverTask(msg, _shardListener));
            }
        }

        class ShardEventDeliverTask : IThreadPoolTask
        {
            Message _message;
            
            IShardListener _shardListener;

            public ShardEventDeliverTask(Message message, IShardListener shardlistener)
            {
                _message = message;
                _shardListener = shardlistener;
            }

            public void Execute()
            {
                try
                {
                    if (_shardListener != null)
                    {
                        _shardListener.OnMessageReceived(_message, null);
                    }
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Error("ShardEventDeliverTask", ex.Message);
                    }
                }
            }
        }
    }
}
