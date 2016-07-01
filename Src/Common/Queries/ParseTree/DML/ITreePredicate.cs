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
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public interface ITreePredicate : ICondition, ICloneable
    {
        bool HasOr { get; }
        bool IsTerminal { get; }
        bool Completed { get; set; }
        string InString { get; }

        List<ITreePredicate> AtomicTreePredicates { get; }

        void AssignConstants(IList<IParameter> parameters);
        void AssignScalarFunctions();

        ITreePredicate Contract();
        ITreePredicate Expand();

        IProxyPredicate GetProxyExecutionPredicate(IIndexProvider indexManager, IQueryStore queryStore, IEnumerable<long> rowsEnumerator);
    }
}
