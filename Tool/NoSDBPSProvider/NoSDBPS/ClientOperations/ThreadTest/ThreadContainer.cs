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
using System.Threading;
using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.NosDBPS.Commandlets;
using Alachisoft.NosDB.NosDBPS.TestPOCO;

namespace Alachisoft.NosDB.NosDBPS
{
    internal class ThreadContainer
    {
        private static int errorCount;
        Collection<Order> _collection = null;
        private string _collectionName = string.Empty;
        int _totalLoopCount = 0;
        int _testCaseIterations = 10;
        int _testCaseIterationDelay = 0;
        private int _instertsPerIteration = 1;
        int _getsPerIteration = 1;
        int _updatesPerIteration = 1;
        int _deletesPerIteration = 1;
        int _reportingInterval = 500;
        private int _threadIndex;
        private Commandlets.StressTest _parent;
        private int threadIndex;
        private StressTest parent;
        private PowerShellAdapter _adapter;
        private ThreadTest _parentTest;
        private int maxErrors = 1000;
        private Random _rand = new Random();
        public static int SerialKey = 0;
        List<int> keys =  new List<int>(); 
        static ThreadContainer()
        {
            errorCount = 0;
        }

        public ThreadContainer(string collectionName, int totalLoopCount, int testCaseIterations, int testCaseIterationDelay, int getsPerIteration, int updatesPerIteration, int instertsPerIteration, int deletesPerIteration, int reportingInterval, int threadIndex, Commandlets.StressTest parent, PowerShellAdapter adapter, ThreadTest parentTest)
        {
            // TODO: Complete member initialization
            _collectionName = collectionName;
            _totalLoopCount = totalLoopCount;
            _testCaseIterations = testCaseIterations;
            _testCaseIterationDelay = testCaseIterationDelay;
            _getsPerIteration = getsPerIteration;
            _updatesPerIteration = updatesPerIteration;
            _instertsPerIteration = instertsPerIteration;
            _deletesPerIteration = deletesPerIteration;
            _reportingInterval = reportingInterval;
            _threadIndex = threadIndex;
            _parent = parent;
            _adapter = adapter;
            _parentTest = parentTest;
        }

        public void DoTest()
        {
            try
            {
                int collectionRetries = 0;
                _collection = _parent.Database.GetCollection<Order>(_collectionName);

                string query = string.Format("Select count(*) from {0}", _collectionName);
                while (collectionRetries < 100)
                {
                    try
                    {
                        _collection.ExecuteScalar(query);
                        break;
                    }
                    catch (DatabaseException databaseException)
                    {
                        if (databaseException.ErrorCode.Equals(ErrorCodes.Distributor.COLLECTION_DOESNOT_EXIST))
                        {
                            collectionRetries++;
                            Thread.Sleep(300);

                        }
                    }
                }
                PerformTestCase();
            }
            catch (Exception)
            {
            }
        }

        private void PerformTestCase()
        {
            if (_totalLoopCount <= 0)
            {
                for (long totalIndex = 0; ; totalIndex++)
                {
                    ProcessGetInsertIteration();
                    if (totalIndex >= _reportingInterval)
                    {
                        try
                        {
                            string query =
                                string.Format("Select count(*) from {0}", _collectionName);
                            var count = _collection.ExecuteScalar(query);
                            _adapter.WriteObject(DateTime.Now.ToString() + ": Docouments count: " + count);
                            totalIndex = 1;
                        }
                        catch (Exception e)
                        {
                            ++errorCount;
                        }
                    }
                }
            }
            else
            {
                for (long totalIndex = 0; totalIndex < _totalLoopCount; totalIndex++)
                {
                    ProcessGetInsertIteration();
                    if (totalIndex >= _reportingInterval)
                    {
                        try
                        {
                            string query =
                                string.Format("Select count(*) from {0}", _collectionName);
                            long count = (long)_collection.ExecuteScalar(query);
                            //_parent.WriteObject(DateTime.Now.ToString() + ": Docouments count: "+count );

                            _adapter.WriteObject(DateTime.Now.ToString() + ": Docouments count: " + count);
                            totalIndex = 1;
                        }
                        catch (Exception e)
                        {
                            ++errorCount;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Perform Get/Insert task on cache.
        /// Called by PerformTestCase method
        /// </summary>
        private void ProcessGetInsertIteration()
        {
            string guid = System.Guid.NewGuid().ToString(); //create a unique key to be inserted in store.



            for (long testCaseIndex = 0; testCaseIndex < _testCaseIterations; testCaseIndex++)
            {
                for (int insertIndex = 0; insertIndex < _instertsPerIteration && Order._serialKey < _parent.MaxDocuments; insertIndex++)
                {
                    InsertDocument();
                }

                for (int getsIndex = 0; getsIndex < _getsPerIteration; getsIndex++)
                {
                    GetDocument();

                }

                for (int updatesIndex = 0; updatesIndex < _updatesPerIteration; updatesIndex++)
                {
                    UpdateDocument();
                }


                
                for (int deleteIndex = 0; deleteIndex < _deletesPerIteration; deleteIndex++)
                {
                    DeleteDocument();
                }
                
                foreach (int key in keys)
                {
                    if(key != 0)
                    InsertDocument(key);
                }
                keys.Clear();


                if (_testCaseIterationDelay > 0)
                {
                    // Sleep for this many seconds
                    Thread.Sleep(_testCaseIterationDelay * 1000);
                }

            }
        }

        private void InsertDocument(int key)
        {
            Order order = null;
            try
            {
                order = new Order(key);
                if (order != null)
                {
                    _collection.InsertDocument(order);

                }

            }
            catch (Exception e)
            {
                if (!e.Message.Contains(" already exists in collection"))
                {
                    if (order != null)
                        _adapter.WriteObject("Insert failed for key: " + order._key + "\n" + e.ToString() + "\n");

                    ++errorCount;
                    if (errorCount >= maxErrors)
                    {
                        _adapter.WriteObject("Too many errors. aborting stress testing");
                        _parentTest.Stop();
                    }
                }
                //throw e;
            }
        }

        private void InsertDocument()
        {
            InsertDocument(++Order._serialKey);
        }

        private void GetDocument()
        {
            string key = string.Empty;
            try
            {
                //int key = rnd.Next(1, 20);
                //string query =
                //    string.Format("Select * from {0} where ShippingLocation = \'{1}\'",ProviderUtil.TEST_COLLECTION_NAME,
                //        DataLoader.cities[key]);

                //ICollectionReader reader = _collection.ExecuteReader(query);
                //while (reader.ReadNext())
                //{
                //    Order o1 = reader.GetObject<Order>();
                //}


                key = Order.GetRandomKey(Order._serialKey);

                Order o1 = _collection.GetDocument(key);
            }
            catch (Exception e)
            {
                _adapter.WriteObject("Get failed for key: " + key + "\n" + e.ToString() + "\n");
                ++errorCount;
                if (errorCount >= maxErrors)
                {
                    _adapter.WriteObject("Too many errors. aborting stress testing");
                    _parentTest.Stop();
                }
                //throw e;
            }
        }

        private void UpdateDocument()
        {
            Order order = null;
            try
            {

                order = Order.GetRandomOrder(Order._serialKey);
                if (order != null)
                {

                    order.ShippingDate = DateTime.Now;

                    _collection.ReplaceDocument(order);

                }
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Document does't exist in the database"))
                {
                    if (order != null)
                        _adapter.WriteObject("Update failed for key: " + order._key + "\n" + e.ToString() + "\n");
                    ++errorCount;
                    if (errorCount >= maxErrors)
                    {
                        _adapter.WriteObject("Too many errors. aborting stress testing");
                        _parentTest.Stop();
                    }
                }
                //throw e;
            }
        }

        private void DeleteDocument()
        {
            string key = null;
            try
            {
                key = Order.GetRandomKey(Order._serialKey);

                _collection.DeleteDocument(key);
                keys.Add( Convert.ToInt32(key));
            }
            catch (Exception e)
            {
                _adapter.WriteObject("Delete failed for key: " + key + "\n" + e.ToString() + "\n");
                ++errorCount;
                if (errorCount >= maxErrors)
                {
                    _adapter.WriteObject("Too many errors. aborting stress testing");
                    _parentTest.Stop();
                }
                //throw e;
            }
        }

        ~ThreadContainer()
        {
            errorCount = 0;
        }

    }
}
