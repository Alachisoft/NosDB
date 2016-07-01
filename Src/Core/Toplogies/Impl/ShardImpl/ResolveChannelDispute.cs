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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.ShardImpl
{
    public class ResolveChannelDispute
    {
        private NodeContext _context = null;
        private IShard _shard = null;
        private int _connIdGenerator = 0;
        private Object _mutex = new Object();
        public ResolveChannelDispute(NodeContext context, IShard shard)
        {
            this._context = context;
            this._shard = shard;
        }

        //To-do: add ConnectStatus logic.
        public IDualChannel SetConnectInfo(IDualChannel channel, ConnectInfo.ConnectStatus status)
        {
            ConnectInfo connectInfo = null;

            if (((Address)_context.LocalAddress).CompareTo((Address)channel.PeerAddress) > 0)
            {
                connectInfo = new ConnectInfo();
                connectInfo.Status = status;
                connectInfo.Id = GetConnectionId();

                channel.SendMessage(connectInfo, true);
                if(LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Debug("ResolveChannelDispute.SetConnectInfo() ", "Node " + _context.LocalAddress.ToString() + ": Sender, " + "Node " + channel.PeerAddress.ToString() + ": Receiver.");
                }
            }
            // This specific check was necessary because somehow an earlier check for a condition was passing when it should not
                // look for an alternative way to this.
            else if (((Address)_context.LocalAddress).CompareTo((Address)channel.PeerAddress) < 0)
            {
                byte[] data = channel.ReadFromSocket();

                IRequest message = _shard.ChannelFormatter.Deserialize(data) as IRequest;
                connectInfo = message.Message as ConnectInfo;

                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                {
                    LoggerManager.Instance.ShardLogger.Debug("ResolveChannelDispute.SetConnectInfo() ", "Node " + _context.LocalAddress.ToString() + ": Receiver, " + "Node " + channel.PeerAddress.ToString() + ": Sender.");
                }
            }
            channel.ConnectInfo = connectInfo;
            return channel;
        }


        public int GetConnectionId()
        {
            lock (this)
            {
                return ++_connIdGenerator;
            }
        }

        public IDualChannel GetValidChannel(IDualChannel channel, IDictionary<Server, IDualChannel> existingChannels)
        {
            Server server = new Server(channel.PeerAddress, Status.Running);
            if (!existingChannels.ContainsKey(server) || server.Address.Equals(_context.LocalAddress))
            {
                return channel;
            }
            else
            {
                IDualChannel tempChannel = existingChannels[server];
                lock(existingChannels)
                {
                    existingChannels.Remove(server);
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("ResolveChannelDispute.GetValidChannel(): ", server.Address.ToString() + " removed from existing channels.");
                }
                if (channel != null && channel.ConnectInfo != null && channel.ConnectInfo.Id < tempChannel.ConnectInfo.Id && channel.ConnectInfo.Status != ConnectInfo.ConnectStatus.CONNECT_FIRST_TIME)
                {
                    channel.ShouldTryReconnecting = false;
                    channel.Disconnect();
                    if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                        LoggerManager.Instance.ShardLogger.Debug("ResolveChannelDispute.GetValidchannel(): if{}", "Connection of local node " + _context.LocalAddress.ToString() + " disconected from node " + channel.PeerAddress.ToString());
                    return tempChannel;
                }
                else
                {
                    tempChannel.ShouldTryReconnecting = false;
                    try
                    {
                        tempChannel.Disconnect();
                        if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsDebugEnabled)
                            LoggerManager.Instance.ShardLogger.Debug("ResolveChannelDispute.GetValidchannel(): else{}", "Connection of local node " + _context.LocalAddress.ToString() + " disconected from node " + tempChannel.PeerAddress.ToString());
                    }
                    catch (Exception e)
                    { }
                    //Thread.Sleep(1000);
                    return channel;
                }
            }
        }

        //Temporary overload added for the RemoteShard
        public IRequestResponseChannel GetValidChannel(IDualChannel newChannel,IRequestResponseChannel existingChannel)
        {
            IDictionary<Server, IDualChannel> existingChannelDic = new Dictionary<Server, IDualChannel>();
            if (existingChannel != null)
            {
                Server server = new Server(existingChannel.PeerAddress, Status.Initializing);
                existingChannelDic.Add(server, existingChannel as IDualChannel);
            }
            return GetValidChannel(newChannel, existingChannelDic);
        }

    }
}
