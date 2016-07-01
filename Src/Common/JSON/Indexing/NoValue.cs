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
    public class NoValue : AttributeValue
    {
        public static readonly NoValue NOValue = new NoValue();

        public override int CompareTo(object obj)
        {
            var value = obj as AttributeValue;
            if(value!=null)
            switch (value.Type)
            {
                    case AttributeValueType.Mask:
                    return 0 - value.CompareTo(this);
                    case AttributeValueType.None:
                    case AttributeValueType.All:
                    return 0;
                default:
                    return int.MaxValue;
            }
            return int.MaxValue;
        }

        public override bool Equals(object obj)
        {

            var value = obj as AttributeValue;
            if (value != null)
                switch (value.Type)
                {
                    case AttributeValueType.None:
                    case AttributeValueType.All:
                        return true;
                    default:
                        return false;
                }
            return false;
        }

        public override string ValueInString
        {
            get { return ""; }
        }

        public override SortOrder Order { get {return SortOrder.ASC;} }
        public override TypeCode ActualType { get {return TypeCode.Empty;} }

        public override Enum.FieldDataType DataType
        {
            get { return FieldDataType.Empty; }
        }

        public override Enum.AttributeValueType Type
        {
            get { return AttributeValueType.None; }
        }

        public override IComparable this[int index]
        {
            get { return null; }
        }

        public override void Deserialize(Serialization.IO.CompactReader reader)
        {

        }

        public override void Serialize(Serialization.IO.CompactWriter writer)
        {

        }

        public override string ToString()
        {
            return ValueInString;
        }

        public override object Clone()
        {
            return NOValue;
        }
    }
}
