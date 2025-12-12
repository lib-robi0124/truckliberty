using System.ComponentModel.DataAnnotations;
using Vozila.Domain.Enums;

namespace Vozila.ViewModels.Models
{
    public class AdminEditOrderVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company is required")]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Transporter is required")]
        [Display(Name = "Transporter")]
        public int TransporterId { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }

        [Required(ErrorMessage = "Loading from date is required")]
        [Display(Name = "Loading From")]
        [DataType(DataType.DateTime)]
        public DateTime DateForLoadingFrom { get; set; }

        [Required(ErrorMessage = "Loading to date is required")]
        [Display(Name = "Loading To")]
        [DataType(DataType.DateTime)]
        public DateTime DateForLoadingTo { get; set; }

        [Display(Name = "Contract Oil Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal ContractOilPrice { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Truck Plate Number")]
        [StringLength(20, ErrorMessage = "Truck plate number cannot exceed 20 characters")]
        public string? TruckPlateNo { get; set; }

        public OrderStatus CurrentStatus { get; set; } // Read-only to show current status
    }
}
