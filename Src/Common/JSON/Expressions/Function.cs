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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class Function : IEvaluable
    {
        private readonly string _name;
        private readonly bool _distinct;
        private List<IEvaluable> _arguments;
        private FunctionExecutionType _executionType = FunctionExecutionType.Unsepcified;
        private ArithmeticOperation _returnOperation = ArithmeticOperation.None;
        private IFunction _funcImpl;

        public Function(string name, bool distinct = false, List<IEvaluable> arguments = null)
        {
            _arguments = new List<IEvaluable>();
            _name = name;            
            _distinct = distinct;

            if (arguments != null)
            {
                _arguments.AddRange(arguments);
            }
        }

        public bool Distinct
        {
            get { return _distinct; }
        }

        public FunctionExecutionType ExecutionType
        {
            set { _executionType = value; }
        }

        public IFunction ExecutionInstance
        {
            set
            {
                _funcImpl = value;
                _executionType = FunctionExecutionType.Scalar;
            }
        }

        public ArithmeticOperation ArithmeticOperation
        {
            set { _returnOperation = value; }
        }

        public List<IEvaluable> Arguments { get  {return _arguments; } }

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

        #region IEvaluable members

        public string InString
        {
            get
            {
                string methodName = "";
                methodName += _name.ToLower() + "(";
                for (int i = 0; i < _arguments.Count; i++)
                {
                    methodName += _arguments[i].InString;
                    if (i != _arguments.Count - 1)
                    {
                        methodName += ", ";
                    }
                }
                return methodName + ")";
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            //ArithmeticOperation consideration...
            value = null;
            object functionResult = null;
            if (_executionType.Equals(FunctionExecutionType.Scalar))
            {
                if (_funcImpl == null)
                {
                    return false;
                }

                var values = new List<IJsonValue>();
                foreach (var argument in _arguments)
                {
                    IJsonValue result;
                    if(!argument.Evaluate(out result, document))
                        return false;
                    values.Add(result);
                }

                if (_arguments.Count != values.Count)
                    return false;

                var arguments = new object[values.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    arguments[i] = JsonWrapper.UnWrap(values[i]);
                }
                try
                {
                    functionResult = _funcImpl.Execute(FunctionName, arguments);

                }
                catch (System.Reflection.TargetParameterCountException)
                {
                    throw;
                }
                catch (System.ArgumentException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    if (LoggerManager.Instance.QueryLogger != null && LoggerManager.Instance.QueryLogger.IsErrorEnabled)
                        LoggerManager.Instance.QueryLogger.Error("UDFs", exception.Message);
                    throw;
                }
                value = JsonWrapper.Wrap(functionResult);
                return true;
            }

            if(_executionType.Equals(FunctionExecutionType.Aggregate))
            {
                string evaluation = "$" + ToString();
                object outValue;
                value = JsonWrapper.Wrap(!document.TryGet(evaluation, out outValue) ? 0 : outValue);
                return true;
            }

            return false;
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

        public EvaluationType EvaluationType
        {      
            get 
            {
                return Enum.EvaluationType.AllVariable;
                //bool isMuilti = false, isSingle = false;
                //foreach(var argument in _arguments)
                //{
                //    if(argument.EvaluationType == Enum.EvaluationType.AllVariable)
                //    {
                //        return Enum.EvaluationType.AllVariable;
                //    }

                //    if(!isMuilti && argument.EvaluationType == Enum.EvaluationType.MultiVariable)
                //    {
                //        isMuilti = true;
                //    }

                //    if(!isSingle && argument.EvaluationType == Enum.EvaluationType.SingleVariable)
                //    {
                //        isSingle = true;
                //    }
                //}

                //if(isMuilti)
                //{
                //    return Enum.EvaluationType.MultiVariable;
                //}

                //if(isSingle)
                //{
                //    return Enum.EvaluationType.SingleVariable;
                //}           
                
                //return Enum.EvaluationType.Constant; 
                //return Enum.EvaluationType.AllVariable;
            }
        }

        public List<Attribute> Attributes
        {
            //For where clause... there is no need for it in projection expressions.
            get { return new List<Attribute>(); }
        }

        public List<Function> Functions
        {
            get
            {
                var list = new List<Function>(){this};
                list.AddRange(ArgumentFunctions);
                return list;
            }
        }

        public List<Function> ArgumentFunctions
        {
            get
            {
                var list = new List<Function>();
                foreach (var argument in _arguments)
                    list.AddRange(argument.Functions);
                return list;
            }
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var argument in _arguments)
                argument.AssignConstants(parameters);
        }
 
        #endregion

        public override string ToString()
        {
            string methodName = "";
            methodName += _name.ToLower() + "(";
            for (int i = 0; i < _arguments.Count; i++)
            {
                methodName += _arguments[i].ToString();
                if (i != _arguments.Count - 1)
                {
                    methodName += ", ";
                }
            }
            return methodName + ")";
        }

        public string FunctionName
        {
            get { return _name.ToLower(); }
        }

        public string FunctionNameActual
        {
            get { return _name; }
        }

        public string GetCaseSensitiveName()
        {
            string methodName = "";
            methodName += _name + "(";
            for (int i = 0; i < _arguments.Count; i++)
            {
                methodName += _arguments[i].ToString();
                if (i == _arguments.Count - 1)
                {
                    methodName += ")";
                }
                else
                {
                    methodName += ", ";
                }
            }
            return methodName;
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Function:{");
            output.Write("Name=" + (_name ?? "null"));
            output.Write(",Distinct=" + _distinct);
            output.Write(",ExecutionType=" + _executionType);
            output.Write(",ReturnOperation=" + _returnOperation);
            output.Write(",FunctionImplementation=" + (_funcImpl != null ? _funcImpl.GetType().Name : "null"));
            output.Write("}");
        }


        public string CaseSensitiveInString
        {
            get
            {
                string methodName = "";
                methodName += _name + "(";
                for (int i = 0; i < _arguments.Count; i++)
                {
                    methodName += _arguments[i].CaseSensitiveInString;
                    if (i != _arguments.Count - 1)
                    {
                        methodName += ", ";
                    }
                }
                return methodName + ")";
            }
        }
    }
}
