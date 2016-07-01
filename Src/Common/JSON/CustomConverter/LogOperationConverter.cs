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

﻿using Alachisoft.NosDB.Common.Server.Engine;
﻿using Alachisoft.NosDB.Common.Server.Engine.Impl;

﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Alachisoft.NosDB.Common.JSON.CustomConverter
{
    class LogOperationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(WriteQueryOperation).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            DatabaseOperationType dbOperationType = (DatabaseOperationType)((int)jObject["OpT"]);
            switch (dbOperationType)
            {
                case DatabaseOperationType.WriteQuery:
                    WriteQueryOperation writeQueryOperation = new WriteQueryOperation();
                    serializer.Populate(jObject.CreateReader(), writeQueryOperation);
                    return writeQueryOperation;
                default:
                    return serializer.Deserialize(reader);
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value is WriteQueryOperation)
            {
                WriteQueryOperation writeQueryOperation = value as WriteQueryOperation;
                if (writeQueryOperation != null)
                {
                    //writer.WriteStartObject();
                    writer.WritePropertyName("col");
                    serializer.Serialize(writer, writeQueryOperation.Collection);
                    writer.WritePropertyName("DB");
                    serializer.Serialize(writer, writeQueryOperation.Database);
                    writer.WritePropertyName("Query");
                    serializer.Serialize(writer, writeQueryOperation.Query);
                    writer.WritePropertyName("OpT");
                    serializer.Serialize(writer, writeQueryOperation.OperationType);
                    //writer.WriteEndObject();
                }
            }
            //else if (value is QueryLogOperation)
            //{
            //    QueryLogOperation operation = value as QueryLogOperation;
            //    writer.WritePropertyName("col");
            //    serializer.Serialize(writer, operation.Collection);
            //    writer.WritePropertyName("DB");
            //    serializer.Serialize(writer, operation.Database);
            //    writer.WritePropertyName("Operation");
            //    serializer.Serialize(writer, operation.Operation);
            //    writer.WritePropertyName("OpId");
            //    serializer.Serialize(writer, operation.OperationId);
            //    writer.WritePropertyName("OperationIdForIndex");
            //    serializer.Serialize(writer, operation.OperationIdForIndex);
            //    writer.WritePropertyName("OpT");
            //    serializer.Serialize(writer, operation.OperationType);
            //    writer.WritePropertyName("perf");
            //    serializer.Serialize(writer, operation.Performed);
            //    writer.WritePropertyName("rep");
            //    serializer.Serialize(writer, operation.Replicated);
            //    writer.WritePropertyName("lOpT");
            //    serializer.Serialize(writer, operation.LogOperationType);
            //    writer.WritePropertyName("req");
            //    serializer.Serialize(writer, operation.RequestId);
            //    writer.WritePropertyName("EId");
            //    serializer.Serialize(writer, operation.ElectionId);
            //    writer.WritePropertyName("ESeqId");
            //    serializer.Serialize(writer, operation.ElectionBasedSequenceId);
            //    //writer.WriteEndObject();
            //}
            
            writer.WriteEndObject();
        }
    }
}
