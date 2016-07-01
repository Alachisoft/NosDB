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
using System.Collections;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Stats;


namespace Alachisoft.NosDB.Core.Storage.Caching.Evictions
{
    /// <summary>
    /// Eviction policy based on the timestamp
    /// When needed objects with lowest timestamp are removed first.
    /// </summary>
    internal class LRUEvictionPolicy : IEvictionPolicy
    {
        private EvictionIndex _index;
        private DateTime _initTime;
        private IStatsCollector _statsCollector;

        internal class TimestampComparer : IComparer
        {
            private Hashtable _unsortedList;

            public TimestampComparer(Hashtable unsortedList)
            {
                _unsortedList = unsortedList;
            }

            int IComparer.Compare(object x, object y)
            {
                object first = _unsortedList[x];
                object second = _unsortedList[y];

                return ((DateTime)first).CompareTo((DateTime)second);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        internal LRUEvictionPolicy(StatsIdentity statsIdentity)
        {
            Initialize();
            _statsCollector = StatsManager.Instance.GetStatsCollector(statsIdentity);
        }

        /// <summary>
        /// Initialize Policy
        /// </summary>
        private void Initialize()
        {
            _index = new EvictionIndex();
            _initTime = new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Check if the provided eviction hint is compatible with the policy
        /// and return the compatible eviction hint
        /// </summary>
        /// <param name="eh">eviction hint.</param>
        /// <returns>a hint compatible to the eviction policy.</returns>
        public EvictionHint CompatibleHint(EvictionHint eh)
        {
            if (eh != null && eh is TimestampHint)
            {
                return eh;
            }
            return new TimestampHint();
        }

        bool IEvictionPolicy.Execute(DocumentCache cache,  long evictSize)
        {
            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsDebugEnabled)
                LoggerManager.Instance.StorageLogger.Debug("LocalCache.Evict()", "Cache Size: {0}" + cache.CacheCount().ToString());

            //muds:
            //if user has updated the values in configuration file then new values will be reloaded.

            DateTime startTime = DateTime.Now;
            IList selectedKeys = GetSelectedKeys(cache, evictSize);
            DateTime endTime = DateTime.Now;
            if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsDebugEnabled)
                LoggerManager.Instance.StorageLogger.Debug("LocalCache.Evict()", String.Format("Time Span for {0} Items: " + (endTime - startTime), selectedKeys.Count));

            IEnumerator e = selectedKeys.GetEnumerator();

            while (e.MoveNext())
            {
                long key = (long)e.Current;
                try
                {
                    cache.CacheRemove(key);
                    //removedItems = cache.RemoveSync(keysTobeRemoved.ToArray(), ItemRemoveReason.Underused, false, lruEvictionOperationContext) as ArrayList;
                    //cache remove counter increment
                    //context.PerfStatsColl.IncrementEvictPerSecStatsBy(keysTobeRemoved.Count);
                    if (_statsCollector != null)
                        _statsCollector.IncrementStatsValue(StatisticsType.CacheEvictionPerSec);
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                        LoggerManager.Instance.StorageLogger.Error("LruEvictionPolicy.Execute", "an error occurred while removing items. Error " + ex.ToString());
                }
            }

            //if (keysTobeRemoved.Count > 0)
            //{
            //    try
            //    {
            //        cache.CacheRemove(key);
            //        //removedItems = cache.RemoveSync(keysTobeRemoved.ToArray(), ItemRemoveReason.Underused, false, lruEvictionOperationContext) as ArrayList;
            //        //update perfmon stats
            //        //context.PerfStatsColl.IncrementEvictPerSecStatsBy(keysTobeRemoved.Count);
            //    }
            //    catch (Exception ex)
            //    {
            //        cacheLog.Error("LruEvictionPolicy.Execute", "an error occurred while removing items. Error " + ex.ToString());
            //    }
            //}
            return true;

        }

        void IEvictionPolicy.Notify(object key, EvictionHint oldhint, EvictionHint newHint)
        {
            lock (_index.SyncRoot)
            {
                EvictionHint hint = oldhint == null ? newHint : oldhint;
                if (_index != null && key != null && hint != null)
                {
                    TimeSpan diffTime = ((TimestampHint)hint).TimeStamp.Subtract(_initTime);
                    long indexKey = GetIndexKey(diffTime);

                    if (_index.Contains(indexKey, key))
                    {
                        _index.Remove(indexKey, key);

                        hint = newHint == null ? oldhint : newHint;
                        hint.Update(); //hint is not new so update.

                        diffTime = ((TimestampHint)hint).TimeStamp.Subtract(_initTime);
                        indexKey = GetIndexKey(diffTime);
                        _index.Add(indexKey, key);
                    }
                    else
                    {
                        _index.Add(indexKey, key);
                    }
                }
            }
        }

        void IEvictionPolicy.Remove(object key, EvictionHint hint)
        {
            if (hint == null) return;

            lock (_index.SyncRoot)
            {
                TimeSpan diffTime = ((TimestampHint)hint).TimeStamp.Subtract(_initTime);
                long indexKey = GetIndexKey(diffTime);
                _index.Remove(indexKey, key);
            }
        }

        void IEvictionPolicy.Clear()
        {
            lock (_index.SyncRoot)
            {
                _index.Clear();
            }
        }

        private IList GetSelectedKeys(DocumentCache cache, long evictSize)
        {
            lock (_index.SyncRoot)
            {
                return _index.GetSelectedKeys(cache, evictSize);
            }
        }

        private long GetIndexKey(TimeSpan diffTime)
        {
            //int totalSeconds =  (diffTime.Hours * 3600) +
            //                    (diffTime.Minutes * 60) +
            //                    (diffTime.Seconds);
            return (long)diffTime.TotalSeconds;
        }

        #region ISizable Impelementation
        public long IndexInMemorySize { get { return LRUEvictionIndexSize; } }

        private long LRUEvictionIndexSize
        {
            get
            {
                long temp = 0;

                if (_index != null)
                {
                    temp += _index.IndexInMemorySize;
                    temp += _index.KeysCount * TimestampHint.InMemorySize;
                }

                return temp;
            }
        }
        #endregion
    }
}

