using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;
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
    public class BackupFile
    {
        private string _filePath;
        private string _name;
        private SegmentManager _segmentManager;
        // private IOStream _fileStream;
        private Stream _fileStream = null;
        private long _sliceCount = 0;
        private long _currentSlice = -1;
        private static long _headerCapacity = 10000;//10000;// 10 kb
        private object _mutex;
        private Header _fileHeader;
        private bool _headerFlushed;
        private string _username;
        private string _password;
        private readonly bool _isSharedPath;


        internal BackupFile(string name, string path, string userName, string password)
        {
            _filePath = path;
            _name = name;
            _segmentManager = new SegmentManager();
            _mutex = new object();
            _fileHeader = new Header(1);//set header id properly
            IsComplete = false;
            _headerFlushed = false;
            _username = userName;
            _password = password;
            _isSharedPath = RecoveryFolderStructure.PathIsNetworkPath(path);
        }

        #region Properties
        public string Name
        {
            get { return _name; }

        }
        public Header FileHeader { get { return _fileHeader; } }
        public static long HeaderCapacity { get { return _headerCapacity; } }
        public bool IsComplete { get; set; }

        public bool HeaderFlushed
        {
            get { return _headerFlushed; }
            //set { _headerFlushed = value; }
        }
        #endregion

        #region Public Methods
        public DataSlice CreateNewDataSlice()
        {
            DataSlice _slice = new DataSlice(_sliceCount);

            _sliceCount++;
            return _slice;
        }

        public void SaveDataSlice(DataSlice dataSlice)
        {
            try
            {
                Impersonation impersonation = null;
                if (_isSharedPath)
                    impersonation = new Impersonation(_username, _password);

                if (_fileStream == null)
                {
                    _fileStream = GetFile();
                }

                //1. get segment from segment manager, 8 is padded for length that is added
                SliceFacilitator[] _segmentList = _segmentManager.GetFacilitatingSegments(dataSlice.Size,
                    dataSlice.HeaderSize, 8);

                if (_segmentList.Length > 1)
                {
                    dataSlice.SliceHeader.OverFlow = true;
                    //  dataSlice.SliceHeader.SegmentIds = _segmentList.Select(x => x.Segment.SegmentHeader.Id).Cast<long>().ToArray();
                    int i = 0;
                    long offset = 0;
                    foreach (SliceFacilitator _fac in _segmentList)
                    {
                        //4. write slice to file
                        if (i + 1 < _segmentList.Length)
                            dataSlice.SliceHeader.NextSegmentId = _segmentList[i + 1].Segment.SegmentHeader.Id;
                        else
                            dataSlice.SliceHeader.NextSegmentId = -1;

                        _fac.Segment.SegmentHeader.NumberOfSlices++;
                        lock (_mutex)
                        {
                            _fac.Segment.WriteDataSlice(_fileStream, dataSlice, offset, _fac.Size);
                        }
                        offset += _fac.Size;
                        i++;
                    }
                }
                else
                {
                    dataSlice.SliceHeader.OverFlow = false;
                    dataSlice.SliceHeader.NextSegmentId = -1;
                    _segmentList[0].Segment.SegmentHeader.NumberOfSlices++;
                    lock (_mutex)
                    {
                        _segmentList[0].Segment.WriteDataSlice(_fileStream, dataSlice, 0);
                    }
                }
                if (_isSharedPath)
                    impersonation.Dispose();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("BackupFile.SaveDataSlice()", ex.ToString());
            }


        }
        // think of a better return value
        public DataSlice ReadDataSlice(DataSlice dataSlice)
        {
            try
            {
                Impersonation impersonation = null;
                if (_isSharedPath)
                    impersonation = new Impersonation(_username, _password);

                if (_fileStream == null)
                {
                    _fileStream = GetFile();
                }
                byte[] _data = new byte[dataSlice.SliceHeader.TotalSize];
                int offset = 0;

                foreach (long segID in dataSlice.SliceHeader.SegmentIds)
                {
                    Segment segment = _segmentManager.GetSegment(segID);

                    byte[] _splitData = segment.ReadDataSlice(_fileStream, dataSlice.SliceHeader.Id);
                    try
                    {
                        System.Buffer.BlockCopy(_splitData, 0, _data, offset, _splitData.Length);

                        offset += _splitData.Length;
                    }
                    catch (Exception exp)
                    {
                        if (LoggerManager.Instance.RecoveryLogger != null &&
                            LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                            LoggerManager.Instance.RecoveryLogger.Error("BackupFile.ReadDataSlice()", exp.ToString());
                    }
                }
                dataSlice.Data = _data;

                if (_isSharedPath)
                    impersonation.Dispose();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("BackupFile.ReadDataSlice()", ex.ToString());
            }

            return dataSlice;
        }

        public bool SaveHeader()
        {
            try
            {
                Impersonation impersonation = null;
                if (_isSharedPath)
                    impersonation = new Impersonation(_username, _password);

                if (!_headerFlushed)
                {
                    if (_fileStream == null)
                    {
                        _fileStream = GetFile();
                    }
                    if (_fileStream != null)
                    {

                        FileHeader.SegmentMap = _segmentManager.SegmentOffsetMap;

                        // flush all segment headers
                        // if already written try not to rewrite it
                        // check it
                        foreach (long _segId in FileHeader.SegmentMap.Keys)
                        {
                            Segment segment = _segmentManager.GetSegment(_segId);
                            if (segment != null)
                            {
                                if (!segment.HeaderFlushed)
                                {
                                    lock (_mutex)
                                    {
                                        segment.SaveHeader(_fileStream);
                                    }
                                }
                            }
                        }

                        //write header in specified header slot
                        long _startingOffset = 0;


                        lock (_mutex)
                        {
                            _fileStream.Seek(_startingOffset, SeekOrigin.Begin);

                            byte[] header = CompactBinaryFormatter.ToByteBuffer(FileHeader, string.Empty);
                            int _headerLength = header.Length;

                            _fileStream.Write(BitConverter.GetBytes(_headerLength), 0, 4);
                            _fileStream.Write(header, 0, _headerLength);
                            _headerFlushed = true;
                        }
                    }
                    else
                        throw new FileNotFoundException();
                }

                if (_isSharedPath)
                    impersonation.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("BackupFile.SaveHeader()", ex.ToString());

                return false;
            }
        }

        public void Close()
        {
            Impersonation impersonation = null;
            if (_isSharedPath)
                impersonation = new Impersonation(_username, _password);

            if (_fileStream != null)
            {
                lock (_mutex)
                {
                    _fileStream.Close();
                }
            }
            if (_isSharedPath)
                impersonation.Dispose();

        }

        public IDictionary<long, DataSlice> RecreateMetaInfo()
        {

            try
            {
                Impersonation impersonation = null;
                if (_isSharedPath)
                    impersonation = new Impersonation(_username, _password);

                if (_fileStream == null)
                {
                    _fileStream = GetFile();
                }
                long _startingOffset = 0;

                lock (_mutex)
                {
                    _fileStream.Seek(_startingOffset, SeekOrigin.Begin);

                    // read header length
                    byte[] len = new byte[4];
                    if (_fileStream.Read(len, 0, 4) > 0)
                    {

                        int length = BitConverter.ToInt32(len, 0);
                        byte[] data = new byte[length];
                        _fileStream.Read(data, 0, length);
                        _fileHeader = CompactBinaryFormatter.FromByteBuffer(data, string.Empty) as BackupFile.Header;

                        // recreate segment header
                        _segmentManager.RecreateSegmentMetaInfo(_fileStream, _fileHeader.SegmentMap);

                        // create slice headers
                        return _segmentManager.RecreateSliceMetaInfo(_fileStream);
                    }
                }
                if (_isSharedPath)
                    impersonation.Dispose();
            }
            catch (Exception ex)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("BackupFile.RecreateMetaInfo()", ex.ToString());
            }
            return null;
        }

        public Header ReadFileHeader()
        {
            if (_fileHeader != null)
            {
                try
                {
                    Impersonation impersonation = null;
                    if (_isSharedPath)
                        impersonation = new Impersonation(_username, _password);

                    if (_fileStream == null)
                    {
                        _fileStream = GetFile();
                    }
                    long _startingOffset = 0;

                    lock (_mutex)
                    {
                        _fileStream.Seek(_startingOffset, SeekOrigin.Begin);

                        // read header length
                        byte[] len = new byte[4];
                        if (_fileStream.Read(len, 0, 4) > 0)
                        {

                            int length = BitConverter.ToInt32(len, 0);
                            byte[] data = new byte[length];
                            _fileStream.Read(data, 0, length);
                            _fileHeader = CompactBinaryFormatter.FromByteBuffer(data, string.Empty) as BackupFile.Header;
                        }
                        _fileStream.Close();
                    }

                    if (_isSharedPath)
                        impersonation.Dispose();
                }
                catch (Exception ex)
                {
                    if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.RecoveryLogger.Error("BackupFile.RecreateMetaInfo()", ex.ToString());
                }
            }
            return _fileHeader;
        }
        #endregion

        #region internal methods
        private Stream GetFile()
        {
            try
            {
                lock (_mutex)
                {
                    if (!Directory.Exists(_filePath))
                        Directory.CreateDirectory(_filePath);

                    if (!File.Exists(_filePath + "\\" + _name))
                        return new FileStream(_filePath + "\\" + _name, FileMode.Create, FileAccess.ReadWrite);//File.Open(_filePath + "\\" + _name, FileMode.Create);
                    else
                        return new FileStream(_filePath + "\\" + _name, FileMode.Open, FileAccess.Read);//File.Open(_filePath + "\\" + _name, FileMode.Append);
                }

            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.RecoveryLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.RecoveryLogger.Error("BackupFile.GetFile()", exp.ToString());
            }
            return null;
        }

        #endregion

        #region Header
        public class Header : ICompactSerializable
        {
            private long segmentCount;
            private long _id;
            private string _delimeter = " ~ ";
            private long _crc = 0;
            private DateTime _creationDate;
            private RecoveryJobType _recoveryType;//job type
            private DateTime _fullBackupDate;//
            private string _version;
            private RecoveryFileState _state;



            internal Header(long id)
            {
                _id = id;
                SegmentMap = new HashVector<long, long>();
                segmentCount = 0;
                _crc = 0;
                _creationDate = DateTime.Now;
                _version = "1";
            }

            #region Properties
            public long Id { get { return _id; } }
            public long NumberOfSlices { get { return segmentCount; } set { segmentCount = value; } }
            public IDictionary<long, long> SegmentMap { get; set; }
            public string DatabaseCluster { get; set; }
            public string Database { get; set; }
            public string Version { get { return _version; } set { _version = value; } }
            public long CRC { get { return _crc; } set { _crc = value; } }
            public RecoveryFileState State
            {
                get { return _state; }
                set { _state = value; }
            }
            public DateTime CreationDate
            {
                get { return _creationDate; }
                set { _creationDate = value; }
            }

            public DateTime FullBackupDate
            {
                get { return _fullBackupDate; }
                set { _fullBackupDate = value; }
            }
            public RecoveryJobType RecoveryType
            {
                get { return _recoveryType; }
                set { _recoveryType = value; }
            }
            #endregion

            public override string ToString()
            {
                string _header = string.Empty;


                _header += "File_ID :" + Id + _delimeter + "Database :" + Database + _delimeter + "Version :" + Version
                    + _delimeter + "Database_Clusrer :" + DatabaseCluster + _delimeter + "Number of Slices :" + NumberOfSlices
                    + _delimeter + "CRC :" + _crc + "State" + _state;

                foreach (long _segID in SegmentMap.Keys)
                {
                    _header += _delimeter + "SegmentID_offset :" + _segID + " :" + SegmentMap[_segID];
                }

                return _header;

            }

            #region ICompactSerializable
            public void Deserialize(Common.Serialization.IO.CompactReader reader)
            {
                _id = reader.ReadInt64();
                NumberOfSlices = reader.ReadInt64();
                Database = reader.ReadString();
                DatabaseCluster = reader.ReadString();
                _crc = reader.ReadInt64();
                Version = reader.ReadString();
                _creationDate = reader.ReadDateTime();
                SegmentMap = Common.Util.SerializationUtility.DeserializeDictionary<long, long>(reader);
                _recoveryType = (RecoveryJobType)reader.ReadInt32();
                _fullBackupDate = reader.ReadDateTime();
                _state = (RecoveryFileState)reader.ReadInt32();
            }

            public void Serialize(Common.Serialization.IO.CompactWriter writer)
            {
                writer.Write(_id);
                writer.Write(NumberOfSlices);
                writer.Write(Database);
                writer.Write(DatabaseCluster);
                writer.Write(_crc);
                writer.Write(_version);
                writer.Write(_creationDate);
                Common.Util.SerializationUtility.SerializeDictionary<long, long>(SegmentMap, writer);
                writer.Write((int)_recoveryType);
                writer.Write(_fullBackupDate);
                writer.Write((int)_state);
            }
            #endregion
        }
        #endregion


    }
}
