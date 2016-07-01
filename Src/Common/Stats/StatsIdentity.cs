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
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Stats
{
    public class StatsIdentity
    {
        public string ShardName { get; set; }
        public string DatabaseName { get; set; }

        public StatsIdentity(string shardName, string databaseName)
        {
            ShardName = shardName;
            DatabaseName = databaseName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash*31 + ShardName.GetHashCode();
                hash = hash * 31 + DatabaseName.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            StatsIdentity other = obj as StatsIdentity;
            if (other == null) return false;

            if (this.ShardName.Equals(other.ShardName) && this.DatabaseName.Equals(other.DatabaseName)) return true;

            return false;
        }
    }
}
