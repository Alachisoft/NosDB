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
using System.Threading;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl
{
    public class ShardMulticastRequest<R,T> : ShardRequestBase<R> where R : IResponseCollection<T>,new()
    {        
        private IList<Server> _destinations=null;
        private IDictionary<Server, IAsyncResult> asyncResultDic = null;
        private R responseCollection =new R();        
        private IAsyncResult asyncResult = new MulticastAsyncResult();
        //private Func<IEnumerable<IClusterResponse<T>>, R> _factory = null;
        public ShardMulticastRequest(IShard shard, IList<Server> destinations, Message message)
            : base(shard, message)
        {
            _destinations = destinations;
          //  _factory = factory;
        }

        public override IAsyncResult BeginExecute()
        {            
            return BeginExecute(Timeout.Infinite);
        }

        public override IAsyncResult BeginExecute(int timeout)
        {
            if (_destinations != null && _destinations.Count > 0)
            {
                asyncResultDic = new Dictionary<Server, IAsyncResult>();
                IList<Server> expectedServers = new List<Server>();

                foreach (Server server in _destinations)
                {
                    try
                    {
                        asyncResultDic.Add(server, _shard.BeginSendMessage(server, _message));
                        expectedServers.Add(server);                
                    }
                    catch (Exception ex)
                    {
                    }
                    
                }

                responseCollection.ExpectedServers = expectedServers;
            }

            return asyncResult;
        }

        public override R EndExecute(IAsyncResult result)
        {
            if (!Object.ReferenceEquals(result,asyncResult))
            {
                throw new ArgumentException("provided result argument does not match");
            }

            if (asyncResultDic != null && asyncResultDic.Count > 0)
            {
                IList<IClusterResponse<T>> clusterResults=new List<IClusterResponse<T>>();

                foreach (KeyValuePair<Server, IAsyncResult> pair in asyncResultDic)
                {
                    IClusterResponse<T> response = null;
                    try
                    {
                        response = new ClusterResponse<T>((T)_shard.EndSendMessage(pair.Key, pair.Value), pair.Key);
                    }
                    catch (Exception ex)
                    {
                        response = new ClusterResponse<T>(default(T), pair.Key, false, 1);
                    }

                    clusterResults.Add(response);
                }


                responseCollection.Responses = clusterResults;
                return responseCollection; //_factory(clusterResults);
            }

            return default(R);
        }
    }
}
