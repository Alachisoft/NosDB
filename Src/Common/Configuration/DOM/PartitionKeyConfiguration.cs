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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class PartitionKeyConfiguration: ICloneable, ICompactSerializable
    {
        private Dictionary<string,PartitionKeyConfigurationAttribute> _partitionKeyConfs = new Dictionary<string, PartitionKeyConfigurationAttribute>();
        
        [ConfigurationSection("attribute")]
        [JsonProperty(PropertyName = "attribute")]
        public Dictionary<string, PartitionKeyConfigurationAttribute> PartitionKeyAttributes
        {
            get { return _partitionKeyConfs; }
            set { _partitionKeyConfs = value; }
        }

        #region ICloneable Members
        public object Clone()
        {
            PartitionKeyConfiguration partitionKeyConfiguration = new PartitionKeyConfiguration();
            partitionKeyConfiguration.PartitionKeyAttributes = PartitionKeyAttributes != null ? new Dictionary<string, PartitionKeyConfigurationAttribute>(_partitionKeyConfs) : null;
            return partitionKeyConfiguration;
        }
        #endregion

        #region ICompactSerializable Methods
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            PartitionKeyAttributes = SerializationUtility.DeserializeDictionary<string, PartitionKeyConfigurationAttribute>(reader);
            //PartitionKeyAttributes = reader.ReadObject() as PartitionKeyConfigurationAttribute[];
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeDictionary<string, PartitionKeyConfigurationAttribute>(_partitionKeyConfs, writer);
            //writer.WriteObject(PartitionKeyAttributes);
        }
        #endregion

        public static void ValidateConfiguration(PartitionKeyConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("PartitionKey Configuration cannot be null.");
            
            foreach (var pair in configuration.PartitionKeyAttributes)
            {
                if (pair.Key == null)
                    throw new Exception("ParitionKey Attribute Key cannot be null.");
                if (pair.Key.Trim() == "")
                    throw new Exception("PartitionKey Attribute Key cannot be empty string.");

                PartitionKeyConfigurationAttribute.ValidateConfiguration(pair.Value);
            }
        }
    }
}
