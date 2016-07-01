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
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.Filters.Aggregations;
using Alachisoft.NosDB.Common.Queries.Results;
using Alachisoft.NosDB.Common.Queries.Results.Transforms;
using Alachisoft.NosDB.Common.Queries.Updation;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;

namespace Alachisoft.NosDB.Common.Queries
{
    public class QueryCriteria : IPrintable
    {
        private readonly LinkMap<string, IEvaluable> _projections;
        private List<DocumentCollector> _aggregations;
        private Field _groupByField;
        private Field _orderByField;
        private Field _distinctField;
        private long _limit;
        private bool _getAllFields = false;
        private IQueryStore _store;
        private UpdateOption _updateOption;
        private Updator _documentUpdate;
        private JSONDocument newDocument;
        private IQueryStore _substituteStore;
        private bool sortResult;
        private long queryId;

        public IQueryStore SubstituteStore
        {
            get { return _substituteStore; }
            set { _substituteStore = value; }
        }

        public QueryCriteria()
        {
            _projections = new LinkMap<string, IEvaluable>();
            _limit = long.MinValue;
        }

        public Updator DocumentUpdate
        {
            get { return _documentUpdate; }
            set { _documentUpdate = value; }
        }

        public IEvaluable[] Projections
        {
            get
            {
                var arr = new IEvaluable[_projections.Count];
                _projections.Values.CopyTo(arr, 0);
                return arr;
            }
        }

        public IEvaluable this[int index]
        {
            get { return _projections[index]; }
        }

        public List<Field> GroupFields
        {
            get
            {
                if (_groupByField != null)
                {
                    List<Field> fieldsList = new List<Field>();
                    if (_groupByField is CompoundField)
                    {
                        CompoundField gbf = (CompoundField) _groupByField;
                        foreach (var field in gbf.Fields)
                        {
                            fieldsList.Add(field);
                        }
                    }
                    else
                    {
                        fieldsList.Add(_groupByField);
                    }
                    return fieldsList;
                }
                return null;
            }
        }

        public IEvaluable GetProjection(string name)
        {
            return _projections[name];
        }

        public long Limit
        {
            get { return _limit; }
        }

        public void AddProjection(string fieldName, IEvaluable field)
        {
            if (field is BinaryExpression)
                fieldName = ((BinaryExpression)field).Alias;

            if (!_projections.ContainsKey(fieldName))
            {
                _projections.Add(fieldName, field);
            }
            else throw new QuerySystemException(ErrorCodes.Query.QUERYCRITERIA_FIELD_ALREADY_EXISTS, new[] {fieldName});
        }

        public int Count
        {
            get { return _projections.Count; }
        }

        public void RemoveProjection(string name)
        {
            _projections.Remove(name);
        }

        public bool ContainsProjection(string name)
        {
            return _projections.ContainsKey(name);
        }

        public int ProjectionCount
        {
            get { return _projections.Count; }
        }

        public List<DocumentCollector> Aggregations
        {
            get { return _aggregations; }
        }

        public Field GroupByField
        {
            get { return _groupByField; }
            set { _groupByField = value; }
        }

        public Field OrderByField
        {
            get { return _orderByField; }
        }

        public Field DistinctField
        {
            get { return _distinctField; }
        }

        public void AddGroupByField(IEvaluable field)
        {
            if (_groupByField == null)
                _groupByField = new Field(field, Field.FieldType.Grouped);
            else if (_groupByField is CompoundField)
            {
                CompoundField gbf = (CompoundField) _groupByField;
                gbf.AddField(new Field(field, Field.FieldType.Grouped));
            }
            else
            {
                CompoundField gbf = new CompoundField(Field.FieldType.Grouped);
                gbf.AddField(_groupByField);
                gbf.AddField(new Field(field, Field.FieldType.Grouped));
                _groupByField = gbf;
            }
        }

        public void RemoveGroupByField(string field)
        {
            if (_groupByField != null)
            {
                if (_groupByField is CompoundField)
                {
                    CompoundField gbf = (CompoundField) _groupByField;
                    gbf.RemoveField(field);
                }
                else
                {
                    _groupByField = null;
                }
            }
        }

        public void AddOrderByField(IEvaluable field, SortOrder order)
        {
            if (_orderByField == null)
                _orderByField = new Field(field, Field.FieldType.Ordered, order);
            else if (_orderByField is CompoundField)
            {
                CompoundField obf = (CompoundField) _orderByField;
                obf.AddField(new Field(field, Field.FieldType.Ordered, order));
            }
            else
            {
                CompoundField obf = new CompoundField(Field.FieldType.Ordered);
                obf.AddField(_orderByField);
                obf.AddField(new Field(field, Field.FieldType.Ordered, order));
                _orderByField = obf;
            }
        }

        public void AddLimit(long limit)
        {
            _limit = limit;
        }

        public void AddDistinctField(IEvaluable projection)
        {
            if (_distinctField == null)
            {
                _distinctField = new Field(projection, Field.FieldType.Distinct);
            }
            else
            {
                var compoundField = _distinctField as CompoundField;
                if (compoundField != null)
                {
                    compoundField.AddField(new Field(projection, Field.FieldType.Distinct));
                }
                else
                {
                    CompoundField field = new CompoundField(Field.FieldType.Distinct);
                    field.AddField(_distinctField);
                    field.AddField(new Field(projection, Field.FieldType.Distinct));
                }
            }
        }

        public void RemoveOrderByField(string field)
        {
            if (_orderByField != null)
            {
                if (_orderByField is CompoundField)
                {
                    CompoundField obf = (CompoundField) _orderByField;
                    obf.RemoveField(field);
                }
                else
                {
                    _orderByField = null;
                }
            }
        }

        public DocumentCollector AddAggregateFunction(AggregateFunctionType type, IEvaluable field)
        {
            if (_aggregations == null) _aggregations = new List<DocumentCollector>();
            IAggregation function;
            switch (type)
            {
                case AggregateFunctionType.AVG:
                    function = new AVG();
                    break;
                case AggregateFunctionType.COUNT:
                    function = new COUNT();
                    break;
                case AggregateFunctionType.MAX:
                    function = new MAX();
                    break;
                case AggregateFunctionType.MIN:
                    function = new MIN();
                    break;
                case AggregateFunctionType.SUM:
                    function = new SUM();
                    break;
                    //case AggregateFunctionType.FIRST:
                    //    function = new FIRST(field);
                    //    break;
                    //case AggregateFunctionType.LAST:
                    //    function = new LAST(field);
                    //    break;
                default:
                    throw new QuerySystemException(ErrorCodes.Query.AGGREGATION_INVALID_FUNCTION);
            }
            var aggregator = new DocumentCollector(function, field);
            _aggregations.Add(aggregator);
            return aggregator;
        }

        public bool ContainsDistinction
        {
            get { return _distinctField != null; }
        }

        public bool ContainsLimit
        {
            get { return _limit != long.MinValue; }
        }

        public bool ContainsOrder
        {
            get { return _orderByField != null; }
        }

        public bool IsGrouped
        {
            get { return _groupByField != null; }
        }

        public bool ContainsAggregations
        {
            get { return _aggregations != null; }
        }

        public bool GetAllFields
        {
            get { return _getAllFields; }
            set { _getAllFields = value; }
        }

        public IQueryStore Store
        {
            get { return _store; }
            set { _store = value; }
        }

        public UpdateOption UpdateOption
        {
            get { return _updateOption; }
            set { _updateOption = value; }
        }

        public JSONDocument NewDocument
        {
            get { return newDocument; }
            set { newDocument = value; }
        }

        public bool SortResult
        {
            get { return sortResult; }
            set { sortResult = value; }
        }


        public long QueryId
        {
            get { return queryId; }
            set { queryId = value; }
        }

        public IDataTransform GeTransform()
        {
            //if (IsGrouped)
            //    return new GroupTransform(this);
            return new SelectTransform(this);
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("QueryCriteria:{");
            output.Write("Projections=[");
            for (int i = 0; i < _projections.Count; i++)
            {
                _projections[i].Print(output);
                if (i != _projections.Count - 1)
                    output.Write(",");
            }
            output.Write("]");
            output.Write(",Aggregations=[");
            if (_aggregations != null)
            {
                for (int i = 0; i < _aggregations.Count; i++)
                {
                    _aggregations[i].Print(output);
                    if (i != _aggregations.Count - 1)
                        output.Write(",");
                }
            }
            else
            {
                output.Write("null");
            }
            output.Write("]");

            output.Write(",GroupByField=");
            if (_groupByField != null)
            {
                _groupByField.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",OrderByField=");
            if (_orderByField != null)
            {
                _orderByField.Print(output);
            }
            else
            {
                output.Write("null");
            }

            output.Write(",DistinctField=");
            if (_distinctField != null)
            {
                _distinctField.Print(output);
            }
            else
            {
                output.Write("null");
            }

            output.Write(",Store=");
            if (_store != null)
            {
                _store.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",SubstituteStore=");
            if (_substituteStore != null)
            {
                _substituteStore.Print(output);
            }
            else
            {
                output.Write("null");
            }

            output.Write(",Update=");
            if (_documentUpdate != null)
            {
                _documentUpdate.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",NewDocuemnt=");
            output.Write(newDocument != null ? newDocument.ToString() : "null");
            output.Write(",UpdateOption=" + _updateOption);
            output.Write(",SortResult=" + sortResult);
            output.Write(",QueryID=" + queryId);
            output.Write("}");
        }
    }
}
