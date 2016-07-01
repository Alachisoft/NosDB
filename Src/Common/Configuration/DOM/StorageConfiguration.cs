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

using Alachisoft.NosDB.Common.Configuration.DOM;
using Newtonsoft.Json;


namespace Alachisoft.NosDB.Common.Configuration.DOM
{

    public class StorageConfiguration : ICloneable, ICompactSerializable
    {
        private CollectionConfigurations _collConfs = new CollectionConfigurations();

        [ConfigurationSection("collections")]
        [JsonProperty(PropertyName = "Collections")]
        public CollectionConfigurations Collections
        {
            get { return _collConfs; }
            set { _collConfs = value; }
        }
        
        [ConfigurationSection("provider")]
        [JsonProperty(PropertyName = "StorageProvider")]
        public StorageProviderConfiguration StorageProvider { get; set; }

        [ConfigurationSection("cache")]
        [JsonProperty(PropertyName = "CacheConfiguration")]
        public CachingConfiguration CacheConfiguration { get; set; }


        #region ICloneable Member
        public object Clone()
        {
            StorageConfiguration sConfiguration = new StorageConfiguration();
            sConfiguration.Collections = Collections != null ? (CollectionConfigurations)Collections.Clone() : null;
            sConfiguration.StorageProvider = StorageProvider !=null ? (StorageProviderConfiguration)StorageProvider.Clone() : null;
            sConfiguration.CacheConfiguration = CacheConfiguration != null? (CachingConfiguration) CacheConfiguration.Clone(): null;
            return sConfiguration;
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Collections = reader.ReadObject() as CollectionConfigurations;
            StorageProvider = reader.ReadObject() as StorageProviderConfiguration;
            CacheConfiguration = reader.ReadObject() as CachingConfiguration;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Collections);
            writer.WriteObject(StorageProvider);
            writer.WriteObject(CacheConfiguration);
        } 
        #endregion

        public static void ValidateConfiguration(StorageConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Storage Configuration cannot be null.");
            if (configuration.Collections != null)
                CollectionConfigurations.ValidateConfiguration(configuration.Collections);

            StorageProviderConfiguration.ValidateConfiguration(configuration.StorageProvider);

            if (configuration.CacheConfiguration != null)
                CachingConfiguration.ValidateConfiguration(configuration.CacheConfiguration);
        }
    }
}
