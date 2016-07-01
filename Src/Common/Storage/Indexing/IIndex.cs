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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Transactions;

namespace Alachisoft.NosDB.Common.Storage.Indexing
{
    public interface IIndex: ITransactable, IDisposable, IPrintable, IQueryable
    {
        void Initialize();
        string Name { get; set; }
        IndexConfiguration Configuration { get; }
        IndexAttribute Attributes { get; }
        IndexKey IndexKey { get; }
        bool IsFunctional { get; set; }
        void Remove(AttributeValue value, long rowId, long operationId);
        void Add(AttributeValue value, long rowId, long operationId);
        void Destroy();
        IDictionary<long, byte> this[AttributeValue key] { get; }
        bool TryGetValue(AttributeValue key, out IDictionary<long, byte> values);
        void Clear();
        bool Contains(AttributeValue key);
        void CopyTo(KeyValuePair<AttributeValue, long>[] array, int offset);
        int KeyCount { get; }
        int ValueCount { get; }
        AttributeValue Min(int attributeNumber, FieldDataType type);
        AttributeValue Max(int attributeNumber, FieldDataType type);
        SortOrder Order { get; }
        object GetStat(StatName name);
        string Path { get; }
        AttributeValue Transform(AttributeValue source);
    }
}
