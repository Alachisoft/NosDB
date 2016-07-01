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
namespace Alachisoft.NosDB.Common.DataStructures
{
    /// <summary>
    /// Enumeration that defines the runtime status of a database.
    /// </summary>
    public class DatabaseStatus
    {
        /// <summary> The database is in initialization phase.</summary>
        public const byte INITIALIZING = 1;
        /// <summary> The database is fully functional. </summary>
        public const byte RUNNING = 2;
        /// <summary> The database is being disposed. </summary>
        public const byte DISPOSING = 4;
        /// <summary> The database is being dropped. </summary>
        public const byte DROPPING = 8;
    }
}
