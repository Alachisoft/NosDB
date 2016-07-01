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
using Alachisoft.NosDB.Common.Serialization;
using System;

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public class ArrayElement : IComparable, ICompactSerializable
    {
        private IComparable _element;
        private int _index;

        public ArrayElement(IComparable element, int index)
        {
            _element = element;
            _index = index;
        }

        public IComparable Element { get { return _element; } set { _element = value; } }

        public int Index { get { return _index; } set { _index = value; } }

        public int CompareTo(object obj)
        {
            var arrayElement = obj as ArrayElement;
            if (arrayElement == null)
                throw new ArgumentException();
            int result;
            if (_element == null && arrayElement._element != null)
                return int.MinValue;
            if (_element != null && arrayElement._element == null)
                return int.MaxValue;
            if ((result = _element.CompareTo(arrayElement._element)) != 0)
                return result;
            return _index.CompareTo(arrayElement._index);
        }

        public override string ToString()
        {
            return _index+":"+_element.ToString();
        }

        public override bool Equals(object obj)
        {
            var arrayElement = obj as ArrayElement;
            if (arrayElement == null)
                return false;
            if (!_element.Equals(arrayElement._element))
                return false;
            return _index.Equals(arrayElement._index);
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _element = reader.ReadObject() as IComparable;
            _index = reader.ReadInt32();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_element);
            writer.Write(_index);
        }
    }
}
