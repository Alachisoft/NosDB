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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.RPC;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class ConfigurationCluster:IDisposable,IRequestListener
    {
        private const byte UNINITIIALIZED = 1;
        private const byte RUNNING = 2;
        private const byte STEPPING_DOWN = 4;

        private OutProcConfigurationSession _session;
        private ConfigServerConfiguration _configuration;
        private string _localServerIP;
        private ServerNode _peerNode;
        private NodeRole _currentRole = NodeRole.None;
        private ConfigurationServer _cfgServer;
        private Thread _reconnctionThread;
        private TimeSpan _reconnectInterval;
        private Latch _status = new Latch(UNINITIIALIZED);
        private bool _disposed;
        
        public ConfigurationCluster(string localServerIP,ConfigurationServer server)
        {
            _localServerIP = localServerIP;
            _reconnectInterval = new TimeSpan(0, 0, 5);
            _cfgServer = server;
        }

        public void Initialize(ConfigServerConfiguration configuration)
        {
            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster", "started configuration cluster");

            UpdateConfiguration(configuration);
        }

        public void UpdateConfiguration(ConfigServerConfiguration configuration)
        {
            _configuration = configuration;
            bool needReconnect = false;
            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster", "configuration has been changed");


            if(_configuration != null)
            {
                ServerNode peerNode = _configuration.Servers.Nodes.Values.FirstOrDefault(p => p.Name.ToLower() != _localServerIP.ToLower());

                if(_peerNode != null)
                {
                    if(peerNode == null || !peerNode.Name.ToLower().Equals(_peerNode.Name.ToLower()))
                    {
                        //peer node is removed from configuration
                        if(_session != null)
                        {
                            _session.Disconnect();
                        }

                        if(_reconnctionThread != null)
                        {
                            _reconnctionThread.Abort();
                        }
                        needReconnect = peerNode != null && !peerNode.Name.ToLower().Equals(_peerNode.Name.ToLower());
                    }
                }
                else
                {
                    if (peerNode != null)
                        needReconnect = true;
                }

                _peerNode = peerNode;
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled && _peerNode != null)
                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.Update", "peer configuration server " + _peerNode.Name);

                if(needReconnect)
                {
                    if(!ConnectToPeerServer())
                    {
                        ConnectInBackground();
                    }
                }
                else
                {
                    if(_peerNode == null)
                    {
         
                        if (_session != null) _session.Disconnect();
                    }
                    if (_reconnctionThread != null && _reconnctionThread.IsAlive)
                        _reconnctionThread.Abort();
                    DetermineRole();
                }
                if(!(_configuration.Servers != null && _configuration.Servers.Nodes != null && _configuration.Servers.Nodes.Count >0))
                {

                    //By default an unitialized server is considered in primary role
                    BecomePrimary();
                }
            }
        }
        
        public bool IsClustered
        {
            get { return _configuration != null && _peerNode != null ? true : false; }
        }


        public bool IsActive
        {
            get 
            {
                _status.WaitForAny(RUNNING);

                return _currentRole == NodeRole.Primary; 
            }
        }

        public NodeRole CurrentRole { get { return _currentRole; } }

        public bool IsConnected
        {
            get { return _session != null && _session.Channel != null && _session.Channel.Connected; }
        }

        public bool HasSynchrnonized
        {
            get
            {
                _status.WaitForAny(RUNNING);
                return true;
            }
        }

        private bool ConnectToPeerServer(bool changeRole = true)
        {
            if(IsClustered)
            {
                try
                {
                    DatabaseRPCService rpcService = new DatabaseRPCService(_peerNode.Name);
                    IConfigurationServer remoteCfgServer = rpcService.GetConfigurationServer(new TimeSpan(0, 1, 30), Common.Communication.SessionTypes.Client, new ConfigurationChannelFormatter());
                    ((OutProcConfigurationClient)remoteCfgServer).AutoReconnect = false;
                    remoteCfgServer.MarkConfiguraitonSession();
                    OutProcConfigurationSession session = remoteCfgServer.OpenConfigurationSession(new SSPIClientAuthenticationCredential()) as OutProcConfigurationSession;
                    session.SetChannelDisconnectedListener(this);
                    session.AutoReconnect = false;
                    session.MarkSessionInternal();
                    _session = session;

                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.Connect", "connected with peer server " + _peerNode.Name);

                }
                catch(Exception e)
                {
                    //if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                    //    LoggerManager.Instance.CONDBLogger.Error("ConfigurationCluster.Connect", e.ToString());

                }
                
            }

            if (changeRole)
            {
                DetermineRole();

                if (_currentRole == NodeRole.Secondary)
                {

                    if (_configuration != null)
                    {
                        ServerNode localNode = _configuration.Servers.GetNode(_localServerIP);

                        if (_peerNode != null && _peerNode.Priority > localNode.Priority)
                        {
                            //Let's take over 
                            TakeOverPrimaryRole();
                        }
                        else
                        {
                            //Need to replicate state from peer node
                            ReplicateStateFromPeerServer();
                        }

                    }
                    _status.SetStatusBit(RUNNING, UNINITIIALIZED);
                }
            }

            return _session != null;
        }

        private void ReplicateStateFromPeerServer()
        {
            if(_session != null)
            {
                try
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.ReplicateStateFromPeerServer", "replicating state from peer server");

                    object state = _session.GetState();

                    if (_cfgServer != null && state != null)
                    {
                        _cfgServer.ApplyState(state);

                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                            LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.ReplicateStateFromPeerServer", "state replicated from peer servre");

                    }
                    else
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                            LoggerManager.Instance.CONDBLogger.Error("ConfigurationCluster.ReplicateStateFromPeerServer", "No state is returned from peer node");

                    }
                }
                catch(Exception e)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("ConfigurationCluster.ReplicateStateFromPeerServer", "An error occured while replicating state from peer server " + e);

                }
            }
        }

        private void TakeOverPrimaryRole()
        {
            if(_session != null)
            {
                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "primary role take over is started");

                _session.BeginTakeOver();
                ReplicateStateFromPeerServer();
                if (_session.Demote())
                {
                    BecomePrimary();

                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "primary role take over is complete");

                }
                else
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "peer server denied to demote himself");

                }
                
            }
        }

        private void DetermineRole()
        {
            if(_configuration != null)
            {
                ServerNode localNode = _configuration.Servers.GetNode(_localServerIP);

                if(localNode != null)
                {
                    if (_peerNode != null)
                    {
                        if (_session == null)
                        {
                            if (_currentRole != NodeRole.Primary)
                            {
                                BecomePrimary();

                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "setting current role :" + _currentRole);
                            }
                            return;
                        }

                        NodeRole peerRole = _session.GetCurrentRole();

                        if(peerRole == NodeRole.Primary)
                        {
                            if (localNode.Priority > _peerNode.Priority)
                            {
                                  if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "peer configuration server is primary, therefore setting current role to " + _currentRole);
                            }
                            else
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "peer configuration server is primary, need to replicate state from it and take over the primary role");

                            }
                            BecomeSecondary();
                        }
                        else if(peerRole == NodeRole.Secondary)
                        {
                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "connected with peer server with secondary role");

                            BecomePrimary();
                        }
                        else if(peerRole == NodeRole.None)
                        {
                            if (_currentRole != NodeRole.Primary)
                            {
                                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "peer node role is set to None");

                                if (localNode.Priority > _peerNode.Priority)
                                {
                                    BecomeSecondary();
                                }
                                else
                                {
                                    BecomePrimary();
                                }
                            }
                        }
                    }
                    else
                    {
                        BecomePrimary();
                    }
                }

                if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                    LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.DetermineRole", "my current role is " + _currentRole);

            }
        }

        private void BecomePrimary()
        {
            _currentRole = NodeRole.Primary;
            _status.SetStatusBit(RUNNING, UNINITIIALIZED);
        }

        private void BecomeSecondary(bool changeStatus = true)
        {
            _currentRole = NodeRole.Secondary;
            if(changeStatus) _status.SetStatusBit(UNINITIIALIZED, RUNNING);
        }

        public bool Demote()
        {
            if(_currentRole == NodeRole.Primary)
            {
                ServerNode localNode = _configuration.Servers.GetNode(_localServerIP);
                if (_peerNode != null)
                {
                    if (_peerNode.Priority < localNode.Priority)
                    {
                        BecomeSecondary(false);

                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                            LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.Demote", "demoting as primary on the request of peer server");

                        return true;
                    }
                    else
                    {
                        if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                            LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.Demote", "denying demoting as primary because my priority is higher than his priority");

                    }
                }
                else
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                        LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster.Demote", "denying demoting as primary because, I am not part of the cluster");

            }

            return false;
        }

        public bool ReplicateTransaction(ConfigurationStore.Transaction transaction)
        {
            if(IsActive)
            {
                if(IsClustered && IsConnected)
                {
                    _session.ReplicateTransaction(transaction);
                }
            }
            
            return true;
        }

        public void Dispose()
        {
            _disposed = true;
            if (_session != null)
            {
                _session.Disconnect();
            }

            if (_reconnctionThread != null && _reconnctionThread.IsAlive)
                _reconnctionThread.Abort();

            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsInfoEnabled)
                LoggerManager.Instance.CONDBLogger.Info("ConfigurationCluster", "stopping configuration cluster");

        }



        public object OnRequest(IRequest request)
        {
            return null;
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            if (_disposed) return;
            lock(this)
            {
                _session = null;
                //Try to re-connect immediately. 
                if (ConnectToPeerServer(false))
                {
                    return;
                }
                //In case we are unable to reconnect 
                DetermineRole();
                ConnectInBackground();
            }

        }

        private void ConnectInBackground()
        {
            if (_reconnctionThread == null)
            {
                _reconnctionThread = new Thread(new ThreadStart(Reconnect));
                _reconnctionThread.IsBackground = true;
                _reconnctionThread.Name = "ConfigurationCluster.Reconnect";
                _reconnctionThread.Start();
            }
        }

        private void Reconnect()
        {
            DateTime loggTime = DateTime.Now;
            while(true && !_disposed)
            {
                try
                {
                    if (ConnectToPeerServer())
                    {
                        lock (this)
                        {
                            _reconnctionThread = null;
                            if (_session != null)
                                break;
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - loggTime).TotalSeconds > 30)
                        {
                            loggTime = DateTime.Now;

                            if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                                LoggerManager.Instance.CONDBLogger.Error("ConfigurationCluster.Reconnect", "failed to re-establish session with peer configuration server " + _peerNode.Name);
                        }
                    }
                }
                catch (ThreadAbortException) { break; }
                catch (Exception e)
                {
                    if (LoggerManager.Instance.CONDBLogger != null && LoggerManager.Instance.CONDBLogger.IsErrorEnabled)
                        LoggerManager.Instance.CONDBLogger.Error("ConfigurationCluster.Reconnect", e.ToString());

                }

                Thread.Sleep(_reconnectInterval);
            }
        }

        internal void BeginTakeOver()
        {
            
        }
     }
}
