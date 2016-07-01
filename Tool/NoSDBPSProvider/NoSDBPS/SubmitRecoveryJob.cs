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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Recovery;
using System.Threading;

namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    public class SubmitRecoveryJob : PSCmdlet
    {
        private RecoveryJobType _jobType;
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify Recovery job type from list of DataBackup,DataRestore, ConfigBackup, FullBackup, Restore, ConfigRestore "
         )]
        [ValidateSet("DataBackup", "DataRestore", "ConfigBackup",
        "FullBackup", "Restore", "ConfigRestore", IgnoreCase = true)]
        [Alias("RecoveryJobType")]
        public string Type
        {
            get;
            set;
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specifies whether recovery is differential."
        )]
       
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify recovery path."
        )]
        [Alias("RecoveryPath")]
        public string Path
        {
            set;
            get;
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = @"Specify exesting database name, not required for config backup\restore."
        )]
        [Alias("Source")]
        public string SourceDatabaseName
        {
            set;
            get;
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = @"Specify resulting database name, not required for config backup\restore."
        )]
        [Alias("Destination")]
        public string DestinationDatabaseName
        {
            set;
            get;
        }

        protected override void BeginProcessing()
        {
            SessionState s1 = SessionState;
            if (s1.Drive.Current is NosDBPSDriveInfo)
            {
                if (ConfigurationConnection.ConfigCluster == null)
                    throw new Exception("You must be connected to database cluster for submit-recovery job.");
                switch (Type.ToLower())
                {
                    case "configrestore":
                        _jobType = RecoveryJobType.ConfigRestore;
                        SourceDatabaseName = string.Empty;
                        break;
                    case "configbackup":
                        _jobType = RecoveryJobType.ConfigBackup;
                        SourceDatabaseName = string.Empty;
                        break;
                    case "databackup":
                        _jobType = RecoveryJobType.DataBackup;
                        VerifyDatabaseName();
                        break;
                    case "datarestore":
                        _jobType = RecoveryJobType.DataRestore;
                        VerifyDatabaseName();
                        break;
                  case "fullbackup":
                        _jobType = RecoveryJobType.FullBackup;
                        VerifyDatabaseName();
                        break;
                    case "restore":
                        _jobType = RecoveryJobType.Restore;
                        VerifyDatabaseName();
                        break;

                }
            }
            else throw new Exception("You must be in NosDB: PSDrive for Submit-RecoveryJob.");

        }

        protected override void ProcessRecord()
        {
            RecoveryOperationStatus status = StartRecoveryJob(SourceDatabaseName,DestinationDatabaseName, Type, Path);
            PrintInfo(status);
            PrintJobState(status.JobIdentifier);
        }

        private void PrintJobState(string jobStateIdentifier)
        {
            bool condtion = true;
            var pRec = new ProgressRecord(0, "ID: ", jobStateIdentifier);
            while (condtion)
            {
                ClusteredRecoveryJobState jobState = ConfigurationConnection.Current.GetJobState(jobStateIdentifier);
                WriteObject(jobState.PercentageExecution);
                pRec.PercentComplete = (int) jobState.PercentageExecution;
                WriteProgress(pRec);
                Thread.Sleep(50);
                condtion = jobState.Status == RecoveryStatus.Completed || jobState.Status == RecoveryStatus.Failure;
            }


        }

        private void VerifyDatabaseName()
        {
            if (string.IsNullOrEmpty(SourceDatabaseName))
                throw new Exception("Must Provide database name for " + Type);
        }

        public RecoveryOperationStatus StartRecoveryJob(string databaseName,string destinationDatabase, string backUpType, string path)
        {
            RecoveryConfiguration recoveryConfiguration = new RecoveryConfiguration();
            recoveryConfiguration.DatabaseMap = new Dictionary<string, string>();
            recoveryConfiguration.DatabaseMap.Add(databaseName,
                string.IsNullOrEmpty(destinationDatabase) ? string.Empty : destinationDatabase);

            recoveryConfiguration.Cluster = ConfigurationConnection.ClusterConfiguration.Name;
            

            recoveryConfiguration.JobType = (RecoveryJobType)Enum.Parse(typeof(RecoveryJobType), backUpType);
            recoveryConfiguration.RecoveryPath = path;
            return ConfigurationConnection.Current.SubmitRecoveryJob(recoveryConfiguration);

           

        }

        protected void PrintInfo(RecoveryOperationStatus opStatus)
        {
            //Console.WriteLine("____________________________________");
            WriteVerbose("ID: " + opStatus.JobIdentifier);
            WriteVerbose("Status: " + opStatus.Status);
            //Console.WriteLine("Message: " + opStatus.Message);
            //Console.WriteLine("____________________________________");
        }
    }

}
