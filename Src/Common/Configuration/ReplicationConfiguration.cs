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

namespace Alachisoft.NosDB.Common.Configuration
{
    public class ReplicationConfiguration : ICloneable, ICompactSerializable
    {

        public ReplicationConfiguration()
        { }

        [ConfigurationAttribute("type")]
        [JsonProperty(PropertyName = "ReplicationType")]
        public string ReplicationType
        {
            set;
            get;
        }

        [ConfigurationAttribute("time-interval", "sec")]
        [JsonProperty(PropertyName = "ReplicationTimeInterval")]
        public int ReplicationTimeInterval
        {
            set;
            get;
        }

        [ConfigurationAttribute("bulk-size", "MB")]
        [JsonProperty(PropertyName = "ReplicationBulkSize")]
        public int ReplicationBulkSize
        {
            set;
            get;
        }

        public object Clone()
        {
            ReplicationConfiguration replicationConfiguration = new ReplicationConfiguration();
            replicationConfiguration.ReplicationType = ReplicationType;
            replicationConfiguration.ReplicationTimeInterval = ReplicationTimeInterval;
            replicationConfiguration.ReplicationBulkSize = ReplicationBulkSize;
            return replicationConfiguration;
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            ReplicationType = reader.ReadString();
            ReplicationTimeInterval = (int)reader.ReadInt32();
            ReplicationBulkSize = (int)reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(ReplicationType);
            writer.Write(ReplicationTimeInterval);
            writer.Write(ReplicationBulkSize);
        }
    }
}
