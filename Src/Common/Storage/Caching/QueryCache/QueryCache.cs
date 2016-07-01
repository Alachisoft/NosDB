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
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Queries.Parser;
using Alachisoft.NosDB.Common.Queries.ParseTree;
using Alachisoft.NosDB.Common.Queries.Util;
using Alachisoft.NosDB.Common.Storage.Caching.LightCache;


namespace Alachisoft.NosDB.Common.Storage.Caching.QueryCache
{
    public class QueryCache<TDqlObject> where TDqlObject : class, IDqlObject
    {
        private readonly QueryParsingHelper _parser = new QueryParsingHelper();
        private readonly LightCache<string, TDqlObject> _parsedObjects;

        public QueryCache()
        {
            _parsedObjects = new LightCache<string, TDqlObject>();
        }

        public QueryCache(int size, int evictionPercentage)
        {
            _parsedObjects = new LightCache<string, TDqlObject>(size, evictionPercentage);
        }

        public TDqlObject GetParsedQuery(string query)
        {
            if (_parsedObjects.Contains(query))
            {
                return _parsedObjects[query];
            }

            if (_parser.Parse(query) != ParseMessage.Accept)
            {
                throw new QuerySystemException(ErrorCodes.Query.INVALID_SYNTAX,
                    new[] { _parser.Keyword, _parser.LineNumber });
            }

            TDqlObject value = _parser.CurrentReduction.Tag as TDqlObject;

            if (value != null)
            {
                _parsedObjects.TryAdd(query, value);
            }

            return value;
        }

        public void RemoveReduction(string query)
        {
            TDqlObject parsedQuery;
            _parsedObjects.TryRemove(query, out parsedQuery);
        }
    }
}
