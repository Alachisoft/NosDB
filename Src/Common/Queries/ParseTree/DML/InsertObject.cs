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
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.ErrorHandling;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class InsertObject : IDmObject, System.ICloneable
    {
        private readonly string _collection;
        private IList<KeyValuePair<Attribute, IEvaluable>> _values;

        public InsertObject(string collection,
            IList<KeyValuePair<Attribute, IEvaluable>> values)
        {
            _collection = collection;
            _values = values;
        }

        public string Collection
        {
            get { return _collection; }
        } 

        public IEnumerable<KeyValuePair<Attribute, IEvaluable>> ValuesToInsert
        {
            get { return _values; }
        }

        public string InString
        {
            get
            {
                string query = "INSERT INTO ";
                    
                if(_collection.Contains("$"))
                {
                    query += "\"" + _collection + "\"";
                }
                else
                {
                    query += "$" + _collection + "$";
                }  
 
                query += " (";

                foreach (KeyValuePair<Attribute, IEvaluable> value in _values)
                {
                    query += value.Key.InString;
                    if (_values[_values.Count - 1].Key != (value.Key))
                    {
                        query += ",";
                    }
                }

                query += " ) VALUES (";

                foreach (KeyValuePair<Attribute, IEvaluable> value in _values)
                {
                    query += value.Value.InString;

                    if (_values[_values.Count - 1].Value != (value.Value))
                    {
                        query += ",";
                    }
                }

                return query + " )";
            }
        }

        public void AddDocumentKey(string key)
        {
            if (_values == null)
                _values = new List<KeyValuePair<Attribute, IEvaluable>>();

            Attribute keyAttribute = new Attribute(JsonDocumentUtil.DocumentKeyAttribute);
            KeyValuePair<Attribute,IEvaluable> keypair = new KeyValuePair<Attribute,IEvaluable>(keyAttribute, new StringConstantValue(key));
            _values.Add(keypair);
        }

        public object Clone()
        {
            IList<KeyValuePair<Attribute, IEvaluable>> values = new List<KeyValuePair<Attribute, IEvaluable>>();
            if (_values != null)
            {
                foreach (var pair in _values)
                {
                    values.Add(new KeyValuePair<Attribute, IEvaluable>(pair.Key, pair.Value));
                }
            }
            return new InsertObject(_collection, values); 
        }
    }
}
