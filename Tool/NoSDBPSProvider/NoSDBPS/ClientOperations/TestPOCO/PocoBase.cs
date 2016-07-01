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
using Newtonsoft.Json;

namespace Alachisoft.NosDB.NosDBPS.TestPOCO
{
    public class PocoBase
    {
        #region Properties ...
        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get/Set the Key.
        /// </summary>
        /// 
        [JsonProperty(PropertyName = "_key")]
        public string _key { get; set; }

        public static int _serialKey = 0;
        //public static string _uniquePrefix;
        //--------------------------------------------------------------------------------------------------------
        #endregion

        //static PocoBase()
        //{
        //    _serialKey = 0;
        //    //_uniquePrefix = Guid.NewGuid().ToString();
        //}

        protected int ToDay(int value)
        {
            return ((++value % 27));
        }

        protected int ToMonth(int value)
        {
            int month = value % 10;
            //if (month == 0)
            //    month++;

            return ++month;
        }

        protected short ToCount(long value)
        {
            return Convert.ToInt16((int)value%1000) ;
        }
    }
}