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

namespace Alachisoft.NosDB.Common.IO
{
    public class TempFileManager : IDisposable
    {
        private IPathGenerator newPathGenerator;
        private IDictionary<Guid, FileInfo> fileLedger;
        private readonly object syncLock = new object();

        public TempFileManager(string tempDirectory = null)
        {
            if (string.IsNullOrEmpty(tempDirectory))
                newPathGenerator = new DefaultPathGenerator();
            else
                newPathGenerator = new CustomPathGenerator(tempDirectory);
        }

        public FileInfo CreateNew(FileMode mode = FileMode.Create, FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.None, int buffer = 4096, FileOptions options = FileOptions.None)
        {
            lock (syncLock)
            {
                var info = new FileInfo(newPathGenerator.GetNewFilePath(), this, mode, access, share, buffer,
                    options);
                fileLedger.Add(info.FileId, info);
                return info;
            }
        }

        public void DisposeFile(FileInfo info)
        {
            lock (syncLock)
            {
                if (fileLedger.ContainsKey(info.FileId))
                    fileLedger.Remove(info.FileId);
                info.Stream.Dispose();
                File.Delete(info.FilePath);
            }
        }

        public void Dispose()
        {
            lock (syncLock)
            {
                var guids = new Guid[fileLedger.Count];
                fileLedger.Keys.CopyTo(guids, 0);
                foreach (var guid in guids)
                {
                    fileLedger[guid].Dispose();
                }
                fileLedger.Clear();
            }
        }
    }
}
