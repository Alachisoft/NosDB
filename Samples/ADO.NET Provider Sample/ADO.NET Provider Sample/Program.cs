using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.EntityObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.ADO.NET
{
    /// <summary>
    /// A sample that demonstrates the usage of ADO.NET for querying on NosDB. This is done through the
    /// ADO.NET provider contained in the installation.
    /// 
    /// Requirements:
    ///     1. A NosDB database and collection
    ///     2. A connection string in app.config
    ///     3. A reference to ADO.NET provider and its configuration in app.config (see programmers guide)
    /// 
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {

                // Reads the Connection string
                ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["NosDbConnection"];

                // Loads the factory
                DbProviderFactory factory = DbProviderFactories.GetFactory(connectionSettings.ProviderName);

                #region Querying NosDB with ADO.NET

                Console.WriteLine("Query data from northwind database...");
                Console.WriteLine();
                // It is recommeded to use the using block
                using (DbConnection connection = factory.CreateConnection())
                {
                    // set the connection string and open the connection.
                    connection.ConnectionString = connectionSettings.ConnectionString;
                    connection.Open();

                    // create command and set the connection property to the above opened connection.
                    DbCommand command = factory.CreateCommand();
                    command.CommandText = "SELECT Name, UnitPrice FROM Products";
                    command.Connection = connection;

                    // Execute and Enumerate the query result.
                    IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine("Name: " + (string)reader["Name"] + " UnitPrice: " + reader["UnitPrice"]);
                    }

                    // # Similiarly, you can use ExecuteScalar()...
                    // # DbDataAdapter can also be used to fill the data tables in the application.
                }

                #endregion

                #region Querying a Non-Query with ADO.NET

                Console.WriteLine("Inserting new data in northwind database...");
                Console.WriteLine();
                // It is recommeded to use the using block
                using (DbConnection connection = factory.CreateConnection())
                {
                    // set the connection string and open the connection.
                    connection.ConnectionString = connectionSettings.ConnectionString;
                    connection.Open();

                    // create command and set the connection property to the above opened connection.
                    DbCommand command = factory.CreateCommand();
                    command.CommandText = "INSERT INTO Products(ID, Name, QuantityPerUnit, UnitPrice, " +
                                          "UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued) " +
                                          "VALUES(10011, 'Mashed Potatos', '56', 45, 13, 3, 8, true)";
                    command.Connection = connection;

                    // Insert records into database..
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine("Rows Inserted: " + rowsAffected);
                }

                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }
        }
    }
}
