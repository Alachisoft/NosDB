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
using Alachisoft.NosDB.Common.Serialization.IO;
using System.Threading;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class CollectionMetadata :ICompactSerializable
    {
        private long _rowId = 0;
        private long _size = 0;

        public long GetNextRowId()
        {
            _rowId = Interlocked.Increment(ref _rowId);
            return _rowId;
        }

        public long Size
        {
            get { return _size; }
            //set { _size = value; }
        }

        public void ChangeDataSize(long change)
        {
            _size += change;
        }

        #region ICompactSerializable Members
        public void Deserialize(CompactReader reader)
        {
            _rowId = reader.ReadInt64();
            _size = reader.ReadInt64();
        }

        public void Serialize(CompactWriter writer)
        {
            writer.Write(_rowId);
            writer.Write(_size);
        }
        #endregion
    }
}
