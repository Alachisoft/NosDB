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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class AuthenticationOperation : DatabaseOperation, IAuthenticationOperation
    {

        private Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.Builder _authenticationCommandBuilder;
        private string _connectionString;
        private string _processID;

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public AuthenticationOperation() : base()
        {
            _authenticationCommandBuilder = new Protobuf.AuthenticationCommand.Builder();
            base.Message = this;
            base.OperationType = DatabaseOperationType.Authenticate;
        }

        public AuthenticationOperation(Alachisoft.NosDB.Common.Protobuf.Command command)
            : base(command.ToBuilder())
        {
            ClientToken = new AuthToken();
            _authenticationCommandBuilder = command.AuthenticationCommand.ToBuilder();
            _connectionString = _authenticationCommandBuilder.ConnectionString;
            _processID = _authenticationCommandBuilder.ProcessID;

            Protobuf.AuthenticationToken.Builder authenticationTokenBuilder = _authenticationCommandBuilder.AuthenticationToken.ToBuilder(); ;

            ClientToken.Status = (Security.SSPI.SecurityStatus) authenticationTokenBuilder.Status;
            ClientToken.Token = authenticationTokenBuilder.Token.ToByteArray();

            base.Message = this;
            base.OperationType = DatabaseOperationType.Authenticate;
        }

        internal override void BuildInternal()
        {
            Protobuf.AuthenticationToken.Builder authenticationTokenBuilder = new Protobuf.AuthenticationToken.Builder();
            authenticationTokenBuilder.SetStatus((int) ClientToken.Status);
            authenticationTokenBuilder.SetToken(Google.ProtocolBuffers.ByteString.CopyFrom(ClientToken.Token));

            _authenticationCommandBuilder.SetAuthenticationToken(authenticationTokenBuilder.Build());
            _authenticationCommandBuilder.ConnectionString = _connectionString;
            _authenticationCommandBuilder.ProcessID = _processID == null? "": _processID;

            _command.SetAuthenticationCommand(_authenticationCommandBuilder);
            _command.SetType(Protobuf.Command.Types.Type.AUTHENTICATION);
        }

        public override IDBResponse CreateResponse()
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            authenticationResponse.RequestId = this.RequestId;
            return authenticationResponse;
        }

        public string ClientProcessID
        {
            get { return _processID == null ? "" : _processID; }
            set { _processID = value; }
        }

        public AuthToken ClientToken { set; get; }
        //public ISessionId SessionId { get; set; } //commented as already a member in database operation

        /// <summary>
        /// used only when authenticating to verify if client is local or remote
        /// </summary>
        public Address Address { set; get; }

    }
}

