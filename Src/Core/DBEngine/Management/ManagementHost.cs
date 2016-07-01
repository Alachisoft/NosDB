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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.RPC;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Core.DBEngine;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using Alachisoft.NosDB.Core.DBEngine.Management;
using System;
using System.Configuration;
using System.Net;
using Alachisoft.NosDB.Serialization;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common;
using System.Diagnostics;
using DeploymentConfiguration = Alachisoft.NosDB.Common.Configuration.DeploymentConfiguration;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;

namespace Alachisoft.NosDB.Core
{
    public class ManagementHost
    {

        private ManagementServer _managementServer;
        private ManagementSessionListener _managementSessionLisioner;
        private bool _initialized = false;

        public ManagementServer ManagementServer
        {
            get
            {
                return _managementServer;
            }
        }

        static ManagementHost()
        {
            RegisterCompactTypes();
        }


        public static void RegisterCompactTypes()
        {
            #region [RegisterQuery Assemblies]
            //    _   _  ___ _____ _____ 
            //   | \ | |/ _ \_   _| ____|
            //   |  \| | | | || | |  _|  
            //   | |\  | |_| || | | |___ 
            //   |_| \_|\___/ |_| |_____|
            //                           
            //Any type that is meant to be shared between processess must be registed from 1 and above 
            //Any type that is to be used internally must be registered from 999 and below


            //Types that can/might be shared

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Communication.ChannelRequest), 1);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Communication.ChannelResponse), 2);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.UserCredentials), 3);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.HashMapBucket), 4);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.HashMapBucket[]), 5);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.RPCFramework.TargetMethodParameter), 6);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.ManagementCommand), 7);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.ManagementResponse), 8);
            //CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Protobuf.ManagementCommands.CommandBase), 9);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CachingConfiguration), 10);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfigurations), 11);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.ClusterConfiguration), 12);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DeploymentConfiguration), 13);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.ShardConfiguration), 14);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.ShardConfiguration[]), 15);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.ServerNode), 16);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.ServerNode[]), 17);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.ServerNodes), 18);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration), 19);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.LMDBConfiguration), 21);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.StorageConfiguration), 22);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.StorageProviderConfiguration), 23);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfigurations), 24);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DatabaseConfiguration[]), 25);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration[]), 26);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration), 27);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.DistributionStrategyConfiguration), 28);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexAttribute), 29);
            CompactFormatterServices.RegisterCompactType(typeof(Membership), 30);
            CompactFormatterServices.RegisterCompactType(typeof(ElectionId), 31);

            CompactFormatterServices.RegisterCompactType(typeof(ClusterInfo), 32);
            CompactFormatterServices.RegisterCompactType(typeof(DatabaseInfo), 33);
            CompactFormatterServices.RegisterCompactType(typeof(CollectionInfo), 34);
            CompactFormatterServices.RegisterCompactType(typeof(ShardInfo), 35);
            CompactFormatterServices.RegisterCompactType(typeof(ServerInfo), 36);
            CompactFormatterServices.RegisterCompactType(typeof(ServerInfo[]), 37);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Stats.CollectionStatistics), 38);
            CompactFormatterServices.RegisterCompactType(typeof(ShardInfo[]), 39);
            CompactFormatterServices.RegisterCompactType(typeof(DatabaseInfo[]), 40);
            CompactFormatterServices.RegisterCompactType(typeof(CollectionInfo[]), 41);

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexAttribute[]), 43);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration), 44);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.IndexConfiguration[]), 45);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.Indices), 46);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.ElectionResult), 47);

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSONDocument), 51);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.DocumentKey), 52);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Enum.SortOrder), 53);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement.ElectionVote), 54);
            CompactFormatterServices.RegisterCompactType(typeof(Message), 55);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement.Election), 56);

            CompactFormatterServices.RegisterCompactType(typeof(ConfigChangeEventArgs), 57);
            CompactFormatterServices.RegisterCompactType(typeof(PartitionKey), 58);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Toplogies.Impl.HeartbeatTasks.HeartbeatInfo), 59);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Stats.ShardInfo), 70);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Stats.ShardStatistics), 71);


            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Replication.OperationId), 80);

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Toplogies.Impl.MembershipManagement.MembershipChangeArgs), 83);
            
            //CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.GetMinorOperationsOperation), 87);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Indexing.BoundingBox), 88);

            CompactFormatterServices.RegisterCompactType(typeof(DatabaseMessage), 91);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.DatabaseOperationType), 92);
            CompactFormatterServices.RegisterCompactType(typeof(OpCode), 93);

            #region State Transfer Classes
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer.StateTransferOperation), 94);
            CompactFormatterServices.RegisterCompactType(typeof(StateTransferIdentity), 95);
            CompactFormatterServices.RegisterCompactType(typeof(OperationParam), 96);
            CompactFormatterServices.RegisterCompactType(typeof(NodeIdentity), 97);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer.StateTxfrInfo), 98);
            #endregion


            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.Parameter), 99);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.Query), 100);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.WriteQueryOperation), 101);
           


            CompactFormatterServices.RegisterCompactType(typeof(CappedInfo), 106);



            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.EvictionConfiguration), 114);
            CompactFormatterServices.RegisterCompactType(typeof(NonShardedDistributionStrategy), 115);
            CompactFormatterServices.RegisterCompactType(typeof(NonShardedDistribution), 116);
            CompactFormatterServices.RegisterCompactType(typeof(NonShardedDistributionRouter), 117);
            CompactFormatterServices.RegisterCompactType(typeof(ConfigServerConfiguration), 119);
            CompactFormatterServices.RegisterCompactType(typeof(ClusterInfo[]), 121);


            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.AuthToken), 122);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Client.SSPIClientAuthenticationCredential), 123);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Server.SSPIServerAuthenticationCredential), 124);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.PartitionKeyConfiguration), 125);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.PartitionKeyConfigurationAttribute), 126);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Configuration.DOM.PartitionKeyConfigurationAttribute[]), 127);
            CompactFormatterServices.RegisterCompactType(typeof(PartitionKeyAttribute), 128);
            CompactFormatterServices.RegisterCompactType(typeof(PartitionKeyAttribute[]), 129);
            


            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.Permission), 131);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.ResourceId), 132);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.Role), 133);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.User), 134);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.ResourceItem), 135);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.RoleInstance), 136);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.RouterSessionId), 137);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Security.Impl.ClientSessionId), 138);
            //143

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Server.Engine.Impl.DataChunk), 143);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.ConfigurationStore.DatabaseCluster), 144);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.ConfigurationStore.Transaction), 145);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Configuration.Services.ConfigurationStore.Transaction.Operation), 146);
            

        
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Monitoring.ClientProcessStats), 150);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Monitoring.ClientProcessStats[]), 151);

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSONDocument.JsonObject), 152);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.KeyValuePair), 153);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.KeyValuePair[]), 154);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.DataStructures.KeyValueStore), 155);

            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSONDocument[]), 156);
         


            //Types to be used internally
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Providers.CollectionMetadata), 999);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Providers.KeyMetadata), 998);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Providers.FileMetadata<long, byte[]>), 997);
            CompactFormatterServices.RegisterCompactType(typeof(Common.JSON.Indexing.SingleAttributeValue), 994);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSON.Indexing.MultiAttributeValue), 993);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSON.NullValue), 992);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Operations.InsertOperation), 991);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Operations.OperationType), 990);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Operations.RemoveOperation), 989);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Operations.UpdateOperation), 988);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Operations.GetOperation), 987);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Operations.Operation), 986);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.Enum.FieldDataType), 985);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Core.Storage.Providers.LMDB.LMDBCollection), 984);
            CompactFormatterServices.RegisterCompactType(typeof(Common.JSON.Indexing.AllValue), 981);
            CompactFormatterServices.RegisterCompactType(typeof(Common.JSON.Indexing.NoValue), 980);
            CompactFormatterServices.RegisterCompactType(typeof(Alachisoft.NosDB.Common.JSON.Indexing.ArrayElement), 979);

            #region recovery classes
            CompactFormatterServices.RegisterCompactType(typeof(Common.Configuration.DOM.RecoveryConfiguration), 978);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ShardRecoveryJobState), 977);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.RecoveryJobStateBase), 976);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ClusteredRecoveryJobState), 975);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.RecoveryOperationStatus), 974);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.RecoveryOperation), 973);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.SubmitBackupOpParams), 972);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.SubmitRestoreOpParams), 971);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Configuration.DOM.RecoveryPersistenceConfiguration), 970);
            CompactFormatterServices.RegisterCompactType(typeof(Core.Recovery.Persistence.DataSlice.Header), 969);
            CompactFormatterServices.RegisterCompactType(typeof(Core.Recovery.Persistence.Segment.Header), 968);
            CompactFormatterServices.RegisterCompactType(typeof(Core.Recovery.Persistence.BackupFile.Header), 967);
            CompactFormatterServices.RegisterCompactType(typeof(Core.Configuration.Recovery.CsBackupableEntities), 966);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.DiffTrackObject), 965);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.Operation.EndOpParams), 964);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.DiffTrackObject[]), 963);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ClusteredRecoveryJobState[]), 962);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.RecoveryOperationStatus[]), 961);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ClusterJobInfoObject), 960);
            CompactFormatterServices.RegisterCompactType(typeof(Common.Recovery.ClusterJobInfoObject[]), 959);
            #endregion
            #endregion
        }

        public ManagementHost()
        {

            _managementServer = new ManagementServer();
            _managementSessionLisioner = new ManagementSessionListener();
            _managementSessionLisioner.ManagementServer = _managementServer;

        }

        public void Initialize()
        {
            _initialized = true;
            string ipAddress = ConfigurationSettings<DBHostSettings>.Current.IP.ToString();
            int dbMgtPort = ConfigurationSettings<DBHostSettings>.Current.Port;
            
            _managementServer.Initialize(IPAddress.Parse(ipAddress), dbMgtPort, _managementSessionLisioner);
           
        }

        public void Start()
        {
            try
            {
                if (!_initialized)
                    throw new Exception("HOST not initialized");

                // + security: SPN registration
                if (ConfigurationSettings<CSHostSettings>.Current.IsSecurityEnabled)
                {
                    SSPIUtility.RegisterSpn(true);
                    if (!SSPIUtility.IsSPNRegistered)
                    {
                        AppUtil.LogEvent("DB Service: SPN is not registered. Only local connections will be served.", EventLogEntryType.Information);
                    }
                }
                // - security

                _managementServer.Start();
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Info("ManagementHost.Start()", "Management Host started.");
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.ServerLogger != null && LoggerManager.Instance.ServerLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.ServerLogger.Error("ManagementHost.Start()", "Error:", ex);
                }
                throw;
            }

        }

        public void Stop()
        {
           //managementServer.ManagementShardServer.Stop();
            _managementServer.Stop();

            // + security: SPN registration
            if (ConfigurationSettings<CSHostSettings>.Current.IsSecurityEnabled)
            {
                SSPIUtility.RegisterSpn(false);
                if (SSPIUtility.IsSPNRegistered)
                {
                    AppUtil.LogEvent("DB Service: SPN is not unregistered. ", EventLogEntryType.Information);
                }
            }
            // - security
            

        }

        public bool StartLocalDbNode()
        {
            return _managementServer.StartLocalDbNode();
        }

        public bool StartDbNodes()
        {
            return _managementServer.StartDbNodes();
        }
    }
}
