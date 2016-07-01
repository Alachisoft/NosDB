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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    /// <summary>
    /// Consumer behaviour againsts a sepecific recovery job
    /// </summary>
    internal class RecoveryIOWriter : PersistenceIOBase
    {
        internal RecoveryIOWriter(string name, string role)
            : base(name, role)
        { }

        #region PersistenceIOBase
        internal override bool Initialize(PersistenceContext context)
        {
            return base.Initialize(context);
        }

        internal override void Run()
        {
            try
            {
                foreach (DataSlice info in Context.SharedQueue.GetConsumingEnumerable(Context.SharedQueue.CancelToken.Token))
                {
                    if (info.SliceHeader.ContentType != DataSliceType.Command)
                    {
                        if (!Context.SharedQueue.IsAddingCompleted)
                        {
                            if (info != null)
                            {

                                WriteData(info);
                            }
                        }
                        else
                        {
                            if (info != null)
                            {
                                WriteData(info);
                            }
                        }
                    }
                    else
                    {
                        if(info.Data != null)
                        {
                            string message = CompactBinaryFormatter.FromByteBuffer(info.Data, string.Empty) as string;
                            if (message.Contains("Data_Complete") || message.Contains("Diflog_Complete"))
                            {
                                Context.SharedQueue.Consumed = true;
                            }
                            else if (message.Contains("Oplog_Complete") || message.Contains("Config_Complete"))
                                break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsDebugEnabled)
                    LoggerManager.Instance.RecoveryLogger.Debug("RecoveryIOWriter.Run()", "Thread stopped");

                Thread.ResetAbort();
            }
            catch (OperationCanceledException)
            {
                // read all remaining documnets if any
                #region Check for any remaining data slices
                DataSlice info;
                // dequeue data slices from queue 
                while (Context.SharedQueue.TryTake(out info, 30000))// default timeout is 5 min= 300000
                {
                    if (info.SliceHeader.ContentType != DataSliceType.Command)
                    {
                        if (info != null)
                        {
                            WriteData(info);
                        }
                    }
                    else
                        break;
                }

                if (info != null)
                {
                    if (info.SliceHeader.ContentType != DataSliceType.Command)
                    {
                        // check if timedout
                        if (!Context.SharedQueue.IsAddingCompleted)
                        {
                            throw new TimeoutException("Timed out while writing file");
                        }
                        else
                        {
                            Context.SharedQueue.Consumed = true;
                        }
                    }
                    else
                        Context.SharedQueue.Consumed = true;
                }
                else
                    if (!Context.SharedQueue.IsAddingCompleted)
                    {
                        throw new TimeoutException("Timed out while writing file");
                    }
                    else
                    {
                        Context.SharedQueue.Consumed = true;
                    }

                #endregion
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryIOWriter.Run()", ex.ToString());
            }
            finally
            {
                Context.SharedQueue.Consumed = true;
            }
        }
        #endregion

        #region helper method
        private void WriteData(DataSlice slice)
        {
            string fileName;
            if (slice.Data != null)
            {
                try
                {
                    if (slice.SliceHeader.ContentType == Common.Recovery.DataSliceType.Data)
                        fileName = slice.SliceHeader.Database;
                    else
                        fileName = RecoveryFolderStructure.CONFIG_SERVER;

                    BackupFile file = Context.GetBackupFile(fileName);
                    if (file != null)
                        file.SaveDataSlice(slice);
                    else
                        throw new NullReferenceException("File " + fileName + " cannot be null");
                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("RecoveryIOWriter.Write()", exp.ToString());
                }
            }
        }
        #endregion


    }
}
