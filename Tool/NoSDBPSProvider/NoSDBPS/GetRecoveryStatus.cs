using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.NosDBPS;

namespace Alachisoft.NosDB.NoSDBPS
{
    [Cmdlet(VerbsCommon.Get, "TasksInfo")]
    public class GetRecoveryStatus : PSCmdlet
    {
        private bool showHistory;
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "shows all recovery job in history."
         )]
        public SwitchParameter ShowHistory
        {
            set { showHistory = value; }
            get { return showHistory; }
        }

        protected override void ProcessRecord()
        {
            if (this.SessionState.Drive.Current is NosDBPSDriveInfo)
            {
                PrintableTable tasksTable = new PrintableTable();
                ClusterJobInfoObject[] tasks = ConfigurationConnection.Current.GetAllRunningJobs();

                if (tasks!= null && tasks.Length > 0)
                {
                    tasksTable.AddHeader("Creation Time", "ID", "Database Name", "Job Type", "Status", "% Execution");
                    foreach (ClusterJobInfoObject task in tasks)
                    {
                        string type = task.ActiveConfig.JobType.ToString().ToLower().EndsWith("backup") ? "Backup" : "Restore";
                                    
                        string dbName = string.IsNullOrEmpty(task.ActiveConfig.DatabaseMap.First().Value) ? task.ActiveConfig.DatabaseMap.First().Key : task.ActiveConfig.DatabaseMap.First().Value;
                        if (!showHistory)
                        {
                            switch (task.ExecutionState.Status)
                            {
                                case RecoveryStatus.Executing:
                                case RecoveryStatus.Waiting:
                                case RecoveryStatus.uninitiated:
                                    tasksTable.AddRow( task.ActiveConfig.CreationTime.ToString(), task.Identifier,dbName,type,task.ExecutionState.Status.ToString(),task.ExecutionState.PercentageExecution.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            tasksTable.AddRow(task.ActiveConfig.CreationTime.ToString(), task.Identifier, dbName, type, task.ExecutionState.Status.ToString(), task.ExecutionState.PercentageExecution.ToString());
                                            
                        }
                    }

                    WriteObject("\n" + tasksTable.GetTableRows());
                }
            }

        }

    }
}
