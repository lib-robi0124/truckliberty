namespace Vozila.ViewModels.Models
{
    public class DestinationDetailsVM
    {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public decimal DestinationContractPrice { get; set; }
        public decimal DailyPricePerLiter { get; set; }
        public decimal ContractOilPrice { get; set; }
        public decimal CalculatedPrice { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public bool IsContractActive { get; set; }
    }
}
