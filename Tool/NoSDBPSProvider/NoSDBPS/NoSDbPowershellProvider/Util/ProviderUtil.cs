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
using System.Threading.Tasks;
using System.Management.Automation;


namespace Alachisoft.NosDB.NosDBPS
{
    public class ProviderUtil
    {
        public const string SHARDS = "shards";
        public const string DATABASES = "databases";
        public const string COLLECTIONS = "collections";
        public const string FUNCTIONS = "functions";
        public const string JOURNALING = "journaling";
        public const string TRIGGERS = "triggers";
        public const string INDICES = "indices";
        public const string SEPARATOR = "\\";
        public const string CONFIGCLUSTER = "configcluster";
        public const string CONFIG_NOT_CONNECTED_EXCEPTION = "Must connect to configuration manager before performing any operation";
        public const string RUNNING = "Running";
        public const string NOT_RUNNING = "Not Running";
        public const string PRIMARY = "Primary";
        public const string NOT_PRIMARY = "Not Primary";
        public const string NOT_FOUND = "not found";
        public const string DRIVE_NAME = "NosDB";
        public const string DRIVE_ROOT = "NosDB";
        public const string STARTED = "Started";
        public const string PRIMARY_STATUS = "(Primary)";
        public const string STOPPED = "Stopped";
        public const string ACTIVE = "<Active>";
        public const string TEST_COLLECTION_NAME = "nosdb_test_coll";
        public static string HEADER = new string('-', 50);
        public const string CONFIGURATION_DUMP_FILE_EXTENSION = ".json";


        public static string[] SplitPath(string path, PSDriveInfo drive)
        {
            // Normalize the path before splitting
            string normalPath = NormalizePath(path);

            // Return the path with the drive name and first path 
            // separator character removed, split by the path separator.
            string pathNoDrive = normalPath.Replace(drive.Root + ":\\"
                                           + ProviderUtil.SEPARATOR, "");
            string[] pathChunks = pathNoDrive.Split(ProviderUtil.SEPARATOR.ToCharArray());

            if (pathChunks[0].Equals(drive.Name + ":"))
            {
                string[] result = new string[pathChunks.Length - 1];
                Array.Copy(pathChunks, 1, result, 0, pathChunks.Length - 1);
                return result;
            }
            return pathChunks;
        }

        private static string NormalizePath(string path)
        {
            string result = path;

            if (!String.IsNullOrEmpty(path))
            {
                result = path.Replace("/", ProviderUtil.SEPARATOR);
            }

            return result;
        }

        public static string GetPrimeryAddress()
        {
            return ConfigurationConnection.Current.ConfigServerIP;
        }

        public static string GetLocalAddress()
        {
            System.Net.IPAddress localAddress = null;
            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            if (hostEntry.AddressList != null)
            {
                foreach (System.Net.IPAddress addr in hostEntry.AddressList)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localAddress = addr;
                        break;
                    }
                }
            }
            return localAddress.ToString();
        }

        public static string GetConnectionString(string databaseName)
        {
            bool localInstance = ConfigurationConnection.ClusterConfiguration.Name.Equals("local",
                StringComparison.InvariantCultureIgnoreCase);
            return "Data Source=" + ConfigurationConnection.Current.ConfigServerIP + ";" + "Port=" +
                            ConfigurationConnection.Current.Port + ";" + "Database=" + databaseName + ";" + "Local Instance=" + localInstance + ";";
        }
    }
}
