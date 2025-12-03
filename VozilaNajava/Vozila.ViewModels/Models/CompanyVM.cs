using Vozila.Domain.Enums;

namespace Vozila.ViewModels.Models
{
    public class CompanyVM
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShipingAddress { get; set; } = string.Empty;
        public Country Country { get; set; }
        public City City { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    }
}
