
using System.Management.Automation;


namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsCommunications.Disconnect, "DatabaseCluster")]
    public class DisconnectDatabseCluster : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (this.SessionState.Drive.Current is NosDBPSDriveInfo)
            {
                ConfigurationConnection.Current.Dispose();
                ConfigurationConnection.ConfigCluster = null;
                ConfigurationConnection.ClusterInfo = null;
                ConfigurationConnection.ClusterConfiguration = null;
                SessionState.Path.SetLocation(ProviderUtil.DRIVE_ROOT + ":\\");
            }

        }
    }
}