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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Alachisoft.NosDB.Common.DataStructures.Clustered
{
    internal sealed class VectorDebugView
    {
        private IDictionary dictionary;

        internal class DebugBucket
        {
            private object _key;
            private object _value;

            public DebugBucket(object key, object value)
            {
                _key = key;
                _value = value;
            }

            public object Key
            {
                get { return _key; }
            }

            public object Value
            {
                get { return _value; }
            }

            public override string ToString()
            {
                return _key.ToString() + " : " + _value.ToString();
            }
        }

        public VectorDebugView(IDictionary vector)
        {
            dictionary = vector;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<DebugBucket> Values
        {
            get {
                return from object key in dictionary.Keys select new DebugBucket(key,dictionary[key]);
            }
        }


    }
}