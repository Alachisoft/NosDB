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
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Results.Transforms;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Queries.Optimizer;
using Alachisoft.NosDB.Core.Queries.Results;

namespace Alachisoft.NosDB.Core.Queries.Filters
{
    public class Predicator
    {
        private QueryPlan _initialPlan;
        private IResultSet<long> rowResultSet;
        private IOperationContext _context;

        public Predicator(QueryPlan initialPlan, IOperationContext context)
        {
            _initialPlan = initialPlan;
            _context = context;

            var rootPredicate = _initialPlan.Predicate;
            var criteria = _initialPlan.Criteria;
            
            if (criteria.IsGrouped)
            {
                IPredicate groupByPredicate = new GroupByPredicate();
                groupByPredicate.AddChildPredicate(rootPredicate);
                rootPredicate = groupByPredicate;
            }

            if (criteria.ContainsDistinction)
            {
                IPredicate distinctPredicate = new DistinctPredicate();
                distinctPredicate.AddChildPredicate(rootPredicate);
                rootPredicate = distinctPredicate;
            }

            if (criteria.ContainsOrder)
            {
                IPredicate orderByPredicate = new OrderByPredicate();
                orderByPredicate.AddChildPredicate(rootPredicate);
                rootPredicate = orderByPredicate;
            }

            if (criteria.ContainsLimit)
            {
                IPredicate limitPredicate = new LimitPredicate();
                limitPredicate.AddChildPredicate(rootPredicate);
                rootPredicate = limitPredicate;
            }

            _initialPlan.Predicate = rootPredicate;
        }

        private void Execute()
        {
            if(_initialPlan.Criteria.SortResult)
                rowResultSet = new SortedResultSet<long>();
            else
                rowResultSet = new ListedResultSet<long>();

            var terminal = _initialPlan.Predicate as TerminalPredicate;
            if (terminal != null)
            {
                foreach (var kvp in terminal.Enumerate(_initialPlan.Criteria))
                {
                    rowResultSet.Add(kvp.Value);
                }
            }
            if (LoggerManager.Instance.QueryLogger != null)
                LoggerManager.Instance.QueryLogger.Debug("ExecuteQuery","ID:"+_initialPlan.Criteria.QueryId+
                    ", Query Executed, Result Count: " + rowResultSet.Count);
        }

        public IQueryResult Result
        {
            get
            {
                Execute();
                IDataTransform transform;
                if (rowResultSet == null)
                    throw new QuerySystemException(ErrorCodes.Query.PREDICATOR_NOT_EXECUTED);
                transform = new SelectTransform(_initialPlan.Criteria);

                if (_initialPlan.Criteria.IsGrouped)
                    return new GroupResult(rowResultSet, _initialPlan.Criteria.Store, transform, _context, _initialPlan.Criteria.QueryId);
                else
                    return new QueryResult(rowResultSet, _initialPlan.Criteria.Store, transform, _context, _initialPlan.Criteria.QueryId);
            }
        }
    }
}
