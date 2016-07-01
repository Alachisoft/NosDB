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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public static class PredicateHelper
    {
        /// <summary>
        /// Creates an instance of IEvaluable constant based on IJSONValue.
        /// </summary>
        public static IEvaluable GetConstant(IJsonValue value)
        {
            switch (value.DataType)
            {
                case FieldDataType.Null:
                    return NullValue.Null;
                case FieldDataType.Bool:
                    return new BooleanConstantValue((bool)value.Value);
                case FieldDataType.Number:
                    {
                        var number = value.Value;
                        if (number is Int64)
                            return new IntegerConstantValue(number.ToString());
                        if (number is Double)
                            return new DoubleConstantValue(number.ToString());
                        break;
                    }
                case FieldDataType.String:
                    return new StringConstantValue((string)value.Value);
                case FieldDataType.DateTime:
                    return new DateTimeConstantValue((DateTime)value.Value);
                case FieldDataType.Array:
                    break;
                case FieldDataType.Object:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Returns IEnumerable<Attribute> of attributes which are repeated.
        /// </summary>
        public static IEnumerable<Attribute> GetRepeatedAttributes(IEnumerable<ITreePredicate> predciates)
        {
            var attributeset = new HashSet<string>();
            var list = new List<Attribute>();
            foreach (ComparisonPredicate predicate in predciates)
            {
                if (attributeset.Contains(predicate.Attributes[0].ToString()))
                {
                    list.Add(predicate.Attributes[0]);
                }
                else
                {
                    attributeset.Add(predicate.Attributes[0].ToString());
                }
            }
            return list;
        }
    }
}
