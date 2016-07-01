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
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;

namespace Alachisoft.NosDB.Common.Queries.Optimizer
{
    //A container of terminal predicate and its expression.
    public class ProxyPredicate : IProxyPredicate
    {
        private IPredicate _predicate;
        private readonly ITreePredicate _expression;

        public ProxyPredicate(IPredicate predicate, ITreePredicate expression)
        {
            _predicate = predicate;
            _expression = expression;

        }

        public ITreePredicate Expression { get { return _expression; } }

        public IPredicate Predicate { set { _predicate = value; } }

        #region IProxyPredicate members

        public List<ITreePredicate> TreePredicates { get { return new List<ITreePredicate>() { _expression }; } }
        
        public void AddChildPredicate(IProxyPredicate predicate) { }
        
        public IDictionary<Common.Enum.Statistic, double> Statistics { get { return _predicate.Statistics; } }

        public TerminalPredicate GetExecutionPredicate(IQueryStore store)
        {
            return (TerminalPredicate) _predicate;
        }

        #endregion
    }
}
