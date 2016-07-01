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
    public class Int32Serializer : IDataSerializer<int>
    {

        public byte[] Serialize(int reference)
        {
            return ToBytes(reference);
        }

        public int Deserialize(byte[] data)
        {
            return FromBytes(data);
        }

        public static byte[] ToBytes(int reference)
        {
            var data = new byte[4];
            data[0] = (byte)(reference >> 24);
            data[1] = (byte)(reference >> 16);
            data[2] = (byte)(reference >> 8);
            data[3] = (byte)(reference);
            return data;
        }

        public static int FromBytes(byte[] data)
        {
            return unchecked(((data[0] << 24) |
                              (data[1] << 16) |
                              (data[2] << 8) |
                              (data[3] << 0)));
        }

        //private long ReadInt64(int position)
        //{
        //    return unchecked((long) (
        //        (_data[position + 0] << 56) |
        //        (_data[position + 1] << 48) |
        //        (_data[position + 2] << 40) |
        //        (_data[position + 3] << 32) |
        //        (_data[position + 4] << 24) |
        //        (_data[position + 5] << 16) |
        //        (_data[position + 6] << 8) |
        //        (_data[position + 7] << 0)
        //        ));
        //}

        //private void WriteInt64(int position, long value)
        //{
        //    _data[position + 0] = (byte)(value >> 56);
        //    _data[position + 1] = (byte)(value >> 48);
        //    _data[position + 2] = (byte)(value >> 40);
        //    _data[position + 3] = (byte)(value >> 32);
        //    _data[position + 4] = (byte)(value >> 24);
        //    _data[position + 5] = (byte)(value >> 16);
        //    _data[position + 6] = (byte)(value >> 8);
        //    _data[position + 7] = (byte)(value >> 0);
        //}
    }
}