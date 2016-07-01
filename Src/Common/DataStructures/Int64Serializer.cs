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
namespace Alachisoft.NosDB.Common.DataStructures
{
    public class Int64Serializer : IDataSerializer<long>
    {

        public byte[] Serialize(long value)
        {
            byte[] _data = new byte[8];
            _data[0] = (byte)(value >> 56);
            _data[1] = (byte)(value >> 48);
            _data[2] = (byte)(value >> 40);
            _data[3] = (byte)(value >> 32);
            _data[4] = (byte)(value >> 24);
            _data[5] = (byte)(value >> 16);
            _data[6] = (byte)(value >> 8);
            _data[7] = (byte)(value >> 0);
            return _data;
        }

        public long Deserialize(byte[] _data)
        {
            return unchecked((
                (_data[0] << 56) |
                (_data[1] << 48) |
                (_data[2] << 40) |
                (_data[3] << 32) |
                (_data[4] << 24) |
                (_data[5] << 16) |
                (_data[6] << 8) |
                (_data[7] << 0)
                ));
        }
    }
}