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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Core.Recovery.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery
{
    /// <summary>
    /// Manages recovery persistance, initiates consumes data produced during backup phase.
    /// </summary>
    public class RecoveryPersistenceManager : IDisposable
    {

        private RecoveryPersistenceQueue _persistenceQueue;

        //private Dictionary<PersistenceContext, List<PersistenceIOBase>> _runningRoles = null;
        private List<PersistenceIOBase> _runningRoles = null;
        private bool isJobActive;
        private PersistenceContext _activeContext = null;
        private string _localShardName;
        private int DEFAULT_JOB_COUNT = 1;
        private const string CONSUMER_THREAD_NAME = "Recovery_Writer_Thread_";
        private const string PRODUCER_THREAD_NAME = "Recovery_Reader_Thread_";
        private const string CONFIG_PRODUCER_THREAD_NAME = "Config_Recovery_Reader_Thread";
        private const string CONFIG_CONSUMER_THREAD_NAME = "Config_Consumer_Writer_Thread";
        private const string WRITER_ROLE = "Writer";
        private const string READER_ROLE = "Reader";

        public RecoveryPersistenceManager()
        {
            _persistenceQueue = new RecoveryPersistenceQueue();
            //_runningRoles = new Dictionary<PersistenceContext, List<PersistenceIOBase>>();
            _runningRoles = new List<PersistenceIOBase>();
            isJobActive = false;
            _localShardName = string.Empty;
        }

        #region Properties
        public bool IsJobActive
        {
            get { return isJobActive; }
            set
            {
                isJobActive = value;
                ManageIOThreads();
            }
        }

        internal RecoveryPersistenceQueue SharedQueue
        {
            get { return _persistenceQueue; }
            set { _persistenceQueue = value; }
        }

        internal PersistenceContext ActiveContext
        {
            get { return _activeContext; }
        }

        internal string LocalShardName
        {
            get { return _localShardName; }
            set { _localShardName = value; }
        }
        #endregion

        #region  Methods
        public bool SetJobConfiguration(RecoveryJobType jobType, RecoveryPersistenceConfiguration config, string db, int jobCount = 0)
        {
            bool status = false;
            if (_activeContext == null)
            {
                try
                {
                    PersistenceContext _context = new PersistenceContext();
                    _context.JobType = jobType;
                    _context.SharedQueue = this._persistenceQueue;
                    _context.PersistenceConfiguration = config;
                    _context.ActiveDB = db;
                    foreach (string fileName in config.FileName)
                    {
                        switch (jobType)
                        {
                            case RecoveryJobType.ConfigBackup:
                            case RecoveryJobType.DataBackup:
                            case RecoveryJobType.FullBackup:


                                BackupFile file = new BackupFile(fileName, config.FilePath, config.UserName, config.Password);
                                file.FileHeader.Database = config.DatabaseName;
                                file.FileHeader.DatabaseCluster = config.Cluster;
                                _context.AddNewFile(file);
                                break;

                            case RecoveryJobType.ConfigRestore:
                            case RecoveryJobType.DataRestore:

                                _context.AddNewFile(new BackupFile(fileName, config.FilePath, config.UserName, config.Password));
                                break;

                        }
                    }

                    _activeContext = _context;
                    CreatePersistenceRoles(_context, jobCount == 0 ? DEFAULT_JOB_COUNT : jobCount);
                    status = true;
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("RecoveryPersistenceManager.SetConfig()", exp.ToString());
                }
            }
            return status;

        }

        //refractor this code
        private void CreatePersistenceRoles(PersistenceContext context, int jobCount)
        {
            try
            {
                // check if roles already running
                if (_runningRoles.Count == 0)
                {
                    switch (context.JobType)
                    {
                        // consumer roles
                        case RecoveryJobType.FullBackup:
                        case RecoveryJobType.DataBackup:
                        case RecoveryJobType.Export:

                            for (int i = 0; i < jobCount; i++)
                            {
                                RecoveryIOWriter _consumer = new RecoveryIOWriter(CONSUMER_THREAD_NAME + i, WRITER_ROLE);
                                _consumer.Initialize(context);
                                _runningRoles.Add(_consumer);
                            }
                            break;

                        //producer roles
                        case RecoveryJobType.Import:
                        case RecoveryJobType.Restore:
                        case RecoveryJobType.DataRestore:

                            for (int i = 0; i < jobCount; i++)
                            {
                                RecoveryIOReader _producer = new RecoveryIOReader(PRODUCER_THREAD_NAME + i, READER_ROLE);
                                _producer.Initialize(context);
                                _runningRoles.Add(_producer);

                            }
                            break;
                        case RecoveryJobType.ConfigBackup:
                            RecoveryIOWriter _configConsumer = new RecoveryIOWriter(CONFIG_CONSUMER_THREAD_NAME, WRITER_ROLE);
                            _configConsumer.Initialize(context);
                            _runningRoles.Add(_configConsumer);

                            break;
                        case RecoveryJobType.ConfigRestore:
                            RecoveryIOReader _configProducer = new RecoveryIOReader(CONFIG_PRODUCER_THREAD_NAME, READER_ROLE);
                            _configProducer.Initialize(context);
                            _runningRoles.Add(_configProducer);
                            break;
                    }

                }
                ManageIOThreads();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryPersistenceManager.CreateRole()", ex.ToString());
            }
        }

        internal RecoveryPersistenceQueue PersistenceQueue(string database, string collection)
        {
            return _persistenceQueue;
        }

        /// <summary>
        /// Returns bool if manager is already executing a role.
        /// </summary>
        /// <returns></returns>
        public bool IsRoleActive()
        {
            if (_runningRoles.Count > 0)
            {
                foreach (PersistenceIOBase ioWorkers in _runningRoles)
                {
                    if (!ioWorkers.IsActive)
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// manages Persistence role threads.
        /// </summary>
        private void ManageIOThreads()
        {
            try
            {
                if (_runningRoles.Count > 0)
                {
                    if (!isJobActive)
                    {
                        foreach (PersistenceIOBase ioWorkers in _runningRoles)
                        {
                            try
                            {
                                ioWorkers.Stop();
                            }
                            catch (ThreadAbortException)
                            {
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsDebugEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Debug("RecoveryPersistenceManager.ManageIOThreads()", "Thread stopped");
                                Thread.ResetAbort();

                            }
                        }
                    }
                    else
                    {
                        foreach (PersistenceIOBase ioWorkers in _runningRoles)
                        {
                            ioWorkers.Start();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryPersistenceManager.ManageIOThreads()", exp.ToString());
            }
        }

        public void CloseBackupFile(string name, RecoveryFileState state)
        {
            //M_TODO:
            // Add timeout interval for file writing, incase the data is not being consumed and timeout span has been reached, break the loop and DIE!!!
            try
            {
                //  TimeSpan timeSpan
                BackupFile file = _activeContext.GetBackupFile(name);
                file.FileHeader.State = state;
                int elapsed = 0;
                // if file not already flushed
                if (!file.HeaderFlushed)
                {
                    while (!_activeContext.SharedQueue.Consumed)
                    {
                        // wait till all data has been consumed and written
                    }

                    if (_activeContext.SharedQueue.Consumed)
                    {
                        if (file.SaveHeader())
                        {
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("RecoveryPersistenceManager.CloseBackupFile()", file.Name + " closing");
                            file.Close();
                        }
                        else
                            throw new Exception("Unable to write data");
                    }
                }
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryPersistenceManager.CloseBackupFile()", exp.ToString());
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            try
            {
                if (_persistenceQueue != null)
                {
                    _persistenceQueue.Clear();
                }

                if (_runningRoles != null)
                {
                    foreach (PersistenceIOBase ioWorkers in _runningRoles)
                    {
                        try
                        {
                            ioWorkers.Dispose();
                        }
                        catch (ThreadAbortException)
                        {
                            Thread.ResetAbort();
                        }
                    }
                    _runningRoles = null;
                }

                if (_persistenceQueue != null)
                {
                    _persistenceQueue.Dispose();
                }
            }
            catch (Exception exp)
            { }
        }

        #endregion
    }
}
