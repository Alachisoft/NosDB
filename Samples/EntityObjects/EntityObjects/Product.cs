using Newtonsoft.Json;

namespace Alachisoft.NosDB.EntityObjects
{
    public class Product
    {
        [JsonProperty(PropertyName = "_key")]
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public short UnitsInStock { get; set; }
        public Category Category { get; set; }
    }
}
