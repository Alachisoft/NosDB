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
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Stats
{
    //This class was moved from client to common  and is currently used for keeping stats in providers
    public class CollectionStatistics : ICloneable, ICompactSerializable
    {
        private long _dataSize;
        private long _documentCount;

        /// <summary>
        /// Gets/Sets size of the data in bytes
        /// </summary>
        [JsonProperty(PropertyName = "DataSize")]
        public long DataSize
        {
            get { return _dataSize; }
            set { _dataSize = value; }
        }

        /// <summary>
        /// Gets/Sets number of documents in a collection.
        /// </summary>
        [JsonProperty(PropertyName = "DocumentCount")]
        public long DocumentCount
        {
            get { return _documentCount; }
            set { _documentCount = value; }
        }
        // TO-DO Add more statistics as needed

        #region ICloneable Member
        public object Clone()
        {
            CollectionStatistics stats = new CollectionStatistics();
            stats.DataSize = _dataSize;
            stats.DocumentCount = _documentCount;
            return stats;
        }
        #endregion

        #region ICompactSerializable Member
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _dataSize = reader.ReadInt64();
            _documentCount = reader.ReadInt64();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(_dataSize);
            writer.Write(_documentCount);
        }
        #endregion
    }
}
