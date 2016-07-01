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
using System.Collections.Generic;
using System.Threading;

namespace Alachisoft.NosDB.Core.Storage
{
    public class LockManager<TDatabase, TCollection, TKey> where TKey : class 
    {
        private readonly LockRecursionPolicy _policy;
        
        private readonly ConcurrentDictionary<TDatabase, ConcurrentDictionary<TCollection, ConcurrentDictionary<TKey,ReaderWriterLockSlim>>>
            _databaseLockTable = new ConcurrentDictionary<TDatabase, ConcurrentDictionary<TCollection, ConcurrentDictionary<TKey,ReaderWriterLockSlim>>>();

        internal LockManager
            (LockRecursionPolicy policy)
        {
            _policy = policy;
        }

        private AutoLockContainer GetOrCreate(TDatabase database, TCollection collection, TKey key, LockType type)
        {
            var collectionLocks = _databaseLockTable.GetOrAdd(database,
                (d) => new ConcurrentDictionary<TCollection, ConcurrentDictionary<TKey, ReaderWriterLockSlim>>());
            var lockLedger = collectionLocks.GetOrAdd(collection,
                (c) => new ConcurrentDictionary<TKey, ReaderWriterLockSlim>());
            var keyLock = lockLedger.GetOrAdd(key, (k) => new ReaderWriterLockSlim(_policy));
            return new AutoLockContainer(keyLock, type, key, lockLedger);
        }

        public void DropCollectionLocks(TDatabase database, TCollection collection)
        {
            ConcurrentDictionary<TCollection, ConcurrentDictionary<TKey, ReaderWriterLockSlim>> collectionLocks;
            ConcurrentDictionary<TKey, ReaderWriterLockSlim> keyLocks;
            if (_databaseLockTable.TryGetValue(database, out collectionLocks))
            {
                collectionLocks.TryRemove(collection, out keyLocks);
            }
        }

        public void DropDatabaseLocks(TDatabase database)
        {

            ConcurrentDictionary<TCollection, ConcurrentDictionary<TKey, ReaderWriterLockSlim>> collectionLocks;
            _databaseLockTable.TryRemove(database, out collectionLocks);
        }

        


        #region Key Level Locking Methods

        internal IDisposable GetKeyReaderLock(TDatabase database, TCollection collection, TKey key)
        {
            return GetOrCreate(database, collection, key, LockType.Read);
        }

        internal IDisposable GetKeyWriterLock(TDatabase database, TCollection collection, TKey key)
        {
            return GetOrCreate(database, collection, key, LockType.Write);
        }
        #endregion

        
        private enum LockType
        {
            Read,
            Write
        }
        
        private class AutoLockContainer : IDisposable
        {
            private ReaderWriterLockSlim _lockObject;
            private TKey _key;
            private LockType _type;
            private IDictionary<TKey, ReaderWriterLockSlim> _parent; 

            public AutoLockContainer(ReaderWriterLockSlim lockObject, LockType type, TKey key, IDictionary<TKey,ReaderWriterLockSlim> parent)
            {
                _lockObject = lockObject;
                _type = type;
                _key = key;
                _parent = parent;
                switch (type)
                {
                    case LockType.Read:
                        _lockObject.EnterReadLock();
                        break;
                    case LockType.Write:
                        _lockObject.EnterWriteLock();
                        break;
                }
            }

            public void Dispose()
            {
                switch (_type)
                {
                    case LockType.Read:
                        _lockObject.ExitReadLock();
                        break;
                    case LockType.Write:
                        _lockObject.ExitWriteLock();
                        break;
                }
                if (!IsLockIdle(_lockObject))
                    _parent.Remove(_key);
            }

            private static bool IsLockIdle(ReaderWriterLockSlim lockObject)
            {
                if (
                    !lockObject.IsWriteLockHeld &&
                    !lockObject.IsReadLockHeld &&
                    !lockObject.IsUpgradeableReadLockHeld &&

                    lockObject.CurrentReadCount == 0 &&
                    lockObject.WaitingReadCount == 0 &&
                    lockObject.RecursiveReadCount == 0 &&
                    lockObject.RecursiveWriteCount == 0 &&
                    lockObject.WaitingWriteCount == 0
                    )
                    return false;
                return true;

            }
        }
        #region Commented Code



        //private class LockLedger
        //{
        //    IDictionary<TKey, WeakReference<ReaderWriterLockSlim>> ledger;

        //    public LockLedger()
        //    {
        //        ledger = new ConcurrentDictionary<TKey, WeakReference<ReaderWriterLockSlim>>();
        //    }

        //    private void Remove(TKey key)
        //    {
        //        ledger.Remove(key);
        //    }

        //    public bool TryGet(TKey key, bool createNew, out ReaderWriterLockSlim keyLock)
        //    {
        //        keyLock = null;
        //        WeakReference<ReaderWriterLockSlim> weakRef;
        //        if (ledger.TryGetValue(key, out weakRef))
        //        {
        //            if (!weakRef.TryGetTarget(out keyLock))
        //            {
        //                using (new SafeLock(weakRef))
        //                {
        //                    keyLock = new AutoReaderWriterLockSlim<TKey>(key, Remove);
        //                    weakRef.SetTarget(keyLock);
        //                }
        //            }
        //            return true;
        //        }

        //        if (createNew)
        //        {
        //            keyLock = new AutoReaderWriterLockSlim<TKey>(key, Remove);
        //            weakRef = new WeakReference<ReaderWriterLockSlim>(keyLock);
        //            ledger[key] = weakRef;
        //            return true;
        //        }
        //        return false;
        //    }
        //}
        //private DatabaseLocker<TCollection, TKey> BorrowDatabaseLockObject(TDatabase database, bool createNew)
        //{
        //    lock (_databaseLockTable)
        //    {
        //        if (_databaseLockTable.ContainsKey(database))
        //            return _databaseLockTable[database];

        //        return (createNew) ? _databaseLockTable[database] = new DatabaseLocker<TCollection, TKey>(_policy) : null;
        //    }
        //}

        //private void ReturnDatabaseLockObject(TDatabase database, DatabaseLocker<TCollection, TKey> lockObject)
        //{
        //    lock (_databaseLockTable)
        //    {
        //        if (!lockObject.IsDeleted
        //            && lockObject.IsLockCountZero)
        //        {
        //            _databaseLockTable.Remove(database);
        //            lockObject.IsDeleted = true;
        //        }
        //    }
        //}

        #region Database Level Locking Methods

        //internal void GetDatabaseReaderLock(TDatabase database)
        //{
        //    BorrowDatabaseLockObject(database, true).GetReaderLock();
        //}

        //internal void ReleaseDatabaseReaderLock(TDatabase database)
        //{
        //    var lockObject = BorrowDatabaseLockObject(database, false);

        //    if (lockObject == null)
        //        return;

        //    lockObject.ReleaseReaderLock();
        //    ReturnDatabaseLockObject(database, lockObject);
        //}

        //internal void GetDatabaseWriterLock(TDatabase database)
        //{
        //    BorrowDatabaseLockObject(database, true).GetWriterLock();
        //}

        //internal void ReleaseDatabaseWriterLock(TDatabase database)
        //{
        //    var databaselock = BorrowDatabaseLockObject(database, false);

        //    if (databaselock == null)
        //        return;

        //    databaselock.ReleaseWriterLock();
        //    ReturnDatabaseLockObject(database, databaselock);
        //}

        #endregion

        #region Collection Level Locking Methods

        //internal void GetCollectionReaderLock(TDatabase database, TCollection collection)
        //{
        //    BorrowDatabaseLockObject(database, true).GetCollectionReaderLock(collection);
        //}

        //internal void ReleaseCollectionReaderLock(TDatabase database, TCollection collection)
        //{
        //    var databaselock = BorrowDatabaseLockObject(database, false);

        //    if (databaselock == null)
        //        return;

        //    databaselock.ReleaseCollectionReaderLock(collection);
        //    ReturnDatabaseLockObject(database, databaselock);
        //}

        //internal void GetCollectionWriterLock(TDatabase database, TCollection collection)
        //{
        //    BorrowDatabaseLockObject(database, true).GetCollectionWriterLock(collection);
        //}

        //internal void ReleaseCollectionWriterLock(TDatabase database, TCollection collection)
        //{
        //    var databaselock = BorrowDatabaseLockObject(database, false);

        //    if (databaselock == null)
        //        return;

        //    databaselock.ReleaseCollectionWriterLock(collection);
        //    ReturnDatabaseLockObject(database, databaselock);
        //}

        #endregion

        //private class DatabaseLocker<TCollection, TKey>
        //{
        //    private readonly LockRecursionPolicy _policy;
        //    //private readonly ReaderWriterLockSlim _databaseLockSlim;
        //    private readonly IDictionary<TCollection, CollectionLocker<TKey>> _collectionLockTable = new Dictionary<TCollection, CollectionLocker<TKey>>();

        //    public DatabaseLocker( LockRecursionPolicy policy)
        //    {
        //        _policy = policy;
        //        //_databaseLockSlim = new ReaderWriterLockSlim(_policy);
        //    }

        //    //public bool IsLockCountZero
        //    //{
        //    //    get
        //    //    {
        //    //        if (_databaseLockSlim.IsWriteLockHeld || _databaseLockSlim.IsReadLockHeld)
        //    //            return false;

        //    //        if (_databaseLockSlim.CurrentReadCount != 0 || _databaseLockSlim.RecursiveWriteCount != 0
        //    //            || _databaseLockSlim.WaitingWriteCount != 0 || _databaseLockSlim.WaitingUpgradeCount != 0
        //    //            || _databaseLockSlim.WaitingReadCount != 0 || _databaseLockSlim.RecursiveUpgradeCount != 0
        //    //            || _databaseLockSlim.RecursiveReadCount != 0)
        //    //            return false;

        //    //        //Todo: Abdul-Rehman remove locking from dictionary and use concurrent dictionaries or whatever.
        //    //        lock (_collectionLockTable)
        //    //        {
        //    //            foreach (var pair in _collectionLockTable)
        //    //            {
        //    //                if (!pair.Value.IsLockCountZero)
        //    //                    return false;
        //    //            }
        //    //        }
        //    //        return true;
        //    //    }
        //    //}

        //    public bool IsDeleted { get; set; }

        //    //public void GetReaderLock()
        //    //{
        //    //    _databaseLockSlim.EnterReadLock();
        //    //}

        //    //public void ReleaseReaderLock()
        //    //{
        //    //    _databaseLockSlim.ExitReadLock();
        //    //}

        //    //public void GetWriterLock()
        //    //{
        //    //    _databaseLockSlim.EnterWriteLock();
        //    //}

        //    //public void ReleaseWriterLock()
        //    //{
        //    //    _databaseLockSlim.ExitWriteLock();
        //    //}

        //    //public void GetCollectionReaderLock(TCollection collection)
        //    //{
        //    //    _databaseLockSlim.EnterReadLock();
        //    //    BorrowCollectionLockObject(collection, true).GetReaderLock();
        //    //}

        //    //public void ReleaseCollectionReaderLock(TCollection collection)
        //    //{
        //    //    var lockObject = BorrowCollectionLockObject(collection, false);

        //    //    if (lockObject == null)
        //    //        return;

        //    //    lockObject.ReleaseReaderLock();
        //    //    ReturnCollectionLockObject(collection, lockObject);
        //    //    _databaseLockSlim.ExitReadLock();
        //    //}

        //    //public void GetCollectionWriterLock(TCollection collection)
        //    //{
        //    //    _databaseLockSlim.EnterWriteLock();
        //    //    BorrowCollectionLockObject(collection, true).GetWriterLock();
        //    //}

        //    //public void ReleaseCollectionWriterLock(TCollection collection)
        //    //{
        //    //    var lockObject = BorrowCollectionLockObject(collection, false);

        //    //    if (lockObject == null)
        //    //        return;

        //    //    lockObject.ReleaseWriterLock();
        //    //    ReturnCollectionLockObject(collection, lockObject);
        //    //    _databaseLockSlim.ExitWriteLock();
        //    //}

        //    public void GetKeyReaderLock(TCollection collection, TKey key)
        //    {
        //        //_databaseLockSlim.EnterReadLock();
        //        BorrowCollectionLockObject(collection, true).GetKeyReaderLock(key);
        //    }

        //    public void ReleaseKeyReaderLock(TCollection collection, TKey key)
        //    {
        //        var lockObject = BorrowCollectionLockObject(collection, false);

        //        if (lockObject == null)
        //            return;

        //        lockObject.ReleaseKeyReaderLock(key);
        //        //ReturnCollectionLockObject(collection, lockObject);
        //        //_databaseLockSlim.ExitReadLock();
        //    }

        //    public void GetKeyWriterLock(TCollection collection, TKey key)
        //    {
        //       // _databaseLockSlim.EnterReadLock();
        //        BorrowCollectionLockObject(collection, true).GetKeyWriterLock(key);
        //    }

        //    public void ReleaseKeyWriterLock(TCollection collection, TKey key)
        //    {
        //        var lockObject = BorrowCollectionLockObject(collection, false);

        //        if (lockObject == null)
        //            return;

        //        lockObject.ReleaseKeyWriterLock(key);
        //        //ReturnCollectionLockObject(collection, lockObject);
        //        //_databaseLockSlim.ExitReadLock();
        //    }

        //    private CollectionLocker<TKey> BorrowCollectionLockObject(TCollection collection, bool createNew)
        //    {
        //        lock (_collectionLockTable)
        //        {
        //            if (_collectionLockTable.ContainsKey(collection))
        //                return _collectionLockTable[collection];

        //            return (createNew) ? _collectionLockTable[collection] = new CollectionLocker<TKey>(_policy) : null;
        //        }
        //    }

        //    //private void ReturnCollectionLockObject(TCollection collection, CollectionLocker<TKey> lockObject)
        //    //{
        //    //    lock (_collectionLockTable)
        //    //    {
        //    //        if (lockObject.IsLockCountZero)
        //    //        {
        //    //            _collectionLockTable.Remove(collection);
        //    //        }
        //    //    }
        //    //}
        //}

        //private class CollectionLocker<TKey>
        //{
        //    private readonly LockRecursionPolicy _policy;
        //    //private readonly ReaderWriterLockSlim _collectionLockSlim;
        //    private readonly IDictionary<TKey, ReaderWriterLockSlim> _keyLockTable = new Dictionary<TKey, ReaderWriterLockSlim>();

        //    public CollectionLocker
        //        (LockRecursionPolicy policy)
        //    {
        //        _policy = policy;
        //        //_collectionLockSlim = new ReaderWriterLockSlim(policy);
        //    }

        //    //public bool IsLockCountZero
        //    //{
        //    //    get
        //    //    {

        //    //        if (_collectionLockSlim.IsWriteLockHeld || _collectionLockSlim.IsReadLockHeld)
        //    //            return false;

        //    //        if (_collectionLockSlim.CurrentReadCount != 0 || _collectionLockSlim.RecursiveWriteCount != 0
        //    //           || _collectionLockSlim.WaitingWriteCount != 0 || _collectionLockSlim.WaitingUpgradeCount != 0
        //    //           || _collectionLockSlim.WaitingReadCount != 0 || _collectionLockSlim.RecursiveUpgradeCount != 0
        //    //           || _collectionLockSlim.RecursiveReadCount != 0)
        //    //            return false;

        //    //        lock (_keyLockTable)
        //    //        {
        //    //            foreach (var pair in _keyLockTable)
        //    //            {
        //    //                if (pair.Value != null && (pair.Value.IsWriteLockHeld || pair.Value.IsReadLockHeld))
        //    //                    return false;

        //    //                if (pair.Value != null && (pair.Value.CurrentReadCount != 0 || pair.Value.RecursiveWriteCount != 0))
        //    //                    return false;
        //    //            }
        //    //        }
        //    //        return true;
        //    //    }
        //    //}

        //    //public void GetWriterLock()
        //    //{
        //    //    _collectionLockSlim.EnterWriteLock();
        //    //}

        //    //public void ReleaseWriterLock()
        //    //{
        //    //    _collectionLockSlim.ExitWriteLock();
        //    //}

        //    //public void GetReaderLock()
        //    //{
        //    //    _collectionLockSlim.EnterReadLock();
        //    //}

        //    //public void ReleaseReaderLock()
        //    //{
        //    //    _collectionLockSlim.ExitReadLock();
        //    //}

        //    public void GetKeyReaderLock(TKey key)
        //    {
        //        //_collectionLockSlim.EnterReadLock();
        //        BorrowKeyLockObject(key, true).EnterReadLock();
        //    }

        //    public void ReleaseKeyReaderLock(TKey key)
        //    {
        //        var lockObject = BorrowKeyLockObject(key, false);

        //        if (lockObject == null)
        //            return;

        //        lockObject.ExitReadLock();
        //        ReturnKeyLockObject(key, lockObject);
        //        //_collectionLockSlim.ExitReadLock();
        //    }

        //    public void GetKeyWriterLock(TKey key)
        //    {
        //        //_collectionLockSlim.EnterReadLock();
        //        BorrowKeyLockObject(key, true).EnterWriteLock();
        //    }

        //    public void ReleaseKeyWriterLock(TKey key)
        //    {
        //        var lockObject = BorrowKeyLockObject(key, false);
        //        if (lockObject != null)
        //        {
        //            lockObject.ExitWriteLock();
        //            ReturnKeyLockObject(key, lockObject);
        //           // _collectionLockSlim.ExitReadLock();
        //        }
        //    }

        //    private ReaderWriterLockSlim BorrowKeyLockObject(TKey key, bool createNew)
        //    {
        //        lock (_keyLockTable)
        //        {
        //            if (_keyLockTable.ContainsKey(key))
        //                return _keyLockTable[key];

        //            return (createNew) ? _keyLockTable[key] = new ReaderWriterLockSlim(_policy) : null;
        //        }
        //    }

        //    private void ReturnKeyLockObject(TKey key, ReaderWriterLockSlim lockObject)
        //    {
        //        lock (_keyLockTable)
        //        {
        //            if (!lockObject.IsWriteLockHeld && !lockObject.IsReadLockHeld
        //                && lockObject.CurrentReadCount == 0 && lockObject.RecursiveWriteCount == 0
        //                /*&& _collectionLockSlim.WaitingWriteCount == 0 && _collectionLockSlim.WaitingUpgradeCount == 0
        //                && _collectionLockSlim.WaitingReadCount == 0 && _collectionLockSlim.RecursiveUpgradeCount == 0
        //                && _collectionLockSlim.RecursiveReadCount == 0*/)
        //            {
        //                _keyLockTable.Remove(key);
        //            }
        //        }
        //    }

        //}
        #endregion
    }
}
