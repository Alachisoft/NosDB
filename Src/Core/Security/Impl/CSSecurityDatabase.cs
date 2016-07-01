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
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common;

namespace Alachisoft.NosDB.Core.Security.Impl
{
    public class CSSecurityDatabase : ISecurityDatabase
    {
        private bool _isInitialized;
        private ConfigurationStore _store;
        
        internal void Initialize(ConfigurationStore store)
        {
            _store = store;
            _isInitialized = true;
        }

        public bool IsInitialized { get { return _isInitialized; } }

        public IResourceItem[] GetAllResourcesSecurityInformation()
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(MiscUtil.CLUSTERED);
            IResourceItem[] resourceItems = _store.GetAllResourcesSecurityInformation();
            return resourceItems;
        }

        public IResourceItem GetResourceSecurityInformatio(string cluster, string resource)
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(cluster);
            IResourceItem resourceItem = transaction.GetResourceSecurityInformation(resource);
            return resourceItem;
        }

        public void InsertOrUpdateResourceSecurityInformation(string cluster, IResourceItem resourceItem)
        {
            resourceItem.ClusterName = cluster;
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(cluster);
            transaction.InsertOrUpdateResourceSecurityInformation(resourceItem);
            _store.CommitTransaction(transaction);
        }

        public void RemoveResourceSecurityInformation(string cluster, string resource)
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(cluster);
            transaction.RemoveResourceSecurityInformation(resource);
            _store.CommitTransaction(transaction);
        }


        public IUser[] GetAllUserInformation()
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(MiscUtil.CLUSTERED);
            IUser[] logins = _store.GetAllUserInformation();
            return logins;

        }

        public IUser GetUserInformatio(string user)
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(MiscUtil.CLUSTERED);
            IUser login = transaction.GetUserInformation(user);
            return login;
        }

        public void InsertOrUpdateUserInformation(IUser userInfo)
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(MiscUtil.CLUSTERED);
            transaction.InsertOrUpdateUserInformation(userInfo);
            _store.CommitTransaction(transaction);
        }

        public void RemoveUserInformation(string username)
        {
            ConfigurationStore.Transaction transaction = _store.BeginTransaction(MiscUtil.CLUSTERED);
            transaction.RemoveUserInformation(username);
            _store.CommitTransaction(transaction);
        }

        //these methods are not currently being used as they belong to custom roles, we are not supporting yet
        //handling their scenarios is difficult before implementing these, these will be implemented using transactions later
        #region Roles Information
        public IRole[] GetAllRolesInformation()
        {
            return _store.GetAllRolesInformation();
        }

        public IRole GetRoleInformatio(string name)
        {
            return _store.GetRoleInformatio(name);
        }

        public void InsertOrUpdateRoleInformation(IRole roleInfo)
        {
            _store.InsertOrUpdateRoleInformation(roleInfo);
        }

        public void RemoveRoleInformation(string name)
        {
            _store.RemoveRoleInformation(name);
        }
        #endregion
    }
}
