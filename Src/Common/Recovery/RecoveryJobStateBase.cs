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
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Recovery
{
    /// <summary>
    /// Base class representing execution state of a particular recoverable entity
    /// </summary>
    public class RecoveryJobStateBase : ICompactSerializable
    {
        private string _identifier;
        private string _message;
        private RecoveryStatus _status;
        private float _percentageExecution;
        private string _entityName;
        private DateTime _submissionTime;
        private DateTime _startTime;
        private DateTime _stopTime;
        private DateTime _messageTime;
        // these properties are only set incase oplog job
        private OperationId _lastOperationID = null;
        private DateTime _lastFullBackupDate;
        
        public RecoveryJobStateBase()
        { }

        public RecoveryJobStateBase(string identifier, string name)
        {
            if (!string.IsNullOrEmpty(identifier))
            {
                _submissionTime = DateTime.Now;
                _identifier = identifier;
                _status = RecoveryStatus.uninitiated;
                if (!string.IsNullOrEmpty(name))
                    _entityName = name;
                else
                    throw new ArgumentNullException();
            }
            else
                throw new ArgumentNullException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            RecoveryJobStateBase _val = obj as RecoveryJobStateBase;
            
            if (_identifier.Equals(_val.Identifier) && _entityName.Equals(_val._entityName))
            {
                return true;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #region properties
        public virtual OperationId LastOperationID
        {
            get { return _lastOperationID; }
            set { _lastOperationID = value; }
        }

        public virtual DateTime LastFullBackupDate
        {
            get { return _lastFullBackupDate; }
            set { _lastFullBackupDate = value; }
        }

        public DateTime MessageTime
        {
            get { return _messageTime; }
            set { _messageTime = value; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime StopTime
        {
            get { return _stopTime; }
            set { _stopTime = value; }
        }

        public DateTime SubmissionTime
        {
            get { return _submissionTime; }
            set { _submissionTime = value; }
        }

        public string Name
        {
            get { return _entityName; }
            set { _entityName = value; }
        }

        public virtual string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public virtual RecoveryStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public virtual float PercentageExecution
        {
            get { return _percentageExecution; }
            set { _percentageExecution = value; }
        }

        public virtual string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }
        #endregion

        #region ICompactSerializable
        public virtual void Deserialize(Serialization.IO.CompactReader reader)
        {
            Identifier = reader.ReadString();
            Message = reader.ReadString();
            Status = (RecoveryStatus)reader.ReadObject();
            PercentageExecution = reader.ReadSingle();
            _entityName = reader.ReadString();
            StartTime = reader.ReadDateTime();
            _submissionTime = reader.ReadDateTime();
            StopTime = reader.ReadDateTime();
            _lastOperationID = (OperationId)reader.ReadObject();
            _lastFullBackupDate = reader.ReadDateTime();
        }

        public virtual void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(Message);
            writer.WriteObject(Status);
            writer.Write(PercentageExecution);
            writer.Write(_entityName);
            writer.Write(StartTime);
            writer.Write(SubmissionTime);
            writer.Write(StopTime);
            writer.WriteObject(_lastOperationID);
            writer.Write(_lastFullBackupDate);
        } 
        #endregion
    }
}
