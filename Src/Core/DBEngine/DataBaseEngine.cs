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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common;

namespace Alachisoft.NosDB.Core.DBEngine
{
    public class DataBaseEngine : IDatabaseEngine
    {
        private IDictionary<string, IList<ISessionId>> initializedDatabases;
        private NodeContext _nodeContext;

        private bool IsDistributorSession { set; get; }

        public DataBaseEngine(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        #region Security

        public ISecurityManager SecurityManager
        {
            set
            {
                _nodeContext.SecurityManager = value;
            }
            get
            {
                return _nodeContext.SecurityManager;
            }
        }

        public IDBResponse Authenticate(IAuthenticationOperation operation)
        {
            var response = operation.CreateResponse() as AuthenticationResponse;
            try
            {
                AuthToken nextAuthToken = SecurityManager.Authenticate(_nodeContext.LocalShardName, operation,SSPIUtility.IsLocalServer(operation.Address.IpAddress), IsDistributorSession ? "NT SERVICE\\" + MiscUtil.NOSDB_DISTSVC_NAME : null);
                if (response != null)
                {
                    response.ServerToken = nextAuthToken;
                    return response;
                }
            }
            catch (SecurityException noSSecurityException)
            {
                if (response != null)
                {
                    response.ErrorCode = noSSecurityException.ErrorCode;
                    return response;
                }
            }
            return response;
        }

        public bool Authorize(IDBOperation operation, bool isInitializeCall)
        {
            if (_nodeContext.StatusLatch.IsAnyBitsSet(NodeStatus.Initializing))
                _nodeContext.StatusLatch.WaitForAny(NodeStatus.Running);

            bool isInitialized = true;
            if (!isInitializeCall)
            //This portion is to verify if database is initialized or not, it has nothing to do with security (authorization or authentication)
            {
                ClientSessionId clientSessionId = operation.SessionId as ClientSessionId;
                if (!(initializedDatabases != null && initializedDatabases.ContainsKey(operation.Database) && (initializedDatabases[operation.Database].Contains(operation.SessionId) || (clientSessionId != null && initializedDatabases[operation.Database].Contains(clientSessionId.RouterSessionId)))))
                    isInitialized = false;
                    //throw new DistributorException(ErrorCodes.Distributor.DATABASE_NOT_INITIALIZED, new[] {operation.Database});
            }
            //Authorization
            bool isAuthorized = false;
            ISessionId sessionId = operation.SessionId;
            Permission permission = null; 
            switch (operation.OperationType)
            {
                case DatabaseOperationType.Get:
                case DatabaseOperationType.GetChunk:
                case DatabaseOperationType.ReadQuery:
                case DatabaseOperationType.DisposeReader:
                    permission = Permission.Read;
                    break;
                case DatabaseOperationType.Insert:
                case DatabaseOperationType.Delete:
                case DatabaseOperationType.Replace:
                case DatabaseOperationType.Update:
                case DatabaseOperationType.WriteQuery:
             
                    permission = Permission.Write;
                    break;
                case DatabaseOperationType.Init:
                    permission = Permission.Init;
                    break;

            }

            //if(LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
            //{
            //    LoggerManager.Instance.SecurityLogger.Info("DataBaseEngine.Authorize", "Operation: " + operation.OperationType);
            //}

            if (permission != null)
            {
                ResourceId resourceId;
                ResourceId superResourceId;
                Security.Impl.SecurityManager.GetSecurityInformation(permission, operation.Database, out resourceId, out superResourceId, null);
                isAuthorized = SecurityManager.Authorize(_nodeContext.LocalShardName, sessionId, resourceId, superResourceId, permission);
                _nodeContext.TopologyImpl.IsOpertionAllow(operation.Database);
            }

            if (isAuthorized && !isInitialized)
            {
                this.InitializeDatabase(new InitDatabaseOperation() { Database = operation.Database, SessionId = operation.SessionId });
            }

            return isAuthorized;
        }

       #endregion

        #region IDatabaseEngine Operations

        public void Start()
        {
            Start(null);
        }

        public void Start(ClusterConfiguration clusterConfig)
        {
            if (_nodeContext == null)
                throw new Exception("Node Context is Not Set");

            if (string.IsNullOrEmpty(_nodeContext.ClusterName))
            {
                throw new Exception("Cluster Name not specified.");
            }
            string clusterName = _nodeContext.ClusterName;

            ClusterConfiguration clusterConf = clusterConfig;
            
            if (clusterConf==null)
            {
                clusterConf =   _nodeContext.ConfigurationSession.GetDatabaseClusterConfiguration(_nodeContext.ClusterName);
            }

            if (clusterConf == null)
                throw new Exception("Cluster Configuration is Not Set");
            _nodeContext.DatabasesManager = new DatabasesManager();
            _nodeContext.TopologyImpl = new PartitionOfReplica(_nodeContext);

            _nodeContext.TopologyImpl.Initialize(clusterConf);

            if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.ServerLogger.Info("DataBaseEngine.start()", "DataBase Engine server started successfully.");
            }
        }

        public void Stop(bool destroy)
        {
            Dispose(destroy);
        }

        public object ExecuteScalar(IQueryOperation operation)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IStore Operations
        public IQueryResponse ExecuteReader(IQueryOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.ExecuteReader(operation);
            }
            return null;
        }

        public IUpdateResponse ExecuteNonQuery(INonQueryOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.ExecuteNonQuery(operation);
            }
            return null;
        }

        public IGetResponse GetDocuments(IGetOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.GetDocuments(operation);
            }
            return null;
        }

        public IDocumentsWriteResponse InsertDocuments(IDocumentsWriteOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.InsertDocuments(operation);
            }
            return null;
        }

        public IUpdateResponse UpdateDocuments(IUpdateOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.UpdateDocuments(operation);
            }
            return null;
        }

        public IDocumentsWriteResponse DeleteDocuments(IDocumentsWriteOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.DeleteDocuments(operation);
            }
            return null;
        }

        public IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.ReplaceDocuments(operation);
            }
            return null;
        }

        public IDBResponse CreateCollection(ICreateCollectionOperation operation)
        {
            return _nodeContext.TopologyImpl.CreateCollection(operation);
        }

        public IDBResponse DropCollection(IDropCollectionOperation operation)
        {
            return _nodeContext.TopologyImpl.DropCollection(operation);
        }

        public IDBResponse CreateIndex(ICreateIndexOperation operation)
        {
            return _nodeContext.TopologyImpl.CreateIndex(operation);
        }

        public IDBResponse RenameIndex(IRenameIndexOperation operation)
        {
            return _nodeContext.TopologyImpl.RenameIndex(operation);
        }

        public IDBResponse RecreateIndex(IRecreateIndexOperation operation)
        {
            return _nodeContext.TopologyImpl.RecreateIndex(operation);
        }

        public IDBResponse DropIndex(IDropIndexOperation operation)
        {
            return _nodeContext.TopologyImpl.DropIndex(operation);
        }

        public void Dispose()
        {
            Dispose(false);
        }
        public void Dispose(bool destroy)
        {
            this._nodeContext.StatusLatch.SetStatusBit(Alachisoft.NosDB.Common.Stats.NodeStatus.Stopped, Alachisoft.NosDB.Common.Stats.NodeStatus.InStateTxfer|Alachisoft.NosDB.Common.Stats.NodeStatus.Initializing | 0);

            _nodeContext.TopologyImpl.Dispose(destroy);

        }
        public IGetChunkResponse GetDataChunk(IGetChunkOperation operation)
        {
            try
            {
                if (Authorize(operation, false))
                {
                    return _nodeContext.TopologyImpl.GetDataChunk(operation);
                }
                return null;
            }
            catch (DatabaseException dbe)
            {
                IGetChunkResponse response = operation.CreateResponse() as IGetChunkResponse;
                response.IsSuccessfull = false;
                response.ErrorCode = dbe.ErrorCode;
                response.ErrorParams = dbe.Parameters;
                response.DataChunk.ReaderUID = operation.ReaderUID;
                return response;
            }
            catch (Exception e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: DataBaseEngine.GetDataChunk()", e.Message + " StackTrace:" + e.StackTrace);
                IGetChunkResponse response = operation.CreateResponse() as IGetChunkResponse;
                response.IsSuccessfull = false;
                response.DataChunk.ReaderUID = operation.ReaderUID;
                return response;
            }
        }

        public IDBResponse DiposeReader(IDiposeReaderOperation operation)
        {
            if (Authorize(operation, false))
            {
                return _nodeContext.TopologyImpl.DiposeReader(operation);
            }
            return null;
        }


        #endregion

        public IDBResponse InitializeDatabase(InitDatabaseOperation initDatabaseOperation)
        {
            if (Authorize(initDatabaseOperation, true))
            {
                var response = _nodeContext.TopologyImpl.InitDatabase(initDatabaseOperation) as InitDatabaseResponse;
                if (response != null && response.IsInitialized)
                {
                    if (initializedDatabases == null)
                        initializedDatabases = new Dictionary<string, IList<ISessionId>>(StringComparer.CurrentCultureIgnoreCase);

                    if (!initializedDatabases.ContainsKey(initDatabaseOperation.Database))
                        initializedDatabases[initDatabaseOperation.Database] = new List<ISessionId>();

                    if (!initializedDatabases[initDatabaseOperation.Database].Contains(initDatabaseOperation.SessionId))
                    {
                        initializedDatabases[initDatabaseOperation.Database].Add(initDatabaseOperation.SessionId);

                        // Log Client Connected
                        if (LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                            LoggerManager.Instance.ServerLogger.Info("DataBaseEngine.InitializeDatabase", "Client [" + initDatabaseOperation.Source + "] with sessionId '" + initDatabaseOperation.SessionId.SessionId + "' connected with database '" + initDatabaseOperation.Database + "'.");
                    }

                    
                    

                    response.IsSuccessfull = true;
                }
                return response;
            }
            return null;
        }

      


        public ClusterConfiguration ClusterConfig
        {
            set;
            get;
        }


        public void MarkDistributorSession(bool IsDistributorSession)
        {
            this.IsDistributorSession = IsDistributorSession;
        }


        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
