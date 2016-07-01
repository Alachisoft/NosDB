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
using Alachisoft.NosDB.Common.Serialization;
using System;
using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{

    public class CachingConfiguration : ICloneable, ICompactSerializable
    {
        private long _cacheSpace = 536870912;
        private string _cachePolicy;

        [ConfigurationAttribute("size")]
        [JsonProperty(PropertyName = "CacheSpace")]
        public long CacheSpace
        {
            get { return _cacheSpace; }
            set
            {
                if (value < MiscUtil.DEFAULT_CACHE_SPACE)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                        LoggerManager.Instance.StorageLogger.Warn("CachingConfiguration.CacheSpace ","Cache Space was less than Minimum Cache Space. Setting it to default " + MiscUtil.DEFAULT_CACHE_SPACE);
                    _cacheSpace = MiscUtil.DEFAULT_CACHE_SPACE;
                }
                else
                {
                    _cacheSpace = value;
                }
            }
        }

        [ConfigurationAttribute("policy")]
        [JsonProperty(PropertyName = "CachePolicy")]
        public string CachePolicy
        {
            get { return _cachePolicy; }
            set { _cachePolicy = value; }
        }



        #region ICloneable Member
        public object Clone()
        {
            CachingConfiguration configuration = new CachingConfiguration();
            configuration.CacheSpace = CacheSpace;
            configuration.CachePolicy = CachePolicy;
            return configuration;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            CacheSpace = reader.ReadInt64();
            CachePolicy = reader.ReadObject() as string;

        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(CacheSpace);
            writer.WriteObject(CachePolicy);

        }
        #endregion

        public static void ValidateConfiguration(CachingConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Caching Configuration cannot be null.");

            if (!string.IsNullOrEmpty(configuration.CachePolicy))
            {
                if (!configuration.CachePolicy.Equals("FCFS", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Invalid Cache Policy '" + configuration.CachePolicy + "' specified.");
            }
            else
                configuration.CachePolicy = "FCFS";
        }
    }

}

