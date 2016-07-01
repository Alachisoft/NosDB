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
    public class CreateSessionOperation: DatabaseOperation, ICreateSessionOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Builder _createSessionCommand;
        private int _clientId;
        private string _userName;
        private string _password;

        public CreateSessionOperation()
        {
            _createSessionCommand = new  Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Builder();
            base.Message = this;
        }

        public CreateSessionOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            _createSessionCommand = command.CreateSessionCommand.ToBuilder();
            _clientId = _createSessionCommand.ClientId;

            //convert encrypted byte array to string 
            _userName = Encoding.UTF8.GetString(_createSessionCommand.Credential.UserName.ToByteArray());
            _password = Encoding.UTF8.GetString(_createSessionCommand.Credential.Password.ToByteArray());

            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _createSessionCommand.ClientId = _clientId;
            Alachisoft.NosDB.Common.Protobuf.Credential.Builder credentials;
            credentials = new Alachisoft.NosDB.Common.Protobuf.Credential.Builder();
            
            //encrypt credentials
            credentials.UserName = Google.ProtocolBuffers.ByteString.CopyFrom(Encoding.ASCII.GetBytes(_userName));
            credentials.UserName =  Google.ProtocolBuffers.ByteString.CopyFrom(Encoding.ASCII.GetBytes(_password));
            _createSessionCommand.SetCredential(credentials);

            base._command.SetCreateSessionCommand(_createSessionCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.CREATE_SESSION);
        }

        public int ClientID
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public override IDBResponse CreateResponse()
        {
            CreateSessionResponse response = new CreateSessionResponse();
            response.RequestId = base.RequestId;
            return response;
        }

        public override IDBOperation Clone()
        {
            return base.Clone();
        }
    }
}
