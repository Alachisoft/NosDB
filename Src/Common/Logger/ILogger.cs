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
using System.Text;
using System.Collections;

namespace Alachisoft.NosDB.Common.Logger
{
    public interface ILogger 
    {
        bool IsInfoEnabled { get; }

        bool IsErrorEnabled { get; }

        bool IsWarnEnabled { get; }

        bool IsDebugEnabled { get; }

        bool IsFatalEnabled { get; }

        /// <summary>
        /// Please don't use this overload
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);
        /// <summary>
        /// Debug method used for detail logging. 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="message"></param>
        void Debug(string module, string message);

        /// <summary>
        /// Please don't use this overload 
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
        /// <summary>
        /// Info method used for critical logging.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="message"></param>
        void Info(string module, string message);

        # region Error Logging methods
        /// <summary>
        /// Please don't use this overload
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);

        void Warn(string module, string message);

        /// <summary>
        /// Please don't use this overload
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);

        void Error(string module, string message);

        /// <summary>
        /// Please don't use this overload
        /// </summary>
        /// <param name="ex"></param>
        void Error(Exception ex);

        void Error(string module, Exception ex);

        void Error(string module, string message, Exception ex);

        /// <summary>
        /// Please don't use this overload
        /// </summary>
        /// <param name="message"></param>
        void Fatal(string message);

        void Fatal(string module, string message);
        # endregion
        
        void Flush();

    }
}
