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
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Distributor.DataCombiners;
using Alachisoft.NosDB.Distributor.DistributedDataSets;

namespace Alachisoft.NosDB.Distributor.DataSelectors
{
    class DataSelectorRoundRobin : IDataSelector
    {
        private IList<ISet> _distributedDataSetsList;
        private List<IDataCombiner> _dataCombiners;
        private object _currentElement;     // ISetElement
        private string _lastReaderId = null;

        #region IDataSelector Methods

        public void Initialize(IList<ISet> sets, List<IDataCombiner> combiners, IComparer comparer)
        {
            _distributedDataSetsList = sets;
            _dataCombiners = combiners;
        }

        public object Current
        {
            get { return _currentElement; }
        }

        public bool MoveNext()
        {
            bool readerUIDFound = false;
            bool documentFound = false;
            IList<ISet> dataSetToBeRemoved = new List<ISet>();     // Remove the shards where no more data is available
            int skipped = 0;

            if (_distributedDataSetsList.Count == 0) return false;
            foreach (ISet set in _distributedDataSetsList)
            {
                if (set.ReaderUID.Equals(_lastReaderId) || readerUIDFound || _lastReaderId == null)
                {
                    _lastReaderId = set.ReaderUID;
                    readerUIDFound = true;
                    int setsToBeRemovedCount = dataSetToBeRemoved.Count;
                    _currentElement = GetCurrentElement(set, dataSetToBeRemoved);

                    if (_currentElement != null)
                    {
                        documentFound = true;
                        break;
                    }
                    else if (dataSetToBeRemoved.Count - setsToBeRemovedCount == 0)
                    {
                        skipped++;
                        set.Load();
                        continue;
                    }

                }
                else
                {
                    skipped++;
                }
            }
            if (!readerUIDFound) { throw new Exception("At QueryRouter: Invalid Reader UID"); }


            int counter = 0;
            if (documentFound == false)
            {
                foreach (ISet set in _distributedDataSetsList)
                {
                     if (skipped == counter) { break; }

                    _currentElement = GetCurrentElement(set, dataSetToBeRemoved);
                    if (_currentElement != null)
                    {
                        _lastReaderId = set.ReaderUID;
                        documentFound = true;
                        break;
                    }
                    counter++;
                }
            }

            if (dataSetToBeRemoved.Count != 0)
            {
                ListUtilMethods.RemoveMultipleItemsFromList(_distributedDataSetsList, dataSetToBeRemoved);
            }
            return documentFound;
        }

        public void Reset()
        {
            _distributedDataSetsList.Clear();
        }
        #endregion

        private ISetElement GetCurrentElement(ISet set, IList<ISet> dataSetToBeRemoved)
        {
            ISetElement currentElement = set.GetTopElement();
            if (currentElement == null)
            {
                if (set.IsFixedSize == false)
                {
                    return null;
                }
                else
                {
                    set.DisposeReader();
                    dataSetToBeRemoved.Add(set);
                }
            }
            else
            {
                set.DeleteTopElement();
            }
            return currentElement;
        }
    }
}
