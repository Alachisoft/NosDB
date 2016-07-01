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
    public class WriteQueryResponse:DatabaseResponse, IUpdateResponse
    {

       private Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Builder _writeQueryResponse;
        private long _affectedDocuments;

        public WriteQueryResponse()
        {
            _writeQueryResponse = new Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Builder();
            base.ResponseMessage = this;
        }

        public WriteQueryResponse(Alachisoft.NosDB.Common.Protobuf.Response response): base(response.ToBuilder())
        {
            _writeQueryResponse = response.WriteQueryResponse.ToBuilder();
            _affectedDocuments = _writeQueryResponse.AffectedDocuments;
            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            _writeQueryResponse.AffectedDocuments = _affectedDocuments;

            base._response.SetWriteQueryResponse(_writeQueryResponse);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.WRITE_QUERY);
        }

        public long AffectedDocuments
        {
            get { return _affectedDocuments; }
            set { _affectedDocuments = value; }
        }
    }
}
