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
using System.Diagnostics;
using System.Runtime;

namespace Alachisoft.NosDB.Common.DataStructures.Clustered
{
    [DebuggerDisplay("{value}", Name = "[{key}]", Type = "")]
    public class KeyValuePairs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object key;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;
        public object Key
        {
#if DEBUG
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
                get
            {
                return this.key;
            }
        }
        public object Value
        {
#if DEBUG
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
                get
            {
                return this.value;
            }
        }
#if DEBUG
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public KeyValuePairs(object key, object value)
        {
            this.value = value;
            this.key = key;
        }
    }
}