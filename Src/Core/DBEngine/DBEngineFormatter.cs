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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Protobuf;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using System;

namespace Alachisoft.NosDB.Core.DBEngine
{
    public class DbEngineFormatter : IChannelFormatter
    {
        public byte[] Serialize(object graph)
        {
            try
            {
                IDBResponse dbResponse = graph as IDBResponse;

                return dbResponse != null ? dbResponse.Serialize() : null;
            }

            catch (Exception e )
            {
                if (LoggerManager.Instance.ShardLogger != null && LoggerManager.Instance.ShardLogger.IsErrorEnabled)
                    LoggerManager.Instance.ShardLogger.Error("Error: ClientMgr.OnRequest() Serialize Response",
                        e.Message + " StackTrace:" + e.StackTrace);
                throw;
            }
        }

        public object Deserialize(byte[] buffer)
        {
            return new RequestParser(buffer);
        }
    }
    
}
