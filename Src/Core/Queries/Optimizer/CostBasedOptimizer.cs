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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries;
using Alachisoft.NosDB.Common.Queries.Filters;
using Alachisoft.NosDB.Common.Queries.Filters.Aggregations;
using Alachisoft.NosDB.Common.Queries.Filters.Scalars;
using Alachisoft.NosDB.Common.Queries.Optimizer;
using Alachisoft.NosDB.Common.Queries.ParseTree;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Updation;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Caching.LightCache;
using Alachisoft.NosDB.Common.Storage.Indexing;
using Alachisoft.NosDB.Core.Storage.Indexing;
using Alachisoft.NosDB.Core.Queries.Filters;

using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Core.Queries.ParseTree.DML;

namespace Alachisoft.NosDB.Core.Queries.Optimizer
{
    //This file is going to be refactored and modulated as per standards, 
    //the current implementation is temporary.
    class CostBasedOptimizer : IQueryOptimizer
    {
        private readonly CollectionIndexManager _indexManager;
        private readonly LightCache<ITreePredicate, IPredicate> _predicateCache;

        public CostBasedOptimizer(CollectionIndexManager indexManager)
        {
            _indexManager = indexManager;
            _predicateCache = new LightCache<ITreePredicate, IPredicate>();
        }

        public QueryPlan GetQueryPlan(IDmObject parsedQuery, IQuery query, IQueryStore queryStore, MetadataIndex rowsEnumerator)
        {
            //Todo: Plan's cache's key decision (query-string based plan cache/optimizable-section based cache).
            OrderedList<double, IProxyPredicate> sortedPlans = new OrderedList<double, IProxyPredicate>();
            var optimizableQuery = parsedQuery as IFilterObject;

            var criteria = AddQueryCriteria(parsedQuery, query.Parameters, queryStore);
            var queryPlan = new QueryPlan { Criteria = criteria };

            if (optimizableQuery != null && optimizableQuery.WherePredicate != null)
            {
                ITreePredicate whereExpression = optimizableQuery.WherePredicate;
                
                ITreePredicate contractedExpression = whereExpression.Contract();

                if (contractedExpression == null)
                    contractedExpression = whereExpression;

                List<ITreePredicate> distribCombinations = new List<ITreePredicate>();
                distribCombinations.Add(contractedExpression);

                //Todo: Restrict this call if there doesn't exist a compound index.
                while (contractedExpression.Expand() != null)
                {
                    distribCombinations.Add(contractedExpression.Expand());
                    contractedExpression = contractedExpression.Expand();
                }

                foreach (var treePredicate in distribCombinations)
                {
                    if (treePredicate is OrTreePredicate || treePredicate is AndTreePredicate)
                        break;
                    if (treePredicate is ComparisonPredicate)
                    {
                        DocumentKey documentKey = null;
                        if (((ComparisonPredicate)treePredicate).TryGetProxyKeyPredicate(rowsEnumerator, out documentKey))
                        {
                            IProxyPredicate optimizablePredicate = new ProxyPredicate(new KeyPredicate(documentKey, rowsEnumerator), treePredicate);
                            sortedPlans.Add(optimizablePredicate.Statistics[Statistic.ExpectedIO], optimizablePredicate);
                            queryPlan.Predicate = sortedPlans.FirstValues[0].GetExecutionPredicate(queryStore);
                            return queryPlan;
                        }
                    }
                }

                foreach (var expressionState in distribCombinations)
                {
                    IProxyPredicate optimizablePredicate = expressionState.GetProxyExecutionPredicate(_indexManager, queryStore, rowsEnumerator);
                    sortedPlans.Add(optimizablePredicate.Statistics[Statistic.ExpectedIO], optimizablePredicate);
                    //Todo: Add optimizedPredicate to the SortedList by the cost.
                }

                queryPlan.Predicate = sortedPlans.FirstValues[0].GetExecutionPredicate(queryStore);

            }
            else
            {
                if (criteria.GroupFields != null && criteria.GroupFields.Count == 1 && criteria.GroupFields[0] is AllField && criteria.ContainsAggregations)
                {
                    if (criteria.Aggregations.Count == 1 && criteria.Aggregations[0].Aggregation is COUNT &&
                        criteria.Aggregations[0].Evaluation is AllEvaluable)
                    {
                        queryPlan.Criteria.GroupByField = null;
                        queryPlan.Predicate = new SpecialCountPredicate(rowsEnumerator.KeyCount);
                        queryPlan.IsSpecialExecution = true;
                        return queryPlan;
                    }
                }

                //Todo:1 Projection variable-based index assigning (Functions' arguments + attributes).
                queryPlan.Predicate = GetSelectAllPredicate(criteria, rowsEnumerator);
            }
            
            return queryPlan;
        }

        public ICollection<QueryPlan> GetAllPlans(IDmObject parsedQuery, IQuery query, IQueryStore queryStore)
        {
            //NO!
            return null;
        }

        private QueryCriteria AddQueryCriteria(IDqlObject query, IList<IParameter> parameters, IQueryStore queryStore)
        {
            var queryCriteria = new QueryCriteria { Store = queryStore };
            if (query is SelectObject)
            {
                var selectQuery = query as SelectObject;
                if (selectQuery.WherePredicate != null && parameters != null)
                {
                    selectQuery.WherePredicate.AssignConstants(parameters);
                    selectQuery.WherePredicate.AssignScalarFunctions();
                }

                var functions = new List<Function>();
                foreach (var projectionValue in selectQuery.Projections)
                {
                    if (selectQuery.IsDistinct)
                    {
                        IEvaluable projValue = projectionValue as IEvaluable;
                        queryCriteria.AddDistinctField(projValue);
                    }

                    var evaluable = projectionValue as IEvaluable;
                    if (parameters != null && evaluable != null)
                    {
                        evaluable.AssignConstants(parameters);
                    }

                    if (evaluable != null)
                    {
                        if (projectionValue is AllEvaluable)
                        {
                            queryCriteria.GetAllFields = true;
                        }
                        else
                        {
                            queryCriteria.AddProjection(evaluable.ToString(), evaluable);
                        }

                        if(evaluable.Functions != null)
                            functions.AddRange(evaluable.Functions);
                    }
                   
                }

                foreach (var function in functions)
                {
                    if (!AssignAggregateFunction(function, queryCriteria))
                        if (!AssignScalarFunction(function))
                            throw new QuerySystemException(ErrorCodes.Query.INVALID_FUNCTION_NAME_SPECIFIED,
                                new[] {function.FunctionNameActual});
                }

                if (selectQuery.GroupValue != null)
                {
                    foreach (var projectionValue in selectQuery.GroupValue)
                    {
                        queryCriteria.AddGroupByField(projectionValue);
                    }
                }

                if (queryCriteria.GroupByField == null && queryCriteria.ContainsAggregations &&
                    queryCriteria.Aggregations.Count > 0)
                    queryCriteria.GroupByField = new AllField(Field.FieldType.Grouped);

                if (selectQuery.OrderValue != null)
                {
                    foreach (var projectionValue in selectQuery.OrderValue)
                    {
                        var attribute = projectionValue;
                        if (attribute != null)
                            queryCriteria.AddOrderByField(attribute,
                                attribute is BinaryExpression? 
                                ((BinaryExpression)attribute).SortOrder: SortOrder.ASC);
                    }
                }

                long limit = -1;
                if (selectQuery.Limit != null)
                {
                    limit = long.Parse(selectQuery.Limit.InString);
                }
                if (limit > -1)
                {
                    long skip = -1;

                    if (selectQuery.Skip != null)
                    {
                        skip = long.Parse(selectQuery.Skip.InString);
                    }
                    
                    if (skip > 0)
                    {
                        limit += skip;  //: Adding this to avoid bug at query router (while applying skip query)
                    }
                    queryCriteria.AddLimit(limit);
                }
            }
            else if (query is UpdateObject)
            {
                var updateQuery = query as UpdateObject;
                if (updateQuery.WherePredicate != null && parameters != null)
                    updateQuery.WherePredicate.AssignConstants(parameters);
                updateQuery.Updator.AssignConstants(parameters);

                foreach(IUpdation update in updateQuery.Updator.Updations)
                {
                    foreach (var function in update.GetFunctions())
                    {
                        foreach (var argument in function.Arguments)
                        {
                            if (argument.EvaluationType != EvaluationType.Constant)
                            {
                                throw new QuerySystemException(ErrorCodes.Query.INVALID_CONSTANT_FUNCTION_SPECIFIED);
                            }
                        }

                        if (!AssignScalarFunction(function))
                        {
                            throw new QuerySystemException(ErrorCodes.Query.INVALID_FUNCTION_NAME_SPECIFIED,
                                new[] {function.FunctionNameActual});
                        }
                    }
                }
                
                queryCriteria.DocumentUpdate = updateQuery.Updator;
                queryCriteria.UpdateOption = UpdateOption.Update;
            }
            else if (query is InsertObject)
            {
                var document = new JSONDocument();
                foreach (KeyValuePair<Attribute, IEvaluable> pair in ((InsertObject)query).ValuesToInsert)
                {
                    pair.Value.AssignConstants(parameters);

                    foreach (var function in pair.Value.Functions)
                    {
                        foreach (var argument in function.Arguments)
                        {
                            if (argument.EvaluationType != EvaluationType.Constant)
                            {
                                throw new QuerySystemException(ErrorCodes.Query.INVALID_CONSTANT_FUNCTION_SPECIFIED);
                            }
                        }

                        if (!AssignScalarFunction(function))
                        {
                            throw new QuerySystemException(ErrorCodes.Query.INVALID_FUNCTION_NAME_SPECIFIED,
                                new[] {function.FunctionNameActual});
                        }
                    }

                    IJsonValue jsonValue;
                    if (!pair.Value.Evaluate(out jsonValue, null))
                    {
                        throw new QuerySystemException(ErrorCodes.Query.INVALID_CONSTANT_BINARY_EXPRESSION_SPECIFIED);
                    }
                    document.Add(pair.Key.ToString(), jsonValue.Value );
                }
                queryCriteria.NewDocument = document;
                queryCriteria.UpdateOption = UpdateOption.Insert;
            }
            else if (query is DeleteObject)
            {
                var deleteQuery = query as DeleteObject;
                if (deleteQuery.WherePredicate != null && parameters != null)
                    deleteQuery.WherePredicate.AssignConstants(parameters);
                queryCriteria.UpdateOption = UpdateOption.Delete;
            }
            return queryCriteria;
        }

        private bool AssignAggregateFunction(Function function, QueryCriteria queryCriteria)
        {
            AggregateFunctionType agrFunctionType;
            switch (function.FunctionName)
            {
                case "max":
                    agrFunctionType = AggregateFunctionType.MAX;
                    break;
                case "min":
                    agrFunctionType = AggregateFunctionType.MIN;
                    break;
                case "avg":
                   agrFunctionType = AggregateFunctionType.AVG;
                    break;
                case "sum":
                    agrFunctionType = AggregateFunctionType.SUM;
                    break;
                case "count":
                    agrFunctionType = AggregateFunctionType.COUNT;
                    break;
                case "first":
                    agrFunctionType = AggregateFunctionType.FIRST;
                    break;
                case "last":
                    agrFunctionType = AggregateFunctionType.LAST;
                    break;
                default:
                    agrFunctionType = AggregateFunctionType.NOTAPPLICABLE;
                    break;
            }

            if(!agrFunctionType.Equals(AggregateFunctionType.NOTAPPLICABLE))
            {
                if (function.Arguments.Count > 1)
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_NUMBER_OF_AGGREGATE_FUNCTION_ARGUMENTS, new[] { function.ToString() });
                }
                if (function.ArgumentFunctions.Count > 0)
                {
                    foreach (var func in function.ArgumentFunctions)
                    {
                        var functionName = func.FunctionName;
                        if (functionName.Equals("max") || functionName.Equals("min")
                            || functionName.Equals("avg") || functionName.Equals("sum")
                            || functionName.Equals("count") || functionName.Equals("first")
                            || functionName.Equals("last"))
                        {
                            //An aggregate function shouldn't contain any aggregate function in its arguments.
                            throw new QuerySystemException(ErrorCodes.Query.INVALID_AGGREGATE_FUNCTION_ARGUMENTS, new[] { functionName });
                        }
                    }
                }

                function.ExecutionType = FunctionExecutionType.Aggregate;
                queryCriteria.AddAggregateFunction(agrFunctionType, function.Arguments[0]);
                return true;
            }
            return false;
        }

       
        private bool AssignBuiltInScalarFunction(Function function)
        {
            IFunction scalarFunction = ScalarFunctionsStore.GetScalarFunction(function);

            if (scalarFunction != null)
            {
                function.ExecutionInstance = scalarFunction;
                return true;
            }

            return false;
        }

        private bool AssignScalarFunction(Function function)
        {
            return AssignBuiltInScalarFunction(function);
        }

        //Marked for refactoring
        private IPredicate GetSelectAllPredicate(QueryCriteria criteria, IEnumerable<long> rowsEnumerator)
        {
            if (criteria.GetAllFields || criteria.Projections.Length == 0){
                return new FullPredicate(rowsEnumerator);
            }

            if (criteria.ProjectionCount > 1)
            {
                IPredicate predicate = new ORPredicate();
                for(int i=0;i<criteria.ProjectionCount;i++)
                {
                    List<IIndex> list = _indexManager.GetIndexes(criteria[i].ToString());
                    
                    if (list == null){
                        return new StorePredicate(new TrueCondition(), criteria.Store);
                    }

                    predicate.AddChildPredicate(new AllPredicate(list[0]));
                }
                return predicate;
            }

            var indexlist = _indexManager.GetIndexes(criteria[0].ToString());
            
            if (indexlist == null){
                return new FullPredicate(rowsEnumerator);
            }

            return new AllPredicate(indexlist[0]);
        }
    }
}

