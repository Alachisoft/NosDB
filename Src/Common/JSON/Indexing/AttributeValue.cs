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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public abstract class AttributeValue : IComparable, ICompactSerializable, ICloneable
    {
        public abstract int CompareTo(object obj);
        public abstract string ValueInString{ get; }
        public abstract FieldDataType DataType { get; }
        public abstract AttributeValueType Type { get; }
        public abstract IComparable this[int index] { get; }
        public abstract TypeCode ActualType { get; }
        public abstract SortOrder Order { get; }

        public abstract void Deserialize(Common.Serialization.IO.CompactReader reader);

        public abstract void Serialize(Common.Serialization.IO.CompactWriter writer);
        public abstract object Clone();

    }
}
