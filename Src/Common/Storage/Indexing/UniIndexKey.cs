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
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Storage.Indexing
{
    public class UniIndexKey : IndexKey
    {
        IndexAttribute _attribute;

        public string Attribute { get { return _attribute.Name; } }
        public SortOrder Order { get { return _attribute.SortOrder; }}

        public UniIndexKey(IndexAttribute attribute)
        {
            _attribute = attribute;
        }
        
        public override string ToString()
        {
            return _attribute.Name;
        }

        public override int GetHashCode()
        {
            return _attribute.Name.GetHashCode();
        }

        public override bool TryGetValue(IJSONDocument document, out AttributeValue[] value)
        {
            value = null;
            string currentAttribute = _attribute.Name;
            if (document.Contains(currentAttribute))
            {
                ExtendedJSONDataTypes type = document.GetAttributeDataType(currentAttribute);

                switch (type)
                {
                    case ExtendedJSONDataTypes.Array:
                        Array objectsArray = document.GetArray<object>(currentAttribute);
                        return TryGetArray(objectsArray, out value);
                    default:
                        IComparable dataValue;
                        if (document.TryGet(currentAttribute, out dataValue))
                        {
                            if (dataValue != null)
                            {
                                value = new AttributeValue[] { new SingleAttributeValue(dataValue) };
                            }
                            else
                            {
                                value = new[] { NullValue.Null };
                            }
                            return true;
                        }
                        return false;
                }
            }
            return false;
        }

    

        private bool TryGetArray(Array source, out AttributeValue[] values)
        {
            var attributeValues = new List<AttributeValue>();
            for (int i=0;i<source.Length;i++)
            {
                var array = source.GetValue(i) as Array;
                if(array!=null)
                    continue;

                var comparable = source.GetValue(i) as IComparable;
                if (comparable == null)
                    continue;

                attributeValues.Add(new SingleAttributeValue(new ArrayElement(comparable, i)));
            }
            values = attributeValues.ToArray();
            if (attributeValues.Count > 0)
                return true;
            return false;
        }
    }
}