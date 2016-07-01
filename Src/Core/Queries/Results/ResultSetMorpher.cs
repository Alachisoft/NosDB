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

using Alachisoft.NosDB.Common.Queries.Results;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public static class ResultSetMorpher<T>
    {
        public static IResultSet<T> ToBaggedResultSet(IResultSet<T> source)
        {
            if (source is ListedResultSet<T>)
                return source;
            return new ListedResultSet<T>(source.GetEnumerator());
        }

        public static IResultSet<T> ToHashedResultSet(IResultSet<T> source)
        {
            if (source is HashedResultSet<T>)
                return source;
            return new HashedResultSet<T>(source.GetEnumerator());
        }

        public static IResultSet<T> ToSortedResultSet(IResultSet<T> source)
        {
            if (source is SortedResultSet<T>)
                return source;
            return new SortedResultSet<T>(source.GetEnumerator());
        }
    }
}
