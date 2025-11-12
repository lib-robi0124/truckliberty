using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShipingAddress { get; set; } = string.Empty;
        public Country Country { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

    }
}
