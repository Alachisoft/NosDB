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
using Alachisoft.NosDB.Common.ErrorHandling;
using System;
using System.Runtime.Serialization;
using System.Security;

namespace Alachisoft.NosDB.Common.Exceptions
{
    [Serializable]
    public class DatabaseException : Exception,ISerializable
    {

        /// <summary>
        /// Gets/Sets the error code 
        /// </summary>
        public virtual int ErrorCode { get; set; }

        /// <summary>
        /// Gets/Sets the parameter for contextual information
        /// </summary>
        public string[] Parameters { get; set; }

        [SecuritySafeCritical]
        public DatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetInt32("ErrorCode");
            //Parameters = info.GetValue("Parameters", typeof(string[])) as string[];
        }

        public DatabaseException() : base()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(int errorCode) : base(ErrorMessages.GetErrorMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        public DatabaseException(int errorCode, string[] parameters)
            : base(ErrorMessages.GetErrorMessage(errorCode, parameters))
        {
            ErrorCode = errorCode;
            Parameters = parameters;
        }

        public DatabaseException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public DatabaseException(int errorCode, string message, string[] paramters) : base(message)
        {
            this.ErrorCode = errorCode;
            this.Parameters = paramters;
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DatabaseException(int errorCode, string message, Exception innerException)
            : base(ErrorMessages.GetErrorMessage(errorCode) + " - " + message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public DatabaseException(int errorCode, string message, Exception innerException, string[] paramters)
            : base(ErrorMessages.GetErrorMessage(errorCode, paramters) + message ?? " - " + message, innerException)
        {
            this.ErrorCode = errorCode;
            this.Parameters = paramters;
        }

        public override string ToString()
        {
            return "Error Code: "+ ErrorCode + " - " + base.ToString();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", ErrorCode);
            info.AddValue("Parameters", Parameters);
        }
    }
}
