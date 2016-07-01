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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public class MultiAttributeValue : AttributeValue
    {
        private List<AttributeValue> _values; 

        public MultiAttributeValue(List<IComparable> values, List<SortOrder> orders)
        {
            _values = new List<AttributeValue>();
            bool implicitOrder = orders == null;
            for (int i = 0; i < values.Count; i++)
            {
                var attribute = new SingleAttributeValue(values[i], implicitOrder ? SortOrder.ASC : orders[i]);
                _values.Add(attribute);
            }
        }

        public MultiAttributeValue(List<IComparable> values) : this(values, null) { }

        public MultiAttributeValue(AttributeValue value)
        {
            _values = new List<AttributeValue>();
            _values.Add(value);
        }

        public MultiAttributeValue(AttributeValue[] values)
        {
            _values = new List<AttributeValue>(values);
        }

        public MultiAttributeValue(List<AttributeValue> values)
        {
            _values = values;
        }
        
        public List<AttributeValue> Values
        {
            get { return _values; }
        }

        public override int CompareTo(object obj)
        {
            var nullValue = obj as NullValue;
            if (nullValue != null)
            {
                return nullValue.Order == SortOrder.ASC ? int.MaxValue : int.MinValue;
            }

            int typeRule = DataType.CompareTo(((AttributeValue) obj).DataType);

            if (typeRule == 0)
            {
                bool countNotEquals = false;

                if (!(obj is MultiAttributeValue))
                    throw new ArgumentException();

                var other = obj as MultiAttributeValue;
                int attribCount = _values.Count;

                if (countNotEquals = (_values.Count != other._values.Count))
                {
                    attribCount = Math.Min(_values.Count, other.Values.Count);
                }

                int result = 0;
                for (int i = 0; i < attribCount; i++)
                {
                    if ((result = _values[i].CompareTo(other._values[i])) != 0)
                        return result;
                }
                if (countNotEquals)
                {
                    if (_values.Count < other._values.Count)
                        return -1;
                    return 1;
                }
                return result;
            }
            return typeRule;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var value in _values)
                {
                    hash = hash * 31 + value.GetHashCode();
                }
                return hash;
            }
        }

        public override string ValueInString
        {
            get {
                if (_values != null)
                {
                    string str = string.Empty;
                    for (int i = 0; i < _values.Count; i++)
                    {
                        str += _values[i].ValueInString + (i != _values.Count - 1 ? "," : string.Empty);
                    }
                    return str;
                }
                return string.Empty;
            }
        }

        public override SortOrder Order { get {return SortOrder.ASC;} }

        public override TypeCode ActualType { get {return TypeCode.Object;} }

        public override bool Equals(object obj)
        {
            var otherAttr = obj as MultiAttributeValue;
            if (otherAttr != null)
            {
                if (_values.Count == otherAttr._values.Count)
                {
                    for (int i = 0; i < _values.Count; i++)
                    {
                        if (!_values[i].Equals(otherAttr._values[i]))
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public override Common.Enum.FieldDataType DataType
        {
            get { return FieldDataType.MultiValue; }
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _values = SerializationUtility.DeserializeList<AttributeValue>(reader);
        }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeList(_values,writer);
        }

        public override IComparable this[int index]
        {
            get { return _values.Count < index ? _values[index][0] : NullValue.Null; }
        }

        public List<AttributeValue> InternalAttributes { get { return _values; } } 

        public override AttributeValueType Type
        {
            get { return AttributeValueType.Multiple; }
        }

        public override string ToString()
        {
            return ValueInString;
        }

        public override object Clone()
        {
            var list = new List<AttributeValue>(_values.Count);
            foreach (var attributeValue in _values)
            {
                list.Add(attributeValue.Clone() as AttributeValue);
            }
            return new MultiAttributeValue(list);
        }
    }
}
