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
using System.Collections;
using System;
using System.IO;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    public class StateTxfrInfo :ICompactSerializable
    {
        public ICollection data;
        public bool transferCompleted;
        public bool loggedData;
        //private Array _payLoad;
        //private ArrayList _payLoadCompilationInformation;
        private long sendDataSize;
        //private Stream stream;
    
        public StateTxfrInfo(bool transferCompleted)
        {
            this.transferCompleted = transferCompleted;
            data = null;
        }

        public StateTxfrInfo(ICollection data, bool transferCompleted, long dataSize)//, Stream st)
        {
            this.data = data;
            this.transferCompleted = transferCompleted;
            this.sendDataSize = dataSize;
          //  this.stream = st;
        }

        //public StateTxfrInfo(Hashtable data,Array payLoad,ArrayList payLoadCompInfo, bool transferCompleted)
        //{
        //    this.data = data;
        //    this.transferCompleted = transferCompleted;
        //    _payLoad = payLoad;
        //    _payLoadCompilationInformation = payLoadCompInfo;
        //}

        //public StateTxfrInfo(Hashtable data, Array payLoad, ArrayList payLoadCompInfo, bool transferCompleted, long dataSize, Stream st)
        //{
        //    this.data = data;
        //    this.transferCompleted = transferCompleted;
        //    _payLoad = payLoad;
        //    _payLoadCompilationInformation = payLoadCompInfo;
        //    this.sendDataSize = dataSize;
        //    this.stream = st;
        //}


        //public Stream SerlizationStream
        //{
        //    get { return this.stream; }
        //}

        public long DataSize
        {
            get { return sendDataSize; }
        }

        //public Array PayLoad
        //{
        //    get { return _payLoad; }
        //}

        //public ArrayList PayLoadCompilationInfo
        //{
        //    get { return _payLoadCompilationInformation; }
        //}

        #region ICompactSerializable Members

        void ICompactSerializable.Deserialize(CompactReader reader)
        {
            data = (ICollection)reader.ReadObject();
            transferCompleted = reader.ReadBoolean();
            //_payLoadCompilationInformation = reader.ReadObject() as ArrayList;
            this.sendDataSize = reader.ReadInt64();
        }

        void ICompactSerializable.Serialize(CompactWriter writer)
        {
            writer.WriteObject(data);
            writer.Write(transferCompleted);
            //writer.WriteObject(_payLoadCompilationInformation);
            writer.Write(this.sendDataSize);
        }

        #endregion
    }
}