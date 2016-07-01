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
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Alachisoft.NosDB.Common.JSON.CustomConverter
{
    class ParameterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Parameter).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            Parameter parameter = new Parameter();
            serializer.Populate(jObject.CreateReader(), parameter);
            return parameter;
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            Parameter parameter = value as Parameter;
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            serializer.Serialize(writer, parameter.Name);
            writer.WritePropertyName("Value");
            if (parameter.Value is JSONDocument)
                serializer.Serialize(writer, parameter.Value.ToString());
            else
                serializer.Serialize(writer, parameter.Value);
            writer.WriteEndObject();
        }
    }
}
