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

namespace Alachisoft.NosDB.NosDBPS.TestPOCO
{
    public class DataLoader
    {
        public static string[] cities = new string[]
        {
            "New York", "Los Angeles", "Chicago","Houston", "Phoenix","San Antonio", "San Diego","San Jose","Dallas",
            "Austin","San Francisco","Fort Worth","Seattle","Boston", "El Paso","Portland","Las Vegas", "Fresno","Mesa","Miami"
        };
        
        public static Order[] LoadOrders(int totalOrders)
        {
            Order[] orders = new Order[totalOrders];

            for (int index = 0; index < totalOrders; index++)
            {
                orders[index] = LoadOrder(index);
            }

            return orders;
        }

        private static Order LoadOrder(int value)
        {
            Order order = new Order();

            return order;
        }
    }
}
