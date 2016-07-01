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
using Alachisoft.NosDB.Common.Memory;

namespace Alachisoft.NosDB.Core.Storage.Caching
{
    public class FirstComeFirstServe : ICacheSpacePolicy
    {
        public void AddConsumer(ICacheSpaceConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public void RemoveConsumer(ICacheSpaceConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public bool CanConsumeSpace(CacheSpace space, ICacheSpaceConsumer consumer, long requiredSpace)
        {
            if (space.AvaialbeSpace < requiredSpace)
            {
                return EvictData(space, consumer, requiredSpace);
            }
            return true;
            // if user thread is supposed to do eviction then there is no need for threshold i guess.
            //if (space.AvaialbeSpace < space.Threshold)
            //{
            //    //TODO: start eviction currently it is need base to user thread will do the eviction.
            //}
        }

        public bool EvictData(CacheSpace space, ICacheSpaceConsumer consumer, long requiredSpace)
        {
            if (!consumer.IsEvictionEnabled)
                return false;
            consumer.EvictData(requiredSpace);
            return true;
        }
    }
}
