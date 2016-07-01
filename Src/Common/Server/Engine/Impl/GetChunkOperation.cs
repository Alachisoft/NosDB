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
    public class GetChunkOperation : DatabaseOperation, IGetChunkOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Builder _getChunkCommand;
        private int _lastChunkId;
        private string _readerUID;
        private string _queryString; // no need to send it to server
        private bool _doCaching = false; // no need to send it to server

        public GetChunkOperation()
        {
            _getChunkCommand = new Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Builder();
            base.Message = this;
            base.OperationType = DatabaseOperationType.GetChunk;
        }

        public GetChunkOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            _getChunkCommand = command.GetChunkCommand.ToBuilder();
            _lastChunkId = _getChunkCommand.LastChunkId;
            _readerUID = _getChunkCommand.ReaderUID;
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _getChunkCommand.LastChunkId = _lastChunkId;           
            _getChunkCommand.ReaderUID = _readerUID==null?"":_readerUID;

            base._command.SetGetChunkCommand(_getChunkCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.GET_CHUNK);
        }

        public int LastChunkId
        {
            get { return _lastChunkId; }
            set { _lastChunkId = value; }
        }

        public string ReaderUID
        {
            get { return _readerUID; }
            set { _readerUID = value; }
        }

        public override IDBOperation Clone()
        {
            GetChunkOperation getChunkOperation = base.Clone() as GetChunkOperation;
            getChunkOperation.LastChunkId = _lastChunkId;
            getChunkOperation.ReaderUID = _readerUID;
            getChunkOperation._getChunkCommand = _getChunkCommand;
            return getChunkOperation;
        }

        public override IDBResponse CreateResponse()
        {
            GetChunkResponse getChunkResponse = new GetChunkResponse();
            getChunkResponse.RequestId = base.RequestId;
            if (getChunkResponse.DataChunk == null)
            {
                getChunkResponse.DataChunk = new DataChunk();
            }
            getChunkResponse.DataChunk.ReaderUID = "-1";
            return getChunkResponse;
        }


        public string QueryString
        {
            get { return _queryString; }
            set { _queryString = value; }
        }
        public bool DoCaching
        {
            get { return _doCaching; }
            set { this._doCaching = value; }
        }
    }
}
