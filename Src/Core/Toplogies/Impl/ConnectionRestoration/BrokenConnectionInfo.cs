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
using Alachisoft.NosDB.Common.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration
{
    public class BrokenConnectionInfo : ICloneable
    {
        public Address BrokenAddress { get; set; }
        public SessionTypes SessionType { get; set; }

        public object Clone()
        {
            BrokenConnectionInfo clone = new BrokenConnectionInfo();
            clone.BrokenAddress = BrokenAddress;
            clone.SessionType = SessionType;
            return clone;
        }

        public bool Equals(BrokenConnectionInfo obj)
        {
            return this.BrokenAddress == obj.BrokenAddress && this.SessionType == obj.SessionType;
        }
    }
}
