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
using System.Collections.Generic;
using System.Linq;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Util;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class OrTreePredicate : ITreePredicate
    {
        readonly List<ITreePredicate> _predciates;

        public OrTreePredicate()
        {
            _predciates = new List<ITreePredicate>();
        }
        
        public void Add(ITreePredicate item)
        {
            _predciates.Add(item);
        }

        public List<ITreePredicate> TreePredicates
        {
            get { return _predciates; }
        }

        #region ITreePredicate members

        public bool Completed { get; set; }

        //Here the IsTerminal donates whether all the childs are ANDTreeExpressions or not.
        public bool IsTerminal
        {
            get
            {
                foreach (var predciate in _predciates)
                {
                    if (!(predciate is AndTreePredicate))
                        return false;
                }
                return true;
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
                        predciates.AddRange(predciate.AtomicTreePredicates);
                }
                return predciates;
            }
        }

        public bool HasOr { get { return true; } }

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
                        value += " OR ";
                    }
                }
                return value + ")";
            }
        }

        public bool IsTrue(IJSONDocument entry)
        {
            foreach (var predciate in _predciates)
            {
                if (predciate.IsTrue(entry))
                    return true;
            }
            return false;
        }

        //We don't need different combinations of an ORTreePredicates, we will instead get its final form.
        public ITreePredicate Expand()
        {
            foreach (var predciate in _predciates)
            {
                var resultExpression = predciate.Expand();
                if (resultExpression != null)
                {
                    OrTreePredicate expandedPred = new OrTreePredicate();
                    foreach (var predciate1 in _predciates)
                    {
                        if (predciate != predciate1)
                        {
                            expandedPred.Add(predciate1);
                        }
                    }
                    expandedPred.Add(resultExpression);
                    return expandedPred;
                }
            }
            return null;
        }

        public ITreePredicate Contract()
        {
            if (!IsTerminal)
            {
                Dictionary<int, ITreePredicate> contractedPreds = new Dictionary<int, ITreePredicate>();
                for (int i = 0; i < _predciates.Count; i++)
                {
                    ITreePredicate contracted = _predciates[i].Contract();
                    if (contracted != null)
                    {
                        while (contracted.Contract() != null)
                        {
                            contracted = contracted.Contract();
                        }
                        contractedPreds[i] = contracted;
                    }
                }

                if (contractedPreds.Count.Equals(0))
                    return null;

                OrTreePredicate returnOr = new OrTreePredicate();
                for (int i = 0; i < _predciates.Count; i++)
                {
                    if (contractedPreds.ContainsKey(i))
                    {
                        returnOr.Add(contractedPreds[i]);
                    }
                    else
                    {
                        returnOr.Add(_predciates[i]);
                    }
                }

                ITreePredicate contractedPred = returnOr;
                while (contractedPred.Contract() != null)
                {
                    contractedPred = contractedPred.Contract();
                }
                return contractedPred;
            }

            List<AndTreePredicate> ands = new List<AndTreePredicate>();

            foreach (var predciate in _predciates)
            {
                if (!(predciate is AndTreePredicate))
                    return null;

                ands.Add((AndTreePredicate)(predciate));
            }

            if(ands.Count < 2)
                return null;

            ITreePredicate commonPred = null;
            foreach (var expression in ands[0].TreePredicates)
            {
                commonPred = expression;
                for (int i = 1; i < ands.Count; i++)
                {
                    if (ands[i].TreePredicates.Count < 2 
                        || !ands[i].TreePredicates.Contains(expression))
                    {
                        return null;
                    }
                }
            }

            AndTreePredicate returnPred = new AndTreePredicate();
            returnPred.Add(commonPred);

            OrTreePredicate orChild = new OrTreePredicate();

            foreach (var and in ands)
            {
                foreach (var predciate in and.TreePredicates)
                {
                    if(!predciate.Equals(commonPred))
                        orChild.Add(predciate);
                }   
            }

            returnPred.Add(orChild);
            ITreePredicate contractPred = returnPred;

            while (contractPred.Contract() != null)
            {
                contractPred = contractPred.Contract();
            }
            return contractPred;
        }

        public IProxyPredicate GetProxyExecutionPredicate(IIndexProvider indexManager, IQueryStore queryStore, IEnumerable<long> rowsEnumerator)
        {
            //Get reducable terminal expressions in the ORExpression...
            List<ComparisonPredicate> terminalPreds = new List<ComparisonPredicate>();
            foreach (var predciate in _predciates)
            {
                ComparisonPredicate comparePred = predciate as ComparisonPredicate;
                if (comparePred != null && 
                    comparePred.PredicateType.Equals(PredicateType.SingleVariable))
                {
                    terminalPreds.Add(comparePred);
                }
            }

            IEnumerable<Attribute> repAttributes = new List<Attribute>();
            List<ITreePredicate> compPreds = new List<ITreePredicate>();
            foreach (var predciate in _predciates)
            {
                if(predciate is ComparisonPredicate)
                    compPreds.Add(predciate);
            }

            if (terminalPreds.Count > 0 && compPreds.Count > 0)
            {
                repAttributes = PredicateHelper.GetRepeatedAttributes(compPreds);
            }

            foreach (var attribute in repAttributes)
            {
                Dictionary<int, ComparisonPredicate> reducablePreds = new Dictionary<int, ComparisonPredicate>();
                for( int i = 0; i < _predciates.Count; i++)
                {
                    if (_predciates[i] is ComparisonPredicate)
                    {
                        ComparisonPredicate comparePred = _predciates[i] as ComparisonPredicate;

                        if (ReducablePredicate(comparePred) 
                            && comparePred.Attributes[0] == attribute)
                        {
                            reducablePreds.Add(i, comparePred);
                        }
                    }
                }

                if (reducablePreds.Count < 2)
                    continue;

                ValueList values = new ValueList();
                foreach (var pair in reducablePreds)
                {
                    values.Add(PredicateHelper.GetConstant(pair.Value.ConstantValues[0]));
                }

                ComparisonPredicate reducedPred = 
                    new ComparisonPredicate(attribute, Condition.In, values);

                for (int i = 0; i < _predciates.Count; i++)
                {
                    if(reducablePreds.Keys.Contains(i))
                        _predciates.RemoveAt(i);
                }

                _predciates.Add(reducedPred);
            }
            
            var predicate = new ProxyOrPredicate();
            foreach (var predciate in _predciates)
            {
                predicate.AddExpression(predciate);
                predicate.AddChildPredicate(predciate.GetProxyExecutionPredicate(indexManager, queryStore, rowsEnumerator));
            }
            return predicate;
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

        #region ICloneable members

        public object Clone()
        {
            var copy = new OrTreePredicate();
            foreach (var expression in _predciates)
            {
                copy.Add((ITreePredicate)expression.Clone());
            }
            return copy;
        }

        #endregion

        #endregion
        
        public override bool Equals(object obj)
        {
            if (!(obj is OrTreePredicate))
            {
                return false;
            }

            OrTreePredicate orObject = (OrTreePredicate) obj;

            if (!_predciates.Count.Equals(orObject.TreePredicates.Count))
                return false;

            foreach (var expression in orObject.TreePredicates)
            {
                if(!_predciates.Contains(expression))
                    return false;
            }

            return true;
        }

        private static bool ReducablePredicate(ComparisonPredicate compareExpr)
        {
            if (!compareExpr.PredicateType.Equals(PredicateType.SingleVariable)
                || compareExpr.Condition != Condition.Equals
                || compareExpr.ConstantValues[0] is ArrayJsonValue
                || compareExpr.ConstantValues[0] is ObjectJsonValue)
                return false;
            return true;
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("OrTreePredicate:{[");
            for (int i = 0; i < _predciates.Count; i++)
            {
                _predciates[i].Print(output);
                if (i != _predciates.Count - 1)
                    output.Write(",");
            }
            output.Write("]}");
        }
    }
}
