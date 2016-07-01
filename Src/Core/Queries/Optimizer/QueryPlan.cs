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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Core.Queries.Optimizer
{
    public class QueryPlan : IPrintable
    {
        public QueryPlan()
        {
            Predicate = null;
            Criteria = null;
        }

        public IPredicate Predicate { get; set; }

        public QueryCriteria Criteria { get; set; }

        public double ExpectedIO
        {
            get { return Predicate.Statistics[Statistic.ExpectedIO]; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("QueryPlan:{");
            output.Write("PredicateTree=");
            if (Predicate != null)
            {
                Predicate.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",Criteria=");
            if (Criteria != null)
            {
                Criteria.Print(output);
            }
            else
            {
                output.Write("null");
            }

            output.Write("}");
        }

        public bool IsSpecialExecution { get; set; }
    }
}
