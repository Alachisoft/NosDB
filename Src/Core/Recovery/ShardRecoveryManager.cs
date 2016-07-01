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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Replication;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Recovery.Persistence;
using Alachisoft.NosDB.Core.Recovery.RecoveryJobs;
using Alachisoft.NosDB.Core.Storage;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Toplogies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Recovery
{
    internal class ShardRecoveryManager : IJobProgressHandler, IDisposable
    {
        //TODO:
        // register collection with oplog somehow??
        // set diff DB with all end values
        //register trigger with oplog file aswell
        private Dictionary<string, JobInfoObject> _databaseJobMap = null;
        private NodeContext _context;
        private RecoveryJobBase _currentJob = null;
        private object _mutex = new object();


        internal ShardRecoveryManager(NodeContext context)
        {
            if (context != null)
            {
                _context = context;
                _databaseJobMap = new Dictionary<String, JobInfoObject>();
            }
        }



        #region Operation Received
        internal RecoveryOperationStatus RecoveryOperationReceived(RecoveryOperation opContext)
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
            status.JobIdentifier = opContext.JobIdentifer;
            try
            {
                switch (opContext.OpCode)
                {
                    case RecoveryOpCodes.SubmitBackupJob:
                    case RecoveryOpCodes.SubmitRestoreJob:

                        if (ValidatePrequisites(opContext).Status == RecoveryStatus.Success)
                        {
                            status.Status = RecoveryStatus.Submitted;
                            status.Message = "Submitted successfuly";
                            SubmitRecoveryJob(opContext);
                        }
                        else
                            status.Message = "Failed to submit task on node";
                        break;
                    case RecoveryOpCodes.StartJob:
                        status = StartRecoveryJob(opContext);
                        break;
                    case RecoveryOpCodes.EndJob:
                    case RecoveryOpCodes.CancelJob:
                        status = CancelRecoveryJob(opContext);
                        break;
                    case RecoveryOpCodes.PauseJob:
                        status = PauseRecoveryJob(opContext);
                        break;
                    case RecoveryOpCodes.ResumeJob:
                        status = ResumeRecoveryJob(opContext);
                        break;
                    case RecoveryOpCodes.CancelAllJobs:
                        status = CancelAllJobs();
                        break;
                    case RecoveryOpCodes.SubmitShardBackupJob:
                    case RecoveryOpCodes.SubmitShardRecoveryJob:
                        if (ValidatePrequisites(opContext).Status == RecoveryStatus.Success)
                        {
                            status.Status = RecoveryStatus.Submitted;
                            status.Message = "Submitted successfuly";
                            SubmitShardRecoveryJob(opContext);
                        }
                        else
                            status.Message = "Failed to submit task on node";
                        break;

                    case RecoveryOpCodes.GetJobStatistics:
                        status = GetJobStatistics(opContext);
                        break;

                }
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.Receive()", exp.ToString());
            }
            return status;
        }


        #endregion

        #region IJobProgressHandler
        public void SubmitRecoveryState(object state)
        {
            if (state != null)
            {
                RecoveryJobStateBase jobState = (RecoveryJobStateBase)state;
                try
                {

                    if (_databaseJobMap.ContainsKey(jobState.Identifier))
                    {

                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.SubmitState()", jobState.Identifier + " : " + jobState.Name + " : " + jobState.PercentageExecution
                                + " : " + jobState.Name + " : " + jobState.MessageTime + " : " + jobState.Status);
                        lock (_mutex)
                        {
                            //update status
                            _databaseJobMap[jobState.Identifier].ExecutionState.UpdateEntityState(jobState);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.SubmitState()", jobState.Identifier + " : " + ex.ToString());
                }
                finally
                {
                    CheckJobState(jobState.Identifier, false);
                }
            }
        }
        #endregion

        #region Internal Methods
        private void SubmitShardRecoveryJob(RecoveryOperation opContext)
        {

            if (opContext != null)
            {
                if (!_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                {
                    // incase of shard level job i.e one with multiple databases
                    //1. create a separate jobInfoObject for each individual db job
                    //2. store it in a separate hashtable or may be edit the existing one
                    //3. run as per request by 
                }
            }

        }

        private void SubmitRecoveryJob(RecoveryOperation opContext)
        {
            if (opContext != null)
            {
                try
                {
                    if (!_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        JobInfoObject infoObject = null;

                        switch (opContext.OpCode)
                        {
                            case RecoveryOpCodes.SubmitBackupJob:
                                infoObject = CreateBackupJob(opContext);
                                break;
                            case RecoveryOpCodes.SubmitRestoreJob:
                                infoObject = CreateRestoreJob(opContext);
                                break;

                        }

                        if (infoObject != null)
                        {
                            _databaseJobMap.Add(opContext.JobIdentifer, infoObject);
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.SubmitRecoveryJob()", opContext.JobIdentifer + "_" + opContext.OpCode + " Submitted");
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.SubmitRecoveryJob()", opContext.JobIdentifer + "_" + opContext.OpCode + " : " + exp.ToString());

                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Status = RecoveryStatus.Failure;
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Message = "Failure during submission phase_" + opContext.OpCode;

                        CheckJobState(opContext.JobIdentifer, true);
                    }
                }
            }

        }

        private RecoveryOperationStatus StartRecoveryJob(RecoveryOperation opContext)
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Executing);
            status.JobIdentifier = opContext.JobIdentifer;
            status.Message = "Recovery Job has successfully started";
            if (opContext != null)
            {
                try
                {
                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        // initialize only data job will be executed,oplog job will start only after datajob has been executed completely

                        RecoveryJobBase job = _databaseJobMap[opContext.JobIdentifer].JobList.Where(x => (x.JobType == RecoveryJobType.DataBackup) ||
                            (x.JobType == RecoveryJobType.DataRestore)).First();


                        job.Start();

                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.StartRecoveryJob()", opContext.JobIdentifer + "_" + _databaseJobMap[opContext.JobIdentifer].JobType + " Started");

                        _databaseJobMap[opContext.JobIdentifer].RecoveryPersistenceManager.IsJobActive = true;
                    }
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.StartRecoveryJob()", opContext.JobIdentifer + "_" + _databaseJobMap[opContext.JobIdentifer].ExecutionState.JobType + " : " + exp.ToString());

                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Status = RecoveryStatus.Failure;
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Message = "Failure during Starting phase";

                        CheckJobState(opContext.JobIdentifer, true);
                    }
                }
            }
            return status;
        }

        private RecoveryOperationStatus CancelRecoveryJob(RecoveryOperation opContext)
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Success);
            status.JobIdentifier = opContext.JobIdentifer;

            if (opContext != null)
            {
                try
                {


                    #region End/Cancel job
                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        List<RecoveryJobBase> jobList = _databaseJobMap[opContext.JobIdentifer].JobList;

                        foreach (RecoveryJobBase job in jobList)
                        {
                            if (opContext.OpCode == RecoveryOpCodes.CancelJob)
                            {
                                switch (job.JobType)
                                {
                                    case RecoveryJobType.DataBackup:
                                        _databaseJobMap[opContext.JobIdentifer].RecoveryPersistenceManager.CloseBackupFile(job.Database, RecoveryFileState.Cancelled);
                                        break;

                                    //NOTE: diffbackup job is ignored under the assumption oplog file will close the file for it
                                }
                            }
                            //if (job.JobType != RecoveryJobType.DifferentialRestore)
                            {
                                try
                                {
                                    if (job.State != ThreadState.Unstarted && job.State != ThreadState.Stopped && job.State != ThreadState.WaitSleepJoin)
                                        job.Stop();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort();
                                }
                                catch (Exception exp)
                                { }

                                try
                                {

                                    job.Dispose();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort();
                                }
                                catch (Exception exp)
                                { }

                            }
                            //else
                            //{
                            //    _databaseJobMap[opContext.JobIdentifer].RecoveryPersistenceManager.IsJobActive = false;
                            //    _databaseJobMap[opContext.JobIdentifer].RecoveryPersistenceManager.Dispose();
                            //}
                        }
                        #region rename folder
                        JobInfoObject infoObj = _databaseJobMap[opContext.JobIdentifer];
                        if (infoObj.JobType == RecoveryJobType.DataBackup)
                        {
                            switch (opContext.OpCode)
                            {
                                case RecoveryOpCodes.CancelJob:
                                    infoObj.RenameRootFolder(RecoveryFileState.Cancelled);
                                    break;
                            }
                        }
                        #endregion
                        // removing job from databaseJobMap
                        _databaseJobMap[opContext.JobIdentifer].RecoveryPersistenceManager.IsJobActive = false;
                        _databaseJobMap[opContext.JobIdentifer].Dispose();
                        _databaseJobMap.Remove(opContext.JobIdentifer);
                    }
                    #endregion
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.CancelRecoveryJob()", opContext.JobIdentifer + "_" + _databaseJobMap[opContext.JobIdentifer].ExecutionState.JobType + " : " + exp.ToString());

                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Status = RecoveryStatus.Failure;
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Message = "Failure during Cancel phase";

                        // commented because the config server will not have any job handler to recieve this message
                        //CheckJobState(opContext.JobIdentifer, true);
                    }
                    status.Status = RecoveryStatus.Failure;
                    status.Message = exp.ToString();
                }
            }
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.CancelRecoveryJob()", status.ToString());

            return status;
        }

        private RecoveryOperationStatus PauseRecoveryJob(RecoveryOperation opContext)
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
            status.JobIdentifier = opContext.JobIdentifer;
            if (opContext != null)
            {
                if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                {
                    List<RecoveryJobBase> _jobList = _databaseJobMap[opContext.JobIdentifer].JobList;
                    if (_jobList.Count == 1)
                    {
                        _jobList[0].Pause();
                    }
                    else
                    {
                        // M_TODO: complete sharded backup  
                    }
                }
            }
            return status;
        }

        private RecoveryOperationStatus ResumeRecoveryJob(RecoveryOperation opContext)
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Failure);
            status.JobIdentifier = opContext.JobIdentifer;
            if (opContext != null)
            {
                if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                {
                    List<RecoveryJobBase> _jobList = _databaseJobMap[opContext.JobIdentifer].JobList;
                    if (_jobList.Count == 1)
                    {
                        _jobList[0].Resume();
                    }
                    else
                    {
                        // M_TODO: complete sharded backup  
                    }
                }
            }
            return status;

        }

        private RecoveryOperationStatus CancelAllJobs()
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Success);

            foreach (RecoveryJobBase job in _databaseJobMap.Values.SelectMany(x => x.JobList))
            {
                try
                {
                    status.JobIdentifier = job.JobIdentifier;
                    try
                    {
                        job.Stop();
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                    }

                    switch (job.JobType)
                    {
                        case RecoveryJobType.DataBackup:
                        case RecoveryJobType.DataRestore:
                            _databaseJobMap[job.JobIdentifier].RecoveryPersistenceManager.CloseBackupFile(job.Database, RecoveryFileState.Cancelled);
                            break;

                    }
                    _databaseJobMap[job.JobIdentifier].RecoveryPersistenceManager.IsJobActive = false;
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.CancelAllRecoveryJob()", job.JobIdentifier + " : " + exp.ToString());

                    if (_databaseJobMap.ContainsKey(job.JobIdentifier))
                    {
                        _databaseJobMap[job.JobIdentifier].ExecutionState.Status = RecoveryStatus.Failure;
                        _databaseJobMap[job.JobIdentifier].ExecutionState.Message = "Failure during Cancel phase";

                        CheckJobState(job.JobIdentifier, true);
                    }
                }

            }
            return status;
        }

        private RecoveryOperationStatus GetJobStatistics(RecoveryOperation opContext)
        {
            RecoveryOperationStatus status = new RecoveryOperationStatus(RecoveryStatus.Success);
            status.JobIdentifier = opContext.JobIdentifer;
            if (opContext != null)
            {
                try
                {
                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        foreach (RecoveryJobBase job in _databaseJobMap[opContext.JobIdentifer].JobList)
                        {
                            // query latest job status 
                            _databaseJobMap[opContext.JobIdentifer].ExecutionState.UpdateEntityState((RecoveryJobStateBase)job.JobStatistics());// job statistics returning mechanism
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.GetStatistics()", opContext.JobIdentifer + "_" + _databaseJobMap[opContext.JobIdentifer].ExecutionState.JobType + " : " + ex.ToString());
                    if (_databaseJobMap.ContainsKey(opContext.JobIdentifer))
                    {
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Status = RecoveryStatus.Failure;
                        _databaseJobMap[opContext.JobIdentifer].ExecutionState.Message = "Failure during get statistics call";
                    }
                }
                finally
                {
                    CheckJobState(opContext.JobIdentifer, true);
                }
            }
            return status;
        }

        #endregion

        #region Submittion helper methods

        private JobInfoObject CreateBackupJob(RecoveryOperation opContext)
        {
            JobInfoObject infoObject = null;
            RecoveryJobBase dataJob = null;
            RecoveryJobBase opLogJob = null;

            #region BackupJob
            SubmitBackupOpParams bckpParam = (SubmitBackupOpParams)opContext.Parameter;

            // create handler object
            infoObject = new JobInfoObject(opContext.JobIdentifer, _context.LocalShardName, _context.LocalAddress.ip, _context.ClusterName,
                RecoveryJobType.DataBackup, bckpParam.PersistenceConfiguration.FilePath);

            foreach (string db in bckpParam.PersistenceConfiguration.DbCollectionMap.Keys)
            {
                //assuming one database is  sent
                dataJob = new DatabaseBackupJob(opContext.JobIdentifer, _context, db, bckpParam.PersistenceConfiguration.DbCollectionMap[db][db].ToList(), infoObject.RecoveryPersistenceManager, _context.ClusterName);
                dataJob.RegisterProgressHandler(this);
                infoObject.AddJob(dataJob);

                // set persistence configuration
                if (bckpParam.PersistenceConfiguration.FileName == null)
                {
                    bckpParam.PersistenceConfiguration.FileName = new List<string>();
                    bckpParam.PersistenceConfiguration.FileName.Add(db);//add name of all databases for shard level job
                }
                infoObject.RecoveryPersistenceManager.SetJobConfiguration(RecoveryJobType.DataBackup, bckpParam.PersistenceConfiguration, db, 1);


            }
            #endregion

            return infoObject;
        }

        //M_TODO: must Specify DB name in case of shard job, so a separate job is created, for now this will run multiple times and override job if more
        //than one db is specified. 
        private JobInfoObject CreateRestoreJob(RecoveryOperation opContext)
        {
            JobInfoObject infoObject = null;
            RecoveryJobBase dataJob = null;


            #region RestoreJob
            SubmitRestoreOpParams resParam = (SubmitRestoreOpParams)opContext.Parameter;

            foreach (string db in resParam.PersistenceConfiguration.DbCollectionMap.Keys)
            {
                // create handler object            
                // Note: this is kept inside the loop with the assumption that a seperate info object is to be kept for each database in complete cluster job
                infoObject = new JobInfoObject(opContext.JobIdentifer, _context.LocalShardName, _context.LocalAddress.ip, _context.ClusterName,
                        RecoveryJobType.DataRestore, resParam.PersistenceConfiguration.FilePath);

                string destination = resParam.PersistenceConfiguration.DbCollectionMap[db].Keys.First();

                // create DataJob
                dataJob = new DatabaseRestoreJob(opContext.JobIdentifer, _context, destination, resParam.PersistenceConfiguration.DbCollectionMap[db][destination].ToList<string>(), infoObject.RecoveryPersistenceManager, _context.ClusterName);
                dataJob.RegisterProgressHandler(this);
                infoObject.AddJob(dataJob);

                // set persistence configuration
                if (resParam.PersistenceConfiguration.FileName == null)
                {
                    resParam.PersistenceConfiguration.FileName = new List<string>();
                    resParam.PersistenceConfiguration.FileName.Add(db);//add name of all databases for shard level job
                }

                infoObject.RecoveryPersistenceManager.SetJobConfiguration(RecoveryJobType.DataRestore, resParam.PersistenceConfiguration, db, 1);

            }
            #endregion

            return infoObject;
        }


        #endregion

        #region helper methods
        private RecoveryOperationStatus ValidatePrequisites(RecoveryOperation opContext)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Success);
            state.JobIdentifier = opContext.JobIdentifer;
            try
            {
                // must check state transfer 
                //if (_context.StatusLatch.IsAnyBitsSet(BucketStatus.UnderStateTxfr))
                //{
                //    state.Status = RecoveryStatus.Success;//M_TODO: change this to false, once this state transfer code is complete
                //    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                //        LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.Validate()", opContext.JobIdentifer + "  failed because node is in state transfer");
                //}

                if (state.Status == RecoveryStatus.Success)
                {
                    string path = string.Empty;
                    string username = "";
                    string password = "";
                    Dictionary<string, Dictionary<string, string[]>> dbMap = new Dictionary<string, Dictionary<string, string[]>>();
                    switch (opContext.OpCode)
                    {
                        case RecoveryOpCodes.SubmitBackupJob:
                            SubmitBackupOpParams _bckpParam = (SubmitBackupOpParams)opContext.Parameter;
                            path = _bckpParam.PersistenceConfiguration.FilePath;
                            dbMap = _bckpParam.PersistenceConfiguration.DbCollectionMap;
                            username = _bckpParam.PersistenceConfiguration.UserName;
                            password = _bckpParam.PersistenceConfiguration.Password;
                            break;
                        case RecoveryOpCodes.SubmitRestoreJob:
                            SubmitRestoreOpParams _resParam = (SubmitRestoreOpParams)opContext.Parameter;

                            path = Path.Combine(_resParam.PersistenceConfiguration.FilePath);
                            username = _resParam.PersistenceConfiguration.UserName;
                            password = _resParam.PersistenceConfiguration.Password;
                            dbMap = _resParam.PersistenceConfiguration.DbCollectionMap;
                            List<string> shardNameList = new List<string>();
                            shardNameList.Add(_context.LocalShardName);
                            Impersonation impersonation = null;
                            bool isSharedPath = RecoveryFolderStructure.PathIsNetworkPath(path);
                            if (isSharedPath)
                            {
                                impersonation = new Impersonation(username, password);
                            }

                            state = RecoveryFolderStructure.ValidateFolderStructure(path, RecoveryJobType.DataRestore, false, shardNameList);

                            if (isSharedPath)
                                impersonation.Dispose();

                            if (state.Status == RecoveryStatus.Failure)
                            {
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.Validate()", state.Message);
                                return state;
                            }
                            else
                            {
                                #region validate files
                                string folderName = string.Empty;
                                string fileName = dbMap.First().Key;
                                string folderPath = Path.Combine(path, folderName);
                                string filePath = Path.Combine(folderPath, fileName);

                                isSharedPath = RecoveryFolderStructure.PathIsNetworkPath(folderPath);
                                bool fileExists = false;
                                if (isSharedPath)
                                {
                                    BackupFile file = new BackupFile(fileName, folderPath, null, null);
                                    impersonation = new Impersonation(username, password);
                                    fileExists = File.Exists(filePath);
                                    impersonation.Dispose();
                                }
                                else
                                {
                                    fileExists = File.Exists(filePath);
                                }

                                if (fileExists)
                                {
                                    BackupFile file = new BackupFile(fileName, folderPath, username, password);


                                    Alachisoft.NosDB.Core.Recovery.Persistence.BackupFile.Header header = file.ReadFileHeader();

                                    if (!header.Database.ToLower().Equals(dbMap.First().Key))
                                    {
                                        state.Status = RecoveryStatus.Failure;
                                        state.Message = "Provided file does not contain the source database " + fileName;
                                        file.Close();
                                        return state;
                                    }
                                    file.Close();
                                }
                                else
                                {
                                    state.Status = RecoveryStatus.Failure;
                                    state.Message = "No file exists in the given folder";
                                    return state;
                                }

                                #endregion
                            }

                            break;
                    }

                    // this will only be false for backup 
                    if (!Directory.Exists(path))
                    {
                        try
                        {
                            Impersonation impersonation = null;
                            if (RecoveryFolderStructure.PathIsNetworkPath(path))
                                impersonation = new Impersonation(username, password);

                            Directory.CreateDirectory(path);

                            if (dbMap.Count > 0)
                            {
                                // check space for backup job
                                long size = 0;
                                foreach (string db in dbMap.Keys)
                                {
                                    DatabaseStore database = _context.DatabasesManager.GetDatabase(db) as DatabaseStore;
                                    size = database.Size;
                                }

                                ulong availableFreeSpace = Util.DirectoryUtil.GetDiskFreeSpace(path);
                                ulong requiredSize = (ulong)size;
                                if (availableFreeSpace > requiredSize)
                                {
                                    state.Status = RecoveryStatus.Success;
                                }
                                else
                                {
                                    state.Status = RecoveryStatus.Failure;
                                    state.Message = "Available memory is less than the required memory for backup";
                                    return state;
                                }
                            }
                            if (impersonation != null)
                                impersonation.Dispose();
                        }
                        catch (Exception ex)
                        {
                            state.Status = RecoveryStatus.Failure;
                            state.Message = ex.Message;
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.Validate()", ex.ToString());


                        }
                    }

                }

            }
            catch (Exception exp)
            {
                state.Status = RecoveryStatus.Failure;
                state.Message = exp.ToString();
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.Validate()", opContext.JobIdentifer + " : " + exp.ToString());
            }
            return state;
        }

        /// <summary>
        /// Manages job threads against any submission of status
        /// </summary>
        /// <param name="id"></param>
        private void CheckJobState(string id, bool ensureSend)
        {
            if (_databaseJobMap.ContainsKey(id))
            {
                // send status to config server
                switch (_databaseJobMap[id].ExecutionState.Status)
                {
                    case RecoveryStatus.Failure:
                    case RecoveryStatus.Completed:
                    case RecoveryStatus.Cancelled:
                        foreach (RecoveryJobBase job in _databaseJobMap[id].JobList)
                        {
                            try
                            {

                                // close all opened backup files
                                switch (job.JobType)
                                {
                                    case RecoveryJobType.DataBackup:
                                        if (_databaseJobMap[id].ExecutionState.Status == RecoveryStatus.Cancelled)
                                            _databaseJobMap[job.JobIdentifier].RecoveryPersistenceManager.CloseBackupFile(job.Database, RecoveryFileState.Cancelled);
                                        else if (_databaseJobMap[id].ExecutionState.Status == RecoveryStatus.Failure)
                                            _databaseJobMap[job.JobIdentifier].RecoveryPersistenceManager.CloseBackupFile(job.Database, RecoveryFileState.Failed);
                                        else if (_databaseJobMap[id].ExecutionState.Status == RecoveryStatus.Completed)
                                            _databaseJobMap[job.JobIdentifier].RecoveryPersistenceManager.CloseBackupFile(job.Database, RecoveryFileState.Completed);
                                        break;


                                    //ASSUMPTION:commented out under the assumption that during diffbackup oplog and dif job will create same file,
                                    //so only oplog job will close the file.
                                    //
                                    //case RecoveryJobType.DifferentialBackup:
                                    //    _databaseJobMap[_job.JobIdentifier].RecoveryPersistenceManager.CloseBackupFile(RecoveryFileNames.Diflog);
                                    //    break;
                                    //
                                }

                                try
                                {
                                    if (job.State != ThreadState.Unstarted && job.State != ThreadState.Stopped)
                                        job.Stop();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort(); // ignore it
                                }
                                try
                                {
                                    job.Dispose();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort(); // ignore it
                                }
                                try
                                {
                                    #region rename folder
                                    JobInfoObject infoObj = _databaseJobMap[job.JobIdentifier];
                                    switch (job.JobType)
                                    {

                                        case RecoveryJobType.DataBackup:
                                            if (infoObj.JobType == RecoveryJobType.DataBackup)
                                            {
                                                if (_databaseJobMap[id].ExecutionState.Status == RecoveryStatus.Failure)
                                                {
                                                    infoObj.RenameRootFolder(RecoveryFileState.Failed);
                                                }
                                                else if (_databaseJobMap[id].ExecutionState.Status == RecoveryStatus.Cancelled)
                                                {
                                                    infoObj.RenameRootFolder(RecoveryFileState.Cancelled);
                                                }
                                                else
                                                    infoObj.RenameRootFolder(RecoveryFileState.Completed);
                                            }
                                            break;

                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                        LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.CheckJobState()", id + " : " + job.JobType + " : Renaming Folder  : " + ex.ToString());
                                }
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.CheckJobState()", id + " : " + job.JobType + " : End");

                            }
                            catch (Exception exp)
                            {
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.CheckJobState()", id + " : " + job.JobType + " : " + exp.ToString());
                            }
                        }
                        _databaseJobMap[id].ExecutionState.StopTime = DateTime.Now;
                        _databaseJobMap[id].RecoveryPersistenceManager.SharedQueue.CompleteAdding();
                        _databaseJobMap[id].RecoveryPersistenceManager.IsJobActive = false;

                        _databaseJobMap[id].ExecutionState.MessageTime = DateTime.Now;
                        _context.ConfigurationSession.SubmitShardJobStatus(_databaseJobMap[id].ExecutionState);
                        break;
                    case RecoveryStatus.Executing:
                    case RecoveryStatus.uninitiated:
                    case RecoveryStatus.Waiting:
                        if (ensureSend)
                        {
                            _databaseJobMap[id].ExecutionState.MessageTime = DateTime.Now;

                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.SubmitState()", _databaseJobMap[id].ExecutionState.ToString());

                            _context.ConfigurationSession.SubmitShardJobStatus(_databaseJobMap[id].ExecutionState);
                        }
                        else
                        {
                            try
                            {
                                // check status of datajob and diffjob
                                RecoveryJobBase dataJob = _databaseJobMap[id].JobList.Where(x => (x.JobType == RecoveryJobType.DataBackup) ||
                                    (x.JobType == RecoveryJobType.DataRestore)).First();

                                RecoveryJobBase oplogJob = null;



                                if (dataJob != null)
                                {


                                    if (dataJob.ExecutionStatus.Status == RecoveryStatus.Completed)
                                    {
                                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                            LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.CheckJobState()", id + "WARNING: Else case executed for DIFbackup");
                                        try
                                        {
                                            if (dataJob.JobType == RecoveryJobType.DataBackup)
                                            {
                                                //close file
                                                _databaseJobMap[dataJob.JobIdentifier].RecoveryPersistenceManager.CloseBackupFile(dataJob.Database, RecoveryFileState.Completed);
                                            }
                                            // stop data job
                                            dataJob.Stop();
                                        }
                                        catch (ThreadAbortException)
                                        {
                                            Thread.ResetAbort(); // ignore it
                                        }

                                        _databaseJobMap[id].RecoveryPersistenceManager.SharedQueue.Consumed = false;
                                        _databaseJobMap[id].RecoveryPersistenceManager.SharedQueue.PauseProducing = false;

                                        SendStatus(id);
                                    }

                                }
                            }
                            catch (Exception exp)
                            {
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Error("ShardRecoveryManager.SubmitState_StartOplog()", id + " : " + exp.ToString());
                            }


                        }
                        break;
                }
            }
            else
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.SubmitState()", "Shard does not contain a job against id: " + id);

            }
        }

        private void SendStatus(string id)
        {
            if ((_databaseJobMap[id].ExecutionState.PercentageExecution >= 25 && _databaseJobMap[id].ExecutionState.PercentageExecution <= 30) ||
                               (_databaseJobMap[id].ExecutionState.PercentageExecution >= 50 && _databaseJobMap[id].ExecutionState.PercentageExecution <= 55)
                          || (_databaseJobMap[id].ExecutionState.PercentageExecution >= 75 && _databaseJobMap[id].ExecutionState.PercentageExecution <= 80))
            {
                _databaseJobMap[id].ExecutionState.MessageTime = DateTime.Now;

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ShardRecoveryManager.SubmitState()", _databaseJobMap[id].ExecutionState.ToString());

                _context.ConfigurationSession.SubmitShardJobStatus(_databaseJobMap[id].ExecutionState);
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_currentJob != null)
                _currentJob = null;

            if (_databaseJobMap != null)
            {
                foreach (JobInfoObject infoObj in _databaseJobMap.Values)
                {
                    infoObj.Dispose();
                }
                _databaseJobMap = null;
            }
        }
        #endregion
    }
}
