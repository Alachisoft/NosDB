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
using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.NosDBPS.TestPOCO;

namespace Alachisoft.NosDB.NosDBPS
{
    internal sealed class ThreadTest
    {
        int _totalLoopCount = 0;
        int _testCaseIterations = 10;
        int _testCaseIterationDelay = 0;
        int _getsPerIteration = 1;
        int _updatesPerIteration = 1;
        int _dataSize = 1024;
        int _expiration = 300;
        int _threadCount = 1;
        int _reportingInterval = 5000;
        private string _connectionString = string.Empty;
        private string _collectionName = string.Empty;
        private Collection<Order> _testCollection;
        private string p;
        private int _instertsPerIteration;
        private int _deletesPerIteration;
        private Commandlets.StressTest parent;
        private Commandlets.StressTest stressTest;
        private PowerShellAdapter adapter;
        private Thread[] threads;
        private int _maxDocuments;
        
        

        public int MaxDocuments
        {
            get { return _maxDocuments; }
        }
        /// <summary>
        /// Overriden constructor that uses all user supplied parameters
        /// </summary>
        public ThreadTest(string connectionString,string collectionName, int threadCount)
        {
            _connectionString = connectionString;
            _threadCount = threadCount;
            _collectionName = collectionName;
        }

        public ThreadTest(string connectionString, string collectionName, int threadCount, int totalLoopCount, int testCaseIterations, int testCaseIterationDelay, int getsPerIteration, int updatesPerIteration, int instertsPerIteration, int deletesPerIteration, int reportingInterval,int maxDocs, Commandlets.StressTest stressTest)
        {
            adapter = new PowerShellAdapter(stressTest);
            _connectionString = connectionString;
            _collectionName = collectionName;
            _threadCount = threadCount;
            _totalLoopCount = totalLoopCount;
            _testCaseIterations = testCaseIterations;
            _testCaseIterationDelay = testCaseIterationDelay;
            _getsPerIteration = getsPerIteration;
            _updatesPerIteration = updatesPerIteration;
            _instertsPerIteration = instertsPerIteration;
            _deletesPerIteration = deletesPerIteration;
            _reportingInterval = reportingInterval;
            _maxDocuments = maxDocs;
            parent = stressTest;
        }

    
        /// <summary>
        /// Main test starting point. This method instantiate multiple threads and keeps track of 
        /// all of them.
        /// </summary>
        public void Test()
        {

            try
            {
                 threads = new Thread[_threadCount];

                
                string pid = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

                for (int threadIndex = 0; threadIndex < _threadCount; threadIndex++)
                {
                    int docCount = _maxDocuments / _threadCount;

                    if(threadIndex == 0 )
                        docCount += _maxDocuments % _threadCount;
                    
                    try
                    {
                        string query = string.Format("Select count(*) from {0}", _collectionName);

                        Double x = (Double)parent.Database.GetCollection(_collectionName).ExecuteScalar(query);

                        Order._serialKey = (int) x;
                    }
                    catch (Exception e)
                    {
                        adapter.WriteObject(e.ToString());
                    }

                    ThreadContainer tc = new ThreadContainer( _collectionName, _totalLoopCount, _testCaseIterations, _testCaseIterationDelay, _getsPerIteration, _updatesPerIteration, _instertsPerIteration, _deletesPerIteration,_reportingInterval,threadIndex,parent,adapter,this);
                    ThreadStart threadDelegate = new ThreadStart(tc.DoTest);
                    
                    threads[threadIndex] = new Thread(threadDelegate);

                    threads[threadIndex].Name = "ThreadIndex: " + threadIndex;
                    threads[threadIndex].Start();
                }

                adapter.Listen();

                //--- wait on threads to complete there work before finishing
                
                //for (int threadIndex = 0; threadIndex < threads.Length; threadIndex++)
                //{
                //    threads[threadIndex].Join();
                //}
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e.Message);
                Console.Error.WriteLine();
                Console.Error.WriteLine(e.ToString());
            }
        }

        public void Stop()
        {
            adapter.Finished = true;
            foreach (Thread thread in threads)
            {
                thread.Abort();
            }
            
        }
    }

}
