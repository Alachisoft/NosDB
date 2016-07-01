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
using Alachisoft.NosDB.Common.Protobuf;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Communication.Formatters
{
    public class DatabaseOperationFormatter : IChannelFormatter
    {
        public byte[] Serialize(object graph)
        {
            if (graph != null)
            {
                try
                {
                    var operation = ((ChannelRequest)graph).Message as IDBOperation;

                    if (operation != null)
                    {
                        operation.RequestId = ((ChannelRequest)graph).RequestId;
                        return operation.Serialize();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Serialize Request on client Side : " + ex);
                    throw;
                }
            }

            return null;
        }

        public object Deserialize(byte[] buffer)
        {
            //deseralize
            IDBResponse dbResponse = null;

            try
            {
                Response response = Response.ParseFrom(buffer);

                if (response != null)
                {
                    switch (response.Type)
                    {
                        case Response.Types.Type.INSERT_DOCUMENTS:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.InsertDocumentsResponse(response);
                            break;
                        case Response.Types.Type.GET_DOCUMENTS:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.GetDocumentsResponse(response);
                            break;
                        case Response.Types.Type.DELETE_DOCUMENTS:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.DeleteDocumentsResponse(response);
                            break;
                        case Response.Types.Type.UPDATE:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.UpdateResponse(response);
                            break;
                        case Response.Types.Type.WRITE_QUERY:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.WriteQueryResponse(response);
                            break;
                        case Response.Types.Type.READ_QUERY:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.ReadQueryResponse(response);
                            break;
                        case Response.Types.Type.CREATE_SESSION:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.CreateSessionResponse(response);
                            break;
                        case Response.Types.Type.GET_CHUNK:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.GetChunkResponse(response);
                            break;
                        case Response.Types.Type.DISPOSE_READER:
                            dbResponse = new DatabaseResponse(response.ToBuilder());
                            break;
                        case Response.Types.Type.REPLACE_DOCUMENTS:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.ReplaceDocumentsResponse(response);
                            break;
                        case Response.Types.Type.AUTHENTICATION:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.AuthenticationResponse(response);
                            break;
                        case Response.Types.Type.INIT_DATABASE:
                            dbResponse = new Alachisoft.NosDB.Common.Server.Engine.Impl.InitDatabaseResponse(response);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Deserialize Response on client Side : " + ex);
                throw;
            }

            return dbResponse;
        }
    }
}