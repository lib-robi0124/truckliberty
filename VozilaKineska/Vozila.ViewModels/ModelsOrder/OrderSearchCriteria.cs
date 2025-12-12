using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsOrder
{
    public class OrderSearchCriteria
    {
        public string? OrderId { get; set; }
        public string? Destination { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
