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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Recovery.Operation;
using Alachisoft.NosDB.Common.Server;
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
    internal class DatabaseBackupJob : RecoveryJobBase
    {
        private NodeContext _context = null;

        private CollectionReader _dbReader = null;

        internal DatabaseBackupJob(string identifier, NodeContext context, string database, List<string> collections, RecoveryPersistenceManager _manager, string cluster)
            : base(identifier, database, collections, RecoveryJobType.DataBackup, _manager, cluster)
        {
            _context = context;
        }

        #region Overridden methods
        internal override bool Initialize(RecoveryOperation operation)
        {

            return base.Initialize(operation);
        }

        internal override void Run()
        {
            int collectionItterated = 1;
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = _context.LocalShardName != null ? _context.LocalShardName : "", DatabaseName = Database != null ? Database : "" });
            try
            {
                IDatabaseStore dbInstance = _context.DatabasesManager.GetDatabase(Database);

                foreach (string _collection in Collections)
                {
                    if (((Storage.DatabaseStore)dbInstance).Collections.ContainsKey(_collection))
                    {
                        //Add custom object

                        Query defaultQuery = new Query();

                        defaultQuery.QueryText = "Select * from ";

                        //M_Note: precausion used incase user goes bonkers whilst naming his collections
                        if (_collection.Contains("\""))
                        {
                            defaultQuery.QueryText += "$" + _collection + "$";
                        }
                        else
                        {
                            defaultQuery.QueryText += "\"" + _collection + "\"";
                        }

                        ReadQueryOperation readQueryOperation = new ReadQueryOperation();
                        readQueryOperation.Database = Database.ToString();
                        readQueryOperation.Collection = _collection;
                        readQueryOperation.Query = defaultQuery;

                        ArrayList docList = new ArrayList();
                        long currentSize = 0;

                        DataSlice _activeSlice = PersistenceManager.ActiveContext.GetBackupFile(Database).CreateNewDataSlice();
                        _activeSlice.SliceHeader.Collection = _collection;
                        _activeSlice.SliceHeader.Database = Database;
                        _activeSlice.SliceHeader.Cluster = Cluster;
                        _activeSlice.SliceHeader.ContentType = DataSliceType.Data;

                        try
                        {
                            ReadQueryResponse readQueryResponse = (ReadQueryResponse)dbInstance.ExecuteReader(readQueryOperation);

                            if (readQueryResponse.IsSuccessfull)
                            {
                                _dbReader = new CollectionReader((DataChunk)readQueryResponse.DataChunk, _context.TopologyImpl, Database, _collection);
                                //create data slice to be written in the common queue
                                while (_dbReader != null && _dbReader.ReadNext() && _dbReader.GetDocument() != null)
                                {
                                    //get document and create chunk and add to shared storage 
                                    IJSONDocument _doc = _dbReader.GetDocument();

                                    // verify size
                                    if (currentSize + _doc.Size <= _activeSlice.Capcity)
                                    {
                                        docList.Add(_doc);
                                        currentSize += _doc.Size + 2;// Hack to accomodate the 2 bytes serialization is going add
                                    }
                                    else
                                    {
                                        DataSlice _nxtSlice = PersistenceManager.ActiveContext.GetBackupFile(Database).CreateNewDataSlice();
                                        _nxtSlice.SliceHeader.Collection = _collection;
                                        _nxtSlice.SliceHeader.Database = Database;
                                        _nxtSlice.SliceHeader.Cluster = Cluster;
                                        _nxtSlice.SliceHeader.ContentType = DataSliceType.Data;

                                        _activeSlice.Data = CompactBinaryFormatter.ToByteBuffer(docList, string.Empty);
                                        _activeSlice.SliceHeader.DataCount = docList.Count;
                                        _activeSlice.SliceHeader.TotalSize = _activeSlice.Data.LongLength;

                                        // Add to shared queue
                                        PersistenceManager.SharedQueue.Add(_activeSlice);
                                        _activeSlice = _nxtSlice;
                                        docList.Clear();

                                        docList.Add(_doc);
                                        currentSize = 0;
                                        currentSize += _doc.Size;
                                    }
                                }
                                _dbReader.Dispose();
                                // write final data set
                                if (docList.Count > 0)
                                {
                                    _activeSlice.Data = CompactBinaryFormatter.ToByteBuffer(docList, string.Empty);
                                    _activeSlice.SliceHeader.DataCount = docList.Count;
                                    _activeSlice.SliceHeader.TotalSize = _activeSlice.Data.LongLength;
                                    // Add to shared queue
                                    PersistenceManager.SharedQueue.Add(_activeSlice);
                                    docList.Clear();
                                }

                                // submit status
                                ExecutionStatus.Status = RecoveryStatus.Executing;
                                ExecutionStatus.PercentageExecution = (collectionItterated / Collections.Count);//[M_NOTE] rudementary logic, change this 
                                ExecutionStatus.MessageTime = DateTime.Now;
                                ExecutionStatus.Message = "Completed Backup of " + Database + "_" + _collection + " : " + _collection;
                                collectionItterated++;

                                if (ProgressHandler != null)
                                    ProgressHandler.SubmitRecoveryState(ExecutionStatus);
                            }
                            else
                                throw new Exception("Operation failed Error code: " + readQueryResponse.ErrorCode);
                        }
                        catch (Exception ex)
                        {
                            if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                                LoggerManager.Instance.RecoveryLogger.Error("DatabaseBackupJob.Run()", Database + " : " + ex.ToString());
                            collectionItterated--;

                        }
                    }
                }
                // Add command slice
                DataSlice finalSlice = new DataSlice(999999);
                finalSlice.SliceHeader.Collection = "Complete";
                finalSlice.SliceHeader.Database = Database;
                finalSlice.SliceHeader.Cluster = Cluster;
                finalSlice.SliceHeader.ContentType = DataSliceType.Command;
                finalSlice.Data = CompactBinaryFormatter.ToByteBuffer("Data_Complete_Adding", string.Empty);
                PersistenceManager.SharedQueue.Add(finalSlice);


                while (!PersistenceManager.SharedQueue.Consumed)
                {
                    // wait till all data has been consumed and written
                    //M_TODO:
                    // Add timeout interval for file writing, incase the data is not being consumed and timeout span has been reached, break the loop and DIE!!!
                }

                if (PersistenceManager.SharedQueue.Consumed)
                {
                    // submit status
                    ExecutionStatus.Status = RecoveryStatus.Completed;
                    ExecutionStatus.PercentageExecution = 1;//[M_NOTE] rudementary logic, change this 
                    ExecutionStatus.MessageTime = DateTime.Now;
                    ExecutionStatus.Message = "Completed Backup of " + Database;
                }
                else
                {
                    // submit status
                    ExecutionStatus.Status = RecoveryStatus.Failure;
                    ExecutionStatus.PercentageExecution = (collectionItterated / Collections.Count);//[M_NOTE] rudementary logic, change this 
                    ExecutionStatus.MessageTime = DateTime.Now;
                    ExecutionStatus.Message = "Failed Backup of " + Database;
                }

                if (ProgressHandler != null)
                    System.Threading.Tasks.Task.Factory.StartNew(() => ProgressHandler.SubmitRecoveryState(ExecutionStatus));
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("DatabaseBackupJob.Run()", Database + "Completed");

            }
            catch (ThreadAbortException)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsDebugEnabled)
                    LoggerManager.Instance.RecoveryLogger.Debug("DatabaseBackupJob.Run()", "Thread stopped");
                Thread.ResetAbort();
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("DatabaseBackupJob.Run()", Database + " : " + exp.ToString());

                ExecutionStatus.Status = RecoveryStatus.Failure;
                ExecutionStatus.PercentageExecution = (collectionItterated / Collections.Count);//[M_NOTE] rudementary logic, change this 
                ExecutionStatus.MessageTime = DateTime.Now;
                ExecutionStatus.Message = "Failed Backup of " + Database;

                if (ProgressHandler != null)
                    System.Threading.Tasks.Task.Factory.StartNew(() => ProgressHandler.SubmitRecoveryState(ExecutionStatus));
            }
        }

        internal override object JobStatistics()
        {
            return ExecutionStatus;
        }

        public override void Dispose()
        {
            try
            {
                if (_dbReader != null)
                    _dbReader.Dispose();
            }
            catch (Exception exp)
            { }
            base.Dispose();

        }
        #endregion

    }
}
