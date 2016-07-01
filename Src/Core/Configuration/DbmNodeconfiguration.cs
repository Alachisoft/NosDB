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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Core.Configuration
{
    [ConfigurationRoot("configuration")]
    public class DbmNodeconfiguration : ICloneable, ICompactSerializable 
    {
        string _address = "";
        DbmClustersConfiguration _clusterConf = new DbmClustersConfiguration();

        [ConfigurationAttribute("ip")]
        public string IP { get { return _address.ToLower(); } set { _address = value; } }

        int _port;
        [ConfigurationAttribute("port")]
        public int Port { get { return _port; } set { _port = value; } }

        [ConfigurationSection("clusters")]
        public DbmClustersConfiguration DbmClusters
        {
            get { return _clusterConf; }
            set { _clusterConf = value; }
        }

        public object Clone()
        {
            DbmNodeconfiguration dbmNodeConfig = new DbmNodeconfiguration();
            dbmNodeConfig.IP = IP;
            dbmNodeConfig.Port = Port;
            dbmNodeConfig.DbmClusters = DbmClusters != null ? (DbmClustersConfiguration)DbmClusters.Clone() : null;
            return dbmNodeConfig;
        } 

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            IP = reader.ReadObject() as string;
            Port = reader.ReadInt32();
            DbmClusters = reader.ReadObject() as DbmClustersConfiguration;

        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(IP);
            writer.Write(Port);
            writer.WriteObject(DbmClusters);
        }
    }
}
