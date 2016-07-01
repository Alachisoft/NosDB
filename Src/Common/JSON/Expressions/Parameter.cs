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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class Parameter : IComparable, IEvaluable
    {
        private readonly string _name;
        //private IComparable _value; //<-- if all IJSONValues implement ToString() then there is no need of this.
        private IJsonValue _jsonValue;
        private ArithmeticOperation _returnOperation = ArithmeticOperation.None;

        public Parameter(string name, IComparable value = null)
        {
            _name = name;
            //_value = value;
        }

        public ArithmeticOperation ArithmeticOperation
        {
            set { _returnOperation = value; }
        }

        public override string ToString()
        {
            return _jsonValue == null ? _name : _jsonValue.ToString();
        }

        private IComparable PerformArithmeticOperation(IEvaluable evaluable, IJSONDocument document, ArithmeticOperation operation)
        {
            IJsonValue value1, value2;
            if (!Evaluate(out value1, document))
            {
                return null;
            }
            if (!evaluable.Evaluate(out value2, document))
            {
                return null;
            }
            TypeCode actualType1, actualType2;
            FieldDataType fieldDataType1 = JSONType.GetJSONType(value1.Value, out actualType1);
            FieldDataType fieldDataType2 = JSONType.GetJSONType(value2.Value, out actualType2);
            if (fieldDataType1.CompareTo(fieldDataType2) != 0)
                return null;
            return Evaluator.PerformArithmeticOperation(value1, actualType1, value2, actualType2, operation,
                fieldDataType1);
        }

        #region IComparable members

        public int CompareTo(object obj)
        {
            if(_jsonValue is ArrayJsonValue || _jsonValue is ObjectJsonValue)
                return 1;
            else if(obj is ArrayJsonValue || _jsonValue is ObjectJsonValue)
                return -1;

            if (_jsonValue is NullValue && obj is NullValue)
                return 0;

            if (_jsonValue is NullValue)
                return 1;

            if (obj is NullValue)
                return -1;

            if (obj is ConstantValue)
            {
                var other = (ConstantValue)obj;
                return other.CompareTo(_jsonValue.Value) > 0 ? other.CompareTo(_jsonValue.Value) : other.CompareTo(_jsonValue.Value) * -1;
            }
            return _jsonValue.CompareTo(obj);
        }

        #endregion

        #region IEvaluable members

        public string InString
        {
            get { return "@" + _name; }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = _jsonValue;
            return true; 
        }

        public EvaluationType EvaluationType { get { return EvaluationType.Constant; } }

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
            get { return new List<Attribute>(); }
        }

        public System.Collections.Generic.List<Function> Functions
        {
            get { return new List<Function>(); }
        }

        //ArithmeticOperation consideration.
        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Name.Equals(_name))
                {
                    if(parameter.Value is string && 
                        (((string)parameter.Value).Contains("%") || ((string)parameter.Value).Contains("[") ||
                        ((string)parameter.Value).Contains("]") || ((string)parameter.Value).Contains("_")))
                    {
                        StringConstantValue newValue = new StringConstantValue((string)parameter.Value);
                        newValue.PossibleWildCard = true;
                        _jsonValue = newValue;
                    }
                    else
                    {                             
                        _jsonValue = JsonWrapper.Wrap(parameter.Value);
                    }
                }
            }

            if (_jsonValue == null)
            {
                throw new QuerySystemException(ErrorCodes.Query.UNASSIGNED_QUERY_PARAMETER, new[] { _name });
            }

        }

        #endregion

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Parameter:{");
            output.Write("Name="+_name??"null");
            //output.Write(",Value=" + (_value != null ? _value.ToString() : "null"));
            output.Write(",JsonValue="+(_jsonValue!=null?_jsonValue.ToString():"null"));
            output.Write(",ReturnOperation="+_returnOperation.ToString());
            output.Write("}");
        }


        public string CaseSensitiveInString
        {
            get { return "@" + _name; }
        }
    }
}
