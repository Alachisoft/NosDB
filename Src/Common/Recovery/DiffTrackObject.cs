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
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Recovery
{
    /// <summary>
    /// Info object associated with database being tracked 
    /// </summary>
    public class DiffTrackObject: ICompactSerializable,IDisposable
    {
        string _database;
        OperationId _lastOperationID;       
        DateTime _lastFullBackupDate;
        string _shard;

        public DiffTrackObject(string database,string shard)
        {
            _database = database;
            _shard = shard;
        }

        #region properties
        public string Database
        {
            get { return _database; }
        }

        public string Shard
        {
            get { return _shard; }

        }

        public OperationId LastOperationID
        {
            get { return _lastOperationID; }
            set { _lastOperationID = value; }
        }

        public DateTime LastFullBackupDate
        {
            get { return _lastFullBackupDate; }
            set { _lastFullBackupDate = value; }
        }
        
        #endregion

        #region Overriden methods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {

            string opID = string.Empty;

            if(_lastOperationID != null)
               opID= _lastOperationID.ElectionBasedSequenceId + " : " + _lastOperationID.ElectionId + " : " + _lastOperationID.Id +
                " : " + _lastOperationID.TimeStamp;

            return "Shard: " + _shard + " _ " + "Database: " +_database+ " _ " + "Date: " + _lastOperationID+" _ " + "OperationID " +  opID;
        } 
        #endregion

        #region ICompact Serialization
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _database = reader.ReadString();
            _shard = reader.ReadString();
            _lastOperationID = reader.ReadObject() as OperationId;
            _lastFullBackupDate = reader.ReadDateTime();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(_database);
            writer.Write(_shard);
            writer.WriteObject(_lastOperationID);
            writer.Write(_lastFullBackupDate);
        } 
        #endregion

        #region IDisposable
        public void Dispose()
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
