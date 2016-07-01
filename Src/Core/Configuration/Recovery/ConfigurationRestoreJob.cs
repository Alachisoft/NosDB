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
    internal class ConfigurationRestoreJob : ConfigurationRecoveryJobBase
    {
        CsBackupableEntities _entity = null;
        IConfigOperationExecutor operationHandler = null;
        Dictionary<string,string> _database;
  
        internal ConfigurationRestoreJob(string identifier, RecoveryPersistenceManager persistenceManager, IConfigOperationExecutor opHandler,string cluster,Dictionary<string,string> database, RecoveryJobType jobType)
            : base(identifier,jobType, persistenceManager, cluster)
        {
            operationHandler = opHandler;
            _database = database;
          
        }

        #region Overridden methods

        internal override void Run()
        {
            try
            {
                LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = ExecutionState.Shard != null ? ExecutionState.Shard : "", DatabaseName = "" });
                #region Data consumtion
                foreach (DataSlice info in PersistenceManager.SharedQueue.GetConsumingEnumerable(PersistenceManager.SharedQueue.CancelToken.Token))
                {
                    if (info.SliceHeader.ContentType != DataSliceType.Command)
                    {
                        if (!PersistenceManager.SharedQueue.IsAddingCompleted)
                        {
                            if (info != null)
                            {
                                RecreateCluster(info);
                            }
                        }
                        else
                        {
                            if (info != null)
                            {
                                //check for any remaining data
                                RecreateCluster(info);
                            }
                        }
                    }
                    else
                    {
                        if (info.Data != null)
                        {
                            string message = CompactBinaryFormatter.FromByteBuffer(info.Data, string.Empty) as string;
                            if (message.Contains("Complete"))
                            {
                                break;
                            }
                        }
                    }

                }
                #endregion
            }
            catch (ThreadAbortException)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsDebugEnabled)
                    LoggerManager.Instance.RecoveryLogger.Debug("ConfigurationRestoreJob.Run()", "Thread stopped");
                Thread.ResetAbort();
            }
            catch (OperationCanceledException)
            {
                // read all remaining documnets if any
                #region Check for any remaining data slices
                DataSlice info;
                // dequeue data slices from queue 
                while (PersistenceManager.SharedQueue.TryTake(out info, 30000))// default timeout is 5 min= 300000
                {
                    if (info != null)
                    {
                        RecreateCluster(info);
                    }
                }

                // check if timedout
                if (!PersistenceManager.SharedQueue.IsAddingCompleted)
                {
                    throw new TimeoutException("Timed out while restoring Configuration");
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("ConfigurationRecoveryJob.Run()", ex.ToString());

                ExecutionState.Status = RecoveryStatus.Failure;
                ExecutionState.PercentageExecution = 0;//[M_NOTE] rudementary logic, change this 
                ExecutionState.MessageTime = DateTime.Now;
                ExecutionState.Message = "Failed to Restore Configuration";

                ProgressHandler.SubmitRecoveryState(ExecutionState);
            }

            ExecutionState.Status = RecoveryStatus.Submitted;
            ExecutionState.PercentageExecution = 25;//[M_NOTE] rudementary logic, change this 
            ExecutionState.MessageTime = DateTime.Now;
            ExecutionState.Message = "Data Submitted to Config Server";

             System.Threading.Tasks.Task.Factory.StartNew(() =>ProgressHandler.SubmitRecoveryState(ExecutionState));
             if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                 LoggerManager.Instance.RecoveryLogger.Info("ConfigurationRecoveryJob.Run()", "completed");
        }

        internal override object JobStatistics()
        {
            return base.JobStatistics();
        }
        #endregion

        #region helper method

        private void RecreateCluster(DataSlice slice)
        {
            if (slice.Data != null)
            {
                _entity = (CsBackupableEntities)CompactBinaryFormatter.FromByteBuffer(slice.Data, string.Empty);
                if (slice.SliceHeader.ContentType == DataSliceType.Config)
                {                   
                    if (operationHandler != null)
                        operationHandler.Restore(_entity, _database, Cluster);
                }
               
            }
        }

        #endregion


    }
}
