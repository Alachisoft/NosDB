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
    public class OperationMapper
    {
        
        public static  DatabaseOperationType MapOperationType(Protobuf.Command.Types.Type operationTypeArg)
        {
            DatabaseOperationType operatinType = DatabaseOperationType.Insert;
            switch (operationTypeArg)
            {
                case Protobuf.Command.Types.Type.INSERT_DOCUMENTS:
                    operatinType = DatabaseOperationType.Insert;
                    break;
                case Protobuf.Command.Types.Type.DELETE_DOCUMENTS:
                    operatinType = DatabaseOperationType.Delete;
                    break;
                case Protobuf.Command.Types.Type.GET_DOCUMENTS:
                    operatinType = DatabaseOperationType.Get;
                    break;
                case Protobuf.Command.Types.Type.UPDATE:
                    operatinType = DatabaseOperationType.Update;
                    break;
                case Protobuf.Command.Types.Type.READ_QUERY:
                    operatinType = DatabaseOperationType.ReadQuery;
                    break;
                case Protobuf.Command.Types.Type.WRITE_QUERY:
                    operatinType = DatabaseOperationType.WriteQuery;
                    break;
                case Protobuf.Command.Types.Type.GET_CHUNK:
                    operatinType = DatabaseOperationType.GetChunk;
                    break;
                case Protobuf.Command.Types.Type.DISPOSE_READER:
                    operatinType = DatabaseOperationType.DisposeReader;
                    break;
                case Protobuf.Command.Types.Type.REPLACE_DOCUMENTS:
                    operatinType = DatabaseOperationType.Replace;
                    break;
                case Protobuf.Command.Types.Type.AUTHENTICATION:
                    operatinType = DatabaseOperationType.Authenticate;
                    break;
                case Protobuf.Command.Types.Type.INIT_DATABASE:
                    operatinType = DatabaseOperationType.Init;
                    break;
               
                default:
                    throw new ArgumentOutOfRangeException("Operation "+operationTypeArg+" not supported");
            }
            return operatinType;
        }

        public static Protobuf.Command.Types.Type MapOperationType(DatabaseOperationType operationTypeArg)
        {
            Protobuf.Command.Types.Type protoOperationType = Protobuf.Command.Types.Type.INSERT_DOCUMENTS;
            switch (operationTypeArg)
            {
                case DatabaseOperationType.Init:
                    protoOperationType = Protobuf.Command.Types.Type.INIT_DATABASE;
                    break;
                case DatabaseOperationType.Insert:
                    protoOperationType = Protobuf.Command.Types.Type.INSERT_DOCUMENTS;
                    break;
                case DatabaseOperationType.Delete:
                    protoOperationType = Protobuf.Command.Types.Type.DELETE_DOCUMENTS;
                    break;
                case DatabaseOperationType.Get:
                    protoOperationType = Protobuf.Command.Types.Type.GET_DOCUMENTS;
                    break;
                case DatabaseOperationType.Replace:
                    protoOperationType = Protobuf.Command.Types.Type.REPLACE_DOCUMENTS;
                    break;
                case DatabaseOperationType.ReadQuery:
                    protoOperationType = Protobuf.Command.Types.Type.READ_QUERY;
                    break;
                case DatabaseOperationType.WriteQuery:
                    protoOperationType = Protobuf.Command.Types.Type.WRITE_QUERY;
                    break;
                case DatabaseOperationType.GetChunk:
                    protoOperationType = Protobuf.Command.Types.Type.GET_CHUNK;
                    break;
                case DatabaseOperationType.DisposeReader:
                    protoOperationType = Protobuf.Command.Types.Type.DISPOSE_READER;
                    break;
               
                default:
                    throw new ArgumentOutOfRangeException("Opration " + operationTypeArg + " not supported");
            }
            return protoOperationType;
        }

        public static IDBResponse GetResponse(DatabaseOperationType operationTypeArg)
        {
            IDBResponse response = null;
            switch (operationTypeArg)
            {
                case DatabaseOperationType.Insert:
                    response = new InsertDocumentsResponse();
                    break;
                case DatabaseOperationType.Delete:
                    response = new DeleteDocumentsResponse();
                    break;
                case DatabaseOperationType.Get:
                    response = new GetDocumentsResponse();
                    break;
                case DatabaseOperationType.Update:
                    response = new UpdateResponse();
                    break;
                case DatabaseOperationType.ReadQuery:
                    response = new ReadQueryResponse();
                    break;
                case DatabaseOperationType.WriteQuery:
                    response = new WriteQueryResponse();
                    break;
                case DatabaseOperationType.GetChunk:
                    response = new GetChunkResponse();
                    break;
                case DatabaseOperationType.DisposeReader:
                    response = new DatabaseResponse();
                    break;
                case DatabaseOperationType.Replace:
                    response = new ReplaceDocumentsResponse();
                    break;
                case DatabaseOperationType.Authenticate:
                    response = new AuthenticationResponse();
                    break;
                case DatabaseOperationType.Init:
                    response = new InitDatabaseResponse();
                    break;
                default:
                    throw new NotSupportedException("Operation " + operationTypeArg + " not supported");
            }
            return response;
        }
    }
}
