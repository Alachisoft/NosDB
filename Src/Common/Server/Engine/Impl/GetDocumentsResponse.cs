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
    public class GetDocumentsResponse : DatabaseResponse, IGetResponse
    {
        private Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.Builder _getDocumentsResponse;
        private DataChunk _dataChunkBuilder;
        
        public GetDocumentsResponse()
        {
            _getDocumentsResponse = new Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.Builder();
            _dataChunkBuilder = new DataChunk();
            base.ResponseMessage = this;
        }

        public GetDocumentsResponse(Alachisoft.NosDB.Common.Protobuf.Response response):base(response.ToBuilder())
        {
            _getDocumentsResponse = response.GetDocumentsResponse.ToBuilder();
            _dataChunkBuilder = new DataChunk();

            _dataChunkBuilder.ChunkId = _getDocumentsResponse.DataChunk.ChunkId;
            _dataChunkBuilder.ReaderUID = _getDocumentsResponse.DataChunk.ReaderUId;
            _dataChunkBuilder.IsLastChunk = _getDocumentsResponse.DataChunk.IsLastChunk;

            foreach (string document in _getDocumentsResponse.DataChunk.DocumentsList)
                _dataChunkBuilder.Documents.Add(JSONDocument.Parse(document));

            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            Alachisoft.NosDB.Common.Protobuf.DataChunk.Builder protoDataChunk;
            protoDataChunk = new Alachisoft.NosDB.Common.Protobuf.DataChunk.Builder();

            protoDataChunk.SetChunkId(_dataChunkBuilder.ChunkId);
            protoDataChunk.SetReaderUId(_dataChunkBuilder.ReaderUID);

            foreach (IJSONDocument document in _dataChunkBuilder.Documents)
                protoDataChunk.AddDocuments(document.ToString());

            protoDataChunk.SetIsLastChunk(_dataChunkBuilder.IsLastChunk);

            _getDocumentsResponse.SetDataChunk(protoDataChunk);

            base._response.SetGetDocumentsResponse(_getDocumentsResponse);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.GET_DOCUMENTS);
        }

        public IDataChunk DataChunk
        {
            get { return _dataChunkBuilder; }
            set { _dataChunkBuilder = (DataChunk)value; }
        }
    }
}
