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
using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.Services
{
    public class CappedInfo : ICompactSerializable, ICloneable
    {
        public CappedInfo() 
        {
        }
        public CappedInfo(long maxSize)
        {
            if(maxSize < 1) throw new Exception("Max Size of capped collection must be greater than zero. Please define max size");
            Size = maxSize;
        }

        public CappedInfo(long maxSize, long maxDocuments) : this(maxSize)
        {
            MaxDocuments = maxDocuments;
        }
        [JsonProperty(PropertyName = "Size")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "MaxDocuments")]
        public long MaxDocuments { get; set; }

        [JsonProperty(PropertyName = "Shard")]
        public string Shard { get; set; }


        #region ICompactSerializable Methods
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Size = reader.ReadInt64();
            MaxDocuments = reader.ReadInt64();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(Size);
            writer.Write(MaxDocuments);
        }
        #endregion

        #region IClonable Methods
        public object Clone()
        {
            CappedInfo cappedInfoClone = MaxDocuments > 0 ? new CappedInfo(Size, MaxDocuments) : new CappedInfo(Size);
            return cappedInfoClone;
        }
        #endregion
    }
}
