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
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.Optimizer;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class SelectObject : IFilterObject
    {
        private readonly bool _isdistinct;
        private readonly string _collection;
        private readonly string _hint;
        private readonly List<IEvaluable> _projections;
        private readonly List<IEvaluable> _groupValue;
        private readonly ITreePredicate _filterPredicate;
        private readonly IntegerConstantValue _skip;
        private readonly IntegerConstantValue _limit;
        private List<IEvaluable> _orderValue;

        public SelectObject (bool isdistinct, List<IEvaluable> projections,
            string collection, ITreePredicate filterPredicate, List<IEvaluable> groupValue,
            List<IEvaluable> orderValue, IntegerConstantValue skip, IntegerConstantValue limit, string hint)
        {
            _isdistinct = isdistinct;
            _projections = projections;
            _collection = collection;
            _filterPredicate = filterPredicate;
            _groupValue = groupValue;
            _orderValue = orderValue;
            _skip = skip;
            _limit = limit;
            _hint = hint;
        }
        
        public bool IsDistinct
        {
            get { return _isdistinct; }            
        }

        public List<IEvaluable> Projections
        {
            get { return _projections; }            
        }

        public List<IEvaluable> GroupValue
        {
            get { return _groupValue; }            
        }

        public List<IEvaluable> OrderValue
        {
            get { return _orderValue; }
        }

        public string Collection
        {
            get { return _collection; }           
        }

        public IntegerConstantValue Skip
        {
            get { return _skip; }
        }

        public IntegerConstantValue Limit
        {
            get { return _limit; }            
        }

        public string Hint
        {
            get { return _hint; }
        }

        public ITreePredicate WherePredicate
        {
            get { return _filterPredicate; }
        }

        public void AddOrderByAttribute(Attribute attribute)
        {
            _orderValue = _orderValue ?? new List<IEvaluable>();
            _orderValue.Add(new BinaryExpression(attribute));
        }

        public string InString
        {
            get
            {
                string query = "SELECT ";

                if (_isdistinct)
                {
                    query += "DISTINCT ";
                }

                if (_limit != null && _skip == null)
                {
                    query += "TOP " + _limit + " ";
                }

                for (int i = 0; i < _projections.Count; i++)
                {
                    query += _projections[i];

                    if (i != Projections.Count - 1)
                    {
                        query += ",";
                    }
                }

                query += " FROM ";

                if (_collection.Contains("$"))
                {
                    query += "\"" + _collection + "\"";
                }
                else
                {
                    query += "$" + _collection + "$";
                }  

                if (_filterPredicate != null)
                {
                    query += " WHERE " + _filterPredicate.InString;
                }
                
                if (_groupValue != null)
                {
                    query += " GROUP BY ";
                    for (int i = 0; i < _groupValue.Count; i++)
                    {
                        query += _groupValue[i];

                        if (i != _groupValue.Count - 1)
                        {
                            query += ",";
                        }
                    }
                }

                if (_orderValue != null)
                {
                    query += " ORDER BY ";
                    for (int i = 0; i < _orderValue.Count; i++)
                    {
                        query += _orderValue[i];

                        if (i != _orderValue.Count - 1)
                        {
                            query += ",";
                        }
                    }
                }

                if (_skip != null)
                {
                    query += " OFFSET " + _skip.InString + " ROWS";
                }

                if (_limit != null && _skip != null)
                {
                    query += " FETCH NEXT" + _limit + " ROWS ONLY ";
                }

                if (_hint != null)
                {
                    query += _hint;
                }

                return query;
            }
        }
    }   
}
