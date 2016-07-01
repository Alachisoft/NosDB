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
using System.Linq;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class FullPredicate : TerminalPredicate
    {
        readonly IEnumerable<long> _enumerator;

        public FullPredicate(IEnumerable<long> rowsEnumerator)
        {
            _enumerator = rowsEnumerator;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            return _enumerator.Select(l => new KeyValuePair<AttributeValue, long>(NullValue.Null, l));
        }

        public override void Print(TextWriter output)
        {
            output.Write("FullPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}
