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
using System.IO;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Queries.Filters;

namespace Alachisoft.NosDB.Core.Queries.Filters
{
    public class ORPredicate : TerminalPredicate
    {
        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            if (_childPredicates.Count > 0)
            {
                var filter = new ClusteredHashSet<long>();
                foreach (var childPredicate in _childPredicates)
                {
                    var predicate = (TerminalPredicate) childPredicate;
                    if (predicate != null)
                    {
                        foreach (var kvp in predicate.Enumerate(value))
                        {
                            if (!value.SortResult)
                            {
                                if (!filter.Contains(kvp.Value))
                                {
                                    filter.Add(kvp.Value);
                                    yield return kvp;
                                }
                            }
                            else
                                yield return kvp;
                        }
                    }
                }
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("ORPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}
