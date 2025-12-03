namespace Vozila.ViewModels.Models
{
    public class ConditionVM
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public decimal ContractOilPrice { get; set; }
        public int DestinationCount { get; set; }
    }
}
