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

namespace Alachisoft.NosDB.Common.Stats
{
    /// <summary>
    /// Enumeration that defines the runtime status of node.
    /// </summary>
    public class NodeStatus
    {
        /// <summary> The node is in initialization phase, possible state transfer.</summary>
        public const byte None = 0;
        /// <summary> The node is in initialization phase, possible state transfer.</summary>
        public const byte Initializing = 1;
        /// <summary> The node is fully functional. </summary>
        public const byte Running = 2;
        /// <summary> The node is stopped. </summary>
        public const byte Stopped = 4;
        /// <summary> The node is running & in state transfer functional. </summary>
        public const byte InStateTxfer = 8;
    }

//    /// <summary>
//    /// Enumeration that defines the runtime status of a collection.
//    /// </summary>
//    public class NodeStatus
//    {
//        /// <summary> The node is in initialization phase.</summary>
//        public const byte INITIALIZING = 1;
//        /// <summary> The node is fully functional. </summary>
//        public const byte RUNNING = 2;
//        /// <summary> The node is stopping. </summary>
//        public const byte STOPPING = 3;
//        /// <summary> The node is being stopped. </summary>
//        public const byte STOPPED = 4;
//        /// <summary> The node is in state transfer. </summary>
//        public const byte STATETXFER = 5;
//    }
}