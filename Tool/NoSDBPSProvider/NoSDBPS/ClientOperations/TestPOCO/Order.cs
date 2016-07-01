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
using System.Threading;

namespace Alachisoft.NosDB.NosDBPS.TestPOCO
{
    public class Order : PocoBase
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public string ShippingLocation { set; get; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public OrderDetails OrderDetails { get; set; }

        static Random rnd = new Random();


        //--------------------------------------------------------------------------------------------------------
        public Order()
            : this(0)
        {
        }

        public Order(int value)
            : this(GetUniqueKey(value), value)
        {

        }

        public Order(string key, int value)
        {
            this._key = key;
            this.OrderID = value;
            this.CustomerID = value.ToString();
            this.EmployeeID = value;
            this.OrderDate = new DateTime(2000, ToMonth(value), 1);
            this.ShippingDate = DateTime.Now;
            this.OrderDetails = new OrderDetails(value);
            this.ShippingLocation = DataLoader.cities[value % 20];
        }


        internal static Order GetRandomOrder(int serial)
        {
            int key = rnd.Next(1, serial);
            return new Order(key);
        }

        internal static string GetRandomKey(int serial)
        {
            int keyToUpdate = rnd.Next(1, serial);
            return GetUniqueKey(keyToUpdate);
        }

        internal static string GetUniqueKey(int value)
        {
                return  value.ToString();
            
        }
    }
}
