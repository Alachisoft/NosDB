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
using Alachisoft.NosDB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Core.Storage.Operations
{
    public class GetOperation : Operation
    {
        private DocumentKey _key;

        public DocumentKey Key { get { return _key; } set { _key = value; } }

        public override OperationType OperationType
        {
            get { return OperationType.Get; }
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            base.Deserialize(reader);
            _key = (DocumentKey)reader.ReadObject();
        }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            base.Serialize(writer);
            writer.WriteObject(_key);
        }
    }
}
