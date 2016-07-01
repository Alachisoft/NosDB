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
using Alachisoft.NosDB.Common.Server.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    /// <summary>
    /// Wrapper class encapsulating temporary storage data structure used for recovery persistance. 
    /// This data structure supports producer/consumer behavior
    /// 
    /// M_TODO: Add methods as per need
    /// </summary>
    internal class RecoveryPersistenceQueue : IDisposable
    {
        private BlockingCollection<DataSlice> documentCollection = null;
        private CancellationTokenSource _cancelToken;
        private const int BoundedCapacity = 25;
        private bool _consumed;
        private object _mutex;
        private bool _pauseProducing;


        public RecoveryPersistenceQueue()
        {
            _cancelToken = new CancellationTokenSource();
            documentCollection = new BlockingCollection<DataSlice>(BoundedCapacity);
            _consumed = false;
            _pauseProducing = false;
            _mutex = new object();
        }

        #region Properties
        public bool PauseProducing
        {
            get { return _pauseProducing; }
            set
            {
                lock (_mutex)
                {
                    _pauseProducing = value;
                }
            }
        }

        public bool Consumed
        {
            get
            {
                return _consumed;
            }
            set
            {
                lock (_mutex)
                {
                    _consumed = value;
                }
            }
        }

        public int Count
        {
            get
            {
                return documentCollection.Count();
            }
        }

        public CancellationTokenSource CancelToken
        {
            get { return _cancelToken; }
            set { _cancelToken = value; }
        }

        public bool IsAddingCompleted
        {
            get
            { return documentCollection.IsAddingCompleted; }
        }

        #endregion

        #region Public Methods
        public bool TryTake(out DataSlice dataSlice, int timeout)
        {
            return documentCollection.TryTake(out dataSlice, timeout);
        }

        public void CompleteAdding()
        {
            documentCollection.CompleteAdding();
        }

        public void Add(DataSlice slice)
        {
            documentCollection.Add(slice);
        }

        public IEnumerable<DataSlice> GetConsumingEnumerable(CancellationToken token)
        {

            return documentCollection.GetConsumingEnumerable(token);

        }

        public void Clear()
        {
            try
            {
                _cancelToken.Cancel();
            }
            catch (Exception exp)
            { }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            try
            {
                if (documentCollection != null)
                {
                    documentCollection.Dispose();
                }

                if (_cancelToken != null)
                    _cancelToken.Dispose();
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("RecoveryPersistenceQueue.Dispose()", exp.ToString());
            }
        }
        #endregion


    }
}
