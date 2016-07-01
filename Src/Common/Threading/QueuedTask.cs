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
using System.Threading;

namespace Alachisoft.NosDB.Common.Threading
{
    public class QueuedTask
    {
        private Task _task;
        private long _interval;
        private Timer _timer;



        public QueuedTask(Task task)
        {
            _task = task;
            _interval = _task.GetNextInterval();
            _timer = new Timer(Execute, null, 0, _interval);
        }

        private void Execute(object state)
        {
            if (!_task.IsCancelled())
            {
                _task.Run();
                long newInterval = _task.GetNextInterval();
                if (newInterval != _interval)
                {
                    _interval = newInterval;
                    _timer.Change(0, _interval);
                }
            }
            else
            {
                _timer.Dispose();
            }
        }

        public bool IsCancelled{get { return _task.IsCancelled(); }}
    }
}