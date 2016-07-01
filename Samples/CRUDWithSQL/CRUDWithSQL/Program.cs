// ===============================================================================
// Alachisoft (R) NosDB Sample Code.
// NosDB CRUD Operations sample using SQL
// ===============================================================================
// Copyright © Alachisoft.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// ===============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.EntityObjects;

namespace Alachisoft.NosDB.CRUDWithSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string conectionString = ConfigurationManager.AppSettings["ConnectionString"];

                //Initialize an instance of the database to begin performing operations:
                Database database = Client.NosDB.InitializeDatabase(conectionString);

                //Get an instance of the 'Products' collection from the database:
                Collection<Product> productsCollection = database.GetCollection<Product>("Products");

                
                //Insert Product 'Chai' into the 'Products' Collection using Insert Query.
                const string insertQuery = "INSERT INTO Products (ProductId, ProductName, UnitsInStock, UnitPrice) VALUES(@productId, @productName,@unitsInStock,@unitPrice)";
                IList<IParameter> insertParameters = new List<IParameter>();
                insertParameters.Add(new Parameter("productId", "Product1"));
                insertParameters.Add(new Parameter("productName", "Chai"));
                insertParameters.Add(new Parameter("unitsInStock", 39));
                insertParameters.Add(new Parameter("unitPrice", 100.0));

                //Executing Insert Query
                productsCollection.ExecuteNonQuery(insertQuery, insertParameters);

                //Get the product 'Chai' from the 'Products' Collection using the Select Query

                const string selectQuery = "Select ProductName, UnitsInStock, UnitPrice from Products WHERE ProductName = @productName";
                IList<IParameter> selectParameters = new List<IParameter>();
                selectParameters.Add(new Parameter("productName", "Chai"));

                //Executing Select Query
                ICollectionReader reader = productsCollection.ExecuteReader(selectQuery, selectParameters);
                while (reader.ReadNext())
                {
                    //Get String Attribute from Reader
                    string prodcutName = reader.GetString("ProductName");
                    
                    //Get short Attribute from Reader
                    short unitsInStock = reader.GetInt16("UnitsInStock");

                    //Get double Attribute from Reader
                    double unitprice = reader.GetInt16("UnitPrice");

                    //Get Complete Prodcut form Reader
                    Product product = reader.GetObject<Product>();
                }

                //Update 'Chai' in the 'Products' Collection using Update Query.
                const string updateQuery = "UPDATE Products SET(UnitsInStock = @unitsInStock, UnitPrice = @unitPrice) WHERE ProductName = @productName";
                IList<IParameter> updateParametes = new List<IParameter>();
                updateParametes.Add(new Parameter("unitsInStock", 45));
                updateParametes.Add(new Parameter("unitPrice", 125.0));
                updateParametes.Add(new Parameter("productName", "Chai"));

                //Executing Update Query
                productsCollection.ExecuteNonQuery(updateQuery, updateParametes);

                //Delete 'Chai' from the Products Collection using Delete Query.
                const string deleteQuery = "DELETE From Products WHERE ProductName = @productName";
                IList<IParameter> deleteParametes = new List<IParameter>();
                deleteParametes.Add(new Parameter("productName", "Chai"));

                //Executing Delete Query
                long rowsAffected = productsCollection.ExecuteNonQuery(deleteQuery, deleteParametes);


                //Release all the resources
                database.Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Environment.Exit(0);
            }
        }
    }
}
