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
using System.IO;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.IO;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Core.Storage;

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public  class DiskResultSet<T> : IResultSet<T>
    {
        private UFileManager _fileManager;
        private IResultSet<T> list; 
        private string path;
        private int batchSize;
        private int batchNo;
        private int count;

        public DiskResultSet(string filePath, int batchingSize = 1000)
        {
            path = filePath;
            batchSize = batchingSize;
            if (File.Exists(path))
                path += "-" + Guid.NewGuid().ToString();
            _fileManager = UFileManager.Create(path, new ObjectStore.CompactObjectSerializer(), false);
            list = new ListedResultSet<T>();
        }

        public void Add(T result)
        {
            list.Add(result);
            if (list.Count >= batchSize)
            {
                _fileManager.WriteObject(batchNo.ToString(), list);
                batchNo++;
            }
            count++;
        }

        public void Remove(T result)
        {
            throw new NotImplementedException();
        }

        public void Populate(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                Add(enumerator.Current);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public void Clear()
        {
            _fileManager.Clear();
            count = 0;
            batchNo = 0;
        }

        public Common.Enum.ResultType ResultType
        {
            get { return ResultType.Persisted; }
        }

        public T this[T reference]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Contains(T value)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<KeyValuePair<string, object>> fileEnumerator = _fileManager.GetEnumerator();
            while (fileEnumerator.MoveNext())
            {
                if (fileEnumerator.Current.Value is T)
                    yield return (T) fileEnumerator.Current.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose()
        {
            _fileManager.Dispose();
            File.Delete(path);
        }
    }
}
