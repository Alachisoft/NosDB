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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Alachisoft.NosDB.Common.Configuration
{
    public class DBHostSettings : ConfigSettingsBase
    {
       
        private long _gCThreshold = 1024 * MiscUtil.MB * 2;
        private long _opLogSize = long.MaxValue;

        public override IPAddress IP
        {
            get;
            protected set;
        }

        public override int Port
        {
            get;
            protected set;
        }

        public override string LogConfiguration
        {
            get;
            protected set;
        }

        public override string BasePath
        {
            get;
            protected set;
        }

        public override string ConfigurationFile
        {
            get;
            protected set;
        }

        public override bool IsSecurityEnabled
        {
            get;
            protected set;
        }


        public string DeploymentPath
        {
            get;
            protected set;
        }

        public long OpLogSize
        {
            get { return _opLogSize;  }
            protected set { _opLogSize = value; }
        }

        public long GCThreshold
        {
            get { return _gCThreshold; }
            private set { _gCThreshold = value; }
        }

        public IPAddress ConfigurationServiceIP { get; set; }

        public int ConfigurationServicePort { get; set; }

        public override string SecurityConfigFile
        { protected set; get; }

        private void LoadCsConfig()
        {
            System.Configuration.Configuration config;
            string serviceExe = string.Empty;
            Process[] process = null;
            try
            {
                process = Process.GetProcessesByName("Alachisoft.NosDB.ConfigurationService");
                if (process.Length > 0)
                    serviceExe = process[0].MainModule.FileName;
                
                if (String.IsNullOrEmpty(serviceExe))
                {
                    process = Process.GetProcessesByName("CSHost.vshost");
                    if (process.Length > 0)
                        serviceExe = process[0].MainModule.FileName;
                }
                if (String.IsNullOrEmpty(serviceExe))
                {
                    process = Process.GetProcessesByName("CSHost");
                    if (process.Length > 0)
                        serviceExe = process[0].MainModule.FileName;
                }
                if (String.IsNullOrEmpty(serviceExe))
                    serviceExe = Path.Combine(Path.Combine(Path.Combine(AppUtil.InstallDir, "bin"), "service"), "Alachisoft.NosDB.ConfigurationService.exe");
                config = ConfigurationManager.OpenExeConfiguration(serviceExe);

            }
            catch (Exception ex) { return; }

            try
            {
                if (config.AppSettings.Settings["ConfigServerIP"] != null)
                    ConfigurationServiceIP = IPAddress.Parse(config.AppSettings.Settings["ConfigServerIP"].Value);
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ConfigServerPort"] != null)
                {
                    int tempInt = Int32.Parse(config.AppSettings.Settings["ConfigServerPort"].Value);

                    if (tempInt <= 0)
                        throw new Exception("");
                    ConfigurationServicePort = tempInt;
                }
            }
            catch (Exception ex) { }
        }

        private void LoadRestApiConfig()
        {
            if (LogConfiguration == null && ConfigurationManager.AppSettings["LogConfiguration"] != null)
                LogConfiguration = ConfigurationManager.AppSettings["LogConfiguration"];
        }

        public override void Load()
        {
            LoadCsConfig();
            LoadRestApiConfig();
            System.Configuration.Configuration config;
            try
            {
                string serviceExe = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
                if (String.IsNullOrEmpty(serviceExe))
                    serviceExe = Path.Combine(Path.Combine(Path.Combine(AppUtil.InstallDir, "bin"), "service"), "Alachisoft.NosDB.DatabaseService.exe");
                config = ConfigurationManager.OpenExeConfiguration(serviceExe);


            }
            catch (Exception ex) { return; }

            try
            {
                if (config.AppSettings.Settings["ManagementServerIP"] != null)
                    IP = IPAddress.Parse(config.AppSettings.Settings["ManagementServerIP"].Value);
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ManagementServerPort"] != null)
                {
                    int tempInt = Int32.Parse(config.AppSettings.Settings["ManagementServerPort"].Value);

                    if (tempInt <= 0)
                        throw new Exception("");
                    Port = tempInt;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["LogConfiguration"] != null)
                    LogConfiguration = config.AppSettings.Settings["LogConfiguration"].Value;
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["DataPath"] != null)
                {
                    BasePath = config.AppSettings.Settings["DataPath"].Value;
                    if (!(BasePath.EndsWith("\\") || BasePath.EndsWith("/")))
                    {
                        BasePath += "\\";
                    }
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["ServerConfigfile"] != null)
                {
                    ConfigurationFile = config.AppSettings.Settings["ServerConfigfile"].Value;
                }
            }
            catch (Exception ex) { }

            try
            {
                IsSecurityEnabled = true;
                if (config.AppSettings.Settings["IsSecurityEnabled"] != null)
                {
                    bool isSecurityEnabled;
                    Boolean.TryParse(config.AppSettings.Settings["IsSecurityEnabled"].Value, out isSecurityEnabled);
                    IsSecurityEnabled = isSecurityEnabled;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["DeploymentPath"] != null)
                {
                    DeploymentPath = config.AppSettings.Settings["DeploymentPath"].Value;
                    if (!(DeploymentPath.EndsWith("\\") || DeploymentPath.EndsWith("/")))
                    {
                        BasePath += "\\";
                    }
                }
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["OperationLogSize"] != null)
                    OpLogSize = Convert.ToInt64(config.AppSettings.Settings["OperationLogSize"].Value) * 1024 * 1024;
            }
            catch (Exception ex) { }

            try
            {
                if (config.AppSettings.Settings["SecurityConfigFile"] != null)
                    SecurityConfigFile = config.AppSettings.Settings["SecurityConfigFile"].Value;
            }
            catch (Exception ex) { }

        }


    }
}
