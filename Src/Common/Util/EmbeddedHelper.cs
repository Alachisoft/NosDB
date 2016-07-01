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
using System.Collections.Generic;

using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Core.Storage.Queries.Util
{
    public static class EmbeddedHelper
    {
        public static bool Between(IJsonValue value1, IJsonValue value2, IJsonValue value3)
        {
            if (value1 is EmbeddedList || value2 is EmbeddedList)
            {
                if (Compare(value2, value1, true, true) <= 0)
                {
                    if (value3 is EmbeddedList)
                    {
                        if (Compare(value1, value3, true, true) <= 0)
                            return true;
                    }

                    if (Compare(value1, value3) <= 0)
                        return true;
                }
            }

            if (value2.CompareTo(value1) <= 0)
            {
                if (value3 is EmbeddedList)
                {
                    if (Compare(value1, value3) <= 0)
                        return true;
                }

                if (value1.CompareTo(value3) <= 0)
                    return true;
            }

            return false;
        }

        //Need refactoring
        public static int Compare(IJsonValue jsonValue1, IJsonValue jsonValue2, bool forward = true, bool flipComparison = false)
        {
            int compValue;
            if (forward)
            {
                if (jsonValue1 is EmbeddedList)
                {
                    return ((EmbeddedList)jsonValue1).CompareLessThen(jsonValue2);
                }
                else if (jsonValue2 is EmbeddedList && flipComparison)
                {
                    return ((EmbeddedList)jsonValue2).CompareGreaterThen(jsonValue1) < 0 ? 1: 0;
                }
                compValue = jsonValue2.CompareTo(jsonValue1);
                return compValue != 0 ? -1 * compValue : 0;
            }
            if (jsonValue2 is EmbeddedList)
            {
                return jsonValue2.CompareTo(jsonValue1);
            }
            compValue = jsonValue1.CompareTo(jsonValue2);
            return compValue != 0 ? -1 * compValue : 0;
        }

        public static bool Equals(IJsonValue value1, IJsonValue value2, bool isWild = false)
        {
            if (isWild)
            {
                StringConstantValue wildString = (StringConstantValue)value2;

                IJsonValue[] jsonValues1 = null;
                if (value1.DataType == Common.Enum.FieldDataType.Array)
                    jsonValues1 = ((ArrayJsonValue)value1).WrapedValue;
                else if (value1.DataType == Common.Enum.FieldDataType.Embedded)
                    jsonValues1 = ((EmbeddedList)value1).WrapedValue.ToArray();
                else jsonValues1 = new IJsonValue[] { value1 };

                foreach (var value in jsonValues1)
                {
                    if (value is StringJsonValue)
                    {
                        if(wildString.WildCompare(value.Value as string))
                            return true;
                    }
                }
                return false;
            }

            if (value1 is EmbeddedList)
            {
                return value1.Equals(value2);
            }
            return value2.Equals(value1);
        }

        public static bool ArrayContainsAny(IJsonValue value1, IEnumerable<IEvaluable> values,
            IJSONDocument entry)
        {
            if (!(value1 is EmbeddedList))
            {
                if (value1 is ArrayJsonValue)
                {
                    return ContainsAny(value1 as ArrayJsonValue, values, entry);
                }
                return false;
            }
            
            return ContainsAny(new ArrayJsonValue(((EmbeddedList)value1).WrapedValue.ToArray()), values, entry);
        }

        public static bool ArrayContainsAll(IJsonValue value1, IEnumerable<IEvaluable> values,
            IJSONDocument entry)
        {
            if (!(value1 is EmbeddedList))
            {
                if (value1 is ArrayJsonValue)
                {
                    return ContainsAll(value1 as ArrayJsonValue, values, entry);
                }
                return false;
            }
            return ContainsAll(new ArrayJsonValue(((EmbeddedList)value1).WrapedValue.ToArray()), values, entry);
        }

        //Needs refactoring.
        public static bool Contains(IJsonValue value1, IJsonValue value2,
            IJSONDocument entry)
        {
            if (value2 is EmbeddedList)
            {
                foreach (var embeddedValue in ((EmbeddedList)value2).WrapedValue)
                {
                    if (embeddedValue is ArrayJsonValue)
                    {
                        if (value1 is EmbeddedList)
                        {
                            foreach (var value in ((EmbeddedList)value1).WrapedValue)
                            {
                                if (((ArrayJsonValue)embeddedValue).Contains(value))
                                    return true;
                            }
                        }

                        if (((ArrayJsonValue)embeddedValue).Contains(value1))
                            return true;
                    }
                }
                return false;
            }

            if (value2 is ValueList)
            {
                if (value1 is EmbeddedList)
                {
                    //if (((ValueList)value2).Contains(value1 as EmbeddedList))
                    IJsonValue[] ijsonValues2 = ((ValueList)value2).Value as IJsonValue[];
                    if (ijsonValues2 == null)
                        return false;
                    foreach (var ijsonValue in ijsonValues2)
                    {
                        bool isMatchFound = false;
                        if (ijsonValue is ArrayJsonValue)
                        {
                            foreach (var value in ((EmbeddedList)value1).WrapedValue)
                            {
                                if (((ArrayJsonValue)ijsonValue).Contains(value))
                                    isMatchFound = true;
                                else
                                    continue;
                            }
                            if (isMatchFound)
                                return true;

                        }
                        else
                        {
                            if (ijsonValue.CompareTo(value1) == 0)
                                return true;
                        }
                    }
                    return false;
                }
                return !((ValueList)value2).Contains(entry, value1);
            }

            if (value2 is ArrayJsonValue)
            {
                if (value1 is EmbeddedList)
                {
                    if (value2.CompareTo(value1) == 0)
                        return true;
                    return false;
                }

                return !((ArrayJsonValue)value2).Contains(value1);
            }

            return false;
        }

        public static bool ArraySize(IJsonValue value1, long size)
        {
            if (!(value1 is EmbeddedList))
                return false;

            if (((EmbeddedList)value1).Length == size)
                return true;
            return false;
            //List<IJsonValue> list = ((EmbeddedList)value1).WrapedValue;
            //foreach (var item in list)
            //{
            //    if (item is ArrayJsonValue
            //        && ((ArrayJsonValue)item).Length == size)
            //        return true;
            //}

            
        }

        private static bool ContainsAny(ArrayJsonValue array, IEnumerable<IEvaluable> values,
            IJSONDocument entry)
        {
            foreach (var evaluable in values)
            {
                IJsonValue value;
                if (!evaluable.Evaluate(out value, entry))
                {
                    continue;
                }
                if (array.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsAll(ArrayJsonValue array, IEnumerable<IEvaluable> values,
            IJSONDocument entry)
        {
            foreach (var evaluable in values)
            {
                IJsonValue value;
                if (!evaluable.Evaluate(out value, entry))
                {
                    return false;
                }
                if (!array.Contains(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
