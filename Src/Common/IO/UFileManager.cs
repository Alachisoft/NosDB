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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Alachisoft.NosDB.Common.IO
{
    /// <summary>
    /// A simple file manager to store multiple objects in a single file
    /// The manager uses block chaining and can become fragmented for large objects
    /// 
    /// Use UFileManager.Create() to create new File
    /// Use UFileManager() constructor to initialize an already created file
    /// 
    /// </summary>
    public class UFileManager : IDisposable, IEnumerable<KeyValuePair<string,object>>
    {
        private int AvailableMidBlockSize;
        private int AvailableEndBlockSize;

        private Header mainHeader;
        private Dictionary<string, long> fileMap;
        private int blockSize;
        private FileStream stream;
        private string blockSizeKey = "$_BlockSize_$";
        private IDataSerializer dataSerializer;

        private UFileManager() { }

        public UFileManager(string fileName, IDataSerializer serializer)
        {
            stream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            mainHeader = Header.FromBytes(ReadBytes(Header.HeaderSize));
            byte[] mainData = GetData(0);
            if (!mainHeader.VerifyChecksum(mainData))
                throw new InvalidDataException("Cyclic Redundancy Check failed. The file map is corrupt. ");
            fileMap = serializer.Deserialize<Dictionary<string, long>>(mainData);
            blockSize = Convert.ToInt32(fileMap["$_BlockSize_$"]);
            AvailableEndBlockSize = blockSize - Header.HeaderSize;
            AvailableMidBlockSize = AvailableEndBlockSize - sizeof (long);
            dataSerializer = serializer;
        }

        public static UFileManager Create(string fileName, IDataSerializer serializer, bool crcEnabled,
            int blockSize = 4096)
        {
            UFileManager manager = new UFileManager();
            manager.dataSerializer = serializer;

            manager.mainHeader = new Header();
            manager.mainHeader.CRCEnabled = crcEnabled;
            manager.mainHeader.Filled = true;
            manager.mainHeader.LastBlock = true;

            manager.blockSize = blockSize;
            manager.fileMap = new Dictionary<string, long>();
            manager.stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            manager.fileMap.Add("$_BlockSize_$", manager.blockSize);
            manager.AvailableEndBlockSize = blockSize - Header.HeaderSize;
            manager.AvailableMidBlockSize = manager.AvailableEndBlockSize - sizeof(long);

            byte[] mapBytes = serializer.Serialize(manager.fileMap);
            manager.FillData("", mapBytes, 0);
            //manager.mainHeader.Length = mapBytes.Length;
            //manager.mainHeader.SetChecksum(mapBytes);
            //byte[] header = Header.ToBytes(manager.mainHeader);
            //manager.stream.Write(header, 0, header.Length);
            //manager.stream.Write(mapBytes, 0, mapBytes.Length);
            //manager.stream.Flush();
            return manager;
        }

        public void WriteObject(string name, object map)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Provided string identifier cannot be null or empty. ");

            long position = 0;
            Header header;
            byte[] serializedObject = dataSerializer.Serialize(map);
            if (fileMap.TryGetValue(name, out position))
            {
                ClearData(position);
                stream.Seek(position, SeekOrigin.Begin);
                FillData(name, serializedObject, position);

            }
            else
            {
                position = GetNextFreeBlock(out header);
                fileMap.Add(name, position);
                FillData(name, serializedObject, position);
                PersistMap();
            }
        }

        public bool DeleteObject(string name)
        {
            long position;
            if (fileMap.TryGetValue(name, out position))
            {
                ClearData(position);
                fileMap.Remove(name);
                PersistMap();
                return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (var position in fileMap.Values)
            {
                ClearData(position);
            }
            fileMap.Clear();
            fileMap.Add(blockSizeKey, blockSize);
            PersistMap();
        }

        private void ClearData(long position)
        {
            stream.Seek(position, SeekOrigin.Begin);
            Header header = Header.FromBytes(ReadBytes(Header.HeaderSize));
            if (header.Filled)
            {

                header.Filled = false;
                stream.Seek(-Header.HeaderSize, SeekOrigin.Current);
                stream.Write(Header.ToBytes(header), 0, Header.HeaderSize);
                stream.Flush();
                if (!header.LastBlock)
                {
                    byte[] nextPointer = new byte[8];
                    stream.Seek(AvailableMidBlockSize, SeekOrigin.Current);
                    stream.Read(nextPointer, 0, 8);
                    long nextPosition = BitConverter.ToInt64(nextPointer, 0);
                    if (nextPosition > 0)
                        ClearData(nextPosition);
                }
            }
        }

        private void FillData(string name, byte[] data, long position)
        {
            Header header = new Header();
            header.Filled = true;
            header.CRCEnabled = mainHeader.CRCEnabled;
            header.ObjectHash = name.GetHashCode();

            int apparentLength = data.Length;
            int bytesWritten = 0;
            if (data.Length > AvailableEndBlockSize)
            {
                int requiedBlocks = (data.Length/AvailableMidBlockSize);
                while (requiedBlocks > 0)
                {
                    header.SeqId = Convert.ToInt16(requiedBlocks);
                    header.Length = AvailableMidBlockSize;
                    header.SetChecksum(data, bytesWritten, AvailableMidBlockSize);

                    stream.Seek(position, SeekOrigin.Begin);
                    stream.Write(Header.ToBytes(header), 0, Header.HeaderSize);
                    stream.Write(data, bytesWritten, AvailableMidBlockSize);

                    bytesWritten += AvailableMidBlockSize;
                    position = stream.Position;

                    Header tempHeader;
                    long nextAvailableBlock = GetNextFreeBlock(out tempHeader);
                    stream.Seek(position, SeekOrigin.Begin);
                    byte[] chain = BitConverter.GetBytes(nextAvailableBlock);
                    stream.Write(chain, 0, chain.Length);
                    stream.Flush();

                    position = nextAvailableBlock;
                    requiedBlocks--;
                }
                apparentLength -= bytesWritten;
            }
            header.LastBlock = true;
            header.Length = apparentLength;
            header.SeqId = 0;
            header.SetChecksum(data, bytesWritten, apparentLength);
            stream.Seek(position, SeekOrigin.Begin);
            stream.Write(Header.ToBytes(header), 0, Header.HeaderSize);
            stream.Write(data, bytesWritten, apparentLength);
            stream.Flush();
        }

        private void PersistMap()
        {
            byte[] serializedMap = dataSerializer.Serialize(fileMap);
            FillData("", serializedMap, 0);
        }

        private byte[] GetData(long position)
        {
            stream.Seek(position, SeekOrigin.Begin);
            byte[] headerBytes = new byte[Header.HeaderSize];
            stream.Read(headerBytes, 0, Header.HeaderSize);
            Header header = Header.FromBytes(headerBytes);
            if (header.Filled)
            {
                using (MemoryStream readMs = new MemoryStream())
                {
                    while (!header.LastBlock && header.SeqId >= 0)
                    {
                        byte[] blockData = new byte[header.Length];
                        stream.Read(blockData, 0, header.Length);
                        readMs.Write(blockData, 0, blockData.Length);
                        if (header.CRCEnabled && !header.VerifyChecksum(blockData))
                            throw new InvalidDataException(
                                "Cyclic Redundancy Check failure. The data for the requested object is corrupt. ");
                        byte[] nextBlockAddress = new byte[8];
                        stream.Read(nextBlockAddress, 0, 8);
                        position = BitConverter.ToInt64(nextBlockAddress, 0);
                        stream.Seek(position, SeekOrigin.Begin);
                        stream.Read(headerBytes, 0, Header.HeaderSize);
                        header = Header.FromBytes(headerBytes);
                    }
                    byte[] finalBlockData = new byte[header.Length];
                    stream.Read(finalBlockData, 0, header.Length);
                    readMs.Write(finalBlockData, 0, finalBlockData.Length);
                    return readMs.ToArray();
                }
            }
            return null;
        }

        public object ReadObject(string name)
        {
            long position;
            if (fileMap.TryGetValue(name, out position))
                return dataSerializer.Deserialize(GetData(position));
            return null;
        }

        private long MaxBlock
        {
            get
            {
                long max = 0;
                foreach (var value in fileMap)
                {
                    if (!value.Key.Equals(blockSizeKey))
                        if (max < value.Value)
                            max = value.Value;
                }
                return max;
            }
        }

        private long GetNextFreeBlock(out Header header)
        {
            header = null;
            long max = this.MaxBlock;
            stream.Seek(max + blockSize, SeekOrigin.Begin);
            while (true)
            {
                header = Header.FromBytes(ReadBytes(Header.HeaderSize));
                if (!header.Filled)
                    return stream.Position;
                stream.Seek(blockSize, SeekOrigin.Current);
            }
        }

        private byte[] ReadBytes(int length)
        {
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        public object this[string name]
        {
            get { return ReadObject(name); }
            set { WriteObject(name, value); }
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public ICollection AllObjectNames
        {
            get { return fileMap.Keys; }
        }

        public int ObjectCount { get { return fileMap.Count; } }

        public bool ContainsObject(string name)
        {
            return fileMap.ContainsKey(name);
        }

        public void VerifyData()
        {
            throw new NotImplementedException();
        }

        public void Defragment()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new UFEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new UFEnumerator(this);
        }

        protected class Header
        {
            public const int HeaderSize = 15;
            private byte flags;
            private int length;
            private int crc;
            private short seqId;
            private int objectHash;

            public bool Filled
            {
                get { return (flags & 1) == 1; }
                set { flags = value ? flags |= 1 : flags &= 254; }
            }

            public bool CRCEnabled
            {
                get { return (flags & 2) >> 1 == 1; }
                set { flags = value ? flags |= 2 : flags &= 253; }
            }

            public bool LastBlock
            {
                get { return (flags & 4) >> 2 == 1; }
                set { flags = value ? flags |= 4 : flags &= 251; }
            }

            public int Length
            {
                get { return length; }
                set { length = value; }
            }

            public int Checksum
            {
                get { return crc; }
                set { crc = value; }
            }

            public short SeqId
            {
                get { return seqId; }
                set { seqId = value; }
            }

            public int ObjectHash
            {
                get { return objectHash; }
                set { objectHash = value; }
            }

            public void SetChecksum(byte[] data)
            {
                if (CRCEnabled)
                {
                    Crc32 _crc = new Crc32(data);
                    crc = _crc.Value;
                }
            }

            public void SetChecksum(byte[] data, int offset, int length)
            {
                if (CRCEnabled)
                {
                    Crc32 _crc = new Crc32();
                    _crc.Add(data, offset, length);
                    crc = _crc.Value;
                }
            }

            public bool VerifyChecksum(byte[] data, int offset, int length)
            {
                if (CRCEnabled)
                {
                    Crc32 _crc = new Crc32();
                    _crc.Add(data, offset, length);
                    return crc == _crc.Value;
                }

                return true;
            }

            public bool VerifyChecksum(byte[] data)
            {
                if (CRCEnabled)
                {
                    Crc32 _crc = new Crc32(data);
                    return crc == _crc.Value;
                }
                return true;
            }

            public static byte[] ToBytes(Header source)
            {
                byte[] bytes = new byte[HeaderSize];
                bytes[0] = source.flags;
                byte[] length = BitConverter.GetBytes(source.length);
                byte[] checksum = BitConverter.GetBytes(source.crc);
                byte[] seqid = BitConverter.GetBytes(source.seqId);
                byte[] hash = BitConverter.GetBytes(source.objectHash);
                length.CopyTo(bytes, 1);
                checksum.CopyTo(bytes, 5);
                seqid.CopyTo(bytes, 9);
                hash.CopyTo(bytes, 11);
                return bytes;
            }

            public static Header FromBytes(byte[] source)
            {
                Header header = new Header();
                header.flags = source[0];
                header.length = BitConverter.ToInt16(source, 1);
                header.crc = BitConverter.ToInt32(source, 5);
                header.seqId = BitConverter.ToInt16(source, 9);
                header.objectHash = BitConverter.ToInt32(source, 11);
                return header;
            }

        }

        protected class UFEnumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private UFileManager source;
            private bool isDisposed = false;
            private IEnumerator<string> keyEnumerator;
            private KeyValuePair<string,object> current;

            public UFEnumerator(UFileManager manager)
            {
                source = manager;
                keyEnumerator = source.fileMap.Keys.GetEnumerator();
            }

            public KeyValuePair<string, object> Current
            {
                get { return current; }
            }

            public void Dispose()
            {
                keyEnumerator.Dispose();
            }

            object IEnumerator.Current
            {
                get { return current.Value; }
            }

            public bool MoveNext()
            {
                if (keyEnumerator.MoveNext())
                {
                    object obj = source.ReadObject(keyEnumerator.Current);
                    current = new KeyValuePair<string, object>(keyEnumerator.Current, obj);
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                keyEnumerator.Reset();
            }
        }
    }
}
