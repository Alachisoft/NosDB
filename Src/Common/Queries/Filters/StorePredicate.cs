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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class StorePredicate : TerminalPredicate
    {
        private readonly ICondition _validator;
        private readonly IQueryStore _store;

        public StorePredicate(ICondition validator, IQueryStore store)
        {
            _validator = validator;
            _store = store;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {

            if (ChildPredicates != null)
            {
                foreach (var predicate in ChildPredicates)
                {
                    var terminalPredicate = predicate as TerminalPredicate;
                    if (terminalPredicate != null)
                    {
                        foreach(var kvp in terminalPredicate.Enumerate(value)){

                            var document = value.Store.GetDocument(kvp.Value, null);
                            if (_validator.IsTrue(document))
                            {
                                yield return kvp;
                            }
                        }
                    }
                }
            }
            else
            {
                IEnumerator<IJSONDocument> enumerator = value.Store.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (_validator.IsTrue(enumerator.Current))
                    {
                        string key = enumerator.Current.Key;
                        yield return
                            new KeyValuePair<AttributeValue, long>(new SingleAttributeValue(key),
                                value.Store.GetRowId(new DocumentKey(enumerator.Current.Key)));
                    }
                }
            }
        }

        public override double SelectionCardinality
        {
            get { return Statistics[Statistic.SelectionCardinality]; }
        }

        public override IDictionary<Statistic, double> Statistics
        {
            get
            {
                var stats = new Dictionary<Statistic, double>();
                if (_store != null)
                {
                    stats.Add(Statistic.ExpectedIO, _store.DocumentCount);
                    stats.Add(Statistic.SelectionCardinality, _store.DocumentCount);
                    stats.Add(Statistic.KeyCount, _store.DocumentCount);
                    stats.Add(Statistic.ValueCount, _store.DocumentCount);
                }
                return stats;
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("StorePredicate:{");
            base.Print(output);
            output.Write(",Store=");
            if (_store != null)
            {
                _store.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",Validator=");
            if (_validator != null)
            {
                _validator.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }
    }
}
