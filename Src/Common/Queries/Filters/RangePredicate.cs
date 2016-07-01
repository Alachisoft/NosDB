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
    public class RangePredicate : TerminalPredicate
    {
        private readonly AttributeValue _rangeStart;
        private readonly AttributeValue _rangeEnd;
        private readonly bool _exStart;
        private readonly bool _exEnd;

        public RangePredicate(IIndex sourceIndex, AttributeValue rangeStart, AttributeValue rangeEnd)
        {
            source = sourceIndex;
            _rangeEnd = rangeEnd;
            _rangeStart = rangeStart;
        }

        public RangePredicate(IIndex sourceIndex, AttributeValue rangeStart, bool excludeStart, AttributeValue rangeEnd, bool excludeEnd, bool isInverse)
            : this(sourceIndex, rangeStart, rangeEnd)
        {
            _exStart = excludeStart;
            _exEnd = excludeEnd;
            IsInverse = isInverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            IEnumerator<KeyValuePair<AttributeValue, long>> enumerator;
            var startComparer = source.Transform(_rangeStart);
            var endComparer = source.Transform(_rangeEnd);
            if (!IsInverse)
            {
                enumerator = source.GetEnumerator(_rangeStart, _rangeEnd);
                while (enumerator.MoveNext())
                {
                    if (_exStart && enumerator.Current.Key.Equals(startComparer)) continue;
                    if (_exEnd && enumerator.Current.Key.Equals(endComparer)) continue;
                    yield return enumerator.Current;
                }
            }
            else
            {
                enumerator = source.GetEnumeratorTo(_rangeStart);
                while (enumerator.MoveNext())
                {
                    if (_exStart && enumerator.Current.Key.Equals(startComparer)) continue;
                    yield return enumerator.Current;
                }

                enumerator = source.GetEnumeratorFrom(_rangeEnd);
                while (enumerator.MoveNext())
                {
                    if (_exEnd && enumerator.Current.Key.Equals(endComparer)) continue;
                    yield return enumerator.Current;
                }
            }
        }

        public override double SelectionCardinality
        {
            get
            {
                if (_rangeStart.DataType.Equals(FieldDataType.Number) && _rangeEnd.DataType.Equals(FieldDataType.Number))
                {
                    var max = source.Max(0, FieldDataType.Number);
                    var min = source.Min(0, FieldDataType.Number);

                    try
                    {
                        if (!max.Equals(min))
                        {
                            double dmax = Convert.ToDouble(max[0]);
                            double dmin = Convert.ToDouble(min[0]);
                            if (!IsInverse)
                            {
                                if (_rangeStart.DataType == FieldDataType.Number &&
                                    _rangeEnd.DataType == FieldDataType.Number)
                                {
                                    return Source.ValueCount*
                                           ((Convert.ToDouble(_rangeEnd[0]) - Convert.ToDouble(_rangeStart[0]))/
                                            (dmax - dmin));
                                }
                            }
                            else
                            {
                                if (_rangeStart.DataType == FieldDataType.Number &&
                                    _rangeEnd.DataType == FieldDataType.Number)
                                {
                                    double startRange = (Convert.ToDouble(_rangeStart[0]) - dmin)/(dmax - dmin);
                                    double endRange = (dmax - Convert.ToDouble(_rangeEnd[0]))/(dmax - dmin);
                                    return Source.ValueCount*(startRange + endRange);
                                }
                            }
                        }
                    }
                    catch
                    {
                        return source.ValueCount;
                    }
                }
                return Source.ValueCount;
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("RangePredicate:{");
            base.Print(output);
            output.Write(",Start=");
            output.Write(_rangeStart != null ? _rangeStart.ValueInString : "null");
            output.Write(",End=");
            output.Write(_rangeEnd != null ? _rangeEnd.ValueInString : "null");
            output.Write(",ExcludeStart="+_exStart.ToString());
            output.Write(",ExcludeEnd="+_exEnd.ToString());
            output.Write("}");
        }
    }
}
