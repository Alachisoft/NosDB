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
using System.Linq;
using System.Text;
using System.Threading;

namespace Alachisoft.NosDB.Common.Threading
{
    public class ClrThreadPool:IThreadPool
    {
        WaitCallback _waitCallback;
        public bool Initialize()
        {
            _waitCallback = new WaitCallback(ExecuteTaskInternal);
            return true;
        }

        public bool ExecuteTask(IThreadPoolTask task)
        {
            DateTime start = DateTime.Now;

            bool result =ThreadPool.QueueUserWorkItem(_waitCallback, task);

            TimeSpan diff = DateTime.Now - start;

            if (diff.TotalSeconds > 5)
                Alachisoft.NosDB.Common.AppUtil.LogEvent("ThreadPool took " + diff.TotalSeconds + " to enquue", System.Diagnostics.EventLogEntryType.Error);

            return result;
        }

        private void ExecuteTaskInternal(object state)
        {
            if (state != null)
                ((IThreadPoolTask)state).Execute();
        }
    }
}
