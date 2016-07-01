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
using System.Collections;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Queries.ParseTree.DDL;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Configuration.Services;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    // Name key of the JSON document is reserved... kindly don't use it for any kind of configuration.
    public static class DQLHelper
    {
        public static bool ValidateDDLArguments(IConfigurationSession session, string cluster, DataDefinitionType defType, DbObjectType objType, 
            DdlConfiguration parsedConf, out ICloneable configuration, 
            out Dictionary<string, object> confValues)
        {
            ConfigurationValidator validator = new ConfigurationValidator(session, cluster, defType, objType);

            if (validator.Validate(ToEqualityComparerDictionary(parsedConf.Configuration),
                out configuration, out confValues))
            {
                return true;
            }
            return false;
        }

        public static ExtendedJSONDataTypes GetValueType(object value)
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
            return ExtendedJSONDataTypes.Object;
        }

        private static bool IsNumber(object value)
        {
            if (value == null)
                return false;
           
            Type valueType = value.GetType();
            TypeCode actualType = Type.GetTypeCode(valueType);
            switch (actualType)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        public static bool KeyInfoEquals(string key, object type, IDictionary<string, object> doc,
           Dictionary<string, object> optionals, ref Dictionary<string, object> configValues, bool isOptionalValidate)
        {
            if (type is ExtendedJSONDataTypes)
            {
                if (!JsonDocumentUtil.GetExtendedJsonType(doc[key]).Equals(type))
                {
                    return false;
                }
                if(!configValues.ContainsKey(key.ToLower()))
                    configValues.Add(key.ToLower(), doc[key]);
            }
            else if (type is IValidator)
            {
                object jsonValue = null;
                if (type is ValueValidator)
                {
                    jsonValue = doc;
                }
                else
                {
                    jsonValue = doc[key];
                }
                if (!((IValidator)type).Validate(key, jsonValue,  optionals, ref configValues, isOptionalValidate))
                {
                    return false;
                }
            }
            return true;
        }

        private static Dictionary<string, object> ToEqualityComparerDictionary(IJSONDocument document)
        {
            return ToObject(document) as Dictionary<string, object>;
        }

        private static object ToObject(object value) 
        {

            if (value is IJSONDocument)
            {
                IJSONDocument document = value as JSONDocument;
                Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in document)
                {
                    dictionary.Add(pair.Key, ToObject(pair.Value));
                }
                return dictionary;
            }
            else if (value is IList)
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < ((IList)value).Count; i++)
                    list.Add(ToObject(((IList)value)[i]));
                return list;
            }
            else
                return value;
        }
    }
    
}