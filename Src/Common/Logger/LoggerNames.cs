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
using System.Text;
using System.Collections;

namespace Alachisoft.NosDB.Common.Logger
{
    public enum LoggerNames
    {
        /// <summary>
        /// Any operation related to management opertion on both configuration and database host service.
        /// </summary>
        CONDBServer,
        /// <summary>
        /// Any activity related to authentication and authorization on both configuration and database host service.
        /// </summary>
        Security,
        /// <summary>
        /// All operations/events/info on starting/stopping and during running both configuration and database host service.
        /// </summary>
        Server,
        /// <summary>
        /// Any activity (about connectivity) taking placed between different shards or among replica nodes of a shard including membership,heart-beats,elections etc.
        /// </summary>
        Shards,
        /// <summary>
        /// Every activity related to replication among replica nodes within a single shard
        /// </summary>
        REP,
        /// <summary>
        /// Every activity related to state transfer between shards or within shard.
        /// </summary>
        StateXfer,
        /// <summary>
        /// Any activity related to storage of documents including caching, journalling, persistence and triggers.
        /// </summary>
        Storage,
        /// <summary>
        /// Any activity related to indexing of documents during application running.
        /// </summary>
        Indexing,
        /// <summary>
        /// log any details about execution of query starting from parsing to end execution (Including optimizer) + UDF
        /// </summary>
        Queries,
        /// <summary>
        /// Logs complete execution path of any opertion related to client (CRUD, Queries).
        /// </summary>
        ClientOp,
        /// <summary>
        /// Logs complete execution path of any opertion related to management (Create Collection, database, Shard, indexes, configuration related).
        /// </summary>
        ManagementOp,
        /// <summary>
        /// Logs complete execution path of a recovery job executing
        /// </summary>

        Recovery,
        /// <summary>
        /// Logs complete execution path of any opertion related to manager
        /// </summary>
        Manager,
        /// <summary>
        /// Logs complete execution path of any opertion related to REST API
        /// </summary>
        RestApi,
        /// <summary>
        /// Logs complete execution path of any operation related to Export-Import
        /// </summary>
        EXIM

    }
}