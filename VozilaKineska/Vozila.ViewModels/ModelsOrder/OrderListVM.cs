using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsOrder
{
    public class OrderListVM
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public DateTime DateForLoadingFrom { get; set; }
        public DateTime DateForLoadingTo { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? TruckPlateNo { get; set; }
    }
}
