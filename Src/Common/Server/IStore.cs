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
using Alachisoft.NosDB.Common.Server.Engine;
using System;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Server.Engine.Impl;

namespace Alachisoft.NosDB.Common.Server
{
    public interface IStore : IDisposable
    {
        IDBResponse CreateCollection(ICreateCollectionOperation operation);
        IDBResponse CreateIndex(ICreateIndexOperation operation);
        IDBResponse RenameIndex(IRenameIndexOperation operation);
        IDBResponse RecreateIndex(IRecreateIndexOperation operation);
        IDocumentsWriteResponse DeleteDocuments(IDocumentsWriteOperation operation);
        IDBResponse DropCollection(IDropCollectionOperation operation);
        IDBResponse DropIndex(IDropIndexOperation operation);
        IUpdateResponse ExecuteNonQuery(INonQueryOperation operation);
        IQueryResponse ExecuteReader(IQueryOperation operation);
        IGetResponse GetDocuments(IGetOperation operation);
        IDocumentsWriteResponse InsertDocuments(IDocumentsWriteOperation operation);
        IUpdateResponse UpdateDocuments(IUpdateOperation operation);
        IDocumentsWriteResponse ReplaceDocuments(IDocumentsWriteOperation operation);
        IGetChunkResponse GetDataChunk(IGetChunkOperation operation);
        IDBResponse DiposeReader(IDiposeReaderOperation operation);
        void Dispose(bool destroy);
        void Destroy();
    }
}
