
using System.Management.Automation;
using Alachisoft.NosDB.Common;

namespace Alachisoft.NosDB.NosDBPS.Commandlets
{

    [Cmdlet(VerbsCommon.Remove, "DatabaseCluster", ConfirmImpact = ConfirmImpact.High)]
    public class RemoveConfigurationManager:PSCmdlet
    {
        private bool _quiet;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specifies whether Remove-DatabaseCluster is in quiet mode."
        )]
        [Alias("Q")]
        public SwitchParameter Quiet { get { return _quiet; } set { _quiet = value; } }

        protected override void BeginProcessing()
        {

            string exceptionString = "Invalid context, to Remove-DatabaseCluster you must be in \n NoSDB:\\";

            SessionState s1 = this.SessionState;
            if (s1.Drive.Current is NosDBPSDriveInfo)
            {
                string currentLocation = s1.Path.CurrentLocation.Path;

                if (currentLocation.EndsWith(":\\"))
                {
                    if (ConfigurationConnection.ConfigCluster == null)
                        throw new System.Exception("In order to remove Database cluster you must be connected to ConfigManager");
                    else if (!ConfigurationConnection.ConfigCluster.Name.Equals(MiscUtil.CLUSTERED))
                        throw new System.Exception("In order to remove Database cluster you must be connected to ConfigManager");
                    return;
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
            this.CommandRuntime.WriteObject(null);
            if (!_quiet)
            {
                if (ShouldContinue("Database cluster will be removed.\nNOTE: Suspend is not supported. It is same as \'NO\'", "Please confirm"))
                {
                    RemoveCongigManager();
                }
            }
            else
            {
                RemoveCongigManager();
            }
        }
        internal void RemoveCongigManager()
        {

            ConfigurationConnection.Current.RemoveConfigurationCluster();
            ConfigurationConnection.ConfigCluster = null;
            ConfigurationConnection.ClusterInfo = null;
            ConfigurationConnection.ClusterConfiguration = null;
        }
    }

}
