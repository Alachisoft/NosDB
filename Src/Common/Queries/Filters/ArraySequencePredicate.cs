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
    public class ArraySequencePredicate : TerminalPredicate
    {
        private IDictionary<int, AttributeValue> arraySequence;

        public ArraySequencePredicate(IIndex source, IDictionary<int,AttributeValue> sequence)
        {
            Source = source;
            arraySequence = sequence;
        }

        public ArraySequencePredicate(IIndex source, IDictionary<int, AttributeValue> sequence, bool inverse)
            : this(source, sequence)
        {
            IsInverse = inverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            var andPredicate = new ANDPredicate();
            foreach (var arrayElement in arraySequence)
            {
                andPredicate.AddChildPredicate(new EqualsPredicate(Source,
                    new SingleAttributeValue(new ArrayElement(arrayElement.Value, arrayElement.Key)), IsInverse));
            }
            return andPredicate.Enumerate(value);
        }

        public override double SelectionCardinality
        {
            get
            {
                if (!IsInverse)
                {
                    try
                    {
                        return ((source.ValueCount/(double) source.KeyCount)*arraySequence.Count);
                    }
                    catch (Exception)
                    {
                        return source.ValueCount;
                    }
                }

                try
                {
                    return (source.ValueCount - ((source.ValueCount/(double) source.KeyCount)*arraySequence.Count));
                }
                catch (Exception)
                {
                    return source.ValueCount;
                }
            }
        }
        
        public override void Print(TextWriter output)
        {
            output.Write("ArraySequence:{");
            base.Print(output);
            output.Write(",Sequence=" );
            if (arraySequence != null)
            {
                output.Write("[");
                int count = 0;
                foreach (var kvp in arraySequence)
                {
                    count++;
                    output.Write("{");
                    output.Write("Index="+kvp.Key+",");
                    output.Write("Value=" + kvp.Value.ValueInString + "}");
                    if(count!=arraySequence.Count)
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
