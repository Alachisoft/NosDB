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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class RecordColumn : ICompactSerializable
    {
        private string _name;
        private bool _isHidden;
        private ColumnType _columnType = ColumnType.AttributeColumn;
        private ColumnDataType _dataType = ColumnDataType.Object;
        private AggregateFunctionType _aggregateFunctionType = AggregateFunctionType.NOTAPPLICABLE;

        //rowID-Object
        private System.Collections.Generic.Dictionary<int, object> _data;


        public System.Collections.Generic.Dictionary<int, object> Data
        {
            get { return _data; }
        }

        public RecordColumn(string name, bool isHidden)
        {
            _name = name;
            _isHidden = isHidden;
            _data = new System.Collections.Generic.Dictionary<int, object>();
        }
        //data-type

        public string Name
        {
            get { return _name; }
        }

        public bool IsHidden
        {
            get { return _isHidden; }
            set { _isHidden = value; }
        }

        public ColumnType Type
        { get { return _columnType; }
            set { _columnType = value; }
        }

        public ColumnDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public AggregateFunctionType AggregateFunctionType
        {
            get { return _aggregateFunctionType; }
            set { _aggregateFunctionType = value; }
        }

        public void Add(object value, int rowID)
        {
            _data[rowID] = value;
        }

        public object Get(int rowID)
        {
            return _data[rowID];
        }

        #region ICompactSerializable member functions
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _name = reader.ReadObject() as string;
            _isHidden = reader.ReadBoolean();
            _columnType = (ColumnType)reader.ReadInt32();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _data.Add(reader.ReadInt32(), reader.ReadObject());
            }
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_name);
            writer.Write(_isHidden);
            writer.Write(Convert.ToInt32(_columnType));
            writer.Write(_data.Count);
            foreach (System.Collections.Generic.KeyValuePair<int, object> de in _data)
            {
                writer.Write(de.Key);
                writer.WriteObject(de.Value);
            }
        }
        #endregion

    }
}