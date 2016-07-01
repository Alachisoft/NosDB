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
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Core.Queries.Filters;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class ArrayAllPredicate : TerminalPredicate
    {
        private AttributeValue[] _allValues;

        public ArrayAllPredicate(IIndex sourceIndex, AttributeValue[] allValues)
        {
            Source = sourceIndex;
            _allValues = allValues;
        }

        public ArrayAllPredicate(IIndex source, AttributeValue[] allValues, bool inverse) : this(source, allValues)
        {
            IsInverse = inverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            var andPredicate = new ANDPredicate();
            foreach (var attributeValue in _allValues)
            {
                andPredicate.AddChildPredicate(new RangePredicate(Source,
                    new SingleAttributeValue(new ArrayElement(attributeValue, 0)), IsInverse,
                    new SingleAttributeValue(new ArrayElement(attributeValue, int.MaxValue))
                    , IsInverse, IsInverse));
            }
            return andPredicate.Enumerate(value);
        }

        public override double SelectionCardinality
        {
            get
            {
                if (IsInverse) return source.ValueCount;
                try
                {
                    if (source.KeyCount != 0)
                    {
                        return ((source.ValueCount/(double) source.KeyCount));
                    }
                    return source.ValueCount;
                }
                catch (Exception)
                {
                    return source.ValueCount;
                }
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("ArrayAllPredicate:{");
            base.Print(output);
            output.Write(",AllValues=");
            if (_allValues != null)
            {
                output.Write("[");
                for (int i = 0; i < _allValues.Length; i++)
                {
                    output.Write(_allValues[i].ValueInString);
                    if (i != _allValues.Length - 1)
                        output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }

            output.Write("}");
        }
    }
}
