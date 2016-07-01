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
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Security.Impl.Enums
{
    public enum ResourceType
    {
        //add any new resource at the end, renamed resource's position should not be changed, serialization/deserialization issue
        System = 1,
        ConfigurationCluster = 2,
        Cluster = 3,
        Database = 4,
        User = 5,
        Trigger = 6,
        UserDefinedFunction = 7,
        Collection = 8,
        Index = 9,
        StoreProcedure = 10,
        Role = 11,
        Operation = 12
    }
}
