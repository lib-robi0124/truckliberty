using Vozila.Domain.Enums;

namespace Vozila.ViewModels.Models
{
    public class DestinationPriceOfferVM
    {
        public City DestinationCity { get; set; }
        public decimal BestPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
