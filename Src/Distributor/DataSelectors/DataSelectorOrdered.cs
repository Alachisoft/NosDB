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
using Alachisoft.NosDB.Distributor.Comparers;
using Alachisoft.NosDB.Distributor.DataCombiners;
using Alachisoft.NosDB.Distributor.DistributedDataSets;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;

namespace Alachisoft.NosDB.Distributor.DataSelectors
{
    public class DataSelectorOrdered:IDataSelector
    {
        private List<ISet> _sets = new List<ISet>();
        private List<ISetElement> _topELements = new List<ISetElement>();
        private List<IDataCombiner> _combiners;
        private bool _closed = false;
        private ISetElement _current;
        IComparer _comparer;
        private IComparer<ISetElement> _topElementComparer;
        private List<IEvaluable> _projectionValues;

        public void Initialize(IList<ISet> sets, List<IDataCombiner> combiners, IComparer comparer)
        {
            _sets.AddRange(sets);
            _combiners = combiners;
            _comparer = comparer;
            _topElementComparer = new SetElementComparer(comparer);
        }

        private object GetNextElement()
        {
            throw new NotImplementedException();
        }

        public object Current
        {
            //get { return _current; }
            get
            {
                if (_projectionValues == null)
                    return _current;
                bool isModified = false;
                IJSONDocument currentDocumentForUser = new JSONDocument();
                foreach (var projectionValue in _projectionValues)
                {
                    if (projectionValue.InString == "*")
                        return _current;
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
                    isModified = true;
                    currentDocumentForUser.Add(name, _current.Value[name]);
                }
                //currentDocumentForUser.Add(_combiner.Name, _current.Value.GetNumber(_combiner.Name));
                if(isModified)
                    _current.Value = currentDocumentForUser;
                return _current;
            }
        }

        public bool MoveNext()
        {
            if (!_closed && _sets.Count > 0)
            {
                if (_current == null)       // For the first time it will be null
                {
                    foreach (ISet set in _sets)
                    {
                        ISetElement element = set.GetTopElement();
                        set.DeleteTopElement();
                        _topELements.Add(element);
                    }

                    _topELements.Sort(_topElementComparer);
                    _current = _topELements[0];

                    if (_current == null)
                    {
                        _closed = true;
                        return false;
                    }

                    _topELements.RemoveAt(0);

                    return true;
                }

                ISetElement newlyExtractedElement = _current.Set.GetTopElement();
                
                if (newlyExtractedElement != null)
                {
                    _current.Set.DeleteTopElement();
                    int index = 0;
                    foreach (ISetElement element in _topELements)
                    {
                        if (_comparer.Compare(element.Value, newlyExtractedElement.Value) < 0)
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    _topELements.Insert(index, newlyExtractedElement);
                    _current = _topELements.First();
                    _topELements.RemoveAt(0);
                    return true;
                }
                else if (_topELements.Count > 0 || _sets.Count > 0)
                {
                    if (!_current.Set.IsFixedSize)
                    {
                        _current.Set.Load();
                        _topELements.Add(_current.Set.GetTopElement());
                        _current.Set.DeleteTopElement();
                        _topELements.Sort(_topElementComparer); // This cost can be reduced
                    }
                    else
                    {
                        _current.Set.DisposeReader();
                        _sets.Remove(_current.Set);
                        if (_topELements.Count == 0)
                        {
                            _current = null;
                            return false;
                        }
                    }
                    _current = _topELements.First();
                    _topELements.RemoveAt(0);
                    return true;
                }
                else
                {
                    _current = null;
                    _closed = true;
                    return false;
                }
            }
            return false;
        }

        public List<IEvaluable> ProjectionValues
        {
            set { _projectionValues = value; }
        }

        public void Reset()
        {
            //throw new NotImplementedException();
            //We do not implement reset
            foreach (ISet set in _sets)
            {
                set.DisposeReader();
            }
        }
    }

 /*   #region /--        For maintaining the _sets index from which the element was extracted  --/

    private class SetIndexMaintainer
    {
  
        ISet _set;

        public SetIndexMaintainer(int index, ISet set)
        {
            _index = index;
            _set = set;
        }

        public 
    }

    #endregion*/
}
