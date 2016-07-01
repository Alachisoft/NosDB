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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Alachisoft.NosDB.Common.Storage.Provider;
using Newtonsoft.Json;


namespace Alachisoft.NosDB.Common.Configuration.DOM
{

    public class StorageProviderConfiguration : ICloneable, ICompactSerializable
    {
        private string _databaseId;
        private string _databasePath;
        private ProviderType _providerType;
        private bool _isMultiFileStore;
        private string _providerString = "";
        private long _maxFileSize = 1073741824;

        /// <summary>
        /// Maximum size database may grow to, used to size the memory mapping.
        /// If databse grows larger than this size an exception will be raised and the user must close and reopn the Environment.
        /// On 64-bit systems there is no penalty for making this huge but it must be smaller than 2GB on 32-bit systems.
        /// </summary>
        [ConfigurationAttribute("max-file-size")]
        [JsonProperty(PropertyName = "MaxFileSize")]
        public long MaxFileSize
        {
            get { return _maxFileSize; }
            set
            {
                if (value < MiscUtil.MIN_FILE_SIZE)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                        LoggerManager.Instance.StorageLogger.Warn("StorageProviderConfiguration.MaxFileSize ", "Max File Size was less than Minimum File Size. Setting it to default " + MiscUtil.MIN_FILE_SIZE);
                    _maxFileSize = MiscUtil.MIN_FILE_SIZE;
                }
                else if (value > MiscUtil.MAX_FILE_SIZE)
                {
                    if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsWarnEnabled)
                        LoggerManager.Instance.StorageLogger.Warn("StorageProviderConfiguration.MaxFileSize ", "Max File Size was greater than Maximum File Size. Setting it to Max " + MiscUtil.MAX_FILE_SIZE);
                    _maxFileSize = MiscUtil.MAX_FILE_SIZE;
                }
                else
                {
                    _maxFileSize = value;
                }
            }
        }

        [JsonProperty(PropertyName = "DatabaseId")]
        public string DatabaseId
        {
            get { return _databaseId; }
            set
            {
                _databaseId = value;
            }
        }

        [JsonProperty(PropertyName = "DatabasePath")]
        public string DatabasePath
        {
            get { return _databasePath; }
            set
            {
                _databasePath = value;
            }
        }


        [ConfigurationAttribute("type")]
        [JsonProperty(PropertyName = "ProviderTypeString")]
        public string ProviderTypeString
        {
            get { return _providerString; }
            set { _providerString = value; StorageProviderType = ProviderType.LMDB; }
        }


        [JsonProperty(PropertyName = "StorageProviderType")]
        public ProviderType StorageProviderType
        {
            get { _providerType =  ProviderType.LMDB; return _providerType; }

            set { _providerType = value; _providerString = "LMDB"; }
        }

        [ConfigurationAttribute("is-multi-file-store")]
        [JsonProperty(PropertyName = "IsMultiFileStore")]
        public bool IsMultiFileStore
        {
            get { return _isMultiFileStore; }
            set { _isMultiFileStore = value; }
        }

        [ConfigurationSection("lmdb")]
        [JsonProperty(PropertyName = "LMDBProvider")]
        public LMDBConfiguration LMDBProvider { get; set; }
     

        #region ICloneable Member
        public object Clone()
        {
            StorageProviderConfiguration SPConfiguration = new StorageProviderConfiguration();
            SPConfiguration.DatabaseId = DatabaseId;
            SPConfiguration.DatabasePath = DatabasePath;
            SPConfiguration.LMDBProvider = LMDBProvider != null ? (LMDBConfiguration)LMDBProvider.Clone() : null;
            SPConfiguration.StorageProviderType = StorageProviderType;
            SPConfiguration.MaxFileSize = MaxFileSize;
            SPConfiguration.ProviderTypeString = ProviderTypeString;
            SPConfiguration.IsMultiFileStore = IsMultiFileStore;
            return SPConfiguration;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            DatabaseId = reader.ReadObject() as string;
            DatabasePath = reader.ReadObject() as string;
            MaxFileSize = reader.ReadInt64();
            LMDBProvider = reader.ReadObject() as LMDBConfiguration;
            StorageProviderType = (ProviderType)reader.ReadInt32();
            IsMultiFileStore = reader.ReadBoolean();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(DatabaseId);
            writer.WriteObject(DatabasePath);
            writer.Write(MaxFileSize);
            writer.WriteObject(LMDBProvider);
            writer.Write((int)StorageProviderType);
            writer.Write(IsMultiFileStore);
        }
        #endregion

        public static void ValidateConfiguration(StorageProviderConfiguration configuration)
        {
            if (configuration == null)
                throw new Exception("Storage Provider Configuration cannot be null.");
            //??
            //else if (configuration.DatabaseId == null)
            //    throw new Exception("Database Id cannot be null.");
            //else if (configuration.DatabaseId.Trim() == "")
            //    throw new Exception("Database Id cannot be empty string.");


            if (configuration.LMDBProvider != null)
                LMDBConfiguration.ValidateConfiguration(configuration.LMDBProvider);
            else
                throw new Exception("Storage Provider Configuration cannot be null.");
        }
    }
}
