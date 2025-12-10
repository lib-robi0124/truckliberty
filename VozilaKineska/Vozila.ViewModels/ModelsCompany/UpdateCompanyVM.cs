using System.ComponentModel.DataAnnotations;
using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsCompany
{
    public class UpdateCompanyVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Shipping address must be between 5 and 200 characters")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        public Country Country { get; set; }

        [Required(ErrorMessage = "City is required")]
        public City City { get; set; }
    }
}
