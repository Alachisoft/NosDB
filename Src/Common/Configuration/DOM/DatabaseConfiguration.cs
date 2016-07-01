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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class DatabaseConfiguration : ICloneable, ICompactSerializable, IObjectId
    {
        string _name = "";

        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name.ToLower(); } set { _name = value; } }


        [ConfigurationSection("storage")]
        [JsonProperty(PropertyName = "Storage")]
        public StorageConfiguration Storage { get; set; }


        [ConfigurationAttribute("UID")]
        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationSection("Mode")]
        [JsonProperty(PropertyName = "mode")]
        public DatabaseMode Mode { get; set; }

        [ConfigurationSection("DatabaseType")]
        [JsonProperty(PropertyName = "databasetype")]
        public DatabaseType Type { get; set; }

      

        #region ICloneable Members
        public object Clone()
        {
            DatabaseConfiguration dConfiguration = new DatabaseConfiguration();
            dConfiguration.Storage = Storage != null? Storage.Clone() as StorageConfiguration: null;
            dConfiguration.Name = Name;

            dConfiguration.UID = UID;
            dConfiguration.Mode = Mode;
            dConfiguration.Type = Type;
            
            return dConfiguration;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Storage = reader.ReadObject() as StorageConfiguration;
            Name = reader.ReadObject() as string;

            UID = reader.ReadObject() as string;
            Mode = (DatabaseMode)reader.ReadInt32();
            Type = (DatabaseType)reader.ReadInt32();
            
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Storage);
            writer.WriteObject(Name);

            writer.WriteObject(UID);
            writer.Write((int)Mode);
            writer.Write((int)Type);
        }
        #endregion

        public static void ValidateConfiguration(DatabaseConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Database Configuration cannot be null.");
            if (configuration.Name == null)
                throw new Exception("Database Name cannot be null.");
            if (configuration.Name.Trim() == "")
                throw new Exception("Database Name cannot be empty string.");

            StorageConfiguration.ValidateConfiguration(configuration.Storage);

        }
    }
}
