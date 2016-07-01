using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NoSQL.Common.Server.Engine;

namespace Alachisoft.NoSQL.Core.Toplogies.Impl.StateTransfer
{

    // NTO: provide implementation for this operation class
    class StateTransferInsertOperation : IInsertOperation
    {

        public IList<IJSONDocument> Documents
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

        public WriteConcern WriteConcern
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

        public long RequestId
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

        public string Database
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

        public string Collection
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

        public IOperationContext Context
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

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public Common.Server.Engine.Impl.DatabaseOperationType OperationType
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

        public IDBOperation Clone()
        {
            throw new NotImplementedException();
        }

        public IDBResponse CreateResponse()
        {
            throw new NotImplementedException();
        }
    }
}
