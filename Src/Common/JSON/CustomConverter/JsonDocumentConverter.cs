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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.JSON.CustomConverter
{
    public class JsonDocumentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JSONDocument).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            return (JSONDocument)JSONDocument.Parse(jObject.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (IsSimpleType(value))
            {
                writer.WriteValue(value);
            }
            else if (value is JSONDocument.JsonObject)
            {
                writer.WriteRawValue(((JSONDocument.JsonObject)value).JsonString);
            }
            else if (value is Array || value is ArrayList)
            {
                writer.WriteRawValue(JsonConvert.SerializeObject(value));
            }
            else
            {
                writer.WriteStartObject();
                JSONDocument document = (JSONDocument)value;
                IDictionary<string, object> values = document.ToDictionary();

                foreach (KeyValuePair<string, object> pair in values)
                {
                    if (IsSimpleType(pair.Value))
                    {
                        writer.WritePropertyName(pair.Key);
                        serializer.Serialize(writer, pair.Value);

                    }
                    else if (pair.Value is IJSONDocument)
                    {
                        writer.WritePropertyName(pair.Key);
                        writer.WriteRawValue(JsonConvert.SerializeObject(pair.Value));
                    }
                    else if (pair.Value is JSONDocument.JsonObject)
                    {
                        writer.WritePropertyName(pair.Key);
                        writer.WriteRawValue(((JSONDocument.JsonObject)pair.Value).JsonString);
                    }
                    else if (pair.Value is Array || pair.Value is ArrayList)
                    {
                        writer.WritePropertyName(pair.Key);
                        writer.WriteRawValue(JsonConvert.SerializeObject(pair.Value));
                    }
                    else
                    {
                        throw new Exception(string.Format("Type {0} is not supported on JSONDocument", pair.Value.GetType().FullName));
                    }

                }

                writer.WriteEndObject();
            }
              
        }

        private bool IsSimpleType(object value)
        {
            if (value == null)
                return true;

            Type type = value.GetType();
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] { 
				    typeof(String),
				    typeof(Decimal),
				    typeof(DateTime)
			    }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
       
    }
}
