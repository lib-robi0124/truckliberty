using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsDestination
{
    public class DestinationVM
    {
        public int Id { get; set; }
        public City City { get; set; }
        public Country Country { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public decimal DestinationContractPrice { get; set; }
        public decimal DailyPricePerLiter { get; set; }
        public int ConditionId { get; set; }
        public decimal ContractOilPrice { get; set; }
        public decimal CalculatedPrice { get; set; }
    }
}
