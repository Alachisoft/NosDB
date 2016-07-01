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
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Queries.Results;

namespace Alachisoft.NosDB.Core.Queries.Filters
{
    public class DistinctPredicate : TerminalPredicate
    {
        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria values)
        {
            using (var resultSet = new SortedResultSet<ResultWrapper<KeyValuePair<AttributeValue, long>>>())
            {
                if (_childPredicates != null)
                {
                    foreach (var child in _childPredicates)
                    {
                        var terminal = (TerminalPredicate) child;
                        if (terminal != null)
                        {
                            foreach (var kvp in terminal.Enumerate(values))
                            {
                                var wrapper = new ResultWrapper<KeyValuePair<AttributeValue, long>>(kvp);
                                bool validDocument = false;
                                if (values.ContainsDistinction)
                                {
                                    IJSONDocument document = values.Store.GetDocument(kvp.Value, null);
                                    AttributeValue attValue;
                                    validDocument = values.DistinctField.GetAttributeValue(document, out attValue);
                                    wrapper.SortField = attValue;
                                }
                                if (validDocument && !resultSet.Contains(wrapper)) resultSet.Add(wrapper);
                            }
                        }
                    }
                    foreach (var resultWrapper in resultSet)
                    {
                        yield return resultWrapper.Value;
                    }
                }
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("DistinctPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}
