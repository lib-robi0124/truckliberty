namespace Vozila.ViewModels.Models
{
    public class PriceOilVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal DailyPricePerLiter { get; set; }
        public List<OilPriceItem> OilPrices { get; set; } = new List<OilPriceItem>();
        public decimal? CurrentAveragePrice { get; set; }
        public decimal? PriceChangePercentage { get; set; }
    }
}
