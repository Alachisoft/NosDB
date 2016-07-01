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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Recovery
{
    /// <summary>
    /// OBject representing execution state of a clustered recovery job
    /// </summary>
    public class ClusteredRecoveryJobState : RecoveryJobStateBase
    {
        List<ShardRecoveryJobState> _jobList = null;

        private string _identifier;
        private RecoveryStatus _status;
        private string _message;
        private float _percentage;

        public ClusteredRecoveryJobState(string identifier)
        {
            if (!string.IsNullOrEmpty(identifier))
                _identifier = identifier;
            _jobList = new List<ShardRecoveryJobState>();
            _status = RecoveryStatus.uninitiated;
            _message = string.Empty;
            _percentage = 0;
            base.SubmissionTime = DateTime.Now;
        }

        #region Helper Methods
        public void UpdateJobStatus(ShardRecoveryJobState job)
        {
            if (_jobList.Contains(job))
            {
                // change this logic
                _jobList.Remove(job);// remove existing


            }

            _jobList.Add(job);// add new

            //M_TODO[Critical]: Update recover
            // check if  exists
            //run update utility
            // else
            // add to the original
        }

        public override string ToString()
        {
            string message = Identifier + " : " + PercentageExecution + " : " + Name + " : " + MessageTime + " : " + Status + " : ";
            foreach (ShardRecoveryJobState _shardJob in _jobList)
            {
                message += "\t" + _shardJob.ToString();
            }
            return message;
        }
        #endregion

        #region overridden propeties
        public List<ShardRecoveryJobState> Details
        {
            get
            {
                return _jobList;
            }
            set
            {
                _jobList = value;
            }

        }

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
                var _failed = _jobList.Where(x => (x.Status == RecoveryStatus.Failure)).FirstOrDefault();
                var _cancelled = _jobList.Where(x => (x.Status == RecoveryStatus.Cancelled)).FirstOrDefault();
                var _running = _jobList.Where(x => (x.Status == RecoveryStatus.Executing)).FirstOrDefault();
                var _waiting = _jobList.Where(x => (x.Status == RecoveryStatus.Waiting || x.Status == RecoveryStatus.Submitted)).FirstOrDefault();
                var _success = _jobList.Where(x => (x.Status == RecoveryStatus.Success || x.Status == RecoveryStatus.Completed)).FirstOrDefault();
                var _uninititaed = _jobList.Where(x => (x.Status == RecoveryStatus.uninitiated)).FirstOrDefault();

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
                    if (_uninititaed != null && _success != null)
                    {
                        _status = RecoveryStatus.Executing;
                    }
                    else
                        _status = RecoveryStatus.uninitiated;
                }
                else
                    _status = RecoveryStatus.Completed;

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
                if (_jobList.Count > 0)
                {
                    float percentage = 0;

                    foreach (ShardRecoveryJobState entity in _jobList)
                        percentage += entity.PercentageExecution;

                    _percentage = percentage / _jobList.Count;
                }

                return _percentage;
            }
            set
            {
                _percentage = value;
            }
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
            Details = Common.Util.SerializationUtility.DeserializeList<ShardRecoveryJobState>(reader);
        }

        public override void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(Message);
            writer.Write((int)Status);
            writer.Write(PercentageExecution);
            writer.Write(MessageTime);
            writer.Write(StopTime);
            Common.Util.SerializationUtility.SerializeList<ShardRecoveryJobState>(Details, writer);
        }
        #endregion
    }
}
