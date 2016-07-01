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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    internal class MetaStore
    {
        private Hashtable _clusterMetaData = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        // private Dictionary<string,ClusterInfo> _clusterMetaData = new Dictionary<string, ClusterInfo>();
        private IConfigurationStore _configurationStore;

        public MetaStore(IConfigurationStore configStore)
        {
            _configurationStore = configStore;
        }
        //IDictionary<string, ClusterInfo> _clusterMetaData = new Dictionary<string, ClusterInfo>();


        public void AddDistributionStrategy(string cluster, string database, string collection, IDistributionStrategy strategy, DistributionStrategyConfiguration configuration, Boolean needTransfer)
        {
            ClusterInfo clusterInfo = (ClusterInfo)_clusterMetaData[cluster];

            if (clusterInfo.Databases != null)
            {
                DatabaseInfo databaseInfo = clusterInfo.GetDatabase(database);
                
                CollectionInfo collectionInfo = databaseInfo.GetCollection(collection);
                collectionInfo.SetDistributionStrategy(configuration, strategy);
                if (collectionInfo.DistributionStrategy.Name.Equals(DistributionType.NonSharded.ToString()))
                {
                    collectionInfo.DistributionStrategy.AddShard(collectionInfo.CollectionShard, null, needTransfer);
                    //In case of NonShardedDistributionStrategy: The shard told by user must be added first. That is why this check is placed
                }

                _clusterMetaData[cluster] = clusterInfo;
                _configurationStore.InsertOrUpdateDistributionStrategy(clusterInfo, database, collection);
            }

        }


        //public Dictionary<string, ClusterInfo> ClusterMetaData
        //{
        //    get { return _clusterMetaData; }
        //    set { _clusterMetaData = value; }
        //}

        public void RemoveDistributionStrategy(string cluster, string database, string collection)
        {
            ClusterInfo clusterInfo = (ClusterInfo)_clusterMetaData[cluster];

            if (clusterInfo.Databases != null)
            {
                clusterInfo.GetDatabase(database).GetCollection(collection).RemoveDistributionStrategy();

                _clusterMetaData[cluster] = clusterInfo;
                _configurationStore.InsertOrUpdateDistributionStrategy(clusterInfo, database, collection);
            }

        }

        public IDistribution GetDistribution(string cluster, string database, string collection)
        {
            ClusterInfo clusterInfo = (ClusterInfo)_clusterMetaData[cluster];

            if (clusterInfo.Databases != null)
            {
                return clusterInfo.GetDatabase(database).GetCollection(collection).DataDistribution;
            }
            return null;
        }

        public ClusterInfo[] GetAllClusterInfo()
        {
            if (_clusterMetaData != null && _clusterMetaData.Values.Count > 0)
            {
                ClusterInfo[] clusterInfo = new ClusterInfo[_clusterMetaData.Values.Count];
                _clusterMetaData.Values.CopyTo(clusterInfo, 0);
                return clusterInfo;
            }

            return null;
        }

        public void SetDistributionStrategy(string cluster, string database, string collection, IDistributionStrategy distribution)
        {
            ClusterInfo clusterInfo = (ClusterInfo)_clusterMetaData[cluster];

            if (clusterInfo.Databases != null)
            {
                clusterInfo.GetDatabase(database).GetCollection(collection).DistributionStrategy = distribution;
                _configurationStore.InsertOrUpdateDistributionStrategy(clusterInfo, database, collection);
            }
        }

        public IDistributionStrategy GetDistributionStrategy(string cluster, string database, string collection)
        {
            ClusterInfo clusterInfo = (ClusterInfo)_clusterMetaData[cluster];

            if (clusterInfo.Databases != null)
            {
                return clusterInfo.GetDatabase(database).GetCollection(collection).DistributionStrategy;
            }
            return null;
        }

        public ClusterInfo GetClusterInfo(string clusterName)
        {
            if (_clusterMetaData.ContainsKey(clusterName))
                return (ClusterInfo)_clusterMetaData[clusterName];

            return null;
        }

        public void AddClusterInfo(string clusterName, ClusterInfo clusterInfo)
        {
            if (clusterName != null)
            {
                _clusterMetaData[clusterName.ToLower()] = clusterInfo;
                //_configurationStore.InsertOrUpdateBucketInfo(clusterInfo);
            }
        }

        public void RemoveClusterInfo(string clusterName)
        {
            if (clusterName != null)
            {
                _clusterMetaData.Remove(clusterName.ToLower());
                _configurationStore.RemoveClusterInfo(clusterName.ToLower());
            }
        }

        //public void Load()
        //{
        //    string metaFilePath = ServiceConfiguration.CSBasePath + "//MetaData.bin";


        //_clusterMetaData = (Hashtable)Alachisoft.NosDB.Serialization.Formatters.CompactBinaryFormatter.FromByteBuffer(data, string.Empty);
        //    if (File.Exists(metaFilePath))
        //    {
        //        Stream stream = File.Open(metaFilePath, FileMode.Open);
        //        try
        //        {


        //            byte[] len = new byte[4];
        //            stream.Read(len, 0, 4);
        //            int length = BitConverter.ToInt32(len, 0);
        //            byte[] data = new byte[length];
        //            stream.Read(data, 0, length);

        //            _clusterMetaData = (Dictionary<string, ClusterInfo>)Alachisoft.NoSQL.Serialization.Formatters.CompactBinaryFormatter.FromByteBuffer(data, string.Empty);



        //try
        //{

        //    //BinaryFormatter bformatter = new BinaryFormatter();
        //    byte[] data = Alachisoft.NosDB.Serialization.Formatters.CompactBinaryFormatter.ToByteBuffer(_clusterMetaData, String.Empty);

        public void Load()
        {
            ClusterInfo[] clusterinfo;
            clusterinfo = _configurationStore.GetAllClusterInfo();
            _configurationStore.GetAllDistributionStrategies(clusterinfo);
            foreach (ClusterInfo info in clusterinfo)
            {
                if (info != null && info.Name != null && !_clusterMetaData.Contains(info.Name.ToLower()))
                    _clusterMetaData.Add(info.Name.ToLower(), info);

            }

        }

        #region recovery operation
        public void RestoreDistributionStrategy(string cluster, string database, string collection, IDistributionStrategy strategy)
        {
            ClusterInfo clusterInfo = (ClusterInfo)_clusterMetaData[cluster];

            if (clusterInfo.Databases != null)
            {
                clusterInfo.GetDatabase(database).GetCollection(collection).DistributionStrategy = strategy;
                //clusterInfo.GetDatabase(database).GetCollection(collection).DataDistribution = strategy.GetCurrentBucketDistribution();
                _configurationStore.InsertOrUpdateDistributionStrategy(clusterInfo, database, collection);
                
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                    LoggerManager.Instance.RecoveryLogger.Info("MetaStore.RestoreDistributionStrrategy()", "Updated");
            }
        }
        #endregion

        //public void Save(IDatabaseStore dbStore,string colName,string dbName)
        //{
        //    JsonSerializer<Dictionary<string, ClusterInfo>> serializer =
        //        new JsonSerializer<Dictionary<string, ClusterInfo>>();
        //    List<IJSONDocument> jdocList=new List<IJSONDocument>();

        //    if (dbStore != null && _clusterMetaData != null && colName != null)
        //    {
        //        JSONDocument jdoc=new JSONDocument();
        //        jdoc=serializer.Serialize(_clusterMetaData);
        //        jdocList.Add(jdoc);
        //        IDocumentsWriteOperation insertOperation=new InsertDocumentsOperation();
        //        insertOperation.Collection = colName;
        //        insertOperation.Database = dbName;
        //        insertOperation.Documents = jdocList;
        //        dbStore.InserDocuments(insertOperation);
        //    }           
        //}


        //public void Save()
        //{
        //    if (_clusterMetaData != null)
        //    {
        //        foreach (KeyValuePair<string, ClusterInfo> entry in _clusterMetaData)
        //        {
        //            _configurationStore.InsertOrUpdateClusterInfo(entry.Value);
        //        }
        //    }
        //}

        //public void Save()
        //{
        //    string metaFilePath = ServiceConfiguration.CSBasePath + "//MetaData.bin";

        //    string path = Path.GetDirectoryName(metaFilePath);
        //    if (Directory.Exists(path))
        //    {
        //        Stream stream = File.Open(metaFilePath, FileMode.Create);

        //        try
        //        {

        //            //BinaryFormatter bformatter = new BinaryFormatter();
        //            byte[] data = Alachisoft.NoSQL.Serialization.Formatters.CompactBinaryFormatter.ToByteBuffer(_clusterMetaData, String.Empty);

        //            int len = data.Length;

        //            stream.Write(BitConverter.GetBytes(len), 0, 4);
        //            stream.Write(data, 0, data.Length);

        //            stream.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //            stream.Close();
        //        }
        //    }

        //}
    }
}
