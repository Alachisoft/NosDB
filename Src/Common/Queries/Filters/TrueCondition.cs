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
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class TrueCondition : ITreePredicate
    {
        public List<ITreePredicate> AtomicTreePredicates
        {
            get
            {
                return new List<ITreePredicate>();
            }
        }

        public bool Completed
        {
            get { return false; }

            set
            { }
        }

        public bool HasOr
        {
            get { return false; }
        }

        public string InString
        {
            get { return "TrueForAll"; }
        }

        public bool IsTerminal
        {
            get { return true; }
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
        }

        public void AssignScalarFunctions()
        {
            
        }

        public object Clone()
        {
            return new TrueCondition();
        }

        public ITreePredicate Contract()
        {
            return null;
        }

        public ITreePredicate Expand()
        {
            return null;
        }

        public IProxyPredicate GetProxyExecutionPredicate(IIndexProvider indexManager, IQueryStore queryStore, IEnumerable<long> rowsEnumerator)
        {
            return null;
        }


        public bool IsTrue(Common.Server.Engine.IJSONDocument entry)
        {
            if (entry != null)
                return true;
            return false;
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("TrueCondition");
        }
    }
}
