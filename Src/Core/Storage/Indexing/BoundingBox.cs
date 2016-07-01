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
using System.Threading;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
    public class BoundingBox : ICompactSerializable
    {
        private AttributeValue[] numericMaxs;
        private AttributeValue[] numericMins;
        private AttributeValue[] arrayMaxs;
        private AttributeValue[] arrayMins;
        
        public BoundingBox(int attributeCount)
        {
            numericMaxs = new AttributeValue[attributeCount];
            numericMins = new AttributeValue[attributeCount];
            arrayMaxs = new AttributeValue[attributeCount];
            arrayMins = new AttributeValue[attributeCount];
            for (int i = 0; i < attributeCount; i++)
            {
                numericMaxs[i] = new SingleAttributeValue(int.MinValue);
                arrayMaxs[i] = numericMaxs[i];
                numericMins[i] = new SingleAttributeValue(int.MaxValue);
                arrayMins[i] = numericMins[i];
            }
        }

        public void SetMax(int attNumber, FieldDataType type, AttributeValue value)
        {
            if(value==null)
                return;

            if (!value.DataType.Equals(type))
                return;

            if (attNumber >= numericMaxs.Length)
                return;
            switch (type)
            {
                case FieldDataType.Number:

                    if (numericMaxs[attNumber].CompareTo(value) < 0)
                        numericMaxs[attNumber] = value;

                    break;

                case FieldDataType.Array:

                    if (arrayMaxs[attNumber].CompareTo(value) < 0)
                        arrayMaxs[attNumber] = value;

                    break;
            }
        }

        public void SetMin(int attNumber, FieldDataType type, AttributeValue value)
        {
            if(value==null)
                return;

            if (attNumber >= numericMaxs.Length)
                return;

            if (!value.DataType.Equals(type))
                return;

            switch (type)
            {
                case FieldDataType.Number:

                    if (numericMins[attNumber].CompareTo(value) > 0)
                        numericMins[attNumber] = value;

                    break;

                case FieldDataType.Array:

                    if (arrayMins[attNumber].CompareTo(value) > 0)
                        arrayMins[attNumber] = value;

                    break;
            }
        }

        public void Resolve(int attNumber, FieldDataType type, AttributeValue value)
        {
            if (value == null)
                return;

            if (attNumber >= numericMaxs.Length)
                return;

            if (!value.DataType.Equals(type))
                return;

            switch (type)
            {
                case FieldDataType.Number:
                    
                    if (numericMaxs[attNumber].CompareTo(value) < 0)
                        numericMaxs[attNumber] = value;
                    else if (numericMins[attNumber].CompareTo(value) > 0)
                        numericMins[attNumber] = value;

                    break;

                case FieldDataType.Array:

                    if (arrayMaxs[attNumber].CompareTo(value) < 0)
                        arrayMaxs[attNumber] = value;
                    else if (arrayMins[attNumber].CompareTo(value) > 0)
                        arrayMins[attNumber] = value;

                    break;
            }
        }

        public AttributeValue Min(int attNumber, FieldDataType type)
        {
            AttributeValue returnValue = null;
            if (attNumber >= arrayMaxs.Length)
                return returnValue;
            switch (type)
            {
                case FieldDataType.Number:
                    returnValue = numericMins[attNumber];
                    break;
                case FieldDataType.Array:
                    returnValue = arrayMins[attNumber];
                    break;
            }
            return returnValue;
        }

        public AttributeValue Max(int attNumber, FieldDataType type)
        {
            AttributeValue returnValue = null;
            if (attNumber >= arrayMaxs.Length)
                return returnValue;
            switch (type)
            {
                case FieldDataType.Number:
                    returnValue = numericMaxs[attNumber];
                    break;
                case FieldDataType.Array:
                    returnValue = arrayMaxs[attNumber];
                    break;
            }
            return returnValue;
        }

        public void ResetMax(int attNumber, FieldDataType type)
        {
            if (attNumber >= arrayMaxs.Length)
                return;
            switch (type)
            {
                case FieldDataType.Number:
                    numericMaxs[attNumber] = new SingleAttributeValue(int.MinValue);
                    break;
                case FieldDataType.Array:
                    arrayMaxs[attNumber] = new SingleAttributeValue(int.MinValue);
                    break;
            }
        }

        public void ResetMin(int attNumber, FieldDataType type)
        {
            if (attNumber >= arrayMaxs.Length)
                return;
            switch (type)
            {
                case FieldDataType.Number:
                    numericMins[attNumber] = new SingleAttributeValue(int.MaxValue);
                    break;
                case FieldDataType.Array:
                    arrayMins[attNumber] = new SingleAttributeValue(int.MaxValue);
                    break;
            }
        }

        #region old
        //private AttributeValue _numericLow;
        //private AttributeValue _numericHigh;
        //private AttributeValue _arrayLow;
        //private AttributeValue _arrayHigh;


        //public void Resolve(FieldDataType key, AttributeValue value)
        //{
        //    if (value == null)
        //        return;

        //    if (value.Equals(AttributeValue.NullValue) && !key.Equals(FieldDataType.Null))
        //        return;

        //    if (key.Equals(FieldDataType.Array))
        //    {
        //        var singleValue = value as SingleAttributeValue;
        //        if(singleValue!=null)
        //            if (!((SingleAttributeValue)((ArrayElement)(singleValue).Value).Element).DataType.Equals(FieldDataType.Number))
        //                return;
        //    }

        //    //if (value is MultiAttributeValue)
        //    //{
        //    //    if (((MultiAttributeValue)value).Values.Count == 0) return;

        //    //    foreach (var attributeValue in ((MultiAttributeValue)value).Values)
        //    //        if(attributeValue != null)
        //    //            Resolve(attributeValue.DataType, attributeValue);

        //    //    return;
        //    //}

        //    AttributeValue lower, upper;



        //    switch (key)
        //    {
        //            case FieldDataType.Number:
        //            if (_numericLow != null)
        //            {
        //                if (_numericLow.CompareTo(value) > 0)
        //                    _numericLow = value;
        //            }
        //            else
        //            {
        //                _numericLow = value;
        //            }

        //            if (_numericHigh != null)
        //            {
        //                if (_numericHigh.CompareTo(value) < 0)
        //                    _numericHigh = value;
        //            }
        //            else
        //            {
        //                _numericHigh = value;
        //            }
        //            break;

        //            case FieldDataType.Array:
        //            if (_arrayHigh != null)
        //            {
        //                if (_arrayHigh.CompareTo(value) > 0)
        //                    _arrayHigh = value;
        //            }
        //            else
        //            {
        //                _arrayHigh = value;
        //            }

        //            if (_arrayLow != null)
        //            {
        //                if (_arrayLow.CompareTo(value) < 0)
        //                    _arrayLow = value;
        //            }
        //            else
        //            {
        //                _arrayLow = value;
        //            }
        //            break;
        //    }
        //}

        //public void ResetMax(FieldDataType key)
        //{
        //    switch (key)
        //    {
        //        case FieldDataType.Number:
        //            _numericHigh = null;
        //            break;

        //        case FieldDataType.Array:
        //            _arrayHigh = null;
        //            break;
        //    }
        //}

        //public void ResetMin(FieldDataType key)
        //{
        //    switch (key)
        //    {
        //        case FieldDataType.Number:
        //            _numericLow = null;
        //            break;

        //        case FieldDataType.Array:
        //            _arrayLow = null;
        //            break;
        //    }
        //}

        //public AttributeValue Min(FieldDataType key)
        //{
        //    switch (key)
        //    {
        //        case FieldDataType.Number:
        //            return _numericLow;
        //        case FieldDataType.Array:
        //            return _arrayLow;
        //        default:
        //            return AttributeValue.NullValue;
        //    }
        //}

        //public AttributeValue Max(FieldDataType key)
        //{
        //    switch (key)
        //    {
        //        case FieldDataType.Number:
        //            return _numericHigh;
        //        case FieldDataType.Array:
        //            return _arrayHigh;
        //        default:
        //            return AttributeValue.NullValue;
        //    }
        //}


        //public void Deserialize(Common.Serialization.IO.CompactReader reader)
        //{
        //    _arrayHigh = reader.ReadObject() as AttributeValue;
        //    _arrayLow = reader.ReadObject() as AttributeValue;
        //    _numericHigh = reader.ReadObject() as AttributeValue;
        //    _numericLow = reader.ReadObject() as AttributeValue;
        //}

        //public void Serialize(Common.Serialization.IO.CompactWriter writer)
        //{
        //    writer.WriteObject(_arrayHigh);
        //    writer.WriteObject(_arrayLow);
        //    writer.WriteObject(_numericHigh);
        //    writer.WriteObject(_numericLow);
        //}
        #endregion
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {

        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {

        }
    }
}
