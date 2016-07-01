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

namespace Alachisoft.NosDB.Common.Util
{
    public class ConfigurationCommandUtil
    {
        public struct MethodName
        {
            #region IConfigurationService Methods
            public const string OpenShardConfigurationSession = "OpenShardConfigurationSession";
            public const string OpenConfigurationSession = "OpenConfigurationSession"; 
            /// <summary>
            /// for authentication
            /// </summary>
            public const string Authenticate = "Authenticate";
            public const string AuthenticateNoSDbClient = "AuthenticateNoSDbClient";

            public const string HasSynchronizedWIthPrimaryServer = "HasSynchronizedWIthPrimaryServer";
            #endregion

            #region IConfigurationSession Methods
            public const string GetShardsPort = "GetShardsPort"; 
            public const string GetConfigurationClusterConfiguration = "GetConfigurationClusterConfiguration";
            public const string GetAllClusterConfiguration = "GetAllClusterConfiguration";
            public const string GetDatabaseClusterConfiguration = "GetDatabaseClusterConfiguration";
            public const string GetConfiguredClusters = "GetConfiguredClusters";
            public const string RegisterClusterConfiguration = "RegisterClusterConfiguration";
            public const string UnregisterClusterConfiguration = "UnregisterClusterConfiguration";
            public const string UpdateClusterConfiguration = "UpdateClusterConfiguration";
            public const string CreateCluster = "CreateCluster";
            public const string RemoveCluster = "RemoveCluster";
            public const string AddShardToCluster = "AddShardToCluster";
            public const string RemoveShardFromCluster = "RemoveShardFromCluster";
            public const string AddServerToShard = "AddServerToShard";
            public const string RemoveServerFromShard = "RemoveServerFromShard";
            public const string GetDatabaseClusterInfo = "GetDatabaseClusterInfo";
            public const string GetCollectionDistribution = "GetCollectionDistribution";
            public const string CreateDatabase = "CreateDatabase";
            public const string DropDatabase = "DropDatabase";
            public const string UpdateMembership = "UpdateMembership";
            public const string CreateCollection = "CreateCollection";
            public const string MoveCollection = "MoveCollection";
            public const string DropCollection = "DropCollection";
            public const string CreateUserDefinedFunction = "CreateUserDefinedFunction";
            public const string CreateTrigger = "CreateTrigger";
            public const string UpdateTrigger = "UpdateTrigger";
            public const string DropTrigger = "DropTrigger";
            public const string DropUserDefinedFunction = "DropUserDefinedFunction";
            public const string GetUserDefinedFunctions = "GetUserDefinedFunctions";
            public const string AddConfigurationListener = "AddConfigurationListener";
            public const string RemoveConfigurationListener = "RemoveConfigurationListener";
            public const string ReportNodeJoining = "ReportNodeJoining";
            public const string ReportNodeLeaving = "ReportNodeLeaving";
            public const string ReportHeartbeat = "ReportHeartbeat";
            public const string CreateIndex = "CreateIndex";
            public const string DropIndex = "DropIndex";
            public const string UpdateCollectionStatistics = "UpdateCollectionStatistics";
            public const string UpdateBucketStatistics = "UpdateBucketStatistics";
            public const string StartShard = "StartShard";
            public const string StartCluster = "StartCluster";
            public const string StopShard = "StopShard";
            public const string StopCluster = "StopCluster";
            //public const string ReportLastOperationTime = "ReportLastOperationTime";
            public const string CreateConfigurationServer = "CreateConfigurationServer";
            public const string RemoveConfigurationCluster = "RemoveConfigurationCluster";
            public const string StartConfigurationServer = "StartConfigurationServer";
            public const string StopConfigurationServer = "StopConfigurationServer";
            public const string AddNodeToConfigurationCluster = "AddNodeToConfigurationCluster";
            public const string RemoveNodeFromConfigurationCluster = "RemoveNodeFromConfigurationCluster";
            public const string VerifyConfigurationServerAvailability = "VerifyConfigurationServerAvailability";
            public const string VerifyConfigurationCluster = "VerifyConfigurationCluster";
            public const string VerifyConfigurationClusterPrimery = "VerifyConfigurationClusterPrimery";
            public const string VerifyConfigurationClusterAvailability = "VerifyConfigurationClusterAvailability";
            public const string NodeAddedToConfigurationCluster = "NodeAddedToConfigurationCluster";
            public const string NodeRemovedFromConfigurationCluster = "NodeRemovedFromConfigurationCluster";

            public const string ListDatabases = "ListDatabases";
            public const string ListCollections = "ListCollections";
            public const string ListIndices = "ListIndices";
            public const string UpdateDatabaseConfiguration = "UpdateDatabaseConfiguration";
            public const string UpdateServerPriority = "UpdateServerPriority";
            public const string UpdateDeploymentConfiguration = "UpdateDeploymentConfiguration";
            public const string UpdateIndexAttribute = "UpdateIndexAttribute";
            public const string UpdateShardConfiguration = "UpdateShardConfiguration";
            public const string UpdateCollectionConfiguration = "UpdateCollectionConfiguration";
            public const string GetDataBaseServerNode = "GetDataBaseServerNode";
            public const string CopyAssemblies = "DeployAssemblies";
            public const string VerifyConfigurationClusterUID = "VerifyConfigurationClusterUID";
            public const string IsRemoteClient = "IsRemoteClient";
            public const string IsAuthorized = "IsAuthorized";
            public const string MarkDatabaseSession = "MarkDatabaseSession";
            public const string MarkDistributorSession = "MarkDistributorSession";
            public const string UpdateCSNodePriority = "UpdateCSNodePriority";
            public const string CreateLocalCluster = "CreateLocalCluster";
            public const string GetShardConfiguration = "GetShardConfiguration";
            public const string IsNodeRunning = "IsNodeRunning";
            #endregion

            #region IShardConfigurationSession Methods
            //public const string GetDatabaseClusterConfiguration = "GetDatabaseClusterConfiguration";
            public const string SetNodeStatus = "SetNodeStatus";
            public const string GetMembershipInfo = "GetMembershipInfo";
            public const string UpdateNodeStatus = "UpdateNodeStatus";
            public const string BeginElection = "BeginElection";
            public const string SubmitElectionResult = "SubmitElectionResult";
            public const string EndElection = "EndElection";
            public const string ConfigureDistributionStategy = "ConfigureDistributionStategy";
            public const string GetDistriubtionStrategy = "GetDistriubtionStrategy";
            public const string GetCurrentDistribution = "GetCurrentDistribution";
            public const string BalanceData = "BalanceData"; 
            #endregion

            #region StateTransfer Operations
            public const string StateTransferOperation = "StateTransferOperation";
            #endregion

            #region IDatabaseConfigurationSession Methods
           
            
            public const string StartNode = "StartNode";
            public const string StopNode = "StopNode";
            public const string StopNodeForClients = "StopNodeForClients";

            #endregion

            #region IMonitorServer Methods


            //public const string InitializeMonitor = "InitializeMonitor";
            public const string RegisterEventViewerEvents = "RegisterEventViewerEvents";
            public const string UnRegisterEventViewerEvents = "UnRegisterEventViewerEvents";
            public const string GetLatestEvents = "GetLatestEvents";
            public const string GetRunningServers = "GetRunningServers";
            public const string GetUpdatedRunningServers = "GetUpdatedRunningServers";
            public const string GetPercentageCPUUsage = "GetPercentageCPUUsage";
            public const string GetShardNodeNIC = "GetShardNodeNIC";
            public const string GetNICForIP = "GetNICForIP";
            public const string GetClientProcessStats = "GetClientProcessStats";
            ////temporary methods
            //public const string InitializeMonitor = "InitializeMonitor";
            //public const string GetShardNodes = "GetShardNodes";
            //public const string GetUpdatedServers = "GetUpdatedServers";


            #endregion
             
            #region IConfigurationMonitor Methods

            public const string GetUpdatedMembershipInfo = "GetUpdatedMembershipInfo";
            public const string GetConfigureServerNodes = "GetConfigureServerNodes";
            public const string GetUpdatedConfigureServerNodes = "GetUpdatedConfigureServerNodes";
            public const string GetConfiguredShards = "GetConfiguredShards";
            public const string GetUpdatedConfiguredShards = "GetUpdatedConfiguredShards";
            public const string GetRunningServerNodes = "GetRunningServerNodes";
            public const string GetUpdatedRunningServerNodes = "GetUpdatedRunningServerNodes";


            #endregion

            #region RecoveryOperations
            public const string SubmitRecoveryJob = "SubmitRecoveryJob";
            public const string CancelRecoveryJob = "CancelRecoveryJob";
            public const string CancelAllRecoveryJobs = "CancelAllRecoveryJobs";
            public const string GetJobState = "GetJobState";
            public const string SubmitJobState = "SubmitJobState";
            public const string GetAllRunningJobs = "GetAllRunningJobs";
            public const string GetAllShardDifRegisteredDbs = "GetAllShardDifRegisteredDbs";
            public const string StartShardDifDataJob = "StartShardDifDataJob";
            public const string CancelAllDIFTracking = "CancelAllDIFTracking";
            public const string CancelDIFTracking = "CancelDIFTracking";
            public const string GetAllDifEnabledDbs = "GetAllDifEnabledDbs";
            #endregion

            #region Security
            public const string Grant = "Grant";
            public const string Revoke = "Revoke";
            public const string PopulateSecurityInformationOnDBServer = "PopulateSecurityInformationOnDBServer";
            public const string CreateUser = "CreateUser";
            public const string DropUser = "DropUser";
            public const string CreateRole = "CreateRole";
            public const string DropRole = "DropRole";
            public const string AlterRole = "AlterRole";
            public const string PublishAuthenticatedUserInfoToDBServer = "PublishAuthenticatedUserInfoToDBServer";

            public const string GetUsersInformation = "GetUsersInformation";
            public const string GetResourcesInformation = "GetResourceInformation";

            public const string GetAuthenticatedUserInfoFromConfigServer = "GetAuthenticatedUserInfoFromConfigServer";

            public const string GetResourcesSecurityInfo = "GetResourcesSecurityInfo";
            public const string GetResourceSecurityInfo = "GetResourceSecurityInfo";
            public const string GetDatabaseCluster = "GetDatabaseCluster";
            public const string GetShards = "GetShards";
            public const string CanAddToDatabaseCluster = "CanAddToDatabaseCluster";
            public const string GetUserInfo = "GetUserInfo";
            public const string ReplicateTransaction = "ReplicateTransaction";
            public const string GetState = "GetState";
            public const string GetCurrentRole = "GetCurrentRole";
            public const string BeginTakeOver = "BeginTakeOver";
            public const string Demote = "Demote";
            public const string MarkSessionInternal = "MarkSessionInternal";

            public const string MarkConfiguritonSession = "MarkConfigurationSession";


            #endregion

            public const string SetDatabaseMode = "SetDatabaseMode";


            public const string ValidateProfessional = "ValidateProfessional";

            public const string GetImplementation = "GetImplementation";

            #region
            public const string GetConfClusterServers = "GetConfClusterServers";

            #endregion
        }
    }
}
