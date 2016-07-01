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
namespace Alachisoft.NosDB.Core.Configuration.Recovery
{
    internal class DIFKey
    {
        internal string Database { get; set; }
        internal string Cluster { get; set; }

        public override bool Equals(object obj)
        {
            bool state = false;
            if (obj == null || GetType() != obj.GetType())
                return false;
            DIFKey _val = obj as DIFKey;

            if (Database.Equals(_val.Database))
                if (Cluster.Equals(_val.Cluster))
                    state = true;


            return state;
        }

        public override int GetHashCode()
        {
            return Database.GetHashCode() + Cluster.GetHashCode();
        }

    }
}