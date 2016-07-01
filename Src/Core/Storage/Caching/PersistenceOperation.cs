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
using System.Diagnostics;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Core.Storage.Providers;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Core.Storage.Caching
{
    public abstract class PersistenceOperation
    {
        private long _operationId;
        private CacheItem _cacheItem;
        protected string _collection;
        public IOperationContext _context;

        public PersistenceOperation(long operationId, CacheItem cacheItem, string collection, IOperationContext context)
        {
            _cacheItem = cacheItem;
            _operationId = operationId;
            _collection = collection;
            if(context == null)
                context = new OperationContext();
            _context = context;
        }

        public abstract long RowId { get; }
        public CacheItem CacheItem { get { return _cacheItem; } }
        public long OperationID { get { return _operationId; } set { _operationId = value; } }
        public abstract StoreResult Execute(ITransaction transaction, IPersistenceManager persistenceManager);
        
    }
}
