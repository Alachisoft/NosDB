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
using System.Diagnostics;
using System.Management.Automation;
using Alachisoft.NosDB.Common.Annotations;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Client;

using Alachisoft.NosDB.NosDBPS.TestPOCO;

using System.Threading;
using System.Collections.Generic;

namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Stress")]
    public class StressTest : PSCmdlet
    {
        private string _databaseName = string.Empty;
        private string _connectionString = string.Empty;
        private Database _database;
        private int _threadCount = 1;
        int _totalLoopCount;
        int _testCaseIterations = 20;
        int _testCaseIterationDelay= 0;
        private int _insertsPerIteration = 10;
        int _getsPerIteration = 25;
        int _updatesPerIteration = 5;
        int _deletesPerIteration = 1;
        int _reportingInterval = 500;
        bool _drop = false;
        private int _maxDocouments = 10000;
        private ThreadTest threadTest;
        private string _shard = string.Empty;
        private string _collectionName;

        [Parameter(
            Mandatory = false,

            HelpMessage = "Specifies Drop mode..")]
        [Alias("d")]
        public SwitchParameter DropCollection
        {
            get { return _drop; }
            set { _drop = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Specifies name of shard to create collection.")]
        public string Shard
        {
            get { return _shard; }
            set { _shard = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Specifies number of client threads for Stress testing.")]
        [ValidateRange(1, 5)]
        public int ThreadCount
        {
            get { return _threadCount; }
            set { _threadCount = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "How many iterations within a test case (default: 20)")]
        public int TestCaseIterations
        {
            get { return _testCaseIterations; }
            set { _testCaseIterations = value; }
        }


        [Parameter(
            Mandatory = false,
            HelpMessage = "Report after this many total iterations")]
        public int ReportingInterval
        {
            get { return _reportingInterval; }
            set { _reportingInterval = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "How much delay (in seconds) between each test case iteration (default: 0)")]
        public int TestCaseIterationDelay
        {
            get { return _testCaseIterationDelay; }
            set { _testCaseIterationDelay = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "How many gets within one iteration of a test case (default: 1)")]
        public int GetsPerIteration
        {
            get { return _getsPerIteration; }
            set { _getsPerIteration = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "How many gets within one iteration of a test case.")]
        public int UpdatesPerIteration
        {
            get { return _updatesPerIteration; }
            set { _updatesPerIteration = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "How many gets within one iteration of a test case.")]
        public int DeletesPerIteration
        {
            get { return _deletesPerIteration; }
            set { _deletesPerIteration = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "How many iterations of a test case.")]
        public int TotalIteration
        {
            get { return _totalLoopCount; }
            set { _totalLoopCount = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Specifies Maximum number of documents to insert in collection for test.")]
        public int MaxDocuments
        {
            get { return _maxDocouments; }
            set { _maxDocouments = value; }
        }

        public Database Database
        {
            get { return _database; }
        }

        protected override void BeginProcessing()
        {
            ConfigurationConnection.UpdateClusterConfiguration();
            List<string> shards =new List<string>(ConfigurationConnection.ClusterConfiguration.Deployment.Shards.Keys);
            if (shards == null || shards.Count == 0)
            {
                throw new Exception("No shard exist");
            }
            if (string.IsNullOrEmpty( Shard))
            {
                
                    Shard = shards[0];
            }
            else if (!shards.Contains(Shard))
            {
                throw new Exception("Specified shard doesn't exist");

            }
            _collectionName = GetCollectionName(Shard);

            string[] paramArray = null;


            NodeDetail thisNode = null;
            SessionState s1 = this.SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path, (PSDriveInfo)s1.Drive.Current);

            if (s1.Drive.Current is NosDBPSDriveInfo)
            {

                if (ConfigurationConnection.ConfigCluster == null)
                {
                    throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
                }
                //NodeDetail thisNode;
                if (new NoSDbDetail(pathChunks, s1.Drive.Current).TryGetNodeDetail(out thisNode))
                {
                    if (thisNode.NodeType.Equals(PathType.Database))
                    {
                        if (_drop)
                        {
                            _databaseName = thisNode.NodeName;
                            _connectionString = ProviderUtil.GetConnectionString(_databaseName);
                            _database = Alachisoft.NosDB.Client.NosDB.InitializeDatabase(_connectionString);
                            ConfigurationConnection.UpdateClusterConfiguration();
                            if (((DatabaseConfiguration)thisNode.Configuration).Storage.Collections.ContainsCollection(
                                    _collectionName))
                            {
                                String query = "DROP COLLECTION $" + _collectionName + "$ {\"Database\":\"" +
                                               thisNode.NodeName + "\"}";
                                _database.ExecuteNonQuery(query);

                                Thread.Sleep(5000);
                                ConfigurationConnection.UpdateClusterConfiguration();



                            }
                        }
                        else
                        {

                            _databaseName = thisNode.NodeName;
                            _connectionString = ProviderUtil.GetConnectionString(_databaseName);
                            _database = Alachisoft.NosDB.Client.NosDB.InitializeDatabase(_connectionString);
                            ConfigurationConnection.UpdateClusterConfiguration();
                            pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path,
                                        (PSDriveInfo)s1.Drive.Current);
                            new NoSDbDetail(pathChunks, s1.Drive.Current).TryGetNodeDetail(out thisNode);

                            if (!((DatabaseConfiguration)thisNode.Configuration).Storage.Collections.ContainsCollection(
                                    _collectionName))
                            {
                                String query = "CREATE COLLECTION $" + _collectionName + "$ {\"Database\":\"" +
                                               thisNode.NodeName + "\", \"Shard\": \"" +Shard+ "\"}";

                                try
                                {
                                    _database.ExecuteNonQuery(query);
                                }
                                catch (Exception e)
                                {
                                    if (!e.Message.Contains("Collection with name: "+_collectionName +" already exists"))
                                        throw e;
                                }
                                do
                                {
                                    Thread.Sleep(2000);
                                    ConfigurationConnection.UpdateClusterConfiguration();
                                    pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path,
                                        (PSDriveInfo)s1.Drive.Current);
                                    new NoSDbDetail(pathChunks, s1.Drive.Current).TryGetNodeDetail(out thisNode);

                                } while (
                                    !((DatabaseConfiguration)thisNode.Configuration).Storage.Collections
                                        .ContainsCollection(
                                            _collectionName));
                            }
                        }


                    }
                    else throw new Exception("You must be in NosDB:\\databasecluster\\databases\\database> for Test-Stress.");


                }
            }
            else throw new Exception("You must be in NosDB:\\databasecluster\\databases\\database> for Test-Stress.");

        }

        private string GetCollectionName(string shard)
        {
            return shard + "_" + ProviderUtil.TEST_COLLECTION_NAME;
        }

        

        protected override void ProcessRecord()
        {
            if (!_drop)
            {

                //string query = "DELETE FROM $" + ProviderUtil.TEST_COLLECTION_NAME + "$";
                //long rowsAffected = _database.ExecuteNonQuery(query);
                Collection<Order> order = _database.GetCollection<Order>(_collectionName);
                WriteObject("\n");
                //order.InsertDocuments(DataLoader.LoadOrders(_initailLoadedValues));


                WriteObject(string.Format("database = {0}, total-loop-count = {1}, test-case-iterations = {2}, testCaseIterationDelay = {3}, gets-per-iteration = {4}, updates-per-iteration = {5}, deletes-per-iteration = {6}, thread-count = {7}, reporting-interval = {8}, max-documents = {9} ."
                    , _databaseName, TotalIteration, TestCaseIterations, TestCaseIterationDelay, GetsPerIteration, UpdatesPerIteration, DeletesPerIteration, ThreadCount, ReportingInterval,MaxDocuments));
                WriteObject("-------------------------------------------------------------------\n");
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;

                threadTest = new ThreadTest(_connectionString, _collectionName, _threadCount, _totalLoopCount, _testCaseIterations, _testCaseIterationDelay, _getsPerIteration, _updatesPerIteration, _insertsPerIteration, _deletesPerIteration, _reportingInterval, _maxDocouments, this);
                threadTest.Test();
            }


        }

        protected override void StopProcessing()
        {
            threadTest.Stop();
            
            
        }

        protected override void EndProcessing()
        {
            try
            {
                _database.Dispose();
            }
            catch (Exception) { }
        }
    }
}
