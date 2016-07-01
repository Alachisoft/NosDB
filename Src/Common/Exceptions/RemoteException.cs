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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Exceptions
{
    /// <summary>
    /// Exception thrown on remote host during an RPC call is wrapped in this class
    /// </summary>
    [Serializable]
    public class RemoteException :DatabaseException
    {

        public RemoteException()
        { 
        }
        /// <summary> 
        /// overloaded constructor, manual serialization. 
        /// </summary>
        protected RemoteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public RemoteException(Exception innerException)
            : base(innerException.Message, innerException)
        {

        }
        public RemoteException(string message,Exception innerException):base(message,innerException)
        {

        }

        public override int ErrorCode
        {
            get
            {
                if (InnerException != null && InnerException is DatabaseException)
                    return ((DatabaseException)InnerException).ErrorCode;

                return base.ErrorCode;
            }
            set
            {
                if (InnerException != null && InnerException is DatabaseException)
                    ((DatabaseException)InnerException).ErrorCode = value;

                base.ErrorCode = value;
            }
        }
   
    }
}
