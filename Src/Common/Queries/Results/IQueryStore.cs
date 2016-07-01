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
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public interface IQueryStore : IEnumerable<IJSONDocument>, IPrintable
    {
        IJSONDocument GetDocument(long rowId, IOperationContext context);
        IJSONDocument GetDocument(DocumentKey key, IOperationContext context);
        bool UpdateDocument(long rowId, IJSONDocument newDocument, IOperationContext operationContext);
        bool DeleteDocument(long rowId, IOperationContext operationContext);
        bool InsertDocument(IJSONDocument document, IOperationContext operationContext);
        long GetRowId(DocumentKey key);
        bool ContainsKey(DocumentKey key);
        long DocumentCount { get; }
        long Size { get; }
        bool HasDisposed { get; }
        Latch Status { get; }
    }
}
