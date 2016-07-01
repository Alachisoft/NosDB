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
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Filters.Aggregations
{
    public class DocumentCollector: IPrintable
    {
        private IAggregation _aggregation;
        private IEvaluable _field;

        public DocumentCollector(IAggregation aggregation, IEvaluable field)
        {
            _aggregation = aggregation;
            _field = field;
        }

        public string FieldName { get { return _aggregation.Name + "(" + _field.ToString() + ")"; } }

        public IAggregation Aggregation { get { return _aggregation; } set { _aggregation = value; } }

        public IEvaluable Evaluation { get { return _field; } set { _field = value; } }

        public IAggregation NewInstance
        {
            get { return Activator.CreateInstance(_aggregation.GetType()) as IAggregation; }
        }

        public void Reset()
        {
            _aggregation = NewInstance;
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Aggregator:{");
            output.Write("Aggregation:" + (_aggregation != null ? _aggregation.Name : "null"));
            output.Write(",Projection=");
            if (_field != null)
            {
                _field.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }
    }
}
