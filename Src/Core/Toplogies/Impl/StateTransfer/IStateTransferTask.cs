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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    public interface IStateTransferTask : IDisposable
    {

        /// <summary>
        /// Is Task Running 
        /// </summary>
        Boolean IsRunning { get; }

        /// <summary>
        /// Task Status 
        /// </summary>
        StateTxfrStatus Status { set; get; }


        /// <summary>
        /// Start State Transfer Task 
        /// </summary>
        /// <param name="map"> Information for task beig started </param>
        void Initialize(ICollection map, StateTransferType transferType, bool forLocal = false);

        /// <summary>
        /// Start State Transfer Task 
        /// </summary>
        /// <param name="map"> Information for task beig started </param>
        void Start();

        /// <summary>
        /// Pause State Transfer Task
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop State Transfer Task
        /// </summary>
        void Stop();

        /// <summary>
        /// On Shard Connected 
        /// </summary>
        /// <param name="shard"></param>
        void OnShardConnected(NodeIdentity shard);
    }   
}
