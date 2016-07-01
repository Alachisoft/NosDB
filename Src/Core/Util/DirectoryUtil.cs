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
using System.IO;
using System.Runtime.InteropServices;

namespace Alachisoft.NosDB.Core.Util
{
    public class DirectoryUtil
    {
        public const string IndexFolderName = @"\indexes\";
        public const string IndexExtension = ".nsi";

        public const string IndexConfigEx = ".conf";

        public const string BoundingBoxExtension = ".bb";
        private readonly static string DeployedAssemblyDir = @"deploy\";
         
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
           out ulong lpFreeBytesAvailable,
           out ulong lpTotalNumberOfBytes,
           out ulong lpTotalNumberOfFreeBytes);

        public static string GetIndexPath(string basePath, string collectionName, string indexName)
        {
            string path = basePath + IndexFolderName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path + collectionName + "." + indexName + IndexExtension;
        }

        public static string GetIndexConfigPath(string basePath, string collectionName, string indexName)
        {
            string path = basePath + IndexFolderName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path + collectionName + "." + indexName + IndexConfigEx;
        }

        public static string GetDeployedPath(string basePath)
        {
            string path = basePath + DeployedAssemblyDir;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// returns free bytes against a specified path provided
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ulong GetDiskFreeSpace(string path)
        {
            ulong FreeBytesAvailable;
            ulong TotalNumberOfBytes;
            ulong TotalNumberOfFreeBytes;

            bool success = GetDiskFreeSpaceEx(path,
                                              out FreeBytesAvailable,
                                              out TotalNumberOfBytes,
                                              out TotalNumberOfFreeBytes);
            if (!success)
                throw new System.ComponentModel.Win32Exception();

            return TotalNumberOfFreeBytes; 
        }
    }
}
