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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Stats
{
    [Serializable]
    public class BucketStatistics : ICompactSerializable,ICloneable
    {
        private long _count;
        private long _dataSize;
        private bool _isDirty = false;

        public BucketStatistics() { }

        public long Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public long DataSize
        {
            get { return _dataSize; }
            set { _dataSize = value; }
        }
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public void Increment(long dataSize)
        {
            lock (this)
            {
                _count++;
                _dataSize += dataSize;
            }
        }

        public void Decrement(long dataSize)
        {
            lock (this)
            {
                _count--;
                _dataSize -= dataSize;
            }
        }

        public void Clear()
        {
            lock (this)
            {
                _count = 0;
                _dataSize = 0;
            }
        }

        public void SerializeLocal(CompactWriter writer)
        {
            writer.Write(_count);
            writer.Write(_dataSize);
            writer.Write(_isDirty);
        }

        public void DeserializeLocal(CompactReader reader)
        {
            _count = reader.ReadInt64();
            _dataSize = reader.ReadInt64();
            _isDirty = reader.ReadBoolean();
        }

        #region ICompactSerializable Members

        void ICompactSerializable.Deserialize(CompactReader reader)
        {
            _count = reader.ReadInt64();
            _dataSize = reader.ReadInt64();
            _isDirty = reader.ReadBoolean();
        }

        void ICompactSerializable.Serialize(CompactWriter writer)
        {
            writer.Write(_count);
            writer.Write(_dataSize);
            writer.Write(_isDirty);
        }

        #endregion

        public object Clone()
        {
            BucketStatistics clone = new BucketStatistics() { _count = this._count, _dataSize = this._dataSize, _isDirty = this._isDirty };
            return clone;
        }
    }

   
}
