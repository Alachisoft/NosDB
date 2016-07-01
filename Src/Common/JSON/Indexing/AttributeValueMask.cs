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
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public  class AttributeValueMask : AttributeValue
    {
        protected FieldDataType type;

        public AttributeValueMask(FieldDataType typeMask)
        {
            type = typeMask;
        }

        public override int CompareTo(object obj)
        {
            var value = obj as AttributeValue;
            if (value != null)
            {
                switch (value.Type)
                {
                    case AttributeValueType.All:
                        return 0;
                    case AttributeValueType.Single:
                    case AttributeValueType.Mask:
                    case AttributeValueType.Multiple:
                    case AttributeValueType.Null:
                        case AttributeValueType.None:
                        return type.CompareTo(value.DataType);
                    default:
                        throw new IndexException(ErrorCodes.Indexes.ATTRIBUTEVALUE_TYPE_MISMATCH,
                            new[] { this.ValueInString, value.ValueInString });
                }
            }
            throw new IndexException(ErrorCodes.Indexes.ATTRIBUTEVALUE_TYPE_MISMATCH,
                            new[] { this.ValueInString, value.ToString() });
        }

        public override bool Equals(object obj)
        {
            var value = obj as AttributeValue;
            if (value != null)
            {
                switch (value.Type)
                {
                    case AttributeValueType.All:
                        return true;
                    case AttributeValueType.Single:
                    case AttributeValueType.Mask:
                    case AttributeValueType.Multiple:
                    case AttributeValueType.Null:
                    case AttributeValueType.None:
                        return type.Equals(value.DataType);
                    default:
                        return false;
                }
            }
            return false;
        }

        public override TypeCode ActualType { get {return TypeCode.Empty;} }
        public override SortOrder Order { get {return SortOrder.ASC;} }

        public override string ValueInString
        {
            get { return type.ToString(); }
        }

        public override Common.Enum.FieldDataType DataType
        {
            get { return type; }
        }

        public override Common.Enum.AttributeValueType Type
        {
            get { return AttributeValueType.Mask; }
        }

        public override IComparable this[int index]
        {
            get
            {
                return NullValue.Null;
            }
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            type = (FieldDataType)reader.ReadByte();
        }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write((byte) type);
        }

        public override object Clone()
        {
            return new AttributeValueMask(type);
        }
    }
}
