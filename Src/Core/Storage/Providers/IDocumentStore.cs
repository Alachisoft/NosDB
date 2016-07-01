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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Operations;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public interface IDocumentStore : IEnumerable<KeyValuePair<long, JSONDocument>>
    {
        bool Initialize(DatabaseContext _dbContext, CollectionConfiguration config);
        StorageManagerBase GetStorageManagerBase { get; }
        CollectionIndexManager IndexManager { get; }

        InsertResult<JSONDocument> InsertDocument(InsertOperation operation);
        DeleteResult<JSONDocument> DeleteDocument(RemoveOperation operation);
        GetResult<JSONDocument> GetDocument(GetOperation operation);
        UpdateResult<JSONDocument> UpdateDocument(UpdateOperation operation);

        void AddFailedOperation(Operation operation);

        void Dispose();
    }
}
