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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class UpdateOperation : DatabaseOperation, IUpdateOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Builder _updateCommand;
        private Query _query;

        public UpdateOperation() 
        {
            _updateCommand = new Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Builder();
            _query = new Query();
            base.Message = this;
            base.OperationType = DatabaseOperationType.Update;
        }

        public UpdateOperation(Alachisoft.NosDB.Common.Protobuf.Command command): base(command.ToBuilder())
        {
            _updateCommand = command.UpdateCommand.ToBuilder();
            _query = new Query();

            _query.QueryText = _updateCommand.Query.Query;

            //foreach (Alachisoft.NosDB.Common.Protobuf.Parameter param in _updateCommand.Query.ParametersList)
            //{
            //    switch ((ParameterType)param.JsonDataType)
            //    {
            //        case ParameterType.NULL:
            //            _query.Parameters.Add(new Parameter(param.Attribute, null));
            //            break;
            //        case ParameterType.BOOLEAN:
            //            _query.Parameters.Add(new Parameter(param.Attribute, bool.Parse(param.Value)));
            //            break;
            //        case ParameterType.DATETIME:
            //            _query.Parameters.Add(new Parameter(param.Attribute, DateTime.Parse(param.Value)));
            //            break;
            //        case ParameterType.STRING:
            //            _query.Parameters.Add(new Parameter(param.Attribute, param.Value));
            //            break;
            //        case ParameterType.LONG:
            //            _query.Parameters.Add(new Parameter(param.Attribute, long.Parse(param.Value)));
            //            break;
            //        case ParameterType.DOUBLE:
            //            _query.Parameters.Add(new Parameter(param.Attribute, double.Parse(param.Value)));
            //            break;
            //        case ParameterType.ARRAY:
            //            _query.Parameters.Add(new Parameter(param.Attribute, JsonConvert.DeserializeObject<JArray>(param.Value)));
            //            break;
            //        default:
            //            _query.Parameters.Add(new Parameter(param.Attribute, JSONDocument.Parse(param.Value)));
            //            break;
            //    }
            //}
            _query.Parameters = QueryParameterConverter.GetParameterList(_updateCommand.Query.ParametersList);
            
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Builder protobufQuery = new Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Builder();
            Alachisoft.NosDB.Common.Protobuf.Parameter.Builder protobufParameter;

            protobufQuery.Query = _query.QueryText;

            foreach (Parameter parameter in _query.Parameters)
            {
                //protobufParameter = new Alachisoft.NosDB.Common.Protobuf.Parameter.Builder();
                //protobufParameter.Attribute = parameter.Name;

                //if (parameter.Value == null)
                //{
                //    protobufParameter.Value = "null";
                //    protobufParameter.JsonDataType = (int)ParameterType.NULL;
                //}
                //else
                //{
                //    protobufParameter.JsonDataType = (int)DataTypeMapper.MapDataType(parameter.Value);
                //    if (protobufParameter.JsonDataType == (int)ParameterType.ARRAY || protobufParameter.JsonDataType == (int)ParameterType.OBJECT)
                //        protobufParameter.Value = JsonConvert.SerializeObject(parameter.Value);
                //    else if (protobufParameter.JsonDataType == (int)ParameterType.JSONDOCUMENT)
                //        protobufParameter.Value = parameter.Value.ToString();
                //    else
                //        protobufParameter.Value = parameter.Value.ToString();
                //}
                protobufParameter = QueryParameterConverter.GetProtobufParameters(parameter);
                protobufQuery.ParametersList.Add(protobufParameter.Build());
            }

            _updateCommand.SetQuery(protobufQuery);
            

            base._command.SetUpdateCommand(_updateCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.UPDATE);
        }

        public IQuery Query
        {
            get { return _query; }
            set { _query = (Query)value; }
        }
        
        public override IDBResponse CreateResponse()
        {
            UpdateResponse response = new UpdateResponse();
            response.RequestId = base.RequestId;
            return response;
        }

        #region Clone
        public override IDBOperation Clone()
        {
            UpdateOperation updateOperation = new UpdateOperation();
            updateOperation.Query.QueryText = _query.QueryText;

            foreach (Parameter parameter in _query.Parameters)
                updateOperation.Query.Parameters.Add(new Parameter(parameter.Name, parameter.Value));
            
            updateOperation.Database = base.Database;
            updateOperation.Collection = base.Collection;
            updateOperation.RequestId = base.RequestId;
            updateOperation.NoResponse = base.NoResponse;
            updateOperation.Source = (Net.Address)base.Source.Clone();
            updateOperation.Channel = base.Channel;
            
            return updateOperation;
        }
        #endregion
    }
}
