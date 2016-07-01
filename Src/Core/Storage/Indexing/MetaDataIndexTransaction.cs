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
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Transactions;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class MetaDataIndexTransaction : ITransaction
    {
        private IDictionary<DocumentKey, MetaDataIndexOperation> _operationsPerformedOnMetaData;
        private ITransaction _persistenceTransaction;
        private readonly object _lock = new object();

        public MetaDataIndexTransaction(ITransaction persistanceTransaction)
        {
            _persistenceTransaction = persistanceTransaction;
            _operationsPerformedOnMetaData = new HashVector<DocumentKey, MetaDataIndexOperation>();
        }

        public IDictionary<DocumentKey, MetaDataIndexOperation> OperationsPerformedOnMetaData
        {
            get { return _operationsPerformedOnMetaData; }
        }

        public ITransaction ParentTransaction
        {
            get { throw new NotImplementedException(); }
        }

        public ITransactable Initiator
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public long Id
        {
            get { throw new NotImplementedException(); }
        }

        public object InnerObject
        {
            get { return _persistenceTransaction; }
        }

        public bool IsReadOnly
        {
            get { return _persistenceTransaction.IsReadOnly; }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void AddOperation(DocumentKey key, MetaDataIndexOperation operation)
        {
            lock (_lock)
            {
                if (!_operationsPerformedOnMetaData.ContainsKey(key))
                    _operationsPerformedOnMetaData.Add(key, operation);
            }
        }

        internal void ClearPerformedOperations()
        {
            lock (_lock)
            {
                _operationsPerformedOnMetaData.Clear();
            }
        }

        internal bool ContainsKey(DocumentKey documentKey)
        {
            lock (_lock)
            {
                return _operationsPerformedOnMetaData.ContainsKey(documentKey);
            }
        }
    }
}