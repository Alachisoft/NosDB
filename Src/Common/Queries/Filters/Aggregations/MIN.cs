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
    public class MIN : IAggregation
    {
        private object _min;
        private byte valueCount = 0;
        
        public string Tag
        {
            get { return Statics.MIN; }
        }
        
        public object Value
        {
            get { return _min; }
            set { _min = value; }
        }
        
        public Common.Enum.AggregateFunctionType Type
        {
            get { return AggregateFunctionType.MIN; }
        }
        
        public object Clone()
        {
            return new MIN() {_min = _min};
        }

        public bool Distinct { get; set; }
        
        public string Name
        {
            get { return Tag; }
        }

        public void ApplyValue(params object[] values)
        {
            if(valueCount == 0)
            {
                _min = values[0];
                valueCount++;
                return;
            }
            if (values != null && values.Length > 0)
            {
                if (values[0] != null)
                {
                    if (JSONComparer.Compare((IComparable) _min, values[0]) > 0)
                    {
                        _min = values[0];
                    }
                }
            }
        }
    }
}
