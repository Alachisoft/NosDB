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
using System.Management.Automation;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.EXIM;
using Alachisoft.NosDB.Client;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Recovery;


namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsData.Export, "Data")]
    public class ExportData:PSCmdlet
    {
        readonly List<IParameter> _queryParam = new List<IParameter>();
        private EXIMDataType _dataType;
        private string _databaseName = string.Empty;
        private string _collectionName = string.Empty;
        
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify whether Export data type is JSON or CSV."
         )]
        [ValidateSet("JSON", "CSV", IgnoreCase = true)]
        
        public string Format
        {
            get;
            set;
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify custom file name to export data."
         )]
        [Alias("ImportDataType")]
        public string FileName
        {
            get;
            set;
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify Export path."
        )]
        [Alias("ImportPath")]
        public string Path
        {
            set;
            get;
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify Query to export result."
        )]
        public string Query
        {
            set;
            get;
        }

        protected override void BeginProcessing()
        {
            
            SessionState s1 = SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path, s1.Drive.Current);
            if (s1.Drive.Current is NosDBPSDriveInfo)
            {

                if (ConfigurationConnection.ConfigCluster == null)
                {
                    throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
                }
                NodeDetail thisNode;
                if (new NoSDbDetail(pathChunks, s1.Drive.Current).TryGetNodeDetail(out thisNode))
                {

                }
                if (thisNode.NodeType.Equals(PathType.Collection))
                {
                    _databaseName = ((CollectionValueDetail)thisNode).Database;
                    _collectionName = thisNode.NodeName;
                    switch (Format.ToLower())
                    {
                        case "json":
                            _dataType = EXIMDataType.JSON;
                            break;
                        case "csv":
                            _dataType = EXIMDataType.CSV;
                            break;

                    }
                }
                else throw new Exception("You must be in NosDB:\\databasecluster\\databases\\database\\collections\\collection> for Export-Data.");
            }
            else throw new Exception("You must be in NosDB:\\databasecluster\\databases\\database\\collections\\collection> for Export-Data.");

        }

        protected override void ProcessRecord()
        {
            Database db = Client.NosDB.InitializeDatabase(ProviderUtil.GetConnectionString(_databaseName));
            Collection<JSONDocument> collection = db.GetCollection(_collectionName);
            RecoveryOperationStatus status = collection.Export(_databaseName, _collectionName, Query, _queryParam, Path,FileName, _dataType);
            if (status.Status == RecoveryStatus.Success)
            {
                WriteObject("Exported successfully");
            }
            else
            {
                WriteObject("failed to export data " + status.Message);
            }
            db.Dispose();
        }
    }
}
