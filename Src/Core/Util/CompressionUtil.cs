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
using System.IO.Compression;
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Core.Util
{
    public class CompressionUtil
    {
        /// <summary>
        /// Compresses a stream of bytes using DeflateStream
        /// </summary>
        /// <param name="inputStream">A Seekable Stream</param>
        /// <returns></returns>
        public static ClusteredMemoryStream Compress(Stream inputStream)
        {
            var ms = new ClusteredMemoryStream();
            using (var compressor = new DeflateStream(ms, CompressionMode.Compress, true))
            {
                inputStream.CopyTo(compressor);
                inputStream.Dispose();
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        /// <summary>
        /// Decompresses a stream of compressed bytes using DeflateStream
        /// </summary>
        /// <param name="inputStream">A Seekable Stream</param>
        /// <returns></returns>
        public static ClusteredMemoryStream Decompress(Stream inputStream)
        {
            var ms = new ClusteredMemoryStream();
            using (var decompressor = new DeflateStream(inputStream, CompressionMode.Decompress, true))
            {
                decompressor.CopyTo(ms);
                inputStream.Dispose();
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

       
     
    }
}
