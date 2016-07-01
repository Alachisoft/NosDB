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
using System.Threading;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Stats;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl;

namespace Alachisoft.NosDB.Core.Toplogies
{

    public class NodeContext
    {
        internal Latch _statusLatch = new Latch();
        private long _electionBasedSequenceId;
        private ElectionResult _electionResult;
        private readonly object _electionLock = new object();


        public String ClusterName { get; set; }

        public IDatabaseTopology TopologyImpl { get; set; }
        public Address LocalAddress{get;set;}
        public String LocalShardName { get; set; }
        public IShardServer ShardServer { get; set; }
        public String BasePath { get; set; }
        public String DataPath { get; set; }
        public String DeploymentPath { get; set; }

        public IConfigurationSession ConfigurationSession { get; set; }

        public IDatabasesManager DatabasesManager { get; set; }

        public IStatsCollector ShardStatsCollector { get; set; }

        public Security.Interfaces.ISecurityManager SecurityManager { set; get; }
        /// <summary>
        /// Status Latch for node level status, wether it is running,initializing or stopped.
        /// </summary>
        public Latch StatusLatch 
        {
            get
            {
                return _statusLatch;
            }
        }

        public ElectionResult ElectionResult
        {
            get
            {
                lock (_electionLock)
                {
                    return _electionResult;
                }
            }
            set
            {
                lock (_electionLock)
                {
                    _electionResult = value;
                    _electionBasedSequenceId = -1;
                }
            }
        }

        public long ElectionBasedSequenceId
        {
            get { return Interlocked.Increment(ref _electionBasedSequenceId); }
        }


        
    }
}
