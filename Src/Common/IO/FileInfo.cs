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
using System.IO;

namespace Alachisoft.NosDB.Common.IO
{
    public class FileInfo : IDisposable
    {
        private string filePath;
        private Guid fileId;
        private FileStream stream;
        private TempFileManager parent;

        internal FileInfo(string path, TempFileManager parentManager, FileMode mode, FileAccess access,FileShare share, int buffer, FileOptions options)
        {
            filePath = path;
            fileId = Guid.NewGuid();
            stream = new FileStream(path, mode, access, share, buffer, options);
            parent = parentManager;
        }

        public string FilePath { get { return filePath; } }

        public Guid FileId { get { return fileId; } }

        public FileStream Stream { get { return stream; } }

        public TempFileManager ParentCreator { get { return parent; } }

        public void Dispose()
        {
            parent.DisposeFile(this);
        }
    }
}