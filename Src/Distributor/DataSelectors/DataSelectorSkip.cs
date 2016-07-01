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
using System.Collections.Generic;
using Alachisoft.NosDB.Distributor.DataCombiners;
using Alachisoft.NosDB.Distributor.DistributedDataSets;

namespace Alachisoft.NosDB.Distributor.DataSelectors
{
    public class DataSelectorSkip : IDataSelector
    {
        private IDataSelector _dataSelector;
        private long _skip;
        private ISetElement _current;

        public DataSelectorSkip(IDataSelector dataSelector, long skip)
        {
            _dataSelector = dataSelector;
            _skip = skip;
        }

        #region IDataSelector Methods
        public void Initialize(IList<ISet> sets, List<IDataCombiner> combiners, System.Collections.IComparer comparer)
        {
            _dataSelector.Initialize(sets, combiners, comparer);
        }

        public object Current
        {
            get { return _current; }
        }

        public bool MoveNext()
        {
            while (_skip > 0)
            {
                if (!_dataSelector.MoveNext()) 
                {
                    _current = null;
                    return false; 
                }
                _skip--;
            }

            if (_dataSelector.MoveNext())
            {
                _current = (ISetElement)_dataSelector.Current;
                return true;
            }
                
            _current = null;
            return false;
        }

        public void Reset()
        {
            _skip = 0;
            _dataSelector.Reset();
        }
        #endregion
    }
}
