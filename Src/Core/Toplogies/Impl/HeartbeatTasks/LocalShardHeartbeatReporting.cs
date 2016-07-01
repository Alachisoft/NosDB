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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Logger;

using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Queries.Filters.Aggregations;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks
{
    /// <summary>
    /// this class helps maintain the data relating the nodes reporting with a heartbeat.
    /// </summary>
    public class LocalShardHeartbeatReporting : ICloneable
    {
        private IDictionary<Address, HeartbeatInfo> _heartbeatReporter;
        private KeyValuePair<Address, OperationId> _minReplicatedOperation;
        private readonly object _mutex = new object();
        
        public LocalShardHeartbeatReporting(Address localAddress)
        {
            _heartbeatReporter = new HashVector<Address, HeartbeatInfo>();
            _minReplicatedOperation = new KeyValuePair<Address, OperationId>(localAddress, new OperationId());
        }

        public OperationId MinReplicatedOperation
        {
            get { return _minReplicatedOperation.Value; }
        }

        public bool AddToReport(Address source, HeartbeatInfo hbInfo)
        {
            bool isAnOldNode = false;

            if (source != null)
            {
                // update the timestamp whenever a heartbeat is received.
                hbInfo.LastHeartbeatTimestamp = DateTime.Now;
                // Reset the missing heartbeats counter as soon as a heartbeat is received.
                hbInfo.MissingHeartbeatsCounter = 0;
                lock (_heartbeatReporter)
                {
                    if (_heartbeatReporter.ContainsKey(source))
                    {
                        isAnOldNode = true;
                    }
                    else
                    {
                        if (LoggerManager.Instance.ShardLogger != null &&
                            LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                            LoggerManager.Instance.ShardLogger.Info("LocalShardHeartbeatReporting.AddToReport() ",
                                "Node joining activity triggered: " + source.IpAddress.ToString() +
                                " added to the report table.");
                    }
                    _heartbeatReporter[source] = hbInfo;
                }
                KeyValuePair<Address, OperationId> lastReplicatedOperation = new KeyValuePair<Address, OperationId>(source, hbInfo.LastOplogOperationId);
                lock (_mutex)
                {
                    if (_minReplicatedOperation.Key.Equals(source))
                    {
                        _minReplicatedOperation = lastReplicatedOperation;
                    }
                    else if(_minReplicatedOperation.Value > hbInfo.LastOplogOperationId)
                    {
                        _minReplicatedOperation = lastReplicatedOperation;
                    }
                }
            }
            return isAnOldNode;
        }

        public HeartbeatInfo GetHeartbeatInfo(Address source)
        {
            if (source != null)
            {
                if (_heartbeatReporter.Keys.Contains(source))
                    return _heartbeatReporter[source];
            }
            return null;
        }

        public void RemoveFromReport(Address source)
        {
            lock (_heartbeatReporter)
            {
                if (source != null && _heartbeatReporter.ContainsKey(source))
                {
                    _heartbeatReporter.Remove(source);
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsInfoEnabled)
                        LoggerManager.Instance.ShardLogger.Info("LocalShardHeartbeatReporting.RemoveFromReport() ", "Node leaving activity triggered: " + source.IpAddress.ToString() + " removed from the report table.");

                }
            }

            if (_minReplicatedOperation.Key.Equals(source))
            {
                CalculateAndSetMinOperationId();
            }
        }

        public IDictionary<Address, HeartbeatInfo> GetReportTable
        {
            get
            {
                return _heartbeatReporter;
            }

        }

        internal void UpdateMissingHeartbeatsCount(Address address)
        {
            lock (_heartbeatReporter)
            {
                if (_heartbeatReporter != null && _heartbeatReporter.ContainsKey(address))
                {
                    _heartbeatReporter[address].MissingHeartbeatsCounter = _heartbeatReporter[address].MissingHeartbeatsCounter + 1;
                }
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                    LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartReporting.UpdateMissingHeartbeatsCount() ", "Number of heartbeats missed from the node " + address.IpAddress.ToString() + " are " + _heartbeatReporter[address].MissingHeartbeatsCounter.ToString());
            }

        }

        internal ServerNode GetCurrentPrimary()
        {
            if (_heartbeatReporter != null && PrimaryExists())
            {
                IList<Address> activeNodes = _heartbeatReporter.Keys.ToList();
                if (activeNodes != null)
                {
                    foreach (var node in activeNodes)
                    {
                        HeartbeatInfo info = GetHeartbeatInfo(node);
                        lock (info)
                        {
                            if (info != null && info.CurrentMembership != null && info.CurrentMembership.Primary != null)
                            {
                                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                    LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartReporting.GetCurrentPrimary() ", "Primary for this shard is: " + info.CurrentMembership.Primary.Name.ToString());
                                return info.CurrentMembership.Primary;
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal ElectionId GetCurrentElectionId()
        {
            
            if(PrimaryExists())
            {
                ServerNode primary = GetCurrentPrimary();
                IList<Address> activeNodes = _heartbeatReporter.Keys.ToList();
                if(activeNodes != null)
                {
                    foreach(var node in activeNodes)
                    {
                        if(node.IpAddress.ToString() == primary.Name)
                        {
                            HeartbeatInfo info = GetHeartbeatInfo(node);
                            lock (info)
                            {
                                if (info != null && info.CurrentMembership != null && info.CurrentMembership.ElectionId != null)
                                {
                                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                        LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartReporting.GetCurrentEditionId() ", "Election ID of the ongoing election term is " + info.CurrentMembership.ElectionId.Id);
                                    return info.CurrentMembership.ElectionId;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal bool PrimaryExists()
        {
            IList<Address> activeNodes = _heartbeatReporter.Keys.ToList();
            if (activeNodes != null)
            {
                foreach (var node in activeNodes)
                {
                    HeartbeatInfo info = GetHeartbeatInfo(node);
                    lock (info)
                    {
                        if (info != null && info.CurrentMembership != null && info.CurrentMembership.Primary != null)
                        {
                            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                                LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartReporting.PrimaryExists() ","Primary for this shard is: " + node.IpAddress.ToString());
                            return true; 
                        }
                            
                    }
                }
            }
            if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                LoggerManager.Instance.ShardLogger.Debug("LocalShardCheckHeartReporting.PrimaryExists() ","Primary for this shard does not exist. ");
            return false;
        }

        public List<ServerNode> GetActiveServerNodes(ServerNodes serverNodes)
        {
            IList<Address> list = _heartbeatReporter.Keys.ToList();
            List<ServerNode> activeNodes = new List<ServerNode>();
            foreach (var node in list)
            {
                activeNodes.Add(serverNodes.GetServerNode(node.IpAddress.ToString()));
            }
            return activeNodes;
        }

        //RTD: revisit this logic
        public object Clone()
        {
            LocalShardHeartbeatReporting clone = new LocalShardHeartbeatReporting(_minReplicatedOperation.Key);
            clone._heartbeatReporter = new Dictionary<Address, HeartbeatInfo>();
            IList<Address> nodes = this._heartbeatReporter.Keys.ToList();
            foreach (var node in nodes)
            {
                HeartbeatInfo info = _heartbeatReporter[node];
                lock (info)
                {
                    clone._heartbeatReporter.Add(node, (HeartbeatInfo)info.Clone());
                }
            }
            return clone;
        }

        private void CalculateAndSetMinOperationId()
        {
            KeyValuePair<Address, OperationId> minValue;
            lock (_heartbeatReporter)
            {
                if (!_heartbeatReporter.Any()) return;
                var firstHeartbeat = _heartbeatReporter.First();
                minValue = new KeyValuePair<Address, OperationId>(firstHeartbeat.Key, firstHeartbeat.Value.LastOplogOperationId);

                foreach (var info in _heartbeatReporter)
                {
                    if (info.Value.LastOplogOperationId < minValue.Value)
                    {
                        minValue = new KeyValuePair<Address, OperationId>(info.Key, info.Value.LastOplogOperationId);
                    }
                }
            }
            _minReplicatedOperation = minValue;
        }
    }
}
