using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class Destination
    {
        public int Id { get; set; }

        // Relation to City
        public int CityId { get; set; }
        public City City { get; set; } = default!;

        // Enum
        public Country Country { get; set; }

        // Distance
        public decimal Km { get; set; }

        // PriceOil.now - from last submitted oil price
        public decimal CurrentOilPrice { get; set; }   // You will update this daily from PriceOil table

        // PriceOil.contract - comes from Contract.Condition
        public decimal ContractOilPrice { get; set; }

        // Computed field (optional)
        public decimal TransportFullPrice => Km * ContractOilPrice;
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
