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
    /// <summary>
    /// Recovery Operations 
    /// </summary>
    public class RecoveryOperation : ICompactSerializable
    {
        private string _identifer;
        private RecoveryOpCodes _opCode;
        private object _parameter=null;

        public RecoveryOperation()
        {
           
        }            

        #region properties
        public RecoveryOpCodes OpCode
        {
            get { return _opCode; }
            set { _opCode = value; }
        }

        public string JobIdentifer
        {
            get { return _identifer; }
            set { _identifer = value; }
        }
              
        public object Parameter
        {
            get { return _parameter; }
            set { _parameter = value; }
        }
        #endregion

        #region ICompact Serializable
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            _identifer = reader.ReadString();
            _opCode = (RecoveryOpCodes)reader.ReadObject();
            _parameter = reader.ReadObject();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(_identifer);
            writer.WriteObject(_opCode);
            writer.WriteObject(_parameter);         
        }
        #endregion
    }
        
}
