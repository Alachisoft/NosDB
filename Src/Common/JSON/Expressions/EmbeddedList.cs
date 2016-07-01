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

using Alachisoft.NosDB.Common.Enum;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class EmbeddedList : IJsonValue
    {
        private readonly List<IJsonValue> _values = new List<IJsonValue>();

        public EmbeddedList()
        {
        }

        public EmbeddedList(List<IJsonValue> values)
        {
            _values = values;
        }

        public void Add(IJsonValue value)
        {
            _values.Add(value);
        }

        public void AddRange(EmbeddedList list)
        {
            IJsonValue[] values = (IJsonValue[])list.Value;
            foreach (var jsonValue in values)
            {
                Add(jsonValue);
            }
        }
        
        #region IJsonValue members

        public FieldDataType DataType
        {
            get
            {
                FieldDataType type = FieldDataType.Object;
                foreach (var value in _values)
                {
                    if (value.DataType != FieldDataType.Object)
                    {
                        if (type == FieldDataType.Object)
                            type = value.DataType;

                        if(value.DataType == FieldDataType.Array)
                            type = FieldDataType.Array;
                    }
                }

                if (type == FieldDataType.Object || type == FieldDataType.Array)
                    return type;
                
                return FieldDataType.Embedded;
            }
        }

        public TypeCode NativeType
        {
            get { return TypeCode.Object;}
        }

        public object Value
        {
            get
            {
                object[] array = new object[_values.Count];
                for (int i = 0; i < _values.Count; i++)
                {
                    array[i] = _values[i].Value;
                }
                return array;
            }
        }

        public int Length
        {
            get { return _values.Count; }
        }

        public List<IJsonValue> WrapedValue
        {
            get { return _values; }
        }
        //value contains in list comparison
        private int CompareJsonToList(List<IJsonValue> values, IJsonValue value)
        {
            bool isGreater = false;
            foreach (var jsonValue in values)
            {
                int compareValue = jsonValue.CompareTo(value);
                if (compareValue == 0)
                {
                    return 0;
                }
                if (compareValue > 0)
                {
                    isGreater = true;
                }
            }
            if (isGreater)
                return 1;
            return -1;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is EmbeddedList))
            {
                if (!(obj is IJsonValue))
                    throw new ArgumentException("IJSON value is expected in here, always.");
               
                return CompareJsonToList(_values, obj as IJsonValue);
            }

            List<IJsonValue> list = (obj as EmbeddedList).WrapedValue;
            bool isGreater = false;

            foreach (var value in list)
            {
                int compareValue = CompareTo(value);
                if (compareValue < 0)
                {
                    return compareValue;
                }
                if (compareValue > 0)
                {
                    isGreater = true;
                }
            }

            if (isGreater)
            {
                return 1;
            }

            return 0;
        }
        
        //compare if all values in EmbeddedList are less then argument value
        public int CompareLessThen(IJsonValue value)
        {
            if (!(value is EmbeddedList))
            {
                bool isArrayComparison = false;
                List<IJsonValue> argumentIJsonValues = null;
                if (value is ArrayJsonValue)
                {
                    isArrayComparison = true;
                    argumentIJsonValues = new List<IJsonValue>(((ArrayJsonValue)value).WrapedValue);
                }

                foreach (var jsonValue in _values)
                {
                    int compareValue = 0;
                    if (isArrayComparison)
                    {
                        foreach (var arrayIJsonValue in argumentIJsonValues)
                        {
                            compareValue = jsonValue.CompareTo(arrayIJsonValue);
                        }
                    }
                    else
                    {
                        compareValue = jsonValue.CompareTo(value);
                    }
                    if (compareValue > 0)
                    {
                        return 1;
                    }
                }
                return 0;
            }
            return CompareTo(value);
        }

        public int CompareGreaterThen(IJsonValue value)
        {
            if (!(value is EmbeddedList))
            {
                bool isArrayComparison = false;
                List<IJsonValue> argumentIJsonValues = null;
                if (value is ArrayJsonValue)
                {
                    isArrayComparison = true;
                    argumentIJsonValues = new List<IJsonValue>(((ArrayJsonValue)value).WrapedValue);
                }

                foreach (var jsonValue in _values)
                {
                    int compareValue = 0;
                    if (isArrayComparison)
                    {
                        foreach (var arrayIJsonValue in argumentIJsonValues)
                        {
                            compareValue = jsonValue.CompareTo(arrayIJsonValue);
                        }
                    }
                    else
                    {
                        compareValue = jsonValue.CompareTo(value);
                    }
                    if (compareValue < 0)
                    {
                        return -1;
                    }
                }
                return 0;
            }
            return CompareTo(value);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is EmbeddedList))
            {
                foreach (var jsonValue in _values)
                {
                    if(jsonValue.Equals(obj))
                        return true;
                }
                return false;
            }

            EmbeddedList compareList = obj as EmbeddedList;

            foreach (var compareValue in compareList._values)
            {
                foreach (var jsonValue in _values)
                {
                    if(compareValue.Equals(jsonValue))
                        return true;
                }
            }

            return false;
        }
        
        #endregion
    }
}
