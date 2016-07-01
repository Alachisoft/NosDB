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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies
{

    public interface IDatabaseTopology : IDatabaseStore,IRecoveryOperationListner
    {
        /// <summary>
        /// Initialize Topology with given Cluster Configuration, Topology initialization include cluster initialization as well.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        bool Initialize(ClusterConfiguration configuration);

        /// <summary>
        /// Create Database on this node with give Configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        bool CreateDatabase(DatabaseConfiguration configuration, IDictionary<string, IDistributionStrategy> collectionStrategy);


        bool DropDatabase(string databaseName, bool dropFiles);

        Common.Server.Engine.IDBResponse InitDatabase(InitDatabaseOperation initDatabaseOperation);

        bool SetDatabaseMode(string databaseName, DatabaseMode databaseMode);
        void IsOpertionAllow(string database);
    }
}
