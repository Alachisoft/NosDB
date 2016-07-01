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

namespace Alachisoft.NosDB.Common.Storage.Attachments
{
    class FileBlob : IBlob
    {
        private FileStream _fileStream;

        public long Length
        {
            get { return _fileStream.Length; }
        }

        public FileBlob(FileStream fs)
        {
            if (fs == null)
                throw new ArgumentNullException("FileStream can not be null.");
            _fileStream = fs;
        }

        public int Read(byte[] buffer, int sourceIndex, int destinationIndex, int count)
        {
            if (_fileStream.Position != destinationIndex)
                _fileStream.Seek(destinationIndex, SeekOrigin.Begin);

            return _fileStream.Read(buffer, sourceIndex, count);
        }

        public void Write(byte[] buffer, int sourceIndex, int destinationIndex, int count)
        {
            if (_fileStream.Position != destinationIndex)
                _fileStream.Seek(destinationIndex, SeekOrigin.Begin);

            _fileStream.Write(buffer, sourceIndex, count);
        }

        public void Dispose()
        {
            _fileStream.Close();
            _fileStream.Dispose();
        }
    }
}
