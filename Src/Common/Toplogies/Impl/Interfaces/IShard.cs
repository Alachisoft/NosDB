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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl.ShardServerImpl;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces
{
    public interface IShard : IDisposable
    {
        string Name { get; set; }
        ShardImpl.Server Primary { get; }
        /// <summary>
        /// Is the current role of the node. A single node can be in any ONE of the three given states at any given 
        /// point in time.
        /// </summary>
        NodeRole NodeRole { get; }
        IList<ShardImpl.Server> Servers { get; }
        IList<ShardImpl.Server> ActiveChannelsList { get; }
        IChannelFormatter ChannelFormatter { get; }
        bool Initialize(ShardConfiguration configuration);
        bool Start();
        bool Stop();
        //void OnMembershipChanged(ConfigChangeEventArgs args);
        Boolean OnSessionEstablished(Session session);
        Object SendUnicastMessage(ShardImpl.Server destination, Object message);
        Object SendBroadcastMessage(Message message);
        Object SendMulticastMessage(List<ShardImpl.Server> destinations, Message message);
        void RegisterShardListener(String name, IShardListener shardListener);
        void UnregisterShardListener(String name, IShardListener shardListener);
        void RemoveBrokenConnection();

        /// <summary>
        /// Begin Aysnc Message Sending, which will return IAsyncResult to manipulate async operation
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        IAsyncResult BeginSendMessage(ShardImpl.Server destination, object msg);

        /// <summary>
        /// End Aync Message Sending, which will return the result of async operation
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        object EndSendMessage(ShardImpl.Server destination, IAsyncResult result);
     
        /// <summary>
        /// Create UnicastResquest on this shard for sending async unicast opeation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        ShardRequestBase<T> CreateUnicastRequest<T>(ShardImpl.Server destination,Message message);

        /// <summary>
        /// Create MulticastRequest on this shard for sending async multicast opeation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destinations"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        ShardMulticastRequest<R, T> CreateMulticastRequest<R, T>(IList<ShardImpl.Server> destinations, Message message) where R : IResponseCollection<T>, new();
    }
}
