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
using System.Net;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Net;
using Alachisoft.NosDB.Common.Server.Engine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Common
{
    public class MiscUtil
    {
        #region String Constants
        public const string CLUSTER_MANAGER = "ClusterManager";
        public const string TOPOLOGY = "Topology";
        public const string MEMBERSHIP_MANAGER = "MembershipManager";
        public const string CONFIGURATION_MANAGER = "ConfigurationManager";
        public const string SHARD_MANAGER = "ShardManager";
        public const string SYSTEM_DATABASE = "sysdb";
        public const string CONFIGURATION_DATABASE = "configdb";
        public const string CONFIGURATION_SHARD_NAME = "configurationserver";
        public const string NOSCONF_SPN = "NosConfSvc";
        public const string RouterEngine = "RouterEngine";
        public const string NOSDB_SPN = "NosDBSvc";
        public const string LOCAL = "local";
        public const string CLUSTERED = "cluster";
        public const string CONFIG_CLUSTER = "databasecluster";
        public const string NOSDB_CLUSTER_SERVER = "NosDB_Cluster_Server"; //for resource id of nosdb system, for security/authorization
        public const string NOSDB_DBSVC_NAME = "NosDBSvc";
        public const string NOSDB_CSVC_NAME = "NosConfSvc";
        public const string NOSDB_DISTSVC_NAME = "NosDistributorSvc";
        public const string DATA_FOLDERS_SEPERATION = ".";
        public const string ROLLBACK_DOCUMENT = "rbDoc";
        public const string STAND_ALONE = "standalone";
        public const string CS_SESSION_ID = "cssessionid";
        #endregion
    
        #region Number Constants
        public const long DEFAULT_CACHE_SPACE = 536870912;
        public const int MONITORING_WAIT_TIME = 5000; //miliseconds
        public const int DEFAULT_CS_PORT = 9950;
        public const long MIN_FILE_SIZE = 5242880;  //5MB
        public const long MAX_FILE_SIZE = 1099511627776;  //1TB
        public const int MAX_CS_LIMIT = 2;  // Maximum number of configuration server nodes that can be added into the config cluster
        #endregion

        public static long MB = 1024 * 1024;
        /// <summary>
        /// Converts bytes into mega bytes (MB).
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>MB</returns>
        public static double ConvertToMegaBytes(long bytes)
        {
            return (double)bytes / (1024 * 1024);
        }

        public static void IsArgumentNull(object argument) 
        {
            if (argument == null)
                throw new ArgumentNullException("provided argument is null");
        }

        private static bool IsNumber(object value)
        {
            if (value == null)
                return false;

            Type valueType = value.GetType();
            TypeCode actualType = Type.GetTypeCode(valueType);
            switch (actualType)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static string GetArrayAsString(ICollection array)
        {
            string query = "[";
            int i = 0;
            foreach(object o in array)
            {
                if (i == 0)
                {
                    query += AsString(o);
                    i++;
                }
                else
                    query += ", " + AsString(o);
            }
            query += "]";
            return query;
        }

        private static string AsString(object value)
        {
            if (value == null)
                return "";
            if (IsNumber(value) || value is bool)
                return value.ToString();
            if (value is string)
                return "'" + value.ToString() + "'";
            if (value is ICollection)
                return GetArrayAsString((ICollection)value);
            if (value is DateTime)
                return "DateTime('" + value.ToString() + "')";

            return value.ToString();
        }


        public static string GetQueryString(IQuery query)
        {
            string queryString = "";
            queryString += query.QueryText;

            if(query.Parameters != null && query.Parameters.Count > 0)
            {
                foreach (IParameter parameter in query.Parameters)
                {
                    // Replace first occurrance only.........
                    int findLengh = ("@" + parameter.Name).Length;
                    int position = queryString.IndexOf("@" + parameter.Name);
                    if(position >= 0)
                        queryString = queryString.Substring(0, position) + AsString(parameter.Value) + queryString.Substring(position + findLengh);
                }
            }

            return queryString;
        }

        public static Queue<T> CloneQueue<T>(Queue<T> source)
        {
            var localCopy = new T[source.Count];
            source.CopyTo(localCopy, 0);
            return new Queue<T>(localCopy);
		}
        public static string GetAddressInfo(System.Net.IPAddress peerIp, int peerPort)
        {
            return peerIp + ":" + peerPort;
        }

        public static Address GetDbManagementAddress()
        {
            ConfigurationSettings<CSHostSettings>.Current.LoadDBConfig();
            var dbAddress = new Address();
            if(ConfigurationSettings<CSHostSettings>.Current.ManagementServerIp!=null)
                dbAddress.ip = ConfigurationSettings<CSHostSettings>.Current.ManagementServerIp.ToString();
            dbAddress.Port = ConfigurationSettings<CSHostSettings>.Current.ManagementServerPort;
            
            return dbAddress;
        }

    }
}

