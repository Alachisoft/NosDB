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
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Communication
{
    [Serializable]
    public class ConnectInfo /*: ICompactSerializable*/
    {
        private ConnectStatus _connectStatus;
        private int _id;

        //public ConnectInfo()
        //{
        //    _connectStatus = ConnectStatus.CONNECT_FIRST_TIME;
        //}

        //public ConnectInfo(ConnectStatus connectStatus, int id)
        //{
        //    this._connectStatus = connectStatus;
        //    this._id = id;
        //}

        public ConnectStatus Status
        {
            get { return _connectStatus; }
            set { _connectStatus = value; }
        }


        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        //public void Deserialize(Common.Serialization.IO.CompactReader reader)
        //{
        //    _connectStatus = (ConnectStatus)reader.ReadInt32();
        //    _id = reader.ReadInt32();
        //}

        //public void Serialize(Common.Serialization.IO.CompactWriter writer)
        //{
        //    writer.Write((int)_connectStatus);
        //    writer.Write(_id);
        //}

        public enum ConnectStatus
        {
            CONNECT_FIRST_TIME = 0,
            RECONNECTING = 1,
        }

        //public object Clone()
        //{
        //    ConnectInfo clone = new ConnectInfo();
        //    clone.Status = Status;
        //    clone.Id = Id;
        //    return clone;
        //}
        //}
    }
}
