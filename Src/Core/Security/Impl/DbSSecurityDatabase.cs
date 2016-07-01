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
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Server;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Core.Security.Interfaces;
using Alachisoft.NosDB.Core.Storage;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Security.Impl
{
    public class DbSSecurityDatabase : ISecurityDatabase
    {
        private bool _isInitialized = false;
        //private JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
        //private JsonSerializer<IUser> userSerializer = new JsonSerializer<IUser>();
        //private JsonSerializer serializer = new JsonSerializer();
        //private IDatabasesManager _store;
        private IDatabaseStore _store;

        internal void Initialize(IDatabasesManager store)
        {
            _store = store.GetDatabase(MiscUtil.SYSTEM_DATABASE);
            _isInitialized = true;
        }

        public bool IsInitialized { get { return _isInitialized; } }

        public Common.Security.Interfaces.IResourceItem[] GetAllResourcesSecurityInformation()
        {
            ICollectionStore collection = ((SystemDatabaseStore)_store).Collections[Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection];
            IResourceItem[] resources = new IResourceItem[collection.Count()];
            int i = 0;
            foreach (JSONDocument doc in collection)
            {
                resources[i] = JsonSerializer.Deserialize<IResourceItem>(doc);
                i++;
            }
            return resources;
        }

        public Common.Security.Interfaces.IResourceItem GetResourceSecurityInformatio(string cluster, string resource)
        {
            IResourceItem resourceItem = null;
            JSONDocument doc = new JSONDocument();
            bool found = FindDocument(resource, Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection, out doc);
            if (found)
            {
                resourceItem = JsonSerializer.Deserialize<IResourceItem>(doc);
            }
            return resourceItem;
        }

        public void InsertOrUpdateResourceSecurityInformation(string cluster, Common.Security.Interfaces.IResourceItem resourceItem)
        {
            lock (_store)
            {
                IDocumentsWriteOperation insertOperation = new InsertDocumentsOperation();
                //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument jdoc = new JSONDocument();
                bool found = false;
                if (resourceItem != null)
                {
                    //jdoc.Key = new DocumentKey(configuration.Name);
                    found = FindDocument(resourceItem.ResourceId.Name, Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection, out jdoc);
                    if (found)
                    {
                        jdoc = JsonSerializer.Serialize<IResourceItem>(resourceItem);
                        jdoc.Key = resourceItem.ResourceId.Name;
                        IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                        replaceOperation.Collection = Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection;
                        replaceOperation.Database = MiscUtil.SYSTEM_DATABASE;
                        jsonDocuments.Add(jdoc);
                        replaceOperation.Documents = jsonDocuments;
                        _store.ReplaceDocuments(replaceOperation);
                        //TODO for updating document only deleting previous document require some time to wait In Future this operation done with replace operation.
                    }
                    else
                    {
                        jsonDocuments.Clear();
                        jdoc = JsonSerializer.Serialize<IResourceItem>(resourceItem);
                        jdoc.Key = resourceItem.ResourceId.Name;
                        jsonDocuments.Add(jdoc);
                        insertOperation.Documents = jsonDocuments;
                        insertOperation.Collection = Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection;
                        insertOperation.Database = MiscUtil.SYSTEM_DATABASE;
                        _store.InsertDocuments(insertOperation);
                    }

                }
            }
        }

        public void RemoveResourceSecurityInformation(string cluster, string resource)
        {
            lock (_store)
            {
                //JsonSerializer<IResourceItem> serializer = new JsonSerializer<IResourceItem>();
                ICollectionStore collection = ((SystemDatabaseStore)_store).Collections[Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (resource != null)
                {
                    doc.Key = resource;
                    found = FindDocument(resource, Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection, out doc);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.SecurityInformationCollection;
                        deleteOperation.Database = MiscUtil.SYSTEM_DATABASE;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _store.DeleteDocuments(deleteOperation);
                    }
                }
            }
        }

        public bool FindDocument(string name, string colname, out JSONDocument doc)
        {
            bool found = false;
            IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
            doc = new JSONDocument();

            if (name != null)
            {
                doc.Key = name.ToLower();
                jsonDocuments.Add(doc);
                IGetOperation getOperation = new GetDocumentsOperation();
                getOperation.Database = MiscUtil.SYSTEM_DATABASE;
                getOperation.Collection = colname;
                getOperation.DocumentIds = jsonDocuments;
                IGetResponse response = _store.GetDocuments(getOperation);
                IDataChunk dataChunk = response.DataChunk;
                if (dataChunk.Documents.Count != 0)
                {
                    doc = dataChunk.Documents[0] as JSONDocument;
                    found = true;
                }
                else
                {
                    doc = null;
                    found = false;
                }
            }
            return found;
        }


        public IUser[] GetAllUserInformation()
        {
            ICollectionStore collection = ((SystemDatabaseStore)_store).Collections[Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection];
            IUser[] users = new IUser[collection.Count()];
            int i = 0;
            foreach (JSONDocument doc in collection)
            {
                users[i] = JsonSerializer.Deserialize<IUser>(doc);
                i++;
            }
            return users;
        }

        public IUser GetUserInformatio(string user)
        {
            IUser resourceItem = null;
            JSONDocument doc = new JSONDocument();
            bool found = FindDocument(user, Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection, out doc);
            if (found)
            {
                resourceItem = JsonSerializer.Deserialize<IUser>(doc);
            }
            return resourceItem;
        }

        public void InsertOrUpdateUserInformation(IUser userInfo)
        {
            lock (_store)
            {
                IDocumentsWriteOperation insertOperation = new InsertDocumentsOperation();
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument jdoc = new JSONDocument();
                bool found = false;
                if (userInfo != null)
                {
                    //jdoc.Key = new DocumentKey(configuration.Name);
                    found = FindDocument(userInfo.Username, Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection, out jdoc);
                    if (found)
                    {
                        jdoc = JsonSerializer.Serialize<IUser>(userInfo);
                        IDocumentsWriteOperation replaceOperation = new ReplaceDocumentsOperation();
                        replaceOperation.Collection = Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection;
                        replaceOperation.Database = MiscUtil.SYSTEM_DATABASE;
                        jsonDocuments.Add(jdoc);
                        replaceOperation.Documents = jsonDocuments;
                        _store.ReplaceDocuments(replaceOperation);
                        //TODO for updating document only deleting previous document require some time to wait In Future this operation done with replace operation.
                    }
                    else
                    {
                        jsonDocuments.Clear();
                        jdoc = JsonSerializer.Serialize<IUser>(userInfo);
                        jdoc.Key = userInfo.Username;
                        jsonDocuments.Add(jdoc);
                        insertOperation.Documents = jsonDocuments;
                        insertOperation.Collection = Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection;
                        insertOperation.Database = MiscUtil.SYSTEM_DATABASE;
                        _store.InsertDocuments(insertOperation);
                    }

                }
            }
        }

        public void RemoveUserInformation(string username)
        {
            lock (_store)
            {
                ICollectionStore collection = ((SystemDatabaseStore)_store).Collections[Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection];
                IList<IJSONDocument> jsonDocuments = new List<IJSONDocument>();
                JSONDocument doc = new JSONDocument();
                bool found = false;
                if (username != null)
                {
                    doc.Key = username;
                    found = FindDocument(username, Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection, out doc);
                    if (found)
                    {
                        IDocumentsWriteOperation deleteOperation = new DeleteDocumentsOperation();
                        deleteOperation.Collection = Alachisoft.NosDB.Core.Util.MiscUtil.SystemCollection.UserInformationCollection;
                        deleteOperation.Database = MiscUtil.SYSTEM_DATABASE;
                        jsonDocuments.Add(doc);
                        deleteOperation.Documents = jsonDocuments;
                        _store.DeleteDocuments(deleteOperation);
                    }
                }
            }
        }


        public IRole[] GetAllRolesInformation()
        {
            throw new NotImplementedException();
        }

        public IRole GetRoleInformatio(string name)
        {
            throw new NotImplementedException();
        }

        public void InsertOrUpdateRoleInformation(IRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoleInformation(string name)
        {
            throw new NotImplementedException();
        }
    }
}
