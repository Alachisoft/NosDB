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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.SSPI;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Toplogies.Impl.MembershipManagement;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Common.Configuration.Services.Client
{
    public class OutProcConfigurationSession : IConfigurationSession, IRequestListener
    {
        private DateTime _sessionStartTime;
        private DualChannel _channel;
        private IList<IConfigurationListener> configurationChangeListener = new List<IConfigurationListener>();
        private bool _autoReconnect = true;
        private bool _failOverToSecondary = true;
        private IRequestListener _channelDisconnctedListener;
        string _serviceURI;
        private string _firstConfiguratioServer;
        private string _secondConfiguratioServer;
        private string _currentConfiguratioServer;
        private IClientAuthenticationCredential _credentials;
        public bool EnableTracing { get; set; }
        object _mutex = new object();
        public ISessionId SessionId { set; get; }
        private IChannelFormatter _channelFormatter;
        int _port;
        string _bindIp;
        private Thread _sessionReestablishmentThread;
        private System.Collections.Generic.IList<IConfigurationSessionListener> _listeners = new System.Collections.Generic.List<IConfigurationSessionListener>();

        private bool isDatabaseSession = false;
        private bool isDistributorSession = false;
        private bool isConfigurationSession = false;
        private Latch _connectivityStatus = new Latch(CONNECTED);

        const int CONNECTED = 1;
        const int RE_CONNECTING = 2;
        const int DISCONNCTED = 4;


        public IDualChannel Channel
        {
            get { return _channel; }
            internal set
            {
                _channel = (DualChannel)value;
                _channel.RegisterRequestHandler(this);
            }
        }

        public IClientAuthenticationCredential ClientCredentials { get { return _credentials; } }

        public OutProcConfigurationSession(String serviceURI, DualChannel channel, IClientAuthenticationCredential securityCredentials, IChannelFormatter formatter)
        {
            _credentials = securityCredentials;
            _channel = channel;
            _channelFormatter = formatter;
            SetURI(serviceURI);
            _channel.RegisterRequestHandler(this);

            _bindIp = NetworkUtil.GetLocalIPAddress().ToString();

            if (channel != null)
            {
                _port = channel.PeerAddress.Port;
            }

            DetermineSecondaryConfigurationServer();
            //+security context initialization
            //-security context initialization
        }

        private void SetURI(String serviceURI)
        {
            _serviceURI = serviceURI;

            if (serviceURI != null)
            {
                string[] splitted = _serviceURI.Split(new char[] { ';' });

                if (splitted != null)
                {
                    if (splitted.Length > 0)
                        _firstConfiguratioServer = splitted[0].Trim();

                    if (splitted.Length > 1)
                        _secondConfiguratioServer = splitted[1].Trim();
                }
            }

        }

        public bool AutoReconnect { get { return _autoReconnect; } set { _autoReconnect = value; } }

        public bool SwithToActiveNode { get { return _swithToAcitve; } set { _swithToAcitve = value; } }

        public void Connect(string serviceURI)
        {
            try
            {
                _channel.Connect(true);
            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: OutProcConfigSession.Connect()", e.ToString());
            }
        }

        public void Disconnect()
        {
            _channel.Disconnect();
        }

        public void Start()
        {
            _sessionStartTime = DateTime.Now;
        }

        public void Stop()
        {
            if (_channel != null)
                _channel.Disconnect();
        }

        public void RegisterListener(IConfigurationSessionListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(IConfigurationSessionListener listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }

        #region Security



        private ClientContext _clientSecurityContext;
        private ClientCredential _clientSecurityCredential;
        private bool _swithToAcitve = true;
        private bool _internalSession;
        private bool _recconnectOnNextCall;

        public bool IsAuthenticated { set; get; }

        public IServerAuthenticationCredential OpenConfigurationSession(string cluster, IClientAuthenticationCredential clientCredentials)
        {
            if (!cluster.Equals(MiscUtil.CLUSTERED))
                    MarkSessionInternal();

            return Authenticate(clientCredentials);
        }

        public IServerAuthenticationCredential Authenticate(IClientAuthenticationCredential clientCredentials)
        {
            if (isDatabaseSession)
                MarkDatabaseSession();
            if (isDistributorSession)
                MarkDistributorSession();
            if (isConfigurationSession)
                MarkConfigurationSession();
            if (clientCredentials is SSPIClientAuthenticationCredential)
                return AuthenticateWindowsClient(clientCredentials as SSPIClientAuthenticationCredential);
            else return null;
        }

        private IServerAuthenticationCredential AuthenticateWindowsClient(SSPIClientAuthenticationCredential clientCredentials)
        {
            #region spn

            string SPN;
            if (SSPIUtility.IsLocalServer(_channel.PeerAddress.IpAddress))
                SPN = null;
            else
            {
                try
                {
                    SPN = SSPIUtility.GetServicePrincipalName(MiscUtil.NOSCONF_SPN, _channel.PeerAddress.IpAddress);
                    //SPN += (":" + _channel.PeerAddress.Port);
                }
                catch (System.Net.Sockets.SocketException exc)
                {
                    SPN = null;
                }
            }

            this._clientSecurityCredential = SSPIUtility.GetClientCredentials(SPN);

            this._clientSecurityContext = SSPIUtility.GetClientContext(_clientSecurityCredential, SPN);
            #endregion
            AuthToken clientAuthToken;
            if (clientCredentials == null)
            {
                clientCredentials = new SSPIClientAuthenticationCredential() { UserName = SSPIUtility.GetCurrentLogin() };
            }
            clientAuthToken = new AuthToken();
            SSPIServerAuthenticationCredential serverAuthenticationCredential = new SSPIServerAuthenticationCredential();
            serverAuthenticationCredential.Token = new AuthToken();
            serverAuthenticationCredential.Token.Status = SecurityStatus.None;
            do
            {
                Byte[] clientToken = null;
                clientAuthToken.Status = this._clientSecurityContext.Init(serverAuthenticationCredential.Token.Token, out clientToken);
                clientAuthToken.Token = clientToken;
                clientCredentials.Token = clientAuthToken;

                if (clientAuthToken.Status == SecurityStatus.ContinueNeeded || (clientAuthToken.Status == SecurityStatus.OK && clientAuthToken.Token != null))
                {
                    ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.Authenticate, 1);
                    command.Parameters.AddParameter(clientCredentials);

                    try
                    {
                        serverAuthenticationCredential = ExecuteCommandOnConfigurationServer(command, true, false) as SSPIServerAuthenticationCredential;
                    }
                    catch (SecurityException exc)
                    {
                        if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                            LoggerManager.Instance.StorageLogger.Error("Authenticating Database Server", exc.Message);
                    }
                }
                if (serverAuthenticationCredential.Token.Status == SecurityStatus.SecurityDisabled || (clientCredentials.Token.Status == SecurityStatus.OK && serverAuthenticationCredential == null))
                    break;
            } while (clientAuthToken.Status == SecurityStatus.ContinueNeeded);
            if(clientAuthToken.Status == SecurityStatus.OK)
                SessionId = serverAuthenticationCredential.SessionId;
            return serverAuthenticationCredential;
        }
        #endregion


        public ClusterConfiguration GetDatabaseClusterConfiguration(string clusterName)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDatabaseClusterConfiguration, 1);
            command.Parameters.AddParameter(clusterName);
            return ExecuteCommandOnConfigurationServer(command, true) as ClusterConfiguration;
        }

        public Dictionary<string, int> GetShardsPort(string clusterName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetShardsPort, 1);
            command.Parameters.AddParameter(clusterName);
            return ExecuteCommandOnConfigurationServer(command, true) as Dictionary<string, int>;
        }

        public ConfigServerConfiguration GetConfigurationClusterConfiguration(string configCluster)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetConfigurationClusterConfiguration, 1);
            command.Parameters.AddParameter(configCluster);
            return ExecuteCommandOnConfigurationServer(command, true) as ConfigServerConfiguration;
        }

        public void RegisterClusterConfiguration(ClusterConfiguration configuration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RegisterClusterConfiguration, 1);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UnregisterClusterConfiguration(ClusterConfiguration configuration)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UnregisterClusterConfiguration, 1);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateClusterConfiguration(ClusterConfiguration configuration)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateClusterConfiguration, 1);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public DateTime SessionStartTime
        {
            get { return this._sessionStartTime; }
        }

        public void Close()
        {
            if (_channel != null)
            {

                _channel.Disconnect();
                _channel.Dispose();
            }
        }

        public void CreateCluster(string name, ClusterConfiguration configuration)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateCluster, 1);
            command.Parameters.AddParameter(name);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void RemoveCluster(string name)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RemoveCluster, 1);
            command.Parameters.AddParameter(name);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool AddShardToCluster(string cluster, string shard, ShardConfiguration shardConfiguration, IDistributionConfiguration distributionConfiguration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.AddShardToCluster, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(shardConfiguration);
            command.Parameters.AddParameter(distributionConfiguration);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool RemoveShardFromCluster(string cluster, string shard, IDistributionConfiguration configuration, Boolean isGraceFull)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RemoveShardFromCluster, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(configuration);
            command.Parameters.AddParameter(isGraceFull);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool AddServerToShard(string cluster, string shard, ServerNode server)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.AddServerToShard, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool RemoveServerFromShard(string cluster, string shard, ServerNode server)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RemoveServerFromShard, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        protected object ExecuteCommandOnConfigurationServer(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.ManagementCommand command, bool response,bool checkConnectivity = true)
        {
            ManagementResponse managementResponse = null;
            DualChannel channel = _channel;

            if(checkConnectivity)
            {
                _connectivityStatus.WaitForAny(DISCONNCTED | CONNECTED,10000);
            }

            
            if (channel != null)
            {
                if (_recconnectOnNextCall)
                {
                    _recconnectOnNextCall = false;
                    string failOverServer = null;

                    if (_firstConfiguratioServer != null && string.Compare(channel.PeerAddress.IpAddress.ToString(), _firstConfiguratioServer) != 0)
                    {
                        failOverServer = _firstConfiguratioServer;
                    }
                    else if (_secondConfiguratioServer != null && string.Compare(channel.PeerAddress.IpAddress.ToString(), _secondConfiguratioServer) != 0)
                    {
                        failOverServer = _secondConfiguratioServer;
                    }

                    if (failOverServer !=null)
                    {
                        channel.Disconnect();
                        channel = new DualChannel(failOverServer, _port, null, SessionType, null, _channelFormatter);

                        if (EstablishSession(channel))
                        {
                            _channel = channel;
                            _connectivityStatus.SetStatusBit(CONNECTED, DISCONNCTED | RE_CONNECTING);
                        }
                        else
                            channel = null;
                    }
                }
            }

            if (channel != null)
            {
                try
                {
                    managementResponse = channel.SendMessage(command, !response) as ManagementResponse;
                }
                catch (System.Exception e)
                {
                    throw e;
                }


                if (managementResponse != null && managementResponse.Exception != null)
                {
                    string connectedServer = channel.PeerAddress.ip;

                    lock (_mutex)
                    {
                        if (SwithToActiveNode && managementResponse.Exception.Message.Equals("Configuration server is currently running in passive mode"))
                        {
                            if (_secondConfiguratioServer != null && channel != null)
                            {
                                _connectivityStatus.SetStatusBit(RE_CONNECTING, CONNECTED | DISCONNCTED);
                                string channelConnectedIP = _channel != null ? _channel.PeerAddress.ip : null;

                                if (connectedServer != null && channelConnectedIP != null && string.Compare(connectedServer, channelConnectedIP, true) != 0)
                                {
                                    /*In casa, 2 parallel thread get's this exception and one has already established connection with the other server. Now when
                                    second server enters this area, it verifies if the server from who he got exception and one which is currently connected are different
                                    then does not try to re-establish another connection*/

                                    channel = _channel;
                                    managementResponse = _channel.SendMessage(command, !response) as ManagementResponse;
                                }
                                else
                                {
                                    string failOverServer = channel.PeerAddress.ip;

                                    if (string.Compare(failOverServer, _firstConfiguratioServer, true) == 0)
                                        failOverServer = _secondConfiguratioServer;
                                    else
                                        failOverServer = _firstConfiguratioServer;

                                    if (channel != null)
                                    {
                                        channel.Disconnect();
                                    }

                                    channel = new DualChannel(failOverServer, _port, null, SessionType, null, _channelFormatter);

                                    if (EstablishSession(channel))
                                    {
                                        _channel = channel;
                                        _connectivityStatus.SetStatusBit(CONNECTED, DISCONNCTED| RE_CONNECTING);
                                        managementResponse = _channel.SendMessage(command, !response) as ManagementResponse;
                                    }
                                }
                            }
                        }
                    }
                    if (managementResponse != null)
                    {
                        if (managementResponse.Exception is ManagementException)
                            throw managementResponse.Exception;
                        else if(managementResponse.Exception != null)
                            throw new ManagementException(managementResponse.Exception.Message);
                    }
                }
            }
            else
                throw new ManagementException("No configuration server is available to process the request");

            if (managementResponse != null)
                return managementResponse.ResponseMessage;

            return null;
        }

        private ManagementCommand GetManagementCommand(string method, int overload)
        {
            ManagementCommand command = new ManagementCommand();
            command.MethodName = method;
            command.Overload = overload;
            return command;
        }

        public bool HasSynchronizedWithPrimaryConfigServer()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.HasSynchronizedWIthPrimaryServer, 1);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public System.Collections.Generic.List<Address> GetConfServers(string cluster)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetConfClusterServers, 1);
            command.Parameters.AddParameter(cluster);
            return ExecuteCommandOnConfigurationServer(command, true) as System.Collections.Generic.List<Address>;
        }

        public ClusterInfo[] GetConfiguredClusters()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetConfiguredClusters, 1);
            return ExecuteCommandOnConfigurationServer(command, true) as ClusterInfo[];
        }

        public ClusterInfo GetDatabaseClusterInfo(string cluster)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDatabaseClusterInfo, 1);
            command.Parameters.AddParameter(cluster);
            return ExecuteCommandOnConfigurationServer(command, true) as ClusterInfo;
        }

        public IDistribution GetCollectionDistribution(string cluster, string database, string collection)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetCollectionDistribution, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            return ExecuteCommandOnConfigurationServer(command, true) as IDistribution;
        }

        public void AddConfigurationListener(IConfigurationListener listener)
        {
            if (!configurationChangeListener.Contains(listener))
                configurationChangeListener.Add(listener);

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.AddConfigurationListener, 1);
            command.Parameters.AddParameter(null);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void RemoveConfigurationListener(IConfigurationListener listener)
        {
            if (configurationChangeListener.Contains(listener))
                configurationChangeListener.Remove(listener);
        }

        public void SetNodeStatus(string cluster, string shard, ServerNode primary, NodeRole status)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SetNodeStatus, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(primary);
            command.Parameters.AddParameter(status);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public Membership[] GetMembershipInfo(string cluster)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 1);
            command.Parameters.AddParameter(cluster);
            return ExecuteCommandOnConfigurationServer(command, true) as Membership[];
        }

        public Membership GetMembershipInfo(string cluster, string shard)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetMembershipInfo, 2);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            return ExecuteCommandOnConfigurationServer(command, true) as Membership;
        }

        public void UpdateNodeStatus(string cluster, string shard, ServerNode server, Status status)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateNodeStatus, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            command.Parameters.AddParameter(status);
            ExecuteCommandOnConfigurationServer(command, true);
        }
       


 

    
      

        public void ConfigureDistributionStategy(string cluster, string database, string collection, IDistributionStrategy strategy)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ConfigureDistributionStategy, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(strategy);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public IDistributionStrategy GetDistriubtionStrategy(string cluster, string database, string collection)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDistriubtionStrategy, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            return ExecuteCommandOnConfigurationServer(command, true) as IDistributionStrategy;
        }

        public IDistribution GetCurrentDistribution(string cluster, string database, string collection)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetCurrentDistribution, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            return ExecuteCommandOnConfigurationServer(command, true) as IDistribution;
        }

        public IDistribution BalanceData(string cluster, string database, string collection)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.BalanceData, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            return ExecuteCommandOnConfigurationServer(command, true) as IDistribution;
        }

        public void CreateDatabase(string cluster, DatabaseConfiguration configuration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateDatabase, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void DropDatabase(string cluster, string database, bool dropFiles)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropDatabase, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(dropFiles);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void CreateCollection(string cluster, string database, CollectionConfiguration configuration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateCollection, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void MoveCollection(string cluster, string database, string collection, string newShard)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.MoveCollection, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(newShard);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void DropCollection(string cluster, string database, string collection)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropCollection, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            ExecuteCommandOnConfigurationServer(command, true);
        }

     

        public void UpdateMembership(string cluster, string shard, Membership mebership)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateMembership, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(mebership);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void NotifyConfigurationChange(ConfigChangeEventArgs arguments)
        {
            if (configurationChangeListener != null)
            {
                foreach (IConfigurationListener listener in configurationChangeListener)
                {
                    listener.OnConfigurationChanged(arguments);
                }
            }
        }

        public Membership ReportNodeJoining(string cluster, string shard, ServerNode joiningServer)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ReportNodeJoining, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(joiningServer);
            return ExecuteCommandOnConfigurationServer(command, true) as Membership;
        }

        public Membership ReportNodeLeaving(string cluster, string shard, ServerNode leavingServer)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ReportNodeLeaving, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(leavingServer);
            return ExecuteCommandOnConfigurationServer(command, true) as Membership;
        }

        public int ReportHeartbeat(string cluster, string shard, ServerNode reportingServer, Membership membership, OperationId lastOpId)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ReportHeartbeat, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(reportingServer);
            command.Parameters.AddParameter(membership);
            command.Parameters.AddParameter(lastOpId);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);

            return returnVal == null ? 0 : (int)returnVal;
        }

        public void CreateIndex(string cluster, string database, string collection, IndexConfiguration configuration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateIndex, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(configuration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void DropIndex(string cluster, string database, string collection, string indexName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropIndex, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(indexName);
            ExecuteCommandOnConfigurationServer(command, true);
        }


  
        

        public void UpdateCollectionStatistics(string cluster, string database, string collection, CollectionStatistics statistics)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateCollectionStatistics, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(statistics);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateBucketStatistics(string cluster, string database, string collection, Common.Stats.ShardInfo shardInfo)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateBucketStatistics, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(shardInfo);
            ExecuteCommandOnConfigurationServer(command, true);

        }



        #region State Transfer Operations

        public Object StateTransferOperation(String clusterName, IStateTransferOperation operation)
        {


            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StateTransferOperation, 1);
            command.Parameters.AddParameter(clusterName);
            command.Parameters.AddParameter(operation);


            return ExecuteCommandOnConfigurationServer(command, true);
        }


        #endregion

        #region Polling Operation
        public object SubmitPollingCommand(PollingOperation operation)
        {
            object response = null;
            if (operation != null)
            {
                ManagementCommand command = GetManagementCommand(operation.MethodName, 1);
                foreach (object val in operation.Parameters)
                {
                    command.Parameters.AddParameter(val);

                }
                response = ExecuteCommandOnConfigurationServer(command, true);
               
            }
            return response;
        }

        public IDictionary<string, byte[]> GetDeploymentSet(string implIdentifier)
        {
            if (implIdentifier == null) throw new ArgumentNullException("implIdentifier");

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetImplementation, 1);
            command.Parameters.AddParameter(implIdentifier);

            object output = ExecuteCommandOnConfigurationServer(command, true);
            return output != null ? (IDictionary<string, byte[]>)output : null;
        }

        #endregion


        public bool StartNode(string cluster, string shard, ServerNode server)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StartNode, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool StopNode(string cluster, string shard, ServerNode server)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StopNode, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public void StartCluster(string name)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StartCluster, 1);
            command.Parameters.AddParameter(name);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void StopCluster(string name)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StopCluster, 1);
            command.Parameters.AddParameter(name);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool StartShard(string cluster, string shard)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StartShard, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool StopShard(string cluster, string shard)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StopShard, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public void CreateConfigurationCluster(ConfigServerConfiguration serverConfig, int heartBeat, ReplicationConfiguration replConfig, string displayName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateConfigurationServer, 1);
            command.Parameters.AddParameter(serverConfig);
            command.Parameters.AddParameter(heartBeat);
            command.Parameters.AddParameter(replConfig);
            command.Parameters.AddParameter(displayName);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void RemoveConfigurationCluster(string configClusterName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RemoveConfigurationCluster, 1);
            command.Parameters.AddParameter(configClusterName);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool VerifyConfigurationServerAvailability()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.VerifyConfigurationServerAvailability, 1);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);
            return (bool)returnVal == true;
        }

        public bool VerifyConfigurationClusterAvailability(string configClusterName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterAvailability, 1);
            command.Parameters.AddParameter(configClusterName);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);
            return (bool)returnVal == true;
        }

        public bool VerifyConfigurationCluster(string configClusterName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.VerifyConfigurationCluster, 1);
            command.Parameters.AddParameter(configClusterName);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);
            return (bool)returnVal == true;
        }

        public bool VerifyConfigurationClusterPrimery(string configClusterName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterPrimery, 1);
            command.Parameters.AddParameter(configClusterName);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);
            return (bool)returnVal == true;
        }

        public void StartConfigurationServer(string name)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StartConfigurationServer, 1);
            command.Parameters.AddParameter(name);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void StopConfigurationServer(string name)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.StopConfigurationServer, 1);
            command.Parameters.AddParameter(name);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public NodeRole GetCurrentRole()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetCurrentRole, 1);
            return (NodeRole)ExecuteCommandOnConfigurationServer(command, true);

        }

        public void AddNodeToConfigurationCluster(string name, ServerNode node)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.AddNodeToConfigurationCluster, 1);
            command.Parameters.AddParameter(name);
            command.Parameters.AddParameter(node);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateConfigServerNodePriority(string cluster, string nodeName, int priority)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateCSNodePriority, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(nodeName);
            command.Parameters.AddParameter(priority);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void RemoveNodeFromConfigurationCluster(string cluster, ServerNode node)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.RemoveNodeFromConfigurationCluster, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(node);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public string[] ListDatabases(string cluster)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ListDatabases, 1);
            command.Parameters.AddParameter(cluster);
            return (string[])ExecuteCommandOnConfigurationServer(command, true);

        }

        public string[] ListCollections(string cluster, string database)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ListCollections, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            return (string[])ExecuteCommandOnConfigurationServer(command, true);
        }

        public string[] ListIndices(string cluster, string database, string collection)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ListIndices, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            return (string[])ExecuteCommandOnConfigurationServer(command, true);
        }

        #region Recovery Operations
        public Common.Recovery.RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SubmitRecoveryJob, 1);
            command.Parameters.AddParameter(config);

            return (RecoveryOperationStatus)ExecuteCommandOnConfigurationServer(command, true);
        }

        public Common.Recovery.RecoveryOperationStatus CancelRecoveryJob(string identifier)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CancelRecoveryJob, 1);
            command.Parameters.AddParameter(identifier);

            return (RecoveryOperationStatus)ExecuteCommandOnConfigurationServer(command, true);
        }

        public Common.Recovery.RecoveryOperationStatus[] CancelAllRecoveryJobs()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CancelAllRecoveryJobs, 1);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);
            return (RecoveryOperationStatus[])returnVal;
        }

        public Common.Recovery.ClusteredRecoveryJobState GetJobState(string identifier)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetJobState, 1);
            command.Parameters.AddParameter(identifier);

            return (ClusteredRecoveryJobState)ExecuteCommandOnConfigurationServer(command, true);
        }

        public void SubmitShardJobStatus(ShardRecoveryJobState status)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SubmitJobState, 1);
            command.Parameters.AddParameter(status);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public ClusterJobInfoObject[] GetAllRunningJobs()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetAllRunningJobs, 1);
            object returnVal = ExecuteCommandOnConfigurationServer(command, true);
            return (ClusterJobInfoObject[])returnVal;
        }

        #endregion






        public bool Grant(string clusterName, Common.Security.Impl.ResourceId resourceId, string userName, string roleName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.Grant, 1);
            command.Parameters.AddParameter(clusterName);
            command.Parameters.AddParameter(resourceId);
            command.Parameters.AddParameter(userName);
            command.Parameters.AddParameter(roleName);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool Revoke(string clusterName, Common.Security.Impl.ResourceId resourceId, string userName, string roleName)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.Revoke, 1);
            command.Parameters.AddParameter(clusterName);
            command.Parameters.AddParameter(resourceId);
            command.Parameters.AddParameter(userName);
            command.Parameters.AddParameter(roleName);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool CreateUser(Common.Security.Interfaces.IUser userInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CreateUser, 1);
            command.Parameters.AddParameter(userInfo);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool DropUser(Common.Security.Interfaces.IUser userInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.DropUser, 1);
            command.Parameters.AddParameter(userInfo);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool VerifyConfigurationClusterUID(string UID)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.VerifyConfigurationClusterUID, 1);
            command.Parameters.AddParameter(UID);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public void CreateRole(Common.Security.Interfaces.IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public object GetState()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetState, 1);
            return ExecuteCommandOnConfigurationServer(command, true);

        }

        public ShardConfiguration GetShardConfiguration(string cluster, string shard)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetShardConfiguration, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            return ExecuteCommandOnConfigurationServer(command, true) as ShardConfiguration;
        }

        public void AlterRole(Common.Security.Interfaces.IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public void DropRole(Common.Security.Interfaces.IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public void BeginTakeOver()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.BeginTakeOver, 1);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public bool Demote()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.Demote, 1);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }

        public IList<IUser> GetUsersInformation()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetUsersInformation, 1);
            return (IList<IUser>)ExecuteCommandOnConfigurationServer(command, true);
        }

        public IList<IResourceItem> GetResourcesInformation(string cluster)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetResourcesInformation, 1);
            command.Parameters.AddParameter(cluster);
            return (IList<IResourceItem>)ExecuteCommandOnConfigurationServer(command, true);
        }


        public void UpdateDatabaseConfiguration(string cluster, string database, DatabaseConfiguration databaseConfiguration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateDatabaseConfiguration, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(databaseConfiguration);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateServerPriority(string cluster, string shard, ServerNode server, int priority)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateServerPriority, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            command.Parameters.AddParameter(priority);
            ExecuteCommandOnConfigurationServer(command, true);
        }

   

        public void UpdateDeploymentConfiguration(string cluster, int heartBeatInterval)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateDeploymentConfiguration, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(heartBeatInterval);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateIndexAttribute(string cluster, string database, string collection, string index, IndexAttribute attribute)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateIndexAttribute, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(index);
            command.Parameters.AddParameter(attribute);
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateShardConfiguration(string cluster, string shard, int heartbeat, int port)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateShardConfiguration, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(heartbeat);
            command.Parameters.AddParameter(port);
           
            ExecuteCommandOnConfigurationServer(command, true);
        }

        public IUser GetAuthenticatedUserInfoFromConfigServer(ISessionId sessionId)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetAuthenticatedUserInfoFromConfigServer, 1);
            command.Parameters.AddParameter(sessionId);
            return (IUser)ExecuteCommandOnConfigurationServer(command, true);
        }


        public IResourceItem GetResourceSecurityInfo(string cluster, ResourceId resourceId)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetResourceSecurityInfo, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(resourceId);
            return (IResourceItem)ExecuteCommandOnConfigurationServer(command, true);
        }

        public IDictionary<IRole, IList<ResourceId>> GetUserInfo(IUser userInfo)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetUserInfo, 1);
            command.Parameters.AddParameter(userInfo);
            return (IDictionary<IRole, IList<ResourceId>>)ExecuteCommandOnConfigurationServer(command, true);
        }

        public void UpdateCollectionConfiguration(string cluster, string database, string collection, CollectionConfiguration collectionConfiguration)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.UpdateCollectionConfiguration, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(database);
            command.Parameters.AddParameter(collection);
            command.Parameters.AddParameter(collectionConfiguration);
            ExecuteCommandOnConfigurationServer(command, true);
        }


        public List<Address> GetDataBaseServerNode()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.GetDataBaseServerNode, 1);
            return ExecuteCommandOnConfigurationServer(command, true) as List<Address>;
        }

        public bool SetDatabaseMode(string cluster, string databaseName, DatabaseMode databaseMode)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SetDatabaseMode, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(databaseName);
            command.Parameters.AddParameter(databaseMode);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }


        public void DeployAssemblies(string cluster, string deploymentId, string deploymentName, string assemblyFileName, byte[] buffer)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.CopyAssemblies, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(deploymentId);
            command.Parameters.AddParameter(assemblyFileName);
            command.Parameters.AddParameter(buffer);
            ExecuteCommandOnConfigurationServer(command, true);
        }


        public bool IsRemoteClient()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.IsRemoteClient, 1);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }


        public object OnRequest(IRequest request)
        {
            try
            {
                if (request != null)
                {
                    ConfigChangeEventArgs args = (ConfigChangeEventArgs)request.Message;

                    if (args != null)
                    {
                        ChangeType type = ChangeType.None;
                        if (args != null)
                            type = args.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType);

                        if (type == ChangeType.ConfigServerAdded)
                        {
                            ServerNode affectedServer = args.GetParamValue<ServerNode>(EventParamName.ConfigServer);

                            if (affectedServer != null && SwithToActiveNode)
                            {
                                if (_secondConfiguratioServer == null)
                                {
                                    _secondConfiguratioServer = affectedServer.Name;
                                }
                            }
                            return null;
                        }
                        else if (type == ChangeType.ConfigServerRemoved)
                        {
                            ServerNode affectedServer = args.GetParamValue<ServerNode>(EventParamName.ConfigServer);

                            if (affectedServer.Name == string.Empty)
                            {
                                affectedServer = new ServerNode() { Name = _channel.PeerAddress.IpAddress.ToString() };
                            }

                            if (affectedServer != null && SwithToActiveNode)
                            {
                                if (_channel != null && string.Compare(_channel.PeerAddress.IpAddress.ToString(), affectedServer.Name, true) == 0)
                                {
                                    _recconnectOnNextCall = true;
                                }

                                if (_secondConfiguratioServer != null && string.Compare(_secondConfiguratioServer, affectedServer.Name, true) == 0)
                                {
                                    _secondConfiguratioServer = null;
                                }
                                else if (_firstConfiguratioServer != null && string.Compare(_firstConfiguratioServer, affectedServer.Name, true) == 0)
                                {
                                    _firstConfiguratioServer = _secondConfiguratioServer;
                                    _secondConfiguratioServer = null;

                                }
                            }
                        }

                    }

                    NotifyConfigurationChange(args);
                }
            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    LoggerManager.Instance.CONDBLogger.Error("Error: OutProcConfigClient.OnRequest()", e.ToString());
            }

            return null;
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            _connectivityStatus.SetStatusBit(RE_CONNECTING, CONNECTED | DISCONNCTED);
           
            if (_channelDisconnctedListener != null)
                _channelDisconnctedListener.ChannelDisconnected(channel, reason);

            RaiseSessionDisconnectedEvent();

            if (!_autoReconnect)
            {
                return;
            }

            lock (this)
            {
                if (_sessionReestablishmentThread == null)
                {
                    _sessionReestablishmentThread = new Thread(new ParameterizedThreadStart(ReestablishSession));
                    _sessionReestablishmentThread.IsBackground = true;
                    _sessionReestablishmentThread.Start(channel);
                }
            }
        }

        private void ReestablishSession(object arg)
        { 
            IRequestResponseChannel channel = arg as IRequestResponseChannel;
            try
            {
                bool connected = false;
                _recconnectOnNextCall = false;
                DateTime loggTime = DateTime.Now;
                if (channel != null)
                {
                    _channel = null;
                    string failOverServer = channel.PeerAddress.ip;
                    TraceProvider traceProvider = EnableTracing ? new TraceProvider() : null;
                    bool firstTime = true;

                    while (!connected)
                    {
                        try
                        {
                            connected = EstablishSession(channel);

                            if (connected)
                            {
                               RaiseSessionEstablishedEvent();
                            }

                            _sessionReestablishmentThread = null;
                        }
                        catch (ThreadAbortException) { break; }
                        catch (ChannelException e)
                        {

                            if ((DateTime.Now - loggTime).TotalSeconds > 180)
                            {
                                loggTime = DateTime.Now;
                                ILogger logger = GetEnvironmentLogger();
                                if (logger != null && logger.IsErrorEnabled)
                                    logger.Error("Error: OutProcConfigClient.ReestablishSession()", e.ToString());
                            }
                        }
                        catch(Exception e)
                        {
                            connected = false;
                            _connectivityStatus.SetStatusBit(DISCONNCTED, CONNECTED | RE_CONNECTING);
                           
                            ILogger logger = GetEnvironmentLogger();
                            if (logger != null && logger.IsErrorEnabled)
                                logger.Error("Error: OutProcConfigClient.ReestablishSession()", e.ToString());
                        }

                        if (!connected)
                        {
                            if (!firstTime)
                            {
                                Thread.Sleep(3000);
                                // we set connectivity status as DISCONNCTED when one cycle of connection re-establishment get's complete
                                _connectivityStatus.SetStatusBit(DISCONNCTED, CONNECTED | RE_CONNECTING);
                            }

                            //we try to re-establish connection with primary and secondary interchangebly, so that which ever is available.
                            if (SwithToActiveNode && _secondConfiguratioServer != null)
                            {
                                if (string.Compare(failOverServer, _firstConfiguratioServer, true) == 0)
                                    failOverServer = _secondConfiguratioServer;
                                else
                                    failOverServer = _firstConfiguratioServer;

                                channel = new DualChannel(failOverServer, _port, null, SessionType, traceProvider, _channelFormatter);

                            }
                        }
                    }

                }

            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                ILogger logger = GetEnvironmentLogger();
                if (logger != null && logger.IsErrorEnabled)
                    logger.Error("Error: OutProcConfigClient.ReestablishSession()", "quitting connection re-establishment due to error " +  e.ToString());
            }
        }

        private ILogger GetEnvironmentLogger()
        {
            if(LoggerManager.Instance != null)
            {
                if (LoggerManager.Instance.ManagerLogger != null)
                    return LoggerManager.Instance.ManagerLogger;

                if (LoggerManager.Instance.ShardLogger != null)
                    return LoggerManager.Instance.ShardLogger;

                if (LoggerManager.Instance.CONDBLogger != null)
                    return LoggerManager.Instance.CONDBLogger;
            }

            return null;
        }

        private void RaiseSessionEstablishedEvent()
        {
            foreach (IConfigurationSessionListener listener in _listeners)
            {
                try
                {
                    listener.OnSessionConnected(this);
                }
                catch (Exception e)
                {
                    ILogger logger = GetEnvironmentLogger();
                    if (logger != null && logger.IsErrorEnabled)
                        logger.Error("Error: OutProcConfigClient.RaiseSessionEstablishedEvent", e.ToString());
                }
            }
        }

        private void RaiseSessionDisconnectedEvent()
        {
            foreach (IConfigurationSessionListener listener in _listeners)
            {
                try
                {
                    listener.OnSessionDisconnected(this);
                }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("Error: OutProcConfigClient.RaiseSessionDisconnectedEvent", e.ToString());

                }
            }
        }
        private bool EstablishSession(IRequestResponseChannel channel)
        {
            bool connected = false;
            if (channel.Connect(true))
            {
                Channel = channel as DualChannel;
                //
                //Connection re-established and needs to be authenticated
                IServerAuthenticationCredential serverAuthCred = Authenticate(_credentials);
                if (serverAuthCred != null && serverAuthCred.IsAuthenticated)
                {
                    Channel.IsAuthenticated = serverAuthCred.IsAuthenticated;
                    SessionId = serverAuthCred.SessionId;
                }
                connected = true;
                _connectivityStatus.SetStatusBit(CONNECTED, RE_CONNECTING| DISCONNCTED);
           
                DetermineSecondaryConfigurationServer();
                if (_internalSession)
                    MarkSessionInternal();
                if(configurationChangeListener != null)
                    foreach (var configurationListener in configurationChangeListener)
                    {
                       AddConfigurationListener(configurationListener); 
                    }
            }
            return connected;
        }

        public void DetermineSecondaryConfigurationServer()
        {
            //if(!)
            if (_autoReconnect && _failOverToSecondary)
            {
                ConfigServerConfiguration configuration = GetConfigurationClusterConfiguration("*");

                if (configuration != null)
                {
                    if (configuration.Servers != null && configuration.Servers.Nodes != null)
                    {
                        ServerNode secondServer = configuration.Servers.Nodes.Values.FirstOrDefault(s => s.Name.ToLower() != _firstConfiguratioServer);

                        if (secondServer != null)
                        {
                            _secondConfiguratioServer = secondServer.Name;
                        }
                        else
                            _secondConfiguratioServer = null;
                    }
                }
            }
        }

        public void SetChannelDisconnectedListener(IRequestListener listener)
        {
            _channelDisconnctedListener = listener;
        }


        public bool IsAuthorized(ISessionId sessionId, ResourceId resourceId, ResourceId superResourceId, Permission operationPermission)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.IsAuthorized, 1);
            command.Parameters.AddParameter(sessionId);
            command.Parameters.AddParameter(resourceId);
            command.Parameters.AddParameter(superResourceId);
            command.Parameters.AddParameter(operationPermission);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }


        public void MarkDatabaseSession()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.MarkDatabaseSession, 1);
            ExecuteCommandOnConfigurationServer(command, false, false);
        }


        public void MarkDistributorSession()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.MarkDistributorSession, 1);
            ExecuteCommandOnConfigurationServer(command, false, false);
        }

        public bool ReplicateTransaction(object transaction)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ReplicateTransaction, 1);
            command.Parameters.AddParameter(transaction);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);

        }

        public void MarkConfigurationSession()
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.MarkConfiguritonSession, 1);
            ExecuteCommandOnConfigurationServer(command, false, false);
        }

        public byte[] ValidateProfessional(byte[] token)
        {
            var command = GetManagementCommand(ConfigurationCommandUtil.MethodName.ValidateProfessional, 1);
            command.Parameters.AddParameter(token);
            return (byte[]) ExecuteCommandOnConfigurationServer(command, true);
        }

        public Object BeginElection(string cluster, string shard, ServerNode server, ElectionType electionType)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.BeginElection, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(server);
            command.Parameters.AddParameter(electionType);
            return ExecuteCommandOnConfigurationServer(command, true);
        }

        public void SubmitElectionResult(string cluster, string shard, ElectionResult result)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.SubmitElectionResult, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(result);
            ExecuteCommandOnConfigurationServer(command, false);
        }

        public void EndElection(string cluster, string shard, ElectionId electionId)
        {

            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.EndElection, 1);
            command.Parameters.AddParameter(cluster);
            command.Parameters.AddParameter(shard);
            command.Parameters.AddParameter(electionId);
            ExecuteCommandOnConfigurationServer(command, false);
        }

        public void MarkSessionInternal()
        {
            SwithToActiveNode = false;
            _internalSession = true;
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.MarkSessionInternal, 1);
            ExecuteCommandOnConfigurationServer(command, true);

        }


        public bool IsDatabaseSession
        {
            set
            {
                isDatabaseSession = value;
            }
        }

        public bool IsDistributorSession
        {
            set
            {
                isDistributorSession = value;
            }
        }

        public bool IsConfigurationSession
        {
            set
            {
                isConfigurationSession = value;
            }
        }

        public SessionTypes SessionType { get; set; }

        public bool IsNodeRunning(string node)
        {
            ManagementCommand command = GetManagementCommand(ConfigurationCommandUtil.MethodName.IsNodeRunning, 1);
            command.Parameters.AddParameter(node);
            return (bool)ExecuteCommandOnConfigurationServer(command, true);
        }
    }
}
