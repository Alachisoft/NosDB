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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Protobuf.ManagementCommands;
using Alachisoft.NosDB.Common.RPCFramework;
using Alachisoft.NosDB.Common.RPCFramework.DotNetRPC;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.DBEngine;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Core.Util;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;

namespace Alachisoft.NosDB.Core.Monitoring
{
    public class MonitorServer : IMonitorServer, IRequestListener
    {
        //private string _shardName;
        private Common.RPCFramework.RPCService<MonitorServer> _rpcService = null;
        private object _syncLockEventLog = new object();
        private object _syncLockCPUUsage = new object();
        private Dictionary<string, EventLog> _eventLogDictionary = new Dictionary<string, EventLog>();	//Will hold all the event log instances against their sources
        private List<EventViewerEvent> _lstEvent = new List<EventViewerEvent>();
        private IDualChannel _channel;
        DateTime _sessionStartTime;
        string _sessionId;
        private CPUUsage _cpuUsage = null;

        private IList<Server> _runningServerNodes = new List<Server>();

        public IDualChannel Channel { get { return _channel; } set { _channel = value; } }
        private NodeContext _nodeContext = null;
        private ClientSessionManager _clientSessionManager = null;

        public MonitorServer(ClientSessionManager clientSessionManager, NodeContext nodeContext, UserCredentials credentials)
        {
            _nodeContext = nodeContext;
            _clientSessionManager = clientSessionManager;
            
            this._sessionStartTime = DateTime.Now;
            _sessionId = Guid.NewGuid().ToString();
            _rpcService = new RPCService<MonitorServer>(new TargetObject<MonitorServer>(this));
        }


        #region IRequestListener Methods

        public object OnRequest(IRequest request)
        {
            if (request.Message is ManagementCommand)
            {
                ManagementCommand command = request.Message as ManagementCommand;
                if (command == null)
                    return null;
                ManagementResponse response = new ManagementResponse();
                response.MethodName = command.MethodName;
                response.Version = command.CommandVersion;
                response.RequestId = command.RequestId;
                byte[] arguments = CompactBinaryFormatter.ToByteBuffer(command.Parameters, null);
                try
                {
                    response.ResponseMessage = _rpcService.InvokeMethodOnTarget(command.MethodName,
                        command.Overload, GetTargetMethodParameters(arguments));
                }
                catch (System.Exception ex)
                {
                    response.Exception = ex;
                }
                return response;
            }
            else
                return null;
        }

        protected object[] GetTargetMethodParameters(byte[] graph)
        {
            TargetMethodParameter parameters = CompactBinaryFormatter.FromByteBuffer(graph, "ok") as TargetMethodParameter;
            return parameters.ParameterList.ToArray();
        }

        public void ChannelDisconnected(IRequestResponseChannel channel, string reason)
        {
            //if (dbMgtServer != null && channel.PeerAddress != null)
            //{
            //    dbMgtServer.RemoveConfigServerChannel(channel.PeerAddress);
            //}
        }


        #endregion


        //[TargetMethod(ConfigurationCommandUtil.MethodName.InitializeMonitor)]
        //public void Initialize()
        //{
        //}

        /// <summary>
        /// Will register the EntryWritten event so that can return the event log entry from this point onwards
        /// </summary>
        /// <param name="sources"></param>
        /// 
        [TargetMethod(ConfigurationCommandUtil.MethodName.RegisterEventViewerEvents)]
        public void RegisterEventViewerEvents(string[] sources)
        {
            UnRegisterEventViewerEvents();

            lock (_syncLockEventLog)
            {

                if (_eventLogDictionary.Count == 0)
                {
                    EventLog eventLog = new EventLog("Application", Dns.GetHostName());
                    eventLog.EntryWritten += new EntryWrittenEventHandler(eventLog_EntryWritten);
                    eventLog.EnableRaisingEvents = true;

                    foreach (string src in sources)
                    {
                        if (_eventLogDictionary.ContainsKey(src))
                            continue;
                        _eventLogDictionary.Add(src, eventLog);
                    }
                }
            }
        }

        void eventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            lock (_syncLockEventLog)
            {
                if (_eventLogDictionary.ContainsKey(e.Entry.Source))
                {
                    EventViewerEvent eventViewerEntry = new EventViewerEvent(e.Entry);
                    _lstEvent.Add(eventViewerEntry);
                }
            }
        }

        /// <summary>
        /// Unregister the EntryWritten event
        /// </summary>
        /// <param name="sources"></param>
        /// 
        [TargetMethod(ConfigurationCommandUtil.MethodName.UnRegisterEventViewerEvents)]
        public void UnRegisterEventViewerEvents()
        {
            lock (_syncLockEventLog)
            {
                foreach (EventLog eventLog in _eventLogDictionary.Values)
                {
                    eventLog.EntryWritten -= new EntryWrittenEventHandler(eventLog_EntryWritten);
                }
                _eventLogDictionary.Clear();
            }
        }

        /// <summary>
        /// Will return the latest event Dictionary of register sources
        /// </summary>
        /// <returns></returns>
        [TargetMethod(ConfigurationCommandUtil.MethodName.GetLatestEvents)]
        public EventViewerEvent[] GetLatestEvents()
        {
            List<EventViewerEvent> returnEvent = new List<EventViewerEvent>();
            lock (_syncLockEventLog)
            {
                foreach (EventViewerEvent entry in _lstEvent)
                {
                    returnEvent.Add((EventViewerEvent)entry.Clone());
                }
                _lstEvent.Clear();
            }
            return returnEvent != null ? returnEvent.ToArray() : null;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetRunningServers)]
        public ServerInfo[] GetRunningServers()
        {
            IList<Server>  runningServers = ((PartitionOfReplica)_nodeContext.TopologyImpl).GetActiveChannelList();
            if (runningServers != null)
            {
                _runningServerNodes = runningServers;
                return GetServerInfoCollection(runningServers);
            }
            return null;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetUpdatedRunningServers)]
        public ServerInfo[] GetUpdatedRunningServers()
        {
            IList<Server> runningServers = ((PartitionOfReplica)_nodeContext.TopologyImpl).GetActiveChannelList();
            if (runningServers != null)
            {
                if (_runningServerNodes.Count < 1 && runningServers.Count > 0)
                {
                    _runningServerNodes = runningServers;
                    return GetServerInfoCollection(runningServers);
                }

                if (_runningServerNodes.Count == runningServers.Count)
                {
                    foreach (Server server in runningServers)
                    {
                        if (!_runningServerNodes.Contains(server))//DEBUG
                        {
                            _runningServerNodes = runningServers;
                            return GetServerInfoCollection(runningServers);
                        }
                    }
                }
                else
                {
                    _runningServerNodes = runningServers;
                    return GetServerInfoCollection(runningServers);

                }
            }
            return null;
        }

        private ServerInfo[] GetServerInfoCollection(IList<Server> runningServers)
        {
            ServerInfo[] servers = new ServerInfo[runningServers.Count];
            int i = 0;
            foreach (Server server in runningServers)
            {
                servers[i] = new ServerInfo { Address = server.Address, Status = server.Status };
                i++;
            }
            return servers;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetPercentageCPUUsage)]
        public int GetPercentageCPUUsage()
        {
            lock (_syncLockCPUUsage)
            {
                if (_cpuUsage == null)
                {
                    _cpuUsage = new CPUUsage();
                }

                try
                {
                    return _cpuUsage.GetUsage();
                }
                catch (System.Exception)
                {
                    return -3;
                }
            }
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetShardNodeNIC)]
        public string GetShardNodeNIC()
        {
           Address localAddres =  ((PartitionOfReplica)_nodeContext.TopologyImpl).Context.LocalAddress;
           if (localAddres != null)
               return GetNICForIP(localAddres.ip.ToString());
           return null;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetNICForIP)]
        public string GetNICForIP(string ip)
        {
            if (ip == null) return null;
            string nic = null;
            try
            {
                // Detecting Network Interface Cards with enabled IPs through WMI:
                //
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("Select * from Win32_NetworkAdapterConfiguration WHERE IPEnabled=True");

                foreach (ManagementObject mo in searcher.Get())
                {
                    string[] ipAddresses = mo.GetPropertyValue("IPAddress") as string[];

                    foreach (string ipAddress in ipAddresses)
                    {
                        if ((string.Compare(ipAddress, ip, true) == 0))
                        {
                            nic = (string)mo.GetPropertyValue("Description");
                            break;
                        }
                    }
                }
            }
            catch (System.Exception) { }

            return nic;
        }

        [TargetMethod(ConfigurationCommandUtil.MethodName.GetClientProcessStats)]
        public ClientProcessStats[] GetClientProcessStats(string database)
        {
            var stats =  _clientSessionManager.GetClientProcessStats(database);
            if(stats != null && stats.Count > 0)
                return stats.ToArray();
            return null;
        }
    }
}
