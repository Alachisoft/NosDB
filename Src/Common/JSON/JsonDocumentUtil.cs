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

namespace Alachisoft.NosDB.Common.JSON
{
    public class JsonDocumentUtil
    {
        public static string DocumentKeyAttribute { get { return "_key"; } }

        public static IJsonValue[] GetArray(IJSONDocument document, string attribute)
        {
            if (document.GetAttributeDataType(attribute) != ExtendedJSONDataTypes.Array)
                return null;
            return ToIJsonList((IEnumerable)document[attribute]).ToArray();
        }

        public static List<IJsonValue> ToIJsonList(IEnumerable arrayList)
        {
            List<IJsonValue> jsonList = new List<IJsonValue>();
            foreach (var value in arrayList)
            {
                if (value == null)
                    jsonList.Add(new NullValue());
                else if (IsNumber(value))
                    jsonList.Add(new NumberJsonValue(value));
                else if (value is bool)
                    jsonList.Add(new BooleanJsonValue((bool)value));
                else if (value is string)
                    jsonList.Add(new StringJsonValue(value as string));
                else if (value is DateTime)
                    jsonList.Add(new DateTimeJsonValue((DateTime)value));
                else if (value is Array || value is ArrayList)
                    jsonList.Add(new ArrayJsonValue(ToIJsonList((IEnumerable)value).ToArray()));
                else
                    jsonList.Add(new ObjectJsonValue((JSONDocument)value));
            }
            return jsonList;
        }

        public static bool IsNumber(object value)
        {
            if (value == null)
                return false;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                //case TypeCode.UInt64:
                //case TypeCode.Decimal:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        public static void Update(IJSONDocument document, IJSONDocument update)
        {
            if (update == null)
                throw new ArgumentNullException("update", "Value cannot be null");

            foreach (string attribute in update.GetAttributes())
            {
                Queue<string> attributeQueue = new Queue<string>(attribute.Split('.'));
                ExpandAttribute(document, attributeQueue, update[attribute]);
                
            }
        }

        private static void ExpandAttribute(IJSONDocument source, Queue<string> attributeQueue, object newValue)
        {
            string currentAttribute = attributeQueue.Dequeue();
            bool lastAttribute = attributeQueue.Count == 0;
            if (lastAttribute)
            {
                source[currentAttribute] = newValue;
            }
            else
            {
                if (source.Contains(currentAttribute))
                {
                    ExtendedJSONDataTypes type = source.GetAttributeDataType(currentAttribute);
                    switch (type)
                    {
                        case ExtendedJSONDataTypes.Object:
                            //Recurecurecurecurecurecurecurecurecurecurecurecurecurecurecursion
                            ExpandAttribute(source.GetDocument(currentAttribute), attributeQueue, newValue);
                            break;
                        default:
                            IJSONDocument subDocument = JSONType.CreateNew();
                            source[currentAttribute] = subDocument;
                            ExpandAttribute(subDocument, attributeQueue, newValue);
                            break;
                    }
                }
                else
                {
                    IJSONDocument subDocument = JSONType.CreateNew();
                    source[currentAttribute] = subDocument;
                    ExpandAttribute(subDocument, attributeQueue, newValue);
                }
            }
        }

        private static bool IsSimpleType(object value)
        {
            if (value == null)
                return true;

            switch(Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Object:
                    return false;
                default:
                    return true;
            }
        }

        public static bool IsSupportedType(object value, out Type type)
        {
            if (value == null)
                type = null;
            else
                type = value.GetType();

            if (IsSimpleType(value))
                return true;

            if (value is IJSONDocument || value is Alachisoft.NosDB.Common.JSONDocument.JsonObject)
                return true;

            if (value is Array || value is ArrayList)
            {
                foreach (var arrayValue in (IList)value)
                {
                    if (!IsSupportedType(arrayValue, out type))
                        return false;
                }
                return true;
            }
            return false;
        }

        public static bool IsSupportedParameterType(object value, out Type type)
        {
            if (value == null)
                type = null;
            else
                type = value.GetType();

            if (IsSimpleType(value))
                return true;
          
            if (value is IJSONDocument || value is Alachisoft.NosDB.Common.JSONDocument.JsonObject)
                return true;
           
            if (value is IParameter)
                if (!IsSupportedParameterType(((IParameter)value).Value, out type))
                    return false;
                else
                    return true;

            if (value is ICollection)
            {
                foreach(var element in (ICollection)value)
                {
                    if (!IsSupportedParameterType(element, out type))
                        return false;
                }
                return true;
            }
            return false;
        }

        public static List<IJsonValue> ParseArray(string jsonString)
        {
            ArrayList list = ParseArray(JsonConvert.DeserializeObject<JArray>(jsonString));
            return ToIJsonList(list);
        }

        public static ArrayList ParseArray(JToken value)
        {
            ArrayList arrayList = new ArrayList();

            foreach (var token in value)
            {
                if (token is JObject)
                {
                    arrayList.Add(JSONDocument.Parse(token.ToString()));
                }
                else if (token is JArray)
                {
                    arrayList.Add(ParseArray(token));
                }
                else if (token is JValue)
                {
                    object tokenValue = ((JValue)token).Value;
                    Type type;
                    if (!IsSupportedParameterType(tokenValue, out type))
                    {
                        throw new NotSupportedException(string.Format("Type {0} is not supported", type.Name));
                    }
                    arrayList.Add(((JValue)token).Value);
                }
                else
                    throw new NotSupportedException(string.Format("Type {0} is not supported", token.GetType().FullName));
            }
            return arrayList;
        }

        public static ExtendedJSONDataTypes GetExtendedJsonType(object value)
        {
            if (value == null)
                return ExtendedJSONDataTypes.Null;
            if (IsNumber(value))
                return ExtendedJSONDataTypes.Number;
            if (value is bool)
                return ExtendedJSONDataTypes.Boolean;
            if (value is string)
                return ExtendedJSONDataTypes.String;
            if (value is DateTime)
                return ExtendedJSONDataTypes.DateTime;
            if (value is Array)
                return ExtendedJSONDataTypes.Array;
            if (value is ArrayList)
                return ExtendedJSONDataTypes.Array;
            if (value is Alachisoft.NosDB.Common.JSONDocument.JsonObject)
                if (((Alachisoft.NosDB.Common.JSONDocument.JsonObject)value).JsonComplexType == Alachisoft.NosDB.Common.JSONDocument.JsonObject.ObjectType.Array)
                    return ExtendedJSONDataTypes.Array;
                else
                    return ExtendedJSONDataTypes.Object;

            return ExtendedJSONDataTypes.Object;
        }

    }
}


