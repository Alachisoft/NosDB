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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.ParseTree;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Storage.Caching;
using Alachisoft.NosDB.Core.Storage.Queries;
using Alachisoft.NosDB.Core.Storage.Queries.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.ADO.NETProvider
{
    public class NosDbCommand : DbCommand, IDbCommand
    {
        private string _commandText;
        private DbConnection _connection;
        private DbTransaction _transaction;
        private bool _designTimeVisible = false;

        private DbParameterCollection _parameters = new NosDataParameterCollection();
        UpdateRowSource _updatedRowSource = UpdateRowSource.None;

        private int _timeout = 0;

        public NosDbCommand(string commandText)
        {
            this._commandText = commandText;
        }

        public NosDbCommand(string commandText, NosDbConnection connection)
        {
            this._commandText = commandText;
            this._connection = connection;
        }
        
        public override void Cancel()
        {
            throw new NotSupportedException();
        }

        public override string CommandText
        {
            get { return _commandText; }
            set { this._commandText = value; }
        }

        public override int CommandTimeout
        {
            get { return this._timeout; }
            set { this._timeout = value; }
        }

        public override System.Data.CommandType CommandType
        {
            get { return System.Data.CommandType.Text; }
            set { if (value != System.Data.CommandType.Text) throw new NotSupportedException(); }
        }

        protected override DbConnection DbConnection
        {
            get { return this._connection; }
            set
            {
                if (_connection != value)
                    this._transaction = null;
                this._connection = (NosDbConnection)value;
            }
        }

        public override int ExecuteNonQuery()
        {
            if (_connection == null)
                throw new Exception("You must set the Connection property before execution.");
            if(string.IsNullOrEmpty(_commandText))
                throw new Exception("Invalid query specified.");

            ICollection<Common.Server.Engine.IParameter> _params = new List<Common.Server.Engine.IParameter>();

            Alachisoft.NosDB.Common.Storage.Caching.QueryCache.QueryCache<IDqlObject> queryParser = new Alachisoft.NosDB.Common.Storage.Caching.QueryCache.QueryCache<IDqlObject>();
            IDqlObject parsedQuery = queryParser.GetParsedQuery(_commandText);

            foreach (IDataParameter idp in _parameters)
            {
                _params.Add(new Common.Server.Engine.Impl.Parameter(idp.ParameterName, idp.Value));
            }
            Client.Collection<JSONDocument> coll = ((NosDbConnection)_connection)._database.GetCollection(((IDmObject)parsedQuery).Collection);

            long val = coll.ExecuteNonQuery(_commandText, _params);
            //ICollectionReader value = _connection._database.ExecuteQuery(query);

            return (int)val;
        }
                
        public override object ExecuteScalar()
        {
            if (_connection == null)
                throw new Exception("You must set the Connection property before execution.");
            if (string.IsNullOrEmpty(_commandText))
                throw new Exception("Invalid query specified.");

            ICollection<Common.Server.Engine.IParameter> _params = new List<Common.Server.Engine.IParameter>();

            Alachisoft.NosDB.Common.Storage.Caching.QueryCache.QueryCache<IDqlObject> queryParser = new Alachisoft.NosDB.Common.Storage.Caching.QueryCache.QueryCache<IDqlObject>();
            IDqlObject parsedQuery = queryParser.GetParsedQuery(_commandText);

            if(parsedQuery is SelectObject)
            {
                foreach (IDataParameter idp in _parameters)
                {
                    _params.Add(new Common.Server.Engine.Impl.Parameter(idp.ParameterName, idp.Value));
                }
                Client.Collection<JSONDocument> coll = ((NosDbConnection)_connection)._database.GetCollection(((SelectObject)parsedQuery).Collection);

                return coll.ExecuteScalar(_commandText, _params);
                //ICollectionReader value = _connection._database.ExecuteQuery(query);
            }
            return null;
        }

        public override void Prepare()
        {
        }

        protected override DbTransaction DbTransaction
        {
            get { return this._transaction; }
            set { this._transaction = value; }
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get { return this._updatedRowSource; }
            set { this._updatedRowSource = value; }
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        protected override DbParameter CreateDbParameter()
        {
            return new NosDataParameter();
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _parameters; }
        }

        public override bool DesignTimeVisible
        {
            get { return _designTimeVisible; }
            set { _designTimeVisible = value; }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            if (_connection == null)
                throw new Exception("You must set the Connection property before execution.");
            if (string.IsNullOrEmpty(_commandText))
                throw new Exception("Invalid query specified.");
            if(_connection.State != ConnectionState.Open)
                throw new Exception("Connection must be open to execute the command.");

            try
            {
                ArrayList al = new ArrayList();
                ICollection<Common.Server.Engine.IParameter> _params = new List<Common.Server.Engine.IParameter>();

                Alachisoft.NosDB.Common.Storage.Caching.QueryCache.QueryCache<IDqlObject> queryParser = new Alachisoft.NosDB.Common.Storage.Caching.QueryCache.QueryCache<IDqlObject>();
                IDqlObject parsedQuery = queryParser.GetParsedQuery(_commandText);

                if (parsedQuery != null && parsedQuery is SelectObject)
                {
                    SelectObject select = parsedQuery as SelectObject;
                    if (select.Projections != null)
                    {
                        al = new ArrayList();
                        foreach (IEvaluable projection in select.Projections)
                        {
                            string prjct = "";
                            if (projection is BinaryExpression)
                                prjct = ((BinaryExpression)projection).Alias;
                            else
                                prjct = projection.CaseSensitiveInString.Replace("$", "");

                            if (!prjct.Contains('*'))
                            {
                                if (prjct.Contains('.')) // Incase of embedded, take the last one out.
                                {
                                    string[] parts = prjct.Split('.');
                                    prjct = parts[parts.Length - 1];
                                }
                                al.Add(prjct);
                            }
                        }
                    }
                }

                foreach (IDataParameter idp in _parameters)
                {
                    _params.Add(new Common.Server.Engine.Impl.Parameter(idp.ParameterName, idp.Value));
                }
                Client.Collection<JSONDocument> coll = ((NosDbConnection)_connection)._database.GetCollection(((SelectObject)parsedQuery).Collection);

                long val = 0;
                ICollectionReader reader = null;
                if (parsedQuery is SelectObject)
                    reader = coll.ExecuteReader(_commandText, _params);
                else
                    val = ((NosDbConnection)_connection)._database.ExecuteNonQuery(_commandText, _params);
                //    reader = _connection._database.ExecuteQuery(query);

                NosDataReader readerr = new NosDataReader(reader);
                readerr.AttributesColumns = al;
                readerr._rowsAffected = (int)val;
                return readerr;
            }
            finally
            {
                if( behavior == CommandBehavior.CloseConnection)
                    this._connection.Close();
            }
        }
        
    }
}
