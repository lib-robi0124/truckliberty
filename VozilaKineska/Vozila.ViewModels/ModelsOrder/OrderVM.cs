using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsOrder
{
    public class OrderVM
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string? TruckPlateNo { get; set; }
        public DateTime DateForLoadingFrom { get; set; }
        public DateTime DateForLoadingTo { get; set; }
        public decimal ContractOilPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
