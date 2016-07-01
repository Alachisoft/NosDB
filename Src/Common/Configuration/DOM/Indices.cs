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
    public class Indices : ICloneable, ICompactSerializable
    {
        private Dictionary<string, IndexConfiguration> _indexConfs = new Dictionary<string, IndexConfiguration>(StringComparer.InvariantCultureIgnoreCase);

        [ConfigurationSection("index")]
        [JsonProperty(PropertyName = "IndexConfigurations")]
        public Dictionary<string, IndexConfiguration> IndexConfigurations
        {
            get { return _indexConfs; }
            set { _indexConfs = value; }
        }

        public object Clone()
        {
            Indices indices = new Indices();
            indices.IndexConfigurations = IndexConfigurations != null ? IndexConfigurations.Clone<string,IndexConfiguration>(): null;

            return indices;
        }

        public void AddIndex(IndexConfiguration index)
        {
            lock (IndexConfigurations)
            {
                IndexConfigurations.Add(index.IndexName, index);
            }
        }

        public void AddIndex(string name, IndexConfiguration index)
        {
            lock (IndexConfigurations)
            {
                IndexConfigurations.Add(name, index);
            }
        }

        public void RemoveIndex(string name)
        {
            lock (IndexConfigurations)
            {
                IndexConfigurations.Remove(name);
            }
        }

        public bool ContainsIndex(string name)
        {
            return IndexConfigurations.ContainsKey(name);
        }

        public bool ContainsIndexWithUID(string uid)
        {
            lock (IndexConfigurations)
            {
                foreach (KeyValuePair<string, IndexConfiguration> index in IndexConfigurations)
                {
                    if (index.Value.UID == uid)
                        return true;
                }
                return false;
            }
        }

        public IndexConfiguration GetIndex(string name)
        {
            lock (IndexConfigurations)
            {
                if (IndexConfigurations.ContainsKey(name))
                    return IndexConfigurations[name];
                return null;
            }
        }

        public IndexConfiguration GetIndexWithUID(string uid)
        {
            lock (IndexConfigurations)
            {
                foreach (KeyValuePair<string, IndexConfiguration> index in IndexConfigurations)
                {
                    if (index.Value.UID == uid)
                        return index.Value;
                }
                return null;
            }
        }

        public static void ValidateConfiguration(Indices configuration)
        {
            if (configuration == null)
                throw new Exception("Indices cannot be null.");
            foreach (var pair in configuration.IndexConfigurations)
            {
                if (pair.Key == null)
                    throw new Exception("Index Name cannot be null.");
                if (pair.Key.Trim() == "")
                    throw new Exception("Index Name cannot be empty string.");

                IndexConfiguration.ValidateConfiguration(pair.Value);
            }
        }

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            IndexConfigurations = SerializationUtility.DeserializeDictionary<string, IndexConfiguration>(reader);
            //IndexConfigurations = reader.ReadObject() as IndexConfiguration[];
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeDictionary<string, IndexConfiguration>(_indexConfs, writer);
            //writer.WriteObject(IndexConfigurations);
        }
        #endregion
    }
}