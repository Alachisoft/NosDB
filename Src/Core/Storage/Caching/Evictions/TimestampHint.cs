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
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Serialization.IO;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Storage.Caching.Evictions
{
    /// <summary>
    /// Eviction Hint based on the timestamp; Used in case of LRU based Eviction.
    /// </summary>

    [Serializable]
    internal class TimestampHint : EvictionHint, ICompactSerializable
    {
        /// <summary>Time stamp for the hint</summary>
        //protected int _dt;
        [CLSCompliant(false)]
        protected DateTime _dt;
        
        new internal  static int InMemorySize = 32;
        
        static TimestampHint()
        {                                         //for _dt
            InMemorySize = MemoryUtil.GetInMemoryInstanceSize(EvictionHint.InMemorySize + MemoryUtil.NetDateTimeSize);
        } 


        /// <summary>
        /// Constructor.
        /// </summary>
        public TimestampHint()
        {
            //_dt = AppUtil.DiffSeconds(DateTime.UtcNow);
            _hintType = EvictionHintType.TimestampHint;
            _dt = DateTime.UtcNow;

            //Random rand = new Random();
            //int min = rand.Next(DateTime.Now.Minute, 60);
            //int sec = rand.Next(1, 60);
            //int year = DateTime.UtcNow.Year;
            //int month = DateTime.UtcNow.Month;
            //int day = DateTime.UtcNow.Day;
            //int hour = DateTime.UtcNow.Hour;

            //_dt = new DateTime(year, month, day, hour, min, sec);
        }


        /// <summary>Return time stamp for the hint</summary>
        //public int TimeStamp
        public DateTime TimeStamp
        {
            get { return _dt;}
        }		


        ///// <summary>Get the slot in which this hint should be placed</summary>
        //public override byte SlotIndex
        //{
        //    get { return AppUtil.Lg(_dt); }
        //}

        /// <summary>
        /// Return if the hint is to be changed on Update
        /// </summary>
        public override bool IsVariant
        {
            get { return true; }
        }


        /// <summary>
        /// Update the hint if required
        /// </summary>
        /// <returns></returns>
        public override bool Update()
        {
            //_dt = AppUtil.DiffSeconds(DateTime.UtcNow);
            _dt = DateTime.UtcNow;
            return true;
        }

        //#region	/                 --- IComparable ---           /

        ///// <summary>
        ///// Compares the current instance with another object of the same type.
        ///// </summary>
        ///// <param name="obj">An object to compare with this instance.</param>
        ///// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
        //public override int CompareTo(object obj)
        //{
        //    if (obj is TimestampHint)
        //    {
        //        return _dt.CompareTo(((TimestampHint)obj)._dt);
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

        //#endregion

        #region	/                 --- ICompactSerializable ---           /

        void ICompactSerializable.Deserialize(CompactReader reader)
        {
            base.Deserialize(reader);
            _dt = reader.ReadDateTime();
        }

        void ICompactSerializable.Serialize(CompactWriter writer)
        {
            base.Serialize(writer);
            writer.Write(_dt);
        }

        #endregion       
    }
}