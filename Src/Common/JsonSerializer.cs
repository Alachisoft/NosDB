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
using System.Collections;
using Newtonsoft.Json;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common
{
    public class JsonSerializer
    {
       //private JsonSerializerSettings _settings = new JsonSerializerSettings();
       //public JsonSerializer()
       //{
       //    _settings.TypeNameHandling = TypeNameHandling.Objects;
       //}
            
        public static JSONDocument Serialize<T>(T document)
        {
            if (document == null)
                return null;
            if (typeof(T) == typeof(JSONDocument))
                return (JSONDocument)(object)document;
            return (JSONDocument)JSONDocument.Parse(JsonConvert.SerializeObject(document));
        }

        public static List<JSONDocument> Serialize<T>(List<T> documents)
        {
            if (typeof(T) == typeof(JSONDocument))
                return documents as List<JSONDocument>;

            List<JSONDocument> JSONDocuments;
            JSONDocuments = new List<JSONDocument>();
            JSONDocument jdoc;

            foreach (T document in documents)
            {
                if (document != null)
                {
                    string str = JsonConvert.SerializeObject(document);
                    jdoc = (JSONDocument)JSONDocument.Parse(str);
                    JSONDocuments.Add(jdoc);
                }
                else
                    throw new ArgumentException("document cannot be null");
            }
            return JSONDocuments ;
        }

        public static JSONDocument Serialize<T>(T instance, JsonConverter[] converters)
        {
            if (instance == null)
                return null;
            if (typeof(T) == typeof(JSONDocument))
                return (JSONDocument)(object)instance;

            return (JSONDocument)JSONDocument.Parse(JsonConvert.SerializeObject(instance, converters));
        }


        public static T Deserialize<T>(IJSONDocument jsonDocument)
        {
            if (jsonDocument == null)
                return default(T);
            if (typeof(T) == typeof(JSONDocument))
                return (T)(object)jsonDocument;

            return JsonConvert.DeserializeObject<T>(jsonDocument.ToString());
            
        }

        public static List<T> Deserialize<T>(List<JSONDocument> jsonDocumments)
        {
            if (typeof(T) == typeof(JSONDocument))
                return jsonDocumments as List<T>;

            List<T> collection = new List<T>();
           
            foreach (JSONDocument jDoc in jsonDocumments)
            {
                if (jDoc != null) 
                {
                    collection.Add(JsonConvert.DeserializeObject<T>(jDoc.ToString()));
                }
                else
                    throw new ArgumentException("document cannot be null");
            }

            return collection;
        }

        public static T Deserialize<T>(IJSONDocument jsonDocument, JsonConverter[] converters)
        {
            if (jsonDocument == null)
                return default(T);
            if (typeof(T) == typeof(JSONDocument))
                return (T)(object)jsonDocument;

            return JsonConvert.DeserializeObject<T>(jsonDocument.ToString(), converters);

        }

    }
}
