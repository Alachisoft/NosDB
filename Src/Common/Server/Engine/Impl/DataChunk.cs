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

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class DataChunk : IDataChunk, ICompactSerializable
    {
        private int _chunkId;
        private string _readerUID;
        private List<IJSONDocument> _documents = new List<IJSONDocument>();
        private bool _isLastChunk;
        private string _queryString;
        private bool _doCaching = false;

        public DataChunk() { }

        public string ReaderUID
        {
            get { return _readerUID; }
            set { _readerUID = value; }
        }

        public IList<IJSONDocument> Documents
        {
            get { return _documents; }
            set { _documents = (List<IJSONDocument>)value; }
        }

        public bool IsLastChunk
        {
            get { return _isLastChunk; }
            set { _isLastChunk = value; }
        }

        public int ChunkId
        {
            get { return _chunkId; }
            set { _chunkId = value; }
        }

        public string QueryString
        {
            get { return _queryString; }
            set { _queryString = value; }
        }

        public bool DoCaching
        {
            get { return _doCaching; }
            set { this._doCaching = value; }
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _isLastChunk = reader.ReadBoolean();
            _readerUID = reader.ReadString();
            _documents = (List<IJSONDocument>)reader.ReadObject();
            _chunkId = reader.ReadInt32();
            _queryString = reader.ReadString();
            _doCaching = reader.ReadBoolean();

        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_isLastChunk);
            writer.Write(_readerUID);
            writer.WriteObject(_documents);
            writer.Write(_chunkId);
            writer.Write(_queryString);
            writer.Write(_doCaching);
        }


    }
}
