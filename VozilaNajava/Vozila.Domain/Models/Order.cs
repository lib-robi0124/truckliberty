using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Related entities
        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        public int TransporterId { get; set; }
        public Transporter Transporter { get; set; } = default!;

        public int DestinationId { get; set; }
        public Destination Destination { get; set; } = default!;

        // Truck details
        public string? TruckPlateNo { get; set; }

        // Loading time window
        public DateTime DateForLoadingFrom { get; set; }
        public DateTime DateForLoadingTo { get; set; }

        // Price from Contract Condition
        public decimal PriceOilFromContractConditions { get; set; }

        // Order status
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Audit / tracking
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
    }
}
