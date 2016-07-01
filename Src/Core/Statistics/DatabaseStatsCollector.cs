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
    public class DatabaseStatsCollector : IStatsCollector
    {
        private string _instanceName;

        private const string PcCategory = "NosDB";

        private IDictionary<StatisticsType, PerformanceCounter> _performanceCounters;

        public string InstanceName
        {
            get { return _instanceName; }
        }

        public DatabaseStatsCollector()
        {
            _performanceCounters = new HashVector<StatisticsType, PerformanceCounter>();
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
            
            lock (this)
            {
                _performanceCounters[StatisticsType.InsertsPerSec] = new PerformanceCounter(PcCategory, "Inserts/sec", _instanceName, false);
                _performanceCounters[StatisticsType.FetchesPerSec] = new PerformanceCounter(PcCategory, "Fetches/sec", _instanceName, false);
                _performanceCounters[StatisticsType.UpdatesPerSec] = new PerformanceCounter(PcCategory, "Updates/sec", _instanceName, false);
                _performanceCounters[StatisticsType.DeletesPerSec] = new PerformanceCounter(PcCategory, "Deletes/sec", _instanceName, false);
                _performanceCounters[StatisticsType.CacheHitsPerSec] = new PerformanceCounter(PcCategory, "Cache Hits /sec", _instanceName, false);
                _performanceCounters[StatisticsType.CacheMissesPerSec] = new PerformanceCounter(PcCategory, "Cache Misses/sec", _instanceName, false);
                _performanceCounters[StatisticsType.RequestsPerSec] = new PerformanceCounter(PcCategory, "Requests/sec", _instanceName, false);
                _performanceCounters[StatisticsType.AvgFetchTime] = new PerformanceCounter(PcCategory, "Average Fetch Time (µs)", _instanceName, false);
                _performanceCounters[StatisticsType.AvgFetchTimeBase] = new PerformanceCounter(PcCategory, "Average Fetch Time base", _instanceName, false);
                _performanceCounters[StatisticsType.AvgInsertTime] = new PerformanceCounter(PcCategory, "Average Insert Time (µs)", _instanceName, false);
                _performanceCounters[StatisticsType.AvgInsertTimeBase] = new PerformanceCounter(PcCategory, "Average Insert Time base", _instanceName, false);
                _performanceCounters[StatisticsType.AvgUpdateTime] = new PerformanceCounter(PcCategory, "Average Update Time (µs)", _instanceName, false);
                _performanceCounters[StatisticsType.AvgUpdateTimeBase] = new PerformanceCounter(PcCategory, "Average Update Time base", _instanceName, false);
                _performanceCounters[StatisticsType.AvgDeleteTime] = new PerformanceCounter(PcCategory, "Average Delete Time (µs)", _instanceName, false);
                _performanceCounters[StatisticsType.AvgDeleteTimeBase] = new PerformanceCounter(PcCategory, "Average Delete Time base", _instanceName, false);
                _performanceCounters[StatisticsType.AvgQueryExecutionTime] = new PerformanceCounter(PcCategory, "Average Query Execution Time (µs)", _instanceName, false);
                _performanceCounters[StatisticsType.AvgQueryExecutionTimeBase] = new PerformanceCounter(PcCategory, "Average Query Execution Time base", _instanceName, false);
               
                _performanceCounters[StatisticsType.PendingPersistentDocuments] = new PerformanceCounter(PcCategory, "Pending Persistent Documents", _instanceName, false);
                _performanceCounters[StatisticsType.DocumentsPersistedPerSec] = new PerformanceCounter(PcCategory, "Documents Persisted/sec", _instanceName, false);
                _performanceCounters[StatisticsType.DocumentCount] = new PerformanceCounter(PcCategory, "Documents Count", _instanceName, false);
                _performanceCounters[StatisticsType.AvgDocumentSize] = new PerformanceCounter(PcCategory, "Average Document Size", _instanceName, false);
                _performanceCounters[StatisticsType.DatabaseSize] = new PerformanceCounter(PcCategory, "Database Size", _instanceName, false);
                _performanceCounters[StatisticsType.CacheCount] = new PerformanceCounter(PcCategory, "Cache Count", _instanceName, false);
                _performanceCounters[StatisticsType.CacheSize] = new PerformanceCounter(PcCategory, "Cache Size", _instanceName, false);
                _performanceCounters[StatisticsType.CacheEvictionPerSec] = new PerformanceCounter(PcCategory, "Cache Evicitions/sec", _instanceName, false);

                //StateTxfer Counters
                _performanceCounters[StatisticsType.DataBalancePerSec] = new PerformanceCounter(PcCategory, "Data Balance/sec", _instanceName, false);
                _performanceCounters[StatisticsType.StateTransferPerSec] = new PerformanceCounter(PcCategory, "State Transfer/sec", _instanceName, false);
               


            }
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
            switch(type)
            {
                case StatisticsType.AvgDeleteTime:
                    lock (_performanceCounters[type])
                    {
                        _performanceCounters[type].IncrementBy(value * 1000000);
                        _performanceCounters[StatisticsType.AvgDeleteTimeBase].Increment();
                    }
                    break;
                case StatisticsType.AvgFetchTime:
                    lock (_performanceCounters[type])
                    {
                        _performanceCounters[type].IncrementBy(value * 1000000);
                        _performanceCounters[StatisticsType.AvgFetchTimeBase].Increment();
                    }
                    break;
                case StatisticsType.AvgInsertTime:
                    lock (_performanceCounters[type])
                    {
                        _performanceCounters[type].IncrementBy(value * 1000000);
                        _performanceCounters[StatisticsType.AvgInsertTimeBase].Increment();
                    }
                    break;
                case StatisticsType.AvgQueryExecutionTime:
                    lock (_performanceCounters[type])
                    {
                        _performanceCounters[type].IncrementBy(value * 1000000);
                        _performanceCounters[StatisticsType.AvgQueryExecutionTimeBase].Increment();
                    }
                    break;
                case StatisticsType.AvgUpdateTime:
                    lock (_performanceCounters[type])
                    {
                        _performanceCounters[type].IncrementBy(value * 1000000);
                        _performanceCounters[StatisticsType.AvgUpdateTimeBase].Increment();
                    }
                    break;
                default:
                    lock (_performanceCounters[type])
                    {
                        _performanceCounters[type].IncrementBy(value);
                    }
                    break;
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
            //throw new NotImplementedException();
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
            lock(this)
            {
                foreach(KeyValuePair<StatisticsType,PerformanceCounter> pair in _performanceCounters)
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
