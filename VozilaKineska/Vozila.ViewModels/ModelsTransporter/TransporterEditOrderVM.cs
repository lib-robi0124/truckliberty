using System.ComponentModel.DataAnnotations;
using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsTransporter
{
    public class TransporterEditOrderVM
    {
        public int Id { get; set; }

        [Display(Name = "Company")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "Transporter")]
        public string TransporterName { get; set; } = string.Empty;

        [Display(Name = "Destination")]
        public string DestinationCity { get; set; } = string.Empty;

        [Display(Name = "Loading From")]
        [DataType(DataType.DateTime)]
        public DateTime DateForLoadingFrom { get; set; }

        [Display(Name = "Loading To")]
        [DataType(DataType.DateTime)]
        public DateTime DateForLoadingTo { get; set; }

        [Required(ErrorMessage = "Truck plate number is required")]
        [Display(Name = "Truck Plate Number *")]
        [StringLength(20, ErrorMessage = "Truck plate number cannot exceed 20 characters")]
        public string TruckPlateNo { get; set; } = string.Empty;

        public OrderStatus CurrentStatus { get; set; }
    }
}
