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

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Cluster
{
    public class ClusterResponse<T> : IClusterResponse<T>
    {
        private T value;
        private ShardImpl.Server server;
        private Boolean isSuccessfull;
        private int errorCode;

        public ClusterResponse(T v, ShardImpl.Server server, Boolean success = true, int errorCode = 0) 
        {
            this.value = v;
            this.server = server;
            this.isSuccessfull = success;
            this.errorCode = errorCode;
        }

        public T Value
        {
            get { return value; }
        }

        public bool IsSuccessfull
        {
            get { return isSuccessfull; }
        }

        public int ErrorCode
        {
            get { return errorCode; }
        }

        public ShardImpl.Server Server
        {
            get { return server; }
        }
    }
}