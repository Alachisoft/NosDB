using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Protobuf.ManagementCommands
{
    public partial class CommandBase :  IRequest, ICompactSerializable, IResponse
    {
        object _responseMsg;
        object _request;

        #region ICompactSerializable Member
        public void Deserialize(Serialization.IO.CompactReader reader)
        {           
            Message = reader.ReadObject();
            NoResponse = reader.ReadBoolean();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {         
            writer.WriteObject(Message);
            writer.Write(NoResponse);
        } 
        #endregion

        #region IRequest
        long IRequest.RequestId
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
            get
            {
                if (this.CommandType == CommandBase.Types.CommandType.COMMAND)
                    return _request;
                else
                    return _responseMsg;
            }
            set
            {
                if (this.CommandType == CommandBase.Types.CommandType.COMMAND)
                     _request = value ;
                else
                     _responseMsg = value;

            }
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

        public Net.Address Source
        {
            get;
            set;
        } 
        #endregion


        public object ResponseMessage
        {
            get
            {
                if (this.CommandType == CommandBase.Types.CommandType.COMMAND)
                    return _request;
                else
                    return _responseMsg;
            }
            set
            {
                if (this.CommandType == CommandBase.Types.CommandType.COMMAND)
                    _request = value;
                else
                    _responseMsg = value;

            }
        }


        public System.Exception Error
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }
    }
}
