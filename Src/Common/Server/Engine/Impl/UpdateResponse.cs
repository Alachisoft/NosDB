﻿// /*
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
    public class UpdateResponse : DatabaseResponse, IUpdateResponse
    {
        private Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Builder _updateResponse;
        private long _affectedDocuments;

        public UpdateResponse()
        {
            _updateResponse = new Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Builder();
            base.ResponseMessage = this;
        }

        public UpdateResponse(Alachisoft.NosDB.Common.Protobuf.Response response):base(response.ToBuilder())
        {
            _updateResponse = response.UpdateResponse.ToBuilder();
            _affectedDocuments = _updateResponse.AffectedDocuments;
            base.ResponseMessage = this;
        }

        internal override void BuildInternal()
        {
            _updateResponse.AffectedDocuments = _affectedDocuments;
            base._response.SetUpdateResponse(_updateResponse);
            base._response.SetType(Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.UPDATE);
        }

        public long AffectedDocuments
        {
            get { return _affectedDocuments; }
            set { _affectedDocuments = value; }
        }
    }
}
