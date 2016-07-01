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
using System.Configuration;
using System.Linq;
using System.Net;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Common.Security;
using System.Threading;

namespace Alachisoft.NosDB.Distributor
{
    public class RemoteShardConnections : IShard, IRequestListener
    {
        private IDictionary<string, ClientContext> _clientContexts;
        private IDictionary<string, ClientCredential> _clientCredentials;

        private String _name;
        private int _shardPort;
        //string _bindingIp;
        private Server _primary;
        private IList<Server> _servers;
        private IRequestResponseChannel _primaryChannel;
        private ShardInfo _shardInfo;

        private readonly Dictionary<string, IRequestResponseChannel> _secondaryChannels =
            new Dictionary<string, IRequestResponseChannel>(StringComparer.InvariantCultureIgnoreCase);

        private IChannelFormatter _channelFormatter;
        private IList<Server> _activeChannelsList = null;
        private IChannelFactory _factory;
        private TraceProvider _traceProvider;
        private SessionTypes _sessionType;
        public ISessionId SessionId { set; private get; }

        private IChannelDisconnectionListener _channelDisconnectionListener;


        public Dictionary<string, IRequestResponseChannel> SecondaryChannels
        {
            get { return _secondaryChannels; }
        }

        public RemoteShardConnections(IChannelFactory factory, IChannelFormatter channelFormatter, ShardInfo shardInfo,
            TraceProvider traceProvider, SessionTypes sessionType)
        {
            _clientContexts = new Dictionary<string, ClientContext>();
            _clientCredentials = new Dictionary<string, ClientCredential>();

            _factory = factory;
            _channelFormatter = channelFormatter;
            _shardInfo = shardInfo;
            _traceProvider = traceProvider;
            if (shardInfo.Primary != null)
            {
                _primary =
                    new Server(
                        new Address(shardInfo.Primary.Address.IpAddress.ToString(), _shardInfo.Primary.Address.Port),
                        Status.Running); // If its primary it must be running
            }
            else
            {
                // TODO: Write exception to log
                //throw new Exception("At Query Distributor: No primary exists for " + shardInfo.Name);
            }
            _sessionType = sessionType;
        }

        public Server Primary
        {
            get { return _primary; }
        }

        public IList<Server> Servers
        {
            //get { return _servers; }
            get
            {
                List<Server> servers = new List<Server>();
                if (_primaryChannel != null)
                {
                    lock (_primaryChannel)
                    {

                        servers.Add(new Server(_primaryChannel.PeerAddress, Status.Running));
                    }
                }

                if (_secondaryChannels != null)
                {
                    lock (_secondaryChannels)
                    {

                        foreach (var secondary in _secondaryChannels)
                        {
                            servers.Add(new Server(secondary.Value.PeerAddress, Status.Running));
                        }

                    }
                }
                return servers;
            }
        }

        public IList<Server> AuthenticatedServers
        {
            //get { return _servers; }
            get
            {
                List<Server> servers = new List<Server>();
                if (_primaryChannel != null)
                {
                    lock (_primaryChannel)
                    {
                        if (_primaryChannel.IsAuthenticated)
                            servers.Add(new Server(_primaryChannel.PeerAddress, Status.Running));
                    }
                }

                if (_secondaryChannels != null)
                {
                    lock (_secondaryChannels)
                    {

                        foreach (var secondary in _secondaryChannels)
                        {
                            if (secondary.Value.IsAuthenticated)
                                servers.Add(new Server(secondary.Value.PeerAddress, Status.Running));
                        }

                    }
                }
                return servers;
            }
        }

        public bool Initialize(ShardConfiguration configuration)
        {
            if (configuration == null) throw new DistributorException("Remote Shard Configuration is Null");

            _name = configuration.Name;
            _shardPort = configuration.Port;
            _servers = GetShardServerList(configuration);

            return true;
        }

        private IList<Server> GetShardServerList(ShardConfiguration configuration)
        {
            if (configuration == null)
                throw new DistributorException(ErrorCodes.Distributor.SHARD_CONFIGURATION_NULL, new string[] {this.Name});

            IList<Server> serverList = new List<Server>();

            int shardPort = configuration.Port;

            if (configuration.Servers == null || configuration.Servers.Nodes == null) return serverList;
            foreach (ServerNode node in configuration.Servers.Nodes.Values)
            {
                Server server = new Server(new Address(node.Name, shardPort), Status.Stopped);
                serverList.Add(server);
            }

            return serverList;
        }

        public ShardInfo ShardInfo
        {
            get { return _shardInfo; }
        }

        public bool Start()
        {
            if (_shardInfo == null) return false;

            // _bindingIp = GetLocalAddress().ToString();

            if (_shardInfo.Primary != null)
            {
                _primaryChannel = CreateConnection(_shardInfo.Primary.Address.IpAddress.ToString(),
                    _shardInfo.Primary.Address.Port);

                //_primaryChannel.IsAuthenticated = Authenticate(_shardInfo.Primary.Address.IpAddress.ToString(), _shardInfo.Primary.Address.Port);
            }
            else
            {
                // TODO: Write exception to log
                //throw new Exception("At Query Distributor: No primary exists for " + shardInfo.Name. Shard would start in read only mode);
            }
            if (_shardInfo.RunningNodes != null)
            {
                foreach (ServerInfo serverInfo in _shardInfo.RunningNodes.Values)
                {
                    if (ShardInfo.Primary != null && serverInfo.Equals(ShardInfo.Primary)) continue;
                    IRequestResponseChannel channel = CreateConnection(serverInfo.Address.IpAddress.ToString(),
                        serverInfo.Address.Port);

                    //channel.IsAuthenticated = Authenticate(serverInfo.Address.IpAddress.ToString(), serverInfo.Address.Port);
                    lock (_secondaryChannels)
                    {
                        _secondaryChannels[ConvertToKey(serverInfo.Address)] = channel;
                    }
                }
            }
            return true;
        }

        public object SendAsync(Server destination, Object message, bool noResponse)
        {
            if (_primaryChannel != null)
            {
                try
                {
                    if (_primaryChannel.IsAuthenticated)
                        return _primaryChannel.SendMessage(message, noResponse);
                    else
                    {
                        throw new SecurityException(ErrorCodes.Security.UNAUTHENTIC_DB_SERVER_CONNECTION,
                            new string[] {this.Name, _primaryChannel.PeerAddress.IpAddress.ToString()});
                    }
                }
                catch (System.TimeoutException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] { _primaryChannel.PeerAddress.ToString() });
                }
                catch (Common.Exceptions.TimeoutException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] { _primaryChannel.PeerAddress.ToString() });
                }
                catch (ChannelException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING,
                        new string[] { _primaryChannel.PeerAddress.ToString() });
                }
            }

            throw new DistributorException(ErrorCodes.Distributor.NO_CHANNEL_EXISTS,
                new string[] { destination.Address.ToString(), this.Name });
        }

        public object SendUnicastMessage(Server destination, Object message)
        {
            if (_primaryChannel != null)
            {
                try
                {
                    if (_primaryChannel.IsAuthenticated)
                        return _primaryChannel.SendMessage(message, false);
                    else
                    {
                        throw new SecurityException(ErrorCodes.Security.UNAUTHENTIC_DB_SERVER_CONNECTION,
                            new string[] {this.Name, _primaryChannel.PeerAddress.IpAddress.ToString()});
                    }
                }
                catch (System.TimeoutException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] {_primaryChannel.PeerAddress.ToString()});
                }
                catch (Common.Exceptions.TimeoutException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] {_primaryChannel.PeerAddress.ToString()});
                }
                catch (ChannelException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING,
                        new string[] {_primaryChannel.PeerAddress.ToString()});
                }
            }

            throw new DistributorException(ErrorCodes.Distributor.NO_CHANNEL_EXISTS,
                new string[] {destination.Address.ToString(), this.Name});
        }

        public object SendBroadcastMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public object SendMulticastMessage(List<Server> destinations, Message message)
        {
            throw new NotImplementedException();
        }

        public void RegisterShardListener(string name, IShardListener shardListener)
        {
            throw new NotImplementedException();
        }

        public void UnregisterShardListener(string name, IShardListener shardListener)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_primaryChannel != null)
                _primaryChannel.Disconnect();

            lock (_secondaryChannels)
            {
                foreach (IRequestResponseChannel channel in _secondaryChannels.Values.ToList())
                {
                    if (channel != null)
                        channel.Disconnect();
                }
            }
        }


        public IList<Server> ActiveChannelsList
        {
            get { throw new NotImplementedException(); }
        }

        public bool OnSessionEstablished(Session session)
        {
            if (_primaryChannel != null)
            {
                session.Connection.Disconnect();
                return false;
            }
            IPAddress localIp = GetLocalAddress();
            _primaryChannel = _factory.GetChannel(session.Connection, session.IP.ToString(), session.RemotePort,
                localIp.ToString(), _sessionType, _traceProvider, _channelFormatter);

            if (!_primaryChannel.Connect(true))
            {
                throw new DistributorException(ErrorCodes.Distributor.CHANNEL_CONNECT_FAILED, new string[]
                {
                    _name,
                    _primaryChannel.PeerAddress.IpAddress.ToString(), _primaryChannel.PeerAddress.Port.ToString()
                });
            }

            return true;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        public IAsyncResult BeginSendMessage(Server destination, object msg)
        {
            if (destination == null)
            {
                throw new DistributorException(ErrorCodes.Distributor.DESTINATION_NULL, new string[] {_shardInfo.Name});
            }
            IRequestResponseChannel channel = null;
            try
            {
                if (_primaryChannel != null && destination.Address.Equals(_primaryChannel.PeerAddress))
                {
                    if (_primaryChannel.IsAuthenticated)
                    {
                        channel = _primaryChannel;
                        var asyncResult = (RequestManager.AsyncResult) _primaryChannel.BeginSendMessage(msg);
                        asyncResult.ChannelAddress = channel.PeerAddress;
                        return asyncResult;
                    }
                    else
                        throw new SecurityException(ErrorCodes.Security.UNAUTHENTIC_DB_SERVER_CONNECTION,
                            new string[] {this.Name, _primaryChannel.PeerAddress.IpAddress.ToString()});
                }

                //Otherwise send to other channel
                string address = ConvertToKey(destination.Address);
                if (_secondaryChannels.ContainsKey(address))
                {
                    channel = _secondaryChannels[address];
                    if (channel.IsAuthenticated)
                    {
                        var asyncResult = (RequestManager.AsyncResult) channel.BeginSendMessage(msg);
                        asyncResult.ChannelAddress = channel.PeerAddress;
                        return asyncResult;
                    }
                    else
                        throw new SecurityException(ErrorCodes.Security.UNAUTHENTIC_DB_SERVER_CONNECTION,
                            new string[] {this.Name, _primaryChannel.PeerAddress.IpAddress.ToString()});
                }
            }
            catch (System.TimeoutException)
            {
                if (channel != null)
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] {channel.PeerAddress.ToString()});
            }
            catch (Common.Exceptions.TimeoutException)
            {
                if (channel != null)
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] {channel.PeerAddress.ToString()});
            }
            catch (ChannelException)
            {
                if (channel != null)
                    throw new DistributorException(ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING,
                        new string[] {channel.PeerAddress.ToString()});
            }
            throw new DistributorException(ErrorCodes.Distributor.NO_CHANNEL_EXISTS,
                new string[] {destination.Address.ToString(), this.ShardInfo.Name});
        }

        public object EndSendMessage(Server destination, IAsyncResult result)
        {
            if (destination == null)
            {
                throw new DistributorException(ErrorCodes.Distributor.DESTINATION_NULL, new string[] {_shardInfo.Name});
            }
            Common.MiscUtil.IsArgumentNull(result);
            IRequestResponseChannel channel = null;
            try
            {
                if (_primaryChannel != null && destination.Address.Equals(_primaryChannel.PeerAddress))
                {
                    channel = _primaryChannel;
                    return _primaryChannel.EndSendMessage(result);
                }

                //Otherwise send to other channel
                string address = ConvertToKey(destination.Address);
                if (_secondaryChannels.ContainsKey(address))
                {
                    channel = _secondaryChannels[address];
                    return channel.EndSendMessage(result);
                }
            }
            catch (System.TimeoutException)
            {
                if (channel != null)
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] {channel.PeerAddress.ToString()});
            }
            catch (Common.Exceptions.TimeoutException)
            {
                if (channel != null)
                    throw new DistributorException(ErrorCodes.Distributor.TIMEOUT,
                        new string[] {channel.PeerAddress.ToString()});
            }
            catch (ChannelException)
            {
                if (channel != null)
                    throw new DistributorException(ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING,
                        new string[] {channel.PeerAddress.ToString()});
            }
            throw new DistributorException(ErrorCodes.Distributor.NO_CHANNEL_EXISTS,
                new string[] {destination.Address.ToString(), this.ShardInfo.Name});
        }

        public ShardRequestBase<T> CreateUnicastRequest<T>(Server destination, Message message)
        {
            throw new NotImplementedException();
        }

        public ShardMulticastRequest<R, T> CreateMulticastRequest<R, T>(IList<Server> destinations, Message message)
            where R : IResponseCollection<T>, new()
        {
            throw new NotImplementedException();
        }

        private string ConvertToKey(Address address)
        {
            return address.IpAddress + ":" + address.Port;
        }

        private IPAddress GetLocalAddress()
        {

            #region Getting Local Address Logic; might be replace with getting address from service config

            IPAddress localAddress = null;

            string localIP = ConfigurationSettings.AppSettings["LocalIP"];
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

        /// <summary>
        /// Removes the broken connection of the shard from the restoration manager.
        /// </summary>
        public void RemoveBrokenConnection()
        {

        }


        public bool Stop()
        {
            if (_primaryChannel != null)
                _primaryChannel.Disconnect();
            return true;
        }

        public Server GetSecondaryRandomly()
        {
            IRequestResponseChannel channel;
            KeyValuePair<string, IRequestResponseChannel> randomEntry;
            lock (_secondaryChannels)
            {
                if (_secondaryChannels.Count > 0)
                {
                    var rnd = new Random();
                    randomEntry = _secondaryChannels.ElementAt(rnd.Next(0, _secondaryChannels.Count));
                    return new Server(randomEntry.Value.PeerAddress, Status.Running);
                }
                else
                {
                    throw new DistributorException(ErrorCodes.Distributor.NO_SECONDARY_NODE, new string[] {Name});
                }
            }
        }

        #region Update Remote Shard Connections Methods (Events Handling)

        public IList<Server> UpdateConnections(ShardInfo shardInfo)
        {
            IList<Server> serversAdded = new List<Server>();
            if (shardInfo.Primary == null)
            {
                OnPrimaryGone(shardInfo);
            }
            else if (_primary == null)
            {
                OnPrimarySelected(shardInfo);
                serversAdded.Add(Primary);
            }
            else
            {
                Server shardInfoPrimary = shardInfo.Primary.ToServer();
                if (!this._primary.Equals(shardInfoPrimary))
                {
                    lock (_secondaryChannels)
                    {
                        _secondaryChannels[ConvertToKey(_primary.Address)] = _primaryChannel;
                    }
                    OnPrimarySelected(shardInfo);
                    serversAdded.Add(Primary);
                }
            }

            foreach (var secondary in shardInfo.Secondaries)
            {
                string key = ConvertToKey(secondary.Address); //IP:port
                lock (_secondaryChannels)
                {
                    if (!_secondaryChannels.ContainsKey(key))
                    {
                        IRequestResponseChannel channel = CreateConnection(secondary.Address.IpAddress.ToString(),
                            secondary.Address.Port);
                        //channel.IsAuthenticated = Authenticate(_shardInfo.Primary.Address.IpAddress.ToString(), _shardInfo.Primary.Address.Port);
                        _secondaryChannels.Add(key, channel);
                        serversAdded.Add(new Server(secondary.Address, Status.Running));
                    }
                }
            }

            var secondariesRemoved =
                from existingSecondary in _secondaryChannels.Keys
                where
                    !shardInfo.Secondaries.Any(
                        newSecondary => ConvertToKey(newSecondary.Address).Equals(existingSecondary))
                select existingSecondary;

            foreach (var removedChannel in secondariesRemoved)
            {
                lock (_secondaryChannels)
                {
                    _secondaryChannels[removedChannel].Disconnect();
                    _secondaryChannels.Remove(removedChannel);
                }
            }
            _shardInfo = shardInfo;
            return serversAdded;
        }

        private void OnPrimarySelected(ShardInfo shardInfo)
        {
            _primary = shardInfo.Primary.ToServer();

            string key = ConvertToKey(shardInfo.Primary.Address); // IP:Port
            lock (_secondaryChannels)
            {
                if (_secondaryChannels.ContainsKey(key))
                {
                    _primaryChannel = _secondaryChannels[key];
                    _secondaryChannels.Remove(key);
                    return;
                }
            }
            _primaryChannel = CreateConnection(shardInfo.Primary.Address.IpAddress.ToString(),
                shardInfo.Primary.Address.Port);

            
        }

        private void OnPrimaryGone(ShardInfo shardInfo)
        {
            
            if (_primary == null) return;
            _primary = null;
            _primaryChannel.Disconnect();
            _primaryChannel = null;
            
          
        }

        #endregion

        #region Utiltiy Methods

        private IRequestResponseChannel CreateConnection(string peerIP, int peerPort)
        {
            IRequestResponseChannel channel = _factory.GetChannel(peerIP, peerPort, null, _sessionType, _traceProvider,
                _channelFormatter);
            int retries = 3;
            while (retries > 0)
            {
                if (!channel.Connect(true))
                {
                    retries--;
                    if (retries == 0)
                        throw new DistributorException(ErrorCodes.Distributor.CHANNEL_CONNECT_FAILED,
                            new string[] {_name, peerIP, peerPort.ToString()});
                }
                break;
            }
            channel.IsAuthenticated = true; //set to false after authentication command failure
            channel.RegisterRequestHandler(this);
            return channel;
        }

        #endregion

        public NodeRole NodeRole
        {
            //to-do: this needs to be set as per the need
            get { return NodeRole.None; }
        }


        public IChannelFormatter ChannelFormatter
        {
            get { throw new NotImplementedException(); }
        }

        public void ChannelAuthenticated(Server server, bool isAuthenticated)
        {
            if (_primaryChannel != null && _primaryChannel.PeerAddress.ToString().Equals(server.Address.ToString()))
            {
                _primaryChannel.IsAuthenticated = isAuthenticated;
            }
            else
            {
                if (_secondaryChannels.ContainsKey(server.ToString()))
                {
                    IRequestResponseChannel channel = _secondaryChannels[server.ToString()];
                    channel.IsAuthenticated = isAuthenticated;
                }
            }
        }

        public void RemoveUnauthenticatedChannel(Server server)
        {
            if (_primaryChannel.PeerAddress.ToString().Equals(server.Address.ToString()))
            {
                _primaryChannel.IsAuthenticated = false;
            }
            else
            {
                string key = ConvertToKey(server.Address);
                if (_secondaryChannels.ContainsKey(key))
                {
                    IRequestResponseChannel channel = _secondaryChannels[key];
                    channel.IsAuthenticated = false;
                }
            }
        }

        #region IRequestListener Members

        public object OnRequest(IRequest request)
        {

            if (request != null)
            {
                Server source = null;
                if (request.Source != null)
                {
                    source = new Server(request.Source, Status.Running);
                }
                return (IDBResponse) request.Message;
            }

            return "";
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            var retries = 3;
            bool isConnected = false;
            if (channel != null)
            {
                while (0 < retries--)
                {
                    try
                    {
                        if (channel.RetryConnect())
                        {
                            isConnected = true;
                            break;
                        }
                    }
                    catch
                    {
                        if (retries == 2)
                            Thread.Sleep(30000);
                        else if (retries == 1)
                            Thread.Sleep(60000);
                    }
                }
                if (_primaryChannel == null || channel == null)
                    return;

                if (isConnected)
                {
                    if(_channelDisconnectionListener != null)
                    {
                        Server server = new Server(channel.PeerAddress, Status.Running);
                        _channelDisconnectionListener.OnChannelDisconnected(this, server);
                    }
                }
            }

        }

        #endregion
        
        internal void RegisterDisconnectionListener(IChannelDisconnectionListener listener)
        {
            _channelDisconnectionListener = listener;
        }

        internal void UnregisterDisconnectionListener()
        {
            _channelDisconnectionListener = null;
        }
    }
}
