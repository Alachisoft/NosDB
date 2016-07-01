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
using System.Text;
using Alachisoft.NosDB.Common.ErrorHandling;
using System;
using System.Runtime.Serialization;
using System.Security;

namespace Alachisoft.NosDB.Common.Exceptions
{
    [Serializable]
    public class ManagementException : Exception, ISerializable
    {
        /// <summary>
        /// Gets/Sets the error code 
        /// </summary>
        public virtual int ErrorCode { get; private set; }

        /// <summary>
        /// Gets/Sets the parameter for contextual information
        /// </summary>
        public string[] Parameters { get; private set; }

        /// <summary>
        /// Retuns a Flag to indicate if Error Code specified
        /// </summary>
        public bool IsErrorCodeSpecified { get; private set; }

        private string _innerExceptionStackTrace;


        [SecuritySafeCritical]
        public ManagementException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetInt32("ErrorCode");
            try
            {
                Parameters = info.GetValue("Parameters", typeof(Array)) as string[];
                IsErrorCodeSpecified = info.GetBoolean("IsErrorCodeSpecified");
                _innerExceptionStackTrace = info.GetString("InnerStackTrace");

            }
            catch (Exception) { }
        }

        public ManagementException():base() { } 

        public ManagementException(string message) : base(message) { }

        public ManagementException(string message, Exception innerException) : base(message) { _innerExceptionStackTrace = innerException != null ? innerException.StackTrace : null; }

        public ManagementException(int errorCode) : base(ErrorMessages.GetErrorMessage(errorCode))
        {
            ErrorCode = errorCode;
            this.IsErrorCodeSpecified = true;
        }

        public ManagementException(int errorCode, string[] parameters)
            : base(ErrorMessages.GetErrorMessage(errorCode, parameters))
        {
            ErrorCode = errorCode;
            Parameters = parameters;
            this.IsErrorCodeSpecified = true;
        }

        public ManagementException(int errorCode,string[] parameters, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.Parameters = parameters;
            this.IsErrorCodeSpecified = true;
        }

        public ManagementException(int errorCode, string message, Exception innerException)
            : base(ErrorMessages.GetErrorMessage(errorCode) + " - " + message)
        {
            this.ErrorCode = errorCode;
            this.IsErrorCodeSpecified = true;
            _innerExceptionStackTrace = innerException != null ? innerException.StackTrace : null;

        }

        public ManagementException(int errorCode, string message, Exception innerException, string[] paramters)
            : base(ErrorMessages.GetErrorMessage(errorCode, paramters) + message)
        {
            this.ErrorCode = errorCode;
            this.Parameters = paramters;
            this.IsErrorCodeSpecified = true;
            _innerExceptionStackTrace = innerException != null ? innerException.StackTrace : null;

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (IsErrorCodeSpecified)
            {
                sb.Append("Error Code: " + ErrorCode);
            }

            sb.Append(base.ToString());

            if (!string.IsNullOrEmpty(_innerExceptionStackTrace))
            {
                sb.AppendLine("Inner exception stack-trace -------------------------------------");
                sb.AppendLine(_innerExceptionStackTrace);
            }

            return sb.ToString();

        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", ErrorCode);
            info.AddValue("Parameters", Parameters);
            info.AddValue("IsErrorCodeSpecified", IsErrorCodeSpecified);
            info.AddValue("InnerStackTrace", _innerExceptionStackTrace);

        }
    }
}
