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
using System.Text;
using System.Threading;

using Alachisoft.NosDB.Common.Threading;
using Alachisoft.NosDB.Common.Net;
using Newtonsoft.Json;
#if JAVA
using Alachisoft.TayzGrid.Runtime.Serialization;
#else
using Alachisoft.NosDB.Common.Serialization;
#endif
#if JAVA
using Alachisoft.TayzGrid.Runtime.Serialization.IO;
#else
using Alachisoft.NosDB.Common.Serialization.IO;
#endif

namespace Alachisoft.NosDB.Common.DataStructures
{
    /// <summary>
    /// Each key based on the hashcode belongs to a bucket.
    /// This class keeps the overall stats for the bucket and points to 
    /// its owner.
    /// </summary>


    public class HashMapBucket : ICompactSerializable, ICloneable
    {
        private int _bucketId;
        private string _finalShard;
        private string _currentShard;

        private Latch _stateTxfrLatch = new Latch(BucketStatus.Functional);
        private object _status_wait_mutex = new object();
        public HashMapBucket(string shard, int id)
        {
            _finalShard = _currentShard = shard;
            _bucketId = id;
            _stateTxfrLatch = new Latch(BucketStatus.Functional);
        }
        public HashMapBucket(string shard, int id, byte status)
            : this(shard, id)
        {
            Status = status;
        }

        public HashMapBucket()
        {

        }

        [JsonProperty(PropertyName = "Bid")]
        public int BucketId
        {
            get { return _bucketId; }
            set { _bucketId = value; }
        }

        [JsonIgnore]
        public object StatusWaitMutex
        {
            get { return _status_wait_mutex; }
            set { _status_wait_mutex = value; }
        }

        [JsonProperty(PropertyName = "Final")]
        public string FinalShard
        {
            get { return _finalShard; }
            set { _finalShard = value; }
        }

        [JsonProperty(PropertyName = "Current")]
        public string CurrentShard
        {
            get { return _currentShard; }
            set { _currentShard = value; }
        }

        public void WaitForStatus(string tmpOwner, byte status)
        {
            if (tmpOwner != null)
            {

                while (tmpOwner == _finalShard)
                {
                    if (_stateTxfrLatch.IsAnyBitsSet(status)) return;
                    lock (_status_wait_mutex)
                    {
                        if ((tmpOwner == _finalShard) || _stateTxfrLatch.IsAnyBitsSet(status))
                            return;
                        Monitor.Wait(_status_wait_mutex);
                    }
                }
            }
        }

        public void NotifyBucketUpdate()
        {
            lock (_status_wait_mutex)
            {
                Monitor.PulseAll(_status_wait_mutex);
            }
        }
        /// <summary>
        /// Sets the status of the bucket. A bucket can have any of the following status
        /// 1- Functional

        /// </summary>

        [JsonProperty(PropertyName = "status")]
        public byte StatusInfo
        {
            get
            {
                return _stateTxfrLatch.Status.Data;
            }
            set
            {
                switch (value)
                {
                    case BucketStatus.Functional:
 
                        if(_stateTxfrLatch==null)
                            _stateTxfrLatch=new Latch();

                        byte oldStatus = _stateTxfrLatch.Status.Data;
                        if (oldStatus == value) return;
                        _stateTxfrLatch.SetStatusBit(value, oldStatus);
                        break;
                }
            }
        }

        [JsonIgnore]
        public byte Status
        {
            get { return _stateTxfrLatch.Status.Data; }
            set
            {
                switch (value)
                {
                    case BucketStatus.Functional:
                    case BucketStatus.NeedTransfer:
                    case BucketStatus.UnderStateTxfr:
                        //these are valid status,we allow them to be set.
                        byte oldStatus = _stateTxfrLatch.Status.Data;
                        if (oldStatus == value) return;
                        _stateTxfrLatch.SetStatusBit(value, oldStatus);
                        break;
                }
            }
        }
        
        [JsonIgnore]
        public Latch StateTxfrLatch
        {
            get { return _stateTxfrLatch; }
            set { _stateTxfrLatch = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj is HashMapBucket)
            {
                return this.BucketId == ((HashMapBucket)obj).BucketId;
            }
            return false;
        }

        public virtual object Clone()
        {
            HashMapBucket hmBucket = new HashMapBucket(_currentShard, _bucketId);
            hmBucket.FinalShard = _finalShard;
            hmBucket.Status = Status;
            hmBucket.CurrentShard = CurrentShard;

            return hmBucket;
        }

        #region ICompactSerializable Members

        public virtual void Deserialize(CompactReader reader)
        {
            //Trace.error("HashMapBucket.Deserialize", "Deserialize Called");
            _bucketId = reader.ReadInt32();
            _finalShard = (string)reader.ReadObject();
            _currentShard = (string)reader.ReadObject();
            byte status = reader.ReadByte();
            _stateTxfrLatch = new Latch(status);

        }

        public virtual void Serialize(CompactWriter writer)
        {
            //Trace.error("HashMapBucket.Serialize", "Serialize Called");
            writer.Write(_bucketId);
            writer.WriteObject(_finalShard);
            writer.WriteObject(_currentShard);

            writer.Write(_stateTxfrLatch.Status.Data);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Bucket[" + _bucketId + " ; ");
            sb.Append("owner = " + _currentShard + " ; ");
            sb.Append("temp = " + _finalShard + " ; ");
            string status = null;
            //huma: object can be zero object(initialization without default values), which may cause exception.
            if (_stateTxfrLatch != null)
                status = BucketStatus.StatusToString(_stateTxfrLatch.Status.Data);
            sb.Append(status + " ]");
            return sb.ToString();

        }
    }
}
