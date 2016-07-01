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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class ReplaceDocumentsResponse : DatabaseResponse, IDocumentsWriteResponse
    {
        private Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.Builder _replaceResponse;
        private List<FailedDocument> _failedDocumentsList; 

        public ReplaceDocumentsResponse()
        {
            _replaceResponse = new Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.Builder();
            base.ResponseMessage = this;
        }

        public ReplaceDocumentsResponse(Alachisoft.NosDB.Common.Protobuf.Response response): base(response.ToBuilder())
        {
            _replaceResponse = response.ReplaceDocumentsResponse.ToBuilder();

            if (_replaceResponse.FailedDocumentsList != null && _replaceResponse.FailedDocumentsList.Count > 0)
            {
                _failedDocumentsList = new List<FailedDocument>();
                foreach (Protobuf.FailedDocument document in _replaceResponse.FailedDocumentsList)
                {
                    FailedDocument failedDocument = new FailedDocument();
                    failedDocument.DocumentKey = document.DocumentId;
                    failedDocument.ErrorCode = document.ErrorCode;
                    failedDocument.ErrorParameters = document.ErrorParamsList.ToArray();
                    _failedDocumentsList.Add(failedDocument);
                }
            }

            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            _replaceResponse.FailedDocumentsList.Clear();
            if (FailedDocumentsList != null && FailedDocumentsList.Count > 0)
            {
                IEnumerator<FailedDocument> enu = _failedDocumentsList.GetEnumerator();
                while (enu.MoveNext())
                {
                    Protobuf.FailedDocument.Builder failedDocuments = new Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder();
                    failedDocuments.SetDocumentId(enu.Current.DocumentKey);
                    failedDocuments.SetErrorCode(enu.Current.ErrorCode);


                    if (enu.Current.ErrorParameters != null)
                    {
                        foreach (var errorParam in enu.Current.ErrorParameters)
                        {
                            failedDocuments.AddErrorParams(errorParam);
                        }
                    }

                    _replaceResponse.FailedDocumentsList.Add(failedDocuments.Build());
                }
            }
            base._response.SetReplaceDocumentsResponse(_replaceResponse);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.REPLACE_DOCUMENTS);
        }
       
        public List<FailedDocument> FailedDocumentsList
        {
            get { return _failedDocumentsList; }
        }

        public void AddFailedDocument(FailedDocument document)
        {
            if (document != null)
            {
                if (_failedDocumentsList == null)
                    _failedDocumentsList = new List<FailedDocument>();

                if (!_failedDocumentsList.Contains(document))
                        _failedDocumentsList.Add(document);
            }
        }
    }
}
