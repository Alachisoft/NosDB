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
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    public class BucketsPack : ICloneable
    {
        private ArrayList _bucketIds = new ArrayList();
        private NodeIdentity _owner;
        private Boolean _movable = true;

        public Boolean Movable
        {
            get { return _movable; }
            set { _movable = value; }
        }
        

        public BucketsPack(ArrayList buckets, NodeIdentity owner)
        {
            if (buckets != null)
                _bucketIds = buckets;
            
            _owner = owner;
        }

        public ArrayList BucketIds
        {
            get
            {
                lock (_bucketIds.SyncRoot)
                {
                    return _bucketIds;
                }
            }
        }

        public NodeIdentity Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj is BucketsPack)
            {
                return this._owner.Equals(((BucketsPack)obj)._owner);
            }
            return false;
        }

#if DEBUGSTATETRANSFER
        public override string ToString()
        {
            return "{ [" + _owner + "] [" + Global.CollectionToString(_bucketIds) + "] }";
        } 
#endif

        public object Clone()
        {
            BucketsPack pack = new BucketsPack(new ArrayList(_bucketIds), _owner);
            return pack;
        }
    }
}