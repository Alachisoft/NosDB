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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class GroupByPredicate : TerminalPredicate
    {
        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            IQueryStore tempCollection = value.SubstituteStore;
            var key = new DocumentKey();
            var finalResultSet = new SortedDictionary<AttributeValue, long>();

            if (_childPredicates != null)
            {
                foreach (var childPredicate in _childPredicates)
                {
                    var predicate = childPredicate as TerminalPredicate;
                    if (predicate != null)
                    {
                        foreach (var kvp in predicate.Enumerate(value))
                        {
                            var newDocument = value.Store.GetDocument(kvp.Value, null);
                            if (newDocument == null) continue;
                            AttributeValue newKey;
                            if (value.GroupByField.GetAttributeValue(newDocument, out newKey))
                            {
                                key.Value = newKey.ValueInString;

                                if (finalResultSet.ContainsKey(newKey))
                                {
                                    if (value.ContainsAggregations)
                                    {
                                        var groupDocument = tempCollection.GetDocument(finalResultSet[newKey], null);
                                        foreach (var aggregation in value.Aggregations)
                                        {
                                            IJsonValue calculatedValue;
                                            if (aggregation.Evaluation.Evaluate(out calculatedValue, newDocument))
                                            {
                                                aggregation.Aggregation.Value = groupDocument[aggregation.FieldName];
                                                aggregation.Aggregation.ApplyValue(calculatedValue.Value);
                                                groupDocument[aggregation.FieldName] = aggregation.Aggregation.Value;
                                            }
                                        }
                                        tempCollection.UpdateDocument(finalResultSet[newKey], groupDocument, new OperationContext());
                                    }
                                }
                                else
                                {
                                    var aggregateDocument = JSONType.CreateNew();
                                    value.GroupByField.FillWithAttributes(newDocument, aggregateDocument);
                                    if (value.ContainsAggregations)
                                    {
                                        foreach (var aggregation in value.Aggregations)
                                        {
                                            IJsonValue calculatedValue;
                                            if ( aggregation.Evaluation.Evaluate(out calculatedValue, newDocument))
                                            {
                                                aggregation.Reset();
                                                aggregation.Aggregation.ApplyValue(calculatedValue.Value);
                                                aggregateDocument[aggregation.FieldName] = aggregation.Aggregation.Value;
                                            }
                                        }
                                    }

                                    aggregateDocument.Key = key.Value as string;
                                    tempCollection.InsertDocument(aggregateDocument,
                                        new OperationContext());
                                    long newRowId = tempCollection.GetRowId(key);
                                    finalResultSet.Add(newKey, newRowId);
                                    
                                }
                            }
                        }
                    }
                }
            }

            if (finalResultSet.Count == 0)
            {
                var emptyDoc = JSONType.CreateNew();
                if (value.ContainsAggregations)
                    foreach (var aggregation in value.Aggregations)
                    {
                        emptyDoc[aggregation.FieldName] = 0;
                    }
                key.Value = value.GroupByField.FieldId.ToString();
                emptyDoc.Key = (string) key.Value;
                tempCollection.InsertDocument(emptyDoc, new OperationContext());
                finalResultSet.Add(new NullValue(), tempCollection.GetRowId(key));
            }
            value.Store = tempCollection;
            return finalResultSet;
        }

        public override void Print(TextWriter output)
        {
            output.Write("GroupByPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}


