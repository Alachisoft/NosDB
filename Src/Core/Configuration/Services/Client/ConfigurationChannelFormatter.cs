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
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Serialization.Formatters;

namespace Alachisoft.NosDB.Core.Configuration.Services.Client
{
    public class ConfigurationChannelFormatter : IChannelFormatter
    {
        #region IChannelFormatter Members
        public byte[] Serialize(object graph)
        {
            try
            {

                IChannelMessage command = graph as IChannelMessage;
                return CompactBinaryFormatter.ToByteBuffer(command, null);
            }
            catch (Exception ex)
            {
                throw ex;
                     
            }
            //byte[] buffer = null;
            //CommandBase command = null;

            //if (graph is IRequest)
            //{
            //    IRequest request = graph as IRequest;
            //    command = request.Message as CommandBase;
            //}

            //else if (graph is IResponse)
            //{
            //    ChannelResponse res = graph as ChannelResponse;
            //    command = res.ResponseMessage as CommandBase;
            //}



            //if (command != null)
            //{
            //    if (command.commandType == CommandBase.CommandType.RESPONSE)
            //    {
            //        if (command.response.ResponseMessage != null)
            //            command.response.returnVal = CompactBinaryFormatter.ToByteBuffer(command.response.ResponseMessage, null);
            //    }
            //    else
            //    {
            //        if (command.command.Parameters != null)
            //            command.command.arguments = CompactBinaryFormatter.ToByteBuffer(command.command.Parameters, null);
            //    }


            //    using (MemoryStream stream = new MemoryStream())
            //    {
            //        ProtoBuf.Serializer.Serialize<CommandBase>(stream, command);
            //        buffer = stream.ToArray();
            //    }
            //}


            //return buffer;


        }

        public object Deserialize(byte[] buffer)
        {
            IChannelMessage response = null;

            if (buffer != null)
            {
                response = (IChannelMessage)CompactBinaryFormatter.FromByteBuffer(buffer, null);
            }
            return response;

            //CommandBase command = null;

            //using (MemoryStream stream = new MemoryStream(buffer))
            //{
            //    command = ProtoBuf.Serializer.Deserialize<CommandBase>(stream);
            //}

            //if (command.commandType == CommandBase.CommandType.RESPONSE)
            //{
            //    if (command.response.returnVal != null)
            //        command.response.ResponseMessage = CompactBinaryFormatter.FromByteBuffer(command.response.returnVal, null);
            //}
            //return command;

        } 
        #endregion
    }
}
