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
using System.Linq;
using System.Text;

using Alachisoft.NosDB.Common.Serialization;

using Newtonsoft.Json;


namespace Alachisoft.NosDB.Common.Server.Engine
{
    public class DocumentKey : ICompactSerializable, IComparable,ICloneable
    {
        private object _value;
        private Type _valueType;

        public DocumentKey() { }

        public DocumentKey(object key)
        {
            if (!IsKeyValid(key))
                throw new ArgumentException("Key is not a valid JSON value");
            _value = key;
            if(key != null)
                _valueType = key.GetType();
        }

        [JsonProperty(PropertyName = "Value")]
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _valueType =  value.GetType();
            }
        }

        public override string ToString()
        {
            return _value != null? _value.ToString(): null;
        }

        public ExtendedJSONDataTypes GetJSONType()
        {
            if (_value == null)
            {
                return ExtendedJSONDataTypes.Null;
            }
            
            //this check will be removed after validation
            if(_value is DocumentKey)
                return ((DocumentKey)_value).GetJSONType();

            Type valueType = _value.GetType();
            TypeCode typeCode = Type.GetTypeCode(valueType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ExtendedJSONDataTypes.Boolean;
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return ExtendedJSONDataTypes.Number;
                case TypeCode.Char:
                case TypeCode.String:
                    return ExtendedJSONDataTypes.String;
                case TypeCode.DateTime:
                    return ExtendedJSONDataTypes.DateTime;
            }
            if (valueType.IsArray)
                return ExtendedJSONDataTypes.Array;
            return ExtendedJSONDataTypes.Object;
        }

        public bool IsKeyValid( object value)
        {
            if (value == null || value is string)
                return true;
            return false;
           
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            DocumentKey key = obj as DocumentKey;
            if (key == null)
                return false;
            
            return key._value.Equals(this._value);
        }

        public static DocumentKey FromJson(string p)
        {
            return JsonConvert.DeserializeObject<DocumentKey>(p);
        }

        public string ToJson()
        {
            //To:DO
            return JsonConvert.SerializeObject(this); 
        }

        #region ICompactSerializable Methods
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            //_value = reader.Read() as string
            _value = reader.ReadObject();
            _valueType = _value.GetType();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            //TODO: 
           // writer.Write(JsonConvert.SerializeObject(_value));
            writer.WriteObject(_value);
        }
        #endregion

        #region IComparable Methods
        public int CompareTo(object docKey)
        {
            DocumentKey doc = docKey as DocumentKey;
            object obj = doc.Value;

            if (_value == null && obj == null)
                return 0;

            if (_value == null)
                return -1;

            if (obj == null)
                return 1;


            Type valueType = obj.GetType();
            if (!valueType.Equals(_valueType))
            {
                throw new Exception("In DocumentKey: Invalid Type for DocumentKey Comparison");
            }
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    long field1 = long.Parse(_value.ToString());
                    long field2 = long.Parse(obj.ToString());
                    return field1.CompareTo(field2);
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    double dField1 = double.Parse(_value.ToString());
                    double dField2 = double.Parse(obj.ToString());
                    return dField1.CompareTo(dField2);
                case TypeCode.Char:
                case TypeCode.String:
                    string sField1 = _value.ToString();
                    string sField2 = obj.ToString();
                    return sField1.CompareTo(sField2);
                case TypeCode.DateTime:
                    DateTime dtField1 = DateTime.Parse(_value.ToString());
                    DateTime dtField2 = DateTime.Parse(obj.ToString());
                    return dtField1.CompareTo(dtField2);
                default:
                    throw new Exception("In DocumentKey: Invalid Type for DocumentKey Comparison");
            }
        }
        #endregion
      
        //public static DocumentKey FromJson(string p)
        //{
        //    return JsonConvert.DeserializeObject<DocumentKey>(p);
        //}

        //public string ToJson()
        //{
        //    return JsonConvert.SerializeObject(this); 
        //}

        public object Clone()
        {
            DocumentKey clone = new DocumentKey();
            clone._value = _value;
            clone._valueType = _valueType;

            return clone;
        }
    }
}
