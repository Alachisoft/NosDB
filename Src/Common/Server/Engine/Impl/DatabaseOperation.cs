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
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    /// <summary>
    /// Protobuff based implementation of Database command
    /// </summary>

    public class DatabaseOperation : IDBOperation
    {
        protected Alachisoft.NosDB.Common.Protobuf.Command.Builder _command;
        private DatabaseOperationType _operationType;
        private IChannel _channel;
        private Net.Address _source;
        private object _message;
        private bool _noResponse;
        private IOperationContext _context = new OperationContext();

        public DatabaseOperation()
        {
            _command = new Alachisoft.NosDB.Common.Protobuf.Command.Builder();
            _message = new object();
        }

        public DatabaseOperation(Alachisoft.NosDB.Common.Protobuf.Command.Builder command)
        {
            _command = command;
            _operationType = OperationMapper.MapOperationType(command.Type);
            _message = new object();
        }

        internal virtual void BuildInternal()
        {
        }

        public DatabaseOperationType OperationType
        {
            get { return _operationType; }
            set { _operationType = value; }
        }

        private long _requestId;

        public long RequestId
        {
            get
            {
                if (_command != null)
                    return _command.RequestId;
                else return _requestId;
            }
            set
            {
                if (_command != null)
                    _command.RequestId = value;
                else _requestId = value;
            }
        }

        private string _database;

        public string Database
        {
            get
            {
                if (_command != null)
                    return _command.DatabaseName;
                else return _database;
            }
            set
            {
                if (_command != null)
                    _command.DatabaseName = value;
                else _database = value;
            }
        }

        private string _collection;

        public string Collection
        {
            get
            {
                if (_command != null)
                    return _command.CollectionName;
                else return _collection;
            }
            set
            {
                if (_command != null)
                    _command.CollectionName = value;
                else _collection = value;
            }
        }

        [JsonIgnore]
        public IOperationContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public byte[] Serialize()
        {
            BuildInternal();
            Alachisoft.NosDB.Common.Protobuf.Command commandObj = _command.Build();
            return commandObj.ToByteArray();
        }

        public virtual IDBResponse CreateResponse()
        {
            DatabaseResponse response = new DatabaseResponse();
            response.RequestId = RequestId;
            return response;
        }

        [JsonIgnore]
        public object Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public bool NoResponse
        {
            get
            {
                if (_command != null)
                    return _command.NoResponse;
                else return _noResponse;
            }
            set
            {
                if (_command != null)
                    _command.NoResponse = value;
                else _noResponse = value;
            }
        }

        [JsonIgnore]
        public IChannel Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        [JsonIgnore]
        public Net.Address Source
        {
            get { return _source; }
            set { _source = value; }
        }

        #region Clone

        public virtual IDBOperation Clone()
        {
            DatabaseOperation databaseOperation = new DatabaseOperation();
            databaseOperation.Database = Database;
            databaseOperation.Collection = Collection;
            databaseOperation.RequestId = RequestId;
            databaseOperation.OperationType = OperationType;
            databaseOperation.NoResponse = NoResponse;
            databaseOperation.Source = (Net.Address) Source.Clone();
            databaseOperation.Message = ((IDBOperation) Message).Clone();
            //databaseOperation.Context = Context;
            databaseOperation.Channel = Channel;
            databaseOperation.SessionId = SessionId;
            return databaseOperation;
        }

        #endregion



        /// <summary>
        /// used to identify which client is performing an operatoin on database engine
        /// </summary>
        public ISessionId SessionId
        {
            set
            {
                if (_command != null)
                {
                    Protobuf.SessionId.Builder sessionBuilder = new Protobuf.SessionId.Builder();
                    if (value is ClientSessionId)
                    {
                        ClientSessionId clientSessionId = value as ClientSessionId;
                        sessionBuilder.ClientSessionId = clientSessionId.SessionId;
                        sessionBuilder.RouterSessionId = clientSessionId.RouterSessionId.SessionId;
                    }
                    else
                    {
                        sessionBuilder.ClientSessionId = value.SessionId;
                    }
                    _command.SessionId = sessionBuilder.Build();
                }
            }
            get
            {
                if (_command != null)
                {
                    Protobuf.SessionId sessionId = _command.SessionId;
                    if (!string.IsNullOrEmpty(sessionId.RouterSessionId))
                    {
                        return new ClientSessionId()
                        {
                            RouterSessionId = new RouterSessionId() {SessionId = sessionId.RouterSessionId},
                            SessionId = sessionId.ClientSessionId
                        };
                    }
                    else
                        return new RouterSessionId() {SessionId = sessionId.ClientSessionId};
                }
                return null;
            }

        }
    }
}
