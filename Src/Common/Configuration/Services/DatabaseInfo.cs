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
﻿using System.Linq;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Serialization;
﻿using Alachisoft.NosDB.Common.Util;
﻿using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration.Services
{

    public class DatabaseInfo : ICloneable, ICompactSerializable,IObjectId
    {
        Dictionary<string, CollectionInfo> _collections = new Dictionary<string, CollectionInfo>(StringComparer.InvariantCultureIgnoreCase);
        /// <summary>
        /// Gets/Sets the name of the datbase
        /// </summary>
        /// 

        string _name = "";
        [JsonProperty(PropertyName = "Name")]
        public string Name { get { return _name.ToLower(); } set { _name = value; } }

        [JsonProperty(PropertyName = "UID")]
        public string UID
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "Mode")]
        public DatabaseMode Mode
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "Type")]
        public DatabaseType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Use in case of data migration. Primary Shard is the shard which contains the data. 
        /// </summary>
        [JsonProperty(PropertyName = "PrimaryShard")]
        public string PrimaryShard
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the information about crated collections 
        /// </summary>
        [JsonProperty(PropertyName = "Collections")]
        public Dictionary<string, CollectionInfo> Collections
        {
            get { return _collections; }
            set { _collections = value; }
        }

        public PartitionKey GetPartitionKey(string collectionName)
        {
            if (Collections.Count == 0)
            {
                throw new Exception("No Collections are available");
            }
            else if (Collections.ContainsKey(collectionName))
            {
                return Collections[collectionName].ParitionKey;
            }
            return null;    // To be decided later if to throw exception here or return null
        }

        public void AddCollection(CollectionInfo collection)
        {
            lock (Collections)
            {
                Collections.Add(collection.Name, collection);
            }
        }

        public void AddCollection(string name, CollectionInfo collection)
        {
            lock (Collections)
            {
                Collections.Add(name, collection);
            }
        }

        public void RemoveCollection(string name)
        {
            lock (Collections)
            {
                Collections.Remove(name);
            }
        }

        public bool ContainsCollection(string name)
        {
            return Collections.ContainsKey(name);
        }

        public CollectionInfo GetCollection(string name)
        {
            lock (Collections)
            {
                if (Collections.ContainsKey(name))
                    return Collections[name];
                return null;
            }
        }

        #region ICloneable Member
        public object Clone()
        {
            DatabaseInfo databaseInfo = new DatabaseInfo();
            databaseInfo.Name = Name;
            databaseInfo._collections = Collections != null ? Collections.Clone<string, CollectionInfo>() : null;
            databaseInfo.UID = UID;
            databaseInfo.Mode = Mode;
            databaseInfo.PrimaryShard = PrimaryShard;
            databaseInfo.Type = Type;
            return databaseInfo;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            _collections = SerializationUtility.DeserializeDictionary<string, CollectionInfo>(reader);
            UID = reader.ReadObject() as string;
            Mode = (DatabaseMode)reader.ReadInt32();
            PrimaryShard = reader.ReadObject() as string;
            Type = (DatabaseType)reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            SerializationUtility.SerializeDictionary<string, CollectionInfo>(Collections, writer);
            writer.WriteObject(UID);
            writer.Write((int)Mode);
            writer.WriteObject(PrimaryShard);
            writer.Write((int)Type);
        }
        #endregion
    }
}
