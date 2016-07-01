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
using System.Management.Automation;
using Alachisoft.NosDB.Core.Configuration;

namespace Alachisoft.NosDB.NosDBPS.Migration
{
    [Cmdlet(VerbsCommon.Move, "Collection")]
    public class MoveCollection : PSCmdlet
    {
        private string _newShard = string.Empty;
        private string _databaseName = string.Empty;
        private string _collectionName = string.Empty;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "name of a shard where you want to move collection.")]
        
        [Alias("n")]
        public string NewShard
        {
            set { _newShard = value; }
            get { return _newShard; }
        }

        protected override void BeginProcessing()
        {
            string exceptionString =
                "Invalid context, to Move-Collection you must be in \n NoSDB:\\databasecluster\\Databases\\[DatabaseName]\\[CollectionName] \n ";
            NodeDetail thisNode;
            SessionState sessionState = SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(sessionState.Path.CurrentLocation.Path, sessionState.Drive.Current);

            if (!(sessionState.Drive.Current is NosDBPSDriveInfo)) throw new Exception(exceptionString);

            if (!new NoSDbDetail(pathChunks, sessionState.Drive.Current).TryGetNodeDetail(out thisNode))
                throw new Exception("Unable to Get Node Details :" + sessionState.Drive.Current);

            if (!thisNode.NodeType.Equals(PathType.Collection))
                throw new Exception(exceptionString);

            _collectionName = thisNode.NodeName;
            _databaseName = ((CollectionValueDetail) thisNode).Database;
        }

        protected override void ProcessRecord()
        {
            RemoteConfigurationManager rcm = ConfigurationConnection.Current;
            rcm.MoveCollection(true, _databaseName, _collectionName, NewShard);
            WriteObject("Data Transfer has started. You can monitor the progress through Perfmon Counters.");
        }
    }
}
