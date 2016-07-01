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
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Toplogies.Impl.Cluster;
using Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.Cluster
{
    public class ResponseCollection<T>:IResponseCollection<T>
    {
        IEnumerable<IClusterResponse<T>> responses = null;
        IEnumerable<Server> _expectedServer = null;

        public IClusterResponse<T> GetResponse(Server server)
        {
            IClusterResponse<T> response = null;
            if (responses != null)
            {
                IEnumerator enumerator = responses.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    IClusterResponse<T> res = (IClusterResponse<T>)enumerator.Current;
                    if (res.Server.Equals(server))
                    {
                        response = res;
                        break;
                    }
                }
            }
            return response;
        }

        IEnumerable<IClusterResponse<T>> IResponseCollection<T>.Responses
        {
            get
            {
                return responses;
            }
            set
            {
                responses = value;
            }
        }


        IEnumerable<Server> IResponseCollection<T>.ExpectedServers
        {
            get
            {
                return _expectedServer;
            }
            set
            {
                _expectedServer = value;
            }
        }
    }
}