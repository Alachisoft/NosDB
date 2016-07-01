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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;
using Alachisoft.NosDB.Common.Storage.Provider;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public class FileMetadata<TKey, TValue> :ICompactSerializable
    {
        private string _filePath;
        private string _databaseId;
        private ProviderType _providerType;
        private IPersistenceProvider _provider;

        public string FilePath
        {
            get { return _filePath; }
        }

        public string DatabaseId
        {
            get { return _databaseId; }
        }

        public ProviderType ProviderType
        {
            get { return _providerType; }
        }

        public IPersistenceProvider Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        public FileMetadata(IPersistenceProvider provider, StorageConfiguration configuration)
        {
            _provider = provider;
            _providerType = configuration.StorageProvider.StorageProviderType;
            _filePath = configuration.StorageProvider.DatabasePath;
            _databaseId = configuration.StorageProvider.DatabaseId;

            _provider.Initialize(configuration);
        }

        #region ICompactSerializable Members
        public void Deserialize(CompactReader reader)
        {
            _filePath = reader.ReadString();
            _databaseId = reader.ReadString();
            _providerType = (ProviderType)reader.ReadInt32();
            //_currentFileSize = reader.ReadInt64();
            //_provider = (IPersistenceProvider<TKey, TValue>)reader.ReadObject();
        }

        public void Serialize(CompactWriter writer)
        {
            writer.Write(_filePath);
            writer.Write(_databaseId);
            writer.Write((int)_providerType);
            //writer.Write(_currentFileSize);
            //writer.WriteObject(_provider);
        }
        #endregion
    }
}
