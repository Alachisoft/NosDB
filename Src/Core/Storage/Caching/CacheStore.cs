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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Operations;
using Alachisoft.NosDB.Core.Storage.Providers;
using Alachisoft.NosDB.Core.Storage.Providers.LMDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.JSON;

namespace Alachisoft.NosDB.Core.Storage.Caching
{
    public class CacheStore : Alachisoft.NosDB.Core.Queries.IQueryable, IEnumerable<KeyValuePair<long, JSONDocument>>
    {
        private IDictionary<DocumentKey, CacheItem> _documentCache = new HashVector<DocumentKey,CacheItem>();
        private StorageManagerBase _storageManager;
        private BaseCollection _parent;

        public CacheStore(BaseCollection parent)
        {
            _parent = parent;
        }


        public bool Initialize(StorageConfiguration config)
        {

            return false;
 
        }

        public InsertResult<JSONDocument> AddDocument(JSONDocument document)
        {
            InsertResult<JSONDocument> result = new InsertResult<JSONDocument>();
            CacheItem item;
            if (_documentCache.TryGetValue(new DocumentKey(document.Key), out item))
            {
                result.RowId = item.Metadata.RowId;
                result.DocumentExists = true;
                return result;
            }
            CacheItem citem = new CacheItem();
            citem.Document = document;
            citem.IsDocumentDirty = true;
            citem.Metadata = new KeyMetadata(_storageManager.GenerateRowId(_parent.Name), -1);
            citem.IsMetadataDirty = true;
            _documentCache.Add(new DocumentKey(document.Key), citem);
            return result;
        }

        public UpdateResult<JSONDocument> UpdateDocument(JSONDocument update)
        {
            UpdateResult<JSONDocument> result = new UpdateResult<JSONDocument>();
            CacheItem item;
            if (_documentCache.TryGetValue(new DocumentKey(update.Key), out item))
            {
                result.OldDocument =item.Document.Clone() as JSONDocument;
                JsonDocumentUtil.Update(item.Document, update);                
                //_documentCache[update.Key].Document.Update(update);
                result.RowId = item.Metadata.RowId;
                result.NewDocument = item.Document.Clone() as JSONDocument;
            }
            return result; 
        }

        public DeleteResult<JSONDocument> DeleteDocument(DocumentKey key)
        {
            DeleteResult<JSONDocument> result = new DeleteResult<JSONDocument>();
            CacheItem item;
            if (_documentCache.TryGetValue(key, out item))
            {
                result.Document = item.Document;
                result.RowId = item.Metadata.RowId;
                _documentCache.Remove(key);
            }
            return result; 
 
        }

        public GetResult<JSONDocument> GetDocument(DocumentKey key)
        {
            GetResult<JSONDocument> result = new GetResult<JSONDocument>();
            CacheItem item;
            if (_documentCache.TryGetValue(key, out item))
            {
                result.Document = item.Document;
                result.RowId = item.Metadata.RowId;
            }
            return result;
        }

        public IDataReader<TKey, TValue> ExecuteQuery<TKey, TValue>(string query, System.Collections.IDictionary parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery<TKey, TValue>(string query, System.Collections.IDictionary parameters)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string query, System.Collections.IDictionary parameters)
        {
            throw new NotImplementedException();
        }

        private class CacheItem
        {
            private JSONDocument _document = null;
            private bool _documentDirty = false;
            
            private KeyMetadata _keyMetaData = null;
            private bool _metadataDirty = false;

            public KeyMetadata Metadata
            {
                get { return _keyMetaData; }
                set { _keyMetaData = value; }
            }

            public bool IsMetadataDirty
            {
                get { return _metadataDirty; }
                set { _metadataDirty = value; }
            }

            public JSONDocument Document
            {
                get { return _document; }
                set { _document = value; }
            }

            public bool IsDocumentDirty
            {
                get { return _documentDirty; }
                set { _documentDirty = value; }
            }
        }

        public IEnumerator<KeyValuePair<long, JSONDocument>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }



}
