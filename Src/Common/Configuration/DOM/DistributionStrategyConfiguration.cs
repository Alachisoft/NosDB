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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class DistributionStrategyConfiguration : ICloneable, ICompactSerializable
    {
        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [ConfigurationAttribute("shard-key-type")]
        [JsonProperty(PropertyName = "ShardKeyType")]
        public string ShardKeyType { get; set; }

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            ShardKeyType = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.WriteObject(ShardKeyType);
        }
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            DistributionStrategyConfiguration distributionStrategyConfig = new DistributionStrategyConfiguration();
            distributionStrategyConfig.Name = Name;
            distributionStrategyConfig.ShardKeyType = ShardKeyType;
            return distributionStrategyConfig;
        }
        #endregion

        public static void ValidateConfiguration(DistributionStrategyConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Distribution Strategy cannot be null.");
            if(configuration.Name == null)
                throw new Exception("Distribution Strategy Name cannot be null.");
            if (configuration.Name.Trim() == "")
                throw new Exception("Distribution Strategy Name cannot be empty string.");

            if (!configuration.Name.Equals(DistributionType.NonSharded.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid Distribution Type '" + configuration.Name + "' specified.");
        }

        public static void ConvertToType(DocumentKey rangeStart, DocumentKey rangeEnd, string shardKeyType)
        {
            switch (shardKeyType)
            {
                case "int":
                case "long":
                    rangeStart.Value = long.Parse(rangeStart.Value.ToString());
                    rangeEnd.Value = long.Parse(rangeEnd.Value.ToString());
                    if (long.Parse(rangeStart.Value.ToString()) > long.Parse(rangeEnd.Value.ToString()))
                        throw new Exception("Start point of range should be smaller than end point of range for shard");
                    break;
                case "float":
                case "double":
                    rangeStart.Value = double.Parse(rangeStart.Value.ToString());
                    rangeEnd.Value = double.Parse(rangeEnd.Value.ToString());
                    if (double.Parse(rangeStart.Value.ToString()) > double.Parse(rangeEnd.Value.ToString()))
                        throw new Exception("Start point of range should be smaller than end point of range for shard");
                    break;
                case "datetime":
                    rangeStart.Value = DateTime.Parse(rangeStart.Value.ToString());
                    rangeEnd.Value = DateTime.Parse(rangeEnd.Value.ToString());
                    if (DateTime.Parse(rangeStart.Value.ToString()).CompareTo(DateTime.Parse(rangeEnd.Value.ToString())) == 1)
                        throw new Exception("Start point of range should be smaller than end point of range for shard");
                    break;
                case "char":
                case "string":
                    rangeStart.Value = rangeStart.Value.ToString();
                    rangeEnd.Value = rangeEnd.Value.ToString();
                    if (rangeStart.Value.ToString().CompareTo(rangeEnd.Value.ToString()) == 1)
                        throw new Exception("Start point of range should be smaller than end point of range for shard");
                    break;
                //default:
                //throw new Exception("At Range.cs: Invalid ShardKeyType!");
            }
        }
    }
}
