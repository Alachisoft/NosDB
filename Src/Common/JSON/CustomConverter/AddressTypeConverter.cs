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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alachisoft.NosDB.Common.JSON.CustomConverter
{
    public class AddressTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string jsonString = value as string;
            if (jsonString == null)
                return null;
            if (jsonString.Contains(":"))
            {
                string[] split = jsonString.Split(':');
                return new Address(split[0], Int32.Parse(split[1]));
            }
            return base.ConvertFrom(context, culture, value);
        }

        //public override bool CanConvert(Type objectType)
        //{
        //    return (objectType == (typeof (IDictionary<,>)) &&
        //           objectType.GetGenericArguments()[0] == typeof (Address)) || typeof(Address).IsAssignableFrom(objectType);
        //    //return typeof(Address).IsAssignableFrom(objectType);
        //}

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        //{
        //    JObject jObject = JObject.Load(reader);
        //    return new Address(jObject["IP"].ToString(), Int32.Parse(jObject["Port"].ToString()));
        //}

        //public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        //{
        //    //Dictionary<A> address = value as Address;
        //    //if(address == null)
        //    //    return;
        //    //writer.WriteStartObject();
        //    //writer.WritePropertyName("IP");
        //    //serializer.Serialize(writer, address.ip);
        //    //writer.WritePropertyName("Port");
        //    //serializer.Serialize(writer, address.Port);
        //    //writer.WriteEndObject();
        //}
    }
}
