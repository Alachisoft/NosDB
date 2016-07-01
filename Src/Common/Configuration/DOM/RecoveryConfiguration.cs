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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class RecoveryConfiguration : ICloneable, ICompactSerializable
    {
        private RecoveryJobType _jobType;
        private RecoveryOpCodes _operation;
        private string _identifier;
        private string _cluster;
        private string _configCluster;
        private string _recoveryPath;
        private Dictionary<string, string> _renameMapping;
        private ExecutionPreference _executionPreference;
        private DateTime _creationTime;
        private string _username;
        private string _password;

        public RecoveryConfiguration()
        {
            _jobType = RecoveryJobType.DataBackup;
            _operation = RecoveryOpCodes.SubmitTask;
            _recoveryPath = string.Empty;
            _renameMapping = new Dictionary<string, string>();
            _executionPreference = ExecutionPreference.Primary;
            _creationTime = DateTime.Now;
            _username = String.Empty;
            _password = String.Empty;

        }

        #region Properties


        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public DateTime CreationTime
        {
            get { return _creationTime; }
            set { _creationTime = value; }
        }



        public string RecoveryPath
        {
            get { return _recoveryPath; }
            set { _recoveryPath = value; }
        }

        public string Cluster
        {
            get { return _cluster; }
            set { _cluster = value; }
        }

        public ExecutionPreference ExecutionPreference
        {
            get { return _executionPreference; }
            set { _executionPreference = value; }
        }


        /// <summary>
        /// List of databases to restore/backup. This dictionary contains any renaming incase of restore.
        /// by default the value will be empty
        /// </summary>
        public Dictionary<string, string> DatabaseMap
        {
            get { return _renameMapping; }
            set { _renameMapping = value; }
        }

        /// <summary>
        /// Client operation against a recovery job
        /// </summary>
        public RecoveryOpCodes Operation
        {
            get { return _operation; }
            set { _operation = value; }
        }

        /// <summary>
        /// client provided recovery process to be initiated
        /// </summary>
        public RecoveryJobType JobType
        {
            get { return _jobType; }
            set { _jobType = value; }
        }

        /// <summary>
        /// Unique recovery job identifier
        /// </summary>
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }
        #endregion

        #region ICompactSerializable
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _jobType = (RecoveryJobType)reader.ReadInt32();
            _operation = (RecoveryOpCodes)reader.ReadInt32();
            _identifier = reader.ReadString();
            _cluster = reader.ReadString();
            _executionPreference = (ExecutionPreference)reader.ReadInt32();
            _configCluster = reader.ReadString();
            _recoveryPath = reader.ReadString();
            _renameMapping = Util.SerializationUtility.DeserializeDictionary<string, string>(reader);
            _creationTime = reader.ReadDateTime();
            _username = reader.ReadString();
            _password = reader.ReadString();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write((int)_jobType);
            writer.Write((int)_operation);
            writer.Write(_identifier);
            writer.Write(_cluster);
            writer.Write((int)_executionPreference);
            writer.Write(_configCluster);
            writer.Write(_recoveryPath);
            Util.SerializationUtility.SerializeDictionary<string, string>(_renameMapping, writer);
            writer.Write(_creationTime);
            writer.Write(_username);
            writer.Write(_password);

        }
        #endregion

        #region ICloneable
        public object Clone()
        {
            RecoveryConfiguration config = new RecoveryConfiguration();
            config.Identifier = Identifier;
            config.DatabaseMap = DatabaseMap;
            config.Cluster = Cluster;
            config.ExecutionPreference = _executionPreference;
            config.RecoveryPath = RecoveryPath;
            config.JobType = JobType;
            config.CreationTime = CreationTime;
            config.UserName = _username;
            config.Password = _password;
            return config;
        }

        public override string ToString()
        {
            const string delim = " : ";
            return _identifier + delim + _jobType + delim + _recoveryPath;
        }
        #endregion




    }
}
