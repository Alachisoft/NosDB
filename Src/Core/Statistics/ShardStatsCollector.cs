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
using System.Configuration;
using System.Diagnostics;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Stats;

namespace Alachisoft.NosDB.Core.Statistics
{
    public class ShardStatsCollector : IStatsCollector
    {
        private string _instanceName;

        private const string PcCategory = "NosDB";

        private IDictionary<StatisticsType, PerformanceCounter> _performanceCounters;

        public string InstanceName
        {
            get { return _instanceName; }
        }

        public ShardStatsCollector()
        {
            _performanceCounters =
                new HashVector<StatisticsType, PerformanceCounter>();
        }

        #region	/                 --- Initialize Counters ---           /

        /// <summary>
        /// Initialize Performance Counters
        /// Performance Counters should be Installed Otherwise this method will throw exception
        /// </summary>
        public void Initialize(string instanceName)
        {
            if (_performanceCounters == null)
                _performanceCounters = new HashVector<StatisticsType, PerformanceCounter>();

            string instanceIdentifier = ConfigurationSettings.AppSettings["InstanceIdentifier"];

            _instanceName = instanceName + instanceIdentifier ?? "";

           /* lock (this)
            {
                _performanceCounters[StatisticsType.PendingReplicatedOperation] = new PerformanceCounter(PcCategory,
                    "Pending Replicated Operations", _instanceName, false);

            }
            * */
        }

        #endregion

        public void IncrementStatsValue(StatisticsType type)
        {
            if (!_performanceCounters.ContainsKey(type)) return;
            if (_performanceCounters[type] == null) return;
            lock (_performanceCounters[type])
            {
                _performanceCounters[type].Increment();
            }
        }

        public void IncrementStatsValue(StatisticsType type, long value)
        {
            if (!_performanceCounters.ContainsKey(type)) return;
            if (_performanceCounters[type] == null) return;
            lock (_performanceCounters[type])
            {
                _performanceCounters[type].IncrementBy(value);
            }
        }


        public void IncrementStatsValue(StatisticsType type, double value)
        {
            throw new NotImplementedException();
        }

        public void DecrementStatsValue(StatisticsType type)
        {
            if (!_performanceCounters.ContainsKey(type)) return;
            if (_performanceCounters[type] == null) return;
            lock (_performanceCounters[type])
            {
                _performanceCounters[type].Decrement();
            }
        }

        public void DecrementStatsValue(StatisticsType type, long value)
        {
            if (!_performanceCounters.ContainsKey(type)) return;
            if (_performanceCounters[type] == null) return;
            lock (_performanceCounters[type])
            {
                _performanceCounters[type].IncrementBy(-value);  //increment or decrement
            }
        }

        public void DecrementStatsValue(StatisticsType type, double value)
        {
            throw new NotImplementedException();
        }

        public void SetStatsValue(StatisticsType type, double value)
        {
            throw new NotImplementedException();
        }

        public void SetStatsValue(StatisticsType type, long value)
        {
            if (!_performanceCounters.ContainsKey(type)) return;
            if (_performanceCounters[type] == null) return;
            lock (_performanceCounters[type])
            {
                _performanceCounters[type].RawValue = value;
            }
        }

        #region	/                 --- IDisposable ---           /

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or 
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                foreach (KeyValuePair<StatisticsType, PerformanceCounter> pair in _performanceCounters)
                {
                    pair.Value.RemoveInstance();
                    pair.Value.Dispose();
                }
                _performanceCounters.Clear();
            }
        }

        #endregion


        public long GetStatsValue(StatisticsType type)
        {
            if (!_performanceCounters.ContainsKey(type)) return -1;
            if (_performanceCounters[type] == null) return -1;
            lock (_performanceCounters[type])
            {
                return _performanceCounters[type].RawValue;
            }
        }
    }
}
