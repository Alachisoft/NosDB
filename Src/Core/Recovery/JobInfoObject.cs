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
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Core.Recovery.RecoveryJobs;
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
    /// Wrapper class contianing job information
    /// </summary>
    internal class JobInfoObject : IDisposable
    {
        //____________________________________
        //
        //NOTE: at any given time joblist will contain only 2 jobs.
        //1. Data job : to backup data of a database
        //2. Oplog job : to backup oplog operations of that database
        //____________________________________
        private List<RecoveryJobBase> _jobList = null;
        private ShardRecoveryJobState _executionState = null;
        private RecoveryJobType _jobType;
        private string _jobIdentifier;
        private RecoveryPersistenceManager _recoveryPersistenceManager;
        object _mutex;
        private string _rootPath = string.Empty;

        internal JobInfoObject(string id, string shardName, string nodeAddress, string clusterName, RecoveryJobType jobType, string path)
        {
            _jobList = new List<RecoveryJobBase>();
            _jobIdentifier = id;
            _rootPath = path;
            _recoveryPersistenceManager = new Recovery.RecoveryPersistenceManager();
            _recoveryPersistenceManager.LocalShardName = shardName;
            _executionState = new ShardRecoveryJobState(id, shardName, nodeAddress, clusterName, jobType);
            _mutex = new object();
        }

        internal void AddJob(RecoveryJobBase job)
        {
            if (job != null)
            {
                if (!_jobList.Contains(job))
                {
                    lock (_mutex)
                    {
                        _jobList.Add(job);
                        _executionState.UpdateEntityState(job.ExecutionStatus);
                    }
                }
            }
        }

        internal void RenameRootFolder(RecoveryFileState state)
        {
            if (!RecoveryFolderStructure.PathIsNetworkPath(_rootPath))
            {
                string[] shardPath = null;

                shardPath = RootPath.Split(new string[] { RecoveryFolderStructure.SHARD_BACKUP_FOLDER }, StringSplitOptions.RemoveEmptyEntries);


                switch (state)
                {
                    case RecoveryFileState.Cancelled:
                        string destination = string.Empty;
                        if (RootPath.EndsWith(RecoveryFolderStructure.INPROGRESS))
                        {
                            string[] name = RootPath.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);

                            destination = name[0] + RecoveryFolderStructure.CANCELLED;

                            if (!Directory.Exists(destination))
                                Directory.CreateDirectory(destination);

                            destination = RootPath.Replace(RecoveryFolderStructure.INPROGRESS, RecoveryFolderStructure.CANCELLED);
                            Directory.Move(RootPath, destination);
                            RootPath = destination;
                        }
                        break;
                    case RecoveryFileState.Completed:
                        string[] folderName = RootPath.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
                        destination = folderName[0] + RecoveryFolderStructure.COMPLETED;


                        if (!Directory.Exists(destination))
                            Directory.CreateDirectory(destination);
                        destination = RootPath.Replace(RecoveryFolderStructure.INPROGRESS, RecoveryFolderStructure.COMPLETED);
                        Directory.Move(RootPath, destination);

                        RootPath = destination;
                        break;

                    case RecoveryFileState.Failed:
                        string[] rootName = RootPath.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
                        destination = rootName[0] + RecoveryFolderStructure.FAILED;

                        if (!Directory.Exists(destination))
                            Directory.CreateDirectory(destination);
                        destination = RootPath.Replace(RecoveryFolderStructure.INPROGRESS, RecoveryFolderStructure.FAILED);
                        Directory.Move(RootPath, destination);
                        RootPath = destination;
                        break;
                }
                if (new DirectoryInfo(shardPath[0]).GetDirectories().Count() == 0)
                {
                    Directory.Delete(shardPath[0]);
                }
            }

        }

        //internal void RenameRootFolder(RecoveryFileState state)
        //{
        //    if (!RecoveryFolderStructure.PathIsNetworkPath(_rootPath))
        //    {
        //        string[] shardPath = null;

        //        shardPath = RootPath.Split(new string[] { RecoveryFolderStructure.SHARD_BACKUP_FOLDER }, StringSplitOptions.RemoveEmptyEntries);

        //        switch (state)
        //        {
        //            case RecoveryFileState.Cancelled:
        //                string destination = string.Empty;
        //                if (RootPath.EndsWith(RecoveryFolderStructure.INPROGRESS))
        //                {
        //                    string[] name = RootPath.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);

        //                    destination = name[0] + RecoveryFolderStructure.COMPLETED;
        //                    Directory.Move(shardPath[0], destination);
        //                    RootPath = Path.Combine(destination, shardPath[1]);
        //                }
        //                break;
        //            case RecoveryFileState.Completed:
        //                string[] folderName = RootPath.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
        //                destination = folderName[0] + RecoveryFolderStructure.COMPLETED;
        //                Directory.Move(shardPath[0], destination);
        //                RootPath = Path.Combine(destination, shardPath[1]);
        //                break;

        //            case RecoveryFileState.Failed:
        //                string[] rootName = RootPath.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
        //                destination = rootName[0] + RecoveryFolderStructure.FAILED;
        //                Directory.Move(shardPath[0], destination);
        //                RootPath = Path.Combine(destination, shardPath[1]);
        //                break;
        //        }
        //    }

        //}

        #region Properties
        internal string RootPath
        {
            get { return _rootPath; }
            set { _rootPath = value; }
        }

        internal RecoveryPersistenceManager RecoveryPersistenceManager
        {
            get { return _recoveryPersistenceManager; }
            set { _recoveryPersistenceManager = value; }
        }

        internal ShardRecoveryJobState ExecutionState
        {
            get { return _executionState; }
            set { _executionState = value; }
        }


        internal RecoveryJobType JobType
        {
            get { return _jobType; }
            set { _jobType = value; }
        }

        internal List<RecoveryJobBase> JobList
        {
            get { return _jobList; }
            //set 
            //{

            //    _jobList = value; // use add range for insert
            //}
        }

        #endregion

        public void Dispose()
        {
            _recoveryPersistenceManager.Dispose();
            foreach (RecoveryJobBase job in _jobList)
            {
                try
                {
                    job.Dispose();
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
            }
            _jobList = null;
            _executionState = null;

        }
    }
}
