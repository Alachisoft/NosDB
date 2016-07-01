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
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Monitoring
{
    public class ClientProcessStats : ICompactSerializable
    {
        private string _processID;
        private float _bytesSent;
        private float _bytesReceived;
        private string _server;
        private Address _client;
        private string _shard;

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public string ProcessID
        {
            get { return _processID; }
            set { _processID = value; }
        }

        public float BytesSent
        {
            get { return _bytesSent; }
            set { _bytesSent = value; }
        }

        public float BytesReceived
        {
            get { return _bytesReceived; }
            set { _bytesReceived = value; }
        }

        public Address Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public string Shard
        {
            get { return _shard; }
            set { _shard = value; }
        }

        #region ICompactSerializable Members

        public void Deserialize(CompactReader reader)
        {
            _processID = reader.ReadObject() as string;
            _bytesSent = reader.ReadSingle();
            _bytesReceived = reader.ReadSingle();
            _server = reader.ReadObject() as string;
            _client = reader.ReadObject() as Address;
        }

        public void Serialize(CompactWriter writer)
        {
            writer.WriteObject(_processID);
            writer.Write(_bytesSent);
            writer.Write(_bytesReceived);
            writer.WriteObject(_server);
            writer.WriteObject(_client);
        }

        #endregion
    }
}
