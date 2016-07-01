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
using System.Collections.Generic;
using System.Threading;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Memory;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Providers;


namespace Alachisoft.NosDB.Core.Storage
{
    public class DatabaseContext
    {
        private DatabaseConfiguration _configurations;
        private long _lastOperationId;
        private readonly object _commitIdLock = new object();
        private long _oppIdToCommit;

        public StorageManagerBase StorageManager { get; set; }

        public PersistenceManager PersistenceManager { get; set; }

        public BPlusPersistanceManager IndexPersistanceManager { get; set; }

        public DatabaseMode DatabaseMode { get; set; }

        public long OppIdToCommit
        {
            get
            {
                lock (_commitIdLock)
                {
                    return _oppIdToCommit;
                }
            }
            set
            {
                lock (_commitIdLock)
                {
                    _oppIdToCommit = value;
                }
            }
        }

        public string DeploymentPath { get; set; }

        //public IPersistenceProvider MetadataPersister { get; set; }

        public DatabaseConfiguration DatabaseConfigurations
        {
            set { _configurations = value; }
            get { return _configurations; }
        }

        public StatsIdentity StatsIdentity { get; set; }

        public LockManager<string, string, DocumentKey> LockManager { get; set; }

        public string DatabaseName
        {
            get { return _configurations.Name; }
        }

        public CacheSpace CacheSpace { get; set; }

        public IDictionary<long, string> UnpersistedOperations { get; set; }

        public long LastOperationId
        {
            get { return _lastOperationId; }
            set { _lastOperationId = value; }
        }
        public long GenerateOperationId()
        {
            return Interlocked.Increment(ref _lastOperationId);
        }

    }
}
