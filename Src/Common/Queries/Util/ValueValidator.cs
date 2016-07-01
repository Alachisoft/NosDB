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
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Server.Engine;
using System.Collections;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public class ValueValidator : IValidator
    {
        private readonly Dictionary<object, IValidator> _valueValidators = new Dictionary<object, IValidator>();

        public void AddValidation(object value, IValidator validator)
        {
            if (value is string)
            {
                value = ((string)value).ToLower();
            }
            _valueValidators.Add(value, validator);
        }

        public bool Validate(string key, object item, Dictionary<string, object> optionals,
            ref Dictionary<string, object> configValues, bool isOptionalValidate)
        {
            if (!(item is IDictionary))
            {
                item = item is string ? ((string)item).ToLower() : item;

                if (!_valueValidators.ContainsKey(item))
                {
                    return false;
                }

                if (!_valueValidators[item].Validate(key, item, optionals, ref configValues, isOptionalValidate))
                {
                    return false;
                }
                configValues.Add(key.ToLower(), item);
                return true;
            }

            IDictionary<string, object> doc = (IDictionary<string, object>)item;

            doc[key] = doc[key] is string ? ((string)doc[key]).ToLower() : doc[key];

            if (!_valueValidators.ContainsKey(doc[key]))
            {
                return false;
            }

            if (!_valueValidators[doc[key]].Validate(key, doc, optionals, ref configValues, isOptionalValidate))
            {
                return false;
            }
            if (!configValues.ContainsKey(key.ToLower()))
                configValues.Add(key.ToLower(), doc[key]);
            return true;
        }
    }

}
