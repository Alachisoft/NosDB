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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Storage.Caching.Evictions;
using Alachisoft.NosDB.Core.Storage.Providers;

namespace Alachisoft.NosDB.Core.Storage.Caching
{
    public class CacheItem : ISize
    {
        private JSONDocument _document = null;
        private BitSet _bitSet = new BitSet();
        private KeyMetadata _keyMetaData = null;
        private EvictionHint _evictionHint;

        public KeyMetadata Metadata
        {
            get { return _keyMetaData; }
            set { _keyMetaData = value; }
        }

        public JSONDocument Document
        {
            get { return _document; }
            set { _document = value; }
        }

        public BitSet Flag
        {
            set { lock (this) { _bitSet = value; } }
            get { return _bitSet; }
        }

        public EvictionHint EvictionHint
        {
            get { return _evictionHint; }
            set { _evictionHint = value; }
        }

        public CacheItem()
        {
            _evictionHint = new TimestampHint();
        }

        public long Size
        {
            get
            {
                //TODO: check keymetada why it is not assigned and if it not to be used anymore then remove it from update operation too.
                return _document.Size + 1;//+ _keyMetaData.Size;
            }
        }
    }
}
