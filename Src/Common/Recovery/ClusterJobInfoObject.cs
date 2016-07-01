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
using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.Recovery
{
    /// <summary>
    /// Wrapper class contianing cluster job information
    /// </summary>
    public class ClusterJobInfoObject : IDisposable, ICompactSerializable
    {
        private RecoveryConfiguration _activeConfig = null;
        private ClusteredRecoveryJobState _executionState = null;
        private string _jobIdentifier = string.Empty;
        private string rootFolderName;
        private DateTime _creationTime;
        // to be used only incase of differential restore with RestoreToNew
        private bool _dataRestored;
        private Dictionary<string, ShardDifState> _shardResponseMap;
        private DateTime? _latestResponseTime;
        object _mutex;

        public ClusterJobInfoObject(string identifier, RecoveryConfiguration config)
        {
            if (!string.IsNullOrEmpty(identifier))
            {
                _jobIdentifier = identifier;
                _executionState = new ClusteredRecoveryJobState(identifier);
                _mutex = new object();
                if (config != null)
                {
                    _activeConfig = config;
                    _creationTime = _activeConfig.CreationTime;
                    string databaseName = config.DatabaseMap.First().Key;
                    rootFolderName = databaseName + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + RecoveryFolderStructure.INPROGRESS;
                    _shardResponseMap = new Dictionary<string, ShardDifState>();

                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("ClusterJobInfoObject.Submit()", config.ToString());
                }

            }
        }

        public void RenameRootFolder(RecoveryFileState state)
        {
            try
            {
                Impersonation impersonation = null;
                if (RecoveryFolderStructure.PathIsNetworkPath(ActiveConfig.RecoveryPath))
                    impersonation = new Impersonation(_activeConfig.UserName, _activeConfig.Password);

                string sourcePath = Path.Combine(ActiveConfig.RecoveryPath, RootFolderName);

                if (Directory.Exists(sourcePath))
                {
                    string destination = string.Empty;
                    string[] name = sourcePath.Split(RecoveryFolderStructure.INDELIMETER,
                        StringSplitOptions.RemoveEmptyEntries);
                    switch (state)
                    {
                        case RecoveryFileState.Cancelled:
                            if (sourcePath.EndsWith(RecoveryFolderStructure.INPROGRESS))
                            {
                                destination = name[0] + RecoveryFolderStructure.CANCELLED;
                            }
                            break;
                        case RecoveryFileState.Completed:
                            string[] folderName = sourcePath.Split(RecoveryFolderStructure.INDELIMETER,
                                StringSplitOptions.RemoveEmptyEntries);
                            destination = folderName[0] + RecoveryFolderStructure.COMPLETED;
                            break;

                        case RecoveryFileState.Failed:
                            string[] rootName = sourcePath.Split(RecoveryFolderStructure.INDELIMETER,
                                StringSplitOptions.RemoveEmptyEntries);
                            destination = rootName[0] + RecoveryFolderStructure.FAILED;

                            break;
                    }

                    if (!RecoveryFolderStructure.PathIsNetworkPath(sourcePath))
                    {
                        if (!Directory.Exists(destination))
                            Directory.CreateDirectory(destination);


                        Directory.Move(Path.Combine(sourcePath, RecoveryFolderStructure.CONFIG_FOLDER),
                            Path.Combine(destination, RecoveryFolderStructure.CONFIG_FOLDER));

                    }
                    else
                    {
                        Directory.Move(sourcePath, destination);
                    }

                    if (Directory.Exists(sourcePath))
                    {
                        if (new DirectoryInfo(sourcePath).GetDirectories().Count() == 0)
                        {
                            Directory.Delete(sourcePath);
                        }
                    }

                    if (impersonation != null)
                        impersonation.Dispose();
                }
            }
            catch (Exception exp)
            {
                LoggerManager.Instance.RecoveryLogger.Error("Config.Rename", exp);
            }

        }

        //public void RenameRootFolder(RecoveryFileState state)
        //{
        //    string path = Path.Combine(ActiveConfig.RecoveryPath, RootFolderName);
        //    if (Directory.Exists(path))
        //    {
        //        switch (state)
        //        {
        //            case RecoveryFileState.Cancelled:
        //                string destination = string.Empty;
        //                if (path.EndsWith(RecoveryFolderStructure.INPROGRESS))
        //                {
        //                    string[] name = path.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
        //                    destination = name[0] + RecoveryFolderStructure.COMPLETED;
        //                    Directory.Move(path, destination);
        //                }
        //                break;
        //            case RecoveryFileState.Completed:
        //                string[] folderName = path.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
        //                destination = folderName[0] + RecoveryFolderStructure.COMPLETED;
        //                Directory.Move(path, destination);

        //                break;

        //            case RecoveryFileState.Failed:
        //                string[] rootName = path.Split(RecoveryFolderStructure.INDELIMETER, StringSplitOptions.RemoveEmptyEntries);
        //                destination = rootName[0] + RecoveryFolderStructure.FAILED;
        //                Directory.Move(path, destination);
        //                break;
        //        }
        //    }


        //}

        public void AddShardResponse(string shard, ShardDifState received)
        {
            lock (_mutex)
            {
                if (_shardResponseMap.ContainsKey(shard))
                {
                    _shardResponseMap[shard] = received;

                    _latestResponseTime = DateTime.UtcNow;
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Info("ClusterJobInfo.AddSharState", shard + " : " + received);
                }
            }
        }

        public ShardDifState ShardReceivedState()
        {
            ShardDifState state = ShardDifState.success;
            foreach (ShardDifState val in _shardResponseMap.Values)
            {
                if (val != ShardDifState.failure)
                {
                    if (val == ShardDifState.none)
                    {
                        state = val;
                    }
                }
                else
                {
                    state = ShardDifState.failure;
                    break;
                }
            }
            return state;
        }

        #region Properties
        [JsonIgnore]
        public DateTime? LatestResponseTime
        {
            get { return _latestResponseTime; }
            set { _latestResponseTime = value; }
        }

        [JsonIgnore]
        public Dictionary<string, ShardDifState> ShardResponseMap
        {
            get { return _shardResponseMap; }
            set { _shardResponseMap = value; }
        }

        [JsonIgnore]
        public bool DataRestored
        {
            get { return _dataRestored; }
            set { _dataRestored = value; }
        }
        [JsonProperty(PropertyName = "ExecutionState")]
        public ClusteredRecoveryJobState ExecutionState
        {
            get { return _executionState; }
            set { _executionState = value; }
        }
        [JsonProperty(PropertyName = "CreationTime")]
        public DateTime CreationTime
        {
            get { return _creationTime; }
            set
            {
                _creationTime = value;

            }
        }

        [JsonProperty(PropertyName = "ActiveConfig")]
        public RecoveryConfiguration ActiveConfig
        {
            get { return _activeConfig; }
            set { _activeConfig = value; }
        }

        [JsonProperty(PropertyName = "RootFolderName")]
        public string RootFolderName
        {
            get { return rootFolderName; }
            set { rootFolderName = value; }
        }

        [JsonProperty(PropertyName = "_key")]
        public string Identifier
        {
            get { return _jobIdentifier; }
            set { _jobIdentifier = value; }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _activeConfig = null;
            _executionState = null;
            _shardResponseMap = null;
        }
        #endregion

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _jobIdentifier = reader.ReadString();
            rootFolderName = reader.ReadString();
            _creationTime = reader.ReadDateTime();
            _activeConfig = reader.ReadObject() as RecoveryConfiguration;
            _executionState = reader.ReadObject() as ClusteredRecoveryJobState;
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_jobIdentifier);
            writer.Write(rootFolderName);
            writer.Write(_creationTime);
            writer.WriteObject(_activeConfig);
            writer.WriteObject(_executionState);
        }
    }
}
