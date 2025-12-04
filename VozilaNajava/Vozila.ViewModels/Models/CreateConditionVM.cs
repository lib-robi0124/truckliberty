namespace Vozila.ViewModels.Models
{
    public class CreateConditionVM
    {
        public int ContractId { get; set; }
        public decimal ContractOilPrice { get; set; }
        public List<int> DestinationIds { get; set; } = new();
    }
}
