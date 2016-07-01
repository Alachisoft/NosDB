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
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    public class StateTransferOperation : IStateTransferOperation, ICompactSerializable
    {
        private StateTransferIdentity identity;
        private OperationParam param;
        private StateTransferOpCode opCode;

        public StateTransferOperation(StateTransferIdentity identity,StateTransferOpCode opCode,OperationParam param) 
        {
            this.identity = identity;
            this.opCode = opCode;
            this.param = param;
        }

        public StateTransferIdentity TaskIdentity
        {
            get { return identity; }
        }

        public StateTransferOpCode OpCode
        {
            get { return opCode; }
        }

        public OperationParam Params
        {
            get { return param; }
        }
        
        #region ICompactSerializable Implementation

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            identity =(StateTransferIdentity) reader.ReadObject();
            opCode = (StateTransferOpCode)reader.ReadByte();
            param = (OperationParam)reader.ReadObject();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(identity);
            writer.Write((byte)opCode);
            writer.WriteObject(param);
        }

        #endregion
    }
}
