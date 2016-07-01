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

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public class AllValue : AttributeValueMask
    {
        public AllValue() : base(FieldDataType.Null)
        {

        }

        public override int CompareTo(object obj)
        {
            return 0;
        }

        public override int GetHashCode()
        {
            return int.MinValue;
        }

        public override bool Equals(object obj)
        {
            return true;
        }

        public override string ValueInString
        {
            get { return "*"; }
        }

        public override Common.Enum.FieldDataType DataType
        {
            get { return FieldDataType.Value; }
        }

        public override Common.Enum.AttributeValueType Type
        {
            get { return AttributeValueType.All; }
        }

        public override IComparable this[int index]
        {
            get { return NullValue.Null; }
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
        }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
        }

        public override string ToString()
        {
            return ValueInString;
        }
    }
}
