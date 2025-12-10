namespace Vozila.ViewModels.Models
{
    public class OilPriceItem
    {
        public int Id { get; set; }
        public string OilType { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
