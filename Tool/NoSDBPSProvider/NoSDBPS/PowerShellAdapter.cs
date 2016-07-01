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
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading;

namespace Alachisoft.NosDB.NosDBPS
{

    public class PowerShellAdapter
    {
        private Cmdlet Cmdlet { get; set; }
        private BlockingCollection<object> Queue { get; set; }
        private object LockToken { get; set; }
        public bool Finished { get; set; }

        public PowerShellAdapter(Cmdlet cmdlet)
        {
            this.Cmdlet = cmdlet;
            this.LockToken = new object();
            this.Queue = new BlockingCollection<object>();
            this.Finished = false;
        }

        public void Listen()
        {
            //ProgressRecord progress = new ProgressRecord(1, "Counting to 100", " ");
            while (!Finished)
            {
                while (Queue.Count > 0)
                {
                    try
                    {
                        lock (LockToken)
                            Cmdlet.CommandRuntime.WriteObject(Queue.Take());
                    }
                    catch (Exception e)
                    {
                        if (e is PipelineStoppedException)
                        {
                            Finished = true;

                            break;
                        }
                        throw e;
                    }

                }

                Thread.Sleep(1000);
            }
        }

        public void WriteObject(object obj)
        {
            lock (LockToken)
                Queue.Add(obj);
        }
    }


}
