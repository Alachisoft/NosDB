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
using Alachisoft.NosDB.Common.Storage.Transactions;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class FileTransaction : ITransaction
    {
        //dbID, transactions
        IDictionary<string, ITransaction> _siblingTransactions = new HashVector<string, ITransaction>();
        private readonly object  _siblingTxnLock = new object();
        private bool _isReadOnly;

        public FileTransaction(IEnumerable<string> dbIds, bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
            lock (_siblingTxnLock)
            {
                foreach (var dbId in dbIds)
                {
                    _siblingTransactions.Add(dbId, null);
                }
            }
        }

        public bool RemoveTransaction(string dbId)
        {
            lock (_siblingTxnLock)
            {
                return _siblingTransactions.Remove(dbId);
            }
        }

        public bool IsTransactionBegin(string dbId)
        {
            ITransaction transaction = null;
            _siblingTransactions.TryGetValue(dbId, out transaction);
            if (transaction == null)
                return false;
            return true;
        }

        public ITransaction GetTransaction(string dbId)
        {
            ITransaction transaction;
            lock (_siblingTxnLock)
            {
                _siblingTransactions.TryGetValue(dbId, out transaction);
            } 
            return transaction;
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
            get { throw new NotImplementedException(); }
        }

        public bool Begin(string dbId, ITransaction transaction)
        {
            if (!_siblingTransactions.ContainsKey(dbId))
            {
                lock (_siblingTxnLock)
                {
                    _siblingTransactions.Add(dbId, transaction);   
                }
            }
            if (_siblingTransactions[dbId] == null)
            {
                lock (_siblingTxnLock)
                {
                    _siblingTransactions[dbId] = transaction;
                }
            }
            return true;
        }
        
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
