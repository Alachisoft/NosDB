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

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public static class Statics
    {
        public const string COUNT = "$count";
        public const string MIN = "$min";
        public const string MAX = "$max";
        public const string AVG = "$avg";
        public const string SUM = "$sum";
        public const string FIRST = "$first";
        public const string LAST = "$last";

        public static string GetAggregateFuncationTag(AggregateFunctionType type)
        {
            switch (type)
            {
                case AggregateFunctionType.AVG:
                    return AVG;
                case AggregateFunctionType.COUNT:
                    return COUNT;
                case AggregateFunctionType.MAX:
                    return MAX;
                case AggregateFunctionType.MIN:
                    return MIN;
                case AggregateFunctionType.SUM:
                    return SUM;
                case AggregateFunctionType.FIRST:
                    return FIRST;
                case AggregateFunctionType.LAST:
                    return LAST;
                default:
                    return null;
            }
        }
    }
}
