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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Recovery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    /// <summary>
    /// Wrapper Class that holds all jobs against a request. This class encapsulates local(configRecoveryJob) & 
    /// Remote(ShardRecoveryJobs)
    /// </summary>
    public class ClusterRecoveryJob : IClusteredRecoveryJob, IJobProgressHandler, IConfigOperationExecutor
    {
        //private RecoveryConfiguration _infoObject.ActiveConfig = null;
        //private string _infoObject.RootFolderName;
        //private string _jobIdentifier = string.Empty;
        //ClusteredRecoveryJobState _infoObject.ExecutionState = null;e

        private ConfigurationRecoveryJobBase _configJob = null;
        private IRecoveryCommunicationHandler _communicationHandler = null;
        private int timeout = 12000;// 1 min
        private RecoveryPersistenceManager _persistenceManager;
        internal object _mutex = new object();
        private IConfigOperationExecutor _operationHandler = null;
        private IConfigurationStore _configurationStore = null;
        private ClusterJobInfoObject _infoObject = null;

        internal ClusterRecoveryJob(string identifier, RecoveryConfiguration config, IConfigOperationExecutor handler, IConfigurationStore configurationStore)
        {
            if (!string.IsNullOrEmpty(identifier))
            {
                config.CreationTime = DateTime.Now;
                _infoObject = new ClusterJobInfoObject(identifier, config);
                _persistenceManager = new RecoveryPersistenceManager();

                _operationHandler = handler;
                _configurationStore = configurationStore;
            }

        }

        #region Properties
        public string JobIdentifier
        {
            get { return _infoObject.Identifier; }

        }

        public RecoveryConfiguration ActiveConfig
        {
            get { return _infoObject.ActiveConfig; }

        }
        #endregion

        #region InfoObject properties
        public void AddShardResponse(string shard, ShardDifState response)
        {
            _infoObject.AddShardResponse(shard, response);
        }

        public ShardDifState ShardReceivedState
        {
            get
            {
                return _infoObject.ShardReceivedState();
            }
        }

        public DateTime? LatestResponseTime
        {
            get
            {
                return _infoObject.LatestResponseTime;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return _infoObject.CreationTime;
            }
        }

        public DateTime StartTime
        {
            get { return _infoObject.ExecutionState.StartTime; }
            set { _infoObject.ExecutionState.StartTime = value; }
        }
        #endregion

        #region IRecoveryJob Operations


        public RecoveryOperationStatus Initialize(RecoveryConfiguration config, object additionalParams)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.JobIdentifier = _infoObject.Identifier;
            config.Identifier = _infoObject.Identifier;
            switch (_infoObject.ActiveConfig.JobType)
            {
                case RecoveryJobType.FullBackup:
                    #region Full Backup
                    state = CreateFullBackupJob(config, additionalParams);
                    break;
                    #endregion
                case RecoveryJobType.Restore:
                    #region Full Restore
                    state = CreateFullRestoreJob(config);
                    #endregion
                    break;
                case RecoveryJobType.DataBackup:
                    #region Data Backup
                    config.Operation = RecoveryOpCodes.SubmitBackupJob;
                    state = CreateRemoteJob(config, (CsBackupableEntities)additionalParams);
                    break;
                    #endregion
                case RecoveryJobType.DataRestore:
                    #region Data Restore
                    config.Operation = RecoveryOpCodes.SubmitRestoreJob;
                    state = CreateRemoteJob(config, (CsBackupableEntities)additionalParams);
                    break;
                    #endregion
                case RecoveryJobType.ConfigBackup:
                    #region Config Backup
                    state = CreateConfigBackupJob(config, additionalParams);
                    break;
                    #endregion
                case RecoveryJobType.ConfigRestore:
                    #region Config Restore
                    state = CreateConfigRestoreJob(config);
                    break;
                    #endregion


            }
            //save execution status in sysDB
            _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);

            return state;
        }

        public RecoveryOperationStatus Start(RecoveryConfiguration config)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            state.JobIdentifier = _infoObject.Identifier;
            try
            {
                CancellationTokenSource cancelToken = new CancellationTokenSource();
                CancellationToken ct = cancelToken.Token;
                List<Task<RecoveryOperationStatus>> _runningTasks = new List<Task<RecoveryOperationStatus>>();

                List<ShardRecoveryJobState> configJobs = _infoObject.ExecutionState.Details.Where(x => ((x.JobType == RecoveryJobType.ConfigBackup) ||
                                                                        (x.JobType == RecoveryJobType.ConfigRestore))).ToList();

                foreach (ShardRecoveryJobState job in configJobs)
                {
                    if (_configJob.ExecutionState.Status == RecoveryStatus.uninitiated)
                    {
                        _persistenceManager.IsJobActive = true;
                        _configJob.Start();

                        state.Status = RecoveryStatus.Executing;
                    }
                }

                #region Initiating remote jobs

                //M_TODO: check how i want run multiple db jobs
                foreach (string db in _infoObject.ActiveConfig.DatabaseMap.Keys)
                {
                    List<ShardRecoveryJobState> dataJobs = _infoObject.ExecutionState.Details.Where(x => (x.JobType == RecoveryJobType.DataBackup) ||
                                                                (x.JobType == RecoveryJobType.DataRestore)).ToList();
                    if (dataJobs != null)
                    {

                        foreach (ShardRecoveryJobState job in dataJobs)
                        {
                            if (job.Status == RecoveryStatus.uninitiated)
                            {
                                RecoveryOperation _operation = new RecoveryOperation();
                                _operation.JobIdentifer = job.Identifier;
                                _operation.OpCode = RecoveryOpCodes.StartJob;

                                string destination = _infoObject.ActiveConfig.DatabaseMap[db];
                                string databaseName = db;

                                if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                                {
                                    databaseName = destination;
                                }

                                _operation.Parameter = databaseName;// specify db name to start job on
                                Task<RecoveryOperationStatus> _task = Task.Factory.StartNew(() => SendRemoteCall(job.Node, job.Cluster, job.Shard,
                                                                                                                        _operation, ct), cancelToken.Token);
                                _runningTasks.Add(_task);
                            }
                        }

                        Task.WaitAll(_runningTasks.ToArray<Task<RecoveryOperationStatus>>(), -1, ct);
                        // take care of this case, this will check if all task statuses is true only then set status as submitted
                        foreach (Task<RecoveryOperationStatus> _taskState in _runningTasks)
                        {
                            if (_taskState.Result.Status != RecoveryStatus.Failure)
                            {
                                state.Status = RecoveryStatus.Executing;
                                state.Message = "Job Started at remote nodes";
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.Start()", config.Identifier + " Started Successfuly");
                            }
                        }

                        // clear all existing tasks on completion
                        _runningTasks.Clear();

                        if (state.Status == RecoveryStatus.Executing)
                        {
                            UpdateExecutionStatus(RecoveryStatus.Executing);
                            //save execution status in sysDB
                            _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);
                        }
                    }
                }
                #endregion
            }
            catch (Exception exp)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.Start()", exp.ToString());
            }
            return state;
        }

        public RecoveryOperationStatus End(RecoveryConfiguration config)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            state.JobIdentifier = _infoObject.Identifier;
            try
            {
                _infoObject.ExecutionState.StopTime = DateTime.Now;
                state = BroadcastCall(config, RecoveryOpCodes.EndJob);
                // rename backup folder 
                #region rename folder

                if (_infoObject.ActiveConfig.JobType == RecoveryJobType.FullBackup)
                {
                    _infoObject.RenameRootFolder(RecoveryFileState.Completed);
                }


                _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);
                #endregion
                _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);
            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.End()", ex.ToString());
            }
            // remove job from active config
            (_operationHandler as RecoveryManager).RemoveRunningJob(_infoObject.ActiveConfig.Identifier);
            return state;
        }

        public RecoveryOperationStatus Cancel(RecoveryConfiguration config, string shard = "", RecoveryFileState folderState = RecoveryFileState.Failed, bool explicitCancel = false)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            state.JobIdentifier = _infoObject.Identifier;
            try
            {
                _infoObject.ExecutionState.StopTime = DateTime.Now;

                state = BroadcastCall(config, RecoveryOpCodes.CancelJob, shard, explicitCancel);

                // forcefully setting code to failure
                if (_infoObject.ExecutionState.Status == RecoveryStatus.Executing || _infoObject.ExecutionState.Status == RecoveryStatus.Waiting
     || _infoObject.ExecutionState.Status == RecoveryStatus.uninitiated)
                    UpdateExecutionStatus(RecoveryStatus.Failure);

                if (_infoObject.ActiveConfig.JobType == RecoveryJobType.FullBackup)
                {
                    if (folderState == RecoveryFileState.Failed)
                        _infoObject.RenameRootFolder(RecoveryFileState.Failed);
                    else
                        _infoObject.RenameRootFolder(RecoveryFileState.Cancelled);
                }

                if (explicitCancel)
                    _infoObject.ExecutionState.Status = RecoveryStatus.Cancelled;

                _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);
            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.Cancel()", ex.ToString());
            }

            return state;
        }

        public object CurrentState(RecoveryConfiguration config)
        {
            return _infoObject.ExecutionState;
        }

        public void RegisterRecoveryCommunicationHandler(IRecoveryCommunicationHandler handler)
        {
            _communicationHandler = handler;
        }

        public void SubmitRecoveryState(object state)
        {
            if (state != null)
            {
                ShardRecoveryJobState jobState = (ShardRecoveryJobState)state;
                try
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                        LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.SubmitRecoveryState()", jobState.ToString());
                    lock (_mutex)
                    {
                        _infoObject.ExecutionState.UpdateJobStatus(jobState);
                    }
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.SubmitRecoveryState()", jobState.ToString() + "\t" + exp.ToString());
                }
                finally
                {
                    CheckJobState();
                    //save execution status in sysDB
                    _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);
                }
            }
        }

        public void SaveStateToStore()
        {
            _configurationStore.InsertOrUpdateRecoveryJobData(_infoObject);
        }


        #endregion

        #region   Operation Initialization Methods
        private RecoveryOperationStatus CreateFullBackupJob(RecoveryConfiguration config, object additionalParams)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submitted the job";
            state.JobIdentifier = _infoObject.Identifier;

            try
            {
                // 1. create local configuration server job
                RecoveryPersistenceConfiguration persistenceConfig = new RecoveryPersistenceConfiguration();
                persistenceConfig.FilePath = Path.Combine(config.RecoveryPath, _infoObject.RootFolderName + @"\" + RecoveryFolderStructure.CONFIG_FOLDER + @"\");
                persistenceConfig.DatabaseName = config.DatabaseMap.First().Key;
                persistenceConfig.Cluster = config.Cluster;
                persistenceConfig.UserName = config.UserName;
                persistenceConfig.Password = config.Password;

                if (persistenceConfig.FileName == null)
                {
                    persistenceConfig.FileName = new List<string>();
                    persistenceConfig.FileName.Add(RecoveryFolderStructure.CONFIG_SERVER);
                }

                if (_persistenceManager.SetJobConfiguration(RecoveryJobType.ConfigBackup, persistenceConfig, string.Empty, 1))
                {
                    CsBackupableEntities entity = (CsBackupableEntities)additionalParams;


                    _configJob = new ConfigurationBackupJob(config.Identifier, _persistenceManager, entity, config.Cluster, false);
                    _configJob.RegisterProgressHandler(this);

                    RecoveryOperation _operation = new RecoveryOperation();
                    _operation.JobIdentifer = config.Identifier;
                    _operation.OpCode = RecoveryOpCodes.SubmitConfigBackupJob;


                    if (_configJob.Initialize(_operation))
                    {
                        // tracking object for config server
                        ShardRecoveryJobState _jobTracker = new ShardRecoveryJobState(config.Identifier, string.Empty, string.Empty, config.Cluster, RecoveryJobType.ConfigBackup);

                        lock (_mutex)
                        {
                            _infoObject.ExecutionState.UpdateJobStatus(_jobTracker);
                        }

                        //create thread to request shard request
                        config.Operation = RecoveryOpCodes.SubmitBackupJob;
                        state = CreateRemoteJob(config, entity);

                        if (state.Status == RecoveryStatus.Submitted || state.Status == RecoveryStatus.Success)
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.CreateFullBackup()", config.Identifier + " Submitted Successfuly");

                    }
                }
                else
                    state.Message = "A recovery job is already running on the configuration server";
            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateFullBackup()", ex.ToString());
            }

            return state;
        }

        private RecoveryOperationStatus CreateFullRestoreJob(RecoveryConfiguration config)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submitted the job";
            state.JobIdentifier = _infoObject.Identifier;

            try
            {
                // 1. create local configuration server job
                RecoveryPersistenceConfiguration _persistenceConfig = new RecoveryPersistenceConfiguration();
                _persistenceConfig.FilePath = Path.Combine(config.RecoveryPath, RecoveryFolderStructure.CONFIG_FOLDER);

                if (_persistenceConfig.FileName == null)
                {
                    _persistenceConfig.FileName = new List<string>();
                    _persistenceConfig.FileName.Add(RecoveryFolderStructure.CONFIG_SERVER);
                }
                _persistenceConfig.UserName = config.UserName;
                _persistenceConfig.Password = config.Password;

                if (_persistenceManager.SetJobConfiguration(RecoveryJobType.ConfigRestore, _persistenceConfig, string.Empty, 1))
                {
                    _configJob = new ConfigurationRestoreJob(config.Identifier, _persistenceManager, this, config.Cluster, config.DatabaseMap, RecoveryJobType.ConfigRestore);
                    _configJob.RegisterProgressHandler(this);

                    RecoveryOperation _operation = new RecoveryOperation();
                    _operation.JobIdentifer = config.Identifier;
                    _operation.OpCode = RecoveryOpCodes.SubmitConfigRestoreJob;
                    if (_configJob.Initialize(_operation))
                    {
                        state.Status = RecoveryStatus.Submitted;
                        state.Message = "Succesfully submitted the job";
                        // tracking object for config server
                        ShardRecoveryJobState _jobTracker = new ShardRecoveryJobState(config.Identifier, string.Empty, string.Empty, config.Cluster, RecoveryJobType.ConfigRestore);
                        lock (_mutex)
                        {
                            _infoObject.ExecutionState.UpdateJobStatus(_jobTracker);
                        }
                    }
                }
                else
                    state.Message = "A recovery job is alreadey running on the configuration server";
            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateFullRestore()", ex.ToString());
            }

            return state;

        }

        private RecoveryOperationStatus CreateConfigBackupJob(RecoveryConfiguration config, object additionalParams)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submitted the job";
            state.JobIdentifier = _infoObject.Identifier;

            try
            {
                // 1. create local configuration server job
                RecoveryPersistenceConfiguration persistenceConfig = new RecoveryPersistenceConfiguration();
                persistenceConfig.FilePath = Path.Combine(config.RecoveryPath, _infoObject.RootFolderName + @"\" + RecoveryFolderStructure.CONFIG_FOLDER + @"\");
                persistenceConfig.DatabaseName = config.DatabaseMap.First().Key;
                persistenceConfig.Cluster = config.Cluster;
                persistenceConfig.UserName = config.UserName;
                persistenceConfig.Password = config.Password;
                if (persistenceConfig.FileName == null)
                {
                    persistenceConfig.FileName = new List<string>();
                    persistenceConfig.FileName.Add(RecoveryFolderStructure.CONFIG_SERVER);
                }

                if (_persistenceManager.SetJobConfiguration(RecoveryJobType.ConfigBackup, persistenceConfig, string.Empty, 1))
                {
                    CsBackupableEntities entity = (CsBackupableEntities)additionalParams;

                    _configJob = new ConfigurationBackupJob(config.Identifier, _persistenceManager, entity, config.Cluster, false);
                    _configJob.RegisterProgressHandler(this);
                    RecoveryOperation _operation = new RecoveryOperation();
                    _operation.JobIdentifer = config.Identifier;
                    _operation.OpCode = RecoveryOpCodes.SubmitConfigBackupJob;
                    if (_configJob.Initialize(_operation))
                    {
                        state.Status = RecoveryStatus.Submitted;
                        ShardRecoveryJobState _jobTracker = new ShardRecoveryJobState(config.Identifier, string.Empty, string.Empty, config.Cluster, RecoveryJobType.ConfigBackup);
                        lock (_mutex)
                        {
                            _infoObject.ExecutionState.UpdateJobStatus(_jobTracker);
                        }
                    }
                }
                else
                    state.Message = "A recovery job is alreadey running on the configuration server";
            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateConfigBackup()", ex.ToString());
            }

            return state;
        }

        private RecoveryOperationStatus CreateConfigRestoreJob(RecoveryConfiguration config)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submitted the job";
            state.JobIdentifier = _infoObject.Identifier;

            try
            {
                // 1. create local configuration server job
                RecoveryPersistenceConfiguration _persistenceConfig = new RecoveryPersistenceConfiguration();
                _persistenceConfig.FilePath = Path.Combine(config.RecoveryPath, RecoveryFolderStructure.CONFIG_FOLDER);

                _persistenceConfig.UserName = config.UserName;
                _persistenceConfig.Password = config.Password;

                if (_persistenceConfig.FileName == null)
                {
                    _persistenceConfig.FileName = new List<string>();
                    _persistenceConfig.FileName.Add(RecoveryFolderStructure.CONFIG_SERVER);
                }

                if (_persistenceManager.SetJobConfiguration(RecoveryJobType.ConfigRestore, _persistenceConfig, string.Empty, 1))
                {
                    _configJob = new ConfigurationRestoreJob(config.Identifier, _persistenceManager, this, config.Cluster, config.DatabaseMap, RecoveryJobType.ConfigRestore);
                    _configJob.RegisterProgressHandler(this);

                    RecoveryOperation _operation = new RecoveryOperation();
                    _operation.JobIdentifer = config.Identifier;
                    _operation.OpCode = RecoveryOpCodes.SubmitConfigRestoreJob;
                    if (_configJob.Initialize(_operation))
                    {
                        state.Status = RecoveryStatus.Submitted;
                        ShardRecoveryJobState _jobTracker = new ShardRecoveryJobState(config.Identifier, string.Empty, string.Empty, config.Cluster, RecoveryJobType.ConfigRestore);
                        lock (_mutex)
                        {
                            _infoObject.ExecutionState.UpdateJobStatus(_jobTracker);
                        }
                    }
                }
                else
                    state.Message = "A recovery job is alreadey running on the configuration server";
            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateConfigRestore()", ex.ToString());
            }

            return state;
        }
        #endregion

        #region Helper Methods




        internal RecoveryOperationStatus CreateRecoveryFolder(string path, string username, string password)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Success);
            state.Message = "Successfully created root folder";
            state.JobIdentifier = _infoObject.Identifier;

            try
            {
                Impersonation impersonation = null;
                if (RecoveryFolderStructure.PathIsNetworkPath(path))
                    impersonation = new Impersonation(username, password);

                LoggerManager.Instance.RecoveryLogger.Info("root folder ", Path.Combine(path, _infoObject.RootFolderName));
                Directory.CreateDirectory(Path.Combine(path, _infoObject.RootFolderName));
                if (impersonation != null)
                    impersonation.Dispose();
            }
            catch (Exception ex)
            {
                state.Status = RecoveryStatus.Failure;
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateRootFolder()", ex.ToString());
            }

            return state;
        }

        private RecoveryOperationStatus BroadcastCall(RecoveryConfiguration config, RecoveryOpCodes opCode, string shard = "", bool explicitCancel = false)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            state.JobIdentifier = _infoObject.Identifier;
            bool configRestoreFailed = false;
            try
            {

                List<ShardRecoveryJobState> configJobs = _infoObject.ExecutionState.Details.Where(x => (x.JobType == RecoveryJobType.ConfigBackup) || (x.JobType == RecoveryJobType.ConfigRestore)).ToList<ShardRecoveryJobState>();

                #region End Config Job
                foreach (ShardRecoveryJobState job in configJobs)
                {
                    try
                    {
                        if (_configJob.ExecutionState.JobType == RecoveryJobType.ConfigBackup)
                        {
                            _persistenceManager.IsJobActive = false;
                            _persistenceManager.CloseBackupFile(RecoveryFolderStructure.CONFIG_SERVER, RecoveryFileState.Completed);
                        }

                        try
                        {
                            _configJob.Dispose();
                        }
                        catch (ThreadAbortException)
                        {
                            Thread.ResetAbort(); // ignore it
                        }

                        switch (opCode)
                        {
                            case RecoveryOpCodes.CancelJob:
                                {
                                    if ((_configJob.ExecutionState.Status == RecoveryStatus.Submitted || _configJob.ExecutionState.Status == RecoveryStatus.Waiting) && _configJob.ExecutionState.PercentageExecution == 25)
                                    {
                                        state.Status = RecoveryStatus.Failure;
                                        configRestoreFailed = true;
                                    }
                                    else
                                    {
                                        state.Status = RecoveryStatus.Cancelled;
                                    }
                                }
                                break;
                            case RecoveryOpCodes.EndJob:
                                state.Status = RecoveryStatus.Completed;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        state.Message = ex.Message;
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                            LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.End()", ex.ToString());
                    }
                }
                #endregion

                #region Remote job calls
                CancellationTokenSource cancelToken = new CancellationTokenSource();
                CancellationToken ct = cancelToken.Token;


                List<Task<RecoveryOperationStatus>> runningTasks = new List<Task<RecoveryOperationStatus>>();


                foreach (string db in _infoObject.ActiveConfig.DatabaseMap.Keys)
                {
                    string destination = _infoObject.ActiveConfig.DatabaseMap[db];
                    string databaseName = db;

                    if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                    {
                        databaseName = destination;
                    }

                    List<ShardRecoveryJobState> shardjobs = _infoObject.ExecutionState.Details.Where(x => (x.JobType != RecoveryJobType.ConfigBackup) ||
                                                                (x.JobType != RecoveryJobType.ConfigRestore)).ToList<ShardRecoveryJobState>();

                    foreach (ShardRecoveryJobState job in shardjobs)
                    {
                        RecoveryOperation operation = new RecoveryOperation();
                        operation.JobIdentifer = job.Identifier;
                        Task<RecoveryOperationStatus> task = null;
                        switch (job.JobType)
                        {

                            case RecoveryJobType.DataBackup:
                            case RecoveryJobType.DataRestore:
                                switch (opCode)
                                {
                                    case RecoveryOpCodes.CancelJob:
                                        operation.OpCode = RecoveryOpCodes.CancelJob;
                                        operation.Parameter = databaseName;

                                        if (string.IsNullOrEmpty(shard) || string.IsNullOrWhiteSpace(shard))
                                        {
                                            task = Task.Factory.StartNew(() => SendRemoteCall(job.Node, job.Cluster, job.Shard,
                                                                                                      operation, ct), cancelToken.Token);
                                        }
                                        else
                                        {
                                            if (!shard.Equals(job.Shard))
                                            {
                                                task = Task.Factory.StartNew(() => SendRemoteCall(job.Node, job.Cluster, job.Shard,
                                                                                                      operation, ct), cancelToken.Token);
                                            }
                                            else // explicitly set status of shard as failure
                                            {
                                                foreach (RecoveryJobStateBase internalJob in job.Detail)
                                                {
                                                    if (internalJob.Status != RecoveryStatus.Completed)
                                                    {
                                                        internalJob.Status = RecoveryStatus.Failure;
                                                    }
                                                }
                                                job.Status = RecoveryStatus.Failure;
                                                job.Message = "Membership changed on shard : " + shard;
                                            }
                                        }
                                        break;
                                    case RecoveryOpCodes.EndJob:
                                        operation.OpCode = RecoveryOpCodes.EndJob;
                                        EndOpParams opParams = new EndOpParams(databaseName);

                                        // incase of diff log enabled send diff info as well
                                        if ((_infoObject.ActiveConfig.JobType == RecoveryJobType.FullBackup))
                                        {
                                            DiffTrackObject trackObject = new DiffTrackObject(databaseName, job.Shard);
                                            trackObject.LastFullBackupDate = job.LastFullBackupDate;
                                            trackObject.LastOperationID = job.LastOperationID;

                                            opParams.TrackObjects = new List<DiffTrackObject>() { trackObject };
                                            operation.Parameter = opParams;
                                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                            {
                                                LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.Broadcast()", "submitted for dispose " + job.Shard + " : " + job.Identifier + " : " + job.JobType + " : " + trackObject);
                                            }
                                            task = Task.Factory.StartNew(() => SendRemoteCall(job.Cluster, job.Shard,
                                                                                                       operation, ct), cancelToken.Token);
                                        }
                                        else
                                        {
                                            operation.Parameter = opParams;
                                            task = Task.Factory.StartNew(() => SendRemoteCall(job.Node, job.Cluster, job.Shard,
                                                                                                   operation, ct), cancelToken.Token);
                                        }
                                        break;
                                }
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                {
                                    LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.Broadcast()", "submitted for dispose " + job.Shard + " : " + job.Identifier + " : " + job.JobType);
                                }

                                if (task != null)
                                {
                                    runningTasks.Add(task);
                                }
                                break;

                        }
                    }
                    if (runningTasks.Count > 0)
                    {

                        Task.WaitAll(runningTasks.ToArray<Task<RecoveryOperationStatus>>(), -1, ct);
                        bool success = true;
                        //M_TODO[Critical]: update status in _infoObject.ExecutionState i thinnk
                        foreach (Task<RecoveryOperationStatus> taskState in runningTasks)
                        {
                            if (taskState.Result.Status == RecoveryStatus.Failure)
                            {
                                success = false;
                                state.Message = taskState.Result.Message;

                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.Broadcast()", _infoObject.Identifier + " Task failed to end " + taskState.Result.ToString());
                            }
                        }

                        if (success != false)
                        {

                            state.Status = RecoveryStatus.Success;
                            switch (opCode)
                            {
                                case RecoveryOpCodes.CancelJob:
                                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    {
                                        LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.Broadcast()", _infoObject.Identifier + " : " + _infoObject.ActiveConfig.JobType + " successfully cancelled ");
                                    }

                                    UpdateExecutionStatus(RecoveryStatus.Cancelled);
                                    break;
                                case RecoveryOpCodes.EndJob:
                                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    {
                                        LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.Broadcast()", _infoObject.Identifier + " : " + _infoObject.ActiveConfig.JobType + " successfully completed");
                                    }

                                    UpdateExecutionStatus(RecoveryStatus.Completed);
                                    break;
                            }

                        }
                    }
                }
                #endregion

                if (configRestoreFailed)
                {
                    UpdateExecutionStatus(RecoveryStatus.Failure);
                }

                if (explicitCancel)
                    UpdateExecutionStatus(RecoveryStatus.Cancelled);


            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.End()", ex.ToString());
                if (!explicitCancel)
                    UpdateExecutionStatus(RecoveryStatus.Failure);
                else
                    UpdateExecutionStatus(RecoveryStatus.Cancelled);
            }

            return state;
        }

        private RecoveryOperationStatus CreateRemoteJob(RecoveryConfiguration config, CsBackupableEntities additionalParams)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            state.Message = "Failure during submission state";
            state.JobIdentifier = _infoObject.Identifier;
            try
            {
                // 1. get cluster info
                ClusterInfo clusterInfo = (_communicationHandler as RecoveryManager).GetConfiguredClusters(config.Cluster);
                if (clusterInfo != null)
                {
                    string clusterName = clusterInfo.Name;

                    List<ShardInfo> availableShards = null;
                    if (clusterInfo.ShardInfo != null)
                    {
                        availableShards = new List<ShardInfo>(clusterInfo.ShardInfo.Values.ToList());
                    }
                    else
                    {
                        state.Message = "No shard exists against the given cluster";

                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                            LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateRemoteJob()", state.Message);
                        return state;
                    }

                    if (availableShards == null)
                    {
                        state.Message = "No shard exists against the given cluster";

                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                            LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateRemoteJob()", state.Message);
                        return state;
                    }

                    // 3.create tasks
                    CancellationTokenSource cancelToken = new CancellationTokenSource();
                    CancellationToken ct = cancelToken.Token;
                    List<Task<RecoveryOperationStatus>> runningTasks = new List<Task<RecoveryOperationStatus>>();

                    //____________________________________________________________________
                    //M_TODO: change operation code shard if more than 1 db is requested to backup
                    //____________________________________________________________________
                    Dictionary<string, ShardDifState> difState = new Dictionary<string, ShardDifState>();
                    foreach (ShardInfo shard in availableShards)
                    {

                        RecoveryOperation recoveryOperation = new RecoveryOperation();
                        ServerInfo server = null;
                        // 4. select node to execute based on priority provided, for now preference is on primary
                        if (!config.Cluster.Equals(Common.MiscUtil.LOCAL))
                        {
                            if (config.ExecutionPreference == Common.Enum.ExecutionPreference.Primary)
                            {
                                server = shard.Primary;
                                if (server != null)
                                    server.Status = Common.Toplogies.Impl.ShardImpl.Status.Running;
                            }
                        }
                        else
                            server = shard.RunningNodes.Values.First();

                        if (server != null && server.Status == Common.Toplogies.Impl.ShardImpl.Status.Running)
                        {
                            Dictionary<string, Dictionary<string, string[]>> _dbList = new Dictionary<string, Dictionary<string, string[]>>();

                            // 5. Create book keeping objects against a job
                            foreach (string db in _infoObject.ActiveConfig.DatabaseMap.Keys)
                            {
                                //create operation
                                recoveryOperation.JobIdentifer = config.Identifier;
                                recoveryOperation.OpCode = config.Operation;

                                RecoveryJobType jobType = RecoveryJobType.DataBackup;
                                switch (config.JobType)
                                {
                                    #region backup
                                    case RecoveryJobType.FullBackup:
                                    case RecoveryJobType.DataBackup:
                                        jobType = RecoveryJobType.DataBackup;

                                        if (additionalParams.Database != null && additionalParams.Database.Count > 0)
                                        {
                                            DatabaseConfiguration dbConfig = additionalParams.Database[db];

                                            if (dbConfig != null)
                                            {
                                                Dictionary<string, string[]> inner = new Dictionary<string, string[]>();

                                                inner.Add(db, dbConfig.Storage.Collections.Configuration.Keys.ToArray());
                                                _dbList.Add(db, inner);
                                            }
                                        }

                                        SubmitBackupOpParams sbparam = new SubmitBackupOpParams();
                                        sbparam.PersistenceConfiguration = new RecoveryPersistenceConfiguration();
                                        sbparam.PersistenceConfiguration.UserName = config.UserName;
                                        sbparam.PersistenceConfiguration.Password = config.Password;

                                        string folderName = @"\" + RecoveryFolderStructure.SHARD_BACKUP_FOLDER + shard.Name + @"\";


                                        sbparam.PersistenceConfiguration.FilePath = Path.Combine(config.RecoveryPath, _infoObject.RootFolderName + folderName);
                                        sbparam.PersistenceConfiguration.DbCollectionMap = _dbList;
                                        sbparam.PersistenceConfiguration.DatabaseName = db;
                                        sbparam.PersistenceConfiguration.Cluster = ActiveConfig.Cluster;
                                        recoveryOperation.Parameter = sbparam;
                                        break;

                                    #endregion

                                    #region Restore
                                    case RecoveryJobType.DataRestore:
                                    case RecoveryJobType.Restore:
                                        jobType = RecoveryJobType.DataRestore;

                                        if (additionalParams.Database != null && additionalParams.Database.Count > 0)
                                        {
                                            string destination = _infoObject.ActiveConfig.DatabaseMap[db];
                                            string databaseName = db;

                                            if (!string.IsNullOrEmpty(destination) && !string.IsNullOrWhiteSpace(destination))
                                            {
                                                databaseName = destination;
                                            }
                                            DatabaseConfiguration dbConfig = null;

                                            // hack key not found should check why this happens
                                            if (additionalParams.Database.ContainsKey(databaseName))
                                            {
                                                dbConfig = additionalParams.Database[databaseName];
                                            }
                                            else if (additionalParams.Database.ContainsKey(db))
                                            {
                                                dbConfig = additionalParams.Database[db];
                                            }

                                            if (dbConfig != null)
                                            {
                                                Dictionary<string, string[]> inner = new Dictionary<string, string[]>();
                                                inner.Add(databaseName, dbConfig.Storage.Collections.Configuration.Keys.ToArray());
                                                _dbList.Add(db, inner);
                                            }
                                        }

                                        SubmitRestoreOpParams resParam = new SubmitRestoreOpParams();
                                        resParam.PersistenceConfiguration = new RecoveryPersistenceConfiguration();
                                        resParam.PersistenceConfiguration.UserName = config.UserName;
                                        resParam.PersistenceConfiguration.Password = config.Password;
                                        //TODO: find method to accomodate diff common files aswell
                                        folderName = RecoveryFolderStructure.SHARD_BACKUP_FOLDER + shard.Name;
                                        resParam.PersistenceConfiguration.FilePath = Path.Combine(config.RecoveryPath, folderName);
                                        resParam.PersistenceConfiguration.DbCollectionMap = _dbList;
                                        recoveryOperation.Parameter = resParam;
                                        break;
                                    #endregion
                                }
                                ShardRecoveryJobState _jobTracker = new ShardRecoveryJobState(config.Identifier, shard.Name, server.Address.ip, clusterName, jobType);

                                //_infoObject.ShardResponseMap = difState;
                                lock (_mutex)
                                {
                                    _infoObject.ExecutionState.UpdateJobStatus(_jobTracker);
                                }
                            }

                            // 6. task to send message to the particular shard
                            Task<RecoveryOperationStatus> _task = Task.Factory.StartNew(() => SendRemoteCall(server.Address.ip, clusterName, shard.Name, recoveryOperation, ct), cancelToken.Token);

                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.CreateRemoteJob()", recoveryOperation.JobIdentifer + " : " + _infoObject.ActiveConfig.JobType + " Task sent to submitted at " + server.Address.ip + " : " + clusterName + " : " + shard.Name);

                            runningTasks.Add(_task);
                        }
                        else
                        {
                            state.Message = "No primary available for " + shard.Name;

                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateRemoteJob()", state.Message);
                            return state;
                        }


                    }

                    // wait for execution
                    Task.WaitAll(runningTasks.ToArray<Task<RecoveryOperationStatus>>(), timeout, ct);
                    // take care of this case, this will check if all task statuses is true only then set status as submitted
                    foreach (Task<RecoveryOperationStatus> _taskState in runningTasks)
                    {
                        if (_taskState.Result.Status == RecoveryStatus.Submitted)
                        {
                            state.Status = RecoveryStatus.Submitted;
                            state.Message = "Job submitted at shards";
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.CreateRemoteJob()", _infoObject.Identifier + " : " + _infoObject.ActiveConfig.JobType + " Task SuccessFully submitted at " + _taskState.Result.ToString());
                        }
                        else if (_taskState.Result.Status == RecoveryStatus.Failure)
                        {
                            state.Status = RecoveryStatus.Failure;
                            state.Message = _taskState.Result.Message;
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("ClusteredJob.CreateRemoteJob()", _infoObject.Identifier + " : " + _infoObject.ActiveConfig.JobType + " Task failed to be submitted at " + _taskState.Result.ToString());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusteredJob.CreateRemoteJob()", exp.ToString());
                state.Message = exp.Message;
            }

            return state;
        }

        private RecoveryOperationStatus SendRemoteCall(string serverNode, string cluster, string shard, RecoveryOperation operation, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
                state.JobIdentifier = _infoObject.Identifier;
                state.Message = "Failure during submission state";
                return state;
            }
            return _communicationHandler.SubmitRecoveryJob(serverNode, cluster, shard, operation);
        }

        private RecoveryOperationStatus SendRemoteCall(string cluster, string shard, RecoveryOperation operation, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
                state.JobIdentifier = _infoObject.Identifier;
                state.Message = "Failure during submission state";
                return state;
            }
            return _communicationHandler.SubmitRecoveryJob(cluster, shard, operation);
        }
        // checks for any exceptions in running task, not used anymore
        private bool WaitAll(Task[] tasks, int timeout, CancellationToken token)
        {
            try
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

                foreach (var task in tasks)
                {
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            // log this exception
                            cts.Cancel();
                        }
                    },
                    cts.Token,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current);
                }

                return Task.WaitAll(tasks, timeout, cts.Token);
            }
            catch (Exception exp)
            {
                //Console.WriteLine(exp.ToString());
                return false;
            }
        }

        private void UpdateExecutionStatus(RecoveryStatus status)
        {
            lock (_mutex)
            {
                switch (status)
                {
                    case RecoveryStatus.Cancelled:
                    case RecoveryStatus.Completed:
                    case RecoveryStatus.Failure:
                        _infoObject.ExecutionState.StopTime = DateTime.Now;
                        break;
                }
                //_infoObject.ExecutionState.Details.ForEach(s => s.Status = status);
                foreach (ShardRecoveryJobState job in _infoObject.ExecutionState.Details)
                {
                    if (_configJob != null && job.Equals(_configJob.ExecutionState))
                    {
                        if (_configJob.ExecutionState.Status != RecoveryStatus.Completed)
                            job.Status = status;
                    }
                    else
                    {
                        job.Status = status;
                    }
                }
            }
        }

        private void CheckJobState()
        {
            if (_infoObject != null)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ClusterRecoveryJob.CheckJobState() ", _infoObject.ExecutionState.ToString());
                switch (_infoObject.ExecutionState.Status)
                {
                    case RecoveryStatus.Failure:
                    case RecoveryStatus.Cancelled:
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("ClusterRecoveryJob.CheckJobState() ", "Cancel call on status failure");

                        if (_infoObject.ExecutionState.Status == RecoveryStatus.Failure)
                            Cancel(null);
                        else
                            Cancel(null, folderState: RecoveryFileState.Cancelled);
                        // remove job from active config
                        (_operationHandler as RecoveryManager).RemoveRunningJob(_infoObject.ActiveConfig.Identifier);
                        break;
                    case RecoveryStatus.Completed:// check this method
                        if ((_infoObject.ActiveConfig.JobType != RecoveryJobType.Restore) || _infoObject.ExecutionState.Details.Count > 1)
                        {

                            RecoveryOperationStatus endState = End(null);
                        }
                        else
                        {
                            if (_configJob != null)
                            {
                                if (_configJob.ExecutionState.Status == RecoveryStatus.Completed)
                                {
                                    if (_configJob.ExecutionState.JobType == RecoveryJobType.ConfigBackup)
                                    {
                                        _persistenceManager.IsJobActive = false;

                                        _persistenceManager.CloseBackupFile(RecoveryFolderStructure.CONFIG_SERVER, RecoveryFileState.Completed);
                                    }
                                    try
                                    {
                                        _configJob.Dispose();
                                    }
                                    catch (ThreadAbortException)
                                    {
                                        Thread.ResetAbort(); // ignore it
                                    }
                                }
                            }
                            else// diff Restore call
                            {

                            }
                        }
                        //method to return status and 
                        break;
                    case RecoveryStatus.Executing:
                    case RecoveryStatus.uninitiated:
                    case RecoveryStatus.Submitted:
                        if (_configJob != null)
                        {
                            if (_configJob.ExecutionState.Status == RecoveryStatus.Completed || _configJob.ExecutionState.Status == RecoveryStatus.Submitted)
                            {
                                if (_configJob.ExecutionState.JobType == RecoveryJobType.ConfigBackup)
                                {
                                    _persistenceManager.IsJobActive = false;
                                    _persistenceManager.CloseBackupFile(RecoveryFolderStructure.CONFIG_SERVER, RecoveryFileState.Completed);
                                }

                                try
                                {
                                    _configJob.Stop();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort(); // ignore it
                                }
                            }
                        }
                        break;
                }
            }
        }

        //initiates shardRestore jobs against, a configuration that has been restored
        public void SubmitConfigChanged(object changeConfig)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Success);
            state.Message = "Failure during submission state";
            state.JobIdentifier = _infoObject.Identifier;

            try
            {
                RecoveryConfiguration config = _infoObject.ActiveConfig.Clone() as RecoveryConfiguration;
                config.Operation = RecoveryOpCodes.SubmitRestoreJob;

                if (ActiveConfig.JobType != RecoveryJobType.ConfigRestore)
                {
                    //add check for individual calls
                    if (changeConfig is CsBackupableEntities)
                    {
                        state = CreateRemoteJob(config, (CsBackupableEntities)changeConfig);

                        if (state.Status != RecoveryStatus.Failure)
                        {
                            state = Start(config);
                        }
                    }

                }
                if (state.Status == RecoveryStatus.Failure)
                {
                    Cancel(config);
                    // remove job from active config
                    (_operationHandler as RecoveryManager).RemoveRunningJob(_infoObject.ActiveConfig.Identifier);
                }
                else
                {
                    // submit  config job status only if it has completely restored
                    _configJob.ExecutionState.Status = RecoveryStatus.Completed;
                    _configJob.ExecutionState.PercentageExecution = 100;//[M_NOTE] rudementary logic, change this 
                    _configJob.ExecutionState.MessageTime = DateTime.Now;
                    _configJob.ExecutionState.Message = "Configuration Restored";

                    SubmitRecoveryState(_configJob.ExecutionState);
                }

            }
            catch (Exception ex)
            {
                state.Message = ex.Message;
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ClusterRecoveryJob.SubmitConfigChanged()", ex.ToString());
            }
        }
        #endregion

        #region IConfigOperationExecutor
        public void Restore(CsBackupableEntities entity, Dictionary<string, string> database, string cluster)
        {
            _operationHandler.Restore(entity, database, cluster);
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            try
            {
                try
                {
                    _configJob.Dispose();
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                _communicationHandler = null;
                _persistenceManager.Dispose();
                _operationHandler = null;
                _infoObject.Dispose();
            }
            catch (Exception ex)

            { }

        }
        #endregion



    }

}

