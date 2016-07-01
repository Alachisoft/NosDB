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
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public class Field : ICloneable, IPrintable
    {
        private IEvaluable _fieldName;
        private SortOrder _sortOrder;
        protected FieldType _type;
        protected Guid _fieldId = Guid.NewGuid();

        public Field(IEvaluable name, FieldType type)
        {
            _fieldName = name;
            _type = type;
        }

        public Field(IEvaluable name, FieldType type, SortOrder order)
        {
            _fieldName = name;
            _type = type;
            _sortOrder = order;
        }

        public FieldType Type { get { return _type; } }

        public Guid FieldId { get { return _fieldId; } }

        public virtual IEvaluable FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        public SortOrder SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        public enum FieldType
        {
            Grouped,
            Ordered,
            Distinct
        }

        //public override bool Equals(object obj)
        //{
        //    var field = obj as Field;
        //    if (field != null)
        //    {
        //        Field target = field;
        //        return FieldName.Equals(target.FieldName) && _fieldId.Equals(target._fieldId);
        //    }
        //    return false;
        //}

        //public virtual int CompareTo(object obj)
        //{
        //    if ((obj is string))
        //        return FieldName.CompareTo(obj);
        //    if (obj is Field)
        //    {
        //        Field target = (Field)obj;
        //        return FieldName.CompareTo(target.FieldName);
        //    }
        //    else
        //    {
        //        throw new ArgumentException("Field name type mismatch. ");
        //    }
        //}

        public virtual int GetHashCode()
        {
            return FieldName.GetHashCode();
        }

        public virtual string ToString()
        {
            return FieldName.ToString();
        }

        public object Clone()
        {
            return new Field(_fieldName, _type, _sortOrder);
        }

        public virtual bool GetAttributeValue(IJSONDocument document, out AttributeValue value)
        {
            if (GetValue(document, out value))
            {
                var attributeList = new List<AttributeValue> {new SingleAttributeValue(_fieldId.ToString())};
                attributeList.Add(value);
                value = new MultiAttributeValue(attributeList);
                return true;
            }
            return false;
        }

        internal bool GetValue(IJSONDocument document, out AttributeValue value)
        {
            value = NullValue.Null;
            if (document != null)
            {
                IJsonValue wrapedValue;
                if (_fieldName.Evaluate(out wrapedValue, document))
                {
                    if (wrapedValue.DataType != FieldDataType.Null)
                    {
                        if ((wrapedValue.DataType != FieldDataType.Array && wrapedValue.DataType != FieldDataType.Embedded))
                            value = new SingleAttributeValue((IComparable)wrapedValue.Value, _sortOrder);
                        else
                        {
                            if (_type != FieldType.Ordered)
                                value = new SingleAttributeValue(wrapedValue, _sortOrder);
                            else
                                throw new QuerySystemException(ErrorCodes.Query.ARRAY_FOUND_IN_ORDERBY);
                        }
                    }
                    else
                    {
                        value = new NullValue(_sortOrder);
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        public virtual bool FillWithAttributes(IJSONDocument source, IJSONDocument target)
        {
            if (source != null && target != null)
            {
                IJsonValue wrapedValue;
                if (_fieldName.ToString().Equals("_key"))
                {
                    if (source.Contains("!_key"))
                    {
                        target.Key = source.GetString("!_key");
                    }
                    else
                    {
                        target.Add("!_key", source.Key);
                    }
                    return true;
                }

                if (_fieldName.Evaluate(out wrapedValue, source))
                {
                    IAttributeChain attributor = Attributor.Create(Attributor.Delimit(_fieldName.InString, new[] {'$'}));
                    Attributor.TryUpdate(target, wrapedValue.Value, attributor, true);
                    return true;
                }
            }
            return false;
        }

        public virtual void Print(System.IO.TextWriter output)
        {
            output.Write("Field:{");
            output.Write("FieldName:");
            if (_fieldName != null)
            {
                _fieldName.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",SortOrder="+_sortOrder.ToString());
            output.Write(",FieldType="+_type.ToString());
            output.Write(",FieldId="+_type.ToString());
            output.Write("}");
        }
    }
}
