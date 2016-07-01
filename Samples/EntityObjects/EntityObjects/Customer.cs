using Newtonsoft.Json;

namespace Alachisoft.NosDB.EntityObjects
{
    public class Customer
    {
        public Customer() { }
        [JsonProperty(PropertyName = "_key")]
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }
}
