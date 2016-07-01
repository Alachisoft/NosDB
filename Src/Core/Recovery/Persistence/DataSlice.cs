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
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Serialization.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    // serialization is responsibility of the class that adds data. 
    public class DataSlice
    {
        // private List<byte[]> _dataList = null;
        private byte[] _data = null;
        private long _capacity = 5999900;// 5.9 mb, .1kb reserved for  max size of segment, assuming that the encryption will souble the size.
        private long _size = 0;
        private Header _sliceHeader = null;
        internal DataSlice(long id)
        {
            //_dataList = new List<byte[]>();
            _sliceHeader = new Header(id);
        }
                
        #region Properties
        public long EmptySpace { get { return Capcity - Size; } }

        public Header SliceHeader { get { return _sliceHeader; } }

        public long Capcity
        {
            get
            {
                return _capacity;
            }
        }

        public long Size { get { return _size; } }

        public long HeaderSize
        {
            get
            {
                return CompactBinaryFormatter.ToByteBuffer(_sliceHeader, string.Empty).Length;
            }
        }

        public byte[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                _size = _data.LongLength;
            }
        }

        #endregion

        #region Methods
        // revist this code,come up with cloneable/ deep copy behavior
        internal void PopulateHeader(Header source)
        {
            if (source != null)
            {
                _sliceHeader.NextSegmentId = source.NextSegmentId;
                _sliceHeader.OverFlow = source.OverFlow;
                _sliceHeader.CRC = source.CRC;
                _sliceHeader.Database = source.Database;
                _sliceHeader.Collection = source.Collection;
                _sliceHeader.DataCount = source.DataCount;
                _sliceHeader.MappedData = source.MappedData;
                _sliceHeader.TotalSize = source.TotalSize;
                _sliceHeader.ContentType = source.ContentType;
            }
        }
        public int WriteData(byte[] data, int offset, int count)
        {
            return 0;
        }

        public void ReadData(byte[] dataBuffer)
        {
            ReadData(dataBuffer, dataBuffer.Length);
        }

        public void ReadData(byte[] dataBuffer, int count)
        {

        }

        #endregion

        #region Header
        public class Header : ICompactSerializable
        {
            string _delimeter = " ~ ";
            private long _id;
            private long _crc;
            private long _dataCount;
            private long _mappedData;
            private DataSliceType _dataType;
            private long _totalSize;

            

            public long Id { get { return _id; } }
            //public long PreviousSegmentId { get; set; }
            public long NextSegmentId { get; set; }
            public long CRC { get { return _crc; } set { _crc = value; } }
            public string Cluster { get; set; }
            public string Database { get; set; }
            public string Collection { get; set; }
            public bool OverFlow { get; set; }
            public List<long> SegmentIds { get; set; }// incase of overflow
            public long DataCount { get { return _dataCount; } set { _dataCount = value; } }
            public long MappedData { get { return _mappedData; } set { _mappedData = value; } }
            public long TotalSize
            {
                get { return _totalSize; }
                set { _totalSize = value; }
            }

            public Header(long id)
            {
                _id = id;
                NextSegmentId = -1;
                DataCount = 0;
                CRC = 0;
                _mappedData = 0;
                _totalSize = 0;
                SegmentIds = new List<long>();
            }
                 

            public DataSliceType ContentType
            {
                get { return _dataType; }
                set { _dataType = value; }
            }

            public override string ToString()
            {
                string _header = string.Empty;

                _header += "Slice_ID :" + Id + _delimeter + "Database :" + Database + _delimeter + "Collection :" + Collection + _delimeter
                         + "DataCount :" + DataCount + _delimeter + "Overflow :" + OverFlow + _delimeter + "NextSegment :" + NextSegmentId
                         + _delimeter + "CRC :" + CRC + _delimeter + "MappedData :" + _mappedData+" TotalSize: "+_totalSize ;

                return _header;
            }

            public void Deserialize(Common.Serialization.IO.CompactReader reader)
            {
                _id = reader.ReadInt64();
                Database = reader.ReadString();
                Collection = reader.ReadString();
                DataCount = reader.ReadInt64();
                OverFlow = reader.ReadBoolean();
                NextSegmentId = reader.ReadInt64();
                _crc = reader.ReadInt64();
                _mappedData = reader.ReadInt64();
                _dataType = (DataSliceType)reader.ReadInt32();
                _totalSize = reader.ReadInt64();
            }

            public void Serialize(Common.Serialization.IO.CompactWriter writer)
            {
                writer.Write(_id);
                writer.Write(Database);
                writer.Write(Collection);
                writer.Write(DataCount);
                writer.Write(OverFlow);
                writer.Write(NextSegmentId);
                writer.Write(CRC);
                writer.Write(_mappedData);
                writer.Write((int)_dataType);
                writer.Write(_totalSize);
            }


        }
        #endregion




    }
}
