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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    internal class PersistenceContext
    {
        //not used for time being might be useful in future
        RecoveryPersistenceConfiguration _configuration;
        RecoveryPersistenceQueue _storage;
        RecoveryJobType _jobType;
        List<BackupFile> _backupFile;
        BackupFile _activeFile;
        string _activeDB;

        //M_TODO: method that provides files for data consumption
        // GetFile(role) i.e. oplog file, db file

        internal PersistenceContext()
        {
            _backupFile = new List<BackupFile>();
            _activeFile = null;
        }

        internal BackupFile GetBackupFile(string name)
        {
            // this might lead to file not found issues
            BackupFile file=_backupFile.Where(x => x.Name.Contains(name)).First();
            
            return file;
        }

       

        internal List<BackupFile> FileList
        {
            get
            {
                return _backupFile;
            }
        }

        public string ActiveDB
        {
            get { return _activeDB; }
            set { _activeDB = value; }
        }

        internal void AddNewFile(BackupFile _file)
        {
            _backupFile.Add(_file);

            if (_activeFile == null)
                _activeFile = _backupFile.First();
            
            if (_activeFile.IsComplete)
            {
                _activeFile = _file;
            }
        }

        public RecoveryJobType JobType
        {
            get { return _jobType; }
            set { _jobType = value; }
        }

        internal RecoveryPersistenceQueue SharedQueue
        {
            get { return _storage; }
            set { _storage = value; }
        }

        public RecoveryPersistenceConfiguration PersistenceConfiguration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }




    }
}
