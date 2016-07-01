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
    public class GetDocumentsOperation : DatabaseOperation, IGetOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Builder _getDocumentsCommand;
        private IList<IJSONDocument> _documentId;

        public GetDocumentsOperation()
        {
            _getDocumentsCommand = new Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Builder();
            _documentId = new List<IJSONDocument>();
            base.Message = this;
            base.OperationType = DatabaseOperationType.Get;
        }

        public GetDocumentsOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            _getDocumentsCommand = command.GetDocumentsCommand.ToBuilder();
            _documentId = new List<IJSONDocument>();

            foreach (string document in _getDocumentsCommand.DocumentIdsList)
                DocumentIds.Add(JSONDocument.Parse(document));

            base.Message = this;
        }

        internal override void BuildInternal()
        {
            _getDocumentsCommand.DocumentIdsList.Clear();
            if (DocumentIds != null && DocumentIds.Count > 0)
                foreach (JSONDocument document in DocumentIds)
                    _getDocumentsCommand.DocumentIdsList.Add(document.ToString());

            base._command.SetGetDocumentsCommand(_getDocumentsCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.GET_DOCUMENTS);
        }

        public IList<IJSONDocument> DocumentIds
        {
            get { return _documentId; }
            set { _documentId = value; }
        }

        public override IDBOperation Clone()
        {
            //deep clone
            return base.Clone();
        }

        public override IDBResponse CreateResponse()
        {
            GetDocumentsResponse response = new GetDocumentsResponse();
            response.RequestId = base.RequestId;
            //temp changes
            response.DataChunk.ReaderUID = "-1";
            return response;
        }
    }
}
