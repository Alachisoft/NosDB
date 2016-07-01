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
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    public class DifferentialRestoreJobInfo : ITaskInfo
    {
        private readonly ClusterRecoveryJob _job;      
        private readonly ConfigurationServer _parent;        

        public DifferentialRestoreJobInfo( ClusterRecoveryJob jobInfo,ConfigurationServer configurationServer)
        {
            Cluster = string.Empty;
            Shard = string.Empty;
            _job = jobInfo;
            _parent = configurationServer;
        }

        #region properties
        public ClusterRecoveryJob Job
        {
            get { return _job; }
        } 
        public ConfigurationServer Parent
        {
            get { return _parent; }
        } 
        public String Cluster { get; set; }
        public String Shard { get; set; }
        public TaskType TaskType
        {
            get { return Services.TaskType.DifferentialRestore; }
        }
        #endregion

        #region overriden methods
        public bool IsTaskCompleted()
        {
            bool isTaskCompleted = true;

            if (_job.ShardReceivedState == ShardDifState.none)
            {
                if (_job.LatestResponseTime.HasValue)
                {
                    // if 5 minutes have passed since last message
                    if (_job.LatestResponseTime.Value.AddMinutes(5) > DateTime.UtcNow)
                    {
                        return false;
                    }
                }
                else
                {
                    // if an hour has passed since response
                    if(_job.CreationTime.AddMinutes(60) > DateTime.Now)
                    {
                        return false;
                    }
                }
            }

            return isTaskCompleted;
        }

        public void OnTaskCompleted()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
