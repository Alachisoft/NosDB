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

namespace Alachisoft.NosDB.NosDBPS.ShardsInfrastructure
{
    [Cmdlet(VerbsLifecycle.Stop, "Shard")]
    public class StopShard:PSCmdlet
    {
        
        private string _shard = string.Empty;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "name of shard to be stopped.")]
        [Alias("n")]
        public string Name
        {
            get { return _shard; }
            set { _shard = value; }
        }

        protected override void BeginProcessing()
        {
            string exceptionString = "invalid Context, to Stop-Shard you must be in \n NoSDB:\\databasecluster\\Shards \n ";
            NodeDetail thisNode = null;
            SessionState s1 = this.SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path, (PSDriveInfo)s1.Drive.Current);
            if (s1.Drive.Current is NosDBPSDriveInfo)
            {
                if (ConfigurationConnection.ConfigCluster == null)
                {
                    throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
                }
                 if(new NoSDbDetail(pathChunks, (PSDriveInfo)s1.Drive.Current).TryGetNodeDetail(out thisNode))
                 {

                 }
                
                if (thisNode.NodeType.Equals(PathType.Shards))
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        throw new Exception("Specify value for parameter Name or try to stop shard from:\nNoSDB:\\databasecluster\\Shards\\shard. ");
                    }
                }
                else if (thisNode.NodeType.Equals(PathType.Shard))
                {
                    Name = thisNode.NodeName;
                }
                else
                {
                    throw new System.Exception(exceptionString);
                }

            }

            else
            {
                throw new System.Exception(exceptionString);
            }

        }

        protected override void ProcessRecord()
        {
            try
            {

                if (ConfigurationConnection.Current.StopShard(Name))
                {
                    WriteObject("shard sucessfully stopped");
                }
            }
            catch
            {
                throw;
            }

        }

    }
}
