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
using Alachisoft.NosDB.Common.Protobuf;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Enum;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class CollectionConfiguration : ICloneable, ICompactSerializable, IEquatable<CollectionConfiguration>, Common.Server.Engine.ICollectionConfiguration,IObjectId
    {
        string _name = "";
        private string _collectionType = string.Empty;
        private long _collectionSize = -1;
        private long _maxDocuments;
        private string _shard = "All";
        private string _path="";

        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "CollectionName")]
        public string CollectionName { get { return _name.ToLower(); } set { _name = value; } }
        
        [ConfigurationAttribute("UID")]
        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationAttribute("size")]
        [JsonProperty(PropertyName = "CollectionSize")]
        public long CollectionSize
        {
            get { return _collectionSize; }
            set { _collectionSize = value; }
        }

        [ConfigurationAttribute("max-documents")]
        [JsonProperty(PropertyName = "MaxDocuments")]
        public long MaxDocuments
        {
            get { return _maxDocuments; }
            set { _maxDocuments = value; }
        }

        [ConfigurationAttribute("shard")]
        [JsonProperty(PropertyName = "Shard")]
        public string Shard
        {
            get { return _shard; }
            set { _shard = value; }
        }

        [ConfigurationSection("indices")]
        [JsonProperty(PropertyName = "indices")]
        public Indices Indices { get; set; }

        [ConfigurationSection("eviction")]
        [JsonProperty(PropertyName = "EvictionConfiguration")]
        public EvictionConfiguration EvictionConfiguration { get; set; }
        
        [ConfigurationSection("caching")]
        [JsonProperty(PropertyName = "Caching")]
        public CachingConfiguration Caching { get; set; }

        [ConfigurationSection("distribution-strategy")]
        [JsonProperty(PropertyName = "DistributionStrategy")]
        public DistributionStrategyConfiguration DistributionStrategy { get; set; }

        [ConfigurationSection("partition-key")]
        [JsonProperty(PropertyName = "partition-key")]
        public PartitionKeyConfiguration PartitionKey { get; set; }

        //Used if AttachmentCollection
        [ConfigurationAttribute("path")]
        [JsonProperty(PropertyName = "Path")]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            CollectionName = reader.ReadObject() as string;
            Indices = reader.ReadObject() as Indices;
            Caching = reader.ReadObject() as CachingConfiguration;
            DistributionStrategy = reader.ReadObject() as DistributionStrategyConfiguration;
            CollectionSize = reader.ReadInt64();
            MaxDocuments = reader.ReadInt64();
            Shard = reader.ReadObject() as string;
            EvictionConfiguration = reader.ReadObject() as EvictionConfiguration;
            PartitionKey = reader.ReadObject() as PartitionKeyConfiguration;
            UID = reader.ReadObject() as string;
            Path = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(CollectionName);
            writer.WriteObject(Indices);
            writer.WriteObject(Caching);
            writer.WriteObject(DistributionStrategy);
            writer.Write(CollectionSize);
            writer.Write(MaxDocuments);
            writer.WriteObject(Shard);
            writer.WriteObject(EvictionConfiguration);
            writer.WriteObject(PartitionKey);
            writer.WriteObject(UID);
            writer.WriteObject(Path);
        }
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            CollectionConfiguration cConfiguration = new CollectionConfiguration();
            cConfiguration.Indices = Indices != null ? (Indices)Indices.Clone() : null;           
            cConfiguration.Caching = Caching != null? Caching.Clone() as CachingConfiguration: null;
            cConfiguration.CollectionSize = CollectionSize;
            cConfiguration.MaxDocuments = MaxDocuments;
            cConfiguration.Shard = Shard;
            cConfiguration.EvictionConfiguration = EvictionConfiguration != null ? (EvictionConfiguration)EvictionConfiguration.Clone() : null;
            cConfiguration.PartitionKey = PartitionKey;
            cConfiguration.Path = Path;
            return cConfiguration;
        }
        #endregion

        public bool Equals(CollectionConfiguration other)
        {
            return CollectionName.Equals(other.CollectionName);
        }

        public static void ValidateConfiguration(CollectionConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Collection Configuration cannot be null.");
            if (configuration.CollectionName == null)
                throw new Exception("Collection Name cannot be null.");
            if (configuration.CollectionName.Trim() == "")
                throw new Exception("Collection Name cannot be empty string.");
            
            //TODO Validate valid collection type and distribution configuration
            DistributionStrategyConfiguration.ValidateConfiguration(configuration.DistributionStrategy);
            
            //Optionals
            if(configuration.PartitionKey != null)
                PartitionKeyConfiguration.ValidateConfiguration(configuration.PartitionKey);
            if (configuration.Caching != null)
                CachingConfiguration.ValidateConfiguration(configuration.Caching);
            if (configuration.EvictionConfiguration != null)
                EvictionConfiguration.ValidateConfiguration(configuration.EvictionConfiguration);
            if (configuration.Indices != null)
                Indices.ValidateConfiguration(configuration.Indices);
        }
    }
}
