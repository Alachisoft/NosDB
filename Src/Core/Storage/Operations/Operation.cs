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
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;

namespace Alachisoft.NosDB.Core.Storage.Operations
{
    public abstract class Operation : ICompactSerializable
    {
        private long _operationId = -1;
        private long _rowId = -1;
        private string _collection = "";
        private IOperationContext _context = new OperationContext();

        public IOperationContext Context 
        { 
            get { return _context; }
            set { _context = value; }
        }

        public string Collection
        {
            get { return _collection; }
            set { _collection = value; }
        }        

        public long RowId
        {
            get { return _rowId; }
            set { _rowId = value; }
        }

        public long OperationId
        {
            get { return _operationId; }
            set { _operationId = value; }
        }

        public abstract OperationType OperationType { get; }


        public virtual void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _operationId = reader.ReadInt64();
            _rowId = reader.ReadInt64();
            _collection = reader.ReadString();
            _context = new OperationContext();
        }

        public virtual void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(_operationId);
            writer.Write(_rowId);
            writer.Write(_collection);
        }
    }
}
