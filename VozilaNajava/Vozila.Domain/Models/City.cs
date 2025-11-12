using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class City
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int PostalCode { get; set; }
        public Country Country { get; set; }
        public ICollection<Condition> Conditions { get; set; } = new HashSet<Condition>();
        public ICollection<Destination> Destinations { get; set; } = new HashSet<Destination>();
        public ICollection<Company> Companies { get; set; } = new HashSet<Company>();
    }
}

