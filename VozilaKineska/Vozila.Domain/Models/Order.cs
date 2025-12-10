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
        public DateTime? TruckSubmittedDate { get; set; } // When Transporter submits TruckPlateNo
        public DateTime? FinishedDate { get; set; } // When admin marks as Finished
        public DateTime? CancelledDate { get; set; } // When cancelled
        public string? CancelledReason { get; set; } // Reason for cancellation
        public int? CancelledByUserId { get; set; } // Who cancelled the order
        public User? CancelledByUser { get; set; }
    }
}
