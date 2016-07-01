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
using System;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public class DocumentValidator : IValidator
    {
        private readonly Dictionary<string, object> _validators = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, object> _optionalValidators = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void AddValidation(string key, object type)
        {
            _validators.Add(key, type);
        }

        public void AddOptionalValidation(string key, object type)
        {
            _optionalValidators.Add(key, type);
        }

        public bool Validate(string key, object item, Dictionary<string, object> optionals,
            ref Dictionary<string, object> configValues, bool isOptionalValidate)
        {
            if (!(item is IDictionary))
                return false;

            IDictionary<string, object> doc = (IDictionary<string, object>)item;

            //if (!isOptionalValidate && (doc.Count < Count ||
            //    Count + optionals.Count < doc.Count))
            //{
            //    return false;
            //}
            
            List<string> extraKeys = new List<string>();

            foreach (var pair in _validators)
            {
                if (!doc.ContainsKey(pair.Key))
                {
                    return false;
                }

                if (!DQLHelper.KeyInfoEquals(pair.Key, _validators[pair.Key.ToLower()], doc,
                     optionals, ref configValues, isOptionalValidate))
                {
                    return false;
                }
            }

            foreach (var pair in _optionalValidators)
            {
                if (!doc.ContainsKey(pair.Key))
                {
                    continue;
                }

                if (!DQLHelper.KeyInfoEquals(pair.Key, _optionalValidators[pair.Key.ToLower()], doc,
                     optionals, ref configValues, true))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOptionals(object item, Dictionary<string, object> optionals, ref Dictionary<string, object> configValues)
        {
            if (!(item is IDictionary))
                return true;

            IDictionary<string, object> doc = (IDictionary<string, object>)item;

            foreach (var pair in optionals)
            {
                if (doc.ContainsKey(pair.Key))
                {
                    if (!DQLHelper.KeyInfoEquals(pair.Key, optionals[pair.Key.ToLower()], doc,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), ref configValues, true))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

}
