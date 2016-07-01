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
    public class InsertDocumentsResponse : DatabaseResponse, IDocumentsWriteResponse
    {
        private Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.Builder _insertResponse;
        private List<FailedDocument> _failedDocumentsList;

        public InsertDocumentsResponse()
        {
            _insertResponse = new Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.Builder();
           
            base.ResponseMessage = this;
        }

        public InsertDocumentsResponse(Alachisoft.NosDB.Common.Protobuf.Response response): base(response.ToBuilder())
        {
            _insertResponse = response.InsertDocumentsResponse.ToBuilder();

            if (_insertResponse.FailedDocumentsList != null && _insertResponse.FailedDocumentsList.Count > 0)
            {
                _failedDocumentsList = new List<FailedDocument>();
                foreach (Protobuf.FailedDocument document in _insertResponse.FailedDocumentsList)
                {
                    FailedDocument failedDocument = new FailedDocument();
                    failedDocument.DocumentKey = document.DocumentId;
                    failedDocument.ErrorCode = document.ErrorCode;
                    failedDocument.ErrorParameters = document.ErrorParamsList.ToArray();
                    failedDocument.ErrorMessage = string.Format(ErrorHandling.ErrorMessages.GetErrorMessage(document.ErrorCode), failedDocument.ErrorParameters);
                    _failedDocumentsList.Add(failedDocument);
                }
            }

            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            _insertResponse.FailedDocumentsList.Clear();
            if (FailedDocumentsList != null && FailedDocumentsList.Count > 0)
            {
                //Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder failedDocument;
                //failedDocument = new Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder();


                //IDictionaryEnumerator enu = FailedDocuments.GetEnumerator();
                //while (enu.MoveNext())
                //{
                //    failedDocument = new Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder();
                //    failedDocument.SetDocumentId((string)enu.Key);
                //    failedDocument.SetErrorCode((int)enu.Value);
                //    failedDocument.SetErrorParams(enu.)
                //    _insertResponse.FailedDocumentsList.Add(failedDocument.Build());
                //}

                IEnumerator<FailedDocument> enumerator = FailedDocumentsList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Protobuf.FailedDocument.Builder failedDocuments = new Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder();
                    failedDocuments.SetDocumentId(enumerator.Current.DocumentKey);
                    failedDocuments.SetErrorCode(enumerator.Current.ErrorCode);

                    if (enumerator.Current.ErrorParameters != null)
                    {
                        foreach (var errorParam in enumerator.Current.ErrorParameters)
                        {
                            failedDocuments.AddErrorParams(errorParam);
                        }
                    }
                    _insertResponse.FailedDocumentsList.Add(failedDocuments.Build());
                }
                //if (_failedDocumentList != null)
                //{
                //    foreach (FailedDocument document in _failedDocumentList)
                //    {
                //        failedDocument = new Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder();
                //        failedDocument.SetDocumentId(document.DocumentKey.ToJson());
                //        failedDocument.SetErrorCode((int)enu.Value);
                //        //failedDocument.ErrorParameters = document.ErrorParameters
                //        _insertResponse.FailedDocumentsList.Add(failedDocument.Build());
                //    }
                //}
            }
            base._response.SetInsertDocumentsResponse(_insertResponse);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.INSERT_DOCUMENTS);
        }


        public List<FailedDocument> FailedDocumentsList
        {
            get { return _failedDocumentsList; }
        }

        public void AddFailedDocument(FailedDocument document)
        {
            if(document != null)
            {
                if(_failedDocumentsList == null)
                    _failedDocumentsList = new List<FailedDocument>();

                _failedDocumentsList.Add(document);
            }
        }
    }
}
