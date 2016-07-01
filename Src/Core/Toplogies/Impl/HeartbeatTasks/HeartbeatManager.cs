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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks;
using Alachisoft.NosDB.Core.Toplogies.Impl.ConnectionRestoration;
using Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks
{
    /// <summary>
    /// This class manages the heartbeat tasks.
    /// </summary>
    public class HeartbeatManager
    {
        private NodeToCSHeartbeatTask _nodeToCSReportingTask = null;
        private LocalShardCheckHeartbeatTask _checkHeartbeatTask = null;

        //RTD: Temporary - needs to be moved.
        public ConnectivityStatus CSStatus
        {
            get
            {
                if (_nodeToCSReportingTask != null)
                    return _nodeToCSReportingTask.CSStatus;
                return ConnectivityStatus.NotConnected;
            }
        }

        /// <summary>
        /// All the heartbeat tasks begin here.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="shard"></param>
        public void BeginTasks(NodeContext context, IShard shard, MembershipManager manager, IConnectionRestoration connectionRestoration, ClusterConfigurationManager clusterConfigMgr)
        {
            _nodeToCSReportingTask = new NodeToCSHeartbeatTask(context, manager, connectionRestoration, clusterConfigMgr);
            _nodeToCSReportingTask.Start();

            //Start the shard level heartbeat tasks.
            _checkHeartbeatTask = new LocalShardCheckHeartbeatTask(shard, context, manager, clusterConfigMgr);
            _checkHeartbeatTask.Start();
        }

        //RTD: Temporary - needs to be moved
        public void StopCSTask()
        {
            if (_nodeToCSReportingTask != null)
                _nodeToCSReportingTask.Stop();
            
        }

        public void Dispose()
        {
            if (_checkHeartbeatTask != null)
                _checkHeartbeatTask.Stop();

            if (_nodeToCSReportingTask != null)
                _nodeToCSReportingTask.Stop();
        }

        /// <summary>
        /// Whenever a new heartbeat is received, it is updated in the heartbeat table.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="info"></param>
        public void ReceiveHeartbeat(Address source, HeartbeatInfo info)
        {
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                LoggerManager.Instance.ShardLogger.Debug("HeatbeatManager.ReceiveHeartbeat() ","Heartbeat from node " + source.IpAddress.ToString() + " received at " + DateTime.Now.ToString());

            if (_checkHeartbeatTask != null)
                _checkHeartbeatTask.ReceiveHeartbeat(source, info);
        }

        public void Stop()
        {
            if (_checkHeartbeatTask != null)
                _checkHeartbeatTask.Stop();

            if (_nodeToCSReportingTask != null)
                _nodeToCSReportingTask.Stop();
        }

        internal void OnActivityComplete()
        {

            if (_checkHeartbeatTask != null)
                _checkHeartbeatTask.OnActivityCompleted();
        }

        internal void OnActivityTriggered(Activity activity, Address node)
        {
            if (_checkHeartbeatTask != null)
                _checkHeartbeatTask.OnActivityTriggered(activity, node);
        }

    }
}
