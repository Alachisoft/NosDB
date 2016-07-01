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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Security.Impl
{
    public class DbSecurityServer : ISecurityServer
    {
        private ConfigurationServer ConfigurationServer { set; get; }

        public void Initialize(ConfigurationServer configurationServer)
        {
            ConfigurationServer = configurationServer;
        }

        public bool Grant(string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser user, Common.Security.Interfaces.IRole role)
        {
            if(ConfigurationServer != null)
                return ConfigurationServer.GrantRoleOnDatabaseServer(shardName, resourceId, user, role);
            return true; //returns true as the call was meant to be sent to db server from config server for granting role and it is already on db server
        }

        public bool Revoke(string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser user, Common.Security.Interfaces.IRole role)
        {
            if (ConfigurationServer != null)
                return ConfigurationServer.RevokeRoleOnDatabaseServer(shardName, resourceId, user, role);
            return true;
        }

        public bool CreateUser(IUser userInfo)
        {
            if (ConfigurationServer != null)
                return ConfigurationServer.CreateUserOnDBServer(userInfo);
            return true;
        }

        public bool DropUser(IUser userInfo)
        {
            if (ConfigurationServer != null)
                return ConfigurationServer.DropUserOnDBServer(userInfo);
            return true;
        }


        public void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId, string username)
        {
            try
            {
                if (ConfigurationServer != null)
                    ConfigurationServer.PublishAuthenticatedUserInfoToDBServer(sessionId, username);
            }
            catch (Exception exc)
            {
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.PublishLoginInfo", exc);
                }
            }

        }

        #region config server related method
        public IUser GetAuthenticatedUserInfoFromConfigServer(ISessionId sessionId)
        {
            throw new NotImplementedException();
        }
        #endregion


        public bool IsAuthorized(ISessionId sessionId, Common.Security.Impl.ResourceId resourceId, Common.Security.Impl.ResourceId superResourceId, Common.Security.Impl.Permission operationPermission)
        {
            return false;
        }


        public bool IsMixedMode
        {
            get { return false; }
        }
    }
}
