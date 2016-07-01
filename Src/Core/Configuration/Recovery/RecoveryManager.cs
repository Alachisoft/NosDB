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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Recovery.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    /// <summary>
    /// Manages all client initiated recovery operations
    /// </summary>
    public class RecoveryManager : IRecoveryManager, IDisposable, IConfigOperationExecutor
    {
        Hashtable _runningClusteredJobMap = null;
        IConfigOperationExecutor _operationHandler;
        IRecoveryCommunicationHandler _communicationHandler = null;
        IConfigurationStore _configurationStore;
        object _mutex;

        public RecoveryManager(IConfigOperationExecutor opHandler, IConfigurationStore configurationStore)
        {
            _runningClusteredJobMap = Hashtable.Synchronized(new Hashtable());
            _operationHandler = opHandler;
            _configurationStore = configurationStore;
            new Dictionary<DIFKey, List<DiffTrackObject>>();
            _mutex = new object();
            //recreate diffMap

        }

        #region IRecoveryManager Implementation
        RecoveryOperationStatus IRecoveryManager.SubmitRecoveryJob(RecoveryConfiguration config, object additionalParams)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";

            try
            {
                // Ensure the prereqs provided are valid
                RecoveryOperationStatus valid = this.EnsurePreRequisites(config, additionalParams);
                if (valid.Status == RecoveryStatus.Success)
                {

                    // 1. create and register clustered job
                    ClusterRecoveryJob _clusteredJob = new ClusterRecoveryJob(this.AssignJobUID(), config, this, _configurationStore);
                    _clusteredJob.RegisterRecoveryCommunicationHandler(this);
                    _runningClusteredJobMap.Add(_clusteredJob.JobIdentifier, _clusteredJob);

                    state.JobIdentifier = _clusteredJob.JobIdentifier;


                    // create rootfolder for recovery
                    switch (config.JobType)
                    {
                        case RecoveryJobType.ConfigBackup:
                        case RecoveryJobType.DataBackup:
                        case RecoveryJobType.FullBackup:

                            // 
                            RecoveryOperationStatus folderStatus = _clusteredJob.CreateRecoveryFolder(config.RecoveryPath, config.UserName, config.Password);

                            if (folderStatus.Status == RecoveryStatus.Failure)
                            {
                                RemoveRunningJob(_clusteredJob.JobIdentifier);
                                _clusteredJob.Dispose();
                                return folderStatus;
                            }
                            break;
                    }

                    //2. call prepare for this job
                    state = _clusteredJob.Initialize(config, additionalParams);

                    //3. verify status
                    if (state.Status == RecoveryStatus.Failure)
                    {

                        RemoveRunningJob(_clusteredJob.JobIdentifier);

                        return state;
                    }

                    if (state.Status == RecoveryStatus.Failure)
                    {
                        // remove job from active config
                        RemoveRunningJob(_clusteredJob.JobIdentifier);
                        return state;
                    }
                    else
                        state = _clusteredJob.Start(config);



                }
                else
                    state = valid;

            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.Submit()", ex.ToString());
                state.Message = ex.Message;
                // log exception
            }

            return state;
        }

        RecoveryOperationStatus IRecoveryManager.CancelRecoveryJob(string identifier)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during cancellation"; // change this default message

            try
            {
                RecoveryConfiguration config = new RecoveryConfiguration();
                config.Identifier = identifier;
                state.JobIdentifier = identifier;
                if (_runningClusteredJobMap.ContainsKey(identifier))
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                        LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.CancelRecoveryJob()", "Explicit canceling initiated");
                    IClusteredRecoveryJob _job = ((IClusteredRecoveryJob)_runningClusteredJobMap[identifier]);
                    state = _job.Cancel(config, explicitCancel: true);
                    // remove job from active config
                    RemoveRunningJob(identifier);
                }
                else
                {
                    state.Message = "Invalid identifier provided";
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.Cancel()", ex.ToString());
                state.Message = ex.Message;
            }
            return state;
        }

        RecoveryOperationStatus[] IRecoveryManager.CancelAllRecoveryJobs()
        {
            RecoveryOperationStatus[] state = new RecoveryOperationStatus[_runningClusteredJobMap.Count];// new RecoveryOperationStatus(RecoveryStatusType.failure);
            //state.Message = "Failure during cancellation"; // change this default message

            try
            {
                foreach (string _key in _runningClusteredJobMap.Keys)
                {
                    IClusteredRecoveryJob _job = ((IClusteredRecoveryJob)_runningClusteredJobMap[_key]);

                    //state = _job.Cancel(_key);
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.CancelAll()", ex.ToString());
                //  state.Message = ex.Message;
            }
            return state;
        }

        public ClusteredRecoveryJobState GetJobState(string identifier)
        {
            ClusteredRecoveryJobState state = new ClusteredRecoveryJobState(identifier);
            try
            {
                RecoveryConfiguration config = new RecoveryConfiguration();
                config.Identifier = identifier;
                state.Identifier = identifier;
                if (_runningClusteredJobMap.ContainsKey(identifier))
                {
                    IClusteredRecoveryJob _job = ((IClusteredRecoveryJob)_runningClusteredJobMap[identifier]);
                    state = _job.CurrentState(config) as ClusteredRecoveryJobState;
                }
                else
                {

                    ClusterJobInfoObject clusterJobs = _configurationStore.GetRecoveryJobData(identifier);
                    if (clusterJobs != null)// this should always be equyal 
                    {
                        state = clusterJobs.ExecutionState;
                    }
                    else
                    {
                        state.Message = "Invalid identifier provided";
                    }
                    // query db for
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.GetState()", ex.ToString());
                state.Message = ex.Message;
            }
            return state;
        }

        void IRecoveryManager.SubmitRecoveryState(object state)
        {
            if (state != null)
            {
                ShardRecoveryJobState _shardStatus = (state as ShardRecoveryJobState);
                if (_runningClusteredJobMap.ContainsKey(_shardStatus.Identifier))
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() => (_runningClusteredJobMap[_shardStatus.Identifier] as IClusteredRecoveryJob).SubmitRecoveryState(_shardStatus));
                }
            }
        }

        public void SubmitConfigChanged(object changeConfig)
        {
            try
            {
                bool failed = true; ;
                // get all clustered jobs working on given config
                foreach (ClusterRecoveryJob job in _runningClusteredJobMap.Values)
                {
                    if (job.ActiveConfig.JobType == RecoveryJobType.Restore || job.ActiveConfig.JobType == RecoveryJobType.ConfigRestore)
                    {
                        string db = string.Empty;
                        // check if exception or actual data
                        if (changeConfig is RecoveryOperationStatus)
                        {
                            RecoveryOperationStatus status = (RecoveryOperationStatus)changeConfig;
                            string[] splitString = status.JobIdentifier.Split('_');
                            if (!string.IsNullOrEmpty(splitString[1]))
                            {
                                db = splitString[1];
                            }
                            else
                                db = splitString[0];


                        }

                        if (changeConfig is CsBackupableEntities)
                        {
                            CsBackupableEntities entity = (CsBackupableEntities)changeConfig;
                            db = entity.Database.First().Key.ToLower();
                            failed = false;
                        }

                        KeyValuePair<string, string> dbMap = job.ActiveConfig.DatabaseMap.First();

                        bool valid = false;
                        if (!string.IsNullOrEmpty(dbMap.Value))
                        {
                            if (dbMap.Value.ToLower().Equals(db))
                            {
                                valid = true;
                            }
                        }
                        else
                        {
                            if (dbMap.Key.ToLower().Equals(db))
                            {
                                valid = true;
                            }
                        }

                        if (valid && !failed)
                        {
                            RecoveryConfiguration config = new RecoveryConfiguration();
                            config.Identifier = job.JobIdentifier;
                            RecoveryStatus state = (job.CurrentState(config) as ClusteredRecoveryJobState).Status;

                            if (state != RecoveryStatus.Failure || state != RecoveryStatus.Cancelled || state != RecoveryStatus.Completed)
                            {
                                if (changeConfig != null)
                                    job.SubmitConfigChanged(changeConfig);
                                else
                                {
                                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                        LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.SubmitConfigChanged()", "failing");
                                    job.Cancel(config);// job has failed
                                    // remove job from active config
                                    RemoveRunningJob(config.Identifier);
                                    break;
                                }
                            }
                        }
                        else if (failed)
                        {
                            RecoveryConfiguration config = new RecoveryConfiguration();
                            config.Identifier = job.JobIdentifier;
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.SubmitConfigChanged()", "overall failure");
                            job.Cancel(config);
                            // remove job from active config
                            RemoveRunningJob(config.Identifier);
                            break;
                        }
                        //job.
                    }
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.SubmitConfigChanged()", ex.ToString());

            }
        }



        public bool IsOperationAllowed(string cluster, string database)
        {
            bool allowed = true;
            lock (_mutex)
            {
                foreach (ClusterRecoveryJob job in _runningClusteredJobMap.Values)
                {
                    if (job.ActiveConfig.Cluster.Equals(cluster))
                    {
                        if (job.ActiveConfig.DatabaseMap.Keys.Contains(database))
                        {
                            allowed = false;
                            break;
                        }
                    }
                }
            }
            return allowed;
        }

        public void OnMembershipChanged(ConfigChangeEventArgs args)
        {
            List<string> removableIds = new List<string>();
            foreach (ClusterRecoveryJob job in _runningClusteredJobMap.Values)
            {
                try
                {
                    string cluster = args.GetParamValue<string>(EventParamName.ClusterName);
                    string shardName = args.GetParamValue<string>(EventParamName.ShardName);
                    if (job.ActiveConfig.Cluster.Equals(cluster))
                    {
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.CancelOnMembership()", "Canceling job " + job.ActiveConfig.Identifier + " due to membership change of " + shardName);
                        removableIds.Add(job.ActiveConfig.Identifier);
                        job.Cancel(job.ActiveConfig, shardName);
                        job.SaveStateToStore();
                        job.Dispose();
                    }
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.CancelOnMembership()", exp.ToString());
                }
            }

            foreach (string id in removableIds)
            {
                _runningClusteredJobMap.Remove(id);
            }
        }


        public ClusterJobInfoObject[] GetAllRunningJobs()
        {
            List<ClusterJobInfoObject> jobStates = new List<ClusterJobInfoObject>();
            foreach (ClusterJobInfoObject infoObj in _configurationStore.GetAllRecoveryJobData())
            {

                jobStates.Add(infoObj);
            }
            return jobStates.ToArray();
        }





        #endregion

        #region helper methods

        private string AssignJobUID()
        {
            return Guid.NewGuid().ToString();
        }

        private RecoveryOperationStatus EnsurePreRequisites(RecoveryConfiguration config, object additionalParams)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.JobIdentifier = config.Identifier;

            Impersonation impersonation = null;
            if (RecoveryFolderStructure.PathIsNetworkPath(config.RecoveryPath))
                impersonation = new Impersonation(config.UserName, config.Password);

            List<string> shardNameList = new List<string>();

            ClusterInfo clusterInfo = GetConfiguredClusters(config.Cluster);
            shardNameList.AddRange(clusterInfo.ShardInfo.Keys);

            state = RecoveryFolderStructure.ValidateFolderStructure(config.RecoveryPath, config.JobType, true,
                shardNameList);
            if (state.Status == RecoveryStatus.Failure)
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.EnsurePreRequisites()", config.RecoveryPath + "  : " + state.Message);

            if (state.Status == RecoveryStatus.Success)
            {
                CsBackupableEntities entity = (CsBackupableEntities)additionalParams;

                #region validate db Name

                switch (config.JobType)
                {
                    case RecoveryJobType.ConfigRestore:
                    case RecoveryJobType.Restore:
                        state = DatabaseExists(config.DatabaseMap, entity, config.JobType);
                        if (state.Status == RecoveryStatus.Failure)
                        {
                            return state;
                        }
                        break;
                    case RecoveryJobType.ConfigBackup:
                    case RecoveryJobType.DataBackup:
                    case RecoveryJobType.FullBackup:

                        state = DatabaseExists(config.DatabaseMap, entity, config.JobType);
                        if (state.Status == RecoveryStatus.Failure)
                        {
                            return state;
                        }

                        break;
                }
                state.Status = RecoveryStatus.Success;

                #endregion

                #region validate files

                if (config.JobType == RecoveryJobType.Restore)
                {
                    string configPath = Path.Combine(config.RecoveryPath, RecoveryFolderStructure.CONFIG_SERVER);
                    string filePath = Path.Combine(configPath, RecoveryFolderStructure.CONFIG_SERVER);

                    if (File.Exists(filePath))
                    {
                        BackupFile file = new BackupFile(RecoveryFolderStructure.CONFIG_SERVER, configPath, config.UserName, config.Password);

                        Alachisoft.NosDB.Core.Recovery.Persistence.BackupFile.Header header = file.ReadFileHeader();
                        if (!header.Database.ToLower().Equals(config.DatabaseMap.First().Key))
                        {
                            state.Status = RecoveryStatus.Failure;
                            state.Message = "Provided file does not contain the source database " +
                                            config.DatabaseMap.First().Key;

                        }
                        file.Close();
                    }
                    else
                    {
                        state.Status = RecoveryStatus.Failure;
                        state.Message = "No file exists in the given folder";
                    }
                }

                #endregion
                if (impersonation != null)
                    impersonation.Dispose();
            }
            if (state.Status == RecoveryStatus.Failure)
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.EnsurePreRequisites()", state.Message);

            return state;
        }

        public void Restore(CsBackupableEntities entity, Dictionary<string, string> database, string cluster)
        {
            _operationHandler.Restore(entity, database, cluster);
        }


        private void GetAll()
        {
            _configurationStore.GetAllRecoveryJobData();
        }

        private RecoveryOperationStatus DatabaseExists(Dictionary<string, string> dbMap, CsBackupableEntities entity, RecoveryJobType jobType)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Success);
            if (dbMap.Count > 0)
            {
                switch (jobType)
                {
                    case RecoveryJobType.ConfigRestore:
                    case RecoveryJobType.Restore:

                        foreach (string db in dbMap.Keys)
                        {
                            string destination = dbMap[db];

                            if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                            {
                                if (entity.Database.Keys.Contains(destination))
                                {
                                    state.Status = RecoveryStatus.Failure;
                                    state.Message = "Destination Database {" + destination + "} already exists";
                                    return state;
                                }
                            }
                            else if (!string.IsNullOrEmpty(db))
                            {
                                if (entity.Database.Keys.Contains(db))
                                {
                                    state.Status = RecoveryStatus.Failure;
                                    state.Message = "Database {" + db + "} already exists with the Name provided";
                                    return state;
                                }
                            }
                            else
                            {
                                state.Status = RecoveryStatus.Failure;
                                state.Message = "In-valid name provided for Backup";
                                return state;
                            }
                        }

                        break;
                    case RecoveryJobType.ConfigBackup:
                    case RecoveryJobType.DataBackup:
                    case RecoveryJobType.FullBackup:


                        foreach (string db in dbMap.Keys)
                        {
                            if (!string.IsNullOrEmpty(db))
                            {
                                if (!entity.Database.Keys.Contains(db))
                                {
                                    state.Status = RecoveryStatus.Failure;
                                    state.Message = "Database {" + db + "} does not exists";
                                    return state;
                                }
                            }
                            else
                            {
                                state.Status = RecoveryStatus.Failure;
                                state.Message = "In-valid name provided for Restore";
                                return state;
                            }
                        }

                        break;
                }
            }
            return state;
        }

        internal void RemoveRunningJob(string identifier)
        {
            try
            {
                if (_runningClusteredJobMap.ContainsKey(identifier))
                {
                    lock (_mutex)
                    {
                        _runningClusteredJobMap.Remove(identifier);
                    }
                }
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("RecoveryManager.RemoveJob()", "remove job : " + identifier + " on completion");
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryManager.RemoveJob()", exp.ToString());
            }
        }
        #endregion

        #region RecoveryCommunication Managment
        public void RegisterRecoveryCommunicationHandler(IRecoveryCommunicationHandler handler)
        {
            _communicationHandler = handler;

        }

        public RecoveryOperationStatus SubmitRecoveryJob(string node, string cluster, string shard, RecoveryOperation opContext)
        {
            return _communicationHandler.SubmitRecoveryJob(node, cluster, shard, opContext);
        }

        public RecoveryOperationStatus SubmitRecoveryJob(string cluster, string shard, RecoveryOperation opContext)
        {
            return _communicationHandler.SubmitRecoveryJob(cluster, shard, opContext);
        }

        //internal ClusterConfiguration GetClusterConfiguration(string database)
        //{
        //    return (_communicationHandler as ConfigurationServer).GetClusterConfiguration(database);
        //}

        internal ClusterInfo GetConfiguredClusters(string cluster)
        {
            return (_communicationHandler as ConfigurationServer).GetDatabaseClusterInfo(cluster);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            foreach (ClusterRecoveryJob job in _runningClusteredJobMap.Values)
            {
                job.Dispose();
            }
            _runningClusteredJobMap = null;

        }
        #endregion

        public RecoveryConfiguration GetJobConfiguration(string identifier)
        {
            RecoveryConfiguration config = new RecoveryConfiguration();
            config.Identifier = identifier;

            if (_runningClusteredJobMap.ContainsKey(identifier))
            {
                config = ((ClusterRecoveryJob)_runningClusteredJobMap[identifier]).ActiveConfig;
            }
            else
            {
                ClusterJobInfoObject jobInfo = _configurationStore.GetRecoveryJobData(identifier);
                if (jobInfo != null)
                    config = jobInfo.ActiveConfig;
            }
            return config;
        }



    }
}
