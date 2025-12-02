using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public City City { get; set; } // enum
        public Country Country { get; set; } // enum
        public decimal DestinationContractPrice { get; set; }
        public decimal DailyPricePerLiter { get; set; }  
        public decimal ContractOilPrice { get; set; }  
        public decimal DestinationPriceFromFormula
        {
            get
            {
                if (ContractOilPrice == 0)
                    return DestinationContractPrice;

                var priceDifference = DailyPricePerLiter - ContractOilPrice;
                var adjustmentFactor = priceDifference / ContractOilPrice * 0.3m;
                return DestinationContractPrice * (1 + adjustmentFactor);
            }
        }
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
