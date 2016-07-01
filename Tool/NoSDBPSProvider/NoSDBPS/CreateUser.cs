using System;
using System.Collections.Generic;
using System.Management.Automation;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Distributor;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security;
using System.Security.Principal;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Security.Impl.Enums;

namespace Alachisoft.NosDB.NosDBPS
{
    [Cmdlet(VerbsCommon.Add, "Login")]
    public class CreateUser:PSCmdlet
    {
        private string _userName;
        private string _password;
        private string _server;
        private int _port = MiscUtil.DEFAULT_CS_PORT;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify username.")]
        [Alias("u")]
        public string Username
        {
            set { _userName = value; }
            get { return _userName; }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify password.")]
        public string Password
        {
            set { _password = value; }
            get { return _password; }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify remote server Address.")]
        public string Server
        {
            set { _server = value; }
            get { return _server; }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify remote server port.")]
        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        bool UserExist(IList<IUser> loginList, string uName)
        {
            bool exist = false;
            foreach (IUser user in loginList)
            {
                if (!string.IsNullOrEmpty(user.Username) && user.Username.ToLower().Equals(uName.ToLower()))
                {
                    exist = true;
                    break;
                }
            }


            return exist;
        }

        protected override void BeginProcessing()
        {
            RemoteConfigurationManager remoteSession = new RemoteConfigurationManager();
            remoteSession.Initilize(MiscUtil.CLUSTERED, Server, Port, new ClientConfigurationFormatter(), ConfigurationConnection.Current.ClientCredential);
            IList<IUser> loginList = remoteSession.GetLogins();
            string currentUser = ConfigurationConnection.Current.ClientCredential.UserName;
            if (!string.IsNullOrEmpty(ConfigurationConnection.Current.ClientCredential.UserName) && !UserExist(loginList, ConfigurationConnection.Current.ClientCredential.UserName))
            {
                try
                {

                    SafeTokenHandle tokenHandle;
                    bool isLogon = SSPIUtility.LogonUser(Username, Password, out tokenHandle);
                    if (isLogon)
                    {
                        using (WindowsIdentity.Impersonate(tokenHandle.DangerousGetHandle()))
                        {
                            remoteSession = new RemoteConfigurationManager();
                            remoteSession.Initilize(MiscUtil.CLUSTERED, Server, Port, new ClientConfigurationFormatter(), ConfigurationConnection.Current.ClientCredential);
                            IUser user;
                            user = new User(currentUser);
                            
                            try
                            {
                                if (remoteSession.CreateUser(user))
                                    remoteSession.Grant(true, new ResourceId() { Name = Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER, ResourceType = Alachisoft.NosDB.Common.Security.Impl.Enums.ResourceType.System }, user.Username, ServerRole.sysadmin.ToString());
                            }
                            catch (SecurityException exc)
                            {
                                if (exc.ErrorCode.Equals(ErrorCodes.Security.USER_ALREADY_EXIST))
                                    remoteSession.Grant(true,  new ResourceId() { Name = Alachisoft.NosDB.Common.MiscUtil.NOSDB_CLUSTER_SERVER, ResourceType = Alachisoft.NosDB.Common.Security.Impl.Enums.ResourceType.System }, user.Username, ServerRole.sysadmin.ToString());
                            }

                        }
                        tokenHandle.Release();
                    }
                }
                catch (Exception ex)
                {
                }

            }
        }
    }
}
