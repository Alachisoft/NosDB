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
using System.IO;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Storage.Indexing;

namespace Alachisoft.NosDB.Common.Queries.Filters
{
    public class FilterPredicate : TerminalPredicate
    {
        private readonly ITreePredicate _condition;
        private IJSONDocument _filterDocument;

        public FilterPredicate(ITreePredicate condition, IIndex sourceIndex)
        {
            _condition = condition;
            source = sourceIndex;
        }

        private void PopulateDocument(AttributeValue value, IndexAttribute Attributes)
        {
            string fieldName;
            if (value.DataType != FieldDataType.Empty)
                switch (value.Type)
                {
                    case AttributeValueType.Null:

                        _filterDocument[Attributes.Name] = null;

                        break;
                    case AttributeValueType.Single:
                        var singleAttValue = value as SingleAttributeValue;
                        if (singleAttValue != null)
                        {
                            fieldName = Attributes.Name;
                            _filterDocument[fieldName] = singleAttValue.Value;
                        }
                        break;
                }
        }

        public override IEnumerable<KeyValuePair<AttributeValue, long>> Enumerate(QueryCriteria value)
        {
            _filterDocument = JSONType.CreateNew();
            if (_childPredicates.Count > 0)
            {
                foreach (var childPredicate in _childPredicates)
                {
                    var predicate = childPredicate as TerminalPredicate;
                    if (predicate != null)
                    {
                        foreach (var kvp in predicate.Enumerate(value))
                        {
                            _filterDocument.Clear();
                            PopulateDocument(kvp.Key, predicate.Source.Attributes);
                            if (_condition.IsTrue(_filterDocument)) yield return kvp;
                        }
                    }
                }
            }
        }

        public override double SelectionCardinality
        {
            get
            {
                double totalCardinality = base.SelectionCardinality;

                Dictionary<string, int> positionMap = new Dictionary<string, int>();
                
                    positionMap.Add(source.Attributes.Name, 0);

                foreach (ComparisonPredicate expression in _condition.AtomicTreePredicates)
                {
                    if (expression != null)
                    {
                        try
                        {
                            double dmax =
                                Convert.ToDouble(source.Max(positionMap[expression.AttributeNames[0]],
                                    FieldDataType.Number));
                            double dmin =
                                Convert.ToDouble(source.Min(positionMap[expression.AttributeNames[0]],
                                    FieldDataType.Number));

                            //ARKT, you need to assign the index source in filter predicate for this, otherwise its nullreferenceexception
                            switch (expression.Condition)
                            {

                                case Condition.IsNull:
                                case Condition.Like:
                                case Condition.Equals:
                                    totalCardinality += source.ValueCount/source.KeyCount;
                                    break;

                                case Condition.IsNotNull:
                                case Condition.NotLike:
                                case Condition.NotEquals:
                                    totalCardinality += source.ValueCount;
                                    break;

                                case Condition.GreaterThan:
                                    double value =
                                        Convert.ToDouble(((IJsonValue) expression.ConstantValues[0]).Value);
                                    totalCardinality += source.ValueCount*((dmax - value)/(dmax - dmin));
                                    break;

                                case Condition.GreaterThanEqualTo:
                                    value = Convert.ToDouble(((IJsonValue) expression.ConstantValues[0]).Value);
                                    totalCardinality += source.ValueCount*((dmax - value)/(dmax - dmin)) + 1;
                                    break;

                                case Condition.LesserThan:
                                    value = Convert.ToDouble(((IJsonValue) expression.ConstantValues[0]).Value);
                                    totalCardinality += source.ValueCount*((value - dmin)/(dmax - dmin));
                                    break;

                                case Condition.LesserThanEqualTo:
                                    value = Convert.ToDouble(((IJsonValue) expression.ConstantValues[0]).Value);
                                    totalCardinality += source.ValueCount*((value - dmin)/(dmax - dmin)) + 1;
                                    break;

                                    //case Condition.In:

                                    //    break;

                                    //case Condition.NotIn:

                                    //    break;


                                case Condition.Between:
                                    double startValue =
                                        Convert.ToDouble(((IJsonValue) expression.ConstantValues[0]).Value);
                                    double endValue =
                                        Convert.ToDouble(((IJsonValue) expression.ConstantValues[1]).Value);
                                    totalCardinality += source.ValueCount*
                                                        ((endValue - startValue)/(dmax - dmin));
                                    break;

                                case Condition.NotBetween:
                                    startValue =
                                        Convert.ToDouble(((IJsonValue) expression.ConstantValues[0]).Value);
                                    endValue =
                                        Convert.ToDouble(((IJsonValue) expression.ConstantValues[1]).Value);
                                    totalCardinality += source.ValueCount*((dmax - endValue)/(dmax - dmin));
                                    totalCardinality += source.ValueCount*((startValue - dmin)/(dmax - dmin));
                                    break;
                            }
                        }
                        catch
                        {
                            totalCardinality += source.ValueCount;
                        }
                    }
                }
                return totalCardinality;


            }
        }


        public override void Print(TextWriter output)
        {
            output.Write("FilterPredicate:{");
            base.Print(output);
            output.Write(",Validator=");
            if (_condition != null)
            {
                _condition.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }
    }
}
