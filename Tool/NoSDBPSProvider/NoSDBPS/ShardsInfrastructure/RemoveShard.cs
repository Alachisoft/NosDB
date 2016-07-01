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
using Alachisoft.NosDB.Common.Util;


namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsCommon.Remove, "Shard", ConfirmImpact = ConfirmImpact.High)]
    public class RemoveShard:PSCmdlet
    {
        private string _shard=string.Empty;
        private string _server=string.Empty;
        private int _configPort=NetworkUtil.DEFAULT_CS_HOST_PORT;
        private bool _forced = false;
        private bool _quiet;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specifies whether Remove-Shard is in quiet mode."
        )]
        [Alias("Q")]
        public SwitchParameter Quiet  { get { return _quiet; } set { _quiet = value; } }
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "name of shard to be removed from databasecluster.")]
        [Alias("n")]
        public string Name
        {
            get { return _shard; }
            set { _shard = value; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify whether removel is forcefull")]
        [Alias("f")]
        public SwitchParameter Forced
        {
            get { return _forced; }
            set { _forced = value; }
        }

        
        protected override void BeginProcessing()
        {
            NodeDetail thisNode = null;
            SessionState s1 = this.SessionState;
            string[] pathChunks = ProviderUtil.SplitPath(s1.Path.CurrentLocation.Path, (PSDriveInfo)s1.Drive.Current);
            if (s1.Drive.Current is NosDBPSDriveInfo)
            {
                if (ConfigurationConnection.ConfigCluster == null)
                {
                    throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
                }
                if (new NoSDbDetail(pathChunks, (PSDriveInfo)s1.Drive.Current).TryGetNodeDetail(out thisNode))
                {

                } 
                
                if (thisNode.NodeType.Equals(PathType.Shards))
                {
                    
                }
                else
                {
                    throw new System.Exception("invalid Context, to Remove-Shard you must be in \n NoSDB:\\databasecluster\\Shards \n ");
                }

            }

            else
            {
                throw new System.Exception("invalid Context, to Remove-Shard you must be in \n NoSDB:\\databasecluster\\Shards \n ");
            }

        }

        protected override void ProcessRecord()
        {
            try
            {

                if (!_quiet)
                {
                    if (ShouldContinue("Do you want to remove shard.\nNOTE: Suspend is not supported. It is same as \'NO\'", "Please confirm"))
                    {
                        RemoveShardInternal();
                    }
                }
                else
                {
                    RemoveShardInternal();
                }
            }
            catch
            {
                throw;
            }
        }
        internal void RemoveShardInternal()
        {

            bool gracefull = false;// !Forced;
            if (ConfigurationConnection.Current.RemoveShardFromCluster(Name, null, gracefull))
            {

                NosDBPSDriveInfo drive = (NosDBPSDriveInfo)this.SessionState.Drive.Current;
            }
        }
    }
}
