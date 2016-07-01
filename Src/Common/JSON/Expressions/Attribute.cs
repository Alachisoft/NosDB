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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Util;
using System.Collections;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class Attribute : IEvaluable, IAttributeChain
    {
        private readonly string _name;
        private int _coefficient = 1;
        private int[] _indecies = null;
        private Attribute _childAttribute = null;
        private ArithmeticOperation _returnOperation = ArithmeticOperation.None;

        public Attribute(string name)
        {
            _name = name;
        }

        public int Coefficient
        {
            get { return _coefficient; }
            set { _coefficient = value; }
        }

        public int[] Indecies
        {
            set { _indecies = value; }
        }
        
        public bool IsMultiLevel
        {
            get { return ((_indecies != null) || (_childAttribute != null)); }
        }

        public ArithmeticOperation ArithmeticOperation
        {
            set { _returnOperation = value; }
        }

        public Attribute ChildAttribute
        {
            set { _childAttribute = value; }
        }

        private IComparable PerformArithmeticOperation(IEvaluable evaluable, IJSONDocument document, ArithmeticOperation operation)
        {
            IJsonValue value1, value2;

            if (!Evaluate(out value1, document))
                return null;
            if (!evaluable.Evaluate(out value2, document))
                return null;

            TypeCode actualType1, actualType2;
            FieldDataType fieldDataType1 = JSONType.GetJSONType(value1.Value, out actualType1);
            FieldDataType fieldDataType2 = JSONType.GetJSONType(value2.Value, out actualType2);
            
            if (fieldDataType1.CompareTo(fieldDataType2) != 0)
                return null;
            
            return Evaluator.PerformArithmeticOperation(value1, actualType1, value2, actualType2, operation,
                fieldDataType1);
        }

        #region IEvaluable members

        public string InString
        {
            get
            {
                string indices = string.Empty;

                if (_indecies != null)
                {
                    foreach (var index in _indecies)
                    {
                        indices += "[" + index + "]";
                    }
                }

                string delimitedName;
                
                if (_name.Contains("$"))
                {
                    delimitedName =  "\"" + _name + "\"";
                }
                else
                {
                    delimitedName = "$" + _name + "$";
                }

                if (_childAttribute == null)
                {
                    return delimitedName + indices;
                }

                return delimitedName + indices + "." + _childAttribute.InString;
            }
        }

        public EvaluationType EvaluationType
        {
            get
            {
                if (_childAttribute != null)
                    return _childAttribute.EvaluationType;

                return EvaluationType.SingleVariable;
            }
        }

        public void UpdateAttribute(IJSONDocument document, IJsonValue newValue)
        {
            if (document.Contains(_name))
            {
                if (_indecies != null && _indecies.Length > 0)
                {
                    ExtendedJSONDataTypes type = document.GetAttributeDataType(_name);
                    if (type == ExtendedJSONDataTypes.Array)
                    {
                        List<IJsonValue> ijsonList = JsonDocumentUtil.ToIJsonList((IEnumerable)document[_name]);
                        if (_indecies != null && _indecies.Length > 0)
                        {
                            if (_childAttribute != null && ijsonList.Count > _indecies[0])
                            {
                                if (ijsonList[_indecies[0]].Value is IJSONDocument)
                                    _childAttribute.UpdateAttribute(ijsonList[_indecies[0]].Value as IJSONDocument, newValue);
                            }
                            else
                            {
                                // Update value in list and replace list in the document.
                                ijsonList[_indecies[0]] = newValue;
                                var values = new ArrayList(ijsonList.Count);
                                foreach (var jsonValue in ijsonList)
                                {
                                    values.Add(jsonValue.Value);
                                }
                                document[_name] = values.ToArray();
                            }
                        }
                        else
                        {
                            foreach (IJsonValue jsondoc in ijsonList)
                            {
                                ((JSONDocument)jsondoc.Value)[_name] = newValue.Value;
                            }
                        }
                    }
                }
                else
                {
                    if (_childAttribute != null)
                    {
                        if (document[_name] is IJSONDocument)
                            _childAttribute.UpdateAttribute(document[_name] as IJSONDocument, newValue);
                        else
                        {
                            ExtendedJSONDataTypes type = document.GetAttributeDataType(_name);
                            if (type == ExtendedJSONDataTypes.Array)
                            {
                                List<IJsonValue> ijsonList = JsonDocumentUtil.ToIJsonList((IEnumerable)document[_name]);
                                foreach(IJsonValue jsondoc in ijsonList)
                                {
                                    _childAttribute.UpdateAttribute((JSONDocument)jsondoc.Value, newValue);
                                }
                            }
                        }
                    }
                    else
                        document[_name] = newValue.Value;
                }
            }
            else
            {
                document[_name] = newValue.Value;
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = null;
            if (document == null) return false;

            if (!document.Contains(_name))
                return false;

            IJsonValue outValue;
            outValue= value = JsonWrapper.Wrap(document[_name]);

            if (_childAttribute == null && _indecies == null && value.DataType != FieldDataType.Array)
            {
                return true;
            }

            if (outValue.DataType != FieldDataType.Array &&
                outValue.DataType != FieldDataType.Object)
            {
                value = null;
                return false;
            }

            if (_indecies != null)
            {
                foreach (int index in _indecies)
                {
                    if (outValue.DataType != FieldDataType.Array)
                    {
                        value = null;
                        return false; 
                    }

                    var values = ((IJsonValue[]) ((ArrayJsonValue)outValue).WrapedValue);

                    if (values.Length - 1 < index)
                    {
                        return false;
                    }

                    outValue = value = values[index];
                }
            }

            if (_childAttribute == null && value.DataType != FieldDataType.Array)
                return true;

            if (outValue.DataType == FieldDataType.Object)
            {
                return _childAttribute.Evaluate(out value, (IJSONDocument)outValue.Value);
            }
            
            EmbeddedList embededValues = new EmbeddedList();

            var arrayValues = ((IJsonValue[])((ArrayJsonValue)outValue).WrapedValue);

            foreach (var arrayValue in arrayValues)
            {
                IJsonValue embededValue;

                if (arrayValue.DataType == FieldDataType.Object)
                {
                    if (_childAttribute == null)
                    {
                        if (value.DataType == FieldDataType.Array)
                            return true;
                        else
                            return false;
                    }
                    //else
                    //{
                    //    return false;
                    //}
                                        
                    if (_childAttribute.Evaluate(out embededValue, (IJSONDocument)arrayValue.Value))
                    {
                        if (outValue is EmbeddedList)
                        {
                            embededValues.AddRange(embededValue as EmbeddedList);
                        }
                        else
                        {
                            embededValues.Add(embededValue);
                        }
                    }
                }
                //else if(arrayValue.DataType != FieldDataType.Array)
                else
                    embededValues.Add(arrayValue);
            }

            value = embededValues;
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Subtraction);
        }

        public IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Multiplication);
        }

        public IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Division);
        }

        public IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Modulus);
        }

        public List<Attribute> Attributes
        {
            get { return new List<Attribute>() { this }; }
        }

        public List<Function> Functions
        {
            get { return new List<Function>(); }
        }

        public void AssignConstants(IList<IParameter> parameters) { }

        #endregion

        public string ParentAttributeName { get { return _name; } }

        public override string ToString()
        {
            
            if (_childAttribute != null)
            {
                return _childAttribute.ToString();
            }

            string indices = string.Empty;

            if (_indecies != null)
            {
                foreach (var index in _indecies)
                {
                    indices += "[" + index + "]";
                }
            }      
            
            return _name + indices;
        }

        public string ToRealString()
        {
            string indices = string.Empty;

            if (_indecies != null)
            {
                foreach (var index in _indecies)
                {
                    indices += "[" + index + "]";
                }
            }
            if (_childAttribute == null)
            {
                return _name + indices;
            }

            return _name + indices + "." + _childAttribute.ToRealString();
        }

        public virtual void Print(System.IO.TextWriter output)
        {
            output.Write("Attribute:{");
            output.Write("Name=");
            output.Write(_name ?? "null");
            output.Write(",");
            output.Write("Indices=");
            if (_indecies != null)
            {
                output.Write("[");
                for (int i = 0; i < _indecies.Length; i++)
                {
                    output.Write(_indecies[i]);
                    if(i!=_indecies.Length-1) output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
            output.Write(",");
            output.Write("Child={" + (_childAttribute != null ? _childAttribute.ToString() : "null"));
            output.Write("}}");
        }

        public string Name
        {
            get { return _name; }
        }

        public int[] Indices
        {
            get { return _indecies; }
        }

        public IAttributeChain Child
        {
            get { return _childAttribute; }
        }
        
        public string CaseSensitiveInString
        {
            get
            {
                if (_childAttribute != null)
                {
                    return _childAttribute.ToString();
                }

                string indices = string.Empty;

                if (_indecies != null)
                {
                    foreach (var index in _indecies)
                    {
                        indices += "[" + index + "]";
                    }
                }

                return _name + indices;
            }
        }
    }
}
