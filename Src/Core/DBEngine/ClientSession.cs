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
using System.Threading;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Serialization.Surrogates;

namespace Alachisoft.NosDB.Core.DBEngine
{
    public class ClientSession : IRequestListener
    {
        private ServerChannel _serverChannel;
        private IDatabaseEngineFactory _databaseEngineFactory;
        private ConnectionStringBuilder _noSConnectionString;
        public ISessionId ClientSessionId { set; get; }
        private ISessionId CsSessionId { set; get; }
        private IDatabaseEngine _dbEngine;
        private readonly IClientDisconnection _clientDisconnection;
        private readonly List<ReaderInfo> _readersList;
        private readonly List<CommandInfo> _conmmandInfos;
        private string _shardName;
        private IStatsCollector _statsCollector;
        private string _database;
        private string _clientProcessID;

        public ClientSession(IDatabaseEngineFactory databaseEngineFactory, IClientDisconnection clientDisconnection, string shardName)
        {
            _databaseEngineFactory = databaseEngineFactory;
            if(_databaseEngineFactory is DatabaseEngineFactory)
                _dbEngine = _databaseEngineFactory.GetDatabaseEngine(null); 
            _clientDisconnection = clientDisconnection;
            _readersList = new List<ReaderInfo>();
            _conmmandInfos = new List<CommandInfo>();
            _shardName = shardName;
        }

        public ServerChannel ServerChannel
        {
            get { return _serverChannel; }
            set
            {
                _serverChannel = value;
                _serverChannel.AddRequestListener(this);
            }
        }

        internal string Database
        {
            get { return _database; }
        }

        internal string ClientProcessID
        {
            get { return _clientProcessID != null? _clientProcessID : ""; }
        }


        public IDatabaseEngine DbEngine
        {
            set { _dbEngine = value; }
            get { return _dbEngine; }
        }

        internal float ClientsBytesSent
        {
            get { return _serverChannel != null ? _serverChannel.ClientsBytesSent : 0; }
        }

        internal float ClientBytesReceived
        {
            get { return _serverChannel != null ? _serverChannel.ClientBytesReceived : 0; }
        }

        public object OnRequest(IRequest request)
        {

            IDBOperation dbOperation = null;
            IDBResponse response = null;
            RequestParser requestParser = null;
            try
            {

                requestParser = request as RequestParser;
                try
                {
                    dbOperation = requestParser.ParseRequest();
                }
                catch (Exception ex)
                {
                   if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                       LoggerManager.Instance.ShardLogger.Error("Error: ClientMgr.OnRequest() On Parse Request", ex.ToString());
                    throw ;
                }

                if (dbOperation == null)
                    throw new NullReferenceException("Database Operation");

                if (_shardName != null && _statsCollector == null)
                    _statsCollector = StatsManager.Instance.GetStatsCollector(new StatsIdentity(_shardName, dbOperation.Database));
                
                //Dispose Reader check is a temporary fix for Single Get Operation.
                if (dbOperation.OperationType != DatabaseOperationType.DisposeReader && _statsCollector != null)
                    _statsCollector.IncrementStatsValue(StatisticsType.RequestsPerSec);

                //TODO: when SessionId will be embedded into command from router, this code needs to be removed
                //if (dbOperation.SessionId == null)
                //    dbOperation.SessionId = new RouterSessionId();
                //dbOperation.SessionId = SessionId;

                //if (_dbEngine == null)
                //    throw new NullReferenceException("DBEngine (Client Manager)");

                //LoggerManager.Instance.SetThreadContext(new LoggerContext { ShardName = ((DataBaseEngine)DbEngine).NodeContext.LocalShardName, DatabaseName = dbOperation.Database });
                switch (dbOperation.OperationType)
                {
                    case DatabaseOperationType.Init:
                        response = _dbEngine.InitializeDatabase((InitDatabaseOperation) dbOperation);
                        if (response.IsSuccessfull)
                        {
                            _database = dbOperation.Database;
                        }
                        break;
                    case DatabaseOperationType.Insert:

                        response = _dbEngine.InsertDocuments((InsertDocumentsOperation) dbOperation);
                        break;

                    case DatabaseOperationType.Delete:

                        response = _dbEngine.DeleteDocuments((DeleteDocumentsOperation) dbOperation);
                        break;

                    case DatabaseOperationType.Get:

                        response = _dbEngine.GetDocuments((GetDocumentsOperation) dbOperation);
                        if (response != null && response.IsSuccessfull)
                        {
                            var getResponse = (GetDocumentsResponse) response;
                            if (!getResponse.DataChunk.IsLastChunk)
                            {
                                var readerInfo = new ReaderInfo
                                {
                                    DatabaseName = dbOperation.Database,
                                    CollectionName = dbOperation.Collection,
                                    ReaderId = getResponse.DataChunk.ReaderUID
                                };
                                lock (_readersList)
                                {
                                    _readersList.Add(readerInfo);
                                }
                            }
                        }
                        break;

                    case DatabaseOperationType.Update:

                        response = _dbEngine.UpdateDocuments((UpdateOperation) dbOperation);
                        break;

                    case DatabaseOperationType.Replace:
                        response = _dbEngine.ReplaceDocuments((IDocumentsWriteOperation) dbOperation);
                        break;

                    case DatabaseOperationType.ReadQuery:

                        response = _dbEngine.ExecuteReader((ReadQueryOperation) dbOperation);
                        if (response != null && response.IsSuccessfull)
                        {
                            var readQueryResponse = (ReadQueryResponse) response;
                            if (!readQueryResponse.DataChunk.IsLastChunk)
                            {
                                var readerInfo = new ReaderInfo
                                {
                                    DatabaseName = dbOperation.Database,
                                    CollectionName = dbOperation.Collection,
                                    ReaderId = readQueryResponse.DataChunk.ReaderUID
                                };
                                lock (_readersList)
                                {
                                    _readersList.Add(readerInfo);
                                }
                            }
                        }
                        break;

                    case DatabaseOperationType.WriteQuery:

                        response = _dbEngine.ExecuteNonQuery((WriteQueryOperation) dbOperation);
                        break;

                    case DatabaseOperationType.CreateSession:
                        break;

                    case DatabaseOperationType.GetChunk:

                        response = _dbEngine.GetDataChunk((GetChunkOperation) dbOperation);
                        break;

                    case DatabaseOperationType.DisposeReader:

                        var disposeReaderOperation = (DisposeReaderOperation) dbOperation;
                        response = _dbEngine.DiposeReader(disposeReaderOperation);
                        if (response != null && response.IsSuccessfull)
                        {
                            lock (_readersList)
                            {
                                _readersList.RemoveAll(
                                    x =>
                                        x.CollectionName == disposeReaderOperation.Collection &&
                                        x.DatabaseName == disposeReaderOperation.Database &&
                                        x.ReaderId == disposeReaderOperation.ReaderUID);
                            }
                        }
                        break;
                    case DatabaseOperationType.Authenticate:

                        var authenticationOperation = (AuthenticationOperation) dbOperation;
                        _noSConnectionString = new ConnectionStringBuilder(authenticationOperation.ConnectionString);
                        _dbEngine = _databaseEngineFactory.GetDatabaseEngine(_noSConnectionString);

                        authenticationOperation.Address = ServerChannel.PeerAddress;
                        response = _dbEngine.Authenticate(authenticationOperation);
                        AuthenticationResponse authResponse = response as AuthenticationResponse;
                        if (
                            !(authResponse.ServerToken.Status == Common.Security.SSPI.SecurityStatus.SecurityDisabled ||
                              authResponse.ServerToken.Status == Common.Security.SSPI.SecurityStatus.ContinueNeeded ||
                              authResponse.ServerToken.Status == Common.Security.SSPI.SecurityStatus.OK))
                            this.ChannelDisconnected(_serverChannel, "unauthenticated");
                        else
                        {
                            CsSessionId = dbOperation.SessionId;
                            _clientProcessID = authenticationOperation.ClientProcessID;
                        }
                        break;
                    default:
                        throw new Exception("Invalid Operation");
                }
            }
            catch (SerializationException se)
            {
                try
                {
                    if (requestParser == null)
                        requestParser = request.Message as RequestParser;

                    response = OperationMapper.GetResponse(requestParser.OperationType);
                    response.IsSuccessfull = false;
                    response.RequestId = requestParser.RequestId;
                    response.ErrorCode = se.ErrorCode != 0 ? se.ErrorCode : ErrorCodes.Cluster.UNKNOWN_ISSUE;
                    response.ErrorParams = se.Parameters;

                }
                catch (Exception)
                {
                    //Create response if exception occurs.
                    if (dbOperation != null)
                        response = dbOperation.CreateResponse();
                    response.ErrorCode = ErrorCodes.Cluster.UNKNOWN_ISSUE;
                }
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: ClientMgr.OnRequest()", se.ToString());

            }
            catch (DatabaseException de)
            {
                if (dbOperation != null) response = dbOperation.CreateResponse();
                if (response != null)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = de.ErrorCode != 0 ? de.ErrorCode : ErrorCodes.Cluster.UNKNOWN_ISSUE;
                    response.ErrorParams = de.Parameters;
                }
            }
            catch (ManagementException me)
            {
                if (dbOperation != null) response = dbOperation.CreateResponse();
                if (response != null)
                {
                    response.IsSuccessfull = false;
                    if (me.IsErrorCodeSpecified)
                    {
                        response.ErrorCode = me.ErrorCode;
                        response.ErrorParams = me.Parameters;
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: ClientMgr.OnRequest()",
                        e.Message + " StackTrace:" + e.StackTrace);

                if (dbOperation != null) response = dbOperation.CreateResponse();
                if (response != null)
                {
                    response.IsSuccessfull = false;
                    response.ErrorCode = ErrorCodes.Cluster.UNKNOWN_ISSUE;
                }
            }

            finally
            {
                try
                {
                    if (request.Channel.Connected)
                    {
                        request.Channel.SendMessage(response);
                    }
                }
                catch (ChannelException e)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("Error: ClientMgr.OnRequest()", e.ToString());
                }
                catch(Exception ex)
                {
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                        LoggerManager.Instance.ShardLogger.Error("Error: Exception on ClientMgr.OnRequest()", ex.ToString());
                }
            }

            return null;
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            if (channel == null) return;
            try
            {
                foreach (ReaderInfo readerInfo in _readersList)
                {
                    var operation = new DisposeReaderOperation
                    {
                        Database = readerInfo.DatabaseName,
                        Collection = readerInfo.CollectionName,
                        ReaderUID = readerInfo.ReaderId,
                        OperationType = DatabaseOperationType.DisposeReader,
                        SessionId = CsSessionId
                    };
                    try
                    {
                        _dbEngine.DiposeReader(operation);
                    }
                    catch (SecurityException exc)
                    {
                        if (LoggerManager.Instance.SecurityLogger != null &&
                            LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                            LoggerManager.Instance.SecurityLogger.Error("ClientSession.ChannelDisconected() ",exc);
                    }
                }
                _readersList.Clear();
                
                _conmmandInfos.Clear();

                _clientDisconnection.DisconnectClient(MiscUtil.GetAddressInfo(channel.PeerAddress.IpAddress,
                    channel.PeerAddress.Port));
                _databaseEngineFactory.Dispose(_noSConnectionString);
                try
                {
                    channel.Disconnect();
                }
                catch (ThreadAbortException e)
                {
                    //Todo: maybe log in future or something.
                }

                // Log Client Disconnected.
                if (LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                    LoggerManager.Instance.ServerLogger.Info("ClientSession.ChannelDisconnected",
                        "Client [" + MiscUtil.GetAddressInfo(channel.PeerAddress.IpAddress,
                    channel.PeerAddress.Port) + "] with sessionId'" + CsSessionId.SessionId + "' disconnected.");

                LoggerManager.Instance.SetThreadContext(new LoggerContext() {ShardName = "", DatabaseName = ""});

            }
            catch (Exception e)
            {
                try
                {
                    //Remove client session from session manager.
                    _clientDisconnection.DisconnectClient(MiscUtil.GetAddressInfo(channel.PeerAddress.IpAddress, channel.PeerAddress.Port));
                }
                catch (Exception ex) { }

                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Error("ClientSessionDisconnect",
                        e.Message + " StackTrace: " + e.StackTrace);
                }
            }
        }

        internal void SendNotification(object msg)
        {
            if (_serverChannel != null)
            {
                _serverChannel.SendMessage(msg, false);
            }
        }
    }
}
