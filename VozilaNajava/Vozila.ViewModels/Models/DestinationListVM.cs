namespace Vozila.ViewModels.Models
{
    public class DestinationListVM
    {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public decimal DestinationContractPrice { get; set; }
        public decimal CalculatedPrice { get; set; }
        public int OrderCount { get; set; }
    }
}
