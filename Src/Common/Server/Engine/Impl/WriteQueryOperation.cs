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
using Alachisoft.NosDB.Common.JSON.CustomConverter;
using Newtonsoft.Json.Linq;
 using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;
using System;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    [JsonConverter(typeof(LogOperationConverter))]
    public class WriteQueryOperation :DatabaseOperation, INonQueryOperation, ICompactSerializable
    {
        private Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.Builder _writeQueryCommand;
        private Query _query;

        public WriteQueryOperation()
            : base()
        {
            _writeQueryCommand = new Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.Builder();
            _query = new Query();
            base.Message = this;
            base.OperationType = DatabaseOperationType.WriteQuery;
        }

        public WriteQueryOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            _writeQueryCommand = command.WriteQueryCommand.ToBuilder();
            _query = new Query();

            _query.QueryText = _writeQueryCommand.Query.Query;

            //foreach (Alachisoft.NosDB.Common.Protobuf.Parameter param in _writeQueryCommand.Query.ParametersList)
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
            _query.Parameters = QueryParameterConverter.GetParameterList(_writeQueryCommand.Query.ParametersList);
            
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

                //if(parameter.Value == null)
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
                //        protobufParameter.Value= parameter.Value.ToString();
                //    else
                //        protobufParameter.Value = parameter.Value.ToString();
                //}
                protobufParameter = QueryParameterConverter.GetProtobufParameters(parameter);
                protobufQuery.ParametersList.Add(protobufParameter.Build());
            }

            _writeQueryCommand.SetQuery(protobufQuery);
            

            base._command.SetWriteQueryCommand(_writeQueryCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.WRITE_QUERY);
        }

        public IQuery Query
        {
            get { return _query; }
            set { _query = (Query)value; }
        }
        

        public override IDBResponse CreateResponse()
        {
            WriteQueryResponse response = new WriteQueryResponse();
            response.RequestId = base.RequestId;
            return response;
        }

        #region Clone 
        public override IDBOperation Clone()
        {
            WriteQueryOperation writeQueryOperation = new WriteQueryOperation();
            writeQueryOperation.Query.QueryText = _query.QueryText;

            foreach (Parameter parameter in _query.Parameters)
                writeQueryOperation.Query.Parameters.Add(new Parameter(parameter.Name, parameter.Value));
            
            writeQueryOperation.Database = base.Database;
            writeQueryOperation.Collection = base.Collection;
            writeQueryOperation.RequestId = base.RequestId;
            writeQueryOperation.NoResponse = base.NoResponse;
            writeQueryOperation.Source = (Net.Address)base.Source.Clone();
            writeQueryOperation.Channel = base.Channel;
            //writeQueryOperation.Context = base.Context;

            return writeQueryOperation;
        }
        #endregion

        #region ICompactSerializable
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            OperationType = reader.ReadObjectAs<DatabaseOperationType>();
            Collection = reader.ReadObject() as string;
            Database = reader.ReadObject() as string;
            Query = reader.ReadObject() as Query;
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(this.OperationType);
            writer.WriteObject(this.Collection);
            writer.WriteObject(this.Database);
            writer.WriteObject(this.Query);
        }
        #endregion
    }
}
