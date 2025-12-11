using System.ComponentModel.DataAnnotations;

namespace Vozila.ViewModels.ModelsOrder
{
    public class CreateOrderVM
    {
        [Required(ErrorMessage = "Company is required")]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Transporter is required")]
        [Display(Name = "Transporter")]
        public int TransporterId { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }

        [Required(ErrorMessage = "Loading start date is required")]
        [Display(Name = "Loading From")]
        [DataType(DataType.DateTime)]
        public DateTime DateForLoadingFrom { get; set; }

        [Required(ErrorMessage = "Loading end date is required")]
        [Display(Name = "Loading To")]
        [DataType(DataType.DateTime)]
        public DateTime DateForLoadingTo { get; set; }

        [Required(ErrorMessage = "Contract oil price is required")]
        [Display(Name = "Contract Oil Price")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        public decimal ContractOilPrice { get; set; }

        [Display(Name = "Truck Plate Number")]
        [StringLength(20, ErrorMessage = "Truck plate number cannot exceed 20 characters")]
        public string? TruckPlateNo { get; set; }
    }
}
