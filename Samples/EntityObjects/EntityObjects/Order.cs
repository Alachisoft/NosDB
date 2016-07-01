using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.EntityObjects
{
    public class Order
    {
        [JsonProperty(PropertyName = "_key")]
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public DateTime ShippedDate { get; set; }
        public int ShipVia { get; set; }
        public double Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string Shipcountry { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
    }
}
