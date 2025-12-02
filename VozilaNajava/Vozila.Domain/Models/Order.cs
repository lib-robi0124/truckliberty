using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;
        public int TransporterId { get; set; }
        public Transporter Transporter { get; set; } = default!;
        public int DestinationId { get; set; }
        public Destination Destination { get; set; } = default!;
        public string? TruckPlateNo { get; set; }
        public DateTime DateForLoadingFrom { get; set; }
        public DateTime DateForLoadingTo { get; set; }
        public decimal ContractOilPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
    }
}
