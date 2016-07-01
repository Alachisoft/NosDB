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
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Queries.ParseTree;
using Alachisoft.NosDB.Core.Queries.ParseTree.DML;
using Alachisoft.NosDB.Core.Queries.Results;
using Alachisoft.NosDB.Core.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Queries.Optimizer
{
    public class RuleBasedOptimizer : IQueryOptimizer
    {
        public QueryPlan GetQueryPlan(IDmObject parsedQuery, IQuery query, IQueryStore queryStore, MetadataIndex rowEnumerator)
        {
            throw new System.NotImplementedException();
        }

        public ICollection<QueryPlan> GetAllPlans(IDmObject parsedQuery, IQuery query, IQueryStore queryStore)
        {
            throw new System.NotImplementedException();
        }
    }
}
