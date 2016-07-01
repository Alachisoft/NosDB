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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    internal class Segment
    {
        private Header _segmentHeader = null;

        private long _dataCapacity = 15999000;// 1kb reserved for header
        private long _headerCapacity = 1000;
        private bool _headerFlushed;


        private long _currentSize = 0;
        private IDictionary<long, DataSlice> _containingDataSlice;
        #region Properties

        public bool HeaderFlushed
        {
            get { return _headerFlushed; }

        }

        public long HeaderStartingOffset
        {
            get
            {
                return (BackupFile.HeaderCapacity + ((_dataCapacity + _headerCapacity) * SegmentHeader.Id)) + 1;
            }
        }

        public long DataStartingOffset
        {
            get
            {
                return (BackupFile.HeaderCapacity + ((_dataCapacity + _headerCapacity) * SegmentHeader.Id)) + _headerCapacity + 1;
            }
        }

        public Header SegmentHeader { get { return _segmentHeader; } }

        public long Capcity
        {
            get
            {
                return _dataCapacity;
            }
        }

        public long Size
        {
            get
            {
                return _currentSize;
            }
        }

        public long EmptySpace { get { return _dataCapacity - _currentSize; } }
        #endregion

        public Segment(long id)
        {
            _segmentHeader = new Header(id);
            _containingDataSlice = new HashVector<long, DataSlice>();
            _headerFlushed = false;

        }

        #region Methods
        public void WriteDataSlice(Stream stream, DataSlice dataSlice, long offset, long length = -1)
        {
            // segment starting offset i.e {[(16mb * segments #)+file_header_space] + (current offset+ reserved_space_for_header+1)} 
            long _startingOffset = ((BackupFile.HeaderCapacity + ((_dataCapacity + _headerCapacity) * SegmentHeader.Id)) + (_headerCapacity + _currentSize + 1));

            //add slice id and starting offset of each slice
            if (!_segmentHeader.SliceMap.ContainsKey(dataSlice.SliceHeader.Id))
            {
                _segmentHeader.SliceMap.Add(dataSlice.SliceHeader.Id, _startingOffset);
            }

            if (length == -1)
                dataSlice.SliceHeader.MappedData = dataSlice.Size;
            else
                dataSlice.SliceHeader.MappedData = length;
             if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsInfoEnabled)
                 LoggerManager.Instance.RecoveryLogger.Info("Segment.Write()", _segmentHeader.ToString() + " Writing to file " + dataSlice.SliceHeader.ToString());
          
            try
            {
                //Calculate crc for data

                // seek to point
                stream.Seek(_startingOffset, SeekOrigin.Begin);

                //1. write slice header
                byte[] header = CompactBinaryFormatter.ToByteBuffer(dataSlice.SliceHeader, string.Empty);

                int _headerLength = header.Length;
                stream.Write(BitConverter.GetBytes(_headerLength), 0, 4);
                _currentSize += 4;

                stream.Write(header, 0, _headerLength);
                _currentSize += _headerLength;

                //2. write data as per offset, if length is 0 then write entire data

                byte[] data = dataSlice.Data;
                stream.Write(BitConverter.GetBytes(length == -1 ? dataSlice.Size : length), 0, 4);
                _currentSize += 4;

                // write complete data
                if (length == -1)
                {
                    int _dataLength = data.Length;

                    stream.Write(data, 0, _dataLength);
                    _currentSize += _dataLength;
                }
                else
                {
                    // write truncated data
                    byte[] truncated = CopySlice(data, offset, length);
                    int _dataLength = truncated.Length;

                    stream.Write(truncated, 0, _dataLength);
                    _currentSize += _dataLength;
                }

                //3. if segment is completely filled write segment header
                if (EmptySpace < 5)
                    SaveHeader(stream);
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("Segment.WriteSlice()", ex.ToString());
            }
        }

        public byte[] ReadDataSlice(Stream stream, long sliceId)
        {
            long dataLength = _containingDataSlice[sliceId].SliceHeader.MappedData;
            byte[] _data = new byte[dataLength];

            long _startingOffset = SegmentHeader.SliceMap[sliceId];

            try
            {
                stream.Seek(_startingOffset, SeekOrigin.Begin);

                // read header length
                byte[] len = new byte[4];
                if (stream.Read(len, 0, 4) > 0)
                {
                    int length = BitConverter.ToInt32(len, 0);
                    byte[] _header = new byte[length];
                    stream.Read(_header, 0, length);

                    if (stream.Read(len, 0, 4) > 0)
                    {
                        length = BitConverter.ToInt32(len, 0);

                        if (length == dataLength)// dont really know the wisdom behind this check
                        {
                            stream.Read(_data, 0, length);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("Segment.ReadSlice()", ex.ToString());
            }

            return _data;
        }

        public void Save()
        {

        }

        public void SaveHeader(Stream stream)
        {
            long _startingOffset = (BackupFile.HeaderCapacity + ((_dataCapacity + _headerCapacity) * SegmentHeader.Id)) + 1;

            try
            {
                stream.Seek(_startingOffset, SeekOrigin.Begin);
                byte[] header = CompactBinaryFormatter.ToByteBuffer(_segmentHeader, string.Empty);
                int _headerLength = header.Length;

                stream.Write(BitConverter.GetBytes(_headerLength), 0, 4);
                stream.Write(header, 0, _headerLength);
                _headerFlushed = true;
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("Segment.SaveHeader()", ex.ToString());
            }
        }
        #endregion

        #region Helper Methods

        private T[] CopySlice<T>(T[] source, long index, long length, bool padToLength = false)
        {
            long n = length;
            T[] slice = null;

            if (source.Length < index + length)
            {
                n = source.Length - index;
                if (padToLength)
                {
                    slice = new T[length];
                }
            }

            if (slice == null) slice = new T[n];
            Array.Copy(source, index, slice, 0, n);
            return slice;
        }

        internal void RecreateMetaInfo(Stream stream, long offset)
        {

            long _startingOffset = offset;

            try
            {

                stream.Seek(_startingOffset, SeekOrigin.Begin);

                // read header length
                byte[] len = new byte[4];
                if (stream.Read(len, 0, 4) > 0)
                {

                    int length = BitConverter.ToInt32(len, 0);
                    byte[] data = new byte[length];
                    stream.Read(data, 0, length);
                    _segmentHeader = CompactBinaryFormatter.FromByteBuffer(data, string.Empty) as Segment.Header;
                }
            }
            catch (Exception ex)
            {

                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("Segment.RecreateMeta()", ex.ToString());
            }
        }

        internal List<DataSlice> RecreateSliceMetaInfo(Stream stream)
        {
            foreach (long key in SegmentHeader.SliceMap.Keys)
            {
                DataSlice _slice = new DataSlice(key);

                long _startingOffset = SegmentHeader.SliceMap[key];

                try
                {

                    stream.Seek(_startingOffset, SeekOrigin.Begin);

                    // read header length
                    byte[] len = new byte[4];
                    if (stream.Read(len, 0, 4) > 0)
                    {

                        int length = BitConverter.ToInt32(len, 0);
                        byte[] data = new byte[length];
                        stream.Read(data, 0, length);

                        _slice.PopulateHeader(CompactBinaryFormatter.FromByteBuffer(data, string.Empty) as DataSlice.Header);

                        _slice.SliceHeader.SegmentIds.Add(_segmentHeader.Id);

                        // internal map
                        _containingDataSlice.Add(_slice.SliceHeader.Id, _slice);

                    }

                }
                catch (Exception ex)
                {

                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("Segment.RecreateSliceMeta()", ex.ToString());
                }
            }
            return _containingDataSlice.Values.ToList();
        }
        #endregion

        public class Header : ICompactSerializable
        {
            string _delimeter = " ~ ";
            private long sliceCount;
            private long _id;
            private long _crc;
            internal Header(long id)
            {
                _id = id;
                SliceMap = new HashVector<long, long>();
                sliceCount = 0;
                _crc = 0;
            }
            public long CRC { get { return _crc; } set { _crc = value; } }
            public long Id { get { return _id; } }
            public long NumberOfSlices { get { return sliceCount; } set { sliceCount = value; } }
            public IDictionary<long, long> SliceMap { get; set; }

            public override string ToString()
            {
                string _header = string.Empty;

                _header += "Segment_ID : " + Id + _delimeter + "Number of Slices : " + sliceCount + _delimeter + "CRC : " + _crc;
                foreach (long _segID in SliceMap.Keys)
                {
                    _header += _delimeter + "SliceID_offset : " + _segID + " : " + SliceMap[_segID];
                }

                return _header;
            }

            public void Deserialize(Common.Serialization.IO.CompactReader reader)
            {
                _id = reader.ReadInt64();

                sliceCount = reader.ReadInt64();
                _crc = reader.ReadInt64();
                SliceMap = Common.Util.SerializationUtility.DeserializeDictionary<long, long>(reader);
            }

            public void Serialize(Common.Serialization.IO.CompactWriter writer)
            {
                writer.Write(_id);
                writer.Write(sliceCount);
                writer.Write(_crc);
                Common.Util.SerializationUtility.SerializeDictionary<long, long>(SliceMap, writer);

            }
        }


    }
}
