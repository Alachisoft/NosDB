using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Protobuf;

using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Logger;


namespace Alachisoft.NosDB.Common.Configuration.DOM
{

    public class LMDBConfiguration : ICloneable, ICompactSerializable
    {
        private LMDBEnvOpenFlags _environmentOpenFlags = LMDBEnvOpenFlags.NoSubDir | LMDBEnvOpenFlags.NoThreadLocalStorage;
        private int _maxCollections;                //can't be changed if environment is opened.
        private int _maxReaders = 126;              //can't be changed if environment is opened.

        public const string EXTENSION = ".nsdb";
        public const double MAX_DATA_THREASHOLD = 0.85;

        /// <summary>
        /// Flags used to open the envoronment.
        /// </summary>
        [ConfigurationAttribute("environment-open-flags")]
        [JsonProperty(PropertyName = "EnvironmentOpenFlags")]
        public LMDBEnvOpenFlags EnvironmentOpenFlags
        {
            get { return _environmentOpenFlags; }
            set { _environmentOpenFlags = (_environmentOpenFlags | value); }
        }

        /// <summary>
        /// Set/Get Maximum numbers of readers that can concurrently read from the database.
        /// Option can not be modified once the environment is opened.
        /// </summary>
        [ConfigurationAttribute("max-readers")]
        [JsonProperty(PropertyName = "MaxReaders")]
        public int MaxReaders
        {
            get { return _maxReaders; }
            set { _maxReaders = value; }
        }

        /// <summary>
        /// Set Maximum numbers of Tables that the database can have.
        /// Option can not be modified once the environment is opened.
        /// </summary>
        [ConfigurationAttribute("max-collections")]
        [JsonProperty(PropertyName = "MaxCollections")]
        public int MaxCollections
        {
            get { return _maxCollections; }
            set { _maxCollections = value; }
        }

        #region ICloneable Member
        public object Clone()
        {
            LMDBConfiguration configuration = new LMDBConfiguration();
            configuration.EnvironmentOpenFlags = EnvironmentOpenFlags;
            configuration.MaxCollections = MaxCollections;
            configuration.MaxReaders = MaxReaders;
            return configuration;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            this.EnvironmentOpenFlags = (LMDBEnvOpenFlags)reader.ReadInt32();
            MaxCollections = reader.ReadInt32();
            MaxReaders = reader.ReadInt32();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write((int)this.EnvironmentOpenFlags);
            writer.Write(MaxCollections);
            writer.Write(MaxReaders);
        }
        #endregion

        public static void ValidateConfiguration(LMDBConfiguration configuraiton)
        {
            if (configuraiton == null)
                throw new Exception("LMDB Configuration cannot be null");

        }
    }
}
