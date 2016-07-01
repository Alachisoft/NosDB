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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Distributor.DataCombiners;
using Alachisoft.NosDB.Distributor.DistributedDataSets;

namespace Alachisoft.NosDB.Distributor.DataSelectors
{
    class DataSelectorGroupBy : IDataSelector
    {
       // IList<ISet> _sets;
        private IDataSelector _dataSelector;
        private List<IDataCombiner> _combiners;
        private ISetElement _current = null;
        private ISetElement _lastUnSentElement = null;
        private bool _closed = false;
        private IComparer _comparer;
        private List<IEvaluable> _projectionValues;


        #region IDataSelector Methods
        public void Initialize(IList<ISet> sets, List<IDataCombiner> combiners, IComparer comparer)
        {
            if(_combiners == null)
                _combiners = combiners;

            if(_dataSelector == null)
                _dataSelector = new DataSelectorOrdered();

            _dataSelector.Initialize(sets, combiners, comparer);
            _comparer = comparer;
        }

        public object Current
        {
            get
            {
                IJSONDocument currentDocumentForUser = new JSONDocument();
                foreach (var projectionValue in _projectionValues)
                {
                    string name;
                    BinaryExpression binaryExpression = projectionValue as BinaryExpression;
                    bool aliasExists = binaryExpression != null && binaryExpression.Alias != null;
                    if (aliasExists)
                        name = binaryExpression.Alias;
                    else
                    {
                        if (projectionValue.Functions.Count == 1)
                        {
                            name = projectionValue.Functions.First().GetCaseSensitiveName();
                        }
                        else
                        {
                            name = projectionValue.ToString();
                        }
                    }
                    currentDocumentForUser.Add(name, _current.Value[name]);
                }
                //currentDocumentForUser.Add(_combiner.Name, _current.Value.GetNumber(_combiner.Name));
                _current.Value = currentDocumentForUser;
                return _current;
            }
        }

        public bool MoveNext()
        {
            if (!_closed)
            {
                if (_combiners.Count < 1) { throw new Exception("DataCombiner is not defined for Group By Operation"); }


                if (_current == null && _lastUnSentElement == null)   // True first time only
                {
                    if (_dataSelector.MoveNext())
                    {
                        _lastUnSentElement = (ISetElement)_dataSelector.Current;
                    }
                    else
                    {
                        _closed = true;
                        return false;
                    }
                }

                if (_lastUnSentElement == null)
                {
                    _closed = true;
                    return false;
                }

               // bool found = false;
                _current = _lastUnSentElement;
                _lastUnSentElement = null;

                foreach (IDataCombiner dataCombiner in _combiners)
                {
                    dataCombiner.Reset();
                    _current.Value = dataCombiner.Initialize(_current.Value) as IJSONDocument;
                }

                while (_dataSelector.MoveNext())
                {
                    _lastUnSentElement = (ISetElement)_dataSelector.Current;
                    if (_comparer.Compare(_current.Value, _lastUnSentElement.Value) == 0)
                    {
                        //JSONDocumentComparer jdocComparer = (JSONDocumentComparer)_comparer;
                        //foreach (string field in jdocComparer.FieldNamesGroupBy)        // Can be optimized for better performance. Will do it later if required
                        //    _current.Value.Remove(field);
                        foreach (IDataCombiner dataCombiner in _combiners)
                        {
                            JsonDocumentUtil.Update(_current.Value, (IJSONDocument)dataCombiner.Combine(_lastUnSentElement.Value));
                           // _current.Value.Update((IJSONDocument)dataCombiner.Combine(_lastUnSentElement.Value));
                        }
                        _lastUnSentElement = null;
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }
            return false;
        }

        public void Reset()
        {
            _dataSelector.Reset();
        }
        #endregion

        public List<IEvaluable> ProjectionValues
        {
            set { _projectionValues = value; }
        }

        public IDataSelector DataSelector
        {
            set { _dataSelector = value; }
        }

        public List<IDataCombiner> DataCombiner
        {
            set { _combiners = value; }
        }
    }
}
