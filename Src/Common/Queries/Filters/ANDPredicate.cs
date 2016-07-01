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
using Alachisoft.NosDB.Common.Queries.Results;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class ANDPredicate : TerminalPredicate
    {
        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            IResultSet<KeyValuePair<AttributeValue, long>> resultSet =
                new ListedResultSet<KeyValuePair<AttributeValue, long>>();
            Evaluate(ref resultSet, value);
            return resultSet;
        }

        public override void Evaluate(ref IResultSet<KeyValuePair<AttributeValue, long>> resultSet, QueryCriteria values)
        {
            if (resultSet == null) resultSet = new ListedResultSet<KeyValuePair<AttributeValue, long>>();
            IDictionary<long,AttributeValue> initial = new HashVector<long, AttributeValue>();
            var firstIteration = true;
            var intersecter = new Intersecter<long, AttributeValue>(initial);
            if (_childPredicates != null)
                foreach (var predicate in _childPredicates)
                {
                    var terminal = (TerminalPredicate) predicate;
                    if (terminal == null) continue;
                    foreach (var kvp in terminal.Enumerate(values))
                    {
                        if (firstIteration)
                            initial[kvp.Value] =  kvp.Key;
                        else
                            intersecter.Add(kvp.Value, kvp.Key);
                    }
                    if (!firstIteration)
                        intersecter.Flip();
                    firstIteration = false;
                }
            foreach (var keyValuePair in intersecter.FinalResult)
            {
                resultSet.Add(new KeyValuePair<AttributeValue, long>(keyValuePair.Value, keyValuePair.Key));
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("ANDPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}
