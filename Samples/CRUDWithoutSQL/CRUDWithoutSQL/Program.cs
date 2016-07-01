// ===============================================================================
// Alachisoft (R) NosDB Sample Code.
// NosDB basic CRUD operations sample without the use of SQL query statements.
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
using Alachisoft.NosDB.EntityObjects;

namespace Alachisoft.NosDB.CRUDWithoutSQL
{
    /// <summary>
    /// A sample program that demonstrates how to use the NosDB CRUD api. 
    /// 
    /// Requirements:
    ///     1. A NosDB databse and collection
    ///     2. A connection string in app.config
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                #region Initialize databases/collections
                string connectionString = ConfigurationSettings.AppSettings["ConnectionString"];

                if (String.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("The connection string cannot be null or empty.");
                    return;
                }
                //Initialize an instance of the database to begin performing operations:
                Database database = Client.NosDB.InitializeDatabase(connectionString);

                //Get an instance of the 'Products' collection from the database:
                Collection<Product> productsCollection = database.GetCollection<Product>("Products");
              
                #endregion

                #region Insert Multiple documents

                ICollection<Product> products = new List<Product>();
                products.Add(new Product { ProductId = "1", ProductName = "Blue Marble Jack Cheese", UnitPrice = 400});
                products.Add(new Product { ProductId = "2", ProductName = "Peanut Biscuits", UnitPrice = 500});
                products.Add(new Product { ProductId = "3", ProductName = "Walmart Delicious Cream", UnitPrice = 300});
                products.Add(new Product { ProductId = "4", ProductName = "Nestle Yogurt", UnitPrice = 400});
                products.Add(new Product { ProductId = "5", ProductName = "American Butter", UnitPrice = 600});
                
                ICollection<string> productKeys = new List<string>();
                productKeys.Add("1");
                productKeys.Add("2");
                productKeys.Add("3");
                productKeys.Add("4");
                productKeys.Add("5");

                // Insert documents into the Products collection:
                // This API returns the documents which could not be inserted.
                List<FailedDocument> failedDocsOnInsert = productsCollection.InsertDocuments(products);

                if (failedDocsOnInsert == null || failedDocsOnInsert.Count == 0)
                    Console.WriteLine("All items inserted into the '{0}' collection successfully. \n", productsCollection.Name);
                else
                {
                    Console.WriteLine("One or more items could not be inserted to the '{0}' collection.\n", productsCollection.Name);

                    foreach (var failedDocument in failedDocsOnInsert)
                    {
                        PrintFailedDocumentDetails(failedDocument);
                    }
                }
                #endregion

                #region Get Multiple documents

                // Get documents from the Products collection:
                // It will return a reader populated with the retrieved items. You may iterate the reader as demonstrated below.
                ICollectionReader reader = productsCollection.GetDocuments(productKeys);
                Console.WriteLine("\n\nThe following items were retrieved from the Products collection:\n");
                while (reader.ReadNext())
                {
                    Product product = reader.GetObject<Product>();
                    PrintProductDetails(product);
                    Console.WriteLine("\n");
                }
                reader.Dispose();
                #endregion

                #region Replace Multiple documents

               // Update the UnitsInStock for the documents which need to be replaced.
                IEnumerator<Product> ie = products.GetEnumerator();
                while (ie.MoveNext())
                {
                    ie.Current.UnitsInStock += 100;
                }
                // Replace documents in the Products collection:
                // This API returns the documents which could not be replaced.
                List<FailedDocument> failedDocsOnReplace = productsCollection.ReplaceDocuments(products);

                if (failedDocsOnReplace == null || failedDocsOnReplace.Count == 0)
                    Console.WriteLine("All items replaced into the '{0}' collection successfully. \n", productsCollection.Name);
                else
                {
                    Console.WriteLine("One or more items could not be replaced to the '{0}' collection.\n", productsCollection.Name);

                    foreach (var failedDocument in failedDocsOnReplace)
                    {
                        PrintFailedDocumentDetails(failedDocument);
                    }
                }
                #endregion

                #region Delete Multiple documents

                // Delete documents from the Products collection:
                // This API returns the documents which could not be deleted.
                List<FailedDocument> failedDocsOnDelete = productsCollection.DeleteDocuments(productKeys);

                if (failedDocsOnDelete == null || failedDocsOnDelete.Count == 0)
                    Console.WriteLine("All items deleted from the '{0}' collection successfully.\n ", productsCollection.Name);
                else
                {
                    Console.WriteLine("One or more items could not be deleted from the '{0}' collection.\n", productsCollection.Name);

                    foreach (var failedDocument in failedDocsOnDelete)
                    {
                        PrintFailedDocumentDetails(failedDocument);
                    }
                }
                #endregion

                //Releasing the resources
                database.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Environment.Exit(0);

        }

        /// <summary>
        /// A method for printing the details of the product type.
        /// </summary>
        /// <param name="product"></param>
        private static void PrintProductDetails(Product product)
        {
            if (product == null)
                return;
            Console.WriteLine("ProductId : " + product.ProductId);
            Console.WriteLine("ProductName : " + product.ProductName);
            Console.WriteLine("UnitsInStock : " + product.UnitsInStock);
        }

        /// <summary>
        /// A method for printing all the failed documents and the reason for their failure.
        /// </summary>
        /// <param name="failedDocument"></param>
        private static void PrintFailedDocumentDetails(FailedDocument failedDocument)
        {
            if (failedDocument == null)
                return;

            Console.WriteLine("Document Key : " + failedDocument.DocumentKey);
            Console.WriteLine("Error Code : " + failedDocument.ErrorCode);
            Console.WriteLine("Error Message : " + failedDocument.ErrorMessage);
        }
    }
}
