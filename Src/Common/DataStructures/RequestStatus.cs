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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class RequestStatus : Common.Serialization.ICompactSerializable, ISizable
    {
        int _status;
        IList<ClusteredArray<byte>> _requestResult;

        public RequestStatus() { }

        public RequestStatus(int status)
        {
            this._status = status;
        }

        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public IList<ClusteredArray<byte>> RequestResult
        {
            get { return _requestResult; }
            set { _requestResult = value; }
        }
        
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _status = reader.ReadInt32();
            bool isResultAvailable = reader.ReadBoolean();
            if (isResultAvailable)
            {
                int count = reader.ReadInt32();
                _requestResult = new ClusteredList<ClusteredArray<byte>>(count);
                for (int i = 0; i < count; i++)
                {
                    _requestResult.Add(SerializationUtility.DeserializeClusteredArray<byte>(reader));
                }
            }
            else
            {
                _requestResult = null;
            }

        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(_status);
            if (_requestResult == null)
                writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(_requestResult.Count);
                foreach (ClusteredArray<byte> value in _requestResult)
                {
                    SerializationUtility.SerializeClusteredArray(value, writer);
                }
            }
        }

        public int Size
        {
            get { return MemoryUtil.NetIntSize + ResponseSize; }
        }

        public int InMemorySize
        {
            get { return Size; }
        }

        public int ResponseSize
        {
            get
            {
                int size = 0;
                if (_requestResult != null)
                {
                    for (int i = 0; i < _requestResult.Count; i++)
                        size += _requestResult[i].Length;
                }
                return size;
            }
        }
    }
}
