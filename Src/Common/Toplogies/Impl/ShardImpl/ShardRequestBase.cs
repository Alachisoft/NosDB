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
using Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl
{
    public abstract class ShardRequestBase<T>:IExecutableRequest<T>
    {
        protected IShard _shard;
        protected Message _message;

        public ShardRequestBase(IShard shard,Message message)
        {
            _shard = shard;
            _message = message;
        }

        public virtual IAsyncResult BeginExecute() { return null; }

        public virtual IAsyncResult BeginExecute(int timeout) { return null; }
       
        public virtual T EndExecute(IAsyncResult result)
        {
            return default(T);
        }

        public long RequestId
        {
            get;
            set;
        }
    }
}
