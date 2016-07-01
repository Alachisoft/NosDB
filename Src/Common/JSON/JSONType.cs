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
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON
{
    public static class JSONType
    {
        public static IJSONDocument CreateNew()
        {
            return new JSONDocument();
        }

        public static bool IsSpecifiedType(object value, FieldDataType type)
        {
            Type valueType = value.GetType();
            TypeCode code = Type.GetTypeCode(valueType);
            switch (type)
            {
                case FieldDataType.Null:
                    switch (code)
                    {
                        case TypeCode.Empty:
                            return true;
                        default:
                            return false;
                    }
                case FieldDataType.Bool:
                    switch (code)
                    {
                        case TypeCode.Boolean:
                            return true;
                        default:
                            return false;
                    }
                case FieldDataType.Number:
                    switch (code)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.SByte:
                        case TypeCode.Single:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                        default:
                            return false;
                    }
                case FieldDataType.String:
                    switch (code)
                    {
                        case TypeCode.Char:
                        case TypeCode.String:
                            return true;
                        default:
                            return false;
                    }
                case FieldDataType.DateTime:
                    switch (code)
                    {
                        case TypeCode.DateTime:
                            return true;
                        default:
                            return false;
                    }
                case FieldDataType.Object:
                    switch (code)
                    {
                        case TypeCode.Object:
                            return true;
                        default:
                            return false;
                    }
                case FieldDataType.Value:
                    switch (code)
                    {
                        case TypeCode.Empty:
                        case TypeCode.Object:
                            return false;
                        default:
                            return true;
                    }
                case FieldDataType.Array:
                    return valueType.IsArray;
                default:
                    return false;

            }
        }

        public static FieldDataType GetJSONType(object value)
        {
            TypeCode type;
            return GetJSONType(value, out type);
        }

        public static FieldDataType GetJSONType(object value, out TypeCode actualType)
        {
            if (value == null)
            {
                actualType = TypeCode.Empty;
                return FieldDataType.Null;
            }
            Type valueType = value.GetType();
            actualType = Type.GetTypeCode(valueType);
            switch (actualType)
            {
                case TypeCode.Boolean:
                    return FieldDataType.Bool;
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return FieldDataType.Number;
                case TypeCode.Char:
                case TypeCode.String:
                    return FieldDataType.String;
                case TypeCode.DateTime:
                    return FieldDataType.DateTime;
                case TypeCode.Object:
                {
                    if (value is ArrayElement || value is Array || value is System.Collections.ArrayList || value.GetType().IsArray)
                        return FieldDataType.Array;
                    return FieldDataType.Object;
                }
            }
            if (valueType.IsArray)
                return FieldDataType.Array;
            if (valueType.IsValueType)
                return FieldDataType.Value;
            return FieldDataType.Object;
        }

        public static IComparable Enforce(object value, out FieldDataType matchType, out TypeCode actualType)
        {
            matchType = GetJSONType(value, out actualType);
            switch (matchType)
            {
                case FieldDataType.Number:
                    switch (actualType)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Convert.ToInt64(value);
                        default:
                            return Convert.ToDouble(value);
                    }
                case FieldDataType.String:
                    return value.ToString();
                case FieldDataType.Bool:
                    return (bool) value;
                default:
                    return (IComparable) value;
            }
        }

        public static IComparable StoreEnforce(object value)
        {
            Type valueType = value.GetType();
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return Convert.ToInt64(value);
                case TypeCode.Decimal:
                case TypeCode.Single:
                    return Convert.ToDouble(value);
                case TypeCode.Char:
                    return value.ToString();
                default:
                    return (IComparable)value;
            }
        }

        public static object Extract(string attribute, IJSONDocument document)
        {
            return null;
        }
    }
}
