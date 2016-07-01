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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration
{
    public class BrokenConnectionEntry : ICloneable
    {
        public BrokenConnectionInfo ConnectionInfo { get; set; }
        public RetryInfo CRetryInfo { get; set; }
        public IConnectionRestorationListener ConnectionRestorationListener { get; set; }
        public IDualChannel Channel { get; set; }


        public object Clone()
        {
            BrokenConnectionEntry clone = new BrokenConnectionEntry();
            clone.ConnectionInfo = ConnectionInfo;
            clone.CRetryInfo = CRetryInfo;
            clone.ConnectionRestorationListener = ConnectionRestorationListener;
            clone.Channel = Channel;
            return clone;
        }

        public bool Equals(BrokenConnectionEntry entry)
        {
            return this.ConnectionInfo != null && entry != null && this.ConnectionInfo.Equals(entry.ConnectionInfo);
        }
        public class RetryInfo
        {
            public long Retries { get; set; }
            public double RetryInterval { get; set; }
            public DateTime LastRetryTimestamp { get; set; }

            #region ICloneable Members
            public object Clone()
            {
                RetryInfo clone = new RetryInfo();

                clone.Retries = Retries;
                clone.RetryInterval = RetryInterval;
                clone.LastRetryTimestamp = LastRetryTimestamp;
                return clone;
            }
            #endregion

        }

        public override string ToString()
        {
            string tostr = "broken-connection -> ";

            if(this.ConnectionRestorationListener != null)
            {
                tostr += "Listener :" + this.ConnectionRestorationListener.Name;
            }

            if (this.ConnectionInfo != null)
            {
                tostr += " " + this.ConnectionInfo.BrokenAddress + " ; session_type : " + this.ConnectionInfo.SessionType;
            }
            else
                tostr += " NULL?";

            if(this.CRetryInfo != null)
            {
                tostr += " retries_count" + this.CRetryInfo.Retries + " interval: " + this.CRetryInfo.RetryInterval + " last_retry :" + this.CRetryInfo.LastRetryTimestamp;
            }

            return tostr;
        }
    }
}
