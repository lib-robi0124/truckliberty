using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class OrderSearchCriteria
    {
        public int? TransporterId { get; set; }
        public int? CompanyId { get; set; }
        public int? DestinationId { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TruckPlateNo { get; set; }
        public string? SearchTerm { get; set; }
    }
}
