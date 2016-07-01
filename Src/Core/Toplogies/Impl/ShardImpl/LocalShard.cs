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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Storage.Providers;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;
using Alachisoft.NosDB.Serialization;
using Alachisoft.NosDB.Core.Configuration.Services;
using System.Net;
using Alachisoft.NosDB.Common.Communication.Formatters;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks;
using Alachisoft.NosDB.Common.Logger;
using System.Threading;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Common;
using System.Diagnostics;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.ErrorHandling;

namespace Alachisoft.NosDB.Core.Toplogies.Impl
{
    public class LocalShard : IShard, IRequestListener, IConnectionRestorationListener
    {
        private delegate Object MessageDelegate(Server destination, Message message);

        private Object _mutex = new Object();

        private IList<Server> _shardPeersList = null;
        private IDictionary<Server, IDualChannel> _shardChannels = null;
        private IDictionary<String, IShardListener> _shardListeners = null;
        private ITraceProvider _traceProvider = null;
        private NodeContext context = null;
        private IChannelFormatter _channelFormatter = new ShardChannelFormatter();
        private MembershipManager _membershipManager = null;

        private ResolveChannelDispute _resolveDispute = null;
        private ClusterConfigurationManager _clusterConfigMgr = null;
        private IConnectionRestoration _connectionRestoration = null;
        private int _connIdGenerator = 0;
        private Object _mutexOnnodeRole = new Object();

        public IChannelFormatter ChannelFormatter
        {
            get { return _channelFormatter; }
        }

        public string Name
        {
            get { return context.LocalShardName; }
            set { context.LocalShardName = value; }
        }

        public LocalShard(NodeContext context, IConnectionRestoration connectionRestoration, ClusterConfigurationManager clusterConfigMgr)
        {
            this.context = context;
            this._connectionRestoration = connectionRestoration;
            this._clusterConfigMgr = clusterConfigMgr;
        }

        public Server Primary
        {
            private set;
            get;
        }

        public IList<Server> Servers
        {
            get { return _shardPeersList; }
        }

        public NodeRole NodeRole
        {
            set;
            get;
        }

        public IList<Server> ActiveChannelsList
        {
            get
            {
                IList<Server> activeChannelsList = new List<Server>();
                if (_shardChannels != null)
                {
                    IList<Server> activeKeys = _shardChannels.Keys.ToList();
                    foreach (var server in activeKeys)
                    {
                        activeChannelsList.Add(server);
                    }
                }
                return activeChannelsList;
            }

        }

        public IDictionary<Server, IDualChannel> ShardChannels
        {
            get { return _shardChannels; }
            set { _shardChannels = value; }
        }

        public bool Initialize(ShardConfiguration configuration)
        {
            if (configuration == null) return false;

            this.Name = configuration.Name;
            IPAddress localIP = GetLocalAddress();

            if (context != null)
                context.LocalAddress = new Common.Net.Address(localIP, configuration.Port);
            _membershipManager = new MembershipManager(this, context, _clusterConfigMgr);

          
            lock (_mutexOnnodeRole)
            {
                if (NodeRole != NodeRole.Intermediate)
                    NodeRole = NodeRole.None;
            }

            this._shardPeersList = GetPeersList(configuration.Servers.Nodes.Values.ToArray(), configuration.Port);
            //an old copy of the membership is required at the time when the node joins for comparison purposes.
            //_oldMembership = context.ConfigurationSession.GetMembershipInfo(context.ClusterName, context.LocalShardName);
            //this._shardPeersList = GetRunningPeersList(configuration.Port);
            //this._shardPeersList = GetRunningPeersList(_oldMembership.Servers, configuration.Port);
            this._traceProvider = new TraceProvider();


            _resolveDispute = new ResolveChannelDispute(context, this);


            return true;
        }

       private IList<Server> GetRunningPeersList(IList<ServerNode> serverNodes, int port)
        {
            IList<Server> serverList = new List<Server>();
            if (serverNodes != null)
            {
                foreach (ServerNode server in serverNodes)
                {
                    serverList.Add(new Server(new Common.Net.Address(server.Name, port), Status.Running));
                }
                return serverList;
            }
            return null;
        }


        public bool Start()
        {

            if (_shardPeersList == null || _shardPeersList.Count == 0) return false;

            if (this._shardChannels == null)
                this._shardChannels = new Dictionary<Server, IDualChannel>();

            try
            {
                //forming connections with all the nodes.
                if (_shardPeersList != null && _shardPeersList.Count != 0)
                {
                    foreach (Server server in this._shardPeersList)
                    {
                        FormChannelConnection(server);
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        {
                            LoggerManager.Instance.ShardLogger.Info("LocalShard.Start()", "forming connections with the node:" + server.Address);
                        }
                    }
                }

                _membershipManager.BeginHeartbeatTasks(context, this, _connectionRestoration);


                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Info("LocalShard.Start()", "Local shard: " + Name + " started successfully on " + context.LocalAddress + ".");
                }

            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("LocalShard.Start()", "Local shard: " + Name + " cannot started successfully : ", ex);
                }
            }



            return true;
        }

        private void FormChannelConnection(Server server)
        {
            if (server.Address.Equals(context.LocalAddress))
            {
                try
                {
                    IDualChannel channel = new LocalChannel(context.LocalAddress, this);
                    lock (_shardChannels)
                    {
                        _shardChannels[server] = channel;
                    }
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Error("LocalShard.FormChannel()", "Local node: " + e.ToString());
                    }
                }
            }
            else
            {
                DualChannel channel = new DualChannel(server.Address.IpAddress.ToString(), server.Address.Port, context.LocalAddress.IpAddress.ToString(), SessionTypes.Shard, _traceProvider, _channelFormatter);
                try
                {
                    if (channel.Connect(false))
                    {

                        SessionInfo sessionInfo = new SessionInfo();
                        sessionInfo.Cluster = this.context.ClusterName;
                        sessionInfo.Shard = this.context.LocalShardName;

                        channel.SendMessage(sessionInfo, true);

                        IDualChannel acceptedChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel, ConnectInfo.ConnectStatus.CONNECT_FIRST_TIME), _shardChannels);
                        lock (_shardChannels)
                        {
                            _shardChannels[server] = acceptedChannel;
                        }

                        _shardChannels[server].RegisterRequestHandler(this);
                        _shardChannels[server].StartReceiverThread();

                    }
                }
                catch (ChannelException ex)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    {
                        LoggerManager.Instance.ShardLogger.Error("LocalShard.Start()", "Local shard: " + Name + ": Connection with " + server.Address + " failed to establish. " + ex);
                    }
                }
            }

        }

        public object SendUnicastMessage(Server destination, Object message)
        {
            if (destination == null) throw new ClusterException(ErrorCodes.Cluster.DESTINATION_NULL);

            if (this._shardChannels != null && _shardChannels.ContainsKey(destination))
            {
                IDualChannel channel = (IDualChannel)_shardChannels[destination];

                return channel.SendMessage(message, false);
            }

            throw new ClusterException(ErrorCodes.Cluster.DESTINATION_SERVER_NOT_EXIST, new string[] { destination.Address.ToString() });
        }

        public object SendBroadcastMessage(Message message)
        {
            if (this._shardChannels != null && _shardChannels.Count > 0)
            {
                IDictionary responses = new HashVector<Server, Object>();
                IDictionary delegates = new HashVector<Server, MessageDelegate>();
                IDictionary results = new HashVector<Server, IAsyncResult>();

                IEnumerator channelsEnumerator = _shardChannels.GetEnumerator();
                while (channelsEnumerator.MoveNext())
                {
                    KeyValuePair<Server, IDualChannel> pair = (KeyValuePair<Server, IDualChannel>)channelsEnumerator.Current;

                    MessageDelegate msgDelegate = new MessageDelegate(SendUnicastMessage);
                    delegates.Add(pair.Key, msgDelegate);
                    IAsyncResult ar = msgDelegate.BeginInvoke(pair.Key, message, null, null);
                    results.Add(pair.Key, ar);
                }



                IEnumerator delegatesEnumerator = results.GetEnumerator();
                while (delegatesEnumerator.MoveNext())
                {
                    DictionaryEntry pair = (DictionaryEntry)delegatesEnumerator.Current;

                    Object ret = ((MessageDelegate)delegates[pair.Key]).EndInvoke(((IAsyncResult)pair.Value));
                    if (ret != null)
                    {
                        responses.Add(pair.Key, ret);
                    }
                }

                return responses;
            }

            throw new ClusterException(ErrorCodes.Cluster.SERVER_NOT_EXIST);
        }

        public object SendMulticastMessage(List<Server> destinations, Message message)
        {
            if (this._shardChannels != null && _shardChannels.Count > 0)
            {
                IDictionary responses = new HashVector<Server, object>();
                IDictionary delegates = new HashVector<Server, MessageDelegate>();
                IDictionary results = new HashVector<Server, IAsyncResult>();

                IEnumerator channelsEnumerator = destinations.GetEnumerator();
                while (channelsEnumerator.MoveNext())
                {
                    Server destination = (Server)channelsEnumerator.Current;
                    if (this._shardChannels[destination] != null)
                    {
                        MessageDelegate msgDelegate = new MessageDelegate(SendUnicastMessage);
                        delegates.Add(destination, msgDelegate);

                        IAsyncResult ar = msgDelegate.BeginInvoke(destination, message, null, null);
                        results.Add(destination, ar);
                    }
                }


                IEnumerator delegatesEnumerator = results.GetEnumerator();
                while (delegatesEnumerator.MoveNext())
                {
                    DictionaryEntry pair = (DictionaryEntry)delegatesEnumerator.Current;

                    Object ret = ((MessageDelegate)delegates[pair.Key]).EndInvoke((IAsyncResult)results[pair.Key]);
                    if (ret != null)
                    {
                        responses.Add(pair.Key, ret);
                    }
                }

                return responses;
            }

            throw new ClusterException(ErrorCodes.Cluster.SERVER_NOT_EXIST);
        }

        public void RegisterShardListener(String name, IShardListener shardListener)
        {
            if (_shardListeners == null)
                _shardListeners = new Dictionary<String, IShardListener>();

            _shardListeners.Add(name, shardListener);
        }

        public void UnregisterShardListener(String listenerDB, IShardListener listener)
        {
            if (_shardListeners != null)
                _shardListeners.Remove(listenerDB);
        }

        /// <summary>
        /// Removes the broken connection of the shard from the restoration manager.
        /// </summary>
        public void RemoveBrokenConnection()
        {

        }

        public void Dispose()
        {
            if (_membershipManager != null)
                _membershipManager.Dispose();
           


            if (_shardChannels != null)
            {
                lock (_shardChannels)
                {
                    foreach (KeyValuePair<Server, IDualChannel> pair in _shardChannels)
                    {
                        try
                        {
                            ((IDualChannel)pair.Value).Disconnect();
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("Localshard.Dispose(): ", "Connection of local node " + context.LocalAddress.ToString() + " disconected from node " + ((IDualChannel)pair.Value).PeerAddress.ToString());
                        }
                        catch (Exception ex)
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            {
                                LoggerManager.Instance.ShardLogger.Error("Local Shard Dispose()", "Error:", ex);
                            }
                        }
                    }
                    //_shardChannels = null;
                }
            }

            lock (_mutexOnnodeRole)
            {
                if (NodeRole != NodeRole.Intermediate)
                    NodeRole = Common.Configuration.Services.NodeRole.None;
            }

        }

        private IList<Server> GetPeersList(ServerNode[] serverIPs, int port)
        {
            IList<Server> serverList = new List<Server>();

            foreach (ServerNode server in serverIPs)
                // if (!server.Name.Equals(this.context.LocalAddress.IpAddress.ToString()))
                serverList.Add(new Server(new Common.Net.Address(server.Name, port), Status.Initializing));

            return serverList;
        }

        private IPAddress GetLocalAddress()
        {

            #region Getting Local Address Logic; might be replace with getting address from service config

            IPAddress localAddress = null;

            string localIP = ConfigurationSettings.AppSettings["ManagementServerIP"];
            try
            {
                localAddress = System.Net.IPAddress.Parse(localIP);
            }
            catch (Exception ex)
            {

                System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());


                if (hostEntry.AddressList != null)
                {
                    foreach (System.Net.IPAddress addr in hostEntry.AddressList)
                    {
                        if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            localAddress = addr;
                            break;
                        }
                    }
                }
            }
            return localAddress;
            #endregion

        }

        public bool OnMembershipChanged(MembershipChangeArgs args)
        {

            if (args != null)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                    LoggerManager.Instance.ShardLogger.Debug("LocalShard.OnMembershipChanged()", "Membership change type: " + args.ChangeType);

                switch (args.ChangeType)
                {
                    case MembershipChangeArgs.MembershipChangeType.PrimarySet:
                    case MembershipChangeArgs.MembershipChangeType.PrimarySelected:
                        if (args.ServerName != null)
                        {
                            if (args.ServerName.Equals(context.LocalAddress))
                                lock (_mutexOnnodeRole)
                                {
                                    NodeRole = Common.Configuration.Services.NodeRole.Primary;
                                }
                            else
                                lock (_mutexOnnodeRole)
                                {
                                    if (NodeRole != NodeRole.Intermediate)
                                        NodeRole = Common.Configuration.Services.NodeRole.Secondary;
                                }
                            Primary = new Server(args.ServerName, Status.Running);

                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled && args.ElectionId != null)
                                LoggerManager.Instance.ShardLogger.Info("LocalShard.OnMembershipChanged()", "This term's election id is: " + args.ElectionId.Id);


                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                            {
                                if (args.ChangeType.Equals(MembershipChangeArgs.MembershipChangeType.PrimarySet))
                                    LoggerManager.Instance.ShardLogger.Info("LocalShard.OnMembershipChanged()", "Node " + args.ServerName.IpAddress.ToString() + " set as the primary node for the shard.");
                                else if (args.ChangeType.Equals(MembershipChangeArgs.MembershipChangeType.PrimarySelected))
                                    LoggerManager.Instance.ShardLogger.Info("LocalShard.OnMembershipChanged()", "Node " + args.ServerName.IpAddress.ToString() + " selected as the primary node for the shard.");
                            }

                            AppUtil.LogEvent(AppUtil.EventLogSource, string.Format("Node {0} is selected as primary for shard \"{1}\"", args.ServerName.ToString(), context.LocalShardName),
                                EventLogEntryType.Information, EventCategories.Information, EventID.PrimaySelected);
                        }
                        break;
                    case MembershipChangeArgs.MembershipChangeType.PrimaryLost:
                    case MembershipChangeArgs.MembershipChangeType.PrimaryDemoted:
                    case MembershipChangeArgs.MembershipChangeType.NodeLeft:

                        if (args.ServerName != null && Primary != null && args.ServerName.Equals(Primary.Address))
                        {
                            lock (_mutexOnnodeRole)
                            {
                                if (NodeRole != NodeRole.Intermediate)
                                    NodeRole = Common.Configuration.Services.NodeRole.None;
                            }
                            Primary = null;

                            if (args.ServerName != null)
                            {
                                if (args.ChangeType.Equals(MembershipChangeArgs.MembershipChangeType.PrimaryDemoted))
                                {
                                    AppUtil.LogEvent(AppUtil.EventLogSource, string.Format("Primary Node {0} is demoted for shard \"{1}\"", args.ServerName.ToString(), context.LocalShardName),
                                       EventLogEntryType.Warning, EventCategories.Warning, EventID.PrimaryLost);

                                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                        LoggerManager.Instance.ShardLogger.Warn("LocalShard.OnMembershipChanged()", "The primary " + args.ServerName.ToString() + " is demoted.");
                                }

                                else if (args.ChangeType.Equals(MembershipChangeArgs.MembershipChangeType.PrimaryLost))
                                {
                                    AppUtil.LogEvent(AppUtil.EventLogSource, string.Format("Connection with the primary node {0} lost \"{1}\"", args.ServerName.ToString(), context.LocalShardName),
                                        EventLogEntryType.Warning, EventCategories.Warning, EventID.PrimaryLost);
                                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                        LoggerManager.Instance.ShardLogger.Warn("LocalShard.OnMembershipChanged()", "The primary " + args.ServerName.ToString() + " is lost.");
                                }

                            }
                        }

                        _clusterConfigMgr.UpdateClusterConfiguration();
                        if (args.ServerName != null)
                        {
                            ShardConfiguration sConfig = null;
                            if (_clusterConfigMgr != null && _clusterConfigMgr.LatestConfiguration != null && _clusterConfigMgr.LatestConfiguration.Deployment != null)
                                sConfig = _clusterConfigMgr.LatestConfiguration.Deployment.GetShardConfiguration(context.LocalShardName);
                            {
                                ServerNode node = null;
                                if (sConfig != null && sConfig.Servers != null)
                                    node = sConfig.Servers.GetServerNode(args.ServerName.IpAddress.ToString());
                                if (node == null)
                                {
                                    if (_connectionRestoration != null)
                                    {
                                        BrokenConnectionInfo info = new BrokenConnectionInfo();
                                        info.BrokenAddress = args.ServerName;
                                        info.SessionType = SessionTypes.Shard;
                                        _connectionRestoration.UnregisterListener(info);
                                    }
                                }
                            }

                        }
                        break;
                    case MembershipChangeArgs.MembershipChangeType.TimeoutOnRestrictedPrimary:
                        return _membershipManager.AbortTakeoverMechanismTask(args);
                    case MembershipChangeArgs.MembershipChangeType.ForcefullyDemotePrimary:
                        return _membershipManager.OnForcefulPrimaryDemotion(args);
                }

                if (_membershipManager != null)
                {
                    _membershipManager.UpdateLocalMembership(args);
                    if (context != null && context.DatabasesManager != null &&
                        (args.ChangeType == MembershipChangeArgs.MembershipChangeType.PrimarySet ||
                         (args.ChangeType == MembershipChangeArgs.MembershipChangeType.PrimarySelected)))
                    {
                        context.DatabasesManager.ElectionResult = _membershipManager.LatestMembership.ElectionId;
                        context.ElectionResult = new ElectionResult();
                        context.ElectionResult.ElectionId = _membershipManager.LatestMembership.ElectionId;

                    }

                    DatabaseMessage primaryChangedMessage = new DatabaseMessage();
                    primaryChangedMessage.OpCode = OpCode.PrimaryChanged;
                    IShardListener listener = _shardListeners[Common.MiscUtil.CLUSTER_MANAGER];
                    listener.OnMessageReceived(primaryChangedMessage, new Server(context.LocalAddress, Status.Running));
                }

            }
            return false;
        }

        internal void OnConfigurationChanged(ConfigChangeEventArgs arguments)
        {
            if (arguments != null && _clusterConfigMgr != null)
            {
                _clusterConfigMgr.UpdateClusterConfiguration();
                Address affectedNode = null;
                if (arguments.EventParameters != null && arguments.EventParameters.ContainsKey(EventParamName.NodeAddress))
                    affectedNode = arguments.GetParamValue<Address>(EventParamName.NodeAddress);

                ChangeType type = arguments.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType);

                if ((type != ChangeType.NodeLeft && type != ChangeType.PrimaryGone) && affectedNode == null)
                    return;

                if (_membershipManager != null)
                {
                    switch (type)
                    {
                        //RTD: look for a better way to handle this scenario.
                        case ChangeType.NodeAdded:
                            _membershipManager.OnMemberJoined(new Server(affectedNode, Status.Running));
                            break;
                        case ChangeType.NodeRemoved:
                            _membershipManager.OnMemberLeft(new Server(affectedNode, Status.Running));
                            break;
                        case ChangeType.PriorityChanged:
                            _membershipManager.SanityCheckForTakeoverElect();
                            break;
                        case ChangeType.NodeLeft:
                        //this is the event received by the CS when the leaving node is a primary. Tada-- Why 
                        case ChangeType.PrimaryGone:
                            Membership membership = null;
                            if (arguments.EventParameters.ContainsKey(EventParamName.Membership))
                                membership = arguments.GetParamValue<Membership>(EventParamName.Membership);

                            if (membership != null && membership.Servers != null)
                            {
                                ShardConfiguration sConfig = _clusterConfigMgr.GetShardConfiguration(context.LocalShardName);
                                if (sConfig != null && sConfig.Servers != null)
                                {
                                    foreach (KeyValuePair<string, ServerNode> node in sConfig.Servers.Nodes)
                                    {
                                        if (!membership.Servers.Contains(node.Value))
                                        {
                                            affectedNode = new Address(node.Value.Name, sConfig.Port);
                                            break;
                                        }
                                    }
                                }
                                if (affectedNode != null && _shardChannels != null && !_shardChannels.ContainsKey(new Server(affectedNode, Status.Running)))
                                {
                                    _membershipManager.OnActivityTriggered(Activity.NodeLeaving, affectedNode);
                                    _membershipManager.OnMemberLeft(new Server(affectedNode, Status.Running));
                                    _membershipManager.OnActivityComplete();
                                }
                            }

                            break;
                    }
                }
            }
        }

        public Boolean OnSessionEstablished(Session session)
        {
            Server server = new Server(new Common.Net.Address(session.IP.ToString(), session.LocalPort), Status.Initializing);
            IDualChannel channel = new DualChannel(session.Connection, session.IP.ToString(), this.context.LocalAddress.Port, context.LocalAddress.ToString(), SessionTypes.Shard, _traceProvider, _channelFormatter);
            bool isConnected = false;
            try
            {
                isConnected = channel.Connect(false);

            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: Localshard.OnSessionEstd()", e.ToString());
            }
            if (isConnected)
            {
                ConnectInfo.ConnectStatus status = ConnectInfo.ConnectStatus.CONNECT_FIRST_TIME;
                if (_shardChannels.ContainsKey(server))
                    status = ConnectInfo.ConnectStatus.RECONNECTING;

                IDualChannel acceptedChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel, status), _shardChannels);

                lock (_shardChannels)
                {
                    _shardChannels[server] = acceptedChannel;
                }

                _shardChannels[server].RegisterRequestHandler(this);
                _shardChannels[server].StartReceiverThread();

                return true;
            }
            else
                return false;

        }



        public ShardRequestBase<T> CreateUnicastRequest<T>(Server destination, Message message)
        {
            return new ShardUnicastRequest<T>(this, destination, message);
        }


        public ShardMulticastRequest<R, T> CreateMulticastRequest<R, T>(IList<Server> destinations, Message message) where R : IResponseCollection<T>, new()
        {
            return new ShardMulticastRequest<R, T>(this, destinations, message);

        }

        public IAsyncResult BeginSendMessage(Server destination, object msg)
        {
            if (destination == null)
            {
                throw new ClusterException(ErrorCodes.Cluster.DESTINATION_NULL);
            }

            if (_shardChannels != null && _shardChannels.ContainsKey(destination))
            {
                IDualChannel channel = _shardChannels[destination];

                return channel.BeginSendMessage(msg);
            }

            throw new ClusterException(ErrorCodes.Cluster.SERVER_NOT_EXIST);
        }

        public object EndSendMessage(Server destination, IAsyncResult result)
        {
            Common.MiscUtil.IsArgumentNull(result);
            if (destination == null) throw new ClusterException(ErrorCodes.Cluster.DESTINATION_NULL);

            if (_shardChannels != null && _shardChannels.ContainsKey(destination))
            {
                IDualChannel channel = _shardChannels[destination];

                return channel.EndSendMessage(result);
            }

            throw new ClusterException(ErrorCodes.Cluster.DESTINATION_SERVER_NOT_EXIST, new string[] { destination.Address.ToString() });
        }


        private bool IsLocalShardOperation(Message msg)
        {
            MembershipChangeArgs args = msg.Payload as MembershipChangeArgs;
            if (args != null)
            {
                OnMembershipChanged(args);
                return true;
            }
            else
                return false;
        }
    



        #region IRequestListener Implemenattion

        public object OnRequest(IRequest request)
        {
            Message msg = (Message)request.Message;
            MessageType msgType = msg.MessageType;
            //MessageResponse response = new MessageResponse();
            Server source = null;

            if (request != null)
            {
                if (request.Source != null)
                {
                    source = new Server(request.Source, Status.Running);
                }

                IShardListener listener = null;

                switch (msgType)
                {
                    case MessageType.DBOperation:

                        listener = _shardListeners[Common.MiscUtil.CLUSTER_MANAGER];
                        break;
                    case MessageType.MembershipOperation:
                        bool isLSOp = IsLocalShardOperation(msg);
                        if (isLSOp)
                            return isLSOp;
                        listener = _shardListeners[Common.MiscUtil.MEMBERSHIP_MANAGER];
                        break;
                    case MessageType.Heartbeat:
                        listener = _shardListeners[Common.MiscUtil.MEMBERSHIP_MANAGER];
                        break;
                    case MessageType.Replication:
                        listener = _shardListeners[Common.MiscUtil.CONFIGURATION_MANAGER];
                        break;
                }
                if (listener == null)
                    throw new DatabaseException("No Listener for " + msgType.ToString() + " MessageType");
                return listener.OnMessageReceived(msg, source);
            }

            return null;
        }

    

        #endregion

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            try
            {
                if (_shardChannels != null)
                {
                    bool connected = false;

                    if (channel != null)
                    {
                        Server server = new Server(channel.PeerAddress, Status.Stopped);
                        Server key = null;
                        if (_shardChannels != null)
                        {
                            IList<Server> shardchannelKeys = _shardChannels.Keys.ToList();
                            if (server != null && server.Address != null && !server.Address.Equals(context.LocalAddress) && shardchannelKeys != null && shardchannelKeys.Count > 0)
                            {
                                foreach (Server node in shardchannelKeys)
                                {
                                    if (_shardChannels[node].PeerAddress != null && _shardChannels[node].PeerAddress.Equals(channel.PeerAddress))
                                    {
                                        key = node;
                                        break;
                                    }
                                }
                            }
                            IDualChannel tempChannel = channel as IDualChannel;
                            if (tempChannel != null && key != null && tempChannel.ShouldTryReconnecting)
                            {
                                lock (_shardChannels)
                                {
                                    //_shardChannels[key].Disconnect();
                                    _shardChannels.Remove(key);
                                    if (LoggerManager.Instance.ShardLogger != null &&
                                        LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                        LoggerManager.Instance.ShardLogger.Debug("Localshard.Channeldisconnected(): ",
                                            server.Address.ToString() + " removed from existing channels.");
                                }
                                if (!key.Address.Equals(context.LocalAddress))
                                {
                                    BrokenConnectionInfo info = new BrokenConnectionInfo();
                                    info.BrokenAddress = key.Address;
                                    info.SessionType = SessionTypes.Shard;

                                    _connectionRestoration.RegisterListener(info, this, context.LocalShardName);
                                }
                            }

                        }


                        try
                        {

                            if (!connected)
                            {
                                IShardListener listener = _shardListeners[Common.MiscUtil.CONFIGURATION_MANAGER];
                                listener.OnMemberLeft(new Server(channel.PeerAddress, Status.Stopped));
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }


        }


        public bool Stop()
        {
            if (_membershipManager != null)
                _membershipManager.StopHeartbeatTasks();
           
            if (_connectionRestoration != null)
                _connectionRestoration.Stop();
            _connectionRestoration = null;

            IList<Server> keys = null;
            if (_shardChannels != null)
                keys = _shardChannels.Keys.ToList();
            if (keys != null && keys.Count > 0)
            {
                foreach (var server in keys)
                {
                    try
                    {
                        IDualChannel channel = _shardChannels[server];
                        lock (_shardChannels)
                        {
                            _shardChannels.Remove(server);
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("Localshard.Stop(): ", server.Address.ToString() + " removed from existing channels.");
                        }
                        channel.ShouldTryReconnecting = false;
                        channel.Disconnect();
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("Localshard.Stop(): ", "Connection of local node " + context.LocalAddress.ToString() + " disconected from node " + channel.PeerAddress.ToString());
                    }
                    catch (ChannelException ex)
                    {
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                            LoggerManager.Instance.ShardLogger.Error("Error stopping the node " + context.LocalAddress.ToString(), ex.ToString());
                    }
                }
            }
            lock (_mutexOnnodeRole)
            {
                if (NodeRole != NodeRole.Intermediate)
                    NodeRole = Common.Configuration.Services.NodeRole.None;
            }

            return true;
        }

        public void OnConnectionRestoration(IDualChannel channel)
        {
            Server server = new Server(channel.PeerAddress, Status.Running);
            SessionInfo sessionInfo = new SessionInfo();
            sessionInfo.Cluster = this.context.ClusterName;
            sessionInfo.Shard = this.context.LocalShardName;

            channel.SendMessage(sessionInfo, true);

            IDualChannel acceptedChannel = _resolveDispute.GetValidChannel(_resolveDispute.SetConnectInfo(channel, ConnectInfo.ConnectStatus.RECONNECTING), _shardChannels);

            lock (_shardChannels)
            {
                _shardChannels[server] = acceptedChannel;
                _shardChannels[server].RegisterRequestHandler(this);
                _shardChannels[server].StartReceiverThread();
            }
        }
    }
}
