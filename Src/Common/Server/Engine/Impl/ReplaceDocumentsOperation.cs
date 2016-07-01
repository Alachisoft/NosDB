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
    public class ReplaceDocumentsOperation : DatabaseOperation, IDocumentsWriteOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.Builder _replaceCommand;
        private IList<IJSONDocument> _documents;

        public ReplaceDocumentsOperation()
        {
            _replaceCommand = new Protobuf.ReplaceDocumentsCommand.Builder();
            _documents = new List<IJSONDocument>();
            base.Message = this;
            base.OperationType = DatabaseOperationType.Replace;
        }

        public ReplaceDocumentsOperation(Alachisoft.NosDB.Common.Protobuf.Command command)
            : base(command.ToBuilder())
        {
            _replaceCommand = command.ReplaceDocumentsCommand.ToBuilder();
            _documents = new List<IJSONDocument>();

            foreach (string document in _replaceCommand.DocumentsList)
                Documents.Add(JSONDocument.Parse(document));
            

            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _replaceCommand.DocumentsList.Clear();
            foreach (JSONDocument document in Documents)
                _replaceCommand.AddDocuments(document.ToString());
            

            base._command.SetReplaceDocumentsCommand(_replaceCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.REPLACE_DOCUMENTS);
        }

        public IList<IJSONDocument> Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }

        public override IDBOperation Clone()
        {
            ReplaceDocumentsOperation replaceOperation = new ReplaceDocumentsOperation();

            foreach (JSONDocument document in Documents)
            {
                replaceOperation.Documents.Add((JSONDocument)document.Clone());
            }
            
            replaceOperation.Database = base.Database;
            replaceOperation.Collection = base.Collection;
            replaceOperation.RequestId = base.RequestId;
            replaceOperation.NoResponse = base.NoResponse;
            replaceOperation.Source = (Net.Address)base.Source.Clone();
            replaceOperation.Channel = base.Channel;
            //replaceOperation.Context = base.Context;

            return replaceOperation;
        }

        public override IDBResponse CreateResponse()
        {
            ReplaceDocumentsResponse response = new ReplaceDocumentsResponse();
            response.RequestId = base.RequestId;
            response.IsSuccessfull = true;
            return response;
        }
    }
}
