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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Core.Storage.Providers;
using Alachisoft.NosDB.Core.Queries.Results;
using System;


namespace Alachisoft.NosDB.Core.Storage.Collections
{
    public interface ICollectionStore : IDisposable, IQueryStore
    {
        string Name { get; }
        string ShardName { get; set; }
        IDocumentStore DocumentStore { get; }
        DatabaseContext DbContext { get; }
        IDistribution Distribution { get; set; }
        IDictionary<int, KeyValuePair<HashMapBucket, BucketStatistics>> BucketStatistics { get; }
        long Size { get; }
        int RowToFileIndexCount { get; }

        bool Initialize(CollectionConfiguration configuration, QueryResultManager queryResultManager, IStore databaseStore, IDistribution distribution);

        IDBResponse CreateIndex(ICreateIndexOperation operation);
        IDBResponse RenameIndex(IRenameIndexOperation operation);
        IDBResponse RecreateIndex(IRecreateIndexOperation operation);
        IDocumentsWriteResponse DeleteDocuments(IDocumentsWriteOperation operation);
        IDBResponse DropIndex(IDropIndexOperation operation);
        IUpdateResponse ExecuteNonQuery(INonQueryOperation operation);
        IQueryResponse ExecuteQuery(IQueryOperation operation);
        IGetResponse GetDocuments(IGetOperation operation);
        IDocumentsWriteResponse InsertDocuments(IDocumentsWriteOperation operation);
        IUpdateResponse UpdateDocuments(IUpdateOperation operation);
        IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation);
        IGetChunkResponse GetDataChunk(IGetChunkOperation operation);
        IDBResponse DiposeReader(IDiposeReaderOperation operation);
        void Destroy(bool destroyingNode);
        void PopulateData();
    }

}
