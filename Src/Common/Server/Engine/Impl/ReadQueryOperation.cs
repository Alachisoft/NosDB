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

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class ReadQueryOperation :  DatabaseOperation, IQueryOperation
    {
        private Protobuf.ReadQueryCommand.Builder _readQueryCommand;
        private Query _query;
        

        public ReadQueryOperation()
        {
            _readQueryCommand = new Protobuf.ReadQueryCommand.Builder();
            _query = new Query();
            Message = this;
            OperationType = DatabaseOperationType.ReadQuery;
        }

        public ReadQueryOperation(Protobuf.Command command): base(command.ToBuilder())
        {
            _readQueryCommand = command.ReadQueryCommand.ToBuilder();

            _query = new Query();

            _query = new Query
            {
                QueryText = _readQueryCommand.Query.Query,
                Parameters = QueryParameterConverter.GetParameterList(_readQueryCommand.Query.ParametersList)
            };

            Message = this;

        }

        internal override void BuildInternal()
        {
            var protobufQuery = new Protobuf.QueryBuilder.Builder();

            protobufQuery.Query = _query.QueryText;

            foreach (Parameter parameter in _query.Parameters)
            {
                Protobuf.Parameter.Builder protobufParameter = QueryParameterConverter.GetProtobufParameters(parameter);
                protobufQuery.ParametersList.Add(protobufParameter.Build());
            }

            _readQueryCommand.SetQuery(protobufQuery);



            _command.SetReadQueryCommand(_readQueryCommand);
            _command.SetType(Protobuf.Command.Types.Type.READ_QUERY);

        }

        public IQuery Query
        {
            get { return _query; }
            set { _query = (Query)value; }
        }

        public override IDBResponse CreateResponse()
        {
            ReadQueryResponse response = new ReadQueryResponse();
            response.RequestId = RequestId;
            //temp changes
            response.DataChunk.ReaderUID = "-1";
            return response;
        }

        #region Clone
        public override IDBOperation Clone()
        {
            ReadQueryOperation readQueryOperation = new ReadQueryOperation();
            readQueryOperation.Query.QueryText = _query.QueryText;

            foreach (Parameter parameter in _query.Parameters)
                readQueryOperation.Query.Parameters.Add(new Parameter(parameter.Name, parameter.Value));

            readQueryOperation.Database = base.Database;
            readQueryOperation.Collection = base.Collection;
            readQueryOperation.RequestId = base.RequestId;
            readQueryOperation.NoResponse = base.NoResponse;
            readQueryOperation.Source = (Net.Address)base.Source.Clone();
            readQueryOperation.Channel = base.Channel;
            //readQueryOperation.Context = base.Context;

            return readQueryOperation;
        }
        #endregion

    }
}
