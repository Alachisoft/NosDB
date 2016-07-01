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
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.SSPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class AuthenticationResponse : DatabaseResponse
    {
        Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.Builder _authenticationResponseBuilder;

        public AuthenticationResponse()
            : base()
        {
            _authenticationResponseBuilder = new Protobuf.AuthenticationResponse.Builder();

            base.ResponseMessage = this;
        }

        public AuthenticationResponse(Alachisoft.NosDB.Common.Protobuf.Response response)
            : base(response.ToBuilder())
        {
            ServerToken = new AuthToken();

            _authenticationResponseBuilder = response.ToBuilder().AuthenticationResponse.ToBuilder();

            Protobuf.ResponseAuthenticationToken.Builder authenticationTokenBuilder = _authenticationResponseBuilder.AuthenticationToken.ToBuilder();

            ServerToken.Token = authenticationTokenBuilder.Token.ToByteArray();
            ServerToken.Status = (SecurityStatus)authenticationTokenBuilder.Status;

            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            Protobuf.ResponseAuthenticationToken.Builder authenticationTokenBuilder = _authenticationResponseBuilder.AuthenticationToken.ToBuilder();
            if (ServerToken != null)
            {
                if(ServerToken.Token != null)
                    authenticationTokenBuilder.Token = Google.ProtocolBuffers.ByteString.CopyFrom(ServerToken.Token);
                else
                    authenticationTokenBuilder.Token = Google.ProtocolBuffers.ByteString.CopyFrom(new byte[0]);
                authenticationTokenBuilder.Status = (int)ServerToken.Status;
            }
            else
            {
                authenticationTokenBuilder.Token = Google.ProtocolBuffers.ByteString.CopyFrom(new byte[0]);
            }

            _authenticationResponseBuilder.SetAuthenticationToken(authenticationTokenBuilder.Build());

            base._response.SetAuthenticationResponse(_authenticationResponseBuilder);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.AUTHENTICATION);
        }

        public AuthToken ServerToken { set; get; }
    }
}
