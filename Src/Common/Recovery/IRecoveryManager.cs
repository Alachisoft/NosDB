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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Recovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Recovery
{
    // manages backup and restore jobs
    public interface IRecoveryManager:IRecoveryCommunicationInitiater,IRecoveryCommunicationHandler
    {
        RecoveryOperationStatus SubmitRecoveryJob(RecoveryConfiguration config,object additionalParams);
        RecoveryOperationStatus CancelRecoveryJob(string identifier);
        RecoveryOperationStatus[] CancelAllRecoveryJobs();
        ClusteredRecoveryJobState GetJobState(string identifier);
        ClusterJobInfoObject[] GetAllRunningJobs();
        void OnMembershipChanged(ConfigChangeEventArgs args);
        /// <summary>
        /// Returns false if an operation locked due to Recovery 
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        bool IsOperationAllowed(string cluster, string database);
        
        /// <summary>
        /// Performs appropriate operation against any change in configuration
        /// </summary>
        /// <param name="changeArgs"></param>
        void SubmitConfigChanged(object changeArgs);

        void SubmitRecoveryState(object state);

        RecoveryConfiguration GetJobConfiguration(string identifier);
    }
}
