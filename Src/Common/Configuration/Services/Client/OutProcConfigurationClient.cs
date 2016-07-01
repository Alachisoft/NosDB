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
using System.Configuration;
using System.Linq;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services;
using System.Threading;

namespace Alachisoft.NosDB.Common.Configuration.Services.Client
{
    public class OutProcConfigurationClient : IConfigurationClient, IRequestListener
    {
        //static OutProcConfigurationClient s_instance = new OutProcConfigurationClient();
       // public static IConfigurationClient Instance { get { return s_instance; } }

        private IClientAuthenticationCredential credentials;
        public bool EnableTracing { get; set; }
        object _mutex = new object();
        private IChannelFormatter _channelFormatter;
        int _port;
        string _bindIp;
        DualChannel _channel;
        string _serviceURI;
        private string _firstConfiguratioServer;
        private string _secondConfiguratioServer;
        private string _currentConfiguratioServer;
        OutProcConfigurationSession _session;
        OutProcShardConfigurationSession _shardSession;
        SessionTypes _sessionType;
        private bool _autoReconnect = true;
        private bool _failOverToSecondary = true;
        private IRequestListener _channelDisconnctedListener;

        private bool isDatabaseClient = false;

        private bool isDistributorClient = false;

        private bool isConfigurationClient = false;

        public OutProcConfigurationClient() { }

        public OutProcConfigurationClient(bool autReconnect,IRequestListener channelDisconnctedListener)
        {
            _autoReconnect = autReconnect;
            _channelDisconnctedListener = channelDisconnctedListener;
        }

        public void Connect(string serviceURI, SessionTypes sessionType)
        {
            //_bindIp = NetworkUtil.GetLocalIPAddress().ToString();
            _port = Int32.Parse(ConfigurationSettings.AppSettings["ConfigServerPort"]);
            SetURI(serviceURI);
            _firstConfiguratioServer = serviceURI;
            _sessionType = sessionType;
        }

        public void Connect(string serviceURI, int port, SessionTypes sessionType, IChannelFormatter channelFormatter)
        {
            //_bindIp = NetworkUtil.GetLocalIPAddress().ToString(); ;
            _port = port;
            _channelFormatter = channelFormatter;
            SetURI(serviceURI);
            _sessionType = sessionType;
            _firstConfiguratioServer = serviceURI;

            try
            {
                Initialize(_firstConfiguratioServer, channelFormatter);
            }
            catch(Exception)
            {
                if (_secondConfiguratioServer != null)
                    Initialize(_secondConfiguratioServer, channelFormatter);
            }
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

        public void Disconnect()
        {

        }

        public bool AutoReconnect { get{return _autoReconnect;} set{_autoReconnect = value;} }
        public bool FailoverToSecondary { get { return _failOverToSecondary; } set { _failOverToSecondary = value; } }

        public void SetChannelDisconnectedListener(IRequestListener listener)
        {
            _channelDisconnctedListener = listener;
        }
        private void Initialize(string address,IChannelFormatter channelFormatter)
        {
            TraceProvider traceProvider = EnableTracing ? new TraceProvider() : null;
        //    ManagementHost.RegisterCompactTypes();
            if(string.IsNullOrEmpty(address))
                address = NetworkUtil.GetLocalIPAddress().ToString();
            //binding IP is null for ConfigClient
            _channel = new DualChannel(address, _port, null, _sessionType, traceProvider, channelFormatter);
            //_channel.RegisterRequestHandler(this);


            //RTD: whats the purpose of this?
            try
            {
                if (_channel.Connect(true))
                {
                    //string _sessionType = ((int)sessionType).ToString();
                    //_channel.SendMessage(_sessionType, true);

                }
            }
            catch (ChannelException ex)
            {
                //if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                //    LoggerManager.Instance.CONDBLogger.Error("Error: OutprocConfigClient.Init()", ex.ToString());
                throw ;
            }

        }

        public IShardConfigurationSession OpenShardConfigurationSession(string cluster, string shard, ServerNode node, UserCredentials credentials, IChannelFormatter channelFormatter)
        {
            Initialize(_serviceURI, channelFormatter);
            _shardSession = new OutProcShardConfigurationSession(cluster, shard, node, credentials, _channel);
            return _shardSession;
        }


        public IServerAuthenticationCredential ServerAuthenticationCredenital { set; get; }


        public IConfigurationSession OpenConfigurationSession(IClientAuthenticationCredential credentials)
        {
            this.credentials = credentials;
            try
            {
                _session = new OutProcConfigurationSession(_serviceURI, _channel, credentials, _channelFormatter);
                _session.IsDatabaseSession = isDatabaseClient;
                _session.IsDistributorSession = isDistributorClient;
                _session.IsConfigurationSession = isConfigurationClient;
                ServerAuthenticationCredenital = _session.Authenticate(credentials);
                _session.Channel.IsAuthenticated = ServerAuthenticationCredenital.IsAuthenticated;
                _session.SessionType = _sessionType;

                if (ServerAuthenticationCredenital.IsAuthenticated)
                {
                    DetermineSecondaryConfigurationServer();
                    _session.SessionId = ServerAuthenticationCredenital.SessionId;
                    return _session;
                }
                else
                    throw new Alachisoft.NosDB.Common.Security.SecurityException(ErrorCodes.Security.USER_NOT_REGISTERED, new string[1] { credentials.UserName });
            }
            catch(Exception ex)
            {
                _channel.Disconnect();
                throw;
            }
        }

        private void DetermineSecondaryConfigurationServer()
        {
            if (AutoReconnect && FailoverToSecondary)
            {
                ConfigServerConfiguration configuration = _session.GetConfigurationClusterConfiguration("*");

                if (configuration != null)
                {
                    if (configuration.Servers != null && configuration.Servers.Nodes != null)
                    {
                        ServerNode secondServer = configuration.Servers.Nodes.Values.FirstOrDefault(s => s.Name.ToLower() != _firstConfiguratioServer);

                        if (secondServer != null)
                        {
                            _secondConfiguratioServer = secondServer.Name;
                        }
                    }
                }
            }
        }

        

        #region IRequestListener Members
        public object OnRequest(IRequest request)
        {
            try
            {
                if (request != null)
                {
                    ConfigChangeEventArgs args = (ConfigChangeEventArgs)request.Message;
                    if (_session != null)
                        _session.NotifyConfigurationChange(args);
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
            if (_channelDisconnctedListener != null)
                _channelDisconnctedListener.ChannelDisconnected(channel, reason);

            if (!_autoReconnect)
            {
                return;
            }

            int retries = 3;
            bool connected = false;

            if (channel != null)
            {
                while (0 < retries--)
                {
                    try
                    {
                        lock (_mutex)
                        {
                            if (_channel.RetryConnect())
                            {
                                IServerAuthenticationCredential serverAuthCred = _session.Authenticate(credentials);
                                if(serverAuthCred != null && serverAuthCred.IsAuthenticated)
                                {
                                    _session.Channel.IsAuthenticated = serverAuthCred.IsAuthenticated;
                                    _session.SessionId = serverAuthCred.SessionId;
                                }
                                connected = true;
                                
                                break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (retries == 2)
                            Thread.Sleep(30000);
                        else if (retries == 1)
                            Thread.Sleep(60000);
                    }
                }

                if (!connected)
                {
                    retries = 3;
                    TraceProvider traceProvider = EnableTracing ? new TraceProvider() : null;
                    if (_secondConfiguratioServer != null)
                    {
                        string disconnectedServer = channel.PeerAddress.ip;
                        string failOverServer = null;
                        
                        if (string.Compare(disconnectedServer, _firstConfiguratioServer, true) == 0)
                            failOverServer = _secondConfiguratioServer;
                        else 
                            failOverServer = _firstConfiguratioServer;

                        _channel = new DualChannel(failOverServer, _port, null, SessionTypes.Management, traceProvider,
                            _channelFormatter);

                        while (0 < retries--)
                        {
                            try
                            {
                                if (_channel.Connect(true))
                                {
                                    _session.Channel = _channel;
                                    //
                                    //Connection re-established and needs to be authenticated
                                    IServerAuthenticationCredential serverAuthCred = _session.Authenticate(credentials);
                                    if (serverAuthCred != null && serverAuthCred.IsAuthenticated)
                                    {
                                        _session.Channel.IsAuthenticated = serverAuthCred.IsAuthenticated;
                                        _session.SessionId = serverAuthCred.SessionId;
                                    }
                                    break;
                                }
                            }
                            catch (ChannelException ex)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                    LoggerManager.Instance.CONDBLogger.Error("Error: OutProcConfigClient.ChannelDisconnected()", ex.ToString());
                            }
                        }
                    }
                }

               
            }

        }
        #endregion

        #region Recovery Operations
        public Common.Recovery.RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config)
        {
            throw new NotImplementedException();
        }

        public Common.Recovery.RecoveryOperationStatus CancelRecoveryJob(string identifier)
        {
            throw new NotImplementedException();
        }

        public Common.Recovery.RecoveryOperationStatus[] CancelAllRecoveryJobs()
        {
            throw new NotImplementedException();
        }

        public Common.Recovery.ClusteredRecoveryJobState GetJobState(string identifier)
        {
            throw new NotImplementedException();
        }
        #endregion




        public IServerAuthenticationCredential AuthenticateClient(IClientAuthenticationCredential credentials)
        {
            return _session.Authenticate(credentials);
        }

        public void MarkDatabaseSesion()
        {
            isDatabaseClient = true;
        }


        public void MarkDistributorSession()
        {
            isDistributorClient = true;
        }


        public void MarkConfiguraitonSession()
        {
            isConfigurationClient = true;
        }
    }
}
