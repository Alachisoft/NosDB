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
using System;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Client
{
    public sealed class NosDB
    {
        private static readonly Dictionary<RouterToken, NoSdbInstance> DatabaseInstances =
            new Dictionary<RouterToken, NoSdbInstance>();

        /// <summary>
        /// Initializes an instance of the database
        /// </summary>
        /// <param name="connectionString">
        /// String format: "nosdb://IP:PORT/cluster/databaseName/UserID:Password"
        /// OR
        /// "ncachedb://IP:PORT/databaseName"
        /// </param>
        /// <returns></returns>
        /// 

        public static Database InitializeDatabase(string connectionString)
        {
            return InitializeDatabase(connectionString, new DBInitParams());
        }

        public static Database InitializeDatabase(ConnectionStringBuilder connectionStringBuilder)
        {
            return InitializeDatabase(connectionStringBuilder, new DBInitParams());
        }

        internal static Database InitializeDatabase(string connectionString, DBInitParams initParams)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString", "Value cannot be null.");
            if (string.IsNullOrEmpty(connectionString.Trim())) throw new ArgumentNullException("connectionString", "Value cannot be empty string.");

            ConnectionStringBuilder connectionStringBuilder = new ConnectionStringBuilder(connectionString);
            return InitializeDatabase(connectionStringBuilder, initParams);
        }

        internal static Database InitializeDatabase(ConnectionStringBuilder connectionString, DBInitParams initParams)
        {
            if (connectionString == null) 
                throw new ArgumentNullException("connectionString", "Value cannot be null");
            if (connectionString.DataSource == null)
                throw new ArgumentNullException("DataSource is not specifed. Value cannot be null.");
            if (string.IsNullOrEmpty(connectionString.Database))
                throw new Exception("Database is not specified. Value cannot be null or empty string.");
            if (string.IsNullOrEmpty(connectionString.UserId) || string.IsNullOrEmpty(connectionString.Password))
                connectionString.IntegeratedSecurity = true;

            var token = new RouterToken(connectionString);

            lock (DatabaseInstances)
            {
                if (DatabaseInstances.ContainsKey(token))
                {
                    return DatabaseInstances[token].GetDatabase();
                }

                var dbInstance = new NoSdbInstance(connectionString);
                DatabaseInstances.Add(token, dbInstance);
                return dbInstance.GetDatabase();
            }
        }

        internal static void Dipose(ConnectionStringBuilder connectionString)
        {
            var token = new RouterToken(connectionString);
            if (DatabaseInstances.ContainsKey(token))
            {
                DatabaseInstances.Remove(token);
            }
        }
    }
}
