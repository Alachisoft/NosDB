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

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class EmptyPredicate : TerminalPredicate
    {
        private IDictionary<Statistic, double> stats;

        public override void Evaluate(ref IResultSet<KeyValuePair<AttributeValue, long>> resultSet, QueryCriteria values)
        {
            resultSet = new ListedResultSet<KeyValuePair<AttributeValue, long>>();
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            return new ListedResultSet<KeyValuePair<AttributeValue, long>>();
        }

        public override IDictionary<Statistic, double> Statistics
        {
            get
            {
                if (stats != null)
                    return stats;

                stats = new Dictionary<Statistic, double>();
                stats.Add(Statistic.SelectionCardinality,0);
                stats.Add(Statistic.ExpectedIO, 0);
                return stats;
            }
        }

        public override void Print(TextWriter output)
        {
            output.Write("EmptyPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}
