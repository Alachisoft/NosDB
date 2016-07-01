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
    public class DisposeReaderOperation : DatabaseOperation, IDiposeReaderOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Builder _disposeReaderCommand;
        private string _readerUID;

        public DisposeReaderOperation()
        {
            _disposeReaderCommand = new Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Builder();
            base.Message = this;
        }

        public DisposeReaderOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            _disposeReaderCommand = command.DisposeReaderCommand.ToBuilder();
            _readerUID = _disposeReaderCommand.ReaderUID;
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _disposeReaderCommand.ReaderUID = _readerUID;

            base._command.SetDisposeReaderCommand(_disposeReaderCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.DISPOSE_READER);
        }

        public string ReaderUID
        {
            get { return _readerUID; }
            set { _readerUID = value; }
        }

        public override IDBOperation Clone()
        {
            return base.Clone();
        }

        public override IDBResponse CreateResponse()
        {
            DatabaseResponse response = new DatabaseResponse();
            response.SetResposeType(Protobuf.Response.Types.Type.DISPOSE_READER);
            response.ResponseMessage = response;
            response.RequestId = base.RequestId;
            return response;
        }
    }
}
