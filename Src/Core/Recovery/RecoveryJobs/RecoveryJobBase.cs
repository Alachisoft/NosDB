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
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.RecoveryJobs
{
    internal abstract class RecoveryJobBase : IDisposable
    {
        private Thread _recoveryThread = null;
        private string _jobIdentifier;
        private string _database;
        private RecoveryJobType _jobType;
        private List<string> _collections;
        private IJobProgressHandler _progressHandler;
        private RecoveryPersistenceManager _persistenceManager;
        private RecoveryJobStateBase _executionStatus = null;
        private string _cluster;

        internal RecoveryJobBase()
        {
            _collections = new List<string>();

        }
        
        internal RecoveryJobBase(string identifier, string database, List<string> collectionList, RecoveryJobType jobType,
            RecoveryPersistenceManager _manager, string cluster)
        {
            _jobIdentifier = identifier;
            _database = database;
            _collections = collectionList;
            _jobType = jobType;

            string entityName = database;

            switch (jobType)
            {
                case RecoveryJobType.ConfigBackup:
                case RecoveryJobType.ConfigRestore:
                    entityName = "ConfigServer";
                    break;
            }
            _executionStatus = new RecoveryJobStateBase(identifier, entityName);
            _persistenceManager = _manager;
            _cluster = cluster;
        }

        #region properties
        public ThreadState State
        {
            get
            {
                if (_recoveryThread != null)
                    return _recoveryThread.ThreadState;
                else
                    return ThreadState.Unstarted;
            }
        }
        public string Cluster
        {
            get { return _cluster; }

        }

        public RecoveryPersistenceManager PersistenceManager
        {
            get { return _persistenceManager; }

        }
        internal RecoveryJobStateBase ExecutionStatus
        {
            get { return _executionStatus; }
            set { _executionStatus = value; }
        }

        public RecoveryJobType JobType
        {
            get { return _jobType; }
            set { _jobType = value; }
        }

        public string Database
        {
            get { return _database; }
            set { _database = value; }
        }

        public string JobIdentifier
        {
            get
            {
                return _jobIdentifier;
            }
        }

        public List<string> Collections
        {
            get { return _collections; }
            set { _collections = value; }
        }

        public IJobProgressHandler ProgressHandler
        {
            get { return _progressHandler; }
        }

        #endregion

        #region virtual methods
        internal virtual bool Initialize(RecoveryOperation operation)
        {
            _jobIdentifier = operation.JobIdentifer;

            return true;
        }

        internal virtual void Run()
        {

        }

        internal virtual object JobStatistics()
        {
            return null;
        }

        #endregion

        #region Internal methods

        internal void Start()
        {

            if (_recoveryThread == null)
            {
                _recoveryThread = new Thread(new ThreadStart(Run));
                _recoveryThread.Name = _database + "_" + _jobIdentifier + "_" + _jobType;
                _recoveryThread.IsBackground = true;
                _recoveryThread.Start();

                _executionStatus.Status = RecoveryStatus.Executing;
                _executionStatus.PercentageExecution = 0;
                _executionStatus.StartTime = DateTime.Now;
                _executionStatus.MessageTime = DateTime.Now;

                _progressHandler.SubmitRecoveryState(_executionStatus);

            }
        }

        internal void RegisterProgressHandler(IJobProgressHandler handler)
        {
            if (handler != null)
                _progressHandler = handler;
        }

        internal void Stop()
        {

            if (_recoveryThread != null)
            {
                _recoveryThread.Abort();
                _recoveryThread = null;

                // status submission
                //_executionStatus.Status = RecoveryStatus.Cancelled;
                //_executionStatus.StopTime = DateTime.Now;
                //_progressHandler.SubmitRecoveryState(_executionStatus);
            }

        }

        internal void Pause()
        {
            // Todo: for future
        }

        internal void Resume()
        {
            // Todo: for future
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region IDisposable
        public virtual void Dispose()
        {
            //_executionStatus.Status = RecoveryStatus.Cancelled;
            //_executionStatus.StopTime = DateTime.Now;
            //_progressHandler.SubmitRecoveryState(_executionStatus);

            if (_collections != null)
                _collections = null;

            //_executionStatus = null;
            _progressHandler = null;

            if (_recoveryThread != null)
            {
                _recoveryThread.Abort();
                _recoveryThread = null;
            }
        }
        #endregion


    }
}
