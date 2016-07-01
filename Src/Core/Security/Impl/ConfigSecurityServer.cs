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
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Security.Impl
{
    public class ConfigSecurityServer : ISecurityServer
    {
        bool isMixedMode = false;
        IConfigurationSession _configurationSession;

        public void Initialize(IConfigurationSession configurationSession)
        {
            _configurationSession = configurationSession;
        }

        public string ClusterName { set; get; }

        public Common.Security.Interfaces.IUser GetAuthenticatedUserInfoFromConfigServer(Common.Security.Interfaces.ISessionId sessionId)
        {
            try
            {
                if (_configurationSession != null)
                    return _configurationSession.GetAuthenticatedUserInfoFromConfigServer(sessionId);
                return null;
            }
            catch (ChannelException)
            {
                return null;
            }

        }

        #region database server related methods
        public bool Grant(string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser userInfo, Common.Security.Interfaces.IRole roleInfo)
        {
            try
            {
                if (_configurationSession != null)
                    return _configurationSession.Grant(this.ClusterName, resourceId, userInfo.Username, roleInfo.RoleName);
                return false;
            }
            catch (ChannelException)
            {
                return false;
            }

        }

        public bool Revoke(string shardName, Common.Security.Impl.ResourceId resourceId, Common.Security.Interfaces.IUser userInfo, Common.Security.Interfaces.IRole roleInfo)
        {
            return true;
        }

        public bool CreateUser(Common.Security.Interfaces.IUser user)
        {
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                LoggerManager.Instance.SecurityLogger.Debug("ConfigSecurityServer.CreateUser", "Create user '" + user.Username + "'");
            try
            {
                if (_configurationSession != null)
                    return _configurationSession.CreateUser(user);
                return false;
            }
            catch (ChannelException)
            {
                return false;
            }

        }

        public bool DropUser(Common.Security.Interfaces.IUser user)
        {
            return true;
        }

        public void PublishAuthenticatedUserInfoToDBServer(Common.Security.Interfaces.ISessionId sessionId, string username)
        {
        }
        #endregion


        public bool IsAuthorized(Common.Security.Interfaces.ISessionId sessionId, Common.Security.Impl.ResourceId resourceId, Common.Security.Impl.ResourceId superResourceId, Common.Security.Impl.Permission operationPermission)
        {
            try
            {
                if (_configurationSession != null)
                {
                    resourceId.Name = ClusterName.ToLower() + "/" + resourceId.Name;
                    return _configurationSession.IsAuthorized(sessionId, resourceId, superResourceId, operationPermission);
                }
                return false;
            }
            catch (ChannelException)
            {
                return false;
            }

        }
    }
}
