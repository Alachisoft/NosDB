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
    public class InsertDocumentsOperation : DatabaseOperation, IDocumentsWriteOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.Builder _insertCommand;

        private IList<IJSONDocument> _documents;

        public InsertDocumentsOperation() 
        {
            _insertCommand = new Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.Builder();
            _documents = new List<IJSONDocument>();
            base.Message = this;
            base.OperationType = DatabaseOperationType.Insert;
        }

        public InsertDocumentsOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            _insertCommand = command.InsertDocumentsCommand.ToBuilder();
            _documents = new List<IJSONDocument>();

            foreach (string document in _insertCommand.DocumentsList)
                Documents.Add(JSONDocument.Parse(document));
            
            
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _insertCommand.DocumentsList.Clear();
            foreach (JSONDocument document in Documents)
                _insertCommand.AddDocuments(document.ToString());
            

            base._command.SetInsertDocumentsCommand(_insertCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.INSERT_DOCUMENTS);
        }

        public IList<IJSONDocument> Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }
      

        public override IDBResponse CreateResponse()
        {
            InsertDocumentsResponse response = new InsertDocumentsResponse();
            response.RequestId = base.RequestId;
            response.IsSuccessfull = true;
            return response;
        }

        #region Clone
        public override IDBOperation Clone()
        {
            InsertDocumentsOperation insertOperation = new InsertDocumentsOperation();

            foreach (JSONDocument document in Documents)
            {
                insertOperation.Documents.Add((JSONDocument)document.Clone());
            }
            
            insertOperation.Database = base.Database;
            insertOperation.Collection = base.Collection;
            insertOperation.RequestId = base.RequestId;
            insertOperation.NoResponse = base.NoResponse;
            insertOperation.Source = (Net.Address)base.Source.Clone();
            insertOperation.Channel = base.Channel;
            //insertOperation.Context = base.Context;

            return insertOperation;
        }
        #endregion
    }
}
