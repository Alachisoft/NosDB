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
using Alachisoft.NosDB.Core.Configuration.Services;
using System;
using System.Linq;
using System.Text;


namespace Alachisoft.NosDB.Core.Configuration
{
    public class DbmClusterConfiguration : ICloneable, ICompactSerializable, IEquatable<DbmClusterConfiguration>,IObjectId
    {
        string _name = "";
        [ConfigurationAttribute("name")]
        public string Name { get { return _name.ToLower(); } set { _name = value; } }

        [ConfigurationAttribute("uid")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationSection("configuration-cluster")]
        public DbmConfigServers ConfigServers { set; get; }

        [ConfigurationSection("shards")]
        public DbmShards Shards { set; get; }

        public object Clone()
        {
            DbmClusterConfiguration dbmClusterConfig = new DbmClusterConfiguration();
            dbmClusterConfig.Name = this.Name;
            dbmClusterConfig.ConfigServers = (this.ConfigServers != null ? (DbmConfigServers)ConfigServers.Clone() : null);
            dbmClusterConfig.Shards = (this.Shards != null ? (DbmShards)Shards.Clone() : null);
            dbmClusterConfig.UID = this.UID;
            return dbmClusterConfig;
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            ConfigServers = reader.ReadObject() as DbmConfigServers;
            Shards = reader.ReadObject() as DbmShards;
            UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.WriteObject(ConfigServers);
            writer.WriteObject(Shards);
            writer.Write(UID);
        }

        public bool Equals(DbmClusterConfiguration cluster)
        {
            return Name.Equals(cluster.Name);
        }
    }
}
