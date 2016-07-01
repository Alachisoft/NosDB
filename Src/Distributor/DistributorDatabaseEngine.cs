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
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Communication.Formatters;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.RPC;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.ParseTree;
using Alachisoft.NosDB.Common.Queries.ParseTree.DCL;
using Alachisoft.NosDB.Common.Queries.ParseTree.DDL;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Queries.Util;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.SSPI;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Storage.Caching.QueryCache;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Distributor.Comparers;
using Alachisoft.NosDB.Distributor.DataCombiners;
using Alachisoft.NosDB.Distributor.DataSelectors;
using Alachisoft.NosDB.Distributor.DistributedDataSets;
using Alachisoft.NosDB.Serialization;
using Alachisoft.NosDB.Common.JSON;

using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using System.Diagnostics;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;

namespace Alachisoft.NosDB.Distributor
{
    public class DistributorDatabaseEngine : IDatabaseEngine, IConfigurationListener, IDataLoader,  IChannelDisconnectionListener

    {
        //  InProcConfigurationClient _client = new InProcConfigurationClient();
        IConfigurationSession _configurationSession;
        IChannelFactory _channelFactory;
        DatabaseOperationFormatter _formatter = new DatabaseOperationFormatter();

        IDictionary<IClientAuthenticationCredential, IServerAuthenticationCredential> clients;

        string _cluster = "";
        string _database = "";
        IClientAuthenticationCredential routerClientCredential;
        IDictionary<string, RemoteShardConnections> _shardConnections = new Dictionary<string, RemoteShardConnections>(StringComparer.InvariantCultureIgnoreCase);    // Added for storing RemoteShards (Connection to all nodes of shard)
        Dictionary<string, IterativeOperation> _dataChunkStore = new Dictionary<string, IterativeOperation>(StringComparer.InvariantCultureIgnoreCase);
        private const int _numOfRequiredDocuments = 98;
        private readonly QueryCache<IDqlObject> _reducedQueryCache = new QueryCache<IDqlObject>();
        private Dictionary<string, Dictionary<string, IDistribution>> _distributions = new Dictionary<string, Dictionary<string, IDistribution>>(StringComparer.InvariantCultureIgnoreCase);

        private readonly object _configurationChangeLock = new object();
        private ReaderWriterLock _shardConnectionsLock = new ReaderWriterLock();
        private ReaderWriterLockSlim _distributionLock = new ReaderWriterLockSlim();
        private ConnectionStringBuilder _connectionStringBuilder;


        private List<int> _retryErrorCodes = new List<int>();

        private const string DDL_GATEWAY_DB = "$sysdb";
        private bool _ddlGatewayRouter = false;
        private string _processID;

        private DatabaseMode _databaseMode;

        private bool RemoteRouter { set; get; }


  

        public DistributorDatabaseEngine() { }

        public DistributorDatabaseEngine(ConnectionStringBuilder connectionStringBuilder, bool remoteRouter = false)
        {
            _connectionStringBuilder = connectionStringBuilder;
            _channelFactory = new DualChannelFactory(_connectionStringBuilder.ConnectionTimeout);

            RemoteRouter = remoteRouter;

            AddRetryErrorCodes();

            //+security context initialization
            _clientContexts = new Dictionary<string, ClientContext>();
            _clientCredentials = new Dictionary<string, ClientCredential>();

            clients = new Dictionary<IClientAuthenticationCredential, IServerAuthenticationCredential>();

            //-security context initialization
        }

        private void AddRetryErrorCodes()
        {
            _retryErrorCodes.Add(ErrorCodes.Collection.BUCKET_UNAVAILABLE);
            _retryErrorCodes.Add(ErrorCodes.Collection.COLLECTION_DISPODED); // Node Down
            _retryErrorCodes.Add(ErrorCodes.Cluster.NOT_PRIMARY);
            _retryErrorCodes.Add(ErrorCodes.Cluster.DATABASE_MANAGER_DISPOSED);
        }

        #region IDatabaseEngine Methods

        public void Start()
        {
            try
            {
                //   RegHelper.LoadSslConfiguration();  ////Do not remove this line. Required for SSL

                RegisterCompactTypes();

                _processID = Process.GetCurrentProcess().Id.ToString();

                if (_connectionStringBuilder == null)
                    throw new DistributorException(ErrorCodes.Distributor.MISSING_CONNECTION_STRING); //Configuration exception
                _cluster = AppUtil.GetClusterName(_connectionStringBuilder.IsLocalInstance);

                routerClientCredential = new SSPIClientAuthenticationCredential();

                //Connect with active configuration server
                IConfigurationServer remote;
                _configurationSession = CSConnectionManager.Connect(_connectionStringBuilder.DataSource,
                    _connectionStringBuilder.Port, _cluster, out remote,
                    new ClientConfigurationFormatter(), routerClientCredential, RemoteRouter);

                ClusterInfo clusterInfo = GetClusterInfo(_cluster);

                if (clusterInfo == null)
                {
                    throw new DistributorException(ErrorCodes.Distributor.CLUSTER_INFO_UNAVAILABLE, new[] { _cluster });
                }
                if (clusterInfo.ShardInfo == null)
                {
                    throw new DistributorException(ErrorCodes.Distributor.SHARD_INFO_UNAVAILABLE, new string[] { clusterInfo.Name });
                }

                if (!DatabaseExists(_connectionStringBuilder.Database))
                {
                    if (DDL_GATEWAY_DB.Equals(_connectionStringBuilder.Database, StringComparison.OrdinalIgnoreCase))
                    {
                        _database = DDL_GATEWAY_DB;
                        _ddlGatewayRouter = true;
                        return;
                    }
                    throw new DistributorException(ErrorCodes.Distributor.DATABASE_DOESNOT_EXIST, new string[] { _connectionStringBuilder.Database });
                }
                _database = _connectionStringBuilder.Database;

                SetDatabaseMode(clusterInfo);

                foreach (ShardInfo shardInfo in clusterInfo.ShardInfo.Values)
                {
                    CreateRemoteShard(shardInfo);
                }

                ClusterConfiguration clusterConfiguration =
                    _configurationSession.GetDatabaseClusterConfiguration(_cluster);
                foreach (ShardConfiguration shardConfiguration in clusterConfiguration.Deployment.Shards.Values)
                {
                    if (!ShardConnectionsContain(shardConfiguration.Name))
                        continue;
                    RemoteShardConnections remoteShard = GetReceiverShard(shardConfiguration.Name);

                    remoteShard.Initialize(shardConfiguration);
                    remoteShard.SessionId = _configurationSession.SessionId;
                    remoteShard.Start();
                    AddShardConnection(shardConfiguration.Name, remoteShard);
                }

                ConfigureDistributions(clusterInfo.Databases.Values.ToArray());
                //AddOrUpdateCollectionDistributions(clusterInfo.Databases.Values.ToArray());



                _configurationSession.AddConfigurationListener(this);

                ////authentication logic goes here

                AuthenticateAll();
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (ManagementException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DistributorException(ErrorCodes.Distributor.UNKNOWN_ISSUE, null, e, new string[] { e.Message });
            }
        }


        //public void Start()
        //{
        //    try
        //    {
        //        _processID = Process.GetCurrentProcess().Id.ToString();

        //        if (_connectionStringBuilder == null)
        //            throw new DistributorException(ErrorCodes.Distributor.MISSING_CONNECTION_STRING); //Configuration exception
        //        _cluster = AppUtil.GetClusterName(_connectionStringBuilder.IsLocalInstance);

        //        string configurationServerIp = _connectionStringBuilder.DataSource;

        //        int configurationServerPort = _connectionStringBuilder.Port;
        //        IConfigurationServer remote = null;

        //        RegisterCompactTypes();

        //        DatabaseRPCService rpc = new DatabaseRPCService(configurationServerIp, configurationServerPort);

        //        routerClientCredential = new SSPIClientAuthenticationCredential();

        //        //routerClientCredential = new SSPIClientAuthenticationCredential();


        //        int retries = 3;
        //        while (retries > 0)
        //        {
        //            try
        //            {
        //                if (remote == null)
        //                {
        //                    remote = rpc.GetConfigurationServer(new TimeSpan(0, 0, 90), SessionTypes.Client, new ClientConfigurationFormatter());
        //                } if (RemoteRouter)
        //                {
        //                    remote.MarkDistributorSession();
        //                }
        //                _configurationSession = remote.OpenConfigurationSession(routerClientCredential);
        //                if (clients.ContainsKey(routerClientCredential))
        //                    clients[routerClientCredential] = ((OutProcConfigurationClient)remote).ServerAuthenticationCredenital;
        //                break;
        //            }
        //            catch (System.TimeoutException)
        //            {
        //                throw new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
        //            }
        //            catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException)
        //            {
        //                throw new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
        //            }
        //            catch (ChannelException)
        //            {
        //                retries--;
        //                if (retries == 0)
        //                    throw new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
        //                continue;
        //            }
        //        }


        //        ClusterInfo clusterInfo = GetClusterInfo(_cluster);

        //        if (clusterInfo == null)
        //        {
        //            throw new DistributorException(ErrorCodes.Distributor.CLUSTER_INFO_UNAVAILABLE);
        //        }
        //        if (clusterInfo.ShardInfo == null)
        //        {
        //            throw new DistributorException(ErrorCodes.Distributor.SHARD_INFO_UNAVAILABLE, new string[] { clusterInfo.Name });
        //        }

        //        if (!DatabaseExists(_connectionStringBuilder.Database))
        //        {
        //            if (DDL_GATEWAY_DB.Equals(_connectionStringBuilder.Database, StringComparison.OrdinalIgnoreCase))
        //            {
        //                _database = DDL_GATEWAY_DB;
        //                _ddlGatewayRouter = true;
        //                return;
        //            }
        //            throw new DistributorException(ErrorCodes.Distributor.DATABASE_DOESNOT_EXIST, new string[] { _connectionStringBuilder.Database });
        //        }
        //        _database = _connectionStringBuilder.Database;

        //        SetDatabaseMode(clusterInfo);

        //        foreach (ShardInfo shardInfo in clusterInfo.ShardInfo.Values)
        //        {
        //            CreateRemoteShard(shardInfo);
        //        }

        //        ClusterConfiguration clusterConfiguration =
        //            _configurationSession.GetDatabaseClusterConfiguration(_cluster);
        //        foreach (ShardConfiguration shardConfiguration in clusterConfiguration.Deployment.Shards.Values)
        //        {
        //            if (!ShardConnectionsContain(shardConfiguration.Name))
        //                continue;
        //            RemoteShardConnections remoteShard = GetReceiverShard(shardConfiguration.Name);

        //            remoteShard.Initialize(shardConfiguration);
        //            remoteShard.SessionId = _configurationSession.SessionId;
        //            remoteShard.Start();
        //            AddShardConnection(shardConfiguration.Name, remoteShard);
        //        }

        //        ConfigureDistributions(clusterInfo.Databases.Values.ToArray());

        //        _configurationSession.AddConfigurationListener(this);

        //        ////authentication logic goes here

    

        //        AuthenticateAll();

        //    }
        //    catch (DatabaseException)
        //    {
        //        throw;
        //    }
        //    catch (ManagementException)
        //    {
        //        throw;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new DistributorException(ErrorCodes.Distributor.UNKNOWN_ISSUE, null, e, new string[] { e.Message });
        //    }
        //}

        private void CreateRemoteShard(ShardInfo shardInfo)
        {
            var routerShardConnections = new RemoteShardConnections(_channelFactory,
                        _formatter, shardInfo, null, SessionTypes.Client);
            //Start it later once the shardConfigurations are initialized. -- Doing that below this loop
            AddShardConnection(shardInfo.Name, routerShardConnections);
            routerShardConnections.RegisterDisconnectionListener(this);
        }


        private void SetDatabaseMode(ClusterInfo clusterInfo)
        {
            if (!clusterInfo.ContainsDatabase(_database))
                throw new DistributorException(ErrorCodes.Distributor.DATABASE_DOESNOT_EXIST, new[] { _database });

            DatabaseInfo databaseInfo = clusterInfo.Databases[_database];
            _databaseMode = databaseInfo.Mode;
            switch (_databaseMode)
            {
                case DatabaseMode.Offline:
                    throw new DistributorException(ErrorCodes.Database.Mode,
                        new[] { _database, _databaseMode.ToString() });
            }
        }

        public void Stop()
        {
            string configSession = System.Configuration.ConfigurationManager.AppSettings["ConfigSession"];
            //if (configSession.Equals("InProc"))
            //{
            //    _client.Disconnect();
            //}
            //else
            //{
            _configurationSession.RemoveConfigurationListener(this);
            _configurationSession.Close();
            // }

            _shardConnectionsLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (KeyValuePair<string, RemoteShardConnections> shard in _shardConnections)
                {
                    RemoteShardConnections routerShardConnection = shard.Value;
                    routerShardConnection.UnregisterDisconnectionListener();
                    routerShardConnection.Dispose();

                }
            }
            finally
            {
                _shardConnectionsLock.ReleaseReaderLock();
            }
        }

        public object ExecuteScalar(IQueryOperation operation)
        {
            ICollection projections = null;

            //add session id assigned by configuration server
            operation.SessionId = _configurationSession.SessionId;

            IQueryResponse queryResponse = this.ExecuteQuery(operation, out projections);

            if (projections != null)
            {
                ICollectionReader reader = null;
                try
                {
                    queryResponse.DataChunk.IsLastChunk = true;
                    reader = new CollectionReader((DataChunk)queryResponse.DataChunk, this, _connectionStringBuilder.Database.ToLower(), operation.Collection);
                    if (reader.ReadNext())
                    {
                        if (((IList)projections)[0] is AllEvaluable)
                        {
                            return reader.GetDocument()[((IList)reader.GetDocument().GetAttributes())[0].ToString()];
                        }
                        return reader[((IList)projections)[0].ToString()];
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Dispose();
                }
            }
            return null;
        }

        public IQueryResponse ExecuteReader(IQueryOperation operation)
        {
            PerformValidation(operation);
            //add session id assigned by configuration server
            operation.SessionId = _configurationSession.SessionId;

            ICollection projections = null;
            IQueryResponse response = (IQueryResponse)operation.CreateResponse();
            response.IsSuccessfull = false;
            for (int i = 0; i < 4; i++) //3 retries
            {
                try
                {
                    response = ExecuteQuery(operation, out projections);
                    if (response.IsSuccessfull)
                        return response;
                }
                catch (Exception)
                {
                    if (i == 3)
                        throw;
                    response.IsSuccessfull = false;
                }
            }
            return response;
        }

        //Marked for restructuring because a query will no longer of the type of DML only.
        private IQueryResponse ExecuteQuery(IQueryOperation operation, out ICollection projections)
        {
            IQueryResponse queryOperationResponse = (IQueryResponse)operation.CreateResponse();
            string formedQuery = MiscUtil.GetQueryString(operation.Query);

            IDqlObject parsedObject = _reducedQueryCache.GetParsedQuery(operation.Query.QueryText);

            if (!(parsedObject is SelectObject))
            {
                throw new DistributorException(ErrorCodes.Distributor.NOT_SELECT_QUERY, new[] { operation.Query.QueryText });
            }

            if (string.IsNullOrEmpty(operation.Collection))
            {
                operation.Collection = ((SelectObject)parsedObject).Collection.Trim().ToLower();
            }

            IDistribution distribution = GetDistribution(operation.Database, operation.Collection);
            var shards = distribution.GetShards();
            Dictionary<string, IAsyncResult> asyncResultForGettingResponse = new Dictionary<string, IAsyncResult>();

            foreach (var shard in shards)
            {
                RemoteShardConnections receiverShard = GetReceiverShard(shard);

                asyncResultForGettingResponse[shard] = SendReadOperationToPrimary(operation, receiverShard, true);
            }

            List<ISet> sets = new List<ISet>();
            foreach (KeyValuePair<string, IAsyncResult> item in asyncResultForGettingResponse)
            {
                string shardName = item.Key;
                IAsyncResult asyncResult = item.Value;
                IQueryResponse queryResponse = null;
                RemoteShardConnections receiverShard = GetReceiverShard(shardName);

                Server address = new Server(((RequestManager.AsyncResult)asyncResult).ChannelAddress, Status.Running);
                try
                {
                    queryResponse = (IQueryResponse)receiverShard.EndSendMessage(address, asyncResult);
                }
                catch (SocketException)
                {
                    // TODO: Error code must be on the basis of each individual key rather than overall
                    queryResponse.IsSuccessfull = false;
                    queryResponse.ErrorCode = ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING;
                    queryResponse.ErrorParams = new string[] { address.Address.ToString() };
                    continue;
                }

                if (!queryResponse.IsSuccessfull)
                {
                    projections = null;
                    return queryResponse;
                }

                ISet set = new DistributedDataSet(queryResponse.DataChunk, receiverShard.Name, operation, this, address);
                sets.Add(set);
            }

            IDataSelector dataSelector = null;
            List<IDataCombiner> dataCombiners = null;
            IComparer comparer = null;

            MakeQueryDecisions(parsedObject, ref dataCombiners, ref dataSelector, ref comparer, out projections);

            IterativeOperation iterativeOperation = new IterativeOperation(sets, dataCombiners, dataSelector, comparer);

            IDataChunk dataChunk = iterativeOperation.GetDataChunk(-1, _numOfRequiredDocuments);    // Always -1 in case of first request

            queryOperationResponse.DataChunk = dataChunk;
            queryOperationResponse.DataChunk.ReaderUID = "-1";
            queryOperationResponse.IsSuccessfull = true;
            if (dataChunk.IsLastChunk) return queryOperationResponse;
            string uniqueKey = AutoGenerateUniqueString();
            _dataChunkStore[uniqueKey] = iterativeOperation;
            queryOperationResponse.DataChunk.ReaderUID = uniqueKey;
            return queryOperationResponse;
        }

        //Todo: Mutually exclusive operations' exception handling.
        public IUpdateResponse ExecuteNonQuery(INonQueryOperation operation)
        {
            bool successful;
            PerformValidation(operation);

            IUpdateResponse response = (IUpdateResponse)operation.CreateResponse();
            IDqlObject parsedObject = _reducedQueryCache.GetParsedQuery(operation.Query.QueryText);

            if (parsedObject is SelectObject)
            {
                response.ErrorCode = ErrorCodes.Query.INVALID_NON_QUERY_TYPE;
                response.AffectedDocuments = 0;
                return response;
            }

            if (!(parsedObject is IList<IDqlObject>))
            {
                long affectedDocs;
                int errorCode;
                string[] errorParams = new string[0];

                if (!ExecuteQueryObject(operation, parsedObject, out affectedDocs,
                    out errorCode, out errorParams, out successful))
                {
                    response.ErrorCode = errorCode;
                    response.ErrorParams = errorParams;
                    response.IsSuccessfull = successful;
                    return response;
                }

                response.IsSuccessfull = successful;
                response.AffectedDocuments = affectedDocs;
                return response;
            }

            var dbExceptions = new List<KeyValuePair<int, string[]>>();

            foreach (var queryObject in ((IList<IDqlObject>)parsedObject))
            {
                try
                {
                    int errorCode;
                    long affectedDocs;
                    string[] errorParams = new string[0];

                    if (!ExecuteQueryObject(operation, queryObject, out affectedDocs,
                        out errorCode, out errorParams, out successful))
                    {
                        dbExceptions.Add(
                            new KeyValuePair<int, string[]>(response.ErrorCode, response.ErrorParams));
                    }

                    response.AffectedDocuments = (affectedDocs == -1) ? affectedDocs :
                        (response.AffectedDocuments + affectedDocs);
                }
                catch (DatabaseException ex)
                {
                    dbExceptions.Add(new KeyValuePair<int, string[]>(ex.ErrorCode, ex.Parameters));
                }
            }

            if (dbExceptions.Count == 0)
            {
                response.IsSuccessfull = true;
                return response;
            }

            response.ErrorCode = dbExceptions[0].Key;
            response.ErrorParams = dbExceptions[0].Value;
            return response;
        }

        #region DQL private region

        private bool ExecuteQueryObject(INonQueryOperation operation, IDqlObject parsedObject,
            out long affectedDocs, out int errorCode, out string[] errorParams, out bool successful)
        {
            successful = true;
            affectedDocs = 0;
            errorCode = 0;
            errorParams = new string[0];

            if (parsedObject is IDmObject)
            {
                if (_ddlGatewayRouter)
                    throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);

                if (!ExecuteDataManipulationObject(operation, (IDmObject)parsedObject,
                    out affectedDocs, out errorCode, out errorParams))
                {
                    successful = false;
                    return false;
                }
            }
            else if (parsedObject is IDdObject)
            {
                if (!IsShardExist(_configurationSession))
                    throw new DatabaseException(ErrorCodes.Distributor.NO_SHARD_EXIST);
                ExecuteDataDefinitionObject(parsedObject as DataDefinitionObject, operation.Database);
                affectedDocs = -1;
            }
            else if (parsedObject is IDcObject)
            {
                if (!IsShardExist(_configurationSession))
                    throw new DatabaseException(ErrorCodes.Distributor.NO_SHARD_EXIST);
                ExecuteDataControlObject(parsedObject as DataControlObject);
                affectedDocs = -1;
            }
            return true;
        }

        private bool ExecuteDataManipulationObject(INonQueryOperation operation,
            IDmObject manipulationObject1, out long affectedDocs, out int errorCode, out string[] errorParams)
        {
            bool routeRequest = false;
            string key = "";
            affectedDocs = 0;
            errorCode = 0;
            errorParams = new string[0];
            IDmObject dmlObject = manipulationObject1; // we don't need to clone every object

            //Generate DocumentKEY on Router
            InsertObject insertObject = dmlObject as InsertObject;
            if (insertObject != null)
            {
                if (!IsValidDocumentKey(insertObject.ValuesToInsert, operation.Query.Parameters))
                {
                    insertObject = (InsertObject)insertObject.Clone();
                    key = GenerateDocumentKey();
                    insertObject.AddDocumentKey(key);
                    dmlObject = insertObject;
                }
                routeRequest = true;
            }
            operation.Query.QueryText = dmlObject.InString;

            if (string.IsNullOrEmpty(operation.Collection))
            {
                operation.Collection = dmlObject.Collection.Trim().ToLower();
            }

            if (!(_distributions[operation.Database].ContainsKey(dmlObject.Collection.Trim().ToLower())))
            {
                throw new DistributorException(ErrorCodes.Distributor.COLLECTION_DOES_NOT_EXIST,
                    new[] { operation.Collection, operation.Database });
            }

            Dictionary<string, IAsyncResult> shardResponses = new Dictionary<string, IAsyncResult>();

            _shardConnectionsLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                List<String> shards = null;
                if (routeRequest)
                {
                    IDistributionRouter distributionRouter = GetDistributionRouter(operation.Database, operation.Collection);
                    shards = new List<string> { distributionRouter.GetShardForDocument(new DocumentKey(key)) };
                }
                else
                {
                    shards = GetDistribution(operation.Database, operation.Collection).GetShards();
                }
                foreach (var shardName in shards)
                {
                    RemoteShardConnections shard = GetReceiverShard(shardName);
                    // Assign Documents list for sending to the shard
                    if (shard.ShardInfo.IsReadOnly)
                    {
                        errorCode = ErrorCodes.Distributor.SHARD_READ_ONLY;
                        return false;
                    }

                    if (shard.ShardInfo.Primary == null)
                    {
                        errorCode = ErrorCodes.Distributor.PRIMARY_DOESNOT_EXIST;
                        errorParams = new string[] { shard.ShardInfo.Name };

                        return false;
                    }
                }
                foreach (var shardName in shards)
                {
                    // Assign Documents list for sending to the shard
                    RemoteShardConnections shard = GetReceiverShard(shardName);
                    shardResponses[shardName] =
                        shard.BeginSendMessage(shard.Primary, operation);
                }
            }
            finally
            {
                _shardConnectionsLock.ReleaseReaderLock();
            }
            foreach (KeyValuePair<string, IAsyncResult> nameResult in shardResponses)
            {
                RemoteShardConnections receiverShard = GetReceiverShard(nameResult.Key);
                if (!ShardConnectionsContain(nameResult.Key))
                {
                    continue;
                }

                IUpdateResponse updateResponse = (IUpdateResponse)
                    receiverShard.EndSendMessage(receiverShard.Primary, nameResult.Value);

                if (!updateResponse.IsSuccessfull)
                {
                    errorCode = updateResponse.ErrorCode;
                    errorParams = updateResponse.ErrorParams;
                    affectedDocs = 0;
                    return false;
                }
                affectedDocs += updateResponse.AffectedDocuments;
            }
            return true;
        }

        private void ExecuteDataDefinitionObject(DataDefinitionObject definitionObject, string database)
        {
            ICloneable configurationObject;
            Dictionary<string, object> confValues;

            if (!DQLHelper.ValidateDDLArguments(_configurationSession, _cluster, definitionObject.DefinitionType, definitionObject.DBObjectType,
                definitionObject.Configuration, out configurationObject, out confValues))
            {
                throw new QuerySystemException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION_JSON);
            }

            switch (definitionObject.DBObjectType)
            {
                case DbObjectType.Database:
                    switch (definitionObject.DefinitionType)
                    {
                        case DataDefinitionType.Create:
                            _configurationSession.CreateDatabase(_cluster,
                                (DatabaseConfiguration)configurationObject);
                            break;

                        case DataDefinitionType.Alter:
                            _configurationSession.UpdateDatabaseConfiguration(_cluster, ((DatabaseConfiguration)configurationObject).Name,
                               (DatabaseConfiguration)configurationObject);
                            break;

                        case DataDefinitionType.Drop:
                            _configurationSession.DropDatabase(_cluster,
                                (string)confValues[ConfigType.Name.ToString().ToLower()], true);
                            break;

                        case DataDefinitionType.Backup:
                            var backupConfiguration = configurationObject as RecoveryConfiguration;
                            backupConfiguration.Cluster = _cluster;
                            _configurationSession.SubmitRecoveryJob(backupConfiguration);
                            break;
                        case DataDefinitionType.Restore:
                            var restoreConfiguration = configurationObject as RecoveryConfiguration;
                            restoreConfiguration.Cluster = _cluster;
                            _configurationSession.SubmitRecoveryJob(restoreConfiguration);
                            break;
                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                    }
                    break;

                case DbObjectType.Collection:

                    string databaseName = "";
                    if (confValues.ContainsKey("database"))
                    {
                        databaseName = (string)confValues[ConfigType.Database.ToString().ToLower()];
                    }
                    else
                        databaseName = database;

                    switch (definitionObject.DefinitionType)
                    {
                        case DataDefinitionType.Create:
                            _configurationSession.CreateCollection(_cluster,
                                databaseName,
                                (CollectionConfiguration)configurationObject);
                            break;

                        case DataDefinitionType.Alter:
                            _configurationSession.UpdateCollectionConfiguration(_cluster,
                              databaseName, ((CollectionConfiguration)configurationObject).CollectionName,
                              (CollectionConfiguration)configurationObject);
                            break;

                        case DataDefinitionType.Drop:
                            _configurationSession.DropCollection(_cluster,
                                databaseName,
                                (string)confValues[ConfigType.Name.ToString().ToLower()]);
                            break;

                        case DataDefinitionType.Truncate:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        //_configurationSession.TruncateCollection("MeraCluster", "MeriDB", "MeriCollection");
                        //break;
                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                    }
                    break;

                case DbObjectType.Index:
                    switch (definitionObject.DefinitionType)
                    {
                        case DataDefinitionType.Create:
                            _configurationSession.CreateIndex(_cluster,
                                (string)confValues[ConfigType.Database.ToString().ToLower()],
                                (string)confValues[ConfigType.Collection.ToString().ToLower()],
                                (IndexConfiguration)configurationObject);
                            break;

                        case DataDefinitionType.Alter:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);

                        case DataDefinitionType.Drop:
                            _configurationSession.DropIndex(_cluster,
                                (string)confValues[ConfigType.Database.ToString().ToLower()],
                                (string)confValues[ConfigType.Collection.ToString().ToLower()],
                                (string)confValues[ConfigType.Name.ToString().ToLower()]);
                            break;

                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                    }
                    break;
                    
                case DbObjectType.Role:
                    throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);                   
                    break;

                case DbObjectType.User:
                    throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                //switch (definitionObject.DefinitionType)
                //{
                //    case DataDefinitionType.Create:
                //        _configurationSession.CreateUser((IUser)configurationObject);
                //        break;

                //    case DataDefinitionType.Alter:
                //        _configurationSession.AlterUser((IUser)configurationObject,
                //            (string)confValues[ConfigType.NewPassword.ToString().ToLower()]);
                //        break;

                //    case DataDefinitionType.Drop:
                //        _configurationSession.CreateUser((IUser)configurationObject);
                //        break;
                //}
                //break;
                case DbObjectType.Login:
                    switch (definitionObject.DefinitionType)
                    {
                        //TODO change CreateUser to CreateLogin
                        case DataDefinitionType.Create:
                            _configurationSession.CreateUser((IUser)configurationObject);
                            break;

                        case DataDefinitionType.Drop:
                            _configurationSession.DropUser((IUser)configurationObject);
                            break;
                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                    }
                    break;
                //case DbObjectType.Backup:
                //switch (definitionObject.DefinitionType)
                //{
                //    case DataDefinitionType.Backup:
                //        _configurationSession.SubmitRecoveryJob((RecoveryConfiguration)configurationObject);
                //        break;

                //    default:
                //        throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                //}
                //break;
                //case DbObjectType.Restore:
                //switch (definitionObject.DefinitionType)
                //{
                //    case DataDefinitionType.Restore:
                //        _configurationSession.SubmitRecoveryJob((RecoveryConfiguration)configurationObject);
                //        break;

                //    default:
                //        throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                //}
                //break;
                default:
                    throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);

            }
        }

        private void ExecuteDataControlObject(DataControlObject controlObject)
        {
            switch (controlObject.Type)
            {
                case ControlType.Grant:
                    _configurationSession.Grant(_cluster, controlObject.ResourceIdentifier,
                        controlObject.UserName, controlObject.RoleName);
                    break;
                case ControlType.Revoke:
                    _configurationSession.Revoke(_cluster, controlObject.ResourceIdentifier,
                        controlObject.UserName, controlObject.RoleName);
                    break;
            }
        }

        #endregion

      
        private IAsyncResult SendReadOperationToPrimary(IReadOperation readOperation, RemoteShardConnections receiverShard, bool retryOnFailure)
        {
            try
            {
                return receiverShard.BeginSendMessage(receiverShard.Primary, readOperation);
            }
            catch (Exception)
            {
                throw new DistributorException(ErrorCodes.Distributor.FAILED_TO_COMMUNICATE_WITH_SHARD, new string[] { receiverShard.Name });
            }
        }

        public IGetResponse GetDocuments(IGetOperation operation)
        {
            PerformValidation(operation);
            IGetResponse response = null;
            for (int i = 0; i < 4; i++)     //3 retries
            {
                try
                {
                    response = GetDocumentsHelper(operation);
                    if (response.IsSuccessfull)
                        return response;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());

                    if (i == 3)
                        throw;
                }
            }
            return response;
        }

        public IGetResponse GetDocumentsHelper(IGetOperation operation)
        {
            IDictionary foundItems = null;
            

            IGetResponse getOperationResponse = (IGetResponse)operation.CreateResponse();
            getOperationResponse.IsSuccessfull = true;
            List<ISet> sets = new List<ISet>();

            if (operation.DocumentIds.Count > 0)
            {
                IDistributionRouter distributionRouter = GetDistributionRouter(operation.Database, operation.Collection);

                Dictionary<string, List<IJSONDocument>> documentRoute = new Dictionary<string, List<IJSONDocument>>();
                foreach (IJSONDocument jDoc in operation.DocumentIds)
                {
                    if (jDoc.Key == null)
                    {
                        MarkToBroadcast(documentRoute, jDoc);
                    }
                    else
                    {
                        try
                        {
                            string shardName = distributionRouter.GetShardForDocument(new DocumentKey(jDoc.Key)); //Expecting this to throw an exception if the shard doesnot exist against the provided documentkey
                            AssignReceiverShardToDocument(documentRoute, shardName, jDoc);
                        }
                        catch (Exception)
                        {
                            //We don't tell user about the failed keys explicitly in case of Get operations as Reader is returned
                        }
                    }
                }

                IGetOperation getOperation = new GetDocumentsOperation();
                getOperation.Database = operation.Database;
                getOperation.Collection = operation.Collection;
                getOperation.RequestId = operation.RequestId;
                getOperation.Context = operation.Context;
                getOperation.OperationType = operation.OperationType;
                getOperation.SessionId = operation.SessionId;

                Dictionary<string, IAsyncResult> asyncResultForGettingResponse = new Dictionary<string, IAsyncResult>();
                foreach (KeyValuePair<string, List<IJSONDocument>> entry in documentRoute)
                {
                    getOperation.DocumentIds = entry.Value;
                    string shardName = entry.Key;

                    RemoteShardConnections receiverShard = GetReceiverShard(shardName);
                    asyncResultForGettingResponse[shardName] = SendReadOperationToPrimary(getOperation, receiverShard, true);
                }

                foreach (KeyValuePair<string, IAsyncResult> item in asyncResultForGettingResponse)
                {
                    string shardName = item.Key;
                    IAsyncResult asyncResult = item.Value;

                    if (!ShardConnectionsContain(shardName))
                    {
                        foreach (var doc in documentRoute[shardName])
                        {
                            Console.WriteLine("This behavior needs to be changed! Failed to get doc:'" + doc.Key + "' because shard '" + shardName + "' was removed");
                            //FailedDocument failedDocument = new FailedDocument();
                            //failedDocument.DocumentKey = doc.Key;
                            //failedDocument.ErrorCode = ErrorCodes.Distributor.UNEXPECTED_SHARD_DOESNOT_EXIST;
                            //failedDocument.ErrorParameters = new string[] { shardName };
                            //failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(failedDocument.ErrorCode, failedDocument.ErrorParameters);
                            //getOperationResponse.AddFailedDocument(failedDocument); TODO: Get operation shoud return Error Code for every individual document
                        }
                        Console.WriteLine("Failed to get " + documentRoute[shardName].Count + " documents");
                        continue;
                    }
                    RemoteShardConnections receiverShard = GetReceiverShard(shardName);
                    IGetResponse getResponse;
                    Server address = new Server(((RequestManager.AsyncResult)asyncResult).ChannelAddress, Status.Running);
                    try
                    {
                        getResponse = (IGetResponse)receiverShard.EndSendMessage(address, asyncResult);
                    }
                    catch (SocketException)
                    {
                        // TODO: Error code must be on the basis of each individual key rather than overall
                        getOperationResponse.IsSuccessfull = false;
                        getOperationResponse.ErrorCode = ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING;
                        getOperationResponse.ErrorParams = new[] { address.Address.ToString() };
                        continue;
                    }

                    if (!getResponse.IsSuccessfull)
                    {
                        return getResponse;
                    }

                    ISet set = new DistributedDataSet(getResponse.DataChunk, receiverShard.Name, operation, this, address);
                    sets.Add(set);
                }
            }
            IterativeOperation iterativeOperation = new IterativeOperation(sets, null, new DataSelectorRoundRobin(), new JsonDocumentComparer(null));

            IDataChunk dataChunk = iterativeOperation.GetDataChunk(-1, _numOfRequiredDocuments);    // Always -1 in case of first request


            if (foundItems != null)
            {
                //Add Cache Found Items to the result set.
                foreach (IJSONDocument doc in foundItems.Values)
                {
                    dataChunk.Documents.Add(doc);
                }
            }

            getOperationResponse.DataChunk = dataChunk;
            getOperationResponse.DataChunk.ReaderUID = "-1";
            if (dataChunk.IsLastChunk) return getOperationResponse;
            string uniqueKey = AutoGenerateUniqueString();
            _dataChunkStore[uniqueKey] = iterativeOperation;
            getOperationResponse.DataChunk.ReaderUID = uniqueKey;
            return getOperationResponse;
        }

        public IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation)
        {
            return PerformWriteOperation(operation);
        }

        public IDocumentsWriteResponse InsertDocuments(IDocumentsWriteOperation operation)
        {

            return  PerformWriteOperation(operation);

        }

        public IUpdateResponse UpdateDocuments(IUpdateOperation operation)
        {
            throw new NotImplementedException();
        }

        public IDocumentsWriteResponse DeleteDocuments(IDocumentsWriteOperation operation)
        {
            return PerformWriteOperation(operation);
        }

        private void PrepareFailedResponse(IDocumentsWriteResponse response, int errorCode, IList<IJSONDocument> documents, string[] errorParams)
        {
            response.IsSuccessfull = false;
            foreach (IJSONDocument doc in documents)
            {
                FailedDocument failedDocument = new FailedDocument();
                failedDocument.DocumentKey = doc.Key;
                failedDocument.ErrorCode = errorCode;
                failedDocument.ErrorMessage = ErrorMessages.GetErrorMessage(errorCode, errorParams);
                failedDocument.ErrorParameters = errorParams;
                response.AddFailedDocument(failedDocument);

            }
        }

        private bool NeedRetry(int errcode)
        {
            if (_retryErrorCodes.Contains(errcode))
                return true;
            return false;
        }

        private IDocumentsWriteResponse PerformWriteOperation(IDocumentsWriteOperation operation)
        {
            PerformValidation(operation);
            Dictionary<string, List<IJSONDocument>> documentRoute = new Dictionary<string, List<IJSONDocument>>();

            IDocumentsWriteResponse clientResponse = operation.CreateResponse() as IDocumentsWriteResponse;

            IDocumentsWriteResponse response = PerformWriteOperationHelper(operation, documentRoute);

            #region If Operation Fails due to State Transfer retry 3 times
            for (int i = 0; i < 3; i++)
            {
                if (response.IsSuccessfull != clientResponse.IsSuccessfull)
                    clientResponse.IsSuccessfull = false;

                if (response.IsSuccessfull == false && response.FailedDocumentsList != null && response.FailedDocumentsList.Count > 0)
                {
                    HashSet<string> retryDocumentsKeys = new HashSet<string>();
                    foreach (var doc in response.FailedDocumentsList)
                    {
                        if (NeedRetry(doc.ErrorCode))
                        {
                            retryDocumentsKeys.Add(doc.DocumentKey);
                        }
                        else
                        {
                            clientResponse.AddFailedDocument(doc);
                        }
                    }

                    if (retryDocumentsKeys.Count > 0)
                    {
                        //to retry we will get latest distribution
                        ConfigChangeEventArgs eventArgs = new ConfigChangeEventArgs(_cluster, ChangeType.DistributionChanged);
                        eventArgs.SetParamValue(EventParamName.DatabaseName, operation.Database);
                        eventArgs.SetParamValue(EventParamName.CollectionName, operation.Collection);

                        OnConfigurationChanged(eventArgs);

                        List<IJSONDocument> retryDocuments = new List<IJSONDocument>();
                        foreach (KeyValuePair<string, List<IJSONDocument>> shardDocuments in documentRoute)
                        {
                            foreach (var document in shardDocuments.Value)
                            {
                                if (retryDocumentsKeys.Contains(document.Key))
                                {
                                    retryDocuments.Add(document);
                                }
                            }
                        }
                        operation.Documents = retryDocuments;
                        documentRoute.Clear();
                        response = PerformWriteOperationHelper(operation, documentRoute);
                    }
                    else
                    {
                        response.FailedDocumentsList.Clear();
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            #endregion

            if (response.FailedDocumentsList != null && response.FailedDocumentsList.Count > 0)
            {
                response.IsSuccessfull = false;
                foreach (var doc in response.FailedDocumentsList)
                {
                    clientResponse.AddFailedDocument(doc);
                }
            }

            if (clientResponse.FailedDocumentsList != null && clientResponse.FailedDocumentsList.Count > 0)
            {
                clientResponse.IsSuccessfull = false;
            }
            else
            {
                clientResponse.IsSuccessfull = true;
            }
            return clientResponse;
        }

        private IDocumentsWriteResponse PerformWriteOperationHelper(IDocumentsWriteOperation operation, Dictionary<string, List<IJSONDocument>> documentRoute)
        {
            IDistributionRouter distributionRouter = GetDistributionRouter(operation.Database, operation.Collection);

            IDocumentsWriteResponse clientResponse = operation.CreateResponse() as IDocumentsWriteResponse;

            foreach (IJSONDocument jDoc in operation.Documents)
            {
                if (string.IsNullOrEmpty(jDoc.Key))
                {
                    if (operation is InsertDocumentsOperation)
                        jDoc.Key = GenerateDocumentKey();
                    else
                        throw new DistributorException(ErrorCodes.Distributor.INVALID_DOCUMENT_KEY);
                }

                try
                {
                    string shardName = distributionRouter.GetShardForDocument(new DocumentKey(jDoc.Key));
                    AssignReceiverShardToDocument(documentRoute, shardName, jDoc);
                }
                catch (Exception)
                {
                    PrepareFailedResponse(clientResponse, ErrorCodes.Distributor.INVALID_DOCUMENT_KEY, new[] { jDoc }, null);
                }
            }
            Dictionary<string, IAsyncResult> asyncResultForGettingResponse = new Dictionary<string, IAsyncResult>();
            foreach (KeyValuePair<string, List<IJSONDocument>> entry in documentRoute)
            {
                string shardName = entry.Key;
                operation.Documents = entry.Value; // Assign Documents list for sending to the shard

                RemoteShardConnections receiverShard = GetReceiverShard(shardName);

                if (receiverShard.ShardInfo.IsReadOnly || receiverShard.Primary == null)
                {
                    clientResponse.IsSuccessfull = false;
                    int errorCode = receiverShard.Primary == null
                        ? ErrorCodes.Distributor.PRIMARY_DOESNOT_EXIST
                        : ErrorCodes.Distributor.SHARD_READ_ONLY;
                    PrepareFailedResponse(clientResponse, errorCode, operation.Documents,
                        new string[] { receiverShard.Name });
                    continue;
                }

                try
                {
                    asyncResultForGettingResponse[shardName] = receiverShard.BeginSendMessage(
                        receiverShard.Primary, operation);
                }
                catch (DatabaseException dbe)
                {
                    PrepareFailedResponse(clientResponse, dbe.ErrorCode, operation.Documents, dbe.Parameters);
                }
                catch (Exception e)
                {
                    PrepareFailedResponse(clientResponse, ErrorCodes.Distributor.UNKNOWN_CHANNEL_SEND_ISSUE,
                        operation.Documents, new string[] { receiverShard.Name });
                }
            }

            foreach (KeyValuePair<string, IAsyncResult> item in asyncResultForGettingResponse)
            {
                string shardName = item.Key;
                IAsyncResult asyncResult = item.Value;

                if (!ShardConnectionsContain(shardName))
                {
                    PrepareFailedResponse(clientResponse, ErrorCodes.Distributor.UNEXPECTED_SHARD_DOESNOT_EXIST,
                        documentRoute[shardName], new string[] { shardName });
                    continue;
                }

                RemoteShardConnections receiverShard = GetReceiverShard(shardName);

                IDocumentsWriteResponse shardResponse;
                try
                {
                    shardResponse =
                        (IDocumentsWriteResponse)receiverShard.EndSendMessage(receiverShard.Primary, asyncResult);
                }
                catch (SocketException)
                {
                    PrepareFailedResponse(clientResponse, ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING,
                        documentRoute[shardName], new string[] { receiverShard.Primary.Address.ToString() });
                    continue;
                }
                if (shardResponse.IsSuccessfull)
                {
                    continue;
                }

                if (shardResponse.FailedDocumentsList == null || shardResponse.FailedDocumentsList.Count == 0)
                {
                    PrepareFailedResponse(clientResponse, shardResponse.ErrorCode, documentRoute[shardName],
                        shardResponse.ErrorParams);
                }
                else
                {
                    clientResponse.IsSuccessfull = false;
                    foreach (var doc in shardResponse.FailedDocumentsList)
                        clientResponse.AddFailedDocument(doc); //setting error code
                }
            }

            return clientResponse;
        }


        public IDBResponse CreateCollection(ICreateCollectionOperation operation)
        {
            PerformValidation(operation);

            throw new NotImplementedException();
        }

        public IDBResponse DropCollection(IDropCollectionOperation operation)
        {
            PerformValidation(operation);
            throw new NotImplementedException();
        }

        public IDBResponse CreateIndex(ICreateIndexOperation operation)
        {
            PerformValidation(operation);
            throw new NotImplementedException();
        }

        public IDBResponse DropIndex(IDropIndexOperation operation)
        {
            PerformValidation(operation);
            throw new NotImplementedException();
        }

        public IGetChunkResponse GetDataChunk(IGetChunkOperation operation)
        {
            PerformValidation(operation);
            if (string.IsNullOrEmpty(operation.ReaderUID)) { throw new DistributorException(ErrorCodes.Distributor.INVALID_READER_ID); }

            string clientReaderId = operation.ReaderUID;
            int clientChunkId = operation.LastChunkId;

            if (!_dataChunkStore.ContainsKey(operation.ReaderUID))
                throw new DistributorException(ErrorCodes.Distributor.INVALID_READER_ID);
            IterativeOperation iterativeOperation = _dataChunkStore[operation.ReaderUID];

            IDataChunk dataChunk = iterativeOperation.GetDataChunk(clientChunkId, _numOfRequiredDocuments);

            IGetChunkResponse getChunkResponse = (IGetChunkResponse)operation.CreateResponse();
            getChunkResponse.DataChunk = dataChunk;
            getChunkResponse.DataChunk.ReaderUID = clientReaderId;
            getChunkResponse.IsSuccessfull = true;
            return getChunkResponse;
            //getChunkResponse   see later if other attributes required to set or not ---
        }

        public IDBResponse DiposeReader(IDiposeReaderOperation operation)
        {
            PerformValidation(operation);
            if (_dataChunkStore.ContainsKey(operation.ReaderUID))
            {
                IterativeOperation iterativeOperation = _dataChunkStore[operation.ReaderUID];
                iterativeOperation.DiposeOperation();
                _dataChunkStore.Remove(operation.ReaderUID);
            }

            IDBResponse dbResponse = operation.CreateResponse();
            dbResponse.IsSuccessfull = true;

            return dbResponse;
        }
      

        #endregion

        /// <summary>
        /// Combines documents going to same shard together to save round trips
        /// </summary>
        /// <param name="documentRoute"></param>
        /// <param name="shardName"></param>
        /// <param name="jDoc"></param>
        private void AssignReceiverShardToDocument(Dictionary<string, List<IJSONDocument>> documentRoute, string shardName, IJSONDocument jDoc)
        {
            List<IJSONDocument> documentsList;
            documentsList = documentRoute.ContainsKey(shardName) ? documentRoute[shardName] : new List<IJSONDocument>();
            documentsList.Add(jDoc);
            documentRoute[shardName] = documentsList;
        }

        private void MarkToBroadcast(Dictionary<string, List<IJSONDocument>> documentRoute, IJSONDocument jDoc)
        {
            foreach (string shardName in _shardConnections.Keys)
            {
                AssignReceiverShardToDocument(documentRoute, shardName, jDoc);
            }
        }

        #region Not Sure if these are required at the moment or not. Written to get PartitionKey based on query parameters
        /*   private string GetFormattedPartitionKey(string[] partitionKeyValues, ParitionKey partitionKey)
        {

            if ()   // Confusion: I don't want an auto-generated key in case of update opertions and Execute Query Right?
                return partitionKey.GetFormattedPartitionKey(partitionKeyValues);
            else
                return null;
        }

        private string[] GetPartitionKeyValues(string dbName, string collectionName, IList<IParameter> parameters, ParitionKey partitionKey)
        {
            if (partitionKey == null)
            {
                throw new Exception("PartitionKey is undefined for this collection or database");
                // To be handled later: Might be a possibility of wrong db name or collection name
            }

            string[] partitonKeyValues = null;

            int tempIndexCouter = 0;
            foreach (IParameter parameter in parameters)
            {
                if (partitionKey.Attributes.Contains(parameter.Name))
                {
                    partitonKeyValues[tempIndexCouter] = parameter.Value.ToString();
                    tempIndexCouter++;
                }
            }
            return partitonKeyValues;
        }*/
        #endregion

        public void Dispose()
        {
            Stop(false);


            if (_shardConnections != null)
            {
                _shardConnections.Clear();
                _shardConnections = null;
            }
            if (_dataChunkStore != null)
            {
                _dataChunkStore.Clear();
                _dataChunkStore = null;
            }
            if (_distributions != null)
            {
                _distributions.Clear();
                _distributions = null;
            }
            if (_retryErrorCodes != null)
            {
                _retryErrorCodes.Clear();
                _retryErrorCodes = null;
            }
            
        }

        public void Dispose(bool destroy)
        {
            Stop(destroy);
            Dispose();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

     private void PerformValidation(IDBOperation operation)
        {
            if (operation == null) throw new DistributorException("Operation Invalid. Please provide an operation");
            if (_configurationSession == null) throw new DistributorException(ErrorCodes.Distributor.CONFIG_SESSION_NOT_AVAILABLE);
            switch (_databaseMode)
            {
                case DatabaseMode.Offline:
                    throw new DistributorException(ErrorCodes.Database.Mode,
                        new[] { operation.Database, _databaseMode.ToString() });
            }

            //add session id assigned by configuration server
            if(operation.SessionId == null || string.IsNullOrEmpty(operation.SessionId.SessionId))
                operation.SessionId = _configurationSession.SessionId;
        }

        #region Listener and its helper functions

        private void StartTheRemoteShard(string newShardName)
        {
            ClusterConfiguration cc = _configurationSession.GetDatabaseClusterConfiguration(_cluster);
            foreach (ShardConfiguration shardConfiguration in cc.Deployment.Shards.Values)
            {
                if (shardConfiguration.Name == newShardName)
                {
                    if (!ShardConnectionsContain(newShardName))
                        continue;//throw new Exception("ShardInfo in cluster and ShardConfiguration in Deployment must be same");

                    RemoteShardConnections remoteShard = GetReceiverShard(shardConfiguration.Name);
                    remoteShard.Initialize(shardConfiguration);
                    remoteShard.Start();
                    AddShardConnection(shardConfiguration.Name, remoteShard);
                    break;
                }
            }
        }

        private IList<string> OnShardAddedInCluster(ClusterInfo clusterInfo)
        {
            IList<string> shardsAdded = new List<string>();
            string newShardName = null;
            foreach (ShardInfo shardInfo in clusterInfo.ShardInfo.Values)
            {
                if (!ShardConnectionsContain(shardInfo.Name))
                {
                    CreateRemoteShard(shardInfo);
                    newShardName = shardInfo.Name;
                    shardsAdded.Add(newShardName);
                    break;
                }
            }

            if (newShardName == null) return shardsAdded;
            StartTheRemoteShard(newShardName);
            OnDistributionStrategyConfigured(clusterInfo);
            return shardsAdded;
        }

        private void OnShardRemovedFromCluster(ClusterInfo clusterInfo)
        {
            ClusterConfiguration clusterConfiguration = _configurationSession.GetDatabaseClusterConfiguration(_cluster);

            IList<string> shardsPresentInCluster = new List<string>();
            foreach (ShardConfiguration shardConfiguration in clusterConfiguration.Deployment.Shards.Values)
            {
                shardsPresentInCluster.Add(shardConfiguration.Name);
            }

            OnDistributionStrategyConfigured(clusterInfo);

            IList<string> shardsRemoved = _shardConnections.Keys.Except(shardsPresentInCluster).ToList();

            foreach (string shardName in shardsRemoved)
            {
                if (_shardConnections.ContainsKey(shardName))
                {
                    RemoteShardConnections remoteShard = _shardConnections[shardName];
                    remoteShard.UnregisterDisconnectionListener();
                    remoteShard.Dispose();
                    _shardConnections.Remove(remoteShard.Name);
                }
            }
        }

        private void OnDistributionStrategyConfigured(ClusterInfo clusterInfo)
        {
            _distributionLock.EnterWriteLock();
            try
            {
                _distributions.Clear();
                ConfigureDistributions(clusterInfo.Databases.Values.ToArray());
            }
            finally
            {
                _distributionLock.ExitWriteLock();
            }
            //ClearDistributions();
            //AddOrUpdateCollectionDistributions(clusterInfo.Databases.Values.ToArray());
        }

        private bool OnDatabaseCreatedHelper(string databaseName, ClusterInfo clusterInfo)
        {
            DatabaseInfo newlyCreatedDatabase = null;
            foreach (var database in clusterInfo.Databases.Values)
            {
                if (database.Name.Equals(databaseName))
                {
                    newlyCreatedDatabase = database;
                    break;
                }
            }

            if (newlyCreatedDatabase == null)
                return false; // Newly created database was not found

            _distributionLock.EnterWriteLock();
            try
            {
                _distributions[databaseName] = new Dictionary<string, IDistribution>();
                foreach (var collection in newlyCreatedDatabase.Collections.Values)
                {
                    _distributions[databaseName].Add(collection.Name, collection.DataDistribution);
                }
                return true;
            }
            finally
            {
                _distributionLock.ExitWriteLock();
            }
        }

        private bool OnDatabaseDroppedHelper(string databaseName, ClusterInfo clusterInfo)
        {
            foreach (var database in clusterInfo.Databases.Values)
            {
                if (database.Name.Equals(databaseName))
                {
                    return false;
                }
            }

            _distributionLock.EnterWriteLock();
            try
            {
                if (_distributions.Remove(databaseName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                _distributionLock.ExitWriteLock();
            }
        }

        private bool OnCollectionCreatedHelper(string databaseName, string collectionName, ClusterInfo clusterInfo)
        {
            CollectionInfo newlyCreatedCollection = null;
            foreach (var database in clusterInfo.Databases.Values)
            {
                if (database.Name.Equals(databaseName))
                {
                    foreach (var collection in database.Collections.Values)
                    {
                        if (collection.Name.Equals(collectionName))
                        {
                            newlyCreatedCollection = collection;
                            break;
                        }
                    }
                }
            }
            if (newlyCreatedCollection == null)
                return false; // Newly created collection was not found

            _distributionLock.EnterWriteLock();
            try
            {
                if (_distributions.ContainsKey(databaseName))
                {
                    _distributions[databaseName][collectionName] = newlyCreatedCollection.DataDistribution;
                }
                else
                {
                    _distributions.Add(databaseName, new Dictionary<string, IDistribution>());
                    _distributions[databaseName].Add(collectionName, newlyCreatedCollection.DataDistribution);
                }
                return true;
            }
            finally
            {
                _distributionLock.ExitWriteLock();
            }
        }

        private bool OnCollectionDroppedHelper(string databaseName, string collectionName, ClusterInfo clusterInfo)
        {
            foreach (var database in clusterInfo.Databases.Values)
            {
                if (database.Name.Equals(databaseName))
                {
                    foreach (var collection in database.Collections.Values)
                    {
                        if (collection.Name.Equals(collectionName))
                            return false;       // Collection was not dropped or the event sequence is not correct
                    }
                }
            }

            _distributionLock.EnterWriteLock();
            try
            {
                if (_distributions.ContainsKey(databaseName))
                {
                    if (_distributions[databaseName].Remove(collectionName))
                    {
                        return true;
                    }
                    else
                    {
                        //Key was not present
                        return false;
                    }
                }
                return false;
            }
            finally
            {
                _distributionLock.ExitWriteLock();
            }
        }

        private void OnDatabaseCreated(string databaseName, ClusterInfo clusterInfo)
        {
            if (!OnDatabaseCreatedHelper(databaseName, clusterInfo))
            {
                OnDatabaseDroppedHelper(databaseName, clusterInfo);
            }
        }

        private void OnDatabaseDropped(string databaseName, ClusterInfo clusterInfo)
        {
            if (!OnDatabaseDroppedHelper(databaseName, clusterInfo))
            {
                OnDatabaseCreatedHelper(databaseName, clusterInfo);
            }
        }

        private void OnCollectionCreated(string databaseName, string collectionName, ClusterInfo clusterInfo)
        {
            if (!OnCollectionCreatedHelper(databaseName, collectionName, clusterInfo))
            {
                OnCollectionDroppedHelper(databaseName, collectionName, clusterInfo);
            }
        }

        private void OnCollectionDropped(string databaseName, string collectionName, ClusterInfo clusterInfo)
        {
            if (!OnCollectionDroppedHelper(databaseName, collectionName, clusterInfo))
            {
                OnCollectionCreatedHelper(databaseName, collectionName, clusterInfo);
            }
        }

        public void OnConfigurationChanged(ConfigChangeEventArgs arguments)
        {
            //TODO: Revert all the changes in RemoteShardConnections if an exception occurs while making connections or any other exception

            string clusterName = String.Empty;
            if (arguments != null)
            {
                clusterName = arguments.GetParamValue<string>(EventParamName.ClusterName);
            }
            if (clusterName != null && !clusterName.Equals(_cluster)) return;
            try
            {
                Monitor.Enter(_configurationChangeLock);

                IList<string> shardsAdded;
                ChangeType type = ChangeType.None;
                if (arguments != null)
                    type = arguments.GetParamValue<ChangeType>(EventParamName.ConfigurationChangeType);
                if (type == ChangeType.ConfigServerRemoved)
                {
                    Thread.Sleep(1000);
                    _configurationSession.AddConfigurationListener(this);
                }
                ClusterInfo clusterInfo = GetClusterInfo(_cluster);

                switch (type)
                {
                    case ChangeType.ShardAdded:
                        shardsAdded = OnShardAddedInCluster(clusterInfo);
                        AuthenticateShards(shardsAdded);
                        break;
                    case ChangeType.ShardRemovedForceful:
                    case ChangeType.ShardRemovedGraceful:
                        OnShardRemovedFromCluster(clusterInfo);
                        break;
                    case ChangeType.NodeJoined:
                    case ChangeType.NodeLeft:
                    case ChangeType.PrimaryGone:
                    case ChangeType.PrimarySelected:

                        foreach (ShardInfo shardInfo in clusterInfo.ShardInfo.Values)
                        {
                            if (shardInfo.Name.Equals(arguments.GetParamValue<string>(EventParamName.ShardName)))
                            {
                                if (ShardConnectionsContain(shardInfo.Name))
                                {
                                    RemoteShardConnections remoteShardConnection = GetReceiverShard(shardInfo.Name);
                                    IList<Server> serversAdded = remoteShardConnection.UpdateConnections(shardInfo);
                                    AuthenticateServersInShard(serversAdded, remoteShardConnection);
                                }
                                else
                                {
                                    shardsAdded = OnShardAddedInCluster(clusterInfo);
                                    AuthenticateShards(shardsAdded);
                                }
                                break;
                            }
                        }
                        break;
                    case ChangeType.NewRangeAdded:
                    case ChangeType.RangeUpdated:
                    case ChangeType.DistributionChanged:
                    case ChangeType.CollectionCreated:
                        OnCollectionCreated(arguments.GetParamValue<string>(EventParamName.DatabaseName), arguments.GetParamValue<string>(EventParamName.CollectionName), clusterInfo);
                        break;
                    case ChangeType.CollectionDropped:
                        OnCollectionDropped(arguments.GetParamValue<string>(EventParamName.DatabaseName), arguments.GetParamValue<string>(EventParamName.CollectionName), clusterInfo);
                        break;
                    case ChangeType.ConfigRestored:
                    case ChangeType.DistributionStrategyConfigured:
                        OnDistributionStrategyConfigured(clusterInfo);
                        break;
                    case ChangeType.DatabaseCreated:
                        OnDatabaseCreated(arguments.GetParamValue<string>(EventParamName.DatabaseName), clusterInfo);
                        break;
                    case ChangeType.DatabaseDropped:
                        OnDatabaseDropped(arguments.GetParamValue<string>(EventParamName.DatabaseName), clusterInfo);
                        break;
                    case ChangeType.ModeChange:
                        if (arguments != null)
                            OnDatabaseModeChange(arguments.GetParamValue<string>(EventParamName.DatabaseName), arguments.GetParamValue<DatabaseMode>(EventParamName.DatabaseMode), clusterInfo);
                        break;
                    case ChangeType.ConfigServerRemoved:
                        OnShardRemovedFromCluster(clusterInfo);
                        foreach (ShardInfo shardInfo in clusterInfo.ShardInfo.Values)
                        {
                            foreach (var remoteShardConnection in _shardConnections.Values)
                            {
                                IList<Server> serversAdded = remoteShardConnection.UpdateConnections(shardInfo);
                                AuthenticateServersInShard(serversAdded, remoteShardConnection);
                            }
                        }
                        break;
                }
            }
            finally
            {
                Monitor.Exit(_configurationChangeLock);
            }
        }


        #endregion

        #region IDataLoader Methods

        public IDataChunk LoadData(DistributedDataSet dataFromShard)
        {
            try
            {
                RemoteShardConnections receiverShard = GetReceiverShard(dataFromShard.ShardName);

                IDBOperation dbOperation = dataFromShard.Operation;
                PerformValidation(dbOperation);
                IGetChunkOperation operation = new GetChunkOperation();
                operation.Collection = dbOperation.Collection;
                operation.Context = dbOperation.Context;
                operation.Database = dbOperation.Database;
                operation.OperationType = DatabaseOperationType.GetChunk;
                operation.RequestId = dbOperation.RequestId;
                operation.SessionId = dbOperation.SessionId;


                operation.ReaderUID = dataFromShard.ReaderUID;


                operation.LastChunkId = dataFromShard.DataChunk.ChunkId;
                IGetChunkResponse queryResponse = (IGetChunkResponse)operation.CreateResponse();

                try
                {
                    IAsyncResult asyncResultForGettingResponse = receiverShard.BeginSendMessage(dataFromShard.ReaderAddress, operation);

                    queryResponse = (IGetChunkResponse)receiverShard.EndSendMessage(dataFromShard.ReaderAddress, asyncResultForGettingResponse);
                }
                catch (SocketException)
                {
                    queryResponse.IsSuccessfull = false;
                    queryResponse.ErrorCode = ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING;
                    queryResponse.ErrorParams = new string[] { receiverShard.Primary.Address.ToString() };
                }

                if (!queryResponse.IsSuccessfull)
                {
                    //throw new Exception("Invalid Query Operation!");
                    if (queryResponse.ErrorParams == null)
                    {
                        throw new DatabaseException(ErrorMessages.GetErrorMessage(queryResponse.ErrorCode));
                    }
                    throw new DatabaseException(string.Format(ErrorMessages.GetErrorMessage(queryResponse.ErrorCode),
                        queryResponse.ErrorParams.ToArray<object>()));
                }

                return queryResponse.DataChunk;
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DistributorException(ErrorCodes.Distributor.UNKNOWN_GETQUERYCHUNK_EXCEPTION, new string[] { dataFromShard.ReaderUID, dataFromShard.ReaderAddress.Address.ToString(), dataFromShard.ShardName });
            }
        }

        public void DisposeSetReader(DistributedDataSet dataFromShard)
        {
            IDBOperation dbOperation = dataFromShard.Operation;
            PerformValidation(dbOperation);
            IDiposeReaderOperation operation = new DisposeReaderOperation();
            operation.Collection = dbOperation.Collection;
            operation.Context = dbOperation.Context;
            operation.Database = dbOperation.Database;
            operation.OperationType = DatabaseOperationType.DisposeReader;
            operation.RequestId = dbOperation.RequestId;
            operation.SessionId = dbOperation.SessionId;

            RemoteShardConnections receiverShard = GetReceiverShard(dataFromShard.ShardName);
            operation.ReaderUID = dataFromShard.ReaderUID;

            IAsyncResult asyncResultForGettingResponse = receiverShard.BeginSendMessage(dataFromShard.ReaderAddress, operation);
            receiverShard.EndSendMessage(dataFromShard.ReaderAddress, asyncResultForGettingResponse);
        }
        #endregion

        private void MakeQueryDecisions(IDqlObject parsedQuery, ref List<IDataCombiner> dataCombiners, ref IDataSelector dataSelector, ref IComparer comparer, out ICollection projections)
        {
            if (parsedQuery == null)
                throw new ArgumentNullException("parsedQuery");

            projections = null;
            if (parsedQuery is SelectObject)
            {
                SelectObject selectQuery = parsedQuery as SelectObject;
                projections = selectQuery.Projections;
                if (selectQuery.OrderValue != null)
                {
                    IList<OrderedAttribute> orderByAttributes = new List<OrderedAttribute>();
                    foreach (object obj in selectQuery.OrderValue)
                    {
                        var item = obj as BinaryExpression;
                        if (item != null)
                        {
                            OrderedAttribute attribute = new OrderedAttribute(new Attribute(item.ToString()), new SortOrderConstant(item.SortOrder));

                            if (item.Attributes.Count > 0)
                            {
                                Attribute binaryExpressionAttribute = item.Attributes[0];
                                attribute = new OrderedAttribute(binaryExpressionAttribute.Name, item.SortOrder);
                                attribute.Indecies = binaryExpressionAttribute.Indices;
                                attribute.ChildAttribute = (Attribute)binaryExpressionAttribute.Child;
                            }
                            orderByAttributes.Add(attribute);
                        }
                    }
                    comparer = new JsonDocumentComparer(orderByAttributes);
                    if (selectQuery.GroupValue == null)
                    {
                        dataSelector = new DataSelectorOrdered();
                        (dataSelector as DataSelectorOrdered).ProjectionValues = selectQuery.Projections;
                    }
                }

                dataCombiners = new List<IDataCombiner>();
                foreach (object obj in selectQuery.Projections)
                {
                    Function aggregateFunction = null;
                    BinaryExpression binaryExpression = null;
                    if (obj is BinaryExpression)
                    {
                        binaryExpression = obj as BinaryExpression;
                        List<Function> functions = binaryExpression.Functions;
                        if (functions.Count > 0)
                            aggregateFunction = functions.First();
                        else
                            continue;
                    }
                    else if (obj is Function)
                        aggregateFunction = obj as Function;
                    else
                    {
                        continue;
                    }

                    string aggregateFunctionName = aggregateFunction.FunctionName;
                    string combinerName = aggregateFunction.FunctionNameActual;
                    bool aliasExists = binaryExpression != null && binaryExpression.Alias != null;
                    if (aliasExists)
                        combinerName = binaryExpression.Alias;
                    switch (aggregateFunctionName)
                    {
                        case "sum":
                            dataCombiners.Add(new SumDataCombiner(aggregateFunction.Arguments, combinerName, aliasExists));
                            break;
                        case "count":
                            dataCombiners.Add(new CountDataCombiner(aggregateFunction.Arguments, combinerName, aliasExists));
                            break;
                        case "max":
                            dataCombiners.Add(new MaxDataCombiner(aggregateFunction.Arguments, combinerName, aliasExists));
                            break;
                        case "min":
                            dataCombiners.Add(new MinDataCombiner(aggregateFunction.Arguments, combinerName, aliasExists));
                            break;
                        case "avg":
                            dataCombiners.Add(new AverageDataCombiner(aggregateFunction.Arguments, combinerName, aliasExists));
                            break;
                    }
                }

                if (selectQuery.GroupValue != null)
                {
                    if (dataCombiners.Count == 0)
                        dataCombiners.Add(new NoAggregationDataCombiner(null));

                    dataSelector = new DataSelectorGroupBy();
                    (dataSelector as DataSelectorGroupBy).ProjectionValues = selectQuery.Projections;

                    if (comparer == null)
                    {
                        IList<OrderedAttribute> groupByAttributes = new List<OrderedAttribute>();
                        foreach (BinaryExpression exp in selectQuery.GroupValue)
                        {
                            OrderedAttribute attribute = new OrderedAttribute(new Attribute(exp.ToString()), new SortOrderConstant(exp.SortOrder));
                            groupByAttributes.Add(attribute);
                        }
                        comparer = new JsonDocumentComparer(groupByAttributes);
                    }
                }
                else if (dataCombiners.Count > 0)
                {
                    dataSelector = new DataSelectorGroupBy();
                    (dataSelector as DataSelectorGroupBy).ProjectionValues = selectQuery.Projections;

                    if (comparer == null)
                    {
                        IList<OrderedAttribute> projectionAttributes = new List<OrderedAttribute>();
                        //Important: Don't uncomment else "Select avg(elevation) from mycollection will fail"
                        //foreach (IEvaluable attribute in selectQuery.Projections)
                        //{
                        //    OrderedAttribute orderedAttribute = new OrderedAttribute(
                        //        new Attribute(attribute.ToString()), new SortOrderConstant(Common.Enum.SortOrder.ASC));
                        //    projectionAttributes.Add(orderedAttribute);
                        //}
                        comparer = new JsonDocumentComparer(projectionAttributes);
                    }
                }

                if (selectQuery.IsDistinct && selectQuery.GroupValue == null)
                {
                    DataSelectorGroupBy distinctDataSelector = new DataSelectorGroupBy();
                    distinctDataSelector.DataSelector = dataSelector;
                    distinctDataSelector.ProjectionValues = selectQuery.Projections;
                    distinctDataSelector.DataCombiner = new List<IDataCombiner>() { new NoAggregationDataCombiner(null) };
                    dataSelector = distinctDataSelector;

                    if (comparer == null)
                    {
                        IList<OrderedAttribute> projectionAttributes = new List<OrderedAttribute>();
                        foreach (IEvaluable attribute in selectQuery.Projections)
                        {
                            OrderedAttribute orderedAttribute = new OrderedAttribute(
                                new Attribute(attribute.ToString()), new SortOrderConstant(Alachisoft.NosDB.Common.Enum.SortOrder.ASC));
                            projectionAttributes.Add(orderedAttribute);
                        }
                        comparer = new JsonDocumentComparer(projectionAttributes);
                    }
                }

                if (dataSelector == null)
                {
                    dataSelector = new DataSelectorRoundRobin();
                }

                if (comparer == null)
                {
                    comparer = new JsonDocumentComparer(null);
                }

                // Must be assigned before Limit otherwise a bug will occur
                if (selectQuery.Skip != null)
                {
                    dataSelector = new DataSelectorSkip(dataSelector, long.Parse(selectQuery.Skip.InString));
                }

                if (selectQuery.Limit != null)
                {
                    dataSelector = new DataSelectorLimit(dataSelector, long.Parse(selectQuery.Limit.InString));
                }
            }
        }

        //M_Note:[Useless] remove this once a centralized methods is developed
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

        #region Utility Methods
        private string AutoGenerateUniqueString()
        {
            return Guid.NewGuid().ToString();
        }

        private bool ShardConnectionsContain(string shardName)
        {
            _shardConnectionsLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _shardConnections.ContainsKey(shardName);
            }
            finally
            {
                _shardConnectionsLock.ReleaseReaderLock();
            }
        }

        private void AddShardConnection(string shardName, RemoteShardConnections remoteShardConnection)
        {
            _shardConnectionsLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _shardConnections[shardName] = remoteShardConnection;
            }
            finally
            {
                _shardConnectionsLock.ReleaseWriterLock();
            }
        }

        private RemoteShardConnections GetReceiverShard(string shardName)
        {
            _shardConnectionsLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (_shardConnections.ContainsKey(shardName))
                {
                    return _shardConnections[shardName];
                }
                else
                {
                    throw new DistributorException(ErrorCodes.Distributor.UNEXPECTED_SHARD_DOESNOT_EXIST, new string[] { shardName });
                }
            }
            finally
            {
                _shardConnectionsLock.ReleaseReaderLock();
            }
        }

        private IDistribution GetDistribution(string database, string collection)
        {
            database = database.ToLower();
            collection = collection.ToLower();

            _distributionLock.EnterReadLock();
            try
            {
                if (_distributions.ContainsKey(database))
                {
                    if (_distributions[database].ContainsKey(collection))
                    {
                        return _distributions[database][collection];
                    }
                    throw new DistributorException(ErrorCodes.Distributor.COLLECTION_DOESNOT_EXIST, new[] { collection, database });
                }
                throw new DistributorException(ErrorCodes.Distributor.DATABASE_DOESNOT_EXIST, new string[] { database });
            }
            finally
            {
                _distributionLock.ExitReadLock();
            }
        }

        private IDistributionRouter GetDistributionRouter(string database, string collection)
        {
            return GetDistribution(database, collection).GetDistributionRouter();
        }

        private void ConfigureDistributions(DatabaseInfo[] databases)
        {
            if (databases == null) return;
            foreach (DatabaseInfo databaseInfo in databases)
            {
                _distributions.Add(databaseInfo.Name, new Dictionary<string, IDistribution>());
                if (databaseInfo.Collections == null) continue;
                foreach (CollectionInfo collectionInfo in databaseInfo.Collections.Values)
                {
                    //Note: DON'T UNCOMMENT THIS LOCK BECAUSE IT IS ALREADY AQUIRED BY OnDistributionStrategyConfigured
                    //_distributionLock.EnterWriteLock();
                    //try
                    //{
                    _distributions[databaseInfo.Name].Add(collectionInfo.Name, collectionInfo.DataDistribution);
                    //}
                    //finally
                    //{
                    //    _distributionLock.ExitWriteLock();
                    //}
                }
            }
        }
        #endregion
       
        

        private ClusterInfo GetClusterInfo(string cluster)
        {
            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    if (_configurationSession == null) throw new DatabaseException(ErrorCodes.Distributor.CONFIG_SESSION_NOT_AVAILABLE);
                    return _configurationSession.GetDatabaseClusterInfo(cluster);
                }
                catch (System.TimeoutException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
                }
                catch (Common.Exceptions.TimeoutException)
                {
                    throw new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
                }
                catch (ChannelException)
                {
                    retries--;
                    if (retries == 0)
                        throw new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
                    continue;
                }
            }
            return null;
        }

        public bool CollectionExists(string databaseName, string collectionName)
        {
            databaseName = databaseName.Trim().ToLower();
            collectionName = collectionName.Trim().ToLower();

            ClusterInfo clusterInfo = GetClusterInfo(_cluster);
            if (clusterInfo.Databases == null) return false;
            if (clusterInfo.ContainsDatabase(databaseName))
            {
                DatabaseInfo databaseInfo = clusterInfo.GetDatabase(databaseName);
                if (databaseInfo != null && databaseInfo.Collections != null)
                    return databaseInfo.ContainsCollection(collectionName);
                return false;
            }
            return false;
        }

        public string GenerateDocumentKey()
        {
            return Guid.NewGuid().ToString();
        }

        public bool DatabaseExists(string databaseName)
        {
            databaseName = databaseName.Trim().ToLower();

            ClusterInfo clusterInfo = GetClusterInfo(_cluster);
            return clusterInfo.ContainsDatabase(databaseName);
        }

        #region Authentication

        private IDictionary<string, ClientContext> _clientContexts;
        private IDictionary<string, ClientCredential> _clientCredentials;

        private void InitializeDatabase(Server server, RemoteShardConnections remoteShardConnection, IClientAuthenticationCredential clientCredentials)
        {
            ISessionId sessionId = _configurationSession.SessionId;
            if (clients.ContainsKey(clientCredentials))
                sessionId = clients[clientCredentials].SessionId;
            InitDatabaseOperation initDatabaseOperation = new InitDatabaseOperation();
            //  initDatabaseOperation.ConnectionString = _connectionStringBuilder.ConnectionString;
            initDatabaseOperation.Database = _database;
            initDatabaseOperation.SessionId = sessionId;
            IAsyncResult asyncResult = remoteShardConnection.BeginSendMessage(server, initDatabaseOperation);
            InitDatabaseResponse response = remoteShardConnection.EndSendMessage(server, asyncResult) as InitDatabaseResponse;
            if (response != null && !response.IsSuccessfull)
            {
                throw new DistributorException(response.ErrorCode, response.ErrorParams);
            }
        }

        private bool AuthenticateServerWithWindows(Server server, RemoteShardConnections remoteShardConnection)
        {
            
            #region Context Initialization

            string SPN = null;
            if (!SSPIUtility.IsLocalServer(server.Address.IpAddress))
            {
                try
                {
                    SPN = SSPIUtility.GetServicePrincipalName(MiscUtil.NOSDB_SPN, server.Address.IpAddress);
                    //SPN += (":" + server.Address.Port);
                }
                catch (SocketException ex)
                {
                    SPN = null;
                }
            }
            ClientCredential clientCred = SSPIUtility.GetClientCredentials(SPN);

            ClientContext clientContext = SSPIUtility.GetClientContext(clientCred, SPN);
            #endregion
            bool isAuthenticated = false;

            AuthToken clientAuthToken = new AuthToken();
            AuthToken serverAuthToken = new AuthToken();
            serverAuthToken.Token = null;
            serverAuthToken.Status = SecurityStatus.None;
            do
            {
                Byte[] clientToken = null;
                clientAuthToken.Status = clientContext.Init(serverAuthToken.Token, out clientToken);
                clientAuthToken.Token = clientToken;

                if (clientAuthToken.Status == SecurityStatus.ContinueNeeded || (clientAuthToken.Status == SecurityStatus.OK && clientAuthToken.Token != null))
                {
                    try
                    {
                        AuthenticationOperation operation = new AuthenticationOperation();
                        operation.ClientToken = clientAuthToken;
                        operation.SessionId = _configurationSession.SessionId;
                        operation.ClientProcessID = _processID;
                        operation.ConnectionString = _connectionStringBuilder.ConnectionString;
                        IAsyncResult result = remoteShardConnection.BeginSendMessage(server, operation) as IAsyncResult;
                        AuthenticationResponse response = remoteShardConnection.EndSendMessage(server, result) as AuthenticationResponse;
                        serverAuthToken.Status = response.ServerToken.Status;
                        serverAuthToken.Token = response.ServerToken.Token;
                    }
                    catch (SecurityException exc)
                    {
                        Console.WriteLine("Authenticating client with databases: Cannot authenticate with server" + server.Address + "Exception: " + exc.Message);
                    }
                    catch (DistributorException exc)
                    {
                        isAuthenticated = true;
                        break;
                    }
                }
                if (serverAuthToken.Status == SecurityStatus.SecurityDisabled || (clientAuthToken.Status == SecurityStatus.OK && (serverAuthToken.Token.Length == 0 || serverAuthToken.Status == SecurityStatus.OK)))
                {
                    isAuthenticated = true;
                    break;
                }
            } while (clientAuthToken.Status == SecurityStatus.ContinueNeeded);

            return isAuthenticated;
        }

        private void AuthenticateServersInShard(IList<Server> servers, RemoteShardConnections remoteShardConnections)
        {
            foreach (Server server in servers)
            {
                AuthenticateServer(server, remoteShardConnections, routerClientCredential);
            }
        }

        private void AuthenticateShards(IList<string> shards)
        {
            foreach (string shardName in shards)
            {
                RemoteShardConnections remoteShardConnection = GetReceiverShard(shardName);
                AuthenticateShard(remoteShardConnection, routerClientCredential);
            }
        }

        private void AuthenticateServer(Server server, RemoteShardConnections remoteShardConnection,
            IClientAuthenticationCredential clientCredential = null)
        {

            bool isAuthenticated = false;
            try
            {
                isAuthenticated = AuthenticateServerWithWindows(server, remoteShardConnection);

            }
            catch (DistributorException)
            {
                //if logging provided on client side, it must be logged
            }
            if (isAuthenticated)
            {
                remoteShardConnection.ChannelAuthenticated(server, isAuthenticated);
                InitializeDatabase(server, remoteShardConnection, clientCredential);
            }
            else
            {
                remoteShardConnection.RemoveUnauthenticatedChannel(server);
            }

        }

        private void AuthenticateShard(RemoteShardConnections remoteShardConnections, IClientAuthenticationCredential clientCredential = null)
        {
            foreach (Server server in remoteShardConnections.Servers)
            {
                AuthenticateServer(server, remoteShardConnections, clientCredential);
            }
        }

        private void AuthenticateAll()
        {
            int totalRunningNodes = 0;
            foreach (RemoteShardConnections remoteShardConnection in this._shardConnections.Values)
            {
                totalRunningNodes += remoteShardConnection.Servers.Count;
                AuthenticateShard(remoteShardConnection, routerClientCredential);
            }
            if (totalRunningNodes == 0)
                throw new DistributorException(ErrorCodes.Distributor.NO_RUNNING_NODE);
            if (clients.Count > 0)
            {
                foreach (IClientAuthenticationCredential clientCredential in clients.Keys)
                {
                    foreach (RemoteShardConnections remoteShardConnection in this._shardConnections.Values)
                    {
                        AuthenticateShard(remoteShardConnection, clientCredential);
                    }
                }
            }
        }

        private void Authenticate(IClientAuthenticationCredential clientCredential = null)
        {
            foreach (RemoteShardConnections remoteShardConnection in this._shardConnections.Values)
            {
                AuthenticateShard(remoteShardConnection, clientCredential);
            }
        }

        public IDBResponse Authenticate(IAuthenticationOperation operation)
        {
            throw new NotImplementedException();
        }

        #endregion




        public void OnChannelDisconnected(ISessionId sessionId)
        {
            //throw new NotImplementedException();
        }


        public IDBResponse InitializeDatabase(InitDatabaseOperation initDatabaseOperation)
        {
            return initDatabaseOperation.CreateResponse();
        }

        public void InitializeDatabase(string database)
        {
            foreach (RemoteShardConnections remoteShardConnection in this._shardConnections.Values)
            {
                foreach (Server server in remoteShardConnection.AuthenticatedServers)
                {
                    try
                    {
                        InitDatabaseOperation initDatabaseOperation = new InitDatabaseOperation();
                        initDatabaseOperation.SessionId = _configurationSession.SessionId;
                        initDatabaseOperation.Database = database;
                        IAsyncResult asyncResult = remoteShardConnection.BeginSendMessage(server, initDatabaseOperation);
                        InitDatabaseResponse response = remoteShardConnection.EndSendMessage(server, asyncResult) as InitDatabaseResponse;
                    }
                    catch (DistributorException)
                    {

                    }
                }
            }
        }

        internal bool IsValidDocumentKey(IEnumerable<KeyValuePair<Attribute, IEvaluable>> values, IList<IParameter> queryParams = null)
        {
            if (values == null)
                return false;

            foreach (var pair in values)
            {
                if (pair.Key.ParentAttributeName == JsonDocumentUtil.DocumentKeyAttribute)
                {
                    if (pair.Value == null || pair.Value is NullValue)
                        return false;
                    if ((pair.Value is StringConstantValue))
                        return true;
                    else if (pair.Value is Alachisoft.NosDB.Common.JSON.Expressions.Parameter)
                    {
                        bool isFound = false;
                        object value = null;
                        if (queryParams != null)
                        {
                            foreach (var param in queryParams)
                            {
                                if (param.Name == pair.Value.ToString())
                                {
                                    isFound = true;
                                    value = param.Value;
                                    break;
                                }
                            }
                        }
                        if (!isFound)
                            throw new DatabaseException(ErrorCodes.Query.UNASSIGNED_QUERY_PARAMETER, new string[] { JsonDocumentUtil.DocumentKeyAttribute });

                        if (value == null)
                            return false;
                        else if (value is string)
                            return true;
                    }

                    //only String type Document key is supported
                    throw new DatabaseException(ErrorCodes.Distributor.INVALID_DOCUMENT_KEY);
                }
            }
            return false;
        }


        public void Stop(bool destroy)
        {
            Stop();
        }

        public static void RegisterCompactTypes()
        {
            #region [Register Assemblies]
            //    _   _  ___ _____ _____ 
            //   | \ | |/ _ \_   _| ____|
            //   |  \| | | | || | |  _|  
            //   | |\  | |_| || | | |___ 
            //   |_| \_|\___/ |_| |_____|
            //                           
            //Any type that is meant to be shared between processess must be registed from 1 and above 
            //Any type that is to be used internally must be registered from 999 and below


            //Types that can/might be shared

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Communication.ChannelRequest), 1);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Communication.ChannelResponse), 2);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.UserCredentials), 3);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.HashMapBucket), 4);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.HashMapBucket[]), 5);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.RPCFramework.TargetMethodParameter), 6);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.ManagementCommand), 7);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.ManagementResponse), 8);
            //CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.CommandBase), 9);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CachingConfiguration), 10);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfigurations), 11);
            CompactFormatterServices.RegisterCompactType(typeof(ClusterConfiguration), 12);
            CompactFormatterServices.RegisterCompactType(typeof(DeploymentConfiguration), 13);
            CompactFormatterServices.RegisterCompactType(typeof(ShardConfiguration), 14);
            CompactFormatterServices.RegisterCompactType(typeof(ShardConfiguration[]), 15);
            CompactFormatterServices.RegisterCompactType(typeof(ServerNode), 16);
            CompactFormatterServices.RegisterCompactType(typeof(ServerNode[]), 17);
            CompactFormatterServices.RegisterCompactType(typeof(ServerNodes), 18);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration), 19);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.LMDBConfiguration), 21);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.StorageConfiguration), 22);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.StorageProviderConfiguration), 23);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfigurations), 24);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration[]), 25);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration[]), 26);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration), 27);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DistributionStrategyConfiguration), 28);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexAttribute), 29);
            CompactFormatterServices.RegisterCompactType(typeof(Membership), 30);
            CompactFormatterServices.RegisterCompactType(typeof(ElectionId), 31);

            CompactFormatterServices.RegisterCompactType(typeof(ClusterInfo), 32);
            CompactFormatterServices.RegisterCompactType(typeof(DatabaseInfo), 33);
            CompactFormatterServices.RegisterCompactType(typeof(CollectionInfo), 34);
            CompactFormatterServices.RegisterCompactType(typeof(ShardInfo), 35);
            CompactFormatterServices.RegisterCompactType(typeof(ServerInfo), 36);
            CompactFormatterServices.RegisterCompactType(typeof(ServerInfo[]), 37);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Stats.CollectionStatistics), 38);
            CompactFormatterServices.RegisterCompactType(typeof(ShardInfo[]), 39);
            CompactFormatterServices.RegisterCompactType(typeof(DatabaseInfo[]), 40);
            CompactFormatterServices.RegisterCompactType(typeof(CollectionInfo[]), 41);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexAttribute[]), 43);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration), 44);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration[]), 45);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.Indices), 46);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.ElectionResult), 47);

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSONDocument), 51);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.DocumentKey), 52);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Enum.SortOrder), 53);
            CompactFormatterServices.RegisterCompactType(typeof(Message), 55);
            CompactFormatterServices.RegisterCompactType(typeof(ConfigChangeEventArgs), 57);
            CompactFormatterServices.RegisterCompactType(typeof(PartitionKey), 58);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Stats.ShardInfo), 70);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Stats.ShardStatistics), 71);
            // CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.QueryLogOperation), 75);
            //CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.GetMinorOperationsOperation), 87);
         
            CompactFormatterServices.RegisterCompactType(typeof(DatabaseMessage), 91);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType), 92);
            CompactFormatterServices.RegisterCompactType(typeof(OpCode), 93);

            #region State Transfer Classes

            CompactFormatterServices.RegisterCompactType(typeof(StateTransferIdentity), 95);
            CompactFormatterServices.RegisterCompactType(typeof(OperationParam), 96);
            CompactFormatterServices.RegisterCompactType(typeof(NodeIdentity), 97);

            #endregion

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.Parameter), 99);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.Query), 100);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.WriteQueryOperation), 101);
            


            CompactFormatterServices.RegisterCompactType(typeof(CappedInfo), 106);



            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.EvictionConfiguration), 114);
            CompactFormatterServices.RegisterCompactType(typeof(NonShardedDistributionStrategy), 115);
            CompactFormatterServices.RegisterCompactType(typeof(NonShardedDistribution), 116);
            CompactFormatterServices.RegisterCompactType(typeof(NonShardedDistributionRouter), 117);
            CompactFormatterServices.RegisterCompactType(typeof(ConfigServerConfiguration), 119);
            CompactFormatterServices.RegisterCompactType(typeof(ClusterInfo[]), 121);


            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.AuthToken), 122);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Client.SSPIClientAuthenticationCredential), 123);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Server.SSPIServerAuthenticationCredential), 124);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.PartitionKeyConfiguration), 125);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.PartitionKeyConfigurationAttribute), 126);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.PartitionKeyConfigurationAttribute[]), 127);
            CompactFormatterServices.RegisterCompactType(typeof(PartitionKeyAttribute), 128);
            CompactFormatterServices.RegisterCompactType(typeof(PartitionKeyAttribute[]), 129);


            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.Permission), 131);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.ResourceId), 132);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.Role), 133);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.User), 134);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.ResourceItem), 135);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.RoleInstance), 136);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.RouterSessionId), 137);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.ClientSessionId), 138);
            //143

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.DataChunk), 143);
           

            
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSONDocument.JsonObject), 152);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.KeyValuePair), 153);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.KeyValuePair[]), 154);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.KeyValueStore), 155);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSONDocument[]), 156);


            //Types to be used internally
            CompactFormatterServices.RegisterCompactType(typeof(Common.JSON.Indexing.SingleAttributeValue), 994);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSON.Indexing.MultiAttributeValue), 993);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSON.NullValue), 992);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Enum.FieldDataType), 985);

            CompactFormatterServices.RegisterCompactType(typeof(Common.JSON.Indexing.AllValue), 981);
            CompactFormatterServices.RegisterCompactType(typeof(Common.JSON.Indexing.NoValue), 980);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSON.Indexing.ArrayElement), 979);

            #region recovery classes
            CompactFormatterServices.RegisterCompactType(typeof(Common.Configuration.DOM.RecoveryConfiguration), 978);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ShardRecoveryJobState), 977);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.RecoveryJobStateBase), 976);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ClusteredRecoveryJobState), 975);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.RecoveryOperationStatus), 974);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.RecoveryOperation), 973);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.SubmitBackupOpParams), 972);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.SubmitRestoreOpParams), 971);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Configuration.DOM.RecoveryPersistenceConfiguration), 970);

            #endregion
            #endregion
        }



        public IDBResponse RenameIndex(IRenameIndexOperation operation)
        {
            PerformValidation(operation);
            throw new NotImplementedException();
        }

        public IDBResponse RecreateIndex(IRecreateIndexOperation operation)
        {
            PerformValidation(operation);
            throw new NotImplementedException();
        }


        private void OnDatabaseModeChange(string databaseName, DatabaseMode databaseMode, ClusterInfo clusterInfo)
        {
            _distributionLock.EnterReadLock();
            try
            {
                if (_distributions.ContainsKey(databaseName))
                    _databaseMode = databaseMode;
            }
            finally
            {
                _distributionLock.ExitReadLock();
            }
        }

        private bool IsShardExist(IConfigurationSession configurationSession)
        {
            ClusterInfo clusterInfo = _configurationSession.GetDatabaseClusterInfo(_cluster);

            if (clusterInfo.ShardInfo == null)
                return false;
            else if (clusterInfo.ShardInfo.Count < 1)
                return false;
            return true;
        }

        public void OnChannelDisconnected(RemoteShardConnections remoteShardConnection, Server server)
        {
            AuthenticateServer(server, remoteShardConnection, routerClientCredential);

            foreach (var clientCred in this.clients.Keys)
            {
                AuthenticateServer(server, remoteShardConnection, clientCred);
            }
        }


        public void MarkDistributorSession(bool IsDistributorSession)
        {
            throw new NotImplementedException();
        }
    }
}
