using System;
using System.IO;
using System.Management.Automation;
using Alachisoft.NosDB.Common;
using Newtonsoft.Json;


namespace Alachisoft.NosDB.NosDBPS.Commandlets
{
    [Cmdlet(VerbsData.Export, "Configuration")]
    public class DumpClusterConfiguration : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify Export path."
        )]
        [Alias("e")]
        public string Path
        {
            set;
            get;
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

                if (!Path.EndsWith("\\"))
                    Path = Path + "\\";
                //Path = Path + ProviderUtil.CONFIGURATION_DUMP_FILE_NAME;
                if (ConfigurationConnection.ConfigCluster.Name.Equals(MiscUtil.CLUSTERED))
                    ConfigurationConnection.UpdateClusterConfiguration();
                object[] configs = new object[2];
                configs[0] = ConfigurationConnection.ConfigCluster;
                configs[1] = ConfigurationConnection.ClusterConfiguration;


                string configurationStr = JsonConvert.SerializeObject(configs,
                    Newtonsoft.Json.Formatting.Indented);

                WriteConfigurationToFile(configurationStr, ConfigurationConnection.ClusterConfiguration.DisplayName);
            }
            else
                throw new Exception(ProviderUtil.CONFIG_NOT_CONNECTED_EXCEPTION);
        }


        private void WriteConfigurationToFile(string configuration, string name)
        {
            if (Path.Length == 0)
            {
                Console.Error.WriteLine("Can not locate path for writing configuration.");
            }
            // string valu=System.IO.Path.GetFullPath(Path);

            bool exist = Directory.Exists(Path);
            if (!exist)
            {
                // Console.Error.WriteLine("Specified path does't exist. ");
                throw new Exception("Specified path does't exist. ");

            }
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(Path + "\\" + name + "_" + DateTime.Now.ToString("yyyy-mm-dd_hh-mm-ss")+ProviderUtil.CONFIGURATION_DUMP_FILE_EXTENSION , FileMode.Create);
                sw = new StreamWriter(fs);

                sw.Write(configuration);
                sw.Flush();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error : {0}", e.Message);
            }
            finally
            {
                if (sw != null)
                {
                    try
                    {
                        sw.Close();
                    }
                    catch (Exception)
                    {
                    }
                    sw.Dispose();
                    sw = null;
                }
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                    }
                    catch (Exception)
                    {
                    }
                    fs.Dispose();
                    fs = null;
                }
            }
        }
    }
}
