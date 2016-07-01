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
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class CollectionConfigurations : ICloneable, ICompactSerializable
    {
        Dictionary<string, CollectionConfiguration> _collectionConfs = new Dictionary<string, CollectionConfiguration>(StringComparer.InvariantCultureIgnoreCase);

        [ConfigurationSection("collection")]
        [JsonProperty(PropertyName = "Configuration")]
        public Dictionary<string, CollectionConfiguration> Configuration
        {
            get { return _collectionConfs; }
            set 
            {
                _collectionConfs = value; 
            }
        }

        public void AddCollection(CollectionConfiguration collection)
        {
            lock (_collectionConfs)
            {
                _collectionConfs.Add(collection.CollectionName, collection);
            }
        }

        public void AddCollection(string name, CollectionConfiguration collection)
        {
            lock (_collectionConfs)
            {
                _collectionConfs.Add(name, collection);
            }
        }

        public void RemoveCollection(string name)
        {
            lock (_collectionConfs)
            {
                _collectionConfs.Remove(name);
            }
        }

        public bool ContainsCollection(string name)
        {
            return _collectionConfs.ContainsKey(name);
        }

        public CollectionConfiguration GetCollection(string name)
        {
            lock (_collectionConfs)
            {
                if (_collectionConfs.ContainsKey(name))
                    return _collectionConfs[name];
                return null;
            }
        }

        #region ICloneable Member
        public object Clone()
        {
            CollectionConfigurations config = new CollectionConfigurations();
            config.Configuration = Configuration != null ? Configuration.Clone<string,CollectionConfiguration>(): null;

            return config; 
        } 
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Configuration = SerializationUtility.DeserializeDictionary<string, CollectionConfiguration>(reader);
            //Configuration = reader.ReadObject() as CollectionConfiguration[];
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeDictionary<string, CollectionConfiguration>(_collectionConfs, writer);
            //writer.WriteObject(Configuration);
        } 
        #endregion

        
        public static void ValidateConfiguration(CollectionConfigurations configuraiton)
        {
            if (configuraiton == null)
                throw new Exception("Collection Configuration cannot be null.");
            foreach (var pair in configuraiton.Configuration)
            {
                if (pair.Key == null)
                    throw new Exception("Collection Name cannot be null.");
                if (pair.Key.Trim() == "")
                    throw new Exception("Collection Name cannot be empty string.");

                CollectionConfiguration.ValidateConfiguration(pair.Value);
            }
        }
    }
}