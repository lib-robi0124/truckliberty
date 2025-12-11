using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsOrder
{
    public class OrderDetailsVM
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TransporterId { get; set; }
        public string TransporterName { get; set; } = string.Empty;
        public int DestinationId { get; set; }
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string? TruckPlateNo { get; set; }
        public DateTime DateForLoadingFrom { get; set; }
        public DateTime DateForLoadingTo { get; set; }
        public decimal ContractOilPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? TruckSubmittedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancelledReason { get; set; }
        public string? CancelledByUserName { get; set; }
        public decimal DestinationPrice { get; set; }
        public bool CanSubmitTruck { get; set; }
        public bool CanCancel { get; set; }
        public bool CanFinish { get; set; }
    }
}
