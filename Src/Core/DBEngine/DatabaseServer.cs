using System.Collections.Generic;
using Alachisoft.NoSDB.Common.Communication.Server;
using Alachisoft.NoSDB.Common.Configuration;
using Alachisoft.NoSDB.Common.Configuration.Services;
using Alachisoft.NoSDB.Common.Configuration.Services.Client;
using Alachisoft.NoSDB.Common.Exceptions;
using Alachisoft.NoSDB.Common.Server;
using Alachisoft.NoSDB.Core.Configuration;
using Alachisoft.NoSDB.Core.Configuration.Services;
using Alachisoft.NoSDB.Core.Configuration.Services.Client;
using Alachisoft.NoSDB.Core.Toplogies;

namespace Alachisoft.NoSDB.Core.DBEngine
{
    public class DatabaseServer
    {
        ClusterConfiguration _configuration;
        IDictionary<string, IStore> _databases = new Dictionary<string, IStore>();
        IDatabaseTopology _databaseTopology;
        IServer _server;
        
        public DatabaseServer()
        {

        }

        public bool Initialize()
        {
            LoadConfiguration();
            _server = new TcpServer();
            //server.Initialize(configuration.Topology.Shards.)
            return true;
        }

        public void Start()
        {

        }

        public void Stop()
        { 
        }


        private void LoadConfiguration()
        {
            IConfigurationSession configurationSession = OutProcConfigurationClient.Instance.OpenConfigurationSession(null);
            _configuration = configurationSession.GetDatabaseClusterConfiguration("Default");
            ValidateConfiguaiton();
        }

        private void ValidateConfiguaiton()
        {
            if (_configuration == null)
                throw new ConfigurationException("No confiugration found");

            //add more validation checks on configuraiton
        }
    }
}
