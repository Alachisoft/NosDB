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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Queries.Util;

namespace Alachisoft.NosDB.Common.Queries.Filters.Aggregations
{
    public class SUM : IAggregation
    {
        private double _sum;

        public string Tag
        {
            get { return Statics.SUM; }
        }

        public object Value
        {
            get { return _sum; }
            set { _sum = (double) value; }
        }

        public Common.Enum.AggregateFunctionType Type
        {
            get { return AggregateFunctionType.SUM; }
        }

        public object Clone()
        {
            return new SUM {_sum = _sum};
        }
        
        public bool Distinct { get; set; }

        public string Name
        {
            get { return Tag; }
        }

        public void ApplyValue(params object[] values)
        {
            if (values != null && values.Length > 0)
            {
                if (values[0] != null)
                {
                    FieldDataType type = JSONType.GetJSONType(values[0]);
                    switch (type)
                    {
                            case FieldDataType.Number:
                            _sum += Convert.ToDouble(values[0]);
                            break;
                        case FieldDataType.DateTime:
                            _sum += Convert.ToDateTime(values[0]).ToOADate();
                            break;
                    }
                }
            }
        }
    }
}
