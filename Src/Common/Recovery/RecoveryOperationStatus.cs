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
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Recovery
{
     // operation status used throughout recovery module
    public class RecoveryOperationStatus:ICompactSerializable
    {
        private string _jobIdentifier;
        private RecoveryStatus _status;
        private string _message;

        public RecoveryOperationStatus(RecoveryStatus status)
        {
            this._status = status;
        }
        
        public string JobIdentifier
        {
            get { return _jobIdentifier; }
            set { _jobIdentifier = value; }
        }

        public RecoveryStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }
        
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public override string ToString()
        {
            string message = "id: " + JobIdentifier + " \tstate: " + Status.ToString() + " \tmessage: " + _message;
            return message;
        }

        #region ICompactSerializable
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _jobIdentifier = reader.ReadString();
            Message = reader.ReadString();
            Status = (RecoveryStatus)reader.ReadObject();
           
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_jobIdentifier);
            writer.Write(Message);
            writer.WriteObject(Status);
           
        }
        #endregion
    }
}
