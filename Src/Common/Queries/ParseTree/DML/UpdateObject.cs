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
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.Updation;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class UpdateObject : IFilterObject
    {
        private readonly string _collection;
        public readonly Updator _updator;
        private readonly ITreePredicate _filterPredicate;

        public UpdateObject(string collection, Updator updator,
            ITreePredicate filterPredicate)
        {
            _collection = collection;
            _updator = updator;
            _filterPredicate = filterPredicate;
        }
        
        public Updator Updator
        {
            get { return _updator; }
        }
       
        public ITreePredicate WherePredicate
        {
            get { return _filterPredicate; }

        }

        public string Collection
        {
            get { return _collection; }
        }

        public string InString
        {
            get
            {
                string query = "UPDATE ";
                
                if (_collection.Contains("$"))
                {
                    query += "\"" + _collection + "\"";
                }
                else
                {
                    query += "$" + _collection + "$";
                }

                query += " SET (" + Updator.UpdateString + ") ";

                if (_filterPredicate != null)
                {
                    query += " WHERE " +_filterPredicate.InString;
                }

                return query;
            }
        }
    }
}
