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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Server.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using Alachisoft.NosDB.Common.JSON.CustomConverter;
using Alachisoft.NosDB.Common.DataStructures;
using System.IO;
using System.Numerics;

namespace Alachisoft.NosDB.Common
{
    [JsonConverter(typeof(JsonDocumentConverter))]
    public class JSONDocument : IJSONDocument, ISize
    {
        private ISizableDictionary _values = new KeyValueStore(4);
        public const string KeyAttribute = "_key";
        private object _lockObject = new object();
        private long _valuesSize = 0;

        public JSONDocument() { _values = new KeyValueStore(4); }

        public JSONDocument(int count)
        {
            if (count < 20)
                _values = new KeyValueStore(count);
            else
                _values = new Alachisoft.NosDB.Common.DataStructures.Clustered.HashVector(count);
        }

        public object this[string attribute]
        {
            get
            {
                Array array;
                if (attribute == null)
                    throw new ArgumentNullException("attribute", "Value cannot be null");

                if (!_values.Contains(attribute))
                    throw new ArgumentException(string.Format("Specified attribute {0} does not exist", attribute));

                TryExpendComplexObject(attribute);

                if (TryGetArray(_values[attribute], out array))
                    return array;
                else
                    return _values[attribute];
    
            }
            set
            {
                if (attribute == KeyAttribute)
                    if(value != null && !(value is string))
                        throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute));
                Type argumentType;
                if (JsonDocumentUtil.IsSupportedType(value, out argumentType))
                {
                    if (value is ArrayList)
                        AddAttribute(attribute, ((ArrayList)value).ToArray(), true);
                    else
                        AddAttribute(attribute, value, true);

                    CalculateSize(value);
                }
                else
                    throw new NotSupportedException(string.Format("Type {0} is not supported on JSONDocument", argumentType));
            }
        }

        public string Key
        {
            get 
            {
                if (_values.Contains(KeyAttribute))
                    return (string)_values[KeyAttribute];
                return null;
            }
            set
            {
                AddAttribute(KeyAttribute, value,true);
                CalculateSize(value);
            }
        }

        public ICollection<string> GetAttributes()
        {
            List<string> keys = new List<string>();
            lock (_lockObject)
            {
                foreach(var key in _values.Keys)
                    keys.Add((string)key);
            }
            return keys;

        }

        #region  ------------------------ Add Methods -------------------------- 

        public void Add(string attribute, bool value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("attribute " + attribute + " already exist in document");
            if(attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}",KeyAttribute), "value");

            AddAttribute(attribute, value);
            CalculateSize(value);
        }

        private void SwitchToHashtable()
        {
            if (_values is KeyValueStore)
            {
                lock (_lockObject)
                {
                    IDictionaryEnumerator ide = _values.GetEnumerator();

                    ISizableDictionary dictionary = new Alachisoft.NosDB.Common.DataStructures.Clustered.HashVector(_values.Count);

                    while (ide.MoveNext())
                    {
                        dictionary.Add(ide.Key, ide.Value);
                    }
                    _values = dictionary;
                }
            }
        }

        public void Add(string attribute, short value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);
            CalculateSize(value);
        }

        public void Add(string attribute, int value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);

            CalculateSize(value);
        }

        public void Add(string attribute, long value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);
            CalculateSize(value);
        }

        private void AddAttribute(string attribute, object value,bool treatAsInsert = false)
        {
            lock (_lockObject)
            {
                if (_values.Count >= 20)
                {
                    //switch to normal hashtable 
                    SwitchToHashtable();
                }
                if (treatAsInsert)
                    _values[attribute] = value;
                else
                    _values.Add(attribute, value);
            }
        }

        public void Add(string attribute, float value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);
            CalculateSize(value);
        }

        public void Add(string attribute, double value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);
            CalculateSize(value);
        }

        public void Add(string attribute, string value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");

            AddAttribute(attribute, value);
            CalculateSize(value);
        }

        public void Add(string attribute, DateTime value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);

            CalculateSize(value);
        }

        public void Add(string attribute, IJSONDocument value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            AddAttribute(attribute, value);

            CalculateSize(value);
        }

        public void Add(string attribute, Array value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");
            
            Type argumentType;
            if (JsonDocumentUtil.IsSupportedType(value, out argumentType))
            {
                AddAttribute(attribute, value);
            }
            else
                throw new NotSupportedException(string.Format("Type {0} is not supported on JSONDocument", argumentType));

            CalculateSize(value);
        }
        
        public void Add(string attribute, object value)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");
            if (_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " already exist in document");
            if (attribute == KeyAttribute)
                if(value != null && !(value is string))
                    throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "value");

            Type argumentType;
            if (JsonDocumentUtil.IsSupportedType(value, out argumentType))
            {
                if (value is ArrayList)
                    AddAttribute(attribute, ((ArrayList)value).ToArray(), true);
                else
                    AddAttribute(attribute, value);

            }
            else
                throw new NotSupportedException(string.Format("Type {0} is not supported on JSONDocument", argumentType));

            CalculateSize(value);
        }

        #endregion


        private void CalculateSize(object value)
        {
            if (value == null)
                return;

            if (value.GetType().IsPrimitive)
                _valuesSize += GetPrimitiveTypeSize(Type.GetTypeCode(value.GetType()));

            else if (value is string)
                _valuesSize += ((string)value).Length;

            else if (value is IJSONDocument)
                _valuesSize += ((IJSONDocument)value).Size;

            else if (value is JsonObject)
            {
                _valuesSize += ((JsonObject)value).Size;
            }

            else if (value is Array || value is ArrayList)
            {
                foreach (var arrayValue in (IList)value)
                {
                    CalculateSize(arrayValue);
                }
            }
        }

        public void Remove(string attribute)
        {
            VerifyParameter(attribute);
            RemoveValueSize(_values[attribute]);
            lock (_lockObject)
            {
                _values.Remove(attribute);
            }
            
        }

        private long GetPrimitiveTypeSize(TypeCode typeCode)
        {
            if (typeCode == TypeCode.Byte) return sizeof(byte);
            else if (typeCode == TypeCode.Int16) return sizeof(short);
            else if (typeCode == TypeCode.Int32) return sizeof(int);
            else if (typeCode == TypeCode.Int64) return sizeof(long);
            else if (typeCode == TypeCode.UInt16) return sizeof(ushort);
            else if (typeCode == TypeCode.UInt32) return sizeof(uint);
            else if (typeCode == TypeCode.Single) return sizeof(float);
            else if (typeCode == TypeCode.Double) return sizeof(double);
            else if (typeCode == TypeCode.Decimal) return sizeof(decimal);
            else if (typeCode == TypeCode.Boolean) return sizeof(bool);
            else if (typeCode == TypeCode.DateTime) unsafe { _valuesSize -= sizeof(DateTime); }
            return 0;
        }

        private void RemoveValueSize(object value)
        {
            if (value == null)
                return;

            Type type = value.GetType();

            if(type.IsPrimitive)
                _valuesSize -= this.GetPrimitiveTypeSize(Type.GetTypeCode(type));

            else if (value is string)
                _valuesSize -= ((string)value).Length;

            else if(value is IJSONDocument)
                _valuesSize -= ((IJSONDocument)value).Size;

            else if (value is JsonObject)
            {
                _valuesSize -= ((JsonObject)value).Size;
            }

            else if(value is Array || value is ArrayList)
            {
                foreach (var arrayValue in (IList)value)
                {
                    RemoveValueSize(arrayValue);
                }
            }
        }

        public bool Contains(string attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "Value cannot be null");

            return _values.Contains(attribute);
        }

        #region  ------------------- Get Utilities -------------------- 

        public short GetAsInt16(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.Number)
            {
                return GetShort(_values[attribute]);
            }
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to short");
        }

        public int GetAsInt32(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.Number)
            {
                return GetInt(_values[attribute]);
            }
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to Int32 ");

        }

        public long GetAsInt64(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.Number)
                return GetLong(_values[attribute]);
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to Int64  ");
        }

        public float GetAsFloat(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);
            
            if (attributeDataType == ExtendedJSONDataTypes.Number)
                return GetFloat(_values[attribute]);
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to float  ");
        }

        public double GetAsDouble(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);
            
            if (attributeDataType == ExtendedJSONDataTypes.Number)
            {
                return GetDouble(_values[attribute]);
            }
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to double  ");
        }

        public decimal GetAsDecimal(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.Number)
                return GetDecimal(_values[attribute]);
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to decimal  ");
        }

        public string GetString(string attribute)
        {
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.String)
                return (string)_values[attribute];
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to string  ");
        }

        public bool GetBoolean(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.Boolean)
                return (bool)_values[attribute];
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to bool  ");
        }

        public DateTime GetDateTime(string attribute)
        {
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.DateTime)
                return (DateTime)_values[attribute];
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to DateTime  ");
        }

        public IJSONDocument GetDocument(string attribute)
        {
            VerifyParameter(attribute);
            if (_values[attribute] == null)
                return null;
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (attributeDataType == ExtendedJSONDataTypes.Object)
            {
                if (_values[attribute] is JsonObject)
                    ExpandComplexObject(attribute);
                return (JSONDocument)_values[attribute];
            }
            else
                throw new InvalidCastException("Unable to cast " + attributeDataType + " to JSONDoucment");
        }

        public T[] GetArray<T>(string attribute)
        {
            Array array = null;
            VerifyParameter(attribute);
            ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);

            if (_values[attribute] == null)
                return null;

            if (attributeDataType == ExtendedJSONDataTypes.Array)
            {
                if (_values[attribute] is JsonObject)
                {
                    ExpandComplexObject(attribute);
                }
                if (TryGetArray(_values[attribute], out array))
                {
                    return array.Cast<T>().ToArray();
                }
                return null;
            }
            throw new InvalidCastException("Unable to cast type " + attributeDataType + " to Array of Type " + typeof(T));
        }

        public string GetToString(string attribute)
        {
            VerifyParameter(attribute);
            if (_values[attribute] == null)
                return null;
           
            if (GetAttributeDataType(attribute) == ExtendedJSONDataTypes.Object)
            {
                if (_values[attribute] is JsonObject)
                    return ((JsonObject)_values[attribute]).JsonString;
                return _values[attribute].ToString();
            }
            return _values[attribute].ToString();
        }

        public T Get<T>(string attribute)
        {
            VerifyParameter(attribute);

            if (_values[attribute] == null)
                return default(T);

            ExtendedJSONDataTypes jsonType = GetAttributeDataType(attribute);
         
            switch (jsonType)
            {
                case ExtendedJSONDataTypes.Number:
                    return GetNumber<T>(attribute);
                case ExtendedJSONDataTypes.Array:
                    Array array = null;
                    if (_values[attribute] is JsonObject)
                    {
                        ExpandComplexObject(attribute);
                    }
                    if (typeof(T) == typeof(object))
                        return (T)_values[attribute];

                    TryGetArray(_values[attribute], out array);

                    if (typeof(T) == typeof(ArrayList))
                    {
                        return (T)(object)new ArrayList(array);
                    }
                    if (typeof(T) == typeof(Array) || typeof(T).IsArray)
                    {
                        return (T)(object)array;
                    }
                    throw new InvalidCastException("Unable to cast type " + _values[attribute].GetType().FullName + " to " + typeof(T));

                case ExtendedJSONDataTypes.Object:
                    if (typeof(T) == typeof(JSONDocument) || typeof(T) == typeof(IJSONDocument) || typeof(T) == typeof(object))
                    {
                        if (_values[attribute] is JsonObject)
                        {
                            ExpandComplexObject(attribute);
                        }
                        return (T)_values[attribute];
                    }
                    else //This case will occure only on client side
                        if (_values[attribute] is JsonObject)
                            return JsonConvert.DeserializeObject<T>(((JsonObject)_values[attribute]).JsonString);
                        else
                            return ((JSONDocument)_values[attribute]).Parse<T>();
                default:
                    return (T)_values[attribute];
            }
        }

        public bool TryGet<T>(string attribute, out T value)
        {
            //TODO: ensure type checking  and then handle exceptions
            value = default(T);
            if (!_values.Contains(attribute))
                return false;

            ExtendedJSONDataTypes jsonType = GetAttributeDataType(attribute);
            try
            {
                switch (jsonType)
                {
                    case ExtendedJSONDataTypes.Null:
                        return true;
                    case ExtendedJSONDataTypes.Boolean:
                    case ExtendedJSONDataTypes.String:
                    case ExtendedJSONDataTypes.DateTime:
                        value = (T)_values[attribute];
                        return true;
                    case ExtendedJSONDataTypes.Number:
                        value = GetNumber<T>(attribute);
                        return true;
                    case ExtendedJSONDataTypes.Array:
                    case ExtendedJSONDataTypes.Object:
                        value = Get<T>(attribute);
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception) { }
            return false;
        }

        public bool TryGet(string attribute, out object value)
        {
            return TryGet<object>(attribute, out value);
        }

        public ExtendedJSONDataTypes GetAttributeDataType(string attribute)
        {
            VerifyParameter(attribute);
            return JsonDocumentUtil.GetExtendedJsonType(_values[attribute]);
        }


        private T GetNumber<T>(string attribute)
        {
            if (GetAttributeDataType(attribute) == ExtendedJSONDataTypes.Number)
            {
                TypeCode expectedType = Type.GetTypeCode(typeof(T));

                if (expectedType == TypeCode.Byte) return (T)(object)GetByte(_values[attribute]);
                else if (expectedType == TypeCode.SByte) return (T)(object)GetSByte(_values[attribute]);
                else if (expectedType == TypeCode.Int16) return (T)(object)GetShort(_values[attribute]);
                else if (expectedType == TypeCode.UInt16) return (T)(object)GetUShort(_values[attribute]);
                else if (expectedType == TypeCode.Int32) return (T)(object)GetInt(_values[attribute]);
                else if (expectedType == TypeCode.UInt32) return (T)(object)GetUInt(_values[attribute]);
                else if (expectedType == TypeCode.Int64) return (T)(object)GetLong(_values[attribute]);
                else if (expectedType == TypeCode.UInt64) return (T)(object)GetULong(_values[attribute]);
                else if (expectedType == TypeCode.Single) return (T)(object)GetFloat(_values[attribute]);
                else if (expectedType == TypeCode.Double) return (T)(object)GetDouble(_values[attribute]);
                else if (expectedType == TypeCode.Decimal) return (T)(object)GetDecimal(_values[attribute]);
                else
                    return (T)_values[attribute];
            }
            throw new InvalidCastException("Cannot cast type of " + _values[attribute].GetType() + " to " + typeof(T));
        }

        private byte GetByte(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (byte)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (byte)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (byte)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (byte)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (byte)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (byte)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (byte)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (byte)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (byte)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (byte)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private sbyte GetSByte(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (sbyte)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (sbyte)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (sbyte)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (sbyte)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (sbyte)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (sbyte)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (sbyte)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (sbyte)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (sbyte)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (sbyte)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private short GetShort(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (short)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (short)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (short)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (short)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (short)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (short)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (short)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (short)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (short)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (short)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private ushort GetUShort(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (ushort)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (ushort)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (ushort)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (ushort)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (ushort)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (ushort)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (ushort)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (ushort)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (ushort)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (ushort)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private int GetInt(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (int)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (int)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (int)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (int)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (int)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (int)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (int)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (int)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (int)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (int)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private uint GetUInt(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (uint)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (uint)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (uint)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (uint)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (uint)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (uint)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (uint)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (uint)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (uint)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (uint)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private long GetLong(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (long)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (long)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (long)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (long)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (long)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (long)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (long)(ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (long)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (long)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (long)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private ulong GetULong(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (ulong)(byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (ulong)(sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (ulong)(short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (ulong)(ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (ulong)(int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (ulong)(uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (ulong)(long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (ulong)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (ulong)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (ulong)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private float GetFloat(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (float)value;
            else if (valueTypeCode == TypeCode.Double) return (float)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (float)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private double GetDouble(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (double)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (double)(decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        private decimal GetDecimal(object value)
        {
            TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());

            if (valueTypeCode == TypeCode.Byte) return (byte)value;
            else if (valueTypeCode == TypeCode.SByte) return (sbyte)value;
            else if (valueTypeCode == TypeCode.Int16) return (short)value;
            else if (valueTypeCode == TypeCode.UInt16) return (ushort)value;
            else if (valueTypeCode == TypeCode.Int32) return (int)value;
            else if (valueTypeCode == TypeCode.UInt32) return (uint)value;
            else if (valueTypeCode == TypeCode.Int64) return (long)value;
            else if (valueTypeCode == TypeCode.UInt64) return (ulong)value;
            else if (valueTypeCode == TypeCode.Single) return (decimal)(float)value;
            else if (valueTypeCode == TypeCode.Double) return (decimal)(double)value;
            else if (valueTypeCode == TypeCode.Decimal)
                return (decimal)value;
            else
                throw new InvalidCastException(string.Format("Unable to cast type {0} to {1}", value.GetType().FullName, value.GetType()));
        }

        #endregion

        public override string ToString()
        {
            lock (_lockObject)
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public T Parse<T>()
        {
            if(typeof(T) == typeof(JSONDocument) || typeof(T) == typeof(IJSONDocument))
            return (T)(object)this;

            return JsonConvert.DeserializeObject<T>(ToString());
        }

        public static IJSONDocument Parse(string jsonString)
        {
            if (jsonString == null)
                throw new ArgumentNullException("jsonString", "Value cannot be null");

            JSONDocument jDoc = new JSONDocument();

            Dictionary<string, object> jsonCollection =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

             //Validate DocumentKey
            if (jsonCollection.ContainsKey(KeyAttribute))
                if (jsonCollection[KeyAttribute] != null && !(jsonCollection[KeyAttribute] is string))
                    throw new ArgumentException(string.Format("Invalid type for attribute {0}", KeyAttribute), "jsonString");

            foreach (KeyValuePair<string, object> keyValuePair in jsonCollection)
            {
                if(keyValuePair.Value is JArray)
                {
                    jDoc.Add(keyValuePair.Key, new JsonObject(((JToken)keyValuePair.Value).ToString(Formatting.None), JsonObject.ObjectType.Array));
                }
                else if (keyValuePair.Value is JObject)
                {
                    jDoc.Add(keyValuePair.Key, new JsonObject(((JToken)keyValuePair.Value).ToString(Formatting.None), JsonObject.ObjectType.Object));
                }
                else
                {
                    jDoc.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return jDoc;
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public void Clear()
        {
            lock(_lockObject)
                _values.Clear();
        }

        public void GenerateDocumentKey()
        {
            Key = Guid.NewGuid().ToString();
        }

        //public IJsonValue[] GetArray(string attribute)
        //{
        //    VerifyParameter(attribute);
        //    ExtendedJSONDataTypes attributeDataType = GetAttributeDataType(attribute);
        //    if (attributeDataType != ExtendedJSONDataTypes.Array) return null;

        //    if (_values[attribute] is JsonObject)
        //        ExpandComplexObject(attribute);
        //    return ToIJsonList((ArrayList)_values[attribute]).ToArray();
        //}

        //private List<IJsonValue> ToIJsonList(IEnumerable arrayList)
        //{
        //    List<IJsonValue> jsonList = new List<IJsonValue>();
        //    foreach (var value in arrayList)
        //    {
        //        if (value == null)
        //            jsonList.Add(new NullValue());
        //        else if (IsNumber(value))
        //            jsonList.Add(new NumberJsonValue(value));
        //        else if (value is bool)
        //            jsonList.Add(new BooleanJsonValue((bool)value));
        //        else if (value is string)
        //            jsonList.Add(new StringJsonValue(value as string));
        //        else if (value is DateTime)
        //            jsonList.Add(new DateTimeJsonValue((DateTime)value));
        //        else if (value is Array || value is ArrayList)
        //            jsonList.Add(new ArrayJsonValue(ToIJsonList((IEnumerable)value).ToArray()));
        //        else if (value is JsonObject)
        //            if (((JsonObject)value).JsonComplexType == JsonObject.ObjectType.Array)
        //                jsonList.Add(new ArrayJsonValue(ToIJsonList((IEnumerable)value).ToArray()));
        //            else
        //                jsonList.Add(new ObjectJsonValue(Parse(((JsonObject)value).JsonString)));
        //        else
        //            jsonList.Add(new ObjectJsonValue((JSONDocument)value));
        //    }
        //    return jsonList;
        //}

        #region Equals/CompareTo

        public override bool Equals(object obj)
        {
            var otherDocument = obj as JSONDocument;
            if (otherDocument == null)
                return false;

            ICollection<string> thisDocAttributes = GetAttributes();
            ICollection<string> otherDocAttributes = otherDocument.GetAttributes();
            if (thisDocAttributes.Count != otherDocAttributes.Count) return false;
            foreach (string attribute in thisDocAttributes)
            {
                if (otherDocAttributes.Contains(attribute))
                {
                    ExtendedJSONDataTypes otherAttributeType = otherDocument.GetAttributeDataType(attribute);
                    ExtendedJSONDataTypes thisAttributeType = GetAttributeDataType(attribute);
                    if (otherAttributeType == thisAttributeType)
                    {
                        switch (otherAttributeType)
                        {
                            case ExtendedJSONDataTypes.Array:
                                var firstArray = new ArrayJsonValue(JsonDocumentUtil.GetArray(this, attribute));
                                var secondArray = new ArrayJsonValue(JsonDocumentUtil.GetArray(otherDocument, attribute));
                                if (!firstArray.Equals(secondArray))
                                    return false;
                                break;
                            case ExtendedJSONDataTypes.Boolean:
                                var firstBool = (bool)_values[attribute];
                                var secondBool = (bool)otherDocument._values[attribute];
                                if (!firstBool.Equals(secondBool))
                                    return false;
                                break;
                            case ExtendedJSONDataTypes.DateTime:
                                var firstDateTime = (DateTime)_values[attribute];
                                var secondDateTime = (DateTime)otherDocument[attribute];
                                if (!firstDateTime.Equals(secondDateTime))
                                    return false;
                                break;
                            case ExtendedJSONDataTypes.Null:
                                break;
                            case ExtendedJSONDataTypes.Number:
                                var firstNumber = new NumberJsonValue(_values[attribute]);
                                var secondNumber = new NumberJsonValue(otherDocument[attribute]);
                                if (!firstNumber.Equals(secondNumber))
                                    return false;
                                break;
                            case ExtendedJSONDataTypes.Object:
                                IJSONDocument firstDocument = Parse(this[attribute].ToString());
                                IJSONDocument secondDocument = Parse(otherDocument[attribute].ToString());
                                if (!firstDocument.Equals(secondDocument))
                                    return false;
                                break;
                            case ExtendedJSONDataTypes.String:
                                var firstString = (string)_values[attribute];
                                var secondString = (string)otherDocument._values[attribute];
                                if (!firstString.Equals(secondString))
                                    return false;
                                break;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is JSONDocument)
            {
                return Equals(obj) ? 0 : int.MinValue;
            }
            return -1;
        }

        //public IJsonValue GetJsonValue(string attribute)
        //{
        //    ExtendedJSONDataTypes jsonType = GetAttributeDataType(attribute);
        //    switch (jsonType)
        //    {
        //        case ExtendedJSONDataTypes.Array:
        //            return new ArrayJsonValue(GetArray(attribute));
        //        case ExtendedJSONDataTypes.Boolean:
        //            return new BooleanJsonValue((bool)_values[attribute]);
        //        case ExtendedJSONDataTypes.DateTime:
        //            return new DateTimeJsonValue((DateTime)_values[attribute]);
        //        case ExtendedJSONDataTypes.Null:
        //            return new NullValue();
        //        case ExtendedJSONDataTypes.Number:
        //            return new NumberJsonValue(_values[attribute]);
        //        case ExtendedJSONDataTypes.Object:
        //            return new ObjectJsonValue(Parse(_values[attribute].ToString()));
        //        case ExtendedJSONDataTypes.String:
        //            return new StringJsonValue((string)_values[attribute]);
        //    }
        //    return null;
        //}

        #endregion

        #region Private Methods

        //private static void ExpandAttribute(IJSONDocument source, Queue<string> attributeQueue, object newValue)
        //{
        //    string currentAttribute = attributeQueue.Dequeue();
        //    bool lastAttribute = attributeQueue.Count == 0;
        //    if (lastAttribute)
        //    {
        //        source[currentAttribute] = newValue;
        //    }
        //    else
        //    {
        //        if (source.Contains(currentAttribute))
        //        {
        //            ExtendedJSONDataTypes type = source.GetAttributeDataType(currentAttribute);
        //            switch (type)
        //            {
        //                case ExtendedJSONDataTypes.Object:
        //                    //Recurecurecurecurecurecurecurecurecurecurecurecurecurecurecursion
        //                    ExpandAttribute(source.GetDocument(currentAttribute), attributeQueue, newValue);
        //                    break;
        //                default:
        //                    IJSONDocument subDocument = JSONType.CreateNew();
        //                    source[currentAttribute] = subDocument;
        //                    ExpandAttribute(subDocument, attributeQueue, newValue);
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            IJSONDocument subDocument = JSONType.CreateNew();
        //            source[currentAttribute] = subDocument;
        //            ExpandAttribute(subDocument, attributeQueue, newValue);
        //        }
        //    }
        //}

     
        


        private static ArrayList ParseArray(JToken value)
        {
            ArrayList arrayList = new ArrayList();

            foreach (var token in value)
            {
                if (token is JObject)
                {
                    arrayList.Add(Parse(token.ToString()));
                }
                else if (token is JArray)
                {
                    arrayList.Add(ParseArray(token));
                }
                else if (token is JValue)
                {
                    arrayList.Add(((JValue)token).Value);
                }
                else
                    throw new Exception(string.Format("Type {0} is not supported", token.GetType().FullName));
            }
            return arrayList;
        }


        private void VerifyParameter(string attribute)
        {
            if (attribute == null)
                throw new ArgumentException("Value cannot be null", attribute);
            if (!_values.Contains(attribute))
                throw new ArgumentException("Specified attribute " + attribute + " does not exist in document");
        }

        internal IDictionary<string, object> ToDictionary()
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (DictionaryEntry entry in _values)
                dictionary.Add((string)entry.Key, entry.Value);
            return dictionary;
        }

        private void ExpandComplexObject(string attribute)
        {
            if (!(_values[attribute] is JsonObject))
                return;

            JsonObject value = (JsonObject)_values[attribute];
            if (value.JsonComplexType == JsonObject.ObjectType.Object)
            {
                JSONDocument document = (JSONDocument)Parse(value.JsonString);
                RemoveValueSize(value); // Remove Un-Expanded object's size
                AddAttribute(attribute, document, true);
                CalculateSize(document); // Add Expanded object's size
            }
            else
            {
                ArrayList objectArray = ParseArray(JArray.Parse(value.JsonString));
                Array convertedArray = objectArray.ToArray();
                RemoveValueSize(value); // Remove Un-Expanded object's size
                AddAttribute(attribute, convertedArray, true);
                CalculateSize(convertedArray); // Add Expanded object's size
            }
        }

        private void TryExpendComplexObject(string attribute)
        {
            ExtendedJSONDataTypes valueType = GetAttributeDataType(attribute);
            if (valueType == ExtendedJSONDataTypes.Array || valueType == ExtendedJSONDataTypes.Object)
            {
                if (_values[attribute] is JsonObject || _values[attribute] is JsonObject[])
                {
                    ExpandComplexObject(attribute);
                }
            }
        }

        private bool TryGetArray(object value, out Array array)
        {
            Array sourceArray = array = null;

            if (value is Array)
                sourceArray = (Array)value;
            else if (value is ArrayList)
                sourceArray = ((ArrayList)value).ToArray();

            if (sourceArray != null)
            {
                //temporary fix
                array = sourceArray.Clone() as Array;
                //Array.Copy(sourceArray, array, sourceArray.Length);
                return true;
            }
            return false;
        }



        private bool IsNumber(object value)
        {
            if (value == null)
                return false;
           
            Type valueType = value.GetType();
            TypeCode actualType = Type.GetTypeCode(valueType);
            switch (actualType)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Compact Serialization

        public virtual void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            JSONDocument doc = Deserialize(new BinaryReader(reader.BaseStream, System.Text.Encoding.UTF8));
            if(doc != null)
            {
                _values = doc._values;
                _valuesSize = doc._valuesSize;
                _lockObject = new object();
            }
            
        }

        public virtual void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
           Serialize(new BinaryWriter(writer.BaseStream,System.Text.Encoding.UTF8), this);
           
        }

        #endregion

        #region Clone

        public object Clone()
        {
            JSONDocument deepCloneDoc = new JSONDocument();
            lock (_lockObject)
            {
                foreach(DictionaryEntry pair in _values)
                {
                    deepCloneDoc.Add((string)pair.Key, CloneObject(pair.Value));   
                }
            }
            return deepCloneDoc;
        }

        private object CloneObject(object value)
        {
            if (value is JsonObject || value is IJSONDocument)
            {
                return ((ICloneable)value).Clone();
            }
            //TODO Implement support for ILIST
            else if (value is Array || value is ArrayList)
            {
                ArrayList list = new ArrayList();
                foreach (var item in (IEnumerable)value) 
                {
                    list.Add(CloneObject(item));
                }
                return list;
            }
            else
            {
                return value;
            }
        }

        #endregion

        #region Size

        public long Size
        {
            get
            {
                //return ToString().Length;
                return _values.IndexInMemorySize // size of HashVector In-Memory
                     + _valuesSize // calculated size of values in HashVector
                     + KeyAttribute.Length // length of the string _key
                     + sizeof(long) // size of _valuesSize 
                     + 32; // size of lockObject's reference and value.
            }
        }

        #endregion

        #region Nested Class JsonObject

        public class JsonObject : ICloneable, ICompactSerializable
        {
            public enum ObjectType
            {
                Array = 0,
                Object = 1
            }

            public JsonObject(string jsonString, ObjectType jsonComplexType)
            {
                JsonString = jsonString;
                JsonComplexType = jsonComplexType;
            }

            public long Size
            {
                get
                {
                    return JsonString.Length + sizeof(int); // jsonstring length + enum size
                }
            }
             public string JsonString { get; set; }

             public ObjectType JsonComplexType { get; set ;}

             public object Clone()
             {
                 JsonObject newObject = new JsonObject("", ObjectType.Object);
                 newObject.JsonString = JsonString.Clone() as string;
                 newObject.JsonComplexType = JsonComplexType;
                 return newObject;
             }

             public void Deserialize(Serialization.IO.CompactReader reader)
             {
                 JsonComplexType = (ObjectType)reader.ReadInt32();
                 JsonString = reader.ReadObject() as string;
             }

             public void Serialize(Serialization.IO.CompactWriter writer)
             {
                 writer.Write((int)JsonComplexType);
                 writer.WriteObject(JsonString);
             }
        }

        #endregion

        #region IEumerabel

        //public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        //{
        //    foreach (var kvp in _values) //.ToArray()
        //    {
        //        if (kvp.Value is JsonObject || kvp.Value is Array || kvp.Value is ArrayList)
        //            ExpandComplexObject(kvp.ToString()); //ExpandComplexObject(kvp.Key);
        //    }
        //    return _values.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        #endregion



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (DictionaryEntry keyValue in _values)
            {
                if (keyValue.Value is JsonObject || keyValue is Array || keyValue is ArrayList)
                    ExpandComplexObject((string)keyValue.Key);
            }

            IDictionaryEnumerator dicEnum = _values.GetEnumerator();
            while (dicEnum.MoveNext())
                yield return new KeyValuePair<string, object>((string)dicEnum.Key, dicEnum.Value);

        }
        public static byte[] Serialize(JSONDocument doc)
        {
            using (var stream = new DataStructures.Clustered.ClusteredMemoryStream())
            {
                var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8);
                Serialize(writer, doc);
                return stream.ToArray();
            }
        }

        public static void Serialize(Stream stream, JSONDocument doc)
        {
            var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8);
            Serialize(writer, doc);
        }

        private static void Serialize(BinaryWriter writer, JSONDocument doc)
        {
            const short version = 1;
            writer.Write(version);
            writer.Write(doc._valuesSize);
            writer.Write(doc._values.Count);
            IDictionaryEnumerator ide = doc._values.GetEnumerator();

            while (ide.MoveNext())
            {
                if (ide.Key != null)
                {
                    writer.Write(ide.Key as string);
                }

                SerializeValue(writer, ide.Value);
            }
        }

        private static void SerializeValue(BinaryWriter writer, object value)
        {
            byte valueType = 0;
            if (value is JSONDocument)
            {
                valueType = SerializeDocument(writer, value);
            }
            else if (value is JsonObject)
            {
                valueType = 3;
                writer.Write(valueType);
                JsonObject jsonObj = value as JsonObject;
                writer.Write((short)jsonObj.JsonComplexType);
                writer.Write(jsonObj.JsonString);
            }
            else if (value is ArrayList)
            {
                ArrayList list = value as ArrayList;
                valueType = 4;
                writer.Write(valueType);
                writer.Write(list.Count);

                for (int i = 0; i < list.Count; i++)
                {
                    SerializeValue(writer,list[i]);
                }
            }
            else if (value is Array)
            {
                Array list = value as Array;
                valueType = 5;
                writer.Write(valueType);
                writer.Write(list.Length);

                for (int i = 0; i < list.Length; i++)
                {
                    SerializeValue(writer, list.GetValue(i));
                }
            }
            else
            {
                SerializeBasicType(writer, value);
            }

        }

        private static byte SerializeBasicType(BinaryWriter writer, object value)
        {
            byte valueType = 1;
            writer.Write(valueType);
            ExtendedJSONDataTypes dataType = JsonDocumentUtil.GetExtendedJsonType(value);
            writer.Write((byte)dataType);

            switch (dataType)
            {
                case ExtendedJSONDataTypes.Number:
                    SerializeNumber(writer, value);
                    break;

                case ExtendedJSONDataTypes.String:
                    writer.Write((string)value);
                    break;

                case ExtendedJSONDataTypes.Boolean:
                    writer.Write((bool)value);
                    break;

                case ExtendedJSONDataTypes.DateTime:
                    writer.Write(((DateTime)value).ToBinary());
                    break;
            }
            return valueType;
        }

        private static byte SerializeNumber(BinaryWriter writer,object value)
        {
            byte type = 0;
            if (value is double)
            {
                writer.Write((byte)1);
                writer.Write((double)value);
            }
            else if (value is decimal)
            {
                writer.Write((byte)2);
                writer.Write((decimal)value);
            }
            else if (value is long )
            {
                writer.Write((byte)3);
                writer.Write((long)value);
            }
            else if (value is BigInteger)
            {
                writer.Write((byte)4);
                writer.Write(((BigInteger)value).ToString());
            }
            else
            {
                TypeCode typeCode = Type.GetTypeCode(value.GetType());
                switch (typeCode)
                {
                    case TypeCode.Byte:
                        writer.Write((byte)3);
                        writer.Write((long)(byte)value);
                        break;
                    case TypeCode.SByte:
                        writer.Write((byte)3);
                        writer.Write((long)(sbyte)value);
                        break;
                    case TypeCode.Int16:
                        writer.Write((byte)3);
                        writer.Write((long)(short)value);
                        break;
                    case TypeCode.UInt16:
                        writer.Write((byte)3);
                        writer.Write((long)(ushort)value);
                        break;
                    case TypeCode.Int32:
                        writer.Write((byte)3);
                        writer.Write((long)(int)value);
                        break;
                    case TypeCode.UInt32:
                        writer.Write((byte)3);
                        writer.Write((long)(uint)value);
                        break;
                    case TypeCode.Int64:
                        writer.Write((byte)3);
                        writer.Write((long)value);
                        break;
                    case TypeCode.UInt64:
                        writer.Write((byte)3);
                        writer.Write((long)(ulong)value);
                        break;
                    case TypeCode.Single:
                        writer.Write((byte)1);
                        writer.Write((double)(float)value);
                        break;
                    case TypeCode.Double:
                        writer.Write((byte)1);
                        writer.Write((double)value);
                        break;
                    case TypeCode.Decimal:
                        writer.Write((byte)2);
                        writer.Write((double)value);
                        break;
                    default:
                        throw new Exception("Unexpected Type" + typeCode.ToString() + " detected ");
                }
            }


            return type;
        }

        private static byte SerializeDocument(BinaryWriter writer, object value)
        {
            byte valueType = 2;
            writer.Write(valueType);
            byte[] buffer = JSONDocument.Serialize(value as JSONDocument);
            writer.Write(buffer.Length);
            writer.Write(buffer);
            return valueType;
        }

        public static JSONDocument Deserialize(byte[] buffer)
        {
            using (var stream = new DataStructures.Clustered.ClusteredMemoryStream(buffer))
            {
                var reader = new BinaryReader(stream, System.Text.Encoding.UTF8);
                return Deserialize(reader);
            }
        }


        public static JSONDocument Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream, System.Text.Encoding.UTF8);
            return Deserialize(reader);
        }
        private static JSONDocument Deserialize(BinaryReader reader)
        {
            JSONDocument doc = null;
            short version = reader.ReadInt16();
            long valuesSize = reader.ReadInt64();
            int valueCount = reader.ReadInt32();

            doc = new JSONDocument(valueCount);
            doc._valuesSize = valuesSize;

            for (int i = 0; i < valueCount; i++)
            {
                string key = null;
                key = reader.ReadString();
                object value = DeserializeValue(reader);
                doc._values.Add(key, value);
            }
            return doc;

        }

        private static object DeserializeValue(BinaryReader reader)
        {
            object value = null;
            byte valueType = reader.ReadByte();
            if (valueType != 0)
            {
                if(valueType == 1)
                {
                    ExtendedJSONDataTypes dataType = (ExtendedJSONDataTypes)reader.ReadByte();

                    switch (dataType)
                    {
                        case ExtendedJSONDataTypes.Number:
                            value = DeserializeNumber(reader);
                            break;

                        case ExtendedJSONDataTypes.String:
                            value = reader.ReadString();
                            break;

                        case ExtendedJSONDataTypes.Boolean:
                            value = reader.ReadBoolean();
                            break;  

                        case ExtendedJSONDataTypes.DateTime:
                            value = DateTime.FromBinary(reader.ReadInt64());
                            break;
                    }
                }
                else if (valueType == 2)
                {
                    valueType = 2;
                    int byteCount = reader.ReadInt32();
                    value = JSONDocument.Deserialize(reader.ReadBytes(byteCount));
                }
                else if (valueType == 3)
                {
                    JsonObject.ObjectType type = (JsonObject.ObjectType)reader.ReadInt16();
                    String jsonStr = reader.ReadString();
                    value = new JsonObject(jsonStr, type);
                }
                else if (valueType == 4)
                {
                    int count = reader.ReadInt32();

                    ArrayList list = new ArrayList(count);
                    value = list;
                    for(int i =0; i<count; i++)
                    {
                        list.Add(DeserializeValue(reader));
                    }

                }
                else if (valueType == 5)
                {
                    int count = reader.ReadInt32();

                    Object[] list = new object[count];
                    value = list;
                    for (int i = 0; i < count; i++)
                    {
                        list[i] = DeserializeValue(reader);
                    }
                }
            }
            return value;
        }

        private static object DeserializeNumber(BinaryReader reader)
        {
            byte type = reader.ReadByte();

            switch(type)
            {
                case 1:
                    return reader.ReadDouble();

                case 2:
                    return reader.ReadDecimal();

                case 3:
                    return reader.ReadInt64();

                case 4:
                    return BigInteger.Parse(reader.ReadString());
            }
            return null;
        }
    }
}
