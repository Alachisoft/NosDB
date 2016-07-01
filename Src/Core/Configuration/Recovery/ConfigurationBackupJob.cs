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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Core.Recovery;
using Alachisoft.NosDB.Core.Recovery.Persistence;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    internal class ConfigurationBackupJob : ConfigurationRecoveryJobBase
    {
        bool _isDIFBackup;
        CsBackupableEntities _entities;
        internal ConfigurationBackupJob(string identifier, RecoveryPersistenceManager persistenceManager,CsBackupableEntities entities,string cluster, bool isDifJob)
            : base(identifier, RecoveryJobType.ConfigBackup, persistenceManager,cluster)
        {
            _entities = entities;
            _isDIFBackup = isDifJob;
        }

        #region Overridden methods


        internal override bool Initialize(RecoveryOperation operation)
        {
            return base.Initialize(operation);
        }

        internal override void Run()
        {
            try
            {
                LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = ExecutionState.Shard != null ? ExecutionState.Shard : "", DatabaseName =  "" });
                DataSlice _activeSlice = PersistenceManager.ActiveContext.GetBackupFile(RecoveryFolderStructure.CONFIG_SERVER).CreateNewDataSlice();
                _activeSlice.SliceHeader.ContentType = _isDIFBackup ? DataSliceType.DifConfig : DataSliceType.Config;
                _activeSlice.SliceHeader.Cluster = Cluster;
                if (_entities.Database.Count == 1)
                {
                    _activeSlice.SliceHeader.Database = _entities.Database.First().Key;
                }
                _activeSlice.Data = CompactBinaryFormatter.ToByteBuffer(_entities, string.Empty);
                _activeSlice.SliceHeader.TotalSize = _activeSlice.Data.LongLength;
                PersistenceManager.SharedQueue.Add(_activeSlice);
               
                DataSlice finalSlice = new DataSlice(999999);
                finalSlice.SliceHeader.Collection = "Complete";
                finalSlice.SliceHeader.Database = "ConfigServer";
                finalSlice.SliceHeader.Cluster = Cluster;
                finalSlice.SliceHeader.ContentType = DataSliceType.Command;
                finalSlice.Data = CompactBinaryFormatter.ToByteBuffer("Config_Complete_Adding", string.Empty);
               
                PersistenceManager.SharedQueue.Add(finalSlice);

                while (!PersistenceManager.SharedQueue.Consumed)
                {
                    // wait till all data has been consumed and written
                    //M_TODO:
                    // Add timeout interval for file writing, incase the data is not being consumed and timeout span has been reached, break the loop and DIE!!!
                }

                if (PersistenceManager.SharedQueue.Consumed)
                {
                    ExecutionState.Status = RecoveryStatus.Completed;
                    ExecutionState.PercentageExecution = 100;//[M_NOTE] rudementary logic, change this 
                    ExecutionState.MessageTime = DateTime.Now;
                    ExecutionState.Message = "Completed Backup of ConfigServer";
                }
                else
                {
                    ExecutionState.Status = RecoveryStatus.Failure;
                    ExecutionState.PercentageExecution = 0;//[M_NOTE] rudementary logic, change this 
                    ExecutionState.MessageTime = DateTime.Now;
                    ExecutionState.Message = "Failed Backup of ConfigServer";
                }

                System.Threading.Tasks.Task.Factory.StartNew(() => ProgressHandler.SubmitRecoveryState(ExecutionState));
                
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("ConfigurationBackupJob.Run()", "completed");
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch(Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigurationBackupJob.Run()",ex.ToString());
            }
        }

        internal override object JobStatistics()
        {
            return base.JobStatistics();
        }
        #endregion
    }
}
