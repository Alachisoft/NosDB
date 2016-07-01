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
using Alachisoft.NosDB.Common.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class DataTypeMapper
    {
        public static ParameterType MapDataType(object value)
        {
            TypeCode actualType;
            if (value == null)
            {
                actualType = TypeCode.Empty;
                return ParameterType.NULL;
            }
            if (value is System.Numerics.BigInteger || value is System.Numerics.Complex)
                return ParameterType.NOTSUPPORTED;

            Type valueType = value.GetType();
            actualType = Type.GetTypeCode(valueType);
            
            switch (actualType)
            {
                case TypeCode.Boolean:
                    return ParameterType.BOOLEAN;
                case TypeCode.Byte:
                case TypeCode.Double:
                case TypeCode.Single:
                    return ParameterType.DOUBLE;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return ParameterType.LONG;
                case TypeCode.Decimal:
                case TypeCode.UInt64:
                    return ParameterType.NOTSUPPORTED;
                case TypeCode.Char:
                case TypeCode.String:
                    return ParameterType.STRING;
                case TypeCode.DateTime:
                    return ParameterType.DATETIME;
            }
            if (valueType.IsArray || value is ArrayList || (value is IList && value.GetType().IsGenericType))
                return ParameterType.ARRAY;
            return ParameterType.OBJECT;
        }
    }
}
