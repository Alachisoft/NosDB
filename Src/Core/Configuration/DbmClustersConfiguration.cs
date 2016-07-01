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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Core.Configuration
{
    public class DbmClustersConfiguration : ICloneable, ICompactSerializable
    {
        List<DbmClusterConfiguration> _clusters = new List<DbmClusterConfiguration>();

        [ConfigurationSection("cluster")]
        public DbmClusterConfiguration[] ClustersConfigurations
        {
            get
            {
                if (_clusters != null)
                    return _clusters.ToArray();
                return null;
            }
            set
            {
                if (_clusters == null)
                    _clusters = new List<DbmClusterConfiguration>();
                _clusters.Clear();
               
                if(value != null)
                {
                    _clusters.AddRange(value);
                }
            }
        }



        public object Clone()
        {
            DbmClustersConfiguration configuration = new DbmClustersConfiguration();
            configuration.ClustersConfigurations = ClustersConfigurations != null ? 
                (DbmClusterConfiguration[])ClustersConfigurations.Clone() : null;
            return configuration;
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            ClustersConfigurations = reader.ReadObject() as DbmClusterConfiguration[];
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(ClustersConfigurations);
        }
    }
}