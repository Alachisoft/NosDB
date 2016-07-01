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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Core.Configuration;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration
{

    [ConfigurationRoot("database-config")]
    public  class ClusterConfiguration : ICloneable, ICompactSerializable ,IObjectId
    {
        string _name = "";
        private string _displayName = "";
        private DatabaseConfigurations _databaseConfs = new DatabaseConfigurations();

        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name !=null? _name.ToLower():null; } set { _name = value; } }

        [ConfigurationAttribute("display-name")]
        [JsonProperty(PropertyName = "DisplayName")]
        public string DisplayName { get { return _displayName != null ? _displayName.ToLower() : null; } set { _displayName = value; } }

        [JsonProperty(PropertyName = "_key")]
        public string JsonDocumetId
        {
            get { return  Name; }
            set { Name = value; }
        }


        
        [ConfigurationAttribute("UID")]
        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationSection("deployment")]
        [JsonProperty(PropertyName = "Deployment")]
        public DeploymentConfiguration Deployment { get; set; }

        [ConfigurationSection("databases")]
        [JsonProperty(PropertyName = "Databases")]
        public DatabaseConfigurations Databases
        {
            get { return _databaseConfs; }
            set { _databaseConfs = value; }
        }


        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            DisplayName = reader.ReadObject() as string;
            Deployment = reader.ReadObject() as DeploymentConfiguration;
            Databases = reader.ReadObject() as DatabaseConfigurations;
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.WriteObject(DisplayName);
            writer.WriteObject(Deployment);
            writer.WriteObject(Databases);
            writer.WriteObject(UID);
        } 
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            ClusterConfiguration cConfiguration = new ClusterConfiguration();
            cConfiguration.Name = Name;
            cConfiguration.DisplayName = DisplayName;
            cConfiguration.Deployment = Deployment != null ? Deployment.Clone() as DeploymentConfiguration : null;
            cConfiguration.Databases = Databases != null ? (DatabaseConfigurations)Databases.Clone() : null;
            cConfiguration.UID = UID;
            return cConfiguration;
        } 
        #endregion

        
    }
}
