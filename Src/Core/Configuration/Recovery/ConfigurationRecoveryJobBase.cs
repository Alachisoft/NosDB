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
using Alachisoft.NosDB.Core.Recovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    internal abstract class ConfigurationRecoveryJobBase : IDisposable
    {
        Thread _recoveryThread = null;
        string _jobIdentifier;
        string _cluster;

        private RecoveryJobType _jobType;
        private IJobProgressHandler _progressHandler;
        private ShardRecoveryJobState _executionState = null;
        private RecoveryPersistenceManager _persistenceManager;

        public string JobIdentifier
        {
            get { return _jobIdentifier; }
            //set { _jobIdentifier = value; }
        }

        internal ConfigurationRecoveryJobBase(string jobIdentifier, RecoveryJobType jobType, RecoveryPersistenceManager persistenceManager, string cluster)
        {
            _jobIdentifier = jobIdentifier;
            _jobType = jobType;
            _persistenceManager = persistenceManager;
            _cluster = cluster;
            _executionState = new ShardRecoveryJobState(jobIdentifier, string.Empty, string.Empty, _cluster, _jobType);
        }

        internal ConfigurationRecoveryJobBase()
        {
        }

        internal virtual bool Initialize(RecoveryOperation operation)
        {
            _jobIdentifier = operation.JobIdentifer;

            return true;
        }

        public RecoveryPersistenceManager PersistenceManager
        {
            get { return _persistenceManager; }

        }

        public string Cluster
        {
            get { return _cluster; }
            set { _cluster = value; }
        }

        internal ShardRecoveryJobState ExecutionState
        {
            get { return _executionState; }
            set { _executionState = value; }
        }

        public IJobProgressHandler ProgressHandler
        {
            get { return _progressHandler; }
        }

        internal void Start()
        {
            if (_recoveryThread == null)
            {
                _recoveryThread = new Thread(new ThreadStart(Run));
                _recoveryThread.Name = "ConfigServer_" + _jobIdentifier + "_" + _jobType;
                _recoveryThread.IsBackground = true;
                
                _executionState.Status = RecoveryStatus.Executing;
                _executionState.PercentageExecution = 0;
                _executionState.StartTime = DateTime.Now;
                _executionState.MessageTime = DateTime.Now;

                _progressHandler.SubmitRecoveryState(_executionState);
                
                _recoveryThread.Start();
            }
        }

        internal void Stop()
        {

            if (_recoveryThread != null)
            {
                _recoveryThread.Abort();
                _recoveryThread = null;

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

        internal void RegisterProgressHandler(IJobProgressHandler handler)
        {
            if (handler != null)
                _progressHandler = handler;
        }

        internal virtual void Run()
        {

        }

        internal virtual object JobStatistics()
        {
            return null;
        }

        public void Dispose()
        {
           // _executionState = null;
            _progressHandler = null;

            if (_recoveryThread != null)
                if (_recoveryThread != null)
                    _recoveryThread.Abort();
        }
    }
}
