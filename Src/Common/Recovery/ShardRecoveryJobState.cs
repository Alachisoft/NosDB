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
using Alachisoft.NosDB.Common.Replication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Recovery
{
    /// <summary>
    /// Object representing execution state of a particular job
    /// </summary>
    public class ShardRecoveryJobState : RecoveryJobStateBase
    {
        List<RecoveryJobStateBase> _entityList = null;
        private string _identifier;
        private string _shard = string.Empty;
        private string _cluster = string.Empty;
        private string _node = string.Empty;
        private RecoveryJobType _jobType;
        private RecoveryStatus _status;
        private string _message;
        private float _percentage = 0;

        // these properties are only set incase databackup job
        private OperationId _lastOperationID = null;
        private DateTime _lastFullBackupDate;

        public ShardRecoveryJobState(string identifier, string shard, string node, string cluster, RecoveryJobType jobType)
        {
            if (!string.IsNullOrEmpty(identifier))
                _identifier = identifier;
            if (!string.IsNullOrEmpty(shard))
                _shard = shard;
            if (!string.IsNullOrEmpty(node))
                _node = node;
            if (!string.IsNullOrEmpty(cluster))
                _cluster = cluster;

            _jobType = jobType;
            _entityList = new List<RecoveryJobStateBase>();
            _status = RecoveryStatus.uninitiated;
            _message = string.Empty;
            _percentage = 0;
        }

        public void UpdateEntityState(RecoveryJobStateBase entity)
        {

            if (_entityList.Contains(entity))
            {
                // change this logic
                _entityList.Remove(entity);// remove existing

            }
            _entityList.Add(entity);
        }

        #region public properties

        public string Node
        {
            get { return _node; }
            set { _node = value; }
        }

        public string Cluster
        {
            get { return _cluster; }
            set { _cluster = value; }
        }

        public string Shard
        {
            get { return _shard; }
            set { _shard = value; }
        }

        public List<RecoveryJobStateBase> Detail
        {
            get { return _entityList; }
            set { _entityList = value; }
        }

        public RecoveryJobType JobType
        {
            get { return _jobType; }
            set { _jobType = value; }
        }
        #endregion

        #region overridden propeties
        public override string Identifier
        {
            get
            {
                return _identifier;
            }
            set
            {
                _identifier = value;
            }
        }

        public override string Message
        {
            get
            {
                _message = string.Empty;

                if (Status == RecoveryStatus.Failure)
                    _message = "Recovery job has failed";
                else if (Status == RecoveryStatus.Waiting)
                    _message = "Recovery job is in waiting";
                else if (Status == RecoveryStatus.Executing)
                    _message = "Recovery job is in execution";
                else
                    _message = "Recovery job has completed";

                return _message;
            }
            set
            {
                _message = value;
            }
        }

        public override RecoveryStatus Status
        {
            get
            {
                if (_entityList.Count > 0)
                {
                    var _failed = _entityList.Where(x => (x.Status == RecoveryStatus.Failure)).FirstOrDefault();
                    var _cancelled = _entityList.Where(x => (x.Status == RecoveryStatus.Cancelled)).FirstOrDefault();

                    var _running = _entityList.Where(x => (x.Status == RecoveryStatus.Executing)).FirstOrDefault();
                    var _waiting = _entityList.Where(x => (x.Status == RecoveryStatus.Waiting || x.Status == RecoveryStatus.Submitted)).FirstOrDefault();
                    var _success = _entityList.Where(x => (x.Status == RecoveryStatus.Success || x.Status == RecoveryStatus.Completed)).FirstOrDefault();
                    var _uninititaed = _entityList.Where(x => (x.Status == RecoveryStatus.uninitiated)).FirstOrDefault();

                    if (_failed != null)
                        _status = RecoveryStatus.Failure;
                    else if (_cancelled != null)
                        _status = RecoveryStatus.Cancelled;
                    else if (_waiting != null)
                        _status = RecoveryStatus.Waiting;
                    else if (_running != null)
                        _status = RecoveryStatus.Executing;
                    else if (_uninititaed != null)
                    {
                        _status = RecoveryStatus.uninitiated;
                    }
                    else
                        _status = RecoveryStatus.Completed;
                    // incase no node has failed,is in waiting,cancelled or running return complete
                }
                return _status;


            }
            set
            {
                _status = value;
            }

        }

        public override float PercentageExecution
        {
            get
            {
                if (_entityList.Count > 0)
                {
                    float percentage = 0;

                    foreach (RecoveryJobStateBase entity in _entityList)
                        percentage += entity.PercentageExecution;
                    return (percentage / _entityList.Count) * 100;
                }
                else
                {
                    return _percentage;
                }
            }
            set
            {
                _percentage = value;
            }
        }

        public override OperationId LastOperationID
        {
            get { return _lastOperationID; }
            set { _lastOperationID = value; }
        }

        public override DateTime LastFullBackupDate
        {
            get { return _lastFullBackupDate; }
            set { _lastFullBackupDate = value; }
        }
        #endregion

        #region overriden methods
        public override bool Equals(object obj)
        {
            bool state = false;
            if (obj == null || GetType() != obj.GetType())
                return false;
            ShardRecoveryJobState _val = obj as ShardRecoveryJobState;

            if (_identifier.Equals(_val.Identifier))
                if (_shard.Equals(_val.Shard))
                    if (_node.Equals(_val._node))
                        if (_cluster.Equals(_val._cluster))
                        {
                            if (_jobType == _val._jobType)
                                state = true;
                        }

            return state;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            string message = Identifier + " : " + PercentageExecution + " : " + Name + " : " + Node + " : " + MessageTime + " : " + Status + " : " + _jobType;
            return message;
        }

        #endregion

        #region ICompactSerializable
        public override void Deserialize(Serialization.IO.CompactReader reader)
        {
            Identifier = reader.ReadString();
            Message = reader.ReadString();
            Status = (RecoveryStatus)reader.ReadInt32();
            PercentageExecution = reader.ReadSingle();
            MessageTime = reader.ReadDateTime();
            StopTime = reader.ReadDateTime();
            _cluster = reader.ReadString();
            _shard = reader.ReadString();
            _node = reader.ReadString();
            _jobType = (RecoveryJobType)reader.ReadInt32();
            Detail = Common.Util.SerializationUtility.DeserializeList<RecoveryJobStateBase>(reader);
            _lastOperationID = (OperationId)reader.ReadObject();
            _lastFullBackupDate = reader.ReadDateTime();
        }

        public override void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(Message);
            writer.Write((int)Status);
            writer.Write(PercentageExecution);
            writer.Write(MessageTime);
            writer.Write(StopTime);
            writer.Write(_cluster);
            writer.Write(_shard);
            writer.Write(_node);
            writer.Write((int)_jobType);
            Common.Util.SerializationUtility.SerializeList<RecoveryJobStateBase>(Detail, writer);
            writer.WriteObject(_lastOperationID);
            writer.Write(_lastFullBackupDate);
        }
        #endregion
    }
}
