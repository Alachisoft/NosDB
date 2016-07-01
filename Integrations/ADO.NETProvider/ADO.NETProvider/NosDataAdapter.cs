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
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.ADO.NETProvider
{
    public class NosDataAdapter : DbDataAdapter, IDbDataAdapter
    {
        private DbCommand m_selectCommand;
        private DbCommand m_insertCommand;
        private DbCommand m_updateCommand;
        private DbCommand m_deleteCommand;

        /*
         * Inherit from Component through DbDataAdapter. The event
         * mechanism is designed to work with the Component.Events
         * property. These variables are the keys used to find the
         * events in the components list of events.
         */
        static private readonly object EventRowUpdated = new object();
        static private readonly object EventRowUpdating = new object();

        public NosDataAdapter()
        {
        }
        public NosDataAdapter(DbCommand command)
        {
            m_selectCommand = command;
        }
        public NosDataAdapter(string commandText, string connectionString)
        {
        }
        public NosDataAdapter(string selectCommandText, DbConnection connection)
        {
            m_selectCommand = new NosDbCommand(selectCommandText);
            m_selectCommand.Connection = connection;
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get { return m_selectCommand; }
            set { m_selectCommand = (NosDbCommand)value; }
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get { return m_insertCommand; }
            set { m_insertCommand = (NosDbCommand)value; }
        }
        
        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get { return m_updateCommand; }
            set { m_updateCommand = (NosDbCommand)value; }
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get { return m_deleteCommand; }
            set { m_deleteCommand = (NosDbCommand)value; }
        }

        /*
         * Implement abstract methods inherited from DbDataAdapter.
         */
        override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new NosRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new NosRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        override protected void OnRowUpdating(RowUpdatingEventArgs value)
        {
            NosRowUpdatingEventHandler handler = (NosRowUpdatingEventHandler)Events[EventRowUpdating];
            if ((null != handler) && (value is NosRowUpdatingEventArgs))
            {
                handler(this, (NosRowUpdatingEventArgs)value);
            }
        }

        override protected void OnRowUpdated(RowUpdatedEventArgs value)
        {
            NosRowUpdatedEventHandler handler = (NosRowUpdatedEventHandler)Events[EventRowUpdated];
            if ((null != handler) && (value is NosRowUpdatedEventArgs))
            {
                handler(this, (NosRowUpdatedEventArgs)value);
            }
        }

        public event NosRowUpdatingEventHandler RowUpdating
        {
            add { Events.AddHandler(EventRowUpdating, value); }
            remove { Events.RemoveHandler(EventRowUpdating, value); }
        }

        public event NosRowUpdatedEventHandler RowUpdated
        {
            add { Events.AddHandler(EventRowUpdated, value); }
            remove { Events.RemoveHandler(EventRowUpdated, value); }
        }
    }
}
