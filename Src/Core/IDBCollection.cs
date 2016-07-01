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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common;

namespace Alachisoft.NosDB.Core
{
    public interface IDBCollection : Alachisoft.NosDB.Core.Queries.IQueryable, IDisposable
    {
        string Name { get; }
        bool Initialize(CollectionConfiguration configuration);

        bool CreateIndex(IndexConfiguration indexConfiguration);
        bool DropIndex(string indexName);

        JSONDocument GetDocument(DocumentKey documentKey);
    }
}
