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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Core.Recovery.Persistence;
using Alachisoft.NosDB.Core.Toplogies;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.RecoveryJobs
{
    internal class DatabaseRestoreJob : RecoveryJobBase
    {
        NodeContext _context = null;
        List<FailedDocument> _failedDocuments = null;
        internal DatabaseRestoreJob(string identifier, NodeContext context, string database, List<string> collections, RecoveryPersistenceManager _manager, string cluster)
            : base(identifier, database, collections, RecoveryJobType.DataRestore, _manager, cluster)
        {
            _context = context;
            _failedDocuments = new List<FailedDocument>();
        }

        #region Overridden methods


        internal override bool Initialize(RecoveryOperation operation)
        {
            return base.Initialize(operation);
        }

        internal override void Run()
        {
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = Database != null ? Database : "" });
            try
            {
                #region Data consumtion
                foreach (DataSlice info in PersistenceManager.SharedQueue.GetConsumingEnumerable(PersistenceManager.SharedQueue.CancelToken.Token))
                {
                    if (info.SliceHeader.ContentType != DataSliceType.Command)
                    {
                        if (!PersistenceManager.SharedQueue.IsAddingCompleted)
                        {
                            if (info != null)
                            {
                                InsertData(info);
                            }
                        }
                        else
                        {
                            if (info != null)
                            {
                                //check for any remaining data
                                InsertData(info);
                            }
                        }
                    }
                    else
                    {

                        if (info.Data != null)
                        {
                            string message = CompactBinaryFormatter.FromByteBuffer(info.Data, string.Empty) as string;
                            if (message.Contains("Complete_Adding_Data"))
                            {
                                PersistenceManager.SharedQueue.PauseProducing = true;
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
                    LoggerManager.Instance.RecoveryLogger.Debug("DatabaseRestoreJob.Run()", "Thread stopped");
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
                        InsertData(info);
                    }
                }

                // check if timedout
                if (!PersistenceManager.SharedQueue.IsAddingCompleted)
                {
                    throw new TimeoutException("Timed out while restoring " + Database);
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("DatabaseRecoveryJob.Run()", ex.ToString());

                ExecutionStatus.Status = RecoveryStatus.Failure;
                ExecutionStatus.PercentageExecution = 0;//[M_NOTE] rudementary logic, change this 
                ExecutionStatus.MessageTime = DateTime.Now;
                ExecutionStatus.Message = "Failed to Restore of " + Database;

                if (ProgressHandler != null)
                    ProgressHandler.SubmitRecoveryState(ExecutionStatus);
            }
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("DataRestore.Run()", "status sent");
            ExecutionStatus.Status = RecoveryStatus.Completed;
            ExecutionStatus.PercentageExecution = 1;//[M_NOTE] rudementary logic, change this 
            ExecutionStatus.MessageTime = DateTime.Now;
            ExecutionStatus.Message = "Completed Restore of " + Database;
            if (ProgressHandler != null)
                ProgressHandler.SubmitRecoveryState(ExecutionStatus);
            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                LoggerManager.Instance.RecoveryLogger.Info("DataRestore.Run()", "Complete");
        }

        internal override object JobStatistics()
        {
            return ExecutionStatus;
        }
        #endregion

        #region helper method
        // manages status and adds data to collection
        private void InsertData(DataSlice slice)
        {
            List<string> collectionsPersisted = new List<string>();// list of collections that have been persisted, this is useful for status
            try
            {
                if (slice.Data != null)
                {
                    ArrayList documentsList = CompactBinaryFormatter.FromByteBuffer(slice.Data, string.Empty) as ArrayList;

                    InsertDocumentsOperation insertOperation = new InsertDocumentsOperation();
                    insertOperation.Documents = documentsList.Cast<IJSONDocument>().ToList();
                    insertOperation.Database = Database;
                    insertOperation.Collection = slice.SliceHeader.Collection;

                    if (documentsList.Count != slice.SliceHeader.DataCount)
                    {
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("DatabaseRecoveryJob.Run()", "Document not same original: " + slice.SliceHeader.DataCount + " restored : " + documentsList.Count);
                    }
                    int i = 0;
                    bool success = false;

                    while (i < 3 && !success)
                    {
                        try
                        {
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                LoggerManager.Instance.RecoveryLogger.Info("DatabaseRecoveryJob.Run()", slice.SliceHeader.Collection + "_" + slice.SliceHeader.DataCount + " Insrt op" + i + " send at " + DateTime.Now);

                            InsertDocumentsResponse insertResponse = (InsertDocumentsResponse)_context.TopologyImpl.InsertDocuments(insertOperation);

                            if (!insertResponse.IsSuccessfull)
                            {
                                i++;
                                success = false;
                                _failedDocuments = _failedDocuments.Union(insertResponse.FailedDocumentsList).ToList();

                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("DatabaseRecoveryJob.Run()", "ErrorCode: " + insertResponse.ErrorCode + "\t Error: " + insertResponse.Error + " \tFailed Documents Count: " + _failedDocuments.Count);

                                // will think of something to do with this failure
                                List<IJSONDocument> retryList = new List<IJSONDocument>();
                                List<IJSONDocument> originalList = insertOperation.Documents.ToList();
                                foreach (FailedDocument failedDoc in insertResponse.FailedDocumentsList)
                                {
                                    foreach (IJSONDocument orgDoc in originalList)
                                    {
                                        if (orgDoc.Key.Equals(failedDoc.DocumentKey))
                                        {
                                            retryList.Add(orgDoc);
                                        }
                                    }
                                }

                                insertOperation.Documents = retryList;
                            }
                            else
                                success = true;
                        }
                        catch (Exception exp)
                        {
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                LoggerManager.Instance.RecoveryLogger.Error("DatabaseRecoveryJob.Run() ", slice.SliceHeader.Collection + " " + exp.ToString());
                            i++;
                            success = false;
                            // Extreme Hack
                            Thread.Sleep(5000);
                        }
                    }
                }
                else
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("DatabaseRecoveryJob.Restore()", "slice.Data is null " + slice.HeaderSize.ToString());

                if (!collectionsPersisted.Contains(slice.SliceHeader.Collection))
                {
                    collectionsPersisted.Add(slice.SliceHeader.Collection);

                    ExecutionStatus.Status = RecoveryStatus.Executing;
                    ExecutionStatus.PercentageExecution = collectionsPersisted.Count / Collections.Count;//[M_NOTE] rudementary logic, change this 
                    ExecutionStatus.MessageTime = DateTime.Now;
                    ExecutionStatus.Message = "Restoring " + Database + "_" + slice.SliceHeader.Collection;
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("DatabaseRecoveryJob.Run() ", slice.SliceHeader.Collection + " " + ex.ToString());
            }
        }
        #endregion
    }
}
