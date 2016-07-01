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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Core.Queries.Filters;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class ArrayEqualsPredicate : TerminalPredicate
    {
        private AttributeValue[] sequence;

        public ArrayEqualsPredicate(IIndex sourceIndex, AttributeValue[] arraySequence)
        {
            Source = sourceIndex;
            sequence = arraySequence;
        }

        public ArrayEqualsPredicate(IIndex sourceIndex, AttributeValue[] arraySequence, bool inverse)
            : this(sourceIndex, arraySequence)
        {
            IsInverse = inverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            var andPredicate = new ANDPredicate();

            for (int i = 0; i < sequence.Length; i++)
            {
                var currentElement = new SingleAttributeValue(new ArrayElement(sequence[i], i));
                andPredicate.AddChildPredicate(new EqualsPredicate(Source, currentElement, IsInverse));

            }

            foreach (var kvp in andPredicate.Enumerate(value))
            {
                IJSONDocument document = value.Store.GetDocument(kvp.Value, null);
                //Array array = document.GetArray(source.Attributes[0].Name);
                Array array = JsonDocumentUtil.GetArray(document, source.Attributes.Name);
                if (array.Length == sequence.Length)
                {
                    yield return kvp;
                }
            }
        }

        
        public override double SelectionCardinality
        {
            get
            {
                if (!IsInverse)
                {
                    try
                    {
                        return ((source.ValueCount/(double) source.KeyCount));
                    }
                    catch (Exception)
                    {
                        return source.ValueCount;
                    }
                }
                return source.ValueCount;
            }
        }
    }
}
