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
using System.Text;
using System.Data;
#if JAVA
using Alachisoft.TayzGrid.Runtime.Serialization;
#else
using Alachisoft.NosDB.Common.Serialization;
#endif
using System.Collections;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Enum;
#if JAVA
using Runtime = Alachisoft.TayzGrid.Runtime;
#else
using Runtime = Alachisoft.NosDB.Common;
#endif
namespace Alachisoft.NosDB.Common.DataStructures
{
    public class RecordSet : ICompactSerializable
    {
        /// <summary>
        /// Gets <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of object
        /// </summary>
        /// <param name="obj">Object whose <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> is required</param>
        /// <returns><see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of object</returns>
        public static ColumnDataType ToColumnDataType(object obj)
        {
            if (obj is string)
                return ColumnDataType.String;
            else if (obj is decimal)
                return ColumnDataType.Decimal;
            else if (obj is Int16)
                return ColumnDataType.Int16;
            else if (obj is Int32)
                return ColumnDataType.Int32;
            else if (obj is Int64)
                return ColumnDataType.Int64;
            else if (obj is UInt16)
                return ColumnDataType.UInt16;
            else if (obj is UInt32)
                return ColumnDataType.UInt32;
            else if (obj is UInt64)
                return ColumnDataType.UInt64;
            else if (obj is double)
                return ColumnDataType.Double;
            else if (obj is float)
                return ColumnDataType.Float;
            else if (obj is byte)
                return ColumnDataType.Byte;
            else if (obj is sbyte)
                return ColumnDataType.SByte;
            else if (obj is bool)
                return ColumnDataType.Bool;
            else if (obj is char)
                return ColumnDataType.Char;
            else if (obj is DateTime)
                return ColumnDataType.DateTime;
            else if (obj is AverageResult)
                return ColumnDataType.AverageResult;
            else
                return ColumnDataType.Object;
        }

        /// <summary>
        /// Converts String represenation to appropriate object of specified <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/>
        /// </summary>
        /// <param name="stringValue">String representation of object</param>
        /// <param name="dataType"><see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of object</param>
        /// <returns></returns>
        public static object ToObject(string stringValue, ColumnDataType dataType)
        {
            switch (dataType)
            {
                case ColumnDataType.Bool:
                    return bool.Parse(stringValue);
                    break;
                case ColumnDataType.Byte:
                    return byte.Parse(stringValue);
                    break;
                case ColumnDataType.Char:
                    return char.Parse(stringValue);
                    break;
                case ColumnDataType.DateTime:
                    return DateTime.ParseExact(stringValue, "dd/MM/yyyy/HH/mm/ss/ffff/zzzz", System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case ColumnDataType.Decimal:
                    return decimal.Parse(stringValue);
                case ColumnDataType.Double:
                    return double.Parse(stringValue);
                    break;
                case ColumnDataType.Float:
                    return float.Parse(stringValue);
                    break;
                case ColumnDataType.Int16:
                    return Int16.Parse(stringValue);
                    break;
                case ColumnDataType.Int32:
                    return Int32.Parse(stringValue);
                    break;
                case ColumnDataType.Int64:
                    return Int64.Parse(stringValue);
                    break;
                case ColumnDataType.SByte:
                    return sbyte.Parse(stringValue);
                case ColumnDataType.String:
                    return stringValue;
                    break;
                case ColumnDataType.UInt16:
                    return UInt16.Parse(stringValue);
                    break;
                case ColumnDataType.UInt32:
                    return UInt32.Parse(stringValue);
                    break;
                case ColumnDataType.UInt64:
                    return UInt64.Parse(stringValue);
                    break;
                default:
                    throw new InvalidCastException();
            }
        }

        public static string GetString(object obj, ColumnDataType dataType)
        {
            if (dataType == Common.DataStructures.ColumnDataType.DateTime)
                return ((DateTime)obj).ToString("dd/MM/yyyy/HH/mm/ss/ffff/zzzz");
            else
                return obj.ToString();
        }

        private int _rowCount;
        private int _hiddenColumnCount;
        private System.Collections.Generic.Dictionary<int, RecordColumn> _dataIntIndex;
        private System.Collections.Generic.Dictionary<string, RecordColumn> _dataStringIndex;

        /// <summary>
        /// Gets Dictionary containg all column names and <see cref="Alachisoft.NosDB.Common.DataStructures.RecordColumn"/>
        /// </summary>
        public System.Collections.Generic.Dictionary<string, RecordColumn> Data
        {
            get { return _dataStringIndex; }
        }

        /// <summary>
        /// Initializes new <see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/>
        /// </summary>
        public RecordSet()
        {
            _rowCount = 0;
            _hiddenColumnCount = 0;
            _dataIntIndex = new System.Collections.Generic.Dictionary<int, RecordColumn>();
            _dataStringIndex = new System.Collections.Generic.Dictionary<string, RecordColumn>();
        }

        /// <summary>
        /// Adds specified value in speicified cell
        /// </summary>
        /// <param name="value">value to be added</param>
        /// <param name="rowID">Zero base row index in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/></param>
        /// <param name="columnID">Zero base column index in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/></param></param>
        public void Add(object value, int rowID, int columnID)
        {
            if (_rowCount <= rowID)
                throw new ArgumentException("Invalid rowID. No of rows in RecordSet are less than specified rowID.");
            if (this.ColumnCount<=columnID)
                throw new ArgumentException("Invalid columnID. No of columns in RecordSet are less than specified columnID.");
            _dataIntIndex[columnID].Add(value, rowID);
        }

        /// <summary>
        /// Adds specified value in speicified cell
        /// </summary>
        /// <param name="value">value to be added</param>
        /// <param name="rowID">Zero base row index in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/></param>
        /// <param name="columnName">Column name in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/></param></param>
        public void Add(object value, int rowID, string columnName)
        {
            if (_rowCount <= rowID)
                throw new ArgumentException("Invalid rowID. No of rows in RecordSet are less than specified rowID.");
            if (!_dataStringIndex.ContainsKey(columnName))
                throw new ArgumentException("Invalid columnName. Specified column does not exist in RecordSet.");
            _dataStringIndex[columnName].Add(value, rowID);
        }

        /// <summary>
        /// Adds a new row in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/>
        /// </summary>
        public void AddRow()
        {
            _rowCount++; 
        }

        /// <summary>
        /// Adds a new column in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/>
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <param name="isHidden">IsHidden property of column</param>
        /// <returns>fasle if column with same name already exists, true otherwise</returns>
        public bool AddColumn(string columnName, bool isHidden)
        {
            return this.AddColumn(columnName, isHidden, ColumnType.AttributeColumn, AggregateFunctionType.NOTAPPLICABLE);
        }

        /// <summary>
        /// Adds a new column in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/>
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <param name="isHidden">IsHidden property of column</param>
        /// <param name="aggregateFunctionType"></param>
        /// <returns>fasle if column with same name already exists, true otherwise</returns>
        public bool AddColumn(string columnName, bool isHidden, AggregateFunctionType aggregateFunctionType)
        {
            return this.AddColumn(columnName, isHidden, ColumnType.AttributeColumn, aggregateFunctionType);
        }

        /// <summary>
        /// Adds a new column in <see cref="Alachisoft.NosDB.Common.DataStructures.Recordset"/>
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <param name="isHidden">IsHidden property of column</param>
        /// <param name="columnType"></param>
        /// <param name="aggregateFunctionType"></param>
        /// <returns>fasle if column with same name already exists, true otherwise</returns>
        public bool AddColumn(string columnName, bool isHidden, ColumnType columnType, AggregateFunctionType aggregateFunctionType)
        {
            if (_dataStringIndex.ContainsKey(columnName))
                return false;
            RecordColumn column = new RecordColumn(columnName, isHidden);
            column.Type = columnType;
            column.AggregateFunctionType = aggregateFunctionType;
            _dataIntIndex[this.ColumnCount] = column;
            _dataStringIndex[columnName] = column;
            if (isHidden)
                _hiddenColumnCount++;
            return true;
        }

        /// <summary>
        /// Gets object at specified cell
        /// </summary>
        /// <param name="rowID">Zero based row ID</param>
        /// <param name="columnID">Zero based column ID</param>
        /// <returns>Object at specified row and column</returns>
        public object GetObject(int rowID, int columnID)
        {
            if (_rowCount <= rowID)
                throw new ArgumentException("Invalid rowID. No of rows in RecordSet are less than specified rowID.");
            if (this.ColumnCount <= columnID)
                throw new ArgumentException("Invalid columnID. No of columns in RecordSet are less than specified columnID.");
            return _dataIntIndex[columnID].Get(rowID);
        }

        /// <summary>
        /// Gets object at specified cell
        /// </summary>
        /// <param name="rowID">Zero based row ID</param>
        /// <param name="columnName">Name of column</param>
        /// <returns>Object at specified row and column</returns>
        public object GetObject(int rowID, string columnName)
        {
            if (_rowCount <= rowID)
                throw new ArgumentException("Invalid rowID. No of rows in RecordSet are less than specified rowID.");
            if (!_dataStringIndex.ContainsKey(columnName))
                throw new ArgumentException("Invalid columnName. Specified column does not exist in RecordSet.");
            return _dataStringIndex[columnName].Get(rowID);
        }

        /// <summary>
        /// Gets column name of specified column ID
        /// </summary>
        /// <param name="columnID">Zero based ID of column</param>
        /// <returns>Name of column</returns>
        public string GetColumnName(int columnID)
        {
            if (this.ColumnCount <= columnID)
                throw new ArgumentException("Invalid index. Specified index does not exist in RecordSet.");
            return _dataIntIndex[columnID].Name;
        }

        public int GetColumnIndex(string columnName)
        {
            if (!_dataStringIndex.ContainsKey(columnName))
                throw new ArgumentException("Invalid column name. Specified column does not exist in RecordSet.");
            for (int i = 0; i < _dataIntIndex.Count; i++)
            {
                if (_dataIntIndex[i].Name == columnName)
                    return i;
            }
            throw new ArgumentException("Invalid column name. Specified column does not exist in RecordSet.");
        }

        /// <summary>
        /// Sets IsHidden property of specified column name true
        /// </summary>
        /// <param name="columnName">Name of column</param>
        public void HideColumn(string columnName)
        {
            if (_dataStringIndex.ContainsKey(columnName))
            {
                if (_dataStringIndex[columnName].IsHidden == true)
                    return;
                else
                {
                    _dataStringIndex[columnName].IsHidden = true;
                    _hiddenColumnCount++;
                }
            }
        }

        /// <summary>
        /// Sets IsHidden property of specified column false
        /// </summary>
        /// <param name="columnName">Name of column</param>
        public void UnHideColumn(string columnName)
        {
            if (_dataStringIndex.ContainsKey(columnName))
            {
                if (_dataStringIndex[columnName].IsHidden == false)
                    return;
                else
                {
                    _dataStringIndex[columnName].IsHidden = false;
                    _hiddenColumnCount--;
                }
            }
            else
                throw new ArgumentException("Invalid columnName. Specified column does not exist in RecordSet.");
        }

        public bool GetIsHiddenColumn(string columnName)
        {
            if (_dataStringIndex.ContainsKey(columnName))
                return _dataStringIndex[columnName].IsHidden;
            else
                throw new ArgumentException("Invalid columnName. Specified column does not exist in RecordSet.");
        }

        public bool GetIsHiddenColumn(int columnID)
        {
            if (this.ColumnCount>columnID)
                return _dataIntIndex[columnID].IsHidden;
            else
                throw new ArgumentException("Invalid columnID. Specified column does not exist in RecordSet.");
        }

        /// <summary>
        /// Sets <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of specified column
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <param name="dataType"><see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> to set</param>
        public void SetColumnDataType(string columnName, ColumnDataType dataType)
        {
            if (_dataStringIndex.ContainsKey(columnName))
                _dataStringIndex[columnName].DataType = dataType;
            else
                throw new ArgumentException("Invalid columnName. Specified column does not exist in RecordSet."); 
        }

        /// <summary>
        /// Sets <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of specified column
        /// </summary>
        /// <param name="index">Zero based index of column</param>
        /// <param name="dataType"><see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> to set</param>
        public void SetColumnDataType(int index, ColumnDataType dataType)
        {
            if (_dataIntIndex.ContainsKey(index))
                _dataIntIndex[index].DataType = dataType;
            else
                throw new ArgumentException("Invalid column index. Specified column does not exist in RecordSet.");
        }

        /// <summary>
        /// Gets <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of specified column
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <returns><see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of column</returns>
        public ColumnDataType GetColumnDataType(string columnName)
        {
            if (_dataStringIndex.ContainsKey(columnName))
                return _dataStringIndex[columnName].DataType;
            else
                throw new ArgumentException("Invalid columnName. Specified column does not exist in RecordSet."); 
        }

        /// <summary>
        /// Gets <see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of specified column
        /// </summary>
        /// <param name="index">Zero based index of column</param>
        /// <returns><see cref="Alachisoft.NosDB.Common.DataStructures.ColumnDataType"/> of column</returns>
        public ColumnDataType GetColumnDataType(int index)
        {
            if (_dataIntIndex.ContainsKey(index))
                return _dataIntIndex[index].DataType;
            else
                throw new ArgumentException("Invalid column index. Specified column does not exist in RecordSet.");
        }

        /// <summary>
        /// Sets <see cref="Alachisoft.NosDB.Common.DataStructures.AggregateFunctionType"/> of specifeid column
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <param name="type"><see cref="Alachisoft.NosDB.Common.DataStructures.AggregateFunctionType"/> to set</param>
        public void SetAggregateFunctionType(string columnName, AggregateFunctionType type)
        {
            _dataStringIndex[columnName].AggregateFunctionType = type;
        }

        /// <summary>
        /// Gets <see cref="Alachisoft.NosDB.Common.DataStructures.AggregateFunctionType"/> of specified column
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <returns><see cref="Alachisoft.NosDB.Common.DataStructures.AggregateFunctionType"/> of column</returns>
        public AggregateFunctionType GetAggregateFunctionType(string columnName)
        {
            return _dataStringIndex[columnName].AggregateFunctionType;
        }

        /// <summary>
        /// Removes last column from <see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/>
        /// </summary>
        public void RemoveLastColumn()
        {
            _dataStringIndex.Remove(this.GetColumnName(this.ColumnCount-1));
            if (_dataIntIndex[_dataIntIndex.Count - 1].IsHidden)
                _hiddenColumnCount--;
            _dataIntIndex.Remove(_dataIntIndex.Count - 1);
        }

        /// <summary>
        /// Gets number of rows in <see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/>
        /// </summary>
        public int RowCount
        {
            get { return _rowCount; }
        }

        /// <summary>
        /// Gets number of columns in <see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/>
        /// </summary>
        public int ColumnCount
        {
            get { return _dataStringIndex.Count; }//_columnCount; }
        }

        /// <summary>
        /// Gets number of hidden columns in <see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/>
        /// </summary>
        public int HiddenColumnCount
        {
            get { return _hiddenColumnCount; }
        }

        /// <summary>
        /// Merges <see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/>
        /// </summary>
        /// <param name="recordSet"><see cref="Alachisoft.NosDB.Common.DataStructures.RecordSet"/> to merge</param>
        public void Union(RecordSet recordSet)
        {
            if (this.ColumnCount != recordSet.ColumnCount)
                throw new InvalidOperationException("Cannot compute union of two RecordSet with different number of columns.");

            int thisRowCount = this._rowCount;
            for (int i = 0; i < recordSet.RowCount; i++)
            {
                bool recordMatch = false;

                for (int l = 0; l < thisRowCount; l++)
                {
                    bool rowMatch = true;
                    System.Collections.Generic.List<string> aggregateColumns = new System.Collections.Generic.List<string>();

                    foreach (System.Collections.Generic.KeyValuePair<string, RecordColumn> kvp in _dataStringIndex)
                    {

                        if (kvp.Value.Type == ColumnType.AggregateResultColumn)
                        {
                            aggregateColumns.Add(kvp.Key);
                            continue;
                        }

                        if (recordSet.GetObject(i, kvp.Key).Equals(this.GetObject(l, kvp.Key)))
                            continue;

                        rowMatch = false;
                        break;
                    }

                    if (rowMatch)
                    {
                        //Rows matched, merging aggregate result columns
                        foreach (string column in aggregateColumns)
                        {
                            switch (this.GetAggregateFunctionType(column))
                            {
                                case AggregateFunctionType.SUM:
                                    decimal a;
                                    decimal b;

                                    object thisVal = this.GetObject(i,column);
                                    object otherVal = recordSet.GetObject(i, column);

                                    Nullable<decimal> sum = null;

                                    if (thisVal == null && otherVal != null)
                                    {
                                        sum = (decimal)otherVal;
                                    }
                                    else if (thisVal != null && otherVal == null)
                                    {
                                        sum = (decimal)thisVal;
                                    }
                                    else if (thisVal != null && otherVal != null)
                                    {
                                        a = (decimal)thisVal;
                                        b = (decimal)otherVal;
                                        sum = a + b;
                                    }

                                    if (sum != null)
                                    {
                                        this.Add(sum, i, column);
//                                        this.AggregateFunctionResult = sum;
                                    }
                                    else
                                    {
                                        this.Add(null, i, column);
//                                        this.AggregateFunctionResult = null;
                                    }
                                    break;

                                case AggregateFunctionType.COUNT:
                                    a = (decimal)this.GetObject(i, column);
                                    b = (decimal)recordSet.GetObject(i, column);
                                    decimal count = a + b;

                                    this.Add(count, i, column);
                                    //this.AggregateFunctionResult = count;
                                    break;

                                case AggregateFunctionType.MIN:
                                    IComparable thisValue = (IComparable)this.GetObject(i, column);
                                    IComparable otherValue = (IComparable)recordSet.GetObject(i, column);
                                    IComparable min = thisValue;

                                    if (thisValue == null && otherValue != null)
                                    {
                                        min = otherValue;
                                    }
                                    else if (thisValue != null && otherValue == null)
                                    {
                                        min = thisValue;
                                    }
                                    else if (thisValue == null && otherValue == null)
                                    {
                                        min = null;
                                    }
                                    else if (otherValue.CompareTo(thisValue) < 0)
                                    {
                                        min = otherValue;
                                    }

                                    //this.AggregateFunctionResult = min;
                                    this.Add(min, i, column);
                                    break;

                                case AggregateFunctionType.MAX:
                                    thisValue = (IComparable)this.GetObject(i, column);
                                    otherValue = (IComparable)recordSet.GetObject(i, column);
                                    IComparable max = thisValue;

                                    if (thisValue == null && otherValue != null)
                                    {
                                        max = otherValue;
                                    }
                                    else if (thisValue != null && otherValue == null)
                                    {
                                        max = thisValue;
                                    }
                                    else if (thisValue == null && otherValue == null)
                                    {
                                        max = null;
                                    }
                                    else if (otherValue.CompareTo(thisValue) > 0)
                                    {
                                        max = otherValue;
                                    }

                                    //this.AggregateFunctionResult = max;
                                    this.Add(max, i, column);
                                    break;

                                case AggregateFunctionType.AVG:
                                    thisVal = this.GetObject(i, column);
                                    otherVal = recordSet.GetObject(i, column);

                                    AverageResult avg = null;
                                    if (thisVal == null && otherVal != null)
                                    {
                                        avg = (AverageResult)otherVal;
                                    }
                                    else if (thisVal != null && otherVal == null)
                                    {
                                        avg = (AverageResult)thisVal;
                                    }
                                    else if (thisVal != null && otherVal != null)
                                    {
                                        AverageResult thisResult = (AverageResult)thisVal;
                                        AverageResult otherResult = (AverageResult)otherVal;

                                        avg = new AverageResult();
                                        avg.Sum = thisResult.Sum + otherResult.Sum;
                                        avg.Count = thisResult.Count + otherResult.Count;
                                    }

                                    if (avg != null)
                                    {
                                        //this.AggregateFunctionResult = avg;
                                        this.Add(avg, i, column);
                                    }
                                    else
                                    {
                                        //this.AggregateFunctionResult = null;
                                        this.Add(null, i, column);
                                    }
                                    break;
                            }
                        }
                        recordMatch = true;
                        break;
                    }
                }

                if (recordMatch == true)
                    continue;
                this.AddRow();
                //Append data to current record set
                for (int j = 0; j < this.ColumnCount; j++)
                    this.Add(recordSet.GetObject(i, j), _rowCount - 1, j);
            }
        }

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _rowCount = reader.ReadInt32();
            //this.ColumnCount = reader.ReadInt32();
            for (int i = 0; i < this.ColumnCount; i++)
            {
                string key = reader.ReadObject() as string;
                RecordColumn col = reader.ReadObject() as RecordColumn;
                _dataStringIndex.Add(key, col);
                _dataIntIndex.Add(i, col);
            }
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(_rowCount);
           // writer.Write(_columnCount);
            foreach (System.Collections.Generic.KeyValuePair<string, RecordColumn> kvp in _dataStringIndex)
            {
                writer.WriteObject(kvp.Key);
                writer.WriteObject(kvp.Value);
            }
        }
    }
}
