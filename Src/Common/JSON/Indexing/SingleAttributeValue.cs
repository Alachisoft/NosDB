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
using log4net.Core;

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public class SingleAttributeValue : AttributeValue
    {
        private IComparable _value;
        private SortOrder _order;
        private FieldDataType _type;
        private TypeCode _actualType;

        public SingleAttributeValue(IComparable value, SortOrder order)
        {
            _value = JSONType.Enforce(value, out _type, out _actualType);
            _order = order;
        }

        public SingleAttributeValue(IComparable value):this(value, SortOrder.ASC)
        {}

        public override int CompareTo(object obj)
        {
            var attributeValue = obj as AttributeValue;
            if(attributeValue!=null)
                switch (attributeValue.Type)
                {
                    case AttributeValueType.Single:
                        return JSONComparer.Compare(_value, _type, _actualType, attributeValue[0],
                            attributeValue.DataType,
                            attributeValue.ActualType, _order);
                    case AttributeValueType.Mask:
                        return 0 - attributeValue.CompareTo(this);
                    case AttributeValueType.All:
                        return 0;
                    case AttributeValueType.None:
                        return -1;
                    case AttributeValueType.Null:
                        return attributeValue.Order == SortOrder.ASC ? int.MaxValue : int.MinValue;
                    default:
                        throw new IndexException(ErrorCodes.Indexes.ATTRIBUTEVALUE_TYPE_MISMATCH,
                            new[] {this.ValueInString, attributeValue.ValueInString});
                }
            throw new IndexException(ErrorCodes.Indexes.ATTRIBUTEVALUE_TYPE_MISMATCH,
                            new[] { this.ValueInString, obj.ToString() });
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override TypeCode ActualType
        {
            get { return _actualType; }
        }

        public override SortOrder Order { get { return _order; } }

        public IComparable Value { get { return _value; } }

        public override bool Equals(object obj)
        {
            var otherAttribute = obj as AttributeValue;
            if (otherAttribute != null)
            {
                switch (otherAttribute.Type)
                {
                    case AttributeValueType.Single:
                        return _value.Equals(otherAttribute[0]);
                    case AttributeValueType.All:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        public override string ValueInString
        {
            get { return _type.ToString() + ":" + (_value!=null?_value.ToString():"null"); }
        }

        public override Common.Enum.FieldDataType DataType
        {
            get { return _type; }
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _value = reader.ReadObject() as IComparable;
            _order = (SortOrder) reader.ReadByte();
            _type = (FieldDataType) reader.ReadByte();
            _actualType = (TypeCode) reader.ReadInt32();
        }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_value);
            writer.Write((byte)_order);
            writer.Write((byte) _type);
            writer.Write((int)_actualType);
        }
        
        public override AttributeValueType Type
        {
            get { return AttributeValueType.Single; }
        }

        public override IComparable this[int index]
        {
            get
            {
                if (index == 0) return _value;
                return NullValue.Null;
            }
        }

        public override string ToString()
        {
            return ValueInString;
        }

        public override object Clone()
        {
            return new SingleAttributeValue(_value, _order);
            
        }
    }
}
