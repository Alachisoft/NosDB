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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    //M_NOTE: might remove this file, class not used remove it
    internal class RecoveryConfigReader : PersistenceIOBase
    {
        internal RecoveryConfigReader(string name, string role)
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
                // 1. recreate slice meta info
                IDictionary<long, DataSlice> sliceMap = Context.GetBackupFile(RecoveryFolderStructure.CONFIG_SERVER).RecreateMetaInfo();

                //2. based on config select slices to read 
                foreach (DataSlice sliceID in sliceMap.Values)
                {
                    // Add data to shared queue
                    Context.SharedQueue.Add(Context.GetBackupFile(RecoveryFolderStructure.CONFIG_SERVER).ReadDataSlice(sliceID));
                }

                Context.SharedQueue.CompleteAdding();
                // called to notify the consuming thread end of production
                Context.SharedQueue.CancelToken.Cancel();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryConfigReader.Run()", exp.ToString());
            }
        }
        #endregion
    }
}
