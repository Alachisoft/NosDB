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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Core.Storage.Queries.Util;

namespace Alachisoft.NosDB.Common.JSON
{
    public class ArrayJsonValue : IJsonValue, ICollection<IJsonValue>
    {
        private IJsonValue[] _value;

        public ArrayJsonValue(IJsonValue[] value)
        {
            _value = value;
        }

        public int Length
        {
            get { return _value.Length; }
        }

        public FieldDataType DataType
        {
            get { return FieldDataType.Array; }
        }

        public object Value
        {
            get
            {
                object[] values = new object[_value.Length];
                for (int i = 0; i < _value.Length; i++)
                {
                    values[i] = _value[i].Value;
                }
                return values;
            }
        }

        public IJsonValue[] WrapedValue
        {
            get { return _value; }
        }

        public bool Contains(IJsonValue item)
        {
            return _value.Contains(item);
        }

        public int CompareTo(object obj)
        {
            IJsonValue secondJsonValue = obj as IJsonValue;
            if (secondJsonValue == null) throw new ArgumentException("Object must be of type IJSONValue");

            if (obj is EmbeddedList)
                return EmbeddedHelper.Compare(this, obj as EmbeddedList);

            int typeRule = DataType.CompareTo(secondJsonValue.DataType);
            if (typeRule != 0)
                return typeRule;
            return Equals(obj) ? 0 : int.MinValue;
        }

        public override bool Equals(object obj)
        {
            var jsonValue = obj as ArrayJsonValue;
            if (jsonValue == null)
                return false;

            var secondList = jsonValue.WrapedValue as IJsonValue[];

            if (secondList == null && _value == null)
                return true;

            if (_value == null || secondList == null)
                return false;

            if (secondList.Length != _value.Length)
                return false;

            for (int i = 0; i < _value.Length; i++)
            {
                FieldDataType jsonType1 = _value[i].DataType;
                FieldDataType jsonType2 = secondList[i].DataType;
                if (jsonType1 != jsonType2)
                    return false;

                switch (jsonType1)
                {
                    case FieldDataType.Array:
                        var firstArray = new ArrayJsonValue(_value[i].Value as IJsonValue[]);
                        var secondArray = new ArrayJsonValue(secondList[i].Value as IJsonValue[]);
                        if (!firstArray.Equals(secondArray))
                            return false;
                        break;
                    case FieldDataType.Bool:
                        var firstBool = (bool)_value[i].Value;
                        var secondBool = (bool)secondList[i].Value;
                        if (!firstBool.Equals(secondBool))
                            return false;
                        break;
                    case FieldDataType.DateTime:
                        var firstDateTime = (DateTime)_value[i].Value;
                        var secondDateTime = (DateTime)secondList[i].Value;
                        if (!firstDateTime.Equals(secondDateTime))
                            return false;
                        break;
                    case FieldDataType.Null:
                        break;
                    case FieldDataType.Number:
                        var firstNumber = new NumberJsonValue(_value[i].Value);
                        var secondNumber = new NumberJsonValue(secondList[i].Value);
                        if (!firstNumber.Equals(secondNumber))
                            return false;
                        break;
                    case FieldDataType.Object:
                        var firstDocument = (IJSONDocument)_value[i].Value;
                        var secondDocument = (IJSONDocument)secondList[i].Value;
                        if (!firstDocument.Equals(secondDocument))
                            return false;
                        break;
                    case FieldDataType.String:
                        var firstString = (string)_value[i].Value;
                        var secondString = (string)secondList[i].Value;
                        if (!firstString.Equals(secondString))
                            return false;
                        break;
                }
            }
            return true;
        }
        
        public TypeCode NativeType
        {
            get { return TypeCode.DBNull; }
        }

        public void Add(IJsonValue item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IJsonValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(IJsonValue item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IJsonValue> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
