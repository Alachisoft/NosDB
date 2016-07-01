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

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class DropIndexOperation:DatabaseOperation, IDropIndexOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Builder _dropIndexCommand;

        public DropIndexOperation() 
        {
            _dropIndexCommand = new Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Builder();
            base.Message = this;
        }

        public DropIndexOperation(Alachisoft.NosDB.Common.Protobuf.Command command)
            : base(command.ToBuilder())
        {
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            //build command
            //base._command.SetGetBulkCommand(_getBulkCommand);
            //base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.GET_BULK);
        }

        public override IDBOperation Clone()
        {
            //deep clone
            return base.Clone();
        }

        public string IndexName { get; set;        }

        public override IDBResponse CreateResponse()
        {
            DatabaseResponse resposne = new DatabaseResponse();
            resposne.RequestId = base.RequestId;
            return resposne;
        }
    }
}
