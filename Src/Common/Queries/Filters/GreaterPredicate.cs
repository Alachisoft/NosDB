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
using System.IO;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class GreaterPredicate : TerminalPredicate
    {
        public AttributeValue _value;

        public GreaterPredicate(IIndex sourceIndex, AttributeValue value)
        {
            source = sourceIndex;
            _value = value;
        }

        public GreaterPredicate(IIndex sourceIndex, AttributeValue value, bool isInverse)
            : this(sourceIndex, value)
        {
            IsInverse = isInverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            IEnumerator<KeyValuePair<AttributeValue, long>> enumerator;
            if (IsInverse)
            {
                enumerator = source.GetEnumeratorTo(_value);
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            else
            {
                var fixedValue = source.Transform(_value);
                enumerator = source.GetEnumeratorFrom(_value);
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key.Equals(fixedValue)) continue;
                    yield return enumerator.Current;
                }
            }
        }


        public override double SelectionCardinality
        {
            get
            {
                if (_value.DataType.Equals(FieldDataType.Number))
                {

                    var max = source.Max(0, FieldDataType.Number);
                    var min = source.Min(0, FieldDataType.Number);
                    if (!min.Equals(max))
                    {
                        try
                        {
                            double dmax = Convert.ToDouble(max[0]);
                            double dmin = Convert.ToDouble(min[0]);
                            if (_value.DataType == FieldDataType.Number)
                            {
                                double value = Convert.ToDouble(_value[0]);
                                if (!IsInverse)
                                {
                                    return source.ValueCount*((dmax - value)/(dmax - dmin));
                                }
                                return source.ValueCount*((value - dmin)/(dmax - dmin)) + 1;
                            }
                        }
                        catch
                        {
                            return source.ValueCount;
                        }
                    }

                }
                if (_value.DataType.Equals(FieldDataType.Array))
                {
                    var arrayMax = source.Max(0, FieldDataType.Array);
                    var arrayMin = source.Min(0, FieldDataType.Array);
                    if (!arrayMax.Equals(arrayMin))
                    {
                        try
                        {
                            double armax = Convert.ToDouble(arrayMax[0]);
                            double armin = Convert.ToDouble(arrayMin[0]);
                            double value = Convert.ToDouble(((SingleAttributeValue)((ArrayElement)((SingleAttributeValue)_value).Value).Element).Value);

                            //Additional checks for realistic cardinality
                            if (value < armin)
                                return source.ValueCount;
                            if (value > armax)
                                return 0;

                            if (!IsInverse)
                                return source.ValueCount * ((armax - value) / (armax - armin));
                            return source.ValueCount * ((value - armin) / (armax - armin)) + 1;
                            
                        }
                        catch (Exception)
                        {
                            return source.ValueCount;
                        }
                    }
                }
                return source.ValueCount;
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("GreaterPredicate:{");
            base.Print(output);
            output.Write(",GreaterThan=" + (_value != null ? _value.ValueInString : "null"));
            output.Write("}");
        }
    }
}
