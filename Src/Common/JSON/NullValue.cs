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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;
using System;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Core.Storage.Queries.Util;

namespace Alachisoft.NosDB.Common.JSON
{
    public class NullValue : AttributeValue, IEvaluable, IJsonValue
    {
        private SortOrder _order;

        public static readonly NullValue Null = new NullValue();

        public NullValue() { }

        public NullValue(SortOrder order)
        {
            _order = order;
        }

        public override int CompareTo(object obj)
        {
            var eList = obj as EmbeddedList;
            if (eList!=null)
                return EmbeddedHelper.Compare(this, eList);
            var value = obj as AttributeValue;
            if(value!=null)
                switch (value.Type)
                {
                    case AttributeValueType.Null:
                    case AttributeValueType.All:
                        return 0;
                        case AttributeValueType.Mask:
                        return 0 - value.CompareTo(this);
                        case AttributeValueType.None:
                        return -1;
                        case AttributeValueType.Single:
                        if (value.DataType.Equals(FieldDataType.Null))
                            return 0;
                        return _order == SortOrder.ASC ? int.MinValue : int.MaxValue;
                }
            return _order == SortOrder.ASC ? int.MinValue : int.MaxValue;

            //if (obj is NullValue || obj is AllValue)
            //    return 0;
            //AttributeValue value = obj as AttributeValue;
            //if (value != null && value.DataType.Equals(FieldDataType.Null))
            //    return 0;
            //return _order == SortOrder.ASC ? int.MinValue : int.MaxValue;
        }

        public override TypeCode ActualType
        {
            get { return TypeCode.DBNull; }
        }

        public override SortOrder Order { get { return _order; } }

        public override string ValueInString
        {
            get { return "NullValue"; }
        }

        public override bool Equals(object obj)
        {
            var value = obj as AttributeValue;
            if (value != null)
            {
                switch (value.Type)
                {
                    case AttributeValueType.All:
                        return true;
                    case AttributeValueType.Null:
                        return _order.Equals(value.Order);
                    default:
                        return false;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
        
        public override FieldDataType DataType
        {
            get { return FieldDataType.Null; }
        }

        public override void Deserialize(Common.Serialization.IO.CompactReader reader)
        { }

        public override void Serialize(Common.Serialization.IO.CompactWriter writer)
        { }

        public override IComparable this[int index]
        {
            get { return this; }
        }
        
        public override AttributeValueType Type
        {
            get { return AttributeValueType.Null; }
        }

        #region IEvaluable members

        public string InString
        {
            get { return "NULL"; }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = new NullValue();
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }
        public IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public void AssignConstants(IList<IParameter> parameters) { }

        public EvaluationType EvaluationType { get { return EvaluationType.Constant; } }

        public List<Expressions.Attribute> Attributes { get { return new List<Expressions.Attribute>(); } }

        public List<Function> Functions { get { return new List<Function>(); } }
        
        #endregion

        public object Value
        {
            get { return null; }
        }

        public TypeCode NativeType
        {
            get { return TypeCode.Empty; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("NullValue");
        }

        public override object Clone()
        {
            return Null;
        }


        public string CaseSensitiveInString
        {
            get { return "NULL"; }
        }
    }
}
