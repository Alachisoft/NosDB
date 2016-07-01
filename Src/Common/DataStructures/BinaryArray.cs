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

namespace Alachisoft.NosDB.Common.DataStructures
{
    /// <summary>
    /// An array list structure to store items on a byte array directly. Faster than normal List
    /// The serializer needs to assure that the length of the byte array it provides is equal to 
    /// the base size provided at initialization time
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryArray<T> : ICollection<T>
    {

        private byte[] _data;
        private int count = 0;
        private int _baseSize = 1;
        private IDataSerializer<T> _serializer;

        public BinaryArray(int baseSize, IDataSerializer<T> serializer)
        {
            _data = new byte[4096];
            _baseSize = baseSize;
            _serializer = serializer;
        }

        public BinaryArray(int newSize, int baseSize, IDataSerializer<T> serializer)
        {
            _data = new byte[newSize];
            _baseSize = baseSize;
            _serializer = serializer;
        }

        private void Expand()
        {
            Array.Resize(ref _data, _data.Length * 2);
        }

        public void SerializeTo(Stream stream)
        {
            stream.Write(Int32Serializer.ToBytes(count), 0, 4);
            stream.Write(Int32Serializer.ToBytes(_data.Length), 0, 4);
            stream.Write(_data, 0, _data.Length);
        }

        public void DeserializeFrom(Stream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            count = Int32Serializer.FromBytes(bytes);
            stream.Read(bytes, 0, 4);
            int length = Int32Serializer.FromBytes(bytes);
            _data = new byte[length];
            stream.Read(_data, 0, length);
        }

        public void Add(T item)
        {
            byte[] data = _serializer.Serialize(item);
            int position = count * _baseSize;
            if (position >= _data.Length)
                Expand();
            Array.Copy(_data, position, data, 0, data.Length);
            count++;
        }

        public void Clear()
        {
            _data = new byte[4096];
            count = 0;
        }

        public int IndexOf(T item)
        {
            int index = -1;
            byte[] data = _serializer.Serialize(item);
            for (int i = 0; i < count; i++)
            {
                int offset = i * _baseSize;
                if (_data[offset].Equals(data[0]))
                {
                    int possibleIndex = i;

                    for (int j = 1; j < _baseSize; j++)
                    {
                        if (!_data[offset + j].Equals(data[j]))
                            continue;
                        index = possibleIndex;
                        break;
                    }
                }
            }
            return index;
        }

        public bool Contains(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
                return true;
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < count; i++)
            {
                byte[] data = new byte[_baseSize];
                Array.Copy(_data, i * _baseSize, data, 0, _baseSize);
                array[arrayIndex + i] = _serializer.Deserialize(data);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                Array.Copy(_data, index, _data, index + _baseSize, _data.Length - (index + _baseSize));
                count--;
                return true;
            }
            return false;
        }

        public T this[int index]
        {
            get
            {
                int targetIndex = index * _baseSize;
                var item = new byte[_baseSize];
                Array.Copy(_data, targetIndex, item, 0, item.Length);
                return _serializer.Deserialize(item);
            }
            set
            {
                byte[] item = _serializer.Serialize(value);
                int targetIndex = index * _baseSize;
                Array.Copy(item, 0, _data, targetIndex, item.Length);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BinaryArrayEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new BinaryArrayEnumerator(this);
        }

        class BinaryArrayEnumerator : IEnumerator<T>
        {
            private BinaryArray<T> _array;
            private T current;
            private bool isDisposed = false;
            private int position = -1;


            internal BinaryArrayEnumerator(BinaryArray<T> array)
            {
                _array = array;
            }

            public T Current
            {
                get { return current; }
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }

            public bool MoveNext()
            {
                if (isDisposed)
                    throw new ObjectDisposedException("The enumerator has been disposed");
                if (position < 0)
                {
                    position = 0;
                }
                current = _array[position];
                position++;
                if (position >= _array.count)
                    return false;
                return true;
            }

            public void Reset()
            {
                position = 0;
            }

            ~BinaryArrayEnumerator()
            {
                Dispose();
            }
        }
    }
}
