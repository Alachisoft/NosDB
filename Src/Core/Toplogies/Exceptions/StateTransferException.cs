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
using System.Security;
using System.Security.Permissions;
using System.Text;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;

namespace Alachisoft.NosDB.Core.Toplogies.Exceptions
{
    [Serializable]
    public class StateTransferException : DatabaseException
    {     
        [SecuritySafeCritical]
        public StateTransferException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetInt32("ErrorCode");
            //Parameters = info.GetValue("Parameters", typeof(string[])) as string[];
        }

        /// <summary> 
        /// default constructor. 
        /// </summary>
        internal StateTransferException(): base()
        {
        }

        public StateTransferException(string message) : base(message)
        {
        }

        public StateTransferException(int errorCode) : base(ErrorMessages.GetErrorMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        public StateTransferException(int errorCode, string[] parameters)
            : base(ErrorMessages.GetErrorMessage(errorCode, parameters))
        {
            ErrorCode = errorCode;
            Parameters = parameters;
        }

        public StateTransferException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public StateTransferException(int errorCode, string message, string[] paramters) : base(message)
        {
            this.ErrorCode = errorCode;
            this.Parameters = paramters;
        }

        public StateTransferException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public StateTransferException(int errorCode, string message, Exception innerException)
            : base(ErrorMessages.GetErrorMessage(errorCode) + " - " + message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public StateTransferException(int errorCode, string message, Exception innerException, string[] paramters)
            : base(ErrorMessages.GetErrorMessage(errorCode, paramters) + message ?? " - " + message, innerException)
        {
            this.ErrorCode = errorCode;
            this.Parameters = paramters;
        }



        #region /                 --- ISerializable ---           /        

        ///// <summary>
        ///// manual serialization
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="context"></param>
        //[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //}

        #endregion
    }
}
