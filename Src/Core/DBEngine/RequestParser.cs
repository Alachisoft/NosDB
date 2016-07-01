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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Protobuf;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.DBEngine
{
    public class RequestParser : IRequest
    {
        private byte[] _buffer;
        private Command _command;
        private IChannel _channel;
        private Alachisoft.NosDB.Common.Net.Address _source;
        private object _message;

        public RequestParser(byte[] buffer)
        {
            _buffer = buffer;
        }

        public long RequestId
        {
            get
            {
                if (_command == null)
                    ParseCommand();
                return _command.RequestId;
            }
            set
            {
                if (_command != null)
                    _command.ToBuilder().RequestId = value;
            }
        }

        public object Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public bool NoResponse
        {
            get
            {
                if (_command == null)
                    ParseCommand();
                return _command.NoResponse;
            }
            set
            {
                if (_command != null)
                    _command.ToBuilder().NoResponse = value;
            }
        }

        public IChannel Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        public Alachisoft.NosDB.Common.Net.Address Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public DatabaseOperationType OperationType
        {
            get
            {
                if (_command == null)
                    ParseCommand();
                return OperationMapper.MapOperationType(_command.Type);
            }
        }

        public IDBOperation ParseRequest()
        {
            try
            {
                _command = Command.ParseFrom(_buffer);           //deserialization
                DatabaseOperation operaiton = Deserialize(_command) as DatabaseOperation;
                if (operaiton != null)
                {
                    operaiton.Channel = _channel;
                    operaiton.Source = _source;
                }
                return operaiton;
            }
            catch (DatabaseException de)
            {
                throw new SerializationException(de.ErrorCode, de.Message, de, de.Parameters);
            }
            catch (Exception ex)
            {
                throw new SerializationException(ErrorCodes.Cluster.UNKNOWN_ISSUE, ex.Message, ex, new string[] { ex.Message});
            }
        }

        #region Private Methods

        private IDBOperation Deserialize(Command command)
        {
            IDBOperation dbOperation = null;
            if (command == null) return null;
            switch (command.Type)
            {
                case Command.Types.Type.INSERT_DOCUMENTS:

                    dbOperation = new Common.Server.Engine.Impl.InsertDocumentsOperation(command);
                    break;

                case Command.Types.Type.DELETE_DOCUMENTS:

                    dbOperation = new Common.Server.Engine.Impl.DeleteDocumentsOperation(command);
                    break;

                case Command.Types.Type.GET_DOCUMENTS:

                    dbOperation = new Common.Server.Engine.Impl.GetDocumentsOperation(command);
                    break;

                case Command.Types.Type.UPDATE:

                    dbOperation = new Common.Server.Engine.Impl.UpdateOperation(command);
                    break;

                case Command.Types.Type.READ_QUERY:

                    dbOperation = new Common.Server.Engine.Impl.ReadQueryOperation(command);
                    break;

                case Command.Types.Type.WRITE_QUERY:

                    dbOperation = new Common.Server.Engine.Impl.WriteQueryOperation(command);
                    break;

                case Command.Types.Type.CREATE_SESSION:

                    dbOperation = new Common.Server.Engine.Impl.CreateSessionOperation(command);
                    break;

                case Command.Types.Type.GET_CHUNK:

                    dbOperation = new Common.Server.Engine.Impl.GetChunkOperation(command);
                    break;

                case Command.Types.Type.DISPOSE_READER:
                    dbOperation = new Common.Server.Engine.Impl.DisposeReaderOperation(command);
                    break;
                case Command.Types.Type.REPLACE_DOCUMENTS:
                    dbOperation = new Common.Server.Engine.Impl.ReplaceDocumentsOperation(command);
                    break;
                case Command.Types.Type.AUTHENTICATION:
                    dbOperation = new Common.Server.Engine.Impl.AuthenticationOperation(command);
                    break;
                case Command.Types.Type.INIT_DATABASE:
                    dbOperation = new Common.Server.Engine.Impl.InitDatabaseOperation(command);
                    break;
               
            }
            return dbOperation;
        }

        private void ParseCommand()
        {
            _command = Command.ParseFrom(_buffer);
        }

        #endregion
    }
}
