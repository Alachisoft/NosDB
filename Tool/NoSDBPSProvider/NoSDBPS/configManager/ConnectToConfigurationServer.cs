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
using System.Management.Automation;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration.Services.Client;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Configuration;


namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsCommunications.Connect, "DatabaseCluster")]
    public class ConnectToConfigurationServer : PSCmdlet
    {
        private string _server = string.Empty;
        private int _port = NetworkUtil.DEFAULT_CS_HOST_PORT;
        private bool _standAlone;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "specifies address of active Node, uses local IP resolved by DNS as default.")]
        [Alias("s")]
        public string Server
        {
            set { _server = value; }
            get { return _server; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 3,
            HelpMessage = "port of database cluster. uses 9950 as default.")]
        [Alias("p")]
        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify whether removel is forcefull")]
        [Alias("a")]
        public SwitchParameter StandAlone
        {
            get { return _standAlone; }
            set { _standAlone = value; }
        }

        protected override void BeginProcessing()
        {
            if(string.IsNullOrEmpty(Server))
                Server = ProviderUtil.GetLocalAddress();

            var rcm = new RemoteConfigurationManager();
            rcm.Initilize(StandAlone?MiscUtil.LOCAL:MiscUtil.CLUSTERED, Server, Port, new ConfigurationChannelFormatter(), new SSPIClientAuthenticationCredential());
            
            if (!_standAlone)
            {
                if (rcm.VerifyConfigurationCluster())
                {
                    ConfigurationConnection.ConfigCluster = rcm.GetConfigurationClusterConfiguration();
                    ConfigurationConnection.ClusterConfiguration = rcm.GetDatabaseClusterConfig(true);
                    ConfigurationConnection.Current = rcm;
                    ConfigurationConnection.UpdateDatabaseClusterInfo();
                }
                else
                {
                    throw new Exception("Database cluster does not exist on the specified server.");
                }
            }
            else
            {
                var localConfig = rcm.GetDatabaseClusterConfig(false);
                if (localConfig != null)
                {
                    var configCluster = new ConfigServerConfiguration
                    {
                        Name = MiscUtil.STAND_ALONE,
                        Port = Port,
                        Servers = new ServerNodes()

                    };
                    configCluster.Servers.AddNode(new ServerNode { Name = Server, Priority = 1 });
                    ConfigurationConnection.ConfigCluster = configCluster;
                    ConfigurationConnection.ClusterConfiguration = localConfig;
                    ConfigurationConnection.Current = rcm;
                    ConfigurationConnection.UpdateClusterConfiguration();
                    ConfigurationConnection.UpdateDatabaseClusterInfo();
                }
                else
                {
                    throw new Exception("Standalone database does not exist on the specified server.");
                }
            }
        }

        protected override void ProcessRecord()
        {
            SessionState.Path.SetLocation(ProviderUtil.DRIVE_ROOT +":\\" + ConfigurationConnection.ClusterConfiguration.DisplayName);
        }

    }
}
