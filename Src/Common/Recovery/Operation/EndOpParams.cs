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
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Recovery.Operation
{
    public class EndOpParams : ICompactSerializable
    {
        private string _database;
        List<DiffTrackObject> trackObjects;

        public EndOpParams(string database)
        {
            _database = database;
            trackObjects = new List<DiffTrackObject>();
        }

        public string Database
        {
            get { return _database; }
        }

        public List<DiffTrackObject> TrackObjects
        {
            get { return trackObjects; }
            set { trackObjects = value; }
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _database = reader.ReadString();
            trackObjects = Util.SerializationUtility.DeserializeList<DiffTrackObject>(reader);
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_database);
            Util.SerializationUtility.SerializeList<DiffTrackObject>(trackObjects, writer);
        }
    }
}
