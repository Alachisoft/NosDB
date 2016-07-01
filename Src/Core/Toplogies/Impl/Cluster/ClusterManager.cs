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
using Alachisoft.NosDB.Common.Communication.Formatters;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Logger;
using DeploymentConfiguration = Alachisoft.NosDB.Common.Configuration.DeploymentConfiguration;



namespace Alachisoft.NosDB.Core.Toplogies.Impl.Cluster
{
    public class ClusterManager : ICluster, ISessionListener, IShardListener, IConfigurationListener
    {
        private delegate Object ShardMessageDelegate(String shard, Message message);

        private IClusterListener _clusterListener = null;

        private IShard _localShard;
        private IDictionary<String, IShard> _remoteShards = new Dictionary<String, IShard>(StringComparer.InvariantCultureIgnoreCase);

        //Dynamic Information about shards
        //private ShardInfo _localShardInfo;
        //private IDictionary<string, ShardInfo> _remoteShardsInfo= new Dictionary<String,ShardInfo>(StringComparer.InvariantCultureIgnoreCase);

        //private ShardConfiguration _localShardConfig = null;
        //private Dictionary<String, ShardConfiguration> _remoteShardConfigs = null;

        private Object _clusterMutex = new Object();
        private NodeContext context = null;

        /// <summary>
        /// ConfigurationChangeTask will look for any change in configuration on config server and handlie accordinlgy
        /// </summary>
        private ConfigurationChangeTask configChangeTask = null;

        /// <summary>
        /// Listener for Configuration Change in this case Topology will listen for the changes
        /// </summary>
        private IConfigurationListener configChangeListener;

        private IConnectionRestoration _connectionRestoration = null;

        private ClusterConfigurationManager _clusterConfigMgr = null;

        public ClusterManager(NodeContext context)
        {
            this.context = context;
            this.context.ConfigurationSession.AddConfigurationListener(this);
        }

        public ClusterConfigurationManager ClusterConfigMgr
        {
            get { return _clusterConfigMgr; }
        }

        #region ICluster Implementation


        //RTD: Needs to be moved.
        public NodeRole ShardNodeRole
        {
            get { return ((LocalShard) _localShard).NodeRole; }
            set { ((LocalShard) _localShard).NodeRole = value; }
        }

        ////RTD: Needs to be moved.
        //public ShardInfo ThisShard
        //{
        //    get { return _localShardInfo; }
        //}

        //public IDictionary<String, ShardInfo> Shards
        //{
        //    get { return _remoteShardsInfo; }
        //}

        public bool Initialize(ClusterConfiguration configuration)

        //public bool Initialize(String clusterName, DeploymentConfiguration configuration)
        {
            #region Getting Local Address Logic; might be replace with getting address from service config

            if (configuration == null)
                throw new ArgumentNullException("cluster configuration can not be null");

            if (configuration.Deployment == null)
                throw new ArgumentNullException("deployment configuration can not be null");

            if (configuration.Deployment.Shards == null)
                throw new ArgumentNullException("topology configuration can not be null");

            if (context != null)
                context.ClusterName = configuration.Name;
            if (string.IsNullOrWhiteSpace(context.LocalShardName))
            {
                //Get Local Shard Name from config 
                //string shardName = ConfigurationManager.AppSettings["ShardName"];

                //if (String.IsNullOrEmpty(shardName))
                //{
                    throw new ArgumentNullException("Shard Name Can not be null or empty");
                //}

                //RTD: Needs to be moved.
                //context.LocalShardName = shardName;
            }
            //Get Base Data Path from config
            string basePath = ConfigurationSettings<DBHostSettings>.Current.BasePath;

            if (String.IsNullOrEmpty(basePath))
            {
                throw new ArgumentNullException("Data Path Can not be null or empty");
            }

            context.DataPath = basePath;
            context.BasePath = basePath + context.LocalShardName + Common.MiscUtil.DATA_FOLDERS_SEPERATION;
            //Get Base Data Path from config
            string deploymentPath = ConfigurationSettings<DBHostSettings>.Current.DeploymentPath;

            if (String.IsNullOrEmpty(deploymentPath))
            {
                throw new ArgumentNullException("Deployment Path Can not be null or empty");
            }

            context.DeploymentPath = deploymentPath;

            ShardConfiguration localShardConfig = null;
            IDictionary<String, ShardConfiguration> remoteShardConfigs = null;

            SetShardConfigs(configuration.Deployment, out localShardConfig, out remoteShardConfigs);

           

            _clusterConfigMgr = new ClusterConfigurationManager(context.ConfigurationSession, configuration);

            _connectionRestoration = new ConnectionRestorationManager();
            _connectionRestoration.Initialize(context);

            if (localShardConfig != null)
            {
                _localShard = new LocalShard(context, _connectionRestoration, _clusterConfigMgr);
                _localShard.Initialize(localShardConfig);
                _localShard.RegisterShardListener(Common.MiscUtil.CLUSTER_MANAGER, this);
            }

            if (context != null)
            {
                context.ShardServer.RegisterSessionListener(SessionTypes.Shard, this);
            }

            if (remoteShardConfigs != null && remoteShardConfigs.Keys.Count > 0)
            {                
                foreach (KeyValuePair<String, ShardConfiguration> pair in remoteShardConfigs)
                {
                    IShard remoteShard = new RemoteShard(new DualChannelFactory(), new ShardChannelFormatter(), context, _connectionRestoration);
                    remoteShard.Initialize(pair.Value);
                    remoteShard.RegisterShardListener(Common.MiscUtil.CLUSTER_MANAGER, this);
                    lock (_remoteShards)
                    {
                        _remoteShards.Add(pair.Key, remoteShard); 
                    }
                }
            }

            return true;

            #endregion
        }

       
        private void SetShardConfigs(DeploymentConfiguration configuration, out ShardConfiguration localShardConfig, out IDictionary<String, ShardConfiguration> remoteShardConfigs)
        {
            Dictionary<String, ShardConfiguration> remoteConfigDic = new Dictionary<String, ShardConfiguration>();
            ShardConfiguration localShard = null;

            foreach (Common.Configuration.ShardConfiguration shardConfig in configuration.Shards.Values)
            {
                if (shardConfig.Name.Equals(context.LocalShardName, StringComparison.OrdinalIgnoreCase))
                    localShard = shardConfig;
                else
                    //localShard = shardConfig;

                    remoteConfigDic.Add(shardConfig.Name, shardConfig);
            }

            if (localShard == null)
                throw new Alachisoft.NosDB.Common.Exceptions.ConfigurationException("Local Shard Configruation is missing");

            localShardConfig = localShard;
            remoteShardConfigs = remoteConfigDic;
        }

        public bool Start()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = context.LocalShardName != null ? context.LocalShardName : "", DatabaseName = "" });
            lock (_clusterMutex)
            {
                //RTD: this needs to be removed.
                //[Start]
                //if (this.context != null && context.ConfigurationSession != null)
                //{
                //    ClusterInfo info = context.ConfigurationSession.GetDatabaseClusterInfo(context.ClusterName);

                //    if (info.ShardInfo != null && info.ShardInfo.Count > 0)
                //    {
                //        foreach (ShardInfo shard in info.ShardInfo.Values)
                //        {
                //            //RTD: Whats the purpose of all this? 
                //            if (shard.Name.Equals(context.LocalShardName, StringComparison.OrdinalIgnoreCase))
                //            {
                //                _localShardInfo = shard;
                //            }
                //            else
                //            {
                //                if (_remoteShardsInfo == null)
                //                    _remoteShardsInfo = new Dictionary<String, ShardInfo>(StringComparer.InvariantCultureIgnoreCase);

                //                _remoteShardsInfo.Add(shard.Name, shard);
                //            }
                //        }

                //    }

                //}

                //[End]
                if (this._localShard != null)
                {
                    this._localShard.Start();
                }

                //if (_remoteShards != null && _remoteShards.Count > 0)
                //{
                //    foreach (KeyValuePair<String, IShard> remoteShard in _remoteShards)
                //    {
                //        //RTD: Same masla. Starting remote shards without any logic? (Initializing might be alright, but this??)
                //        if (remoteShard.Value != null)
                //            remoteShard.Value.Start();
                //    }
                //}

                if (configChangeTask == null)
                {
                    configChangeTask = new ConfigurationChangeTask(this, context);
                    configChangeTask.Start();
                }
            }

            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
            {

                LoggerManager.Instance.ShardLogger.Info("ClusterManager.Start()", "Cluster started with these parameters ( deploymentPath: " + context.DeploymentPath + " DataPath: " + context.BasePath + " ShardName: " + context.LocalShardName + " ).");
                LoggerManager.Instance.ShardLogger.Info("ClusterManager.Start()", "Cluster Name: " + context.ClusterName + " is started successfully.");
                
            }

            return true;
        }

        public object SendMessage(string shard, Message message)
        {
            if (_remoteShards != null && _remoteShards.ContainsKey(shard))
            {
                return _remoteShards[shard].SendUnicastMessage(null, message);
            }

            throw new ArgumentException("Specified Shard Does not exist");
        }

        public object SendBroadcastMessage(Message message)
        {

            if (this._remoteShards != null && _remoteShards.Count > 0)
            {
                IDictionary responses = new HashVector<Server, Object>();
                IDictionary delegates = new HashVector<Server, ShardMessageDelegate>();
                IDictionary results = new HashVector<Server, IAsyncResult>();

                IEnumerator _remoteShardsEnumerator = _remoteShards.GetEnumerator();
                while (_remoteShardsEnumerator.MoveNext())
                {
                    KeyValuePair<String, IShard> pair = (KeyValuePair<String, IShard>)_remoteShardsEnumerator.Current;

                    ShardMessageDelegate msgDelegate = new ShardMessageDelegate(SendMessage);
                    delegates.Add(pair.Key, msgDelegate);
                    IAsyncResult ar = msgDelegate.BeginInvoke(pair.Key, message, null, null);
                    results.Add(pair.Key, ar);
                }


                IEnumerator delegatesEnumerator = results.GetEnumerator();
                while (delegatesEnumerator.MoveNext())
                {
                    KeyValuePair<Server, IAsyncResult> pair = (KeyValuePair<Server, IAsyncResult>)delegatesEnumerator.Current;

                    Object ret = ((ShardMessageDelegate)delegates[pair.Key]).EndInvoke((IAsyncResult)results[pair.Key]);
                    if (ret != null)
                    {
                        responses.Add(pair.Key, ret);
                    }
                }

                return responses;
            }

            throw new Exception("no shard exist in servers list");

        }

        public object SendMulticastMessage(List<string> shards, Message message)
        {
            if (this._remoteShards != null && _remoteShards.Count > 0)
            {
                IDictionary responses = new HashVector<Server, Object>();
                IDictionary delegates = new HashVector<Server, ShardMessageDelegate>();
                IDictionary results = new HashVector<Server, IAsyncResult>();

                IEnumerator shardsEnumerator = shards.GetEnumerator();
                while (shardsEnumerator.MoveNext())
                {
                    String destination = (String)shardsEnumerator.Current;
                    if (this._remoteShards[destination] != null)
                    {
                        ShardMessageDelegate msgDelegate = new ShardMessageDelegate(SendMessage);
                        delegates.Add(destination, msgDelegate);

                        IAsyncResult ar = msgDelegate.BeginInvoke(destination, message, null, null);
                        results.Add(destination, ar);
                    }
                }


                IEnumerator delegatesEnumerator = results.GetEnumerator();
                while (delegatesEnumerator.MoveNext())
                {
                    DictionaryEntry pair = (DictionaryEntry)delegatesEnumerator.Current;

                    Object ret = ((ShardMessageDelegate)delegates[pair.Key]).EndInvoke((IAsyncResult)results[pair.Key]);
                    if (ret != null)
                    {
                        responses.Add(pair.Key, ret);
                    }
                }

                return responses;
            }

            throw new Exception("No server exist in servers list");

        }

        public bool RemoveRemoteShard(string shardName)
        {
            if (!_remoteShards.ContainsKey(shardName))
            {
                throw new ArgumentException("Specified Shard Does not exist");
            } 

            lock (_remoteShards)
            {
                _remoteShards[shardName].RemoveBrokenConnection();
                return _remoteShards.Remove(shardName);
            }
        }

        public void RegisterClusterListener(IClusterListener clusterListener)
        {
            this._clusterListener = clusterListener;
        }

        public void UnregisterClusterListener(IClusterListener clusterListener)
        {
            this._clusterListener = null;
        }

        public void RegisterConfigChangeListener(IConfigurationListener configChangeListener)
        {
            if (this.configChangeListener == null)
                this.configChangeListener = configChangeListener;
        }

        public void UnregisterConfigChangeListener(IConfigurationListener configChangeListener)
        {
            if (this.configChangeListener != null)
                this.configChangeListener = null;
        }

        #endregion

        #region ISessionListener Implementation

        public bool OnSessionEstablished(Session session)
        {
            if (session == null || session.SessionType != SessionTypes.Shard || session.Connection == null) return false;

            SessionInfo info = GetSessionInfo(session);
            //SessionInfo info = new SessionInfo();
            //info.Cluster = "cluster1";
            //info.Shard = "shard1";

            if (info == null || String.IsNullOrEmpty(info.Cluster) || String.IsNullOrEmpty(info.Shard))
            {
                session.Connection.Disconnect();
                return false;
            }

            if (info.Cluster.Equals(context.ClusterName) && info.Shard.Equals(_localShard.Name))
            {
                return _localShard.OnSessionEstablished(session);
            }
            else if (_localShard != null && ((LocalShard)_localShard).NodeRole == NodeRole.Primary)
            {
                if(_remoteShards!=null)
                {
                    if (this._remoteShards.ContainsKey(info.Shard))
                    {
                        RemoteShard shard = this._remoteShards[info.Shard] as RemoteShard;
                        return shard.OnSessionEstablished(session);
                    }
                    else
                    {
                        session.Connection.Disconnect();
                    }
                }
            }
            else
            {
                session.Connection.Disconnect();
            }

            return false;
        }

        private SessionInfo GetSessionInfo(Session session)
        {
            SessionInfo sessionInfo = null;

            if (session.Connection != null && session.Connection.IsConnected)
            {
                int DATA_SIZE_BUFFER_LENGTH = 10;

                Byte[] sizeBuffer = new Byte[DATA_SIZE_BUFFER_LENGTH];
                Alachisoft.NosDB.Common.Util.NetworkUtil.ReadFromTcpConnection(session.Connection, sizeBuffer);

                int rspLength = Convert.ToInt32(UTF8Encoding.UTF8.GetString(sizeBuffer, 0, sizeBuffer.Length));

                if (rspLength > 0)
                {
                    byte[] dataBuffer = new byte[rspLength];
                    Alachisoft.NosDB.Common.Util.NetworkUtil.ReadFromTcpConnection(session.Connection, dataBuffer);

                    if (_localShard != null)
                    {
                        //deserialize the message
                        IRequest message = ((LocalShard)_localShard).ChannelFormatter.Deserialize(dataBuffer) as IRequest;
                        sessionInfo = (SessionInfo)message.Message; 
                    }
                }
            }

            return sessionInfo;
        }
        #endregion

        #region IShardListener Implementation

        //public string Name
        //{
        //    get
        //    {
        //        if (context != null)
        //            return context.ClusterName;

        //        return String.Empty;
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public object OnMessageReceived(Message message, Server source)
        {
            if (this._clusterListener != null)
                return _clusterListener.OnMessageReceived(message, source);

            return null;
        }

        public void OnMemberJoined(Server server)
        {
            throw new NotImplementedException();
        }

        public void OnMemberLeft(Server server)
        {
            throw new NotImplementedException();
        }


        #endregion

        public T SendMessage<T>(string shard, Message message)
        {
            return SendMessage<T>(shard, null, message);

            //IShard dest = GetShardInstance(shard);

            //if (dest != null)
            //    return SendMessage<T>(shard, dest.Primary, message);

            //throw new ArgumentException("Specified Shard Does not exist");
        }

        public T SendMessage<T>(string shard, Server server, Message message)
        {
            IShard dest = GetShardInstance(shard);

            if (dest != null)
            {
                if (server == null)
                    server = dest.Primary;                                

                ShardRequestBase<T> unicastRequest = dest.CreateUnicastRequest<T>(server, message);

                IAsyncResult asyncResult = unicastRequest.BeginExecute();

                return unicastRequest.EndExecute(asyncResult);
            }

            throw new ArgumentException("Specified Shard '" + shard + "' Does not exist");

        }

        public IResponseCollection<T> SendMessageToAllServers<T>(string shard, Message message)
        {
            IShard dest = GetShardInstance(shard);

            if (dest != null)
            {
                ShardMulticastRequest<ResponseCollection<T>, T> multicastRequest = dest.CreateMulticastRequest<ResponseCollection<T>, T>(dest.ActiveChannelsList, message);

                IAsyncResult asyncResult = multicastRequest.BeginExecute();

                return multicastRequest.EndExecute(asyncResult);
            }

            throw new ArgumentException("Specified Shard Does not exist");
        }

        public IDictionary<String, IResponseCollection<T>> SendMessageToAllShards<T>(Message message, bool primaryOnly)
        {
            IDictionary<String, IResponseCollection<T>> responses = new HashVector<String, IResponseCollection<T>>();
            IDictionary<String, RequestAsync> asyncResults = new HashVector<String, RequestAsync>();
            foreach (String shard in _remoteShards.Keys)
            {
                IShard dest = GetShardInstance(shard);
                if (dest != null)
                {
                    try
                    {
                        if (primaryOnly)
                        {
                            ShardRequestBase<T> request = dest.CreateUnicastRequest<T>(dest.Primary, message);
                            asyncResults.Add(shard, new RequestAsync(request.BeginExecute(), request, dest.Primary));
                        }
                        else
                        {
                            ShardMulticastRequest<ResponseCollection<T>, T> multicastRequest = dest.CreateMulticastRequest<ResponseCollection<T>, T>(dest.ActiveChannelsList, message);
                            asyncResults.Add(shard, new RequestAsync(multicastRequest.BeginExecute(), multicastRequest));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

            foreach (KeyValuePair<String, RequestAsync> pair in asyncResults)
            {
                String shardName = pair.Key;
                IResponseCollection<T> shardResponse = new ResponseCollection<T>();

                if (primaryOnly)
                {
                    ShardRequestBase<T> req = pair.Value.Request;
                    IClusterResponse<T> clusterResponse = new ClusterResponse<T>(req.EndExecute(pair.Value.AsyncResult), pair.Value.Destination);
                    IList<IClusterResponse<T>> list = new List<IClusterResponse<T>>();
                    list.Add(clusterResponse);

                    shardResponse.Responses = list;
                }
                else
                {
                    ShardMulticastRequest<ResponseCollection<T>, T> req = pair.Value.Request;
                    shardResponse = req.EndExecute(pair.Value.AsyncResult);

                }

                responses.Add(shardName, shardResponse);
            }

            return responses;
        }

        class RequestAsync
        {
            private dynamic request;
            private IAsyncResult asyncResult;
            private Server destinatiion;
            public RequestAsync(IAsyncResult asyncResult, dynamic request, Server destination = null)
            {
                this.asyncResult = asyncResult;
                this.request = request;
                this.destinatiion = destination;
            }
            public IAsyncResult AsyncResult { get { return asyncResult; } }
            public dynamic Request { get { return request; } }
            public Server Destination { get { return destinatiion; } }
        }

        private IShard GetShardInstance(String shard)
        {

            IShard dest = null;

            if (this.context != null && context.LocalShardName.Equals(shard, StringComparison.OrdinalIgnoreCase))
            {
                dest = _localShard;
            }
            else if (_remoteShards != null && _remoteShards.ContainsKey(shard))
            {
                dest = _remoteShards[shard];
            }

            return dest;
        }
        
        public bool Stop()
        {
            try
            {
                if(LoggerManager.Instance.ServerLogger.IsDebugEnabled)
                    LoggerManager.Instance.ServerLogger.Debug("ClusterManager.Stop","Stopping Cluster");

                if (_connectionRestoration != null)
                    _connectionRestoration.Stop();

                if (configChangeTask != null)
                    configChangeTask.Stop();

                if (this._localShard != null)
                    _localShard.Stop();

                if (this._remoteShards != null)
                    foreach (KeyValuePair<String, IShard> shard in _remoteShards)
                        shard.Value.Stop();
                return true;
            }
            catch (Exception Ex)
            {
                if (LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ClusterManager.Stop",Ex.Message);
            }
            return false;

        }

        public void Dispose()
        {
            if (_localShard != null)
                this._localShard.Dispose();

            if (this._remoteShards != null)
                foreach (KeyValuePair<String, IShard> shard in _remoteShards)
                    shard.Value.Dispose();
            if (_connectionRestoration != null)
                _connectionRestoration.Dispose();
            if (configChangeTask != null)
                configChangeTask.Dispose();
            //this._localShard = null;
           // this._remoteShards = null;
        }

        #region IConfigurationListener Implementation

        public void OnConfigurationChanged(ConfigChangeEventArgs arguments)
        {
            ChangeType type = ChangeType.None;
            if (arguments != null)
            {
                string clusterName = arguments.GetParamValue<string>(EventParamName.ClusterName);
                if (clusterName != null && !clusterName.Equals(context.ClusterName)) return;

                type = arguments.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType);
                switch (type)
                {
                    case ChangeType.DistributionStrategyConfigured:
                    case ChangeType.DatabaseCreated:
                    case ChangeType.DatabaseDropped:
                    case ChangeType.CollectionCreated:
                    case ChangeType.CollectionMoved:
                    case ChangeType.CollectionDropped:
                    case ChangeType.ConfigRestored:
                    case ChangeType.ResyncDatabase:
                    case ChangeType.IntraShardStateTrxferCompleted:
                        if (this.configChangeListener != null)
                        {
                            configChangeListener.OnConfigurationChanged(arguments);
                        }
                        break;

                    case ChangeType.ConfigurationUpdated:
                        break;
                    case ChangeType.ShardAdded:
                        {
                            ShardInfo newShard = null;

                            if (context.ConfigurationSession != null)
                            {
                                ClusterInfo latestInfo = context.ConfigurationSession.GetDatabaseClusterInfo(arguments.GetParamValue<string>(EventParamName.ClusterName));
                                newShard = latestInfo.GetShardInfo(arguments.GetParamValue<string>(EventParamName.ShardName));
                            }

                            OnShardAdded(newShard);

                            if (this._clusterListener != null)
                                _clusterListener.OnShardAdd(newShard);
                        }
                        break;
                    case ChangeType.ShardRemovedForceful:
                        if (this._clusterListener != null)
                        {
                            ShardInfo removedShard = new ShardInfo() { Name = arguments.GetParamValue<string>(EventParamName.ShardName)};
                            _clusterListener.OnShardRemove(removedShard, false);
                        }
                        break;
                    case ChangeType.ShardRemovedGraceful:
                        if (this._clusterListener != null)
                        {
                            ShardInfo removedShard = new ShardInfo() { Name = arguments.GetParamValue<string>(EventParamName.ShardName) };
                            _clusterListener.OnShardRemove(removedShard, true);
                        }
                        break;
                    case ChangeType.DistributionChanged:
                        if (this._clusterListener != null)
                            _clusterListener.OnDistributionChanged();
                        break;
                    //write code for check if the primary has been changed for remote shard(s) connect with the new one        

                    case ChangeType.MembershipChanged:
                    case ChangeType.PrimarySelected:
                    case ChangeType.NodeJoined:

                        // AR: This should be removing the node from the restoration manager of the local shard.


                        //if (arguments.ConfigurationChangeType == ChangeType.PrimarySelected || arguments.ConfigurationChangeType == ChangeType.PrimaryGone)
                        //{
                        // ControlConfigurationChangeTask(arguments);
                        if (arguments.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType) == ChangeType.MembershipChanged)
                            HandlePrimaryChangeForRemoteshard(arguments);
                        
                        //}

                        //if (arguments.ConfigurationChangeType.Equals(ChangeType.PrimarySelected) && arguments.Membership.Primary != null || (arguments.ConfigurationChangeType.Equals(ChangeType.PrimaryGone) && arguments.Membership.Primary == null))
                        //{


                        //HandleMembershipChangeForRemoteShard();
                        //}
                        //OnMembershipChanged(arguments);
                        //HandlePrimaryChangeForRemoteshard(arguments);
                        //HandleMembershipChanged();
                        break;

                    case ChangeType.NodeAdded:
                    case ChangeType.NodeRemoved:
                    case ChangeType.PriorityChanged:
                    case ChangeType.NodeLeft:
                    case ChangeType.PrimaryGone:
                        if (_localShard != null)
                            ((LocalShard)_localShard).OnConfigurationChanged(arguments);

                        break;
                    case ChangeType.RangeUpdated:
                        _clusterListener.OnRangesUpdated();
                        break;
                    case ChangeType.NewRangeAdded:
                        _clusterListener.OnNewRangeAdded();
                        break;
                    default:
                        //write code for check if the primary has been changed for remote shard(s) connect with the new one                

                        break;


                }
            }
        }


        //private void OnMembershipChanged(ConfigChangeEventArgs args)
        //{
        //    // needs to be reviewed. Esp for the remote shard purposes.
        //    //Refactoring required.            
        //    //if (_localShard != null)
        //    //    _localShard.OnMembershipChanged(args);

        //    if (args.ConfigurationChangeType.Equals(ChangeType.PrimarySelected) && args.Membership.Primary != null || (args.ConfigurationChangeType.Equals(ChangeType.PrimaryGone) && args.Membership.Primary==null))
        //    {

        //        HandlePrimaryChangeForRemoteshard(args);

        //        //HandleMembershipChangeForRemoteShard();
        //    }

        //}

        private void HandlePrimaryChangeForRemoteshard(ConfigChangeEventArgs args)
        {
            try
            {

                if (_localShard.NodeRole == NodeRole.Primary)
                {
                    if (_remoteShards != null && _remoteShards.Count > 0)
                    {
                        string clusterName = args.GetParamValue<string>(EventParamName.ClusterName);
                        string shardName = args.GetParamValue<string>(EventParamName.ShardName);
                        if (clusterName == null || shardName == null)
                            return;
                        foreach (KeyValuePair<String, IShard> remoteShard in _remoteShards)
                        {
                            if (args != null && clusterName == this.context.ClusterName &&
                                shardName == remoteShard.Key)
                            {
                                if (remoteShard.Value != null)
                                {
                                    //if (!((RemoteShard)remoteShard.Value).IsStarted)
                                    //    remoteShard.Value.Start();
                                    //else
                                    //{
                                    if (context.ConfigurationSession != null)
                                    {
                                        //RTD: Should the dependency on CS be removed?
                                        ClusterInfo latestInfo =
                                            context.ConfigurationSession.GetDatabaseClusterInfo(context.ClusterName);
                                        ShardInfo latestShard = null;
                                        if (latestInfo != null)
                                            latestShard = latestInfo.GetShardInfo(remoteShard.Key);
                                        if (latestShard != null)
                                            ((RemoteShard)remoteShard.Value).OnPrimaryChanged(latestShard.Primary,
                                                latestShard.Port);

                                    }
                                    //}
                                    break;
                                }
                            }
                        }
                    }
                    //if (context.ConfigurationSession != null)
                    //{
                    //    ClusterInfo latestInfo = context.ConfigurationSession.GetDatabaseClusterInfo(context.ClusterName);
                    //    ShardInfo[] latestShards = latestInfo.ShardInfo;
                    //    if (latestShards.Length > 1)
                    //    {
                    //        foreach (ShardInfo info in latestShards)
                    //        {
                    //            if (info.Name != context.LocalShardName)
                    //            {
                    //                RemoteShard remoteShard = _remoteShards[info.Name] as RemoteShard;
                    //                if (remoteShard != null)
                    //                    remoteShard.OnPrimaryChanged(info.Primary, info.Port);
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                    LoggerManager.Instance.ServerLogger.Error("ClusterManager.HandlePrimaryChangedForRemoteshard() ","HandlePrimaryChangeForRemoteshard :"+ex);
            }
        }

        //private void ControlConfigurationChangeTask(ConfigChangeEventArgs args)
        //{
        //    if (args.ClusterName != null && args.ClusterName == context.ClusterName && args.ShardName != null && context.LocalShardName == args.ShardName)
        //    {
        //        if (args.ConfigurationChangeType == ChangeType.PrimarySelected)
        //        {
        //            if (args.Membership != null && args.Membership.Primary.Name.Equals(context.LocalAddress.IpAddress.ToString()))
        //            {
        //                //Start Monitoring Configuration Change on Config Server
        //                if (configChangeTask == null)
        //                {
        //                    configChangeTask = new ConfigurationChangeTask(this, context, new Common.Threading.ClrThreadPool());

        //                    Alachisoft.NoSQL.Common.Threading.IThreadPool threadPool = new Alachisoft.NoSQL.Common.Threading.ClrThreadPool();
        //                    threadPool.Initialize();
        //                    threadPool.ExecuteTask(configChangeTask);

        //                    //configChangeTask.Start();
        //                }
        //                configChangeTask.Start();
        //            }

        //        }
        //        else if (args.ConfigurationChangeType == ChangeType.PrimaryGone && ((LocalShard)_localShard).NodeRole == NodeRole.Primary)
        //        {
        //            if (configChangeTask != null)
        //                configChangeTask.Stop();
        //        }
        //    }
        //}

        public void ManageRemoteShards()
        {              
            switch (_localShard.NodeRole)
            {
                case NodeRole.Primary:

                    if (_remoteShards != null && _remoteShards.Count > 0)
                    {
                        IList<string> remoteShardKeys = _remoteShards.Keys.ToList();
                        foreach (var remoteShard in remoteShardKeys)
                        {
                            if(remoteShard == context.LocalShardName)
                                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                                    LoggerManager.Instance.ServerLogger.Info("ClusterManager.ManagerRemoteShards() ","Local shard part of the remote shards list. Problem.");

                            if (_remoteShards[remoteShard] != null && !((RemoteShard)_remoteShards[remoteShard]).IsStarted)
                                _remoteShards[remoteShard].Start();
                        }

                        //if (configChangeTask != null)
                        //    configChangeTask.Resume(); 
                    }

                    break;
                case NodeRole.None:
                case NodeRole.Secondary:

                    //if (configChangeTask != null)
                    //    configChangeTask.Pause();
                  
                    if (_remoteShards != null && _remoteShards.Count > 0)
                    {
                        
                        IList<string> remoteShardKeys = _remoteShards.Keys.ToList();
                        foreach (var remoteShard in remoteShardKeys)
                        {
                            if (_remoteShards[remoteShard] != null && ((RemoteShard)_remoteShards[remoteShard]).IsStarted)
                                _remoteShards[remoteShard].Stop();
                        }
                    }
                   
                    break;
            }
        }

        //When a new shard is added, do the following
        public void OnShardAdded(ShardInfo shard)
        {
            if(shard != null)
            {               
                if(shard.Name != context.LocalShardName && _remoteShards != null && !_remoteShards.ContainsKey(shard.Name))
                {
                    if(_clusterConfigMgr!= null)
                    {
                        _clusterConfigMgr.UpdateClusterConfiguration();
                        ShardConfiguration sConfig = _clusterConfigMgr.GetShardConfiguration(context.LocalShardName);
                        if (sConfig == null)
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsWarnEnabled)
                                LoggerManager.Instance.ShardLogger.Warn("ClusterManager.OnShardAdded() ","The shard " + shard.Name + " does not exist in the configuration.");
                            return;
                        }

                        IShard remoteShard = new RemoteShard(new DualChannelFactory(), new ShardChannelFormatter(), context, _connectionRestoration);
                        remoteShard.Initialize(sConfig);
                        remoteShard.RegisterShardListener(Common.MiscUtil.CLUSTER_MANAGER, this);
                        lock(_remoteShards)
                        {
                            _remoteShards.Add(shard.Name, remoteShard);
                        }
                    }
                }
            }
        }

        //private void HandleMembershipChangeForRemoteShard()
        //{
        //    //For Phase I we will only check for if there is any membership change in remote shards info            
        //    if (context.ConfigurationSession != null)
        //    {
        //        ClusterInfo latestInfo = context.ConfigurationSession.GetDatabaseClusterInfo(context.ClusterName);

        //        ShardInfo[] latestShards = latestInfo.ShardInfo;

        //        foreach (ShardInfo latestShard in latestShards)
        //        {
        //            if (latestShard.Name.Equals(context.LocalShardName, StringComparison.OrdinalIgnoreCase)) continue;

        //            ShardInfo oldShard = null;

        //            foreach (ShardInfo shard in this._remoteShardsInfo.Values)
        //            {
        //                if (shard.Name.Equals(latestShard.Name, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    oldShard = shard;
        //                    break;
        //                }
        //            }

        //            if (oldShard != null)
        //            {
        //                if ((oldShard.Primary == null && latestShard.Primary != null) || (oldShard.Primary != null && latestShard.Primary != null && !oldShard.Primary.Equals(latestShard.Primary)))
        //                {
        //                    RemoteShard remoteShard = _remoteShards[latestShard.Name] as RemoteShard;
        //                    if (remoteShard!=null)
        //                        remoteShard.OnPrimaryChanged(latestShard.Primary, latestShard.Port);
        //                }


        //            }

        //        }
        //    }
        //}
        #endregion

        public IList<Server> GetActiveChannelList()
        {
            return _localShard.ActiveChannelsList;
        }
        
        public bool IsShardConnected(string shardName)
        {
            if (String.IsNullOrEmpty(shardName)) return false;

            if(_localShard != null && this._localShard.Name.Equals(shardName,StringComparison.OrdinalIgnoreCase))
            {
                return _localShard.ActiveChannelsList.Contains(_localShard.Primary);
            }
            
            if (this._remoteShards != null && _remoteShards.ContainsKey(shardName))
            {
                IShard shard = _remoteShards[shardName];
                return shard.ActiveChannelsList.Contains(shard.Primary);
            }

            return false;
        }
    }
}
