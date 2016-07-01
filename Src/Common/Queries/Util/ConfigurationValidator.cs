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
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Security.Impl;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Provider;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public class ConfigurationValidator
    {
        private static readonly Dictionary<ExpectedType, object> ArgumentValues
            = new Dictionary<ExpectedType, object>();

        private readonly List<IValidator> _validators = new List<IValidator>();

        private Dictionary<string, object> _optionals = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private readonly DbObjectType _confobjectType;
        private readonly DataDefinitionType _objType;
        private IConfigurationSession _session;
        private string _cluster;
        private const long MB = 1024 * 1024;

        static ConfigurationValidator()
        {
            ArgumentValues.Add(ExpectedType.Null, ExtendedJSONDataTypes.Null);
            ArgumentValues.Add(ExpectedType.Bool, ExtendedJSONDataTypes.Boolean);
            ArgumentValues.Add(ExpectedType.Number, ExtendedJSONDataTypes.Number);
            ArgumentValues.Add(ExpectedType.String, ExtendedJSONDataTypes.String);
            ArgumentValues.Add(ExpectedType.NumberArray, new ArrayValidator(ExtendedJSONDataTypes.Number));
            ArgumentValues.Add(ExpectedType.StringArray, new ArrayValidator(ExtendedJSONDataTypes.String));
        }

        public ConfigurationValidator(IConfigurationSession session, string cluster, DataDefinitionType defType, DbObjectType objType)
        {
            DocumentValidator validator = new DocumentValidator();
            _confobjectType = objType;
            _objType = defType;
            _session = session;
            _cluster = cluster;

            bool isNameRequired = true;
            switch (objType)
            {
                case DbObjectType.Database:
                    switch (defType)
                    {
                        #region database configuration validator section.
                        case DataDefinitionType.Alter:
                        case DataDefinitionType.Create:
                            #region Create or Alter Database Validation
                            {

                                _optionals.Add(ConfigType.MultiFile.ToString(), ArgumentValues[ExpectedType.Bool]);
                                _optionals.Add(ConfigType.CacheSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.InitialSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.MaxFileSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.MaxCollections.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.Provider.ToString(), ArgumentValues[ExpectedType.String]);



                                _validators.Add(validator);
                            }
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region DROP
                            _validators.Add(validator);
                            break;
                            #endregion

                        #region Alter Database
                        /*
                        case DataDefinitionType.Alter:
                            #region Alter Database
                            {
                              
                                _optionals.Add(ConfigType.SingleFiled.ToString(), ArgumentValues[ExpectedType.Bool]);
                                _optionals.Add(ConfigType.CacheSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.InitialSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.FinalSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.MaxCollections.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.Provider.ToString(), ArgumentValues[ExpectedType.String]);

                                _optionals.Add(ConfigType.AttachmentsEnabled.ToString(), ArgumentValues[ExpectedType.Bool]);
                                _optionals.Add(ConfigType.AttachmentsPath.ToString(), ArgumentValues[ExpectedType.String]);
                            }
                            break;
                            #endregion
                            */
                        #endregion

                        case DataDefinitionType.Backup:
                            #region Backup configuration validator section.
                            //we can use value validator to validate backuptype at once but need to change recovery configuration
                            //prefere way at this point is to manually validate 
                            ValueValidator backupTypeValueValidator = new ValueValidator();
                            backupTypeValueValidator.AddValidation("Full", new TrueValidator());


                            validator.AddValidation(ConfigType.BackupType.ToString(), backupTypeValueValidator);
                            validator.AddValidation(ConfigType.Path.ToString(), ArgumentValues[ExpectedType.String]);
                            _optionals.Add(ConfigType.UserName.ToString(), ArgumentValues[ExpectedType.String]);
                            _optionals.Add(ConfigType.Password.ToString(), ArgumentValues[ExpectedType.String]);


                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Restore:
                            #region Restore configuration validator section.
                            ValueValidator restoreTypeValueValidator = new ValueValidator();
                            restoreTypeValueValidator.AddValidation("Full", new TrueValidator());
                            validator.AddValidation(ConfigType.RestoreType.ToString(), restoreTypeValueValidator);
                            validator.AddValidation(ConfigType.Path.ToString(), ArgumentValues[ExpectedType.String]);
                            _optionals.Add(ConfigType.SourceDatabase.ToString(), ArgumentValues[ExpectedType.String]);
                            _optionals.Add(ConfigType.UserName.ToString(), ArgumentValues[ExpectedType.String]);
                            _optionals.Add(ConfigType.Password.ToString(), ArgumentValues[ExpectedType.String]);


                            //TODO Temporarilly added

                            _validators.Add(validator);
                            break;
                            #endregion

                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        #endregion
                    }
                    break;

                case DbObjectType.Collection:
                    switch (defType)
                    {
                        #region collection configuration validator section.
                        case DataDefinitionType.Alter:
                        case DataDefinitionType.Create:
                            #region Create OR Alter collection
                            {
                                _optionals.Add(ConfigType.Database.ToString(), ArgumentValues[ExpectedType.String]);
                                //start: Distribution validator.
                                DocumentValidator strategyDocument = new DocumentValidator();

                                DocumentValidator arrayRangeValues = new DocumentValidator();
                                //DocumentValidator nonShardedDocument = new DocumentValidator();

                                //start: Min and Max validator for range
                                arrayRangeValues.AddValidation(ConfigType.MinRange.ToString(), ArgumentValues[ExpectedType.String]);
                                arrayRangeValues.AddValidation(ConfigType.MaxRange.ToString(), ArgumentValues[ExpectedType.String]);
                                arrayRangeValues.AddValidation(ConfigType.Shard.ToString(), ArgumentValues[ExpectedType.String]);

                                ValueValidator strategyValValidator = new ValueValidator();

                                strategyValValidator.AddValidation(DistributionType.NonSharded.ToString(), new TrueValidator());

                                strategyDocument.AddValidation(ConfigType.Strategy.ToString(), strategyValValidator);

                                _optionals.Add(ConfigType.Distribution.ToString(), strategyDocument);

                                //end: Distribution validator.
                                ValueValidator collectionTypeValValidator = new ValueValidator();

                                // temporary validations > Need to find another way to conditional validate capped size 
                                _optionals.Add(ConfigType.CappedSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                _optionals.Add(ConfigType.MaxDocuments.ToString(), ArgumentValues[ExpectedType.Number]);



                                //start capcollection
                                DocumentValidator capCollectionDocValidator = new DocumentValidator();
                                capCollectionDocValidator.AddOptionalValidation(ConfigType.CappedSize.ToString(), ArgumentValues[ExpectedType.Number]);
                                capCollectionDocValidator.AddOptionalValidation(ConfigType.MaxDocuments.ToString(), ArgumentValues[ExpectedType.Number]);
                                capCollectionDocValidator.AddOptionalValidation(ConfigType.Shard.ToString(), ArgumentValues[ExpectedType.String]);
                                //end capcollection

                                //start SingleShardCollection
                                DocumentValidator singleShardDocValidator = new DocumentValidator();
                                singleShardDocValidator.AddOptionalValidation(ConfigType.Shard.ToString(), ArgumentValues[ExpectedType.String]);

                                _optionals.Add(ConfigType.CollectionType.ToString(), collectionTypeValValidator);
                                _optionals.Add(ConfigType.Shard.ToString(), ArgumentValues[ExpectedType.String]);

                                _optionals.Add(ConfigType.EvictionEnabled.ToString(), ArgumentValues[ExpectedType.Bool]);

                                _validators.Add(validator);
                            }
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region DROP
                            _optionals.Add(ConfigType.Database.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);

                        #endregion
                    }
                    break;

                case DbObjectType.Index:
                    switch (defType)
                    {
                        #region index configuration validator section.
                        case DataDefinitionType.Create:
                            #region Create
                            validator.AddValidation(ConfigType.Database.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.Collection.ToString(), ArgumentValues[ExpectedType.String]);

                            DocumentValidator dv1 = new DocumentValidator();
                            dv1.AddValidation(ConfigType.Attribute.ToString(), ArgumentValues[ExpectedType.String]);
                            dv1.AddValidation(ConfigType.SortOrder.ToString(), ArgumentValues[ExpectedType.String]);

                            validator.AddValidation(ConfigType.Attributes.ToString(), (dv1));
                            _optionals.Add(ConfigType.JournalEnabled.ToString(), ArgumentValues[ExpectedType.Bool]);
                            _optionals.Add(ConfigType.CachePolicy.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region DROP
                            validator.AddValidation(ConfigType.Database.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.Collection.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        #endregion
                    }
                    break;

                case DbObjectType.Function:
                    switch (defType)
                    {
                        #region function configuration validator section.
                        case DataDefinitionType.Create:
                            #region Create
                            validator.AddValidation(ConfigType.Database.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.AssemblyFile.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.ClassName.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.DeploymentId.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region DROP
                            validator.AddValidation(ConfigType.Database.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        #endregion

                    }
                    break;


                case DbObjectType.Role:
                    switch (defType)
                    {
                        #region role configuration validator section.
                        case DataDefinitionType.Create:
                            #region Create
                            validator.AddValidation(ConfigType.PermissionSet.ToString(), ArgumentValues[ExpectedType.StringArray]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Alter:
                            #region Alter
                            validator.AddValidation(ConfigType.AddPermissions.ToString(), ArgumentValues[ExpectedType.StringArray]);
                            _validators.Add(validator);

                            DocumentValidator validator2 = new DocumentValidator();
                            validator2.AddValidation(ConfigType.RemovePermissions.ToString(), ArgumentValues[ExpectedType.StringArray]);
                            _validators.Add(validator2);
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region DROP
                            _validators.Add(validator);
                            break;
                            #endregion
                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        #endregion

                    }
                    break;

                case DbObjectType.User:
                    switch (defType)
                    {
                        #region user configuration validator section.
                        case DataDefinitionType.Create:
                            #region Create
                            ValueValidator valueValidator = new ValueValidator();
                            valueValidator.AddValidation("Windows", new TrueValidator());
                            valueValidator.AddValidation("Custom", new TrueValidator());
                            validator.AddValidation(ConfigType.UserType.ToString(), valueValidator);
                            _optionals.Add(ConfigType.Password.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Alter:
                            #region Alter
                            validator.AddValidation(ConfigType.OldPassword.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.NewPassword.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region Drop
                            validator.AddValidation(ConfigType.Password.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion
                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        #endregion

                    }
                    break;
                case DbObjectType.Login:
                    switch (defType)
                    {
                        #region Login configuration validator section.
                        case DataDefinitionType.Create:
                            #region Create
                            DocumentValidator customPasswordValidator = new DocumentValidator();
                            customPasswordValidator.AddValidation(ConfigType.Password.ToString(), ArgumentValues[ExpectedType.String]);
                            ValueValidator valueValidator = new ValueValidator();
                            valueValidator.AddValidation("Windows", new TrueValidator());
                            valueValidator.AddValidation("Custom", customPasswordValidator);
                            validator.AddValidation(ConfigType.UserType.ToString(), valueValidator);
                            //TODO Temporarilly added
                            _optionals.Add(ConfigType.Password.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Alter:
                            #region Alter
                            validator.AddValidation(ConfigType.OldPassword.ToString(), ArgumentValues[ExpectedType.String]);
                            validator.AddValidation(ConfigType.NewPassword.ToString(), ArgumentValues[ExpectedType.String]);
                            _validators.Add(validator);
                            break;
                            #endregion

                        case DataDefinitionType.Drop:
                            #region
                            _validators.Add(validator);
                            break;
                            #endregion
                        default:
                            throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
                        #endregion

                    }
                    break;

                default:
                    throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
            }

            if (isNameRequired)
            {
                validator.AddValidation(ConfigType.Name.ToString(),
               ExtendedJSONDataTypes.String);
            }
        }

        public bool Validate(object value, out ICloneable configuration,
            out Dictionary<string, object> configValues)
        {
            configuration = null;
            configValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            bool isValidated = true;

            foreach (var validator in _validators)
            {
                if (!validator.Validate(null, value, _optionals, ref configValues, false))
                {
                    isValidated = false;
                    break;
                }
            }
            if (!DocumentValidator.ValidateOptionals(value, _optionals, ref configValues) && isValidated)
            {
                isValidated = false;
            }

            if (isValidated)
            {
                PopulateConfigurationObject(configValues, out configuration);
            }

            return isValidated;
        }

        private void PopulateConfigurationObject(Dictionary<string, object> configValues,
            out ICloneable configuration)
        {
            configuration = null;
            //TODO change validation to DataDefinitionTYpe
            DbObjectType configurationObjectType = _confobjectType;
            if (configurationObjectType == DbObjectType.Database)
            {
                if (_objType == DataDefinitionType.Backup)
                    configurationObjectType = DbObjectType.Backup;
                else if (_objType == DataDefinitionType.Restore)
                    configurationObjectType = DbObjectType.Restore;
            }

            switch (configurationObjectType)
            {
                case DbObjectType.Database:
                    PopulateDatabaseConfiguration(configValues, out configuration);
                    break;

                case DbObjectType.Collection:
                    PopulateCollectionConfiguration(configValues, out configuration);
                    break;

                case DbObjectType.Index:
                    PopulateIndexConfiguration(configValues, out configuration);
                    break;

                case DbObjectType.Login:
                    PopulateLoginConfiguration(configValues, out configuration);
                    break;

                case DbObjectType.Backup:
                    PopulateBackupConfiguration(configValues, out configuration);
                    break;

                case DbObjectType.Restore:
                    PopulateRestoreConfiguration(configValues, out configuration);
                    break;

                case DbObjectType.Role:
                case DbObjectType.User:
                case DbObjectType.MasterKey:
                default:
                    throw new DatabaseException(ErrorCodes.Query.NOT_SUPPORTED);
            }
        }


        #region Database configuration population.
        public void PopulateDatabaseConfiguration(Dictionary<string, object> configValues,
           out ICloneable configuration)
        {
            DatabaseConfiguration dbConf = new DatabaseConfiguration();
            string databaseName = "";
            DatabaseConfiguration remoteDatabase = null;
            string errorMessage = null;

            if (_objType == DataDefinitionType.Alter)
            {
                if (configValues.ContainsKey(ConfigType.Name.ToString()))
                {
                    databaseName = configValues[ConfigType.Name.ToString()] as string;
                }

                #region Get Database Configuration from Configuraiton Server

                ClusterConfiguration clusterConfig = _session.GetDatabaseClusterConfiguration(_cluster);
                if (clusterConfig != null)
                {
                    if (clusterConfig.Databases != null)
                    {
                        if (clusterConfig.Databases.ContainsDatabase(databaseName))
                        {
                            remoteDatabase = clusterConfig.Databases.GetDatabase(databaseName);
                        }
                        else
                            errorMessage = string.Format("Specified database {0} does not exist.", databaseName);
                    }
                    else
                        errorMessage = string.Format("Specified database {0} does not exist.", databaseName);
                }
                else
                    errorMessage = string.Format("No configuraiton exist for cluster {0}", _cluster);

                if (remoteDatabase == null)
                    throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { errorMessage });

                #endregion

                errorMessage = null;


                if (configValues.ContainsKey(ConfigType.MultiFile.ToString()) && errorMessage == null)
                {
                    long maxFileSize = GetInt64(ConfigType.MaxFileSize.ToString(), configValues[ConfigType.MaxFileSize.ToString()]);
                    if (maxFileSize * MB > long.MaxValue || maxFileSize * MB < long.MinValue)
                        throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Value must be within the range of Int64 for attribute MaxFileSize." });

                    if (remoteDatabase.Storage.StorageProvider.MaxFileSize != MB * maxFileSize) ;

                    errorMessage = "MultiFile";
                }

                if (configValues.ContainsKey(ConfigType.MaxFileSize.ToString()) && errorMessage == null)
                {
                    if (remoteDatabase.Storage.StorageProvider.MaxFileSize != MB *
                        GetInt64(ConfigType.MaxFileSize.ToString(), configValues[ConfigType.MaxFileSize.ToString()]))
                        errorMessage = "MaxFileSize";
                }

                if (configValues.ContainsKey(ConfigType.MaxCollections.ToString()) && errorMessage == null && remoteDatabase.Storage.StorageProvider.LMDBProvider != null)
                {
                    if (remoteDatabase.Storage.StorageProvider.LMDBProvider.MaxCollections !=
                        GetInt32(ConfigType.MaxCollections.ToString(), configValues[ConfigType.MaxCollections.ToString()]))
                        errorMessage = "MaxCollections";
                }

                if (configValues.ContainsKey(ConfigType.InitialSize.ToString()) && errorMessage == null && remoteDatabase.Storage.StorageProvider.LMDBProvider != null)
                {
                    long initialSize = GetInt64(ConfigType.InitialSize.ToString(), configValues[ConfigType.InitialSize.ToString()]);
                    if (initialSize * MB > long.MaxValue || initialSize * MB < long.MinValue)
                        throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Value must be within the range of Int64 for attribute InitialSize." });

                }

                if (configValues.ContainsKey(ConfigType.CacheSize.ToString()) && errorMessage == null)
                {
                    long cacheSize = GetInt64(ConfigType.CacheSize.ToString(), configValues[ConfigType.CacheSize.ToString()]);
                    if (cacheSize * MB > long.MaxValue || cacheSize * MB < long.MinValue)
                        throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Value must be within the range of Int64 for attribute CacheSize." });

                    if (remoteDatabase.Storage.CacheConfiguration.CacheSpace != MB * cacheSize)

                        errorMessage = "CacheSize";
                }

                if (errorMessage != null)
                    throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Configuration attribute " + errorMessage + " is not Modifiable." });

                dbConf = remoteDatabase;
            }
            else
            {
                if (configValues.ContainsKey(ConfigType.Name.ToString()))
                {
                    dbConf.Name = configValues[ConfigType.Name.ToString()] as string;
                }

                dbConf.Storage = new StorageConfiguration
                {
                    Collections = new CollectionConfigurations(),
                    StorageProvider = new StorageProviderConfiguration()
                };


                dbConf.Storage.StorageProvider.LMDBProvider = new LMDBConfiguration();
                dbConf.Storage.StorageProvider.StorageProviderType = ProviderType.LMDB;


                if (configValues.ContainsKey(ConfigType.MultiFile.ToString()) && (!(bool)configValues[ConfigType.MultiFile.ToString()]))
                {
                    dbConf.Storage.StorageProvider.IsMultiFileStore = false;
                }
                else
                {
                    dbConf.Storage.StorageProvider.IsMultiFileStore = true;
                }

                if (configValues.ContainsKey(ConfigType.MaxFileSize.ToString()))
                {
                    long maxFileSize = GetInt64(ConfigType.MaxFileSize.ToString(), configValues[ConfigType.MaxFileSize.ToString()]);
                    if (maxFileSize * MB > long.MaxValue || maxFileSize * MB < long.MinValue)
                        throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Value must be within the range of Int64 for attribute MaxFileSize." });

                    dbConf.Storage.StorageProvider.MaxFileSize = MB * maxFileSize;
                }
                else
                {
                    //50 Gb...
                    dbConf.Storage.StorageProvider.MaxFileSize = MB * 1024 * 50;
                }


                if (configValues.ContainsKey(ConfigType.MaxCollections.ToString()) && dbConf.Storage.StorageProvider.LMDBProvider != null)
                {
                    dbConf.Storage.StorageProvider.LMDBProvider.MaxCollections =
                        GetInt32(ConfigType.MaxCollections.ToString(), configValues[ConfigType.MaxCollections.ToString()]);
                }
                else if (dbConf.Storage.StorageProvider.LMDBProvider != null)
                {
                    dbConf.Storage.StorageProvider.LMDBProvider.MaxCollections = 1000;
                }


                dbConf.Storage.CacheConfiguration = new CachingConfiguration();
                dbConf.Storage.CacheConfiguration.CachePolicy = "FCFS";

                if (configValues.ContainsKey(ConfigType.CacheSize.ToString()))
                {
                    long cacheSize = GetInt64(ConfigType.CacheSize.ToString(), configValues[ConfigType.CacheSize.ToString()]);
                    if (cacheSize * MB > long.MaxValue || cacheSize * MB < long.MinValue)
                        throw new DatabaseException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Value must be within the range of Int64 for attribute CacheSize." });

                    dbConf.Storage.CacheConfiguration.CacheSpace = MB * cacheSize;

                }
            }
            configuration = dbConf;
        }
        #endregion

        #region Collection configuration population.
        public void PopulateCollectionConfiguration(Dictionary<string, object> configValues,
           out ICloneable configuration)
        {
            CollectionConfiguration collConf = new CollectionConfiguration();

            if (_objType == DataDefinitionType.Alter)
            {
                //TODO currently we allow collection DDL without DatabaseName from PowerShell 
                // Can't validate collection configuraion on client side.

                string errorAttribute = "";
                if (configValues.ContainsKey(ConfigType.CollectionType.ToString()))
                    errorAttribute = "CollectionType";
                else if (configValues.ContainsKey(ConfigType.Strategy.ToString()))
                    errorAttribute = "Distribution Strategy";
                else if (configValues.ContainsKey(ConfigType.Shard.ToString()))
                    errorAttribute = "Shard";

                if (!string.IsNullOrEmpty(errorAttribute))
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, new string[] { "Attribute " + errorAttribute + " is not modifiable" });

                if (configValues.ContainsKey(ConfigType.Name.ToString()))
                    collConf.CollectionName = (string)configValues[ConfigType.Name.ToString()];

                if (configValues.ContainsKey(ConfigType.CappedSize.ToString()))
                    collConf.CollectionSize = 1024 * MB * GetInt64(ConfigType.CappedSize.ToString(), configValues[ConfigType.CappedSize.ToString()]);
                else
                    collConf.CollectionSize = -1;

                if (configValues.ContainsKey(ConfigType.MaxDocuments.ToString()))
                    collConf.MaxDocuments = GetInt64(ConfigType.MaxDocuments.ToString(), configValues[ConfigType.MaxDocuments.ToString()]);
                else
                    collConf.MaxDocuments = -1;
            }
            else
            {
                collConf = new CollectionConfiguration
                {
                    Indices = new Indices(),
                    EvictionConfiguration = new EvictionConfiguration { EnabledEviction = true, Policy = "lru" }
                };
                collConf.Indices.IndexConfigurations = new Dictionary<string, IndexConfiguration>();

                if (configValues.ContainsKey(ConfigType.Name.ToString()))
                    collConf.CollectionName = (string)configValues[ConfigType.Name.ToString()];

                if (configValues.ContainsKey(ConfigType.CappedSize.ToString()))
                    collConf.CollectionSize = GetInt64(ConfigType.CappedSize.ToString(), configValues[ConfigType.CappedSize.ToString()]);
                else
                    collConf.CollectionSize = 2;

                collConf.CollectionSize *= 1024 * MB;

                if (configValues.ContainsKey(ConfigType.MaxDocuments.ToString()))
                    collConf.MaxDocuments = GetInt64(ConfigType.MaxDocuments.ToString(), configValues[ConfigType.MaxDocuments.ToString()]);
                else
                    collConf.MaxDocuments = 8796093022; //??

                collConf.DistributionStrategy = new DistributionStrategyConfiguration();

                if (configValues.ContainsKey(ConfigType.Strategy.ToString()))
                {
                    collConf.DistributionStrategy.Name = (string)configValues[ConfigType.Strategy.ToString()];
                }
                else
                {
                    collConf.DistributionStrategy.Name = "NonSharded";
                }

                //if (configValues.ContainsKey(ConfigType.KeyType.ToString()))
                //    collConf.DistributionStrategy.ShardKeyType = (string)configValues[ConfigType.KeyType.ToString()];
                collConf.DistributionStrategy.ShardKeyType = "string";

                if (configValues.ContainsKey(ConfigType.Shard.ToString()))
                    collConf.Shard = (string)configValues[ConfigType.Shard.ToString()];
                else
                    collConf.Shard = "ALL";

                collConf.PartitionKey = new PartitionKeyConfiguration();

                //if (configValues.ContainsKey(ConfigType.PartitionKey.ToString()))
                //{
                //    IList partitionKeys = configValues[ConfigType.PartitionKey.ToString()] as IList;
                //    if (partitionKeys != null)
                //    {
                //        //if (!collConf.DistributionStrategy.Name.Equals("HashBased", StringComparison.OrdinalIgnoreCase) &&
                //        //    !collConf.DistributionStrategy.Name.Equals("RangeBasedDistributionStrategy", StringComparison.OrdinalIgnoreCase))
                //        //    throw new QuerySystemException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION_JSON);

                //        Dictionary<string, PartitionKeyConfigurationAttribute> keyList =
                //            new Dictionary<string, PartitionKeyConfigurationAttribute>();
                //        foreach (Dictionary<string, object> partitionKeyConf in partitionKeys)
                //        {
                //            PartitionKeyConfigurationAttribute partitionKey = new PartitionKeyConfigurationAttribute();
                //            partitionKey.Name = (string)partitionKeyConf[ConfigType.KeyName.ToString()];
                //            partitionKey.Type = (string)partitionKeyConf[ConfigType.KeyType.ToString()];
                //            keyList.Add(partitionKey.Name, partitionKey);
                //        }
                //        collConf.PartitionKey.PartitionKeyAttributes = keyList;
                //    }
                //}
            }
            configuration = collConf;
        }
        #endregion

        #region Index configuration population.
        public void PopulateIndexConfiguration(Dictionary<string, object> configValues,
           out ICloneable configuration)
        {
            IndexConfiguration indexConf = new IndexConfiguration();
            if (configValues.ContainsKey(ConfigType.Name.ToString()))
                indexConf.Name = configValues[ConfigType.Name.ToString()].ToString();

            IndexAttribute attribute = new IndexAttribute();
            if (configValues.ContainsKey(ConfigType.Attribute.ToString()))
            {
                attribute.Name = (string)configValues[ConfigType.Attribute.ToString()];
            }

            if (configValues.ContainsKey(ConfigType.SortOrder.ToString()))
            {
                attribute.Order = (string)configValues[ConfigType.SortOrder.ToString()];
            }
            indexConf.Attributes = attribute;

            if (configValues.ContainsKey(ConfigType.JournalEnabled.ToString()))
            {
                indexConf.JournalEnabled = (bool)configValues[ConfigType.JournalEnabled.ToString()];
            }

            if (configValues.ContainsKey(ConfigType.CachePolicy.ToString()))
            {
                indexConf.CachePolicy = (string)configValues[ConfigType.CachePolicy.ToString()];
            }

            configuration = indexConf;

        }
        #endregion

        #region Login configuration population.
        public void PopulateLoginConfiguration(Dictionary<string, object> configValues,
            out ICloneable configuration)
        {
            IUser user;
            string username = "";
            string userType = null;
            if (configValues.ContainsKey(ConfigType.UserType.ToString()))
            {
                userType = configValues[ConfigType.UserType.ToString()] as string;
            }

            if (configValues.ContainsKey(ConfigType.Name.ToString()))
            {
                username = (string)
                    configValues[ConfigType.Name.ToString()];
            }
            user = new User(username);
            configuration = user;
        }
        #endregion
        #region Backup configuration population.
        public void PopulateBackupConfiguration(Dictionary<string, object> configValues,
        out ICloneable configuration)
        {
            RecoveryConfiguration recoveryConfig = new RecoveryConfiguration();
            if (configValues.ContainsKey(ConfigType.Name.ToString()))
            {
                recoveryConfig.DatabaseMap.Add(configValues[ConfigType.Name.ToString()] as string, string.Empty);
            }
            if (configValues.ContainsKey(ConfigType.BackupType.ToString()))
            {
                string jobType = (string)configValues[ConfigType.BackupType.ToString()];
                if ("Full".Equals(jobType, StringComparison.OrdinalIgnoreCase))
                {
                    recoveryConfig.JobType = RecoveryJobType.FullBackup;
                }
                else
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION_JSON);
            }

            if (configValues.ContainsKey(ConfigType.Path.ToString()))
            {
                string path = (string)configValues[ConfigType.Path.ToString()];
                //validate
                recoveryConfig.RecoveryPath = path;
            }
            if (configValues.ContainsKey(ConfigType.UserName.ToString()))
            {
                string userName = (string)configValues[ConfigType.UserName.ToString()];
                //validate
                recoveryConfig.UserName = userName;
            }

            if (configValues.ContainsKey(ConfigType.Password.ToString()))
            {
                string password = (string)configValues[ConfigType.Password.ToString()];
                //validate
                recoveryConfig.Password = password;
            }
            configuration = recoveryConfig;
        }
        #endregion

        #region Resotre configuration population.
        private void PopulateRestoreConfiguration(Dictionary<string, object> configValues,
            out ICloneable configuration)
        {
            RecoveryConfiguration recoveryConfig = new RecoveryConfiguration();
            string databaseName = "";
            if (configValues.ContainsKey(ConfigType.Name.ToString()))
            {
                databaseName = configValues[ConfigType.Name.ToString()] as string;
            }
            if (configValues.ContainsKey(ConfigType.SourceDatabase.ToString()))
            {
                recoveryConfig.DatabaseMap.Add(configValues[ConfigType.SourceDatabase.ToString()] as string, databaseName.ToLower());
            }
            else
            {
                recoveryConfig.DatabaseMap.Add(databaseName.ToLower(), string.Empty);
            }
            if (configValues.ContainsKey(ConfigType.RestoreType.ToString()))
            {
                string jobType = configValues[ConfigType.RestoreType.ToString()] as string;
                if ("Full".Equals(jobType, StringComparison.OrdinalIgnoreCase))
                {
                    recoveryConfig.JobType = RecoveryJobType.Restore;
                }
                else
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_DDL_CONFIGURATION_JSON);
            }
            if (configValues.ContainsKey(ConfigType.Path.ToString()))
            {
                string path = (string)configValues[ConfigType.Path.ToString()];
                //validate
                recoveryConfig.RecoveryPath = path;
            }
            if (configValues.ContainsKey(ConfigType.UserName.ToString()))
            {
                string userName = (string)configValues[ConfigType.UserName.ToString()];
                //validate
                recoveryConfig.UserName = userName;
            }

            if (configValues.ContainsKey(ConfigType.Password.ToString()))
            {
                string password = (string)configValues[ConfigType.Password.ToString()];
                //validate
                recoveryConfig.Password = password;
            }

            configuration = recoveryConfig;

        }
        #endregion

        private int GetInt32(string attribute, object value)
        {
            if (value == null)
                throw new ArgumentException("value cannot be null", attribute);
            if (!Alachisoft.NosDB.Common.JSON.JsonDocumentUtil.IsNumber(value))
                throw new ArgumentException(string.Format("Unable to convert {0} to Integer", value.GetType().Name), attribute);

            return Int32.Parse(value.ToString());
        }

        private long GetInt64(string attribute, object value)
        {
            if (value == null)
                throw new ArgumentException("value cannot be null", attribute);
            if (!Alachisoft.NosDB.Common.JSON.JsonDocumentUtil.IsNumber(value))
                throw new ArgumentException(string.Format("Unable to convert {0} to Integer", value.GetType().Name), attribute);

            return Int64.Parse(value.ToString());
        }
    }
}
