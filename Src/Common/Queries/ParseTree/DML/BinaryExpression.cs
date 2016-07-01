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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class BinaryExpression : IEvaluable
    {
        private ArithmeticOperation _operation;
        private ArithmeticOperation _returnOperation;
        private SortOrder _sortOrder = SortOrder.ASC;
        private IEvaluable _lhs;
        private IEvaluable _rhs;

        public BinaryExpression(IEvaluable lhs,
            ArithmeticOperation operation = ArithmeticOperation.None,
            IEvaluable rhs = null, string alias = null)
        {
            _lhs = lhs;
            _operation = operation;
            _rhs = rhs;
            Alias = alias;
        }

        public ArithmeticOperation Operation
        {
            get { return _operation; }
        }

        public ArithmeticOperation ReturnOperation
        {
            set { _returnOperation = value; }
        }

        public SortOrder SortOrder
        {
            set { _sortOrder = value; }
            get { return _sortOrder; }
        }

        public string Alias { get; set; }

        public void PerformOperation()
        {
            if (EvaluationType.Equals(EvaluationType.Constant))
            {
                //_lhs = ExpressionHelper.GetConstant(Evaluate(null));
                _operation = ArithmeticOperation.None;
                _rhs = null;
            }

            //if (EvaluationType.Equals(EvaluationType.SingleVariable))
            //{
            //    _lhs = AndExpression.GetConstant(Evaluate(null));
            //    _operation = ArithmeticOperation.None;
            //    _rhs = null;
            //}
        }

        private string OperationToString()
        {
            switch (_operation)
            {
                case ArithmeticOperation.Addition:
                    return "+";
                case ArithmeticOperation.Subtraction:
                    return "+";
                case ArithmeticOperation.Multiplication:
                    return "+";
                case ArithmeticOperation.Division:
                    return "+";
                default:
                    return string.Empty;
            }
        }

        private IComparable PerformArithematicOperation(IEvaluable evaluable, IJSONDocument document,
           ArithmeticOperation operation)
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
            {
                return null;
            }

            return Evaluator.PerformArithmeticOperation(value1, actualType1, value2, actualType2, operation,
                fieldDataType1);
        }

        #region IEvaluable members

        public string InString
        {
            get
            {
                string value = _lhs.InString;

                if (_operation != ArithmeticOperation.None)
                {
                    value += OperationToString();
                }

                if (_rhs != null)
                {
                    value += _rhs.InString;
                }

                return value;
            }
        }
        
        public EvaluationType EvaluationType
        {
            get
            {
                if (_rhs == null)
                {
                    return _lhs.EvaluationType;
                }

                if (_lhs.EvaluationType.CompareTo(EvaluationType.Constant) == 0 &&
                    _rhs.EvaluationType.CompareTo(EvaluationType.Constant) == 0)
                {
                    return EvaluationType.Constant;
                }

                if (_lhs.EvaluationType.CompareTo(EvaluationType.SingleVariable) == 0 &&
                    _rhs.EvaluationType.CompareTo(EvaluationType.SingleVariable) == 0)
                {
                    return EvaluationType.MultiVariable;
                }

                if (_lhs.EvaluationType.CompareTo(EvaluationType.AllVariable) == 0 ||
                    _rhs.EvaluationType.CompareTo(EvaluationType.AllVariable) == 0)
                {
                    return EvaluationType.AllVariable;
                }

                if (_lhs.EvaluationType.CompareTo(EvaluationType.MultiVariable) == 0 ||
                    _rhs.EvaluationType.CompareTo(EvaluationType.MultiVariable) == 0)
                {
                    return EvaluationType.MultiVariable;
                }

                return EvaluationType.SingleVariable;
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            //ReturnOperation consideration.

            if (_operation == ArithmeticOperation.None && _rhs == null)
                return _lhs.Evaluate(out value, document);

            IComparable opValue;
            value = null;
            switch (_operation)
            {
                case ArithmeticOperation.Addition:
                    opValue = _lhs.Add(_rhs, document);

                    if (opValue == null)
                    {
                        return false;
                    }

                    value = JsonWrapper.Wrap(opValue);
                    break;

                case ArithmeticOperation.Subtraction:

                    opValue = _lhs.Subtract(_rhs, document);

                    if (opValue == null)
                    {
                        return false;
                    }
                    value = JsonWrapper.Wrap(opValue);
                    break;

                case ArithmeticOperation.Multiplication:

                    opValue = _lhs.Multiply(_rhs, document);

                    if (opValue == null)
                    {
                        return false;
                    }

                    value = JsonWrapper.Wrap(opValue);
                    break;

                case ArithmeticOperation.Division:

                    opValue = _lhs.Divide(_rhs, document);

                    if (opValue == null)
                    {
                        return false;
                    }

                    value = JsonWrapper.Wrap(opValue);
                    break;

                case ArithmeticOperation.Modulus:

                    opValue = _lhs.Modulate(_rhs, document);

                    if (opValue == null)
                    {
                        return false;
                    }

                    value = JsonWrapper.Wrap(opValue);
                    break;
            }
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithematicOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithematicOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithematicOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithematicOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithematicOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public List<Common.JSON.Expressions.Attribute> Attributes
        {
            get
            {
                var list = _lhs.Attributes;
                if (_rhs != null)
                {
                    list.AddRange(_rhs.Attributes);
                }
                return list;
            }
        }
        
        public List<Function> Functions
        {
            get
            {
                List<Function> lhsList = _lhs.Functions;
                if (_rhs != null)
                {
                    lhsList.AddRange(_rhs.Functions);
                }
                return lhsList;
            }
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            if (_rhs != null)
            {
                _rhs.AssignConstants(parameters);
            }
            _lhs.AssignConstants(parameters);
        }

        #endregion
        
        public override string ToString()
        {
            if (_rhs != null)
            {
                return _lhs.ToString() + _operation + _rhs;
            }
            return _lhs.ToString();
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("BinaryExpression:{");
            output.Write("Operation="+_operation.ToString()+",");
            output.Write("ReturnOperation="+_operation.ToString()+",");
            output.Write("SortOrder="+_sortOrder.ToString()+",");
            output.Write("LeftEvaluable=");
            if (_lhs != null)
            {
                _lhs.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",");
            output.Write("RightEvaluable=");
            if (_rhs != null)
            {
                _rhs.Print(output);
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
                string value = _lhs.CaseSensitiveInString;

                if (_operation != ArithmeticOperation.None)
                {
                    value += OperationToString();
                }

                if (_rhs != null)
                {
                    value += _rhs.CaseSensitiveInString;
                }

                return value;
            }
        }
    }
}
