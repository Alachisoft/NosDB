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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Recovery
{
    public class RecoveryFolderStructure
    {
        #region FileNames
        public const string CONFIG_SERVER = "ConfigServer";
        public const string OPLOG = "Oplog";
        #endregion

        #region FolderNames
        public const string SHARD_BACKUP_FOLDER = "Backup-";
        public const string CONFIG_FOLDER = "Configserver";
        public const string ROOT_FOLDER = "Recovery-";
        #endregion

        #region Folder states
        public const string INPROGRESS = "-in-progress";
        public const string CANCELLED = "-cancelled";
        public const string COMPLETED = "-completed";
        public const string FAILED = "-failed";
        public static string[] INDELIMETER = new string[] { "-in" };
        #endregion
        public static RecoveryOperationStatus ValidateFolderStructure(string path, RecoveryJobType jobType, bool configCheck, List<string> shardNameList)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Success);
            try
            {
                // based on folder type  shared or local
                // 1. check if folder accessible
                // 2. check if provided folders comply with the requirements
                bool networkPath = PathIsNetworkPath(path);

                if (networkPath)
                {
                    if (configCheck)
                    {
                        if (!Directory.Exists(path))
                        {
                            state.Status = RecoveryStatus.Failure;
                            state.Message = "Provided path'" + path + "' is invalid or access is denied";
                            return state;
                        }

                    }

                    switch (jobType)
                    {
                        case RecoveryJobType.ConfigRestore:
                            state = ValidateConfigFolder(path, false);
                            if (state.Status == RecoveryStatus.Failure)
                            {
                                return state;
                            }
                            break;
                        case RecoveryJobType.Restore:
                            state = ValidateConfigFolder(path, false);
                            if (state.Status == RecoveryStatus.Failure)
                            {
                                return state;
                            }
                            state = RemoteValidateShardFolder(path, false, shardNameList);
                            break;

                        case RecoveryJobType.DataRestore:
                            state = RemoteValidateShardFolder(path, false, shardNameList);
                            if (state.Status == RecoveryStatus.Failure)
                            {
                                return state;
                            }
                            //check for file paths of all shards
                            break;

                        case RecoveryJobType.Export:
                            if (!Directory.Exists(path))
                            {
                                state.Message = "Path provided is invalid or access is denied";
                            }
                            break;
                        case RecoveryJobType.Import:
                            if (!File.Exists(path))
                            {
                                state.Message = "Path provided is invalid or access is denied";
                            }
                            break;
                    }
                    if (state.Status == RecoveryStatus.Failure)
                    {
                        return state;
                    }
                }
                else
                {
                    if (configCheck)
                    {
                        if (Directory.Exists(path))
                        {
                            switch (jobType)
                            {
                                case RecoveryJobType.ConfigRestore:
                                case RecoveryJobType.Restore:
                                    state = ValidateConfigFolder(path, false);
                                    if (state.Status == RecoveryStatus.Failure)
                                    {
                                        return state;
                                    }
                                    break;

                            }
                        }
                        else
                        {
                            state.Message = "Path provided is invalid or access is denied";
                        }
                    }
                    else
                    {
                        switch (jobType)
                        {
                            case RecoveryJobType.DataRestore:
                                state = ValidateShardFolder(path, false);
                                if (state.Status == RecoveryStatus.Failure)
                                {
                                    return state;
                                }
                                break;

                            case RecoveryJobType.Export:
                                if (!Directory.Exists(path))
                                {
                                    state.Message = "Path provided is invalid or access is denied";
                                    return state;
                                }
                                break;
                            case RecoveryJobType.Import:
                                if (!File.Exists(path))
                                {
                                    state.Message = "Path provided is invalid or access is denied";
                                    return state;
                                }
                                break;
                        }
                    }

                }

            }
            catch (Exception exp)
            {
                state.Message = exp.ToString();
            }
            return state;
        }

        #region Helper Methods
        [DllImport("shlwapi.dll")]
        public static extern bool PathIsNetworkPath(string pszPath);

        private static RecoveryOperationStatus ValidateConfigFolder(string path, bool checkDif)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            string configPath = Path.Combine(path, CONFIG_FOLDER);
            if (!checkDif)
            {
                if (!Directory.Exists(configPath))
                {
                    state.Message = "Path for config server restore not provided";
                    return state;
                }
            }

            state.Status = RecoveryStatus.Success;


            return state;
        }

        private static RecoveryOperationStatus ValidateShardFolder(string path, bool checkDif)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            string shardPath = Path.Combine(path);

            if (!checkDif)
            {
                if (!Directory.Exists(shardPath))
                {
                    state.Status = RecoveryStatus.Failure;
                    state.Message = "Provided path '" + path + "' for restore does not exist";
                    return state;
                }
            }

            state.Status = RecoveryStatus.Success;
            return state;
        }

        private static RecoveryOperationStatus RemoteValidateShardFolder(string path, bool checkDif, List<string> shardNameList)
        {
            RecoveryOperationStatus state = new RecoveryOperationStatus(RecoveryStatus.Failure);
            foreach (string shardName in shardNameList)
            {
                if (!string.IsNullOrEmpty(shardName))
                {
                    if (!checkDif)
                    {
                        if (!Directory.Exists(path))
                        {
                            state.Message = "Path for '" + shardName + "' restore not provided";
                            return state;
                        }
                    }
                }
            }
            state.Status = RecoveryStatus.Success;
            return state;
        }

        #endregion

    }
}
