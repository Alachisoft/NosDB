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
using System.IO;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Core.Util;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using CSharpTest.Net.Synchronization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class BPlusIndex : IIndex, TimeScheduler.Task, IBPlusPersister
    {
        protected readonly IndexConfiguration _configuration;

        protected BPlusTree<AttributeValue, long> _tree;

        protected BPlusTree<AttributeValue, IndexOp<long>> _transitionTree;

        protected BoundingBox _bounds;
        protected IndexKey _indexKey;

        protected IDictionary<long,IList<IndexOp<long>>> opsToCommit;

        protected ClusteredList<long> appliedOps;

        
        protected string _path;
        protected SortOrder order;
        protected readonly object disposeLock = new object();

        private bool _isRunning;
        private object _isRunnigLock = new object();
        private bool _runAgain = false;

        private CollectionIndexManager _parent;

        public BPlusIndex(IndexConfiguration configurations, string collectionName, string path, CollectionIndexManager parent)
        {
            _configuration = configurations; //configurations.Clone() as IndexConfiguration;

            _indexKey = new UniIndexKey(_configuration.Attributes);
            
            order = _configuration.Attributes.SortOrder;

            _path = DirectoryUtil.GetIndexPath(path, collectionName, _configuration.IndexName);
            _parent = parent;
        }

        public virtual void Initialize()
        {
            #region Main Tree Initialization

            var treeOptions = new BPlusTree<AttributeValue, long>.OptionsV2(AttributeValueSerializer.Global,
                new PrimitiveSerializer());

            if (_configuration.JournalEnabled)
            {
                var transactionLogOptions = new TransactionLogOptions<AttributeValue, long>(_path + ".tlog",
                    new AttributeValueSerializer(), new PrimitiveSerializer());
                transactionLogOptions.FileOptions = FileOptions.WriteThrough;
                transactionLogOptions.FileBuffer = 4096;
                treeOptions.TransactionLog = new TransactionLog<AttributeValue, long>(transactionLogOptions);
                treeOptions.TransactionLogFileName = transactionLogOptions.FileName;
                treeOptions.StoragePerformance = StoragePerformance.LogFileNoCache;
            }
            else
            {
                treeOptions.StoragePerformance = StoragePerformance.LogFileInCache;
            }

            if (_configuration.CachePolicy != null)
            {
                switch (_configuration.CachePolicy.ToLower())
                {
                    case "all":
                        treeOptions.CachePolicy = CachePolicy.All;
                        break;
                    case "none":
                        treeOptions.CachePolicy = CachePolicy.None;
                        break;
                    default:
                        treeOptions.CachePolicy = CachePolicy.Recent;
                        break;
                }
            }
            else
                treeOptions.CachePolicy = CachePolicy.Recent;

            treeOptions.FileName = _path;
            treeOptions.StorageType = StorageType.Disk;
            treeOptions.CreateFile = CreatePolicy.IfNeeded;
            treeOptions.BTreeOrder = 64;
            treeOptions.LockingFactory = new IgnoreLockFactory();
            treeOptions.CallLevelLock = new IgnoreLocking();
            treeOptions.StoragePerformance = StoragePerformance.Default;

            try
            {
                _tree = new BPlusTree<AttributeValue, long>(treeOptions);
                _tree.EnableCount();

                if (LoggerManager.Instance.IndexLogger != null && LoggerManager.Instance.IndexLogger.IsInfoEnabled)
                {
                    LoggerManager.Instance.IndexLogger.Info("BPlusIndex",
                        "Index (s) " + _indexKey.ToString() + " defined");
                }
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                        "Error: " + ErrorCodes.Indexes.TREE_INITIALIZATION_FAILURE +
                        " - Failed to initialize Index for attribute(s) " + _indexKey.ToString() + Environment.NewLine + ex.ToString());
                    throw new IndexException(ErrorCodes.Indexes.TREE_INITIALIZATION_FAILURE);
                }

            }

            #endregion

            #region Transitory Tree Initialization

            var _transitoryTreeOps =
                new BPlusTree<AttributeValue, IndexOp<long>>.Options(AttributeValueSerializer.Global,
                    IndexOpSerializer<long>.Global);
            _transitoryTreeOps.StorageType = StorageType.Memory;
            _transitoryTreeOps.LockingFactory = new LockFactory<ReaderWriterLocking>();
            _transitoryTreeOps.CallLevelLock = new ReaderWriterLocking();
            _transitionTree = new BPlusTree<AttributeValue, IndexOp<long>>(_transitoryTreeOps);
            opsToCommit = new ConcurrentDictionary<long, IList<IndexOp<long>>>();
            appliedOps = new ClusteredList<long>();

            #endregion

            _bounds = new BoundingBox(1);
            try
            {
                RestoreBoundsFromTree();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                        "Error: " + ErrorCodes.Indexes.NUMERIC_BOUNDS_CALCULATION_FAILURE +
                        " - Failed to calculate numeric bounds of Index for attribute(s) " + _indexKey.ToString() +
                        Environment.NewLine + ex);
                    throw new IndexException(ErrorCodes.Indexes.NUMERIC_BOUNDS_CALCULATION_FAILURE);
                }
            }
        }

        public string Path { get { return _path; } }

        public IndexKey IndexKey { get { return _indexKey; } }

        public bool IsFunctional { get; set; }

        public CollectionIndexManager Parent { get { return _parent; } }

        public string Name
        {
            get
            {
                return _configuration.IndexName;
            }
            set { _configuration.IndexName = value; }
        }

        public IndexConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        public IndexAttribute Attributes
        {
            get
            {
                return _configuration.Attributes;
            }
        }
        
        protected virtual void RestoreBoundsFromTree()
        {
            var bound = GetBound(FieldDataType.Number, Bound.Max);
            _bounds.SetMax(0, FieldDataType.Number, bound);
            bound = GetBound(FieldDataType.Number, Bound.Min);
            _bounds.SetMin(0, FieldDataType.Number, bound);
            bound = GetBound(FieldDataType.Array, Bound.Max);
            _bounds.SetMax(0, FieldDataType.Array, bound);
            bound = GetBound(FieldDataType.Array, Bound.Min);
            _bounds.SetMin(0, FieldDataType.Array, bound);
        }

        public void Remove(AttributeValue value, long rowId, long operationId)
        {
            if (!_tree.IsDisposed)
            {
                var op = new IndexRemoveOp<long>(value, rowId);
                _transitionTree.Add(value, op);
                BoundCheck(value);
                AddToCommitOps(operationId, op);
            }
        }

        private void AddToCommitOps(long operationId, IndexOp<long> op)
        {
            IList<IndexOp<long>> ops;
            if (!opsToCommit.TryGetValue(operationId, out ops))
            {
                ops = new List<IndexOp<long>>();
                opsToCommit.Add(operationId, ops);
            }
            ops.Add(op);
        }

        public virtual void Add(AttributeValue value, long rowId, long operationId)
        {
            if (!_tree.IsDisposed)
            {
                var op = new IndexInsertOp<long>(value, rowId);
                _transitionTree.Add(value, op);
                ResolveBound(value);
                AddToCommitOps(operationId, op);
            }
        }

        public void PersistOperation(long operationId)
        {
            IList<IndexOp<long>> opToCommit;
            if (opsToCommit.TryGetValue(operationId, out opToCommit))
            {
                foreach (var op in opToCommit)
                {
                    switch (op.OperationType)
                    {
                        case OpType.Insert:
                            _tree.Add(op.Key, op.RowId);
                            break;
                        case OpType.Remove:
                            _tree.TryRemove(op.Key, op.RowId);
                            break;
                    }
                }
                appliedOps.Add(operationId);
            }
        }

        public void PersistAndCommitAll()
        {
            var OperationIds = new long[opsToCommit.Count];
            opsToCommit.Keys.CopyTo(OperationIds, 0);
            foreach (var operationId in OperationIds)
            {
                IList<IndexOp<long>> opToCommit;
                if (opsToCommit.TryGetValue(operationId, out opToCommit))
                {
                    foreach (var indexOp in opToCommit)
                    {
                        _transitionTree.TryRemove(indexOp.Key, indexOp);
                        switch (indexOp.OperationType)
                        {
                            case OpType.Insert:
                                _tree.Add(indexOp.Key, indexOp.RowId);
                                break;
                            case OpType.Remove:
                                _tree.TryRemove(indexOp.Key, indexOp.RowId);
                                break;
                        }
                    }
                    opsToCommit.Remove(operationId);
                }
            }
            try
            {
                if (!_tree.IsDisposed)
                    _tree.Commit();
            }
            catch (Exception ex)
            {
                Rollback(null);
                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                        "Error Code: " + ErrorCodes.Indexes.TREE_COMMIT_FAILURE + " - " + ex.ToString());
                    throw new IndexException(ErrorCodes.Indexes.TREE_COMMIT_FAILURE);
                }
            }

        }

        protected virtual void ResolveBound(AttributeValue value)
        {
            if (value.DataType.Equals(FieldDataType.Number))
            {
                _bounds.Resolve(0, FieldDataType.Number, value);
            }
            if (value.DataType.Equals(FieldDataType.Array))
            {
                _bounds.Resolve(0, FieldDataType.Array, value);
            }
        }

        protected virtual void BoundCheck(AttributeValue removedValue)
        {
            if (removedValue.DataType.Equals(FieldDataType.Array))
            {
                AttributeValue unWrapped = (SingleAttributeValue)((ArrayElement)((SingleAttributeValue)removedValue).Value).Element;
                if (_bounds.Max(0,FieldDataType.Array) != null && _bounds.Max(0,FieldDataType.Array).Equals(unWrapped))
                {
                    _bounds.ResetMax(0, FieldDataType.Array);
                    System.Threading.Tasks.Task.Factory.StartNew(
                        () => _bounds.Resolve(0, FieldDataType.Array, GetBound(FieldDataType.Array, Bound.Max)));
                }
                else if (_bounds.Min(0, FieldDataType.Array) != null && _bounds.Min(0, FieldDataType.Array).Equals(unWrapped))
                {
                    _bounds.ResetMin(0, FieldDataType.Array);
                    System.Threading.Tasks.Task.Factory.StartNew(
                        () => _bounds.Resolve(0, FieldDataType.Array, GetBound(FieldDataType.Array, Bound.Min)));
                }
            }
            else if (removedValue.DataType.Equals(FieldDataType.Number))
            {
                if (_bounds.Max(0, FieldDataType.Number) != null && _bounds.Max(0, FieldDataType.Number).Equals(removedValue))
                {
                    _bounds.ResetMax(0, FieldDataType.Number);
                    System.Threading.Tasks.Task.Factory.StartNew(
                        () => _bounds.Resolve(0, FieldDataType.Number, GetBound(FieldDataType.Number, Bound.Max)));
                }
                else if (_bounds.Min(0, FieldDataType.Number) != null && _bounds.Min(0, FieldDataType.Number).Equals(removedValue))
                {
                    _bounds.ResetMin(0, FieldDataType.Number);
                    System.Threading.Tasks.Task.Factory.StartNew(
                        () => _bounds.Resolve(0, FieldDataType.Number, GetBound(FieldDataType.Number, Bound.Min)));
                }
            }
        }

        public void Clear()
        {
            _tree.Clear();
        }

        public ITransaction BeginTransaction(ITransaction parentTransaction, bool isReadOnly)
        {
            throw new NotImplementedException();
        }

        public void Commit(ITransaction transaction)
        {
            try
            {
                if (!_tree.IsDisposed)
                    _tree.Commit();
                foreach (var appliedOp in appliedOps)
                {
                    IList<IndexOp<long>> ops;
                    if (opsToCommit.TryGetValue(appliedOp, out ops))
                    {
                        foreach (var indexOp in ops)
                        {
                            _transitionTree.TryRemove(indexOp.Key, indexOp);
                        }
                        opsToCommit.Remove(appliedOp);
                    }
                }

                
            }
            catch (Exception ex)
            {
                Rollback(transaction);
                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                        "Error Code: " + ErrorCodes.Indexes.TREE_COMMIT_FAILURE + " - " + ex.ToString());
                    throw new IndexException(ErrorCodes.Indexes.TREE_COMMIT_FAILURE);
                }
            }
        }

        public void Rollback(ITransaction transaction)
        {
            try
            {
                _tree.Rollback();
                appliedOps.Clear();

            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.IndexLogger != null)
                {
                    LoggerManager.Instance.IndexLogger.Error("BPlusIndex",
                        "Error Code: " + ErrorCodes.Indexes.TREE_ROLLBACK_FAILURE + " - " + ex.ToString());
                    throw new IndexException(ErrorCodes.Indexes.TREE_ROLLBACK_FAILURE);
                }
            }
        }

        public SortOrder Order { get { return order; } }

        public virtual IEnumerator<KeyValuePair<AttributeValue, long>> GetEnumerator()
        {
            try
            {
                return MergeEnumerators(_tree.GetEnumerator(), _transitionTree.GetEnumerator());
            }
            catch (Exception ex)
            {
                LoggerManager.Instance.IndexLogger.Error("BPlusIndex", "Index Enumeration Failure: " + ex.ToString());
                throw;
            }
        }

        public virtual IEnumerator<KeyValuePair<AttributeValue, long>> GetEnumerator(FieldDataType type)
        {
            try
            {
                return
                    MergeEnumerators(
                        _tree.EnumerateRange(new BoundaryValueMask(type, Bound.Min),
                            new BoundaryValueMask(type, Bound.Max))
                            .GetEnumerator(), _transitionTree.EnumerateRange(new BoundaryValueMask(type, Bound.Min),
                                new BoundaryValueMask(type, Bound.Max)).GetEnumerator());
            }
            catch (Exception ex)
            {
                LoggerManager.Instance.IndexLogger.Error("BPlusIndex", "Index Enumeration Failure: " + ex.ToString());
                throw;
            }
        }

        public virtual IEnumerator<KeyValuePair<AttributeValue, long>> GetEnumerator(AttributeValue start, AttributeValue end)
        {
            try
            {
                if (order == SortOrder.DESC)
                {
                    var temp = end;
                    end = start;
                    start = temp;
                }

                return MergeEnumerators(_tree.EnumerateRange(start, end).GetEnumerator(),
                    _transitionTree.EnumerateRange(start, end).GetEnumerator());
            }
            catch (Exception ex)
            {
                LoggerManager.Instance.IndexLogger.Error("BPlusIndex", "Index Enumeration Failure: " + ex.ToString());
                throw;
            }
        }
        
        protected virtual IEnumerator<KeyValuePair<AttributeValue, long>> EnumerateFrom(AttributeValue start)
        {
            try
            {
                return MergeEnumerators(_tree.EnumerateFrom(start).GetEnumerator(),
                    _transitionTree.EnumerateFrom(start).GetEnumerator());
            }
            catch (Exception ex)
            {
                LoggerManager.Instance.IndexLogger.Error("BPlusIndex","Index Enumeration Failure: "+ ex.ToString());
                throw;
            }
        }

        protected virtual IEnumerator<KeyValuePair<AttributeValue, long>> EnumerateTo(AttributeValue end)
        {
            try
            {
                var min = new BoundaryValueMask(end.DataType, Bound.Min);
                return MergeEnumerators(_tree.EnumerateRange(min, end).GetEnumerator(),
                    _transitionTree.EnumerateRange(min, end).GetEnumerator());
            }
            catch (Exception ex)
            {
                LoggerManager.Instance.IndexLogger.Error("BPlusIndex", "Index Enumeration Failure: " + ex.ToString());
                throw;
            }
        }

        protected virtual IEnumerator<KeyValuePair<AttributeValue, long>> MergeEnumerators(
            IEnumerator<KeyValuePair<AttributeValue, IDictionary<long, byte>>> mainEnumerator,
            IEnumerator<KeyValuePair<AttributeValue, IDictionary<IndexOp<long>, byte>>> transitoryEnumerator)
        {
            bool mainExists, transitoryExists;
            mainExists = mainEnumerator.MoveNext();
            transitoryExists = transitoryEnumerator.MoveNext();

            while (true)
            {
                if (!mainExists && !transitoryExists)
                    yield break;

                if (mainExists && !transitoryExists)
                {
                    while (mainExists)
                    {
                        foreach (var rowId in mainEnumerator.Current.Value)
                        {
                            yield return new KeyValuePair<AttributeValue, long>(mainEnumerator.Current.Key, rowId.Key);
                        }
                        mainExists = mainEnumerator.MoveNext();
                    }
                    yield break;
                }

                if (transitoryExists && !mainExists)
                {
                    while (transitoryExists)
                    {
                        foreach (var indexOp in transitoryEnumerator.Current.Value.Keys)
                        {
                            if (indexOp is IndexInsertOp<long>)
                                yield return new KeyValuePair<AttributeValue, long>(indexOp.Key, indexOp.RowId);
                        }
                        transitoryExists = transitoryEnumerator.MoveNext();
                    }
                    yield break;
                }

                bool bothEnumeratorsExist = true;
                while (bothEnumeratorsExist)
                {
                    int comparisonResult = FixComparison(mainEnumerator.Current.Key.CompareTo(transitoryEnumerator.Current.Key));

                    if (comparisonResult < 0)
                    {
                        foreach (var rowId in mainEnumerator.Current.Value)
                        {
                            yield return new KeyValuePair<AttributeValue, long>(mainEnumerator.Current.Key, rowId.Value);
                        }
                        bothEnumeratorsExist = mainExists = mainEnumerator.MoveNext();
                    }
                    else if (comparisonResult > 0)
                    {
                        foreach (var indexOp in transitoryEnumerator.Current.Value.Keys)
                        {
                            if (indexOp is IndexInsertOp<long>)
                                yield return new KeyValuePair<AttributeValue, long>(indexOp.Key, indexOp.RowId);
                        }
                        bothEnumeratorsExist = transitoryExists = transitoryEnumerator.MoveNext();
                    }
                    else
                    {
                        var mainSet = mainEnumerator.Current.Value;
                        foreach (var indexOp in transitoryEnumerator.Current.Value.Keys)
                        {
                            indexOp.MergeWith(mainSet);
                        }
                        foreach (var rowId in mainSet.Keys)
                        {
                            yield return new KeyValuePair<AttributeValue, long>(mainEnumerator.Current.Key, rowId);
                        }
                        bothEnumeratorsExist = (mainExists = mainEnumerator.MoveNext()) &
                                               (transitoryExists = transitoryEnumerator.MoveNext());
                    }
                }
            }
        }

        protected virtual int FixComparison(int initial)
        {
            if (order == SortOrder.DESC)
            {
                initial = -initial;
            }
            return initial;
        }

        public virtual IEnumerator<KeyValuePair<AttributeValue, long>> GetEnumeratorTo(AttributeValue end)
        {
            if (order == SortOrder.ASC)
                return EnumerateTo(end);
            return EnumerateFrom(end);
        }

        public virtual IEnumerator<KeyValuePair<AttributeValue, long>> GetEnumeratorFrom(AttributeValue start)
        {
            if (order == SortOrder.ASC)
                return EnumerateFrom(start);
            return EnumerateTo(start);
        }

        public virtual IEnumerator<KeyValuePair<AttributeValue, long>> GetEnumerator(AttributeValue value)
        {
            try
            {
                return GetValueEnumerator(value);
            }
            catch (Exception ex)
            {
                LoggerManager.Instance.IndexLogger.Error("BPlusIndex", "Index Enumeration Failure: " + ex.ToString());
                throw;
            }
        }

        private IEnumerator<KeyValuePair<AttributeValue, long>> GetValueEnumerator(AttributeValue value)
        {
            IDictionary<long, byte> outRowIds, rowIds;
            if (_tree.TryGetValue(value, out outRowIds))
            {
                rowIds = new HashVector<long, byte>(outRowIds);
            }
            else
            {
                rowIds = new HashVector<long, byte>();
            }
          
            IDictionary<IndexOp<long>,byte> ops;
            if (_transitionTree.TryGetValue(value, out ops))
            {
                foreach (var indexOp in ops.Keys)
                {
                    indexOp.MergeWith(rowIds);
                }
            }

            foreach (var rowId in rowIds)
            {
                yield return new KeyValuePair<AttributeValue, long>(value, rowId.Key);
            }
        }
       
        public IDictionary<long, byte> this[AttributeValue key]
        {
            get { return _tree[key]; }
        }

        public void Dispose()
        {
            if (_tree != null)
                _tree.Dispose();
            if (_transitionTree != null) _transitionTree.Dispose();
        }

        protected virtual AttributeValue GetBound(FieldDataType type, Bound bound)
        {
            var boundary = new BoundaryValueMask(type, bound);
            var enumerator = GetEnumerator(boundary);
            while (enumerator.MoveNext()) ;
            return boundary.State;
        }

        public int KeyCount
        {
            get { return _tree.Count; }
        }

        public int ValueCount
        {
            get { return _tree.ValueCount; }
        }


        public virtual object GetStat(StatName name)
        {
            switch (name)
            {
                case StatName.Type:
                    return IndexType.SingleAttribute;
                case StatName.BTreeOrder:
                    return 64;
                default:
                    return null;
            }
        }

        public bool IsCancelled()
        {
            return _tree.IsDisposed;
        }

        public long GetNextInterval()
        {
            return 30000; // 30 seconds.
        }

        public void Run()
        {
            lock (disposeLock)
            {
                if (!_tree.IsDisposed)
                {
                    if (_bounds != null)
                    {
                       RestoreBoundsFromTree();
                    }
                }
            }
        }


        public void Destroy()
        {
            lock (disposeLock)
            {
                if (_tree!=null && !_tree.IsDisposed)
                    _tree.Dispose();

                if (File.Exists(_path))
                    File.Delete(_path);
                string _logPath = _path.Substring(0, _path.Length - 3) + "tlog";
                if (File.Exists(_logPath))
                    File.Delete(_logPath);
            }
        }


        public bool Contains(AttributeValue key)
        {
            return _tree.ContainsKey(key);
        }


        public void CopyTo(KeyValuePair<AttributeValue, long>[] array, int offset)
        {
            _tree.CopyTo(array, offset);
        }

        public bool RunAgain
        {
            get { return _runAgain; }
            set { _runAgain = value; }
        }

        public bool IsRunning
        {
            get
            {
                lock (_isRunnigLock)
                {
                    return _isRunning;
                }
            }
            set
            {
                lock (_isRunnigLock)
                {
                    _isRunning = value;
                }
            }
        }

        public override int GetHashCode()
        {
            return _configuration.Name.GetHashCode();
        }

        public virtual void Print(TextWriter output)
        {
            output.Write("BPlusIndex:{");
            output.Write("Name=" + _configuration.IndexName);
            output.Write(",Attributes=[");
            output.Write(_configuration.Attributes.Name);

            output.Write("]");
            output.Write("}");
        }

        public AttributeValue Min(int attNumber, FieldDataType type)
        {
            return _bounds.Min(attNumber, type);
        }

        public AttributeValue Max(int attNumber, FieldDataType type)
        {
            return _bounds.Max(attNumber, type);
        }

        public virtual AttributeValue Transform(AttributeValue source)
        {
            return source;
        }

       

        public bool IsDisposed
        {
            get { return _tree == null || _tree.IsDisposed; }
        }

        public void Commit()
        {
            Commit(null);
        }

       

        public override bool Equals(object obj)
        {
            var otherIndex = obj as BPlusIndex;
            if (otherIndex == null)
                return false;

            return _configuration.Name.Equals(otherIndex._configuration.Name);
        }

        public void Rollback()
        {
            Rollback(null);
        }

        public override string ToString()
        {
            return _configuration.Name + "(" + _indexKey.ToString() + ")";
        }

        public bool TryGetValue(AttributeValue key, out IDictionary<long, byte> values)
        {
            return _tree.TryGetValue(key, out values);
        }
    }
}
