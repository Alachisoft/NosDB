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
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Core.Queries.Filters;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    //Todo: Calculate the stats when the indecies' min and max values are ready for arrays.
    public class ArrayRangePredicate: TerminalPredicate
    {
        private AttributeValue start, end;
        private bool excStart, excEnd;

        public ArrayRangePredicate(IIndex source, AttributeValue rangeStart, AttributeValue rangeEnd)
        {
            Source = source;
            start = rangeStart;
            end = rangeEnd;
        }

        public ArrayRangePredicate(IIndex source, AttributeValue rangeStart, bool excludeStart, AttributeValue rangeEnd,
            bool excludeEnd, bool inverse) : this(source,rangeStart,rangeEnd)
        {
            excStart = excludeStart;
            excEnd = excludeEnd;
            IsInverse = inverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            using (
                var resultSet =
                    new SortedResultSet<KeyValuePair<AttributeValue, long>>(new KVPValueComparer<AttributeValue, long>())
                )
            {
                var rangePredicate = new RangePredicate(Source, start, excStart, end, excEnd, IsInverse);
                foreach (var kvp in rangePredicate.Enumerate(value))
                {
                    resultSet.Add(kvp);
                }
                return resultSet;
            }
        }

        public override double SelectionCardinality
        {
            get
            {
                var arrayMax = source.Max(0, FieldDataType.Array);
                var arrayMin = source.Min(0, FieldDataType.Array);
                if (arrayMax != null && !arrayMax.Equals(arrayMin))
                {
                    double armax = Convert.ToDouble(arrayMax[0]);
                    double armin = Convert.ToDouble(arrayMin[0]);
                    if (start.DataType == FieldDataType.Array && end.DataType == FieldDataType.Array)
                    {
                        if (
                            ((SingleAttributeValue) ((ArrayElement) ((SingleAttributeValue) start).Value).Element)
                                .DataType == FieldDataType.Number)
                        {
                            double startRange =
                                Convert.ToDouble(
                                    ((SingleAttributeValue)
                                        ((ArrayElement) ((SingleAttributeValue) start).Value).Element).Value);
                            double endRange =
                                Convert.ToDouble(
                                    ((SingleAttributeValue) ((ArrayElement) ((SingleAttributeValue) end).Value).Element)
                                        .Value);

                            // Additional checks for some realistic cardinality.
                            if (startRange > armax && endRange > armax)
                                return 0;
                            if (startRange < armin && endRange < armin)
                                return 0;
                            if (startRange < armin || endRange > armax)
                                return Source.ValueCount;

                            if (!IsInverse)
                            {
                                return Source.ValueCount*(endRange - startRange)/(armax - armin);
                            }
                            else
                            {
                                double sRange = (startRange - armin)/(armax - armin);
                                double eRange = (armax - endRange)/(armax - armin);
                                return Source.ValueCount*(sRange + eRange);
                            }
                        }
                    }
                }
                return Source.ValueCount;
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("ArrayRangePredicate:{");
            base.Print(output);
            output.Write(",Start=");
            output.Write(start != null ? start.ValueInString : "null");
            output.Write(",End=");
            output.Write(end != null ? end.ValueInString : "null");
            output.Write(",ExcludeStart=" + excStart.ToString());
            output.Write(",ExcludeEnd=" + excEnd.ToString());
            output.Write("}");
        }
    }
}
