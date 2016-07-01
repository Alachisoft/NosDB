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
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.ErrorHandling;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    class QueryParameterConverter
    {

        public static IList<IParameter> GetParameterList(IList<Protobuf.Parameter> paramList )
        {
           
            IList<IParameter> parameterList = new List<IParameter>();
            foreach (Protobuf.Parameter param in paramList)
            {
                try
                {
                    switch ((ParameterType)param.JsonDataType)
                    {
                        case ParameterType.NULL:
                            parameterList.Add(new Parameter(param.Attribute, null));
                            break;
                        case ParameterType.BOOLEAN:
                            parameterList.Add(new Parameter(param.Attribute, bool.Parse(param.Value)));
                            break;
                        case ParameterType.DATETIME:
                            parameterList.Add(new Parameter(param.Attribute, DateTime.Parse(param.Value)));
                            break;
                        case ParameterType.STRING:
                            parameterList.Add(new Parameter(param.Attribute, param.Value));
                            break;
                        case ParameterType.LONG:
                            parameterList.Add(new Parameter(param.Attribute, long.Parse(param.Value)));
                            break;
                        case ParameterType.DOUBLE:
                            parameterList.Add(new Parameter(param.Attribute, double.Parse(param.Value)));
                            break;
                        case ParameterType.ARRAY:
                            parameterList.Add(new Parameter(param.Attribute,
                                Alachisoft.NosDB.Common.JSON.JsonDocumentUtil.ParseArray(JsonConvert.DeserializeObject<JArray>(param.Value))));
                            break;
                        default:
                            parameterList.Add(new Parameter(param.Attribute, JSONDocument.Parse(param.Value)));
                            break;
                    }
                }
                catch (NotSupportedException ex)
                {
                    throw new QuerySystemException(ErrorCodes.Query.PARAMETER_NOT_SUPPORTED, new[] { param.Attribute, ex.Message });
                }
            }
            return parameterList;   
        }

        public static Alachisoft.NosDB.Common.Protobuf.Parameter.Builder GetProtobufParameters(Parameter param)
        {
            try
            {
                Alachisoft.NosDB.Common.Protobuf.Parameter.Builder protobufParameter = new Alachisoft.NosDB.Common.Protobuf.Parameter.Builder();
                protobufParameter.Attribute = param.Name;
                if (param.Value == null)
                {
                    protobufParameter.Value = "null";
                    protobufParameter.JsonDataType = (int)ParameterType.NULL;
                }
                else
                {
                    ParameterType parameterDataType = DataTypeMapper.MapDataType(param.Value);
                    if (parameterDataType == ParameterType.NOTSUPPORTED)
                        throw new DatabaseException(ErrorCodes.Distributor.DATA_TYPE_NOT_SUPPORTED, new[] { param.Value.GetType().Name });

                    protobufParameter.JsonDataType = (int)parameterDataType;

                    if (protobufParameter.JsonDataType == (int)ParameterType.ARRAY || protobufParameter.JsonDataType == (int)ParameterType.OBJECT)
                    {
                        protobufParameter.Value = JsonConvert.SerializeObject(param.Value);
                    }
                    else
                    {
                        protobufParameter.Value = param.Value.ToString();
                    }
                }
                return protobufParameter;
            }
            catch (NotSupportedException ex)
            {
                throw new QuerySystemException(ErrorCodes.Query.PARAMETER_NOT_SUPPORTED, new[] { param.Name, ex.Message });
            }
        }
    }
}
