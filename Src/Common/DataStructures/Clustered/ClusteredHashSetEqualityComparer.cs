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

namespace Alachisoft.NosDB.Common.DataStructures.Clustered
{
    /// <summary>
    /// Equality comparer for hashsets of hashsets
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if !FEATURE_NETCORE
    [Serializable()]
#endif
    internal class ClusteredHashSetEqualityComparer<T> : IEqualityComparer<ClusteredHashSet<T>>
    {

        private IEqualityComparer<T> m_comparer;

        public ClusteredHashSetEqualityComparer()
        {
            m_comparer = EqualityComparer<T>.Default;
        }

        public ClusteredHashSetEqualityComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                m_comparer = EqualityComparer<T>.Default;
            }
            else
            {
                m_comparer = comparer;
            }
        }

        // using m_comparer to keep equals properties in tact; don't want to choose one of the comparers
        public bool Equals(ClusteredHashSet<T> x, ClusteredHashSet<T> y)
        {
            return ClusteredHashSet<T>.HashSetEquals(x, y, m_comparer);
        }

        public int GetHashCode(ClusteredHashSet<T> obj)
        {
            int hashCode = 0;
            if (obj != null)
            {
                foreach (T t in obj)
                {
                    hashCode = hashCode ^ (m_comparer.GetHashCode(t) & 0x7FFFFFFF);
                }
            } // else returns hashcode of 0 for null hashsets
            return hashCode;
        }

        // Equals method for the comparer itself. 
        public override bool Equals(Object obj)
        {
            ClusteredHashSetEqualityComparer<T> comparer = obj as ClusteredHashSetEqualityComparer<T>;
            if (comparer == null)
            {
                return false;
            }
            return (this.m_comparer == comparer.m_comparer);
        }

        public override int GetHashCode()
        {
            return m_comparer.GetHashCode();
        }
    }
}