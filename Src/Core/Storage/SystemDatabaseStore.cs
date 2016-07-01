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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Toplogies;
using System;


namespace Alachisoft.NosDB.Core.Storage
{
    class SystemDatabaseStore : DatabaseStore
    {
        protected override bool CreateCollectionInternal(CollectionConfiguration configuration, NodeContext nodeContext, bool isNew)
        {
            if (_collections.ContainsKey(configuration.CollectionName))
                return false;
            lock (Collections)
            {
                ICollectionStore collection;
               
                    collection = new BaseCollection(_dbContext, nodeContext);
                collection.ShardName = _nodeContext.LocalShardName;
                bool success = collection.Initialize(configuration, _queryResultManager, this, null);
                if (success)
                {
                    //    // set status running incase of internal collections
                    //   collection.Status.SetStatusBit(CollectionStatus.RUNNING, CollectionStatus.INITIALIZING);
                    _collections[collection.Name] = collection;
                    //    if (configuration.CollectionType.Equals(CollectionType.CappedCollection.ToString()))
                    //        ((CappedCollection) collection).PopulateData();
                }
                return success;
            }

            /*if (_collections.ContainsKey(configuration.CollectionName))
                return false;
            if (_dbContext.StorageManager.CreateCollection(configuration.CollectionName))
            {
                ICollectionStore collection = new BaseCollection(_dbContext, nodeContext);
                bool success = collection.Initialize(configuration, _queryResultManager, this);
                if (success)
                    _collections[collection.Name] = collection;
                return success;
            }
            return false;*/
        }

        protected override void RegisterTasks()
        {
            //BucketInfo Task should not be registered with SYSDB
            //TimeScheduler.Global.AddTask(new UpdateBucketInfoTask(this, _nodeContext, _dbContext.DatabaseName));
            
            //TODO: uncomment the code after consulting 
            //TimeScheduler.Task esentDefragmentationTask = new EsentDefragmentationTask(_dbContext.StorageManager);
            //_dbTimeScheduler.AddTask(esentDefragmentationTask);
        }

        protected void CreateSystemCollections()
        {
        }
    }
}
