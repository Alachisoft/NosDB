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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.RPCFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Protobuf.ManagementCommands
{
    public class ManagementCommand:  IRequest, Common.Serialization.ICompactSerializable
    {
        private TargetMethodParameter _parameters = new TargetMethodParameter();
        private long requestId_;
        private int _commandVersion;
        private string _methodName;

        public TargetMethodParameter Parameters
        {
            get { return _parameters; }
            private set
            {
                _parameters = value;
            }
        }

        public long RequestId
        {
            get
            {
                return requestId_;
            }
            set
            {
                requestId_ = value;
            }
        }

        public object Message
        {
            get;
            set;
        }

        public bool NoResponse
        {
            get;
            set;
        }

        public IChannel Channel
        {
            get;
            set;
        }

         Net.Address IChannelMessage.Source
        {
            get;
            set;
        }
        
        public  int Overload { get; set; }

        public int CommandVersion
        {
            get
            {
                return _commandVersion;
            }

            set
            {
                _commandVersion = value;

            }
        }

        public string MethodName {
            get
            {
                return _methodName;
            }

            set
            {
                _methodName = value;

            }
        }

        #region CompactSerializable Members
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            RequestId =  reader.ReadInt64();
            Message = reader.ReadObject();
            NoResponse = reader.ReadBoolean();
            MethodName = reader.ReadObject() as string;
            Parameters = reader.ReadObject() as TargetMethodParameter;
            Overload = reader.ReadInt32();
            CommandVersion = reader.ReadInt32();
        }
        

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(RequestId);
            writer.WriteObject(Message);
            writer.Write(NoResponse);
            writer.WriteObject(MethodName);
            writer.WriteObject(Parameters);
            writer.Write(Overload);
            writer.Write(CommandVersion);
        }
        #endregion



        

       
    }
}
