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
using System.Text.RegularExpressions;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Linq;
using System.Text;
//using CSharpTest.Net;

using Alachisoft.NosDB.Common.Server.Engine;
using Newtonsoft.Json;


namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class IndexConfiguration : ICloneable, ICompactSerializable, IIndexConfigration,IObjectId
    {
        string _name="";
        private string apparentName;
        IndexAttribute _attributes;
        private string _cachePolicy;
        private bool _journalEnabled;

        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { 
            get { return _name.ToLower(); }
            set
            {
                Regex validator = new Regex("^[_a-zA-Z0-9]*$");
                if (validator.IsMatch(value))
                    _name = value;
                else
                    throw new System.Exception("Invalid Configuration Value Exception: The index name can only be alphanumeric characters. ");
            }
        }

        [ConfigurationAttribute("UID")]
        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationSection("attribute", true, true)]
        [JsonProperty(PropertyName = "Attributes")]
        public IndexAttribute Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        [ConfigurationAttribute("cache-policy")]
        [JsonProperty(PropertyName = "CachePolicy")]
        public string CachePolicy
        {
            get { return _cachePolicy; }
            set { _cachePolicy = value; }
        }

        [ConfigurationAttribute("journal-enabled")]
        [JsonProperty(PropertyName = "JournalEnabled")]
        public bool JournalEnabled
        {
            get { return _journalEnabled; }
            set { _journalEnabled = value; }
        }

        [JsonProperty(PropertyName = "IndexName")]
        public string IndexName
        {
            get
            {
                if (apparentName != null) return apparentName;
                if (_name != null) apparentName = Name;
                else
                {
                    apparentName = _attributes.Name;
                }
                return apparentName;

            }
            set { _name = value; }
        }

        public object Clone()
        {
            IndexConfiguration config = new IndexConfiguration();
            config.Name = Name;
            config.Attributes = Attributes != null ? (IndexAttribute)Attributes.Clone(): null;
            config.CachePolicy = _cachePolicy;
            config.JournalEnabled = _journalEnabled;
            return config;
        }

        #region ICompactSerializable  Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            Attributes = reader.ReadObject() as IndexAttribute;
            CachePolicy = reader.ReadObject() as string;
            JournalEnabled = reader.ReadBoolean();
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.WriteObject(Attributes);
            writer.WriteObject(CachePolicy);
            writer.Write(JournalEnabled);
            writer.WriteObject(UID);
        }
        #endregion


        IIndexAttribute[] IIndexConfigration.Attributes
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public static void ValidateConfiguration(IndexConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Index Configuration cannot be null.");
            if (configuration.Name == null)
                throw new Exception("Index Name cannot be null.");
            if (configuration.Name.Trim() == "")
                throw new Exception("Index Name cannot be empty string.");

            IndexAttribute.ValidateConfiguration(configuration.Attributes);
            
            if(configuration.CachePolicy != null && configuration.CachePolicy.Trim() != "")
            {
                if (!configuration.CachePolicy.Equals("Recent", StringComparison.OrdinalIgnoreCase) &&
                    !configuration.CachePolicy.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                    !configuration.CachePolicy.Equals("None", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Invalid Cache Policy '" + configuration.CachePolicy + "' specified for index.");
            }

        }
    }
}
