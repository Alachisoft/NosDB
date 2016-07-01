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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Alachisoft.NosDB.Common.Storage.Caching.LightCache
{
    public class LightCache<TKey, TValue> 
    {
        private readonly int _cacheSzie;
        private readonly int _evictionPercentage;
        private readonly ConcurrentDictionary<KeyIdentifier<TKey>, TValue> _cache =
            new ConcurrentDictionary<KeyIdentifier<TKey>, TValue>();

        public LightCache(int cacheSize = 1000, int evictionPercentage = 10)
        {
            _cacheSzie = cacheSize;
            _evictionPercentage = evictionPercentage;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                var cacheKey = new KeyIdentifier<TKey>(key);

                if (!_cache.TryGetValue(cacheKey, out value))
                {
                    throw new KeyNotFoundException("Key:" + key + " could not be found in the cache.");
                }
                return value;
            }

            set
            {
                if (Contains(key))
                {
                    var cacheKey = new KeyIdentifier<TKey>(key);
                    _cache[cacheKey] = value;
                }

                TryAdd(key, value);
                PerformEviction();
            }

        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (this)
            {
                var cacheKey = new KeyIdentifier<TKey>(key);

                if (!_cache.TryAdd(cacheKey, value))
                    return false;

                PerformEviction();
                return true;
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (this)
            {
                var cacheKey = new KeyIdentifier<TKey>(key);
                return _cache.TryRemove(cacheKey, out value);
            }
        }
        
        public bool Contains(TKey key)
        {
            lock (this)
            {
                var cacheKey = new KeyIdentifier<TKey>(key);
                return _cache.ContainsKey(cacheKey);
            }
        }

       

        private void PerformEviction()
        {
            lock (this)
            {
                if (_cache.Count > _cacheSzie)
                {
                    List<KeyIdentifier<TKey>> list = _cache.Keys.ToList();

                    list.Sort();

                    int evictCount = (_cache.Count*_evictionPercentage)/100;

                    foreach (var item in list)
                    {
                        if (evictCount == 0)
                            break;

                        TValue value;
                        TryRemove(item.Key, out value);

                        evictCount--;
                    }
                }

            }
        }
    }
}
