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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    /// <summary>
    /// Wrapper of all the entities in config server that are to backedup
    /// </summary>
    public class CsBackupableEntities : ICompactSerializable
    {
       
        private Dictionary<string, Dictionary<string, IDistributionStrategy>> distributionStrategyMap;
        private Dictionary<string, DatabaseConfiguration> database;
        private ResourceItem securityResourcre;
        private List<string> shardList;

        
        
        
        internal CsBackupableEntities()
        {
            distributionStrategyMap = new Dictionary<string, Dictionary<string, IDistributionStrategy>>();
            database = new Dictionary<string, DatabaseConfiguration>();
            securityResourcre = null;
            shardList = new List<string>();
        }

        public List<string> ShardList
        {
            get { return shardList; }
            set { shardList = value; }
        }

        public ResourceItem SecurityResource
        {
            get { return securityResourcre; }
            set { securityResourcre = value; }
        }
        public Dictionary<string, Dictionary<string, IDistributionStrategy>> DistributionStrategyMap
        {
            get { return distributionStrategyMap; }
            set { distributionStrategyMap = value; }
        }

        public Dictionary<string, DatabaseConfiguration> Database
        {
            get { return database; }
            set { database = value; }
        }

        #region ICompact Serialization
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            distributionStrategyMap = Common.Util.SerializationUtility.DeserializeDD<string,string,IDistributionStrategy>(reader);
            Database = Common.Util.SerializationUtility.DeserializeDictionary<string, DatabaseConfiguration>(reader);
            securityResourcre = reader.ReadObject() as ResourceItem;
            shardList = Common.Util.SerializationUtility.DeserializeList<string>(reader);
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {         
            Common.Util.SerializationUtility.SerializeDD<string, string, IDistributionStrategy>(distributionStrategyMap, writer);
            Common.Util.SerializationUtility.SerializeDictionary<string, DatabaseConfiguration>(database, writer);
            writer.WriteObject(securityResourcre);
            Common.Util.SerializationUtility.SerializeList<string>(shardList, writer);
        }
#endregion
    }
}
