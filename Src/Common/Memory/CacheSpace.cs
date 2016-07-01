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

using System.Collections.Generic;
using Alachisoft.NosDB.Common.Stats;

namespace Alachisoft.NosDB.Common.Memory
{
    public class CacheSpace
    {
        private IList<ICacheSpaceConsumer> _consumers = new List<ICacheSpaceConsumer>();
        private long _capacity;
        private long _currentSize;
        private ICacheSpacePolicy _spacePolicy;
        private IStatsCollector _statsCollector;

        public long Capacity { get { return _capacity; } }

        public long CurrentSize { get { return _currentSize; } }

        public long AvaialbeSpace { get { return _capacity - _currentSize; } }

        public bool IsFull { get { return AvaialbeSpace < 0; } }

        public ICacheSpacePolicy Policy { get { return _spacePolicy; } }

        public CacheSpace(long capacity, ICacheSpacePolicy spacePolicy, StatsIdentity statsIdentity)
        {
            _capacity = capacity;
            _spacePolicy = spacePolicy;
            _statsCollector = StatsManager.Instance.GetStatsCollector(statsIdentity);
            if (_statsCollector != null)
                _statsCollector.SetStatsValue(StatisticsType.CacheSize, 0);
        }

        public void AddConsumer(ICacheSpaceConsumer consumer)
        {
            lock (this)
            {
                if (!_consumers.Contains(consumer))
                {
                    _consumers.Add(consumer);
                }
            }
        }

        public void RemoveConsumer(ICacheSpaceConsumer consumer)
        {
            lock (this)
            {
                if (_consumers.Contains(consumer))
                {
                    _consumers.Remove(consumer);
                }
            }
        }

        public void Consume(ICacheSpaceConsumer consumer, long neededSpace)
        {
            lock (this)
            {
                if (IsFull)
                {
                    if (!_spacePolicy.EvictData(this, consumer, neededSpace))
                        return;
                }
                if (_spacePolicy.CanConsumeSpace(this, consumer, neededSpace))
                {
                    _currentSize += neededSpace;
                    if (_statsCollector != null)
                        _statsCollector.SetStatsValue(StatisticsType.CacheSize, _currentSize);
                }
            }
        }

        public void Release(ICacheSpaceConsumer consumer, long restoreSize)
        {
            lock (this)
            {
                _currentSize -= restoreSize;
            }
            if (_statsCollector != null)
                _statsCollector.SetStatsValue(StatisticsType.CacheSize, _currentSize);
        }
    }
}
