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
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Core.Storage.Queries.Util;

namespace Alachisoft.NosDB.Common.JSON
{
    public class NumberJsonValue : IJsonValue
    {
        private object _value;
        private TypeCode _actualType;
        public NumberJsonValue(object number)
        {
            _value = number;
            Type valueType = _value.GetType();
            _actualType = Type.GetTypeCode(valueType);
        }

        public FieldDataType DataType
        {
            get { return FieldDataType.Number; }
        }

        public int CompareTo(object obj)
        {
            IJsonValue secondJsonValue = obj as IJsonValue;
            if (secondJsonValue == null) throw new ArgumentException("Object must be of type IJSONValue");

            if (obj is EmbeddedList)
                return EmbeddedHelper.Compare(this, obj as EmbeddedList);

            return JSONComparer.Compare((IComparable) _value, DataType, NativeType, secondJsonValue.Value,
                secondJsonValue.DataType, secondJsonValue.NativeType, SortOrder.ASC);
        }


        public object Value
        {
            get
            {
                switch (_actualType)
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return Convert.ToInt64(_value);
                    default:
                        return Convert.ToDouble(_value);
                }
            }
        }

        public override bool Equals(object obj)
        {
            NumberJsonValue secondJsonValue = obj as NumberJsonValue;
            
            if (secondJsonValue == null)
                return false;
           
            switch (_actualType)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return Convert.ToInt64(_value).Equals(Convert.ToInt64(secondJsonValue.Value));
                default:
                    return Convert.ToDouble(_value).Equals(Convert.ToDouble(secondJsonValue.Value));
            }
        }


        public TypeCode NativeType
        {
            get { return _actualType; }
        }
    }
}
