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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Results.Transforms
{
    public class SelectTransform : IDataTransform
    {
        private QueryCriteria _criteria;

        public SelectTransform(QueryCriteria criteria)
        {
            _criteria = criteria;
        }

        public virtual IJSONDocument Transform(IJSONDocument document)
        {
            IJSONDocument newDocument;
            if (_criteria.GetAllFields)
                newDocument = document.Clone() as IJSONDocument;
            else
                newDocument = JSONType.CreateNew();

            //if (!_criteria.IsGrouped)
            //    newDocument.Key = document.Key;

            for (int i = 0; i < _criteria.ProjectionCount; i++)
            {
                IEvaluable field = _criteria[i];
                IJsonValue finalValue;

                if (field.Evaluate(out finalValue, document))
                {
                    //newDocument[field.ToString()] = finalValue.Value;
                    var binaryExpression = field as BinaryExpression;

                    if (binaryExpression != null)
                    {
                        if (binaryExpression.Alias != null)
                        {
                            newDocument[binaryExpression.Alias] = finalValue.Value;
                            continue;
                        }
                    }
                    newDocument[field.CaseSensitiveInString] = finalValue.Value;
                }
                else return null;
            }

            if (_criteria.ContainsOrder && !_criteria.IsGrouped)
                _criteria.OrderByField.FillWithAttributes(document, newDocument);
            else if (_criteria.IsGrouped)
                _criteria.GroupByField.FillWithAttributes(document, newDocument);

            if (newDocument.Count == 0)
                return null;

            return newDocument;
        }
    }
}
