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
    public class CreateSessionResponse:DatabaseResponse, ICreateSessionResponse
    {
        Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Builder _createSessionResponse;
        private string _sessionId;

        public CreateSessionResponse()
        {
            _createSessionResponse = new Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Builder();
            base.ResponseMessage = this;
        }

        public CreateSessionResponse(Alachisoft.NosDB.Common.Protobuf.Response response)
            : base(response.ToBuilder())
        {
            _createSessionResponse = response.CreateSessionResponse.ToBuilder();
            _sessionId = _createSessionResponse.SessionId;
            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            _createSessionResponse.SessionId = _sessionId;

            base._response.SetCreateSessionResponse(_createSessionResponse);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.CREATE_SESSION);
        }

        public string SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }
    }
}
