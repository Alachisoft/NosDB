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

namespace Alachisoft.NosDB.Common.Configuration.Services
{
    public class PartitionKeyAttribute : ICloneable, ICompactSerializable
    {
        public string Name { get; set; }
        public string Type { get; set; }

        #region ICompactSerializable Methods
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadString();
            Type = reader.ReadString();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(Name);
            writer.Write(Type);
        }
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            PartitionKeyAttribute partitionKeyAttribute = new PartitionKeyAttribute();
            partitionKeyAttribute.Name = Name;
            partitionKeyAttribute.Type = Type;
            return partitionKeyAttribute;
        }
        #endregion
    }
}