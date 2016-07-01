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

namespace Alachisoft.NosDB.Common.Stats
{
    public class StatsManager
    {
        private static StatsManager _statsManagerInstance = new StatsManager();

        public static StatsManager Instance { get { return _statsManagerInstance; } }

        private Dictionary<StatsIdentity, IStatsCollector> _statsCollector = new Dictionary<StatsIdentity, IStatsCollector>();

        public void AddStatsCollector(StatsIdentity instance,IStatsCollector statsCollector)
        {
            lock (_statsCollector)
            {
                if (_statsCollector.ContainsKey(instance))
                    throw new Exception("Stats Collector with the same Instance already exists");
                _statsCollector.Add(instance, statsCollector);
            }
        }

        public IStatsCollector GetStatsCollector(StatsIdentity instance)
        {
            return _statsCollector.ContainsKey(instance) ? _statsCollector[instance] : null;
        }

        public void RemoveStatsCollector(StatsIdentity instance)
        {
            lock (_statsCollector)
            {
                if (_statsCollector.ContainsKey(instance))
                    _statsCollector.Remove(instance);
            }
        }
    }
}
