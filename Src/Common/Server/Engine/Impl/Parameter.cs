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

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class Parameter : IParameter, ICompactSerializable
    {
        private string _name;
        private object _value;

        public Parameter() { }

        public Parameter(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #region ICompactSerializable
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadString();
            Value = reader.ReadObject();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(Name);
            writer.WriteObject(Value);
        }
        #endregion
    }
}
