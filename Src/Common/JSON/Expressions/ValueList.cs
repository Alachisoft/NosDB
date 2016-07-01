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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    //Todo: In this list if there is an array or document is specified,
    //Todo: this means that it will act as a single variable.
    public class ValueList : IEnumerable<IEvaluable>, IEvaluable, IJsonValue
    {
        private readonly List<IEvaluable> _values = new List<IEvaluable>();

        public int Count
        {
            get { return _values.Count; }
        }

        public IEvaluable this[int index]
        {
            get
            {
                return _values[index];
            }
        }

        public ValueList(){}

        public ValueList(List<IEvaluable> values)
        {
            _values = values;
        }

        public void Add(IEvaluable item)
        {
            _values.Add(item);
        }

        public void AddRange(IEnumerable<IEvaluable> values)
        {
            if(values != null)  
                _values.AddRange(values);
        }

        public bool Contains(IJSONDocument entry, IJsonValue item)
        {
            foreach (var value in _values)
            {
                IJsonValue outValue;
                if (value.Evaluate(out outValue, entry) 
                    && item.Equals(outValue))
                    return true;
            }
            return false;
        }

        public bool Contains(IJsonValue value)
        {

            return ((IJsonValue[])Value).Contains(value);
        }
        
        public IEnumerable<IEvaluable> Values { get { return _values.ToList(); } } 

        #region IEnumerable<IEvaluable> members

        public IEnumerator<IEvaluable> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var value in _values)
                value.AssignConstants(parameters);
        }

        #endregion

        #region IEvaluable members

        public string InString
        {
            get
            {
                string value = "(";

                for (int i = 0; i < _values.Count; i++)
                {
                    value += _values[i].InString;

                    if (i != _values.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + ")";
            }
        }

        public EvaluationType EvaluationType
        {
            get
            {
                EvaluationType evaluation = EvaluationType.Constant;
                foreach (var evaluable in _values)
                {
                    if (evaluable is ArrayEvaluable)
                        return EvaluationType.AllVariable;

                    switch (evaluable.EvaluationType)
                    {
                        case EvaluationType.AllVariable:
                            return EvaluationType.AllVariable;

                        case EvaluationType.MultiVariable:
                            evaluation = EvaluationType.MultiVariable;
                            break;

                        case EvaluationType.SingleVariable:
                            if (evaluation != EvaluationType.MultiVariable)
                            {
                                if (evaluation == EvaluationType.SingleVariable)
                                {
                                    evaluation = EvaluationType.MultiVariable;
                                }
                                else
                                {
                                    evaluation = EvaluationType.SingleVariable;
                                }
                            }
                            break;
                    }
                }
                return evaluation;
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = this;
            return true;
        }

        public IComparable Add(IEvaluable item, IJSONDocument document)
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

        public List<Attribute> Attributes { get { return new List<Attribute>(); } }

        public List<Function> Functions 
        { 
            get 
            {
                List<Function> functions = new List<Function>();
                foreach (var evaluable in _values)
                    if (evaluable.Functions != null && evaluable.Functions.Count > 0)
                        functions.AddRange(evaluable.Functions);
                return functions;
            }
        }

        #endregion

        #region IComparable members

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
        
        #region IJsonValue members

        public FieldDataType DataType
        {
            get { return FieldDataType.Array; }
        }

        public object Value
        {
            get
            {
                IJsonValue[] arrayJsonValues = new IJsonValue[_values.Count];
                for (int i = 0; i < _values.Count; i++)
                {                    
                    if(!_values[i].Evaluate(out arrayJsonValues[i], null))
                        return null;
                }
                return arrayJsonValues;
            }
        }
        #endregion

        public override string ToString()
        {
            string name = String.Empty;
            for (int i = 0; i < _values.Count; i++)
            {
                name += _values[i] + (i != _values.Count - 1 ? "-" : string.Empty);
            }
            return name;
        }

        public override bool Equals(object obj)
        {
            ValueList valueList2 = (ValueList) obj;
            IJsonValue[] array1JsonValues = (IJsonValue[]) Value;
            IJsonValue[] array2JsonValues = (IJsonValue[]) valueList2.Value;
            if (array1JsonValues.Length != array2JsonValues.Length)
                return false;

            for (int i = 0; i < array1JsonValues.Length; i++)
            {
                if (array1JsonValues[i].DataType != array2JsonValues[i].DataType)
                    return false;

                if (!array1JsonValues[i].Equals(array2JsonValues[i]))
                    return false;
            }
            return true;
        }
        
        public TypeCode NativeType
        {
            get { return TypeCode.DBNull; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("ValueList:{");
            output.Write("Values=");
            if (_values != null)
            {
                output.Write("[");
                for (int i = 0; i < _values.Count; i++)
                {
                    _values[i].Print(output);
                    if(i!=_values.Count-1)
                        output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }


        public string CaseSensitiveInString
        {
            get
            {
                string value = "(";

                for (int i = 0; i < _values.Count; i++)
                {
                    value += _values[i].CaseSensitiveInString;

                    if (i != _values.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + ")";
            }
        }
    }
}
