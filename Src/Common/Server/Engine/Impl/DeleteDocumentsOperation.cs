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
using Alachisoft.NosDB.Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class DeleteDocumentsOperation: DatabaseOperation,IDocumentsWriteOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.Builder _deleteCommand;
        private IList<IJSONDocument> _documentIds;

        public DeleteDocumentsOperation()
        {
            _deleteCommand = new Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.Builder();
            _documentIds = new List<IJSONDocument>();
            base.Message = this;
            base.OperationType = DatabaseOperationType.Delete;
        }

        public DeleteDocumentsOperation(Alachisoft.NosDB.Common.Protobuf.Command command) : base(command.ToBuilder())
        {
            _deleteCommand = command.DeleteDocumentsCommand.ToBuilder();
            _documentIds = new List<IJSONDocument>();

            foreach (string document in _deleteCommand.DocumentIdsList)
            {
                _documentIds.Add(JSONDocument.Parse(document));
            }
            
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _deleteCommand.DocumentIdsList.Clear();
            foreach (JSONDocument document in _documentIds)
            {
                _deleteCommand.DocumentIdsList.Add(document.ToString());
            }

            base._command.SetDeleteDocumentsCommand(_deleteCommand);
            base._command.SetType( Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.DELETE_DOCUMENTS);
        }

        public IList<IJSONDocument> Documents
        {
            get { return _documentIds; }
            set { _documentIds = value; }
        }

       

        public override IDBOperation Clone()
        {
            DeleteDocumentsOperation deleteOperation = new DeleteDocumentsOperation();

            foreach (IJSONDocument documentId in Documents)
            {
                deleteOperation.Documents.Add((JSONDocument)documentId.Clone());
            }

            deleteOperation.Database = base.Database;
            deleteOperation.Collection = base.Collection;
            deleteOperation.RequestId = base.RequestId;
            deleteOperation.NoResponse = base.NoResponse;
            deleteOperation.Source = (Net.Address)base.Source.Clone();
            deleteOperation.Channel = base.Channel;
            //deleteOperation.Context = base.Context;
            
            return deleteOperation;
        }

        public override IDBResponse CreateResponse()
        {
            DeleteDocumentsResponse response = new DeleteDocumentsResponse();
            response.RequestId = base.RequestId;
            response.IsSuccessfull = true;
            return response;
        }
       
    }
}
