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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Configuration.DOM
{
    public class RecoveryPersistenceConfiguration : ICloneable, ICompactSerializable
    {
        string _filePath;
        List<string> _fileName;
        int chunkSize;
        string _database;
        string _cluster;
        private string _username;
        private string _password;

        private Dictionary<string, Dictionary<string, string[]>> dbCollectionMap;

        public RecoveryPersistenceConfiguration()
        {
            dbCollectionMap = new Dictionary<string, Dictionary<string, string[]>>();
        }

        #region Properties
        /// <summary>
        /// contains database name in file, on system and collections
        /// </summary>
        public Dictionary<string, Dictionary<string, string[]>> DbCollectionMap
        {
            get { return dbCollectionMap; }
            set { dbCollectionMap = value; }
        }

        public int ChunkSize
        {
            get { return chunkSize; }
            set { chunkSize = value; }
        }
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        public List<string> FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string DatabaseName
        {
            get
            {
                return _database;
            }
            set
            {
                _database = value;
            }
        }

        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string Cluster
        {
            get { return _cluster; }
            set { _cluster = value; }
        }
        #endregion

        #region IClonable
        public object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _filePath = reader.ReadString();
            FileName = Common.Util.SerializationUtility.DeserializeList<string>(reader);
            dbCollectionMap = Util.SerializationUtility.DeserializeDD<string, string, string[]>(reader);
            _database = reader.ReadString();
            _cluster = reader.ReadString();
            _username = reader.ReadString();
            _password = reader.ReadString();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_filePath);
            Common.Util.SerializationUtility.SerializeList<string>(FileName, writer);
            Util.SerializationUtility.SerializeDD<string, string, string[]>(dbCollectionMap, writer);
            writer.Write(_database);
            writer.Write(_cluster);
            writer.Write(_username);
            writer.Write(_password);
        }
    }
}
