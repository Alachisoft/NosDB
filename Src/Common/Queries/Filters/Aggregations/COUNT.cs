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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Queries.Util;

namespace Alachisoft.NosDB.Common.Queries.Filters.Aggregations
{
    public class COUNT : IAggregation
    {
        private long _count = 0;
        
        public string Tag
        {
            get { return Statics.COUNT; }
        }
     
        public object Value
        {
            get { return _count; }
            set { _count = (long) value; }
        }
        
        public Common.Enum.AggregateFunctionType Type
        {
            get { return AggregateFunctionType.COUNT; }
        }


        public object Clone()
        {
           return new COUNT {_count = _count};
        }
       
        public bool Distinct { get; set; }

        public string Name
        {
            get { return Tag; }
        }

        public void ApplyValue(params object[] values)
        {
            _count++;
        }
    }
}
