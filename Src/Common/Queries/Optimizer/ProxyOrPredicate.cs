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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Core.Queries.Filters;

namespace Alachisoft.NosDB.Common.Queries.Optimizer
{
    public class ProxyOrPredicate : IProxyPredicate
    {
        private readonly List<IProxyPredicate> _predicates;
        private readonly List<ITreePredicate> _treePredicates = new List<ITreePredicate>();

        public ProxyOrPredicate()
        {
            _predicates = new List<IProxyPredicate>();
        }

        public List<ITreePredicate> TreePredicates
        {
            get { return _treePredicates; }
        }

        public void AddExpression(ITreePredicate expression)
        {
            _treePredicates.Add(expression);
        }

        public void AddExpressions(IEnumerable<ITreePredicate> expressions)
        {
            foreach (var expression in expressions)
                _treePredicates.Add(expression);
        }

        private double Cardinality
        {
            get
            {
                double cardinaliy = 0;

                if (_predicates.Count == 0)
                    return cardinaliy;

                foreach (var predicate in _predicates)
                {
                    cardinaliy += predicate.Statistics[Statistic.SelectionCardinality];
                }

                return cardinaliy;
            }
        }

        private double ExpectedIOs
        {
            get
            {
                double expectedIOs = 0;

                if (_predicates.Count == 0)
                    return expectedIOs;

                foreach (var predicate in _predicates)
                {
                    expectedIOs += predicate.Statistics[Statistic.ExpectedIO];
                }

                return expectedIOs;
            }
        }

        private bool IsTerminal
        {
            get
            {
                foreach (var predicate in _predicates)
                {
                    if (predicate is ProxyAndPredicate || predicate is ProxyOrPredicate)
                        return false;
                }
                return true;
            }
        }

        private ITreePredicate OrTreePredicate
        {
            get
            {
                OrTreePredicate orPredicate = new OrTreePredicate();

                foreach (var predciate in _treePredicates){
                    orPredicate.Add(predciate);
                }

                return orPredicate;
            }
        }

        #region IProxyPredicate Members

        public TerminalPredicate GetExecutionPredicate(IQueryStore queryStore)
        {
            if (_predicates.Count == 1){
                return _predicates[0].GetExecutionPredicate(queryStore);
            }

            bool optimized = true;
            ORPredicate orPredicate = new ORPredicate();
            
            foreach (var predicate in _predicates){
                TerminalPredicate execPred = predicate.GetExecutionPredicate(queryStore);

                if (execPred is StorePredicate){
                    optimized = false;
                    break;
                }
                orPredicate.AddChildPredicate(execPred);
            }

            if (!optimized){
                return new StorePredicate(OrTreePredicate, queryStore);
            }

            return orPredicate;
        }

        public IDictionary<Statistic, double> Statistics
        {
            get
            {
                var stats = new Dictionary<Statistic, double>();
                if (_predicates != null)
                {
                    stats.Add(Statistic.SelectionCardinality, Cardinality);
                    stats.Add(Statistic.ExpectedIO, ExpectedIOs);
                }
                return stats;
            }
        }

        public void AddChildPredicate(IProxyPredicate predicate)
        {
            _predicates.Add(predicate);
        }

        #endregion
    }
}
