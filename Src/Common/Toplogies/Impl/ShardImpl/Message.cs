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
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl
{
 //   [Serializable]
    public class Message:ICompactSerializable
    {
        public MessageType MessageType { get; set; }
        public Object Payload { get; set; }
        public bool NeedsResponse { get; set; }

        public virtual void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            MessageType =(MessageType) reader.ReadInt32();
            Payload = reader.ReadObject();
        }

        public virtual void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write((int)MessageType);
            writer.WriteObject(Payload);
        }
    }
}
