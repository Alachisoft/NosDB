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
using System.Linq;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;

namespace Alachisoft.NosDB.Common.Queries.Optimizer
{
    public class ProxyAndPredicate : IProxyPredicate, ICloneable
    {
        private readonly List<IProxyPredicate> _predicates = new List<IProxyPredicate>();
        private List<ITreePredicate> _treePredicates = new List<ITreePredicate>();

        public List<ITreePredicate> TreePredicates
        {
            get { return _treePredicates; }
            set { _treePredicates = value; }
        }

        private ITreePredicate AndTreePredicate
        {
            get
            {
                var andPredicate = new AndTreePredicate();
                foreach (var predciate in _treePredicates)
                    andPredicate.Add(predciate);
                return andPredicate;
            }
        }

        public void AddTreePredicate(ITreePredicate predciate)
        {
            _treePredicates.Add(predciate);
        }

        public void AddTreePredicates(IEnumerable<ITreePredicate> predciates)
        {
            foreach (var predciate in predciates)
                _treePredicates.Add(predciate);
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
                    double predicateCardinality = predicate.Statistics[Statistic.SelectionCardinality];
                    if(predicateCardinality>cardinaliy)
                        cardinaliy = predicateCardinality;
                }
                return cardinaliy;
            }
        }
        
        /// <summary>
        /// 1. Sum of IOs of all predicates.
        /// 2. Lowest IO of the child predicate (IO + ER).
        /// The lesser value (1 or 2) will be returned.
        /// </summary>
        private double LowestExpectedIOs
        {
            //If there are more than 1 predicates... else
            get
            {
                double lowestPredicateIOs = 0;
                double predicatesIOs = 0;
                
                if (_predicates.Count == 0)
                    return predicatesIOs;

                lowestPredicateIOs = _predicates[0].Statistics[Statistic.ExpectedIO] 
                    + _predicates[0].Statistics[Statistic.SelectionCardinality];

                for (int i = 0; i < _predicates.Count; i++)
                {
                    if ((_predicates[i].Statistics[Statistic.ExpectedIO] 
                        + _predicates[i].Statistics[Statistic.SelectionCardinality]) 
                        < lowestPredicateIOs)
                    {
                        lowestPredicateIOs = (_predicates[i].Statistics[Statistic.ExpectedIO] +
                                    _predicates[i].Statistics[Statistic.SelectionCardinality]);
                    }
                    predicatesIOs += _predicates[i].Statistics[Statistic.ExpectedIO];
                }
                
                return (lowestPredicateIOs < predicatesIOs) ? lowestPredicateIOs : predicatesIOs;
            }
        }

        private bool IsTerminal
        {
            get
            {
                foreach (var predicate in _predicates)
                    if (predicate is ProxyAndPredicate || predicate is ProxyOrPredicate)
                        return false;
                return true;
            }
        }

        #region IProxyPredicate members.

        public List<IProxyPredicate> Predicates
        {
            get
            {
                var list = new List<IProxyPredicate>();
                foreach (var proxyPredicate in _predicates)
                {
                    list.Add(proxyPredicate);
                }
                return list;
            }
        }

        public IDictionary<Statistic, double> Statistics
        {
            get
            {
                var stats = new Dictionary<Statistic, double>();
                if (_predicates != null)
                {
                    stats.Add(Statistic.SelectionCardinality, Cardinality);
                    stats.Add(Statistic.ExpectedIO, LowestExpectedIOs);
                }
                return stats;
            }
        }

        /// <summary>
        /// Gets a plan which is executable by the store.
        /// Optimizations remaining:
        /// * X AND ( Y AND Z ).
        /// </summary>
        public TerminalPredicate GetExecutionPredicate(IQueryStore queryStore)
        {
            if (_predicates.Count == 1)
                return _predicates[0].GetExecutionPredicate(queryStore);

            var andPredicate = new ANDPredicate();

                if (!IsTerminal)
                {
                    foreach (var predicate in _predicates)
                    {
                        andPredicate.AddChildPredicate(predicate.GetExecutionPredicate(queryStore));
                    }
                    return andPredicate;
                }

                //If terminal AND... apply optimizations here...
            // ANDProxyAlgo:
            // - Add IOs of all the predicates and consider it as a plan.
            // - Consider each Predicate for solution... and get their costs by IOs + ER.

            var sortedPredicates = new OrderedList<double, IProxyPredicate>();
            foreach (var predicate in _predicates)
            {
                sortedPredicates.Add(predicate.Statistics[Statistic.ExpectedIO]
                    + predicate.Statistics[Statistic.SelectionCardinality], predicate);
            }

            if (sortedPredicates.FirstValues[0].Statistics[Statistic.ExpectedIO] > LowestExpectedIOs)
            {
                var returnAnd = new ANDPredicate();
                foreach (var predicate in _predicates)
                {
                    returnAnd.AddChildPredicate(predicate.GetExecutionPredicate(queryStore));
                }
                return returnAnd;
            }

            //No need for cost calculation on this stage.
            var returnPredicate = new StorePredicate(AndTreePredicate, null);
            returnPredicate.AddChildPredicate(sortedPredicates.FirstValues[0].GetExecutionPredicate(queryStore));
            return returnPredicate;
        }

        public void AddChildPredicate(IProxyPredicate predicate)
        {
            _predicates.Add(predicate);
        }
        
        #endregion

        public object Clone()
        {
            ProxyAndPredicate proxyPredicate = new ProxyAndPredicate();
            foreach (var predicate in _predicates)
            {
                proxyPredicate.AddChildPredicate(predicate);
            }
            proxyPredicate.AddTreePredicates(_treePredicates);
            return proxyPredicate;
        }
    }
}
