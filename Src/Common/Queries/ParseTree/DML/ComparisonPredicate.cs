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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Filters.Scalars;
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Queries.Util;
using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class ComparisonPredicate : ITreePredicate
    {
        private IEvaluable _leftEvaluable;
        private IEvaluable _middleEvaluable;
        private IEvaluable _rightEvaluable;
        private Condition _condition = Condition.None;
        private bool _isNot;
        private bool _isExpression;

        public ComparisonPredicate(IEvaluable leftEvaluable,
            Condition condition, IEvaluable middleEvaluable = null,
            IEvaluable rightEvaluable = null)
        {
            _leftEvaluable = leftEvaluable;
            _middleEvaluable = middleEvaluable;
            _rightEvaluable = rightEvaluable;
            _condition = condition;
            _isExpression = _leftEvaluable is BinaryExpression;
        }

        private void InvertCondition()
        {
            switch (_condition)
            {
                case Condition.Like: _condition = Condition.NotLike; break;
                case Condition.Equals: _condition = Condition.NotEquals; break;
                case Condition.NotEquals: _condition = Condition.Equals; break;
                case Condition.NotLike: _condition = Condition.Like; break;
                case Condition.GreaterThan: _condition = Condition.LesserThanEqualTo; break;
                case Condition.GreaterThanEqualTo: _condition = Condition.LesserThan; break;
                case Condition.LesserThan: _condition = Condition.GreaterThanEqualTo; break;
                case Condition.LesserThanEqualTo: _condition = Condition.GreaterThan; break;
                case Condition.In: _condition = Condition.NotIn; break;
                case Condition.NotIn: _condition = Condition.In; break;
                case Condition.Between: _condition = Condition.NotBetween; break;
                case Condition.NotBetween: _condition = Condition.Between; break;
                case Condition.ContainsAny: _condition = Condition.NotContainsAny; break;
                case Condition.NotContainsAny: _condition = Condition.ContainsAny; break;
                case Condition.ContainsAll: _condition = Condition.NotContainsAll; break;
                case Condition.NotContainsAll: _condition = Condition.ContainsAll; break;
                case Condition.ArraySize: _condition = Condition.NotArraySize; break;
                case Condition.NotArraySize: _condition = Condition.ArraySize; break;
                case Condition.Exists: _condition = Condition.NotExists; break;
                case Condition.NotExists: _condition = Condition.Exists; break;
                case Condition.IsNull: _condition = Condition.IsNotNull; break;
                case Condition.IsNotNull: _condition = Condition.IsNull; break;
            }
        }

        private string ConditionString()
        {
            switch (_condition)
            {
                case Condition.Like: return " LIKE ";
                case Condition.Equals: return " = ";
                case Condition.NotEquals: return " != ";
                case Condition.NotLike: return " NOT LIKE ";
                case Condition.GreaterThan: return " > ";
                case Condition.GreaterThanEqualTo: return " >= ";
                case Condition.LesserThan: return " < ";
                case Condition.LesserThanEqualTo: return " <= ";
                case Condition.In: return " IN ";
                case Condition.NotIn: return " NOT IN ";
                case Condition.Between: return " BETWEEN ";
                case Condition.NotBetween: return " NOT BETWEEN ";
                case Condition.ContainsAny: return " CONTAINS ANY ";
                case Condition.NotContainsAny: return " NOT CONTAINS ANY ";
                case Condition.ContainsAll: return " CONTAINS ALL ";
                case Condition.NotContainsAll: return " NOT CONTAINS ALL ";
                case Condition.ArraySize: return " ARRAY SIZE ";
                case Condition.NotArraySize: return " NOT ARRAY SIZE ";
                case Condition.Exists: return " EXISTS ";
                case Condition.NotExists: return " NOT EXISTS ";
                case Condition.IsNull: return " IS NULL ";
                case Condition.IsNotNull: return " IS NOT NULL ";
            }
            return string.Empty;
        }

        public bool IsNot
        {
            set
            {
                if (_isNot != value)
                    InvertCondition();

                _isNot = value;
            }
        }

        public string InString
        {
            get
            {
                string value = "(" +_leftEvaluable.InString + ConditionString();

                if (_middleEvaluable != null)
                {
                    value += _middleEvaluable.InString;
                }

                if (_rightEvaluable != null)
                {
                    value += " AND " + _rightEvaluable.InString;
                }

                return value + ")";
            }
        }

        public bool IsBinaryExpression
        {
            get { return _isExpression; }
        }

        public PredicateType PredicateType
        {
            get
            {
                if (_leftEvaluable.EvaluationType.Equals(EvaluationType.Constant) &&
                    (_middleEvaluable == null || _middleEvaluable.EvaluationType.Equals(EvaluationType.Constant)) &&
                    (_rightEvaluable == null || _rightEvaluable.EvaluationType.Equals(EvaluationType.Constant)))
                {
                    return PredicateType.Constant;
                }

                if (_leftEvaluable.EvaluationType.Equals(EvaluationType.AllVariable) ||
                    (_middleEvaluable != null && _middleEvaluable.EvaluationType.Equals(EvaluationType.AllVariable)) ||
                    (_rightEvaluable != null && _rightEvaluable.EvaluationType.Equals(EvaluationType.AllVariable)))
                {
                    return PredicateType.AllVariable;
                }

                if (_leftEvaluable.EvaluationType.Equals(EvaluationType.MultiVariable) ||
                    (_middleEvaluable != null && _middleEvaluable.EvaluationType.Equals(EvaluationType.MultiVariable)) ||
                    (_rightEvaluable != null && _rightEvaluable.EvaluationType.Equals(EvaluationType.MultiVariable)))
                {
                    return PredicateType.MultiVariable;
                }

                //If _rightEvaluable is not null, then _middleEvaluable will always be a non-null value.
                if (_rightEvaluable != null)
                {
                    if (_leftEvaluable.EvaluationType.Equals(EvaluationType.SingleVariable) &&
                        _middleEvaluable.EvaluationType.Equals(EvaluationType.SingleVariable) &&
                        _middleEvaluable.EvaluationType.Equals(EvaluationType.SingleVariable))
                    {
                        return PredicateType.MultiVariable;
                    }
                }

                if (_middleEvaluable != null)
                {
                    if (_leftEvaluable.EvaluationType.Equals(EvaluationType.SingleVariable) &&
                        _middleEvaluable.EvaluationType.Equals(EvaluationType.SingleVariable))
                    {
                        return PredicateType.MultiVariable;
                    }
                }

                return PredicateType.SingleVariable;
            }            
        }

        public List<Attribute> Attributes
        {
            get
            {
                var list = _leftEvaluable.Attributes;

                if (_middleEvaluable != null)
                {
                    list.AddRange(_middleEvaluable.Attributes);
                }
                if (_rightEvaluable != null)
                {
                    list.AddRange(_rightEvaluable.Attributes);
                }
                return list;
            }
        }

        public List<string> AttributeNames
        {
            get
            {
                return Attributes.Select(attribute => attribute.ToString()).ToList();
            }
        }

        public List<IJsonValue> ConstantValues
        {
            get
            {
                IJsonValue value;
                var values = new List<IJsonValue>();
                if (_leftEvaluable.EvaluationType == EvaluationType.Constant)
                {
                    if (_leftEvaluable.Evaluate(out value, null))
                        values.Add(value);
                }
                if (_middleEvaluable != null && _middleEvaluable.EvaluationType.Equals(EvaluationType.Constant))
                {
                    if (_middleEvaluable.Evaluate(out value, null))
                        values.Add(value);
                }
                if (_rightEvaluable != null && _rightEvaluable.EvaluationType.Equals(EvaluationType.Constant))
                {
                    if (_rightEvaluable.Evaluate(out value, null))
                        values.Add(value);
                }
                return values;
            }
        }

        public Condition Condition
        {
            get { return _condition; }
        }

        //ax + b = 0 => x = -b/a
        public void SolveLinearExpression()
        {
            if (!PredicateType.Equals(PredicateType.SingleVariable))
                return;

            if (_leftEvaluable.EvaluationType.Equals(EvaluationType.Constant)
                && _leftEvaluable is BinaryExpression)
            {
                //_leftEvaluable = ExpressionHelper.GetConstant(_middleEvaluable.Evaluate(null));
            }

            if (_middleEvaluable != null &&
                _middleEvaluable.EvaluationType.Equals(EvaluationType.Constant) &&
                _middleEvaluable is BinaryExpression)
            {
               // _middleEvaluable = ExpressionHelper.GetConstant(_middleEvaluable.Evaluate(null));
            }

            if (_rightEvaluable != null &&
                _rightEvaluable.EvaluationType.Equals(EvaluationType.Constant) &&
                _rightEvaluable is BinaryExpression)
            {
                //_rightEvaluable = ExpressionHelper.GetConstant(_rightEvaluable.Evaluate(null));
            }
        }

        public IPredicate AssignIndexPredicate(IIndex index, IQueryStore queryStore)
        {
            bool singleIndex = true;
            IPredicate predicate = null;
            List<AttributeValue> values;
            IJsonValue value = ConstantValues.Count > 0 ? ConstantValues[0] : null;

            if (value == null && (_condition != Condition.Exists))
            {
                return new StorePredicate(this, queryStore);
            }
            
            switch (_condition)
            {
                case Condition.Equals:
                case Condition.Like:
                    if (value.DataType.Equals(FieldDataType.Array))
                    {
                        predicate = new ArrayEqualsPredicate(index,
                            GetAttributeValues(value, singleIndex));
                    }
                    else
                    {
                        predicate = new EqualsPredicate(index,
                            GetAttributeValue(value, false, false, singleIndex));
                    }
                    break;

                case Condition.NotEquals:
                case Condition.NotLike:
                    if (value.DataType.Equals(FieldDataType.Array))
                    {
                        predicate = new ArrayEqualsPredicate(index, 
                            GetAttributeValues(value, singleIndex), true);
                    }
                    else
                    {
                        predicate = new EqualsPredicate(index,
                            GetAttributeValue(value, false, false, singleIndex), true);
                    }
                    break;

                // If JSON array, then use only one value for comparison... else return a terminal predicate.
                case Condition.GreaterThan:

                    if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                    {
                        return new EmptyPredicate();
                    }

                    predicate = new GreaterPredicate(index,
                        GetAttributeValue(value, value.DataType.Equals(FieldDataType.Array),
                            true, singleIndex));
                    break;

                // If JSON array, then use only one value for comparison... else return a terminal predicate.
                case Condition.GreaterThanEqualTo:

                    if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                    //Exception!!!!
                    if (value.DataType.Equals(FieldDataType.Array) && ((ArrayJsonValue)value.Value).Length > 1)
                    {
                        return new EmptyPredicate();
                    }

                    predicate = new LesserPredicate(index,
                        GetAttributeValue(value, value.DataType.Equals(FieldDataType.Array),
                            true, singleIndex), true);
                    break;

                // If JSON array, then use only one value for comparison... else return a terminal predicate.
                case Condition.LesserThan:

                    if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                    {
                        return new EmptyPredicate();
                    }

                    predicate = new LesserPredicate(index,
                        GetAttributeValue(value, value.DataType.Equals(FieldDataType.Array),
                            true, singleIndex));
                    break;

                // If JSON array, then use only one value for comparison... else return a terminal predicate.
                case Condition.LesserThanEqualTo:

                    if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                    {
                        return new EmptyPredicate();
                    }

                     predicate = new GreaterPredicate(index,
                        GetAttributeValue(value, value.DataType.Equals(FieldDataType.Array),
                            true, singleIndex), true);
                    break;

                // If JSON array, then use only one value for comparison... else return a terminal predicate.
                //IN's handling: the left hand side will always be a single value, where the right hand side will always
                //be a value list, we as an element of the array also support DateAndTime and Scalar-functions (UDFs/Built-ins).
                case Condition.In:
                    //ValueList of Constant evaluables will be sent here... they don't need a document to get evaluated.
                    //if (!(value is ValueList))
                    //{
                    //    throw new QuerySystemException(ErrorCodes.Query.INVALID_IN_OPERATOR_ARGUMENTS);
                    //}

                    values = new List<AttributeValue>();

                    IJsonValue outValue;
                    foreach (var evaluable in ((ValueList)value).Values)
                    {
                        if (evaluable.Evaluate(out outValue, null))
                        {
                            values.Add(GetAttributeValue((IComparable)outValue.Value,
                                singleIndex));
                        }
                    }
                    
                    predicate = new INPredicate(index, values.ToArray());
                    break;

                //IN's handling: the left hand side will always be a single value, where the right hand side will always
                //be a value list, we as an element of the array also support DateAndTime and Scalar-functions (UDFs/Built-ins).
                case Condition.NotIn:
                    //ValueList of Constant evaluables will be sent here... they don't need a document to get evaluated.
                    //if (!(value is ValueList))
                    //{
                    //    throw new QuerySystemException(ErrorCodes.Query.INVALID_IN_OPERATOR_ARGUMENTS);
                    //}

                    values = new List<AttributeValue>();
                    
                    foreach (var evaluable in ((ValueList)value).Values)
                    {
                        if (evaluable.Evaluate(out outValue, null))
                            values.Add(GetAttributeValue((IComparable)outValue.Value, singleIndex));
                    }

                    predicate = new INPredicate(index, values.ToArray(), true);
                    break;

                //TSQL values' check... X BETWEEN Y AND Z :: Y should be always less than Z
                case Condition.Between:

                    IJsonValue value2 = ConstantValues[1];

                    if (value.CompareTo(value2) > 0)
                    {
                        throw new QuerySystemException(ErrorCodes.Query.INVALID_BETWEEN_OPERATOR_ARGUMENTS);
                    }

                    //If both values are =
                    if (value.Equals(value2))
                    {
                        if (value.DataType.Equals(FieldDataType.Array))
                        {
                            if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            if (value2.DataType.Equals(FieldDataType.Array) && ((object[])value2.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            predicate = new ArrayAnyPredicate(index, GetAttributeValues(value, singleIndex));
                        }
                        else
                        {
                            predicate = new EqualsPredicate(index,
                                GetAttributeValue(value2, false, false,singleIndex));
                        }
                    }
                    else
                    {
                        if (value.DataType.Equals(FieldDataType.Array))
                        {
                            if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            if (value2.DataType.Equals(FieldDataType.Array) && ((object[])value2.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            predicate = new ArrayRangePredicate(index,
                                GetAttributeValue(value, true, true, singleIndex),
                                false,
                                GetAttributeValue(value2, true, false, singleIndex),
                                false, false);
                        }
                        else
                        {
                            predicate = new RangePredicate(index,
                                GetAttributeValue(value, false, false, singleIndex),
                                false,
                                GetAttributeValue(value2, false, false, singleIndex),
                                false, false);
                        }
                    }
                    break;

                //TSQL values' check... X BETWEEN Y AND Z :: Y should be always less than Z
                case Condition.NotBetween:

                    value2 = ConstantValues[1];

                    if (value.CompareTo(value2) > 0)
                    {
                        throw new QuerySystemException(ErrorCodes.Query.INVALID_BETWEEN_OPERATOR_ARGUMENTS);
                    }

                    //If both values are =
                    if (value.Equals(value2))
                    {
                        if (value.DataType.Equals(FieldDataType.Array))
                        {
                            if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            if (value2.DataType.Equals(FieldDataType.Array) && ((object[])value2.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            predicate = new ArrayAnyPredicate(index,
                                GetAttributeValues(value, singleIndex), true);
                        }
                        else
                        {
                            predicate = new EqualsPredicate(index,
                                GetAttributeValue(value2, false, false,
                                singleIndex), true);
                        }
                    }
                    else
                    {
                        if (value.DataType.Equals(FieldDataType.Array))
                        {
                            if (value.DataType.Equals(FieldDataType.Array) && ((object[])value.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            if (value2.DataType.Equals(FieldDataType.Array) && ((object[])value2.Value).Length > 1)
                            {
                                return new EmptyPredicate();
                            }

                            predicate = new ArrayRangePredicate(index,
                                GetAttributeValue(value, true, true, singleIndex),
                                true,
                                GetAttributeValue(value2, true, false, singleIndex),
                                true, true);
                        }
                        else
                        {
                            predicate = new RangePredicate(index,
                                GetAttributeValue(value, false, false, singleIndex),
                                true,
                                GetAttributeValue(value2, false, false, singleIndex),
                                true, true);
                        }
                    }
                    break;

                case Condition.ContainsAny:
                    predicate = new ArrayAnyPredicate(index, 
                        GetAttributeValues(value, singleIndex));
                    break;

                case Condition.NotContainsAny:
                    predicate = new ArrayAnyPredicate(index,
                        GetAttributeValues(value, singleIndex), true);
                    break;

                case Condition.ContainsAll:
                    predicate = new ArrayAllPredicate(index, 
                        GetAttributeValues(value, singleIndex));
                    break;

                case Condition.NotContainsAll:
                    predicate = new ArrayAllPredicate(index,
                        GetAttributeValues(value, singleIndex), true);
                    break;

                case Condition.Exists:
                    predicate = new AllPredicate(index);
                    break;

                case Condition.IsNull:
                    AttributeValue nullValue = new SingleAttributeValue(NullValue.Null);
                    if (!singleIndex)
                    {
                        nullValue = new MultiAttributeValue(NullValue.Null);
                    }
                    predicate = new EqualsPredicate(index, nullValue);
                    break;

                case Condition.IsNotNull:
                    nullValue = new SingleAttributeValue(NullValue.Null);
                    if (!singleIndex)
                    {
                        nullValue = new MultiAttributeValue(NullValue.Null);
                    }
                    predicate = new EqualsPredicate(index, nullValue, true);
                    break;
            }
            return predicate;
        }

        private bool CheckValuesEquality(IEvaluable leftEvaluable, IEvaluable middleEvaluable, IEvaluable rightEvaluable)
        {
            if ((_rightEvaluable != null && rightEvaluable == null) || 
                (_rightEvaluable == null && rightEvaluable != null))
                return false;

            if (_rightEvaluable != null && rightEvaluable != null
                && !_rightEvaluable.ToString().Equals(rightEvaluable.ToString()))
                return false;

            if (_rightEvaluable != null && (_middleEvaluable == null || middleEvaluable == null))
                return false;

            if (_middleEvaluable != null && middleEvaluable == null || 
                _middleEvaluable == null && middleEvaluable != null)
                return false;

            if (!_leftEvaluable.ToString().Equals(leftEvaluable.ToString()) && _middleEvaluable != null 
                && !_middleEvaluable.ToString().Equals(leftEvaluable.ToString()))
                return false;

            if( !(_middleEvaluable != null && middleEvaluable != null &&
                (_middleEvaluable.ToString().Equals(middleEvaluable.ToString()) ||
                _leftEvaluable.ToString().Equals(middleEvaluable.ToString()))))
                return false;
            
            return true;
        }

        #region ITreePredicate members

        public bool Completed { get; set; }

        public bool IsTerminal { get{ return true; }}

        public bool HasOr { get { return false; } }

        public List<ITreePredicate> AtomicTreePredicates
        {
            get
            {
                var predciates = new List<ITreePredicate>() { this };
                return predciates;
            }
        }

        public ITreePredicate Contract()
        {
            return null;
        }

        public ITreePredicate Expand()
        {
            return null;
        }
        
        public bool IsTrue(IJSONDocument entry)
        {
            IJsonValue value1, value2 = null, value3 = null;

            if (!_leftEvaluable.Evaluate(out value1, entry))
            {
                if (_condition == Condition.NotExists)
                {
                    return true;
                }
                return false;
            }

            if (_middleEvaluable != null && !_middleEvaluable.Evaluate(out value2, entry))
            {
                return false;
            }

            if (_rightEvaluable != null && !_rightEvaluable.Evaluate(out value3, entry))
            {
                return false;
            }

            if (InValidCompareCheck(value1, value2, value3, _condition))
            {
                return false;
            }

            switch (_condition)
            {
                case Condition.Equals:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return EmbeddedHelper.Equals(value1, value2);
                    }
                    return value1.Equals(value2);

                case Condition.NotEquals:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return !EmbeddedHelper.Equals(value1, value2);
                    }
                    return !value1.Equals(value2);

                case Condition.Like:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        if (value2 is StringConstantValue)
                        {
                            return EmbeddedHelper.Equals(value1, value2, true);
                        }
                        return EmbeddedHelper.Equals(value1, value2);
                    }
                    
                    if(!(value2 is StringConstantValue))
                        return value1.Equals(value2);
                    
                    if (value1.Value is string)
                    {
                        return ((StringConstantValue)value2).WildCompare((string)value1.Value);
                    }
                    return false;

                case Condition.NotLike:
                     if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        if (value2 is StringConstantValue)
                        {
                            return !EmbeddedHelper.Equals(value1, value2, true);
                        }
                        return !EmbeddedHelper.Equals(value1, value2);
                    }
                    
                    if(!(value2 is StringConstantValue))
                        return !value1.Equals(value2);
                    
                    if (value1.Value is string)
                    {
                        return !((StringConstantValue)value2).WildCompare((string)value1.Value);
                    }
                    return false;

                case Condition.GreaterThan:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return EmbeddedHelper.Compare(value1, value2, false) > 0;
                    }
                    return value1.CompareTo(value2) > 0;

                case Condition.GreaterThanEqualTo:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return EmbeddedHelper.Compare(value1, value2, false) >= 0;
                    }
                    return value1.CompareTo(value2) >= 0;
                    
                case Condition.LesserThan:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return EmbeddedHelper.Compare(value1, value2) < 0;
                    }
                    return value1.CompareTo(value2) < 0;

                case Condition.LesserThanEqualTo:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return EmbeddedHelper.Compare(value1, value2) <= 0;
                    }
                    return value1.CompareTo(value2) <= 0;

                //IN's handling: the right hand side will always be a value list or ArrayJsonValue, we as an element of the array (in ValueList) 
                //also support DateAndTime and Scalar-functions (UDFs/Built-ins).
                case Condition.In:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return EmbeddedHelper.Contains(value1, value2, entry);
                    }

                    if (value2 is ValueList)
                    {
                        return ((ValueList) value2).Contains(entry, value1);
                    }
                    if (value2 is ArrayJsonValue)
                    {
                        return ((ArrayJsonValue)value2).Contains(value1);
                    }
                    return false;

                case Condition.NotIn:
                    if (value1 is EmbeddedList || value2 is EmbeddedList)
                    {
                        return !EmbeddedHelper.Contains(value1, value2, entry);
                    }

                    if (value2 is ValueList)
                    {
                        return !((ValueList)value2).Contains(entry, value1);
                    }
                    if (value2 is ArrayJsonValue)
                    {
                        return !((ArrayJsonValue)value2).Contains(value1);
                    }
                    return true;

                case Condition.ContainsAny:
                    if (value1 is EmbeddedList)
                    {
                        return EmbeddedHelper.ArrayContainsAny(value1,
                            value2 as ValueList, entry);
                    }

                    if (value1.DataType.Equals(FieldDataType.Array) && value2 is ValueList)
                    {
                        ArrayJsonValue jsonArray = ((ArrayJsonValue) value1);
                        foreach (var evaluable in ((ValueList)value2))
                        {
                            IJsonValue value;
                            if (!evaluable.Evaluate(out value, entry))
                            {
                                return false;
                            }
                            if (jsonArray.Contains(value))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
               
                case Condition.NotContainsAny:
                    if (value1 is EmbeddedList)
                    {
                        return !EmbeddedHelper.ArrayContainsAny(value1, 
                            value2 as ValueList, entry);
                    }
                    
                    if (value1.DataType.Equals(FieldDataType.Array) && value2 is ValueList)
                    {
                        ArrayJsonValue jsonArray = ((ArrayJsonValue) value1);
                        foreach (var evaluable in ((ValueList)value2))
                        {
                            IJsonValue value;
                            if (!evaluable.Evaluate(out value, entry))
                            {
                                return false;
                            }
                            if (jsonArray.Contains(value))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    return false;
                    
                case Condition.ContainsAll:
                    if (value1 is EmbeddedList)
                    {
                        return EmbeddedHelper.ArrayContainsAll(value1,
                            value2 as ValueList, entry);
                    }

                    if (value1.DataType.Equals(FieldDataType.Array) && value2 is ValueList)
                    {
                        ArrayJsonValue jsonArray = ((ArrayJsonValue) value1);
                        foreach (var evaluable in ((ValueList)value2))
                        {
                            IJsonValue value;
                            if (!evaluable.Evaluate(out value, entry))
                            {
                                return false;
                            }

                            if (!jsonArray.Contains(value))
                                return false;
                        }
                        return true;
                    }
                    return false;

                case Condition.NotContainsAll:
                    if (value1 is EmbeddedList)
                    {
                        return !EmbeddedHelper.ArrayContainsAll(value1,
                            value2 as ValueList, entry);
                    }

                    if (value1.DataType.Equals(FieldDataType.Array) && value2 is ValueList)
                    {
                        ArrayJsonValue jsonArray = ((ArrayJsonValue) value1);
                        foreach (var evaluable in ((ValueList)value2))
                        {
                            IJsonValue value;
                            if (!evaluable.Evaluate(out value, entry))
                            {
                                return false;
                            }

                            if (jsonArray.Contains(value))
                                return false;
                        }
                        return true;
                    }
                    
                    return false;

                case Condition.ArraySize:
                    if (value1 is EmbeddedList)
                    {
                        return EmbeddedHelper.ArraySize(value1, (long)value2.Value);
                    }
                    if (value1.DataType.Equals(FieldDataType.Array))
                    {
                        return ((IComparable)value2.Value).CompareTo((long)((ArrayJsonValue)value1).Length) == 0;
                    }
                    return false;

                case Condition.NotArraySize:
                    if (value1 is EmbeddedList)
                    {
                        return !EmbeddedHelper.ArraySize(value1, (long)value2.Value);
                    }
                    if (value1.DataType.Equals(FieldDataType.Array))
                    {
                        return ((IComparable)value2.Value).CompareTo((long)((ArrayJsonValue)value1).Length) != 0;
                    }
                    return false;

                case Condition.Between:
                    if (value1 is EmbeddedList || value2 is EmbeddedList || value3 is EmbeddedList)
                    {
                        return EmbeddedHelper.Between(value1, value2, value3);
                    }
                    return value1.CompareTo(value2) >= 0 && value1.CompareTo(value3) <= 0;

                case Condition.NotBetween:
                    if (value1 is EmbeddedList || value2 is EmbeddedList || value3 is EmbeddedList)
                    {
                        return EmbeddedHelper.Between(value1, value2, value3);
                    }
                    return value1.CompareTo(value2) < 0 || value1.CompareTo(value3) > 0;

                case Condition.Exists:
                    IJsonValue existValue;
                    if (_leftEvaluable.Evaluate(out existValue, entry))
                    {
                        return true;
                    }
                    return false;

                case Condition.IsNull:
                    if (value1 is EmbeddedList)
                    {
                        return EmbeddedHelper.Equals(value1, NullValue.Null);
                    }
                    if (value1 is NullValue)
                    {
                        return true;
                    }
                    return false;

                case Condition.IsNotNull:
                    if (value1 is EmbeddedList)
                    {
                        return !EmbeddedHelper.Equals(value1, NullValue.Null);
                    }
                    if (!(value1 is NullValue))
                    {
                        return true;
                    }
                    return false;
            }
            return false;        
        }

        public bool TryGetProxyKeyPredicate(IEnumerable<long> rowsEnumerator, out DocumentKey comparisonKey)
        {
            if (Attributes.Count > 0 && _condition == Condition.Equals)
            {
                if (Attributes[0].ToRealString().Equals(JsonDocumentUtil.DocumentKeyAttribute))
                {
                    StringConstantValue stringValue = _middleEvaluable as StringConstantValue;
                    if (stringValue != null)
                    {
                        comparisonKey = new DocumentKey(stringValue.Value.ToString());
                        return true;
                    }
                }
            }
            comparisonKey = null;
            return false;
        }

        public IProxyPredicate GetProxyExecutionPredicate(IIndexProvider indexManager, IQueryStore queryStore, IEnumerable<long> rowsEnumerator ) 
        {
            if (PredicateType.Equals(PredicateType.Constant))
            {
                if (IsTrue(null))
                {
                    return new ProxyPredicate(new StorePredicate(new TrueCondition(), queryStore), this);
                }
                return new ProxyPredicate(new EmptyPredicate(), null);
            }

            OrderedList<int, IIndex> indexes = indexManager.OrderedIndexList;
            bool isCompoundIndex = IsCompoundIndex(indexes);

            if (PredicateType.Equals(PredicateType.SingleVariable) && !isCompoundIndex)
            {
                foreach (var index in indexes.Values.Reverse())
                {
                    //Todo: Consider candidate list for this...
                    //Incase of binary expression evaluate full store.
                    if (!_isExpression && Attributes[0].ToRealString().Equals(index.Attributes.Name))
                    {
                        return new ProxyPredicate(AssignIndexPredicate(index, queryStore), this);
                    }
                } 
            }
            else if (PredicateType.Equals(PredicateType.MultiVariable) || isCompoundIndex)
            {
                var candidatesList = new OrderedList<int, IIndex>();
                foreach (var index in indexes.Values)
                {
                        continue;
                    List<string> indexAttributes = new List<string>();
                    //foreach (var attribute in index.Attributes)
                    //{
                    //    indexAttributes.Add(attribute.Name);
                    //}              

                    IIndex candIndex = null;
                    foreach (var attribute in Attributes)
                    {
                        if (indexAttributes.Contains(attribute.ToString()))
                        {
                            candIndex = index;
                            continue;
                        }
                        candIndex = null;
                        break;
                    }

                    if (candIndex != null)
                    {
                        candidatesList.Add(candIndex.ValueCount, candIndex);
                    }
                }

                if (candidatesList.Count > 0)
                {
                    IPredicate predicate = new FilterPredicate(this, candidatesList.FirstValues[0]);
                    predicate.AddChildPredicate(new AllPredicate(candidatesList.FirstValues[0]));
                    return new ProxyPredicate(predicate, this);
                }
            }
            return new ProxyPredicate(new StorePredicate(this, queryStore), this);
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            _leftEvaluable.AssignConstants(parameters);

            if (_middleEvaluable != null)
            {
                _middleEvaluable.AssignConstants(parameters);
            }

            if (_rightEvaluable != null)
            {
                _rightEvaluable.AssignConstants(parameters);
            }

            //'BETWEEN' valid constants check.
            if (_condition == Condition.Between || _condition == Condition.NotBetween)
            {
                if (_middleEvaluable != null && _middleEvaluable.EvaluationType == EvaluationType.Constant &&
                    _rightEvaluable != null && _rightEvaluable.EvaluationType == EvaluationType.Constant)
                {
                    IJsonValue startRange, endRange;
                    if(!_middleEvaluable.Evaluate(out startRange, null) ||  !_rightEvaluable.Evaluate(out endRange, null))
                    {
                        throw new QuerySystemException(ErrorCodes.Query.INVALID_CONSTANT_BINARY_EXPRESSION_SPECIFIED); 
                    }

                    if (startRange.CompareTo(endRange) > 0)
                    {
                        throw new DatabaseException(ErrorCodes.Query.INVALID_BETWEEN_OPERATOR_ARGUMENTS);
                    }
                }
            }
        }

        public void AssignScalarFunctions()
        {
            if (_middleEvaluable!=null)
            {
                foreach (Function function in _middleEvaluable.Functions)
                    AssignFunction(function);
            }

            if (_rightEvaluable != null)
            {
                foreach (Function function in _rightEvaluable.Functions)
                    AssignFunction(function);
                
            }

            if (_leftEvaluable == null) 
                return;

            foreach (Function function in _leftEvaluable.Functions)
                AssignFunction(function);
            
        }

        private void AssignFunction(Function function)
        {
            IFunction scalarFunction = ScalarFunctionsStore.GetScalarFunction(function);
            if (scalarFunction != null)
            {
                function.ExecutionInstance = scalarFunction;
                return;
            }
            throw new QuerySystemException(ErrorCodes.Query.INVALID_FUNCTION_NAME_SPECIFIED, new[] { function.FunctionNameActual });

        }

        #endregion

        #region ICloneable members
        
        public object Clone()
        {
            return this;
        }

        #endregion

        #region static helpers

        private static bool InValidCompareCheck(IJsonValue value1, IJsonValue value2,
            IJsonValue value3, Condition condition)
        {
            switch (condition)
            {
                case Condition.GreaterThan:
                case Condition.GreaterThanEqualTo:
                case Condition.LesserThan:
                case Condition.LesserThanEqualTo:
                    return InValidCompareValue(value1, value2);
                case Condition.Between:
                case Condition.NotBetween:
                    return InValidCompareValue(value1, value2, value3);
            }
            return false;
        }

        private static bool InValidCompareValue(IJsonValue value1, IJsonValue value2, IJsonValue value3 = null)
        {
            if (value1.DataType.Equals(FieldDataType.Embedded))
                return false;
            if (value1.DataType.Equals(FieldDataType.Array) || value1.DataType.Equals(FieldDataType.Object))
                return true;

            if (value2.DataType.Equals(FieldDataType.Array) || value2.DataType.Equals(FieldDataType.Object))
                return true;

            if (value3 != null && (value1.DataType.Equals(FieldDataType.Array) || value1.DataType.Equals(FieldDataType.Object)))
                return true;

            return false;
        }

        private static AttributeValue[] GetAttributeValues(IJsonValue arrayValue, bool singleIndex)
        {
            object[] array = (object[])((ArrayJsonValue)arrayValue).Value;
            AttributeValue[] values = new AttributeValue[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                values[i] = new SingleAttributeValue((IComparable)array[i]);
            }

            if (!singleIndex)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if(values[i] == null)
                        values[i] = new MultiAttributeValue(NullValue.Null);
                    else
                        values[i] = new MultiAttributeValue(values[i]);
                }
            }

            return values;
        }

        private static AttributeValue GetAttributeValue(IJsonValue jsonValue, bool isArray, bool minRange, bool singleIndex)
        {
            IComparable returnValue = (IComparable)jsonValue.Value;

            if (isArray)
            {
                object[] array = (object[])jsonValue.Value;

                if (array.Length != 1)
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_NUMBER_OF_ARRAY_RANGE_ELEMENTS,
                        new[] { jsonValue.ToString() });
                }

                returnValue = new ArrayElement(
                    new SingleAttributeValue(array[0] as IComparable), minRange ? 0 : int.MaxValue);
            }
            AttributeValue attributeValue;

            if (jsonValue is NullValue)
            {
                attributeValue = new NullValue();
            }
            else
            {
                attributeValue = new SingleAttributeValue(returnValue);
            }

            if (!singleIndex)
            {
                attributeValue = new MultiAttributeValue(attributeValue);
            }
            return attributeValue;
        }

        private static AttributeValue GetAttributeValue(IComparable value, bool singleIndex)
        {
            AttributeValue attributeValue = new SingleAttributeValue(value);
            if (!singleIndex)
            {
                if(value == null || value is NullValue)
                    attributeValue = new MultiAttributeValue(NullValue.Null);
                else
                    attributeValue = new MultiAttributeValue(attributeValue);
            }
            return attributeValue;
        }

        private static AttributeValue[] GetAttributes(AttributeValue attributeValue, int attrNum)
        {
            List<AttributeValue> attributeValues = 
                new List<AttributeValue> {attributeValue};
            for (int i = 0; i < attrNum - 1; i++)
            {
                attributeValues.Add(new AllValue());
            }
            return attributeValues.ToArray();
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is ComparisonPredicate))
            {
                return false;
            }

            ComparisonPredicate predicate = ((ComparisonPredicate)obj);

            if (!Condition.Equals(predicate.Condition))
                return false;

            return predicate.CheckValuesEquality(_leftEvaluable, _middleEvaluable, _rightEvaluable);
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("ComparisonPredicate:{");
            output.Write("LHS=");
            if (_leftEvaluable != null)
            {
                _leftEvaluable.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",Op="+_condition.ToString());
            output.Write(",RHS=");
            if (_rightEvaluable != null)
            {
                _rightEvaluable.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",IsNot="+_isNot.ToString());
            output.Write(",MHS=");
            if (_middleEvaluable != null)
            {
                _middleEvaluable.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }

        private bool IsCompoundIndex(OrderedList<int, IIndex> indexes)
        {
            return false;
        }
    }
}
