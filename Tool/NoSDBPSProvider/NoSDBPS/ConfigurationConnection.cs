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
using System.Configuration;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Security.Client;

namespace Alachisoft.NosDB.NosDBPS
{
    public class ConfigurationConnection
    {
        static RemoteConfigurationManager activeRcm;
        static ConfigServerConfiguration _configClusterInfo;
        private static Common.Configuration.Services.ClusterInfo _databaseClusterInfo;
        private static Common.Configuration.ClusterConfiguration _clusterConfiguration;
        private static string _activeConfigServerIp;

        public static ConfigServerConfiguration ConfigCluster
        {
            get
            {
                return _configClusterInfo;
            }
            set { _configClusterInfo = value; }
        }

        public static string ActiveConfigServerIp
        {
            get { return _activeConfigServerIp; }
            set { _activeConfigServerIp = value; }
        }

        public static RemoteConfigurationManager Current
        {
            get
            {

                VerifyPrimeryNode();
                return activeRcm;
            }
            set
            {
                activeRcm = value;
            }
        }

        public static ClusterConfiguration ClusterConfiguration {
            set
            {
                
                _clusterConfiguration = value;
            }
            get
            {
                return _clusterConfiguration;
            }
        }
        
        public ConfigurationConnection(ConfigServerConfiguration configCluster)
        {
            _configClusterInfo = configCluster;
        }

        public static ClusterInfo ClusterInfo
        {
            get
            {
                return _databaseClusterInfo;
            }
            set
            {
                _databaseClusterInfo=value;
            }
        }

        public static void UpdateDatabaseClusterInfo()
        {
            if (ClusterConfiguration != null)
                _databaseClusterInfo = Current.GetDatabaseClusterInfo(ClusterConfiguration.Name);
        }

        public static void UpdateClusterConfiguration()
        {
            try
            { 
                var config = Current.GetDatabaseClusterConfig(ClusterConfiguration.Name.Equals(MiscUtil.CLUSTERED));
                ClusterConfiguration = config;
                if (config == null)
                {
                    throw new Exception("Cann't update informition from configuration server[s]");
                }
            }
            catch(Exception e)
            {
                ClusterConfiguration = null;
                throw e;
            }
             
        }

        private static void VerifyPrimeryNode()
        {

            if (_configClusterInfo == null)
            {
                throw new Exception("Kindly connect database cluster to continue");
            }
            if (ConfigCluster.Name.Equals(MiscUtil.CLUSTERED)||ConfigCluster.Name.Equals("standalone",StringComparison.InvariantCultureIgnoreCase))
            {
                if (activeRcm != null)
                {
                    if (ConfigCluster.Name.Equals("standalone", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    try
                    {
                        if (activeRcm.VerifyConfigurationClusterPrimery(_configClusterInfo.Name))
                            return;
                    }
                    catch
                    {

                    }
                }
                if (activeRcm == null)
                {
                    activeRcm = new RemoteConfigurationManager();
                }
                try
                {
                    
                    if (!activeRcm.IsInitialised)
                    {
                        var configServer = ConfigCluster.Servers.Nodes.First(p => p.Value.Priority == 1);
                        ActiveConfigServerIp = configServer.Value.Name;
                        activeRcm.Initilize(ConfigCluster.Name,configServer.Value.Name, _configClusterInfo.Port, new ConfigurationChannelFormatter(), new SSPIClientAuthenticationCredential());
                    }
                    if (activeRcm.VerifyConfigurationClusterPrimery(_configClusterInfo.Name))
                    {
                        return;
                    }
                    activeRcm.Dispose();
                }
                catch
                {
                    try
                    {
                        var configServer = ConfigCluster.Servers.Nodes.First(p => p.Value.Priority == 2);
                        ActiveConfigServerIp = configServer.Value.Name;
                        activeRcm.Initilize(ConfigCluster.Name,configServer.Value.Name, _configClusterInfo.Port, new ConfigurationChannelFormatter(), new SSPIClientAuthenticationCredential());
                        if (activeRcm.VerifyConfigurationClusterPrimery(_configClusterInfo.Name))
                        {

                            return;
                        }
                        activeRcm.Dispose();
                    }
                    catch (Exception)
                    {
                        
                        
                    }
                    
                }
                }
                throw new Exception("Cann't connect to any of the servers in database cluster ");
            }
        
    }
}
