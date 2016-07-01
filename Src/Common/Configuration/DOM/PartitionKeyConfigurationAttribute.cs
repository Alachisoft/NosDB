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
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class PartitionKeyConfigurationAttribute : ICloneable, ICompactSerializable
    {
        private string _name;
        private string _type;

        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [ConfigurationAttribute("type")]
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            set { _type = value; }
            get { return _type; }
        }

        #region ICompactSerializable Methods
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadString();
            Type = reader.ReadString();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_name);
            writer.Write(_type);
        }
        #endregion

        #region ICloneable Methods
        public object Clone()
        {
            PartitionKeyConfigurationAttribute partitionKeyAttribute = new PartitionKeyConfigurationAttribute();
            partitionKeyAttribute.Name = _name;
            partitionKeyAttribute.Type = _type;
            return partitionKeyAttribute;
        }
        #endregion

        public static void ValidateConfiguration(PartitionKeyConfigurationAttribute configuration)
        {
            if (configuration == null)
                throw new Exception("PartitionKey Attribute cannot be null.");
            else if (configuration.Name == null)
                throw new Exception("PartitionKey Attribute Name cannot be null.");
            else if (configuration.Name.Trim() == "")
                throw new Exception("PartitionKey Attribut Name cannot be empty string.");
            else if (configuration.Type == null)
                throw new Exception("PartitionKey Attribute Type cannot be null.");
            else if (configuration.Type.Trim() == "")
                throw new Exception("PartitionKey Attribut Type cannot be empty string.");
        }
    }
}