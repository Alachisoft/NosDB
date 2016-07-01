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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Distributor.Comparers
{
    public class JsonDocumentComparer : IComparer
    {
        private IList<OrderedAttribute> _attributesOrderBy = new List<OrderedAttribute>();

        public JsonDocumentComparer(IList<OrderedAttribute> orderByFieldsNames)
        {
            _attributesOrderBy = orderByFieldsNames;   
        }

        public IList<OrderedAttribute> FieldsOrderBy
        {
            get { return _attributesOrderBy; }
        }

        public int Compare(object x, object y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            IJSONDocument xDoc = (IJSONDocument)x;
            IJSONDocument yDoc = (IJSONDocument)y;
            
            foreach (OrderedAttribute attr in _attributesOrderBy)
            {
                int result = 0;
                IComparable first;
                object second;
                
                var attribute = attr.ToString();

                IJsonValue firstValue;
                IJsonValue secondValue;

                if (attr.Evaluate(out firstValue, xDoc))
                {
                    if (attr.Evaluate(out secondValue, yDoc))
                    {
                        result = JSONComparer.Compare((IComparable)firstValue.Value, secondValue.Value);
                    }
                    else
                        result = 1;
                }
                else if (xDoc.TryGet(attribute, out first))
                {
                    if (yDoc.TryGet(attribute, out second))
                    {
                        result = JSONComparer.Compare(first, second);
                    }
                    else
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = -1;
                }

                //object fieldObj = xDoc[attr.ToString()];

                //if (fieldObj is double)
                //{
                //    double field1 = xDoc.Get<double>(attr.ToString());
                //    double field2 = yDoc.Get<double>(attr.ToString());
                //    result = field1.CompareTo(field2);
                //}
                //else if (fieldObj is string)
                //{
                //    string field1 = xDoc.Get<string>(attr.ToString());
                //    string field2 = yDoc.Get<string>(attr.ToString());
                //    result = field1.CompareTo(field2);
                //}
                //else if (fieldObj is bool)
                //{
                //    bool field1 = xDoc.Get<bool>(attr.ToString());
                //    bool field2 = yDoc.Get<bool>(attr.ToString());
                //    result = field1.CompareTo(field2);
                //}
                //else if (fieldObj is long)
                //{
                //    long field1 = xDoc.Get<long>(attr.ToString());
                //    long field2 = yDoc.Get<long>(attr.ToString());
                //    result = field1.CompareTo(field2);
                //}
                //else
                //{
                //    throw new Exception("Invalid type for comparison");
                //}

                if (result != 0)
                {
                    if (attr.Order.SortOrder == Alachisoft.NosDB.Common.Enum.SortOrder.DESC)
                    {
                        if (result < 0)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = -1;
                        }
                    }
                    return result;
                }
            }
            return 0;
        }
    }
}
