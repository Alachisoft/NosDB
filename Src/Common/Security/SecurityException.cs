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
using Alachisoft.NosDB.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security
{
    [Serializable]
    public class SecurityException : DatabaseException, ISerializable
    {
        public SecurityException()
        { }

        [SecuritySafeCritical]
        public SecurityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //Parameters = info.GetValue("Parameters", typeof(string[])) as string[];
        }
        public SecurityException(string message)
            : base(message)
        { }
        public SecurityException(int errorCode)
            :base(errorCode)
        { }

        public SecurityException(int errorCode, string[] parameters)
            : base(errorCode, parameters)
        { }

    }
}
