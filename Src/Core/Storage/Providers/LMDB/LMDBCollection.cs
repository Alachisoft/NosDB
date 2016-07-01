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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Stats;
using LightningDB;

namespace Alachisoft.NosDB.Core.Storage.Providers.LMDB
{
    public class LMDBCollection : ICompactSerializable, ICloneable
    {
        private LightningDatabase _collection;
        private CollectionStatistics _stats;
        //Temporary size calculated while transaction is in progress and is added on commit and discarded afterwords. 
        private CollectionStatistics _tempStats;

        public LightningDatabase Collection
        {
            get { return _collection; }
            set { _collection = value; }
        }

        public CollectionStatistics Stats
        {
            get { return _stats; }
        }

        public CollectionStatistics TemporaryStats
        {
            get { return _tempStats; }
        }

        public LMDBCollection(LightningDatabase collection)
        {
            _collection = collection;
            _stats = new CollectionStatistics();
            _tempStats = new CollectionStatistics();
        }

        public void IncrementTemporaryStats(long size)
        {
            _tempStats.DataSize += size;
            _tempStats.DocumentCount += 1;
        }

        public void DecrementTemporaryStats(long size)
        {
            _tempStats.DataSize -= size;
            _tempStats.DocumentCount -= 1;
        }

        public void UpdateTemporaryStats()
        {
            _stats.DataSize += _tempStats.DataSize;
            _stats.DocumentCount += _tempStats.DocumentCount;
            ResetTemporaryStats();
        }

        public void ResetTemporaryStats()
        {
            _tempStats.DataSize = 0;
            _tempStats.DocumentCount = 0;
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _stats = reader.ReadObject() as CollectionStatistics;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_stats);
        }

        public object Clone()
        {
            LMDBCollection collection = new LMDBCollection(null);
            collection.Stats.DataSize = _stats.DataSize;
            collection.Stats.DocumentCount = _stats.DocumentCount;
            return collection;
        }
    }
}
