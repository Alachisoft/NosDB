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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Monitoring.Configuration
{
    public interface IConfigurationMonitor
    {
        ClusterInfo[] GetConfiguredClusters();
        Membership GetMembershipInfo(string cluster, string shard);
        Membership GetUpdatedMembershipInfo(string cluster, string shard);

        Dictionary<string, List<ServerInfo>> GetConfigureServerNodes(string cluster);
        Dictionary<string, List<ServerInfo>> GetUpdatedConfigureServerNodes(string cluster);

        Dictionary<string, List<ServerInfo>> GetRunningServerNodes(string cluster);
        Dictionary<string, List<ServerInfo>> GetUpdatedRunningServerNodes(string cluster);

        ShardInfo[] GetConfiguredShards(string cluster);
        ShardInfo[] GetUpdatedConfiguredShards(string cluster);
    }
}
