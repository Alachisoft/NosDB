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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Common.JSON.CustomConverter
{
    class QueryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Query).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            Query query = new Query();
            serializer.Populate(jObject.CreateReader(), query);
            return query;
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
            Query query = value as Query;
            writer.WriteStartObject();
            writer.WritePropertyName("Query");
            serializer.Serialize(writer, query.QueryText);
            //writer.WriteEndObject();
            writer.WritePropertyName("Parameters");
            writer.WriteStartArray();
            if (query.Parameters == null)
                query.Parameters = new List<IParameter>(); 
            foreach (Parameter parameter in query.Parameters)
            {
                //writer.WriteStartArray();
                serializer.Serialize(writer, parameter);
                //writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
