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
    public class ArrayAnyPredicate : TerminalPredicate
    {
        private AttributeValue[] _anyValues;

        public ArrayAnyPredicate(IIndex sourceIndex, AttributeValue[] anyValues)
        {
            Source = sourceIndex;
            _anyValues = anyValues;
        }

        public ArrayAnyPredicate(IIndex sourceIndex, AttributeValue[] anyValues, bool isInverse)
            : this(sourceIndex, anyValues)
        {
            IsInverse = isInverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            var orPredicate = new ORPredicate();
            foreach (var attributeValue in _anyValues)
            {
                orPredicate.AddChildPredicate(new RangePredicate(Source,
                    new SingleAttributeValue(new ArrayElement(attributeValue, 0)), IsInverse,
                    new SingleAttributeValue(new ArrayElement(attributeValue, int.MaxValue))
                    , IsInverse, IsInverse));
            }

            return orPredicate.Enumerate(value);
        }

        public override double SelectionCardinality
        {
            get
            {
                if (!IsInverse)
                {
                    try
                    {
                        return ((source.ValueCount/(double) source.KeyCount)*_anyValues.Length);
                    }
                    catch (Exception)
                    {
                        return source.ValueCount;
                    }
                }
                try
                {
                    return (source.ValueCount - ((source.ValueCount/(double) source.KeyCount)*_anyValues.Length));
                }
                catch (Exception)
                {
                    return source.ValueCount;
                }
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("ArrayAnyPredicate:{");
            base.Print(output);
            output.Write(",AnyValues=");
            if (_anyValues != null)
            {
                output.Write("[");
                for (int i = 0; i < _anyValues.Length; i++)
                {
                    output.Write(_anyValues[i].ValueInString);
                    if (i != _anyValues.Length - 1)
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
