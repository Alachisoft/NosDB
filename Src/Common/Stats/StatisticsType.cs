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
using System.Text;

namespace Alachisoft.NosDB.Common.Stats
{
    public enum StatisticsType
    {
        /// <summary>Inserts pers seconds</summary>
        InsertsPerSec,
        /// <summary>Fetches pers seconds</summary>
        FetchesPerSec,
        /// <summary>Updates pers seconds</summary>
        UpdatesPerSec,
        /// <summary>Deletes pers seconds</summary>
        DeletesPerSec,
        /// <summary>Cache hits pers seconds</summary>
        CacheHitsPerSec,
        /// <summary>Cache mises pers seconds</summary>
        CacheMissesPerSec,
        /// <summary>Requests pers seconds</summary>
        RequestsPerSec,
        /// <summary>Average Fetch Time</summary>
        AvgFetchTime,
        /// <summary>Average Insert Time</summary>
        AvgInsertTime,
        /// <summary>Average Update Time</summary>
        AvgUpdateTime,
        /// <summary>Average Delete Time</summary>
        AvgDeleteTime,
        /// <summary>Average Query Execution Time</summary>
        AvgQueryExecutionTime,
        /// <summary>Average Fetch Time Base</summary>
        AvgFetchTimeBase,
        /// <summary>Average Insert Time Base</summary>
        AvgInsertTimeBase,
        /// <summary>Average Update Time Base</summary>
        AvgUpdateTimeBase,
        /// <summary>Average Delete Time Base</summary>
        AvgDeleteTimeBase,
        /// <summary>Average Query Execution Time Base</summary>
        AvgQueryExecutionTimeBase,
        /// <summary>Documents not Persisted UpTill</summary>
        PendingPersistentDocuments,
        /// <summary>Documents Persisted pers seconds</summary>
        DocumentsPersistedPerSec,
        /// <summary>Total Documents Count in the database</summary>
        DocumentCount,
        /// <summary>Average Size of Document in Database</summary>
        AvgDocumentSize,
        /// <summary>Total Database Size</summary>
        DatabaseSize,
        /// <summary>Items in Cache</summary>
        CacheCount,
        /// <summary>Size of Cache</summary>
        CacheSize,
        /// <summary>Cache Eviction pers seconds</summary>
        CacheEvictionPerSec,

        /// <summary>DataBalance per second</summary>
        DataBalancePerSec,
        /// <summary>State Transfer per second</summary>
        StateTransferPerSec,

        /// <summary>Pending Replicated Operations at Primary</summary>
        PendingReplicatedOperation

    }
}
