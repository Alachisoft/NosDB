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
using System.Data;
using System.Linq;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Core.Storage;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Core.Toplogies.Impl
{
    public sealed class DatabasesManager : IDatabasesManager//, IStateTxfrOperationListener
    {
        private IDictionary<String, IDatabaseStore> _databases = new Dictionary<String, IDatabaseStore>(StringComparer.InvariantCultureIgnoreCase);
        private IDictionary<String, IStore> _specializedDatabases = new Dictionary<String, IStore>();
        private IDictionary<String, IDictionary<String, IDistribution>> _distributionMap;
     
        public bool InitDatabase(string name)
        {
            lock (_databases)
            {
                if (_databases.ContainsKey(name))
                {
                    var dbStore = (DatabaseStore)_databases[name];
                    switch (dbStore.DatabaseContext.DatabaseMode)
                    {
                        case DatabaseMode.Online:
                            return true;
                        case DatabaseMode.Offline:
                            throw new DatabaseException(ErrorCodes.Database.Mode,
                                new[] {name, dbStore.DatabaseContext.DatabaseMode.ToString()});
                    }
                }
            }
            return false;
        }

       

        public IDictionary<String, IDictionary<String, IDistribution>> DistributionMap
        {
            get { return _distributionMap; }
            set
            {
                _distributionMap = value;
                SetDistributionMaps();
            }
        }
        public IDatabaseStore GetDatabase(string name)
        {
            IDatabaseStore store;
            _databases.TryGetValue(name, out store);
            return store;
        }

        public IStore GetSpecialDatabase(string name)
        {
            IStore store;
            _specializedDatabases.TryGetValue(name, out store);
            return store;
        }

        public bool Initialize(DatabaseConfigurations configurations, NodeContext context, IDictionary<String, IDictionary<String, IDistribution>> distributionMaps)
        {
            _distributionMap = distributionMaps;
          
            foreach (DatabaseConfiguration dbConfig in configurations.Configurations.Values)
            {
                lock (_databases)
                {
                    if (!_databases.ContainsKey(dbConfig.Name))
                    {
                        IDatabaseStore dbStore = new DatabaseStore();
                        IDictionary<string, IDistribution> colDistributions;
                        _distributionMap.TryGetValue(dbConfig.Name, out colDistributions);
                        if (((DatabaseStore)dbStore).Initialize(dbConfig, context, colDistributions))
                        {
                            _databases.Add(dbConfig.Name, dbStore);
                        }
                    }
                }
            }

            return true;
        }

        public void SetDistributionMaps()
        {
            foreach (KeyValuePair<string, IDatabaseStore> kvp in _databases)
            {
                if (!(kvp.Value is SystemDatabaseStore))
                    ((DatabaseStore)kvp.Value).CollectionDistributionMap = _distributionMap[kvp.Key];
            }
        }

        public bool CreateDatabase(DatabaseConfiguration configuration, NodeContext context, IDictionary<string, IDistribution> colDistributions)
        {
            lock (_databases)
            {
                if (!_databases.ContainsKey(configuration.Name))
                {
                    IDatabaseStore dbStore = new DatabaseStore();
                    if (((DatabaseStore)dbStore).Initialize(configuration, context, colDistributions))  
                    {
                        _databases.Add(configuration.Name, dbStore);
                        return true;
                    }
                }
                else
                {
                    throw new Exception("Database with specified name already exist.");
                }
            }

            return false;
        }

        public bool DropDatabase(string name, bool dropFiles)
        {
            if (!dropFiles) return StopDatabase(name);

            lock (_databases)
            {
                if (_databases.ContainsKey(name))
                {
                    IStore dbStore = _databases[name];
                    dbStore.Destroy();
                    _databases.Remove(name);
                    return true;
                }
                return false;
            }
        }

        private bool StopDatabase(string name)
        {
            lock (_databases)
            {
                if (_databases.ContainsKey(name))
                {
                    IStore dbStore = _databases[name];
                    dbStore.Dispose();
                    _databases.Remove(name);
                    return true;
                }
                return false;
            }
        }

        public bool DropAllDatabases()
        {
            lock (_databases)
            {
                string[] databases = _databases.Keys.ToArray();
                foreach (string database in databases)
                {
                    DropDatabase(database, false);
                }
                _databases.Clear();
                return true;
            }
        }

        public bool CreateSystemDatabase(DatabaseConfiguration configuration, NodeContext context)
        {
            lock (_databases)
            {
                if (!_databases.ContainsKey(configuration.Name))
                {
                    IDatabaseStore dbStore = new SystemDatabaseStore();

                    if (((DatabaseStore)dbStore).Initialize(configuration, context, null))
                    {
                        _databases.Add(configuration.Name, dbStore);
                        return true;
                    }
                }
                else
                    return false;
            }

            return false;
        }

        public IDBResponse CreateCollection(ICreateCollectionOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.CreateCollection(operation);
        }

        public IDBResponse CreateIndex(ICreateIndexOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.CreateIndex(operation);
        }

        public IDBResponse RenameIndex(IRenameIndexOperation operation)
        {
            DatabaseExists(operation.Database);
            
            IStore database = _databases[operation.Database];
            return database.RenameIndex(operation);
        }

        public IDBResponse RecreateIndex(IRecreateIndexOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];
            return database.RecreateIndex(operation);
        }

        public IDocumentsWriteResponse DeleteDocuments(IDocumentsWriteOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.DeleteDocuments(operation);
        }

        public IDBResponse DropCollection(IDropCollectionOperation operation)
        {

            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.DropCollection(operation);
        }

        public IDBResponse DropIndex(IDropIndexOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.DropIndex(operation);
        }

        public IUpdateResponse ExecuteNonQuery(INonQueryOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.ExecuteNonQuery(operation);
        }

        public IQueryResponse ExecuteReader(IQueryOperation operation)
        {
           DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.ExecuteReader(operation);
        }

        public IGetResponse GetDocuments(IGetOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.GetDocuments(operation);
        }
        
        public IDocumentsWriteResponse InsertDocuments(IDocumentsWriteOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.InsertDocuments(operation);
        }

        public IUpdateResponse UpdateDocuments(IUpdateOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.UpdateDocuments(operation);
        }

        public IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.ReplaceDocuments(operation);
        }

        public IGetChunkResponse GetDataChunk(IGetChunkOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.GetDataChunk(operation);
        }

       
        public IDBResponse DiposeReader(IDiposeReaderOperation operation)
        {
            DatabaseExists(operation.Database);

            IStore database = _databases[operation.Database];

            return database.DiposeReader(operation);
        }

        private void DatabaseExists(String databaseName)
        {
            if (databaseName == null) throw new ArgumentNullException("databaseName");

            if (!_databases.ContainsKey(databaseName))
                throw new DatabaseException(ErrorMessages.GetErrorMessage(ErrorCodes.Database.DATABASE_DOESNOT_EXIST, new String[] { databaseName }));
        }


        public void Dispose()
        {
            Dispose(false);
        }
        public void Dispose(bool destroy)
        {
            IDatabaseStore sysdbStore = null;
            foreach (KeyValuePair<String, IDatabaseStore> pair in _databases)
            {
                if (pair.Value is SystemDatabaseStore)
                    sysdbStore = pair.Value; // dispose it at the end
                else if (pair.Value != null)
                    pair.Value.Dispose(destroy);
            }
            if (sysdbStore != null) sysdbStore.Dispose(destroy);
        }

        #region IStateTransferOps Implementation

        /// <summary>
        /// Check for if collection is disposed already
        /// Used for State Transfer Purpose
        /// if Collection Name is not specified check for whole database disposed
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public bool HasDisposed(string dbName, string colName = "")
        {
            DatabaseExists(dbName);


            DatabaseStore database = (DatabaseStore)_databases[dbName];

            if (!String.IsNullOrEmpty(colName))
            {
                if (database.Collections.ContainsKey(colName))
                {
                    var collection = database.Collections[colName];
                    return collection != null ? collection.HasDisposed : true;
                }
            }

            return true;
        }

        #endregion

        #region IStateTxfrOperationListener Impelmentation

        /// <summary>
        /// Will recieve local operations from local DatabaseStateTransferManager class 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public object OnOperationRecieved(IStateTransferOperation operation)
        {
            String dbName = operation.TaskIdentity.DBName;

            if (!String.IsNullOrEmpty(dbName) && _databases.ContainsKey(dbName))
            {
                IStateTxfrOperationListener dbStore = _databases[dbName] as IStateTxfrOperationListener;
                if (dbStore != null) return dbStore.OnOperationRecieved(operation);
            }
            return null;
        }

        #endregion

        public ElectionId ElectionResult
        {
            set
            {
                foreach (var databaseStore in _databases.Values)
                {
                    var database = (DatabaseStore)databaseStore;
                    if (!(database is SystemDatabaseStore))
                        database.ElectionResult = value;
                }
            }
        }
       


       

      
        public void Destroy()
        {
            throw new NotImplementedException();
        }
        
        public bool SetDatabaseMode(string databaseName, DatabaseMode databaseMode)
        {
            lock (_databases)
            {
                if (_databases.ContainsKey(databaseName))
                {
                    var dbStore = (DatabaseStore) _databases[databaseName];
                    dbStore.DatabaseContext.DatabaseMode = databaseMode;
                    return true;
                }
                return false;
            }
        }

        public void IsOperationAllow(string database)
        {
            lock (_databases)
            {
                if (_databases.ContainsKey(database))
                {
                    var dbStore = (DatabaseStore) _databases[database];
                    switch (dbStore.DatabaseContext.DatabaseMode)
                    {
                        case DatabaseMode.Offline:
                            throw new DatabaseException(ErrorCodes.Database.Mode,
                                new[] {database, dbStore.DatabaseContext.DatabaseMode.ToString()});
                    }
                }
            }
        }

      

        #region IHandleBucketInfoTask
        public void StartBucketInfoTask()
        {
            if (_databases != null && _databases.Count > 0)
            {
                IList<String> databaseKeys = _databases.Keys.ToList();
                if (databaseKeys != null)
                {
                    foreach (var key in databaseKeys)
                    {
                        var store =(DatabaseStore)_databases[key];
                        if(store != null)
                        {
                            store.StartBucketInfoTask();
                        }
                    }
                }
            }
        }

        public void StopBucketInfoTask()
        {
            if (_databases != null && _databases.Count > 0)
            {
                IList<String> databaseKeys = _databases.Keys.ToList();
                if (databaseKeys != null)
                {
                    foreach (var key in databaseKeys)
                    {
                        var store = (DatabaseStore)_databases[key];
                        if (store != null)
                        {
                            store.StopBucketInfoTask();
                        }
                    }
                }
            }
        } 
        #endregion
    }
}
