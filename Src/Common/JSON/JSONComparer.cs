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

namespace Alachisoft.NosDB.Common.JSON
{
    public static class JSONComparer
    {
        public static int Compare(IComparable first, object second)
        {
            return Compare(first, second, SortOrder.ASC);
        }

        public static int Compare(IComparable first, object second, SortOrder order)
        {
            FieldDataType firstType, secondType;
            TypeCode firstActualType, secondActualType;
            firstType = JSONType.GetJSONType(first, out firstActualType);
            secondType = JSONType.GetJSONType(second, out secondActualType);
            return Compare(first, firstType, firstActualType, second, secondType, secondActualType, order);
        }

        public static int Compare(IComparable first, FieldDataType firstType, TypeCode firstActualType,
            object second, FieldDataType secondType, TypeCode secondActualType, SortOrder order)
        {
            int typeRule = firstType.CompareTo(secondType);
            if (typeRule != 0)
                return typeRule;
            switch (firstType)
            {
                case FieldDataType.Number:
                    return order == SortOrder.ASC
                        ? CompareNumbers(first, firstActualType, second, secondActualType)
                        : 0 - CompareNumbers(first, firstActualType, second, secondActualType);
                case FieldDataType.Null:
                    return 0;
                default:
                    return order == SortOrder.ASC ? first.CompareTo(second) : 0 - first.CompareTo(second);
            }
        }

        public static int CompareNumbers(IComparable first, TypeCode numberType1, object second,
            TypeCode numberType2)
        {
            switch (numberType1)
            {
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    switch (numberType2)
                    {
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return Convert.ToDouble(first).CompareTo(Convert.ToDouble(second));
                        default:
                            return Convert.ToDouble(first).CompareTo(Convert.ToInt64(second));
                    }
                default:
                    return Convert.ToInt64(first).CompareTo(Convert.ToInt64(second));
            }
        }
    }
}