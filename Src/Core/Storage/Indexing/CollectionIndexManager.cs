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
using System;
using System.Collections.Concurrent;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Server.Engine;
using System.Collections.Generic;
using System.Linq;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class CollectionIndexManager : IIndexProvider, IDisposable
    {
        private readonly ConcurrentDictionary<string, IIndex> _indexes =
            new ConcurrentDictionary<string, IIndex>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, string> _attributeToNameReference;
        private BPlusPersistanceManager persistanceManager;
        private IEnumerable<KeyValuePair<long, JSONDocument>> _store;
        private string _collectionName;

        private readonly DatabaseContext _databaseContext;

        public CollectionIndexManager(DatabaseContext context, BPlusPersistanceManager persister, IEnumerable<KeyValuePair<long, JSONDocument>> store, string collectionName)
        {
            _databaseContext = context;
            persistanceManager = persister;
            _attributeToNameReference = new ConcurrentDictionary<string, string>();
            _store = store;
            _collectionName = collectionName;
        }

        public OrderedList<int, IIndex> OrderedIndexList
        {
            get
            {
                var list = new OrderedList<int, IIndex>();
                foreach (var kvp in _indexes)
                {
                    list.Add(1, kvp.Value);
                }
                return list;
            }
        }

        public bool Initialize(Indices indices)
        {
            if (indices == null)
                return true;
            var databaseDirectory = _databaseContext.DatabaseConfigurations.Storage.StorageProvider.DatabasePath + _databaseContext.DatabaseName;
            foreach (var config in indices.IndexConfigurations.Values)
            {
                IIndex index = new BPlusIndex(config, _collectionName, databaseDirectory, this);
                
                if (_attributeToNameReference.TryAdd(index.IndexKey.ToString(), index.Name))
                {
                    try
                    {
                        index.Initialize();
                        if (!_indexes.TryAdd(index.Name, index))
                        {
                            string removedAttribs;
                            _attributeToNameReference.TryRemove(index.IndexKey.ToString(), out removedAttribs);
                            index.Dispose();
                            if (LoggerManager.Instance.IndexLogger != null)
                            {
                                LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                                    "Failed to initialize index: " + index.Name + ", an index with name " +
                                    index.Name + "already exists. ");
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        if (LoggerManager.Instance.IndexLogger != null)
                        {
                            LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                                "Failed to initialize index: " + index.Name + ", Corruption detected, " +ex.Message+ 
                                index.Name + ", regenerating index... ");
                        }
                        string removedAttribs;
                        _attributeToNameReference.TryRemove(index.IndexKey.ToString(), out removedAttribs);
                        index.Destroy();
                        RecreateIndex(config);
                        continue;
                    }
                }
                else
                {
                    if (LoggerManager.Instance.IndexLogger != null)
                    {
                        LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                            "Failed to initialize index: " + index.Name + ", an index on attributes " +
                            index.Attributes.ToString() + "already exists. ");
                    }
                }
                index.IsFunctional = true;
                persistanceManager.RegisterPersister((BPlusIndex)index);
            }
            
            return true;
        }

        public void UpdateIndex(long rowId, IJSONDocument oldDocument, IJSONDocument newDocument, long operationId)
        {
            foreach (var index in _indexes.Values)
            {
                AttributeValue[] newAttributeValue, oldAttributeValue;
                bool oldExists = index.IndexKey.TryGetValue(oldDocument, out oldAttributeValue);
                bool newEntry = index.IndexKey.TryGetValue(newDocument, out newAttributeValue);
                if (newEntry)
                {
                    if (oldExists)
                    {
                        foreach (var attributeValue in oldAttributeValue)
                        {
                            index.Remove(attributeValue, rowId, operationId);
                        }
                        foreach (var attributeValue in newAttributeValue)
                        {
                            index.Add(attributeValue, rowId, operationId);
                        }
                    }
                    else
                    {
                        foreach (var attributeValue in newAttributeValue)
                        {
                            index.Add(attributeValue, rowId, operationId);
                        }
                    }
                }
                else
                {
                    if (oldExists)
                    {
                        foreach (var attributeValue in oldAttributeValue)
                        {
                            index.Remove(attributeValue, rowId, operationId);
                        }
                    }
                }
            }
        }

        public void AddToIndex(long rowId, IJSONDocument document, long operationId)
        {
            foreach (IIndex index in _indexes.Values)
            {
                AttributeValue[] attribValue;
                if (index.IndexKey.TryGetValue(document, out attribValue))
                {
                    foreach (var attributeValue in attribValue)
                    {
                        index.Add(attributeValue, rowId, operationId);
                    }
                }
            }
        }

        public IIndex GetIndexStore(string indexName)
        {
            var index = _indexes[indexName];
            if (!index.IsFunctional)
                return null;
            return index;
        }

        public void CreateIndex(IndexConfiguration indexConfig)
        {
            var databaseDirectory = _databaseContext.DatabaseConfigurations.Storage.StorageProvider.DatabasePath +
                                    _databaseContext.DatabaseName;

            if (_indexes.ContainsKey(indexConfig.Name))
            {
                throw new IndexException(ErrorCodes.Indexes.INDEX_ALREADY_DEFINED, new[] {indexConfig.Name});
            }

            IIndex index = new BPlusIndex(indexConfig, _collectionName, databaseDirectory, this);

            if (!_attributeToNameReference.TryAdd(index.IndexKey.ToString(), index.Name))
                throw new IndexException(ErrorCodes.Indexes.INDEX_ALREADY_DEFINED_FOR_ATTRIBUTES,
                    new[] {index.IndexKey.ToString()});

            index.Initialize();

            if (_store != null)
            {
                if (!_indexes.TryAdd(index.Name, index))
                {
                    index.Destroy();
                    if (LoggerManager.Instance.IndexLogger != null)
                        LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                            "Failed to initialize index: " + index.Name + ", an index with name " +
                                index.Name + "already exists. ");
                }

                PopulateIndex(index, _store);
            }
        }


        public void RecreateIndex(IndexConfiguration indexConfig)
        {
            var databaseDirectory = _databaseContext.DatabaseConfigurations.Storage.StorageProvider.DatabasePath +
                                   _databaseContext.DatabaseName;

            IIndex removedIndex;
            if (_indexes.TryRemove(indexConfig.Name, out removedIndex))
            {
                string attributes;
                _attributeToNameReference.TryRemove(removedIndex.IndexKey.ToString(), out attributes);
            }

            IIndex index = new BPlusIndex(indexConfig, _collectionName, databaseDirectory, this);

            if (!_attributeToNameReference.TryAdd(index.IndexKey.ToString(), index.Name))
            {
                _indexes.TryAdd(removedIndex.Name, removedIndex);
                _attributeToNameReference.TryAdd(removedIndex.IndexKey.ToString(), removedIndex.Name);
                throw new IndexException(ErrorCodes.Indexes.INDEX_ALREADY_DEFINED_FOR_ATTRIBUTES,
                    new[] {index.IndexKey.ToString()});
            }

            if (removedIndex != null)
                removedIndex.Destroy();
            index.Initialize();

            if (_store != null)
            {
                if (!_indexes.TryAdd(index.Name, index))
                {
                    index.Destroy();
                    if (LoggerManager.Instance.IndexLogger != null)
                        LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                            "Failed to initialize index: " + index.Name + ", an index with name " +
                                index.Name + "already exists. ");
                }

                PopulateIndex(index, _store);
            }

        }

        public void RenameIndex(string oldName, string newName)
        {
            IIndex index;
            if (_indexes.TryRemove(oldName, out index))
            {
                if (!_indexes.TryAdd(newName, index))
                {
                    _indexes.TryAdd(oldName, index);
                    throw new IndexException(ErrorCodes.Indexes.INDEX_ALREADY_DEFINED, new[] {newName});
                }
                _attributeToNameReference[index.IndexKey.ToString()] = newName;
                index.Name = newName;
                return;
            }
            throw new IndexException(ErrorCodes.Indexes.INDEX_DOESNOT_EXIST, new[] {oldName});

        }

        public void DropIndex(string indexName)
        {
            IIndex index;
            if (_indexes.TryRemove(indexName, out index))
            {
                _attributeToNameReference.TryRemove(index.IndexKey.ToString(), out indexName);
                index.Destroy();
                return;
            }
            throw new IndexException(ErrorCodes.Indexes.INDEX_DOESNOT_EXIST, new[] {indexName});
        }

        private void PopulateIndex(IIndex index, IEnumerable<KeyValuePair<long, JSONDocument>> store)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var enumerator = store.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    AttributeValue[] value;
                    if (!index.IndexKey.TryGetValue(enumerator.Current.Value, out value)) continue;
                    foreach (var attributeValue in value)
                    {
                        index.Add(attributeValue, enumerator.Current.Key, -1);
                    }
                }
                ((IBPlusPersister)index).PersistAndCommitAll();
                index.IsFunctional = true;
                persistanceManager.RegisterPersister((BPlusIndex)index);
                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Debug("BPlusIndex","Index "+index.Name+" population complete, it is now available and ready to use");
                }
            });
        }

        public void Commit()
        {
            foreach (var kvp in _indexes)
            {
                kvp.Value.Commit(null);
            }
        }

        public void Rollback()
        {
            foreach (var kvp in _indexes)
            {
                kvp.Value.Rollback(null);
            }
        }

        //#MarkedForDeletion
        public List<IIndex> GetIndexes(string attribute)
        {
            return GetIndexes(new[] {attribute});
        }

        //#MarkedForDeletion
        public List<IIndex> GetIndexes(string[] attributes)
        {
            var indexList = new List<IIndex>();
            foreach (var kvp in _indexes)
            {
                var list = new List<string>();
              
                    list.Add(kvp.Value.Attributes.Name);
                    if (ArraysEqual(attributes, list.ToArray()))
                        if (kvp.Value.IsFunctional)
                            indexList.Add(kvp.Value);
                
            }
            return indexList.Count != 0 ? indexList : null;
        }

        //#MarkedForDeletion
        private static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            return !a1.Where((t, i) => !comparer.Equals(t, a2[i])).Any();
        }

        public void Destroy()
        {
            foreach (var index in _indexes.Values)
            {
                try
                {
                    index.Destroy();
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.IndexLogger != null)
                        LoggerManager.Instance.IndexLogger.Error("CollectionIndexManager",
                            "Faild to destroy index: " + index.Name + ", " + ex);
                }
            }
        }

        public void Dispose()
        {

            foreach (var index in _indexes.Values)
            {
                try
                {
                    index.Dispose();
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.IndexLogger != null)
                        LoggerManager.Instance.IndexLogger.Error("CollectionIndexManager",
                            "Faild to disposed index: " + index.Name + ", " + ex);
                }
            }
        }
    }
}
