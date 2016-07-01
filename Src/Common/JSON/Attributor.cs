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
using System.Linq;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON
{
    // Fair Notice: The monstrosity you see here was not created with enthusaism,
    // they made me do it against my will!
    public static class Attributor
    {
        private class SimpleAttribute : IAttributeChain
        {
            public string Name { get; set; }

            public int[] Indices { get; set; }

            public IAttributeChain Child { get; set; }
        }


        public static string Delimit(string source, char[] characters)
        {
            var set = new HashSet<char>(characters);
            var charList = new List<char>(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                if (!set.Contains(source[i]))
                    charList.Add(source[i]);
            }
            return new string(charList.ToArray());

        }

        public static IAttributeChain Create(string attributeString)
        {
            if (string.IsNullOrEmpty(attributeString))
                throw new QuerySystemException(ErrorCodes.Query.ATTRIBUTE_NULL_OR_EMPTY);
            var attributor = new SimpleAttribute();
            var queue = new Queue<string>(attributeString.Split('.'));
            SimpleAttribute currentAttributor = attributor;
            while (queue.Count > 0)
            {
                string attribute = queue.Dequeue();
                var split = attribute.Split('[');
                if (split.Length > 1)
                {
                    currentAttributor.Name = split[0];
                    currentAttributor.Indices = new int[split.Length];
                    for (int i = 1; i < split.Length; i++)
                    {
                        var isolate = split[i].Split(']');
                        int index;
                        if (isolate.Length < 2 || !int.TryParse(isolate[0], out index))
                            throw new QuerySystemException(ErrorCodes.Query.INVALID_ATTRIBUTE);
                        currentAttributor.Indices[i] = index;
                    }
                }
                else
                {
                    currentAttributor.Name = attribute;
                }
                if (queue.Count > 0)
                {
                    currentAttributor.Child = new SimpleAttribute();
                    currentAttributor = currentAttributor.Child as SimpleAttribute;
                }

            }
            return attributor;
        }

        public static bool TryUpdate<T>(IJSONDocument document, T newValue, IAttributeChain thisAttribute, bool setNew)
        {
            bool isArray = false;
            if (document.Contains(thisAttribute.Name))
            {
                var type = document.GetAttributeDataType(thisAttribute.Name);
                if (thisAttribute.Child == null)
                {
                    if (thisAttribute.Indices != null)
                    {
                        if (type != ExtendedJSONDataTypes.Array)
                            return false;
                        Array array;
                        if (document.TryGet(thisAttribute.Name, out array))
                        {
                            try
                            {
                                array.SetValue(newValue, thisAttribute.Indices);
                                document[thisAttribute.Name] = array;
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    document[thisAttribute.Name] = newValue;
                    return true;
                }

                object fetch;

                if (thisAttribute.Indices != null)
                {
                    if (type != ExtendedJSONDataTypes.Array)
                        return false;
                    Array array;
                    if (document.TryGet(thisAttribute.Name, out array))
                    {
                        try
                        {
                            fetch = array.GetValue(thisAttribute.Indices);
                            isArray = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                    fetch = document[thisAttribute.Name];

                IJSONDocument newDocument = fetch as IJSONDocument;
                if (newDocument != null)
                {
                    bool result = TryUpdate(newDocument, newValue, thisAttribute.Child, setNew);
                    if (result)
                    {
                        if (isArray)
                        {
                            Array array;
                            if (document.TryGet(thisAttribute.Name, out array))
                            {
                                try
                                {
                                    array.SetValue(newDocument, thisAttribute.Indices);
                                    document[thisAttribute.Name] = array;
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            document[thisAttribute.Name] = newDocument;
                        }
                    }
                    return result;
                }
                
                var  valueArray = fetch as Array;
                if (valueArray != null)
                {
                    bool result = TryUpdateInternalArray(valueArray, newValue, thisAttribute.Child, setNew);
                    if (result)
                        document[thisAttribute.Name] = valueArray;
                    return result;
                }
                return false;

            }
            if (setNew)
            {
                if (thisAttribute.Child == null)
                {
                    if (thisAttribute.Indices != null)
                    {
                        Array newArray = Array.CreateInstance(typeof(object),
                            thisAttribute.Indices.Select(index => index + 1).ToArray());
                        newArray.SetValue(newValue, thisAttribute.Indices);
                        document[thisAttribute.Name] = newArray;
                        return true;
                    }
                    document[thisAttribute.Name] = newValue;
                    return true;
                }
                object newChain = JSONType.CreateNew();
                if (thisAttribute.Indices != null)
                {
                    Array newArray = Array.CreateInstance(typeof(object),
                           thisAttribute.Indices.Select(index => index + 1).ToArray());
                    newArray.SetValue(newChain, thisAttribute.Indices);
                    newChain = newArray;
                }
                document[thisAttribute.Name] = newChain;

                return TryUpdate(document, newValue, thisAttribute, setNew);
            }
            return false;
        }

        public static bool TryRename(IJSONDocument document, string newName, IAttributeChain thisAttribute)
        {
            bool isArray = false;
            if (document.Contains(thisAttribute.Name))
            {
                if (thisAttribute.Child == null)
                {
                    if (thisAttribute.Indices != null)
                        return false;
                    try
                    {
                        var value = document[thisAttribute.Name];
                        document.Remove(thisAttribute.Name);
                        document.Add(newName, value);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                object fetch;
                if (thisAttribute.Indices != null)
                {
                    var type = document.GetAttributeDataType(thisAttribute.Name);
                    if (type != ExtendedJSONDataTypes.Array)
                        return false;
                    Array array;
                    if (document.TryGet(thisAttribute.Name, out array))
                    {
                        try
                        {
                            fetch = array.GetValue(thisAttribute.Indices);
                            isArray = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    fetch = document[thisAttribute.Name];
                }

                var newDocument = fetch as IJSONDocument;
                if (newDocument != null)
                {
                    bool result = TryRename(newDocument, newName, thisAttribute.Child);
                    if (result)
                    {
                        if (isArray)
                        {
                            Array array;
                            if (document.TryGet(thisAttribute.Name, out array))
                            {
                                try
                                {
                                    array.SetValue(newDocument, thisAttribute.Indices);
                                    document[thisAttribute.Name] = array;
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            document[thisAttribute.Name] = newDocument;
                        }
                    }
                    return result;
                }

                var valueArray = fetch as Array;
                if (valueArray != null)
                {
                    bool result = TryRenameInternalArray(valueArray, newName, thisAttribute.Child);
                    if (result)
                        document[thisAttribute.Name] = valueArray;
                    return result;
                }
                return false;
            }
            return false;
        }

        public static bool TryDelete(IJSONDocument document, IAttributeChain thisAttribute)
        {
            if (document.Contains(thisAttribute.Name))
            {
                if (thisAttribute.Child == null)
                {
                    document.Remove(thisAttribute.Name);
                    return true;
                }
                var fetch = document[thisAttribute.Name];
                var newDocument = fetch as IJSONDocument;
                if (newDocument != null)
                {
                    bool result = TryDelete(newDocument, thisAttribute.Child);
                    if (result)
                        document[thisAttribute.Name] = newDocument;
                    return result;
                }

                var array = fetch as Array;
                if (array != null)
                {
                    bool result = TryDeleteFromInternalArray(array, thisAttribute.Child);
                    if (result)
                        document[thisAttribute.Name] = array;
                    return result;
                }
                return false;
            }
            return false;
        }


        private static bool TryUpdateInternalArray<T>(Array firstArray, T newValue, IAttributeChain attributeChain, bool setNew)
        {
            if (firstArray != null)
            {
                for (int i = 0; i < firstArray.Length; i++)
                {
                    var arrayItem = firstArray.GetValue(i);
                    var internalArray = arrayItem as Array;
                    if (internalArray != null)
                    {
                        if (!TryUpdateInternalArray(internalArray, newValue, attributeChain, setNew))
                            return false;
                    }
                    else
                    {
                        var internalDocument = arrayItem as IJSONDocument;
                        if (internalDocument == null)
                            return false;
                        if (!TryUpdate(internalDocument, newValue, attributeChain, setNew))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool TryRenameInternalArray(Array array, string newName, IAttributeChain thisAttribute)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var arrayItem = array.GetValue(i);
                    var internalArray = arrayItem as Array;
                    if (internalArray != null)
                    {
                        if (!TryRenameInternalArray(internalArray, newName, thisAttribute))
                            return false;
                    }
                    else
                    {
                        var internalDocument = arrayItem as IJSONDocument;
                        if (internalDocument == null)
                            return false;
                        if (!TryRename(internalDocument, newName, thisAttribute))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool TryDeleteFromInternalArray(Array array, IAttributeChain thisAttribute)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var arrayItem = array.GetValue(i);
                    var internalArray = arrayItem as Array;
                    if (internalArray != null)
                    {
                        if (!TryDeleteFromInternalArray(internalArray, thisAttribute))
                            return false;
                    }
                    else
                    {
                        var internalDocument = arrayItem as IJSONDocument;
                        if (internalDocument == null)
                            return false;
                        if (!TryDelete(internalDocument, thisAttribute))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static IEnumerable<object> ExtrudeArray(Array array)
        {
            var docList = new List<object>();
            foreach (var item in array)
            {
                var innerArray = item as Array;
                if (innerArray != null)
                {
                    var list = ExtrudeArray(innerArray);
                    docList.AddRange(list);
                }
                else
                    docList.Add(item);
            }
            return docList;
        } 

        public static bool TryGetArray(IJSONDocument document, out Array array, IAttributeChain thisAttribute)
        {
            array = default(Array);
            if (document.Contains(thisAttribute.Name))
            {
                if (thisAttribute.Child == null)
                {
                    if (document.GetAttributeDataType(thisAttribute.Name) != ExtendedJSONDataTypes.Array)
                        return false;

                    if (thisAttribute.Indices != null)
                    {
                        var indexesExceptLast = new int[thisAttribute.Indices.Length - 1];
                        Array.Copy(thisAttribute.Indices, 0, indexesExceptLast, 0, indexesExceptLast.Length);

                        Array documentArray;
                        if (document.TryGet(thisAttribute.Name, out documentArray))
                        {
                            try
                            {
                                var value = documentArray.GetValue(indexesExceptLast);
                                array = value as Array;
                                return array != null;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    return document.TryGet(thisAttribute.Name, out array);
                }


                object fetch;
                if (thisAttribute.Indices != null)
                {
                    Array documentArray;
                    if (document.TryGet(thisAttribute.Name, out documentArray))
                    {
                        try
                        {
                            fetch = documentArray.GetValue(thisAttribute.Indices);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                    fetch = document[thisAttribute.Name];

                document = fetch as IJSONDocument;
                return document != null && TryGetArray(document, out array, thisAttribute.Child);
            }
            return false;
        }

        public static bool TrySetArray(IJSONDocument document, Array array, IAttributeChain thisAttribute)
        {
            if (document.Contains(thisAttribute.Name))
            {
                if (thisAttribute.Child == null)
                {
                    if (document.GetAttributeDataType(thisAttribute.Name) != ExtendedJSONDataTypes.Array)
                        return false;

                    if (thisAttribute.Indices != null)
                    {
                        var indexesExceptLast = new int[thisAttribute.Indices.Length - 1];
                        Array.Copy(thisAttribute.Indices, 0, indexesExceptLast, 0, indexesExceptLast.Length);

                        Array documentArray;
                        if (document.TryGet(thisAttribute.Name, out documentArray))
                        {
                            try
                            {
                                documentArray.SetValue(array, indexesExceptLast);
                                document[thisAttribute.Name] = documentArray;
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    document[thisAttribute.Name] = array;
                    return true;
                }


                object fetch;
                if (thisAttribute.Indices != null)
                {
                    Array documentArray;
                    if (document.TryGet(thisAttribute.Name, out documentArray))
                    {
                        try
                        {
                            fetch = documentArray.GetValue(thisAttribute.Indices);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                    fetch = document[thisAttribute.Name];

                var newDocument = fetch as IJSONDocument;
                if (newDocument != null)
                {
                    bool result =  TrySetArray(newDocument, array, thisAttribute.Child);
                    if (result)
                        document[thisAttribute.Name] = document;
                    return result;
                }
            }
            return false;
        }


        public static void ForceSet<T>(IJSONDocument document, T value, IAttributeChain thisAttribute)
        {
            if (thisAttribute.Child == null)
            {
                if (thisAttribute.Indices != null)
                {
                    var lengths = new int[thisAttribute.Indices.Length];
                    for (int i = 0; i < thisAttribute.Indices.Length; i++)
                    {
                        lengths[i] = thisAttribute.Indices[i] + 1;
                    }
                    Array array = Array.CreateInstance(typeof (object), lengths);
                    array.SetValue(value, thisAttribute.Indices);
                    document[thisAttribute.Name] = array;
                }
                else
                    document[thisAttribute.Name] = value;

            }
            else
            {
                if (thisAttribute.Indices != null)
                {
                    var lengths = new int[thisAttribute.Indices.Length];
                    for (int i = 0; i < thisAttribute.Indices.Length; i++)
                    {
                        lengths[i] = thisAttribute.Indices[i] + 1;
                    }
                    Array array = Array.CreateInstance(typeof(object), lengths);
                    document[thisAttribute.Name] = array;
                    var newDocument = JSONType.CreateNew();
                    array.SetValue(newDocument, thisAttribute.Indices);
                    ForceSet(newDocument, value, thisAttribute.Child);
                }
                else
                {
                    if (!document.Contains(thisAttribute.Name) ||
                        document.GetAttributeDataType(thisAttribute.Name) != ExtendedJSONDataTypes.Object)
                    {
                        document[thisAttribute.Name] = JSONType.CreateNew();
                    }
                    document = document.GetDocument(thisAttribute.Name);
                    ForceSet(document, value, thisAttribute.Child);
                }
            }
        }
    }
}

