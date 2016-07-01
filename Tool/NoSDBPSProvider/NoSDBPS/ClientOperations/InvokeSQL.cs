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
using System.IO;
using System.Management.Automation;
using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Queries.ParseTree;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Storage.Caching.QueryCache;

namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsLifecycle.Invoke, "SQL")]
    public class InvokeSQL : PSCmdlet
    {
        private string _query = string.Empty;
        private bool _nonQuery;
        private string conString;
        private string _inputFile = string.Empty;
        private string[] queryArr;
        private Database database;
        private bool _databaseContext = false;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify the inputfile name:")]
        [Alias("i")]
        public string InputFile
        {
            get { return _inputFile; }
            set { _inputFile = value; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify Query:")]
        [Alias("q")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        protected override void BeginProcessing()
        {
            if (Query.Equals(string.Empty) && InputFile.Equals(string.Empty))
                throw new Exception("Invoke the command with argument Query|InputFile");
            if (Query != string.Empty && InputFile != string.Empty)
                throw new Exception("The Query and the InputFile options are mutually exclusive");
            string dbName = string.Empty;
            NodeDetail thisNode = null;
            SessionState s1 = this.SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path, (PSDriveInfo)s1.Drive.Current);
            if (s1.Drive.Current is NosDBPSDriveInfo)
            {
                if (ConfigurationConnection.ConfigCluster == null)
                {
                    throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
                }
                new NoSDbDetail(pathChunks, (PSDriveInfo)s1.Drive.Current).TryGetNodeDetail(out thisNode);

                if (thisNode is IDatabaseContext)
                {
                    dbName = ((IDatabaseContext)thisNode).DatabaseName;
                    _databaseContext = true;
                }
                else
                {
                    dbName = "$sysdb";
                }
                //conString = "nosdb://" + ConfigurationConnection.Current.ConfigServerIP + ":" +
                //            ConfigurationConnection.Current.Port + "/" +
                //            ConfigurationConnection.ClusterConfiguration.Name + "/" + dbName;
                bool localInstance;
                if (ConfigurationConnection.ClusterConfiguration.Name.Equals("local",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    localInstance = true;
                }
                else
                {
                    localInstance = false;
                }
                conString = ProviderUtil.GetConnectionString(dbName);
                //conString = "Data Source=" + ConfigurationConnection.Current.ConfigServerIP + ";" + "Port=" +
                //            ConfigurationConnection.Current.Port + ";" + "Database=" + dbName + ";" + "Local Instance="+localInstance+";";
            }

            if (InputFile != string.Empty)
            {
                if (!File.Exists(InputFile))
                    throw new Exception("Input file does't exist");
                string text = File.ReadAllText(InputFile);
                text = text.Replace("\r\n", "");
                queryArr = text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        protected override void ProcessRecord()
        {
            try
            {




                database = Alachisoft.NosDB.Client.NosDB.InitializeDatabase(conString);
                if (Query != string.Empty)
                {
                    ExecuteQuery(Query);

                }
                else
                {
                    if (queryArr != null)
                    {
                        foreach (string queryText in queryArr)
                        {
                            ExecuteQuery(queryText);
                        }
                    }
                }
                database.Dispose();


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void ExecuteQuery(string query)
        {
            QueryCache<IDqlObject> _reducedQueryCache = new QueryCache<IDqlObject>();
            IDqlObject parsedObject = _reducedQueryCache.GetParsedQuery(query);

            if (parsedObject is SelectObject)
            {
                if (_databaseContext)
                    Execute(query, QueryType.Reader);
                else
                    throw new Exception("For Execution of Data Query you must be in context of database.");
            }
            else if (parsedObject is InsertObject || parsedObject is DeleteObject ||
                parsedObject is UpdateObject)
            {
                if (_databaseContext)
                    Execute(query, QueryType.NonQuery);
                else
                    throw new Exception("For Execution of Data Query you must be in context of database.");
            }
            else
                Execute(query, QueryType.NonDataQuery);

        }

        private void Execute(string query, QueryType type)
        {


            switch (type)
            {
                case QueryType.NonQuery:
                    long x = database.ExecuteNonQuery(query);
                    WriteObject(ProviderUtil.HEADER + "\n" + ProviderUtil.HEADER + "\n" + query + "\n" + ProviderUtil.HEADER + "\n" + ProviderUtil.HEADER);
                    WriteObject(x + " rows updated");
                    break;
                case QueryType.Reader:
                    ICollectionReader r1 = database.ExecuteReader(query);
                    WriteObject(ProviderUtil.HEADER + "\n" + ProviderUtil.HEADER + "\n" + query + "\n" + ProviderUtil.HEADER + "\n" + ProviderUtil.HEADER);

                    while (r1.ReadNext())
                    {
                        WriteObject(r1.GetDocument().ToString());
                    }
                    r1.Dispose();
                    break;
                case QueryType.NonDataQuery:
                    long y = database.ExecuteNonQuery(query);
                    break;
            }
        }

        
        
    }

}
