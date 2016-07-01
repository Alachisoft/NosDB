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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class Predicate<T> : IPredicate
    {
        protected IList<IPredicate> _childPredicates;
        private bool _inverse = false;
        protected IIndex source;
        protected long predicateId = -1;
        private static long autoIncrement = 0;
        private bool isRoot = true;
        protected Dictionary<Statistic, double> stats;

        public Predicate()
        {
            predicateId = NextId;
        }

        public Predicate(IList<IPredicate> childPredicates)
        {
            _childPredicates = childPredicates;
            predicateId = NextId;
        }

        public virtual void Evaluate(ref IResultSet<T> resultSet, QueryCriteria values)
        {
            if(resultSet==null) resultSet = new ListedResultSet<T>();
            foreach (T result in Enumerate(values))
            {
                resultSet.Add(result);
            }
        }

        public virtual IEnumerable<T> Enumerate(QueryCriteria value)
        {
            return null;
        }

        public void AddChildPredicate(IPredicate predicate)
        {
            if (_childPredicates == null) _childPredicates = new List<IPredicate>();
            _childPredicates.Add(predicate);
            predicate.IsRoot = false;
        }

        public IPredicate[] ChildPredicates
        {
            get { return _childPredicates != null ? _childPredicates.ToArray() : null; }
        }

        public bool IsInverse
        {
            get { return _inverse; }
            set { _inverse = value; }
        }
        
        public virtual PredicateLevel Level { get { return PredicateLevel.Terminal; } }

        public IIndex Source
        {
            get { return source; }
            set { source = value; }
        }

        protected static long NextId
        {
            get { return ++autoIncrement; }
        }

        public virtual string PredicateId
        {
            get { return predicateId.ToString(); }
        }

        public bool IsRoot
        {
            get { return isRoot; }
            set { isRoot = value; }
        }

        public virtual IDictionary<Statistic, double> Statistics
        {
            get
            {
                if (stats != null)
                    return stats;
                stats = new Dictionary<Statistic, double>();
                if (source != null)
                {
                    AddBaseStats(stats);
                    stats.Add(Statistic.SelectionCardinality, SelectionCardinality);
                    stats.Add(Statistic.ExpectedIO, stats[Statistic.Depth] +
                                                    (stats[Statistic.SelectionCardinality]/stats[Statistic.TreeOrder]));
                }
                return stats;
            }
        }

        protected virtual void AddBaseStats(Dictionary<Statistic, double> stats)
        {
            stats.Add(Statistic.KeyCount, source.KeyCount);
            stats.Add(Statistic.ValueCount, source.ValueCount);
            stats.Add(Statistic.TreeOrder, Convert.ToDouble(source.GetStat(StatName.BTreeOrder)));
            stats.Add(Statistic.Depth, Math.Ceiling(Math.Log(source.KeyCount, stats[Statistic.TreeOrder])) + 1);
            stats.Add(Statistic.KeysPerNode, source.KeyCount / stats[Statistic.TreeOrder]);
        }

        public virtual double SelectionCardinality
        {
            get { return source.ValueCount; }
        }

        public virtual void Print(System.IO.TextWriter output)
        {
            output.Write("IsInverse=" + _inverse);
            output.Write(",Source=");
            if (source != null)
            {
                source.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",Childeren=");
            if (_childPredicates != null)
            {
                output.Write("[");
                for (int i = 0; i < _childPredicates.Count; i++)
                {
                    _childPredicates[i].Print(output);
                    if(i!=_childPredicates.Count-1)
                        output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
        }
    }
}
