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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.ProtocolBuffers.Collections;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    /// <summary>
    /// Protobuff based implementation of database response
    /// </summary>
    public class DatabaseResponse:IDBResponse,Alachisoft.NosDB.Common.Communication.IResponse
    {
        protected Alachisoft.NosDB.Common.Protobuf.Response.Builder _response;
        private object _message;
        private IChannel _channel = null;
        private Net.Address _source = null;

        public DatabaseResponse()
        {
            _response = new Alachisoft.NosDB.Common.Protobuf.Response.Builder();
            _message = this;
        }

        public DatabaseResponse(Alachisoft.NosDB.Common.Protobuf.Response.Builder response)
        {
            _response = response;
            _message = this;
        }

        internal virtual void BuildInternal() {}
        
        internal void SetResposeType(Protobuf.Response.Types.Type responseType)
        {
            _response.SetType(responseType);
        }

        public long RequestId
        {
            get { return _response.RequestId; }
            set { _response.RequestId = value; }
        }

        public bool IsSuccessfull
        {
            get { return _response.IsSuccessful; }
            set { _response.IsSuccessful = value; }
        }

        public int ErrorCode
        {
            get { return _response.ErrorCode; }
            set { _response.ErrorCode = value; }
        }

        public string[] ErrorParams
        {
            get { return _response.ErrorParamsList.ToArray(); } 
            set
            {
                if (value == null) return;
                for (int i = 0; i < value.Length; i++)
                {
                    _response.ErrorParamsList.Add(value[i]);
                }
            }
        }

        public byte[] Serialize()
        {
            BuildInternal();
            Alachisoft.NosDB.Common.Protobuf.Response response = _response.Build();
            return response.ToByteArray();
        }

        public object ResponseMessage
        {
            get { return _message; }
            set { _message = value; }
        }

        public IChannel Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        public Net.Address Source
        {
            get { return _source; }
            set { _source = value; }
        }


        public Exception Error
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }
    }
}
