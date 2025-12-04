namespace Vozila.ViewModels.Models
{
    public class UpdateConditionVM
    {
        public int Id { get; set; }
        public decimal? ContractOilPrice { get; set; } // Optional update
        public List<int>? NewDestinationIds { get; set; } // Add more destinations
    }

}
