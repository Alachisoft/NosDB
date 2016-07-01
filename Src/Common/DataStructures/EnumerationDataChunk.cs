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
using System.Text;
#if JAVA
using Alachisoft.TayzGrid.Runtime.Serialization;
#else
using Alachisoft.NosDB.Common.Serialization;
#endif
#if JAVA
using Runtime = Alachisoft.TayzGrid.Runtime;
#else
using Runtime = Alachisoft.NosDB.Common;
#endif
namespace Alachisoft.NosDB.Common.DataStructures
{
    public class EnumerationDataChunk : ICompactSerializable
    {
        private List<string> _data;
        private EnumerationPointer _pointer;

        public List<string> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public EnumerationPointer Pointer
        {
            get { return _pointer; }
            set { _pointer = value; }
        }

        public bool IsLastChunk
        {
            get { return _pointer.HasFinished; }
        }

        #region ICompactSerializable Members

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            _data = (List<string>)reader.ReadObject();
            _pointer = (EnumerationPointer)reader.ReadObject();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_data);
            writer.WriteObject(_pointer);
        }

        #endregion
    }
}
