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
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Serialization.Formatters;
using CSharpTest.Net.Serialization;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class AttributeValueSerializer :ISerializer<AttributeValue>
    {
        public static AttributeValueSerializer Global = new AttributeValueSerializer();

        public AttributeValue ReadFrom(System.IO.Stream stream)
        {
            int size = PrimitiveSerializer.Int32.ReadFrom(stream);
            byte[] data = new byte[size];
            stream.Read(data, 0, size);
            AttributeValue value = CompactBinaryFormatter.FromByteBuffer(data, "") as AttributeValue;
            return value;
        }

        public void WriteTo(AttributeValue value, System.IO.Stream stream)
        {
            byte[] data = CompactBinaryFormatter.ToByteBuffer(value, "");
            int size = data.Length;
            PrimitiveSerializer.Int32.WriteTo(size, stream);
            stream.Write(data, 0, size);
        }
    }
}
