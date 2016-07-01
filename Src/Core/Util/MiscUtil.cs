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
using System.Collections.Generic;
using System.Linq;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Util;
using System.Collections;
using System.Text;
using System;

namespace Alachisoft.NosDB.Core.Util
{
    public static class MiscUtil
    {
        public static List<CollectionConfiguration> SystemCollections = new List<CollectionConfiguration>();

        static MiscUtil()
        {
            //InitializeSystemCollections();
        }

        public class SystemCollection
        {
            public const string ReplicationCollection = "syscolloplog";
            public const string QueryResultCollection = "queryrsltcoll";
            public const string SecurityInformationCollection = "securityinformationcoll";
            public const string UserInformationCollection = "userinformationcoll";
            
            public const string ConfigCollection = "configcoll";

        }



        
        /// <summary>
        /// Returns the number of processors on the system.
        /// </summary>
        public static int NumProcessors
        {
            get
            {
                return DBLicenseDll.GetNumProcessors();
            }
        }

        /// <summary>
        /// Returns the number of total cores available in the system.
        /// </summary>
        public static int NumCores
        {
            get
            {
                return DBLicenseDll.GetNumCores();
            }
        }

        /// <summary>
        /// Returns 0 or 1, If VM based OS found returns 1 else 0
        /// </summary>        
        public static int IsEmulatedOS
        {
            get
            {
                if (DBLicenseDll.IsEmulatedOS() == 1)
                    return 1;

                if (IsHyperV() == 1)
                    return 1;

                return 0;
            }
        }

        /// <summary>
        /// Returns 0 or 1, If VM based OS found returns 1 else 0
        /// </summary>        
        public static int IsHyperV()
        {
            MSHyperVThread mst = new MSHyperVThread();
            return mst.IsHyperV();
        }


        /// <summary>
        /// Returns a list of mac addresses found on the system.
        /// </summary>
        public static ArrayList AdapterAddresses
        {
            get
            {

                StringBuilder addrList = new StringBuilder(2048);

                int num = DBLicenseDll.GetAdaptersAddressList(addrList);
                string address = addrList.ToString();
                string[] addresses = address.Split(new Char[] { ':' });

                ArrayList addrs = new ArrayList(addresses.Length);
                for (int i = 0; i < addresses.Length; i++)
                {
                    addrs.Add(addresses[i].ToLower());
                }
                return addrs;
            }
		}

        internal static CollectionConfiguration GetSecurityInfoCollectionConfig()
        {
            CollectionConfiguration collectionConfiguration = new CollectionConfiguration();

            collectionConfiguration.CollectionName = SystemCollection.SecurityInformationCollection;

            collectionConfiguration.Indices = new Indices();
            collectionConfiguration.EvictionConfiguration = new EvictionConfiguration();
            collectionConfiguration.EvictionConfiguration.EnabledEviction = true;
            collectionConfiguration.EvictionConfiguration.Policy = "lru";

            return collectionConfiguration;
        }

        internal static CollectionConfiguration GetUserInformationCollection()
        {
            CollectionConfiguration collectionConfiguration = new CollectionConfiguration();

            collectionConfiguration.CollectionName = SystemCollection.UserInformationCollection;

            collectionConfiguration.Indices = new Indices();
            collectionConfiguration.EvictionConfiguration = new EvictionConfiguration();
            collectionConfiguration.EvictionConfiguration.EnabledEviction = true;
            collectionConfiguration.EvictionConfiguration.Policy = "lru";

            return collectionConfiguration;
        }
        
        internal static Dictionary<string, CollectionConfiguration> GetSystemCollections(string shardName)
        {
            List<CollectionConfiguration> configurationList=new List<CollectionConfiguration>();
            configurationList.Add(GetSecurityInfoCollectionConfig());
            configurationList.Add(GetQueryCollectionConfig(shardName));
            configurationList.Add(GetUserInformationCollection());
            configurationList.Add(GetConfigCollectionConfig(shardName));

            return configurationList.ToDictionary(x => x.CollectionName, x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        private static CollectionConfiguration GetQueryCollectionConfig(string shardName)
        {
            var queryConfig = new CollectionConfiguration
            {
                CollectionName = SystemCollection.QueryResultCollection,
                Shard = shardName,
                DistributionStrategy = new DistributionStrategyConfiguration
                {
                    Name = DistributionType.NonSharded.ToString(),
                },
                EvictionConfiguration = new EvictionConfiguration
                {
                    EnabledEviction = true,
                    Policy = "lru"
                },
                Indices = new Indices(),
            };
            return queryConfig;
        }

        public static CollectionConfiguration GetConfigCollectionConfig(string shardName)
        {
            var configCol = new CollectionConfiguration
            {
                CollectionName = SystemCollection.ConfigCollection,
                Shard = shardName,
                DistributionStrategy = new DistributionStrategyConfiguration
                {
                    Name = DistributionType.NonSharded.ToString(),
                },
                EvictionConfiguration = new EvictionConfiguration
                {
                    EnabledEviction = true,
                    Policy = "lru"
                },
                Indices = new Indices(),
            };
            return configCol;
        }
    }
}
