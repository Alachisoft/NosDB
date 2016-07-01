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
using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.ADO.NETProvider
{
    public class NosDbConnection : DbConnection, IDbConnection
    {
        private string _connectionString;
        private System.Data.ConnectionState _state;
        internal Database _database = null;

        public NosDbConnection(string connectionString)
        {
            this._state = System.Data.ConnectionState.Closed;
            this._connectionString = connectionString;
        }

        public override void ChangeDatabase(string databaseName)
        {
            //TODO Change Database Logic
        }
        
        public override string ConnectionString
        {
            get { return _connectionString; }
            set { this._connectionString = value; }
        }
        
        public override string Database
        {
            get { return _database != null ? _database.ToString() : ""; }
        }

        public override void Open()
        {
            if (string.IsNullOrEmpty(_connectionString) || string.IsNullOrWhiteSpace(_connectionString))
                throw new ConnectionException("Connection string cannot be empty or whitespace");

            _database = Alachisoft.NosDB.Client.NosDB.InitializeDatabase(_connectionString);
            _state = ConnectionState.Open;
        }


        public void Dispose()
        {
            if (_state == System.Data.ConnectionState.Open)
                this.Close();
            _state = ConnectionState.Closed;
        }
        
        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return new NosDbTransaction(this);
        }

        public override void Close()
        {
            //TODO _database.Close(); OR _database = null;
            _state = System.Data.ConnectionState.Closed;
        }


        protected override DbCommand CreateDbCommand()
        {
            return new NosDbCommand("", this);
        }

        public override string DataSource
        {
            get { return _database.ToString(); }
        }
        

        public override string ServerVersion
        {
            get { throw new NotImplementedException(); }
        }


        public override System.Data.ConnectionState State
        {
            get { return _state; }
        }
    }
}
