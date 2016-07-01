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

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class EqualsPredicate : TerminalPredicate
    {
        private AttributeValue _value;

        public EqualsPredicate(IIndex sourceIndex, AttributeValue value)
        {
            source = sourceIndex;
            _value = value;
        }

        public EqualsPredicate(IIndex sourceIndex, AttributeValue value, bool isInverse)
            : this(sourceIndex, value)
        {
            IsInverse = isInverse;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            IEnumerator<KeyValuePair<AttributeValue, long>> enumerator;
            if (IsInverse)
            {
                var fixedValue = source.Transform(_value);
                enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key.Equals(fixedValue)) continue;
                    yield return enumerator.Current;
                }
            }
            else
            {
                enumerator = source.GetEnumerator(_value);
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
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
                        return source.ValueCount/(double) source.KeyCount;
                    }
                    catch (Exception)
                    {
                        return source.ValueCount;
                    }
                }
                return source.ValueCount;
            }
        }
        
        public override void Print(TextWriter output)
        {
            output.Write("EqualsPredicate:{");
            base.Print(output);
            output.Write(",EqualTo=" + (_value != null ? _value.ValueInString : "null"));
            output.Write("}");
        }
    }
}
