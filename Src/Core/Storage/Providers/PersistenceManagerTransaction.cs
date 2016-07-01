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
using Alachisoft.NosDB.Common.Storage.Transactions;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class PersistenceManagerTransaction :ITransaction
    {
        private FileTransaction _dataTransaction;
        private ITransaction _metadataTransaction;

        public FileTransaction DataTransaction
        {
            get { return _dataTransaction; }
            set { _dataTransaction = value; }
        }

        public ITransaction MetadataTransaction
        {
            get { return _metadataTransaction; }
            set { _metadataTransaction = value; }
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

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public ITransaction GetTransaction(string dbId)
        {
            throw new NotImplementedException();
        }

        public bool IsTransactionBegin(string dbId)
        {
            throw new NotImplementedException();
        }

        public bool Begin(string dbId, ITransaction transaction)
        {
            throw new NotImplementedException();
        }

        public bool Begin()
        {
            throw new NotImplementedException();
        }

        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public bool Rollback()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
