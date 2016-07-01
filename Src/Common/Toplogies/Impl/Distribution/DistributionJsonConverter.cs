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
using Alachisoft.NosDB.Common.JSON.CustomConverter;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution.NonShardedDistributionStrategy;
using Newtonsoft.Json.Linq;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution
{
    public class DistributionJsonConverter:JsonCreationConverter<IDistribution>
    {
        protected override IDistribution Create(Type objecType, Newtonsoft.Json.Linq.JObject jObject)
        {
            if (GetStrategyName(jObject).Equals(DistributionName.NonShardedDistribution.ToString()))
            {
                return new NonShardedDistribution();
            }


            throw new Exception("Unknown distribution strategy");
        }

        private string GetStrategyName(JObject jObject)
        {
            return jObject["Name"] != null ? jObject.GetValue("Name").ToString() as string : null;
        }
    }
}
