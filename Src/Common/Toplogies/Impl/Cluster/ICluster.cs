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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Cluster
{
    public interface ICluster : IDisposable
    {
        //ShardInfo ThisShard { get; }
        //IDictionary<String,ShardInfo> Shards { get; }
        bool Initialize(ClusterConfiguration configuration);
        bool Start();
        bool Stop();

        /// <summary>
        /// Check the role of the node.
        /// </summary>
        NodeRole ShardNodeRole { get; set; }

        /// <summary>
        /// Check if specified shard is connected with this node on or not
        /// </summary>
        /// <param name="shardName">Shard Name</param>
        /// <returns></returns>
        Boolean IsShardConnected(String shardName);
        
        /// <summary>
        /// Register for cluster Events like message recieved on cluster layer
        /// </summary>
        /// <param name="clusterListener"></param>
        void RegisterClusterListener(IClusterListener clusterListener);


        /// <summary>
        /// Removes the specified remote shard from the list of remote shards.
        /// </summary>
        /// <param name="shard"></param>
        bool RemoveRemoteShard(string shardName);

        /// <summary>
        /// Unregister cluster events listener
        /// </summary>
        /// <param name="clusterListener"></param>
        void UnregisterClusterListener(IClusterListener clusterListener);

        /// <summary>
        /// RegisterQuery for cluster Events like message recieved on cluster layer
        /// </summary>
        /// <param name="clusterListener"></param>
        void RegisterConfigChangeListener(IConfigurationListener configChangeListener);


        /// <summary>
        /// Unregister cluster events listener
        /// </summary>
        /// <param name="clusterListener"></param>
        void UnregisterConfigChangeListener(IConfigurationListener configChangeListener);

        /// <summary>
        /// Message is unicast to the primary node of the shard
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        T SendMessage<T>(String shard, Message message);

        /// <summary>
        /// Message is unicast to a specific server for the given shard
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="server"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        T SendMessage<T>(String shard, ShardImpl.Server server, Message message);

        /// <summary>
        /// Message is broacasted to all servers within the given shard
        /// </summary>
        /// <param name="shard"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        IResponseCollection<T> SendMessageToAllServers<T>(String shard, Message message);

        /// <summary>
        /// Message is send to all shards. PrimaryOny flag dicates whether message is meant to be delivered
        /// to ony primary nodes of the shards or all secondary nodes
        /// </summary>
        /// <param name="message"></param>
        /// <param name="primaryOnly">If 'true' message is sent to only primary nodes of the shard(s)</param>
        /// <returns></returns>
        IDictionary<String,IResponseCollection<T>> SendMessageToAllShards<T>(Message message, bool primaryOnly);

        IList<ShardImpl.Server> GetActiveChannelList();

    }
}
