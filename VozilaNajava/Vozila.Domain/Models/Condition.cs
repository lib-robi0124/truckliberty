using System.Diagnostics.Contracts;

namespace Vozila.Domain.Models
{
    public class Condition
    {
        public int Id { get; set; }

        // Relationship
        public int CityId { get; set; }
        public City City { get; set; } = default!;

        // Prices
        public decimal PriceOilBase { get; set; }     // Base oil price at contract creation
        public decimal Km { get; set; }
        public decimal PricePerKm { get; set; }
        public decimal FullPrice => Km * PricePerKm;  // Computed full price

        // Relationship to Contract
        public int ContractId { get; set; }
        public Contract Contract { get; set; } = default!;
    }
}
