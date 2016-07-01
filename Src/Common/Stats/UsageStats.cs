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
using Alachisoft.NosDB.Common.Interop;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Stats
{
    [Serializable]
    public class UsageStats : ICompactSerializable
    {
        /// <summary> Timestamp for the begining of a sample. </summary>
        private long _lastStart;
        /// <summary> Timestamp for the end of a sample. </summary>
        private long _lastStop;

        private long _frequency;
        /// <summary>
        /// Constructor
        /// </summary>
        public UsageStats()
        {
            Reset();
        }

        /// <summary>
        /// Returns the time interval for the last sample
        /// </summary>
        public long Current
        {
            get { lock (this) { return _lastStop - _lastStart; } }
        }

        public long Frequency
        {
            get { return _frequency; }
        }

        /// <summary>
        /// Resets the statistics collected so far.
        /// </summary>
        public void Reset()
        {
            Win32.QueryPerformanceFrequency(ref _frequency);
            _lastStart = _lastStop = 0;
        }

        /// <summary>
        /// Timestamps the start of a sampling interval.
        /// </summary>
        public void BeginSample()
        {
            Win32.QueryPerformanceCounter(ref _lastStart);
        }


        /// <summary>
        /// Timestamps the end of interval and calculates the sample time
        /// </summary>
        public void EndSample()
        {
            Win32.QueryPerformanceCounter(ref _lastStop);

        }


        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _lastStart = reader.ReadInt64();
            _lastStop = reader.ReadInt64();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_lastStart);
            writer.Write(_lastStop);
        }
    }
    
}

