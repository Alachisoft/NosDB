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

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public class Intersecter<K, V>
    {
        private IDictionary<K, V> _setA;
        private IDictionary<K, V> _setB;
        private bool flipped = false;

        public Intersecter(IDictionary<K, V> initialResult)
        {
            _setA = initialResult;
            _setB = (IDictionary<K, V>) Activator.CreateInstance(initialResult.GetType());
        }

        public void Add(K key, V item)
        {
            if (!flipped)
            {
                if (_setA.ContainsKey(key))
                    _setB[key] = item;
            }
            else
            {
                if (_setB.ContainsKey(key))
                    _setA[key]= item;
            }
        }

        public IDictionary<K, V> FinalResult
        {
            get
            {
                if (!flipped)
                {
                    return _setA;
                }
                return _setB;
            }
        }

        public void Flip()
        {
            if (!flipped)
            {
                _setA.Clear();
            }
            else
            {
                _setB.Clear();
            }
            flipped = !flipped;
        }

    }
}
