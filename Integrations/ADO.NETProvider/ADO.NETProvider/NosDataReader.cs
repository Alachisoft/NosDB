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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Server.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.ADO.NETProvider
{
    public class NosDataReader : DbDataReader, IDataReader
    {
        // The DataReader should always be open when returned to the user.
        private bool m_fOpen = true;

        internal int _rowsAffected = 0;
        // Keep track of the results and position
        // within the resultset (starts prior to first record).
        private static int m_STARTPOS = -1;
        private int m_nPos = m_STARTPOS;

        ICollectionReader _dbReader = null;

        private ArrayList _attributeCols = new ArrayList();
        internal ArrayList AttributesColumns
        {
            set { _attributeCols = value; }
        }
        public NosDataReader(ICollectionReader reader) 
        {
            this._dbReader = reader;
        }

        /// <summary>
        /// Closes the reader
        /// </summary>
        public override void Close()
        {
            _dbReader.Dispose();
            m_fOpen = false;
        }

        public override int Depth
        {
            get { return 0; }
        }

        public override System.Data.DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override bool IsClosed
        {
            get
            {
                if (_dbReader != null)
                    return !_dbReader.HasRows;
                return true;
            }
        }

        public override bool NextResult()
        {
            return false;
        }

        /// <summary>
        /// Advances the cursor to the next record row.
        /// </summary>
        /// <returns>True if there are more records else false</returns>
        public override bool Read()
        {
            if (_dbReader != null && _dbReader.ReadNext())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the number of records affected by the query
        /// </summary>
        public override int RecordsAffected
        {
            get { return this._rowsAffected; }
        }

        /// <summary>
        /// Disposes and frees the reader resources
        /// </summary>
        public void Dispose()
        {
            if (_dbReader != null)
            {
                _dbReader.Dispose();
                _dbReader = null;
            }
        }

        /// <summary>
        /// Gets the number of Fields in a record
        /// </summary>
        public override int FieldCount
        {
            get { return _attributeCols.Count; } // TODO do this
        }

        /// <summary>
        /// Gets the value of the specified column index as a Boolean.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Boolean at the specified index.</returns>
        public override bool GetBoolean(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetBoolean(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Char.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Char at the specified index.</returns>
        public override char GetChar(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetString(_attributeCols[index].ToString())[0];
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }
        
        /// <summary>
        /// Gets the value of the specified column index as a DateTime.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>DateTime at the specified index.</returns>
        public override DateTime GetDateTime(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return Convert.ToDateTime(_dbReader.Get<object>(_attributeCols[index].ToString()));
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Decimal.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Decimal at the specified index.</returns>
        public override decimal GetDecimal(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetDecimal(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Double.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Double at the specified index.</returns>
        public override double GetDouble(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetDouble(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the type of value at specified column index.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Type of value at the specified index.</returns>
        public override Type GetFieldType(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                ExtendedJSONDataTypes type = _dbReader.GetAttributeDataType(_attributeCols[index].ToString());
                switch (type)
                {
                    case ExtendedJSONDataTypes.Boolean:
                        return typeof(bool);
                    case ExtendedJSONDataTypes.DateTime:
                        return typeof(DateTime);
                    case ExtendedJSONDataTypes.Number:
                        return typeof(double);
                    case ExtendedJSONDataTypes.Null:
                        return typeof(string); // nullable string...
                    case ExtendedJSONDataTypes.Array:
                        return typeof(Array);
                    case ExtendedJSONDataTypes.Object:
                        return typeof(object);
                    case ExtendedJSONDataTypes.String:
                        return typeof(string);
                }
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Float.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Float at the specified index.</returns>
        public override float GetFloat(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return (float)_dbReader.GetDouble(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Guid.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Guid at the specified index.</returns>
        public override Guid GetGuid(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return (Guid)_dbReader.Get<object>(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Short.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Short at the specified index.</returns>
        public override short GetInt16(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetInt16(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Integer.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Integer at the specified index.</returns>
        public override int GetInt32(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetInt32(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index as a Long.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Long at the specified index.</returns>
        public override long GetInt64(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetInt64(_attributeCols[index].ToString());
            }
            return 0;
        }

        /// <summary>
        /// Gets the name of the column at specified column index.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Name of the column at the specified index.</returns>
        public override string GetName(int index)
        {
            if (index >= 0 && _attributeCols.Count > index)
            {
                return _attributeCols[index].ToString();
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the index of column against column name
        /// </summary>
        /// <param name="name">Column name</param>
        /// <returns>Index of specified column.</returns>
        public override int GetOrdinal(string name)
        {
            if (_attributeCols.Contains(name))
                return _attributeCols.IndexOf(name);
            else
                return -1;

            // Throw an exception if the ordinal cannot be found.
            //throw new IndexOutOfRangeException("Could not find specified column in results");
        }

        /// <summary>
        /// Gets the value of the specified column index as a String .
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>String at the specified index.</returns>
        public override string GetString(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.GetString(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        /// <summary>
        /// Gets the value of the specified column index.
        /// </summary>
        /// <param name="index">Index of the column (0 based index)</param>
        /// <returns>Value at the specified index.</returns>
        public override object GetValue(int index)
        {
            if (index >= 0 && _attributeCols.Count > index && _dbReader != null)
            {
                return _dbReader.Get<object>(_attributeCols[index].ToString());
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        public override int GetValues(object[] values)
        {
            int i = 0;
            foreach (string col in _attributeCols)
            {
                values[i] = _dbReader[col];
                i++;
            }

            return i;
        }

        public override bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Indexer on the basis of Column name.
        /// </summary>
        /// <param name="name">Column name</param>
        /// <returns>Value at specified column name.</returns>
        public override object this[string name]
        {
            get
            {
                return _dbReader != null ? _dbReader[name] : null;
            }
        }

        /// <summary>
        ///  Indexer on the basis of Column index.
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns>Value at specified column index.</returns>
        public override object this[int index]
        {
            get
            {
                if (_attributeCols.Count > index && _dbReader != null)
                {
                    return _dbReader.Get<object>(_attributeCols[index].ToString());
                }
                throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
            }
        }


        public override IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public override bool HasRows
        {
            get 
            {
                if(_dbReader != null)
                    return _dbReader.HasRows;
                return false;
            }
        }


        public override byte GetByte(int ordinal)
        {
            if (ordinal >= 0 && _attributeCols.Count > ordinal && _dbReader != null)
            {
                return _dbReader.GetAttributeDataType(_attributeCols[ordinal].ToString()) == ExtendedJSONDataTypes.Number
                    ? (byte)_dbReader.Get<object>(_attributeCols[ordinal].ToString()) : (byte)0;
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            if (ordinal >= 0 && _attributeCols.Count > ordinal && _dbReader != null)
            {
                return _dbReader.GetAttributeDataType(_attributeCols[ordinal].ToString()).ToString();
            }
            throw new Exception("Index is not valid. Use [name] if * is used in the Query.");
        }
    }
}
