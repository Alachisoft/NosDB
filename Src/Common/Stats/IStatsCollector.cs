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

namespace Alachisoft.NosDB.Common.Stats
{
    public interface IStatsCollector:IDisposable
    {
        string InstanceName { get; }

        void Initialize(string instanceName);

        long GetStatsValue(StatisticsType type);

        void IncrementStatsValue(StatisticsType type);

        void IncrementStatsValue(StatisticsType type,long value);

        void IncrementStatsValue(StatisticsType type, double value);

        void DecrementStatsValue(StatisticsType type);

        void DecrementStatsValue(StatisticsType type, long value);

        void DecrementStatsValue(StatisticsType type, double value);

        void SetStatsValue(StatisticsType type, double value);

        void SetStatsValue(StatisticsType type, long value);

    }
}
