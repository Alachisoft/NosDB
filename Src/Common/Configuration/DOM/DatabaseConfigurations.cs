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
    public class DatabaseConfigurations : ICloneable, ICompactSerializable
    {
        private Dictionary<string, DatabaseConfiguration> _databaseConfs = new Dictionary<string, DatabaseConfiguration>(StringComparer.InvariantCultureIgnoreCase);

        [ConfigurationSection("database")]
        [JsonProperty(PropertyName = "Configurations")]
        public Dictionary<string, DatabaseConfiguration> Configurations
        {
            get { return _databaseConfs; }
            set { _databaseConfs = value; }
        }

        public void AddDatabase(DatabaseConfiguration database)
        {
            lock (_databaseConfs)
            {
                _databaseConfs.Add(database.Name, database);
            }
        }

        public void AddDatabase(string name, DatabaseConfiguration database)
        {
            lock (_databaseConfs)
            {
                _databaseConfs.Add(name, database);
            }
        }
       
        /// <summary>
        /// Replaces current DatabaseConfiguration with the one provided if database exists else adds a new database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        public void UpdateDatabase(string name,DatabaseConfiguration database)
        {

            lock (_databaseConfs)
            {
                if (ContainsDatabase(name))
                {
                    _databaseConfs[name] = database;
                }
                else
                {
                    _databaseConfs.Add(name, database);
                }
            }
        }

        public void RemoveDatabase(string name)
        {
            lock (_databaseConfs)
            {
                _databaseConfs.Remove(name);
            }
        }

        public bool ContainsDatabase(string name)
        {
            return _databaseConfs.ContainsKey(name);
        }

        public DatabaseConfiguration GetDatabase(string name)
        {
            lock (_databaseConfs)
            {
                if (_databaseConfs.ContainsKey(name))
                    return _databaseConfs[name];
                return null;
            }
        }

        #region ICloneable Member
        public object Clone()
        {
            DatabaseConfigurations configuration = new DatabaseConfigurations();
            configuration.Configurations = Configurations != null ? Configurations.Clone<string,DatabaseConfiguration>()  : null;

            return configuration;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Configurations = SerializationUtility.DeserializeDictionary<string, DatabaseConfiguration>(reader);
            //Configurations = reader.ReadObject() as DatabaseConfiguration[];
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeDictionary<string, DatabaseConfiguration>(_databaseConfs, writer);
            //writer.WriteObject(Configurations);
        }
        #endregion
    }
}