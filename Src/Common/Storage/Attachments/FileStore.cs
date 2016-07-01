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
using System.IO;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Common.Storage.Attachments
{
    public class FileStore : IBlobStorageProvider
    {
        private string _basePath = @"\";

        public void Initialize(IDictionary<string, object> initparams)
        {
            //_basePath = initparams["path"] + @"\Attachments\";
            _basePath = initparams["path"].ToString();
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
            TestWrite(_basePath, false);
        }

        public IBlob CreateBlob(string serverID)
        {
            return new FileBlob(new FileStream(_basePath + serverID, FileMode.Create, FileAccess.Write));
        }

        public IBlob GetBlob(string serverID)
        {
            return new FileBlob(new FileStream(_basePath + serverID, FileMode.Open, FileAccess.Read));
        }

        public bool ContainsBlob(string serverID)
        {
            return File.Exists(_basePath + serverID);
        }

        public void DeleteBlob(string serverID)
        {
            File.Delete(_basePath + serverID);
        }

        private bool TestWrite(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                }
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsInfoEnabled)
                    LoggerManager.Instance.StorageLogger.Info("Attachments", "Initialization Successfull.");
                return true;
            }
            catch(Exception e)
            {
                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("Attachments", "Initialization failed with error: " + e);
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }
    }
}
