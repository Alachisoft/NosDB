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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Communication.Exceptions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Security;
using Alachisoft.NosDB.Common.Security.Client;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Server;
using Alachisoft.NosDB.Common.Security.SSPI;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alachisoft.NosDB.Core.DBEngine;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Core.Configuration.SecurityConfig;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Alachisoft.NosDB.Core.Security.Impl
{
    public class SecurityManager : ISecurityManager
    {
        private string configFilePath = "";
        private string defaultWindowsUser = "";

        public ISecurityServer SecurityServer { set; get; }

        private IDictionary<string, ISecurityDatabase> _databaseManagers;
        private IDictionary<string, IDictionary<ResourceId, IResourceItem>> _shardsResources;

        private bool IsRemoteServer { set; get; }

        private ConcurrentDictionary<IUser, IUserLogin> _usersInfo; //key is session id against authenticated user
        private ConcurrentDictionary<ISessionId, IUser> _logins;
        private IDictionary<string, HashSet<ISessionId>> _connectionInfos;
        private IList<IUser> _users;
        private ConcurrentDictionary<ISessionId, ServerContext> _serverContexts;

        public IList<IUser> Users(string localShardName)
        {
            IUser[] users = null;
            if (_databaseManagers.ContainsKey(localShardName))
                users = _databaseManagers[localShardName].GetAllUserInformation();
            if (users != null)
            {
                foreach (IUser user in users)
                {
                    if (!_users.Contains(user))
                        _users.Add(user);
                }
            }
            return _users;
        }


        public void Initialize(string localShardName)
        {
            if (localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
            {
                configFilePath = ConfigurationSettings<CSHostSettings>.Current.SecurityConfigFile;
                IsSecurityEnabled = ConfigurationSettings<CSHostSettings>.Current.IsSecurityEnabled;
            }
            else
            {
                configFilePath = ConfigurationSettings<DBHostSettings>.Current.SecurityConfigFile;
                IsSecurityEnabled = ConfigurationSettings<DBHostSettings>.Current.IsSecurityEnabled;
            }
            _databaseManagers = new Dictionary<string, ISecurityDatabase>();
            _shardsResources = new Dictionary<string, IDictionary<ResourceId, IResourceItem>>(StringComparer.CurrentCultureIgnoreCase);

            //+security context initialization
            _serverContexts = new ConcurrentDictionary<ISessionId, ServerContext>();
            IsRemoteServer = SSPIUtility.IsSPNRegistered; // if spn is registered this is a remote server
            //-security context initialization 

            _usersInfo = new ConcurrentDictionary<IUser, IUserLogin>();
            _logins = new ConcurrentDictionary<ISessionId, IUser>();
            _users = new List<IUser>();
            _connectionInfos= new Dictionary<string, HashSet<ISessionId>>();

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new SecurityException(ErrorCodes.Security.PATH_NOT_FOUND);
            }
            ConfigurationBuilder builder = new ConfigurationBuilder(configFilePath);
            if (builder.FileExist)
            {
                builder.RegisterRootConfigurationObject(typeof(SecurityConfiguration));
                builder.ReadConfiguration();

                SecurityConfiguration[] configArray = new SecurityConfiguration[builder.Configuration.Length];
                builder.Configuration.CopyTo(configArray, 0);

                if (configArray.Length > 0)
                {
                    SecurityConfiguration config = configArray[0];
                    if(string.IsNullOrEmpty(config.DefaultWindowsUser))
                        throw new SecurityException(ErrorCodes.Security.VALUE_NOT_FOUND, new string[] { "DefaultWindowsUser" });
                    defaultWindowsUser = config.DefaultWindowsUser;
                }
            }
            else
            {
                throw new SecurityException(ErrorCodes.Security.CONFIG_NOT_FOUND, new string[] { configFilePath });
            }
        }

        private void SaveConfiguration()
        {
            SecurityConfiguration config = new SecurityConfiguration();
            config.DefaultWindowsUser = this.defaultWindowsUser;
            StringBuilder xml = new StringBuilder();

            ConfigurationBuilder cBuilder = new ConfigurationBuilder(new object[] {config});
            cBuilder.RegisterRootConfigurationObject(typeof(SecurityConfiguration));
            xml.Append(cBuilder.GetXmlString());
            WriteXMLToFile(xml.ToString());
        }

        private void WriteXMLToFile(string xml)
        {
            System.IO.StreamWriter sw = null;
            System.IO.FileStream fs = null;
            try
            {
                fs = new System.IO.FileStream(configFilePath, System.IO.FileMode.Create);
                sw = new System.IO.StreamWriter(fs);
                sw.Write(xml);
                sw.Flush();
            }
            catch (Exception e)
            {
                throw e;
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
                    { }
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
                    { }
                    fs.Dispose();
                    fs = null;
                }
            }
        }

        public void LoadAuthorizationInformation(string localShardName, IUser user, ISessionId sessionId)
        {
            if (IsSecurityEnabled)
            {
                //for workgroup users
                if (_users.Contains(user))
                {
                    int indexof = _users.IndexOf(user);
                    if (indexof >= 0)
                        user = _users[indexof];
                }
                if(!_logins.ContainsKey(sessionId))
                    _logins.TryAdd(sessionId, user);
                else
                {
                    IUser existingUser = _logins[sessionId];
                    _logins.TryUpdate(sessionId, user, existingUser);
                }

                IUserLogin userLogin = new UserLogin();
                userLogin.Username = user.Username;
                userLogin.Roles = new Dictionary<IRole, IList<ResourceId>>();
                if (_shardsResources.ContainsKey(localShardName))
                {
                    foreach (IResourceItem resourceItem in _shardsResources[localShardName].Values)
                    {
                        foreach (IRole role in resourceItem.Roles.Keys)
                        {
                            if (resourceItem.Roles[role].AuthorizedUsers.Contains(user.Username, StringComparer.CurrentCultureIgnoreCase))
                            {
                                if (!userLogin.Roles.ContainsKey(role))
                                    userLogin.Roles[role] = new List<ResourceId>();
                                userLogin.Roles[role].Add(resourceItem.ResourceId);
                            }
                        }
                    }
                    if(!_usersInfo.ContainsKey(_logins[sessionId]))
                        _usersInfo.TryAdd(_logins[sessionId], userLogin);
                    else
                    {
                        IUserLogin existingUserLogin = _usersInfo[_logins[sessionId]];
                        _usersInfo.TryUpdate(_logins[sessionId], userLogin, existingUserLogin);
                    }

                    if (_users.Contains(user) && LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
                        LoggerManager.Instance.SecurityLogger.Info("SecurityManager.LoadAuthorizationInformation", "User: '" + userLogin.Username + "' logged in successfully with session id: " + sessionId.SessionId);
                    
                    try
                    {
                        if (SecurityServer != null && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                            SecurityServer.PublishAuthenticatedUserInfoToDBServer(sessionId, userLogin.Username);
                    }
                    catch (Exception exc)
                    {
                        //TODO: this was a temporary fix, needs to be reviewed and properly fixed
                    }
                }
                _usersInfo[_logins[sessionId]] = userLogin;
                if (SecurityServer != null && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                    SecurityServer.PublishAuthenticatedUserInfoToDBServer(sessionId, userLogin.Username);
            }
        }


        public bool Authorize(string localShardName, ISessionId sessionId, ResourceId resourceId, ResourceId superResourceId, Permission operationPermission, bool LogError=true)
        {
            bool isAuthorized = false;
            try
            {
                string username = "";
                if (IsSecurityEnabled)
                {
                    IUser user = null;
                    if (_logins.ContainsKey(sessionId))
                    {
                        user = _logins[sessionId];
                        if (!_users.Contains(user)) //not registered
                        {
                            IUser userFromStore = null;
                            if (_databaseManagers.ContainsKey(localShardName))
                            {
                                userFromStore = _databaseManagers[localShardName].GetUserInformatio(user.Username);
                            }
                            if (userFromStore != null)
                            {
                                _users.Add(userFromStore);
                                user = userFromStore;
                                username = user.Username;
                            }
                            else
                            {
                                throw new SecurityException(ErrorCodes.Security.USER_NOT_REGISTERED, new string[1] { user.Username });
                            }
                        }

                        int index = _users.IndexOf(user);
                        if (index >= 0)
                            user = _users[index];

                        user.IsAuthenticated = true;

                    }
                    else if (MiscUtil.CS_SESSION_ID.Equals(sessionId.SessionId, StringComparison.CurrentCultureIgnoreCase))
                        isAuthorized = true;
                    //start authorization from super resources
                    if (_shardsResources.ContainsKey(localShardName))
                    {
                        ResourceId tempResourceId = resourceId;
                        if (resourceId.ResourceType == ResourceType.Collection)
                            tempResourceId = superResourceId;
                        if (resourceId.ResourceType == ResourceType.Cluster)
                            LoadResourceFromDB(localShardName, resourceId);

                        IResourceItem[] resourceItemArray = new ResourceItem[_shardsResources[localShardName].Values.Count];
                        _shardsResources[localShardName].Values.CopyTo(resourceItemArray, 0);
                        foreach (IResourceItem resourceItem in resourceItemArray)
                        {
                            if (resourceItem.SubResources.Contains(tempResourceId))
                            {
                                try
                                {
                                    isAuthorized = Authorize(localShardName, sessionId, resourceItem.ResourceId, null,
                                        operationPermission, false);
                                    break;
                                }
                                catch (SecurityException exc)
                                {
                                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                                    {
                                        LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.Authorize.SuperResource", exc.ToString());
                                    }
                                }
                            }
                        }
                    }
                    //if not authorized on super resources, than verify on currentResource
                    if (!isAuthorized)
                    {
                        if (superResourceId == null)
                        {
                            superResourceId = resourceId;
                        }
                        if (_logins.ContainsKey(sessionId) && _usersInfo.ContainsKey(_logins[sessionId]))
                        {
                            IUserLogin currentUser = _usersInfo[_logins[sessionId]];
                            username = currentUser.Username;
                            if (!_shardsResources.ContainsKey(localShardName) || !_shardsResources[localShardName].ContainsKey(superResourceId))
                                if (operationPermission.OperationType == OperationType.Delete &&
                                    operationPermission.ResourceType == ResourceType.Database)
                                    return true;
                                else
                                {
                                    if (_databaseManagers.ContainsKey(localShardName))
                                    {
                                        IResourceItem resourceFromStore =
                                            _databaseManagers[localShardName].GetResourceSecurityInformatio(
                                                MiscUtil.CLUSTERED, superResourceId.Name);
                                        if (resourceFromStore != null)
                                        {
                                            foreach (IRole role in resourceFromStore.Roles.Keys)
                                            {

                                                if (resourceFromStore.Roles[role].AuthorizedUsers.Contains(
                                                    user.Username, StringComparer.CurrentCultureIgnoreCase))
                                                {
                                                    if (!currentUser.Roles.ContainsKey(role))
                                                        currentUser.Roles[role] = new List<ResourceId>();
                                                    if (!currentUser.Roles[role].Contains(resourceFromStore.ResourceId))
                                                        currentUser.Roles[role].Add(resourceFromStore.ResourceId);
                                                    if (role.HasPermission(operationPermission))
                                                        isAuthorized = true;
                                                }
                                            }
                                        }
                                        else
                                            throw new SecurityException(ErrorCodes.Security.NO_RESOURCE_EXIST,new[] {superResourceId.ResourceType + ":" + superResourceId.Name});
                                    }
                                }
                            isAuthorized = currentUser.IsAuthorized(operationPermission, superResourceId);
                            if (!isAuthorized)
                                if (SecurityServer != null && SecurityServer.IsAuthorized(sessionId, resourceId, superResourceId, operationPermission))
                                    isAuthorized = true;
                            if (!isAuthorized && user!=null)
                            {
                                throw new SecurityException(ErrorCodes.Security.UNAUTHORIZED_USER, new[] { user.Username, operationPermission.OperationType.ToString() + " " + operationPermission.ResourceType.ToString(), superResourceId.ResourceType.ToString() + ":" + superResourceId.Name });
                            }
                        }
                        else
                        {
                            if (SecurityServer != null && SecurityServer.IsAuthorized(sessionId, resourceId, superResourceId, operationPermission))
                                return true;
                             throw new SecurityException(ErrorCodes.Security.LOGIN_NOT_EXIST, new[] {sessionId.SessionId ?? "null"});
                        }
                    }
                }
                else
                {
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                    {
                        LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.Authorize() ", "Security is disabled. Authorization skipped.");
                    }
                    return true;
                }
                if (!isAuthorized)
                    throw new SecurityException(ErrorCodes.Security.UNAUTHORIZED_USER, new string[] { username, operationPermission.OperationType.ToString() + " " + operationPermission.ResourceType.ToString(), resourceId.ResourceType.ToString() + ": " + resourceId.Name });
            }
            catch (SecurityException exc)
            {
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled && LogError)
                {
                    LoggerManager.Instance.SecurityLogger.Error("SecurtiyManager.Authorize() ", exc);
                }
                throw exc;
            }
            catch (Exception exc)
            {
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled && LogError)
                {
                    LoggerManager.Instance.SecurityLogger.Error("SecurtiyManager.Authorize() ", exc);
                }
                throw exc;
            }
            return true;
        }


        public IServerAuthenticationCredential Authenticate(string localShardName, IClientAuthenticationCredential clientCredentials, ISessionId sessionId, bool isLocalClient, string serviceName = null)
        {
            try
            {
                if (clientCredentials is SSPIClientAuthenticationCredential)
                    return AuthenticateWindowsClient(localShardName, clientCredentials as SSPIClientAuthenticationCredential, sessionId, isLocalClient, serviceName);
                return null;
            }
            catch (DatabaseException exc)
            {
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.Authenticate", exc.ToString());
                throw exc;
            }
        }

        public AuthToken Authenticate(string localShardName, IAuthenticationOperation operation, bool isLocalClient, string distributorServiceName = null)
        {

            AuthToken nextAuthToken = null;
            var authenticationOperation = operation as AuthenticationOperation;
            if (authenticationOperation != null)
            {
                var sessionId = authenticationOperation.SessionId;
                var serverAddress = authenticationOperation.Address;

                var clientAuthToken = authenticationOperation.ClientToken;
                string username = "";
                nextAuthToken = GetAuthToken(clientAuthToken, sessionId, isLocalClient, ref username);
                if (nextAuthToken.Status == SecurityStatus.OK)
                {
                    if (!string.IsNullOrEmpty(distributorServiceName))
                        username = distributorServiceName;
                    sessionId.Username = username;
                    LoadAuthorizationInformation(localShardName, new User(username), sessionId);

                }
                AddConnectionInfo(serverAddress, sessionId);
            }
            return nextAuthToken;
        }

        public void AddSecurityDatabase(string clusterName, ISecurityDatabase securityDatabase)
        {
                if (!_databaseManagers.ContainsKey(clusterName))
                {
                    _databaseManagers.Add(clusterName, securityDatabase);
                }
        }

        public ISecurityDatabase GetSecurityDatabase(string clusterName)
        {
            if (_databaseManagers.ContainsKey(clusterName))
                return _databaseManagers[clusterName];
            return null;
        }

        public void RemoveSecurityDatabase(string clusterName)
        {
            if (_databaseManagers.ContainsKey(clusterName))
            {
                _databaseManagers.Remove(clusterName);
            }
        }

        public IDictionary<ResourceId, IResourceItem> LoadShardResources(string shardName)
        {
            IDictionary<ResourceId, IResourceItem> resources = new Dictionary<ResourceId, IResourceItem>();
            if(_databaseManagers.ContainsKey(shardName))
            {
                IResourceItem[] resourceItems = null;
                IUser[] registeredUsers = null;
                if (_databaseManagers[shardName].IsInitialized)
                {
                    resourceItems = _databaseManagers[shardName].GetAllResourcesSecurityInformation();

                    foreach (IResourceItem resource in resourceItems)
                    {
                        if (!resources.ContainsKey(resource.ResourceId))
                            resources[resource.ResourceId] = resource;
                    }

                    registeredUsers = _databaseManagers[shardName].GetAllUserInformation();

                    foreach (IUser user in registeredUsers)
                    {
                        if (!_users.Contains(user))
                        {
                            _users.Add(user);
                        }
                    }
                }
            }
            return resources; 
        }

        public void InitializeSecurityInformation(string shard)
        {
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.InitializeSecurityInformation", "Initializing security information for shard: " + shard);
            if (!_shardsResources.ContainsKey(shard))
            {
                if (!_shardsResources.ContainsKey(shard) &&
                    (_databaseManagers.ContainsKey(shard) && _databaseManagers[shard].IsInitialized))
                {
                    IDictionary<ResourceId, IResourceItem> resources = LoadShardResources(shard);
                    _shardsResources[shard] = resources;
                }
                try
                {
                    IUser confSvcUser = new User(@"NT SERVICE\" + MiscUtil.NOSDB_CSVC_NAME);
                    try
                    {
                        if (!_users.Contains(confSvcUser))
                            CreateUser(shard, confSvcUser);
                    }
                    catch (Exception exc)
                    {
                        if (IsSecurityEnabled)
                            throw exc;
                    }
                }
                catch (ManagementException exc)
                {
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                        LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.InitializeSecurityInformaiton", exc.ToString());
                }

                if (shard.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                {
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                        LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.InitializeSecurityInformation", "Initializing security information on configuraiton server");
                    var resourceId = new ResourceId()
                    {
                        Name = MiscUtil.NOSDB_CLUSTER_SERVER,
                        ResourceType = ResourceType.System
                    };

                    IResourceItem resourceItem = new ResourceItem(resourceId);
                    
                    if (!_shardsResources[shard].ContainsKey(resourceId))
                    {
                        _shardsResources[shard][resourceId] = resourceItem;
                    }
                    bool isDwuRegistered = false;

                    if (!string.IsNullOrEmpty(defaultWindowsUser))
                    {
                        IUser user = new User( defaultWindowsUser);
                        try
                        {
                            if (!_users.Contains(user))
                            {
                                isDwuRegistered = CreateUser(shard, user);
                                if (isDwuRegistered)
                                    Grant(shard, resourceId, user, Role.sysadmin);
                            }
                            else
                                isDwuRegistered = true;
                        }
                        catch (SecurityException)
                        {
                            if (!isDwuRegistered && IsSecurityEnabled)
                            {
                                throw;
                            }
                        }
                        catch (Exception)
                        {
                            if (IsSecurityEnabled)
                                throw;
                        }
                    }

             

                    #region NoS Service Accounts

                    IUser dbSvcUser = new User(@"NT SERVICE\" + MiscUtil.NOSDB_DBSVC_NAME);
                    try
                    {
                        if (!_users.Contains(dbSvcUser))
                            CreateUser(shard, dbSvcUser);
                        Grant(shard, resourceId, dbSvcUser, Role.sysadmin);
                    }
                    catch (Exception exc)
                    {
                        if (IsSecurityEnabled)
                            throw;
                    }

                    #endregion


                    if (_databaseManagers.ContainsKey(shard))
                    {
                        _databaseManagers[shard].InsertOrUpdateResourceSecurityInformation(MiscUtil.CLUSTERED,
                            resourceItem);
                    }
                }
                else
                {
                    if (LoggerManager.Instance.SecurityLogger != null &&
                        LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                        LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.InitializeSecurityInformation",
                            "Initializing security information on " + shard);
                    if (!_shardsResources.ContainsKey(shard))
                    {
                        _shardsResources[shard] = new Dictionary<ResourceId, IResourceItem>();
                    }

                    ResourceId resourceId = new ResourceId()
                    {
                        Name = MiscUtil.NOSDB_CLUSTER_SERVER,
                        ResourceType = ResourceType.System
                    };

                    IResourceItem resourceItem = new ResourceItem(resourceId);

                    #region Default Users Registeration

                    bool isDWURegistered = false;
                    IUser user = null;
                    if (!SSPIUtility.IsDomainEnvironment)
                    {
                        if (LoggerManager.Instance.SecurityLogger != null &&
                            LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                            LoggerManager.Instance.SecurityLogger.Debug(
                                "SecurityManager.InitializeSecurityInformation", "Registering default user");
                        if (!string.IsNullOrEmpty(defaultWindowsUser))
                        {
                            user = new User(defaultWindowsUser);
                            try
                            {
                                if (!_users.Contains(user) && this.SecurityServer != null)
                                {
                                    try
                                    {
                                        if (SecurityServer != null)
                                        {
                                            isDWURegistered = this.SecurityServer.CreateUser(user);
                                            this.SecurityServer.Grant(shard, resourceId, user, Role.sysadmin);
                                        }
                                        else
                                        {
                                            if (LoggerManager.Instance.SecurityLogger != null &&
                                                LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                                            {
                                                LoggerManager.Instance.SecurityLogger.Debug(
                                                    "SecurityManager.InitializeSecurityInformation",
                                                    "unable to connect to configuraiton server.");
                                            }
                                            isDWURegistered = true;
                                        }
                                    }
                                    catch (ManagementException exc)
                                    {
                                        if (LoggerManager.Instance.SecurityLogger != null &&
                                            LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                                            LoggerManager.Instance.SecurityLogger.Debug(
                                                "SecurityManager.InitializeSecurityInformaiton", exc.ToString());
                                        isDWURegistered = true;
                                    }
                                }
                                else
                                {
                                    if (LoggerManager.Instance.SecurityLogger != null &&
                                        LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                                        LoggerManager.Instance.SecurityLogger.Debug(
                                            "SecurityManager.InitializeSecurityInformaiton",
                                            "User '" + user.Username + "' already exists.");

                                    isDWURegistered = true;
                                }
                            }
                            catch (Exception exc)
                            {
                                if (IsSecurityEnabled)
                                    throw exc;
                            }
                        }
                        else
                        {
                            isDWURegistered = false;
                        }

                        if (!isDWURegistered)
                        {
                            if (IsSecurityEnabled)
                                throw new SecurityException(ErrorCodes.Security.ERROR_READING_REGISTRY);
                        }

                        //try
                        //{
                        //    if (anonymousUser != null && SecurityServer != null)
                        //    {
                        //        this.SecurityServer.Grant(shard, resourceId, anonymousUser, Role.sysadmin);
                        //    }
                        //}
                        //catch (ManagementException exc)
                        //{ }
                        //catch (Exception exc)
                        //{
                        //    if (IsSecurityEnabled)
                        //    {
                        //        throw exc;
                        //    }
                        //}
                    }
                    else
                    {
                        if (LoggerManager.Instance.SecurityLogger != null &&
                            LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                            LoggerManager.Instance.SecurityLogger.Debug(
                                "SecurityManager.InitializeSecurityInformation", "user registeration skipped");
                    }

                    #endregion

                }
            }
        }

        public void AddResource(string localShardName, IResourceItem resourceItem, ISessionId sessionId, ResourceId superResourceId = null, string clusterName = MiscUtil.CLUSTERED)
        {
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.AddResource() ","AddResource(resourceName: " + resourceItem.ResourceId.Name + ", resourceType: " + resourceItem.ResourceId.ResourceType.ToString());
                if (!_shardsResources.ContainsKey(localShardName))
                    _shardsResources[localShardName] = new Dictionary<ResourceId, IResourceItem>();
                if (_shardsResources.ContainsKey(localShardName))
                {
                    if (!_shardsResources[localShardName].ContainsKey(resourceItem.ResourceId))
                    {
                        _shardsResources[localShardName][resourceItem.ResourceId] = resourceItem;
                    }

                    if (sessionId != null && _logins.ContainsKey(sessionId))
                    {
                        IUser currentUser = new User(_usersInfo[_logins[sessionId]].Username);
                        switch (resourceItem.ResourceId.ResourceType)
                        {
                            case Common.Security.Impl.Enums.ResourceType.Cluster:
                            case ResourceType.ConfigurationCluster:
                                this.Grant(localShardName, resourceItem.ResourceId, currentUser, Role.clusteradmin, clusterName);
                                this.Grant(localShardName, resourceItem.ResourceId, currentUser, Role.clustermanager, clusterName);
                                this.Grant(localShardName, resourceItem.ResourceId, currentUser, Role.dbcreator, clusterName);
                                break;
                            case Common.Security.Impl.Enums.ResourceType.Database:
                                this.Grant(localShardName, resourceItem.ResourceId, currentUser, Role.db_owner, superResourceId == null ? null : superResourceId.Name);
                                this.Grant(localShardName, resourceItem.ResourceId, currentUser, Role.db_admin, superResourceId == null ? null : superResourceId.Name);
                                this.Grant(localShardName, resourceItem.ResourceId, currentUser, Role.db_user, superResourceId == null ? null : superResourceId.Name);
                                break;
                            default:
                                break;
                        }
                    }

                    if (resourceItem.ResourceId.ResourceType == ResourceType.Database)
                        if(superResourceId != null)
                            GrantRolesToDefaultUsers(localShardName, resourceItem.ResourceId, superResourceId.Name);
                        else
                            GrantRolesToDefaultUsers(localShardName, resourceItem.ResourceId);
                    else if (resourceItem.ResourceId.ResourceType == ResourceType.Cluster)
                        GrantRolesToDefaultUsers(localShardName, resourceItem.ResourceId);
                }
                else
                    throw new ArgumentException("No shard found against provided local shard name.", "localShardName");
                
                if (superResourceId == null && localShardName.Equals(Alachisoft.NosDB.Common.MiscUtil.CONFIGURATION_SHARD_NAME))
                {
                    if (resourceItem.ResourceId.ResourceType == Common.Security.Impl.Enums.ResourceType.Cluster || resourceItem.ResourceId.ResourceType == Common.Security.Impl.Enums.ResourceType.ConfigurationCluster || resourceItem.ResourceId.ResourceType == Common.Security.Impl.Enums.ResourceType.Database)
                    {
                        if (_shardsResources.ContainsKey(localShardName))
                            superResourceId = new ResourceId() { ResourceType = Common.Security.Impl.Enums.ResourceType.System, Name = "NosDB_Cluster_Server" };
                    }
                }

                IResourceItem superResource = null;

                if (superResourceId != null && _shardsResources.ContainsKey(localShardName))
                {
                    if (superResourceId != null && _shardsResources[localShardName].ContainsKey(superResourceId))
                    {
                        superResource = _shardsResources[localShardName][superResourceId];
                    }
                    else
                    {
                        throw new ArgumentException("No Super Resource found against provided Super Resource Id '" + superResourceId.ResourceType.ToString() + ":" + superResourceId.Name + "'.", "superResourceId");
                    }

                    superResource.AddSubResource(resourceItem.ResourceId);
                }

                if (_databaseManagers.ContainsKey(localShardName) && _databaseManagers[localShardName].IsInitialized)
                {
                    if(superResource != null)
                        _databaseManagers[localShardName].InsertOrUpdateResourceSecurityInformation(clusterName, superResource);
                    _databaseManagers[localShardName].InsertOrUpdateResourceSecurityInformation(clusterName, resourceItem);
                }
        }

        private void GrantRolesToDefaultUsers(string localShardName, ResourceId resourceId, string cluster = MiscUtil.CLUSTERED)
        {
            string windowsUserId = defaultWindowsUser;
            if (windowsUserId != null)
            {
                IUser user = new User(windowsUserId);
                if (_users.Contains(user))
                {
                    switch (resourceId.ResourceType)
                    {
                        case ResourceType.Cluster:
                            Grant(localShardName, resourceId, user, Role.clusteradmin, resourceId.Name);
                            Grant(localShardName, resourceId, user, Role.clustermanager, resourceId.Name);
                            Grant(localShardName, resourceId, user, Role.dbcreator, resourceId.Name);
                            break;
                        case ResourceType.Database:
                            Grant(localShardName, resourceId, user, Role.db_owner, cluster);
                            Grant(localShardName, resourceId, user, Role.db_admin, cluster);
                            Grant(localShardName, resourceId, user, Role.db_user, cluster);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void RemoveResource(string localShardName, ResourceId resouceId, ISessionId sessionId, ResourceId superResourceId = null, string clusterName = null)
        {
                //if (superResourceId == null)
                //{
                //    if (resouceId.ResourceType == Common.Security.Impl.Enums.ResourceType.Cluster)
                //    {
                //        if (_shardsResources.ContainsKey(localShardName))
                //            superResourceId = new ResourceId() { ResourceType = Common.Security.Impl.Enums.ResourceType.System, Name = "NosDB_Cluster_Server" };
                //    }
                //    else
                //        throw new ArgumentNullException("superResouceId", "superResourceId can only be null if resouce to be added is cluster. Default in the case will be system");
                //}

                IResourceItem superResource = null;
                IResourceItem resource = null;
                if (_shardsResources.ContainsKey(localShardName))
                {
                    if (superResourceId != null)
                    {
                        if (_shardsResources[localShardName].ContainsKey(superResourceId))
                        {
                            superResource = _shardsResources[localShardName][superResourceId];
                        }
                        else
                            throw new ArgumentException("No Super Resource found against provided Super Resource Id.", "superResourceId");
                    }
                    else if(resouceId.ResourceType == ResourceType.Database)
                    {
                        foreach (ResourceItem resourceitem in _shardsResources[localShardName].Values)
                        {
                            if (resourceitem.SubResources.Contains(resouceId))
                                superResource = resourceitem;
                        }
                    }

                    if (_shardsResources[localShardName].ContainsKey(resouceId))
                    {
                        resource = _shardsResources[localShardName][resouceId];
                        _shardsResources[localShardName].Remove(resouceId);
                    }
                }
                else
                    throw new ArgumentException("No shard found against provided local shard name.", "localShardName");

                if (superResource != null)
                    superResource.RemoveSubResource(resouceId);

                if (_databaseManagers.ContainsKey(localShardName) && _databaseManagers[localShardName].IsInitialized)
                {
                    if (superResource != null)
                        _databaseManagers[localShardName].InsertOrUpdateResourceSecurityInformation(clusterName, superResource);
                    _databaseManagers[localShardName].RemoveResourceSecurityInformation(clusterName, resouceId.Name);
                }
        }

        public bool Grant(string localShardName, ResourceId resourceId, IUser userInfo, IRole roleInfo, string clusterName = MiscUtil.CLUSTERED)
        {
       
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = localShardName != null ? localShardName : "", DatabaseName = "" });
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.Grant() ","SecurityManager.Grant(resourceName: " + resourceId.Name + ", resourceType: " + resourceId.ResourceType.ToString() + ", Role: " + roleInfo.RoleName);
            if (resourceId.ResourceType == ResourceType.Database && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME, StringComparison.CurrentCultureIgnoreCase)) //cluster name is prefixed with database name on configuration server to seperate standalone cluster's dbs with same name as cluster's dbs
            {
                if (!resourceId.Name.Contains(clusterName.ToLower() + "/"))
                    resourceId.Name = clusterName.ToLower() + "/" + resourceId.Name;
            }
            bool isSuccessful = false;
            if (_shardsResources.ContainsKey(localShardName))
            {
                if (_users.Contains(userInfo))
                {
                        
                    if (_shardsResources[localShardName].ContainsKey(resourceId))
                    {
                        if (roleInfo.RoleLevel == RoleLevel.Database && resourceId.ResourceType == ResourceType.Database && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                        {
                            ResourceId dbServerResourceId = new ResourceId();
                            dbServerResourceId.Name = resourceId.Name.Substring(resourceId.Name.LastIndexOf('/') + 1);
                            dbServerResourceId.ResourceType = resourceId.ResourceType;
                            try
                            {
                                isSuccessful = SecurityServer.Grant(clusterName.ToLower(), dbServerResourceId, userInfo, roleInfo);
                            }
                            catch (Exception exc)
                            { }
                        }
                        IResourceItem resourceItem = _shardsResources[localShardName][resourceId];
                        isSuccessful = resourceItem.GrantRole(roleInfo, userInfo.Username) || isSuccessful;
                        if (isSuccessful && resourceItem.SubResources.Count > 0)
                            isSuccessful = GrantRolesOnSubResource(localShardName, resourceId, userInfo, clusterName.ToLower()) || isSuccessful;
                        if (_usersInfo.ContainsKey(userInfo))
                            _usersInfo[userInfo].GrantRole(roleInfo, resourceId);
                        if (_databaseManagers.ContainsKey(localShardName))
                            _databaseManagers[localShardName].InsertOrUpdateResourceSecurityInformation(clusterName.ToLower(), resourceItem);
                        if (isSuccessful)
                        {
                            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
                                LoggerManager.Instance.SecurityLogger.Info("SecurityManager.Grant", roleInfo.RoleName + " is granted on " + resourceId.ResourceType.ToString() + " '" + resourceId.Name + "' to user : " + userInfo.Username);
                        }
                    }
                    else
                    {
                        SecurityException exc = new SecurityException(ErrorCodes.Security.NO_RESOURCE_EXIST, new string[] { resourceId.ResourceType.ToString() + ":" + resourceId.Name });
                        if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                            LoggerManager.Instance.SecurityLogger.Error("SecurityManager.Grant() ",exc);
                        throw exc;
                    }
                }
                else
                {
                    SecurityException exc = new SecurityException(ErrorCodes.Security.NO_USER_EXIST, new string[1] { userInfo.Username });
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                        LoggerManager.Instance.SecurityLogger.Error("SecurityManager.Grant", exc);
                    throw exc;
                }
            }
            return isSuccessful;
        }

        public bool Revoke(string localShardName, ResourceId resourceId, IUser userInfo, IRole roleInfo, string clusterName = MiscUtil.CLUSTERED)
        {
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.Revoke() ","SecurityManager.Revoke(resourceName: " + resourceId.Name + ", resourceType: " + resourceId.ResourceType.ToString() + ", Role: " + roleInfo.RoleName);
            
            if (resourceId.ResourceType == ResourceType.Database && !string.IsNullOrEmpty(clusterName)) //cluster name is prefixed with database name on configuration server to seperate standalone cluster's dbs with same name as cluster's dbs
            {
                if (!resourceId.Name.Contains(clusterName + "/"))
                    resourceId.Name = clusterName + "/" + resourceId.Name;
            }
            bool isSuccessful = false;
            if (!_users.Contains(userInfo))
            {
                SecurityException exc = new SecurityException(ErrorCodes.Security.NO_USER_EXIST, new string[1] { userInfo.Username });
                if(LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.Revoke", exc);
                throw exc;
            }
                if (_shardsResources.ContainsKey(localShardName))
                {
                    if (_shardsResources[localShardName].ContainsKey(resourceId))
                    {
                        if (roleInfo.RoleLevel == RoleLevel.Database && resourceId.ResourceType == ResourceType.Database && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                        {
                            ResourceId dbServerResourceId = new ResourceId();
                            dbServerResourceId.Name = resourceId.Name.Substring(resourceId.Name.LastIndexOf('/') + 1);
                            dbServerResourceId.ResourceType = resourceId.ResourceType;
                            isSuccessful = SecurityServer.Revoke(clusterName, dbServerResourceId, userInfo, roleInfo);
                        }
                        IResourceItem resourceItem = _shardsResources[localShardName][resourceId];
                        isSuccessful = resourceItem.RevokeRole(roleInfo, userInfo.Username) || isSuccessful;
                        //if (isSuccessful && resourceItem.SubResources.Count > 0)
                        //    isSuccessful = RevokeRolesOnSubResource(localShardName, resourceId, userInfo, clusterName) || isSuccessful;
                        if (_usersInfo.ContainsKey(userInfo))
                            _usersInfo[userInfo].RevokeRole(roleInfo, resourceId);
                        if (_databaseManagers.ContainsKey(localShardName))
                            _databaseManagers[localShardName].InsertOrUpdateResourceSecurityInformation(clusterName, resourceItem);
                        if (isSuccessful)
                        {
                            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                                LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.Revoke() ",roleInfo.RoleName + " is revoked on " + resourceId.ResourceType.ToString() + " '" + resourceId.Name + "' from user : " + userInfo.Username);
                        }
                    }
                    else
                    {
                        SecurityException exc = new SecurityException(ErrorCodes.Security.NO_RESOURCE_EXIST, new string[] { resourceId.ResourceType.ToString() + ":" + resourceId.Name });
                        if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                            LoggerManager.Instance.SecurityLogger.Error("SecurityManager.Revoke() ",exc);
                        throw exc;
                    }
                }
            return isSuccessful;
        }

        public static void GetSecurityInformation(Permission permission, string resourceName, out ResourceId resourceId, out ResourceId superResourceId, string cluster, string superResourceName = null)
        {
            if (superResourceName == null)
            {
                superResourceId = null;
            }
            else
            {
                superResourceId = new ResourceId() { Name = superResourceName };
                switch (permission.ResourceType)
                {
                    case ResourceType.Cluster:
                    case ResourceType.ConfigurationCluster:
                        superResourceId.ResourceType = ResourceType.System;
                        break;
                    case ResourceType.Database:
                        superResourceId.ResourceType = ResourceType.Cluster;
                        break;
                    case ResourceType.Collection:
                    case ResourceType.Index:
                    case ResourceType.UserDefinedFunction:
                    case ResourceType.Trigger:
                        superResourceId.Name = cluster + "/" + superResourceId.Name;
                        superResourceId.ResourceType = ResourceType.Database;
                        break;
                    default:
                        break;
                }
            }

            ResourceType resourceType = permission.ResourceType;
            string resourceNamePrefix = ""; //if the resource is database, cluster name will be added in prefix to differentiate different cluster's databases with same name.
            switch (resourceType)
            {
                case ResourceType.Database:
                    if(!string.IsNullOrEmpty(superResourceName))
                        resourceNamePrefix = superResourceName + "/";
                    if (permission.OperationType == OperationType.Delete)
                        superResourceId = null; //for dbowner role
                    if (permission.OperationType == OperationType.Backup)
                        resourceType = ResourceType.Cluster;
                    break;
                case ResourceType.Trigger:
                    resourceType = ResourceType.Collection;
                    break;
                case ResourceType.User:
                    resourceType = ResourceType.System;
                    break;
                default:
                    break;
            }

            resourceId = new ResourceId() { Name = resourceNamePrefix + resourceName, ResourceType = resourceType };
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.GetSecurityInformation() ","GetSecurityInformation(resourceName: " + resourceId.Name + ", resourceType: " + resourceId.ResourceType.ToString());
        }

        public IList<IResourceItem> GetSubResources(string localShardName, ResourceId resourceId)
        {
            IList<IResourceItem> resources = null;
            LoadResourceFromDB(localShardName, resourceId);
            if (_shardsResources.ContainsKey(localShardName) && _shardsResources[localShardName].ContainsKey(resourceId))
            {
                IList<ResourceId> resourceIds = _shardsResources[localShardName][resourceId].SubResources.Clone<ResourceId>();
                resources = new List<IResourceItem>();
                foreach (ResourceId subResourceId in resourceIds)
                {
                    if (_shardsResources[localShardName].ContainsKey(subResourceId))
                    {
                        IResourceItem resource = new ResourceItem(new ResourceId() { Name = _shardsResources[localShardName][subResourceId].ResourceId.Name.Split('/')[1], ResourceType = _shardsResources[localShardName][subResourceId].ResourceId.ResourceType });
                        resource.Roles = _shardsResources[localShardName][subResourceId].Roles;
                        resource.SubResources = _shardsResources[localShardName][subResourceId].SubResources;
                        resources.Add(resource);
                    }
                    else
                    {
                        IResourceItem resourceItem = null;
                        if (_databaseManagers.ContainsKey(localShardName))
                            resourceItem = _databaseManagers[localShardName].GetResourceSecurityInformatio(MiscUtil.CLUSTERED, resourceId.Name);
                        if (resourceItem == null)
                            _shardsResources[localShardName][resourceId].RemoveSubResource(subResourceId);
                        else
                        {
                            _shardsResources[localShardName][resourceItem.ResourceId] = resourceItem;

                            IResourceItem resource = new ResourceItem(new ResourceId() { Name = _shardsResources[localShardName][subResourceId].ResourceId.Name.Split('/')[1], ResourceType = _shardsResources[localShardName][subResourceId].ResourceId.ResourceType });
                            resource.Roles = _shardsResources[localShardName][subResourceId].Roles;
                            resource.SubResources = _shardsResources[localShardName][subResourceId].SubResources;
                            resources.Add(resource);
                        }

                    }
                }
            }
            return resources;
        }

        private void LoadResourceFromDB(string localShardName, ResourceId resourceId)
        {
            if (_databaseManagers.ContainsKey(localShardName))
            {
                IResourceItem resourceItem = _databaseManagers[localShardName].GetResourceSecurityInformatio(MiscUtil.CLUSTERED, resourceId.Name);

                if (resourceItem != null)
                {
                    if (_shardsResources.ContainsKey(localShardName) && _shardsResources[localShardName].ContainsKey(resourceId))
                    {
                        IResourceItem imResource = _shardsResources[localShardName][resourceId].Clone() as IResourceItem;

                        foreach (ResourceId resourceIde in resourceItem.SubResources)
                        {
                            if (!_shardsResources[localShardName].ContainsKey(resourceIde))
                            {
                                IResourceItem subResource = _databaseManagers[localShardName].GetResourceSecurityInformatio(MiscUtil.CLUSTERED, resourceIde.Name);
                                if (subResource != null)
                                    _shardsResources[localShardName][resourceIde] = subResource;
                                else
                                    imResource.RemoveSubResource(resourceIde);
                            }
                            if (!imResource.SubResources.Contains(resourceIde) && _shardsResources[localShardName].ContainsKey(resourceIde))
                                imResource.SubResources.Add(resourceIde);
                        }

                        _shardsResources[localShardName][resourceId] = imResource;
                    }
                    else
                    {
                        if (_shardsResources.ContainsKey(localShardName))
                            _shardsResources[localShardName][resourceItem.ResourceId] = resourceItem;
                    }
                }
            }
        }



        public void PopulateSecurityInformation(string shardName, IList<IResourceItem> resources, IList<IUser> users)
        {
            if (_shardsResources.ContainsKey(shardName))
            {
                if (resources != null)
                {
                    foreach (IResourceItem resourceItem in resources)
                    {
                        if (!_shardsResources[shardName].ContainsKey(resourceItem.ResourceId))
                        {
                            _shardsResources[shardName][resourceItem.ResourceId] = new ResourceItem(resourceItem.ResourceId);
                        }
                        _shardsResources[shardName][resourceItem.ResourceId].Merge(resourceItem);
                    }
                }
            }

            foreach(IUser user in users)
            {
                if (_users != null)
                {
                    if(!_users.Contains(user))
                        //CreateUser(shardName, user);
                        _users.Add(user);
                }
            }
        }

        public bool Grant(string localShardName, ResourceId resourceId, string userName, string roleName, string clusterName = null)
        {
            try
            {
                IRole role = Role.GetRoleByName(roleName);
                return Grant(localShardName, resourceId, new User(userName) , role, clusterName);
            }
            catch (ArgumentException exc)
            {
                throw new SecurityException(ErrorCodes.Security.INVALID_ROLE);
            }
        }

        public bool Revoke(string localShardName, ResourceId resourceId, string userName, string roleName, string clusterName = null)
        {
            DCLRole dclRole;
            try
            {
                dclRole = (DCLRole)Enum.Parse(typeof(DCLRole), roleName, true);
                IRole role = null;
                switch (dclRole)
                {
                    case DCLRole.sysadmin:
                        role = Role.sysadmin;
                        break;
                    case DCLRole.securityadmin:
                        role = Role.securityadmin;
                        break;
                    case DCLRole.clusteradmin:
                        role = Role.clusteradmin;
                        break;
                    case DCLRole.clustermanager:
                        role = Role.clustermanager;
                        break;
                    case DCLRole.dbcreator:
                        role = Role.dbcreator;
                        break;
                    case DCLRole.distributor:
                        role = Role.distributor;
                        break;
                    case DCLRole.db_owner:
                        role = Role.db_owner;
                        break;
                    case DCLRole.db_admin:
                        role = Role.db_admin;
                        break;
                    case DCLRole.db_user:
                        role = Role.db_user;
                        break;
                    case DCLRole.db_datawriter:
                        role = Role.db_datawriter;
                        break;
                    case DCLRole.db_datareader:
                        role = Role.db_datareader;
                        break;
                    default:
                        role = Role.configreader;
                        break;
                }
                return Revoke(localShardName, resourceId, new User(userName), role, clusterName);
            }
            catch (ArgumentException exc)
            {
                throw new SecurityException(ErrorCodes.Security.INVALID_ROLE);
            }
        }

        public static bool IsValidPassword(string password)
        {
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (!r.IsMatch(password))
                return false;
            return !string.IsNullOrEmpty(password) && password.Length >= 8 && password.Length < 128;
        }

        public bool CreateUser(string localShardName, IUser userInfo)
        {
            bool isSuccessful = false;
            LoggerManager.Instance.SetThreadContext(new LoggerContext() { ShardName = localShardName != null ? localShardName : "", DatabaseName = "" });
            if (!SSPIUtility.IsValidUser(userInfo.Username))
            {
                SecurityException exc = new SecurityException(ErrorCodes.Security.INVALID_WINDOWS_USER, new string[1] { userInfo.Username });
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.CreateUser() ", exc);
                throw exc;
            }

            userInfo = new User(userInfo.Username);

            if (!_users.Contains(userInfo))
            {
                _users.Add(userInfo);
                if (_databaseManagers.ContainsKey(localShardName))
                {
                    _databaseManagers[localShardName].InsertOrUpdateUserInformation(userInfo);
                }
                if(this.SecurityServer != null && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                    this.SecurityServer.CreateUser(userInfo);
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
                    LoggerManager.Instance.SecurityLogger.Info("SecurityManager.CreateUser", "User: '" + userInfo.Username + "' is successfully created");
                isSuccessful = true;
            }
            else
            {
                SecurityException exc = new SecurityException(ErrorCodes.Security.USER_ALREADY_EXIST, new string[] {userInfo.Username});
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.CreateUser() ", exc);
                throw exc;
            }

            return isSuccessful;
        }

        private bool IsResourceUser(string localShardName, IUser userInfo, out string resourceList)
        {
            resourceList = "";
            bool isResourceUser = false;
            if (_shardsResources.ContainsKey(localShardName))
            {
                IDictionary<ResourceId, IResourceItem> dict = this._shardsResources[localShardName];
                foreach (IResourceItem resourceItem in dict.Values)
                {
                    foreach (IRoleInstance roleInstance in resourceItem.Roles.Values)
                        if (roleInstance.AuthorizedUsers.Contains(userInfo.Username))
                        {
                            resourceList += ("- " + resourceItem.ResourceId.ResourceType + ":" + resourceItem.ResourceId.Name + "\n");
                            isResourceUser = true;
                            break;
                        }
                }
            }
            return isResourceUser;
        }

        public bool DropUser(string localShardName, IUser userInfo)
        {
            bool isSuccessful = false;

            if (_users.Contains(userInfo))
            {
                string resourceList;
                if (IsResourceUser(localShardName, userInfo, out resourceList))
                {
                    SecurityException exc = new SecurityException(ErrorCodes.Security.DATABASE_OR_CLUSTER_USER, new string[] { userInfo.Username, resourceList } );
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                        LoggerManager.Instance.SecurityLogger.Error("SecurityManager.DropUser() ", exc);
                    throw exc;
                }
                _users.Remove(userInfo);
                if (_databaseManagers.ContainsKey(localShardName))
                {
                    _databaseManagers[localShardName].RemoveUserInformation(userInfo.Username);
                }
                if (this.SecurityServer != null && localShardName.Equals(MiscUtil.CONFIGURATION_SHARD_NAME))
                    this.SecurityServer.DropUser(userInfo);
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
                    LoggerManager.Instance.SecurityLogger.Info("SecurityManager.DropUser", "User: '" + userInfo.Username + "' is successfully dropped");
                isSuccessful = true;
            }
            else
            {
                SecurityException exc = new SecurityException(ErrorCodes.Security.NO_USER_EXIST, new string[1] { userInfo.Username });
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.DropUser() ", exc);
                throw exc;
            }

            return isSuccessful;
        }

        public void PublishAuthenticatedUserInfoToDBServer(ISessionId sessionId, string username)
        {
            _logins[sessionId] = new User(username) ;
        }

        public IUser GetAuthenticatedUserInfo(ISessionId sessionId)
        {
            if(_logins.ContainsKey(sessionId))
                return _logins[sessionId];
            return null;
        }

        public IList<IResourceItem> GetDatabaseResources(string localShardName)
        {
            IList<IResourceItem> databaseResources = new List<IResourceItem>();
            if (_databaseManagers.ContainsKey(localShardName))
            {
                foreach (IResourceItem resourceItem in _databaseManagers[localShardName].GetAllResourcesSecurityInformation())
                {
                    if (resourceItem.ResourceId.ResourceType == ResourceType.Database)
                    {
                        IResourceItem resource = new ResourceItem(new ResourceId { Name = resourceItem.ResourceId.Name.Split('/')[1], ResourceType = resourceItem.ResourceId.ResourceType });
                        databaseResources.Add(resource);
                        if (_shardsResources.ContainsKey(localShardName) && !_shardsResources[localShardName].ContainsKey(resourceItem.ResourceId))
                        {
                            _shardsResources[localShardName][resourceItem.ResourceId] = resource;
                        }
                    }
                }
            }
            return databaseResources;
        }

        public IList<IResourceItem> GetClusterResources(string localShardName)
        {
            IList<IResourceItem> databaseResources = new List<IResourceItem>();

            if (_databaseManagers.ContainsKey(localShardName))
            {
                foreach (IResourceItem resourceItem in _databaseManagers[localShardName].GetAllResourcesSecurityInformation())
                {
                    if (resourceItem.ResourceId.ResourceType == ResourceType.Cluster)
                    {
                        IResourceItem resource = new ResourceItem(new ResourceId() { Name = resourceItem.ResourceId.Name.Split('/')[1], ResourceType = resourceItem.ResourceId.ResourceType });
                        databaseResources.Add(resource);
                        if (_shardsResources.ContainsKey(localShardName) && !_shardsResources[localShardName].ContainsKey(resourceItem.ResourceId))
                        {
                            _shardsResources[localShardName][resourceItem.ResourceId] = resource;
                        }
                    }
                }
            }
            return databaseResources;
        }


        public IResourceItem GetResource(string localShardName, string cluster, ResourceId resourceId)
        {
            IResourceItem resourceItemReturn = null;
            if (resourceId.ResourceType == ResourceType.Database)
                resourceId.Name = cluster.ToLower() + "/" + resourceId.Name;
            if (_shardsResources.ContainsKey(localShardName))
            {
                if (_shardsResources[localShardName].ContainsKey(resourceId))
                {
                    resourceItemReturn = _shardsResources[localShardName][resourceId];
                }
                else
                {
                    if (_databaseManagers.ContainsKey(localShardName))
                    {
                        IResourceItem resourceItem = _databaseManagers[localShardName].GetResourceSecurityInformatio(MiscUtil.CLUSTERED, resourceId.Name);
                        if (resourceItem != null)
                        {
                            _shardsResources[localShardName][resourceItem.ResourceId] = resourceItem;
                            resourceItemReturn = resourceItem;
                        }
                    }
                }
            }
            return resourceItemReturn;
        }

        public void SetResource(string localShardName, string cluster, ResourceItem resourceItem)
        {
            ResourceId resourceId = resourceItem.ResourceId;
            ResourceId superResourceId = new ResourceId();
            if (resourceId.ResourceType == ResourceType.Database)
            {
                if (!resourceId.Name.Contains(cluster + "/"))
                {
                    resourceId.Name = cluster + "/" + resourceId.Name;
                    resourceItem.ResourceId = resourceId;
                }

                superResourceId.Name = cluster;
                superResourceId.ResourceType = ResourceType.Cluster;
            }
            if (_shardsResources.ContainsKey(localShardName))
            {
                if (_shardsResources[localShardName].ContainsKey(resourceId))
                { }
                else
                {
                    _shardsResources[localShardName][resourceId] = resourceItem;

                    if (resourceId.ResourceType == ResourceType.Database && !_shardsResources[localShardName][superResourceId].SubResources.Contains(resourceId))
                    {
                        _shardsResources[localShardName][superResourceId].SubResources.Add(resourceId);
                    }
                }
            }
            if (resourceItem.ResourceId.ResourceType == ResourceType.Database)
               GrantRolesToDefaultUsers(localShardName, resourceItem.ResourceId, cluster);
        }

        public IResourceItem GetClusterResource(string localShardName, string clusterName)
        {
            ResourceId resourceId = new ResourceId() { Name = clusterName, ResourceType = ResourceType.Cluster };
            if (_shardsResources.ContainsKey(localShardName))
                if (_shardsResources[localShardName].ContainsKey(resourceId))
                    return _shardsResources[localShardName][resourceId];
            return null;
        }

        public static IDictionary<string, IList<string>> GetUserInfo(IResourceItem resourceItem)
        {
            IDictionary<string, IList<string>> usersRoleSet = null;
            if (resourceItem != null)
            {
                usersRoleSet = new Dictionary<string, IList<string>>();
                foreach (IRole role in resourceItem.Roles.Keys)
                {
                    foreach (string user in resourceItem.Roles[role].AuthorizedUsers)
                    {
                        if (!usersRoleSet.ContainsKey(user))
                            usersRoleSet[user] = new List<string>();
                        usersRoleSet[user].Add(role.RoleName);
                    }
                }
            }
            return usersRoleSet;
        }

        public string GetConnectionInfo(ISessionId sessionId)
        {
            string clientId = String.Empty;
            foreach (KeyValuePair<string, HashSet<ISessionId>> connectionInfo in _connectionInfos)
            {
                clientId = connectionInfo.Key;
                if (connectionInfo.Value.Contains(sessionId))
                    break;
            }
            return clientId;
        }

        public void DisconnectClient(string id)
        {
            if (_connectionInfos.ContainsKey(id))
            {
                HashSet<ISessionId> sessionIds = _connectionInfos[id];
                foreach (var sessionId in sessionIds)
                {
                    if (_logins.ContainsKey(sessionId))
                    {
                        IUser user = null;
                            _logins.TryRemove(sessionId, out user);
                    }
                }
                RemoveConnectionInfo(id);
            }
        }

        #region Helper Methods 

        private IServerAuthenticationCredential AuthenticateWindowsClient(string localShardName, SSPIClientAuthenticationCredential clientCredentials, ISessionId sessionId, bool isLocalClient, string serviceName)
        {
            var serverAuthenticationCredential = new SSPIServerAuthenticationCredential { Token = new AuthToken() };
            var clientAuthToken = clientCredentials.Token;
            string userName = clientCredentials.UserName;
            var nextAuthToken = GetAuthToken(clientAuthToken, sessionId, isLocalClient, ref userName);
            if (nextAuthToken.Status == SecurityStatus.OK || nextAuthToken.Status == SecurityStatus.ContinueNeeded || nextAuthToken.Status == SecurityStatus.SecurityDisabled)
            {
                serverAuthenticationCredential.Token = nextAuthToken;
                if (nextAuthToken.Status == SecurityStatus.OK || nextAuthToken.Status == SecurityStatus.SecurityDisabled)
                {
                    serverAuthenticationCredential.IsAuthenticated = true;
                    if (clientCredentials.RouterSessionId != null)
                    {
                        IClientSessionId clientSessionId = new ClientSessionId();
                        clientSessionId.RouterSessionId = clientCredentials.RouterSessionId;
                        clientSessionId.SessionId = Guid.NewGuid().ToString();
                        serverAuthenticationCredential.SessionId = clientSessionId;
                    }
                    else
                    {
                        ISessionId routerSessionId = new RouterSessionId();
                        routerSessionId.SessionId = sessionId.SessionId;
                        serverAuthenticationCredential.SessionId = routerSessionId;
                    }
                    if (!string.IsNullOrEmpty(serviceName))
                        userName = "NT SERVICE\\" + serviceName;
                    serverAuthenticationCredential.SessionId.Username = userName;
                    LoadAuthorizationInformation(localShardName, new User(userName), sessionId);
                }
                return serverAuthenticationCredential;
            }
            throw new SecurityException(ErrorCodes.Security.INVALID_TOKEN);
        }

        private bool RevokeRolesOnSubResource(string localShardName, ResourceId resourceId, IUser userInfo, string clusterName = null)
        {
            // even if a user is granted role on a single subresource, function returns success
            // TODO: review the logic
            bool isSuccessful = false;
            if (_shardsResources.ContainsKey(localShardName))
            {
                if (_shardsResources[localShardName].ContainsKey(resourceId))
                {
                    IResourceItem superResource = _shardsResources[localShardName][resourceId];

                    foreach (ResourceId subResource in superResource.SubResources)
                    {
                        switch (superResource.ResourceId.ResourceType)
                        {
                            case ResourceType.System:
                                isSuccessful = Revoke(localShardName, subResource, userInfo, Role.clusteradmin, clusterName) || isSuccessful;
                                isSuccessful = Revoke(localShardName, subResource, userInfo, Role.clustermanager, clusterName) || isSuccessful;
                                isSuccessful = Revoke(localShardName, subResource, userInfo, Role.dbcreator, clusterName) || isSuccessful;
                                break;
                            case ResourceType.Cluster:
                                isSuccessful = Revoke(localShardName, subResource, userInfo, Role.db_owner, resourceId.Name) || isSuccessful;
                                isSuccessful = Revoke(localShardName, subResource, userInfo, Role.db_admin, resourceId.Name) || isSuccessful;
                                isSuccessful = Revoke(localShardName, subResource, userInfo, Role.db_user, resourceId.Name) || isSuccessful;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return isSuccessful;
        }

        private bool GrantRolesOnSubResource(string localShardName, ResourceId resourceId, IUser userInfo, string clusterName = null)
        {
            // even if a user is granted role on a single subresource, function returns success
            // TODO: review the logic
            bool isSuccessful = false;
            if (_shardsResources.ContainsKey(localShardName))
            {
                if (_shardsResources[localShardName].ContainsKey(resourceId))
                {
                    IResourceItem superResource = _shardsResources[localShardName][resourceId];

                    foreach (ResourceId subResource in superResource.SubResources)
                    {
                        switch (superResource.ResourceId.ResourceType)
                        {
                            case ResourceType.System:
                                isSuccessful = Grant(localShardName, subResource, userInfo, Role.clusteradmin, clusterName) || isSuccessful;
                                isSuccessful = Grant(localShardName, subResource, userInfo, Role.clustermanager, clusterName) || isSuccessful;
                                isSuccessful = Grant(localShardName, subResource, userInfo, Role.dbcreator, clusterName) || isSuccessful;
                                break;
                            case ResourceType.Cluster:
                                isSuccessful = Grant(localShardName, subResource, userInfo, Role.db_owner, resourceId.Name) || isSuccessful;
                                isSuccessful = Grant(localShardName, subResource, userInfo, Role.db_admin, resourceId.Name) || isSuccessful;
                                isSuccessful = Grant(localShardName, subResource, userInfo, Role.db_user, resourceId.Name) || isSuccessful;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return isSuccessful;
        }

        private bool IsSecurityEnabled
        {
            get;
            set;
        }
        
        private AuthToken GetAuthToken(AuthToken clientAuthToken, ISessionId sessionId, bool isLocalClient, ref string username)
        {
            username = String.Empty;
            var nextAuthToken = new AuthToken();
            Byte[] nextToken;

            if (!IsSecurityEnabled)
            {
                nextAuthToken = new AuthToken { Status = SecurityStatus.SecurityDisabled };
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                {
                    LoggerManager.Instance.SecurityLogger.Debug("SecurityManager.GetAuthToken() ","Security is disabled. Authentication skipped.");
                }
                return nextAuthToken;
            }
            ServerContext serverContext = GetContext(clientAuthToken.Token, sessionId, false);
            //ServerContext serverContext = GetContext(sessionId, isLocalClient, false);
            try
            {
                nextAuthToken.Status = serverContext.AcceptToken(clientAuthToken.Token, out nextToken);
            }
            catch (InvalidOperationException exc)
            {
                //serverContext = GetContext(sessionId, isLocalClient, true);
                serverContext = GetContext(clientAuthToken.Token, sessionId, true);
                nextAuthToken.Status = serverContext.AcceptToken(clientAuthToken.Token, out nextToken);
            }
            catch (SSPIException exc)
            {
                throw new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { exc.Message });
            }
            nextAuthToken.Token = nextToken;
            int errorCode = -5;
            string[] errorArguments = null;
            if (nextAuthToken.Status != SecurityStatus.ContinueNeeded &&
                nextAuthToken.Status != SecurityStatus.OK)
            {
                switch (nextAuthToken.Status)
                {
                    case SecurityStatus.IncompleteMessage:
                    case SecurityStatus.IncompleteCredentials:
                    case SecurityStatus.BufferNotEnough:
                    case SecurityStatus.WrongPrincipal:
                    case SecurityStatus.TimeSkew:
                    case SecurityStatus.UntrustedRoot:
                    case SecurityStatus.IllegalMessage:
                    case SecurityStatus.CertUnknown:
                    case SecurityStatus.CertExpired:
                    case SecurityStatus.AlgorithmMismatch:
                    case SecurityStatus.SecurityQosFailed:
                    case SecurityStatus.SmartcardLogonRequired:
                    case SecurityStatus.UnsupportedPreauth:
                    case SecurityStatus.BadBinding:
                        errorCode = ErrorCodes.Security.CERTIFICATE_ERROR;
                        break;
                    case SecurityStatus.NoAuthenticatingAuthority:
                        errorCode = ErrorCodes.Security.NO_AUTHENTICATING_AUTHROITY;
                        break;
                    case SecurityStatus.TargetUnknown:
                        errorCode = ErrorCodes.Security.TARGET_UNKNOWN;
                        break;
                    case SecurityStatus.NoCredentials:
                        errorCode = ErrorCodes.Security.NO_CREDENTIALS;
                        errorArguments = new string[] { username };
                        break;
                    default:
                        errorCode = ErrorCodes.Security.UNKNOWN_ISSUE;
                        break;
                }
                SecurityException securityException = new SecurityException(errorCode, errorArguments);
                if (LoggerManager.Instance.SecurityLogger != null &&
                    LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                {
                    LoggerManager.Instance.SecurityLogger.Error("SecurityManager.GetAuthToken() ", securityException);
                }
                throw securityException;
            }
            if (nextAuthToken.Status == SecurityStatus.OK)
                username = serverContext.ContextUserName;
            return nextAuthToken;
        }

        private ServerContext GetContext(byte[] token, ISessionId sessionId, bool newContext)
        {
            string strUTF8 = System.Text.Encoding.UTF8.GetString(token);
            if (strUTF8.Contains("NTLMSSP"))
            {
                if (sessionId != null && (!_serverContexts.ContainsKey(sessionId) || newContext))
                    _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.LocalServerCredential);
            }
            else
            {
                if (sessionId != null && (!_serverContexts.ContainsKey(sessionId) || newContext))
                    _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.RemoteServerCredential);
            }
            if (_serverContexts.ContainsKey(sessionId))
                return _serverContexts[sessionId];
            else throw new SecurityException(ErrorCodes.Security.UNKNOWN_ISSUE);
        }

        private ServerContext GetContext(ISessionId sessionId, bool isLocalClient, bool newContext)
        {
            if (isLocalClient)
            {
                if (!_serverContexts.ContainsKey(sessionId) || newContext)
                    _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.LocalServerCredential);
                //else if(newContext)
                //    _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.LocalServerCredential);
            }
            else
            {
                if (IsRemoteServer)
                {
                    if (!_serverContexts.ContainsKey(sessionId) || newContext)
                        _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.RemoteServerCredential);
                    //else if (newContext)
                    //    _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.RemoteServerCredential);
                }
                else
                {
                    if (!_serverContexts.ContainsKey(sessionId) || newContext)
                        _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.LocalServerCredential);
                    //else if (newContext)
                    //    _serverContexts[sessionId] = SSPIUtility.GetServerContext(SSPIUtility.LocalServerCredential);
                }
            }

            if (_serverContexts.ContainsKey(sessionId))
                return _serverContexts[sessionId];
            else throw new SecurityException(ErrorCodes.Security.UNKNOWN_ISSUE);
        }

        private void AddConnectionInfo(Address serverAddress, ISessionId sessionId)
        {
            string clientId = MiscUtil.GetAddressInfo(serverAddress.IpAddress, serverAddress.Port);
            if (!_connectionInfos.ContainsKey(clientId))
            {
                var sessionIds = new HashSet<ISessionId> { sessionId };
                _connectionInfos.Add(clientId, sessionIds);
            }
            else
            {
                var sessionIds = _connectionInfos[clientId];
                if (!sessionIds.Contains(sessionId))
                    sessionIds.Add(sessionId);
            }
        }

        private void RemoveConnectionInfo(string serverAddress)
        {
            if (_connectionInfos.ContainsKey(serverAddress))
                _connectionInfos.Remove(serverAddress);
        }

        #endregion


        public void OnChannelDisconnected(ISessionId sessionId)
        {
            if (_logins.ContainsKey(sessionId))
            {
                IUser user = null;
                _logins.TryRemove(sessionId, out user);
            }

        }

        public IDictionary<IRole, IList<ResourceId>> GetUserInfo(string localShardName, IUser user)
        {
            string username = user.Username;
            IDictionary<IRole, IList<ResourceId>> roles = new Dictionary<IRole, IList<ResourceId>>();
            foreach (IResourceItem resourceItem in _shardsResources[localShardName].Values)
            {
                foreach (IRole role in resourceItem.Roles.Keys)
                {
                    if (resourceItem.Roles[role].AuthorizedUsers.Contains(username, StringComparer.CurrentCultureIgnoreCase))
                    {
                        if (!roles.ContainsKey(role))
                            roles[role] = new List<ResourceId>();
                        roles[role].Add(resourceItem.ResourceId);
                    }
                }
            }
            return roles;
        }


        public IUser GetUser(ISessionId sessionId)
        {
            if (_logins.ContainsKey(sessionId))
                return _logins[sessionId];
            return null;
        }
    }
}
