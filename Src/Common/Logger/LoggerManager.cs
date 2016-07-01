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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Logger
{
    public class LoggerManager
    {
        private readonly Dictionary<LoggerNames, ILogger> _loggersPool = new Dictionary<LoggerNames, ILogger>();
        private static LoggerManager _loggerManager;
        /// <summary>
        /// get Instance of Logger Manager.  
        /// </summary>
        public static LoggerManager Instance
        {
            get
            {
                if (_loggerManager == null)
                    _loggerManager = new LoggerManager();
                return _loggerManager;
            }
        }

        #region Named based Logger Instances

        public ILogger CONDBLogger { get { return GetLogger(LoggerNames.CONDBServer); } }

        public ILogger SecurityLogger { get { return GetLogger(LoggerNames.Security); } }

        public ILogger ServerLogger { get { return GetLogger(LoggerNames.Server); } }

        public ILogger ShardLogger { get { return GetLogger(LoggerNames.Shards); } }

        public ILogger REPLogger { get { return GetLogger(LoggerNames.REP); } }

        public ILogger StateXferLogger { get { return GetLogger(LoggerNames.StateXfer); } }

        public ILogger StorageLogger { get { return GetLogger(LoggerNames.Storage); } }

        public ILogger IndexLogger { get { return GetLogger(LoggerNames.Indexing); } }

        public ILogger QueryLogger { get { return GetLogger(LoggerNames.Queries); } }

        public ILogger ClientOPLogger { get { return GetLogger(LoggerNames.ClientOp); } }

        public ILogger ManagementOPLogger { get { return GetLogger(LoggerNames.ManagementOp); } }

        public ILogger RecoveryLogger { get { return GetLogger(LoggerNames.Recovery); } }

        public ILogger ManagerLogger { get { return GetLogger(LoggerNames.Manager); } }

        public ILogger RestApiLogger { get { return GetLogger(LoggerNames.RestApi); } }

        public ILogger EXIMLogger { get { return GetLogger(LoggerNames.EXIM);} }

        #endregion

        public void Close(LoggerNames loggerNameEnum)
        {
            ILogger toClose;
            if (_loggersPool.TryGetValue(loggerNameEnum, out toClose))
            {
                ((Logger)toClose).Close();
                lock(this)
                {
                    _loggersPool.Remove(loggerNameEnum);
                }
            }
        }

        public void Close()
        {
            lock(this)
            {
                foreach (LoggerNames loggerNameEnum in System.Enum.GetValues(typeof(LoggerNames)))
                {
                    ILogger toClose;
                    if (_loggersPool.TryGetValue(loggerNameEnum, out toClose))
                    {
                        ((Logger)toClose).Close();
                        _loggersPool.Remove(loggerNameEnum);
                    }
                }
            }
        }

        public void SetThreadContext(LoggerContext logerContext)
        {
            if (logerContext == null) return;
            Logger.SetThreadContext(logerContext);
        }

        private ILogger GetLogger(LoggerNames logerNameEnum)
        {
            ILogger dbLogger = null;
            try
            {
                lock (_loggersPool)
                {
                    if (!_loggersPool.TryGetValue(logerNameEnum, out dbLogger))
                    { 
                        var logger = new Logger();
                        logger.Getlogger(logerNameEnum);
                        lock (this)
                        {
                            _loggersPool.Add(logerNameEnum, logger);
                        }
                        dbLogger = logger;
                    }
                }
            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return dbLogger;
        }
    }
}
