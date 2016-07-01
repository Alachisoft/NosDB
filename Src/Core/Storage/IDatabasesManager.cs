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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Core.Storage;

namespace Alachisoft.NosDB.Core.Toplogies.Impl
{

    public interface IDatabasesManager :  IDatabaseStore, IHandleBucketInfoTask

    {
        bool Initialize(DatabaseConfigurations configurations, NodeContext context, IDictionary<String, IDictionary<String, IDistribution>> distributionMaps);
        bool CreateDatabase(DatabaseConfiguration configuration, NodeContext context, IDictionary<string, IDistribution> colDistributions);
        bool DropDatabase(string name,bool dropFiles);
        bool DropAllDatabases();
        bool CreateSystemDatabase(DatabaseConfiguration configuration, NodeContext context);
        IDictionary<String, IDictionary<String, IDistribution>> DistributionMap { get; set; }
        bool HasDisposed(string dbName, string colName);
        object OnOperationRecieved(IStateTransferOperation operation);
        IDatabaseStore GetDatabase(string name);
        bool InitDatabase(string name);

        bool SetDatabaseMode(string databaseName, Common.DataStructures.DatabaseMode databaseMode);

        void IsOperationAllow(string database);
        Common.Configuration.Services.ElectionId ElectionResult { set; }

    }
}
