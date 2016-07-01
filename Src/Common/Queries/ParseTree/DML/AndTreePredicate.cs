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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Util;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class AndTreePredicate : ITreePredicate
    {
        readonly List<ITreePredicate> _predciates;        

        public AndTreePredicate()
        {
            _predciates = new List<ITreePredicate>();
        }

        public List<ITreePredicate> TreePredicates
        {
            get { return _predciates; }
        }
        
        public void Add(ITreePredicate item)
        {
            _predciates.Add(item);
        }

        #region ITreePredicate memebers

        public bool Completed { get; set; }

        public bool HasOr
        {
            get
            {
                foreach (var predciate in _predciates)
                {
                    if (predciate is OrTreePredicate || predciate.HasOr)
                        return true;
                }
                return false;
            }
        }

        public bool IsTrue(IJSONDocument entry)
        {
            foreach (var predciate in _predciates)
            {
                if (!predciate.IsTrue(entry))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsTerminal
        {
            get
            {
                foreach (var predciate in _predciates)
                {
                    if (!(predciate is ComparisonPredicate))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public string InString
        {
            get
            {
                string value = "(";
                for (int i = 0; i < _predciates.Count; i++)
                {
                    value += _predciates[i].InString;
                    if (i != _predciates.Count - 1)
                    {
                        value += " AND ";
                    }
                }
                return value + ")";
            }
        }

        public List<ITreePredicate> AtomicTreePredicates
        {
            get
            {
                var predciates = new List<ITreePredicate>();
                foreach (var predciate in _predciates)
                {
                    if (!(predciate is OrTreePredicate))
                    {
                        predciates.AddRange(predciate.AtomicTreePredicates);
                    }
                }
                return predciates;
            }
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var predciate in _predciates)
            {
                predciate.AssignConstants(parameters);
            }
        }

        public void AssignScalarFunctions()
        {
            foreach (var predciate in _predciates)
            {
                predciate.AssignScalarFunctions();
            }
        }
        
        public ITreePredicate Contract()
        {
            Dictionary<int, ITreePredicate> contractedPreds = new Dictionary<int, ITreePredicate>();
            for (int i = 0; i < _predciates.Count; i++)
            {
                ITreePredicate contractedPred = _predciates[i].Contract();
                if (contractedPred != null)
                {
                    while (contractedPred.Contract() != null)
                    {
                        contractedPred = contractedPred.Contract();
                    }
                    contractedPreds[i] = contractedPred;
                }
            }

            if (contractedPreds.Count.Equals(0))
            {
                return null;
            }

            AndTreePredicate returnAnd = new AndTreePredicate();

            for (int i = 0; i < _predciates.Count; i++)
            {
                if (contractedPreds.ContainsKey(i))
                {
                    returnAnd.Add(contractedPreds[i]);
                }
                else
                {
                    returnAnd.Add(_predciates[i]);
                }
            }

            ITreePredicate contractedExpr = returnAnd;
            while (contractedExpr.Contract() != null)
            {
                contractedExpr = contractedExpr.Contract();
            }
            return contractedExpr;
        }

        public ITreePredicate Expand()
        {
            foreach (var predciate in _predciates)
            {
                var orPredicate = predciate as OrTreePredicate;
                if (orPredicate != null)
                {
                    ITreePredicate orParent = null;
                    foreach (var childPredciate in orPredicate.TreePredicates)
                    {
                        orParent = orParent ?? new OrTreePredicate();
                        if (orPredicate.TreePredicates.Count > 1)
                        {
                            var andChild = new AndTreePredicate();
                            foreach (var sibling in _predciates)
                            {
                                if (sibling != orPredicate)
                                {
                                    andChild.Add(sibling);
                                }
                            }
                            andChild.Add(childPredciate);
                            ((OrTreePredicate)orParent).Add(andChild);
                        }
                        else
                        {
                            orParent = childPredciate;
                        }
                    }
                    return orParent;
                }
            }

            foreach (var predciate in _predciates)
            {
                var resultPred = predciate.Expand();
                if (resultPred != null)
                {
                    ITreePredicate returnPredciate = new AndTreePredicate();
                    foreach (var sibling in _predciates)
                    {
                        if (sibling != predciate)
                        {
                            ((AndTreePredicate)returnPredciate).Add((ITreePredicate)sibling.Clone());
                        }
                    }
                    ((AndTreePredicate)returnPredciate).Add(resultPred);
                    return returnPredciate;
                }
            }
            return null;
        }

        public IProxyPredicate GetProxyExecutionPredicate(IIndexProvider indexManager, IQueryStore queryStore, IEnumerable<long> rowsEnumerator)
        {
            ProxyAndPredicate proxyPredicate = new ProxyAndPredicate();
            proxyPredicate.AddTreePredicates(_predciates);

            if (!IsTerminal)
            {
                foreach (var predicate in _predciates)
                {
                    proxyPredicate.AddChildPredicate(
                        predicate.GetProxyExecutionPredicate(indexManager, queryStore, rowsEnumerator));
                }
                return proxyPredicate;
            }

            OrderedList<int,IIndex> indexes = indexManager.OrderedIndexList;
            ArrayList sets = new ArrayList();

            ProxyAndPredicate singleSet = new ProxyAndPredicate();
            foreach (var value in proxyPredicate.TreePredicates)
            {
                ComparisonPredicate predicate = (ComparisonPredicate)value;
                if (predicate.PredicateType.Equals(PredicateType.Constant))
                {
                    //When any constant expression is false in an AND Expression it will be resulting into 0 rowIds.
                    if (!predicate.IsTrue(null))
                    {
                        return new ProxyPredicate(new EmptyPredicate(), predicate);
                    }
                }
                else if (predicate.PredicateType.Equals(PredicateType.SingleVariable))
                {
                    singleSet.AddTreePredicate(predicate);
                }
                else if (predicate.PredicateType.Equals(PredicateType.MultiVariable))
                {
                    sets.Add(predicate);
                }
                else if (predicate.PredicateType.Equals(PredicateType.AllVariable))
                {
                    proxyPredicate.AddChildPredicate(
                        predicate.GetProxyExecutionPredicate(indexManager, queryStore, rowsEnumerator));
                }
            }

            if (singleSet.TreePredicates.Count > 0)
            {
                sets.Add(singleSet);
            }

            foreach (var set in sets)
            {
                if (set is ITreePredicate)
                {
                    proxyPredicate.AddChildPredicate(
                        GetMultiAttributePredicate((ComparisonPredicate) set, indexes.Values,queryStore));
                }
                else if (set is ProxyAndPredicate)
                {
                    var singleAttributeSet = (ProxyAndPredicate)set;
                    ReduceRecuringRanges(ref singleAttributeSet);

                    List<ProxyAndPredicate> proxyAnds = AssignCompoundIndices(singleAttributeSet, indexes.Values, queryStore);

                    if (proxyAnds.Count == 0)
                        proxyAnds.Add(singleAttributeSet);
                    
                    foreach (var proxyAnd in proxyAnds)
                    {
                        if (proxyAnd.TreePredicates.Count == 0)
                        {
                            continue;
                        }
                        AssignSingleAttributesPredicate(proxyAnd, indexes.Values, queryStore);
                    }

                    if (proxyAnds.Count.Equals(1))
                    {
                        foreach (var predicate in proxyAnds[0].Predicates)
                        {
                            proxyPredicate.AddChildPredicate(predicate);
                            proxyPredicate.TreePredicates = _predciates;
                        }
                    }
                    else
                    {
                        //Getting the ProxyANDPredicate with the lowest cost...
                        OrderedList<double, ProxyAndPredicate> orderedPredicates = new OrderedList<double, ProxyAndPredicate>();

                        foreach (var proxyAnd in proxyAnds)
                            orderedPredicates.Add(proxyAnd.Statistics[Statistic.ExpectedIO], proxyAnd);

                        foreach (var predicate in orderedPredicates.FirstValues[0].Predicates)
                        {
                            proxyPredicate.AddChildPredicate(predicate);
                            proxyPredicate.TreePredicates = _predciates;
                        }
                        proxyPredicate.TreePredicates = _predciates;
                    }
                }
            }
            return proxyPredicate;
        }

        #region ICloneable members

        public object Clone()
        {
            var copy = new AndTreePredicate();
            foreach (var expression in _predciates)
            {
                copy.Add((ITreePredicate)expression.Clone());
            }
            return copy;
        }

        #endregion

        #endregion

        #region Static helpers

        /// <summary>
        /// Assigns a proxy predicate to a single-attributed expressions.
        /// </summary>
        private static void AssignSingleAttributesPredicate(ProxyAndPredicate proxyAnd,
            ICollection<IIndex> indexes, IQueryStore queryStore)
        {
            OrderedList<string, ComparisonPredicate> attribExprList = new OrderedList<string, ComparisonPredicate>();
            foreach (var value in proxyAnd.TreePredicates)
            {
                var expression = (ComparisonPredicate)value;
                if (expression.IsBinaryExpression)
                    continue;
                attribExprList.Add(expression.AttributeNames[0], expression);
            }

            if (attribExprList.Count > 0)
            {
                foreach (var attribute in attribExprList.Keys)
                {
                    IIndex lowestIndex = null;
                    foreach (var index in indexes)
                    {
                        if (attribute.Equals(index.Attributes.Name))
                        {
                            if (lowestIndex == null)
                            {
                                lowestIndex = index;
                            }
                            else if (lowestIndex.ValueCount > index.ValueCount)
                            {
                                lowestIndex = index;
                            }
                        }
                    }

                    if (lowestIndex != null)
                    {
                        foreach (var expression in attribExprList[attribute])
                        {
                            proxyAnd.AddChildPredicate(
                                new ProxyPredicate(expression.AssignIndexPredicate(lowestIndex, queryStore), expression));
                            proxyAnd.TreePredicates.Remove(expression);
                        }
                    }
                }

                //For every unassigned expression...
                if (proxyAnd.TreePredicates.Count > 0)
                {
                    ITreePredicate assingedExpression;
                    if (proxyAnd.TreePredicates.Count.Equals(1))
                    {
                        assingedExpression = proxyAnd.TreePredicates[0];
                    }
                    else
                    {
                        AndTreePredicate assingedAnd = new AndTreePredicate();
                        foreach (var expression in proxyAnd.TreePredicates)
                        {
                            assingedAnd.Add(expression);
                        }
                        assingedExpression = assingedAnd;
                    }
                    proxyAnd.AddChildPredicate(new ProxyPredicate(
                        new StorePredicate(assingedExpression, queryStore), assingedExpression));
                }
            }
        }

        /// <summary>
        /// Assigns a proxy predicate to a multi-attributed expression.
        /// </summary>
        private static IProxyPredicate GetMultiAttributePredicate(ComparisonPredicate expression,
            ICollection<IIndex> indices, IQueryStore queryStore)
        {
            var candidatesList = new OrderedList<int, IIndex>();
            foreach (var index in indices)
            {
                List<string> indexAttributes = new List<string>();
               
                    indexAttributes.Add(index.Attributes.Name);

                if (ContainsRange(indexAttributes, expression.AttributeNames))
                {
                    candidatesList.Add(index.KeyCount, index);
                }
            }

            if (candidatesList.Count > 0)
            {
                var predicate = new FilterPredicate(expression, candidatesList.FirstValues[0]);
                predicate.AddChildPredicate(new AllPredicate(candidatesList.FirstValues[0]));
                return new ProxyPredicate(predicate, expression);
            }

            return new ProxyPredicate(new StorePredicate(expression, queryStore), expression);
        }

        /// <summary>
        /// Tries to assign compound indexes to expressions if applicable...
        /// </summary>
        private static List<ProxyAndPredicate> AssignCompoundIndices(ProxyAndPredicate set,
            IEnumerable<IIndex> indexes, IQueryStore queryStore)
        {
            var attribExprList = new OrderedList<string, ComparisonPredicate>();
            var proxyPredicates = new List<ProxyAndPredicate>();

            //For every expression in the set ready for assignment.
            foreach (var expresson in set.TreePredicates)
            {
                var expression = (ComparisonPredicate) expresson;
                if (expression.IsBinaryExpression)
                    continue;
                attribExprList.Add(expression.AttributeNames[0], expression);
            }

            //Each compound index will create a new state of the expression. 
            foreach (var index in indexes)
            {

                int matchedNumber = 0;
                var matchedAttribs = new List<string>();

                //If keys (attributes list) does not contain the first prefix of the index
                //Then the index is not usable.
                if (attribExprList.ContainsKey(index.Attributes.Name))
                {
                    matchedAttribs.Add(index.Attributes.Name);
                    matchedNumber += attribExprList[index.Attributes.Name].Count;
                }


                //if matched attributes are < 2 no need of compound assignment.
                if (matchedAttribs.Count < 2)
                {
                    continue;
                }

                //Assign each of them an index and get them to the predicate, and get the one with the lowest cost.
                ComparisonPredicate cheapestExpression = attribExprList[matchedAttribs[0]][0];
                IPredicate cheapestPredicate = cheapestExpression.AssignIndexPredicate(index, queryStore);
                for (int i = 1; i < attribExprList[matchedAttribs[0]].Count; i++)
                {
                    ComparisonPredicate tempExpression = attribExprList[matchedAttribs[0]][i];
                    IPredicate tempPredicate = tempExpression.AssignIndexPredicate(index, queryStore);

                    if (tempPredicate.Statistics[Statistic.ExpectedIO] <
                        cheapestPredicate.Statistics[Statistic.ExpectedIO])
                    {
                        cheapestExpression = tempExpression;
                        cheapestPredicate = tempPredicate;
                    }
                }

                //For filteration and adding rest of expressions to the index.
                ITreePredicate assingedExpression = null;
                if (!matchedNumber.Equals(2))
                {
                    AndTreePredicate assingedAnd = new AndTreePredicate();
                    foreach (var attribute in matchedAttribs)
                    {
                        foreach (var expression in attribExprList[attribute])
                        {
                            if (expression != cheapestExpression)
                                assingedAnd.Add(expression);
                        }
                    }
                    assingedExpression = assingedAnd;
                }
                else
                {
                    foreach (var expression in attribExprList[matchedAttribs[1]])
                    {
                        assingedExpression = expression;
                    }
                }

                FilterPredicate filterPredicate = new FilterPredicate(assingedExpression, index);
                filterPredicate.AddChildPredicate(cheapestPredicate);
                ProxyAndPredicate proxyAnd = (ProxyAndPredicate) set.Clone();

                //Removing assigned expressions.
                foreach (var attribute in matchedAttribs)
                {
                    foreach (var expression in attribExprList[attribute])
                    {
                        proxyAnd.TreePredicates.Remove(expression);
                    }
                }

                proxyAnd.AddChildPredicate(new ProxyPredicate(filterPredicate, null));
                proxyPredicates.Add(proxyAnd);
            }

            foreach (var proxyAnd in proxyPredicates)
            {
                //Recursive call for each ProxyAndPredicate.
                if (proxyAnd.TreePredicates.Count > 1)
                {
                    var values = AssignCompoundIndices(proxyAnd, indexes, queryStore);
                    foreach (var value in values)
                    {
                        proxyPredicates.Add(value);
                    }
                }
            }
            return proxyPredicates;
        }

        /// <summary>
        /// A static method implementing range containment on two enumurables.
        /// source1 = super-set and source2 = sub-set. 
        /// </summary>
        private static bool ContainsRange<T>(IEnumerable<T> source1, IEnumerable<T> source2)
        {
            foreach (var item in source2)
            {
                if (!source1.Contains(item))
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Returns two pins (smaller and greater)... both will be null if there is no intersecting range between the expressions.
        /// Unhandled cases: 
        /// IN, NotIn, NotEquals, NotLike, NotBetween.
        /// </summary>
        private static void ReduceRecuringRanges(ref ProxyAndPredicate singleAttributeSet)
        {
            var repeatedAttributes = PredicateHelper.GetRepeatedAttributes(singleAttributeSet.TreePredicates);

            foreach (var attribute in repeatedAttributes)
            {
                var repeatedExpressions = new List<ComparisonPredicate>();
                foreach (var value in singleAttributeSet.TreePredicates)
                {
                    var expression = (ComparisonPredicate)value;

                    if (attribute.ToString().Equals(expression.AttributeNames[0])
                        && ReducableExpression(expression))
                    {
                        repeatedExpressions.Add(expression);
                    }
                }

                if (repeatedExpressions.Count > 0)
                {
                    //Ranges could be between, <, >, =, Not equals to Null...
                    //Constants could be: Null, Bool, Number, String, DateTime, -not Array, -not JSONObject.
                    Tuple<IJsonValue, IJsonValue> tuple = GetJSONValueRange(repeatedExpressions);
                    if (tuple != null)
                    {
                        if (!(tuple.Item1 is NullValue) && !(tuple.Item2 is ObjectJsonValue))
                        {
                            singleAttributeSet.TreePredicates.Add(
                                new ComparisonPredicate(attribute, Condition.Between,
                                    PredicateHelper.GetConstant(tuple.Item1), PredicateHelper.GetConstant(tuple.Item2)));
                        }
                        else if (tuple.Item2 is ObjectJsonValue)
                        {
                            //Greater than/greater and equals to.
                            singleAttributeSet.TreePredicates.Add(
                                new ComparisonPredicate(attribute, Condition.GreaterThan, PredicateHelper.GetConstant(tuple.Item1)));
                        }
                        else
                        {
                            //Lesser than/lesser than and equals to. 
                            singleAttributeSet.TreePredicates.Add(
                                new ComparisonPredicate(attribute, Condition.LesserThan, PredicateHelper.GetConstant(tuple.Item1)));
                        }
                    }
                    else
                    {
                        singleAttributeSet.AddChildPredicate(new ProxyPredicate(new EmptyPredicate(), null));
                    }

                    foreach (var expression in repeatedExpressions)
                    {
                        singleAttributeSet.TreePredicates.Remove(expression);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a tuple of reduced JSON values.
        /// Bug: Greater than = Greater than equals to.
        /// </summary>
        private static Tuple<IJsonValue, IJsonValue> GetJSONValueRange(IEnumerable<ComparisonPredicate> expressions)
        {
            IJsonValue pin1 = NullValue.Null;
            IJsonValue pin2 = new ObjectJsonValue(new JSONDocument());

            foreach (var compareExpression in expressions)
            {
                //If pin1 is greater than pin2 the result set will be null.
                if (pin1.CompareTo(pin2) > 0)
                    return null;

                IJsonValue value = compareExpression.ConstantValues[0];
                switch (compareExpression.Condition)
                {
                    case Condition.Equals:
                    case Condition.Like:
                        if (pin2.CompareTo(value) < 0 || pin1.CompareTo(value) > 0)
                        return null;

                        pin2 = pin1 = value;
                        break;

                    case Condition.GreaterThan:
                        if (value.CompareTo(pin2) >= 0)
                            return null;

                        if (pin1.CompareTo(value) < 0)
                            pin1 = value;
                        break;

                    case Condition.GreaterThanEqualTo:
                        if (value.CompareTo(pin2) > 0)
                            return null;

                        if (pin1.CompareTo(value) < 0)
                            pin1 = value;
                        break;

                    case Condition.LesserThan:
                        if (value.CompareTo(pin1) <= 0)
                            return null;

                        if (pin2.CompareTo(value) > 0)
                            pin2 = value;
                        break;

                    case Condition.LesserThanEqualTo:
                        if (value.CompareTo(pin1) < 0)
                            return null;

                        if (pin2.CompareTo(value) > 0)
                            pin2 = value;
                        break;

                    case Condition.Between:
                        //Both values could be equal.
                        if (value.CompareTo(pin2) > 0 ||
                            value.CompareTo(compareExpression.ConstantValues[0]) > 0)
                        {
                            return null;
                        }
                        pin1 = value;
                        pin2 = compareExpression.ConstantValues[0];
                        break;

                    case Condition.IsNull:
                        if (NullValue.Null.CompareTo(pin1) > 0)
                            return null;

                        pin1 = pin2 = NullValue.Null;
                        break;

                    case Condition.IsNotNull:
                        if (NullValue.Null.CompareTo(pin2) == 0)
                            return null;

                        pin1 = new BooleanJsonValue(false);
                        break;

                    case Condition.Exists:
                        break;

                    case Condition.NotExists:
                        return null;
                }
            }
            return new Tuple<IJsonValue, IJsonValue>(pin1, pin2);
        }
        
        /// <summary>
        /// Determines whether a comparison expression is reduceable or not.
        /// </summary>
        private static bool ReducableExpression(ComparisonPredicate expression)
        {
            switch (expression.Condition)
            {
                case Condition.NotEquals: return false;
                case Condition.In: return false;
                case Condition.NotIn: return false;
                case Condition.NotBetween: return false;
                case Condition.ContainsAny: return false;
                case Condition.NotContainsAny: return false;
                case Condition.ContainsAll: return false;
                case Condition.NotContainsAll: return false;
                case Condition.ArraySize: return false;
                case Condition.NotArraySize: return false;
            }

            foreach (var constant in expression.ConstantValues)
            {
                if (constant is ValueList || constant.Value is ArrayJsonValue || constant.Value is ObjectJsonValue)
                    return false;
            }
            return true;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is AndTreePredicate))
            {
                return false;
            }

            AndTreePredicate andObject = (AndTreePredicate)obj;

            if (!_predciates.Count.Equals(andObject.TreePredicates.Count))
                return false;

            foreach (var expression in andObject.TreePredicates)
            {
                if (!_predciates.Contains(expression))
                    return false;
            }

            return true;
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("AndTreePredicate:{[");
            for (int i = 0; i < _predciates.Count; i++)
            {
                _predciates[i].Print(output);
                if(i!=_predciates.Count-1)
                    output.Write(",");
            }
            output.Write("]}");
        }
    }
}
