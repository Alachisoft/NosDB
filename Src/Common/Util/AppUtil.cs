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
using Alachisoft.NosDB.Common.Util;
using Microsoft.Win32;
using System.Configuration;

namespace Alachisoft.NosDB.Common
{
    /// <summary>
    /// Utility class to help with common tasks.
    /// </summary>
    public class AppUtil
    {
        
        public static readonly string InstallDir = "";

        public readonly static string DeployedAssemblyDir = "deploy\\";
        static readonly int SLogLevel = 7;
        private static int _csPort = 9950;

        static AppUtil()
        {
            InstallDir = GetInstallDir();

            string logLevel = ConfigurationSettings.AppSettings["EventLogLevel"];

            if (!string.IsNullOrEmpty(logLevel))
            {
                logLevel = logLevel.ToLower();
                switch (logLevel)
                {
                    case "error":
                        SLogLevel = 1;
                        break;

                    case "warning":
                        SLogLevel = 3;
                        break;

                    case "all":
                        SLogLevel = 7;
                        break;
                }
            }
        }

        private static string GetInstallDir()
        {
            return GetAppSetting("InstallDir");
        }

        public static string GetAppSetting(string key)
        {
            return GetAppSetting("", key);
        }

        public static string GetClusterName(bool isLocal)
        {
            return isLocal ? "local" : "cluster";
        }


        public static string GetUtilityLogo(string toolName)
        {
            string logo = "\n"
                + @"Alachisoft (R) NosDB Utility {0} Version 1.3.0.0" +
               "\n" + @"Copyright (C) Alachisoft 2016. All rights reserved.";

            return string.Format(logo, toolName);
        }


        public static string GetAppSetting(string section, string key)
        {
            section = RegHelper.ROOT_KEY + section;

            object tempVal = RegHelper.GetRegValue(section, key, 0);
            if (!(tempVal is String))
            {
                return Convert.ToString(tempVal);
            }
            return (String)tempVal;
        }

        public static bool IsRunningAsWow64
        {
            get { return  false; }
        }

        public static bool IsNew { get { return true; } }

       
    
        public static string EventLogSource
        {
            get { return "NosDB"; }
        }

        /// <summary>
        /// Writes an error, warning, information, success audit, or failure audit 
        /// entry with the given message text to the event log.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="msg">The string to write to the event log.</param>
        /// <param name="type">One of the <c>EventLogEntryType</c> values.</param>
        /// <param name="category"></param>
        /// <param name="eventId"></param>
        public static void LogEvent(string source, string msg, EventLogEntryType type, short category, int eventId)
        {
            try
            {
                var level = (int)type;
                if ((level & SLogLevel) == level)
                {
                    using (var nosdbLog = new EventLog("Application"))
                    {
                        nosdbLog.Source = source;
                        nosdbLog.WriteEntry(msg, type, eventId);
                    }
                }
            }
            catch (Exception) { }
        }
        
        /// <summary>
        /// Writes an error, warning, information, success audit, or failure audit 
        /// entry with the given message text to the event log.
        /// </summary>
        /// <param name="msg">The string to write to the event log.</param>
        /// <param name="type">One of the <c>EventLogEntryType</c> values.</param>
        public static void LogEvent(string msg, EventLogEntryType type)
        {
            string source= EventLogSource;
            if (type == EventLogEntryType.Information)
                LogEvent(source, msg, type, EventCategories.Information, EventID.GeneralInformation);
            else
                LogEvent(source, msg, type, EventCategories.Warning, EventID.GeneralError);
        }

        /// <summary>
        /// Returns lg(Log2) of a number.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte Lg(int val)
        {
            byte i = 0;
            while (val > 1)
            {
                val >>= 1;
                i++;
            }
            return i;
        }

        /// <summary>
        /// Store all date time values as a difference to this time
        /// </summary>
        private static DateTime _startDt = new DateTime(2004, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Convert DateTime to integer taking 31-12-2004 as base
        /// and removing millisecond information
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int DiffSeconds(DateTime dt)
        {
            dt = dt.ToUniversalTime();
            TimeSpan interval = dt - _startDt;
            return (int)interval.TotalSeconds;
        }

        public static int DiffMilliseconds(DateTime dt)
        {
            dt = dt.ToUniversalTime();
            TimeSpan interval = dt - _startDt;
            return interval.Milliseconds;
        }

        public static long DiffTicks(DateTime dt)
        {
            dt = dt.ToUniversalTime();
            TimeSpan interval = dt - _startDt;
            return interval.Ticks;
        }

        /// <summary>
        /// Convert DateTime to integer taking 31-12-2004 as base
        /// and removing millisecond information
        /// </summary>
        /// <param name="absoluteTime"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(int absoluteTime)
        {
            var dt = new DateTime(_startDt.Ticks, DateTimeKind.Utc);
            return dt.AddSeconds(absoluteTime);
        }

        /// <summary>
        /// Checks environment to verify if there is 'Any' version of Visual Studio installed.
        /// and removing millisecond information
        /// </summary>
        public static bool IsVsIdeInstalled()
        {
            //Check VS.Net 2005
            RegistryKey rKey8 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\8.0");
            RegistryKey rKey9 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\9.0");
            if (rKey8 != null)
            {
                if (rKey8.GetValue("InstallDir", "").ToString().Length != 0)
                    return true;
            }

            if (rKey9 != null)
            {
                if (rKey9.GetValue("InstallDir", "").ToString().Length != 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Hashcode algorithm returning same hash code for both 32bit and 64 bit apps. 
        /// Used for data distribution under por/partitioned topologies.
        /// </summary>
        /// <param name="strArg"></param>
        /// <returns></returns>
        public static unsafe int GetHashCode(string strArg)
        {
            fixed (void* str = strArg)
            {
                char* chPtr = (char*)str;
                int num = 0x15051505;
                int num2 = num;
                int* numPtr = (int*)chPtr;
                for (int i = strArg.Length; i > 0; i -= 4)
                {
                    num = (((num << 5) + num) + (num >> 0x1b)) ^ numPtr[0];
                    if (i <= 2)
                    {
                        break;
                    }
                    num2 = (((num2 << 5) + num2) + (num2 >> 0x1b)) ^ numPtr[1];
                    numPtr += 2;
                }
                return (num + (num2 * 0x5d588b65));
            }
        }

        public static int ConfigurationServerPort
        {
            get
            {
                try
                {
                    object v = RegHelper.GetRegValue(RegHelper.ROOT_KEY, "ConfigurationServerPort", 0);
                    if (v != null)
                    {
                        int port = Convert.ToInt32(v);
                        if (port >= System.Net.IPEndPoint.MinPort &&
                            port <= System.Net.IPEndPoint.MaxPort)
                            return port;
                    }
                }
                catch (FormatException) { }
                catch (OverflowException) { }

                return _csPort;
            }

        }

    }
}