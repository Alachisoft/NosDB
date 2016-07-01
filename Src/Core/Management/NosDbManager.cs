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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services.Client;

namespace Alachisoft.NosDB.Core.Management
{
    public class NosDbManager
    {
        public static void StartNode(ManagementInfo managementInfo)
        {
            try
            {
                RemoteConfigurationManager remoteConfigurationManager = InitializeRemoteConfigurationManager(managementInfo);

                var serverNode = new ServerNode { Name = managementInfo.ServerName };

                remoteConfigurationManager.StartNode(managementInfo.ShardName, serverNode);
            }
            catch (ManagementException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ManagementException(exception.Message, exception);
            }
        }

        public static void StopNode(ManagementInfo managementInfo)
        {
            try
            {
                RemoteConfigurationManager remoteConfigurationManager = InitializeRemoteConfigurationManager(managementInfo);

                var serverNode = new ServerNode { Name = managementInfo.ServerName };

                remoteConfigurationManager.StopNode(managementInfo.ShardName, serverNode);
            }
            catch (ManagementException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ManagementException(exception.Message, exception);
            }
        }
        public static void StopShard(ManagementInfo managementInfo)
        {
            try
            {
                RemoteConfigurationManager remoteConfigurationManager = InitializeRemoteConfigurationManager(managementInfo);
                remoteConfigurationManager.StopShard(managementInfo.ShardName);
            }
            catch (ManagementException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ManagementException(exception.Message, exception);
            }
        }

        public static void StartShard(ManagementInfo managementInfo)
        {
            try
            {
                RemoteConfigurationManager remoteConfigurationManager = InitializeRemoteConfigurationManager(managementInfo);
                remoteConfigurationManager.StartShard(managementInfo.ShardName);
            }
            catch (ManagementException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ManagementException(exception.Message, exception);
            }
        }
        public static void MoveCollection(ManagementInfo managementInfo)
        {
            try
            {
                RemoteConfigurationManager remoteConfigurationManager = InitializeRemoteConfigurationManager(managementInfo);
                remoteConfigurationManager.MoveCollection(true, managementInfo.DatabaseName,
                    managementInfo.CollectionName, managementInfo.ShardName);
            }
            catch (ManagementException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ManagementException(exception.Message, exception);
            }
        }

        private static RemoteConfigurationManager InitializeRemoteConfigurationManager(ManagementInfo managementInfo)
        {
            if (managementInfo.ConfigServerIp == null)
                managementInfo.ConfigServerIp = NetworkUtil.GetLocalIPAddress().ToString();

            if (managementInfo.ConfigServerPort == 0)
                managementInfo.ConfigServerPort = NetworkUtil.DEFAULT_CS_HOST_PORT;

            IClientAuthenticationCredential clientAuthenticationCredential = new SSPIClientAuthenticationCredential();

            var remoteConfigurationManager = new RemoteConfigurationManager();
            remoteConfigurationManager.Initilize(MiscUtil.CLUSTERED, managementInfo.ConfigServerIp, managementInfo.ConfigServerPort, new ConfigurationChannelFormatter(), clientAuthenticationCredential);

            return remoteConfigurationManager;
        }
    }
}
