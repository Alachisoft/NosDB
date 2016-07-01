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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common
{
    public static class ExtensionsMethods
    {
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> original) //where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);

            bool isKeyCloneable = typeof(TKey).IsAssignableFrom(typeof(ICloneable)) ? true : false;
            bool isValueCloneable = typeof(TValue).IsAssignableFrom(typeof(ICloneable)) ? true : false;
            
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                TKey clonedKey = isKeyCloneable ? (TKey)((ICloneable)entry.Key).Clone() : entry.Key;
                TValue clonedValue = isValueCloneable ? (TValue)((ICloneable)entry.Value).Clone() : entry.Value;

                ret.Add(clonedKey, clonedValue);
            }
            return ret;
        }


        public static Dictionary<TKey, List<TValue>> Clone<TKey, TValue>(this Dictionary<TKey, List<TValue>> original) //where TValue : ICloneable
        {
            Dictionary<TKey, List<TValue>> ret = new Dictionary<TKey, List<TValue>>(original.Count, original.Comparer);

            bool isKeyCloneable = typeof(TKey).IsAssignableFrom(typeof(ICloneable)) ? true : false;
    
            foreach (KeyValuePair<TKey, List<TValue>> entry in original)
            {
                TKey clonedKey = isKeyCloneable ? (TKey)((ICloneable)entry.Key).Clone() : entry.Key;
                List<TValue> clonedValue = entry.Value.Clone<TValue>();

                ret.Add(clonedKey, clonedValue);
            }
            return ret;
        }

        public static Dictionary<TKey, TValue[]> Clone<TKey, TValue>(this Dictionary<TKey, TValue[]> original) //where TValue : ICloneable
        {
            Dictionary<TKey, TValue[]> ret = new Dictionary<TKey, TValue[]>(original.Count, original.Comparer);

            bool isKeyCloneable = typeof(TKey).IsAssignableFrom(typeof(ICloneable)) ? true : false;

            foreach (KeyValuePair<TKey, TValue[]> entry in original)
            {
                TKey clonedKey = isKeyCloneable ? (TKey)((ICloneable)entry.Key).Clone() : entry.Key;
                TValue[] clonedValue = entry.Value.Clone<TValue>();

                ret.Add(clonedKey, clonedValue);
            }
            return ret;
        }

        public static List<TValue> Clone<TValue>(this List<TValue> original) //where TValue : ICloneable
        {
            List<TValue> ret = new List<TValue>(original.Count);

            bool isValueCloneable = typeof(TValue).IsAssignableFrom(typeof(ICloneable)) ? true : false;

            foreach (TValue value in original)
            {
                TValue clonedValue = isValueCloneable ? (TValue)((ICloneable)value).Clone() : value;
                ret.Add(clonedValue);
            }
            return ret;
        }

        public static TValue[] Clone<TValue>(this TValue[] original) //where TValue : ICloneable
        {
            TValue[] ret = new TValue[original.Length];

            bool isValueCloneable = typeof(TValue).IsAssignableFrom(typeof(ICloneable)) ? true : false;

            int index = 0;
            foreach (TValue value in original)
            {
                TValue clonedValue = isValueCloneable ? (TValue)((ICloneable)value).Clone() : value;
                ret[index] =clonedValue;
                index++;
            }
            return ret;
        }
    }
}
