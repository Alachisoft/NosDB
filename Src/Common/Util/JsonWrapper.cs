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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Server.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alachisoft.NosDB.Common.Util
{
    public class JsonWrapper
    {
        public static IJsonValue Wrap(object value)
        {
            if (value is JValue)
                value = ((JValue)value).Value;

            FieldDataType dataType = JSONType.GetJSONType(value);
            IJsonValue jsonValue = null;
            switch (dataType)
            {
                case FieldDataType.Array:
                    var values = new List<IJsonValue>();
                    if (value is System.Collections.ArrayList)
                    {
                        var array = (System.Collections.ArrayList)value;
                        foreach (var obj in array)
                            values.Add(Wrap(obj));
                    }
                    else 
                    {
                        var array = (Array)value;
                        foreach (var obj in array)
                            values.Add(Wrap(obj));
                    }
                    jsonValue = new ArrayJsonValue(values.ToArray());
                    break;
                case FieldDataType.Bool:
                    jsonValue=new BooleanJsonValue((bool) value);
                    break;
                case FieldDataType.DateTime:
                    jsonValue=new DateTimeJsonValue((DateTime) value);
                    break;
                case FieldDataType.Null:
                    jsonValue = new NullValue();
                    break;
                case FieldDataType.Number:
                    jsonValue = new NumberJsonValue(value);
                    break;
                case FieldDataType.Object:
                    if (value.GetType() == typeof(JArray))
                    {
                        var arr= (JArray)value;
                        var vals= new List<IJsonValue>();
                        foreach (var obj in arr)
                        {
                            vals.Add(Wrap(obj));
                        }
                        jsonValue = new ArrayJsonValue(vals.ToArray());
                    }
                    else if (value.GetType() == typeof(IJsonValue[]))
                    {
                        var arr = (IJsonValue[])value;
                        //var vals = new List<IJsonValue>();
                        //foreach (var obj in arr)
                        //{
                        //    vals.Add(Wrap(obj));
                        //}
                        jsonValue = new ArrayJsonValue(arr);//vals.ToArray());
                    }
                    else
                        jsonValue = new ObjectJsonValue(Serialize(value));
                    break;
                case FieldDataType.String:
                    jsonValue=new StringJsonValue((string) value);
                    break;
            }
            return jsonValue;
        }

        public static IJsonValue Wrap(IJSONDocument document, string name)
        {
            return null;
            //IJsonValue result = null;
            //Attributor attributor = new Attributor(name);
            //object finalValue;
            //if (attributor.TryGet(document, out finalValue))
            //{
            //    result = Wrap(finalValue);
            //}
            //ExtendedJSONDataTypes dataType = document.GetAttributeDataType(name);
            //switch (dataType)
            //{
            //    case ExtendedJSONDataTypes.Array:
            //        result = new ArrayJsonValue(document.GetArray(name));
            //        break;
            //    case ExtendedJSONDataTypes.Object:
            //        result = new ObjectJsonValue(document.GetDocument(name));
            //        break;
            //    case ExtendedJSONDataTypes.Boolean:
            //        result = new BooleanJsonValue(document.GetBoolean(name));
            //        break;
            //    case ExtendedJSONDataTypes.DateTime:
            //        DateTime dateTime;
            //        document.TryGet(name, out dateTime);
            //        result = new DateTimeJsonValue(dateTime);
            //        break;
            //    case ExtendedJSONDataTypes.Number:
            //        object number;
            //        document.TryGet(name, out number);
            //        result = new NumberJsonValue(number);
            //        break;
            //    case ExtendedJSONDataTypes.String:
            //        result = new StringJsonValue(document.GetString(name));
            //        break;
            //    case ExtendedJSONDataTypes.Null:
            //        result = new NullValue();
            //        break;
            //}
            //return result;
        }

        public static object UnWrap(IJsonValue value)
        {
            object result = null;
            switch (value.DataType)
            {
                case FieldDataType.Array:
                    var array = (IJsonValue[]) value.Value;
                    var objects = new object[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        objects[i] = UnWrap(array[i]);
                    }
                    return objects;
                case FieldDataType.Bool:
                    result = (bool) value.Value;
                    break;
                case FieldDataType.DateTime:
                    result = (DateTime) value.Value;
                    break;
                case FieldDataType.Null:
                    break;
                case FieldDataType.Number:
                case FieldDataType.Object:
                case FieldDataType.String:
                    result = value.Value;
                    break;
            }
            return result;
        }


        public static IJSONDocument Serialize(object document)
        {
            var serialize = document as IJSONDocument;
            if (serialize != null)
                return serialize;

            string str = JsonConvert.SerializeObject(document);
            return JSONDocument.Parse(str);
        }
    }
}
