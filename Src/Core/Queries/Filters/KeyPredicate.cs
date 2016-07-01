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
﻿using System.Collections.Generic;
using System.IO;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Queries.Filters
{
    public class KeyPredicate : TerminalPredicate
    {
        private DocumentKey _key;
        private MetadataIndex _index;

        public KeyPredicate(DocumentKey key, MetadataIndex metadataIndex)
        {
            _key = key;
            _index = metadataIndex;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            long rowId = -1;
            if (_index.TryGetRowId(_key, out rowId))
                yield return new KeyValuePair<AttributeValue, long>(NullValue.Null, rowId);

            //yield return new KeyValuePair<AttributeValue, long>(NullValue.Null, -1);
        }

        public override void Print(TextWriter output)
        {
            output.Write("KeyPredicate:{ _key=[");
            output.Write(_key);
            output.Write("]");

            base.Print(output);
            output.Write("}");
        }

        public override IDictionary<Statistic, double> Statistics
        {
            get
            {
                if (stats != null)
                    return stats;
                stats = new Dictionary<Statistic, double>();
                stats.Add(Statistic.SelectionCardinality, 1);
                stats.Add(Statistic.ExpectedIO, 1);
                return stats;
            }
        }
    }
}
