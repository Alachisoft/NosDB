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
using System.Diagnostics;
using System.IO;
using Alachisoft.NosDB.Common.Configuration;
using log4net.Appender;
using log4net;


namespace Alachisoft.NosDB.Common.Logger
{
    public class Logger : ILogger
    {
        private log4net.ILog _log;
        //private static object mutex = new object();

        static Logger()
        {
            //lock (mutex)
            //{
                string configFilePath = null;

                try
                {

                    //configFilePath = ConfigurationSettings<DBHostSettings>.Current.LogConfiguration;
                if (System.Configuration.ConfigurationManager.AppSettings["LogConfiguration"] != null)
                    configFilePath = System.Configuration.ConfigurationManager.AppSettings["LogConfiguration"];
                if (configFilePath != null)
                {                    
                    if (!File.Exists(configFilePath))
                        throw new Exception("Configuraiton file not found.");
                }
                else
                    throw new Exception("Configuration file not found.");

                System.IO.FileInfo file = new System.IO.FileInfo(configFilePath);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(file);

                }
                catch (Exception ex) 
                {
                    AppUtil.LogEvent("Error While Loading log configuration file "+configFilePath +" "+  ex.Message,EventLogEntryType.Error);
                }
            //}

        }
       
        public  void Getlogger(LoggerNames loggerNameEnum)
        {
            try
            {
                _log = LogManager.GetLogger(loggerNameEnum.ToString());
            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

       

        public void Close()
        {
            try
            {
                log4net.Core.IAppenderAttachable closingAppenders = (log4net.Core.IAppenderAttachable)_log.Logger;
                AppenderCollection collection = closingAppenders.Appenders;
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i] is BufferingForwardingAppender)
                    {
                        //This FLUSH and close the current appenders along with all of its children appenders
                        ((BufferingForwardingAppender)collection[i]).Close();
                    }
                }
                this.RemoveAllAppender();
            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public void Flush()
        {
            try
            {
                IAppender[] logAppenders = _log.Logger.Repository.GetAppenders();

                for (int i = 0; i < logAppenders.Length; i++)
                {
                    if (logAppenders[i] != null)
                    {
                        BufferingAppenderSkeleton buffered = logAppenders[i] as BufferingAppenderSkeleton;
                        if (buffered is BufferingForwardingAppender)
                        {
                            ((BufferingForwardingAppender)buffered).Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        private void RemoveAllAppender()
        {
            try
            {
                log4net.Repository.Hierarchy.Logger l = (log4net.Repository.Hierarchy.Logger)_log.Logger;
                l.RemoveAllAppenders();
            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        #region ILogger Members

        public bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled; }
        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void Debug(string module, string message)
        {
            //int space2 = 40;

            //if (module.Length == 0)
            //    space2 = 4;

            string line = null;

           // line = module.PadRight(space2, ' ') + message;
            line = GetMessage(module, message);

            Debug(line);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Info(string module, string message)
        {
            //int space2 = 40;

            //if (module.Length == 0)
            //    space2 = 4;

            string line = null;

           // line = module.PadRight(space2, ' ') + message;
            line = GetMessage(module, message);

            Info(line);
        }

        
        public void Warn(string message)
        {
            _log.Warn(message);
        }

        public void Warn(string module, string message)
        {

            //int space2 = 40;

            //if (module.Length == 0)
            //    space2 = 4;

            string line = null;

            //line = module.PadRight(space2, ' ') + message;
            line = GetMessage(module, message);

            Warn(line);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }
        
        public void Error(string module, string message)
        {
            //int space2 = 40;

            //if (module.Length == 0)
            //    space2 = 4;

            string line = null;

            //line = module.PadRight(space2, ' ') + message;
            line = GetMessage(module, message);

            Error(line);
        }

        public void Error(Exception ex)
        {
            Error(ex.ToString());
        }

        public void Error(string module, Exception ex)
        {
            Error(module, ex.ToString());
        }

        public void Error(string module, string message, Exception ex)
        {
            //int moduleSpace = 40;
            //int messageSpace = 40;
            
            //if (module.Length == 0)
            //    moduleSpace = 4;

            //if (message.Length == 0)
            //    messageSpace = 4;

            string line = null;

           // line = module.PadRight(moduleSpace, ' ') + message.PadRight(messageSpace, ' ') + ex.ToString();
            line = GetMessage(module, message, ex);

            Error(line);
        }


        private string GetMessage(string module, string message)
        {
            var moduleSpace = 15;
            var moduleFixedLength = 70;
            //if (module.Length > 0)
                //moduleSpace += moduleFixedLength;

                //module += new string(' ', moduleFixedLength);

            return module.PadRight(moduleFixedLength, ' ') + message;
           // return module += new string(' ', moduleFixedLength) + message;
        }

        private string GetMessage(string module, string message, Exception ex)
        {
            var moduleSpace = 15;
            var messageSpace = 40;
            var moduleFixedLength = 70;
            //if (module.Length > 0)
           // moduleSpace += moduleFixedLength;


            //if (message.Length > 0)
            //    messageSpace += message.Length;
           
            //s += new string(' ', 35);
            return module.PadRight(moduleFixedLength, ' ') + message.PadRight(messageSpace, ' ') + ex;
        }

        public void Fatal(string message)
        {
            _log.Fatal(message);
        }

        public void Fatal(string module, string message)
        {
            //int space2 = 40;

            //if (module.Length == 0)
            //    space2 = 4;

            string line = null;

            line = GetMessage(module, message);

            Fatal(line);
        }

      

        #endregion

        internal static void SetThreadContext(LoggerContext logerContext)
        {
            //NDC.Push(logerContext.ToString()); 
            
            ThreadContext.Properties["ShardName"] = logerContext!=null?logerContext.ToString():"";
        }

        //public static string ThreadContexts
        //{
        //    get { return log4net.ThreadContext.Properties["ShardName"] as string; }
        //}
    }
}
