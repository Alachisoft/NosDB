﻿using System;
using System.Collections.Generic;
using System.IO;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Queries.Filters
{
    class SpecialCountPredicate : TerminalPredicate
    {
        private long _count;

        public SpecialCountPredicate(long count)
        {
            _count = count;
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            IQueryStore tempCollection = value.SubstituteStore;
            IJSONDocument doc = JSONType.CreateNew();
            doc.Add("$count(*)", _count);
            doc.Key = Guid.NewGuid().ToString();
            tempCollection.InsertDocument(doc, null);
            var rowid = tempCollection.GetRowId(new DocumentKey(doc.Key));
            value.Store = tempCollection;
            value.GroupByField = new AllField(Field.FieldType.Grouped);
            yield return new KeyValuePair<AttributeValue, long>(NullValue.Null, rowid);
        }

        public override void Print(TextWriter output)
        {
            output.WriteLine("SpecialCountPredicate:{");
            base.Print(output);
            output.Write("}");
        }
    }
}
