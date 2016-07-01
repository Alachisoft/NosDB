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
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    internal class RecoveryIOReader : PersistenceIOBase
    {
        internal RecoveryIOReader(string name, string role)
            : base(name, role)
        { }

        #region PersistenceIOBase
        internal override bool Initialize(PersistenceContext context)
        {
            return base.Initialize(context);
        }

        internal override void Run()
        {
            // 1. based on job type, select file name
            // read that file
            // and then read the other file and viola
            try
            {
                switch (Context.JobType)
                {
                    case RecoveryJobType.ConfigRestore:
                   
                        #region config restore
                        foreach (BackupFile file in Context.FileList)
                        {
                            if (file.Name.Contains(RecoveryFolderStructure.CONFIG_SERVER))
                            {
                                IDictionary<long, DataSlice> sliceMap = file.RecreateMetaInfo();
                                foreach (DataSlice sliceID in sliceMap.Values)
                                {
                                    // Add data to shared queue
                                    Context.SharedQueue.Add(file.ReadDataSlice(sliceID));
                                }

                                file.Close();
                                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                                    LoggerManager.Instance.RecoveryLogger.Info("RecoveryIOReader.Run()", "Config file reading complete");
                            }
                        }
                        #endregion
                        break;
                                         
                    case RecoveryJobType.DataRestore:
                        #region DataRestore
                        //1. Select data files
                        List<BackupFile> dataFile = Context.FileList.Where(x => !x.Name.Contains(RecoveryFolderStructure.OPLOG)).ToList<BackupFile>();
                        //2. iterate and produce them completely
                        foreach (BackupFile file in dataFile)
                        {
                            IDictionary<long, DataSlice> sliceMap = file.RecreateMetaInfo();
                            Dictionary<string, string[]> inner = Context.PersistenceConfiguration.DbCollectionMap.Where(x => x.Key.Equals(Context.ActiveDB)).Select(x => x.Value).First();
                            string[] collectionList = inner.First().Value;
                            //2. based on config select slices to read 
                            foreach (string collection in collectionList)
                            {
                                // get all slices holding data of collection to be restored
                                List<DataSlice> _slicesToRestore = sliceMap.Where(x => x.Value.SliceHeader.Collection.Equals(collection)).Select(x => x.Value).ToList<DataSlice>();

                                foreach (DataSlice sliceID in _slicesToRestore)
                                {
                                    // Add data to shared queue
                                    Context.SharedQueue.Add(file.ReadDataSlice(sliceID));
                                }
                            }
                            file.Close();
                        }
                        //3. inform consuming job of complete iteration
                        DataSlice commandSlice = new DataSlice(999998);
                        commandSlice.SliceHeader.Collection = "Complete";
                        commandSlice.SliceHeader.Database = "";
                        commandSlice.SliceHeader.Cluster = "";
                        commandSlice.SliceHeader.ContentType = DataSliceType.Command;
                        commandSlice.Data = CompactBinaryFormatter.ToByteBuffer("Complete_Adding_Data", string.Empty);
                        Context.SharedQueue.Add(commandSlice);
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("RecoveryIOReader.Run()", "Db file reading complete");
                        //4. wait for message to produce next
                        while (!Context.SharedQueue.PauseProducing)
                        {
                            // wait with time out
                        }

                        // check if producing allowed
                        if (!Context.SharedQueue.PauseProducing)
                        {

                        }
                        
                        if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                            LoggerManager.Instance.RecoveryLogger.Info("RecoveryIOReader.Run()", "oplog file reading complete");
                        #endregion
                        break;
                    
                }

                // Add command slice
                DataSlice finalSlice = new DataSlice(999999);
                finalSlice.SliceHeader.Collection = "Complete";
                finalSlice.SliceHeader.Database = "";
                finalSlice.SliceHeader.Cluster = "";
                finalSlice.SliceHeader.ContentType = DataSliceType.Command;
                finalSlice.Data = CompactBinaryFormatter.ToByteBuffer("Complete_Adding", string.Empty);
                Context.SharedQueue.Add(finalSlice);
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("RecoveryIOReader.Run()", "Complete status set");
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryIOReader.Run()", exp.ToString());
            }
        }
        #endregion
    }
}
