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
using System.Configuration;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Alachisoft.NosDB.Common.Util
{
    public class ServiceConfiguration
    {
        private const long MB = 1024 * 1024;

        private static long _gCThreshold = 1024 * MB * 2;

        private static IPAddress _configServerIp;
        private static string _configSession = "OutProc";
        private static IPAddress _localIp;
        private static int _configServerPort = 9950;
        private static int _managementServerPort = 9960;
        private static long _operationLogSize = long.MaxValue;
        private static int _localShardPort = 9980;

        #region CSHOST
        private static string _basePathCS = @"..\..\..\..\MetaData\CS Host\";
        private static string _logPath = @"..\..\log.txt";
        private static string _logConfigurationCS = "LogConfiguration.xml";
        private static bool _isSecurityEnabledCS = false;
        private static string _configurationFileCS = "ConfigCluster.nconf";
        #endregion

        #region DBHOST
        private static IPAddress _managementServerIp;
        private static string _shardName;
        private static int _port;
        private static string _cluster = "cluster1";
        private static string _dataPath = @"..\..\..\..\MetaData\DB Host\DataPath\";
        private static string _deploymentPath = @"..\..\..\..\MetaData\DB Host\DeploymentPath\";
        private static string _logConfigurationDB = "LogConfiguration.xml";
        private static bool _isSecurityEnabledDB = false;
        private static string _configurationFileDB = "nodeconfig.xml";
        #endregion

        private static int _tempInt;
        private static long _tempLong;

        static ServiceConfiguration()
        {
            LoadCSConfig();
            LoadDBConfig();
        }

        public static void LoadCSConfig()
        {
            System.Configuration.Configuration config;
            try
            {
                string serviceEXE1 = Path.Combine(AppUtil.InstallDir, "bin");
                string serviceEXE2 = Path.Combine(serviceEXE1, "service");
                string serviceEXE3 = Path.Combine(serviceEXE2, "Alachisoft.NosDB.ConfigurationService.exe");
                //  AppUtil.LogEvent("InstallDir::"+AppUtil.InstallDir,EventLogEntryType.Error);
                config = ConfigurationManager.OpenExeConfiguration(serviceEXE3);
                // Console.ReadLine();
            }
            catch (Exception ex) { return; }

            try
            {
                if (config.AppSettings.Settings["ConfigServerIP"] != null)
                    _configServerIp = IPAddress.Parse(config.AppSettings.Settings["ConfigServerIP"].Value);
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ConfigServerPort"] != null)
                {
                    _tempInt = Int32.Parse(config.AppSettings.Settings["ConfigServerPort"].Value);

                    if (_tempInt <= 0)
                        throw new Exception("");
                    _configServerPort = _tempInt;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["LogConfiguration"] != null)
                    _logConfigurationCS = config.AppSettings.Settings["LogConfiguration"].Value;
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["BasePath"] != null)
                {
                    _basePathCS = config.AppSettings.Settings["BasePath"].Value;
                }
            }
            catch (Exception ex) { }

            try
            {
                _isSecurityEnabledCS = true;
                if (config.AppSettings.Settings["IsSecurityEnabled"] != null)
                {
                    Boolean.TryParse(config.AppSettings.Settings["IsSecurityEnabled"].Value, out _isSecurityEnabledCS);
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ConfigServerConfigurationFile"] != null)
                {
                    _configurationFileCS = config.AppSettings.Settings["ConfigServerConfigurationFile"].Value;
                }
            }
            catch (Exception ex) { }

        }

        public static void LoadDBConfig()
        {
            System.Configuration.Configuration config;
            try
            {
                string serviceEXE1 = Path.Combine(AppUtil.InstallDir, "bin");
                string serviceEXE2 = Path.Combine(serviceEXE1, "service");
                string serviceEXE3 = Path.Combine(serviceEXE2, "Alachisoft.NosDB.ConfigurationService.exe");
                //  AppUtil.LogEvent("InstallDir::"+AppUtil.InstallDir,EventLogEntryType.Error);
                config = ConfigurationManager.OpenExeConfiguration(serviceEXE3);
                // Console.ReadLine();
            }
            catch (Exception ex) { return; }

            try
            {
                if (config.AppSettings.Settings["ManagementServerIP"] != null)
                    _managementServerIp = IPAddress.Parse(config.AppSettings.Settings["ManagementServerIP"].Value);
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ManagementServerPort"] != null)
                {
                    _tempInt = Int32.Parse(config.AppSettings.Settings["ManagementServerPort"].Value);

                    if (_tempInt <= 0)
                        throw new Exception("");
                    _managementServerPort = _tempInt;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["DataPath"] != null)
                {
                    _dataPath = config.AppSettings.Settings["DataPath"].Value;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["DeploymentPath"] != null)
                {
                    _deploymentPath = config.AppSettings.Settings["DeploymentPath"].Value;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["LogConfiguration"] != null)
                    _logConfigurationDB = config.AppSettings.Settings["LogConfiguration"].Value;
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ServerConfigfile"] != null)
                {
                    _configurationFileCS = config.AppSettings.Settings["ServerConfigfile"].Value;
                }
            }
            catch (Exception ex) { }

            try
            {
                _isSecurityEnabledDB = true;
                if (config.AppSettings.Settings["IsSecurityEnabled"] != null)
                {
                    Boolean.TryParse(config.AppSettings.Settings["IsSecurityEnabled"].Value, out _isSecurityEnabledDB);
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["OperationLogSize"] != null)
                    _operationLogSize = Convert.ToInt64(config.AppSettings.Settings["OperationLogSize"].Value);
            }
            catch (Exception ex) { }
        }

        public static long GCThreshold
        {
            get { return _gCThreshold; }
            private set { _gCThreshold = value; }
        }

        public static IPAddress ConfigServerIP
        {
            get { return _configServerIp; }
            set { _configServerIp = value; }
        }

        public static string ConfigSession
        {
            get { return _configSession; }
            set { _configSession = value; }
        }

        public static IPAddress LocalIP
        {
            get { return _localIp; }
            set { _localIp = value; }
        }

        public static bool CSIsSecurityEnabled
        {
            get { return _isSecurityEnabledCS; }
            set { _isSecurityEnabledCS = value; }
        }

        public static bool DBIsSecurityEnabled
        {
            get { return _isSecurityEnabledDB; }
            set { _isSecurityEnabledDB = value; }
        }

        public static int ConfigServerPort
        {
            get { return _configServerPort; }
            set { _configServerPort = value; }
        }

        public static string CSLogConfiguration
        {
            get { return _logConfigurationCS; }
            set { _logConfigurationCS = value; }
        }

        public static string DBLogConfiguration
        {
            get { return _logConfigurationDB; }
            set { _logConfigurationDB = value; }
        }

        public static string CSBasePath
        {
            get { return _basePathCS; }
            set { _basePathCS = value; }
        }

        public static string LogPath
        {
            get { return _logPath; }
            set { _logPath = value; }
        }

        public static IPAddress ManagementServerIP
        {
            get { return _managementServerIp; }
            set { _managementServerIp = value; }
        }

        public static string ShardName
        {
            get { return _shardName; }
            set { _shardName = value; }
        }

        public static int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public static int ManagementServerPort
        {
            get { return _managementServerPort; }
            set { _managementServerPort = value; }
        }

        public static string Cluster
        {
            get { return _cluster; }
            set { _cluster = value; }
        }

        public static string DataPath
        {
            get { return _dataPath; }
            set { _dataPath = value; }
        }

        public static string DeploymentPath
        {
            get { return _deploymentPath; }
            set { _deploymentPath = value; }
        }

        public static long OperationLogSize
        {
            get { return _operationLogSize; }
            set { _operationLogSize = value; }
        }

        public static int LocalShardPort
        {

            get { return _localShardPort; }
            set { _localShardPort = value; }
        }
    }
}