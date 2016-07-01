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
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Configuration.Services;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ConfigurationTasks
{
    public class ClusterConfigurationManager
    {        
        private IConfigurationSession _configurationSession;
        private String _clusterName = null;

        public ClusterConfigurationManager(IConfigurationSession configSession,ClusterConfiguration clusterConfig)
        {
            _configurationSession = configSession;
            LatestConfiguration = clusterConfig;
            _clusterName = clusterConfig.Name;
            if(LatestConfiguration==null)
                UpdateClusterConfiguration();
        }
        public ClusterConfiguration LatestConfiguration { get; set; }

        internal void UpdateClusterConfiguration()
        {
            try
            {
                if (_configurationSession != null)
                    LatestConfiguration = _configurationSession.GetDatabaseClusterConfiguration(_clusterName);
            }
            catch (ChannelException e)
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("ClusterConfigurationManager.UpdateClusterConfiguration() " , e.ToString());
            }
        }

        public ShardConfiguration GetShardConfiguration(string shardName)
        {
            ShardConfiguration sConfig = null;
            if (LatestConfiguration != null && LatestConfiguration.Deployment != null)
                sConfig = LatestConfiguration.Deployment.GetShard(shardName);
            return sConfig;
        }
    }
}
