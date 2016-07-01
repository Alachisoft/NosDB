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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    internal class MembershipData
    {
        private IConfigurationStore _configurationStore;

        public MembershipData(IConfigurationStore configStore)
        {
            _configurationStore = configStore;
        }
        //public Dictionary<string, Membership> MemberShipData
        //{
        //    get { return _membershipMetaData; }
        //    set { _membershipMetaData = value; }
        //}

        public void AddMembership(string cluster, string shard, Membership membership)
        {
           _configurationStore.InsertOrUpdateMembershipData(membership);
        }

        public void RemoveShard(string cluster, string shard)
        {
            _configurationStore.RemoveMembershipData(cluster, shard);
        }

        public Membership GetMemberShip(string cluster, string shard)
        {
            return _configurationStore.GetMembershipData(cluster, shard);
        }

        public Membership GetMemberShip(string cluster, string shard,bool createIfMissing)
        {
            Membership membership = null;

            lock (this)
            {
                if (_configurationStore.GetMembershipData(cluster,shard) != null)
                {
                    membership = (Membership)_configurationStore.GetMembershipData(cluster,shard);

                    if (membership == null && createIfMissing)
                    {
                        if (createIfMissing)
                        {
                            membership = new Membership();
                            membership.Cluster = cluster;
                            membership.Shard = shard;
                            membership.Servers = new List<ServerNode>();
                            membership.ElectionId = new ElectionId();

                            _configurationStore.InsertOrUpdateMembershipData(membership);
                        }
                    }

                }
            }

            return membership;
        }


        public void AddNodeToMemberList(string cluster, string shard, ServerNode node)
        {
            lock (this)
            {
                if (_configurationStore.GetMembershipData(cluster,shard) != null)
                {
                    List<ServerNode> sNode = _configurationStore.GetMembershipData(cluster,shard).Servers;
                    if (!sNode.Contains(node))
                        sNode.Add(node);
                    _configurationStore.GetMembershipData(cluster,shard).Servers = sNode;
                    _configurationStore.InsertOrUpdateMembershipData(_configurationStore.GetMembershipData(cluster,shard));
                }
            }
        }

        public void SetPrimary(string cluster, string shard, ServerNode node)
        {
            lock (this)
            {
                if (_configurationStore.GetMembershipData(cluster,shard) != null)
                {

                    _configurationStore.GetMembershipData(cluster,shard).Primary = node;
                    _configurationStore.InsertOrUpdateMembershipData(_configurationStore.GetMembershipData(cluster,shard));
                }
            }
        }

        public void RemoveClusterMemberShip(ClusterConfiguration clusterConfig)
        {
            lock (this)
            {
                if (clusterConfig != null && clusterConfig.Deployment != null && clusterConfig.Deployment.Shards != null)
                {
                    if (clusterConfig.Deployment.Shards.Count > 0)
                    {
                        foreach (ShardConfiguration sConfig in clusterConfig.Deployment.Shards.Values)
                        {
                            _configurationStore.RemoveMembershipData(clusterConfig.Name.ToLower(),sConfig.Name);
                        }
                    }
                }
            }

        }

        public bool RemoveNodeFromMemberList(string cluster, string shard, ServerNode node)
        {
            bool isPrimary = false;
            lock (this)
            {
                if (_configurationStore.GetMembershipData(cluster,shard) != null)
                {
                    List<ServerNode> sNode = _configurationStore.GetMembershipData(cluster,shard).Servers;
                    if (sNode.Contains(node))
                        sNode.Remove(node);

                    _configurationStore.GetMembershipData(cluster,shard).Servers = sNode;
                    _configurationStore.InsertOrUpdateMembershipData(_configurationStore.GetMembershipData(cluster,shard));

                    if (_configurationStore.GetMembershipData(cluster,shard).Primary != null && _configurationStore.GetMembershipData(cluster,shard).Primary.Equals(node))
                    {
                        _configurationStore.GetMembershipData(cluster,shard).Primary = null;
                        _configurationStore.InsertOrUpdateMembershipData(_configurationStore.GetMembershipData(cluster,shard));
                        isPrimary = true;
                    }

                }
            }
            return isPrimary;
        }


        public void UpdateElectionId(string cluster, string shard, ElectionId electionId)
        {
            lock (this)
            {
                if (_configurationStore.GetMembershipData(cluster, shard) != null)
                {
                    _configurationStore.GetMembershipData(cluster, shard).ElectionId = electionId;
                    _configurationStore.InsertOrUpdateMembershipData(_configurationStore.GetMembershipData(cluster, shard));
                }
            }
        }
     

        
    }
}
