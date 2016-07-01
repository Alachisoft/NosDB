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
    public class AVG : IAggregation
    {
        private double sum = 0;
        private int count = 0;

        public string Tag
        {
            get { return Statics.AVG; }
        }

        public object Value
        {
            get { return new[] {sum, count}; }
            set
            {
                try
                {
                    var array = ((double[]) value);
                    sum = (double) array[0];
                    count = (int) array[1];
                }
                catch
                {
                    sum = 0;
                    count = 0;
                }
            }
        }


        public Common.Enum.AggregateFunctionType Type
        {
            get { return AggregateFunctionType.AVG; }
        }

        public object Clone()
        {
            return new AVG {sum = sum, count = count};
        }

        public bool Distinct { get; set; }

        public string Name
        {
            get { return Tag; }
        }

        public void ApplyValue(params object[] values)
        {
            if (values != null && values.Length>0)
            {
                if (values[0] != null)
                {
                    FieldDataType type = JSONType.GetJSONType(values[0]);
                    switch (type)
                    {
                            case FieldDataType.Number:
                            sum += Convert.ToDouble(values[0]);
                            count++;
                            break;
                            case FieldDataType.DateTime:
                            sum += Convert.ToDateTime(values[0]).ToOADate();
                            count++;
                            break;
                    }
                }
            }
        }
    }
}
