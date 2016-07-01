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
using System.Linq;
using System.Text;

using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class EvictionConfiguration: ICloneable, ICompactSerializable
    {
        private bool _enabledEvication = false;
        private string _policy;


        [ConfigurationAttribute("enabled")]
        [JsonProperty(PropertyName = "EnabledEviction")]
        public bool EnabledEviction
        {
            set { _enabledEvication = value; }
            get { return _enabledEvication; }
        }

        [ConfigurationAttribute("policy")]
        [JsonProperty(PropertyName = "Policy")]
        public string Policy
        {
            set { _policy = value; }
            get { return _policy; }
        }

        public object Clone()
        {
            EvictionConfiguration evictConfiguration=new EvictionConfiguration();
            evictConfiguration.EnabledEviction = EnabledEviction;
            evictConfiguration.Policy = Policy;
            return evictConfiguration;
        }
      
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            EnabledEviction = reader.ReadBoolean();
            Policy = reader.ReadObject() as string;
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(EnabledEviction);
            writer.WriteObject(Policy);
        }

        public static void ValidateConfiguration(EvictionConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Eviction Configuration cannot be null.");
            
            //if (configuration.Policy != null)
            //{
            //TODO Eviction Policy Values LRU and ?? 
            //}

        }
    }
}
