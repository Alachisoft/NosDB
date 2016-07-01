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
using System.Text;
using Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer;
using Alachisoft.NosDB.Common.Security.Interfaces;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    public class LocalDeleteOperation : LocalOperation, Alachisoft.NosDB.Common.Server.Engine.IDocumentsWriteOperation
    {
        public IList<Common.Server.Engine.IJSONDocument> Documents { get; set; }

        //public Common.Server.Engine.WriteConcern WriteConcern { get; set; }


        public Alachisoft.NosDB.Common.Server.Engine.IDBResponse CreateResponse()
        {
            return new LocalDeleteResponse();
        }

        /// <summary>
        /// used to identify which client is performing an operatoin on database engine
        /// </summary>
        public ISessionId SessionId { set; get; } 
    }
}
