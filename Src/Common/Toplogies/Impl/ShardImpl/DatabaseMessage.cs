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
namespace Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl
{
    public class DatabaseMessage:Message
    {
        public string Database { get; set; }
        public string Collection { get; set; }
        public OpCode OpCode { get; set; }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Database);
            writer.Write(Collection);
            writer.WriteObject(OpCode);
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            base.Deserialize(reader);
            Database = reader.ReadString();
            Collection = reader.ReadString();
            OpCode = reader.ReadObjectAs<OpCode>();
        }
    }
}
