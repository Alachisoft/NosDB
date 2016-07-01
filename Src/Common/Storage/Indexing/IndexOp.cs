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
using Alachisoft.NosDB.Common.JSON.Indexing;

namespace Alachisoft.NosDB.Common.Storage.Indexing
{
    public abstract class IndexOp<T>
    {
        protected T _rowId;
        protected AttributeValue _key;
        
        public T RowId { get { return _rowId; } }

        public AttributeValue Key { get { return _key; } }

        public abstract void MergeWith(IDictionary<T, byte> initialSet);
        
        public abstract OpType OperationType { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _rowId.GetHashCode();
                return hash;
            }
        }
    }
}
