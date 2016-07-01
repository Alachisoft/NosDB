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
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public class ArrayValidator : IValidator
    {
        private readonly object _elementsType;

        public ArrayValidator(object elementType)
        {
            _elementsType = elementType;
        }

        public bool Validate(string key, object item, Dictionary<string, object> optionals,
            ref Dictionary<string, object> configValues, bool isOptionalValidate)
        {
            IEnumerable enumerable  = item as IEnumerable;
            IList arrayValue = arrayValue = new List<IComparable>();

            if (DQLHelper.GetValueType(item) != ExtendedJSONDataTypes.Array)
                return false;

            if (!enumerable.GetEnumerator().MoveNext())
                return false;
            else
                enumerable.GetEnumerator().Reset();

            if (_elementsType is IValidator)
                arrayValue = new List<Dictionary<string, object>>();
           
            foreach (var value in enumerable)
            {
                if (_elementsType is ExtendedJSONDataTypes)
                {
                    if (!DQLHelper.GetValueType(value).Equals(_elementsType))
                        return false;

                    arrayValue.Add(value);
                }
                else if (_elementsType is IValidator)
                {
                    Dictionary<string, object> elementValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    if (!((IValidator)_elementsType).Validate(key, value, optionals, ref elementValues, isOptionalValidate))
                        return false;
                    
                    arrayValue.Add(elementValues);
                }
            }

            if(!configValues.ContainsKey(key.ToLower()))
                configValues.Add(key.ToLower(), arrayValue);
            return true;
        }
    }

}
