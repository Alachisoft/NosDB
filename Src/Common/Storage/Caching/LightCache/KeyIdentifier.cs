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

namespace Alachisoft.NosDB.Common.Storage.Caching.LightCache
{
    class KeyIdentifier<TKey> : IComparable
    {
        private readonly TKey _key;
        private ulong _refCount;

        public KeyIdentifier(TKey key)
        {
            _key = key;
            _refCount = 1;
        }

        public TKey Key
        {
            get { return _key; }
        }

        private void AddRef()
        {
            lock (this)
            {
                _refCount++;
            }
        }

        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var other = obj as KeyIdentifier<TKey>;
            if (other != null && _key.Equals(other.Key))
            {
                AddRef();
                other.AddRef();
                return true;
            }
            return false;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            int result = 0;
            if (obj is KeyIdentifier<TKey>)
            {
                KeyIdentifier<TKey> other = (KeyIdentifier<TKey>)obj;
                
                if (other._refCount > _refCount)
                {
                    result = -1;
                }
                else if (other._refCount < _refCount)
                {
                    result = 1;
                }
            }
            return result;
        }

        #endregion
    }
}