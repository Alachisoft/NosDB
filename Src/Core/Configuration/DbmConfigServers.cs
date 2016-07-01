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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Configuration
{
    public class DbmConfigServers : ICloneable, ICompactSerializable,IObjectId
    {
        List<DbmConfigServer> csList = new List<DbmConfigServer>();

        [ConfigurationAttribute("uid")]
        public string UID
        {
            get;
            set;
        }

        [ConfigurationAttribute("name")]
        public string Name
        {
            get;
            set;
        }

        [ConfigurationSection("server")]
        public DbmConfigServer[] Nodes
        {
            get
            {
                if (csList != null)
                    return csList.ToArray();
                return null;

            }
            set
            {
                if (csList == null)
                    csList = new List<DbmConfigServer>();

                csList.Clear();

                if (value != null)
                {
                    csList.AddRange(value);

                }
            }
        }

        public object Clone()
        {
            DbmConfigServers dbmConfigServers = new DbmConfigServers();
            dbmConfigServers.Nodes = (Nodes != null ? (DbmConfigServer[])Nodes.Clone() : null);
            dbmConfigServers.Name = this.Name;
            dbmConfigServers.UID = this.UID;
            return dbmConfigServers;
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Nodes = reader.ReadObject() as DbmConfigServer[];
            this.Name = reader.ReadObject() as string;
            this.UID = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Nodes);
            writer.Write(Name);
            writer.Write(UID);
        }

       
    }
}
