namespace Vozila.ViewModels.Models
{
    public class PriceOilHistoryVM
    {
        public DateTime Date { get; set; }
        public decimal DailyPricePerLiter { get; set; }
        public decimal? PriceChange { get; set; }
        public decimal? PercentageChange { get; set; }
    }
}
