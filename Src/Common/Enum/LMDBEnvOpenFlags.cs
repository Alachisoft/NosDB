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


namespace Alachisoft.NosDB.Common.Enum
{
    [Flags]
    public enum LMDBEnvOpenFlags
    {
        None = 0,
        FixedMap = 1,
        NoSubDir = 16384,
        NoSync = 65536,
        ReadOnly = 131072,
        NoMetaSync = 262144,
        WriteMap = 524288,
        MapAsync = 1048576,
        NoThreadLocalStorage = 2097152,
        NoLock = 4194304,
        NoReadAhead = 8388608,
        NoMemoryInitialization = 16777216,
    }
}
