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

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public static class Evaluator
    {
        public static IComparable PerformArithmeticOperation(IComparable value1, TypeCode actualType1,
            IComparable value2, TypeCode actualType2,
            ArithmeticOperation operation, FieldDataType valuesType)
        {
            switch (operation)
            {
                case ArithmeticOperation.Addition:
                    if (valuesType == FieldDataType.Number)
                        if (IsFloat(actualType1) || IsFloat(actualType2))
                            return Convert.ToDouble(((IJsonValue) value1).Value) +
                                   Convert.ToDouble(((IJsonValue) value2).Value);
                        else
                            return Convert.ToInt64(((IJsonValue) value1).Value) +
                                   Convert.ToInt64(((IJsonValue) value2).Value);
                    if (valuesType == FieldDataType.String)
                        return (string) ((IJsonValue) value1).Value + (string) ((IJsonValue) value2).Value;
                    return null;

                case ArithmeticOperation.Subtraction:
                    if (valuesType != FieldDataType.Number)
                        return null;
                    if (IsFloat(actualType1) || IsFloat(actualType2))
                        return Convert.ToDouble(((IJsonValue)value1).Value) -
                               Convert.ToDouble(((IJsonValue)value2).Value);
                    return Convert.ToInt64(((IJsonValue)value1).Value) -
                           Convert.ToInt64(((IJsonValue)value2).Value);

                case ArithmeticOperation.Multiplication:
                    if (valuesType != FieldDataType.Number)
                        return null;
                    if (IsFloat(actualType1) || IsFloat(actualType2))
                        return Convert.ToDouble(((IJsonValue)value1).Value) *
                               Convert.ToDouble(((IJsonValue)value2).Value);
                    return Convert.ToInt64(((IJsonValue)value1).Value) *
                           Convert.ToInt64(((IJsonValue)value2).Value);

                case ArithmeticOperation.Division:
                    if (valuesType != FieldDataType.Number)
                        return null;
                    if (IsFloat(actualType1) || IsFloat(actualType2))
                        return Convert.ToDouble(((IJsonValue)value1).Value) /
                               Convert.ToDouble(((IJsonValue)value2).Value);
                    return Convert.ToInt64(((IJsonValue)value1).Value) /
                           Convert.ToInt64(((IJsonValue)value2).Value);

                case ArithmeticOperation.Modulus:
                    if (valuesType != FieldDataType.Number)
                        return null;
                    if (IsFloat(actualType1) || IsFloat(actualType2))
                        return Convert.ToDouble(((IJsonValue)value1).Value) %
                               Convert.ToDouble(((IJsonValue)value2).Value);
                    return Convert.ToInt64(((IJsonValue)value1).Value) %
                           Convert.ToInt64(((IJsonValue)value2).Value);
            }
            return null;
        }

        public static bool IsFloat(IComparable value)
        {
            Type valueType = value.GetType();
            TypeCode typeCode = Type.GetTypeCode(valueType);
            switch (typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
            }
            return false;
        }

        public static bool IsFloat(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
