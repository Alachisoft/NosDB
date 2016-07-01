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
using System.Threading;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Distributor;

namespace Alachisoft.NosDB.Client
{
    internal class NoSdbInstance : IResource
    {
        private readonly DistributorDatabaseEngine _distributor;
        private readonly Database _database;
        private ConnectionStringBuilder _connectionString;

        
        private int _refCount;

        public int RefCount
        {
            get { return _refCount; }
        }

        public NoSdbInstance(ConnectionStringBuilder connectionString)
        {
            _connectionString = connectionString;

            _distributor = new DistributorDatabaseEngine(_connectionString);
            _distributor.Start();
            _database = new Database(this)
            {
                ExecutionMapper = _distributor,
                DatabaseName = _connectionString.Database
            };
           
        }

        public void FreeResource()
        {
            Interlocked.Decrement(ref _refCount);
            if (_refCount < 0) Interlocked.Exchange(ref _refCount, 0);
            if (_refCount == 0)
            {
                _distributor.Dispose();
                NosDB.Dipose(_connectionString);
            }
        }

        public Database GetDatabase()
        {
            Interlocked.Increment(ref _refCount);
            return _database;
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref _refCount);
            if (_refCount == 0)
            {
                (_database as IDisposable).Dispose();
                _distributor.Stop();
            }
        }
        public string ConnectionString
        {
            get { return _connectionString.ToString(); }
        }
    }
}